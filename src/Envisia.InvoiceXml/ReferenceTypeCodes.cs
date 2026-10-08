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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envisia.InvoiceXml
{
    /// <summary>
    /// UNTDID 1153 — Reference code qualifier
    /// Version 24A, complete list as allowed by EN 16931 (code list v16/v17) for BT-18-1 and BT-128-1
    ///
    /// Sources:
    /// https://service.unece.org/trade/untdid/d24a/tred/tred1153.htm
    /// https://service.unece.org/trade/untdid/d21a/tred/tred1153.htm
    /// and
    /// https://www.xrepository.de/details/urn:xoev-de:kosit:codeliste:untdid.1153_3#version
    /// </summary>
    public enum ReferenceTypeCodes
    {
        /// <summary>
        /// Auftragsbestätigungsnummer
        /// </summary>
        AAA,

        /// <summary>
        /// Proforma-Rechnung
        /// </summary>
        AAB,

        /// <summary>
        /// Documentary credit identifier
        ///
        /// [1172] Reference number to identify a documentary credit.
        /// </summary>
        AAC,

        /// <summary>
        /// Contract document addendum identifier
        ///
        /// [1318] Reference number to identify an addendum to a
        /// contract.
        /// </summary>
        AAD,

        /// <summary>
        /// Goods declaration number
        ///
        /// Reference number assigned to a goods declaration.
        /// </summary>
        AAE,

        /// <summary>
        /// Debit card number
        ///
        /// A reference number identifying a debit card.
        /// </summary>
        AAF,

        /// <summary>
        /// Angebotsnummer
        /// </summary>
        AAG,

        /// <summary>
        /// Bank's batch interbank transaction reference number
        ///
        /// Reference number allocated by the bank to a batch of
        /// different underlying interbank transactions.
        /// </summary>
        AAH,

        /// <summary>
        /// Bank's individual interbank transaction reference number
        ///
        /// Reference number allocated by the bank to one specific
        /// interbank transaction.
        /// </summary>
        AAI,

        /// <summary>
        /// Lieferauftragsnummer
        /// </summary>
        AAJ,

        /// <summary>
        /// Despatch advice number
        ///
        /// [1035] Reference number assigned by issuing party to a
        /// despatch advice.
        /// </summary>
        AAK,

        /// <summary>
        /// Zeichnungsnummer
        /// </summary>
        AAL,

        /// <summary>
        /// Frachtbriefnummer
        /// </summary>
        AAM,

        /// <summary>
        /// Delivery schedule number
        ///
        /// Reference number assigned by buyer to a delivery schedule.
        /// </summary>
        AAN,

        /// <summary>
        /// Consignment identifier, consignee assigned
        ///
        /// [1362] Reference number assigned by the consignee to
        /// identify a particular consignment.
        /// </summary>
        AAO,

        /// <summary>
        /// Partial shipment identifier
        ///
        /// [1310] Identifier of a shipment which is part of an order.
        /// </summary>
        AAP,

        /// <summary>
        /// Transport equipment identifier
        ///
        /// [8260] To identify a piece if transport equipment e.g.
        /// container or unit load device.
        /// </summary>
        AAQ,

        /// <summary>
        /// Municipality assigned business registry number
        ///
        /// A reference number assigned by a municipality to identify a
        /// business.
        /// </summary>
        AAR,

        /// <summary>
        /// Transportdokumenten-Nummer
        ///
        /// Referenz zu einem Transportdokument, vergeben vom Frachtführer oder seinem Agenten. (z.B.
        /// Paketdienst-Zustell-Nr.)
        /// </summary>
        AAS,

        /// <summary>
        /// Master label number
        ///
        /// Identifies the master label number of any package type.
        /// </summary>
        AAT,

        /// <summary>
        /// Despatch note document identifier
        ///
        /// [1128] Reference number to identify a Despatch Note.
        /// </summary>
        AAU,

        /// <summary>
        /// Enquiry number
        ///
        /// Reference number assigned to an enquiry.
        /// </summary>
        AAV,

        /// <summary>
        /// Docket number
        ///
        /// A reference number identifying the docket.
        /// </summary>
        AAW,

        /// <summary>
        /// Civil action number
        ///
        /// A reference number identifying the civil action.
        /// </summary>
        AAX,

        /// <summary>
        /// Carrier's agent reference number
        ///
        /// Reference number assigned by the carriers agent to a
        /// transaction.
        /// </summary>
        AAY,

        /// <summary>
        /// Standard Carrier Alpha Code (SCAC) number
        ///
        /// For maritime shipments, this code qualifies a Standard Alpha
        /// Carrier Code (SCAC) as issued by the United Stated National
        /// Motor Traffic Association Inc.
        /// </summary>
        AAZ,

        /// <summary>
        /// Customs valuation decision number
        ///
        /// Reference by an importing party to a previous decision made
        /// by a Customs administration regarding the valuation of
        /// goods.
        /// </summary>
        ABA,

        /// <summary>
        /// End use authorization number
        ///
        /// Reference issued by a Customs administration authorizing a
        /// preferential rate of duty if a product is used for a
        /// specified purpose, see: 1001 = 990.
        /// </summary>
        ABB,

        /// <summary>
        /// Anti-dumping case number
        ///
        /// Reference issued by a Customs administration pertaining to a
        /// past or current investigation of goods "dumped" at a price
        /// lower than the exporter's domestic market price.
        /// </summary>
        ABC,

        /// <summary>
        /// Customs tariff number
        ///
        /// (7357) Code number of the goods in accordance with the
        /// tariff nomenclature system of classification in use where
        /// the Customs declaration is made.
        /// </summary>
        ABD,

        /// <summary>
        /// Declarant's reference number
        ///
        /// Unique reference number assigned to a document or a message
        /// by the declarant for identification purposes.
        /// </summary>
        ABE,

        /// <summary>
        /// Repair estimate number
        ///
        /// A number identifying a repair estimate.
        /// </summary>
        ABF,

        /// <summary>
        /// Customs decision request number
        ///
        /// Reference issued by Customs pertaining to a pending tariff
        /// classification decision requested by an importer or agent.
        /// </summary>
        ABG,

        /// <summary>
        /// Sub-house bill of lading number
        ///
        /// Reference assigned to a sub-house bill of lading.
        /// </summary>
        ABH,

        /// <summary>
        /// Tax payment identifier
        ///
        /// [1168] Reference number identifying a payment of a duty or
        /// tax e.g. under a transit procedure.
        /// </summary>
        ABI,

        /// <summary>
        /// Quota number
        ///
        /// Reference number allocated by a government authority to
        /// identify a quota.
        /// </summary>
        ABJ,

        /// <summary>
        /// Transit (onward carriage) guarantee (bond) number
        ///
        /// Reference number to identify the guarantee or security
        /// provided for Customs transit operation (CCC).
        /// </summary>
        ABK,

        /// <summary>
        /// Customs guarantee number
        ///
        /// Reference assigned to a Customs guarantee.
        /// </summary>
        ABL,

        /// <summary>
        /// Replacing part number
        ///
        /// New part number which replaces the existing part number.
        /// </summary>
        ABM,

        /// <summary>
        /// Seller's catalogue number
        ///
        /// Identification number assigned to a seller's catalogue.
        /// </summary>
        ABN,

        /// <summary>
        /// Originator's reference
        ///
        /// A unique reference assigned by the originator.
        /// </summary>
        ABO,

        /// <summary>
        /// Declarant's Customs identity number
        /// 
        /// Reference to the party whose posted bond or security is
        /// being declared in order to accept responsibility for a
        /// goods declaration and the applicable duties and taxes.
        /// </summary>
        ABP,

        /// <summary>
        /// Importer reference number
        ///
        /// Reference number assigned by the importer to identify a
        /// particular shipment for his own purposes.
        /// </summary>
        ABQ,

        /// <summary>
        /// Export clearance instruction reference number
        ///
        /// Reference number of the clearance instructions given by the
        /// consignor through different means.
        /// </summary>
        ABR,

        /// <summary>
        /// Import clearance instruction reference number
        ///
        /// Reference number of the import clearance instructions given
        /// by the consignor/consignee through different means.
        /// </summary>
        ABS,

        /// <summary>
        /// Zollerklärungsnummer
        /// </summary>
        ABT,

        /// <summary>
        /// Article number
        ///
        /// A number that identifies an article.
        /// </summary>
        ABU,

        /// <summary>
        /// Intra-plant routing
        ///
        /// To define routing within a plant.
        /// </summary>
        ABV,

        /// <summary>
        /// Stock keeping unit number
        ///
        /// A number that identifies the stock keeping unit.
        /// </summary>
        ABW,

        /// <summary>
        /// Text Element Identifier deletion reference
        ///
        /// The reference used within a given TEI (Text Element
        /// Identifier) which is to be deleted.
        /// </summary>
        ABX,

        /// <summary>
        /// Allotment identification (Air)
        ///
        /// Reference assigned to guarantied capacity on one or more
        /// specific flights on specific date(s) to third parties as
        /// agents and other airlines.
        /// </summary>
        ABY,

        /// <summary>
        /// Vehicle licence number
        ///
        /// Number of the licence issued for a vehicle by an agency of government.
        /// </summary>
        ABZ,

        /// <summary>
        /// Air cargo transfer manifest
        ///
        /// A number assigned to an air cargo list of goods to be
        /// transferred.
        /// </summary>
        AC,

        /// <summary>
        /// Cargo acceptance order reference number
        ///
        /// Reference assigned to the cargo acceptance order.
        /// </summary>
        ACA,

        /// <summary>
        /// US government agency number
        ///
        /// A number that identifies a United States Government agency.
        /// </summary>
        ACB,

        /// <summary>
        /// Shipping unit identification
        ///
        /// Identifying marks on the outermost unit that is used to
        /// transport merchandise.
        /// </summary>
        ACC,

        /// <summary>
        /// Additional reference number
        ///
        /// [1010] Reference number provided in addition to another
        /// given reference.
        /// </summary>
        ACD,

        /// <summary>
        /// Related document number
        ///
        /// Reference number identifying a related document.
        /// </summary>
        ACE,

        /// <summary>
        /// Addressee reference
        ///
        /// A reference number of an addressee.
        /// </summary>
        ACF,

        /// <summary>
        /// ATA carnet number
        ///
        /// Reference number assigned to an ATA carnet.
        /// </summary>
        ACG,

        /// <summary>
        /// Packaging unit identification
        ///
        /// Identifying marks on packing units.
        /// </summary>
        ACH,

        /// <summary>
        /// Outerpackaging unit identification
        ///
        /// Identifying marks on packing units contained within an
        /// outermost shipping unit.
        /// </summary>
        ACI,

        /// <summary>
        /// Customer material specification number
        ///
        /// Number for a material specification given by customer.
        /// </summary>
        ACJ,

        /// <summary>
        /// Bank reference
        ///
        /// Cross reference issued by financial institution.
        /// </summary>
        ACK,

        /// <summary>
        /// Principal reference number
        ///
        /// A number that identifies the principal reference.
        /// </summary>
        ACL,

        /// <summary>
        /// Collection advice document identifier
        ///
        /// [1030] Reference number to identify a collection advice
        /// document.
        /// </summary>
        ACN,

        /// <summary>
        /// Iron charge number
        ///
        /// Number attributed to the iron charge for the production of
        /// steel products.
        /// </summary>
        ACO,

        /// <summary>
        /// Hot roll number
        ///
        /// Number attributed to a hot roll coil.
        /// </summary>
        ACP,

        /// <summary>
        /// Cold roll number
        ///
        /// Number attributed to a cold roll coil.
        /// </summary>
        ACQ,

        /// <summary>
        /// Railway wagon number
        ///
        /// (8260) Registered identification initials and numbers of
        /// railway wagon. Synonym: Rail car number.
        /// </summary>
        ACR,

        /// <summary>
        /// Unique claims reference number of the sender
        ///
        /// A number that identifies the unique claims reference of the
        /// sender.
        /// </summary>
        ACT,

        /// <summary>
        /// Loss/event number
        ///
        /// To reference to the unique number that is assigned to each
        /// major loss hitting the reinsurance industry.
        /// </summary>
        ACU,

        /// <summary>
        /// Estimate order reference number
        ///
        /// Reference number assigned by the ordering party of the
        /// estimate order.
        /// </summary>
        ACV,

        /// <summary>
        /// Reference number to previous message
        ///
        /// Reference number assigned to the message which was
        /// previously issued (e.g. in the case of a cancellation, the
        /// primary reference of the message to be cancelled will be
        /// quoted in this element).
        /// </summary>
        ACW,

        /// <summary>
        /// Banker's acceptance
        ///
        /// Reference number for banker's acceptance issued by the
        /// accepting financial institution.
        /// </summary>
        ACX,

        /// <summary>
        /// Duty memo number
        ///
        /// Reference number assigned by customs to a duty memo.
        /// </summary>
        ACY,

        /// <summary>
        /// Equipment transport charge number
        ///
        /// Reference assigned to a specific equipment transportation
        /// charge.
        /// </summary>
        ACZ,

        /// <summary>
        /// Buyer's item number
        ///
        /// [7304] Reference number assigned by the buyer to an item.
        /// </summary>
        ADA,

        /// <summary>
        /// Matured certificate of deposit
        ///
        /// Reference number for certificate of deposit allocated by
        /// issuing financial institution.
        /// </summary>
        ADB,

        /// <summary>
        /// Loan
        ///
        /// Reference number for loan allocated by lending financial
        /// institution.
        /// </summary>
        ADC,

        /// <summary>
        /// Analysis number/test number
        ///
        /// Number given to a specific analysis or test operation.
        /// </summary>
        ADD,

        /// <summary>
        /// Account number
        ///
        /// Identification number of an account.
        /// </summary>
        ADE,

        /// <summary>
        /// Treaty number
        ///
        /// A number that identifies a treaty.
        /// </summary>
        ADF,

        /// <summary>
        /// Catastrophe number
        ///
        /// A number that identifies a catastrophe.
        /// </summary>
        ADG,

        /// <summary>
        /// Bureau signing (statement reference)
        ///
        /// A statement reference that identifies a bureau signing.
        /// </summary>
        ADI,

        /// <summary>
        /// Company / syndicate reference 1
        ///
        /// First reference of a company/syndicate.
        /// </summary>
        ADJ,

        /// <summary>
        /// Company / syndicate reference 2
        ///
        /// Second reference of a company/syndicate.
        /// </summary>
        ADK,

        /// <summary>
        /// Ordering customer consignment reference number
        ///
        /// Reference number assigned to the consignment by the ordering
        /// customer.
        /// </summary>
        ADL,

        /// <summary>
        /// Shipowner's authorization number
        ///
        /// Reference number assigned by the shipowner as an
        /// authorization number to transport certain goods (such as
        /// hazardous goods, cool or reefer goods).
        /// </summary>
        ADM,

        /// <summary>
        /// Inland transport order number
        ///
        /// Reference number assigned by the principal to the transport
        /// order for inland carriage.
        /// </summary>
        ADN,

        /// <summary>
        /// Container work order reference number
        ///
        /// Reference number assigned by the principal to the work order
        /// for a (set of) container(s).
        /// </summary>
        ADO,

        /// <summary>
        /// Statement number
        ///
        /// A reference number identifying a statement.
        /// </summary>
        ADP,

        /// <summary>
        /// Unique market reference
        ///
        /// A number that identifies a unique market.
        /// </summary>
        ADQ,

        /// <summary>
        /// Group accounting
        ///
        /// A number that identifies group accounting.
        /// </summary>
        ADT,

        /// <summary>
        /// Broker reference 1
        ///
        /// First reference of a broker.
        /// </summary>
        ADU,

        /// <summary>
        /// Broker reference 2
        ///
        /// Second reference of a broker.
        /// </summary>
        ADV,

        /// <summary>
        /// Lloyd's claims office reference
        ///
        /// A number that identifies a Lloyd's claims office.
        /// </summary>
        ADW,

        /// <summary>
        /// Secure delivery terms and conditions agreement reference
        ///
        /// A reference to a secure delivery terms and conditions
        /// agreement. A secured delivery agreement is an agreement
        /// containing terms and conditions to secure deliveries in case
        /// of failure in the production or logistics process of the
        /// supplier.
        /// </summary>
        ADX,

        /// <summary>
        /// Report number
        ///
        /// Reference to a report to Customs by a carrier at the point
        /// of entry, encompassing both conveyance and consignment
        /// information.
        /// </summary>
        ADY,

        /// <summary>
        /// Trader account number
        ///
        /// Number assigned by a Customs authority which uniquely
        /// identifies a trader (i.e. importer, exporter or declarant)
        /// for Customs purposes.
        /// </summary>
        ADZ,

        /// <summary>
        /// Authorization for expense (AFE) number
        ///
        /// A number that identifies an authorization for expense (AFE).
        /// </summary>
        AE,

        /// <summary>
        /// Government agency reference number
        ///
        /// Coded reference number that pertains to the business of a
        /// government agency.
        /// </summary>
        AEA,

        /// <summary>
        /// Assembly number
        ///
        /// A number that identifies an assembly.
        /// </summary>
        AEB,

        /// <summary>
        /// Symbol number
        ///
        /// A number that identifies a symbol.
        /// </summary>
        AEC,

        /// <summary>
        /// Commodity number
        ///
        /// A number that identifies a commodity.
        /// </summary>
        AED,

        /// <summary>
        /// Eur 1 certificate number
        ///
        /// Reference number assigned to a Eur 1 certificate.
        /// </summary>
        AEE,

        /// <summary>
        /// Customer process specification number
        ///
        /// Retrieval number for a process specification defined by
        /// customer.
        /// </summary>
        AEF,

        /// <summary>
        /// Customer specification number
        ///
        /// Retrieval number for a specification defined by customer.
        /// </summary>
        AEG,

        /// <summary>
        /// Applicable instructions or standards
        ///
        /// Instructions or standards applicable for the whole message
        /// or a message line item. These instructions or standards may
        /// be published by a neutral organization or authority or
        /// another party concerned.
        /// </summary>
        AEH,

        /// <summary>
        /// Registration number of previous Customs declaration
        ///
        /// Registration number of the Customs declaration lodged for
        /// the previous Customs procedure.
        /// </summary>
        AEI,

        /// <summary>
        /// Post-entry reference
        ///
        /// Reference to a message related to a post-entry.
        /// </summary>
        AEJ,

        /// <summary>
        /// Payment order number
        ///
        /// A number that identifies a payment order.
        /// </summary>
        AEK,

        /// <summary>
        /// Delivery number (transport)
        ///
        /// Reference number by which a haulier/carrier will announce
        /// himself at the container terminal or depot when delivering
        /// equipment.
        /// </summary>
        AEL,

        /// <summary>
        /// Transport route
        ///
        /// A predefined and identified sequence of points where goods
        /// are collected, agreed between partners, e.g. the party in
        /// charge of organizing the transport and the parties where
        /// goods will be collected. The same collecting points may be
        /// included in different transport routes, but in a different
        /// sequence.
        /// </summary>
        AEM,

        /// <summary>
        /// Customer's unit inventory number
        ///
        /// Number assigned by customer to a unique unit for inventory
        /// purposes.
        /// </summary>
        AEN,

        /// <summary>
        /// Product reservation number
        ///
        /// Number assigned by seller to identify reservation of
        /// specified products.
        /// </summary>
        AEO,

        /// <summary>
        /// Project number
        ///
        /// Reference number assigned to a project.
        /// </summary>
        AEP,

        /// <summary>
        /// Drawing list number
        ///
        /// Reference number identifying a drawing list.
        /// </summary>
        AEQ,

        /// <summary>
        /// Projektspezifikationsnummer
        /// </summary>
        AER,

        /// <summary>
        /// Primary reference
        ///
        /// A number that identifies the primary reference.
        /// </summary>
        AES,

        /// <summary>
        /// Request for cancellation number
        ///
        /// A number that identifies a request for cancellation.
        /// </summary>
        AET,

        /// <summary>
        /// Supplier's control number
        ///
        /// Reference to a file regarding a control of the supplier
        /// carried out on departure of the goods.
        /// </summary>
        AEU,

        /// <summary>
        /// Shipping note number
        ///
        /// [1123] Reference number assigned to a shipping note.
        /// </summary>
        AEV,

        /// <summary>
        /// Empty container bill number
        ///
        /// Reference number assigned to an empty container bill, see:
        /// 1001 = 708.
        /// </summary>
        AEW,

        /// <summary>
        /// Non-negotiable maritime transport document number
        ///
        /// Reference number assigned to a sea waybill, see: 1001 = 712.
        /// </summary>
        AEX,

        /// <summary>
        /// Substitute air waybill number
        ///
        /// Reference number assigned to a substitute air waybill, see:
        /// 1001 = 743.
        /// </summary>
        AEY,

        /// <summary>
        /// Despatch note (post parcels) number
        ///
        /// (1128) Reference number assigned to a despatch note (post
        /// parcels), see: 1001 = 750.
        /// </summary>
        AEZ,

        /// <summary>
        /// Airlines flight identification number
        ///
        /// (8028) Identification of a commercial flight by carrier code
        /// and number as assigned by the airline (IATA).
        /// </summary>
        AF,

        /// <summary>
        /// Through bill of lading number
        ///
        /// Reference number assigned to a through bill of lading, see:
        /// 1001 = 761.
        /// </summary>
        AFA,

        /// <summary>
        /// Cargo manifest number
        ///
        /// [1037] Reference number assigned to a cargo manifest.
        /// </summary>
        AFB,

        /// <summary>
        /// Bordereau number
        ///
        /// Reference number assigned to a bordereau, see: 1001 = 787.
        /// </summary>
        AFC,

        /// <summary>
        /// Customs item number
        ///
        /// Number (1496 in CST) assigned by the declarant to an item.
        /// </summary>
        AFD,

        /// <summary>
        /// Export Control Commodity number (ECCN)
        ///
        /// Reference number to relevant item within Commodity Control
        /// List covering actual products change functionality.
        /// </summary>
        AFE,

        /// <summary>
        /// Marking/label reference
        ///
        /// Reference where marking/label information derives from.
        /// </summary>
        AFF,

        /// <summary>
        /// Tariff number
        ///
        /// A number that identifies a tariff.
        /// </summary>
        AFG,

        /// <summary>
        /// Replenishment purchase order number
        ///
        /// Purchase order number specified by the buyer for the
        /// assignment to vendor's replenishment orders in a vendor
        /// managed inventory program.
        /// </summary>
        AFH,

        /// <summary>
        /// Immediate transportation no. for in bond movement
        ///
        /// A number that identifies immediate transportation for in
        /// bond movement.
        /// </summary>
        AFI,

        /// <summary>
        /// Transportation exportation no. for in bond movement
        ///
        /// A number that identifies the transportation exportation
        /// number for an in bond movement.
        /// </summary>
        AFJ,

        /// <summary>
        /// Immediate exportation no. for in bond movement
        ///
        /// A number that identifies the immediate exportation number
        /// for an in bond movement.
        /// </summary>
        AFK,

        /// <summary>
        /// Associated invoices
        ///
        /// A number that identifies associated invoices.
        /// </summary>
        AFL,

        /// <summary>
        /// Secondary Customs reference
        ///
        /// A number that identifies the secondary customs reference.
        /// </summary>
        AFM,

        /// <summary>
        /// Account party's reference
        ///
        /// Reference of the account party.
        /// </summary>
        AFN,

        /// <summary>
        /// Beneficiary's reference
        ///
        /// Reference of the beneficiary.
        /// </summary>
        AFO,

        /// <summary>
        /// Second beneficiary's reference
        ///
        /// Reference of the second beneficiary.
        /// </summary>
        AFP,

        /// <summary>
        /// Applicant's bank reference
        ///
        /// Reference number of the applicant's bank.
        /// </summary>
        AFQ,

        /// <summary>
        /// Issuing bank's reference
        ///
        /// Reference number of the issuing bank.
        /// </summary>
        AFR,

        /// <summary>
        /// Beneficiary's bank reference
        ///
        /// Reference number of the beneficiary's bank.
        /// </summary>
        AFS,

        /// <summary>
        /// Direct payment valuation number
        ///
        /// Reference number assigned to a direct payment valuation.
        /// </summary>
        AFT,

        /// <summary>
        /// Direct payment valuation request number
        ///
        /// Reference number assigned to a direct payment valuation
        /// request.
        /// </summary>
        AFU,

        /// <summary>
        /// Quantity valuation number
        ///
        /// Reference number assigned to a quantity valuation.
        /// </summary>
        AFV,

        /// <summary>
        /// Quantity valuation request number
        ///
        /// Reference number assigned to a quantity valuation request.
        /// </summary>
        AFW,

        /// <summary>
        /// Bill of quantities number
        ///
        /// Reference number assigned to a bill of quantities.
        /// </summary>
        AFX,

        /// <summary>
        /// Payment valuation number
        ///
        /// Reference number assigned to a payment valuation.
        /// </summary>
        AFY,

        /// <summary>
        /// Situation number
        ///
        /// Common reference number given to documents concerning a
        /// determined period of works.
        /// </summary>
        AFZ,

        /// <summary>
        /// Agreement to pay number
        ///
        /// A number that identifies an agreement to pay.
        /// </summary>
        AGA,

        /// <summary>
        /// Contract party reference number
        ///
        /// Reference number assigned to a party for a particular
        /// contract.
        /// </summary>
        AGB,

        /// <summary>
        /// Account party's bank reference
        ///
        /// Reference number of the account party's bank.
        /// </summary>
        AGC,

        /// <summary>
        /// Agent's bank reference
        ///
        /// Reference number issued by the agent's bank.
        /// </summary>
        AGD,

        /// <summary>
        /// Agent's reference
        ///
        /// Reference number of the agent.
        /// </summary>
        AGE,

        /// <summary>
        /// Applicant's reference
        ///
        /// Reference number of the applicant.
        /// </summary>
        AGF,

        /// <summary>
        /// Reklamationsummer
        /// </summary>
        AGG,

        /// <summary>
        /// Credit rating agency's reference number
        ///
        /// Reference number assigned by a credit rating agency to a
        /// debtor.
        /// </summary>
        AGH,

        /// <summary>
        /// Request number
        ///
        /// The reference number of a request.
        /// </summary>
        AGI,

        /// <summary>
        /// Single transaction sequence number
        ///
        /// A number that identifies a single transaction sequence.
        /// </summary>
        AGJ,

        /// <summary>
        /// Application reference number
        ///
        /// A number that identifies an application reference.
        /// </summary>
        AGK,

        /// <summary>
        /// Delivery verification certificate
        ///
        /// Formal identification of delivery verification certificate
        /// which is a formal document from Customs etc. confirming that
        /// physical goods have been delivered. It may be needed to
        /// support a tax reclaim based on an invoice.
        /// </summary>
        AGL,

        /// <summary>
        /// Number of temporary importation document
        ///
        /// Number assigned by customs to identify consignment in
        /// transit.
        /// </summary>
        AGM,

        /// <summary>
        /// Reference number quoted on statement
        ///
        /// Reference number quoted on the statement sent to the
        /// beneficiary for information purposes.
        /// </summary>
        AGN,

        /// <summary>
        /// Sender's reference to the original message
        ///
        /// The reference provided by the sender of the original
        /// message.
        /// </summary>
        AGO,

        /// <summary>
        /// Company issued equipment ID
        ///
        /// Owner/operator, non-government issued equipment reference
        /// number.
        /// </summary>
        AGP,

        /// <summary>
        /// Domestic flight number
        ///
        /// Airline flight number assigned to a flight originating and
        /// terminating within the same country.
        /// </summary>
        AGQ,

        /// <summary>
        /// International flight number
        ///
        /// Airline flight number assigned to a flight originating and
        /// terminating across national borders.
        /// </summary>
        AGR,

        /// <summary>
        /// Employer identification number of service bureau
        ///
        /// Reference number assigned by a service/processing bureau to
        /// an employer.
        /// </summary>
        AGS,

        /// <summary>
        /// Service group identification number
        ///
        /// Identification used for a group of services.
        /// </summary>
        AGT,

        /// <summary>
        /// Member number
        ///
        /// Reference number assigned to a person as a member of a group
        /// of persons or a service scheme.
        /// </summary>
        AGU,

        /// <summary>
        /// Previous member number
        ///
        /// Reference number previously assigned to a member.
        /// </summary>
        AGV,

        /// <summary>
        /// Scheme/plan number
        ///
        /// Reference number assigned to a service scheme or plan.
        /// </summary>
        AGW,

        /// <summary>
        /// Previous scheme/plan number
        ///
        /// Reference number previously assigned to a service scheme or
        /// plan.
        /// </summary>
        AGX,

        /// <summary>
        /// Receiving party's member identification
        ///
        /// Identification used by the receiving party for a member of a
        /// service scheme or group of persons.
        /// </summary>
        AGY,

        /// <summary>
        /// Payroll number
        ///
        /// Reference number assigned to the payroll of an organisation.
        /// </summary>
        AGZ,

        /// <summary>
        /// Packaging specification number
        ///
        /// Reference number of documentation specifying the technical
        /// detail of packaging requirements.
        /// </summary>
        AHA,

        /// <summary>
        /// Authority issued equipment identification
        ///
        /// Identification issued by an authority, e.g. government,
        /// airport authority.
        /// </summary>
        AHB,

        /// <summary>
        /// Training flight number
        ///
        /// Non-revenue producing airline flight for training purposes.
        /// </summary>
        AHC,

        /// <summary>
        /// Fund code number
        ///
        /// Reference number to identify appropriation and branch
        /// chargeable for item.
        /// </summary>
        AHD,

        /// <summary>
        /// Signal code number
        ///
        /// Reference number to identify a signal.
        /// </summary>
        AHE,

        /// <summary>
        /// Major force program number
        ///
        /// Reference number according to Major Force Program (US).
        /// </summary>
        AHF,

        /// <summary>
        /// Nomination number
        ///
        /// Reference number assigned by a shipper to a request/
        /// commitment-to-ship on a pipeline system.
        /// </summary>
        AHG,

        /// <summary>
        /// Laboratory registration number
        ///
        /// Reference number is the official registration number of the
        /// laboratory.
        /// </summary>
        AHH,

        /// <summary>
        /// Transport contract reference number
        ///
        /// Reference number of a transport contract.
        /// </summary>
        AHI,

        /// <summary>
        /// Payee's reference number
        ///
        /// Reference number of the party to be paid.
        /// </summary>
        AHJ,

        /// <summary>
        /// Payer's reference number
        ///
        /// Reference number of the party who pays.
        /// </summary>
        AHK,

        /// <summary>
        /// Creditor's reference number
        ///
        /// Reference number of the party to whom a debt is owed.
        /// </summary>
        AHL,

        /// <summary>
        /// Debtor's reference number
        ///
        /// Reference number of the party who owes an amount of money.
        /// </summary>
        AHM,

        /// <summary>
        /// Joint venture reference number
        ///
        /// Reference number assigned to a joint venture agreement.
        /// </summary>
        AHN,

        /// <summary>
        /// Chamber of Commerce registration number
        ///
        /// The registration number by which a company/organization is
        /// known to the Chamber of Commerce.
        /// </summary>
        AHO,

        /// <summary>
        /// Tax registration number
        ///
        /// The registration number by which a company/organization is
        /// identified with the tax administration.
        /// </summary>
        AHP,

        /// <summary>
        /// Wool identification number
        ///
        /// Shipping Identification Mark (SIM) allocated to a wool
        /// consignment by a shipping company.
        /// </summary>
        AHQ,

        /// <summary>
        /// Wool tax reference number
        ///
        /// Reference or indication of the payment of wool tax.
        /// </summary>
        AHR,

        /// <summary>
        /// Meat processing establishment registration number
        ///
        /// Registration number allocated to a registered meat packing
        /// establishment by the local quarantine and inspection
        /// authority.
        /// </summary>
        AHS,

        /// <summary>
        /// Quarantine/treatment status reference number
        ///
        /// Coded quarantine/treatment status of a container and its
        /// cargo and packing materials, generated by a shipping company
        /// based upon declarations presented by a shipper.
        /// </summary>
        AHT,

        /// <summary>
        /// Request for quote number
        ///
        /// Reference number assigned by the requestor to a request for
        /// quote.
        /// </summary>
        AHU,

        /// <summary>
        /// Manual processing authority number
        ///
        /// Number allocated to allow the manual processing of an
        /// entity.
        /// </summary>
        AHV,

        /// <summary>
        /// Rate note number
        ///
        /// Reference assigned to a specific rate.
        /// </summary>
        AHX,

        /// <summary>
        /// Freight Forwarder number
        ///
        /// An identification code of a Freight Forwarder.
        /// </summary>
        AHY,

        /// <summary>
        /// Customs release code
        ///
        /// A code associated to a requirement that must be presented to
        /// gain the release of goods by Customs.
        /// </summary>
        AHZ,

        /// <summary>
        /// Compliance code number
        ///
        /// Number assigned to indicate regulatory compliance.
        /// </summary>
        AIA,

        /// <summary>
        /// Department of transportation bond number
        ///
        /// Number of a bond assigned by the department of
        /// transportation.
        /// </summary>
        AIB,

        /// <summary>
        /// Export establishment number
        ///
        /// Number to identify export establishment.
        /// </summary>
        AIC,

        /// <summary>
        /// Certificate of conformity
        ///
        /// Certificate certifying the conformity to predefined
        /// definitions.
        /// </summary>
        AID,

        /// <summary>
        /// Ministerial certificate of homologation
        ///
        /// Certificate of approval for components which are subject to
        /// legal restrictions and must be approved by the government.
        /// </summary>
        AIE,

        /// <summary>
        /// Previous delivery instruction number
        ///
        /// The identification of a previous delivery instruction.
        /// </summary>
        AIF,

        /// <summary>
        /// Passport number
        ///
        /// Number assigned to a passport.
        /// </summary>
        AIG,

        /// <summary>
        /// Common transaction reference number
        ///
        /// Reference number applicable to different underlying
        /// individual transactions.
        /// </summary>
        AIH,

        /// <summary>
        /// Bank's common transaction reference number
        ///
        /// Bank's reference number allocated by the bank to different
        /// underlying individual transactions.
        /// </summary>
        AII,

        /// <summary>
        /// Customer's individual transaction reference number
        ///
        /// Customer's reference number allocated by the customer to one
        /// specific transaction.
        /// </summary>
        AIJ,

        /// <summary>
        /// Bank's individual transaction reference number
        ///
        /// Bank's reference number allocated by the bank to one
        /// specific transaction.
        /// </summary>
        AIK,

        /// <summary>
        /// Customer's common transaction reference number
        ///
        /// Customer's reference number allocated by the customer to
        /// different underlying individual transactions.
        /// </summary>
        AIL,

        /// <summary>
        /// Individual transaction reference number
        ///
        /// Reference number applying to one specific transaction.
        /// </summary>
        AIM,

        /// <summary>
        /// Product sourcing agreement number
        ///
        /// Reference number assigned to a product sourcing agreement.
        /// </summary>
        AIN,

        /// <summary>
        /// Customs transhipment number
        ///
        /// Approval number issued by Customs for cargo to be
        /// transhipped under Customs control.
        /// </summary>
        AIO,

        /// <summary>
        /// Customs preference inquiry number
        ///
        /// The number assigned by Customs to a preference inquiry.
        /// </summary>
        AIP,

        /// <summary>
        /// Packing plant number
        ///
        /// Number to identify packing establishment.
        /// </summary>
        AIQ,

        /// <summary>
        /// Original certificate number
        ///
        /// Number giving reference to an original certificate number.
        /// </summary>
        AIR,

        /// <summary>
        /// Processing plant number
        ///
        /// Number to identify processing plant.
        /// </summary>
        AIS,

        /// <summary>
        /// Slaughter plant number
        ///
        /// Number to identify slaughter plant.
        /// </summary>
        AIT,

        /// <summary>
        /// Charge card account number
        ///
        /// Number to identify charge card account.
        /// </summary>
        AIU,

        /// <summary>
        /// Event reference number
        ///
        /// [1007] Reference number identifying an event.
        /// </summary>
        AIV,

        /// <summary>
        /// Transport section reference number
        ///
        /// A number identifying a transport section.
        /// </summary>
        AIW,

        /// <summary>
        /// Referred product for mechanical analysis
        ///
        /// A product number identifying the product which is used for
        /// mechanical analysis considered valid for a group of
        /// products.
        /// </summary>
        AIX,

        /// <summary>
        /// Referred product for chemical analysis
        ///
        /// A product number identifying the product which is used for
        /// chemical analysis considered valid for a group of products.
        /// </summary>
        AIY,

        /// <summary>
        /// Consolidated invoice number
        ///
        /// Invoice number into which other invoices are consolidated.
        /// </summary>
        AIZ,

        /// <summary>
        /// Part reference indicator in a drawing
        ///
        /// To designate the number which provides a cross reference
        /// between parts contained in a drawing and a parts catalogue.
        /// </summary>
        AJA,

        /// <summary>
        /// U.S. Code of Federal Regulations (CFR)
        ///
        /// A reference indicating a citation from the U.S. Code of
        /// Federal Regulations (CFR).
        /// </summary>
        AJB,

        /// <summary>
        /// Purchasing activity clause number
        ///
        /// A number indicating a clause applicable to a purchasing
        /// activity.
        /// </summary>
        AJC,

        /// <summary>
        /// U.S. Defense Federal Acquisition Regulation Supplement
        ///
        /// A reference indicating a citation from the U.S. Defense
        /// Federal Acquisition Regulation Supplement.
        /// </summary>
        AJD,

        /// <summary>
        /// Agency clause number
        ///
        /// A number indicating a clause applicable to a particular
        /// agency.
        /// </summary>
        AJE,

        /// <summary>
        /// Circular publication number
        ///
        /// A number specifying a circular publication.
        /// </summary>
        AJF,

        /// <summary>
        /// U.S. Federal Acquisition Regulation
        ///
        /// A reference indicating a citation from the U.S. Federal
        /// Acquisition Regulation.
        /// </summary>
        AJG,

        /// <summary>
        /// U.S. General Services Administration Regulation
        ///
        /// A reference indicating a citation from U.S. General Services
        /// Administration Regulation.
        /// </summary>
        AJH,

        /// <summary>
        /// U.S. Federal Information Resources Management Regulation
        ///
        /// A reference indicating a citation from U.S. Federal
        /// Information Resources Management Regulation.
        /// </summary>
        AJI,

        /// <summary>
        /// Paragraph
        ///
        /// A reference indicating a paragraph cited as the source of
        /// information.
        /// </summary>
        AJJ,

        /// <summary>
        /// Special instructions number
        ///
        /// A number indicating a citation used for special
        /// instructions.
        /// </summary>
        AJK,

        /// <summary>
        /// Site specific procedures, terms, and conditions number
        ///
        /// A number indicating a set of site specific procedures, terms
        /// and conditions.
        /// </summary>
        AJL,

        /// <summary>
        /// Master solicitation procedures, terms, and conditions
        ///
        /// number A number indicating a master solicitation containing
        /// procedures, terms and conditions.
        /// </summary>
        AJM,

        /// <summary>
        /// U.S. Department of Veterans Affairs Acquisition Regulation
        ///
        /// A reference indicating a citation from the U.S. Department
        /// of Veterans Affairs Acquisition Regulation.
        /// </summary>
        AJN,

        /// <summary>
        /// Military Interdepartmental Purchase Request (MIPR) number
        ///
        /// A number indicating an interdepartmental purchase request
        /// used by the military.
        /// </summary>
        AJO,

        /// <summary>
        /// Foreign military sales number
        ///
        /// A number specifying a sale to a foreign military.
        /// </summary>
        AJP,

        /// <summary>
        /// Defense priorities allocation system priority rating
        ///
        /// A reference indicating a priority rating assigned to
        /// allocate resources for defense purchases.
        /// </summary>
        AJQ,

        /// <summary>
        /// Wage determination number
        ///
        /// A number specifying a wage determination.
        /// </summary>
        AJR,

        /// <summary>
        /// Vereinbarungs-Nummer
        /// </summary>
        AJS,

        /// <summary>
        /// Standard Industry Classification (SIC) number
        ///
        /// A number specifying a standard industry classification.
        /// </summary>
        AJT,

        /// <summary>
        /// End item number
        ///
        /// A number specifying the end item applicable to a subordinate
        /// item.
        /// </summary>
        AJU,

        /// <summary>
        /// Federal supply schedule item number
        ///
        /// A number specifying an item listed in a federal supply
        /// schedule.
        /// </summary>
        AJV,

        /// <summary>
        /// Technical document number
        ///
        /// A number specifying a technical document.
        /// </summary>
        AJW,

        /// <summary>
        /// Technical order number
        ///
        /// A reference to an order that specifies a technical change.
        /// </summary>
        AJX,

        /// <summary>
        /// Suffix
        ///
        /// A reference to specify a suffix added to the end of a basic
        /// identifier.
        /// </summary>
        AJY,

        /// <summary>
        /// Transportation account number
        ///
        /// An account number to be charged or credited for
        /// transportation.
        /// </summary>
        AJZ,

        /// <summary>
        /// Container disposition order reference number
        ///
        /// Reference assigned to the empty container disposition order.
        /// </summary>
        AKA,

        /// <summary>
        /// Container prefix
        ///
        /// The first part of the unique identification of a container
        /// formed by an alpha code identifying the owner of the
        /// container.
        /// </summary>
        AKB,

        /// <summary>
        /// Transport equipment return reference
        ///
        /// Reference known at the address to return equipment to.
        /// </summary>
        AKC,

        /// <summary>
        /// Transport equipment survey reference
        ///
        /// Reference number assigned by the ordering party to the
        /// transport equipment survey order.
        /// </summary>
        AKD,

        /// <summary>
        /// Transport equipment survey report number
        ///
        /// Reference number used by a party to identify its transport
        /// equipment survey report.
        /// </summary>
        AKE,

        /// <summary>
        /// Transport equipment stuffing order
        ///
        /// Reference number assigned to the order to stuff goods in
        /// transport equipment.
        /// </summary>
        AKF,

        /// <summary>
        /// Vehicle Identification Number (VIN)
        ///
        /// The identification number which uniquely distinguishes one vehicle from another through the lifespan of the vehicle.
        /// </summary>
        AKG,

        /// <summary>
        /// Government bill of lading
        ///
        /// Bill of lading as defined by the government.
        /// </summary>
        AKH,

        /// <summary>
        /// Ordering customer's second reference number
        ///
        /// Ordering customer's second reference number.
        /// </summary>
        AKI,

        /// <summary>
        /// Direct debit reference
        ///
        /// Reference number assigned to the direct debit operation.
        /// </summary>
        AKJ,

        /// <summary>
        /// Meter reading at the beginning of the delivery
        ///
        /// Meter reading at the beginning of the delivery.
        /// </summary>
        AKK,

        /// <summary>
        /// Meter reading at the end of delivery
        ///
        /// Meter reading at the end of the delivery.
        /// </summary>
        AKL,

        /// <summary>
        /// Replenishment purchase order range start number
        ///
        /// Starting number of a range of purchase order numbers
        /// assigned by the buyer to vendor's replenishment orders.
        /// </summary>
        AKM,

        /// <summary>
        /// Third bank's reference
        ///
        /// Reference number of the third bank.
        /// </summary>
        AKN,

        /// <summary>
        /// Action authorization number
        ///
        /// A reference number authorizing an action.
        /// </summary>
        AKO,

        /// <summary>
        /// Appropriation number
        ///
        /// The number identifying a type of funding for a specific
        /// purpose (appropriation).
        /// </summary>
        AKP,

        /// <summary>
        /// Product change authority number
        ///
        /// Number which authorises a change in form, fit or function of
        /// a product.
        /// </summary>
        AKQ,

        /// <summary>
        /// General cargo consignment reference number
        ///
        /// Reference number identifying a particular general cargo
        /// (non-containerised or break bulk) consignment.
        /// </summary>
        AKR,

        /// <summary>
        /// Catalogue sequence number
        ///
        /// A number which uniquely identifies an item within a
        /// catalogue according to a standard numbering system.
        /// </summary>
        AKS,

        /// <summary>
        /// Forwarding order number
        ///
        /// Reference number assigned to the forwarding order by the
        /// ordering customer.
        /// </summary>
        AKT,

        /// <summary>
        /// Transport equipment survey reference number
        ///
        /// Reference number known at the address where the transport
        /// equipment will be or has been surveyed.
        /// </summary>
        AKU,

        /// <summary>
        /// Lease contract reference
        ///
        /// Reference number of the lease contract.
        /// </summary>
        AKV,

        /// <summary>
        /// Transport costs reference number
        ///
        /// Reference number of the transport costs.
        /// </summary>
        AKW,

        /// <summary>
        /// Transport equipment stripping order
        ///
        /// Reference number assigned to the order to strip goods from
        /// transport equipment.
        /// </summary>
        AKX,

        /// <summary>
        /// Prior policy number
        ///
        /// The number of the prior policy.
        /// </summary>
        AKY,

        /// <summary>
        /// Policy number
        ///
        /// Number assigned to a policy.
        /// </summary>
        AKZ,

        /// <summary>
        /// Procurement budget number
        ///
        /// A number which uniquely identifies a procurement budget
        /// against which commitments or invoices can be allocated.
        /// </summary>
        ALA,

        /// <summary>
        /// Domestic inventory management code
        ///
        /// Code to identify the management of domestic inventory.
        /// </summary>
        ALB,

        /// <summary>
        /// Customer reference number assigned to previous balance of
        ///
        /// payment information Identification number of the previous
        /// balance of payments information from customer message.
        /// </summary>
        ALC,

        /// <summary>
        /// Previous credit advice reference number
        ///
        /// Reference number of the previous "Credit advice" message.
        /// </summary>
        ALD,

        /// <summary>
        /// Reporting form number
        ///
        /// Reference number assigned to the reporting form.
        /// </summary>
        ALE,

        /// <summary>
        /// Authorization number for exception to dangerous goods
        ///
        /// regulations Reference number allocated by an authority. This
        /// number contains an approval concerning exceptions on the
        /// existing dangerous goods regulations.
        /// </summary>
        ALF,

        /// <summary>
        /// Dangerous goods security number
        ///
        /// Reference number allocated by an authority in order to
        /// control the dangerous goods on board of a specific means of
        /// transport for dangerous goods security purposes.
        /// </summary>
        ALG,

        /// <summary>
        /// Dangerous goods transport licence number
        ///
        /// Licence number allocated by an authority as to the
        /// permission of carrying dangerous goods by a specific means
        /// of transport.
        /// </summary>
        ALH,

        /// <summary>
        /// Previous rental agreement number
        ///
        /// Number to identify the previous rental agreement number.
        /// </summary>
        ALI,

        /// <summary>
        /// Next rental agreement reason number
        ///
        /// Number to identify the reason for the next rental agreement.
        /// </summary>
        ALJ,

        /// <summary>
        /// Consignee's invoice number
        ///
        /// The invoice number assigned by a consignee.
        /// </summary>
        ALK,

        /// <summary>
        /// Message batch number
        ///
        /// A number identifying a batch of messages.
        /// </summary>
        ALL,

        /// <summary>
        /// Previous delivery schedule number
        ///
        /// A reference number identifying a previous delivery schedule.
        /// </summary>
        ALM,

        /// <summary>
        /// Physical inventory recount reference number
        ///
        /// A reference to a re-count of physically held inventory.
        /// </summary>
        ALN,

        /// <summary>
        /// Returnable container reference number
        ///
        /// A reference number identifying a returnable container.
        /// </summary>
        ALP,

        /// <summary>
        /// Nummer einer Rücksendungsanzeige
        ///
        /// Referenznummer für eine Rücksendungsanzeige. (z.B. Retourennummer)
        /// </summary>
        ALQ,

        /// <summary>
        /// Wareneingangsmeldung-Nummer
        /// </summary>
        ALO,

        /// <summary>
        /// Sales forecast number
        ///
        /// A reference number identifying a sales forecast.
        /// </summary>
        ALR,

        /// <summary>
        /// Sales report number
        ///
        /// A reference number identifying a sales report.
        /// </summary>
        ALS,

        /// <summary>
        /// Previous tax control number
        ///
        /// A reference number identifying a previous tax control
        /// number.
        /// </summary>
        ALT,

        /// <summary>
        /// AGERD (Aerospace Ground Equipment Requirement Data) number
        ///
        /// Identifies the equipment required to conduct maintenance.
        /// </summary>
        ALU,

        /// <summary>
        /// Registered capital reference
        ///
        /// Registered capital reference of a company.
        /// </summary>
        ALV,

        /// <summary>
        /// Standard number of inspection document
        ///
        /// Code identifying the standard number of the inspection
        /// document supplied.
        /// </summary>
        ALW,

        /// <summary>
        /// Model
        ///
        /// (7242) A reference used to identify a model.
        /// </summary>
        ALX,

        /// <summary>
        /// Financial management reference
        ///
        /// A financial management reference.
        /// </summary>
        ALY,

        /// <summary>
        /// NOTIfication for COLlection number (NOTICOL)
        ///
        /// A reference assigned by a consignor to a notification
        /// document which indicates the availability of goods for
        /// collection.
        /// </summary>
        ALZ,

        /// <summary>
        /// Previous request for metered reading reference number
        ///
        /// Number to identify a previous request for a recording or
        /// reading of a measuring device.
        /// </summary>
        AMA,

        /// <summary>
        /// Next rental agreement number
        ///
        /// Number to identify the next rental agreement.
        /// </summary>
        AMB,

        /// <summary>
        /// Reference number of a request for metered reading
        ///
        /// Number to identify a request for a recording or reading of a
        /// measuring device to be taken.
        /// </summary>
        AMC,

        /// <summary>
        /// Hastening number
        ///
        /// A number which uniquely identifies a request to hasten an
        /// action.
        /// </summary>
        AMD,

        /// <summary>
        /// Repair data request number
        ///
        /// A number which uniquely identifies a request for data about
        /// repairs.
        /// </summary>
        AME,

        /// <summary>
        /// Consumption data request number
        ///
        /// A number which identifies a request for consumption data.
        /// </summary>
        AMF,

        /// <summary>
        /// Profile number
        ///
        /// Reference number allocated to a discrete set of criteria.
        /// </summary>
        AMG,

        /// <summary>
        /// Case number
        ///
        /// Number assigned to a case.
        /// </summary>
        AMH,

        /// <summary>
        /// Government quality assurance and control level Number
        ///
        /// A number which identifies the level of quality assurance and
        /// control required by the government for an article.
        /// </summary>
        AMI,

        /// <summary>
        /// Payment plan reference
        ///
        /// A number which uniquely identifies a payment plan.
        /// </summary>
        AMJ,

        /// <summary>
        /// Replaced meter unit number
        ///
        /// Number identifying the replaced meter unit.
        /// </summary>
        AMK,

        /// <summary>
        /// Replenishment purchase order range end number
        ///
        /// Ending number of a range of purchase order numbers assigned
        /// by the buyer to vendor's replenishment orders.
        /// </summary>
        AML,

        /// <summary>
        /// Insurer assigned reference number
        ///
        /// A unique reference number assigned by the insurer.
        /// </summary>
        AMM,

        /// <summary>
        /// Canadian excise entry number
        ///
        /// An excise entry number assigned by the Canadian Customs.
        /// </summary>
        AMN,

        /// <summary>
        /// Premium rate table
        ///
        /// Identifies the premium rate table.
        /// </summary>
        AMO,

        /// <summary>
        /// Advise through bank's reference
        ///
        /// Financial institution through which the advising bank is to
        /// advise the documentary credit.
        /// </summary>
        AMP,

        /// <summary>
        /// US, Department of Transportation bond surety code
        ///
        /// A bond surety code assigned by the United States Department
        /// of Transportation (DOT).
        /// </summary>
        AMQ,

        /// <summary>
        /// US, Food and Drug Administration establishment indicator
        ///
        /// An establishment indicator assigned by the United States
        /// Food and Drug Administration.
        /// </summary>
        AMR,

        /// <summary>
        /// US, Federal Communications Commission (FCC) import
        ///
        /// condition number A number known as the United States Federal
        /// Communications Commission (FCC) import condition number
        /// applying to certain types of regulated communications
        /// equipment.
        /// </summary>
        AMS,

        /// <summary>
        /// Goods and Services Tax identification number
        ///
        /// Identifier assigned to an entity by a tax authority for
        /// Goods and Services Tax (GST) related purposes.
        /// </summary>
        AMT,

        /// <summary>
        /// Integrated logistic support cross reference number
        ///
        /// Provides the identification of the reference which allows
        /// cross referencing of items between different areas of
        /// integrated logistics support.
        /// </summary>
        AMU,

        /// <summary>
        /// Department number
        ///
        /// Number assigned to a department within an organization.
        /// </summary>
        AMV,

        /// <summary>
        /// Buyer's catalogue number
        ///
        /// Identification of a catalogue maintained by a buyer.
        /// </summary>
        AMW,

        /// <summary>
        /// Financial settlement party's reference number
        ///
        /// Reference number of the party who is responsible for the
        /// financial settlement.
        /// </summary>
        AMX,

        /// <summary>
        /// Standard's version number
        ///
        /// The version number assigned to a standard.
        /// </summary>
        AMY,

        /// <summary>
        /// Pipeline number
        ///
        /// Number to identify a pipeline.
        /// </summary>
        AMZ,

        /// <summary>
        /// Account servicing bank's reference number
        ///
        /// Reference number of the account servicing bank.
        /// </summary>
        ANA,

        /// <summary>
        /// Completed units payment request reference
        ///
        /// A reference to a payment request for completed units.
        /// </summary>
        ANB,

        /// <summary>
        /// Payment in advance request reference
        ///
        /// A reference to a request for payment in advance.
        /// </summary>
        ANC,

        /// <summary>
        /// Parent file
        ///
        /// Identifies the parent file in a structure of related files.
        /// </summary>
        AND,

        /// <summary>
        /// Sub file
        ///
        /// Identifies the sub file in a structure of related files.
        /// </summary>
        ANE,

        /// <summary>
        /// CAD file layer convention
        ///
        /// Reference number identifying a layer convention for a file
        /// in a Computer Aided Design (CAD) environment.
        /// </summary>
        ANF,

        /// <summary>
        /// Technical regulation
        ///
        /// Reference number identifying a technical regulation.
        /// </summary>
        ANG,

        /// <summary>
        /// Plot file
        ///
        /// Reference number indicating that the file is a plot file.
        /// </summary>
        ANH,

        /// <summary>
        /// File conversion journal
        ///
        /// Reference number identifying a journal recording details
        /// about conversion operations between file formats.
        /// </summary>
        ANI,

        /// <summary>
        /// Authorization number
        ///
        /// A number which uniquely identifies an authorization.
        /// </summary>
        ANJ,

        /// <summary>
        /// Reference number assigned by third party
        ///
        /// Reference number assigned by a third party.
        /// </summary>
        ANK,

        /// <summary>
        /// Deposit reference number
        ///
        /// A reference number identifying a deposit.
        /// </summary>
        ANL,

        /// <summary>
        /// Named bank's reference
        ///
        /// Reference number of the named bank.
        /// </summary>
        ANM,

        /// <summary>
        /// Drawee's reference
        ///
        /// Reference number of the drawee.
        /// </summary>
        ANN,

        /// <summary>
        /// Case of need party's reference
        ///
        /// Reference number of the case of need party.
        /// </summary>
        ANO,

        /// <summary>
        /// Collecting bank's reference
        ///
        /// Reference number of the collecting bank.
        /// </summary>
        ANP,

        /// <summary>
        /// Remitting bank's reference
        ///
        /// Reference number of the remitting bank.
        /// </summary>
        ANQ,

        /// <summary>
        /// Principal's bank reference
        ///
        /// Reference number of the principal's bank.
        /// </summary>
        ANR,

        /// <summary>
        /// Presenting bank's reference
        ///
        /// Reference number of the presenting bank.
        /// </summary>
        ANS,

        /// <summary>
        /// Consignee's reference
        ///
        /// Reference number of the consignee.
        /// </summary>
        ANT,

        /// <summary>
        /// Financial transaction reference number
        ///
        /// Reference number of the financial transaction.
        /// </summary>
        ANU,

        /// <summary>
        /// Credit reference number
        ///
        /// The reference number of a credit instruction.
        /// </summary>
        ANV,

        /// <summary>
        /// Receiving bank's authorization number
        ///
        /// Authorization number of the receiving bank.
        /// </summary>
        ANW,

        /// <summary>
        /// Clearing reference
        ///
        /// Reference allocated by a clearing procedure.
        /// </summary>
        ANX,

        /// <summary>
        /// Sending bank's reference number
        ///
        /// Reference number of the sending bank.
        /// </summary>
        ANY,

        /// <summary>
        /// Documentary payment reference
        ///
        /// Reference of the documentary payment.
        /// </summary>
        AOA,

        /// <summary>
        /// Accounting file reference
        ///
        /// Reference of an accounting file.
        /// </summary>
        AOD,

        /// <summary>
        /// Sender's file reference number
        ///
        /// File reference number assigned by the sender.
        /// </summary>
        AOE,

        /// <summary>
        /// Receiver's file reference number
        ///
        /// File reference number assigned by the receiver.
        /// </summary>
        AOF,

        /// <summary>
        /// Source document internal reference
        ///
        /// Reference number assigned to a source document for internal
        /// usage.
        /// </summary>
        AOG,

        /// <summary>
        /// Principal's reference
        ///
        /// Reference number of the principal.
        /// </summary>
        AOH,

        /// <summary>
        /// Debit reference number
        ///
        /// The reference number of a debit instruction.
        /// </summary>
        AOI,

        /// <summary>
        /// Calendar
        ///
        /// A calendar reference number.
        /// </summary>
        AOJ,

        /// <summary>
        /// Work shift
        ///
        /// A work shift reference number.
        /// </summary>
        AOK,

        /// <summary>
        /// Work breakdown structure
        ///
        /// A structure reference that identifies the breakdown of work
        /// for a project.
        /// </summary>
        AOL,

        /// <summary>
        /// Organisation breakdown structure
        ///
        /// A structure reference that identifies the breakdown of an
        /// organisation.
        /// </summary>
        AOM,

        /// <summary>
        /// Work task charge number
        ///
        /// A reference assigned to a specific work task charge.
        /// </summary>
        AON,

        /// <summary>
        /// Functional work group
        ///
        /// A reference to identify a functional group performing work.
        /// </summary>
        AOO,

        /// <summary>
        /// Work team
        ///
        /// A reference to identify a team performing work.
        /// </summary>
        AOP,

        /// <summary>
        /// Department
        ///
        /// Section of an organisation.
        /// </summary>
        AOQ,

        /// <summary>
        /// Statement of work
        ///
        /// A reference number for a statement of work.
        /// </summary>
        AOR,

        /// <summary>
        /// Work package
        ///
        /// A reference for a detailed package of work.
        /// </summary>
        AOS,

        /// <summary>
        /// Planning package
        ///
        /// A reference for a planning package of work.
        /// </summary>
        AOT,

        /// <summary>
        /// Cost account
        ///
        /// A cost control account reference.
        /// </summary>
        AOU,

        /// <summary>
        /// Work order
        ///
        /// Reference number for an order to do work.
        /// </summary>
        AOV,

        /// <summary>
        /// Transportation Control Number (TCN)
        ///
        /// A number assigned for transportation purposes.
        /// </summary>
        AOW,

        /// <summary>
        /// Constraint notation
        ///
        /// Identifies a reference to a constraint notation.
        /// </summary>
        AOX,

        /// <summary>
        /// ETERMS reference
        ///
        /// Identifies a reference to the ICC (International Chamber of
        /// Commerce) ETERMS(tm) repository of electronic commerce
        /// trading terms and conditions.
        /// </summary>
        AOY,

        /// <summary>
        /// Implementation version number
        ///
        /// Identifies a version number of an implementation.
        /// </summary>
        AOZ,

        /// <summary>
        /// Accounts receivable number
        ///
        /// Reference number assigned by accounts receivable department
        /// to the account of a specific debtor.
        /// </summary>
        AP,

        /// <summary>
        /// Incorporated legal reference
        ///
        /// Identifies a legal reference which is deemed incorporated by
        /// reference.
        /// </summary>
        APA,

        /// <summary>
        /// Payment instalment reference number
        ///
        /// A reference number given to a payment instalment to identify
        /// a specific instance of payment of a debt which can be paid
        /// at specified intervals.
        /// </summary>
        APB,

        /// <summary>
        /// Equipment owner reference number
        ///
        /// Reference number issued by the owner of the equipment.
        /// </summary>
        APC,

        /// <summary>
        /// Cedent's claim number
        ///
        /// To identify the number assigned to the claim by the ceding
        /// company.
        /// </summary>
        APD,

        /// <summary>
        /// Reinsurer's claim number
        ///
        /// To identify the number assigned to the claim by the
        /// reinsurer.
        /// </summary>
        APE,

        /// <summary>
        /// Price/sales catalogue response reference number
        ///
        /// A reference number identifying a response to a price/sales
        /// catalogue.
        /// </summary>
        APF,

        /// <summary>
        /// General purpose message reference number
        ///
        /// A reference number identifying a general purpose message.
        /// </summary>
        APG,

        /// <summary>
        /// Invoicing data sheet reference number
        ///
        /// A reference number identifying an invoicing data sheet.
        /// </summary>
        APH,

        /// <summary>
        /// Bestandsberichtnr. Bei Inventurdifferenzen in Berechnung
        /// </summary>
        API,

        /// <summary>
        /// Ceiling formula reference number
        ///
        /// The reference number which identifies a formula for
        /// determining a ceiling.
        /// </summary>
        APJ,

        /// <summary>
        /// Price variation formula reference number
        ///
        /// The reference number which identifies a price variation
        /// formula.
        /// </summary>
        APK,

        /// <summary>
        /// Reference to account servicing bank's message
        ///
        /// Reference to the account servicing bank's message.
        /// </summary>
        APL,

        /// <summary>
        /// Party sequence number
        ///
        /// Reference identifying a party sequence number.
        /// </summary>
        APM,

        /// <summary>
        /// Purchaser's request reference
        ///
        /// Reference identifying a request made by the purchaser.
        /// </summary>
        APN,

        /// <summary>
        /// Contractor request reference
        ///
        /// Reference identifying a request made by a contractor.
        /// </summary>
        APO,

        /// <summary>
        /// Accident reference number
        ///
        /// Reference number assigned to an accident.
        /// </summary>
        APP,

        /// <summary>
        /// Commercial account summary reference number
        ///
        /// A reference number identifying a commercial account summary.
        /// </summary>
        APQ,

        /// <summary>
        /// Contract breakdown reference
        ///
        /// A reference which identifies a specific breakdown of a
        /// contract.
        /// </summary>
        APR,

        /// <summary>
        /// Contractor registration number
        ///
        /// A reference number used to identify a contractor.
        /// </summary>
        APS,

        /// <summary>
        /// Applicable coefficient identification number
        ///
        /// The identification number of the coefficient which is
        /// applicable.
        /// </summary>
        APT,

        /// <summary>
        /// Special budget account number
        ///
        /// The number of a special budget account.
        /// </summary>
        APU,

        /// <summary>
        /// Authorisation for repair reference
        ///
        /// Reference of the authorisation for repair.
        /// </summary>
        APV,

        /// <summary>
        /// Manufacturer defined repair rates reference
        ///
        /// Reference assigned by a manufacturer to their repair rates.
        /// </summary>
        APW,

        /// <summary>
        /// Original submitter log number
        ///
        /// A control number assigned by the original submitter.
        /// </summary>
        APX,

        /// <summary>
        /// Original submitter, parent Data Maintenance Request (DMR)
        ///
        /// log number A Data Maintenance Request (DMR) original
        /// submitter's reference log number for the parent DMR.
        /// </summary>
        APY,

        /// <summary>
        /// Original submitter, child Data Maintenance Request (DMR)
        ///
        /// log number A Data Maintenance Request (DMR) original
        /// submitter's reference log number for a child DMR.
        /// </summary>
        APZ,

        /// <summary>
        /// Entry point assessment log number
        ///
        /// The reference log number assigned by an entry point
        /// assessment group for the DMR.
        /// </summary>
        AQA,

        /// <summary>
        /// Entry point assessment log number, parent DMR
        ///
        /// The reference log number assigned by an entry point
        /// assessment group for the parent Data Maintenance Request
        /// (DMR).
        /// </summary>
        AQB,

        /// <summary>
        /// Entry point assessment log number, child DMR
        ///
        /// The reference log number assigned by an entry point
        /// assessment group for a child Data Maintenance Request (DMR).
        /// </summary>
        AQC,

        /// <summary>
        /// Data structure tag
        ///
        /// The tag assigned to a data structure.
        /// </summary>
        AQD,

        /// <summary>
        /// Central secretariat log number
        ///
        /// The reference log number assigned by the central secretariat
        /// for the Data Maintenance Request (DMR).
        /// </summary>
        AQE,

        /// <summary>
        /// Central secretariat log number, parent Data Maintenance
        ///
        /// Request (DMR) The reference log number assigned by the
        /// central secretariat for the parent Data Maintenance Request
        /// (DMR).
        /// </summary>
        AQF,

        /// <summary>
        /// Central secretariat log number, child Data Maintenance
        ///
        /// Request (DMR) The reference log number assigned by the
        /// central secretariat for the child Data Maintenance Request
        /// (DMR).
        /// </summary>
        AQG,

        /// <summary>
        /// International assessment log number
        ///
        /// The reference log number assigned to a Data Maintenance
        /// Request (DMR) changed in international assessment.
        /// </summary>
        AQH,

        /// <summary>
        /// International assessment log number, parent Data
        ///
        /// Maintenance Request (DMR) The reference log number assigned
        /// to a Data Maintenance Request (DMR) changed in international
        /// assessment that is a parent to the current DMR.
        /// </summary>
        AQI,

        /// <summary>
        /// International assessment log number, child Data Maintenance
        ///
        /// Request (DMR) The reference log number assigned to a Data
        /// Maintenance Request (DMR) changed in international
        /// assessment that is a child to the current DMR.
        /// </summary>
        AQJ,

        /// <summary>
        /// Status report number
        ///
        /// (1125) The reference number for a status report.
        /// </summary>
        AQK,

        /// <summary>
        /// Message design group number
        ///
        /// Reference number for a message design group.
        /// </summary>
        AQL,

        /// <summary>
        /// US Customs Service (USCS) entry code
        ///
        /// An entry number assigned by the United States (US) customs
        /// service.
        /// </summary>
        AQM,

        /// <summary>
        /// Beginning job sequence number
        ///
        /// The number designating the beginning of the job sequence.
        /// </summary>
        AQN,

        /// <summary>
        /// Sender's clause number
        ///
        /// The number that identifies the sender's clause.
        /// </summary>
        AQO,

        /// <summary>
        /// Dun and Bradstreet Canada's 8 digit Standard Industrial
        ///
        /// Classification (SIC) code Dun and Bradstreet Canada's 8
        /// digit Standard Industrial Classification (SIC) code
        /// identifying activities of the company.
        /// </summary>
        AQP,

        /// <summary>
        /// Activite Principale Exercee (APE) identifier
        ///
        /// The French industry code for the main activity of a company.
        /// </summary>
        AQQ,

        /// <summary>
        /// Dun and Bradstreet US 8 digit Standard Industrial
        ///
        /// Classification (SIC) code Dun and Bradstreet United States'
        /// 8 digit Standard Industrial Classification (SIC) code
        /// identifying activities of the company.
        /// </summary>
        AQR,

        /// <summary>
        /// Nomenclature Activity Classification Economy (NACE)
        ///
        /// identifier A European industry classification code used to
        /// identify the activity of a company.
        /// </summary>
        AQS,

        /// <summary>
        /// Norme Activite Francaise (NAF) identifier
        ///
        /// A French industry classification code assigned by the French
        /// government to identify the activity of a company.
        /// </summary>
        AQT,

        /// <summary>
        /// Registered contractor activity type
        ///
        /// Reference number identifying the type of registered
        /// contractor activity.
        /// </summary>
        AQU,

        /// <summary>
        /// Statistic Bundes Amt (SBA) identifier
        ///
        /// A German industry classification code issued by Statistic
        /// Bundes Amt (SBA) to identify the activity of a company.
        /// </summary>
        AQV,

        /// <summary>
        /// State or province assigned entity identification
        ///
        /// Reference number of an entity assigned by a state or
        /// province.
        /// </summary>
        AQW,

        /// <summary>
        /// Institute of Security and Future Market Development (ISFMD)
        ///
        /// serial number A number used to identify a public but not
        /// publicly traded company.
        /// </summary>
        AQX,

        /// <summary>
        /// File identification number
        ///
        /// A number assigned to identify a file.
        /// </summary>
        AQY,

        /// <summary>
        /// Bankruptcy procedure number
        ///
        /// A number identifying a bankruptcy procedure.
        /// </summary>
        AQZ,

        /// <summary>
        /// National government business identification number
        ///
        /// A business identification number which is assigned by a
        /// national government.
        /// </summary>
        ARA,

        /// <summary>
        /// Prior Data Universal Number System (DUNS) number
        ///
        /// A previously assigned Data Universal Number System (DUNS)
        /// number.
        /// </summary>
        ARB,

        /// <summary>
        /// Companies Registry Office (CRO) number
        ///
        /// Identifies the reference number assigned by the Companies
        /// Registry Office (CRO).
        /// </summary>
        ARC,

        /// <summary>
        /// Costa Rican judicial number
        ///
        /// A number assigned by the government to a business in Costa
        /// Rica.
        /// </summary>
        ARD,

        /// <summary>
        /// Numero de Identificacion Tributaria (NIT)
        ///
        /// A number assigned by the government to a business in some
        /// Latin American countries.
        /// </summary>
        ARE,

        /// <summary>
        /// Patron number
        ///
        /// A number assigned by the government to a business in some
        /// Latin American countries. Note that "Patron" is a Spanish
        /// word, it is not a person who gives financial or other
        /// support.
        /// </summary>
        ARF,

        /// <summary>
        /// Registro Informacion Fiscal (RIF) number
        ///
        /// A number assigned by the government to a business in some
        /// Latin American countries.
        /// </summary>
        ARG,

        /// <summary>
        /// Registro Unico de Contribuyente (RUC) number
        ///
        /// A number assigned by the government to a business in some
        /// Latin American countries.
        /// </summary>
        ARH,

        /// <summary>
        /// Tokyo SHOKO Research (TSR) business identifier
        ///
        /// A number assigned to a business by TSR.
        /// </summary>
        ARI,

        /// <summary>
        /// Personal identity card number
        ///
        /// An identity card number assigned to a person.
        /// </summary>
        ARJ,

        /// <summary>
        /// Systeme Informatique pour le Repertoire des ENtreprises
        ///
        /// (SIREN) number An identification number known as a SIREN
        /// assigned to a business in France.
        /// </summary>
        ARK,

        /// <summary>
        /// Systeme Informatique pour le Repertoire des ETablissements
        ///
        /// (SIRET) number An identification number known as a SIRET
        /// assigned to a business location in France.
        /// </summary>
        ARL,

        /// <summary>
        /// Publication issue number
        ///
        /// A number assigned to identify a publication issue.
        /// </summary>
        ARM,

        /// <summary>
        /// Original filing number
        ///
        /// A number assigned to the original filing.
        /// </summary>
        ARN,

        /// <summary>
        /// Document page identifier
        ///
        /// [1212] To identify a page number.
        /// </summary>
        ARO,

        /// <summary>
        /// Public filing registration number
        ///
        /// A number assigned at the time of registration of a public
        /// filing.
        /// </summary>
        ARP,

        /// <summary>
        /// Regiristo Federal de Contribuyentes
        ///
        /// A federal tax identification number assigned by the Mexican
        /// tax authority.
        /// </summary>
        ARQ,

        /// <summary>
        /// Social security number
        ///
        /// An identification number assigned to an individual by the
        /// social security administration.
        /// </summary>
        ARR,

        /// <summary>
        /// Document volume number
        ///
        /// The number of a document volume.
        /// </summary>
        ARS,

        /// <summary>
        /// Book number
        ///
        /// A number assigned to identify a book.
        /// </summary>
        ART,

        /// <summary>
        /// Stock exchange company identifier
        ///
        /// A reference assigned by the stock exchange to a company.
        /// </summary>
        ARU,

        /// <summary>
        /// Imputation account
        ///
        /// An account to which an amount is to be posted.
        /// </summary>
        ARV,

        /// <summary>
        /// Financial phase reference
        ///
        /// A reference which identifies a specific financial phase.
        /// </summary>
        ARW,

        /// <summary>
        /// Technical phase reference
        ///
        /// A reference which identifies a specific technical phase.
        /// </summary>
        ARX,

        /// <summary>
        /// Prior contractor registration number
        ///
        /// A previous reference number used to identify a contractor.
        /// </summary>
        ARY,

        /// <summary>
        /// Stock adjustment number
        ///
        /// A number identifying a stock adjustment.
        /// </summary>
        ARZ,

        /// <summary>
        /// Dispensation reference
        ///
        /// A reference number assigned to an official exemption from a
        /// law or obligation.
        /// </summary>
        ASA,

        /// <summary>
        /// Investment reference number
        ///
        /// A reference to a specific investment.
        /// </summary>
        ASB,

        /// <summary>
        /// Assuming company
        ///
        /// A number that identifies an assuming company.
        /// </summary>
        ASC,

        /// <summary>
        /// Budget chapter
        ///
        /// A reference to the chapter in a budget.
        /// </summary>
        ASD,

        /// <summary>
        /// Duty free products security number
        ///
        /// A security number allocated for duty free products.
        /// </summary>
        ASE,

        /// <summary>
        /// Duty free products receipt authorisation number
        ///
        /// Authorisation number allocated for the receipt of duty free
        /// products.
        /// </summary>
        ASF,

        /// <summary>
        /// Party information message reference
        ///
        /// Reference identifying a party information message.
        /// </summary>
        ASG,

        /// <summary>
        /// Formal statement reference
        ///
        /// A reference to a formal statement.
        /// </summary>
        ASH,

        /// <summary>
        /// Referenznummer zum Abliefernachweis
        /// </summary>
        ASI,

        /// <summary>
        /// Supplier's credit claim reference number
        ///
        /// A reference number identifying a supplier's credit claim.
        /// </summary>
        ASJ,

        /// <summary>
        /// Picture of actual product
        ///
        /// Reference identifying the picture of an actual product.
        /// </summary>
        ASK,

        /// <summary>
        /// Picture of a generic product
        ///
        /// Reference identifying a picture of a generic product.
        /// </summary>
        ASL,

        /// <summary>
        /// Trading partner identification number
        ///
        /// Code specifying an identification assigned to an entity with
        /// whom one conducts trade.
        /// </summary>
        ASM,

        /// <summary>
        /// Prior trading partner identification number
        ///
        /// Code specifying an identification number previously assigned
        /// to a trading partner.
        /// </summary>
        ASN,

        /// <summary>
        /// Password
        ///
        /// Code used for authentication purposes.
        /// </summary>
        ASO,

        /// <summary>
        /// Formal report number
        ///
        /// A number uniquely identifying a formal report.
        /// </summary>
        ASP,

        /// <summary>
        /// Fund account number
        ///
        /// Account number of fund.
        /// </summary>
        ASQ,

        /// <summary>
        /// Safe custody number
        ///
        /// The number of a file or portfolio kept for safe custody on
        /// behalf of clients.
        /// </summary>
        ASR,

        /// <summary>
        /// Master account number
        ///
        /// A reference number identifying a master account.
        /// </summary>
        ASS,

        /// <summary>
        /// Group reference number
        ///
        /// The reference number identifying a group.
        /// </summary>
        AST,

        /// <summary>
        /// Accounting transmission number
        ///
        /// A number used to identify the transmission of an accounting
        /// book entry.
        /// </summary>
        ASU,

        /// <summary>
        /// Product data file number
        ///
        /// The number of a product data file.
        /// </summary>
        ASV,

        /// <summary>
        /// Cadastro Geral do Contribuinte (CGC)
        ///
        /// Brazilian taxpayer number.
        /// </summary>
        ASW,

        /// <summary>
        /// Foreign resident identification number
        ///
        /// Number assigned by a government agency to identify a foreign
        /// resident.
        /// </summary>
        ASX,

        /// <summary>
        /// CD-ROM
        ///
        /// Identity number of the Compact Disk Read Only Memory (CD-
        /// ROM).
        /// </summary>
        ASY,

        /// <summary>
        /// Physical medium
        ///
        /// Identifies the physical medium.
        /// </summary>
        ASZ,

        /// <summary>
        /// Financial cancellation reference number
        ///
        /// Reference number of a financial cancellation.
        /// </summary>
        ATA,

        /// <summary>
        /// Purchase for export Customs agreement number
        ///
        /// A number assigned by a Customs authority allowing the
        /// purchase of goods free of tax because they are to be
        /// exported immediately after the purchase.
        /// </summary>
        ATB,

        /// <summary>
        /// Judgment number
        ///
        /// A reference number identifying the legal decision.
        /// </summary>
        ATC,

        /// <summary>
        /// Secretariat number
        ///
        /// A reference number identifying a secretariat.
        /// </summary>
        ATD,

        /// <summary>
        /// Previous banking status message reference
        ///
        /// Message reference number of the previous banking status
        /// message being responded to.
        /// </summary>
        ATE,

        /// <summary>
        /// Last received banking status message reference
        ///
        /// Reference number of the latest received banking status
        /// message.
        /// </summary>
        ATF,

        /// <summary>
        /// Bank's documentary procedure reference
        ///
        /// Reference allocated by the bank to a documentary procedure.
        /// </summary>
        ATG,

        /// <summary>
        /// Customer's documentary procedure reference
        ///
        /// Reference allocated by a customer to a documentary
        /// procedure.
        /// </summary>
        ATH,

        /// <summary>
        /// Safe deposit box number
        ///
        /// Number of the safe deposit box.
        /// </summary>
        ATI,

        /// <summary>
        /// Receiving Bankgiro number
        ///
        /// Number of the receiving Bankgiro.
        /// </summary>
        ATJ,

        /// <summary>
        /// Sending Bankgiro number
        ///
        /// Number of the sending Bankgiro.
        /// </summary>
        ATK,

        /// <summary>
        /// Bankgiro reference
        ///
        /// Reference of the Bankgiro.
        /// </summary>
        ATL,

        /// <summary>
        /// Guarantee number
        ///
        /// Number of a guarantee.
        /// </summary>
        ATM,

        /// <summary>
        /// Collection instrument number
        ///
        /// To identify the number of an instrument used to remit funds
        /// to a beneficiary.
        /// </summary>
        ATN,

        /// <summary>
        /// Converted Postgiro number
        ///
        /// To identify the reference number of a giro payment having
        /// been converted to a Postgiro account.
        /// </summary>
        ATO,

        /// <summary>
        /// Cost centre alignment number
        ///
        /// Number used in the financial management process to align
        /// cost allocations.
        /// </summary>
        ATP,

        /// <summary>
        /// Kamer Van Koophandel (KVK) number
        ///
        /// An identification number assigned by the Dutch Chamber of
        /// Commerce to a business in the Netherlands.
        /// </summary>
        ATQ,

        /// <summary>
        /// Institut Belgo-Luxembourgeois de Codification (IBLC) number
        ///
        /// An identification number assigned by the Luxembourg National
        /// Bank to a business in Luxembourg.
        /// </summary>
        ATR,

        /// <summary>
        /// External object reference
        ///
        /// A reference identifying an external object.
        /// </summary>
        ATS,

        /// <summary>
        /// Exceptional transport authorisation number
        ///
        /// Authorisation number for exceptional transport (using
        /// specific equipment, out of gauge, materials and/or specific
        /// routing).
        /// </summary>
        ATT,

        /// <summary>
        /// Clave Unica de Identificacion Tributaria (CUIT)
        ///
        /// Tax identification number in Argentina.
        /// </summary>
        ATU,

        /// <summary>
        /// Registro Unico Tributario (RUT)
        ///
        /// Tax identification number in Chile.
        /// </summary>
        ATV,

        /// <summary>
        /// Flat rack container bundle identification number
        ///
        /// Reference number assigned to a bundle of flat rack
        /// containers.
        /// </summary>
        ATW,

        /// <summary>
        /// Transport equipment acceptance order reference
        ///
        /// Reference number assigned to an order to accept transport
        /// equipment that is to be delivered by an inland carrier to a
        /// specified facility.
        /// </summary>
        ATX,

        /// <summary>
        /// Transport equipment release order reference
        ///
        /// Reference number assigned to an order to release transport
        /// equipment which is to be picked up by an inland carrier from
        /// a specified facility.
        /// </summary>
        ATY,

        /// <summary>
        /// Ship's stay reference number
        ///
        /// (1099) Reference number assigned by a port authority to the
        /// stay of a vessel in the port.
        /// </summary>
        ATZ,

        /// <summary>
        /// Authorization to meet competition number
        ///
        /// A number assigned by a requestor to an offer incoming
        /// following request for quote.
        /// </summary>
        AU,

        /// <summary>
        /// Place of positioning reference
        ///
        /// Identifies the reference pertaining to the place of
        /// positioning.
        /// </summary>
        AUA,

        /// <summary>
        /// Party reference
        ///
        /// The reference to a party.
        /// </summary>
        AUB,

        /// <summary>
        /// Issued prescription identification
        ///
        /// The identification of the issued prescription.
        /// </summary>
        AUC,

        /// <summary>
        /// Inkasso-Referenz
        /// </summary>
        AUD,

        /// <summary>
        /// Travel service
        ///
        /// Reference identifying a travel service.
        /// </summary>
        AUE,

        /// <summary>
        /// Consignment stock contract
        ///
        /// Reference identifying a consignment stock contract.
        /// </summary>
        AUF,

        /// <summary>
        /// Importer's letter of credit reference
        ///
        /// Letter of credit reference issued by importer.
        /// </summary>
        AUG,

        /// <summary>
        /// Performed prescription identification
        ///
        /// The identification of the prescription that has been carried
        /// into effect.
        /// </summary>
        AUH,

        /// <summary>
        /// Image reference
        ///
        /// A reference number identifying an image.
        /// </summary>
        AUI,

        /// <summary>
        /// Proposed purchase order reference number
        ///
        /// A reference number assigned to a proposed purchase order.
        /// </summary>
        AUJ,

        /// <summary>
        /// Application for financial support reference number
        ///
        /// Reference number assigned to an application for financial
        /// support.
        /// </summary>
        AUK,

        /// <summary>
        /// Manufacturing quality agreement number
        ///
        /// Reference number of a manufacturing quality agreement.
        /// </summary>
        AUL,

        /// <summary>
        /// Software editor reference
        ///
        /// Reference identifying the software editor.
        /// </summary>
        AUM,

        /// <summary>
        /// Software reference
        ///
        /// Reference identifying the software.
        /// </summary>
        AUN,

        /// <summary>
        /// Software quality reference
        ///
        /// Reference allocated to the software by a quality assurance
        /// agency.
        /// </summary>
        AUO,

        /// <summary>
        /// Consolidated orders' reference
        ///
        /// A reference number to identify orders which have been, or
        /// shall be consolidated.
        /// </summary>
        AUP,

        /// <summary>
        /// Customs binding ruling number
        ///
        /// Binding ruling number issued by customs.
        /// </summary>
        AUQ,

        /// <summary>
        /// Customs non-binding ruling number
        ///
        /// Non-binding ruling number issued by customs.
        /// </summary>
        AUR,

        /// <summary>
        /// Delivery route reference
        ///
        /// A reference to the route of the delivery.
        /// </summary>
        AUS,

        /// <summary>
        /// Net area supplier reference
        /// A reference identifying a supplier within a net area.
        /// </summary>
        AUT,

        /// <summary>
        /// Time series reference
        ///
        /// Reference to a time series.
        /// </summary>
        AUU,

        /// <summary>
        /// Connecting point to central grid
        ///
        /// Reference to a connecting point to a central grid.
        /// </summary>
        AUV,

        /// <summary>
        /// Marketing plan identification number (MPIN)
        ///
        /// Number identifying a marketing plan.
        /// </summary>
        AUW,

        /// <summary>
        /// Entity reference number, previous
        ///
        /// The previous reference number assigned to an entity.
        /// </summary>
        AUX,

        /// <summary>
        /// International Standard Industrial Classification (ISIC)
        ///
        /// code A code specifying an international standard industrial
        /// classification.
        /// </summary>
        AUY,

        /// <summary>
        /// Customs pre-approval ruling number
        ///
        /// Pre-approval ruling number issued by Customs.
        /// </summary>
        AUZ,

        /// <summary>
        /// Account payable number
        ///
        /// Reference number assigned by accounts payable department to
        /// the account of a specific creditor.
        /// </summary>
        AV,

        /// <summary>
        /// First financial institution's transaction reference
        ///
        /// Identifies the reference given to the individual transaction
        /// by the financial institution that is the transaction's point
        /// of entry into the interbank transaction chain.
        /// </summary>
        AVA,

        /// <summary>
        /// Product characteristics directory
        ///
        /// A reference to a product characteristics directory.
        /// </summary>
        AVB,

        /// <summary>
        /// Supplier's customer reference number
        ///
        /// A number, assigned by a supplier, to reference a customer.
        /// </summary>
        AVC,

        /// <summary>
        /// Inventory report request number
        ///
        /// Reference number assigned to a request for an inventory
        /// report.
        /// </summary>
        AVD,

        /// <summary>
        /// Metering point
        ///
        /// Reference to a metering point.
        /// </summary>
        AVE,

        /// <summary>
        /// Passenger reservation number
        ///
        /// Number assigned by the travel supplier to identify the
        /// passenger reservation.
        /// </summary>
        AVF,

        /// <summary>
        /// Slaughterhouse approval number
        ///
        /// Veterinary licence number allocated by a national authority
        /// to a slaughterhouse.
        /// </summary>
        AVG,

        /// <summary>
        /// Meat cutting plant approval number
        ///
        /// Veterinary licence number allocated by a national authority
        /// to a meat cutting plant.
        /// </summary>
        AVH,

        /// <summary>
        /// Customer travel service identifier
        ///
        /// A reference identifying a travel service to a customer.
        /// </summary>
        AVI,

        /// <summary>
        /// Export control classification number
        ///
        /// Number identifying the classification of goods covered by an
        /// export licence.
        /// </summary>
        AVJ,

        /// <summary>
        /// Broker reference 3
        ///
        /// Third reference of a broker.
        /// </summary>
        AVK,

        /// <summary>
        /// Consignment information
        ///
        /// Code identifying that the reference number given applies to
        /// the consignment information segment group in the referred
        /// message .
        /// </summary>
        AVL,

        /// <summary>
        /// Goods item information
        ///
        /// Code identifying that the reference number given applies to
        /// the goods item information segment group in the referred
        /// message.
        /// </summary>
        AVM,

        /// <summary>
        /// Dangerous Goods information
        ///
        /// Code identifying that the reference number given applies to
        /// the dangerous goods information segment group in the
        /// referred message.
        /// </summary>
        AVN,

        /// <summary>
        /// Pilotage services exemption number
        ///
        /// Number identifying the permit to not use pilotage services.
        /// </summary>
        AVO,

        /// <summary>
        /// Person registration number
        ///
        /// A number assigned to an individual.
        /// </summary>
        AVP,

        /// <summary>
        /// Place of packing approval number
        ///
        /// Approval Number of the place where goods are packaged.
        /// </summary>
        AVQ,

        /// <summary>
        /// Original Mandate Reference
        ///
        /// Reference to a specific related original mandate given by
        /// the relevant party for underlying business or action in case
        /// of reference or mandate change.
        /// </summary>
        AVR,

        /// <summary>
        /// Mandate Reference
        ///
        /// Reference to a specific mandate given by the relevant party
        /// for underlying business or action.
        /// </summary>
        AVS,

        /// <summary>
        /// Reservation station indentifier
        ///
        /// Reference to the station where a reservation was made.
        /// </summary>
        AVT,

        /// <summary>
        /// Unique goods shipment identifier
        ///
        /// Unique identifier assigned to a shipment of goods linking
        /// trade, tracking and transport information.
        /// </summary>
        AVU,

        /// <summary>
        /// Framework Agreement Number
        ///
        /// A reference to an agreement between one or more contracting
        /// authorities and one or more economic operators, the purpose
        /// of which is to establish the terms governing contracts to be
        /// awarded during a given period, in particular with regard to
        /// price and, where appropriate, the quantity envisaged.
        /// </summary>
        AVV,

        /// <summary>
        /// Hash value
        ///
        /// Contains the hash value of a related document.
        /// </summary>
        AVW,

        /// <summary>
        /// Movement reference number
        ///
        /// Number assigned by customs referencing receipt of an Entry
        /// Summary Declaration.
        /// </summary>
        AVX,

        /// <summary>
        /// Economic Operators Registration and Identification Number
        ///
        /// (EORI) Number assigned by an authority to an economic
        /// operator.
        /// </summary>
        AVY,

        /// <summary>
        /// Local Reference Number
        ///
        /// Number assigned by a national customs authority to an Entry
        /// Summary Declaration.
        /// </summary>
        AVZ,

        /// <summary>
        /// Rate code number
        ///
        /// Number assigned by a buyer to rate a product.
        /// </summary>
        AWA,

        /// <summary>
        /// Air waybill number
        ///
        /// Reference number assigned to an air waybill, see: 1001 =
        /// 740.
        /// </summary>
        AWB,

        /// <summary>
        /// Documentary credit amendment number
        ///
        /// Number of the amendment of the documentary credit.
        /// </summary>
        AWC,

        /// <summary>
        /// Advising bank's reference
        ///
        /// Reference number of the advising bank.
        /// </summary>
        AWD,

        /// <summary>
        /// Cost centre
        ///
        /// A number identifying a cost centre.
        /// </summary>
        AWE,

        /// <summary>
        /// Work item quantity determination
        ///
        /// A reference assigned to a work item quantity determination.
        /// </summary>
        AWF,

        /// <summary>
        /// Internal data process number
        ///
        /// A number identifying an internal data process.
        /// </summary>
        AWG,

        /// <summary>
        /// Category of work reference
        ///
        /// A reference identifying a category of work.
        /// </summary>
        AWH,

        /// <summary>
        /// Policy form number
        ///
        /// Number assigned to a policy form.
        /// </summary>
        AWI,

        /// <summary>
        /// Net area
        ///
        /// Reference to an area of a net.
        /// </summary>
        AWJ,

        /// <summary>
        /// Service provider
        ///
        /// Reference of the service provider.
        /// </summary>
        AWK,

        /// <summary>
        /// Error position
        ///
        /// Reference to the position of an error in a message.
        /// </summary>
        AWL,

        /// <summary>
        /// Service category reference
        ///
        /// Reference identifying the service category.
        /// </summary>
        AWM,

        /// <summary>
        /// Connected location
        ///
        /// Reference of a connected location.
        /// </summary>
        AWN,

        /// <summary>
        /// Related party
        ///
        /// Reference of a related party.
        /// </summary>
        AWO,

        /// <summary>
        /// Latest accounting entry record reference
        ///
        /// Code identifying the reference of the latest accounting
        /// entry record.
        /// </summary>
        AWP,

        /// <summary>
        /// Accounting entry
        ///
        /// Accounting entry to which this item is related.
        /// </summary>
        AWQ,

        /// <summary>
        /// Ursprungsbelegnummer
        /// </summary>
        AWR,

        /// <summary>
        /// Hygienic Certificate number, national
        ///
        /// Nationally set Hygienic Certificate number, such as
        /// sanitary, epidemiologic.
        /// </summary>
        AWS,

        /// <summary>
        /// Administrative Reference Code
        ///
        /// Reference number assigned by Customs to a ‘shipment of
        /// excise goods’.
        /// </summary>
        AWT,

        /// <summary>
        /// Pick-up sheet number
        ///
        /// Reference number assigned to a pick-up sheet.
        /// </summary>
        AWU,

        /// <summary>
        /// Phone number
        ///
        /// A sequence of digits used to call from one telephone line to
        /// another in a public telephone network.
        /// </summary>
        AWV,

        /// <summary>
        /// Buyer's fund number
        ///
        /// A reference number indicating the fund number used by the
        /// buyer.
        /// </summary>
        AWW,

        /// <summary>
        /// Company trading account number
        ///
        /// A reference number identifying a company trading account.
        /// </summary>
        AWX,

        /// <summary>
        /// Reserved goods identifier
        ///
        /// A reference number identifying goods in stock which have
        /// been reserved for a party.
        /// </summary>
        AWY,

        /// <summary>
        /// Handling and movement reference number
        ///
        /// A reference number identifying a previously transmitted
        /// cargo/goods handling and movement message.
        /// </summary>
        AWZ,

        /// <summary>
        /// Instruction to despatch reference number
        ///
        /// A reference number identifying a previously transmitted
        /// instruction to despatch message.
        /// </summary>
        AXA,

        /// <summary>
        /// Instruction for returns number
        ///
        /// A reference number identifying a previously communicated
        /// instruction for return message.
        /// </summary>
        AXB,

        /// <summary>
        /// Metered services consumption report number
        ///
        /// A reference number identifying a previously communicated
        /// metered services consumption report.
        /// </summary>
        AXC,

        /// <summary>
        /// Order status enquiry number
        ///
        /// A reference number to a previously sent order status
        /// enquiry.
        /// </summary>
        AXD,

        /// <summary>
        /// Firm booking reference number
        ///
        /// A reference number identifying a previous firm booking.
        /// </summary>
        AXE,

        /// <summary>
        /// Product inquiry number
        ///
        /// A reference number identifying a previously communicated
        /// product inquiry.
        /// </summary>
        AXF,

        /// <summary>
        /// Split delivery number
        ///
        /// A reference number identifying a split delivery.
        /// </summary>
        AXG,

        /// <summary>
        /// Service relation number
        ///
        /// A reference number identifying the relationship between a
        /// service provider and a service client, e.g., treatment of a
        /// patient in a hospital, usage by a member of a library
        /// facility, etc.
        /// </summary>
        AXH,

        /// <summary>
        /// Serial shipping container code
        ///
        /// Reference number identifying a logistic unit.
        /// </summary>
        AXI,

        /// <summary>
        /// Test specification number
        ///
        /// A reference number identifying a test specification.
        /// </summary>
        AXJ,

        /// <summary>
        /// Transport status report number
        ///
        /// [1125] A reference number identifying a transport status
        /// report.
        /// </summary>
        AXK,

        /// <summary>
        /// Tooling contract number
        ///
        /// A reference number of the tooling contract.
        /// </summary>
        AXL,

        /// <summary>
        /// Formula reference number
        ///
        /// The reference number which identifies a formula.
        /// </summary>
        AXM,

        /// <summary>
        /// Pre-agreement number
        ///
        /// A reference number identifying a pre-agreement.
        /// </summary>
        AXN,

        /// <summary>
        /// Product certification number
        ///
        /// Number assigned by a governing body (or their agents) to a
        /// product which certifies compliance with a standard.
        /// </summary>
        AXO,

        /// <summary>
        /// Consignment contract number
        ///
        /// Reference number identifying a consignment contract.
        /// </summary>
        AXP,

        /// <summary>
        /// Product specification reference number
        ///
        /// Number assigned by the issuer to his product specification.
        /// </summary>
        AXQ,

        /// <summary>
        /// Payroll deduction advice reference
        ///
        /// A reference number identifying a payroll deduction advice.
        /// </summary>
        AXR,

        /// <summary>
        /// TRACES party identification
        ///
        /// The party identification number used in the European Union's
        /// Trade Control and Expert System (TRACES).
        /// </summary>
        AXS,

        /// <summary>
        /// Block Stowage Reference
        /// </summary>
        AXU,

        /// <summary>
        /// Aktueller Anfangszählerstand
        /// (z.B. Kilometerstand eines Fahrzeugs)
        /// </summary>
        BA,

        /// <summary>
        /// Bid number
        ///
        /// Number assigned by a submitter of a bid to his bid.
        /// </summary>
        BD,

        /// <summary>
        /// Beginning meter reading estimated
        ///
        /// Meter reading at the beginning of an invoicing period where
        /// an actual reading is not available.
        /// </summary>
        BE,

        /// <summary>
        /// House bill of lading number
        ///
        /// [1039] Reference number assigned to a house bill of lading.
        /// </summary>
        BH,

        /// <summary>
        /// Bill of lading number
        ///
        /// Reference number assigned to a bill of lading, see: 1001 =
        /// 705.
        /// </summary>
        BM,

        /// <summary>
        /// Consignment identifier, carrier assigned
        ///
        /// [1016] Reference number assigned by a carrier of its agent
        /// to identify a specific consignment such as a booking
        /// reference number when cargo space is reserved prior to
        /// loading.
        /// </summary>
        BN,

        /// <summary>
        /// Rahmenauftragsnummer
        /// </summary>
        BO,

        /// <summary>
        /// Vertragsnummer (Käufer)
        ///
        /// Referenznummer vergeben vom Käufer für einen Vertrag (z.B. Abkommennummer)
        /// </summary>
        BC,

        /// <summary>
        /// Broker or sales office number
        ///
        /// A number that identifies a broker or sales office.
        /// </summary>
        BR,

        /// <summary>
        /// Batch number/lot number
        ///
        /// [7338] Reference number assigned by manufacturer to a series
        /// of similar products or goods produced under similar
        /// conditions.
        /// </summary>
        BT,

        /// <summary>
        /// Battery and accumulator producer registration number
        ///
        /// Registration number of producer of batteries and
        /// accumulators.
        /// </summary>
        BTP,

        /// <summary>
        /// Blended with number
        ///
        /// The batch/lot/package number a product is blended with.
        /// </summary>
        BW,

        /// <summary>
        /// IATA Cargo Agent CASS Address number
        ///
        /// Code issued by IATA to identify agent locations for CASS
        /// billing purposes.
        /// </summary>
        CAS,

        /// <summary>
        /// Matching of entries, balanced
        ///
        /// Reference to a balanced matching of entries.
        /// </summary>
        CAT,

        /// <summary>
        /// Entry flagging
        ///
        /// Reference to a flagging of entries.
        /// </summary>
        CAU,

        /// <summary>
        /// Matching of entries, unbalanced
        ///
        /// Reference to an unbalanced matching of entries.
        /// </summary>
        CAV,

        /// <summary>
        /// Document reference, internal
        ///
        /// Internal reference to a document.
        /// </summary>
        CAW,

        /// <summary>
        /// European Value Added Tax identification
        ///
        /// Value Added Tax identification number according to European
        /// regulation.
        /// </summary>
        CAX,

        /// <summary>
        /// Cost accounting document
        ///
        /// The reference to a cost accounting document.
        /// </summary>
        CAY,

        /// <summary>
        /// Grid operator's customer reference number
        ///
        /// A number, assigned by a grid operator, to reference a
        /// customer.
        /// </summary>
        CAZ,

        /// <summary>
        /// Ticket control number
        ///
        /// Reference giving access to all the details associated with
        /// the ticket.
        /// </summary>
        CBA,

        /// <summary>
        /// Order shipment grouping reference
        ///
        /// A reference number identifying the grouping of purchase
        /// orders into one shipment.
        /// </summary>
        CBB,

        /// <summary>
        /// Gutschrift
        /// </summary>
        CD,

        /// <summary>
        /// Ceding company
        ///
        /// Company selling obligations to a third party.
        /// </summary>
        CEC,

        /// <summary>
        /// Debit letter number
        ///
        /// Reference number identifying the letter of debit document.
        /// </summary>
        CED,

        /// <summary>
        /// Consignee's further order
        ///
        /// Reference of an order given by the consignee after departure
        /// of the means of transport.
        /// </summary>
        CFE,

        /// <summary>
        /// Animal farm licence number
        ///
        /// Veterinary licence number allocated by a national authority
        /// to an animal farm.
        /// </summary>
        CFF,

        /// <summary>
        /// Consignor's further order
        ///
        /// Reference of an order given by the consignor after departure
        /// of the means of transport.
        /// </summary>
        CFO,

        /// <summary>
        /// Consignee's order number
        ///
        /// A number that identifies a consignee's order.
        /// </summary>
        CG,

        /// <summary>
        /// Customer catalogue number
        ///
        /// Number identifying a catalogue for customer's usage.
        /// </summary>
        CH,

        /// <summary>
        /// Cheque number
        ///
        /// Unique number assigned to one specific cheque.
        /// </summary>
        CK,

        /// <summary>
        /// Checking number
        ///
        /// Number assigned by checking party to one specific check
        /// action.
        /// </summary>
        CKN,

        /// <summary>
        /// Credit memo number
        ///
        /// Reference number assigned by issuer to a credit memo.
        /// </summary>
        CM,

        /// <summary>
        /// Road consignment note number
        ///
        /// Reference number assigned to a road consignment note, see:
        /// 1001 = 730.
        /// </summary>
        CMR,

        /// <summary>
        /// Carrier's reference number
        ///
        /// Reference number assigned by carrier to a consignment.
        /// </summary>
        CN,

        /// <summary>
        /// Charges note document attachment indicator
        ///
        /// [1070] Indication that a charges note has been established
        /// and attached to a transport contract document or not.
        /// </summary>
        CNO,

        /// <summary>
        /// Call off order number
        ///
        /// A number that identifies a call off order.
        /// </summary>
        COF,

        /// <summary>
        /// Condition of purchase document number
        ///
        /// Reference number identifying the conditions of purchase
        /// relevant to a purchase.
        /// </summary>
        CP,

        /// <summary>
        /// Customer reference number
        ///
        /// Reference number assigned by the customer to a transaction.
        /// </summary>
        CR,

        /// <summary>
        /// Transport means journey identifier
        ///
        /// [8028] To identify a journey of a means of transport, for
        /// example voyage number, flight number, trip number.
        /// </summary>
        CRN,

        /// <summary>
        /// Condition of sale document number
        ///
        /// Reference number identifying the conditions of sale relevant
        /// to a sale.
        /// </summary>
        CS,

        /// <summary>
        /// Team assignment number
        ///
        /// Team number assigned to a group that is responsible for
        /// working a particular transaction.
        /// </summary>
        CST,

        /// <summary>
        /// Contract number
        ///
        /// [1296] Reference number of a contract concluded between
        /// parties.
        /// </summary>
        CT,

        /// <summary>
        /// Consignment identifier, consignor assigned
        ///
        /// [1140] Reference number assigned by the consignor to
        /// identify a particular consignment.
        /// </summary>
        CU,

        /// <summary>
        /// Container operators reference number
        ///
        /// Reference number assigned by the party operating or
        /// controlling the transport container to a transaction or
        /// consignment.
        /// </summary>
        CV,

        /// <summary>
        /// Package number
        ///
        /// (7070) Reference number identifying a package or carton
        /// within a consignment.
        /// </summary>
        CW,

        /// <summary>
        /// Cooperation contract number
        ///
        /// Number issued by a party concerned given to a contract on
        /// cooperation of two or more parties.
        /// </summary>
        CZ,

        /// <summary>
        /// Deferment approval number
        ///
        /// Number assigned by authorities to a party to approve
        /// deferment of payment of tax or duties.
        /// </summary>
        DA,

        /// <summary>
        /// Debit account number
        ///
        /// Reference number assigned by issuer to a debit account.
        /// </summary>
        DAN,

        /// <summary>
        /// Buyer's debtor number
        ///
        /// Reference number assigned to a debtor.
        /// </summary>
        DB,

        /// <summary>
        /// Distributor invoice number
        ///
        /// Reference number assigned by issuer to a distributor
        /// invoice.
        /// </summary>
        DI,

        /// <summary>
        /// Belastunganzeige
        /// </summary>
        DL,

        /// <summary>
        /// Document identifier
        ///
        /// [1004] Reference number identifying a specific document.
        /// </summary>
        DM,

        /// <summary>
        /// Lieferscheinnummer
        ///
        ///  Delivery note number
        /// </summary>
        DQ,

        /// <summary>
        /// Dock receipt number
        ///
        /// Number of the cargo receipt submitted when cargo is
        /// delivered to a marine terminal.
        /// </summary>
        DR,

        /// <summary>
        /// Ending meter reading actual
        ///
        /// Meter reading at the end of an invoicing period.
        /// </summary>
        EA,

        /// <summary>
        /// Embargo permit number
        ///
        /// Reference number assigned by issuer to an embargo permit.
        /// </summary>
        EB,

        /// <summary>
        /// Export declaration
        ///
        /// Number assigned by the exporter to his export declaration
        /// number submitted to an authority.
        /// </summary>
        ED,

        /// <summary>
        /// Ending meter reading estimated
        ///
        /// Meter reading at the end of an invoicing period where an
        /// actual reading is not available.
        /// </summary>
        EE,

        /// <summary>
        /// Electrical and electronic equipment producer registration
        ///
        /// number Registration number of producer of electrical and
        /// electronic equipment.
        /// </summary>
        EEP,

        /// <summary>
        /// Employer's identification number
        ///
        /// Number issued by an authority to identify an employer.
        /// </summary>
        EI,

        /// <summary>
        /// Embargo number
        ///
        /// Number assigned to specific goods or a family of goods in a
        /// classification of embargo measures.
        /// </summary>
        EN,

        /// <summary>
        /// Equipment number
        ///
        /// Number assigned by the manufacturer to specific equipment.
        /// </summary>
        EQ,

        /// <summary>
        /// Container/equipment receipt number
        ///
        /// Number of the Equipment Interchange Receipt issued for full
        /// or empty equipment received.
        /// </summary>
        ER,

        /// <summary>
        /// Exporter's reference number
        ///
        /// Reference to a party exporting goods.
        /// </summary>
        ERN,

        /// <summary>
        /// Excess transportation number
        ///
        /// (1041) Number assigned to excess transport.
        /// </summary>
        ET,

        /// <summary>
        /// Export permit identifier
        ///
        /// [1208] Reference number to identify an export licence or
        /// permit.
        /// </summary>
        EX,

        /// <summary>
        /// Fiscal number
        ///
        /// Tax payer's number. Number assigned to individual persons as
        /// well as to corporates by a public institution; this number
        /// is different from the VAT registration number.
        /// </summary>
        FC,

        /// <summary>
        /// Consignment identifier, freight forwarder assigned
        ///
        /// [1460] Reference number assigned by the freight forwarder to
        /// identify a particular consignment.
        /// </summary>
        FF,

        /// <summary>
        /// File line identifier
        ///
        /// Number assigned by the file issuer or sender to identify a
        /// specific line.
        /// </summary>
        FI,

        /// <summary>
        /// Flow reference number
        ///
        /// Number given to a usual sender which has regular expeditions
        /// of the same goods, to the same destination, defining all
        /// general conditions of the transport.
        /// </summary>
        FLW,

        /// <summary>
        /// Freight bill number
        ///
        /// Reference number assigned by issuing party to a freight
        /// bill.
        /// </summary>
        FN,

        /// <summary>
        /// Foreign exchange
        ///
        /// Exchange of two currencies at an agreed rate.
        /// </summary>
        FO,

        /// <summary>
        /// Final sequence number
        ///
        /// A number that identifies the final sequence.
        /// </summary>
        FS,

        /// <summary>
        /// Free zone identifier
        ///
        /// Identifier to specify the territory of a State where any
        /// goods introduced are generally regarded, insofar as import
        /// duties and taxes are concerned, as being outside the Customs
        /// territory and are not subject to usual Customs control
        /// (CCC).
        /// </summary>
        FT,

        /// <summary>
        /// File version number
        ///
        /// Number given to a version of an identified file.
        /// </summary>
        FV,

        /// <summary>
        /// Foreign exchange contract number
        ///
        /// Reference number identifying a foreign exchange contract.
        /// </summary>
        FX,

        /// <summary>
        /// Standard's number
        ///
        /// Number to identify a standardization description (e.g. ISO
        /// 9375).
        /// </summary>
        GA,

        /// <summary>
        /// Government contract number
        ///
        /// Number assigned to a specific government/public contract.
        /// </summary>
        GC,

        /// <summary>
        /// Standard's code number
        ///
        /// Number to identify a specific parameter within a
        /// standardization description (e.g. M5 for screws or DIN A4
        /// for paper).
        /// </summary>
        GD,

        /// <summary>
        /// General declaration number
        ///
        /// Number of the declaration of incoming goods out of a vessel.
        /// </summary>
        GDN,

        /// <summary>
        /// Government reference number
        ///
        /// A number that identifies a government reference.
        /// </summary>
        GN,

        /// <summary>
        /// Harmonised system number
        ///
        /// Number specifying the goods classification under the
        /// Harmonised Commodity Description and Coding System of the
        /// Customs Co-operation Council (CCC).
        /// </summary>
        HS,

        /// <summary>
        /// House waybill number
        ///
        /// Reference number assigned to a house waybill, see: 1001 =
        /// 703.
        /// </summary>
        HWB,

        /// <summary>
        /// Internal vendor number
        ///
        /// Number identifying the company-internal vending
        /// department/unit.
        /// </summary>
        IA,

        /// <summary>
        /// In bond number
        ///
        /// Customs assigned number that is used to control the movement
        /// of imported cargo prior to its formal Customs clearing.
        /// </summary>
        IB,

        /// <summary>
        /// IATA cargo agent code number
        ///
        /// Code issued by IATA identify each IATA Cargo Agent whose
        /// name is entered on the Cargo Agency List.
        /// </summary>
        ICA,

        /// <summary>
        /// Insurance certificate reference number
        ///
        /// A number that identifies an insurance certificate reference.
        /// </summary>
        ICE,

        /// <summary>
        /// Insurance contract reference number
        ///
        /// A number that identifies an insurance contract reference.
        /// </summary>
        ICO,

        /// <summary>
        /// Initial sample inspection report number
        ///
        /// Inspection report number given to the initial sample
        /// inspection.
        /// </summary>
        II,

        /// <summary>
        /// Internal order number
        ///
        /// Number assigned to an order for internal handling/follow up.
        /// </summary>
        IL,

        /// <summary>
        /// Intermediary broker
        ///
        /// A number that identifies an intermediary broker.
        /// </summary>
        INB,

        /// <summary>
        /// Interchange number new
        ///
        /// Number assigned by the interchange sender to identify one
        /// specific interchange. This number points to the actual
        /// interchange.
        /// </summary>
        INN,

        /// <summary>
        /// Interchange number old
        ///
        /// Number assigned by the interchange sender to identify one
        /// specific interchange. This number points to the previous
        /// interchange.
        /// </summary>
        INO,

        /// <summary>
        /// Import permit identifier
        ///
        /// [1107] Reference number to identify an import licence or
        /// permit.
        /// </summary>
        IP,

        /// <summary>
        /// Invoice number suffix
        ///
        /// A number added at the end of an invoice number.
        /// </summary>
        IS,

        /// <summary>
        /// Internal customer number
        ///
        /// Number assigned by a seller, supplier etc. to identify a
        /// customer within his enterprise.
        /// </summary>
        IT,

        /// <summary>
        /// Invoice document identifier
        /// [1334]
        /// Reference number to identify an invoice.
        /// </summary>
        IV,

        /// <summary>
        /// Job number
        ///
        /// [1043] Identifies a piece of work.
        /// </summary>
        JB,

        /// <summary>
        /// Ending job sequence number
        ///
        /// A number that identifies the ending job sequence.
        /// </summary>
        JE,

        /// <summary>
        /// Shipping label serial number
        ///
        /// The serial number on a shipping label.
        /// </summary>
        LA,

        /// <summary>
        /// Loading authorisation identifier
        ///
        /// [4092] Identifier assigned to the loading authorisation
        /// granted by the forwarding location e.g. railway or airport,
        /// when the consignment is subject to traffic limitations.
        /// </summary>
        LAN,

        /// <summary>
        /// Lower number in range
        ///
        /// Lower number in a range of numbers.
        /// </summary>
        LAR,

        /// <summary>
        /// Lockbox
        ///
        /// Type of cash management system offered by financial
        /// institutions to provide for collection of customers
        /// 'receivables'.
        /// </summary>
        LB,

        /// <summary>
        /// Letter of credit number
        ///
        /// Reference number identifying the letter of credit document.
        /// </summary>
        LC,

        /// <summary>
        /// Document line identifier
        ///
        /// [1156] To identify a line of a document.
        /// </summary>
        LI,

        /// <summary>
        /// Load planning number
        ///
        /// The reference that identifies the load planning number.
        /// </summary>
        LO,

        /// <summary>
        /// Reservation office identifier
        ///
        /// Reference to the office where a reservation was made.
        /// </summary>
        LRC,

        /// <summary>
        /// Bar coded label serial number
        ///
        /// The serial number on a bar code label.
        /// </summary>
        LS,

        /// <summary>
        /// Ship notice/manifest number
        ///
        /// The number assigned to a ship notice or manifest.
        /// </summary>
        MA,

        /// <summary>
        /// Master bill of lading number
        ///
        /// Reference number assigned to a master bill of lading, see:
        /// 1001 = 704.
        /// </summary>
        MB,

        /// <summary>
        /// Manufacturer's part number
        ///
        /// Reference number assigned by the manufacturer to his product
        /// or part.
        /// </summary>
        MF,

        /// <summary>
        /// Zählernummer
        ///
        /// z.B. Zählpunktbezeichnung
        /// </summary>
        MG,

        /// <summary>
        /// Manufacturing order number
        ///
        /// Reference number assigned by manufacturer for a given
        /// production quantity of products.
        /// </summary>
        MH,

        /// <summary>
        /// Message recipient
        ///
        /// A number that identifies the message recipient.
        /// </summary>
        MR,

        /// <summary>
        /// Mailing reference number
        ///
        /// Identifies the party designated by the importer to receive
        /// certain customs correspondence in lieu of its being mailed
        /// directly to the importer.
        /// </summary>
        MRN,

        /// <summary>
        /// Message sender
        ///
        /// A number that identifies the message sender.
        /// </summary>
        MS,

        /// <summary>
        /// Manufacturer's material safety data sheet number
        ///
        /// A number that identifies a manufacturer's material safety
        /// data sheet.
        /// </summary>
        MSS,

        /// <summary>
        /// Master air waybill number
        ///
        /// Reference number assigned to a master air waybill, see: 1001
        /// = 741.
        /// </summary>
        MWB,

        /// <summary>
        /// North American hazardous goods classification number
        ///
        /// Reference to materials designated as hazardous for purposes
        /// of transportation in North American commerce.
        /// </summary>
        NA,

        /// <summary>
        /// Nota Fiscal
        ///
        /// Nota Fiscal is a registration number for shipments /
        /// deliveries within Brazil, issued by the local tax
        /// authorities and mandated for each shipment.
        /// </summary>
        NF,

        /// <summary>
        /// Current invoice number
        ///
        /// Reference number identifying the current invoice.
        /// </summary>
        OH,

        /// <summary>
        /// Vorherige Rechnungsnummer
        /// </summary>
        OI,

        /// <summary>
        /// Order document identifier, buyer assigned
        /// [1022] 
        /// Identifier assigned by the buyer to an order.
        /// </summary>
        ON,

        /// <summary>
        /// Original purchase order
        ///
        /// Reference to the order previously sent.
        /// </summary>
        OP,

        /// <summary>
        /// General order number
        ///
        /// Customs number assigned to imported merchandise that has
        /// been left unclaimed and subsequently moved to a Customs
        /// bonded warehouse for storage.
        /// </summary>
        OR,

        /// <summary>
        /// Payer's financial institution account number
        ///
        /// Originated company account number (ACH transfer), check,
        /// draft or wire.
        /// </summary>
        PB,

        /// <summary>
        /// Production code
        ///
        /// Number assigned by the manufacturer to a specified article
        /// or batch to identify the manufacturing date etc. for
        /// subsequent reference.
        /// </summary>
        PC,

        /// <summary>
        /// Promotion deal number
        ///
        /// Number assigned by a vendor to a special promotion activity.
        /// </summary>
        PD,

        /// <summary>
        /// Plant number
        ///
        /// A number that identifies a plant.
        /// </summary>
        PE,

        /// <summary>
        /// Prime contractor contract number
        ///
        /// Reference number assigned by the client to the contract of
        /// the prime contractor.
        /// </summary>
        PF,

        /// <summary>
        /// Price list version number
        ///
        /// A number that identifies the version of a price list.
        /// </summary>
        PI,

        /// <summary>
        /// Preisliste
        /// </summary>
        PL,

        /// <summary>
        /// Packlistennummer
        /// </summary>
        PK,

        /// <summary>
        /// Bestellantwort
        /// </summary>
        POR,

        /// <summary>
        /// Bestelländerung
        /// </summary>
        PP,

        /// <summary>
        /// Payment reference
        ///
        /// Reference number assigned to a payment.
        /// </summary>
        PQ,

        /// <summary>
        /// Price quote number
        ///
        /// Reference number assigned by the seller to a quote.
        /// </summary>
        PR,

        /// <summary>
        /// Purchase order number suffix
        ///
        /// A number added at the end of a purchase order number.
        /// </summary>
        PS,

        /// <summary>
        /// Prior purchase order number
        ///
        /// Reference number of a purchase order previously sent to the
        /// supplier.
        /// </summary>
        PW,

        /// <summary>
        /// Payee's financial institution account number
        ///
        /// Receiving company account number (ACH transfer), check,
        /// draft or wire.
        /// </summary>
        PY,

        /// <summary>
        /// Remittance advice number
        ///
        /// A number that identifies a remittance advice.
        /// </summary>
        RA,

        /// <summary>
        /// Rail/road routing code
        ///
        /// International Western and Eastern European route code used
        /// in all rail organizations and specified in the international
        /// tariffs (rail tariffs) known by the customers.
        /// </summary>
        RC,

        /// <summary>
        /// Railway consignment note number
        ///
        /// Reference number assigned to a rail consignment note, see:
        /// 1001 = 720.
        /// </summary>
        RCN,

        /// <summary>
        /// Release number
        ///
        /// Reference number assigned to identify a release of a set of
        /// rules, conventions, conditions, etc.
        /// </summary>
        RE,

        /// <summary>
        /// Consignment receipt identifier
        ///
        /// [1150] Reference number assigned to identify a consignment
        /// upon its arrival at its destination.
        /// </summary>
        REN,

        /// <summary>
        /// Export reference number
        ///
        /// Reference number given to an export shipment.
        /// </summary>
        RF,

        /// <summary>
        /// Payer's financial institution transit routing No.(ACH
        ///
        /// ODFI (ACH transfer).
        /// </summary>
        RR,

        /// <summary>
        /// Payee's financial institution transit routing No.
        ///
        /// RDFI Transit routing number (ACH transfer).
        /// </summary>
        RT,

        /// <summary>
        /// Sales person number
        ///
        /// Identification number of a sales person.
        /// </summary>
        SA,

        /// <summary>
        /// Sales region number
        ///
        /// A number that identifies a sales region.
        /// </summary>
        SB,

        /// <summary>
        /// Sales department number
        ///
        /// A number that identifies a sales department.
        /// </summary>
        SD,

        /// <summary>
        /// Seriennummer
        /// </summary>
        SE,

        /// <summary>
        /// Allocated seat
        ///
        /// Reference to a seat allocated to a passenger.
        /// </summary>
        SEA,

        /// <summary>
        /// Ship from
        ///
        /// A number that identifies a ship from location.
        /// </summary>
        SF,

        /// <summary>
        /// Previous highest schedule number
        ///
        /// Number of the latest schedule of a previous period (ODETTE
        /// DELINS).
        /// </summary>
        SH,

        /// <summary>
        /// SID (Shipper's identifying number for shipment)
        ///
        /// A number that identifies the SID (shipper's identification)
        /// number for a shipment.
        /// </summary>
        SI,

        /// <summary>
        /// Sales office number
        ///
        /// A number that identifies a sales office.
        /// </summary>
        SM,

        /// <summary>
        /// Transport equipment seal identifier
        ///
        /// [9308] The identification number of a seal affixed to a
        /// piece of transport equipment.
        /// </summary>
        SN,

        /// <summary>
        /// Scan line
        ///
        /// A number that identifies a scan line.
        /// </summary>
        SP,

        /// <summary>
        /// Equipment sequence number
        ///
        /// (1492) A temporary reference number identifying a particular
        /// piece of equipment within a series of pieces of equipment.
        /// </summary>
        SQ,

        /// <summary>
        /// Shipment reference number
        ///
        /// [1065] Reference number assigned to a shipment.
        /// </summary>
        SRN,

        /// <summary>
        /// Sellers reference number
        ///
        /// Reference number assigned to a transaction by the seller.
        /// </summary>
        SS,

        /// <summary>
        /// Station reference number
        ///
        /// International UIC code assigned to every European rail
        /// station (CIM convention).
        /// </summary>
        STA,

        /// <summary>
        /// Swap order number
        ///
        /// Number assigned by the seller to a swap order (see
        /// definition of DE 1001, code 229).
        /// </summary>
        SW,

        /// <summary>
        /// Specification number
        ///
        /// Number assigned by the issuer to his specification.
        /// </summary>
        SZ,

        /// <summary>
        /// Trucker's bill of lading
        ///
        /// A cargo list/description issued by a motor carrier of
        /// freight.
        /// </summary>
        TB,

        /// <summary>
        /// Terminal operator's consignment reference
        ///
        /// Reference assigned to a consignment by the terminal
        /// operator.
        /// </summary>
        TCR,

        /// <summary>
        /// Telex message number
        ///
        /// Reference number identifying a telex message.
        /// </summary>
        TE,

        /// <summary>
        /// Transfer number
        ///
        /// An extra number assigned to goods or a container which
        /// functions as a reference number or as an authorization
        /// number to get the goods or container released from a certain
        /// party.
        /// </summary>
        TF,

        /// <summary>
        /// TIR carnet number
        ///
        /// Reference number assigned to a TIR carnet.
        /// </summary>
        TI,

        /// <summary>
        /// Transportauftragsnummer
        /// </summary>
        TIN,

        /// <summary>
        /// Tax exemption licence number
        ///
        /// Number assigned by the tax authorities to a party indicating
        /// its tax exemption authorization. This number could relate to
        /// a specified business type, a specified local area or a class
        /// of products.
        /// </summary>
        TL,

        /// <summary>
        /// Transaction reference number
        ///
        /// Reference applied to a transaction between two or more
        /// parties over a defined life cycle; e.g. number applied by
        /// importer or broker to obtain release from Customs, may then
        /// used to control declaration through final accounting
        /// (synonyms: declaration, entry number).
        /// </summary>
        TN,

        /// <summary>
        /// Test report number
        ///
        /// Reference number identifying a test report document relevant
        /// to the product.
        /// </summary>
        TP,

        /// <summary>
        /// Upper number of range
        ///
        /// Upper number in a range of numbers.
        /// </summary>
        UAR,

        /// <summary>
        /// Ultimate customer's reference number
        ///
        /// The originator's reference number as forwarded in a sequence
        /// of parties involved.
        /// </summary>
        UC,

        /// <summary>
        /// Unique consignment reference number
        ///
        /// [1202] Unique reference identifying a particular consignment
        /// of goods. Synonym: UCR, UCRN.
        /// </summary>
        UCN,

        /// <summary>
        /// United Nations Dangerous Goods identifier
        ///
        /// [7124] United Nations Dangerous Goods Identifier (UNDG) is
        /// the unique serial number assigned within the United Nations
        /// to substances and articles contained in a list of the
        /// dangerous goods most commonly carried.
        /// </summary>
        UN,

        /// <summary>
        /// Ultimate customer's order number
        ///
        /// The originator's order number as forwarded in a sequence of
        /// parties involved.
        /// </summary>
        UO,

        /// <summary>
        /// Uniform Resource Identifier
        ///
        /// A string of characters used to identify a name of a resource
        /// on the worldwide web.
        /// </summary>
        URI,

        /// <summary>
        /// VAT registration number
        ///
        /// Unique number assigned by the relevant tax authority to
        /// identify a party for use in relation to Value Added Tax
        /// (VAT).
        /// </summary>
        VA,

        /// <summary>
        /// Vendor contract number
        ///
        /// Number assigned by the vendor to a contract.
        /// </summary>
        VC,

        /// <summary>
        /// Transport equipment gross mass verification reference
        ///
        /// number Reference number identifying the documentation of a
        /// transport equipment gross mass (weight) verification.
        /// </summary>
        VGR,

        /// <summary>
        /// Vessel identifier
        ///
        /// (8123) Reference identifying a vessel.
        /// </summary>
        VM,

        /// <summary>
        /// Auftragsnummer (Lieferant)
        /// </summary>
        VN,

        /// <summary>
        /// Voyage number
        ///
        /// (8028) Reference number assigned to the voyage of the
        /// vessel.
        /// </summary>
        VON,

        /// <summary>
        /// Transport equipment gross mass verification order reference
        ///
        /// Reference number identifying the order for obtaining a
        /// Verified Gross Mass (weight) of a packed transport equipment
        /// as per SOLAS Chapter VI, Regulation 2, paragraphs 4-6.
        /// </summary>
        VOR,

        /// <summary>
        /// Vendor product number
        ///
        /// Number assigned by vendor to another manufacturer's product.
        /// </summary>
        VP,

        /// <summary>
        /// Vendor ID number
        ///
        /// A number that identifies a vendor's identification.
        /// </summary>
        VR,

        /// <summary>
        /// Vendor order number suffix
        ///
        /// The suffix for a vendor order number.
        /// </summary>
        VS,

        /// <summary>
        /// Motor vehicle identification number
        ///
        /// (8213) Reference identifying a motor vehicle used for
        /// transport. Normally is the vehicle registration number.
        /// </summary>
        VT,

        /// <summary>
        /// Voucher number
        ///
        /// Reference number identifying a voucher.
        /// </summary>
        VV,

        /// <summary>
        /// Warehouse entry number
        ///
        /// Entry number under which imported merchandise was placed in
        /// a Customs bonded warehouse.
        /// </summary>
        WE,

        /// <summary>
        /// Weight agreement number
        ///
        /// A number identifying a weight agreement.
        /// </summary>
        WM,

        /// <summary>
        /// Well number
        ///
        /// A number assigned to a shaft sunk into the ground.
        /// </summary>
        WN,

        /// <summary>
        /// Warehouse receipt number
        ///
        /// A number identifying a warehouse receipt.
        /// </summary>
        WR,

        /// <summary>
        /// Warehouse storage location number
        ///
        /// A number identifying a warehouse storage location.
        /// </summary>
        WS,

        /// <summary>
        /// Rail waybill number
        ///
        /// The number on a rail waybill.
        /// </summary>
        WY,

        /// <summary>
        /// Company/place registration number
        ///
        /// Company registration and place as legally required.
        /// </summary>
        XA,

        /// <summary>
        /// Cargo control number
        ///
        /// Reference used to identify and control a carrier and
        /// consignment from initial entry into a country until release
        /// of the cargo by Customs.
        /// </summary>
        XC,

        /// <summary>
        /// Previous cargo control number
        ///
        /// Where a consignment is deconsolidated and/or transferred to
        /// the control of another carrier or freight forwarder (e.g.
        /// housebill, abstract) this references the previous (e.g.
        /// master) cargo control number.
        /// </summary>
        XP,

        /// <summary>
        /// Mutually defined reference number
        ///
        /// Number based on party agreement.
        /// </summary>
        ZZZ
    }
}
