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
using System.Xml.Linq;
using Envisia.InvoiceXml.Validation.Schematron;
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Tests.Validation
{
    /// <summary>
    /// Runs the unit tests of the CEN EN 16931 validation artefacts (TestData/en16931-unit-tests, format of the
    /// vefa validator) against the embedded EN 16931 Schematron rules.
    /// </summary>
    [TestClass]
    public class EN16931UnitTests
    {
        private static readonly XNamespace _Vefa = "http://difi.no/xsd/vefa/validator/1.0";

        private static readonly Lazy<SchematronSchema> _Ubl = new Lazy<SchematronSchema>(() => SchematronSchema.LoadResource(ResourceLoader.EmbeddedUri("en16931/ubl/EN16931-UBL-validation.sch")));
        private static readonly Lazy<SchematronSchema> _Cii = new Lazy<SchematronSchema>(() => SchematronSchema.LoadResource(ResourceLoader.EmbeddedUri("en16931/cii/EN16931-CII-validation.sch")));


        [TestMethod]
        [DataRow("Invoice-unit-UBL")]
        [DataRow("CreditNote-unit-UBL")]
        [DataRow("cii")]
        public void UnitTestsPass(string folder)
        {
            SchematronSchema schema = folder == "cii" ? _Cii.Value : _Ubl.Value;
            List<string> failures = new List<string>();
            int tests = 0;
            foreach (string file in Directory.GetFiles(XPathEngineTests.TestDataPath(Path.Combine("en16931-unit-tests", folder)), "*.xml").OrderBy(f => f))
            {
                XDocument testSet = XDocument.Load(file, LoadOptions.PreserveWhitespace);
                int index = 0;
                foreach (XElement test in testSet.Root!.Elements(_Vefa + "test"))
                {
                    index++;
                    tests++;
                    XElement document = test.Elements().Single(e => e.Name != _Vefa + "assert");
                    SchematronResult result = schema.Validate(new MemoryStream(Encoding.UTF8.GetBytes(document.ToString(SaveOptions.DisableFormatting))));
                    List<SchematronMessage> failed = result.FailedAssertions.ToList();
                    foreach (XElement expectation in test.Element(_Vefa + "assert")!.Elements())
                    {
                        string id = expectation.Value.Trim();
                        List<SchematronMessage> matches = failed.Where(m => m.Id == id).ToList();
                        string where = Path.GetFileName(file) + " test " + index + " (" + (string?)test.Element(_Vefa + "assert")?.Element(_Vefa + "description") + "): ";
                        switch (expectation.Name.LocalName)
                        {
                            case "success":
                                if (matches.Count > 0)
                                {
                                    failures.Add(where + id + " was expected to pass but failed: " + matches[0].Text.Trim());
                                }
                                break;
                            case "error":
                            case "warning":
                                string flag = expectation.Name.LocalName == "error" ? "fatal" : "warning";
                                int expected = (int?)expectation.Attribute("number") ?? -1;
                                int actual = matches.Count(m => m.Flag == flag);
                                if (expected >= 0 ? actual != expected : actual == 0)
                                {
                                    failures.Add(where + id + " was expected as " + flag + (expected >= 0 ? " " + expected + " times" : "") + ", found " + actual);
                                }
                                break;
                        }
                    }
                }
            }
            Assert.IsTrue(tests > 0, "no tests found");
            Assert.AreEqual(0, failures.Count, failures.Count + " of " + tests + " unit tests failed:\n" + string.Join("\n", failures));
        }
    }
}
