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
    /// Enumeration of the different ZUGFeRD versions supported by ZUGFeRD-csharp
    /// </summary>
    public enum ZUGFeRDVersion
    {
        /// <summary>
        /// Version 1.x - first public ZUGFeRD version
        /// </summary>
        Version1 = 100,

        /// <summary>
        /// Version 2.0 - second major ZUGFeRD version 
        /// </summary>
        Version20 = 200,

        /// <summary>
        /// ZUGFeRD 2.1 - 2.4 (Factur-X 1.0 - 1.08), supports XRechnung.
        ///
        /// All these versions use the same guideline identifiers (BT-24). The output is valid against the
        /// ZUGFeRD 2.4 / Factur-X 1.08 schemas (elements that were added in 2.4 are only written when they are set).
        /// </summary>
        Version23 = 230,

        /// <summary>
        /// ZUGFeRD 2.5 (Factur-X 1.09, currently 2.5.2 / 1.09.2).
        ///
        /// Uses the same guideline identifiers as <see cref="Version23"/> and additionally writes the EXTENDED
        /// elements that were introduced with Factur-X 1.09: debtor BIC and account name, manufacturer of an item and
        /// financial adjustments. Invoices written with this version may therefore not be accepted by receivers that
        /// still validate against ZUGFeRD 2.4.
        ///
        /// When reading, ZUGFeRD 2.x invoices cannot be told apart and are always reported as <see cref="Version23"/>.
        /// </summary>
        Version25 = 250
    }




    internal static class ZUGFeRDVersionExtensions
    {
        public static ZUGFeRDVersion FromString(this ZUGFeRDVersion _, string s)
        {
            return EnumExtensions.StringToEnum<ZUGFeRDVersion>(s);
        } // !FromString()


        public static string EnumToString(this ZUGFeRDVersion c)
        {
            return EnumExtensions.EnumToString<ZUGFeRDVersion>(c);
        } // !ToString()


        public static string GetDottedVersion(this ZUGFeRDVersion c)
        {
            switch (c)
            {
                case ZUGFeRDVersion.Version1: return "1.0";
                case ZUGFeRDVersion.Version20: return "2.0";
                case ZUGFeRDVersion.Version23: return "2.3";
                case ZUGFeRDVersion.Version25: return "2.5";
                default: return string.Empty;
            }
        } // !GetDottedVersion()
    }
}
