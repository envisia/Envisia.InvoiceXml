# Prototype: the KoSIT validator as a native library (GraalVM) for .NET 10

This prototype compiles the **unmodified official KoSIT validator 1.6.3** (Java, with Saxon-HE 12) ahead of time
into a native shared library with [GraalVM](https://www.graalvm.org) `native-image --shared`, and calls it from
.NET 10 through P/Invoke. No JVM is needed at runtime, and the .NET process loads one native library.

```
.NET 10 ── P/Invoke ──> libkosit.so ── KoSIT 1.6.3 + Saxon-HE 12 + JDK classes, compiled ahead of time
           (src/KositNative)            (java/src/KositNative.java: C entry points)
```

```csharp
using KositValidator validator = new KositValidator("config/scenarios.xml", "config");
KositReport report = validator.Validate(File.ReadAllBytes("invoice.xml"), "invoice.xml");
Console.WriteLine(report.Recommendation);   // Acceptable / Reject
string varl = report.Xml;                   // the KoSIT report
```

## How it works

- `java/src/de/envisia/kosit/KositNative.java`: C entry points (`@CEntryPoint`) around the KoSIT Java API:
  - `kosit_create` loads a configuration (`Configuration.load(...).build(...)`, `new DefaultCheck(...)`),
  - `kosit_validate` validates a document and returns the recommendation and the report,
  - `kosit_transform` and `kosit_xpath` expose Saxon for XSLT and XPath (used by the tests),
  - `kosit_free` and `kosit_destroy` release memory and configurations.
- `java/metadata/reachability-metadata.json`: the reflection and resource metadata native-image needs (JAXB, Saxon,
  the JDK XML stack), recorded with the tracing agent (`trace.sh`) while validating all documents of the test suite
  with the XRechnung and Factur-X configurations.
- `build-native.sh`: downloads the official jar from Maven Central (checksum verified), compiles the entry points
  and runs native-image. That takes about 3.5 minutes and 3 GB of memory.
- `src/KositNative`: the .NET binding (`KositValidator`, `KositXml`). Each OS thread that calls the library is
  attached to the GraalVM isolate once. The validator is thread-safe.
- `src/KositGraalVm`: a command line tool. `tests/`: the shared conformance suite.

Unlike the IKVM prototype (#3), this needs no changes to KoSIT. GraalVM supports current Java versions, so the
released jar is used as it is.

## Results

The conformance suite of `prototypes/kosit-testsuite` (see its README):

```shell
XRECHNUNG_CONFIGURATION=<unpacked configuration release> ../kosit-testsuite/prepare-testdata.sh
GRAALVM_HOME=<GraalVM 25> ./build-native.sh
dotnet test tests/KositGraalVm.Tests.csproj -c Release
```

**537 passed, 0 failed, no known differences** (35 s):

- all 230 reports are identical to those of the official KoSIT validator on the JVM (XRechnung test suite, tests of
  the XRechnung configuration, CEN examples, Factur-X samples),
- all 25 reports of the configuration's own tests meet their 327 assertions,
- all 1,146 CEN EN 16931 unit tests give the same results as Saxon on the JVM,
- concurrent validations equal sequential ones.

Performance, measured back to back on the same 159 documents with the XRechnung configuration (4-core container,
single runs, expect about ±20 %). "Warmed up" is the steady state after several passes over the documents:

| | Native C# (#2) | GraalVM native library | KoSIT on the JVM (Java 21) | IKVM (#3) |
|---|---|---|---|---|
| Loading the configuration | 3.2 s | 6–12 s | 8–11 s (+ JVM start) | 14–15 s |
| First pass, 1 thread | 14–16 ms per document | 40–53 ms | 42–56 ms | 75 ms |
| Warmed up, 1 thread | 5.5 ms | 25–28 ms | 15–16 ms | 45–48 ms |
| Warmed up, 4 threads | 4.3 ms | 8.5–9.5 ms | 5–6 ms | 20–23 ms |
| Memory | 300 MB peak working set | 650–675 MB | 650–690 MB heap in use | 870–940 MB |
| Size | one DLL with the rules | 55 MB `libkosit.so` | JVM + 11 MB jar | 96 MB |

The native library starts faster than the JVM and needs no warm-up, but once the JVM's JIT compiler has warmed up,
the JVM is about 1.7 times faster: native-image compiles ahead of time without runtime profiles (GraalVM
Community has no profile-guided optimization and uses the serial garbage collector). It is about twice as fast as
IKVM.

## Limits and open points

- native-image cannot cross-compile: the library has to be built on each target platform (linux-x64,
  linux-arm64, win-x64, osx-arm64, ...), e.g. with a CI matrix, and packaged as `runtimes/<rid>/native` in a NuGet
  package. Only linux-x64 was built and tested here. `build-native.sh` is a bash script; on Windows the same steps
  run with the GraalVM tools (the library is then `kosit.dll`).
- native-image also writes AWT libraries (`libawt*.so` etc.) next to the library because `java.desktop` is
  reachable. They were not needed: the tests ran with only `libkosit.so` in the output directory.
- The reflection metadata must be recorded again (`trace.sh`) when the KoSIT version changes. Code paths that the
  trace did not reach can fail at runtime with missing reflection metadata; the trace covers all documents of the
  test suite with all configurations.
- Threads attached to the isolate stay attached for the lifetime of the process (prototype simplification).
- GraalVM Community Edition is licensed under the GPL v2 with the Classpath Exception. The library contains JDK
  classes under that license, besides KoSIT (Apache 2.0) and Saxon-HE (MPL 2.0).

This folder is not part of `Envisia.InvoiceXml.sln` and is not built by CI.
