# Third-party notices

Envisia.InvoiceXml.Validation embeds the following official validation artefacts. They are used
unchanged unless noted otherwise and keep their licenses. The folders refer to
`src/Envisia.InvoiceXml.Validation/Resources/`, which is embedded into the assembly.

## CEN EN 16931 validation artefacts (`en16931/`)

Schematron rules for EN 16931 in UBL and CII syntax, version 1.3.16.

- Source: https://github.com/ConnectingEurope/eInvoicing-EN16931 (release validation-1.3.16)
- Copyright: CEN/TC 434 and the contributors of the eInvoicing-EN16931 project
- License: European Union Public Licence (EUPL) 1.2, see `en16931/LICENSE.txt`

## XRechnung Schematron and validator configuration (`xrechnung/`, `scenarios/xrechnung.xml`)

Schematron rules for XRechnung 3.0.2 (version 2.6.0) and the scenarios of the KoSIT validator
configuration for XRechnung (release 2026-08-31). The scenario file was adapted to reference the
Schematron sources instead of the compiled XSLT. The texts of the HTML report follow the report
stylesheets of the validator configuration.

- Source: https://github.com/itplr-kosit/xrechnung-schematron and
  https://github.com/itplr-kosit/validator-configuration-xrechnung
- Copyright: Coordination Office for IT Standards (KoSIT)
- License: Apache License 2.0, see `xrechnung/LICENSE.txt`

## Factur-X / ZUGFeRD XML schemas and Schematron (`facturx/`)

XML schemas and Schematron rules of Factur-X 1.09.2 / ZUGFeRD 2.5.2 for the profiles MINIMUM,
BASIC WL, BASIC, EN 16931 and EXTENDED.

- Source: published by FNFE-MPE (https://fnfe-mpe.org/factur-x/) and FeRD (https://www.ferd-net.de),
  as redistributed by the Mustang project (https://github.com/ZUGFeRD/mustangproject)
- The XML schemas are derived from the UN/CEFACT Cross Industry Invoice schemas and contain the
  UN/CEFACT copyright notice.

## UN/CEFACT Cross Industry Invoice D16B schemas (`cii/`)

- Source: UN/CEFACT, Cross Industry Invoice, D16B (SCRDM, CII uncoupled), as distributed with the
  KoSIT validator configuration for XRechnung
- Copyright (C) UN/CEFACT (2016). All Rights Reserved. Redistributed under the terms stated in the
  schema files, which permit copying and distribution provided the copyright notice is retained.

## OASIS UBL 2.1 schemas (`ubl/`)

- Source: OASIS Universal Business Language (UBL) 2.1, http://docs.oasis-open.org/ubl/os-UBL-2.1/
- Copyright (c) OASIS Open 2013. All Rights Reserved. Distributed under the OASIS IPR Policy, which
  permits copying and distribution of the specification artefacts.

## Report format

The XML report follows the schema `report.xsd` (VARL 1.0) of the KoSIT validator
(https://github.com/itplr-kosit/validator, Apache License 2.0); the scenario configuration format
is the one of the KoSIT validator (`scenarios.xsd`).
