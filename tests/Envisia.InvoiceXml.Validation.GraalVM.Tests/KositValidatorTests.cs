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
using Envisia.InvoiceXml.Tests.KositConformance;
using Envisia.InvoiceXml.Tests.Validation;

namespace Envisia.InvoiceXml.Validation.GraalVM.Tests
{
    /// <summary>
    /// The <see cref="IInvoiceValidator"/> contract with the GraalVM implementation.
    /// </summary>
    [TestClass]
    public class KositValidatorContractTests : InvoiceValidatorContractTests
    {
        private static readonly Lazy<IInvoiceValidator> _Validator = new Lazy<IInvoiceValidator>(() => new KositValidator());


        protected override IInvoiceValidator Validator => _Validator.Value;

        protected override Type ImplementationType => typeof(KositValidator);
    }


    /// <summary>
    /// The KoSIT conformance suite (tests/KositConformance) with the GraalVM implementation.
    /// </summary>
    [TestClass]
    public class KositValidatorConformanceTests : KositConformanceTests
    {
        private static readonly IKositEngine _Engine = new GraalVMEngine();


        protected override IKositEngine Engine => _Engine;


        private sealed class GraalVMEngine : IKositEngine
        {
            public IKositValidator LoadConfiguration(string scenariosFile, string repositoryDirectory)
            {
                return new Validator(new KositValidator(KositConfiguration.FromFiles(scenariosFile, repositoryDirectory)));
            }


            public string Transform(string stylesheetFile, byte[] document)
            {
                return KositXml.Transform(stylesheetFile, document);
            }


            public bool EvaluateBoolean(string xml, string xpath, IReadOnlyDictionary<string, string> namespaces)
            {
                return KositXml.EvaluateBoolean(Encoding.UTF8.GetBytes(xml), xpath, namespaces);
            }
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
                return _Validator.ValidateReport(document, name).ReportXml;
            }
        }
    }
}
