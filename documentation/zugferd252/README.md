# ZUGFeRD 2.5.2 / Factur-X 1.09.2 validation artefacts

XSD schemas, Schematron rules and the compiled Schematron (XSLT 2.0) for all
Factur-X 1.09.2 profiles (MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED).

- Version: 1.09.2 (ZUGFeRD 2.5.2), released 2026-08-04, effective 2026-09-01
- Guideline identifiers (BT-24) are unchanged compared to Factur-X 1.07 / 1.08.
- Source: the official artefacts as redistributed by the Mustang project
  (`validator/src/main/resources/{schema,schematron,xslt}/ZF_250` in
  https://github.com/ZUGFeRD/mustangproject). The EXTENDED XSD is identical to
  the copy shipped by https://github.com/akretion/factur-x.

The complete specification package (PDF specification, code lists, examples)
is available from FeRD (https://www.ferd-net.de) and FNFE-MPE
(https://fnfe-mpe.org/factur-x/).

The unit tests validate the generated invoice XML against these schemas
(see `tests/Envisia.InvoiceXml.Tests/FacturXConformanceTests.cs`).
