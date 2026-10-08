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
    /// Maps an enum member to the code that is used for it in the invoice XML.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumStringValueAttribute : Attribute
    {
        /// <summary>
        /// Code that is written for the enum member
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Additional codes that are accepted for the enum member when reading (e.g. withdrawn or misspelled codes)
        /// </summary>
        public string[] LegacyValues { get; }

        /// <summary>
        /// Creates the mapping for an enum member.
        /// </summary>
        /// <param name="value">Code that is written for the enum member</param>
        /// <param name="legacyValues">Additional codes that are accepted when reading</param>
        public EnumStringValueAttribute(string value, params string[] legacyValues)
        {
            Value = value;
            LegacyValues = legacyValues ?? Array.Empty<string>();
        }
    }
}
