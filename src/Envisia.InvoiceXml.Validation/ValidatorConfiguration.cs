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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// A set of validation scenarios in the configuration format of the KoSIT validator (scenarios.xml).
    ///
    /// The built-in configurations use the official validation artefacts embedded in this package:
    /// <see cref="XRechnung"/>, <see cref="FacturX"/> and <see cref="EN16931"/>. A custom configuration can be
    /// loaded with <see cref="Load(string, string)"/>; its Schematron resources must be Schematron files (.sch),
    /// compiled XSLT is not supported.
    /// </summary>
    public sealed class ValidatorConfiguration
    {
        internal static readonly XNamespace ScenarioNamespace = "http://www.xoev.de/de/validator/framework/1/scenarios";

        private static readonly Lazy<ValidatorConfiguration> _XRechnung = new Lazy<ValidatorConfiguration>(() => _LoadEmbedded("scenarios/xrechnung.xml"), LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<ValidatorConfiguration> _FacturX = new Lazy<ValidatorConfiguration>(() => _LoadEmbedded("scenarios/factur-x.xml"), LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<ValidatorConfiguration> _EN16931 = new Lazy<ValidatorConfiguration>(() => _LoadEmbedded("scenarios/en16931.xml"), LazyThreadSafetyMode.ExecutionAndPublication);


        private ValidatorConfiguration(string name, string author, string date, string description, IReadOnlyList<ValidationScenario> scenarios, ScenarioResource noScenarioReport)
        {
            Name = name;
            Author = author;
            Date = date;
            Description = description;
            Scenarios = scenarios;
            NoScenarioReport = noScenarioReport;
        }


        /// <summary>
        /// XRechnung 3.0 (standard, extension and CVD; UBL Invoice, UBL CreditNote and CII): the scenarios of the
        /// official KoSIT validator configuration for XRechnung 3.0.2 (2026-08-31) with the CEN EN 16931 rules
        /// 1.3.16 and the XRechnung Schematron 2.6.0.
        /// </summary>
        public static ValidatorConfiguration XRechnung => _XRechnung.Value;


        /// <summary>
        /// ZUGFeRD 2.x / Factur-X 1.x: the profiles MINIMUM, BASIC WL, BASIC, EN 16931 and EXTENDED, validated
        /// with the XML schemas and Schematron rules of Factur-X 1.09.2 (ZUGFeRD 2.5.2).
        /// </summary>
        public static ValidatorConfiguration FacturX => _FacturX.Value;


        /// <summary>
        /// Any other EN 16931 invoice (CustomizationID / guideline starting with urn:cen.eu:en16931:2017, e.g. Peppol
        /// BIS Billing 3.0) in UBL or CII syntax, validated with the XML schema and the CEN EN 16931 rules 1.3.16 only
        /// (the rules of the CIUS / extension are not checked).
        /// </summary>
        public static ValidatorConfiguration EN16931 => _EN16931.Value;


        /// <summary>
        /// The configurations used by default, in this order: <see cref="FacturX"/>, <see cref="XRechnung"/>,
        /// <see cref="EN16931"/>. ZUGFeRD / Factur-X invoices are validated with the Factur-X rules (also those
        /// of the EN 16931 profile, which the KoSIT XRechnung configuration validates with the CEN rules only);
        /// use <see cref="XRechnung"/> alone to get exactly the results of the KoSIT XRechnung configuration.
        /// </summary>
        public static IReadOnlyList<ValidatorConfiguration> Default => new[] { FacturX, XRechnung, EN16931 };


        /// <summary>The name of the configuration.</summary>
        public string Name { get; }

        /// <summary>The author of the configuration.</summary>
        public string Author { get; }

        /// <summary>The date of the configuration.</summary>
        public string Date { get; }

        /// <summary>The description of the configuration.</summary>
        public string Description { get; }

        /// <summary>The scenarios.</summary>
        public IReadOnlyList<ValidationScenario> Scenarios { get; }

        /// <summary>The report resource used when no scenario matches (informational).</summary>
        public ScenarioResource NoScenarioReport { get; }


        /// <summary>
        /// Loads a configuration from a scenarios.xml file.
        /// </summary>
        /// <param name="scenariosPath">Path of the scenarios.xml</param>
        /// <param name="repositoryPath">Directory the resource locations are relative to; defaults to the directory of the scenarios.xml</param>
        public static ValidatorConfiguration Load(string scenariosPath, string repositoryPath = null)
        {
            string repository = repositoryPath ?? Path.GetDirectoryName(Path.GetFullPath(scenariosPath));
            using (FileStream stream = File.OpenRead(scenariosPath))
            {
                return Load(stream, repository);
            }
        }


        /// <summary>
        /// Loads a configuration from a stream with the content of a scenarios.xml file.
        /// </summary>
        /// <param name="scenarios">The scenarios.xml content</param>
        /// <param name="repositoryPath">Directory the resource locations are relative to</param>
        public static ValidatorConfiguration Load(Stream scenarios, string repositoryPath)
        {
            string baseUri = ResourceLoader.FileUri(Path.Combine(repositoryPath, "scenarios.xml"));
            return _Load(scenarios, baseUri);
        }


        private static ValidatorConfiguration _LoadEmbedded(string path)
        {
            using (Stream stream = ResourceLoader.Open(ResourceLoader.EmbeddedUri(path)))
            {
                return _Load(stream, ResourceLoader.EmbeddedUri("scenarios.xml"));
            }
        }


        private static ValidatorConfiguration _Load(Stream stream, string baseUri)
        {
            XDocument document;
            using (XmlReader reader = XmlReader.Create(stream, XdmDocumentBuilder.CreateSecureSettings()))
            {
                document = XDocument.Load(reader);
            }
            XNamespace s = ScenarioNamespace;
            XElement root = document.Root;
            if (root == null || root.Name != s + "scenarios")
            {
                throw new InvalidOperationException("Not a validator scenario configuration (expected the element {" + s.NamespaceName + "}scenarios)");
            }

            List<ValidationScenario> scenarios = new List<ValidationScenario>();
            foreach (XElement scenario in root.Elements(s + "scenario"))
            {
                Dictionary<string, string> namespaces = scenario.Elements(s + "namespace").ToDictionary(n => (string)n.Attribute("prefix"), n => n.Value.Trim());
                XElement createReport = scenario.Element(s + "createReport");
                List<CustomLevel> customLevels = new List<CustomLevel>();
                if (createReport != null)
                {
                    foreach (XElement customLevel in createReport.Elements(s + "customLevel"))
                    {
                        customLevels.Add(new CustomLevel(_ParseLevel((string)customLevel.Attribute("level")), customLevel.Value.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)));
                    }
                }
                scenarios.Add(new ValidationScenario(
                    scenario,
                    ((string)scenario.Element(s + "name"))?.Trim(),
                    _Description(scenario.Element(s + "description")),
                    namespaces,
                    ((string)scenario.Element(s + "match"))?.Trim(),
                    _Resources(scenario.Element(s + "validateWithXmlSchema"), baseUri, false),
                    scenario.Elements(s + "validateWithSchematron").SelectMany(v => _Resources(v, baseUri, true)).ToList(),
                    _Resources(createReport, baseUri, false).FirstOrDefault(),
                    customLevels,
                    ((string)scenario.Element(s + "acceptMatch"))?.Trim()));
            }
            return new ValidatorConfiguration(
                ((string)root.Element(s + "name"))?.Trim(),
                ((string)root.Element(s + "author"))?.Trim(),
                ((string)root.Element(s + "date"))?.Trim(),
                _Description(root.Element(s + "description")),
                scenarios,
                _Resources(root.Element(s + "noScenarioReport"), baseUri, false).FirstOrDefault());
        }


        private static string _Description(XElement description)
        {
            if (description == null)
            {
                return null;
            }
            return String.Join(Environment.NewLine, description.Elements().Select(p => Casting.Collapse(p.Value)));
        }


        private static List<ScenarioResource> _Resources(XElement parent, string baseUri, bool schematron)
        {
            List<ScenarioResource> result = new List<ScenarioResource>();
            if (parent == null)
            {
                return result;
            }
            XNamespace s = ScenarioNamespace;
            foreach (XElement resource in parent.Elements(s + "resource"))
            {
                string location = ((string)resource.Element(s + "location"))?.Trim();
                string uri = UriHelper.Resolve(baseUri, location);
                if (schematron && (uri.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase) || uri.EndsWith(".xslt", StringComparison.OrdinalIgnoreCase)))
                {
                    // compiled Schematron cannot be executed: use the Schematron source next to it if there is one
                    string source = uri.Substring(0, uri.LastIndexOf('.')) + ".sch";
                    if (!ResourceLoader.Exists(source))
                    {
                        throw new NotSupportedException("The Schematron resource " + location + " is compiled XSLT, which is not supported. Configure the Schematron source (.sch) instead.");
                    }
                    uri = source;
                }
                result.Add(new ScenarioResource(((string)resource.Element(s + "name"))?.Trim(), location, uri));
            }
            return result;
        }


        private static ValidationLevel _ParseLevel(string level)
        {
            switch (level)
            {
                case "information":
                    return ValidationLevel.Information;
                case "warning":
                    return ValidationLevel.Warning;
                case "error":
                    return ValidationLevel.Error;
                default:
                    throw new InvalidOperationException("Invalid custom level '" + level + "'");
            }
        }


        /// <summary>
        /// Compiles all XML schemas and Schematron rules (otherwise done on first use of a scenario).
        /// </summary>
        public void Compile()
        {
            foreach (ValidationScenario scenario in Scenarios)
            {
                scenario.Compile();
            }
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
