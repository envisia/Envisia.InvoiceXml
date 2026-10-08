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
using System.Xml;
using System.Xml.Schema;

namespace Envisia.InvoiceXml.Tests
{
    /// <summary>
    /// Validates invoice XML against the official XSD schemas shipped in the documentation folder.
    /// </summary>
    internal static class SchemaValidator
    {
        private static readonly ConcurrentDictionary<string, XmlSchemaSet> _SchemaCache = new ConcurrentDictionary<string, XmlSchemaSet>();


        /// <summary>
        /// Root folder of the repository (the folder that contains Envisia.InvoiceXml.sln).
        /// </summary>
        internal static string RepositoryRoot { get; } = _FindRepositoryRoot();


        /// <summary>
        /// Factur-X / ZUGFeRD versions whose schemas are available in the documentation folder.
        /// </summary>
        internal const string FacturX108 = "1.08";      // ZUGFeRD 2.4
        internal const string FacturX1092 = "1.09.2";   // ZUGFeRD 2.5.2


        /// <summary>
        /// Returns the path of the official Factur-X XSD for the given version and profile.
        /// XRechnung invoices in CII syntax are validated against the EN 16931 schema.
        /// </summary>
        internal static string GetFacturXSchemaPath(string version, Profile profile)
        {
            switch (version)
            {
                case FacturX108:
                    {
                        string schemaFolder = Path.Combine(RepositoryRoot, "documentation", "zugferd240en", "Schema");
                        switch (profile)
                        {
                            case Profile.Minimum: return Path.Combine(schemaFolder, "0_Factur-X_1.08_MINIMUM", "FACTUR-X_MINIMUM.xsd");
                            case Profile.BasicWL: return Path.Combine(schemaFolder, "1_Factur-X_1.08_BASICWL", "FACTUR-X_BASIC-WL.xsd");
                            case Profile.Basic: return Path.Combine(schemaFolder, "2_Factur-X_1.08_BASIC", "FACTUR-X_BASIC.xsd");
                            case Profile.Comfort:
                            case Profile.XRechnung:
                            case Profile.XRechnung1: return Path.Combine(schemaFolder, "3_Factur-X_1.08_EN16931", "FACTUR-X_EN16931.xsd");
                            case Profile.Extended: return Path.Combine(schemaFolder, "4_Factur-X_1.08_EXTENDED", "FACTUR-X_EXTENDED.xsd");
                        }
                        break;
                    }
                case FacturX1092:
                    {
                        string schemaFolder = Path.Combine(RepositoryRoot, "documentation", "zugferd252", "Schema");
                        switch (profile)
                        {
                            case Profile.Minimum: return Path.Combine(schemaFolder, "MINIMUM", "FACTUR-X_MINIMUM.xsd");
                            case Profile.BasicWL: return Path.Combine(schemaFolder, "BASIC-WL", "FACTUR-X_BASICWL.xsd");
                            case Profile.Basic: return Path.Combine(schemaFolder, "BASIC", "FACTUR-X_BASIC.xsd");
                            case Profile.Comfort:
                            case Profile.XRechnung:
                            case Profile.XRechnung1: return Path.Combine(schemaFolder, "EN16931", "FACTUR-X_EN16931.xsd");
                            case Profile.Extended: return Path.Combine(schemaFolder, "EXTENDED", "FACTUR-X_EXTENDED.xsd");
                        }
                        break;
                    }
            }

            throw new ArgumentOutOfRangeException(nameof(profile), profile, $"No Factur-X {version} schema for this profile");
        } // !GetFacturXSchemaPath()


        /// <summary>
        /// Validates the xml in the given stream against the given schema and returns all validation errors.
        /// The stream position is reset to the beginning afterwards.
        /// </summary>
        internal static List<string> Validate(Stream xml, string schemaPath)
        {
            XmlSchemaSet schemas = _SchemaCache.GetOrAdd(schemaPath, path =>
            {
                XmlSchemaSet set = new XmlSchemaSet
                {
                    XmlResolver = new XmlUrlResolver()
                };

                // the UBL signature module (xmldsig) contains a DTD
                XmlReaderSettings schemaReaderSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Parse,
                    XmlResolver = new XmlUrlResolver()
                };
                using (XmlReader schemaReader = XmlReader.Create(path, schemaReaderSettings))
                {
                    set.Add(null, schemaReader);
                }
                set.Compile();
                return set;
            });

            List<string> errors = new List<string>();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            };
            settings.ValidationEventHandler += (sender, args) => errors.Add($"{args.Severity}: {args.Message} (line {args.Exception?.LineNumber})");

            xml.Seek(0, SeekOrigin.Begin);
            using (XmlReader reader = XmlReader.Create(xml, settings))
            {
                while (reader.Read())
                {
                }
            }
            xml.Seek(0, SeekOrigin.Begin);

            return errors;
        } // !Validate()


        /// <summary>
        /// Returns the path of the OASIS UBL 2.1 schema for invoices or credit notes.
        /// </summary>
        internal static string GetUblSchemaPath(bool creditNote)
        {
            return Path.Combine(RepositoryRoot, "documentation", "ubl21", "xsd", "maindoc", creditNote ? "UBL-CreditNote-2.1.xsd" : "UBL-Invoice-2.1.xsd");
        } // !GetUblSchemaPath()


        private static string _FindRepositoryRoot()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Envisia.InvoiceXml.sln")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find the repository root starting at " + AppContext.BaseDirectory);
        } // !_FindRepositoryRoot()
    }
}
