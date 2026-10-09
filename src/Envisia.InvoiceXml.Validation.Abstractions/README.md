# Envisia.InvoiceXml.Validation.Abstractions

The interfaces and the result model for validating electronic invoices (XRechnung, ZUGFeRD / Factur-X, EN 16931)
with the official XML schemas and Schematron rules, with reports in the format of the KoSIT validator.

Reference one implementation package; both register `IInvoiceValidator` with the same method:

| Package | Implementation |
|---|---|
| `Envisia.InvoiceXml.Validation` | Pure .NET: own XPath and Schematron engine running the official rules |
| `Envisia.InvoiceXml.Validation.GraalVM` | The official KoSIT validator (Java), compiled into a native library with GraalVM |

```csharp
services.AddInvoiceValidator();   // from the implementation package you reference

public sealed class InvoiceInbox(IInvoiceValidator validator)
{
    public bool Accept(byte[] xml) => validator.Validate(xml, "invoice.xml").IsAcceptable;
}
```

`InvoiceValidationResult` holds the recommendation, the matched scenario, every message (rule id, level, text,
location) and the KoSIT report itself (`ReportXml`, `GetReportHtml()`).
