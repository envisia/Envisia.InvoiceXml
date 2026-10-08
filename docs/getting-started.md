# Introduction

Envisia.InvoiceXml creates and reads the XML of structured electronic invoices according to EN 16931:
ZUGFeRD 1.0/2.x, Factur-X (MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED) and XRechnung in CII and UBL syntax.

The library tries to be as simple as possible. Still, an e-invoice is a complete invoice in XML, so it is worth
going through the creation process step by step. The unit tests in `tests/Envisia.InvoiceXml.Tests` contain many
more examples.



# Relationship between the different standards

- ZUGFeRD was developed by the German Forum elektronische Rechnung Deutschland (FeRD, https://www.ferd-net.de/).
- Since ZUGFeRD 2.1 it is technically identical to the French Factur-X standard (ZUGFeRD 2.1 = Factur-X 1.0,
  ZUGFeRD 2.4 = Factur-X 1.08, ZUGFeRD 2.5 = Factur-X 1.09).
- Factur-X / ZUGFeRD conforms to the European norm EN 16931, which in turn is based on the UN/CEFACT
  Cross Industry Invoice (CII). EN 16931 also defines a binding to OASIS UBL 2.1.
- XRechnung is the German CIUS (core invoice usage specification) of EN 16931, maintained by KoSIT
  (https://xeinkauf.de/xrechnung/). It comes with its own validation rules and can be expressed in CII or UBL.
- Factur-X / ZUGFeRD and XRechnung are both EN 16931 conformant, but that does not make the invoices identical;
  ZUGFeRD therefore contains an XRECHNUNG reference profile.



# Installation

```shell
dotnet add package Envisia.InvoiceXml
```

or search for `Envisia.InvoiceXml` in the NuGet package manager.



# Building on your own

Prerequisites: .NET 10 SDK.

```shell
dotnet build Envisia.InvoiceXml.sln
dotnet test Envisia.InvoiceXml.sln
```



# Step-by-step guide for creating invoices

Central class for users is class `InvoiceDescriptor`.

This class does not only allow to read and set all ZUGFeRD attributes and structures but also allows to load and save ZUGFeRD files.



However, the standard has become quite large during the recent years. So it is worthwhile to go through the creation process step by step.



## Creating an invoice



```csharp

InvoiceDescriptor desc = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2013, 6, 5), CurrencyCodes.EUR, "GE2020211-471102");

desc.Name = "WARENRECHNUNG";

desc.ReferenceOrderNo = "AB-312";

desc.AddNote("Rechnung gemäß Bestellung Nr. 2013-471331 vom 01.03.2013.");

desc.AddNote("Es bestehen Rabatt- und Bonusvereinbarungen.", SubjectCodes.AAK);

desc.SetBuyer("Kunden Mitte AG", "69876", "Frankfurt", "Kundenstraße 15", CountryCodes.DE, "88", new GlobalID(GlobalIDSchemeIdentifiers.GLN, "4000001123452"));

desc.AddBuyerTaxRegistration("DE234567890", TaxRegistrationSchemeID.VA);

desc.SetBuyerContact("Hans Muster");

desc.SetSeller("Lieferant GmbH", "80333", "München", "Lieferantenstraße 20", CountryCodes.DE, "88", new GlobalID(GlobalIDSchemeIdentifiers.GLN, "4000001123452"));

desc.AddSellerTaxRegistration("201/113/40209", TaxRegistrationSchemeID.FC);

desc.AddSellerTaxRegistration("DE123456789", TaxRegistrationSchemeID.VA);

desc.SetBuyerOrderReferenceDocument("2013-471331", new DateTime(2013, 03, 01));

desc.SetDeliveryNoteReferenceDocument("2013-51111", new DateTime(2013, 6, 3));

desc.ActualDeliveryDate = new DateTime(2013, 6, 3);

desc.SetTotals(202.76m, 5.80m, 14.73m, 193.83m, 21.31m, 215.14m, 50.0m, 165.14m);

desc.AddApplicableTradeTax(129.37m, 7m, TaxTypes.VAT, TaxCategoryCodes.S);

desc.AddApplicableTradeTax(64.46m, 19m, TaxTypes.VAT, TaxCategoryCodes.S);

desc.AddLogisticsServiceCharge(5.80m, "Versandkosten", TaxTypes.VAT, TaxCategoryCodes.S, 7m);

desc.AddTradePaymentTerms("Zahlbar innerhalb 30 Tagen netto bis 04.04.2018", new DateTime(2018, 4, 4));

desc.AddTradePaymentTerms("3% Skonto innerhalb 10 Tagen bis 15.03.2018", new DateTime(2018, 3, 15), PaymentTermsType.Skonto, 30, 3m);

```



Optionally, to support Peppol, an electronic address can be passed:



```csharp

desc.SetSellerElectronicAddress("DE123456789", ElectronicAddressSchemeIdentifiers.GermanyVatNumber);

desc.SetBuyerElectronicAddress("LU987654321", ElectronicAddressSchemeIdentifiers.LuxemburgVatNumber);

```



The fields are only necessary if you want to send the XRechnung via the Peppol network.

A description of the fields can be found in the following documents:



[https://docs.peppol.eu/edelivery/policies/PEPPOL-EDN-Policy-for-use-of-identifiers-4.3.0-2024-10-03.pdf](https://docs.peppol.eu/edelivery/policies/PEPPOL-EDN-Policy-for-use-of-identifiers-4.3.0-2024-10-03.pdf)





In Luxembourg, it has been mandatory since this year to process all invoices via Peppol:



https://gouvernement.lu/de/dossiers.gouv/_digitalisation%2Bde%2Bdossiers%2B2021%2Bfacturation-electronique.html



In Germany, this has so far only been necessary for invoices in the course of a public contract from the federal government:



https://www.e-rechnung-bund.de/ubertragungskanale/peppol/



## Adding line items

### Handling of line ids

The library allows to operate in two modes: you can either let the library generate the line ids automatically or you can alternatively pass distinct line ids. This is helpful if you want to convert existing invoices, e.g. from ERP systems, to ZUGFeRD/ Factur-X.



To let the library create line ids, you can use:



```csharp

InvoiceDescriptor desc = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2013, 6, 5), CurrencyCodes.EUR, "GE2020211-471102");

desc.AddTradeLineItem("Item name", 23.99m, QuantityCodes.H87, "Detail description", ....);

```



This will generate an invoice with trade line item numbered as '1'.



To pass pre-defined line ids, this is the way to go:



```csharp

InvoiceDescriptor desc = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2013, 6, 5), CurrencyCodes.EUR, "GE2020211-471102");

desc.AddTradeLineItem(lineID: "0001", 23.99m, QuantityCodes.H87, "Item name", "Detail description", ....);

desc.AddTradeLineItem(lineID: "0002", 49.99m, QuantityCodes.H87, "Item name", "Detail description", ....);

```

which will generate an invoice with two trade line items, with the first one as number '0001' and the second one as number '0002'.


## Storing the invoice
```csharp
using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
{
    desc.Save(stream, ZUGFeRDVersion.Version23, Profile.XRechnung);
}
```

`ZUGFeRDVersion.Version23` covers ZUGFeRD 2.1 up to 2.5.2 (Factur-X 1.0 up to 1.09.2): the guideline identifiers did
not change between these versions. XRechnung is written as XRechnung 3.0 (valid for 3.0.x, currently 3.0.2).



# Hybrid invoices (PDF/A-3)

ZUGFeRD and Factur-X invoices are usually exchanged as PDF/A-3 files with the invoice XML embedded as
`factur-x.xml` (ZUGFeRD 2.x) or `xrechnung.xml`. Envisia.InvoiceXml deliberately only deals with the XML; use a
PDF library that supports PDF/A-3 attachments and the Factur-X XMP metadata to create or read the PDF.
`ProfileExtensions.GetXMPName()` returns the conformance level name required for the XMP metadata.
