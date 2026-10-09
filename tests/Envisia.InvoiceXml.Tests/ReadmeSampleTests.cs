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

namespace Envisia.InvoiceXml.Tests
{
    /// <summary>
    /// Keeps the quick start sample of README.md compiling and producing valid invoices.
    /// </summary>
    [TestClass]
    public class ReadmeSampleTests : TestBase
    {
        internal static InvoiceDescriptor _CreateReadmeInvoice()
        {
            // --- copy of the README quick start ---
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
            // --- end of copy ---
            return invoice;
        } // !_CreateReadmeInvoice()


        [TestMethod]
        public void TestReadmeSampleIsValidFacturX()
        {
            InvoiceDescriptor invoice = _CreateReadmeInvoice();
            Assert.IsTrue(InvoiceValidator.Validate(invoice, ZUGFeRDVersion.Version23).IsValid);

            using MemoryStream ms = new MemoryStream();
            invoice.Save(ms, ZUGFeRDVersion.Version23, Profile.Comfort);
            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetFacturXSchemaPath(SchemaValidator.FacturX1092, Profile.Comfort));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

            InvoiceDescriptor loaded = InvoiceDescriptor.Load(ms);
            Assert.AreEqual(Profile.Comfort, loaded.Profile);
            Assert.AreEqual("471102", loaded.InvoiceNo);
            Assert.AreEqual(235.62m, loaded.DuePayableAmount);
        } // !TestReadmeSampleIsValidFacturX()


        [TestMethod]
        [DataRow(ZUGFeRDFormats.UBL)]
        [DataRow(ZUGFeRDFormats.CII)]
        public void TestReadmeSampleIsValidXRechnung(ZUGFeRDFormats format)
        {
            InvoiceDescriptor invoice = _CreateReadmeInvoice();

            using MemoryStream ms = new MemoryStream();
            invoice.Save(ms, ZUGFeRDVersion.Version23, Profile.XRechnung, format);
            string schemaPath = format == ZUGFeRDFormats.UBL
                ? SchemaValidator.GetUblSchemaPath(false)
                : SchemaValidator.GetFacturXSchemaPath(SchemaValidator.FacturX1092, Profile.XRechnung);
            List<string> errors = SchemaValidator.Validate(ms, schemaPath);
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

            InvoiceDescriptor loaded = InvoiceDescriptor.Load(ms);
            Assert.AreEqual(Profile.XRechnung, loaded.Profile);
            Assert.AreEqual("04011000-12345-34", loaded.ReferenceOrderNo);
        } // !TestReadmeSampleIsValidXRechnung()
    }
}
