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
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
using Envisia.InvoiceXml.Validation.Schematron;
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// A resource of a scenario (XML schema, Schematron rules or report), as declared in scenarios.xml.
    /// </summary>
    public sealed class ScenarioResource
    {
        internal ScenarioResource(string name, string location, string uri)
        {
            Name = name;
            Location = location;
            Uri = uri;
        }


        /// <summary>The name of the resource.</summary>
        public string Name { get; }

        /// <summary>The location as written in the configuration (relative to the repository).</summary>
        public string Location { get; }

        /// <summary>The resolved location (a file URI or an embedded resource).</summary>
        internal string Uri { get; }
    }


    /// <summary>
    /// Overrides the level of messages with the given codes (s:customLevel).
    /// </summary>
    public sealed class CustomLevel
    {
        internal CustomLevel(ValidationLevel level, IReadOnlyList<string> codes)
        {
            Level = level;
            Codes = codes;
        }


        /// <summary>The level messages with one of the codes get for the assessment.</summary>
        public ValidationLevel Level { get; }

        /// <summary>The message codes (e.g. BR-CL-23).</summary>
        public IReadOnlyList<string> Codes { get; }
    }


    /// <summary>
    /// A validation scenario: which documents it applies to (match), the XML schema and the Schematron rules they
    /// are validated with and how the result is assessed. The format is the scenario configuration of the KoSIT
    /// validator (namespace http://www.xoev.de/de/validator/framework/1/scenarios).
    /// </summary>
    public sealed class ValidationScenario
    {
        private readonly Lazy<XmlSchemaSet> _Schema;
        private readonly Lazy<SchematronSchema>[] _Schematrons;
        private readonly XPathExpression _MatchExpression;
        private readonly XPathExpression _AcceptMatchExpression;


        internal ValidationScenario(XElement element, string name, string description, IReadOnlyDictionary<string, string> namespaces, string match,
                                    IReadOnlyList<ScenarioResource> xmlSchemas, IReadOnlyList<ScenarioResource> schematrons, ScenarioResource report,
                                    IReadOnlyList<CustomLevel> customLevels, string acceptMatch)
        {
            Element = element;
            Name = name;
            Description = description;
            Namespaces = namespaces;
            Match = match;
            XmlSchemas = xmlSchemas;
            SchematronRules = schematrons;
            Report = report;
            CustomLevels = customLevels;
            AcceptMatch = acceptMatch;

            StaticContext context = new StaticContext(namespaces.ToDictionary(n => n.Key, n => n.Value), null, null);
            try
            {
                _MatchExpression = XPathExpression.Compile(match, context);
                _AcceptMatchExpression = acceptMatch != null ? XPathExpression.Compile(acceptMatch, context) : null;
            }
            catch (XPathException e)
            {
                throw new InvalidOperationException("Invalid XPath expression in scenario '" + name + "': " + e.Message, e);
            }
            _Schema = new Lazy<XmlSchemaSet>(() => XmlSchemaLoader.Load(xmlSchemas), LazyThreadSafetyMode.ExecutionAndPublication);
            _Schematrons = schematrons.Select(s => new Lazy<SchematronSchema>(() => SchematronSchema.LoadResource(s.Uri, new SchematronOptions { LocationFormat = SchematronSchema.DefaultLocationFormat(s.Uri) }), LazyThreadSafetyMode.ExecutionAndPublication)).ToArray();
        }


        /// <summary>The s:scenario element of the configuration (copied into the report).</summary>
        internal XElement Element { get; }

        /// <summary>The name of the scenario, e.g. "EN16931 XRechnung (UBL Invoice)".</summary>
        public string Name { get; }

        /// <summary>The description of the scenario.</summary>
        public string Description { get; }

        /// <summary>The namespace prefixes available in <see cref="Match"/> and <see cref="AcceptMatch"/>.</summary>
        public IReadOnlyDictionary<string, string> Namespaces { get; }

        /// <summary>The XPath expression that identifies the documents of this scenario.</summary>
        public string Match { get; }

        /// <summary>The XML schemas the document is validated with.</summary>
        public IReadOnlyList<ScenarioResource> XmlSchemas { get; }

        /// <summary>The Schematron files the document is validated with (in this order).</summary>
        public IReadOnlyList<ScenarioResource> SchematronRules { get; }

        /// <summary>The report resource of the scenario (informational, the report is created by this library).</summary>
        public ScenarioResource Report { get; }

        /// <summary>The levels that override the levels of certain messages.</summary>
        public IReadOnlyList<CustomLevel> CustomLevels { get; }

        /// <summary>The XPath expression evaluated on the report to decide about acceptance, null if not configured.</summary>
        public string AcceptMatch { get; }


        internal bool Matches(XdmDocument document)
        {
            try
            {
                return _MatchExpression.EvaluateBoolean(new EvalContext(new EvaluationEnvironment()), document.Root);
            }
            catch (XPathException)
            {
                // as in the KoSIT validator: a match expression that fails does not match
                return false;
            }
        }


        internal bool? EvaluateAcceptMatch(XdmDocument report)
        {
            if (_AcceptMatchExpression == null)
            {
                return null;
            }
            return _AcceptMatchExpression.EvaluateBoolean(new EvalContext(new EvaluationEnvironment()), report.Root);
        }


        internal XmlSchemaSet GetXmlSchema()
        {
            return XmlSchemas.Count == 0 ? null : _Schema.Value;
        }


        internal SchematronSchema GetSchematron(int index)
        {
            return _Schematrons[index].Value;
        }


        /// <summary>
        /// Compiles the XML schemas and Schematron rules of the scenario (otherwise done on first use).
        /// </summary>
        public void Compile()
        {
            GetXmlSchema();
            for (int i = 0; i < _Schematrons.Length; i++)
            {
                GetSchematron(i);
            }
        }


        internal ValidationLevel GetCustomLevel(string code, ValidationLevel level)
        {
            if (code == "PROCESSING_ERROR")
            {
                return level;
            }
            foreach (CustomLevel customLevel in CustomLevels)
            {
                if (customLevel.Codes.Contains(code))
                {
                    return customLevel.Level;
                }
            }
            return level;
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
