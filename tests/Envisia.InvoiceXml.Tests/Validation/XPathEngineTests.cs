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
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Tests.Validation
{
    /// <summary>
    /// Compares the XPath engine of the validator with Saxon-HE 12 (the XSLT processor of the KoSIT validator).
    ///
    /// TestData/xpath-expected.txt holds the results of TestData/xpath-expressions.txt evaluated by Saxon against
    /// TestData/xpath-context.xml (one line per expression: "OK:" + the string values joined by "|", or "ERROR:" + the error code).
    /// </summary>
    [TestClass]
    public class XPathEngineTests
    {
        internal static string TestDataPath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Validation", "TestData", fileName);
        }


        internal static StaticContext CreateStaticContext()
        {
            return new StaticContext(new Dictionary<string, string>
            {
                ["ubl"] = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2",
                ["cac"] = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2",
                ["cbc"] = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
            }, null, null);
        }


        internal static string Evaluate(string expression, XdmDocument document)
        {
            try
            {
                XPathExpression compiled = XPathExpression.Compile(expression, CreateStaticContext());
                EvalContext context = new EvalContext(new EvaluationEnvironment());
                Sequence result = compiled.Evaluate(context, document.Root);
                return "OK:" + string.Join("|", result.Select(i => (i is XdmNode n ? n.StringValue : ((AtomicValue)i).StringValue).Replace("\n", "\\n")));
            }
            catch (XPathException e)
            {
                return "ERROR:" + e.Code;
            }
        }


        [TestMethod]
        public void ResultsMatchSaxon()
        {
            XdmDocument document;
            using (FileStream stream = File.OpenRead(TestDataPath("xpath-context.xml")))
            {
                document = XdmDocumentBuilder.Load(stream, "file:///xpath-context.xml");
            }
            string[] expressions = File.ReadAllLines(TestDataPath("xpath-expressions.txt"));
            string[] expected = File.ReadAllLines(TestDataPath("xpath-expected.txt"));
            Assert.AreEqual(expressions.Length, expected.Length);

            List<string> failures = new List<string>();
            for (int i = 0; i < expressions.Length; i++)
            {
                string actual = Evaluate(expressions[i], document);
                if (actual != expected[i])
                {
                    failures.Add($"{expressions[i]}\n    expected {expected[i]}\n    actual   {actual}");
                }
            }
            Assert.AreEqual(0, failures.Count, failures.Count + " of " + expressions.Length + " expressions differ from Saxon:\n" + string.Join("\n", failures));
        }
    }
}
