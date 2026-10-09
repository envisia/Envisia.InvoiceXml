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
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Envisia.InvoiceXml.Validation.Schematron;
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// Options of the <see cref="InvoiceXmlValidator"/>.
    /// </summary>
    public sealed class ValidatorOptions
    {
        /// <summary>The language of the HTML report (default: German, as the KoSIT validator).</summary>
        public ReportLanguage ReportLanguage { get; set; } = ReportLanguage.German;

        /// <summary>
        /// Run the Schematron rules even if the document is not valid against the XML schema. The KoSIT validator
        /// skips them (default: false).
        /// </summary>
        public bool ValidateSchematronOnSchemaErrors { get; set; }

        /// <summary>Include the content of the validated document in the HTML report (default: true, as the KoSIT validator).</summary>
        public bool IncludeDocumentContentInReport { get; set; } = true;
    }


    /// <summary>
    /// Validates electronic invoices like the KoSIT validator: the scenario that matches the document is
    /// selected, the document is validated against the XML schema and the Schematron rules of the scenario, and a
    /// report with an acceptance recommendation is created.
    ///
    /// By default the official rules for XRechnung, ZUGFeRD / Factur-X and EN 16931 are used, see
    /// <see cref="ValidatorConfiguration"/>. Instances are thread-safe.
    /// </summary>
    /// <example>
    /// <code>
    /// InvoiceXmlValidator validator = new InvoiceXmlValidator();
    /// ValidationReport report = validator.Validate("invoice.xml");
    /// if (!report.IsAcceptable)
    /// {
    ///     foreach (ValidationMessage error in report.Errors)
    ///     {
    ///         Console.WriteLine(error);
    ///     }
    /// }
    /// report.SaveHtml("invoice-report.html");
    /// </code>
    /// </example>
    public sealed class InvoiceXmlValidator : IInvoiceValidator
    {
        private static readonly string _EngineName = "Envisia.InvoiceXml.Validation " + _Version();
        private readonly IReadOnlyList<ValidatorConfiguration> _Configurations;
        private readonly ValidatorOptions _Options;


        /// <summary>
        /// Creates a validator with the built-in configurations (<see cref="ValidatorConfiguration.Default"/>).
        /// </summary>
        public InvoiceXmlValidator(ValidatorOptions options = null) : this(ValidatorConfiguration.Default, options)
        {
        }


        /// <summary>
        /// Creates a validator with one configuration.
        /// </summary>
        public InvoiceXmlValidator(ValidatorConfiguration configuration, ValidatorOptions options = null) : this(new[] { configuration }, options)
        {
        }


        /// <summary>
        /// Creates a validator with several configurations. They are tried in order: the first configuration in
        /// which exactly one scenario matches the document is used. Within a configuration, a document matching
        /// more than one scenario is not accepted (as in the KoSIT validator).
        /// </summary>
        public InvoiceXmlValidator(IEnumerable<ValidatorConfiguration> configurations, ValidatorOptions options = null)
        {
            if (configurations == null)
            {
                throw new ArgumentNullException(nameof(configurations));
            }
            _Configurations = configurations.ToList();
            if (_Configurations.Count == 0 || _Configurations.Any(c => c == null))
            {
                throw new ArgumentException("At least one configuration is required", nameof(configurations));
            }
            _Options = options ?? new ValidatorOptions();
        }


        /// <summary>
        /// The configurations of the validator.
        /// </summary>
        public IReadOnlyList<ValidatorConfiguration> Configurations => _Configurations;


        /// <summary>
        /// Validates an XML file.
        /// </summary>
        public ValidationReport Validate(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return Validate(File.ReadAllBytes(path), path);
        }


        /// <summary>
        /// Validates the XML document read from the stream.
        /// </summary>
        /// <param name="stream">The document</param>
        /// <param name="documentReference">Name of the document shown in the report</param>
        public ValidationReport Validate(Stream stream, string documentReference = null)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            using (MemoryStream buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                return Validate(buffer.ToArray(), documentReference);
            }
        }


        /// <summary>
        /// Validates the XML document.
        /// </summary>
        /// <param name="document">The document</param>
        /// <param name="documentReference">Name of the document shown in the report</param>
        public ValidationReport Validate(byte[] document, string documentReference = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            ValidationReport report = new ValidationReport
            {
                EngineName = _EngineName,
                Timestamp = DateTimeOffset.UtcNow,
                DocumentReference = documentReference ?? "",
                DocumentHash = _Hash(document),
                Language = _Options.ReportLanguage,
                IncludeDocumentContent = _Options.IncludeDocumentContentInReport
            };
            List<ValidationStepResult> steps = new List<ValidationStepResult>();
            List<string> processingErrors = new List<string>();

            // 1. well-formedness
            ValidationStepResult wellFormedness = new ValidationStepResult("val-xml", null);
            XdmDocument input = null;
            try
            {
                using (MemoryStream stream = new MemoryStream(document, false))
                {
                    input = XdmDocumentBuilder.Load(stream, _DocumentUri(documentReference));
                }
            }
            catch (XmlException e)
            {
                wellFormedness.IsValid = false;
                wellFormedness._Messages.Add(new ValidationMessage
                {
                    Id = "val-xml.1",
                    Code = "generic-error",
                    Level = ValidationLevel.Error,
                    CustomLevel = ValidationLevel.Error,
                    Text = "Error reported by XML parser: " + e.Message,
                    LineNumber = e.LineNumber > 0 ? e.LineNumber : (int?)null,
                    ColumnNumber = e.LinePosition > 0 ? e.LinePosition : (int?)null,
                    StepId = "val-xml"
                });
            }
            report.IsWellFormed = input != null;
            report.InputDocument = input;

            // 2. scenario
            ValidationScenario scenario = null;
            if (input != null)
            {
                foreach (ValidatorConfiguration configuration in _Configurations)
                {
                    List<ValidationScenario> matches = configuration.Scenarios.Where(s => s.Matches(input)).ToList();
                    if (matches.Count == 0)
                    {
                        continue;
                    }
                    if (matches.Count == 1 && !ReportWriter._IsFallback(matches[0]))
                    {
                        scenario = matches[0];
                        report.Configuration = configuration;
                    }
                    break;
                }
            }
            report.Scenario = scenario;

            if (scenario != null)
            {
                // 3. XML schema
                bool schemaValid = true;
                if (scenario.XmlSchemas.Count > 0)
                {
                    ValidationStepResult xsd = new ValidationStepResult("val-xsd", scenario.XmlSchemas[0]);
                    try
                    {
                        XmlSchemaSet schema = scenario.GetXmlSchema();
                        foreach (XmlSchemaException error in XmlSchemaLoader.Validate(document, schema))
                        {
                            xsd._Messages.Add(new ValidationMessage
                            {
                                Id = "val-xsd." + (xsd._Messages.Count + 1),
                                Code = "generic-error",
                                Level = ValidationLevel.Error,
                                CustomLevel = scenario.GetCustomLevel("generic-error", ValidationLevel.Error),
                                Text = error.Message,
                                LineNumber = error.LineNumber > 0 ? error.LineNumber : (int?)null,
                                ColumnNumber = error.LinePosition > 0 ? error.LinePosition : (int?)null,
                                StepId = xsd.Id
                            });
                        }
                    }
                    catch (Exception e) when (!(e is OutOfMemoryException))
                    {
                        processingErrors.Add("Error processing the XML schema validation " + scenario.XmlSchemas[0].Name + ": " + e.Message);
                        xsd._Messages.Add(_ProcessingError(xsd.Id, e.Message));
                    }
                    xsd.IsValid = xsd.Messages.Count == 0;
                    schemaValid = xsd.IsValid;
                    steps.Add(xsd);
                }
                report.IsSchemaValid = schemaValid;

                // 4. Schematron
                if (schemaValid || _Options.ValidateSchematronOnSchemaErrors)
                {
                    for (int i = 0; i < scenario.SchematronRules.Count; i++)
                    {
                        string stepId = "val-sch." + (i + 1);
                        ValidationStepResult step = new ValidationStepResult(stepId, scenario.SchematronRules[i]);
                        try
                        {
                            SchematronResult result = scenario.GetSchematron(i).Validate(input);
                            step.SchematronResult = result;
                            foreach (SchematronMessage message in result.Messages)
                            {
                                ValidationLevel level = _Level(message.Flag, message.Role);
                                string code = message.Id ?? "UNSPECIFIC";
                                step._Messages.Add(new ValidationMessage
                                {
                                    Id = stepId + "." + (step._Messages.Count + 1),
                                    Code = code,
                                    Level = level,
                                    CustomLevel = scenario.GetCustomLevel(code, level),
                                    Text = message.Text,
                                    XPathLocation = message.Location,
                                    LineNumber = message.LineNumber,
                                    ColumnNumber = message.LinePosition,
                                    StepId = stepId,
                                    SchematronMessage = message
                                });
                            }
                            step.IsValid = !step.Messages.Any(m => m.Level == ValidationLevel.Error || m.Level == ValidationLevel.Warning);
                        }
                        catch (Exception e) when (e is SchematronException || e is XPathException || e is IOException || e is InvalidOperationException || e is NotSupportedException)
                        {
                            processingErrors.Add("Error processing schematron validation " + scenario.SchematronRules[i].Name + ". Error is " + e.Message);
                            step._Messages.Add(_ProcessingError(stepId, e.Message));
                            step.IsValid = false;
                        }
                        steps.Add(step);
                    }
                }
                report.DocumentData = _DocumentData(input);
            }
            steps.Add(wellFormedness);
            report.Steps = steps;
            report.ProcessingErrors = processingErrors;

            report.IsValid = processingErrors.Count == 0 && scenario != null && steps.All(s => s.IsValid);
            report.Assessment = ReportWriter.ComputeAssessment(report);
            report.Recommendation = _Accept(report, scenario);
            return report;
        }


        /// <summary>
        /// Validates the XML document (<see cref="IInvoiceValidator"/>).
        /// </summary>
        InvoiceValidationResult IInvoiceValidator.Validate(byte[] document, string documentName)
        {
            return Validate(document, documentName).ToInvoiceValidationResult();
        }


        /// <summary>
        /// The acceptance as the KoSIT validator computes it: the acceptMatch expression of the scenario evaluated on
        /// the report (if the document is schema valid), else valid schema and no failed assertion.
        /// </summary>
        private static AcceptRecommendation _Accept(ValidationReport report, ValidationScenario scenario)
        {
            if (scenario == null)
            {
                return AcceptRecommendation.Reject;
            }
            if (report.ProcessingErrors.Count > 0)
            {
                return AcceptRecommendation.Reject;
            }
            if (report.IsSchemaValid && scenario.AcceptMatch != null)
            {
                XDocument xml = report.ToXml();
                XdmDocument reportDocument;
                using (XmlReader reader = xml.CreateReader())
                {
                    reportDocument = XdmDocumentBuilder.Load(reader, null);
                }
                try
                {
                    return scenario.EvaluateAcceptMatch(reportDocument) == true ? AcceptRecommendation.Accept : AcceptRecommendation.Reject;
                }
                catch (XPathException)
                {
                    return AcceptRecommendation.Undefined;
                }
            }
            bool failedAssertions = report.Steps.Any(s => s.SchematronResult != null && s.SchematronResult.FailedAssertions.Any());
            return report.IsSchemaValid && !failedAssertions ? AcceptRecommendation.Accept : AcceptRecommendation.Reject;
        }


        /// <summary>
        /// The level of a Schematron message from its flag and role (default-report.xsl).
        /// </summary>
        private static ValidationLevel _Level(string flag, string role)
        {
            string[] values = { flag, role };
            if (values.Any(v => v == "fatal" || v == "error"))
            {
                return ValidationLevel.Error;
            }
            if (values.Any(v => v == "warning" || v == "warn"))
            {
                return ValidationLevel.Warning;
            }
            if (values.Any(v => v == "information" || v == "info"))
            {
                return ValidationLevel.Information;
            }
            return ValidationLevel.Error;
        }


        private static ValidationMessage _ProcessingError(string stepId, string text)
        {
            return new ValidationMessage
            {
                Id = stepId + ".processing-error",
                Code = "PROCESSING_ERROR",
                Level = ValidationLevel.Error,
                CustomLevel = ValidationLevel.Error,
                Text = text,
                XPathLocation = "/",
                StepId = stepId
            };
        }


        private static readonly XPathExpression[] _DocumentDataExpressions = _CompileDocumentData();


        private static XPathExpression[] _CompileDocumentData()
        {
            StaticContext context = new StaticContext(new Dictionary<string, string>
            {
                ["cac"] = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2",
                ["cbc"] = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2",
                ["rsm"] = "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100",
                ["ram"] = "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100",
                ["udt"] = "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100"
            }, null, null);
            // the document data of xrechnung-report.xsl
            return new[]
            {
                XPathExpression.Compile("*/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName, rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeAgreement/ram:SellerTradeParty/ram:Name", context),
                XPathExpression.Compile("*/cbc:ID, rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:ID", context),
                XPathExpression.Compile("*/cbc:IssueDate, rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:IssueDateTime/udt:DateTimeString", context)
            };
        }


        private static List<KeyValuePair<string, string>> _DocumentData(XdmDocument document)
        {
            string[] names = { "seller", "id", "issueDate" };
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < names.Length; i++)
            {
                foreach (Item item in _DocumentDataExpressions[i].Evaluate(new EvalContext(new EvaluationEnvironment()), document.Root))
                {
                    result.Add(new KeyValuePair<string, string>(names[i], ((XdmNode)item).StringValue));
                }
            }
            return result;
        }


        private static string _DocumentUri(string reference)
        {
            if (String.IsNullOrEmpty(reference))
            {
                return null;
            }
            try
            {
                return Path.IsPathRooted(reference) ? new Uri(reference).AbsoluteUri : null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        private static string _Hash(byte[] document)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(document));
            }
        }


        private static string _Version()
        {
            Assembly assembly = typeof(InvoiceXmlValidator).Assembly;
            string informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!String.IsNullOrEmpty(informational))
            {
                int plus = informational.IndexOf('+');
                return plus > 0 ? informational.Substring(0, plus) : informational;
            }
            return assembly.GetName().Version.ToString(3);
        }
    }
}
