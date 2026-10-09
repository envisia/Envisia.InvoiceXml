# Envisia.InvoiceXml.Validation.GraalVM

Validates electronic invoices (XRechnung, ZUGFeRD / Factur-X, EN 16931) with the **official
[KoSIT validator](https://github.com/itplr-kosit/validator) 1.6.3**, compiled ahead of time into a native
library with GraalVM native-image: the same engine (Saxon-HE 12) and the same reports as the KoSIT
validator, **without a JVM**. The package implements `IInvoiceValidator` of
`Envisia.InvoiceXml.Validation.Abstractions` and can replace the pure .NET implementation
`Envisia.InvoiceXml.Validation`:

```csharp
services.AddInvoiceValidator();   // KoSIT with the built-in configurations

public sealed class InvoiceInbox(IInvoiceValidator validator)
{
    public bool Accept(byte[] xml) => validator.Validate(xml, "invoice.xml").IsAcceptable;
}
```

Reference only one of the two implementation packages: both define `AddInvoiceValidator()`.

## Configurations

| Configuration | Documents |
|---|---|
| `KositConfiguration.FacturX` | ZUGFeRD 2.5.2 / Factur-X 1.09.2 CII: MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED (official XSD and Schematron) |
| `KositConfiguration.XRechnung` | The KoSIT configuration for XRechnung 3.0.2 (release 2026-08-31): UBL Invoice, UBL CreditNote, CII, Extension, CVD and EN 16931 |
| `KositConfiguration.FromFiles("scenarios.xml")` | Any KoSIT validator configuration, e.g. an unpacked release or your own |

The configurations are tried in order; the first one with a scenario that matches the document validates it.
By default these are Factur-X, then XRechnung (`KositConfiguration.Default`). The built-in configurations are
unpacked on first use into the temporary directory (`Envisia.InvoiceXml.Validation.GraalVM/<hash>`), because
KoSIT loads them from files.

```csharp
using KositValidator validator = new KositValidator(KositConfiguration.XRechnung);   // thread-safe, reuse it
InvoiceValidationResult result = validator.Validate("invoice.xml");

Console.WriteLine($"{result.Scenario}: {result.Recommendation}");
foreach (InvoiceValidationMessage error in result.Errors)
{
    Console.WriteLine($"{error.Code} {error.Text} ({error.Location})");
}

File.WriteAllText("invoice-report.xml", result.ReportXml);       // KoSIT report (VARL)
File.WriteAllText("invoice-report.html", result.GetReportHtml()); // KoSIT HTML report
```

`services.AddInvoiceValidator(KositConfiguration.FromFiles("scenarios.xml"))` registers a validator with own
configurations. `KositValidator.ValidateReport` returns the recommendation and the report of KoSIT unchanged.

## Platforms

The package contains the native library for **linux-x64, linux-arm64, win-x64 and osx-arm64** (about 55 MB
each, in `runtimes/<rid>/native`). .NET picks the right one when running and when publishing for a runtime
identifier. Loading the configurations takes some seconds; create the validator once and reuse it.

## Choosing an implementation

Both implementations pass the same tests (`tests/Shared/InvoiceValidatorContractTests.cs`); the GraalVM
package also passes the KoSIT conformance suite (`tests/KositConformance`) with results identical to KoSIT on
the JVM. Measured on the same 159 documents with the XRechnung configuration:

| | Envisia.InvoiceXml.Validation | Envisia.InvoiceXml.Validation.GraalVM |
|---|---|---|
| Engine | Own XPath and Schematron engine in .NET | The official KoSIT validator and Saxon-HE |
| Loading the configuration | 3.2 s | 6–12 s |
| Per document, 1 thread | 5.5 ms | 25–28 ms |
| Per document, 4 threads | 4.3 ms | 8.5–9.5 ms |
| Memory | 300 MB | 650 MB |
| Platforms | Any | linux-x64, linux-arm64, win-x64, osx-arm64 |

Use the GraalVM package where the results must come from the KoSIT validator itself, or for KoSIT
configurations with compiled Schematron (XSLT) or own report stylesheets (`createReport`), which the .NET
engine does not execute.

## Building the native library

`native/build-native.sh` compiles the unmodified KoSIT jar from Maven Central (checksum verified) and the C
entry points in `native/java` with GraalVM 25 (`GRAALVM_HOME`). native-image cannot cross-compile, so CI builds
the library on each platform (`.github/workflows/graalvm.yml`) and packs them together
(`dotnet pack -p:PackGraalVM=true`). After changing the KoSIT version or the entry points, record the
reflection metadata again with `native/trace.sh`.

Documentation: https://github.com/envisia/ZUGFeRD-csharp/blob/master/docs/validation.md

## License

The .NET code is licensed under the Apache License 2.0. The native library contains the KoSIT validator
(Apache 2.0), Saxon-HE (MPL 2.0) and classes of GraalVM Community Edition (GPL v2 with the Classpath
Exception); the embedded rules keep their licenses, in particular the CEN EN 16931 Schematron rules are
licensed under the EUPL 1.2. See THIRD-PARTY-NOTICES.md in the package.
