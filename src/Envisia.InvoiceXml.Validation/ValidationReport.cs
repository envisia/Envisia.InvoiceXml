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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Envisia.InvoiceXml.Validation.Schematron;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// A message of a validation step (rep:message of the KoSIT report).
    /// </summary>
    public sealed class ValidationMessage
    {
        internal ValidationMessage()
        {
        }


        /// <summary>The id of the message, e.g. val-sch.1.3 (third message of the first Schematron step).</summary>
        public string Id { get; internal set; }

        /// <summary>The code of the message: the Schematron assertion id (BR-CO-10), generic-error or PROCESSING_ERROR.</summary>
        public string Code { get; internal set; }

        /// <summary>The level of the message as reported by the validation step.</summary>
        public ValidationLevel Level { get; internal set; }

        /// <summary>The level used for the assessment: <see cref="Level"/> unless a custom level of the scenario applies.</summary>
        public ValidationLevel CustomLevel { get; internal set; }

        /// <summary>The message text.</summary>
        public string Text { get; internal set; }

        /// <summary>The location of the affected node as XPath (Schematron messages).</summary>
        public string XPathLocation { get; internal set; }

        /// <summary>The line in the validated document, if known.</summary>
        public int? LineNumber { get; internal set; }

        /// <summary>The column in the validated document, if known.</summary>
        public int? ColumnNumber { get; internal set; }

        /// <summary>The id of the validation step the message belongs to (val-xml, val-xsd, val-sch.1, ...).</summary>
        public string StepId { get; internal set; }

        /// <summary>The Schematron message, for Schematron validation steps.</summary>
        public SchematronMessage SchematronMessage { get; internal set; }


        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + CustomLevel.ToString().ToLowerInvariant() + "] " + Code + ": " + NormalizedText + (XPathLocation != null ? " (" + XPathLocation + ")" : LineNumber.HasValue ? " (line " + LineNumber + ")" : "");
        }


        /// <summary>The message text with normalized whitespace.</summary>
        public string NormalizedText => XPath.Casting.Collapse(Text ?? "");
    }


    /// <summary>
    /// The result of one validation step: well-formedness (val-xml), XML schema (val-xsd) or one Schematron file
    /// (val-sch.1, val-sch.2, ...).
    /// </summary>
    public sealed class ValidationStepResult
    {
        internal ValidationStepResult(string id, ScenarioResource resource)
        {
            Id = id;
            Resource = resource;
        }


        /// <summary>The id of the step.</summary>
        public string Id { get; }

        /// <summary>The resource (schema / Schematron file) of the step, null for the well-formedness check.</summary>
        public ScenarioResource Resource { get; }

        /// <summary>False if the step reported an error or a warning (or could not be completed).</summary>
        public bool IsValid { get; internal set; } = true;

        /// <summary>The messages of the step.</summary>
        public IReadOnlyList<ValidationMessage> Messages => _Messages;

        internal List<ValidationMessage> _Messages = new List<ValidationMessage>();

        /// <summary>The Schematron result, for Schematron validation steps that completed.</summary>
        public SchematronResult SchematronResult { get; internal set; }


        /// <inheritdoc/>
        public override string ToString()
        {
            return Id + (IsValid ? " valid" : " invalid") + " (" + Messages.Count + " messages)";
        }
    }


    /// <summary>
    /// The validation report: the result of all validation steps and the recommendation whether to accept the
    /// document. <see cref="ToXml"/> creates the report in the format of the KoSIT validator (VARL 1.0.0),
    /// <see cref="ToHtml"/> the human readable report.
    /// </summary>
    public sealed class ValidationReport
    {
        internal ValidationReport()
        {
        }


        /// <summary>Name and version of the validator.</summary>
        public string EngineName { get; internal set; }

        /// <summary>When the document was validated.</summary>
        public DateTimeOffset Timestamp { get; internal set; }

        /// <summary>The reference (usually the file name) of the validated document.</summary>
        public string DocumentReference { get; internal set; }

        /// <summary>The SHA-256 hash (Base64) of the validated document.</summary>
        public string DocumentHash { get; internal set; }

        /// <summary>The matching scenario, null if no scenario matched.</summary>
        public ValidationScenario Scenario { get; internal set; }

        /// <summary>The configuration of the matching scenario.</summary>
        public ValidatorConfiguration Configuration { get; internal set; }

        /// <summary>True if the document is well-formed XML.</summary>
        public bool IsWellFormed { get; internal set; }

        /// <summary>True if the document is valid against the XML schema of the scenario.</summary>
        public bool IsSchemaValid { get; internal set; }

        /// <summary>
        /// True if a scenario matched and no validation step reported an error or a warning (the valid attribute of the KoSIT report).
        /// </summary>
        public bool IsValid { get; internal set; }

        /// <summary>The validation steps (val-xsd, val-sch.1, ..., val-xml).</summary>
        public IReadOnlyList<ValidationStepResult> Steps { get; internal set; } = new ValidationStepResult[0];

        /// <summary>Errors that prevented the validation from completing.</summary>
        public IReadOnlyList<string> ProcessingErrors { get; internal set; } = new string[0];

        /// <summary>The recommendation (rep:assessment).</summary>
        public AcceptRecommendation Recommendation { get; internal set; }

        /// <summary>True if the document can be accepted (KoSIT: acceptable).</summary>
        public bool IsAcceptable => Recommendation == AcceptRecommendation.Accept;

        /// <summary>Data from the document shown in the report (seller, invoice number, issue date).</summary>
        public IReadOnlyList<KeyValuePair<string, string>> DocumentData { get; internal set; } = new KeyValuePair<string, string>[0];

        /// <summary>All messages of all steps.</summary>
        public IEnumerable<ValidationMessage> Messages => Steps.SelectMany(s => s.Messages);

        /// <summary>The messages that lead to the rejection of the document.</summary>
        public IEnumerable<ValidationMessage> Errors => Messages.Where(m => m.CustomLevel == ValidationLevel.Error);

        /// <summary>The warnings (after applying the custom levels).</summary>
        public IEnumerable<ValidationMessage> Warnings => Messages.Where(m => m.CustomLevel == ValidationLevel.Warning);

        internal XPath.XdmDocument InputDocument;

        /// <summary>The assessment computed from the messages and custom levels (rep:assessment of the report).</summary>
        internal AcceptRecommendation Assessment;
        internal ReportLanguage Language;
        internal bool IncludeDocumentContent;


        /// <summary>
        /// The result in the implementation independent model of <see cref="IInvoiceValidator"/>.
        /// </summary>
        public InvoiceValidationResult ToInvoiceValidationResult()
        {
            return InvoiceValidationResult.FromReport(ToXml().ToString(SaveOptions.DisableFormatting), Recommendation);
        }


        /// <summary>
        /// The report in the XML format of the KoSIT validator (namespace http://www.xoev.de/de/validator/varl/1),
        /// including the HTML explanation.
        /// </summary>
        public XDocument ToXml()
        {
            return ReportWriter.CreateXml(this);
        }


        /// <summary>
        /// The human readable report (HTML), like the report of the KoSIT validator.
        /// </summary>
        public string ToHtml()
        {
            XElement html = HtmlReportWriter.Create(this);
            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>\n");
            using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
            {
                html.WriteTo(writer);
            }
            return sb.ToString();
        }


        /// <summary>
        /// Saves the XML report.
        /// </summary>
        public void SaveXml(string path)
        {
            using (FileStream stream = File.Create(path))
            {
                SaveXml(stream);
            }
        }


        /// <summary>
        /// Writes the XML report.
        /// </summary>
        public void SaveXml(Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) }))
            {
                ToXml().Save(writer);
            }
        }


        /// <summary>
        /// Saves the HTML report.
        /// </summary>
        public void SaveHtml(string path)
        {
            File.WriteAllText(path, ToHtml(), new UTF8Encoding(false));
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Recommendation == AcceptRecommendation.Accept ? "ACCEPT" : Recommendation == AcceptRecommendation.Reject ? "REJECT" : "UNDEFINED");
            sb.Append(" - ").Append(Scenario?.Name ?? "no matching scenario");
            foreach (ValidationMessage message in Messages)
            {
                sb.Append(Environment.NewLine).Append("  ").Append(message);
            }
            foreach (string error in ProcessingErrors)
            {
                sb.Append(Environment.NewLine).Append("  processing error: ").Append(error);
            }
            return sb.ToString();
        }
    }
}
