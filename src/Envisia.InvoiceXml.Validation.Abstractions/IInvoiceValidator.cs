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
    /// Validates electronic invoices (XRechnung, ZUGFeRD / Factur-X, EN 16931) against the official XML schemas and
    /// Schematron rules, as the KoSIT validator does.
    ///
    /// Implementations: <c>Envisia.InvoiceXml.Validation</c> (pure .NET) and
    /// <c>Envisia.InvoiceXml.Validation.GraalVM</c> (the KoSIT validator compiled into a native library). Both
    /// packages register their implementation with <c>services.AddInvoiceValidator()</c>, so the implementation is
    /// chosen by the package an application references. Implementations are thread-safe.
    /// </summary>
    public interface IInvoiceValidator
    {
        /// <summary>
        /// Validates an XML document.
        /// </summary>
        /// <param name="document">The XML document</param>
        /// <param name="documentName">Name of the document in the report (e.g. the file name)</param>
        /// <returns>The result with the KoSIT report</returns>
        public InvoiceValidationResult Validate(byte[] document, string documentName = null);
    }


    /// <summary>
    /// Convenience methods for <see cref="IInvoiceValidator"/>.
    /// </summary>
    public static class InvoiceValidatorExtensions
    {
        /// <summary>
        /// Validates an XML file.
        /// </summary>
        public static InvoiceValidationResult Validate(this IInvoiceValidator validator, string path)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return validator.Validate(File.ReadAllBytes(path), Path.GetFileName(path));
        }


        /// <summary>
        /// Validates the XML document read from a stream.
        /// </summary>
        public static InvoiceValidationResult Validate(this IInvoiceValidator validator, Stream stream, string documentName = null)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            using (MemoryStream buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                return validator.Validate(buffer.ToArray(), documentName);
            }
        }
    }
}
