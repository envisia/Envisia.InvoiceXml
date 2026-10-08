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
using System.Text;

namespace Envisia.InvoiceXml
{
    /// <summary>
    /// Fluent builder for <see cref="InvoiceFormatOptions"/>.
    /// </summary>
    public sealed class InvoiceOptionsBuilder
    {
        private readonly InvoiceFormatOptions _options;

        private InvoiceOptionsBuilder()
        {
            _options = new InvoiceFormatOptions();
        } // !InvoiceOptionsBuilder()


        private InvoiceOptionsBuilder(InvoiceFormatOptions baseOptions)
        {
            _options = baseOptions?.Clone() ?? throw new ArgumentNullException(nameof(baseOptions));
        } // !InvoiceOptionsBuilder()


        /// <summary>
        /// Creates a builder with empty options.
        /// </summary>
        public static InvoiceOptionsBuilder Create()
        {
            return new InvoiceOptionsBuilder();
        } // !Create()


        /// <summary>
        /// Creates a builder that starts with a copy of the given options.
        /// </summary>
        /// <param name="options">Options to start from</param>
        public static InvoiceOptionsBuilder From(InvoiceFormatOptions options)
        {
            return new InvoiceOptionsBuilder(options);
        } // !From()


        /// <summary>
        /// Creates a builder with the recommended default options.
        /// </summary>
        public static InvoiceOptionsBuilder CreateDefault()
        {
            return Create().UseRecommendedDefaults();
        } // !CreateDefault()


        /// <summary>
        /// Returns the configured options.
        /// </summary>
        public InvoiceFormatOptions Build()
        {
            return _options;
        } // !Build()


        /// <summary>
        /// Enables or disables explanatory comments in the written XML.
        /// </summary>
        /// <param name="enable">True to write comments</param>
        public InvoiceOptionsBuilder EnableXmlComments(bool enable = true)
        {
            _options.IncludeXmlComments = enable;
            return this;
        } // !EnableXmlComments()


        /// <summary>
        /// Adds comments that are written at the beginning of the XML document.
        /// </summary>
        /// <param name="comments">Comments to add</param>
        public InvoiceOptionsBuilder AddHeaderXmlComment(List<string> comments)
        {
            _options.XmlHeaderComments.AddRange(comments);
            return this;
        } // !AddHeaderXmlComment()


        /// <summary>
        /// Adds a comment that is written at the beginning of the XML document. Empty comments are ignored.
        /// </summary>
        /// <param name="comment">Comment to add</param>
        public InvoiceOptionsBuilder AddHeaderXmlComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return this;
            }
            _options.XmlHeaderComments.Add(comment);
            return this;
        } // !AddHeaderXmlComment()


        /// <summary>
        /// Applies the recommended defaults (no XML comments).
        /// </summary>
        public InvoiceOptionsBuilder UseRecommendedDefaults()
        {
            return EnableXmlComments(false);
        } // !UseRecommendedDefaults()


        /// <summary>
        /// Removes characters that are not allowed in XML instead of throwing an <see cref="IllegalCharacterException"/>.
        /// </summary>
        public InvoiceOptionsBuilder AutomaticallyCleanInvalidCharacters()
        {
            _options.AutomaticallyCleanInvalidCharacters = true;
            return this;
        } // !AutomaticallyCleanInvalidCharacters()
    }
}
