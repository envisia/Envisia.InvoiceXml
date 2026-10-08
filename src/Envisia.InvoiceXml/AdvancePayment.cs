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

namespace Envisia.InvoiceXml
{
    /// <summary>
    /// Detailangaben zu einer Vorauszahlung im Extended-Profil (BG-X-45).
    /// </summary>
    public class AdvancePayment
    {
        /// <summary>
        /// Gezahlter Vorauszahlungsbetrag. Pflichtangabe innerhalb von BG-X-45.
        /// </summary>
        public decimal? PaidAmount { get; set; }

        /// <summary>
        /// Datum des Zahlungseingangs.
        /// </summary>
        public DateTime? FormattedReceivedDateTime { get; set; }

        /// <summary>
        /// In der Vorauszahlung enthaltene Steuern. Mindestens ein Eintrag ist erforderlich.
        /// </summary>
        public List<Tax> IncludedTradeTaxes { get; internal set; } = new List<Tax>();

        /// <summary>
        /// Referenz auf die zugehörige Vorauszahlungsrechnung.
        /// </summary>
        public InvoiceReferencedDocument InvoiceSpecifiedReferencedDocument { get; internal set; }


        /// <summary>
        /// Fügt eine enthaltene Steuer hinzu.
        /// </summary>
        /// <param name="tax">Enthaltene Steuer</param>
        /// <returns>Hinzugefügte Steuer</returns>
        public Tax AddIncludedTradeTax(Tax tax)
        {
            if (tax == null)
            {
                throw new ArgumentNullException(nameof(tax));
            }

            IncludedTradeTaxes.Add(tax);
            return tax;
        } // !AddIncludedTradeTax()


        /// <summary>
        /// Legt die Referenz auf die zugehörige Vorauszahlungsrechnung an oder aktualisiert sie.
        /// </summary>
        /// <param name="id">Rechnungsnummer</param>
        /// <param name="issueDateTime">Rechnungsdatum</param>
        /// <param name="typeCode">Rechnungstyp</param>
        public void SetInvoiceReferencedDocument(string id, DateTime? issueDateTime = null, InvoiceType? typeCode = null)
        {
            if (InvoiceSpecifiedReferencedDocument == null)
            {
                InvoiceSpecifiedReferencedDocument = new InvoiceReferencedDocument();
            }
            InvoiceSpecifiedReferencedDocument.ID = id;
            InvoiceSpecifiedReferencedDocument.IssueDateTime = issueDateTime;
            InvoiceSpecifiedReferencedDocument.TypeCode = typeCode;
        } // !SetInvoiceReferencedDocument()
    }
}
