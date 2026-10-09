# Validation

The package **Envisia.InvoiceXml.Validation** validates invoice XML against the official XML schemas
and Schematron rules of XRechnung, ZUGFeRD / Factur-X and EN 16931 and creates reports in the format
of the [KoSIT validator](https://github.com/itplr-kosit/validator), the reference validator for
XRechnung.

```shell
dotnet add package Envisia.InvoiceXml.Validation
```

Target framework: .NET 10.

The package runs entirely in .NET. It contains an XPath 2.0/3.1 engine and an ISO Schematron
implementation that execute the official Schematron sources, so neither Java nor an XSLT processor is
needed, and no network access happens during validation.

## Implementations and dependency injection

The interface `IInvoiceValidator` and its result `InvoiceValidationResult` are in
**Envisia.InvoiceXml.Validation.Abstractions**. Two packages implement it; reference one of them:

| Package | Engine |
|---|---|
| `Envisia.InvoiceXml.Validation` | Pure .NET: own XPath and Schematron engine running the official rules (this page) |
| [`Envisia.InvoiceXml.Validation.GraalVM`](../src/Envisia.InvoiceXml.Validation.GraalVM/README.md) | The official KoSIT validator 1.6.3, compiled into a native library with GraalVM native-image (linux-x64, linux-arm64, win-x64, osx-arm64) |

Both register the validator as a singleton with the same method, so switching means replacing the
package reference:

```csharp
services.AddInvoiceValidator();

public sealed class InvoiceInbox(IInvoiceValidator validator)
{
    public bool Accept(byte[] xml) => validator.Validate(xml, "invoice.xml").IsAcceptable;
}
```

`InvoiceValidationResult` contains the recommendation, the matched scenario, every message (rule id,
level, text, location, step) and the report in the format of the KoSIT validator (`ReportXml`,
`GetReportHtml()`). Both implementations pass the same tests
(`tests/Shared/InvoiceValidatorContractTests.cs`) and choose the scenario the same way. The .NET
implementation is about five times faster (5.5 ms against 25–28 ms per document on one thread) and
needs half the memory; the GraalVM package runs the KoSIT validator itself and also KoSIT
configurations with compiled Schematron (XSLT) or own report stylesheets. In the .NET implementation
`AddInvoiceValidator(options)` takes `ValidatorOptions`, and `InvoiceXmlValidator` also offers the
richer `ValidationReport` described below.

## Validating a document

```csharp
using Envisia.InvoiceXml.Validation;

InvoiceXmlValidator validator = new InvoiceXmlValidator();
ValidationReport report = validator.Validate("invoice.xml");

if (!report.IsAcceptable)
{
    foreach (ValidationMessage error in report.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Text} at {error.XPathLocation}");
    }
}

report.SaveXml("invoice-report.xml");
report.SaveHtml("invoice-report.html");
```

`Validate` accepts a file path, a stream or a byte array (with an optional name for the report).
An `InvoiceXmlValidator` is thread-safe; create it once and reuse it. The XML schemas and Schematron
rules of a scenario are compiled when they are used for the first time (about 2 seconds for all
XRechnung scenarios). Call `ValidatorConfiguration.Compile()` at startup to do that up front. After that a
validation takes a few milliseconds (8 ms on average for the XRechnung test suite, including the XML
schema validation).

Invoices created with `Envisia.InvoiceXml` can be validated without writing a file:

```csharp
ValidationReport report = invoice.ValidateXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);
Assert.IsTrue(report.IsAcceptable, string.Join(Environment.NewLine, report.Errors));
```

Only the XML is validated. To validate a hybrid ZUGFeRD / Factur-X PDF, extract the embedded
`factur-x.xml` / `zugferd-invoice.xml` with your PDF library first; the PDF/A-3 conformance of the PDF is
not checked.

## Built-in configurations

| Configuration | Scenarios | Rules |
|---|---|---|
| `ValidatorConfiguration.FacturX` | ZUGFeRD / Factur-X MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED (CII, matched by the guideline identifier BT-24) | XSD and Schematron of Factur-X 1.09.2 / ZUGFeRD 2.5.2 |
| `ValidatorConfiguration.XRechnung` | XRechnung 3.0 UBL Invoice, UBL CreditNote and CII; XRechnung Extension (UBL Invoice, CII); XRechnung CVD (UBL Invoice, UBL CreditNote, CII); EN 16931 UBL Invoice, UBL CreditNote and CII | The KoSIT validator configuration for XRechnung 3.0.2 (release 2026-08-31): UBL 2.1 and CII D16B XSD, CEN EN 16931 Schematron 1.3.16, XRechnung Schematron 2.6.0 |
| `ValidatorConfiguration.EN16931` | Every CIUS of EN 16931 (`urn:cen.eu:en16931:2017…`, e.g. Peppol BIS Billing) in UBL Invoice, UBL CreditNote and CII | UBL 2.1 and CII D16B XSD, CEN EN 16931 Schematron 1.3.16 |

`new InvoiceXmlValidator()` uses `ValidatorConfiguration.Default`, which is FacturX, XRechnung and
EN16931 in this order. The configurations are tried in order and the first one in which a scenario
matches decides. Within a configuration, a document must match exactly one scenario (as in the KoSIT
validator). So a ZUGFeRD invoice in the EN 16931 profile is validated with the Factur-X rules (which
contain the EN 16931 rules), an XRechnung with the XRechnung rules and, for example, a Peppol BIS
invoice with the EN 16931 rules (the Peppol-specific rules are not part of the package).

To get exactly the behaviour of the KoSIT validator with its XRechnung configuration, use only that
configuration:

```csharp
InvoiceXmlValidator validator = new InvoiceXmlValidator(ValidatorConfiguration.XRechnung);
```

## The report

| Property | Meaning |
|---|---|
| `Scenario` | The scenario that matched, `null` if none matched (the document is then rejected) |
| `IsWellFormed`, `IsSchemaValid` | Result of parsing and of the XML schema validation |
| `IsValid` | No errors and no warnings in any step (the `valid` attribute of the KoSIT report) |
| `Recommendation`, `IsAcceptable` | The acceptance recommendation (`Accept` / `Reject`) computed like the KoSIT validator |
| `Steps` | The validation steps (`val-xsd`, `val-sch.1`, `val-sch.2`, ..., `val-xml`) with their messages; Schematron steps also hold the full `SchematronResult` |
| `Messages`, `Errors`, `Warnings` | All messages; errors and warnings according to the custom level of the scenario |
| `DocumentData` | Seller, invoice number and issue date found in the document |
| `ToXml()`, `SaveXml()` | The report in the format of the KoSIT validator (VARL, `http://www.xoev.de/de/validator/varl/1`) |
| `ToHtml()`, `SaveHtml()` | The HTML report of the KoSIT validator ("Prüfbericht"), in German or English |

Each `ValidationMessage` contains the rule id as `Code` (e.g. `BR-DE-15`), the `Level` derived from
the `flag` / `role` of the Schematron assertion, the `CustomLevel` after applying the `customLevel`
settings of the scenario, the `Text`, the `XPathLocation` of the node in the format of the KoSIT
validator and, for Schematron messages, the line and column of the node.

As in the KoSIT validator:

- If the document is not valid against the XML schema, the Schematron rules are not run. Set
  `ValidatorOptions.ValidateSchematronOnSchemaErrors` to run them anyway.
- The acceptance is decided by the `acceptMatch` expression of the scenario, evaluated on the XML
  report. Without one, a schema valid document without failed assertions is accepted.
- The HTML report says "accept" when there are no errors after applying the custom levels. Warnings
  do not lead to a rejection.

```csharp
ValidatorOptions options = new ValidatorOptions
{
    ReportLanguage = ReportLanguage.English,       // HTML report in English (default: German)
    IncludeDocumentContentInReport = false,        // no copy of the invoice in the HTML report
    ValidateSchematronOnSchemaErrors = true
};
InvoiceXmlValidator validator = new InvoiceXmlValidator(options);
```

## Custom configurations

Configurations use the scenario format of the KoSIT validator (`scenarios.xml`, namespace
`http://www.xoev.de/de/validator/framework/1/scenarios`), so configurations written for the KoSIT
validator can be used, provided they reference the Schematron sources:

```csharp
ValidatorConfiguration own = ValidatorConfiguration.Load("my-rules/scenarios.xml");
InvoiceXmlValidator validator = new InvoiceXmlValidator(new[] { own, ValidatorConfiguration.XRechnung });
```

Resource locations are relative to the directory of `scenarios.xml` (or the `repositoryPath`
argument). The KoSIT configurations reference compiled Schematron (`.xsl`); when a `.sch` file with the
same name exists next to it, that one is used, otherwise loading fails with a `NotSupportedException`.
`createReport` resources are not executed: the report is always the KoSIT default report, and the
`customLevel` settings of the scenario are applied.

## Running Schematron directly

```csharp
using Envisia.InvoiceXml.Validation.Schematron;

SchematronSchema schema = SchematronSchema.Load("rules.sch", phase: "#ALL");
SchematronResult result = schema.Validate("document.xml");
foreach (SchematronMessage message in result.FailedAssertions)
{
    Console.WriteLine($"{message.Id} [{message.Flag}] {message.Text} at {message.Location}");
}
result.ToSvrl().Save("document.svrl");
```

Supported are ISO Schematron with query binding `xslt2` / `xslt3` (XPath 3.1 functions): includes,
phases, abstract patterns and rules, `let` on all levels, `value-of`, `name`, `diagnostics`,
`subject`, `xsl:key`, and `xsl:function` with `xsl:variable`, `xsl:sequence`, `xsl:value-of`,
`xsl:choose`, `xsl:if` and `xsl:text` in the body. Dynamic errors (for example a cast of an invalid
value) raise a `SchematronException`; the validator reports them as processing error, as the KoSIT
validator does. `SchematronOptions.LocationFormat` selects the location format of the messages:
`EQName` (`/Q{ns}Invoice[1]/...`, as SchXslt, used by the KoSIT validator for XRechnung) or
`IsoSkeleton` (`/*:Invoice[namespace-uri()='ns'][1]/...`, as the CEN and Factur-X XSLT).

## Compatibility with the KoSIT validator

The XPath engine follows the semantics of Saxon-HE 12, the XSLT processor of the KoSIT validator,
including details the official rules depend on (decimal arithmetic and rounding, comparisons of
untyped values with numbers, the evaluation order of `and` / `or` when an operand raises an error,
the location paths). The repository tests this with

- the results of more than 400 XPath expressions compared with Saxon,
- the unit tests of the CEN EN 16931 validation artefacts (915 test cases),
- the reports of the KoSIT validator for the XRechnung test suite and the official ZUGFeRD /
  Factur-X samples (158 documents), which must match scenario, validity, assessment and every
  message with code, level, location and text.

Known differences:

- The texts of XML schema errors come from `System.Xml` and differ from those of the Java XML parser.
  As in the KoSIT validator, their code is `generic-error`.
- `xs:decimal` values are limited to the precision of `System.Decimal` (28 significant digits).
- Compiled Schematron (XSLT) cannot be executed, only Schematron sources.

## Licenses of the rules

The library is licensed under the Apache License 2.0. The embedded artefacts keep their licenses:
the CEN EN 16931 Schematron rules are licensed under the EUPL 1.2, the XRechnung rules and the KoSIT
configuration under the Apache License 2.0; the Factur-X, UN/CEFACT and OASIS UBL schemas are
redistributed under the terms of their publishers. See
[THIRD-PARTY-NOTICES.md](../src/Envisia.InvoiceXml.Validation/Resources/THIRD-PARTY-NOTICES.md).

The native library of Envisia.InvoiceXml.Validation.GraalVM additionally contains the KoSIT validator
(Apache License 2.0), Saxon-HE (MPL 2.0) and classes of GraalVM Community Edition (GPL v2 with the
Classpath Exception), see
[its THIRD-PARTY-NOTICES.md](../src/Envisia.InvoiceXml.Validation.GraalVM/THIRD-PARTY-NOTICES.md).
