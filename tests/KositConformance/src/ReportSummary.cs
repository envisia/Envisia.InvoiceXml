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

namespace Envisia.InvoiceXml.Tests.KositConformance
{
    /// <summary>
    /// The summary of a KoSIT report that the tests compare: validity, matched scenario, assessment, validation steps
    /// and every message (id, code, level, location, text). Same format as ReferenceRunner.summarize.
    /// </summary>
    public static class ReportSummary
    {
        private static readonly XNamespace _Rep = "http://www.xoev.de/de/validator/varl/1";
        private static readonly XNamespace _Scenarios = "http://www.xoev.de/de/validator/framework/1/scenarios";


        public static string Create(string reportXml)
        {
            XElement root = XDocument.Parse(reportXml).Root!;
            StringBuilder sb = new StringBuilder();
            sb.Append("valid=").Append((string?)root.Attribute("valid") ?? "").Append('\n');
            XElement? name = root.Descendants(_Scenarios + "name").FirstOrDefault(e => e.Parent?.Name == _Scenarios + "scenario");
            sb.Append("scenario=").Append(name != null ? name.Value.Trim() : "NONE").Append('\n');
            XElement? assessment = root.Descendants(_Rep + "assessment").FirstOrDefault();
            sb.Append("assessment=").Append(assessment?.Elements().FirstOrDefault()?.Name.LocalName ?? "").Append('\n');
            foreach (XElement step in root.Descendants(_Rep + "validationStepResult"))
            {
                sb.Append("step ").Append((string?)step.Attribute("id") ?? "").Append(" valid=").Append((string?)step.Attribute("valid") ?? "").Append('\n');
                foreach (XElement message in step.Elements(_Rep + "message"))
                {
                    sb.Append("  ").Append((string?)message.Attribute("id") ?? "").Append('|')
                      .Append((string?)message.Attribute("code") ?? "").Append('|')
                      .Append((string?)message.Attribute("level") ?? "").Append('|')
                      .Append((string?)message.Attribute("xpathLocation") ?? "").Append('|')
                      .Append(Normalize(message.Value)).Append('\n');
                }
            }
            return sb.ToString();
        }


        /// <summary>Collapses runs of whitespace (characters up to U+0020 and U+00A0) into one space and trims.</summary>
        public static string Normalize(string text)
        {
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (char c in text)
            {
                if (c <= ' ' || c == ' ')
                {
                    space = sb.Length > 0;
                }
                else
                {
                    if (space)
                    {
                        sb.Append(' ');
                        space = false;
                    }
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
