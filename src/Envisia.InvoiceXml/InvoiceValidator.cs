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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envisia.InvoiceXml
{
    /// <summary>
    /// Validator for ZUGFeRD invoice descriptor.
    ///
    /// Currently limited to summarizing line totals
    ///
    /// Output syntax copied from Konik library (https://konik.io/)
    /// </summary>
    public class InvoiceValidator
    {
        public static void ValidateAndPrint(InvoiceDescriptor descriptor, ZUGFeRDVersion version, string filename = null)
        {
            ValidationResult validationResult = Validate(descriptor, version);

            if (!String.IsNullOrWhiteSpace(filename))
            {
                System.IO.File.WriteAllText(filename, string.Join("\n", validationResult.Messages));
            }

            foreach (string line in validationResult.Messages)
            {
                System.Console.WriteLine(line);
            }
        } // !ValidateAndPrint()

        public static ValidationResult Validate(InvoiceDescriptor descriptor, ZUGFeRDVersion version)
        {
            ValidationResult retval = new ValidationResult()
            {
                IsValid = true
            };

            if (descriptor == null)
            {
                retval.Messages.Add("Invalid invoice descriptor");
                retval.IsValid = false;
                return retval;
            }

            // line item summation
            retval.Messages.Add("Validating invoice monetary summation");
            retval.Messages.Add(String.Format("Starting recalculating line total from {0} items...", descriptor.GetTradeLineItems().Count));
            int lineCounter = 0;

            decimal lineTotal = 0m;
            foreach (TradeLineItem item in descriptor.GetTradeLineItems())
            {
                decimal total = decimal.Multiply(item.NetUnitPrice, item.BilledQuantity);

                // BT-146 is stated per price base quantity BT-149; an absent base quantity means 1.
                if (item.NetQuantity.HasValue)
                {
                    if (item.NetQuantity.Value > 0m)
                    {
                        total /= item.NetQuantity.Value;
                    }
                    else
                    {
                        retval.Messages.Add(String.Format("BT-149: Price base quantity for line item [{0}] must be greater than 0", item.Name));
                        retval.IsValid = false;
                    }
                }

                // BT-131 includes BG-28 line charges and excludes BG-27 line allowances.
                total -= item.GetSpecifiedTradeAllowances().Sum(allowance => allowance.ActualAmount);
                total += item.GetSpecifiedTradeCharges().Sum(charge => charge.ActualAmount);

                lineTotal += total;

                /*
                retval.Add(String.Format("==> {0}:", ++lineCounter));
                retval.Add(String.Format("Recalculating item: [{0}]", item.Name));
                retval.Add(String.Format("Line total formula: {0:0.0000} EUR (net price) x {1:0.0000} (quantity)", item.NetUnitPrice, item.BilledQuantity));

                retval.Add(String.Format("Recalculated item line total = {0:0.0000} EUR", total));
                retval.Add(String.Format("Recalculated item tax = {0:0.00} %", item.TaxPercent));
                retval.Add(String.Format("Current monetarySummation.lineTotal = {0:0.0000} EUR(the sum of all line totals)", lineTotal));
                */

                retval.Messages.Add(String.Format("{0};{1};{2}", ++lineCounter, item.Name, total));
            }

            retval.Messages.Add("==> DONE!");
            retval.Messages.Add("Finished recalculating monetarySummation.lineTotal...");
            retval.Messages.Add("Adding tax amounts from invoice allowance charge...");
            
            decimal chargeTotal = 0.0m;
            foreach (TradeCharge charge in descriptor.GetTradeCharges())
            {
                retval.Messages.Add(String.Format("==> added {0:0.00} to {1:0.00}%", charge.ActualAmount, charge.Tax.Percent));

                chargeTotal += charge.ActualAmount;
            }

            decimal allowanceTotal = 0.0m;
            foreach (TradeAllowance allowance in descriptor.GetTradeAllowances())
            {
                retval.Messages.Add(String.Format("==> subtracted {0:0.00} from {1:0.00}%", allowance.ActualAmount, allowance.Tax.Percent));

                allowanceTotal += allowance.ActualAmount;
            }

            retval.Messages.Add("Adding tax amounts from invoice service charge...");
            // TODO

            // TODO ausgeben: Recalculating tax basis for tax percentages: [Key{percentage=7.00, code=[VAT] Value added tax, category=[S] Standard rate}, Key{percentage=19.00, code=[VAT] Value added tax, category=[S] Standard rate}]
            retval.Messages.Add(String.Format("Recalculated tax basis = {0:0.0000}", lineTotal - allowanceTotal + chargeTotal));
            retval.Messages.Add("Calculating tax total...");

            decimal taxTotal = 0.0m;
            foreach (Tax tax in descriptor.GetApplicableTradeTaxes())
            {
                if (!tax.TypeCode.HasValue)
                {
                    retval.Messages.Add("Tax type code is required for every tax breakdown");
                    retval.IsValid = false;
                    continue;
                }
                if (tax.TypeCode != TaxTypes.VAT)
                {
                    continue;
                }

                // BR-CO-17 recalculates and rounds each VAT breakdown independently.
                decimal expectedTaxAmount = Math.Round(tax.BasisAmount * tax.Percent / 100m, 2, MidpointRounding.AwayFromZero);
                // BR-CO-14 derives BT-110 from declared BT-117 values, including tolerated BR-CO-17 deviations.
                taxTotal += tax.TaxAmount;
                retval.Messages.Add(String.Format("===> {0:0.0000} x {1:0.00}% = {2:0.00}", tax.BasisAmount, tax.Percent, expectedTaxAmount));

                // BR-DEC-20 limits BT-117 to two decimal places.
                if (tax.TaxAmount != Math.Round(tax.TaxAmount, 2, MidpointRounding.AwayFromZero))
                {
                    retval.Messages.Add(String.Format(
                        "BR-DEC-20: Declared tax amount [{0:0.0000}] has more than two decimal places",
                        tax.TaxAmount));
                    retval.IsValid = false;
                }

                // This validator is generally profile and output-format independent. Factur-X 1.08/1.09
                // FX-SCH-A-000052 accepts the inclusive 1.00 boundary, while current CEN/Peppol
                // BR-CO-17 uses a strict < 1 boundary. Preserve the inclusive Factur-X behavior used by Delphi.
                decimal taxDeviation = tax.TaxAmount - expectedTaxAmount;
                if (Math.Abs(taxDeviation) > 1m)
                {
                    retval.Messages.Add(String.Format(
                        "BR-CO-17: Berechneter Steuerbetrag ist[{0:0.0000}] aber vorhandener Steuerbetrag ist[{1:0.0000}] bei Bemessungsgrundlage[{2:0.0000}] und Steuersatz[{3:0.0000}]",
                        expectedTaxAmount, tax.TaxAmount, tax.BasisAmount, tax.Percent));
                    retval.IsValid = false;
                }
                else if (taxDeviation != 0m)
                {
                    retval.Messages.Add(String.Format(
                        "Note: Declared tax amount [{0:0.0000}] deviates by [{1:0.0000}] from [{2:0.0000}] but remains within the inclusive BR-CO-17 tolerance of one currency unit",
                        tax.TaxAmount, taxDeviation, expectedTaxAmount));
                }
            }

            // BR-CO-14 rounds the sum of declared BT-117 values to two decimal places.
            taxTotal = Math.Round(taxTotal, 2, MidpointRounding.AwayFromZero);

            decimal grandTotal = lineTotal - allowanceTotal + taxTotal + chargeTotal;
            decimal prepaid = descriptor.TotalPrepaidAmount.GetValueOrDefault();
            decimal rounding = descriptor.RoundingAmount.GetValueOrDefault();

            // BR-CO-16: BT-115 equals BT-112 minus BT-113 plus BT-114.
            decimal duePayable = grandTotal - prepaid + rounding;

            retval.Messages.Add(String.Format("Recalculated tax total = {0:0.00}", taxTotal));
            retval.Messages.Add(String.Format("Recalculated grand total = {0:0.0000} EUR(tax basis total + tax total)", grandTotal));
            retval.Messages.Add("Recalculating invoice monetary summation DONE!");
            retval.Messages.Add(String.Format("==> result: MonetarySummation[lineTotal = {0:0.0000} EUR, chargeTotal = {1:0.0000} EUR, allowanceTotal = {2:0.0000} EUR, taxBasisTotal = {3:0.0000} EUR, taxTotal = {4:0.0000} EUR, grandTotal = {5:0.0000} EUR, totalPrepaid = {6:0.0000} EUR, duePayable = {7:0.0000} EUR]",
                                     lineTotal,
                                     chargeTotal,
                                     allowanceTotal,
                                     lineTotal - allowanceTotal + chargeTotal, // tax basis total
                                     taxTotal,
                                     grandTotal,
                                     prepaid,
                                     duePayable
                                     ));


            decimal taxBasisTotal = descriptor.GetApplicableTradeTaxes()
                .Where(tax => tax.TypeCode == TaxTypes.VAT)
                .Sum(tax => tax.BasisAmount);
            decimal declaredAllowanceTotal = descriptor.AllowanceTotalAmount.GetValueOrDefault();
            decimal declaredChargeTotal = descriptor.ChargeTotalAmount.GetValueOrDefault();

            if (!descriptor.TaxTotalAmount.HasValue)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.taxTotal Message: Kein TaxTotalAmount vorhanden"));
                retval.IsValid = false;
            }
            else if (Math.Abs(taxTotal - descriptor.TaxTotalAmount.Value) < 0.01m)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.taxTotal Message: Berechneter Wert ist wie vorhanden:[{0:0.0000}]", taxTotal));
            }
            else
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.taxTotal Message: Berechneter Wert ist[{0:0.0000}] aber tatsächliche vorhander Wert ist[{1:0.0000}] | Actual value: {1:0.0000})", taxTotal, descriptor.TaxTotalAmount));
                retval.IsValid = false;
            }

            if (!descriptor.LineTotalAmount.HasValue)
            {
                retval.Messages.Add("trade.settlement.monetarySummation.lineTotal Message: Kein LineTotalAmount vorhanden");
                retval.IsValid = false;
            }
            else if (Math.Abs(lineTotal - descriptor.LineTotalAmount.Value) < 0.01m)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.lineTotal Message: Berechneter Wert ist wie vorhanden:[{0:0.0000}]", lineTotal));
            }
            else
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.lineTotal Message: Berechneter Wert ist[{0:0.0000}] aber tatsächliche vorhander Wert ist[{1:0.0000}] | Actual value: {1:0.0000})", lineTotal, descriptor.LineTotalAmount));
                retval.IsValid = false;
            }

            if (!descriptor.GrandTotalAmount.HasValue)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.grandTotal Message: Kein GrandTotalAmount vorhanden"));
                retval.IsValid = false;
            }
            else if (Math.Abs(grandTotal - descriptor.GrandTotalAmount.Value) < 0.01m)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.grandTotal Message: Berechneter Wert ist wie vorhanden:[{0:0.0000}]", grandTotal));
            }
            else
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.grandTotal Message: Berechneter Wert ist[{0:0.0000}] aber tatsächliche vorhander Wert ist[{1:0.0000}] | Actual value: {1:0.0000})", grandTotal, descriptor.GrandTotalAmount));
                retval.IsValid = false;
            }

            if (!descriptor.DuePayableAmount.HasValue)
            {
                retval.Messages.Add("trade.settlement.monetarySummation.duePayable Message: Kein DuePayableAmount vorhanden");
                retval.IsValid = false;
            }
            else if (descriptor.GrandTotalAmount.HasValue)
            {
                decimal expectedDuePayable = descriptor.GrandTotalAmount.Value - prepaid + rounding;
                if (Math.Abs(expectedDuePayable - descriptor.DuePayableAmount.Value) < 0.01m)
                {
                    retval.Messages.Add(String.Format("trade.settlement.monetarySummation.duePayable Message: Berechneter Wert ist wie vorhanden:[{0:0.0000}]", expectedDuePayable));
                }
                else
                {
                    retval.Messages.Add(String.Format("trade.settlement.monetarySummation.duePayable Message: Berechneter Wert ist[{0:0.0000}] aber tatsächlicher vorhandener Wert ist[{1:0.0000}] | Actual value: {1:0.0000})", expectedDuePayable, descriptor.DuePayableAmount.Value));
                    retval.IsValid = false;
                }
            }

            // The sum of VAT category taxable amounts (BT-116) must equal the invoice total amount without VAT (BT-109).
            if (!descriptor.TaxBasisAmount.HasValue)
            {
                retval.Messages.Add("trade.settlement.monetarySummation.taxBasisTotal Message: Kein TaxBasisAmount vorhanden");
                retval.IsValid = false;
            }
            else if (Math.Abs(taxBasisTotal - descriptor.TaxBasisAmount.Value) < 0.01m)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.taxBasisTotal Message: Berechneter Wert ist wie vorhanden:[{0:0.0000}]", taxBasisTotal));
            }
            else
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.taxBasisTotal Message: Berechneter Wert ist[{0:0.0000}] aber tatsächlicher vorhandener Wert ist[{1:0.0000}] | Actual value: {1:0.0000})", taxBasisTotal, descriptor.TaxBasisAmount.Value));
                retval.IsValid = false;
            }

            // BR-CO-11/12 compare declared BT-107/108 with the sums of individual document allowances/charges.
            // Optional declared totals are treated as zero when absent.
            if (Math.Abs(allowanceTotal - declaredAllowanceTotal) < 0.01m)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.allowanceTotal  Message: Berechneter Wert ist wie vorhanden:[{0:0.0000}]", declaredAllowanceTotal));
            }
            else
            {
                retval.Messages.Add(String.Format("BR-CO-11: trade.settlement.monetarySummation.allowanceTotal Message: Berechneter Wert ist[{0:0.0000}] aber tatsächlich vorhandener Wert ist[{1:0.0000}]", allowanceTotal, declaredAllowanceTotal));
                retval.IsValid = false;
            }

            if (Math.Abs(chargeTotal - declaredChargeTotal) < 0.01m)
            {
                retval.Messages.Add(String.Format("trade.settlement.monetarySummation.chargeTotal  Message: Berechneter Wert ist wie vorhanden:[{0:0.0000}]", declaredChargeTotal));
            }
            else
            {
                retval.Messages.Add(String.Format("BR-CO-12: trade.settlement.monetarySummation.chargeTotal Message: Berechneter Wert ist[{0:0.0000}] aber tatsächlich vorhandener Wert ist[{1:0.0000}]", chargeTotal, declaredChargeTotal));
                retval.IsValid = false;
            }

            // BR-CO-13 requires BT-109 = BT-106 - BT-107 + BT-108.
            if (descriptor.LineTotalAmount.HasValue && descriptor.TaxBasisAmount.HasValue)
            {
                decimal expectedTaxBasis = descriptor.LineTotalAmount.Value - declaredAllowanceTotal + declaredChargeTotal;
                if (Math.Abs(expectedTaxBasis - descriptor.TaxBasisAmount.Value) < 0.01m)
                {
                    retval.Messages.Add(String.Format("BR-CO-13: Tax basis from BT-106 - BT-107 + BT-108 matches [{0:0.0000}]", expectedTaxBasis));
                }
                else
                {
                    retval.Messages.Add(String.Format("BR-CO-13: Tax basis from BT-106 - BT-107 + BT-108 is [{0:0.0000}] but declared BT-109 is [{1:0.0000}]", expectedTaxBasis, descriptor.TaxBasisAmount.Value));
                    retval.IsValid = false;
                }
            }

            // version-specific validation
            ValidationResult versionSpecificResults;
            switch (version)
            {
                case ZUGFeRDVersion.Version1:
                {
                    versionSpecificResults = _ValidateAccordingToVersion1(descriptor);
                    break;
                }
                default:
                {
                    versionSpecificResults = new ValidationResult { IsValid = true };
                    break;
                }
            }

            retval.IsValid = retval.IsValid && versionSpecificResults.IsValid;
            retval.Messages.AddRange(versionSpecificResults.Messages);

            return retval;
        } // !Validate()

        private static ValidationResult _ValidateAccordingToVersion1(InvoiceDescriptor descriptor)
        {
            ValidationResult retval = new ValidationResult()
            {
                IsValid = true
            };

            if (!EnumExtensions.In<GlobalIDSchemeIdentifiers>(descriptor.Buyer?.GlobalID?.SchemeID, GlobalIDSchemeIdentifiers.Swift, GlobalIDSchemeIdentifiers.DUNS, GlobalIDSchemeIdentifiers.GLN, GlobalIDSchemeIdentifiers.EAN, GlobalIDSchemeIdentifiers.Odette))
            {
                retval.IsValid = false;
                retval.Messages.Add($"Global identifier scheme {descriptor.Buyer?.GlobalID?.SchemeID} is not supported for buyers in ZUGFeRD 1.0");
            }

            if (!EnumExtensions.In<GlobalIDSchemeIdentifiers>(descriptor.Seller?.GlobalID?.SchemeID, GlobalIDSchemeIdentifiers.Swift, GlobalIDSchemeIdentifiers.DUNS, GlobalIDSchemeIdentifiers.GLN, GlobalIDSchemeIdentifiers.EAN, GlobalIDSchemeIdentifiers.Odette))
            {
                retval.IsValid = false;
                retval.Messages.Add($"Global identifier scheme {descriptor.Buyer?.GlobalID?.SchemeID} is not supported for sellers in ZUGFeRD 1.0");
            }

            if (!EnumExtensions.In<GlobalIDSchemeIdentifiers>(descriptor.ShipFrom?.GlobalID?.SchemeID, GlobalIDSchemeIdentifiers.Swift, GlobalIDSchemeIdentifiers.DUNS, GlobalIDSchemeIdentifiers.GLN, GlobalIDSchemeIdentifiers.EAN, GlobalIDSchemeIdentifiers.Odette))
            {
                retval.IsValid = false;
                retval.Messages.Add($"Global identifier scheme {descriptor.Buyer?.GlobalID?.SchemeID} is not supported for senders (ShipFrom) in ZUGFeRD 1.0");
            }

            if (!EnumExtensions.In<GlobalIDSchemeIdentifiers>(descriptor.ShipTo?.GlobalID?.SchemeID, GlobalIDSchemeIdentifiers.Swift, GlobalIDSchemeIdentifiers.DUNS, GlobalIDSchemeIdentifiers.GLN, GlobalIDSchemeIdentifiers.EAN, GlobalIDSchemeIdentifiers.Odette))
            {
                retval.IsValid = false;
                retval.Messages.Add($"Global identifier scheme {descriptor.Buyer?.GlobalID?.SchemeID} is not supported for recipients (ShipTo) in ZUGFeRD 1.0");
            }

            return retval;
        } // !_ValidateAccordingToVersion1()
    }
}
