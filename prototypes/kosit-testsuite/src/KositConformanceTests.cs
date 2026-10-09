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
using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;

// the engines must be thread-safe; running the tests in parallel also exercises that
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]

namespace Envisia.KositPrototypes.Tests
{
    /// <summary>
    /// Conformance tests for a way of running the KoSIT validator from .NET (<see cref="IKositEngine"/>):
    ///
    /// - every report equals the report of the official KoSIT validator on the JVM (reference/kosit-*-jvm.txt) for the
    ///   XRechnung test suite, the tests of the XRechnung configuration, the CEN examples and the Factur-X samples,
    /// - the assertions of the KoSIT XRechnung configuration's own test suite (assertions.xml,
    ///   assertions-integration-testing.xml) hold,
    /// - the CEN EN 16931 unit tests pass with CEN's compiled Schematron (Saxon in the engine),
    /// - concurrent validations give the same results as sequential ones.
    /// </summary>
    public abstract class KositConformanceTests
    {
        private static readonly XNamespace _Assertions = "http://www.xoev.de/de/validator/framework/1/assertions";
        private static readonly XNamespace _Vefa = "http://difi.no/xsd/vefa/validator/1.0";
        private static readonly XNamespace _Svrl = "http://purl.oclc.org/dsdl/svrl";
        private static readonly ConcurrentDictionary<string, Lazy<IKositValidator>> _Validators = new ConcurrentDictionary<string, Lazy<IKositValidator>>();


        /// <summary>The engine under test.</summary>
        protected abstract IKositEngine Engine { get; }

        /// <summary>Documents whose report is known to differ from the JVM reference, with the reason.</summary>
        protected virtual IReadOnlyDictionary<string, string> KnownDifferences => new Dictionary<string, string>();


        private IKositValidator _Validator(string configuration, string scenarios)
        {
            return _Validators.GetOrAdd(Path.Combine(configuration, scenarios),
                                        path => new Lazy<IKositValidator>(() => Engine.LoadConfiguration(path, configuration))).Value;
        }


        private IKositValidator _Validator(string set)
        {
            return _Validator(set == "facturx" ? KositTestData.FacturXConfiguration : KositTestData.XRechnungConfiguration, "scenarios.xml");
        }


        public static IEnumerable<object[]> ReferenceDocuments => KositTestData.Reference.Keys.Select(k => new object[] { k.Set, k.Id });


        [TestMethod]
        [DynamicData(nameof(ReferenceDocuments), typeof(KositConformanceTests))]
        public void ReportMatchesJvmReference(string set, string id)
        {
            KositTestData.RequirePrepared();
            (string Set, string Id, string Path) document = KositTestData.Manifest().SingleOrDefault(d => d.Set == set && d.Id == id);
            Assert.IsNotNull(document.Path, "document missing in build/manifest.txt: " + id);

            string expected = KositTestData.Reference[(set, id)];
            string actual = ReportSummary.Create(_Validator(set).Validate(File.ReadAllBytes(document.Path), id));
            if (actual != expected && KnownDifferences.TryGetValue(id, out string? reason))
            {
                Assert.Inconclusive("Known difference: " + reason);
            }
            if (actual != expected)
            {
                string[] expectedLines = expected.Split('\n'), actualLines = actual.Split('\n');
                Assert.Fail("The report differs from the JVM reference:\n"
                            + string.Join("\n", expectedLines.Except(actualLines).Select(l => "  jvm:    " + l)) + "\n"
                            + string.Join("\n", actualLines.Except(expectedLines).Select(l => "  engine: " + l)));
            }
        }


        [TestMethod]
        public void ManifestMatchesReference()
        {
            KositTestData.RequirePrepared();
            CollectionAssert.AreEquivalent(KositTestData.Reference.Keys.ToList(), KositTestData.Manifest().Select(d => (d.Set, d.Id)).ToList(),
                                           "build/manifest.txt and the reference list different documents; recreate the reference with make-reference.sh");
        }


        public static IEnumerable<object[]> KositAssertionReports
        {
            get
            {
                foreach (string file in new[] { "assertions.xml", "assertions-integration-testing.xml" })
                {
                    string path = Path.Combine(KositTestData.KositTests, file);
                    if (!File.Exists(path))
                    {
                        yield return new object[] { file, "(test data not prepared)" };
                        continue;
                    }
                    foreach (string report in XDocument.Load(path).Root!.Elements(_Assertions + "assertion").Select(a => (string)a.Attribute("report-doc")!).Distinct())
                    {
                        yield return new object[] { file, report };
                    }
                }
            }
        }


        /// <summary>
        /// The assertions of the XRechnung configuration's own tests, as its build runs them: assertions.xml for the
        /// instances with the test scenarios (customLevel BR-09), assertions-integration-testing.xml for the
        /// integration documents with the released scenarios.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(KositAssertionReports), typeof(KositConformanceTests))]
        public void KositConfigurationAssertionsHold(string assertionsFile, string reportDocument)
        {
            KositTestData.RequirePrepared();
            bool integration = assertionsFile == "assertions-integration-testing.xml";
            string baseName = reportDocument.Substring(0, reportDocument.Length - "-report.xml".Length);
            string[] folders = integration ? new[] { "integration", "cen-unit-test" } : new[] { "instances" };
            string document = folders.SelectMany(f => Directory.GetFiles(Path.Combine(KositTestData.KositTests, f), baseName + ".xml", SearchOption.AllDirectories)).First();

            string report = _Validator(KositTestData.XRechnungConfiguration, integration ? "scenarios.xml" : "scenarios-test.xml")
                .Validate(File.ReadAllBytes(document), Path.GetFileName(document));

            XElement assertions = XDocument.Load(Path.Combine(KositTestData.KositTests, assertionsFile)).Root!;
            Dictionary<string, string> namespaces = assertions.Elements(_Assertions + "namespace").ToDictionary(n => (string)n.Attribute("prefix")!, n => n.Value.Trim());
            List<string> failures = new List<string>();
            int count = 0;
            foreach (XElement assertion in assertions.Elements(_Assertions + "assertion").Where(a => (string?)a.Attribute("report-doc") == reportDocument))
            {
                count++;
                string test = (string)assertion.Attribute("test")!;
                if (!Engine.EvaluateBoolean(report, test, namespaces))
                {
                    failures.Add(ReportSummary.Normalize(assertion.Value) + " (" + test + ")");
                }
            }
            Assert.IsGreaterThan(0, count);
            Assert.IsEmpty(failures, count + " assertions, failed:\n" + string.Join("\n", failures));
        }


        public static IEnumerable<object[]> CenUnitTestFiles
        {
            get
            {
                string tests = Path.Combine(KositTestData.EN16931, "test");
                if (!Directory.Exists(tests))
                {
                    yield return new object[] { "(test data not prepared)" };
                    yield break;
                }
                foreach (string folder in new[] { "Invoice-unit-UBL", "CreditNote-unit-UBL", "cii" })
                {
                    foreach (string file in Directory.GetFiles(Path.Combine(tests, folder), "*.xml").OrderBy(f => f, StringComparer.Ordinal))
                    {
                        yield return new object[] { folder + "/" + Path.GetFileName(file) };
                    }
                }
            }
        }


        /// <summary>
        /// The unit tests of the CEN EN 16931 validation artefacts (vefa format) with CEN's compiled Schematron.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(CenUnitTestFiles), typeof(KositConformanceTests))]
        public void CenUnitTestsPass(string file)
        {
            KositTestData.RequirePrepared();
            bool cii = file.StartsWith("cii/", StringComparison.Ordinal);
            string stylesheet = Path.Combine(KositTestData.EN16931, cii ? "cii/xslt/EN16931-CII-validation.xslt" : "ubl/xslt/EN16931-UBL-validation.xslt");
            XDocument testSet = XDocument.Load(Path.Combine(KositTestData.EN16931, "test", file), LoadOptions.PreserveWhitespace);
            List<string> failures = new List<string>();
            int index = 0;
            foreach (XElement test in testSet.Root!.Elements(_Vefa + "test"))
            {
                index++;
                XElement expectations = test.Element(_Vefa + "assert")!;
                XElement document = test.Elements().Single(e => e.Name != _Vefa + "assert");
                string svrl = Engine.Transform(stylesheet, Encoding.UTF8.GetBytes(document.ToString(SaveOptions.DisableFormatting)));
                List<(string Id, string Flag)> failed = XDocument.Parse(svrl).Descendants(_Svrl + "failed-assert")
                    .Select(f => ((string?)f.Attribute("id") ?? "", (string?)f.Attribute("flag") ?? "")).ToList();
                string where = "test " + index + ": ";
                foreach (XElement expectation in expectations.Elements())
                {
                    string id = expectation.Value.Trim();
                    switch (expectation.Name.LocalName)
                    {
                        case "success":
                            if (failed.Any(f => f.Id == id))
                            {
                                failures.Add(where + id + " was expected to pass");
                            }
                            break;
                        case "error":
                        case "warning":
                            string flag = expectation.Name.LocalName == "error" ? "fatal" : "warning";
                            int expected = (int?)expectation.Attribute("number") ?? -1;
                            int actual = failed.Count(f => f.Id == id && f.Flag == flag);
                            if (expected >= 0 ? actual != expected : actual == 0)
                            {
                                failures.Add(where + id + " expected as " + flag + (expected >= 0 ? " " + expected + " times" : "") + ", found " + actual);
                            }
                            break;
                    }
                }
            }
            Assert.IsGreaterThan(0, index);
            // the expectations Saxon on the JVM does not meet either are stale upstream tests, not engine errors
            List<string> jvm = KositTestData.CenUnitTestReference.Where(r => r.File == file).Select(r => r.Failure).ToList();
            CollectionAssert.AreEquivalent(jvm, failures, "Unit test results differ from Saxon on the JVM.\nengine: " + string.Join("; ", failures) + "\njvm: " + string.Join("; ", jvm));
        }


        [TestMethod]
        public void ConcurrentValidationsMatchSequentialOnes()
        {
            KositTestData.RequirePrepared();
            List<(string Set, string Id, string Path)> documents = KositTestData.Manifest().Where(d => d.Set == "xrechnung").Take(24).ToList();
            IKositValidator validator = _Validator("xrechnung");
            Dictionary<string, string> sequential = documents.ToDictionary(d => d.Id, d => ReportSummary.Create(validator.Validate(File.ReadAllBytes(d.Path), d.Id)));
            ConcurrentBag<string> mismatches = new ConcurrentBag<string>();
            Parallel.ForEach(documents.Concat(documents), new ParallelOptions { MaxDegreeOfParallelism = 4 }, d =>
            {
                if (ReportSummary.Create(validator.Validate(File.ReadAllBytes(d.Path), d.Id)) != sequential[d.Id])
                {
                    mismatches.Add(d.Id);
                }
            });
            Assert.IsEmpty(mismatches, string.Join("\n", mismatches));
        }
    }
}
