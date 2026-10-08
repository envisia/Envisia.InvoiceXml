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
using System.Xml;

namespace Envisia.InvoiceXml.Tests
{
    /// <summary>
    /// Conformance tests against the official ZUGFeRD 2.4 / Factur-X 1.08 package (schemas and examples in
    /// documentation/zugferd240en) and the ZUGFeRD 2.5.2 / Factur-X 1.09.2 schemas (documentation/zugferd252).
    /// </summary>
    [TestClass]
    public class FacturXConformanceTests : TestBase
    {
        private readonly InvoiceProvider _InvoiceProvider = new InvoiceProvider();


        public static IEnumerable<object[]> OfficialExamples
        {
            get
            {
                string examplesFolder = Path.Combine(SchemaValidator.RepositoryRoot, "documentation", "zugferd240en", "Examples");
                return Directory.GetFiles(examplesFolder, "*.xml", SearchOption.AllDirectories)
                                .OrderBy(path => path, StringComparer.Ordinal)
                                .Select(path => new object[] { Path.GetRelativePath(examplesFolder, path) });
            }
        }


        public static IEnumerable<object[]> SchemaVersions => new[]
        {
            new object[] { SchemaValidator.FacturX108 },
            new object[] { SchemaValidator.FacturX1092 }
        };


        public static IEnumerable<object[]> ProfilesForEverySchemaVersion
        {
            get
            {
                Profile[] profiles = { Profile.Minimum, Profile.BasicWL, Profile.Basic, Profile.Comfort, Profile.Extended, Profile.XRechnung };
                return SchemaVersions.SelectMany(version => profiles.Select(profile => new object[] { version[0], profile }));
            }
        }


        public static IEnumerable<object[]> OfficialExamplesForEverySchemaVersion
        {
            get
            {
                return SchemaVersions.SelectMany(version => OfficialExamples.Select(example => new object[] { version[0], example[0] }));
            }
        }


        [TestMethod]
        [DynamicData(nameof(ProfilesForEverySchemaVersion))]
        public void TestGeneratedInvoiceIsSchemaValid(string schemaVersion, Profile profile)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, profile);

            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetFacturXSchemaPath(schemaVersion, profile));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        } // !TestGeneratedInvoiceIsSchemaValid()


        [TestMethod]
        [DynamicData(nameof(OfficialExamples))]
        public void TestOfficialExampleIsReadable(string relativePath)
        {
            string path = Path.Combine(SchemaValidator.RepositoryRoot, "documentation", "zugferd240en", "Examples", relativePath);

            InvoiceDescriptor desc = InvoiceDescriptor.Load(path);

            Assert.AreNotEqual(Profile.Unknown, desc.Profile, "Profile of the example was not recognized");
            Assert.IsFalse(string.IsNullOrWhiteSpace(desc.InvoiceNo), "BT-1 invoice number was not read");
            Assert.IsNotNull(desc.InvoiceDate, "BT-2 invoice date was not read");
        } // !TestOfficialExampleIsReadable()


        /// <summary>
        /// Factur-X 1.09.2 is backwards compatible, so the 1.08 examples have to stay valid against both schema versions.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(OfficialExamplesForEverySchemaVersion))]
        public void TestOfficialExampleRoundTripIsSchemaValid(string schemaVersion, string relativePath)
        {
            string path = Path.Combine(SchemaValidator.RepositoryRoot, "documentation", "zugferd240en", "Examples", relativePath);
            InvoiceDescriptor desc = InvoiceDescriptor.Load(path);
            Profile profile = desc.Profile;

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, profile);

            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetFacturXSchemaPath(schemaVersion, profile));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        } // !TestOfficialExampleRoundTripIsSchemaValid()


        /// <summary>
        /// Every official example combined with every profile that is not richer than the example's own profile.
        /// </summary>
        public static IEnumerable<object[]> ExamplesForLowerProfiles
        {
            get
            {
                Profile[] profiles = { Profile.Minimum, Profile.BasicWL, Profile.Basic, Profile.Comfort, Profile.Extended };
                foreach (object[] example in OfficialExamples)
                {
                    int sourceLevel = _GetProfileLevel(_GetExampleProfile((string)example[0]));
                    foreach (Profile profile in profiles.Where(p => _GetProfileLevel(p) <= sourceLevel))
                    {
                        yield return new object[] { example[0], profile };
                    }
                }
            }
        }


        /// <summary>
        /// The writer has to drop everything the target profile does not know, so that an invoice
        /// loaded from a richer profile can be written as a schema valid invoice of a lower profile.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(ExamplesForLowerProfiles))]
        public void TestOfficialExampleSavedInLowerProfileIsSchemaValid(string relativePath, Profile profile)
        {
            string path = Path.Combine(SchemaValidator.RepositoryRoot, "documentation", "zugferd240en", "Examples", relativePath);
            InvoiceDescriptor desc = InvoiceDescriptor.Load(path);

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, profile);

            List<string> errors = SchemaValidator.Validate(ms, SchemaValidator.GetFacturXSchemaPath(SchemaValidator.FacturX1092, profile));
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        } // !TestOfficialExampleSavedInLowerProfileIsSchemaValid()


        /// <summary>
        /// The guideline identifiers (BT-24) did not change from Factur-X 1.07 to 1.09.2, XRechnung 3.0.x uses one identifier for all patch versions.
        /// </summary>
        [TestMethod]
        [DataRow(Profile.Minimum, "urn:factur-x.eu:1p0:minimum")]
        [DataRow(Profile.BasicWL, "urn:factur-x.eu:1p0:basicwl")]
        [DataRow(Profile.Basic, "urn:cen.eu:en16931:2017#compliant#urn:factur-x.eu:1p0:basic")]
        [DataRow(Profile.Comfort, "urn:cen.eu:en16931:2017")]
        [DataRow(Profile.Extended, "urn:cen.eu:en16931:2017#conformant#urn:factur-x.eu:1p0:extended")]
        [DataRow(Profile.XRechnung, "urn:cen.eu:en16931:2017#compliant#urn:xeinkauf.de:kosit:xrechnung_3.0")]
        public void TestGuidelineIdentifier(Profile profile, string expectedGuidelineId)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();

            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, profile);

            XmlDocument doc = new XmlDocument();
            doc.Load(ms);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100");
            nsmgr.AddNamespace("ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100");
            Assert.AreEqual(expectedGuidelineId, doc.SelectSingleNode("/rsm:CrossIndustryInvoice/rsm:ExchangedDocumentContext/ram:GuidelineSpecifiedDocumentContextParameter/ram:ID", nsmgr)?.InnerText);
        } // !TestGuidelineIdentifier()


        [TestMethod]
        [DataRow("urn:cen.eu:en16931:2017#compliant#urn:xeinkauf.de:kosit:xrechnung_3.0#conformant#urn:xeinkauf.de:kosit:extension:xrechnung_3.0", Profile.XRechnung)]
        [DataRow("urn:zugferd.de:2p0:basicwl", Profile.BasicWL)]
        public void TestReadsAlternativeGuidelineIdentifiers(string guidelineId, Profile expectedProfile)
        {
            InvoiceDescriptor desc = _InvoiceProvider.CreateInvoice();
            using MemoryStream ms = new MemoryStream();
            desc.Save(ms, ZUGFeRDVersion.Version23, expectedProfile == Profile.BasicWL ? Profile.BasicWL : Profile.XRechnung);

            string xml = Encoding.UTF8.GetString(ms.ToArray());
            xml = Regex.Replace(xml, "(<ram:GuidelineSpecifiedDocumentContextParameter>\\s*<ram:ID>)[^<]+(</ram:ID>)", "${1}" + guidelineId + "${2}");
            StringAssert.Contains(xml, guidelineId);

            InvoiceDescriptor loaded = InvoiceDescriptor.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
            Assert.AreEqual(expectedProfile, loaded.Profile);
        } // !TestReadsAlternativeGuidelineIdentifiers()


        private static Profile _GetExampleProfile(string relativePath)
        {
            return InvoiceDescriptor.Load(Path.Combine(SchemaValidator.RepositoryRoot, "documentation", "zugferd240en", "Examples", relativePath)).Profile;
        } // !_GetExampleProfile()


        private static int _GetProfileLevel(Profile profile)
        {
            switch (profile)
            {
                case Profile.Minimum: return 0;
                case Profile.BasicWL: return 1;
                case Profile.Basic: return 2;
                case Profile.Comfort:
                case Profile.XRechnung:
                case Profile.XRechnung1: return 3;
                case Profile.Extended: return 4;
                default: throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }
        } // !_GetProfileLevel()
    }
}
