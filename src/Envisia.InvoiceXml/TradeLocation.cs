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
namespace Envisia.InvoiceXml
{
    /// <summary>
    /// Location that is relevant for the delivery terms (RelevantTradeLocation), e.g. the place named in an INCOTERM.
    ///
    /// Used on document level (BG-X-88) and on line level (BG-X-89), EXTENDED profile only.
    /// </summary>
    public class TradeLocation
    {
        /// <summary>
        /// Country code of the location
        ///
        /// Document level: BT-X-563, line level: BT-X-565
        /// </summary>
        public CountryCodes? Country { get; set; }

        /// <summary>
        /// Name of the location
        ///
        /// Document level: BT-X-564, line level: BT-X-566
        /// </summary>
        public string Name { get; set; }
    }
}
