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
    /// Zusätzliche Produkteigenschaft
    /// </summary>
    public class ApplicableProductCharacteristic
    {
        /// <summary>
        /// Art der Produkteigenschaft (Code), UNTDID 6313 + Factur-X-Erweiterung
        ///
        /// Nur im Profil Extended. BT-X-11
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// Beschriebene Produkteigenschaft
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Wert der Produkteigenschaft als numerische Messgröße
        ///
        /// Nur im Profil Extended. Factur-X 1.09 erlaubt je Eigenschaft entweder Value (BT-161) oder ValueMeasure, nicht beides (BR-FXEXT-BR-54-2).
        /// BT-X-12
        /// </summary>
        public decimal? ValueMeasure { get; set; }
        /// <summary>
        /// Maßeinheit der numerischen Messgröße
        ///
        /// BT-X-12-0
        /// </summary>
        public QuantityCodes? ValueMeasureUnitCode { get; set; }
        /// <summary>
        /// Wert der Eigenschaft
        /// </summary>
        public string Value { get; set; }
    }
}