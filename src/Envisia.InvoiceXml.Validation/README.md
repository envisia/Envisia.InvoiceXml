# Envisia.InvoiceXml.Validation

Validates electronic invoices against the **official XML schemas and Schematron rules** and creates
validation reports in the format of the [KoSIT validator](https://github.com/itplr-kosit/validator)
(XML report and HTML "Prüfbericht"):

| Configuration | Documents | Rules |
|---|---|---|
| `ValidatorConfiguration.FacturX` | ZUGFeRD 2.x / Factur-X CII: MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED | Factur-X 1.09.2 / ZUGFeRD 2.5.2 XSD and Schematron |
| `ValidatorConfiguration.XRechnung` | XRechnung 3.0 (UBL Invoice, UBL CreditNote, CII, Extension, CVD) and plain EN 16931 | KoSIT configuration 2026-08-31: UBL 2.1 / CII D16B XSD, CEN EN 16931 Schematron 1.3.16, XRechnung Schematron 2.6.0 |
| `ValidatorConfiguration.EN16931` | Any CIUS of EN 16931 (`urn:cen.eu:en16931:2017…`) in UBL or CII | UBL 2.1 / CII D16B XSD, CEN EN 16931 Schematron 1.3.16 |

Everything runs in .NET: the package contains its own XPath 2.0/3.1 and ISO Schematron engine that
executes the Schematron sources directly. **No Java, no Saxon, no network access.** The engine follows
the semantics of Saxon-HE 12 (the XSLT processor of the KoSIT validator); for the XRechnung test suite
and the official Factur-X samples the reports match those of the KoSIT validator message by message.

## Usage

```csharp
using Envisia.InvoiceXml.Validation;

InvoiceXmlValidator validator = new InvoiceXmlValidator();   // thread-safe, reuse it
ValidationReport report = validator.Validate("invoice.xml");

Console.WriteLine($"{report.Scenario?.Name}: {report.Recommendation}");
foreach (ValidationMessage error in report.Errors)
{
    Console.WriteLine($"{error.Code} {error.Text} ({error.XPathLocation})");
}

report.SaveXml("invoice-report.xml");    // KoSIT report (VARL)
report.SaveHtml("invoice-report.html");  // KoSIT HTML report
```

Invoices created with `Envisia.InvoiceXml` can be validated directly:

```csharp
ValidationReport report = invoice.ValidateXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);
```

Like the KoSIT validator, the validator selects the scenario that matches the document, validates it
against the XML schema and, if the document is schema valid, against the Schematron rules of the
scenario. The result carries the acceptance recommendation of the scenario (`report.IsAcceptable`).
Own rules can be used with a scenario configuration in the format of the KoSIT validator
(`ValidatorConfiguration.Load("scenarios.xml")`) or directly with `SchematronSchema.Load("rules.sch")`.

Documentation: https://github.com/envisia/ZUGFeRD-csharp/blob/master/docs/validation.md

## License

The library is licensed under the Apache License 2.0. The embedded validation artefacts keep their
licenses, in particular the CEN EN 16931 Schematron rules are licensed under the EUPL 1.2. See
THIRD-PARTY-NOTICES.md in the package.
