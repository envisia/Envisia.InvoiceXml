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

import java.io.File;
import java.io.PrintWriter;
import java.io.StringReader;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

import de.kosit.validationtool.api.Check;
import de.kosit.validationtool.api.Configuration;
import de.kosit.validationtool.api.InputFactory;
import de.kosit.validationtool.impl.DefaultCheck;
import de.kosit.validationtool.impl.xml.ProcessorProvider;
import net.sf.saxon.s9api.Processor;

/**
 * Creates the reference of the KoSIT prototype test suite on the JVM:
 *
 * - validates every document of build/manifest.txt with the official KoSIT validator and writes the report summaries
 *   (same format as ReportSummary.cs),
 * - runs the CEN EN 16931 unit tests with CEN's compiled Schematron and Saxon and writes the expectations that are not
 *   met (same format as KositConformanceTests.CenUnitTestsPass).
 *
 * usage: java -cp validator-standalone.jar ReferenceRunner.java <build dir> <report reference> <CEN unit test reference>
 */
public final class ReferenceRunner {

    private static final String REP = "http://www.xoev.de/de/validator/varl/1";
    private static final String SCN = "http://www.xoev.de/de/validator/framework/1/scenarios";

    public static void main(final String[] args) throws Exception {
        final File build = new File(args[0]);
        final Processor processor = ProcessorProvider.getProcessor();
        final Map<String, Check> checks = new HashMap<>();
        final List<String> manifest = Files.readAllLines(new File(build, "manifest.txt").toPath(), StandardCharsets.UTF_8);
        cenUnitTests(new File(build, "sources/eInvoicing-EN16931"), args[2]);
        try (PrintWriter out = new PrintWriter(args[1], "UTF-8")) {
            for (final String line : manifest) {
                final String[] parts = line.split("\t");
                final Check check = checks.computeIfAbsent(parts[0], set -> {
                    final File config = new File(build, "config/" + set);
                    final Configuration configuration = Configuration.load(new File(config, "scenarios.xml").toURI(), config.toURI()).build(processor);
                    return new DefaultCheck(processor, configuration);
                });
                final byte[] document = Files.readAllBytes(new File(parts[2]).toPath());
                final Document report = reparse(check.checkInput(InputFactory.read(document, parts[1])).getReportDocument());
                out.print("## " + parts[0] + " " + parts[1] + "\n");
                out.print(summarize(report));
            }
        }
    }

    private static final String VEFA = "http://difi.no/xsd/vefa/validator/1.0";
    private static final String SVRL = "http://purl.oclc.org/dsdl/svrl";

    /** Writes "<folder>/<file> TAB <failure>" for every expectation of the CEN unit tests that Saxon does not meet. */
    static void cenUnitTests(final File en16931, final String output) throws Exception {
        final Processor saxon = new Processor(false);
        final DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        try (PrintWriter out = new PrintWriter(output, "UTF-8")) {
            for (final String folder : new String[] { "Invoice-unit-UBL", "CreditNote-unit-UBL", "cii" }) {
                final String stylesheet = folder.equals("cii") ? "cii/xslt/EN16931-CII-validation.xslt" : "ubl/xslt/EN16931-UBL-validation.xslt";
                final net.sf.saxon.s9api.XsltExecutable executable = saxon.newXsltCompiler().compile(new StreamSource(new File(en16931, stylesheet)));
                final File[] files = new File(en16931, "test/" + folder).listFiles((dir, name) -> name.endsWith(".xml"));
                java.util.Arrays.sort(files);
                for (final File file : files) {
                    final Document testSet = factory.newDocumentBuilder().parse(file);
                    int index = 0;
                    for (Node test = testSet.getDocumentElement().getFirstChild(); test != null; test = test.getNextSibling()) {
                        if (!(test instanceof Element) || !VEFA.equals(test.getNamespaceURI()) || !"test".equals(test.getLocalName())) {
                            continue;
                        }
                        index++;
                        Element expectations = null;
                        Element document = null;
                        for (Node child = test.getFirstChild(); child != null; child = child.getNextSibling()) {
                            if (child instanceof Element) {
                                if (VEFA.equals(child.getNamespaceURI()) && "assert".equals(child.getLocalName())) {
                                    expectations = (Element) child;
                                } else {
                                    document = (Element) child;
                                }
                            }
                        }
                        final StringWriter svrl = new StringWriter();
                        final net.sf.saxon.s9api.XsltTransformer transformer = executable.load();
                        // the test document as a document of its own (as the .NET tests pass it)
                        final StringWriter source = new StringWriter();
                        TransformerFactory.newInstance().newTransformer().transform(new DOMSource(document), new StreamResult(source));
                        transformer.setSource(new StreamSource(new StringReader(source.toString())));
                        transformer.setDestination(saxon.newSerializer(svrl));
                        transformer.transform();
                        final Document result = factory.newDocumentBuilder().parse(new InputSource(new StringReader(svrl.toString())));
                        final NodeList failed = result.getElementsByTagNameNS(SVRL, "failed-assert");
                        for (Node e = expectations.getFirstChild(); e != null; e = e.getNextSibling()) {
                            if (!(e instanceof Element)) {
                                continue;
                            }
                            final String kind = e.getLocalName();
                            final String id = e.getTextContent().trim();
                            String failure = null;
                            if (kind.equals("success")) {
                                for (int i = 0; i < failed.getLength(); i++) {
                                    if (id.equals(((Element) failed.item(i)).getAttribute("id"))) {
                                        failure = id + " was expected to pass";
                                        break;
                                    }
                                }
                            } else if (kind.equals("error") || kind.equals("warning")) {
                                final String flag = kind.equals("error") ? "fatal" : "warning";
                                final String number = ((Element) e).getAttribute("number");
                                final int expected = number.isEmpty() ? -1 : Integer.parseInt(number);
                                int actual = 0;
                                for (int i = 0; i < failed.getLength(); i++) {
                                    final Element f = (Element) failed.item(i);
                                    if (id.equals(f.getAttribute("id")) && flag.equals(f.getAttribute("flag"))) {
                                        actual++;
                                    }
                                }
                                if (expected >= 0 ? actual != expected : actual == 0) {
                                    failure = id + " expected as " + flag + (expected >= 0 ? " " + expected + " times" : "") + ", found " + actual;
                                }
                            }
                            if (failure != null) {
                                out.print(folder + "/" + file.getName() + "\ttest " + index + ": " + failure + "\n");
                            }
                        }
                    }
                }
            }
        }
    }

    /**
     * Serializes the report (a read-only Saxon DOM wrapper) and parses it again with a namespace aware DOM parser, as
     * the .NET tests parse the serialized report.
     */
    static Document reparse(final Document report) throws Exception {
        final StringWriter writer = new StringWriter();
        TransformerFactory.newInstance().newTransformer().transform(new DOMSource(report), new StreamResult(writer));
        final DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        return factory.newDocumentBuilder().parse(new InputSource(new StringReader(writer.toString())));
    }

    /** The summary of a report: validity, scenario, assessment, steps and messages. */
    static String summarize(final Document report) {
        final StringBuilder sb = new StringBuilder();
        final Element root = report.getDocumentElement();
        sb.append("valid=").append(root.getAttribute("valid")).append('\n');
        final NodeList names = root.getElementsByTagNameNS(SCN, "name");
        String scenario = "NONE";
        for (int i = 0; i < names.getLength(); i++) {
            final Node parent = names.item(i).getParentNode();
            if (SCN.equals(parent.getNamespaceURI()) && "scenario".equals(parent.getLocalName())) {
                scenario = names.item(i).getTextContent().trim();
                break;
            }
        }
        sb.append("scenario=").append(scenario).append('\n');
        String assessment = "";
        final NodeList assessments = root.getElementsByTagNameNS(REP, "assessment");
        if (assessments.getLength() > 0) {
            for (Node child = assessments.item(0).getFirstChild(); child != null; child = child.getNextSibling()) {
                if (child.getNodeType() == Node.ELEMENT_NODE) {
                    assessment = child.getLocalName();
                    break;
                }
            }
        }
        sb.append("assessment=").append(assessment).append('\n');
        final NodeList steps = root.getElementsByTagNameNS(REP, "validationStepResult");
        for (int i = 0; i < steps.getLength(); i++) {
            final Element step = (Element) steps.item(i);
            sb.append("step ").append(step.getAttribute("id")).append(" valid=").append(step.getAttribute("valid")).append('\n');
            for (Node child = step.getFirstChild(); child != null; child = child.getNextSibling()) {
                if (child.getNodeType() == Node.ELEMENT_NODE && REP.equals(child.getNamespaceURI()) && "message".equals(child.getLocalName())) {
                    final Element message = (Element) child;
                    sb.append("  ").append(message.getAttribute("id")).append('|').append(message.getAttribute("code")).append('|')
                            .append(message.getAttribute("level")).append('|').append(message.getAttribute("xpathLocation")).append('|')
                            .append(normalize(message.getTextContent())).append('\n');
                }
            }
        }
        return sb.toString();
    }

    /** Collapses runs of whitespace (characters up to U+0020 and U+00A0) into one space and trims. */
    static String normalize(final String text) {
        final StringBuilder sb = new StringBuilder();
        boolean space = false;
        for (int i = 0; i < text.length(); i++) {
            final char c = text.charAt(i);
            if (c <= ' ' || c == ' ') {
                space = sb.length() > 0;
            } else {
                if (space) {
                    sb.append(' ');
                    space = false;
                }
                sb.append(c);
            }
        }
        return sb.toString();
    }
}
