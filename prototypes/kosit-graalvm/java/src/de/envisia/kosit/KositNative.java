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
package de.envisia.kosit;

import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
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
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import de.kosit.validationtool.api.Check;
import de.kosit.validationtool.api.Configuration;
import de.kosit.validationtool.api.InputFactory;
import de.kosit.validationtool.api.Result;
import de.kosit.validationtool.impl.DefaultCheck;
import de.kosit.validationtool.impl.xml.ProcessorProvider;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XPathCompiler;
import net.sf.saxon.s9api.XPathSelector;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;

/**
 * C entry points of the KoSIT validator, compiled into a shared library with GraalVM native-image. Strings are
 * UTF-8; strings returned to the caller are allocated with malloc and released with kosit_free.
 */
public final class KositNative {

    private static final Map<Long, Check> CHECKS = new ConcurrentHashMap<>();
    private static final AtomicLong IDS = new AtomicLong();
    private static final Processor SAXON = new Processor(false);
    private static final Map<String, XsltExecutable> STYLESHEETS = new ConcurrentHashMap<>();

    private KositNative() {
    }

    /** Loads a configuration; returns a handle > 0, or 0 and an error message in error. */
    @CEntryPoint(name = "kosit_create")
    public static long create(final IsolateThread thread, final CCharPointer scenarios, final CCharPointer repository,
            final CCharPointerPointer error) {
        try {
            final long id = IDS.incrementAndGet();
            CHECKS.put(id, createCheck(CTypeConversion.toJavaString(scenarios), CTypeConversion.toJavaString(repository)));
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
            final Result result = validate(CHECKS.get(handle), toBytes(data, length), CTypeConversion.toJavaString(name));
            output.write(toCString(serialize(result)));
            return result.getAcceptRecommendation().ordinal();
        } catch (final Throwable e) {
            output.write(toCString(e.toString()));
            return -1;
        }
    }

    @CEntryPoint(name = "kosit_destroy")
    public static void destroy(final IsolateThread thread, final long handle) {
        CHECKS.remove(handle);
    }

    /** Applies an XSLT stylesheet (cached by path); returns 0 and the result in output, or -1 and the error. */
    @CEntryPoint(name = "kosit_transform")
    public static int transform(final IsolateThread thread, final CCharPointer stylesheet, final CCharPointer data,
            final int length, final CCharPointerPointer output) {
        try {
            output.write(toCString(transform(CTypeConversion.toJavaString(stylesheet), toBytes(data, length))));
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
            return evaluate(toBytes(data, length), CTypeConversion.toJavaString(expression), CTypeConversion.toJavaString(namespaces)) ? 1 : 0;
        } catch (final Throwable e) {
            error.write(toCString(e.toString()));
            return -1;
        }
    }

    @CEntryPoint(name = "kosit_free")
    public static void free(final IsolateThread thread, final CCharPointer pointer) {
        UnmanagedMemory.free(pointer);
    }

    static Check createCheck(final String scenarios, final String repository) {
        final Processor processor = ProcessorProvider.getProcessor();
        final Configuration configuration = Configuration
                .load(new File(scenarios).getAbsoluteFile().toPath().normalize().toUri(), new File(repository).getAbsoluteFile().toPath().normalize().toUri())
                .build(processor);
        return new DefaultCheck(processor, configuration);
    }

    static Result validate(final Check check, final byte[] document, final String name) {
        return check.checkInput(InputFactory.read(document, name));
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
     * metadata (trace.sh): validates every document of the test suite manifest with each configuration, applies the
     * CEN Schematron and evaluates XPath expressions.
     *
     * usage: KositNative &lt;kosit-testsuite build dir&gt;
     */
    public static void main(final String[] args) throws Exception {
        final File build = new File(args[0]);
        final File xrechnung = new File(build, "config/xrechnung");
        final File facturx = new File(build, "config/facturx");
        final Check[] checks = { createCheck(new File(xrechnung, "scenarios.xml").getPath(), xrechnung.getPath()),
                createCheck(new File(xrechnung, "scenarios-test.xml").getPath(), xrechnung.getPath()),
                createCheck(new File(facturx, "scenarios.xml").getPath(), facturx.getPath()) };
        final List<String> manifest = Files.readAllLines(new File(build, "manifest.txt").toPath(), StandardCharsets.UTF_8);
        String report = null;
        for (final String line : manifest) {
            final String[] parts = line.split("\t");
            final byte[] document = Files.readAllBytes(new File(parts[2]).toPath());
            for (final Check check : checks) {
                report = serialize(validate(check, document, parts[1]));
            }
        }
        final File en16931 = new File(build, "sources/eInvoicing-EN16931");
        for (final String[] test : new String[][] { { "ubl/xslt/EN16931-UBL-validation.xslt", "ubl/examples/ubl-tc434-example1.xml" },
                { "cii/xslt/EN16931-CII-validation.xslt", "cii/examples/CII_example1.xml" } }) {
            transform(new File(en16931, test[0]).getPath(), Files.readAllBytes(new File(en16931, test[1]).toPath()));
        }
        evaluate(report.getBytes(StandardCharsets.UTF_8), "format-dateTime(rep:report/rep:timestamp, '[Y0001][M01][D01]') = format-date(current-date(),'[Y0001][M01][D01]')",
                "rep=http://www.xoev.de/de/validator/varl/1\nhtml=http://www.w3.org/1999/xhtml");
        System.out.println("traced " + manifest.size() + " documents");
    }
}
