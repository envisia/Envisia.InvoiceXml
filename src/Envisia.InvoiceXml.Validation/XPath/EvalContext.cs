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
using System;
using System.Collections.Generic;

namespace Envisia.InvoiceXml.Validation.XPath
{
    /// <summary>
    /// Resolves variables that are not bound inside the expression (in Schematron: the sch:let variables).
    /// </summary>
    internal interface IVariableResolver
    {
        public bool TryResolve(string expandedName, out Sequence value);
    }


    /// <summary>
    /// Loads the documents requested by fn:doc() and document().
    /// </summary>
    internal interface IDocumentResolver
    {
        /// <summary>
        /// Returns the document with the given absolute URI, or throws an exception if it cannot be loaded.
        /// </summary>
        public XdmDocument Resolve(string absoluteUri);
    }


    /// <summary>
    /// State shared by all evaluations of one validation run.
    /// </summary>
    internal sealed class EvaluationEnvironment
    {
        public IDocumentResolver Documents;

        /// <summary>The value of fn:current-dateTime(), stable during the run.</summary>
        public DateTimeOffset Now = DateTimeOffset.Now;

        /// <summary>Keys declared with xsl:key, by expanded name.</summary>
        public Dictionary<string, KeyDefinition> Keys;

        /// <summary>The global variables, visible in xsl:key declarations.</summary>
        public IVariableResolver GlobalVariables;

        /// <summary>Cache of built key indexes: (document id, key name) → value → nodes.</summary>
        public Dictionary<string, Dictionary<string, List<XdmNode>>> KeyIndexes = new Dictionary<string, Dictionary<string, List<XdmNode>>>();
    }


    /// <summary>
    /// An xsl:key declaration.
    /// </summary>
    internal sealed class KeyDefinition
    {
        public string Name;

        /// <summary>The match pattern, compiled to select all matching nodes from the document node.</summary>
        public XPathExpression Match;
        public XPathExpression Use;
    }


    /// <summary>
    /// The dynamic context of an evaluation. The focus (context item, position, size) and the local variables are
    /// mutated while the expression tree is evaluated; every expression restores what it changed.
    /// </summary>
    internal sealed class EvalContext
    {
        public Item ContextItem;
        public int Position;
        public int Size;

        /// <summary>Slots of the variables bound inside the expression (for, let, some/every, function parameters).</summary>
        public Sequence[] Locals;

        /// <summary>The item returned by current() (the context item at the start of the outermost evaluation).</summary>
        public Item CurrentItem;
        public IVariableResolver Variables;
        public EvaluationEnvironment Environment;
        public int CallDepth;


        public EvalContext(EvaluationEnvironment environment)
        {
            Environment = environment;
        }


        public XdmNode ContextNode
        {
            get
            {
                if (ContextItem == null)
                {
                    throw new XPathException("XPDY0002", "The context item is absent");
                }
                XdmNode node = ContextItem as XdmNode;
                if (node == null)
                {
                    throw new XPathException("XPTY0020", "The context item is not a node");
                }
                return node;
            }
        }
    }
}
