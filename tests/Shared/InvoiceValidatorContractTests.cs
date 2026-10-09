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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Envisia.InvoiceXml;
using Envisia.InvoiceXml.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Envisia.InvoiceXml.Tests.Validation
{
    /// <summary>
    /// The contract of <see cref="IInvoiceValidator"/>: every implementation (Envisia.InvoiceXml.Validation and
    /// Envisia.InvoiceXml.Validation.GraalVM) runs these tests with the same expectations.
    /// </summary>
    public abstract class InvoiceValidatorContractTests
    {
        /// <summary>The implementation under test.</summary>
        protected abstract IInvoiceValidator Validator { get; }

        /// <summary>The type services.AddInvoiceValidator() of the referenced implementation package registers.</summary>
        protected abstract Type ImplementationType { get; }


        internal static InvoiceDescriptor CreateInvoice()
        {
            InvoiceDescriptor invoice = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2026, 3, 5), CurrencyCodes.EUR);
            invoice.BusinessProcess = "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0";
            invoice.ReferenceOrderNo = "04011000-12345-34";
            invoice.ActualDeliveryDate = new DateTime(2026, 3, 3);
            invoice.SetSeller("Lieferant GmbH", "80333", "München", "Lieferantenstraße 20", CountryCodes.DE);
            invoice.AddSellerTaxRegistration("DE123456789", TaxRegistrationSchemeID.VA);
            invoice.SetSellerContact("Max Mustermann", emailAddress: "max@lieferant.de", phoneno: "+49 89 123456");
            invoice.SetSellerElectronicAddress("rechnung@lieferant.de", ElectronicAddressSchemeIdentifiers.ElectronicMailSmtp);
            invoice.SetBuyer("Kunden AG", "69876", "Frankfurt", "Kundenstraße 15", CountryCodes.DE);
            invoice.SetBuyerElectronicAddress("einkauf@kunde.de", ElectronicAddressSchemeIdentifiers.ElectronicMailSmtp);
            invoice.AddTradeLineItem(name: "Trennblätter A4", netUnitPrice: 9.90m, billedQuantity: 20m, unitCode: QuantityCodes.H87,
                                     taxType: TaxTypes.VAT, categoryCode: TaxCategoryCodes.S, taxPercent: 19m);
            invoice.AddApplicableTradeTax(198.00m, 19m, 37.62m, TaxTypes.VAT, TaxCategoryCodes.S);
            invoice.SetTotals(lineTotalAmount: 198.00m, taxBasisAmount: 198.00m, taxTotalAmount: 37.62m, grandTotalAmount: 235.62m, duePayableAmount: 235.62m);
            invoice.SetPaymentMeans(PaymentMeansTypeCodes.SEPACreditTransfer);
            invoice.AddCreditorFinancialAccount("DE02120300000000202051", "BYLADEM1001");
            invoice.AddTradePaymentTerms("Zahlbar innerhalb von 30 Tagen ohne Abzug", new DateTime(2026, 4, 4));
            return invoice;
        }


        internal static byte[] CreateInvoiceXml(Profile profile, ZUGFeRDFormats format = ZUGFeRDFormats.CII)
        {
            using MemoryStream stream = new MemoryStream();
            CreateInvoice().Save(stream, ZUGFeRDVersion.Version23, profile, format);
            return stream.ToArray();
        }


        private static string _Describe(InvoiceValidationResult result)
        {
            return result + Environment.NewLine + string.Join(Environment.NewLine, result.Messages);
        }


        [TestMethod]
        [DataRow(ZUGFeRDFormats.UBL, "EN16931 XRechnung (UBL Invoice)")]
        [DataRow(ZUGFeRDFormats.CII, "EN16931 XRechnung (CII)")]
        public void XRechnungIsAccepted(ZUGFeRDFormats format, string scenario)
        {
            InvoiceValidationResult result = Validator.Validate(CreateInvoiceXml(Profile.XRechnung, format), "xrechnung.xml");
            Assert.AreEqual(scenario, result.Scenario, _Describe(result));
            Assert.IsTrue(result.IsWellFormed);
            Assert.IsTrue(result.IsSchemaValid, _Describe(result));
            Assert.IsTrue(result.IsValid, _Describe(result));
            Assert.AreEqual(AcceptRecommendation.Accept, result.Recommendation);
            Assert.IsTrue(result.IsAcceptable);
            Assert.IsEmpty(result.Errors.ToList(), _Describe(result));
            Assert.AreEqual("report", XDocument.Parse(result.ReportXml).Root!.Name.LocalName);
            Assert.IsFalse(string.IsNullOrEmpty(result.Engine));
            StringAssert.Contains(result.GetReportHtml(), "Es wird empfohlen das Dokument anzunehmen");
        }


        [TestMethod]
        [DataRow(Profile.Minimum, "ZUGFeRD / Factur-X (MINIMUM)")]
        [DataRow(Profile.BasicWL, "ZUGFeRD / Factur-X (BASIC WL)")]
        [DataRow(Profile.Basic, "ZUGFeRD / Factur-X (BASIC)")]
        [DataRow(Profile.Comfort, "ZUGFeRD / Factur-X (EN 16931)")]
        [DataRow(Profile.Extended, "ZUGFeRD / Factur-X (EXTENDED)")]
        public void FacturXIsAccepted(Profile profile, string scenario)
        {
            InvoiceValidationResult result = Validator.Validate(CreateInvoiceXml(profile), "factur-x.xml");
            Assert.AreEqual(scenario, result.Scenario, _Describe(result));
            Assert.IsTrue(result.IsAcceptable, _Describe(result));
            Assert.IsEmpty(result.Errors.ToList(), _Describe(result));
        }


        [TestMethod]
        public void BusinessRuleViolationIsRejected()
        {
            string xml = Encoding.UTF8.GetString(CreateInvoiceXml(Profile.XRechnung, ZUGFeRDFormats.UBL));
            // without the buyer reference (BT-10), which XRechnung requires (BR-DE-15)
            byte[] invalid = Encoding.UTF8.GetBytes(Regex.Replace(xml, "<cbc:BuyerReference>[^<]*</cbc:BuyerReference>", ""));

            InvoiceValidationResult result = Validator.Validate(invalid, "invoice.xml");
            Assert.AreEqual("EN16931 XRechnung (UBL Invoice)", result.Scenario);
            Assert.IsTrue(result.IsSchemaValid);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(AcceptRecommendation.Reject, result.Recommendation);
            InvoiceValidationMessage error = result.Errors.Single(m => m.Code == "BR-DE-15");
            Assert.AreEqual("/Q{urn:oasis:names:specification:ubl:schema:xsd:Invoice-2}Invoice[1]", error.Location);
            StringAssert.StartsWith(error.StepId, "val-sch.");
            StringAssert.Contains(error.Text, "BR-DE-15");
            StringAssert.Contains(result.GetReportHtml(), "zurückzuweisen");
        }


        [TestMethod]
        public void SchemaViolationIsRejected()
        {
            string xml = Encoding.UTF8.GetString(CreateInvoiceXml(Profile.XRechnung, ZUGFeRDFormats.UBL));
            byte[] invalid = Encoding.UTF8.GetBytes(xml.Replace("<cbc:IssueDate>", "<cbc:Unknown>1</cbc:Unknown><cbc:IssueDate>"));

            InvoiceValidationResult result = Validator.Validate(invalid, "invoice.xml");
            Assert.AreEqual("EN16931 XRechnung (UBL Invoice)", result.Scenario);
            Assert.IsTrue(result.IsWellFormed);
            Assert.IsFalse(result.IsSchemaValid);
            Assert.AreEqual(AcceptRecommendation.Reject, result.Recommendation);
            Assert.IsTrue(result.Errors.Any(m => m.StepId == "val-xsd"), _Describe(result));
        }


        [TestMethod]
        public void DocumentThatIsNotWellFormedIsRejected()
        {
            InvoiceValidationResult result = Validator.Validate(Encoding.UTF8.GetBytes("<Invoice><cbc:ID>1</Invoice>"), "broken.xml");
            Assert.IsFalse(result.IsWellFormed);
            Assert.IsNull(result.Scenario);
            Assert.AreEqual(AcceptRecommendation.Reject, result.Recommendation);
            Assert.IsTrue(result.Errors.Any(m => m.StepId == "val-xml"), _Describe(result));
        }


        [TestMethod]
        public void UnknownDocumentIsRejected()
        {
            InvoiceValidationResult result = Validator.Validate(Encoding.UTF8.GetBytes("<Order xmlns='urn:example:order'/>"), "order.xml");
            Assert.IsTrue(result.IsWellFormed);
            Assert.IsNull(result.Scenario);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(AcceptRecommendation.Reject, result.Recommendation);
        }


        [TestMethod]
        public void FilesAndStreams()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            try
            {
                File.WriteAllBytes(path, CreateInvoiceXml(Profile.XRechnung, ZUGFeRDFormats.CII));
                Assert.IsTrue(Validator.Validate(path).IsAcceptable);
                using FileStream stream = File.OpenRead(path);
                Assert.IsTrue(Validator.Validate(stream, "invoice.xml").IsAcceptable);
            }
            finally
            {
                File.Delete(path);
            }
        }


        [TestMethod]
        public void ConcurrentValidation()
        {
            byte[][] documents = { CreateInvoiceXml(Profile.XRechnung, ZUGFeRDFormats.UBL), CreateInvoiceXml(Profile.XRechnung, ZUGFeRDFormats.CII), CreateInvoiceXml(Profile.Comfort) };
            string[] expected = documents.Select(d => Validator.Validate(d, "invoice.xml").Scenario!).ToArray();
            string?[] actual = new string?[documents.Length * 8];
            Parallel.For(0, actual.Length, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i => actual[i] = Validator.Validate(documents[i % documents.Length], "invoice.xml").Scenario);
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(expected[i % documents.Length], actual[i]);
            }
        }


        [TestMethod]
        public void ServiceRegistration()
        {
            // the same code for both packages: the implementation is chosen by the referenced package
            using ServiceProvider provider = new ServiceCollection().AddInvoiceValidator().BuildServiceProvider();
            IInvoiceValidator validator = provider.GetRequiredService<IInvoiceValidator>();
            Assert.IsInstanceOfType(validator, ImplementationType);
            Assert.AreSame(validator, provider.GetRequiredService<IInvoiceValidator>());
            Assert.IsTrue(validator.Validate(CreateInvoiceXml(Profile.XRechnung, ZUGFeRDFormats.UBL), "invoice.xml").IsAcceptable);
        }
    }
}
