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
using System.IO;

namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// Validation of invoices created with <see cref="InvoiceDescriptor"/>.
    /// </summary>
    public static class InvoiceDescriptorValidationExtensions
    {
        /// <summary>
        /// Writes the invoice in the given version, profile and syntax and validates the XML with the official XML
        /// schemas and Schematron rules (XRechnung, ZUGFeRD / Factur-X, EN 16931).
        /// </summary>
        /// <param name="descriptor">The invoice</param>
        /// <param name="version">The version the invoice is written in</param>
        /// <param name="profile">The profile the invoice is written in</param>
        /// <param name="format">The syntax (CII or UBL)</param>
        /// <param name="validator">The validator to use, by default one with the built-in configurations</param>
        /// <returns>The validation report</returns>
        public static ValidationReport ValidateXml(this InvoiceDescriptor descriptor, ZUGFeRDVersion version, Profile profile, ZUGFeRDFormats format = ZUGFeRDFormats.CII, InvoiceXmlValidator validator = null)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                descriptor.Save(stream, version, profile, format);
                return (validator ?? _Default.Value).Validate(stream.ToArray(), descriptor.InvoiceNo);
            }
        }


        private static readonly Lazy<InvoiceXmlValidator> _Default = new Lazy<InvoiceXmlValidator>(() => new InvoiceXmlValidator());
    }
}
