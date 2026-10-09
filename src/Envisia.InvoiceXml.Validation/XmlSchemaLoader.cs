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
using System.Net;
using System.Xml;
using System.Xml.Schema;
using Envisia.InvoiceXml.Validation.XPath;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// Loads XML schemas (with their includes and imports) from files or embedded resources and validates documents.
    /// </summary>
    internal static class XmlSchemaLoader
    {
        public static XmlSchemaSet Load(IReadOnlyList<ScenarioResource> resources)
        {
            ResourceResolver resolver = new ResourceResolver();
            XmlSchemaSet set = new XmlSchemaSet { XmlResolver = resolver };
            List<string> errors = new List<string>();
            set.ValidationEventHandler += (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    errors.Add(e.Message);
                }
            };
            foreach (ScenarioResource resource in resources)
            {
                // the reader settings are also used for the included and imported schemas
                using (Stream stream = ResourceLoader.Open(resource.Uri))
                using (XmlReader reader = XmlReader.Create(stream, _SchemaReaderSettings(resolver), resource.Uri))
                {
                    set.Add(null, reader);
                }
            }
            set.Compile();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException("The XML schema " + resources[0].Location + " is invalid: " + String.Join("; ", errors));
            }
            return set;
        }


        private static XmlReaderSettings _SchemaReaderSettings(XmlResolver resolver)
        {
            // the UBL signature module (xmldsig) declares the ds prefix in its internal DTD subset;
            // external DTDs and entities cannot be loaded, the resolver only serves local schema files
            return new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                MaxCharactersFromEntities = 1024 * 1024,
                XmlResolver = resolver
            };
        }


        /// <summary>
        /// Validates the document against the schema; returns the errors (severity, message, line, column).
        /// </summary>
        public static List<XmlSchemaException> Validate(byte[] document, XmlSchemaSet schema)
        {
            List<XmlSchemaException> errors = new List<XmlSchemaException>();
            XmlReaderSettings settings = XdmDocumentBuilder.CreateSecureSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.AllowXmlAttributes;
            settings.ValidationEventHandler += (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    errors.Add(e.Exception);
                }
            };
            using (MemoryStream stream = new MemoryStream(document, false))
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while (reader.Read())
                {
                }
            }
            return errors;
        }


        /// <summary>
        /// Resolves schema includes and imports to files and embedded resources; never accesses the network.
        /// </summary>
        private sealed class ResourceResolver : XmlResolver
        {
            public override ICredentials Credentials
            {
                set
                {
                }
            }


            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                if (baseUri != null && baseUri.OriginalString.StartsWith(ResourceLoader.EmbeddedScheme, StringComparison.Ordinal))
                {
                    return new Uri(ResourceLoader.ResolveEmbedded(baseUri.OriginalString, relativeUri));
                }
                return base.ResolveUri(baseUri, relativeUri);
            }


            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                string uri = absoluteUri.OriginalString;
                if (!uri.StartsWith(ResourceLoader.EmbeddedScheme, StringComparison.Ordinal) && !absoluteUri.IsFile)
                {
                    throw new XmlException("Access to " + uri + " is not allowed, only local schema files are loaded");
                }
                return ResourceLoader.Open(absoluteUri.IsFile ? absoluteUri.AbsoluteUri : uri);
            }
        }
    }
}
