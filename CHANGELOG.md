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
- Test input files are opened read-only, so tests for several target frameworks can run in parallel.

### Removed

- The PDF (`ZUGFeRD.PDF-csharp`), rendering and Excel companion projects.
- `InvoiceDescriptor.AddTradeeCharge()`, which was already obsolete (error) in ZUGFeRD-csharp.
