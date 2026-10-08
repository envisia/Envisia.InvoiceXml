# Envisia.InvoiceXml

[![CI](https://github.com/envisia/ZUGFeRD-csharp/actions/workflows/ci.yml/badge.svg)](https://github.com/envisia/ZUGFeRD-csharp/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Envisia.InvoiceXml?logo=nuget)](https://www.nuget.org/packages/Envisia.InvoiceXml)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE.txt)

A .NET library to **create and read structured electronic invoices according to EN 16931** –
ZUGFeRD, Factur-X and XRechnung – in both EN 16931 syntaxes, UN/CEFACT **CII** and OASIS **UBL**.

The library produces and parses the invoice XML. Embedding the XML into a PDF/A-3 (hybrid
ZUGFeRD / Factur-X invoice) is intentionally left to the PDF library of your choice.

## Supported standards

| Standard | Versions | Profiles | Syntax | Read | Write |
|---|---|---|---|---|---|
| ZUGFeRD 1.0 | 1.0 | BASIC, COMFORT, EXTENDED | CII D13B | ✔ | ✔ |
| ZUGFeRD 2.0 | 2.0 | MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED | CII D16B | ✔ | ✔ |
| ZUGFeRD 2.1 – **2.5.2** / Factur-X 1.0 – **1.09.2** | `ZUGFeRDVersion.Version23` | MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED | CII D22B | ✔ | ✔ |
| XRechnung | 1.2 – **3.0.2** (writes 3.0.x) | XRechnung (CIUS) | CII and UBL 2.1 | ✔ | ✔ |
| French e-reporting | | EREPORTING | CII | ✔ | ✔ |

The guideline identifiers (BT-24) did not change between Factur-X 1.0 and 1.09.2, therefore
`ZUGFeRDVersion.Version23` covers every ZUGFeRD version from 2.1 up to the current 2.5.2.
Elements that were added to the EXTENDED profile in ZUGFeRD 2.4 and 2.5 are only written when
you fill them.

The test suite validates the generated XML against the **official schemas of Factur-X 1.08
(ZUGFeRD 2.4) and Factur-X 1.09.2 (ZUGFeRD 2.5.2)** for every profile, round-trips all official
ZUGFeRD 2.4 example invoices and validates XRechnung UBL output against the OASIS UBL 2.1 schemas.

## Installation

```shell
dotnet add package Envisia.InvoiceXml
```

Target frameworks: .NET 10, .NET 8, .NET Standard 2.0/2.1, .NET Framework 4.6.2 and 4.8.

## Quick start

```csharp
using Envisia.InvoiceXml;

InvoiceDescriptor invoice = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2026, 3, 5), CurrencyCodes.EUR);
invoice.BusinessProcess = "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0";
invoice.ReferenceOrderNo = "04011000-12345-34";                       // BT-10 buyer reference (Leitweg-ID)

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

### Calculation checks

`InvoiceValidator.Validate(invoice, ZUGFeRDVersion.Version23)` recalculates line totals, allowances/charges, the VAT
breakdown and the document totals (BR-CO-*) and reports deviations. For full compliance
checks of the generated XML use the official validators: the KoSIT validator for XRechnung and
the Factur-X / ZUGFeRD Schematron (e.g. via [Mustang](https://www.mustangproject.org)).

More documentation:

- [Getting started](docs/getting-started.md)
- [Profiles and formats](docs/profiles-and-formats.md)
- [ZUGFeRD details](docs/zugferd-details.md)
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

## License and attribution

Licensed under the [Apache License 2.0](LICENSE.txt).

Envisia.InvoiceXml is a derivative of [ZUGFeRD-csharp](https://github.com/stephanstapel/ZUGFeRD-csharp)
by Stephan Stapel / STwo Industries GmbH and its contributors. See [NOTICE](NOTICE).
