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
using System.Xml.Linq;
using System.Xml.Schema;
using Envisia.InvoiceXml.Validation;
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Tests.Validation
{
    /// <summary>
    /// Tests the validator API: invoices written by the library validated with the built-in configurations, error
    /// cases, the report formats (the XML report is checked against report.xsd of the KoSIT validator) and custom
    /// scenario configurations.
    /// </summary>
    [TestClass]
    public class ValidatorTests
    {
        private static readonly XNamespace _Rep = "http://www.xoev.de/de/validator/varl/1";
        private static readonly Lazy<XmlSchemaSet> _KositSchemas = new Lazy<XmlSchemaSet>(_LoadKositSchemas);


        private static XmlSchemaSet _LoadKositSchemas()
        {
            XmlSchemaSet set = new XmlSchemaSet();
            set.Add(null, XPathEngineTests.TestDataPath(Path.Combine("kosit-xsd", "scenarios.xsd")));
            set.Add(null, XPathEngineTests.TestDataPath(Path.Combine("kosit-xsd", "report.xsd")));
            set.Compile();
            return set;
        }


        private static void _AssertValidAgainstKositSchemas(XDocument document)
        {
            List<string> errors = new List<string>();
            document.Validate(_KositSchemas.Value, (sender, e) => errors.Add(e.Severity + ": " + e.Message));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors) + Environment.NewLine + document);
        }


        private static string _Describe(ValidationReport report)
        {
            return report + Environment.NewLine + string.Join(Environment.NewLine, report.Messages.Select(m => m.ToString()));
        }


        private static byte[] _ReadmeInvoiceXml(ZUGFeRDVersion version, Profile profile, ZUGFeRDFormats format)
        {
            using MemoryStream stream = new MemoryStream();
            ReadmeSampleTests._CreateReadmeInvoice().Save(stream, version, profile, format);
            return stream.ToArray();
        }


        private static ValidatorConfiguration _CustomConfiguration()
        {
            return ValidatorConfiguration.Load(XPathEngineTests.TestDataPath(Path.Combine("custom", "scenarios.xml")));
        }


        private static ValidatorConfiguration _ConfigurationFromString(string scenarios)
        {
            return ValidatorConfiguration.Load(new MemoryStream(Encoding.UTF8.GetBytes(scenarios)), XPathEngineTests.TestDataPath("custom"));
        }


        [TestMethod]
        [DataRow(ZUGFeRDFormats.UBL, "EN16931 XRechnung (UBL Invoice)")]
        [DataRow(ZUGFeRDFormats.CII, "EN16931 XRechnung (CII)")]
        public void ReadmeSampleIsValidXRechnung(ZUGFeRDFormats format, string scenario)
        {
            ValidationReport report = ReadmeSampleTests._CreateReadmeInvoice().ValidateXml(ZUGFeRDVersion.Version23, Profile.XRechnung, format);

            Assert.AreEqual(scenario, report.Scenario?.Name, _Describe(report));
            Assert.AreSame(ValidatorConfiguration.XRechnung, report.Configuration);
            Assert.IsTrue(report.IsWellFormed);
            Assert.IsTrue(report.IsSchemaValid, _Describe(report));
            Assert.IsEmpty(report.Errors.ToList(), _Describe(report));
            Assert.IsEmpty(report.Warnings.ToList(), _Describe(report));
            Assert.IsTrue(report.IsValid, _Describe(report));
            Assert.AreEqual(AcceptRecommendation.Accept, report.Recommendation);
            Assert.IsTrue(report.IsAcceptable);
            CollectionAssert.AreEqual(new[] { "val-xsd", "val-sch.1", "val-sch.2", "val-xml" }, report.Steps.Select(s => s.Id).ToArray());
            Assert.AreEqual("471102", report.DocumentReference);

            Dictionary<string, string> data = report.DocumentData.ToDictionary(d => d.Key, d => d.Value);
            Assert.AreEqual("Lieferant GmbH", data["seller"]);
            Assert.AreEqual("471102", data["id"]);
            Assert.AreEqual(format == ZUGFeRDFormats.UBL ? "2026-03-05" : "20260305", data["issueDate"]);

            _AssertValidAgainstKositSchemas(report.ToXml());
        }


        [TestMethod]
        [DataRow(Profile.Minimum, "ZUGFeRD / Factur-X (MINIMUM)")]
        [DataRow(Profile.BasicWL, "ZUGFeRD / Factur-X (BASIC WL)")]
        [DataRow(Profile.Basic, "ZUGFeRD / Factur-X (BASIC)")]
        [DataRow(Profile.Comfort, "ZUGFeRD / Factur-X (EN 16931)")]
        [DataRow(Profile.Extended, "ZUGFeRD / Factur-X (EXTENDED)")]
        public void ReadmeSampleIsValidFacturX(Profile profile, string scenario)
        {
            ValidationReport report = ReadmeSampleTests._CreateReadmeInvoice().ValidateXml(ZUGFeRDVersion.Version23, profile);

            Assert.AreEqual(scenario, report.Scenario?.Name, _Describe(report));
            Assert.AreSame(ValidatorConfiguration.FacturX, report.Configuration);
            Assert.IsEmpty(report.Errors.ToList(), _Describe(report));
            Assert.IsTrue(report.IsAcceptable, _Describe(report));
            CollectionAssert.AreEqual(new[] { "val-xsd", "val-sch.1", "val-xml" }, report.Steps.Select(s => s.Id).ToArray());
            _AssertValidAgainstKositSchemas(report.ToXml());
        }


        [TestMethod]
        public void EN16931ConfigurationValidatesWithCenRules()
        {
            InvoiceXmlValidator validator = new InvoiceXmlValidator(ValidatorConfiguration.EN16931);
            ValidationReport report = validator.Validate(_ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL));
            Assert.AreEqual("EN16931 CIUS (UBL Invoice)", report.Scenario?.Name, _Describe(report));
            CollectionAssert.AreEqual(new[] { "val-xsd", "val-sch.1", "val-xml" }, report.Steps.Select(s => s.Id).ToArray());
            Assert.IsTrue(report.IsAcceptable, _Describe(report));
        }


        [TestMethod]
        public void SchematronErrorsLeadToRejection()
        {
            string xml = Encoding.UTF8.GetString(_ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL));
            // remove the buyer reference (BT-10), required by XRechnung (BR-DE-15)
            string withoutBuyerReference = System.Text.RegularExpressions.Regex.Replace(xml, "<cbc:BuyerReference>[^<]*</cbc:BuyerReference>", "");
            Assert.AreNotEqual(xml, withoutBuyerReference);

            ValidationReport report = new InvoiceXmlValidator().Validate(Encoding.UTF8.GetBytes(withoutBuyerReference), "invoice.xml");
            Assert.IsTrue(report.IsSchemaValid);
            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(AcceptRecommendation.Reject, report.Recommendation);
            ValidationMessage error = report.Errors.Single(m => m.Code == "BR-DE-15");
            Assert.AreEqual("val-sch.2", error.StepId);
            Assert.AreEqual(ValidationLevel.Error, error.Level);
            Assert.AreEqual("/Q{urn:oasis:names:specification:ubl:schema:xsd:Invoice-2}Invoice[1]", error.XPathLocation);
            Assert.IsNotNull(error.SchematronMessage);
            Assert.AreEqual("fatal", error.SchematronMessage.Flag);

            XDocument xmlReport = report.ToXml();
            _AssertValidAgainstKositSchemas(xmlReport);
            Assert.AreEqual("false", (string?)xmlReport.Root!.Attribute("valid"));
            Assert.IsNotNull(xmlReport.Root.Element(_Rep + "assessment")!.Element(_Rep + "reject"));
            XElement message = xmlReport.Descendants(_Rep + "message").Single(m => (string?)m.Attribute("code") == "BR-DE-15");
            Assert.AreEqual("error", (string?)message.Attribute("level"));
            Assert.AreEqual(error.XPathLocation, (string?)message.Attribute("xpathLocation"));
        }


        [TestMethod]
        public void SchemaErrorsSkipSchematronByDefault()
        {
            string xml = Encoding.UTF8.GetString(_ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL));
            byte[] invalid = Encoding.UTF8.GetBytes(xml.Replace("<cbc:IssueDate>", "<cbc:Unknown>1</cbc:Unknown><cbc:IssueDate>"));

            ValidationReport report = new InvoiceXmlValidator().Validate(invalid);
            Assert.AreEqual("EN16931 XRechnung (UBL Invoice)", report.Scenario?.Name);
            Assert.IsTrue(report.IsWellFormed);
            Assert.IsFalse(report.IsSchemaValid);
            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(AcceptRecommendation.Reject, report.Recommendation);
            CollectionAssert.AreEqual(new[] { "val-xsd", "val-xml" }, report.Steps.Select(s => s.Id).ToArray());
            ValidationMessage error = report.Errors.First();
            Assert.AreEqual("val-xsd.1", error.Id);
            Assert.AreEqual("generic-error", error.Code);
            Assert.IsNotNull(error.LineNumber);
            Assert.IsNotNull(error.ColumnNumber);
            _AssertValidAgainstKositSchemas(report.ToXml());

            ValidationReport withSchematron = new InvoiceXmlValidator(new ValidatorOptions { ValidateSchematronOnSchemaErrors = true }).Validate(invalid);
            CollectionAssert.AreEqual(new[] { "val-xsd", "val-sch.1", "val-sch.2", "val-xml" }, withSchematron.Steps.Select(s => s.Id).ToArray());
            Assert.AreEqual(AcceptRecommendation.Reject, withSchematron.Recommendation);
        }


        [TestMethod]
        public void DocumentThatIsNotWellFormed()
        {
            ValidationReport report = new InvoiceXmlValidator().Validate(Encoding.UTF8.GetBytes("<Invoice><cbc:ID>1</Invoice>"), "broken.xml");
            Assert.IsFalse(report.IsWellFormed);
            Assert.IsNull(report.Scenario);
            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(AcceptRecommendation.Reject, report.Recommendation);
            ValidationMessage error = report.Errors.Single();
            Assert.AreEqual("val-xml.1", error.Id);
            StringAssert.StartsWith(error.Text, "Error reported by XML parser: ");

            XDocument xml = report.ToXml();
            _AssertValidAgainstKositSchemas(xml);
            Assert.IsNotNull(xml.Root!.Element(_Rep + "noScenarioMatched"));
        }


        [TestMethod]
        public void DocumentTypeDefinitionsAreNotProcessed()
        {
            string xml = "<?xml version=\"1.0\"?><!DOCTYPE lolz [<!ENTITY lol \"lol\"><!ENTITY lol2 \"&lol;&lol;&lol;&lol;\">]><x>&lol2;</x>";
            ValidationReport report = new InvoiceXmlValidator().Validate(Encoding.UTF8.GetBytes(xml));
            Assert.IsFalse(report.IsWellFormed);
            Assert.AreEqual(AcceptRecommendation.Reject, report.Recommendation);
        }


        [TestMethod]
        public void UnknownDocumentMatchesNoScenario()
        {
            ValidationReport report = new InvoiceXmlValidator().Validate(Encoding.UTF8.GetBytes("<Order xmlns='urn:example:order'/>"));
            Assert.IsTrue(report.IsWellFormed);
            Assert.IsNull(report.Scenario);
            Assert.IsNull(report.Configuration);
            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(AcceptRecommendation.Reject, report.Recommendation);
            CollectionAssert.AreEqual(new[] { "val-xml" }, report.Steps.Select(s => s.Id).ToArray());
            _AssertValidAgainstKositSchemas(report.ToXml());
            StringAssert.Contains(report.ToHtml(), "Es wird empfohlen das Dokument zurückzuweisen. Da kein Pruefszenario gegriffen hat.");
        }


        [TestMethod]
        public void HtmlReport()
        {
            byte[] xml = _ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.CII);

            string german = new InvoiceXmlValidator().Validate(xml, "rechnung.xml").ToHtml();
            StringAssert.Contains(german, "<title>Prüfbericht</title>");
            StringAssert.Contains(german, "Es wird empfohlen das Dokument anzunehmen und weiter zu verarbeiten.");
            StringAssert.Contains(german, "Lieferant GmbH");
            StringAssert.Contains(german, "Inhalt des Rechnungsdokuments:");

            string english = new InvoiceXmlValidator(new ValidatorOptions { ReportLanguage = ReportLanguage.English, IncludeDocumentContentInReport = false }).Validate(xml, "invoice.xml").ToHtml();
            StringAssert.Contains(english, "<title>Validation report</title>");
            StringAssert.Contains(english, "It is recommended to accept and process the document.");
            Assert.DoesNotContain("Content of the invoice document:", english);

            // well-formed XHTML
            XDocument.Parse(german);
            XDocument.Parse(english);
        }


        [TestMethod]
        public void SaveReports()
        {
            ValidationReport report = new InvoiceXmlValidator().Validate(_ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.Comfort, ZUGFeRDFormats.CII));
            using MemoryStream stream = new MemoryStream();
            report.SaveXml(stream);
            stream.Position = 0;
            XDocument loaded = XDocument.Load(stream);
            Assert.AreEqual(_Rep + "report", loaded.Root!.Name);
            Assert.AreEqual(report.DocumentHash, loaded.Root.Element(_Rep + "documentIdentification")!.Element(_Rep + "documentHash")!.Element(_Rep + "hashValue")!.Value);

            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".html");
            try
            {
                report.SaveHtml(path);
                StringAssert.Contains(File.ReadAllText(path, Encoding.UTF8), "Prüfbericht");
            }
            finally
            {
                File.Delete(path);
            }
        }


        [TestMethod]
        [DataRow("xrechnung.xml")]
        [DataRow("factur-x.xml")]
        [DataRow("en16931.xml")]
        public void BuiltInConfigurationsAreValidScenarioFiles(string name)
        {
            XDocument scenarios;
            using (Stream stream = ResourceLoader.Open(ResourceLoader.EmbeddedUri("scenarios/" + name)))
            {
                scenarios = XDocument.Load(stream);
            }
            _AssertValidAgainstKositSchemas(scenarios);
        }


        [TestMethod]
        public void BuiltInConfigurationsCompile()
        {
            foreach (ValidatorConfiguration configuration in ValidatorConfiguration.Default)
            {
                configuration.Compile();
                Assert.IsNotEmpty(configuration.Scenarios);
                foreach (ValidationScenario scenario in configuration.Scenarios)
                {
                    Assert.IsNotEmpty(scenario.XmlSchemas, scenario.Name);
                    Assert.IsNotEmpty(scenario.SchematronRules, scenario.Name);
                }
            }
            Assert.HasCount(11, ValidatorConfiguration.XRechnung.Scenarios);
            Assert.HasCount(5, ValidatorConfiguration.FacturX.Scenarios);
            Assert.HasCount(3, ValidatorConfiguration.EN16931.Scenarios);
        }


        [TestMethod]
        public void CustomConfiguration()
        {
            ValidatorConfiguration configuration = _CustomConfiguration();
            Assert.AreEqual("Test configuration", configuration.Name);
            ValidationScenario scenario = configuration.Scenarios.Single();
            Assert.AreEqual("Example order", scenario.Name);
            Assert.AreEqual(ValidationLevel.Warning, scenario.CustomLevels.Single().Level);

            InvoiceXmlValidator validator = new InvoiceXmlValidator(configuration);
            ValidationReport report = validator.Validate(XPathEngineTests.TestDataPath(Path.Combine("custom", "order.xml")));
            Assert.AreSame(scenario, report.Scenario);
            Assert.IsTrue(report.IsSchemaValid);
            CollectionAssert.AreEqual(new[] { "val-xsd", "val-sch.1", "val-xml" }, report.Steps.Select(s => s.Id).ToArray());
            CollectionAssert.AreEqual(new[] { "H-01", "H-02", "H-03", "L-99", "L-03", "L-01", "L-02", "L-03", "P-01" }, report.Messages.Select(m => m.Code).ToArray());
            CollectionAssert.AreEqual(new[] { "val-sch.1.1", "val-sch.1.2" }, report.Messages.Take(2).Select(m => m.Id).ToArray());

            // levels from flag and role; a message without flag and role is an error; L-01 is lowered by the customLevel
            CollectionAssert.AreEqual(new[] { "H-01", "L-99", "L-02", "P-01" }, report.Errors.Select(m => m.Code).ToArray());
            CollectionAssert.AreEqual(new[] { "H-02", "L-03", "L-01", "L-03" }, report.Warnings.Select(m => m.Code).ToArray());
            ValidationMessage l01 = report.Messages.Single(m => m.Code == "L-01");
            Assert.AreEqual(ValidationLevel.Error, l01.Level);
            Assert.AreEqual(ValidationLevel.Warning, l01.CustomLevel);
            Assert.AreEqual(ValidationLevel.Information, report.Messages.Single(m => m.Code == "H-03").CustomLevel);
            Assert.AreEqual(AcceptRecommendation.Reject, report.Recommendation);

            XDocument xml = report.ToXml();
            _AssertValidAgainstKositSchemas(xml);
            XElement l01Element = xml.Descendants(_Rep + "message").Single(m => (string?)m.Attribute("code") == "L-01");
            // as in the KoSIT validator, the report holds the level of the rule; the custom level only affects the assessment
            Assert.AreEqual("error", (string?)l01Element.Attribute("level"));
            Assert.IsNotNull(xml.Root!.Element(_Rep + "assessment")!.Element(_Rep + "reject"));
        }


        [TestMethod]
        public void CustomLevelCanTolerateErrors()
        {
            // only L-01 fails: an error, lowered to a warning by the configuration, so the document is accepted
            string order = "<Order xmlns='urn:example:order'><Id>1</Id><Line><Product>ABC-1</Product><Quantity>150</Quantity><Price>1</Price><Total>150</Total></Line></Order>";
            ValidationReport report = new InvoiceXmlValidator(_CustomConfiguration()).Validate(Encoding.UTF8.GetBytes(order));
            Assert.AreEqual("L-01", report.Messages.Single().Code);
            Assert.IsEmpty(report.Errors.ToList());
            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(AcceptRecommendation.Accept, report.Recommendation);
            Assert.IsTrue(report.IsAcceptable);
            StringAssert.Contains(report.ToHtml(), "Es wird empfohlen das Dokument anzunehmen und zu verarbeiten, da die vorhandenen Fehler derzeit toleriert werden.");
        }


        [TestMethod]
        public void ValidOrderIsAccepted()
        {
            string order = "<Order xmlns='urn:example:order'><Id>1</Id><Line><Product>ABC-1</Product><Quantity>2</Quantity><Price>1.5</Price><Total>3.0</Total></Line></Order>";
            ValidationReport report = new InvoiceXmlValidator(_CustomConfiguration()).Validate(Encoding.UTF8.GetBytes(order));
            Assert.IsEmpty(report.Messages.ToList(), _Describe(report));
            Assert.IsTrue(report.IsValid);
            Assert.IsTrue(report.IsAcceptable);
            _AssertValidAgainstKositSchemas(report.ToXml());
        }


        [TestMethod]
        public void DocumentMatchingSeveralScenariosMatchesNone()
        {
            string scenario = "<scenario><name>{0}</name><namespace prefix='o'>urn:example:order</namespace><match>/o:Order</match>"
                + "<validateWithXmlSchema><resource><name>Schema</name><location>order.xsd</location></resource></validateWithXmlSchema>"
                + "<createReport><resource><name>Report</name><location>report.xsl</location></resource></createReport></scenario>";
            ValidatorConfiguration configuration = _ConfigurationFromString("<scenarios xmlns='http://www.xoev.de/de/validator/framework/1/scenarios'><name>Ambiguous</name>"
                + string.Format(scenario, "A") + string.Format(scenario, "B")
                + "<noScenarioReport><resource><name>Report</name><location>report.xsl</location></resource></noScenarioReport></scenarios>");

            ValidationReport report = new InvoiceXmlValidator(new[] { configuration, _CustomConfiguration() }).Validate(Encoding.UTF8.GetBytes("<Order xmlns='urn:example:order'><Line/></Order>"));
            // the first configuration with a matching scenario decides, as in the KoSIT validator
            Assert.IsNull(report.Scenario);
            Assert.AreEqual(AcceptRecommendation.Reject, report.Recommendation);
        }


        [TestMethod]
        public void CompiledSchematronIsReplacedBySource()
        {
            string configuration = "<scenarios xmlns='http://www.xoev.de/de/validator/framework/1/scenarios'><name>XSLT</name><scenario><name>Order</name>"
                + "<namespace prefix='o'>urn:example:order</namespace><match>/o:Order</match>"
                + "<validateWithSchematron><resource><name>Rules</name><location>{0}</location></resource></validateWithSchematron>"
                + "<createReport><resource><name>Report</name><location>report.xsl</location></resource></createReport></scenario></scenarios>";

            ValidationScenario scenario = _ConfigurationFromString(string.Format(configuration, "rules.xsl")).Scenarios.Single();
            Assert.AreEqual("rules.xsl", scenario.SchematronRules.Single().Location);
            ValidationReport report = new InvoiceXmlValidator(_ConfigurationFromString(string.Format(configuration, "rules.xsl"))).Validate(XPathEngineTests.TestDataPath(Path.Combine("custom", "order.xml")));
            Assert.AreEqual(9, report.Messages.Count());
            CollectionAssert.AreEqual(new[] { "val-sch.1", "val-xml" }, report.Steps.Select(s => s.Id).ToArray());

            Assert.ThrowsExactly<NotSupportedException>(() => _ConfigurationFromString(string.Format(configuration, "missing.xsl")));
        }


        [TestMethod]
        public void InvalidMatchExpressionIsRejected()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => _ConfigurationFromString("<scenarios xmlns='http://www.xoev.de/de/validator/framework/1/scenarios'><name>X</name>"
                + "<scenario><name>Broken</name><match>/o:Order[</match></scenario></scenarios>"));
        }


        [TestMethod]
        public void ValidatorIsThreadSafe()
        {
            InvoiceXmlValidator validator = new InvoiceXmlValidator();
            List<byte[]> documents = new List<byte[]>
            {
                _ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL),
                _ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.CII),
                _ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.Comfort, ZUGFeRDFormats.CII),
                _ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.Extended, ZUGFeRDFormats.CII),
                Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(_ReadmeInvoiceXml(ZUGFeRDVersion.Version23, Profile.XRechnung, ZUGFeRDFormats.UBL)).Replace("<cbc:BuyerReference>", "<cbc:BuyerReference>x"))
            };
            string[] expected = documents.Select(d => _Summary(validator.Validate(d))).ToArray();

            string[] actual = new string[documents.Count * 8];
            Parallel.For(0, actual.Length, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
            {
                actual[i] = _Summary(validator.Validate(documents[i % documents.Count]));
            });
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(expected[i % documents.Count], actual[i]);
            }
        }


        private static string _Summary(ValidationReport report)
        {
            return report.Scenario?.Name + "|" + report.Recommendation + "|" + string.Join(";", report.Messages.Select(m => m.Id + " " + m.Code + " " + m.XPathLocation + " " + m.Text));
        }
    }
}
