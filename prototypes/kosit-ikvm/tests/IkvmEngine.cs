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
using System.Collections.Concurrent;
using de.kosit.validationtool.api;
using de.kosit.validationtool.impl;
using de.kosit.validationtool.impl.xml;
using javax.xml.transform.stream;
using net.sf.saxon.s9api;

namespace Envisia.KositPrototypes.Tests
{
    /// <summary>
    /// The KoSIT validator converted to .NET with IKVM, called through its Java API.
    /// </summary>
    public sealed class IkvmEngine : IKositEngine
    {
        private readonly Processor _Saxon = new Processor(false);
        private readonly ConcurrentDictionary<string, XsltExecutable> _Stylesheets = new ConcurrentDictionary<string, XsltExecutable>();


        public IKositValidator LoadConfiguration(string scenariosFile, string repositoryDirectory)
        {
            Processor processor = ProcessorProvider.getProcessor();
            de.kosit.validationtool.api.Configuration configuration =
                de.kosit.validationtool.api.Configuration.load(new java.io.File(scenariosFile).toURI(), new java.io.File(repositoryDirectory).toURI()).build(processor);
            return new Validator(new DefaultCheck(processor, configuration));
        }


        public string Transform(string stylesheetFile, byte[] document)
        {
            XsltExecutable stylesheet = _Stylesheets.GetOrAdd(stylesheetFile, f => _Saxon.newXsltCompiler().compile(new StreamSource(new java.io.File(f))));
            XsltTransformer transformer = stylesheet.load();
            transformer.setSource(new StreamSource(new java.io.ByteArrayInputStream(document)));
            java.io.StringWriter writer = new java.io.StringWriter();
            transformer.setDestination(_Saxon.newSerializer(writer));
            transformer.transform();
            return writer.toString();
        }


        public bool EvaluateBoolean(string xml, string xpath, IReadOnlyDictionary<string, string> namespaces)
        {
            XPathCompiler compiler = _Saxon.newXPathCompiler();
            foreach (KeyValuePair<string, string> ns in namespaces)
            {
                compiler.declareNamespace(ns.Key, ns.Value);
            }
            XdmNode document = _Saxon.newDocumentBuilder().build(new StreamSource(new java.io.StringReader(xml)));
            XPathSelector selector = compiler.compile(xpath).load();
            selector.setContextItem(document);
            return selector.effectiveBooleanValue();
        }


        private sealed class Validator : IKositValidator
        {
            private readonly Check _Check;


            public Validator(Check check)
            {
                _Check = check;
            }


            public string Validate(byte[] document, string name)
            {
                Result result = _Check.checkInput(InputFactory.read(document, name));
                java.io.StringWriter writer = new java.io.StringWriter();
                javax.xml.transform.TransformerFactory.newInstance().newTransformer()
                    .transform(new javax.xml.transform.dom.DOMSource(result.getReportDocument()), new StreamResult(writer));
                return writer.toString();
            }
        }
    }


    /// <summary>
    /// The conformance tests of prototypes/kosit-testsuite with the IKVM engine.
    /// </summary>
    [TestClass]
    public class IkvmConformanceTests : KositConformanceTests
    {
        private static readonly Lazy<IKositEngine> _Engine = new Lazy<IKositEngine>(() => new IkvmEngine());


        protected override IKositEngine Engine => _Engine.Value;

        protected override IReadOnlyDictionary<string, string> KnownDifferences { get; } =
            File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "known-differences.txt"))
                .Where(l => l.Length > 0 && !l.StartsWith('#'))
                .Select(l => l.Split('\t', 2))
                .ToDictionary(p => p[0], p => p[1]);
    }
}
