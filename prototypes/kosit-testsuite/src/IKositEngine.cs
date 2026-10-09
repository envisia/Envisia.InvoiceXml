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
namespace Envisia.KositPrototypes.Tests
{
    /// <summary>
    /// A way of running the KoSIT validator (and the Saxon it contains) from .NET, e.g. converted with IKVM or
    /// compiled with GraalVM native-image. The conformance tests run against this interface.
    /// </summary>
    public interface IKositEngine
    {
        /// <summary>Loads a KoSIT configuration (scenarios file and the repository its locations are relative to).</summary>
        IKositValidator LoadConfiguration(string scenariosFile, string repositoryDirectory);

        /// <summary>Applies an XSLT stylesheet (e.g. compiled Schematron) and returns the serialized result.</summary>
        string Transform(string stylesheetFile, byte[] document);

        /// <summary>Evaluates an XPath expression on an XML document and returns its effective boolean value.</summary>
        bool EvaluateBoolean(string xml, string xpath, IReadOnlyDictionary<string, string> namespaces);
    }


    /// <summary>
    /// A loaded KoSIT configuration.
    /// </summary>
    public interface IKositValidator
    {
        /// <summary>Validates a document and returns the KoSIT report (VARL) as XML.</summary>
        string Validate(byte[] document, string name);
    }
}
