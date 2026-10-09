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
using System.Text;
using Envisia.KositNative;

namespace Envisia.KositPrototypes.Tests
{
    /// <summary>
    /// The KoSIT validator compiled into a native library with GraalVM native-image, called through P/Invoke.
    /// </summary>
    public sealed class GraalVmEngine : IKositEngine
    {
        public IKositValidator LoadConfiguration(string scenariosFile, string repositoryDirectory)
        {
            return new Validator(new KositValidator(scenariosFile, repositoryDirectory));
        }


        public string Transform(string stylesheetFile, byte[] document)
        {
            return KositXml.Transform(stylesheetFile, document);
        }


        public bool EvaluateBoolean(string xml, string xpath, IReadOnlyDictionary<string, string> namespaces)
        {
            return KositXml.EvaluateBoolean(Encoding.UTF8.GetBytes(xml), xpath, namespaces);
        }


        private sealed class Validator : IKositValidator
        {
            private readonly KositValidator _Validator;


            public Validator(KositValidator validator)
            {
                _Validator = validator;
            }


            public string Validate(byte[] document, string name)
            {
                return _Validator.Validate(document, name).Xml;
            }
        }
    }


    /// <summary>
    /// The conformance tests of prototypes/kosit-testsuite with the native library.
    /// </summary>
    [TestClass]
    public class GraalVmConformanceTests : KositConformanceTests
    {
        private static readonly IKositEngine _Engine = new GraalVmEngine();


        protected override IKositEngine Engine => _Engine;
    }
}
