# Prototype: the KoSIT validator on .NET 10 with IKVM

This prototype runs the original **KoSIT validator** (Java) inside a .NET 10 process. It uses
[IKVM](https://github.com/ikvmnet/ikvm), which converts Java bytecode to .NET assemblies, and it calls the KoSIT
Java API from C# the same way a Java application uses the validator as a library:

```csharp
Processor processor = ProcessorProvider.getProcessor();
Configuration configuration = Configuration.load(scenariosUri, repositoryUri).build(processor);
Check check = new DefaultCheck(processor, configuration);
Result result = check.checkInput(InputFactory.read(new java.io.File("invoice.xml")));
bool acceptable = result.isAcceptable();
org.w3c.dom.Document report = result.getReportDocument();
```

No JVM is needed at runtime. Validation is done by the KoSIT code itself, with Saxon-HE 12 and the compiled
XSLT of the validator configuration, so the results come from the same engine as the Java validator.

## Why KoSIT has to be rebuilt

IKVM 8 implements Java SE 8 and only converts Java 8 class files. The KoSIT validator 1.6.x is compiled for
Java 11 and uses JAXB 4 (also Java 11), so the released jar cannot be converted ("class format error 55.0").
`build-kosit.sh` therefore builds the release tag `v1.6.3` from source with `kosit-java8.patch`:

- compile for Java 8 (`<release>8</release>`), JAXB 3.0 (Java 8, same `jakarta.xml.bind` API) instead of 4.0,
- `Map.entry`, `Set.of` and `URLEncoder.encode(String, Charset)` replaced by their Java 8 equivalents,
- two stream pipelines over the SVRL result (`DefaultCheck.buildCustomFailedAssertsList`,
  `DefaultResult.filterSchematronResult`) written as loops: IKVM translates them into invalid IL
  (`InvalidProgramException`).

The older Java 8 build that KoSIT published for validator 1.5.0 also runs under IKVM. It needs the bundled copies
of `javax.xml`, `org.w3c.dom` and `org.xml.sax` removed and has the same stream problem. It is an older
validator version, though.

## Results

Reports compared with those of the KoSIT validator 1.6.3 on the JVM (Java 21) for the same documents and the
XRechnung configuration 3.0.2 (release 2026-08-31), with `compare-reports.py`. The comparison covers scenario,
validity, assessment, steps and every message's id, code, level, location and text.

| Documents | Identical reports |
|---|---|
| XRechnung test suite, KoSIT test instances, CEN examples (159) | 157 |
| Official ZUGFeRD / Factur-X samples (79, Factur-X scenarios) | 78 |

The 3 differences:

- `cenex-CII_example6`: the document has an invalid `xsi:schemaLocation` (an odd number of URIs). The XML schema
  validator of IKVM's Java 8 class library reports this as a schema error, so the document is rejected. KoSIT on
  Java 21 ignores it and accepts the document. **This is the only difference in the result.**
- 2 XML schema messages differ in wording only (`'cbc:Foo'` instead of `'{"urn:…":Foo}'`), for the same reason.

Performance on a 4-core container. For comparison: Java KoSIT 1.6.3 on the JVM needs 25 s for the 159
documents including start-up; Envisia.InvoiceXml.Validation needs about 8 ms per document.

| | KoSIT on .NET 10 via IKVM |
|---|---|
| Loading the XRechnung configuration (compiling the XSLT) | 15–17 s |
| First pass, 159 documents | 11–13 s (70–80 ms per document) |
| Following passes | 40–52 ms per document |
| Peak working set | ~800 MB |
| Size of the published application (linux-x64, framework-dependent) | 96 MB (IKVM.Java.dll 63 MB, KoSIT + Saxon 11 MB, IKVM runtime images 15 MB) |

Other observations:

- IKVM warns that Saxon 12 calls `ByteBuffer.flip()`/`clear()` with Java 9 signatures in
  `AnyURIValue.decode`. That method throws `NoSuchMethodError` if it is ever reached; it was not reached with
  these documents.
- The rebuilt KoSIT is a patched fork that has to be maintained for each KoSIT release.

## Tests

`tests/` runs the shared conformance suite of `prototypes/kosit-testsuite` (see its README) against this
prototype. It covers report parity with KoSIT on the JVM for 230 documents, the 327 assertions of the XRechnung
configuration's own tests and the 1,146 CEN EN 16931 unit tests through the converted Saxon.

```shell
XRECHNUNG_CONFIGURATION=<unpacked configuration release> ../kosit-testsuite/prepare-testdata.sh
./build-kosit.sh
dotnet test tests/KositIkvm.Tests.csproj -c Release
```

Result: **535 passed, 2 skipped as known differences, 0 failed** (537 tests, 47 s):

- all 25 reports of the configuration's own tests meet their 327 assertions,
- all CEN unit tests give the same results as Saxon on the JVM,
- 228 of 230 reports are identical to the JVM reference; the 2 known differences (`tests/known-differences.txt`)
  are `CII_example6` (invalid `xsi:schemaLocation`, rejected by the Java 8 schema validator) and the wording of
  one XML schema message (`ubl005`).

## Running it

Requirements: .NET 10 SDK, git, a JDK (11 or newer) and Maven to build the KoSIT jar.

```shell
./build-kosit.sh                                  # builds lib/kosit-validator-java8.jar
dotnet build -c Release
# the validator configuration from https://github.com/itplr-kosit/validator-configuration-xrechnung/releases
dotnet bin/Release/net10.0/KositIkvm.dll --repository <configuration dir> --output reports invoice1.xml invoice2.xml
# compare with reports of the Java validator (java -jar validator-1.6.3-standalone.jar ... -o jvm-reports ...)
python3 compare-reports.py jvm-reports reports
```

`--scenarios <file>` selects another scenario file of the configuration and `--passes <n>` repeats the
validation to measure warmed-up performance.

This folder is not part of `Envisia.InvoiceXml.sln` and is not built by CI.
