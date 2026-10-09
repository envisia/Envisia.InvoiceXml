# Third-party notices

Envisia.InvoiceXml.Validation.GraalVM contains third-party software and validation artefacts. They are used
unchanged unless noted otherwise and keep their licenses.

## Native library (`runtimes/<rid>/native/libkosit.so`, `libkosit.dylib`, `kosit.dll`)

The native library is the standalone jar of the KoSIT validator 1.6.3 from Maven Central
(`org.kosit:validator:1.6.3:standalone`, unmodified), compiled ahead of time with GraalVM native-image
together with the C entry points in `native/java` (Apache License 2.0). It contains:

| Component | Version | License |
|---|---|---|
| KoSIT XML Validator (https://github.com/itplr-kosit/validator), Copyright 2017-2026 Koordinierungsstelle für IT-Standards (KoSIT) | 1.6.3 | Apache License 2.0 |
| Saxon-HE (https://github.com/Saxonica/Saxon-HE), Saxonica | 12.9 | Mozilla Public License 2.0 |
| XML Resolver (https://github.com/xmlresolver/xmlresolver), bundled with Saxon-HE; contains W3C schemas under the W3C license | | Apache License 2.0 |
| Eclipse JAXB runtime, core and TXW2 (https://github.com/eclipse-ee4j/jaxb-ri) | 4.0.8 | Eclipse Distribution License 1.0 |
| Jakarta XML Binding API | 4.0.5 | Eclipse Distribution License 1.0 |
| Jakarta Activation API | 2.1.4 | Eclipse Distribution License 1.0 |
| Eclipse Angus Activation | 2.0.3 | Eclipse Distribution License 1.0 |
| istack common utility code runtime | 4.1.2 | Eclipse Distribution License 1.0 |
| Apache Commons Lang, Copyright 2001-2025 The Apache Software Foundation | 3.20.0 | Apache License 2.0 |
| Apache Commons IO, Copyright 2002-2026 The Apache Software Foundation | 2.22.0 | Apache License 2.0 |
| SLF4J API and simple binding (https://www.slf4j.org/), QOS.ch | 2.0.18 | MIT License |
| Jansi (http://fusesource.github.io/jansi/), FuseSource | 2.4.3 | Apache License 2.0 |
| picocli (https://picocli.info/), Remko Popma | | Apache License 2.0 |
| Classes of the Java class library and the runtime of GraalVM Community Edition (https://www.graalvm.org/) | 25 | GNU General Public License v2 with the Classpath Exception |

The NOTICE file of the KoSIT validator jar reads:

```
KoSIT XML Validator
Copyright 2017-2026 Koordinierungsstelle für IT-Standards (KoSIT)

This product includes software developed by
Koordinierungsstelle für IT-Standards (<https://xeinkauf.de/>).

This product includes software developed at
The Apache Software Foundation (http://www.apache.org/).
```

The source code of Saxon-HE is available at https://github.com/Saxonica/Saxon-HE and on Maven Central
(`net.sf.saxon:Saxon-HE:12.9:sources`), the source code of GraalVM Community Edition at
https://github.com/graalvm/graalvm-community-jdk-releases.

## XRechnung validator configuration (`Configurations/xrechnung/`)

The KoSIT validator configuration for XRechnung 3.0.2 (release 2026-08-31): scenarios, report stylesheets,
the compiled XRechnung Schematron 2.6.0 and the compiled CEN EN 16931 Schematron 1.3.16.

- Source: https://github.com/itplr-kosit/validator-configuration-xrechnung (tag v2026-08-31)
- Copyright: Coordination Office for IT Standards (KoSIT)
- License: Apache License 2.0; the CEN EN 16931 validation artefacts (`resources/ubl/2.1/xsl`,
  `resources/cii/16b/xsl`, from https://github.com/ConnectingEurope/eInvoicing-EN16931) are licensed under
  the European Union Public Licence (EUPL) 1.2.
- UN/CEFACT Cross Industry Invoice D16B schemas (`resources/cii/16b/xsd`): Copyright (C) UN/CEFACT (2016).
  All Rights Reserved. Redistributed under the terms stated in the schema files, which permit copying and
  distribution provided the copyright notice is retained.
- OASIS UBL 2.1 schemas (`resources/ubl/2.1/xsd`): Copyright (c) OASIS Open 2013. All Rights Reserved.
  Distributed under the OASIS IPR Policy, which permits copying and distribution of the specification
  artefacts.

## Factur-X / ZUGFeRD configuration (`Configurations/facturx/`)

The scenarios file was written for this package in the format of the KoSIT validator. It uses the XML schemas
and the compiled Schematron of Factur-X 1.09.2 / ZUGFeRD 2.5.2 (MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED)
from `documentation/zugferd252` of the repository, and the default report stylesheet of the XRechnung
configuration.

- Source: published by FNFE-MPE (https://fnfe-mpe.org/factur-x/) and FeRD (https://www.ferd-net.de), as
  redistributed by the Mustang project (https://github.com/ZUGFeRD/mustangproject)
- The XML schemas are derived from the UN/CEFACT Cross Industry Invoice schemas and contain the UN/CEFACT
  copyright notice.
