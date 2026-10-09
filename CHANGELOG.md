# Changelog

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project
follows [Semantic Versioning](https://semver.org/).

## [1.0.0] - unreleased

First release of **Envisia.InvoiceXml**, based on ZUGFeRD-csharp 18.0.0
(https://github.com/stephanstapel/ZUGFeRD-csharp).

### Changed

- Package id, assembly name and root namespace are now `Envisia.InvoiceXml`
  (previously `ZUGFeRD-csharp` / `s2industries.ZUGFeRD`). The assembly is signed with a new key.
- Target frameworks: .NET 10, .NET 8, .NET Standard 2.0/2.1, .NET Framework 4.6.2 and 4.8
  (.NET Framework 4.6.1 was replaced by 4.6.2).
- XRechnung is always written with the XRechnung 3.0 identifier (valid for 3.0.x, currently 3.0.2);
  the time-dependent fallback to XRechnung 2.3 was removed.
- The CII writer now strictly follows the Factur-X profile schemas:
  - MINIMUM: the mandatory `ApplicableHeaderTradeDelivery` is always written; party identifiers,
    electronic addresses, address details except the seller country, the buyer address and VAT id,
    the tax representative, the contract reference, the delivery date, the VAT breakdown and
    `TotalPrepaidAmount` are no longer written (they do not exist in MINIMUM).
  - BASIC WL, BASIC, EN 16931: a single `SpecifiedTradePaymentTerms` element is written; the
    descriptions of several payment terms are combined into BT-20 and the first due date is used
    as BT-9. A direct debit mandate (BT-89) is also written without payment terms.
  - Logistics service charges exist in EXTENDED only; in the other profiles they are written as
    document level charges (BG-21) so that BT-108 stays consistent.
  - Outside of EXTENDED only one item price discount (BT-147) is allowed; several gross price
    allowances and charges are combined. BASIC now also contains the gross price (BT-148).
  - Item attributes (BG-32) are no longer written in BASIC, `ClassName` is no longer written in
    the EN 16931 profile, the payer account only contains the IBAN (BT-91); EXTENDED additionally
    contains the account name, which was added with Factur-X 1.09.
- Seller and buyer contact: person name and department name both represent BT-41 / BT-56 and must
  not occur together (CII-SR-465/466). The department is now only written when there is no person name.

### Added

- New package **Envisia.InvoiceXml.Validation** (.NET 10): validates invoice XML against the official XML schemas and
  Schematron rules and creates reports in the format of the KoSIT validator (XML report and HTML report in
  German or English). Built-in configurations:
  - XRechnung 3.0.2: port of the KoSIT validator configuration (release 2026-08-31) with the CEN EN 16931
    Schematron 1.3.16 and the XRechnung Schematron 2.6.0,
  - ZUGFeRD 2.5.2 / Factur-X 1.09.2: XSD and Schematron of all profiles,
  - EN 16931: any CIUS in UBL and CII with the CEN rules.

  The Schematron sources are executed by an own XPath 2.0/3.1 and ISO Schematron engine (no Java), which
  follows the semantics of Saxon-HE 12. Custom rules can be used with scenario configurations in the format of
  the KoSIT validator or directly (`SchematronSchema`, SVRL output). `InvoiceDescriptor.ValidateXml()` validates
  an invoice in a given version, profile and syntax. Tested against the unit tests of the CEN artefacts and the
  reports of the KoSIT validator for the XRechnung test suite and the official Factur-X samples.
- New package **Envisia.InvoiceXml.Validation.Abstractions** (.NET 10): `IInvoiceValidator` and
  `InvoiceValidationResult` (recommendation, scenario, messages, KoSIT XML and HTML report), implemented by
  `InvoiceXmlValidator`. Both validation packages register it with `services.AddInvoiceValidator()`, so the
  implementation is chosen by the package reference.
- New package **Envisia.InvoiceXml.Validation.GraalVM** (.NET 10): the official KoSIT validator 1.6.3, compiled
  ahead of time into a native library with GraalVM native-image (linux-x64, linux-arm64, win-x64, osx-arm64; no
  JVM needed), with the KoSIT configuration for XRechnung 3.0.2 and a Factur-X 1.09.2 configuration. Its reports
  equal those of the KoSIT validator on the JVM for the XRechnung test suite, the tests of the XRechnung
  configuration, the CEN unit tests and the official Factur-X samples (`tests/KositConformance`).
- `ZUGFeRDVersion.Version25` for ZUGFeRD 2.5 / Factur-X 1.09 (2.5.2 / 1.09.2). It uses the same guideline identifiers
  as `Version23` and additionally writes the EXTENDED elements introduced with Factur-X 1.09:
  - debtor BIC (`PayerSpecifiedDebtorFinancialInstitution`) and debtor account name,
  - manufacturer of an item (`TradeLineItem.Manufacturer`),
  - financial adjustments (`InvoiceDescriptor.AddFinancialAdjustment()`, BR-FXEXT-CO-16 in `InvoiceValidator`).
- EXTENDED elements of ZUGFeRD 2.4 / Factur-X 1.08 (written when set):
  - per-package unit quantity (`TradeLineItem.SetPerPackageUnitQuantity()`, BT-X-561),
  - delivery location of the delivery terms (`InvoiceDescriptor.SetApplicableTradeDeliveryTerms()`, BG-X-88),
  - line level delivery terms (`TradeLineItem.SetApplicableTradeDeliveryTerms()`, BG-X-87),
  - item seller (`TradeLineItem.ItemSeller`, BG-X-90),
  - line totals `ChargeTotalAmount`, `AllowanceTotalAmount`, `TaxTotalAmount`, `TaxTotalAmountInAccountingCurrency`,
    `GrandTotalAmount` on `TradeLineItem`,
  - type code (BT-X-11) and measured value (BT-X-12) of item attributes.
- Code lists updated to the EN 16931 code lists v16/v17 (as used by Factur-X 1.09.2 and XRechnung 3.0.2):
  EAS 0242, 0244–0246, 0248; ICD 0241–0248; VATEX-EU-135-1; currencies CNH and XCG (ANG and BGN are
  documented as withdrawn); invoice type 935; the complete UNTDID 1153 and 4451 lists (e.g. AXU); unit H16.
- UBL: value added tax point date code (BT-8) as `cac:InvoicePeriod/cbc:DescriptionCode`.
- All line level receivable accounting accounts are read (previously only the first).
- Conformance tests against the official XSDs of Factur-X 1.08 (ZUGFeRD 2.4) and Factur-X 1.09.2
  (ZUGFeRD 2.5.2) for every profile, round trips of all official ZUGFeRD 2.4 examples and XSD
  validation of XRechnung UBL output (OASIS UBL 2.1). The 1.09.2 and UBL 2.1 schemas are part of
  `documentation/`.
- `urn:zugferd.de:2p0:basicwl` and the XRechnung 3.0 extension identifier are recognised when reading.
- CI workflow, tag-based release workflow (nuget.org trusted publishing), NuGet package metadata,
  source link, symbol packages and package validation.

### Fixed

- Header `ApplicableTradeDeliveryTerms` is written in schema order.
- UBL: `cac:AddressLine` (BT-163/BT-165) was written before `CityName`, which violates the UBL
  schema; the deliver-to address wrote line 3 into `AdditionalStreetName` and lost line 2.
- Missing document type codes and accounting account type codes were read as `0` and written back
  as invalid codes.
- XRechnung payment terms no longer produce an empty `Description` element.
- UBL credit notes are detected by their root element.
- UBL: every UNTDID 1001 invoice type can be written (credit notes according to the EN 16931 interpretation as
  `ubl:CreditNote`); previously many valid codes threw `NotImplementedException`.
- UBL: the seller tax representative (BG-11) name is written and read as `cac:PartyName` (BT-62) and its VAT
  identifier (BT-63) is written and read; empty address elements and tax registrations without number are no
  longer written.
- CII: `ReferenceTypeCode` of referenced documents is written in schema order (after name and attachment).
- Test input files are opened read-only, so tests for several target frameworks can run in parallel.

### Removed

- The PDF (`ZUGFeRD.PDF-csharp`), rendering and Excel companion projects.
- `InvoiceDescriptor.AddTradeeCharge()`, which was already obsolete (error) in ZUGFeRD-csharp.
