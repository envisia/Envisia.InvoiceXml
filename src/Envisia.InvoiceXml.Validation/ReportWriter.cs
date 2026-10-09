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
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// The language of the human readable report.
    /// </summary>
    public enum ReportLanguage
    {
        /// <summary>German, the texts of the KoSIT validator.</summary>
        German,

        /// <summary>English.</summary>
        English
    }


    /// <summary>
    /// Writes the validation report in the XML format of the KoSIT validator (VARL 1.0.0).
    /// </summary>
    internal static class ReportWriter
    {
        public static readonly XNamespace Rep = "http://www.xoev.de/de/validator/varl/1";
        public static readonly XNamespace S = ValidatorConfiguration.ScenarioNamespace;
        public static readonly XNamespace Html = "http://www.w3.org/1999/xhtml";


        public static string LevelName(ValidationLevel level)
        {
            switch (level)
            {
                case ValidationLevel.Information:
                    return "information";
                case ValidationLevel.Warning:
                    return "warning";
                default:
                    return "error";
            }
        }


        public static XDocument CreateXml(ValidationReport report)
        {
            XElement root = CreateXmlWithoutAssessment(report);
            XElement recommendation = new XElement(Rep + (report.Assessment == AcceptRecommendation.Accept ? "accept" : "reject"),
                new XElement(Rep + "explanation", HtmlReportWriter.Create(report)));
            root.Add(new XElement(Rep + "assessment", recommendation));
            return new XDocument(root);
        }


        /// <summary>
        /// The report without rep:assessment, plus the assessment computed from the messages (used to evaluate acceptMatch).
        /// </summary>
        public static XElement CreateXmlWithoutAssessment(ValidationReport report)
        {
            XElement root = new XElement(Rep + "report",
                new XAttribute(XNamespace.Xmlns + "rep", Rep.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "s", S.NamespaceName),
                new XAttribute("varlVersion", "1.0.0"),
                new XAttribute("valid", report.IsValid ? "true" : "false"),
                new XElement(Rep + "engine", new XElement(Rep + "name", report.EngineName)),
                new XElement(Rep + "timestamp", report.Timestamp.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture)),
                new XElement(Rep + "documentIdentification",
                    new XElement(Rep + "documentHash",
                        new XElement(Rep + "hashAlgorithm", "SHA-256"),
                        new XElement(Rep + "hashValue", report.DocumentHash)),
                    new XElement(Rep + "documentReference", report.DocumentReference ?? "")));

            if (report.Scenario != null)
            {
                XElement matched = new XElement(Rep + "scenarioMatched", new XElement(report.Scenario.Element));
                if (report.DocumentData.Count > 0)
                {
                    matched.Add(new XElement(Rep + "documentData", report.DocumentData.Select(d => new XElement(d.Key, d.Value))));
                }
                foreach (ValidationStepResult step in report.Steps)
                {
                    matched.Add(_Step(step));
                }
                root.Add(matched);
            }
            else
            {
                root.Add(new XElement(Rep + "noScenarioMatched", report.Steps.Select(_Step)));
            }
            return root;
        }


        /// <summary>
        /// The assessment element computed from the messages and the custom levels (default-report.xsl of the
        /// KoSIT XRechnung configuration).
        /// </summary>
        public static AcceptRecommendation ComputeAssessment(ValidationReport report)
        {
            if (report.ProcessingErrors.Count > 0 || report.Scenario == null || _IsFallback(report.Scenario))
            {
                return AcceptRecommendation.Reject;
            }
            if (report.Messages.Any(m => m.CustomLevel == ValidationLevel.Error))
            {
                return AcceptRecommendation.Reject;
            }
            return AcceptRecommendation.Accept;
        }


        internal static bool _IsFallback(ValidationScenario scenario)
        {
            return scenario.Name != null && Casting.Collapse(scenario.Name).ToLowerInvariant().Contains("fallback");
        }


        private static XElement _Step(ValidationStepResult step)
        {
            XElement element = new XElement(Rep + "validationStepResult", new XAttribute("id", step.Id), new XAttribute("valid", step.IsValid ? "true" : "false"));
            if (step.Resource != null)
            {
                element.Add(new XElement(S + "resource", new XElement(S + "name", step.Resource.Name), new XElement(S + "location", step.Resource.Location)));
            }
            foreach (ValidationMessage message in step.Messages)
            {
                XElement m = new XElement(Rep + "message", new XAttribute("id", message.Id), new XAttribute("level", LevelName(message.Level)));
                if (message.LineNumber.HasValue)
                {
                    m.Add(new XAttribute("lineNumber", message.LineNumber.Value));
                }
                if (message.ColumnNumber.HasValue)
                {
                    m.Add(new XAttribute("columnNumber", message.ColumnNumber.Value));
                }
                if (message.XPathLocation != null)
                {
                    m.Add(new XAttribute("xpathLocation", message.XPathLocation));
                }
                m.Add(new XAttribute("code", message.Code));
                m.Add(new XText(message.Text ?? ""));
                element.Add(m);
            }
            return element;
        }
    }


    /// <summary>
    /// Creates the human readable report (XHTML) of default-report.xsl / xrechnung-report.xsl of the KoSIT validator.
    /// </summary>
    internal static class HtmlReportWriter
    {
        private static readonly XNamespace _H = ReportWriter.Html;

        private const string _Css = @"
        body {
          font-family: Calibri;
          width: 230mm;
        }
        .metadata dt {
          float: left;
          width: 230px;
          clear: left;
        }
        .metadata dd {
          margin-left: 250px;
        }
        table {
          border-collapse: collapse;
          width: 100%;
        }
        table.tbl-errors {
          font-size: smaller;
        }
        table.document {
          font-size: smaller;
        }
        table.document td {
          vertical-align: top;
        }
        .tbl-errors td {
          border: 1px solid lightgray;
          padding: 2px;
          vertical-align: top;
        }
        thead {
          font-weight: bold;
          background-color: #f0f0f0;
          padding-top: 6pt;
          padding-bottom: 2pt;
        }
        .tbl-meta td {
          padding-right: 1em;
        }
        td.pos {
          padding-left: 3pt;
          width: 5%;
          color: gray
        }
        td.element {
          width: 95%;
          word-wrap: break-word;
        }
        td.element:before {
          content: attr(title);
          color: gray;
        }
        div.attribute {
          display: inline;
          font-style: italic;
          color: gray;
        }
        div.attribute:before {
          content: attr(title) '=';
        }
        div.val {
          display: inline;
          font-weight: bold;
        }
        td.level1 {
          padding-left: 2mm;
        }
        td.level2 {
          padding-left: 5mm;
        }
        td.level3 {
          padding-left: 10mm;
        }
        td.level4 {
          padding-left: 15mm;
        }
        td.level5 {
          padding-left: 20mm;
        }
        td.level6 {
          padding-left: 25mm;
        }
        tr {
          vertical-align: bottom;
          border-bottom: 1px solid #c0c0c0;
        }
        .error {
          color: red;
        }
        .warning {
        }
        p.important {
          font-weight: bold;
          text-align: left;
          background-color: #e0e0e0;
          padding: 3pt;
        }
        td.right {
          text-align: right
        }";


        private sealed class Texts
        {
            public string Title;
            public string DocumentInformation;
            public string Reference;
            public string Timestamp;
            public string DocumentType;
            public string Unknown;
            public string Seller;
            public string InvoiceNumber;
            public string IssueDate;
            public string ProcessingErrorLabel;
            public string ProcessingErrorText;
            public string ConformanceLabel;
            public string ConformanceProcessingError;
            public string ConformanceContains;
            public string ConformanceNoErrors;
            public string ConformanceErrorsFormat;
            public string NotConformant;
            public string ConformanceSuffix;
            public string ConformanceNoScenario;
            public string Overview;
            public string Step;
            public string ErrorsHeader;
            public string WarningsHeader;
            public string InformationHeader;
            public string Details;
            public string Position;
            public string Code;
            public string Level;
            public string Text;
            public string Path;
            public string Line;
            public string Column;
            public string AssessmentProcessingError;
            public string AssessmentNoScenario;
            public string AssessmentAccept;
            public string AssessmentAcceptTolerated;
            public string AssessmentReject;
            public string Content;
            public string Epilog;
        }


        private static readonly Texts _German = new Texts
        {
            Title = "Prüfbericht",
            DocumentInformation = "Angaben zum geprüften Dokument",
            Reference = "Referenz:",
            Timestamp = "Zeitpunkt der Prüfung:",
            DocumentType = "Erkannter Dokumenttyp:",
            Unknown = "unbekannt",
            Seller = "Erkannter Rechnungssteller:",
            InvoiceNumber = "Erkannte Rechnungsnummer:",
            IssueDate = "Erkanntes Rechnungsdatum:",
            ProcessingErrorLabel = "Verarbeitungsfehler: ",
            ProcessingErrorText = "Bei der Validierung sind Verarbeitungsfehler aufgetreten. Das Ergebnis ist unvollständig und nicht verwertbar.",
            ConformanceLabel = "Konformitätsprüfung: ",
            ConformanceProcessingError = "Die Konformitätsprüfung konnte aufgrund eines Verarbeitungsfehlers nicht vollständig durchgeführt werden. Eine Aussage zur Konformität ist daher nicht möglich.",
            ConformanceContains = "Das geprüfte Dokument enthält ",
            ConformanceNoErrors = "weder Fehler noch Warnungen. Es ist konform zu den formalen Vorgaben.",
            ConformanceErrorsFormat = "{0} Fehler / {1} Warnungen. Es ist ",
            NotConformant = "nicht konform",
            ConformanceSuffix = " zu den formalen Vorgaben.",
            ConformanceNoScenario = "Das geprüfte Dokument entspricht keinem zulässigen Dokumenttyp und ist damit ",
            Overview = "Übersicht der Validierungsergebnisse:",
            Step = "Prüfschritt",
            ErrorsHeader = "Fehler",
            WarningsHeader = "Warnungen",
            InformationHeader = "Informationen",
            Details = "Validierungsergebnisse im Detail:",
            Position = "Pos",
            Code = "Code",
            Level = "Adj. Grad",
            Text = "Text",
            Path = "Pfad: ",
            Line = " Zeile: ",
            Column = " Spalte: ",
            AssessmentProcessingError = "Bewertung: Die Validierung konnte nicht vollständig durchgeführt werden. Das Dokument sollte nicht automatisiert angenommen werden.",
            AssessmentNoScenario = "Bewertung: Es wird empfohlen das Dokument zurückzuweisen. Da kein Pruefszenario gegriffen hat.",
            AssessmentAccept = "Bewertung: Es wird empfohlen das Dokument anzunehmen und weiter zu verarbeiten.",
            AssessmentAcceptTolerated = "Bewertung: Es wird empfohlen das Dokument anzunehmen und zu verarbeiten, da die vorhandenen Fehler derzeit toleriert werden.",
            AssessmentReject = "Bewertung: Es wird empfohlen das Dokument zurückzuweisen.",
            Content = "Inhalt des Rechnungsdokuments:",
            Epilog = "Dieser Prüfbericht wurde erstellt mit "
        };


        private static readonly Texts _English = new Texts
        {
            Title = "Validation report",
            DocumentInformation = "Information about the validated document",
            Reference = "Reference:",
            Timestamp = "Time of validation:",
            DocumentType = "Detected document type:",
            Unknown = "unknown",
            Seller = "Detected seller:",
            InvoiceNumber = "Detected invoice number:",
            IssueDate = "Detected invoice date:",
            ProcessingErrorLabel = "Processing errors: ",
            ProcessingErrorText = "Processing errors occurred during the validation. The result is incomplete and cannot be used.",
            ConformanceLabel = "Conformance check: ",
            ConformanceProcessingError = "The conformance check could not be completed because of a processing error. No statement about the conformance is possible.",
            ConformanceContains = "The validated document contains ",
            ConformanceNoErrors = "neither errors nor warnings. It conforms to the formal requirements.",
            ConformanceErrorsFormat = "{0} errors / {1} warnings. It does ",
            NotConformant = "not conform",
            ConformanceSuffix = " to the formal requirements.",
            ConformanceNoScenario = "The validated document does not match any accepted document type and does therefore ",
            Overview = "Overview of the validation results:",
            Step = "Validation step",
            ErrorsHeader = "Errors",
            WarningsHeader = "Warnings",
            InformationHeader = "Information",
            Details = "Validation results in detail:",
            Position = "Pos",
            Code = "Code",
            Level = "Adj. level",
            Text = "Text",
            Path = "Path: ",
            Line = " Line: ",
            Column = " Column: ",
            AssessmentProcessingError = "Assessment: The validation could not be completed. The document should not be accepted automatically.",
            AssessmentNoScenario = "Assessment: It is recommended to reject the document, as no validation scenario matched.",
            AssessmentAccept = "Assessment: It is recommended to accept and process the document.",
            AssessmentAcceptTolerated = "Assessment: It is recommended to accept and process the document, as the existing errors are currently tolerated.",
            AssessmentReject = "Assessment: It is recommended to reject the document.",
            Content = "Content of the invoice document:",
            Epilog = "This validation report was created with "
        };


        public static XElement Create(ValidationReport report)
        {
            Texts t = report.Language == ReportLanguage.English ? _English : _German;
            XElement body = new XElement(_H + "body");
            XElement html = new XElement(_H + "html",
                new XAttribute("data-report-type", "report"),
                new XElement(_H + "head",
                    new XElement(_H + "title", t.Title),
                    new XElement(_H + "meta", new XAttribute("charset", "utf-8")),
                    new XElement(_H + "style", _Css)),
                body);

            body.Add(new XElement(_H + "h1", t.Title));
            body.Add(_Metadata(report, t));
            if (report.ProcessingErrors.Count > 0)
            {
                body.Add(new XElement(_H + "div", new XAttribute("class", "processing-errors"),
                    new XElement(_H + "p", new XAttribute("class", "important error"), new XElement(_H + "b", t.ProcessingErrorLabel), t.ProcessingErrorText),
                    new XElement(_H + "ul", report.ProcessingErrors.Select(e => new XElement(_H + "li", e)))));
            }
            body.Add(_Conformance(report, t));
            if (report.Messages.Any())
            {
                body.Add(_Results(report, t));
            }
            body.Add(_Assessment(report, t));
            if (report.IncludeDocumentContent && report.InputDocument != null)
            {
                XdmNode element = report.InputDocument.Root.Children.FirstOrDefault(n => n.Kind == XdmNodeKind.Element);
                if (element != null)
                {
                    XElement table = new XElement(_H + "table", new XAttribute("class", "document"));
                    int counter = 0;
                    _Content(element, 1, table, ref counter);
                    body.Add(new XElement(_H + "p", new XAttribute("class", "important"), t.Content));
                    body.Add(table);
                }
            }
            body.Add(new XElement(_H + "p", new XAttribute("class", "info"), t.Epilog + report.EngineName + "."));
            return html;
        }


        private static XElement _Metadata(ValidationReport report, Texts t)
        {
            DateTime timestamp = report.Timestamp.UtcDateTime;
            XElement metadata = new XElement(_H + "div", new XAttribute("class", "metadata"),
                new XElement(_H + "p", new XAttribute("class", "important"), t.DocumentInformation),
                new XElement(_H + "dl",
                    new XElement(_H + "dt", t.Reference),
                    new XElement(_H + "dd", report.DocumentReference ?? ""),
                    new XElement(_H + "dt", t.Timestamp),
                    new XElement(_H + "dd", timestamp.ToString("d.M.yyyy H:mm:ss", CultureInfo.InvariantCulture)),
                    new XElement(_H + "dt", t.DocumentType),
                    new XElement(_H + "dd", report.Scenario != null ? (object)report.Scenario.Name : new XElement(_H + "b", new XAttribute("class", "error"), t.Unknown))));
            if (report.Scenario != null && report.DocumentData.Count > 0)
            {
                XElement data = new XElement(_H + "dl");
                foreach (KeyValuePair<string, string> item in report.DocumentData)
                {
                    string label = item.Key == "seller" ? t.Seller : item.Key == "id" ? t.InvoiceNumber : item.Key == "issueDate" ? t.IssueDate : item.Key + ":";
                    data.Add(new XElement(_H + "dt", label), new XElement(_H + "dd", item.Value));
                }
                metadata.Add(data);
            }
            return metadata;
        }


        private static XElement _Conformance(ValidationReport report, Texts t)
        {
            int errors = report.Messages.Count(m => m.Level == ValidationLevel.Error);
            int warnings = report.Messages.Count(m => m.Level == ValidationLevel.Warning);
            if (report.ProcessingErrors.Count > 0)
            {
                return new XElement(_H + "p", new XAttribute("class", "important error"), new XElement(_H + "b", t.ConformanceLabel), t.ConformanceProcessingError);
            }
            if (report.Scenario != null)
            {
                XElement p = new XElement(_H + "p", new XAttribute("class", "important"), new XElement(_H + "b", t.ConformanceLabel), t.ConformanceContains);
                if (errors + warnings == 0)
                {
                    p.Add(t.ConformanceNoErrors);
                }
                else
                {
                    p.Add(String.Format(CultureInfo.InvariantCulture, t.ConformanceErrorsFormat, errors, warnings), new XElement(_H + "b", t.NotConformant), t.ConformanceSuffix);
                }
                return p;
            }
            return new XElement(_H + "p", new XAttribute("class", "important"), new XElement(_H + "b", t.ConformanceLabel), t.ConformanceNoScenario, new XElement(_H + "b", t.NotConformant), t.ConformanceSuffix);
        }


        private static IEnumerable<XElement> _Results(ValidationReport report, Texts t)
        {
            yield return new XElement(_H + "p", t.Overview);
            XElement overview = new XElement(_H + "tbody");
            foreach (ValidationStepResult step in report.Steps)
            {
                overview.Add(new XElement(_H + "tr",
                    new XElement(_H + "td", (step.Resource?.Name ?? "") + " (" + step.Id + ")"),
                    new XElement(_H + "td", new XAttribute("style", "width: 30mm;"), step.Messages.Count(m => m.Level == ValidationLevel.Error)),
                    new XElement(_H + "td", new XAttribute("style", "width: 30mm;"), step.Messages.Count(m => m.Level == ValidationLevel.Warning)),
                    new XElement(_H + "td", new XAttribute("style", "width: 30mm;"), step.Messages.Count(m => m.Level == ValidationLevel.Information))));
            }
            yield return new XElement(_H + "table", new XAttribute("class", "tbl-errors"),
                new XElement(_H + "thead", new XElement(_H + "tr",
                    new XElement(_H + "th", t.Step), new XElement(_H + "th", t.ErrorsHeader), new XElement(_H + "th", t.WarningsHeader), new XElement(_H + "th", t.InformationHeader))),
                overview);

            yield return new XElement(_H + "p", t.Details);
            XElement details = new XElement(_H + "tbody");
            foreach (ValidationMessage message in report.Messages)
            {
                string level = ReportWriter.LevelName(message.CustomLevel);
                details.Add(new XElement(_H + "tr", new XAttribute("class", level),
                    new XElement(_H + "td", new XAttribute("rowspan", "2"), message.Id),
                    new XElement(_H + "td", new XAttribute("rowspan", "2"), message.Code),
                    new XElement(_H + "td", new XAttribute("rowspan", "2"), level),
                    new XElement(_H + "td", message.NormalizedText)));
                string location = "";
                if (message.XPathLocation != null)
                {
                    location += t.Path + message.XPathLocation;
                }
                if (message.LineNumber.HasValue)
                {
                    location += t.Line + message.LineNumber.Value.ToString(CultureInfo.InvariantCulture);
                }
                if (message.ColumnNumber.HasValue)
                {
                    location += t.Column + message.ColumnNumber.Value.ToString(CultureInfo.InvariantCulture);
                }
                details.Add(new XElement(_H + "tr", new XAttribute("class", level), new XElement(_H + "td", location)));
            }
            yield return new XElement(_H + "table", new XAttribute("class", "tbl-errors"),
                new XElement(_H + "thead", new XElement(_H + "tr",
                    new XElement(_H + "th", new XAttribute("style", "width: 30mm;"), t.Position),
                    new XElement(_H + "th", new XAttribute("style", "width: 25mm;"), t.Code),
                    new XElement(_H + "th", new XAttribute("style", "width: 25mm;"), t.Level),
                    new XElement(_H + "th", t.Text))),
                details);
        }


        private static XElement _Assessment(ValidationReport report, Texts t)
        {
            int errors = report.Messages.Count(m => m.Level == ValidationLevel.Error);
            int customErrors = report.Messages.Count(m => m.CustomLevel == ValidationLevel.Error);
            if (report.ProcessingErrors.Count > 0)
            {
                return new XElement(_H + "p", new XAttribute("class", "important error"), t.AssessmentProcessingError);
            }
            if (report.Scenario == null || ReportWriter._IsFallback(report.Scenario))
            {
                return new XElement(_H + "p", new XAttribute("class", "important error"), t.AssessmentNoScenario);
            }
            if (errors == 0 && customErrors == 0)
            {
                return new XElement(_H + "p", new XAttribute("class", "important"), t.AssessmentAccept);
            }
            if (errors > 0 && customErrors == 0)
            {
                return new XElement(_H + "p", new XAttribute("class", "important"), t.AssessmentAcceptTolerated);
            }
            return new XElement(_H + "p", new XAttribute("class", "important error"), t.AssessmentReject);
        }


        private static void _Content(XdmNode element, int level, XElement table, ref int counter)
        {
            counter++;
            string number = counter.ToString("0000", CultureInfo.InvariantCulture);
            XElement cell = new XElement(_H + "td", new XAttribute("class", "element level" + level), new XAttribute("title", element.LocalName));
            bool binary = element.LocalName == "EmbeddedDocumentBinaryObject" || element.LocalName == "AttachmentBinaryObject";
            foreach (XdmNode text in element.Children.Where(c => c.Kind == XdmNodeKind.Text))
            {
                cell.Add(new XElement(_H + "div", new XAttribute("class", "val"), binary && !String.IsNullOrWhiteSpace(text.Value) ? "[ … ]" : text.Value));
            }
            foreach (XdmNode attribute in element.Attributes)
            {
                if (attribute.LocalName == "schemaLocation" && attribute.NamespaceUri == XmlNamespaces.Xsi)
                {
                    continue;
                }
                cell.Add(new XElement(_H + "div", new XAttribute("class", "attribute"), new XAttribute("title", attribute.LocalName), attribute.Value));
            }
            table.Add(new XElement(_H + "tr", new XAttribute("class", "row"), new XAttribute("id", number),
                new XElement(_H + "td", new XAttribute("class", "pos"), number),
                cell));
            foreach (XdmNode child in element.Children.Where(c => c.Kind == XdmNodeKind.Element))
            {
                _Content(child, level + 1, table, ref counter);
            }
        }
    }
}
