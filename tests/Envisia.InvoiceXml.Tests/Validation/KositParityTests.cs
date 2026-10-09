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
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Envisia.InvoiceXml.Validation;

namespace Envisia.InvoiceXml.Tests.Validation
{
    /// <summary>
    /// Validates the XRechnung test suite, the test documents of the KoSIT validator configuration and the
    /// official ZUGFeRD / Factur-X samples and compares the reports with the reports of the KoSIT validator
    /// (TestData/kosit-reference.txt): scenario, validity, assessment, validation steps and every Schematron
    /// message with code, level, location and text.
    /// </summary>
    [TestClass]
    public class KositParityTests
    {
        private static readonly XNamespace _Rep = "http://www.xoev.de/de/validator/varl/1";
        private static readonly XNamespace _S = "http://www.xoev.de/de/validator/framework/1/scenarios";
        private const string _TestSuite = "documentation/xRechnung/XRechnung 3.0.2/xrechnung-3.0.2-testsuite-2026-08-31.zip";


        private static Dictionary<string, List<string>> _LoadReference()
        {
            Dictionary<string, List<string>> reference = new Dictionary<string, List<string>>();
            List<string>? current = null;
            foreach (string line in File.ReadAllLines(XPathEngineTests.TestDataPath("kosit-reference.txt")))
            {
                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    current = new List<string>();
                    reference[line.Substring(3)] = current;
                }
                else if (current != null && !line.StartsWith("#", StringComparison.Ordinal))
                {
                    current.Add(line);
                }
            }
            return reference;
        }


        private static byte[] _ReadDocument(string key, ZipArchive testSuite)
        {
            if (key.StartsWith("xrechnung-testsuite/", StringComparison.Ordinal))
            {
                using (Stream stream = testSuite.GetEntry(key.Substring("xrechnung-testsuite/".Length))!.Open())
                using (MemoryStream buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    return buffer.ToArray();
                }
            }
            if (key.StartsWith("kosit-instances/", StringComparison.Ordinal))
            {
                return File.ReadAllBytes(XPathEngineTests.TestDataPath(key));
            }
            return File.ReadAllBytes(Path.Combine(SchemaValidator.RepositoryRoot, key));
        }


        internal static List<string> Summarize(XDocument report)
        {
            XElement root = report.Root!;
            List<string> summary = new List<string>();
            XElement? matched = root.Element(_Rep + "scenarioMatched");
            summary.Add("scenario=" + (matched?.Element(_S + "scenario")?.Element(_S + "name")?.Value.Trim() ?? "(none)"));
            summary.Add("valid=" + (string?)root.Attribute("valid"));
            summary.Add("assessment=" + root.Element(_Rep + "assessment")!.Elements().First().Name.LocalName);
            foreach (XElement step in root.Descendants(_Rep + "validationStepResult"))
            {
                string id = (string)step.Attribute("id")!;
                List<XElement> messages = step.Elements(_Rep + "message").ToList();
                summary.Add($"step {id} valid={(string?)step.Attribute("valid")} messages={messages.Count}");
                if (id.StartsWith("val-sch", StringComparison.Ordinal))
                {
                    foreach (XElement message in messages)
                    {
                        summary.Add($"message {(string?)message.Attribute("id")}|{(string?)message.Attribute("code")}|{(string?)message.Attribute("level")}|{(string?)message.Attribute("xpathLocation")}|{Regex.Replace(message.Value, @"\s+", " ").Trim()}");
                    }
                }
            }
            return summary;
        }


        [TestMethod]
        public void ReportsMatchKositValidator()
        {
            Dictionary<string, List<string>> reference = _LoadReference();
            Assert.IsGreaterThan(150, reference.Count, "reference results missing");
            InvoiceXmlValidator xrechnung = new InvoiceXmlValidator(ValidatorConfiguration.XRechnung);
            InvoiceXmlValidator facturX = new InvoiceXmlValidator(ValidatorConfiguration.FacturX);

            List<string> failures = new List<string>();
            using (ZipArchive testSuite = ZipFile.OpenRead(Path.Combine(SchemaValidator.RepositoryRoot, _TestSuite)))
            {
                foreach (KeyValuePair<string, List<string>> entry in reference)
                {
                    int space = entry.Key.IndexOf(' ');
                    string configuration = entry.Key.Substring(0, space);
                    string document = entry.Key.Substring(space + 1);
                    InvoiceXmlValidator validator = configuration == "xrechnung" ? xrechnung : facturX;
                    ValidationReport report = validator.Validate(_ReadDocument(document, testSuite), document);
                    List<string> actual = Summarize(report.ToXml());
                    if (!actual.SequenceEqual(entry.Value))
                    {
                        failures.Add(entry.Key + "\n    expected:\n      " + string.Join("\n      ", entry.Value.Except(actual)) + "\n    actual:\n      " + string.Join("\n      ", actual.Except(entry.Value)));
                    }
                }
            }
            Assert.IsEmpty(failures, failures.Count + " of " + reference.Count + " reports differ from the KoSIT validator:\n" + string.Join("\n", failures.Take(20)));
        }
    }
}
