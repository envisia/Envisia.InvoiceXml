/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
using System.Xml;

namespace Envisia.InvoiceXml.Tests
{
    /// <summary>
    /// Round trip tests for codes of the EN 16931 code lists v16/v17 (as used by Factur-X 1.08/1.09.2 and XRechnung 3.0.2).
    ///
    /// Each code is written as CII (Factur-X EN 16931 profile, validated against the Factur-X 1.08 and 1.09.2 schemas)
    /// and, where the element exists in UBL, as XRechnung UBL. The tests check that the exact code string is written
    /// and that reading the invoice back yields the same enum value.
    /// </summary>
    [TestClass]
    public class CodeListTests : TestBase
    {
        private readonly InvoiceProvider _InvoiceProvider = new InvoiceProvider();


        /// <summary>
        /// CEF EAS codes added with the EN 16931 code lists v16/v17 (BT-34-1, BT-49-1).
        /// </summary>
        [TestMethod]
        [DataRow(ElectronicAddressSchemeIdentifiers.OpenPeppolServiceProviderIdentificationScheme, "0242")]
        [DataRow(ElectronicAddressSchemeIdentifiers.NigeriaTaxIdentification, "0244")]
        [DataRow(ElectronicAddressSchemeIdentifiers.SlovakiaTaxIdentificationNumber, "0245")]
        [DataRow(ElectronicAddressSchemeIdentifiers.GermanElectronicBusinessAddress, "0246")]
        [DataRow(ElectronicAddressSchemeIdentifiers.OmanVatIdentificationNumber, "0248")]
        public void TestElectronicAddressSchemeIdentifier(ElectronicAddressSchemeIdentifiers scheme, string expectedCode)
        {
            Action<InvoiceDescriptor> modify = desc =>
            {
                desc.SetSellerElectronicAddress("seller@example.com", scheme);
                desc.SetBuyerElectronicAddress("buyer@example.com", scheme);
            };

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:SellerTradeParty/ram:URIUniversalCommunication/ram:URIID/@schemeID"));
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:BuyerTradeParty/ram:URIUniversalCommunication/ram:URIID/@schemeID"));
            Assert.AreEqual(scheme, loadedCii.SellerElectronicAddress.ElectronicAddressSchemeID);
            Assert.AreEqual(scheme, loadedCii.BuyerElectronicAddress.ElectronicAddressSchemeID);

            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "//cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID"));
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "//cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID"));
            Assert.AreEqual(scheme, loadedUbl.SellerElectronicAddress.ElectronicAddressSchemeID);
            Assert.AreEqual(scheme, loadedUbl.BuyerElectronicAddress.ElectronicAddressSchemeID);
        } // !TestElectronicAddressSchemeIdentifier()


        /// <summary>
        /// ISO/IEC 6523 ICD codes added with the EN 16931 code lists v16/v17 (e.g. BT-29-1, BT-30-1, BT-157-1).
        /// </summary>
        [TestMethod]
        [DataRow(GlobalIDSchemeIdentifiers.HitachiRail, "0241")]
        [DataRow(GlobalIDSchemeIdentifiers.OpenPeppolServiceProviderIdentificationScheme, "0242")]
        [DataRow(GlobalIDSchemeIdentifiers.CatenaXBusinessPartnerNumber, "0243")]
        [DataRow(GlobalIDSchemeIdentifiers.NigeriaTaxIdentification, "0244")]
        [DataRow(GlobalIDSchemeIdentifiers.SlovakiaTaxIdentificationNumber, "0245")]
        [DataRow(GlobalIDSchemeIdentifiers.GermanElectronicBusinessAddress, "0246")]
        [DataRow(GlobalIDSchemeIdentifiers.DescriptionNotKnown3, "0247")]
        [DataRow(GlobalIDSchemeIdentifiers.OmanVatIdentificationNumber, "0248")]
        public void TestGlobalIDSchemeIdentifier(GlobalIDSchemeIdentifiers scheme, string expectedCode)
        {
            Action<InvoiceDescriptor> modify = desc =>
            {
                desc.Seller.GlobalID = new GlobalID(scheme, "4711");
                desc.Seller.SpecifiedLegalOrganization = new LegalOrganization(scheme, "0815", "Lieferant GmbH");
                desc.TradeLineItems[0].GlobalID = new GlobalID(scheme, "4712");
            };

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:SellerTradeParty/ram:GlobalID/@schemeID"));
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:SellerTradeParty/ram:SpecifiedLegalOrganization/ram:ID/@schemeID"));
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:IncludedSupplyChainTradeLineItem/ram:SpecifiedTradeProduct/ram:GlobalID/@schemeID"));
            Assert.AreEqual(scheme, loadedCii.Seller.GlobalID.SchemeID);
            Assert.AreEqual("4711", loadedCii.Seller.GlobalID.ID);
            Assert.AreEqual(scheme, loadedCii.Seller.SpecifiedLegalOrganization.ID.SchemeID);
            Assert.AreEqual("0815", loadedCii.Seller.SpecifiedLegalOrganization.ID.ID);
            Assert.AreEqual(scheme, loadedCii.TradeLineItems[0].GlobalID.SchemeID);
            Assert.AreEqual("4712", loadedCii.TradeLineItems[0].GlobalID.ID);

            // the UBL writer writes the scheme of the seller's legal registration identifier (BT-30-1)
            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID"));
            Assert.AreEqual(scheme, loadedUbl.Seller.SpecifiedLegalOrganization.ID.SchemeID);
            Assert.AreEqual("0815", loadedUbl.Seller.SpecifiedLegalOrganization.ID.ID);
        } // !TestGlobalIDSchemeIdentifier()


        /// <summary>
        /// VATEX-EU-135-1 was added with the EN 16931 code list v16 (VATEX v8, BT-121).
        /// </summary>
        [TestMethod]
        public void TestTaxExemptionReasonCodeVatexEu135_1()
        {
            const string expectedCode = "VATEX-EU-135-1";
            Action<InvoiceDescriptor> modify = desc =>
            {
                desc.AddApplicableTradeTax(100m, 0m, 0m, TaxTypes.VAT, TaxCategoryCodes.E,
                                           exemptionReasonCode: TaxExemptionReasonCodes.VATEX_EU_135_1,
                                           exemptionReason: "Exempt based on article 135, section 1 of Council Directive 2006/112/EC");
            };

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:ApplicableHeaderTradeSettlement/ram:ApplicableTradeTax[ram:CategoryCode='E']/ram:ExemptionReasonCode"));
            Assert.AreEqual(TaxExemptionReasonCodes.VATEX_EU_135_1, loadedCii.Taxes.Single(t => t.CategoryCode == TaxCategoryCodes.E).ExemptionReasonCode);

            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "//cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory[cbc:ID='E']/cbc:TaxExemptionReasonCode"));
            Assert.AreEqual(TaxExemptionReasonCodes.VATEX_EU_135_1, loadedUbl.Taxes.Single(t => t.CategoryCode == TaxCategoryCodes.E).ExemptionReasonCode);
        } // !TestTaxExemptionReasonCodeVatexEu135_1()


        /// <summary>
        /// CNH and XCG were added with the EN 16931 code lists v16/v17 (BT-5).
        /// ANG and BGN were withdrawn with v17 but must still be readable.
        /// </summary>
        [TestMethod]
        [DataRow(CurrencyCodes.CNH, "CNH")]
        [DataRow(CurrencyCodes.XCG, "XCG")]
        [DataRow(CurrencyCodes.ANG, "ANG")]
        [DataRow(CurrencyCodes.BGN, "BGN")]
        public void TestCurrencyCode(CurrencyCodes currency, string expectedCode)
        {
            Action<InvoiceDescriptor> modify = desc => desc.Currency = currency;

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:ApplicableHeaderTradeSettlement/ram:InvoiceCurrencyCode"));
            Assert.AreEqual(currency, loadedCii.Currency);

            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "/inv:Invoice/cbc:DocumentCurrencyCode"));
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "//cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID"));
            Assert.AreEqual(currency, loadedUbl.Currency);
        } // !TestCurrencyCode()


        /// <summary>
        /// UNTDID 1153 codes for the invoiced object identifier scheme (BT-18-1), AXU is new in Factur-X 1.08.
        /// </summary>
        [TestMethod]
        [DataRow(ReferenceTypeCodes.AXU, "AXU")]
        [DataRow(ReferenceTypeCodes.AAC, "AAC")]
        [DataRow(ReferenceTypeCodes.XA, "XA")]
        [DataRow(ReferenceTypeCodes.ZZZ, "ZZZ")]
        public void TestReferenceTypeCode(ReferenceTypeCodes referenceTypeCode, string expectedCode)
        {
            Action<InvoiceDescriptor> modify = desc =>
            {
                desc.AddAdditionalReferencedDocument(id: "OBJ-4711",
                                                     typeCode: AdditionalReferencedDocumentTypeCode.InvoiceDataSheet,
                                                     referenceTypeCode: referenceTypeCode);
            };

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:ApplicableHeaderTradeAgreement/ram:AdditionalReferencedDocument/ram:ReferenceTypeCode"));
            Assert.HasCount(1, loadedCii.AdditionalReferencedDocuments);
            Assert.AreEqual(referenceTypeCode, loadedCii.AdditionalReferencedDocuments[0].ReferenceTypeCode);
            Assert.AreEqual("OBJ-4711", loadedCii.AdditionalReferencedDocuments[0].ID);

            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "//cac:AdditionalDocumentReference/cbc:ID/@schemeID"));
            Assert.HasCount(1, loadedUbl.AdditionalReferencedDocuments);
            Assert.AreEqual(referenceTypeCode, loadedUbl.AdditionalReferencedDocuments[0].ReferenceTypeCode);
            Assert.AreEqual("OBJ-4711", loadedUbl.AdditionalReferencedDocuments[0].ID);
        } // !TestReferenceTypeCode()


        /// <summary>
        /// UNTDID 4451 codes for the invoice note subject (BT-21).
        /// The UBL writer does not encode subject codes in cbc:Note, therefore only CII is checked.
        /// </summary>
        [TestMethod]
        [DataRow(SubjectCodes.AAA, "AAA")]
        [DataRow(SubjectCodes.AAR, "AAR")]
        [DataRow(SubjectCodes.GS7, "GS7")]
        [DataRow(SubjectCodes.TRR, "TRR")]
        public void TestSubjectCode(SubjectCodes subjectCode, string expectedCode)
        {
            const string noteContent = "Note with a subject code";
            Action<InvoiceDescriptor> modify = desc => desc.AddNote(noteContent, subjectCode);

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, $"//rsm:ExchangedDocument/ram:IncludedNote[ram:Content='{noteContent}']/ram:SubjectCode"));
            Assert.AreEqual(subjectCode, loadedCii.Notes.Single(n => n.Content == noteContent).SubjectCode);
        } // !TestSubjectCode()


        /// <summary>
        /// 935 (customs invoice) is part of the EN 16931 subset of UNTDID 1001 (BT-3) and is interpreted as an invoice,
        /// so it has to be written as ubl:Invoice and not as ubl:CreditNote.
        /// </summary>
        [TestMethod]
        public void TestInvoiceTypeCustomsInvoice()
        {
            const string expectedCode = "935";
            Action<InvoiceDescriptor> modify = desc => desc.Type = InvoiceType.CustomsInvoice;

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:TypeCode"));
            Assert.AreEqual(InvoiceType.CustomsInvoice, loadedCii.Type);

            // XRechnung itself only allows a subset of type codes (BR-DE-17), the UBL writer does not restrict them
            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual("Invoice", ublXml.DocumentElement?.LocalName);
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "/inv:Invoice/cbc:InvoiceTypeCode"));
            Assert.IsNull(ublXml.SelectSingleNode("//cbc:CreditNoteTypeCode", _CreateNamespaceManager(ublXml)));
            Assert.AreEqual(InvoiceType.CustomsInvoice, loadedUbl.Type);
        } // !TestInvoiceTypeCustomsInvoice()


        /// <summary>
        /// H16 (square decametre, i.e. 'Ar') as unit code (BT-130).
        /// </summary>
        [TestMethod]
        public void TestQuantityCodeSquareDecametre()
        {
            const string expectedCode = "H16";
            Action<InvoiceDescriptor> modify = desc => desc.TradeLineItems[0].UnitCode = QuantityCodes.H16;

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCode, _SelectValue(ciiXml, "//ram:IncludedSupplyChainTradeLineItem/ram:SpecifiedLineTradeDelivery/ram:BilledQuantity/@unitCode"));
            Assert.AreEqual(QuantityCodes.H16, loadedCii.TradeLineItems[0].UnitCode);

            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual(expectedCode, _SelectValue(ublXml, "//cac:InvoiceLine/cbc:InvoicedQuantity/@unitCode"));
            Assert.AreEqual(QuantityCodes.H16, loadedUbl.TradeLineItems[0].UnitCode);
        } // !TestQuantityCodeSquareDecametre()


        /// <summary>
        /// Value added tax point date code (BT-8): CII uses UNTDID 2475 (5, 29, 72), UBL uses UNTDID 2005 (3, 35, 432)
        /// in cac:InvoicePeriod/cbc:DescriptionCode.
        /// </summary>
        [TestMethod]
        [DataRow(DateTypeCodes.InvoiceDate, "5", "3")]
        [DataRow(DateTypeCodes.DeliveryDate, "29", "35")]
        [DataRow(DateTypeCodes.PaymentDate, "72", "432")]
        public void TestTaxPointDateCode(DateTypeCodes dateTypeCode, string expectedCiiCode, string expectedUblCode)
        {
            Action<InvoiceDescriptor> modify = desc =>
            {
                desc.BillingPeriodStart = new DateTime(2018, 03, 01);
                desc.BillingPeriodEnd = new DateTime(2018, 03, 31);
                desc.Taxes.ForEach(t => t.DueDateTypeCode = dateTypeCode);
            };

            InvoiceDescriptor loadedCii = _SaveAndLoad(modify, Profile.Comfort, ZUGFeRDFormats.CII, out XmlDocument ciiXml);
            Assert.AreEqual(expectedCiiCode, _SelectValue(ciiXml, "//ram:ApplicableHeaderTradeSettlement/ram:ApplicableTradeTax/ram:DueDateTypeCode"));
            Assert.IsTrue(loadedCii.Taxes.All(t => t.DueDateTypeCode == dateTypeCode));

            InvoiceDescriptor loadedUbl = _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.AreEqual(expectedUblCode, _SelectValue(ublXml, "/inv:Invoice/cac:InvoicePeriod/cbc:DescriptionCode"));
            XmlNodeList? periodElements = ublXml.SelectNodes("/inv:Invoice/cac:InvoicePeriod/*", _CreateNamespaceManager(ublXml));
            Assert.IsNotNull(periodElements);
            CollectionAssert.AreEqual(new[] { "StartDate", "EndDate", "DescriptionCode" }, periodElements.Cast<XmlNode>().Select(n => n.LocalName).ToArray());
            Assert.IsNotEmpty(loadedUbl.Taxes);
            Assert.IsTrue(loadedUbl.Taxes.All(t => t.DueDateTypeCode == dateTypeCode));
            Assert.AreEqual(new DateTime(2018, 03, 01), loadedUbl.BillingPeriodStart);
            Assert.AreEqual(new DateTime(2018, 03, 31), loadedUbl.BillingPeriodEnd);
        } // !TestTaxPointDateCode()


        /// <summary>
        /// Value added tax point date (BT-7) and code (BT-8) are mutually exclusive (BR-CO-3), the date takes precedence.
        /// Without a billing period no empty cac:InvoicePeriod must be written.
        /// </summary>
        [TestMethod]
        public void TestTaxPointDateCodeIsNotWrittenWithTaxPointDate()
        {
            Action<InvoiceDescriptor> modify = desc => desc.Taxes.ForEach(t => t.SetTaxPointDate(new DateTime(2018, 03, 05), DateTypeCodes.DeliveryDate));

            _SaveAndLoad(modify, Profile.XRechnung, ZUGFeRDFormats.UBL, out XmlDocument ublXml);
            Assert.IsNull(ublXml.SelectSingleNode("/inv:Invoice/cac:InvoicePeriod", _CreateNamespaceManager(ublXml)));
        } // !TestTaxPointDateCodeIsNotWrittenWithTaxPointDate()


        /// <summary>
        /// Creates the test invoice, applies the given modification, saves it in the given profile and format and loads it again.
        /// CII output is additionally validated against the Factur-X 1.08 and 1.09.2 schemas.
        /// </summary>
        private InvoiceDescriptor _SaveAndLoad(Action<InvoiceDescriptor> modify, Profile profile, ZUGFeRDFormats format, out XmlDocument xml)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            modify(desc);

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, profile, format);

            if (format == ZUGFeRDFormats.CII)
            {
                foreach (string schemaVersion in new[] { SchemaValidator.FacturX108, SchemaValidator.FacturX1092 })
                {
                    List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetFacturXSchemaPath(schemaVersion, profile));
                    Assert.IsEmpty(errors, $"Factur-X {schemaVersion}: " + string.Join(Environment.NewLine, errors));
                }
            }

            ms.Seek(0, SeekOrigin.Begin);
            xml = new XmlDocument();
            xml.Load(ms);

            ms.Seek(0, SeekOrigin.Begin);
            return InvoiceDescriptor.Load(ms);
        } // !_SaveAndLoad()


        private static string? _SelectValue(XmlDocument xml, string xpath)
        {
            XmlNode? node = xml.SelectSingleNode(xpath, _CreateNamespaceManager(xml));
            return node?.Value ?? node?.InnerText;
        } // !_SelectValue()


        private static XmlNamespaceManager _CreateNamespaceManager(XmlDocument xml)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100");
            nsmgr.AddNamespace("ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100");
            nsmgr.AddNamespace("inv", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            return nsmgr;
        } // !_CreateNamespaceManager()
    }
}
