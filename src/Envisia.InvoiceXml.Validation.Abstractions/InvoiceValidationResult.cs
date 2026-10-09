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
using System.Xml;
using System.Xml.Linq;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// A message of a validation step (rep:message of the KoSIT report).
    /// </summary>
    public sealed class InvoiceValidationMessage
    {
        /// <summary>The id of the message in the report, e.g. "val-sch.1.3".</summary>
        public string Id { get; init; }

        /// <summary>The rule id (e.g. "BR-DE-15") or "generic-error" for XML schema messages.</summary>
        public string Code { get; init; }

        /// <summary>The level of the message.</summary>
        public ValidationLevel Level { get; init; }

        /// <summary>The message text.</summary>
        public string Text { get; init; }

        /// <summary>The location of the node the message is about (XPath), if known.</summary>
        public string Location { get; init; }

        /// <summary>The line in the document, if known.</summary>
        public int? LineNumber { get; init; }

        /// <summary>The column in the document, if known.</summary>
        public int? ColumnNumber { get; init; }

        /// <summary>The validation step: "val-xml" (parsing), "val-xsd" (XML schema), "val-sch.N" (Schematron).</summary>
        public string StepId { get; init; }


        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + Level + "] " + Code + ": " + Text + (Location != null ? " (" + Location + ")" : "");
        }
    }


    /// <summary>
    /// The result of a validation: the recommendation and the KoSIT report (VARL,
    /// http://www.xoev.de/de/validator/varl/1) with the messages of all validation steps.
    /// </summary>
    public sealed class InvoiceValidationResult
    {
        private static readonly XNamespace _Rep = "http://www.xoev.de/de/validator/varl/1";
        private static readonly XNamespace _Scenarios = "http://www.xoev.de/de/validator/framework/1/scenarios";
        private static readonly XNamespace _Html = "http://www.w3.org/1999/xhtml";


        private InvoiceValidationResult()
        {
        }


        /// <summary>The recommendation of the validator.</summary>
        public AcceptRecommendation Recommendation { get; private set; }

        /// <summary>True if the recommendation is to accept the document.</summary>
        public bool IsAcceptable => Recommendation == AcceptRecommendation.Accept;

        /// <summary>True if no validation step reported an error or a warning (the valid attribute of the report).</summary>
        public bool IsValid { get; private set; }

        /// <summary>True if the document is well-formed XML.</summary>
        public bool IsWellFormed { get; private set; }

        /// <summary>True if the document is valid against the XML schema of the matched scenario.</summary>
        public bool IsSchemaValid { get; private set; }

        /// <summary>The name of the matched scenario, e.g. "EN16931 XRechnung (UBL Invoice)"; null if no scenario matched.</summary>
        public string Scenario { get; private set; }

        /// <summary>The engine that created the report.</summary>
        public string Engine { get; private set; }

        /// <summary>The messages of all validation steps.</summary>
        public IReadOnlyList<InvoiceValidationMessage> Messages { get; private set; }

        /// <summary>The messages with the level error.</summary>
        public IEnumerable<InvoiceValidationMessage> Errors => Messages.Where(m => m.Level == ValidationLevel.Error);

        /// <summary>The messages with the level warning.</summary>
        public IEnumerable<InvoiceValidationMessage> Warnings => Messages.Where(m => m.Level == ValidationLevel.Warning);

        /// <summary>The report in the format of the KoSIT validator (VARL).</summary>
        public string ReportXml { get; private set; }


        /// <summary>
        /// The HTML report contained in the assessment of the report (what the KoSIT validator writes with --html),
        /// null if the report has none.
        /// </summary>
        public string GetReportHtml()
        {
            XElement html = XDocument.Parse(ReportXml).Root?.Element(_Rep + "assessment")?.Elements().FirstOrDefault()
                                     ?.Element(_Rep + "explanation")?.Element(_Html + "html");
            return html?.ToString();
        }


        /// <summary>
        /// Creates the result from a KoSIT report (VARL).
        /// </summary>
        /// <param name="reportXml">The report</param>
        /// <param name="recommendation">The recommendation of the validator</param>
        public static InvoiceValidationResult FromReport(string reportXml, AcceptRecommendation recommendation)
        {
            if (reportXml == null)
            {
                throw new ArgumentNullException(nameof(reportXml));
            }
            XElement root = XDocument.Parse(reportXml).Root;
            if (root == null || root.Name != _Rep + "report")
            {
                throw new ArgumentException("Not a KoSIT validation report (rep:report)", nameof(reportXml));
            }
            List<InvoiceValidationMessage> messages = new List<InvoiceValidationMessage>();
            bool wellFormed = false, schemaValid = false;
            foreach (XElement step in root.Descendants(_Rep + "validationStepResult"))
            {
                string stepId = (string)step.Attribute("id");
                bool valid = (string)step.Attribute("valid") == "true";
                if (stepId == "val-xml")
                {
                    wellFormed = valid;
                }
                else if (stepId == "val-xsd")
                {
                    schemaValid = valid;
                }
                foreach (XElement message in step.Elements(_Rep + "message"))
                {
                    messages.Add(new InvoiceValidationMessage
                    {
                        Id = (string)message.Attribute("id"),
                        Code = (string)message.Attribute("code"),
                        Level = _Level((string)message.Attribute("level")),
                        Text = message.Value.Trim(),
                        Location = (string)message.Attribute("xpathLocation"),
                        LineNumber = (int?)message.Attribute("lineNumber"),
                        ColumnNumber = (int?)message.Attribute("columnNumber"),
                        StepId = stepId
                    });
                }
            }
            XElement scenario = root.Element(_Rep + "scenarioMatched")?.Element(_Scenarios + "scenario")?.Element(_Scenarios + "name");
            return new InvoiceValidationResult
            {
                Recommendation = recommendation,
                IsValid = (string)root.Attribute("valid") == "true",
                IsWellFormed = wellFormed,
                IsSchemaValid = schemaValid,
                Scenario = scenario?.Value.Trim(),
                Engine = root.Element(_Rep + "engine")?.Element(_Rep + "name")?.Value.Trim(),
                Messages = messages,
                ReportXml = reportXml
            };
        }


        private static ValidationLevel _Level(string level)
        {
            switch (level)
            {
                case "information":
                    return ValidationLevel.Information;
                case "warning":
                    return ValidationLevel.Warning;
                default:
                    return ValidationLevel.Error;
            }
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return (Scenario ?? "no scenario") + ": " + Recommendation + ", " + Errors.Count() + " errors, " + Warnings.Count() + " warnings";
        }
    }
}
