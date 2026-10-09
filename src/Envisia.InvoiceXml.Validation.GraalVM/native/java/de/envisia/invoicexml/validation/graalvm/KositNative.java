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
package de.envisia.invoicexml.validation.graalvm;

import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;

import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.UnmanagedMemory;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CCharPointerPointer;
import org.graalvm.word.WordFactory;

import de.kosit.validationtool.api.Check;
import de.kosit.validationtool.api.Configuration;
import de.kosit.validationtool.api.InputFactory;
import de.kosit.validationtool.api.Result;
import de.kosit.validationtool.impl.DefaultCheck;
import de.kosit.validationtool.impl.Scenario;
import de.kosit.validationtool.impl.xml.ProcessorProvider;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XPathCompiler;
import net.sf.saxon.s9api.XPathSelector;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;

/**
 * C entry points of the KoSIT validator for Envisia.InvoiceXml.Validation.GraalVM, compiled into a shared library
 * with GraalVM native-image. Strings passed in and returned are NUL terminated UTF-8; returned strings are allocated
 * with malloc and released with kosit_free.
 */
public final class KositNative {

    /** A KoSIT configuration and the check for it. */
    private static final class Entry {

        final Configuration configuration;

        final Check check;

        Entry(final Configuration configuration, final Check check) {
            this.configuration = configuration;
            this.check = check;
        }
    }

    /** Discards the messages of Saxon (e.g. parser errors, which the report contains as well). */
    private static final class SilentLogger extends Logger {

        @Override
        public void println(final String message, final int severity) {
            // discarded
        }
    }

    private static final Map<Long, List<Entry>> VALIDATORS = new ConcurrentHashMap<>();
    private static final AtomicLong IDS = new AtomicLong();
    private static final Processor SAXON = silent(new Processor(false));
    private static final Map<String, XsltExecutable> STYLESHEETS = new ConcurrentHashMap<>();

    private KositNative() {
    }

    /**
     * Loads configurations, one "scenarios file TAB repository directory" per line, tried in this order. Returns a
     * handle > 0, or 0 and an error message in error.
     */
    @CEntryPoint(name = "kosit_create")
    public static long create(final IsolateThread thread, final CCharPointer configurations, final CCharPointerPointer error) {
        try {
            final long id = IDS.incrementAndGet();
            VALIDATORS.put(id, load(utf8(configurations)));
            error.write(WordFactory.nullPointer());
            return id;
        } catch (final Throwable e) {
            error.write(toCString(e.toString()));
            return 0;
        }
    }

    /**
     * Validates a document; returns the accept recommendation (0 undefined, 1 acceptable, 2 reject) or -1 on errors.
     * The report (or the error message) is written to output.
     */
    @CEntryPoint(name = "kosit_validate")
    public static int validate(final IsolateThread thread, final long handle, final CCharPointer data, final int length,
            final CCharPointer name, final CCharPointerPointer output) {
        try {
            final Result result = validate(VALIDATORS.get(handle), toBytes(data, length), utf8(name));
            output.write(toCString(serialize(result)));
            return result.getAcceptRecommendation().ordinal();
        } catch (final Throwable e) {
            output.write(toCString(e.toString()));
            return -1;
        }
    }

    @CEntryPoint(name = "kosit_destroy")
    public static void destroy(final IsolateThread thread, final long handle) {
        VALIDATORS.remove(handle);
    }

    /** Applies an XSLT stylesheet (cached by path); returns 0 and the result in output, or -1 and the error. */
    @CEntryPoint(name = "kosit_transform")
    public static int transform(final IsolateThread thread, final CCharPointer stylesheet, final CCharPointer data,
            final int length, final CCharPointerPointer output) {
        try {
            output.write(toCString(transform(utf8(stylesheet), toBytes(data, length))));
            return 0;
        } catch (final Throwable e) {
            output.write(toCString(e.toString()));
            return -1;
        }
    }

    /**
     * Evaluates an XPath expression on an XML document; namespaces are "prefix=uri" lines. Returns 1 (true), 0
     * (false) or -1 and the error in error.
     */
    @CEntryPoint(name = "kosit_xpath")
    public static int xpath(final IsolateThread thread, final CCharPointer data, final int length, final CCharPointer expression,
            final CCharPointer namespaces, final CCharPointerPointer error) {
        try {
            error.write(WordFactory.nullPointer());
            return evaluate(toBytes(data, length), utf8(expression), utf8(namespaces)) ? 1 : 0;
        } catch (final Throwable e) {
            error.write(toCString(e.toString()));
            return -1;
        }
    }

    @CEntryPoint(name = "kosit_free")
    public static void free(final IsolateThread thread, final CCharPointer pointer) {
        UnmanagedMemory.free(pointer);
    }

    static List<Entry> load(final String configurations) {
        // KoSIT logs every loaded resource and checked document (slf4j-simple, standard error); the report and the
        // exceptions carry everything the caller needs
        if (System.getProperty("org.slf4j.simpleLogger.defaultLogLevel") == null) {
            System.setProperty("org.slf4j.simpleLogger.defaultLogLevel", "off");
        }
        final Processor processor = silent(ProcessorProvider.getProcessor());
        final List<Entry> entries = new ArrayList<>();
        for (final String line : configurations.split("\n")) {
            final String[] parts = line.split("\t");
            if (parts.length != 2) {
                continue;
            }
            final Configuration configuration = Configuration
                    .load(new File(parts[0]).getAbsoluteFile().toPath().normalize().toUri(), new File(parts[1]).getAbsoluteFile().toPath().normalize().toUri())
                    .build(processor);
            entries.add(new Entry(configuration, new DefaultCheck(processor, configuration)));
        }
        if (entries.isEmpty()) {
            throw new IllegalArgumentException("No configuration given");
        }
        return entries;
    }

    /**
     * Validates with the first configuration in which a scenario matches the document. KoSIT requires exactly one
     * matching scenario across all configurations of a check, so the configurations are checked one after the other,
     * as Envisia.InvoiceXml.Validation does: a configuration with several matching scenarios decides as well (KoSIT
     * then reports the ambiguity), without any match the first configuration creates the report.
     */
    static Result validate(final List<Entry> entries, final byte[] document, final String name) {
        Entry selected = entries.get(0);
        if (entries.size() > 1) {
            XdmNode node = null;
            try {
                node = ProcessorProvider.getProcessor().newDocumentBuilder().build(new StreamSource(new ByteArrayInputStream(document)));
            } catch (final SaxonApiException e) {
                // not well-formed: the first configuration reports it
            }
            if (node != null) {
                for (final Entry entry : entries) {
                    if (matchingScenarios(entry.configuration, node) > 0) {
                        selected = entry;
                        break;
                    }
                }
            }
        }
        return selected.check.checkInput(InputFactory.read(document, name));
    }

    private static Processor silent(final Processor processor) {
        processor.getUnderlyingConfiguration().setLogger(new SilentLogger());
        return processor;
    }

    private static int matchingScenarios(final Configuration configuration, final XdmNode document) {
        int count = 0;
        for (final Scenario scenario : configuration.getScenarios()) {
            try {
                final XPathSelector selector = scenario.getMatchSelector();
                selector.setContextItem(document);
                if (selector.effectiveBooleanValue()) {
                    count++;
                }
            } catch (final SaxonApiException e) {
                // as in KoSIT: a failing match expression does not match
            }
        }
        return count;
    }

    static String serialize(final Result result) throws Exception {
        final StringWriter writer = new StringWriter();
        TransformerFactory.newInstance().newTransformer().transform(new DOMSource(result.getReportDocument()), new StreamResult(writer));
        return writer.toString();
    }

    static String transform(final String stylesheet, final byte[] document) throws Exception {
        XsltExecutable executable = STYLESHEETS.get(stylesheet);
        if (executable == null) {
            executable = SAXON.newXsltCompiler().compile(new StreamSource(new File(stylesheet)));
            STYLESHEETS.put(stylesheet, executable);
        }
        final XsltTransformer transformer = executable.load();
        transformer.setSource(new StreamSource(new ByteArrayInputStream(document)));
        final StringWriter writer = new StringWriter();
        transformer.setDestination(SAXON.newSerializer(writer));
        transformer.transform();
        return writer.toString();
    }

    static boolean evaluate(final byte[] xml, final String expression, final String namespaces) throws Exception {
        final XPathCompiler compiler = SAXON.newXPathCompiler();
        for (final String line : namespaces.split("\n")) {
            final int equals = line.indexOf('=');
            if (equals > 0) {
                compiler.declareNamespace(line.substring(0, equals), line.substring(equals + 1));
            }
        }
        final XdmNode document = SAXON.newDocumentBuilder().build(new StreamSource(new ByteArrayInputStream(xml)));
        final XPathSelector selector = compiler.compile(expression).load();
        selector.setContextItem(document);
        return selector.effectiveBooleanValue();
    }

    private static String utf8(final CCharPointer pointer) {
        int length = 0;
        while (pointer.read(length) != 0) {
            length++;
        }
        return new String(toBytes(pointer, length), StandardCharsets.UTF_8);
    }

    private static byte[] toBytes(final CCharPointer data, final int length) {
        final byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++) {
            bytes[i] = data.read(i);
        }
        return bytes;
    }

    private static CCharPointer toCString(final String value) {
        final byte[] bytes = value.getBytes(StandardCharsets.UTF_8);
        final CCharPointer pointer = UnmanagedMemory.malloc(bytes.length + 1);
        for (int i = 0; i < bytes.length; i++) {
            pointer.write(i, bytes[i]);
        }
        pointer.write(bytes.length, (byte) 0);
        return pointer;
    }

    /**
     * Runs the code paths of the library on the JVM, for the native-image tracing agent that records the reflection
     * metadata (trace.sh): validates the given documents with the given configurations (in order, as the library
     * does), applies an XSLT stylesheet and evaluates an XPath expression.
     *
     * usage: KositNative &lt;configurations file ("scenarios TAB repository" lines)&gt; &lt;stylesheet&gt; &lt;documents...&gt;
     */
    public static void main(final String[] args) throws Exception {
        final List<Entry> entries = load(new String(Files.readAllBytes(new File(args[0]).toPath()), StandardCharsets.UTF_8));
        String report = null;
        for (int i = 2; i < args.length; i++) {
            final byte[] document = Files.readAllBytes(new File(args[i]).toPath());
            report = serialize(validate(entries, document, new File(args[i]).getName()));
            try {
                transform(args[1], document);
            } catch (final SaxonApiException e) {
                // documents that are not well-formed
            }
        }
        evaluate(report.getBytes(StandardCharsets.UTF_8), "format-dateTime(rep:report/rep:timestamp, '[Y0001][M01][D01]') = format-date(current-date(),'[Y0001][M01][D01]')",
                "rep=http://www.xoev.de/de/validator/varl/1\nhtml=http://www.w3.org/1999/xhtml");
        System.out.println("traced " + (args.length - 2) + " documents");
    }
}
