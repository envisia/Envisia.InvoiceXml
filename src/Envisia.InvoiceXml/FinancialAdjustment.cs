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
    /// Financial adjustment on document level (ApplicableHeaderTradeSettlement/SpecifiedFinancialAdjustment).
    ///
    /// Introduced with Factur-X 1.09 / ZUGFeRD 2.5, EXTENDED profile only. The amounts are not part of the
    /// invoice total amount with VAT (BT-112) but are added to the amount due for payment (BT-115), see BR-FXEXT-CO-16:
    /// BT-115 = BT-112 - BT-113 + BT-114 + sum of the financial adjustment amounts.
    /// The Factur-X 1.09.2 schematron describes the amount as "Charge amount collected on behalf of a third party (BT-179)".
    /// </summary>
    public class FinancialAdjustment
    {
        /// <summary>
        /// Reason of the financial adjustment (mandatory)
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Amount of the financial adjustment (mandatory), without currency
        /// </summary>
        public decimal ActualAmount { get; set; }
    }
}
