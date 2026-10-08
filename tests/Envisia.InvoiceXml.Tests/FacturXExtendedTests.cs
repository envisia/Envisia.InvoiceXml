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
using System.Text;
using System.Xml;

namespace Envisia.InvoiceXml.Tests
{
    /// <summary>
    /// Tests for EXTENDED elements of Factur-X 1.08 / ZUGFeRD 2.4 and Factur-X 1.09.2 / ZUGFeRD 2.5.2.
    ///
    /// Every test writes the element in profile EXTENDED, validates the result against the official schema(s),
    /// checks the position in the xml, reads the invoice back and finally makes sure that the element is
    /// omitted in profile EN 16931 (COMFORT) while the output stays schema valid.
    /// </summary>
    [TestClass]
    public class FacturXExtendedTests : TestBase
    {
        private const string TransactionPath = "/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction";
        private const string FirstLinePath = TransactionPath + "/ram:IncludedSupplyChainTradeLineItem[1]";
        private const string HeaderAgreementPath = TransactionPath + "/ram:ApplicableHeaderTradeAgreement";
        private const string HeaderSettlementPath = TransactionPath + "/ram:ApplicableHeaderTradeSettlement";

        private readonly InvoiceProvider _InvoiceProvider = new InvoiceProvider();


        #region Factur-X 1.08 / ZUGFeRD 2.4
        /// <summary>
        /// BT-X-561, Number of units per package, directly after PackageQuantity (BT-X-47)
        /// </summary>
        [TestMethod]
        public void TestPerPackageUnitQuantity()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.TradeLineItems[0].SetPackageQuantity(2m, QuantityCodes.XCT);
            desc.TradeLineItems[0].SetPerPackageUnitQuantity(10m, QuantityCodes.H87);

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            Assert.AreEqual("10.0000", _Text(doc, nsmgr, FirstLinePath + "/ram:SpecifiedLineTradeDelivery/ram:PerPackageUnitQuantity"));
            Assert.AreEqual("H87", _Text(doc, nsmgr, FirstLinePath + "/ram:SpecifiedLineTradeDelivery/ram:PerPackageUnitQuantity/@unitCode"));
            Assert.AreEqual("PerPackageUnitQuantity", _Text(doc, nsmgr, "local-name(" + FirstLinePath + "/ram:SpecifiedLineTradeDelivery/ram:PackageQuantity/following-sibling::*[1])"));

            TradeLineItem loadedItem = _Load(xml).TradeLineItems[0];
            Assert.AreEqual(10m, loadedItem.PerPackageUnitQuantity);
            Assert.AreEqual(QuantityCodes.H87, loadedItem.PerPackageUnitCode);
            Assert.AreEqual(2m, loadedItem.PackageQuantity);
            Assert.AreEqual(QuantityCodes.XCT, loadedItem.PackageUnitCode);
            Assert.IsNull(_Load(xml).TradeLineItems[1].PerPackageUnitQuantity);

            _AssertNotWrittenInComfort(desc, TransactionPath + "/ram:IncludedSupplyChainTradeLineItem/ram:SpecifiedLineTradeDelivery/ram:PerPackageUnitQuantity");
        } // !TestPerPackageUnitQuantity()


        /// <summary>
        /// BG-X-88, location of the delivery terms on document level (BT-X-563, BT-X-564) together with BT-X-145
        /// </summary>
        [TestMethod]
        public void TestHeaderDeliveryTermsLocation()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.SetApplicableTradeDeliveryTerms(TradeDeliveryTermCodes.FCA, CountryCodes.DE, "Hamburg Hafen");

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string termsPath = HeaderAgreementPath + "/ram:ApplicableTradeDeliveryTerms";
            Assert.AreEqual("FCA", _Text(doc, nsmgr, termsPath + "/ram:DeliveryTypeCode"));
            Assert.AreEqual("DE", _Text(doc, nsmgr, termsPath + "/ram:RelevantTradeLocation/ram:CountryID"));
            Assert.AreEqual("Hamburg Hafen", _Text(doc, nsmgr, termsPath + "/ram:RelevantTradeLocation/ram:Name"));

            InvoiceDescriptor loaded = _Load(xml);
            Assert.AreEqual(TradeDeliveryTermCodes.FCA, loaded.ApplicableTradeDeliveryTermsCode);
            Assert.IsNotNull(loaded.ApplicableTradeDeliveryTermsLocation);
            Assert.AreEqual(CountryCodes.DE, loaded.ApplicableTradeDeliveryTermsLocation.Country);
            Assert.AreEqual("Hamburg Hafen", loaded.ApplicableTradeDeliveryTermsLocation.Name);

            _AssertNotWrittenInComfort(desc, HeaderAgreementPath + "/ram:ApplicableTradeDeliveryTerms");
        } // !TestHeaderDeliveryTermsLocation()


        /// <summary>
        /// The location alone cannot be written because the delivery terms code (BT-X-145) is mandatory within BG-X-22.
        /// </summary>
        [TestMethod]
        public void TestHeaderDeliveryTermsLocationWithoutCodeIsNotWritten()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.ApplicableTradeDeliveryTermsLocation = new TradeLocation() { Country = CountryCodes.DE, Name = "Hamburg" };

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);

            Assert.IsNull(doc.SelectSingleNode(HeaderAgreementPath + "/ram:ApplicableTradeDeliveryTerms", _CreateNamespaceManager(doc)));
            Assert.IsNull(_Load(xml).ApplicableTradeDeliveryTermsLocation);
        } // !TestHeaderDeliveryTermsLocationWithoutCodeIsNotWritten()


        /// <summary>
        /// BG-X-87, delivery terms on line level (BT-X-562, BG-X-89 with BT-X-565 and BT-X-566), first child of SpecifiedLineTradeAgreement
        /// </summary>
        [TestMethod]
        public void TestLineDeliveryTerms()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.TradeLineItems[0].SetApplicableTradeDeliveryTerms(TradeDeliveryTermCodes.DAP, CountryCodes.FR, "Paris");
            desc.TradeLineItems[0].SetContractReferencedDocument("V-2018-1", new DateTime(2018, 1, 15));
            desc.TradeLineItems[1].SetApplicableTradeDeliveryTerms(TradeDeliveryTermCodes.EXW);

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string termsPath = FirstLinePath + "/ram:SpecifiedLineTradeAgreement/ram:ApplicableTradeDeliveryTerms";
            Assert.AreEqual("ApplicableTradeDeliveryTerms", _Text(doc, nsmgr, "local-name(" + FirstLinePath + "/ram:SpecifiedLineTradeAgreement/*[1])"));
            Assert.AreEqual("DAP", _Text(doc, nsmgr, termsPath + "/ram:DeliveryTypeCode"));
            Assert.AreEqual("FR", _Text(doc, nsmgr, termsPath + "/ram:RelevantTradeLocation/ram:CountryID"));
            Assert.AreEqual("Paris", _Text(doc, nsmgr, termsPath + "/ram:RelevantTradeLocation/ram:Name"));
            Assert.AreEqual("EXW", _Text(doc, nsmgr, TransactionPath + "/ram:IncludedSupplyChainTradeLineItem[2]/ram:SpecifiedLineTradeAgreement/ram:ApplicableTradeDeliveryTerms/ram:DeliveryTypeCode"));
            Assert.IsNull(doc.SelectSingleNode(TransactionPath + "/ram:IncludedSupplyChainTradeLineItem[2]/ram:SpecifiedLineTradeAgreement/ram:ApplicableTradeDeliveryTerms/ram:RelevantTradeLocation", nsmgr));

            InvoiceDescriptor loaded = _Load(xml);
            Assert.AreEqual(TradeDeliveryTermCodes.DAP, loaded.TradeLineItems[0].ApplicableTradeDeliveryTermsCode);
            Assert.AreEqual(CountryCodes.FR, loaded.TradeLineItems[0].ApplicableTradeDeliveryTermsLocation?.Country);
            Assert.AreEqual("Paris", loaded.TradeLineItems[0].ApplicableTradeDeliveryTermsLocation?.Name);
            Assert.AreEqual(TradeDeliveryTermCodes.EXW, loaded.TradeLineItems[1].ApplicableTradeDeliveryTermsCode);
            Assert.IsNull(loaded.TradeLineItems[1].ApplicableTradeDeliveryTermsLocation);
            Assert.IsNull(loaded.ApplicableTradeDeliveryTermsCode, "Line delivery terms must not be read as document level delivery terms");

            _AssertNotWrittenInComfort(desc, TransactionPath + "/ram:IncludedSupplyChainTradeLineItem/ram:SpecifiedLineTradeAgreement/ram:ApplicableTradeDeliveryTerms");
        } // !TestLineDeliveryTerms()


        /// <summary>
        /// BG-X-90, deviating item seller, after NetPriceProductTradePrice
        /// </summary>
        [TestMethod]
        public void TestItemSellerTradeParty()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            TradeLineItem item = desc.TradeLineItems[0];
            item.ItemSeller = new Party()
            {
                ID = new GlobalID(null, "HAENDLER-42"),
                GlobalID = new GlobalID(GlobalIDSchemeIdentifiers.GLN, "4000001987658"),
                Name = "Marktplatz Händler GmbH",
                Street = "Händlerweg 1",
                Postcode = "10115",
                City = "Berlin",
                Country = CountryCodes.DE,
                SpecifiedLegalOrganization = new LegalOrganization(GlobalIDSchemeIdentifiers.GLN, "4000001987658", "Händler Berlin")
            };
            item.AddItemSellerTaxRegistration("DE987654321", TaxRegistrationSchemeID.VA);
            item.AddItemSellerTaxRegistration("30/123/45678", TaxRegistrationSchemeID.FC);

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string partyPath = FirstLinePath + "/ram:SpecifiedLineTradeAgreement/ram:ItemSellerTradeParty";
            Assert.AreEqual("ItemSellerTradeParty", _Text(doc, nsmgr, "local-name(" + FirstLinePath + "/ram:SpecifiedLineTradeAgreement/ram:NetPriceProductTradePrice/following-sibling::*[1])"));
            Assert.AreEqual("HAENDLER-42", _Text(doc, nsmgr, partyPath + "/ram:ID"));
            Assert.AreEqual("4000001987658", _Text(doc, nsmgr, partyPath + "/ram:GlobalID[@schemeID='0088']"));
            Assert.AreEqual("Marktplatz Händler GmbH", _Text(doc, nsmgr, partyPath + "/ram:Name"));
            Assert.AreEqual("Händler Berlin", _Text(doc, nsmgr, partyPath + "/ram:SpecifiedLegalOrganization/ram:TradingBusinessName"));
            Assert.AreEqual("Berlin", _Text(doc, nsmgr, partyPath + "/ram:PostalTradeAddress/ram:CityName"));
            Assert.AreEqual("DE", _Text(doc, nsmgr, partyPath + "/ram:PostalTradeAddress/ram:CountryID"));
            Assert.AreEqual("DE987654321", _Text(doc, nsmgr, partyPath + "/ram:SpecifiedTaxRegistration/ram:ID[@schemeID='VA']"));
            // BR-FXEXT-03 (Factur-X 1.09.2): only VAT IDs for parties other than the seller
            Assert.IsNull(doc.SelectSingleNode(partyPath + "/ram:SpecifiedTaxRegistration/ram:ID[@schemeID='FC']", nsmgr));
            Assert.IsNull(doc.SelectSingleNode(TransactionPath + "/ram:IncludedSupplyChainTradeLineItem[2]//ram:ItemSellerTradeParty", nsmgr));

            InvoiceDescriptor loaded = _Load(xml);
            Party? itemSeller = loaded.TradeLineItems[0].ItemSeller;
            Assert.IsNotNull(itemSeller);
            Assert.AreEqual("HAENDLER-42", itemSeller.ID.ID);
            Assert.AreEqual(GlobalIDSchemeIdentifiers.GLN, itemSeller.GlobalID.SchemeID);
            Assert.AreEqual("4000001987658", itemSeller.GlobalID.ID);
            Assert.AreEqual("Marktplatz Händler GmbH", itemSeller.Name);
            Assert.AreEqual("Händlerweg 1", itemSeller.Street);
            Assert.AreEqual("10115", itemSeller.Postcode);
            Assert.AreEqual("Berlin", itemSeller.City);
            Assert.AreEqual(CountryCodes.DE, itemSeller.Country);
            Assert.AreEqual("Händler Berlin", itemSeller.SpecifiedLegalOrganization?.TradingBusinessName);
            List<TaxRegistration> taxRegistrations = loaded.TradeLineItems[0].GetItemSellerTaxRegistration();
            Assert.HasCount(1, taxRegistrations);
            Assert.AreEqual(TaxRegistrationSchemeID.VA, taxRegistrations[0].SchemeID);
            Assert.AreEqual("DE987654321", taxRegistrations[0].No);
            Assert.IsNull(loaded.TradeLineItems[1].ItemSeller);

            // the item seller must not be mixed up with the seller
            Assert.AreEqual("Lieferant GmbH", loaded.Seller.Name);
            Assert.HasCount(2, loaded.SellerTaxRegistration);

            _AssertNotWrittenInComfort(desc, "//ram:ItemSellerTradeParty");
        } // !TestItemSellerTradeParty()


        /// <summary>
        /// Line totals BT-X-327, BT-X-328, BT-X-329, BT-X-330 and BT-X-98 in SpecifiedTradeSettlementLineMonetarySummation
        /// </summary>
        [TestMethod]
        public void TestLineMonetarySummation()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            TradeLineItem item = desc.TradeLineItems[0];
            item.ChargeTotalAmount = 1.5m;
            item.AllowanceTotalAmount = 2m;
            item.TaxTotalAmount = 37.62m;
            item.GrandTotalAmount = 235.62m;
            item.TotalAllowanceChargeAmount = 0.5m;
            item.TaxTotalAmountInAccountingCurrency = 40m; // not written: the invoice has no VAT accounting currency (BT-6)

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string summationPath = FirstLinePath + "/ram:SpecifiedLineTradeSettlement/ram:SpecifiedTradeSettlementLineMonetarySummation";
            Assert.AreEqual("198.00", _Text(doc, nsmgr, summationPath + "/ram:LineTotalAmount"));
            Assert.AreEqual("1.50", _Text(doc, nsmgr, summationPath + "/ram:ChargeTotalAmount"));
            Assert.AreEqual("2.00", _Text(doc, nsmgr, summationPath + "/ram:AllowanceTotalAmount"));
            Assert.AreEqual("37.62", _Text(doc, nsmgr, summationPath + "/ram:TaxTotalAmount[@currencyID='EUR']"));
            Assert.AreEqual("235.62", _Text(doc, nsmgr, summationPath + "/ram:GrandTotalAmount"));
            Assert.AreEqual("0.50", _Text(doc, nsmgr, summationPath + "/ram:TotalAllowanceChargeAmount"));
            Assert.HasCount(1, doc.SelectNodes(summationPath + "/ram:TaxTotalAmount", nsmgr)!.Cast<XmlNode>());
            Assert.IsEmpty(doc.SelectNodes(summationPath + "/ram:*[not(self::ram:TaxTotalAmount)]/@currencyID", nsmgr)!.Cast<XmlNode>(), "Only the line tax totals carry a currency");

            TradeLineItem loadedItem = _Load(xml).TradeLineItems[0];
            Assert.AreEqual(198m, loadedItem.LineTotalAmount);
            Assert.AreEqual(1.5m, loadedItem.ChargeTotalAmount);
            Assert.AreEqual(2m, loadedItem.AllowanceTotalAmount);
            Assert.AreEqual(37.62m, loadedItem.TaxTotalAmount);
            Assert.IsNull(loadedItem.TaxTotalAmountInAccountingCurrency);
            Assert.AreEqual(235.62m, loadedItem.GrandTotalAmount);
            Assert.AreEqual(0.5m, loadedItem.TotalAllowanceChargeAmount);

            _AssertNotWrittenInComfort(desc,
                                       "//ram:SpecifiedTradeSettlementLineMonetarySummation/ram:ChargeTotalAmount",
                                       "//ram:SpecifiedTradeSettlementLineMonetarySummation/ram:AllowanceTotalAmount",
                                       "//ram:SpecifiedTradeSettlementLineMonetarySummation/ram:TaxTotalAmount",
                                       "//ram:SpecifiedTradeSettlementLineMonetarySummation/ram:GrandTotalAmount",
                                       "//ram:SpecifiedTradeSettlementLineMonetarySummation/ram:TotalAllowanceChargeAmount");
        } // !TestLineMonetarySummation()


        /// <summary>
        /// BT-X-590, line tax total in VAT accounting currency, written together with BT-6 and BT-111
        /// </summary>
        [TestMethod]
        public void TestLineTaxTotalInAccountingCurrency()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.SetTaxTotalInAccountingCurrency(61.42m, CurrencyCodes.USD);
            desc.TradeLineItems[0].TaxTotalAmount = 37.62m;
            desc.TradeLineItems[0].TaxTotalAmountInAccountingCurrency = 40.63m;
            desc.TradeLineItems[1].TaxTotalAmount = 19.25m;

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string summationPath = FirstLinePath + "/ram:SpecifiedLineTradeSettlement/ram:SpecifiedTradeSettlementLineMonetarySummation";
            Assert.AreEqual("USD", _Text(doc, nsmgr, HeaderSettlementPath + "/ram:TaxCurrencyCode"));
            Assert.AreEqual("37.62", _Text(doc, nsmgr, summationPath + "/ram:TaxTotalAmount[1][@currencyID='EUR']"));
            Assert.AreEqual("40.63", _Text(doc, nsmgr, summationPath + "/ram:TaxTotalAmount[2][@currencyID='USD']"));

            InvoiceDescriptor loaded = _Load(xml);
            Assert.AreEqual(37.62m, loaded.TradeLineItems[0].TaxTotalAmount);
            Assert.AreEqual(40.63m, loaded.TradeLineItems[0].TaxTotalAmountInAccountingCurrency);
            Assert.AreEqual(19.25m, loaded.TradeLineItems[1].TaxTotalAmount);
            Assert.IsNull(loaded.TradeLineItems[1].TaxTotalAmountInAccountingCurrency);
            Assert.AreEqual(61.42m, loaded.TaxTotalAmountInAccountingCurrency);

            _AssertNotWrittenInComfort(desc, "//ram:SpecifiedTradeSettlementLineMonetarySummation/ram:TaxTotalAmount");
        } // !TestLineTaxTotalInAccountingCurrency()


        /// <summary>
        /// BT-133-00: EXTENDED allows several accounting references per line, the reader used to stop after the first one.
        /// </summary>
        [TestMethod]
        public void TestMultipleLineAccountingAccounts()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.TradeLineItems[0].AddReceivableSpecifiedTradeAccountingAccount("4711", AccountingAccountTypeCodes.Financial);
            desc.TradeLineItems[0].AddReceivableSpecifiedTradeAccountingAccount("KST-0815", AccountingAccountTypeCodes.Cost_Accounting);

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string accountPath = FirstLinePath + "/ram:SpecifiedLineTradeSettlement/ram:ReceivableSpecifiedTradeAccountingAccount";
            Assert.HasCount(2, doc.SelectNodes(accountPath, nsmgr)!.Cast<XmlNode>());
            Assert.AreEqual("4711", _Text(doc, nsmgr, accountPath + "[1]/ram:ID"));
            Assert.AreEqual("1", _Text(doc, nsmgr, accountPath + "[1]/ram:TypeCode"));
            Assert.AreEqual("KST-0815", _Text(doc, nsmgr, accountPath + "[2]/ram:ID"));
            Assert.AreEqual("4", _Text(doc, nsmgr, accountPath + "[2]/ram:TypeCode"));

            List<ReceivableSpecifiedTradeAccountingAccount> accounts = _Load(xml).TradeLineItems[0].ReceivableSpecifiedTradeAccountingAccounts;
            Assert.HasCount(2, accounts);
            Assert.AreEqual("4711", accounts[0].TradeAccountID);
            Assert.AreEqual(AccountingAccountTypeCodes.Financial, accounts[0].TradeAccountTypeCode);
            Assert.AreEqual("KST-0815", accounts[1].TradeAccountID);
            Assert.AreEqual(AccountingAccountTypeCodes.Cost_Accounting, accounts[1].TradeAccountTypeCode);

            // EN 16931 knows a single accounting reference per line (BT-133) without type code
            byte[] comfortXml = _SaveAndValidate(desc, Profile.Comfort, SchemaValidator.FacturX1092);
            XmlDocument comfortDoc = _ToXmlDocument(comfortXml);
            XmlNamespaceManager comfortNsmgr = _CreateNamespaceManager(comfortDoc);
            Assert.HasCount(1, comfortDoc.SelectNodes(accountPath, comfortNsmgr)!.Cast<XmlNode>());
            Assert.AreEqual("4711", _Text(comfortDoc, comfortNsmgr, accountPath + "/ram:ID"));
            Assert.IsNull(comfortDoc.SelectSingleNode(accountPath + "/ram:TypeCode", comfortNsmgr));
            Assert.HasCount(1, _Load(comfortXml).TradeLineItems[0].ReceivableSpecifiedTradeAccountingAccounts);
        } // !TestMultipleLineAccountingAccounts()


        /// <summary>
        /// All Factur-X 1.08 line extensions at once, to make sure that their relative order is valid.
        /// </summary>
        [TestMethod]
        public void TestAllFacturX108LineExtensionsCombined()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.SetApplicableTradeDeliveryTerms(TradeDeliveryTermCodes.CIF, CountryCodes.NL, "Rotterdam");
            foreach (TradeLineItem item in desc.TradeLineItems)
            {
                item.SetApplicableTradeDeliveryTerms(TradeDeliveryTermCodes.DAP, CountryCodes.DE, "München");
                item.SetOrderReferencedDocument("B-4711", new DateTime(2018, 2, 1), "1");
                item.ItemSeller = new Party() { Name = "Händler", Country = CountryCodes.AT };
                item.SetPackageQuantity(5m, QuantityCodes.XCT);
                item.SetPerPackageUnitQuantity(4m, QuantityCodes.H87);
                item.ShipTo = new Party() { Name = "Lager", Country = CountryCodes.DE };
                item.ChargeTotalAmount = 0m;
                item.AllowanceTotalAmount = 0m;
                item.GrandTotalAmount = 100m;
                item.AddReceivableSpecifiedTradeAccountingAccount("4711");
                item.AddReceivableSpecifiedTradeAccountingAccount("4712");
            }

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
            InvoiceDescriptor loaded = _Load(xml);
            Assert.HasCount(2, loaded.TradeLineItems);
            Assert.IsTrue(loaded.TradeLineItems.All(i => (i.ItemSeller?.Name == "Händler") && (i.ShipTo?.Name == "Lager") && (i.PerPackageUnitQuantity == 4m) && (i.ReceivableSpecifiedTradeAccountingAccounts.Count == 2)));

            _SaveAndValidate(desc, Profile.Comfort, SchemaValidator.FacturX108, SchemaValidator.FacturX1092);
        } // !TestAllFacturX108LineExtensionsCombined()
        #endregion


        #region Factur-X 1.09.2 / ZUGFeRD 2.5.2
        /// <summary>
        /// ManufacturerTradeParty in SpecifiedTradeProduct, between OriginTradeCountry and IncludedReferencedProduct
        /// </summary>
        [TestMethod]
        public void TestManufacturerTradeParty()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            TradeLineItem item = desc.TradeLineItems[0];
            item.OriginTradeCountry = CountryCodes.DE;
            item.AddIncludedReferencedProduct("Deckblatt", 1m, QuantityCodes.H87);
            item.Manufacturer = new Party()
            {
                GlobalID = new GlobalID(GlobalIDSchemeIdentifiers.GLN, "4000001000005"),
                Name = "Papierfabrik AG",
                Street = "Fabrikstraße 3",
                Postcode = "09111",
                City = "Chemnitz",
                Country = CountryCodes.DE
            };

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string productPath = FirstLinePath + "/ram:SpecifiedTradeProduct";
            Assert.AreEqual("ManufacturerTradeParty", _Text(doc, nsmgr, "local-name(" + productPath + "/ram:OriginTradeCountry/following-sibling::*[1])"));
            Assert.AreEqual("IncludedReferencedProduct", _Text(doc, nsmgr, "local-name(" + productPath + "/ram:ManufacturerTradeParty/following-sibling::*[1])"));
            Assert.AreEqual("Papierfabrik AG", _Text(doc, nsmgr, productPath + "/ram:ManufacturerTradeParty/ram:Name"));
            Assert.AreEqual("4000001000005", _Text(doc, nsmgr, productPath + "/ram:ManufacturerTradeParty/ram:GlobalID[@schemeID='0088']"));
            Assert.AreEqual("Chemnitz", _Text(doc, nsmgr, productPath + "/ram:ManufacturerTradeParty/ram:PostalTradeAddress/ram:CityName"));
            Assert.AreEqual("DE", _Text(doc, nsmgr, productPath + "/ram:ManufacturerTradeParty/ram:PostalTradeAddress/ram:CountryID"));

            InvoiceDescriptor loaded = _Load(xml);
            TradeLineItem loadedItem = loaded.TradeLineItems[0];
            Assert.IsNotNull(loadedItem.Manufacturer);
            Assert.AreEqual("Papierfabrik AG", loadedItem.Manufacturer.Name);
            Assert.AreEqual(GlobalIDSchemeIdentifiers.GLN, loadedItem.Manufacturer.GlobalID.SchemeID);
            Assert.AreEqual("4000001000005", loadedItem.Manufacturer.GlobalID.ID);
            Assert.AreEqual("Fabrikstraße 3", loadedItem.Manufacturer.Street);
            Assert.AreEqual("09111", loadedItem.Manufacturer.Postcode);
            Assert.AreEqual("Chemnitz", loadedItem.Manufacturer.City);
            Assert.AreEqual(CountryCodes.DE, loadedItem.Manufacturer.Country);
            // the product data must not be affected by the party inside SpecifiedTradeProduct
            Assert.AreEqual("Trennblätter A4", loadedItem.Name);
            Assert.AreEqual("4012345001235", loadedItem.GlobalID.ID);
            Assert.AreEqual(CountryCodes.DE, loadedItem.OriginTradeCountry);
            Assert.HasCount(1, loadedItem.IncludedReferencedProducts);
            Assert.AreEqual("Deckblatt", loadedItem.IncludedReferencedProducts[0].Name);
            Assert.IsNull(loaded.TradeLineItems[1].Manufacturer);

            _AssertNotWrittenInComfort(desc, "//ram:ManufacturerTradeParty");
        } // !TestManufacturerTradeParty()


        /// <summary>
        /// BIC of the debtor (PayerSpecifiedDebtorFinancialInstitution/BICID) and debtor account name, Factur-X 1.09
        /// </summary>
        [TestMethod]
        public void TestDebtorFinancialInstitution()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.CreditorBankAccounts.Clear();
            desc.SetPaymentMeansSepaDirectDebit("DE98ZZZ09999999999", "MANDAT-0815", "Lastschrift");
            desc.AddDebitorFinancialAccount(iban: "DE21860000000086001055", bic: "MARKDEF1860", name: "Kunden AG Mitte");

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string paymentMeansPath = HeaderSettlementPath + "/ram:SpecifiedTradeSettlementPaymentMeans";
            Assert.AreEqual("59", _Text(doc, nsmgr, paymentMeansPath + "/ram:TypeCode"));
            Assert.AreEqual("DE21860000000086001055", _Text(doc, nsmgr, paymentMeansPath + "/ram:PayerPartyDebtorFinancialAccount/ram:IBANID"));
            Assert.AreEqual("Kunden AG Mitte", _Text(doc, nsmgr, paymentMeansPath + "/ram:PayerPartyDebtorFinancialAccount/ram:AccountName"));
            Assert.AreEqual("MARKDEF1860", _Text(doc, nsmgr, paymentMeansPath + "/ram:PayerSpecifiedDebtorFinancialInstitution/ram:BICID"));
            Assert.AreEqual("PayerSpecifiedDebtorFinancialInstitution", _Text(doc, nsmgr, "local-name(" + paymentMeansPath + "/ram:PayerPartyDebtorFinancialAccount/following-sibling::*[1])"));

            InvoiceDescriptor loaded = _Load(xml);
            Assert.HasCount(1, loaded.GetDebitorFinancialAccounts());
            BankAccount account = loaded.GetDebitorFinancialAccounts()[0];
            Assert.AreEqual("DE21860000000086001055", account.IBAN);
            Assert.AreEqual("MARKDEF1860", account.BIC);
            Assert.AreEqual("Kunden AG Mitte", account.Name);
            Assert.IsEmpty(loaded.GetCreditorFinancialAccounts());

            _AssertNotWrittenInComfort(desc,
                                       "//ram:PayerSpecifiedDebtorFinancialInstitution",
                                       "//ram:PayerPartyDebtorFinancialAccount/ram:AccountName");
        } // !TestDebtorFinancialInstitution()


        /// <summary>
        /// SpecifiedFinancialAdjustment between SpecifiedTradeSettlementHeaderMonetarySummation and InvoiceReferencedDocument
        /// </summary>
        [TestMethod]
        public void TestFinancialAdjustments()
        {
            InvoiceDescriptor desc = _CreateInvoiceWithFinancialAdjustments();
            desc.AddInvoiceReferencedDocument("471101", new DateTime(2018, 2, 1));

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string adjustmentPath = HeaderSettlementPath + "/ram:SpecifiedFinancialAdjustment";
            Assert.HasCount(2, doc.SelectNodes(adjustmentPath, nsmgr)!.Cast<XmlNode>());
            Assert.AreEqual("SpecifiedFinancialAdjustment", _Text(doc, nsmgr, "local-name(" + HeaderSettlementPath + "/ram:SpecifiedTradeSettlementHeaderMonetarySummation/following-sibling::*[1])"));
            Assert.AreEqual("InvoiceReferencedDocument", _Text(doc, nsmgr, "local-name(" + adjustmentPath + "[2]/following-sibling::*[1])"));
            Assert.AreEqual("Pfand", _Text(doc, nsmgr, adjustmentPath + "[1]/ram:Reason"));
            Assert.AreEqual("10.00", _Text(doc, nsmgr, adjustmentPath + "[1]/ram:ActualAmount"));
            Assert.AreEqual("Verrechnung Dritter", _Text(doc, nsmgr, adjustmentPath + "[2]/ram:Reason"));
            Assert.AreEqual("-2.50", _Text(doc, nsmgr, adjustmentPath + "[2]/ram:ActualAmount"));
            Assert.IsNull(doc.SelectSingleNode(adjustmentPath + "/ram:ActualAmount/@currencyID", nsmgr));
            Assert.AreEqual("537.37", _Text(doc, nsmgr, HeaderSettlementPath + "/ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:DuePayableAmount"));

            InvoiceDescriptor loaded = _Load(xml);
            List<FinancialAdjustment> adjustments = loaded.GetFinancialAdjustments();
            Assert.HasCount(2, adjustments);
            Assert.AreEqual("Pfand", adjustments[0].Reason);
            Assert.AreEqual(10m, adjustments[0].ActualAmount);
            Assert.AreEqual("Verrechnung Dritter", adjustments[1].Reason);
            Assert.AreEqual(-2.5m, adjustments[1].ActualAmount);
            Assert.HasCount(1, loaded.GetInvoiceReferencedDocuments());
            Assert.AreEqual(0, loaded.GetTradeAllowances().Count + loaded.GetTradeCharges().Count, "Financial adjustments must not be read as allowances or charges");

            _AssertNotWrittenInComfort(desc, "//ram:SpecifiedFinancialAdjustment");
        } // !TestFinancialAdjustments()


        [TestMethod]
        public void TestFinancialAdjustmentWithoutReasonIsRejected()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.AddFinancialAdjustment(5m, "");

            using MemoryStream ms = new MemoryStream();
            Assert.ThrowsExactly<MissingDataException>(() => desc.Save(ms, ZUGFeRDVersion.Version25, Profile.Extended));
        } // !TestFinancialAdjustmentWithoutReasonIsRejected()


        /// <summary>
        /// BR-FXEXT-CO-16: BT-115 = BT-112 - BT-113 + BT-114 + sum of the financial adjustment amounts
        /// </summary>
        [TestMethod]
        public void TestValidatorAddsFinancialAdjustmentsToDuePayableAmount()
        {
            InvoiceDescriptor desc = _CreateInvoiceWithFinancialAdjustments();

            ValidationResult result = InvoiceValidator.Validate(desc, ZUGFeRDVersion.Version25);
            Assert.IsTrue(result.IsValid, string.Join(Environment.NewLine, result.Messages));

            // the invoice also validates after a round trip
            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX1092);
            ValidationResult loadedResult = InvoiceValidator.Validate(_Load(xml), ZUGFeRDVersion.Version25);
            Assert.IsTrue(loadedResult.IsValid, string.Join(Environment.NewLine, loadedResult.Messages));

            // a due payable amount that ignores the adjustments violates BR-FXEXT-CO-16
            desc.DuePayableAmount = 529.87m;
            ValidationResult invalidResult = InvoiceValidator.Validate(desc, ZUGFeRDVersion.Version25);
            Assert.IsFalse(invalidResult.IsValid);
            Assert.IsTrue(invalidResult.Messages.Any(m => m.Contains("duePayable") && m.Contains("537.37")), string.Join(Environment.NewLine, invalidResult.Messages));
        } // !TestValidatorAddsFinancialAdjustmentsToDuePayableAmount()


        /// <summary>
        /// Item attribute code (BT-X-11) and item attribute value with unit of measure (BT-X-12)
        /// </summary>
        [TestMethod]
        public void TestProductCharacteristicTypeCodeAndValueMeasure()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            TradeLineItem item = desc.TradeLineItems[0];
            item.AddApplicableProductCharacteristic("Nettogewicht", 2.5m, QuantityCodes.KGM, "AAA");
            item.AddApplicableProductCharacteristic("Farbe", "weiß", "AAK");
            item.AddApplicableProductCharacteristic("Format", "A4");

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX1092);
            XmlDocument doc = _ToXmlDocument(xml);
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);

            string characteristicPath = FirstLinePath + "/ram:SpecifiedTradeProduct/ram:ApplicableProductCharacteristic";
            Assert.HasCount(3, doc.SelectNodes(characteristicPath, nsmgr)!.Cast<XmlNode>());
            Assert.AreEqual("AAA", _Text(doc, nsmgr, characteristicPath + "[1]/ram:TypeCode"));
            Assert.AreEqual("Nettogewicht", _Text(doc, nsmgr, characteristicPath + "[1]/ram:Description"));
            Assert.AreEqual("2.5000", _Text(doc, nsmgr, characteristicPath + "[1]/ram:ValueMeasure"));
            Assert.AreEqual("KGM", _Text(doc, nsmgr, characteristicPath + "[1]/ram:ValueMeasure/@unitCode"));
            Assert.IsNull(doc.SelectSingleNode(characteristicPath + "[1]/ram:Value", nsmgr));
            Assert.AreEqual("TypeCode,Description,ValueMeasure", string.Join(",", doc.SelectNodes(characteristicPath + "[1]/*", nsmgr)!.Cast<XmlNode>().Select(n => n.LocalName)));
            Assert.AreEqual("TypeCode,Description,Value", string.Join(",", doc.SelectNodes(characteristicPath + "[2]/*", nsmgr)!.Cast<XmlNode>().Select(n => n.LocalName)));
            Assert.AreEqual("AAK", _Text(doc, nsmgr, characteristicPath + "[2]/ram:TypeCode"));
            Assert.AreEqual("weiß", _Text(doc, nsmgr, characteristicPath + "[2]/ram:Value"));
            Assert.AreEqual("Description,Value", string.Join(",", doc.SelectNodes(characteristicPath + "[3]/*", nsmgr)!.Cast<XmlNode>().Select(n => n.LocalName)));

            List<ApplicableProductCharacteristic> characteristics = _Load(xml).TradeLineItems[0].ApplicableProductCharacteristics;
            Assert.HasCount(3, characteristics);
            Assert.AreEqual("AAA", characteristics[0].TypeCode);
            Assert.AreEqual("Nettogewicht", characteristics[0].Description);
            Assert.AreEqual(2.5m, characteristics[0].ValueMeasure);
            Assert.AreEqual(QuantityCodes.KGM, characteristics[0].ValueMeasureUnitCode);
            Assert.IsTrue(string.IsNullOrEmpty(characteristics[0].Value));
            Assert.AreEqual("AAK", characteristics[1].TypeCode);
            Assert.AreEqual("Farbe", characteristics[1].Description);
            Assert.AreEqual("weiß", characteristics[1].Value);
            Assert.IsNull(characteristics[1].ValueMeasure);
            Assert.IsNull(characteristics[1].ValueMeasureUnitCode);
            Assert.IsNull(characteristics[2].TypeCode);
            Assert.AreEqual("A4", characteristics[2].Value);

            // EN 16931 requires BT-160 and BT-161: the measured attribute is omitted, the code is dropped
            byte[] comfortXml = _SaveAndValidate(desc, Profile.Comfort, SchemaValidator.FacturX1092);
            XmlDocument comfortDoc = _ToXmlDocument(comfortXml);
            XmlNamespaceManager comfortNsmgr = _CreateNamespaceManager(comfortDoc);
            Assert.HasCount(2, comfortDoc.SelectNodes(characteristicPath, comfortNsmgr)!.Cast<XmlNode>());
            Assert.IsNull(comfortDoc.SelectSingleNode("//ram:ApplicableProductCharacteristic/ram:TypeCode", comfortNsmgr));
            Assert.IsNull(comfortDoc.SelectSingleNode("//ram:ApplicableProductCharacteristic/ram:ValueMeasure", comfortNsmgr));
            Assert.AreEqual("Farbe", _Text(comfortDoc, comfortNsmgr, characteristicPath + "[1]/ram:Description"));
            Assert.AreEqual("Format", _Text(comfortDoc, comfortNsmgr, characteristicPath + "[2]/ram:Description"));
        } // !TestProductCharacteristicTypeCodeAndValueMeasure()


        /// <summary>
        /// All Factur-X 1.08 and 1.09 extensions at once, to make sure that their relative order is valid.
        /// </summary>
        [TestMethod]
        public void TestAllExtensionsCombined()
        {
            InvoiceDescriptor desc = _CreateInvoiceWithFinancialAdjustments();
            desc.SetApplicableTradeDeliveryTerms(TradeDeliveryTermCodes.DAP, CountryCodes.DE, "Frankfurt");
            desc.AddDebitorFinancialAccount(iban: "DE21860000000086001055", bic: "MARKDEF1860", name: "Kunden AG Mitte");
            desc.SetTaxTotalInAccountingCurrency(61.42m, CurrencyCodes.USD);
            foreach (TradeLineItem item in desc.TradeLineItems)
            {
                item.AddApplicableProductCharacteristic("Gewicht", 1.25m, QuantityCodes.KGM, "AAA");
                item.OriginTradeCountry = CountryCodes.DE;
                item.Manufacturer = new Party() { Name = "Hersteller", Country = CountryCodes.DE };
                item.SetApplicableTradeDeliveryTerms(TradeDeliveryTermCodes.DAP, CountryCodes.DE, "Frankfurt");
                item.ItemSeller = new Party() { Name = "Händler", Country = CountryCodes.DE };
                item.AddItemSellerTaxRegistration("DE987654321", TaxRegistrationSchemeID.VA);
                item.SetPerPackageUnitQuantity(10m, QuantityCodes.H87);
                item.TaxTotalAmount = 1m;
                item.TaxTotalAmountInAccountingCurrency = 1.08m;
                item.GrandTotalAmount = 100m;
                item.AddReceivableSpecifiedTradeAccountingAccount("4711");
                item.AddReceivableSpecifiedTradeAccountingAccount("4712");
            }

            byte[] xml = _SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX1092);
            InvoiceDescriptor loaded = _Load(xml);
            Assert.HasCount(2, loaded.GetFinancialAdjustments());
            Assert.AreEqual("MARKDEF1860", loaded.GetDebitorFinancialAccounts()[0].BIC);
            Assert.IsTrue(loaded.TradeLineItems.All(i => (i.Manufacturer?.Name == "Hersteller") && (i.ItemSeller?.Name == "Händler") && (i.TaxTotalAmountInAccountingCurrency == 1.08m)));

            foreach (Profile profile in new[] { Profile.Comfort, Profile.Basic, Profile.BasicWL, Profile.Minimum })
            {
                _SaveAndValidate(desc, profile, SchemaValidator.FacturX1092);
            }
        } // !TestAllExtensionsCombined()
        #endregion


        /// <summary>
        /// ZUGFeRD 2.4 (Version23) does not know the elements that were added with Factur-X 1.09: they are not written,
        /// so existing invoices with a debtor BIC stay valid for receivers that validate against ZUGFeRD 2.4.
        /// </summary>
        [TestMethod]
        public void TestFacturX109ElementsAreOnlyWrittenForZUGFeRD25()
        {
            InvoiceDescriptor desc = _CreateInvoiceWithFinancialAdjustments();
            desc.CreditorBankAccounts.Clear();
            desc.SetPaymentMeansSepaDirectDebit("DE98ZZZ09999999999", "MANDAT-0815", "Lastschrift");
            desc.AddDebitorFinancialAccount(iban: "DE21860000000086001055", bic: "MARKDEF1860", name: "Kunden AG Mitte");
            desc.TradeLineItems[0].Manufacturer = new Party() { Name = "Papierfabrik AG", Country = CountryCodes.DE };
            string[] facturX109Elements = { "PayerSpecifiedDebtorFinancialInstitution", "ManufacturerTradeParty", "SpecifiedFinancialAdjustment", "AccountName>Kunden AG Mitte" };

            string zugferd24 = Encoding.UTF8.GetString(_SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX108, SchemaValidator.FacturX1092));
            foreach (string element in facturX109Elements)
            {
                Assert.DoesNotContain(element, zugferd24);
            }
            StringAssert.Contains(zugferd24, "<ram:IBANID>DE21860000000086001055</ram:IBANID>");

            string zugferd25 = Encoding.UTF8.GetString(_SaveAndValidate(desc, Profile.Extended, SchemaValidator.FacturX1092));
            foreach (string element in facturX109Elements)
            {
                StringAssert.Contains(zugferd25, element);
            }

            // the validator only adds the adjustments to BT-115 for ZUGFeRD 2.5, where they are written
            desc.DuePayableAmount = desc.GrandTotalAmount;
            Assert.IsTrue(InvoiceValidator.Validate(desc, ZUGFeRDVersion.Version23).IsValid);
            Assert.IsFalse(InvoiceValidator.Validate(desc, ZUGFeRDVersion.Version25).IsValid);
        } // !TestFacturX109ElementsAreOnlyWrittenForZUGFeRD25()


        /// <summary>
        /// Invoice from InvoiceProvider with two financial adjustments (+10.00, -2.50) that are included in BT-115.
        /// </summary>
        private InvoiceDescriptor _CreateInvoiceWithFinancialAdjustments()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.AddFinancialAdjustment(10m, "Pfand");
            desc.AddFinancialAdjustment(-2.5m, "Verrechnung Dritter");
            desc.DuePayableAmount = desc.GrandTotalAmount + 7.5m; // 529.87 + 10.00 - 2.50
            return desc;
        } // !_CreateInvoiceWithFinancialAdjustments()


        /// <summary>
        /// Saves the invoice in the given profile and validates the result against the Factur-X schemas of the given versions.
        /// Invoices that have to be valid against Factur-X 1.08 are written as ZUGFeRD 2.4 (Version23), invoices that are only
        /// validated against Factur-X 1.09.2 as ZUGFeRD 2.5 (Version25), which also writes the elements added with 1.09.
        /// </summary>
        private static byte[] _SaveAndValidate(InvoiceDescriptor desc, Profile profile, params string[] schemaVersions)
        {
            ZUGFeRDVersion version = schemaVersions.Contains(SchemaValidator.FacturX108) ? ZUGFeRDVersion.Version23 : ZUGFeRDVersion.Version25;
            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, version, profile);

            foreach (string schemaVersion in schemaVersions)
            {
                List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetFacturXSchemaPath(schemaVersion, profile));
                Assert.IsEmpty(errors, $"Factur-X {schemaVersion} {profile}:{Environment.NewLine}" + string.Join(Environment.NewLine, errors));
            }

            return ms.ToArray();
        } // !_SaveAndValidate()


        /// <summary>
        /// Saves the invoice in profile EN 16931 (COMFORT), validates it against the Factur-X 1.09.2 EN 16931 schema
        /// and makes sure that none of the given elements is written.
        /// </summary>
        private static void _AssertNotWrittenInComfort(InvoiceDescriptor desc, params string[] xpaths)
        {
            XmlDocument doc = _ToXmlDocument(_SaveAndValidate(desc, Profile.Comfort, SchemaValidator.FacturX1092));
            XmlNamespaceManager nsmgr = _CreateNamespaceManager(doc);
            foreach (string xpath in xpaths)
            {
                Assert.IsNull(doc.SelectSingleNode(xpath, nsmgr), $"{xpath} must not be written in profile EN 16931 (COMFORT)");
            }
        } // !_AssertNotWrittenInComfort()


        private static InvoiceDescriptor _Load(byte[] xml)
        {
            using MemoryStream ms = new MemoryStream(xml);
            return InvoiceDescriptor.Load(ms);
        } // !_Load()


        private static XmlDocument _ToXmlDocument(byte[] xml)
        {
            XmlDocument doc = new XmlDocument();
            using MemoryStream ms = new MemoryStream(xml);
            doc.Load(ms);
            return doc;
        } // !_ToXmlDocument()


        private static XmlNamespaceManager _CreateNamespaceManager(XmlDocument doc)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100");
            nsmgr.AddNamespace("ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100");
            nsmgr.AddNamespace("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100");
            nsmgr.AddNamespace("qdt", "urn:un:unece:uncefact:data:standard:QualifiedDataType:100");
            return nsmgr;
        } // !_CreateNamespaceManager()


        /// <summary>
        /// Evaluates the given xpath (node or string expression) and returns the text, fails if nothing is found.
        /// </summary>
        private static string _Text(XmlDocument doc, XmlNamespaceManager nsmgr, string xpath)
        {
            object result = doc.CreateNavigator()!.Evaluate(xpath, nsmgr);
            string? text = null;
            if (result is System.Xml.XPath.XPathNodeIterator iterator)
            {
                text = iterator.MoveNext() ? iterator.Current?.Value : null;
            }
            else
            {
                text = Convert.ToString(result, System.Globalization.CultureInfo.InvariantCulture);
            }

            Assert.IsFalse(string.IsNullOrEmpty(text), $"Nothing found at {xpath}");
            return text!;
        } // !_Text()
    }
}
