# Envisia.InvoiceXml

[![CI](https://github.com/envisia/ZUGFeRD-csharp/actions/workflows/ci.yml/badge.svg)](https://github.com/envisia/ZUGFeRD-csharp/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Envisia.InvoiceXml?logo=nuget)](https://www.nuget.org/packages/Envisia.InvoiceXml)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE)
[![Sponsor](https://img.shields.io/github/sponsors/schmitch?label=sponsor&logo=github)](https://github.com/sponsors/schmitch)

A .NET library to **create and read structured electronic invoices according to EN 16931** –
ZUGFeRD, Factur-X and XRechnung – in both EN 16931 syntaxes, UN/CEFACT **CII** and OASIS **UBL**.

The library produces and parses the invoice XML. Embedding the XML into a PDF/A-3 (hybrid
ZUGFeRD / Factur-X invoice) is intentionally left to the PDF library of your choice.

| Package | Purpose |
|---|---|
| [Envisia.InvoiceXml](https://www.nuget.org/packages/Envisia.InvoiceXml) | Create and read invoice XML |
| [Envisia.InvoiceXml.Validation](https://www.nuget.org/packages/Envisia.InvoiceXml.Validation) | Validate invoice XML with the official XSD and Schematron rules of XRechnung, ZUGFeRD / Factur-X and EN 16931; reports like the KoSIT validator. Pure .NET, no Java |
| [Envisia.InvoiceXml.Validation.GraalVM](https://www.nuget.org/packages/Envisia.InvoiceXml.Validation.GraalVM) | Alternative implementation: the official KoSIT validator, compiled into a native library with GraalVM (no JVM needed) |
| [Envisia.InvoiceXml.Validation.Abstractions](https://www.nuget.org/packages/Envisia.InvoiceXml.Validation.Abstractions) | `IInvoiceValidator`, implemented by both validation packages (`services.AddInvoiceValidator()`) |

## Supported standards

| Standard | Versions | Profiles | Syntax | Read | Write |
|---|---|---|---|---|---|
| ZUGFeRD 1.0 | 1.0 | BASIC, COMFORT, EXTENDED | CII D13B | ✔ | ✔ |
| ZUGFeRD 2.0 | 2.0 | MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED | CII D16B | ✔ | ✔ |
| ZUGFeRD 2.1 – 2.4 / Factur-X 1.0 – 1.08 | `ZUGFeRDVersion.Version23` | MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED | CII D22B | ✔ | ✔ |
| ZUGFeRD 2.5 – **2.5.2** / Factur-X 1.09 – **1.09.2** | `ZUGFeRDVersion.Version25` | MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED | CII D22B | ✔ | ✔ |
| XRechnung | 1.2 – **3.0.2** (writes 3.0.x) | XRechnung (CIUS) | CII and UBL 2.1 | ✔ | ✔ |
| French e-reporting | | EREPORTING | CII | ✔ | ✔ |

The guideline identifiers (BT-24) did not change between Factur-X 1.0 and 1.09.2, so both
versions produce the same document identifiers:

- `Version23` writes invoices that are valid against ZUGFeRD 2.4 / Factur-X 1.08 (and older 2.x
  receivers as long as you don't use elements that were introduced in 2.4, such as sub invoice lines).
- `Version25` additionally writes the EXTENDED elements that were introduced with ZUGFeRD 2.5 /
  Factur-X 1.09: debtor BIC and account name, the manufacturer of an item and financial adjustments.
  Use it when your receivers validate against ZUGFeRD 2.5.

All other new EXTENDED elements (item seller, line delivery terms, delivery location, per-package
quantity, line totals, typed item attributes, ...) are only written when you fill them.
When reading, ZUGFeRD 2.x invoices cannot be told apart and are reported as `Version23`.

The test suite validates the generated XML against the **official schemas of Factur-X 1.08
(ZUGFeRD 2.4) and Factur-X 1.09.2 (ZUGFeRD 2.5.2)** for every profile, round-trips all official
ZUGFeRD 2.4 example invoices and validates XRechnung UBL output against the OASIS UBL 2.1 schemas.

## Installation

```shell
dotnet add package Envisia.InvoiceXml
dotnet add package Envisia.InvoiceXml.Validation   # optional, the validator
```

Target frameworks: .NET 10, .NET 8, .NET Standard 2.0/2.1, .NET Framework 4.6.2 and 4.8
(Envisia.InvoiceXml.Validation: .NET 10).

## Quick start

```csharp
using Envisia.InvoiceXml;

InvoiceDescriptor invoice = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2026, 3, 5), CurrencyCodes.EUR);
invoice.BusinessProcess = "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0";
invoice.ReferenceOrderNo = "04011000-12345-34";                       // BT-10 buyer reference (Leitweg-ID)
invoice.ActualDeliveryDate = new DateTime(2026, 3, 3);                // BT-72

invoice.SetSeller("Lieferant GmbH", "80333", "München", "Lieferantenstraße 20", CountryCodes.DE);
invoice.AddSellerTaxRegistration("DE123456789", TaxRegistrationSchemeID.VA);
invoice.SetSellerContact("Max Mustermann", emailAddress: "max@lieferant.de", phoneno: "+49 89 123456");
invoice.SetSellerElectronicAddress("rechnung@lieferant.de", ElectronicAddressSchemeIdentifiers.ElectronicMailSmtp);

invoice.SetBuyer("Kunden AG", "69876", "Frankfurt", "Kundenstraße 15", CountryCodes.DE);
invoice.SetBuyerElectronicAddress("einkauf@kunde.de", ElectronicAddressSchemeIdentifiers.ElectronicMailSmtp);

invoice.AddTradeLineItem(name: "Trennblätter A4", netUnitPrice: 9.90m, billedQuantity: 20m,
                         unitCode: QuantityCodes.H87, taxType: TaxTypes.VAT,
                         categoryCode: TaxCategoryCodes.S, taxPercent: 19m);

invoice.AddApplicableTradeTax(198.00m, 19m, 37.62m, TaxTypes.VAT, TaxCategoryCodes.S);
invoice.SetTotals(lineTotalAmount: 198.00m, taxBasisAmount: 198.00m, taxTotalAmount: 37.62m,
                  grandTotalAmount: 235.62m, duePayableAmount: 235.62m);
invoice.SetPaymentMeans(PaymentMeansTypeCodes.SEPACreditTransfer);
invoice.AddCreditorFinancialAccount("DE02120300000000202051", "BYLADEM1001");
invoice.AddTradePaymentTerms("Zahlbar innerhalb von 30 Tagen ohne Abzug", new DateTime(2026, 4, 4));

// ZUGFeRD / Factur-X (CII), profile EN 16931
invoice.Save("factur-x.xml", ZUGFeRDVersion.Version23, Profile.Comfort);

// XRechnung 3.0 in UBL or CII syntax
invoice.Save("xrechnung-ubl.xml", ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);
invoice.Save("xrechnung-cii.xml", ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.CII);
```

Reading detects version, profile and syntax automatically:

```csharp
InvoiceDescriptor loaded = InvoiceDescriptor.Load("incoming-invoice.xml");
Console.WriteLine($"{loaded.Profile}: {loaded.InvoiceNo} – {loaded.DuePayableAmount} {loaded.Currency}");
```

`Profile.Comfort` is the EN 16931 profile. When you write an invoice in a smaller profile, the
writer leaves out everything the profile does not know (for example, MINIMUM only keeps the
document header and totals), so an invoice read from a richer profile can be written as a
schema-valid invoice of a smaller one.

### Validation

`Envisia.InvoiceXml.Validation` checks invoice XML against the official XML schemas and Schematron
rules – XRechnung 3.0.2 (the configuration of the KoSIT validator), ZUGFeRD 2.5.2 / Factur-X 1.09.2
in all profiles and EN 16931 in UBL and CII – and creates the XML and HTML reports of the KoSIT
validator. It runs the Schematron sources with its own XPath engine, without Java:

```csharp
using Envisia.InvoiceXml.Validation;

ValidationReport report = invoice.ValidateXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);
// or for any file: new InvoiceXmlValidator().Validate("incoming-invoice.xml")
if (!report.IsAcceptable)
{
    foreach (ValidationMessage error in report.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Text}");
    }
}
report.SaveHtml("report.html");
```

See [Validation](docs/validation.md) for the configurations, the report and custom rules. With
dependency injection, `services.AddInvoiceValidator()` registers `IInvoiceValidator`; reference
`Envisia.InvoiceXml.Validation.GraalVM` instead to validate with the official KoSIT validator itself
(compiled into a native library, no JVM needed).

`InvoiceValidator.Validate(invoice, ZUGFeRDVersion.Version23)` in the main package only recalculates line
totals, allowances/charges, the VAT breakdown and the document totals (BR-CO-*) of an `InvoiceDescriptor`
and reports deviations.

More documentation:

- [Getting started](docs/getting-started.md)
- [Profiles and formats](docs/profiles-and-formats.md)
- [ZUGFeRD details](docs/zugferd-details.md)
- [Validation](docs/validation.md)
- [References](docs/references.md)

## Migrating from ZUGFeRD-csharp

Envisia.InvoiceXml is based on ZUGFeRD-csharp 18.0. The API is the same apart from:

- Package `ZUGFeRD-csharp` → `Envisia.InvoiceXml`, assembly `s2industries.ZUGFeRD` →
  `Envisia.InvoiceXml`, namespace `s2industries.ZUGFeRD` → `Envisia.InvoiceXml`.
- The PDF (`ZUGFeRD.PDF-csharp`), rendering and Excel companion packages are not part of this
  project.
- The writers follow the profile schemas strictly; see the [changelog](CHANGELOG.md) for the
  behaviour changes (e.g. MINIMUM output, a single payment terms element in BASIC/EN 16931,
  seller/buyer contact point).

## Building and testing

```shell
dotnet build Envisia.InvoiceXml.sln
dotnet test Envisia.InvoiceXml.sln
```

The .NET 10 SDK is required (see `global.json`); the tests run on .NET 8 and .NET 10.
The official specification packages and schemas used by the tests are in `documentation/`.

Releases are published to nuget.org by pushing a version tag (`v1.2.3`); see
[CONTRIBUTING.md](CONTRIBUTING.md).

## Sponsoring

If Envisia.InvoiceXml is useful for you, please consider sponsoring its development:
[github.com/sponsors/schmitch](https://github.com/sponsors/schmitch).

## License and attribution

Licensed under the [Apache License 2.0](LICENSE). The validation package embeds the official
validation artefacts under their own licenses (the CEN EN 16931 Schematron rules under the EUPL 1.2),
see [THIRD-PARTY-NOTICES.md](src/Envisia.InvoiceXml.Validation/Resources/THIRD-PARTY-NOTICES.md). The
native library of Envisia.InvoiceXml.Validation.GraalVM contains the KoSIT validator (Apache 2.0), Saxon-HE
(MPL 2.0) and GraalVM classes (GPL v2 with the Classpath Exception), see
[its THIRD-PARTY-NOTICES.md](src/Envisia.InvoiceXml.Validation.GraalVM/THIRD-PARTY-NOTICES.md).

Envisia.InvoiceXml is a derivative of [ZUGFeRD-csharp](https://github.com/stephanstapel/ZUGFeRD-csharp)
by Stephan Stapel / STwo Industries GmbH and its contributors. See [NOTICE](NOTICE).
