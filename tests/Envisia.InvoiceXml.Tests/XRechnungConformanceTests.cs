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

namespace Envisia.InvoiceXml.Tests
{
    /// <summary>
    /// XRechnung 3.0.x output checks: UBL output against the OASIS UBL 2.1 schemas (documentation/ubl21)
    /// and regression tests for rules of the current CEN / KoSIT validation artefacts.
    /// </summary>
    [TestClass]
    public class XRechnungConformanceTests : TestBase
    {
        private readonly InvoiceProvider _InvoiceProvider = new InvoiceProvider();


        public static IEnumerable<object[]> UblDemoInvoices
        {
            get
            {
                string folder = Path.Combine(SchemaValidator.RepositoryRoot, "demodata", "xRechnung");
                return Directory.GetFiles(folder, "*.xml")
                                .Where(path => File.ReadAllText(path).Contains("urn:oasis:names:specification:ubl:schema:xsd:"))
                                .OrderBy(path => path, StringComparer.Ordinal)
                                .Select(path => new object[] { Path.GetFileName(path) });
            }
        }


        [TestMethod]
        [DataRow(InvoiceType.Invoice, false)]
        [DataRow(InvoiceType.CreditNote, true)]
        public void TestGeneratedUblIsSchemaValid(InvoiceType type, bool isCreditNote)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.Type = type;

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);

            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetUblSchemaPath(isCreditNote));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        } // !TestGeneratedUblIsSchemaValid()


        public static IEnumerable<object[]> AllInvoiceTypes => Enum.GetValues(typeof(InvoiceType)).Cast<InvoiceType>().Select(t => new object[] { t });


        /// <summary>
        /// Every UNTDID 1001 code can be written in UBL: credit notes (EN 16931 interpretation) as ubl:CreditNote, everything else as ubl:Invoice.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(AllInvoiceTypes))]
        public void TestEveryInvoiceTypeCanBeWrittenInUbl(InvoiceType type)
        {
            InvoiceType[] creditNotes =
            {
                InvoiceType.CreditNoteRelatedToGoodsOrServices, InvoiceType.CreditNoteRelatedToFinancialAdjustments, InvoiceType.SelfBilledCreditNote,
                InvoiceType.ConsolidatedCreditNoteGoodsAndServices, InvoiceType.CreditNoteForPriceVariation, InvoiceType.DelcredereCreditNote,
                InvoiceType.CreditNote, InvoiceType.FactoredCreditNote, InvoiceType.OcrPaymentCreditNote, InvoiceType.ReversalOfCredit,
                InvoiceType.SelfBilledFactoredCreditNote, InvoiceType.PrepaymentCreditNoteCorrected, InvoiceType.ForwardersCreditNote
            };
            bool isCreditNote = creditNotes.Contains(type);

            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.Type = type;

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);

            StringAssert.Contains(Encoding.UTF8.GetString(ms.ToArray()), isCreditNote ? "<ubl:CreditNote" : "<ubl:Invoice");
            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetUblSchemaPath(isCreditNote));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
            Assert.AreEqual(type, InvoiceDescriptor.Load(ms).Type);
        } // !TestEveryInvoiceTypeCanBeWrittenInUbl()


        [TestMethod]
        [DynamicData(nameof(UblDemoInvoices))]
        public void TestUblDemoInvoiceRoundTripIsSchemaValid(string fileName)
        {
            string path = Path.Combine(SchemaValidator.RepositoryRoot, "demodata", "xRechnung", fileName);
            InvoiceDescriptor desc = InvoiceDescriptor.Load(path);

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);

            string xml = Encoding.UTF8.GetString(ms.ToArray());
            bool isCreditNote = xml.Contains("<ubl:CreditNote");
            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetUblSchemaPath(isCreditNote));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

            // PEPPOL-EN16931-R008: no empty elements
            List<string> emptyElements = System.Xml.Linq.XDocument.Parse(xml).Descendants()
                .Where(e => !e.HasElements && !e.HasAttributes && String.IsNullOrWhiteSpace(e.Value))
                .Select(e => e.Name.LocalName)
                .ToList();
            Assert.IsEmpty(emptyElements, "Empty elements: " + string.Join(", ", emptyElements));
        } // !TestUblDemoInvoiceRoundTripIsSchemaValid()


        /// <summary>
        /// Address line 3 (cac:AddressLine) has to follow CountrySubentity in UBL; the deliver-to address
        /// uses AdditionalStreetName for line 2 (BT-76) and AddressLine for line 3 (BT-165).
        /// </summary>
        [TestMethod]
        public void TestUblAddressLinesRoundTrip()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.Buyer.Street = "Buyer line 1";
            desc.Buyer.Street2 = "Buyer line 2";
            desc.Buyer.AddressLine3 = "Buyer line 3";
            desc.Buyer.CountrySubdivisionName = "Bayern";
            desc.ShipTo = new Party()
            {
                Name = "Ship to",
                Street = "Ship to line 1",
                Street2 = "Ship to line 2",
                AddressLine3 = "Ship to line 3",
                City = "München",
                Postcode = "80333",
                Country = CountryCodes.DE
            };

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL);

            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetUblSchemaPath(false));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

            InvoiceDescriptor loaded = InvoiceDescriptor.Load(ms);
            Assert.AreEqual("Buyer line 1", loaded.Buyer.Street);
            Assert.AreEqual("Buyer line 2", loaded.Buyer.Street2);
            Assert.AreEqual("Buyer line 3", loaded.Buyer.AddressLine3);
            Assert.AreEqual("Ship to line 1", loaded.ShipTo.Street);
            Assert.AreEqual("Ship to line 2", loaded.ShipTo.Street2);
            Assert.AreEqual("Ship to line 3", loaded.ShipTo.AddressLine3);
        } // !TestUblAddressLinesRoundTrip()


        /// <summary>
        /// BG-11 in UBL: name (BT-62) as cac:PartyName, VAT identifier (BT-63) as cac:PartyTaxScheme, no empty address elements.
        /// </summary>
        [TestMethod]
        [DataRow(ZUGFeRDFormats.UBL)]
        [DataRow(ZUGFeRDFormats.CII)]
        public void TestSellerTaxRepresentativeRoundTrip(ZUGFeRDFormats format)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.SellerTaxRepresentative = new Party() { Name = "Steuervertreter GmbH", Country = CountryCodes.DE };
            desc.AddSellerTaxRepresentativeTaxRegistration("DE124567890", TaxRegistrationSchemeID.VA);

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, Profile.XRechnung, format);
            string xml = Encoding.UTF8.GetString(ms.ToArray());
            if (format == ZUGFeRDFormats.UBL)
            {
                List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetUblSchemaPath(false));
                Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);
                System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                Assert.AreEqual("Steuervertreter GmbH", doc.SelectSingleNode("//cac:TaxRepresentativeParty/cac:PartyName/cbc:Name", nsmgr)?.InnerText);
                Assert.AreEqual("DE124567890", doc.SelectSingleNode("//cac:TaxRepresentativeParty/cac:PartyTaxScheme/cbc:CompanyID", nsmgr)?.InnerText);
                Assert.DoesNotContain("<cbc:CityName />", xml);
                Assert.DoesNotContain("<cbc:PostalZone />", xml);
            }

            InvoiceDescriptor loaded = InvoiceDescriptor.Load(ms);
            Assert.AreEqual("Steuervertreter GmbH", loaded.SellerTaxRepresentative.Name);
            Assert.AreEqual(CountryCodes.DE, loaded.SellerTaxRepresentative.Country);
            Assert.AreEqual("DE124567890", loaded.GetSellerTaxRepresentativeTaxRegistration().Single().No);
            Assert.AreEqual(TaxRegistrationSchemeID.VA, loaded.GetSellerTaxRepresentativeTaxRegistration().Single().SchemeID);
        } // !TestSellerTaxRepresentativeRoundTrip()


        /// <summary>
        /// An additional document without document type code must not get a type code "0" on the way.
        /// </summary>
        [TestMethod]
        [DataRow(ZUGFeRDFormats.UBL)]
        [DataRow(ZUGFeRDFormats.CII)]
        public void TestAdditionalReferencedDocumentWithoutTypeCode(ZUGFeRDFormats format)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.AddAdditionalReferencedDocument("DOC-1", null, name: "Supporting document");

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, Profile.XRechnung, format);
            string xml = Encoding.UTF8.GetString(ms.ToArray());
            Assert.DoesNotContain(">0</cbc:DocumentTypeCode>", xml);
            Assert.DoesNotContain(">0</ram:TypeCode>", xml);

            InvoiceDescriptor loaded = InvoiceDescriptor.Load(ms);
            AdditionalReferencedDocument document = loaded.AdditionalReferencedDocuments.Single(d => d.ID == "DOC-1");
            Assert.IsNull(document.TypeCode);

            // reading the result and writing it again must not invent a type code either
            using MemoryStream ms2 = new MemoryStream();
            loaded.Save(ms2, ZUGFeRDVersion.Version23, Profile.XRechnung, format);
            Assert.IsNull(InvoiceDescriptor.Load(ms2).AdditionalReferencedDocuments.Single(d => d.ID == "DOC-1").TypeCode);
        } // !TestAdditionalReferencedDocumentWithoutTypeCode()


        /// <summary>
        /// CII-SR-465 / CII-SR-466: only one BT-41 / BT-56 element, so person name and department name
        /// must not be written together for seller and buyer.
        /// </summary>
        [TestMethod]
        [DataRow(Profile.XRechnung)]
        [DataRow(Profile.Comfort)]
        [DataRow(Profile.Extended)]
        public void TestSellerAndBuyerContactPointIsWrittenOnce(Profile profile)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.SetSellerContact("Max Mustermann", "Sales", "max@example.com", "+49 89 123");
            desc.SetBuyerContact("Erika Musterfrau", "Purchasing", "erika@example.com", "+49 30 456");

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, profile);
            string xml = Encoding.UTF8.GetString(ms.ToArray());

            Assert.DoesNotContain("<ram:DepartmentName>", xml);
            InvoiceDescriptor loaded = InvoiceDescriptor.Load(ms);
            Assert.AreEqual("Max Mustermann", loaded.SellerContact.Name);
            Assert.AreEqual("Erika Musterfrau", loaded.BuyerContact.Name);

            // without a person name the department is used as contact point
            desc.SetSellerContact(String.Empty, "Sales", "sales@example.com", "+49 89 123");
            using MemoryStream ms2 = new MemoryStream();
            desc.Save(ms2, ZUGFeRDVersion.Version23, profile);
            Assert.AreEqual("Sales", InvoiceDescriptor.Load(ms2).SellerContact.OrgUnit);
        } // !TestSellerAndBuyerContactPointIsWrittenOnce()


        /// <summary>
        /// The accounting account type code (BT-X-290) is optional and must not be written as "0" after reading an invoice without it.
        /// </summary>
        [TestMethod]
        public void TestAccountingAccountWithoutTypeCodeRoundTrip()
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            desc.AddReceivableSpecifiedTradeAccountingAccount("4711");

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, Profile.Extended);
            InvoiceDescriptor loaded = InvoiceDescriptor.Load(ms);
            Assert.IsNull(loaded.GetReceivableSpecifiedTradeAccountingAccounts().Single().TradeAccountTypeCode);

            using MemoryStream ms2 = new MemoryStream();
            loaded.Save(ms2, ZUGFeRDVersion.Version23, Profile.Extended);
            Assert.DoesNotContain("<ram:TypeCode>0</ram:TypeCode>", Encoding.UTF8.GetString(ms2.ToArray()));
        } // !TestAccountingAccountWithoutTypeCodeRoundTrip()
    }
}
