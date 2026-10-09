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
namespace Envisia.InvoiceXml.Validation
{
    /// <summary>
    /// The level of a validation message.
    /// </summary>
    public enum ValidationLevel
    {
        /// <summary>Information, does not affect the validity.</summary>
        Information,

        /// <summary>Warning: the document is not valid, but it is acceptable.</summary>
        Warning,

        /// <summary>Error: the document is rejected.</summary>
        Error
    }


    /// <summary>
    /// The recommendation of the validator.
    /// </summary>
    public enum AcceptRecommendation
    {
        /// <summary>The validation could not be completed (processing error).</summary>
        Undefined,

        /// <summary>The document can be accepted and processed.</summary>
        Accept,

        /// <summary>The document should be rejected.</summary>
        Reject
    }
}
