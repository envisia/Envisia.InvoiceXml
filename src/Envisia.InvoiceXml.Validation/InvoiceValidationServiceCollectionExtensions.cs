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
using Envisia.InvoiceXml.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Registers the .NET implementation of <see cref="IInvoiceValidator"/>.
    /// </summary>
    public static class InvoiceXmlValidationServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="InvoiceXmlValidator"/> with the built-in configurations (XRechnung, ZUGFeRD / Factur-X,
        /// EN 16931) as singleton <see cref="IInvoiceValidator"/>. Envisia.InvoiceXml.Validation.GraalVM has a method
        /// with the same name, so the implementation is chosen by the referenced package.
        /// </summary>
        public static IServiceCollection AddInvoiceValidator(this IServiceCollection services)
        {
            return services.AddInvoiceValidator(new ValidatorOptions());
        }


        /// <summary>
        /// Registers <see cref="InvoiceXmlValidator"/> with the built-in configurations and the given options as
        /// singleton <see cref="IInvoiceValidator"/>.
        /// </summary>
        public static IServiceCollection AddInvoiceValidator(this IServiceCollection services, ValidatorOptions options)
        {
            services.AddSingleton(_ => new InvoiceXmlValidator(options));
            services.AddSingleton<IInvoiceValidator>(provider => provider.GetRequiredService<InvoiceXmlValidator>());
            return services;
        }
    }
}
