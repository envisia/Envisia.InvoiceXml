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
using System.Text;

namespace Envisia.InvoiceXml.Validation.XPath
{
    /// <summary>
    /// Base class of all items of the XPath data model (nodes and atomic values).
    /// </summary>
    internal abstract class Item
    {
    }


    internal enum XdmNodeKind : byte
    {
        Document,
        Element,
        Attribute,
        Text,
        Comment,
        ProcessingInstruction,
        Namespace
    }


    /// <summary>
    /// A node of an immutable XML tree as defined by the XPath 2.0 data model.
    ///
    /// Every node knows its position in document order (<see cref="Order"/>) and the order of the last
    /// node in its subtree (<see cref="EndOrder"/>), so document order comparisons and descendant tests
    /// are integer comparisons.
    /// </summary>
    internal sealed class XdmNode : Item
    {
        internal static readonly XdmNode[] NoNodes = new XdmNode[0];

        public readonly XdmNodeKind Kind;
        public readonly XdmDocument Document;
        public XdmNode Parent;
        public string LocalName = "";
        public string NamespaceUri = "";
        public string Prefix = "";

        /// <summary>Content of text, comment, processing instruction, attribute and namespace nodes.</summary>
        public string Value;
        public XdmNode[] Children = NoNodes;
        public XdmNode[] Attributes = NoNodes;

        /// <summary>Namespace declarations (prefix, uri) of an element, empty prefix for the default namespace.</summary>
        public KeyValuePair<string, string>[] NamespaceDeclarations;
        public int Order;
        public int EndOrder;

        /// <summary>Index of the node in the children (or attributes) of its parent.</summary>
        public int Index;
        public int LineNumber;
        public int LinePosition;
        private string _StringValue;


        public XdmNode(XdmNodeKind kind, XdmDocument document)
        {
            Kind = kind;
            Document = document;
        }


        public bool IsElement => Kind == XdmNodeKind.Element;


        /// <summary>
        /// The string value of the node (dm:string-value).
        /// </summary>
        public string StringValue
        {
            get
            {
                switch (Kind)
                {
                    case XdmNodeKind.Element:
                    case XdmNodeKind.Document:
                        if (_StringValue == null)
                        {
                            _StringValue = _ComputeStringValue();
                        }
                        return _StringValue;
                    default:
                        return Value ?? "";
                }
            }
        }


        private string _ComputeStringValue()
        {
            if (Children.Length == 0)
            {
                return "";
            }
            if (Children.Length == 1 && Children[0].Kind == XdmNodeKind.Text)
            {
                return Children[0].Value;
            }
            StringBuilder sb = new StringBuilder();
            _AppendText(this, sb);
            return sb.ToString();
        }


        private static void _AppendText(XdmNode node, StringBuilder sb)
        {
            foreach (XdmNode child in node.Children)
            {
                if (child.Kind == XdmNodeKind.Text)
                {
                    sb.Append(child.Value);
                }
                else if (child.Kind == XdmNodeKind.Element)
                {
                    _AppendText(child, sb);
                }
            }
        }


        /// <summary>
        /// The lexical QName of the node, as returned by fn:name().
        /// </summary>
        public string Name
        {
            get
            {
                switch (Kind)
                {
                    case XdmNodeKind.Element:
                    case XdmNodeKind.Attribute:
                        return String.IsNullOrEmpty(Prefix) ? LocalName : Prefix + ":" + LocalName;
                    case XdmNodeKind.ProcessingInstruction:
                    case XdmNodeKind.Namespace:
                        return LocalName;
                    default:
                        return "";
                }
            }
        }


        /// <summary>
        /// The root of the tree that contains this node.
        /// </summary>
        public XdmNode Root
        {
            get
            {
                XdmNode node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                }
                return node;
            }
        }


        public bool IsAncestorOf(XdmNode other)
        {
            return other.Document == Document && other.Order > Order && other.Order <= EndOrder;
        }


        /// <summary>
        /// Compares two nodes in document order. Nodes of different trees are ordered by the creation of their trees.
        /// </summary>
        public static int CompareOrder(XdmNode a, XdmNode b)
        {
            if (a.Document != b.Document)
            {
                return a.Document.Id.CompareTo(b.Document.Id);
            }
            return a.Order.CompareTo(b.Order);
        }


        /// <summary>
        /// Returns the in-scope namespaces of an element (prefix → uri), including the xml prefix.
        /// </summary>
        public Dictionary<string, string> GetInScopeNamespaces()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            List<XdmNode> chain = new List<XdmNode>();
            for (XdmNode node = this; node != null; node = node.Parent)
            {
                if (node.Kind == XdmNodeKind.Element)
                {
                    chain.Add(node);
                }
            }
            for (int i = chain.Count - 1; i >= 0; i--)
            {
                KeyValuePair<string, string>[] declarations = chain[i].NamespaceDeclarations;
                if (declarations == null)
                {
                    continue;
                }
                foreach (KeyValuePair<string, string> declaration in declarations)
                {
                    if (String.IsNullOrEmpty(declaration.Value))
                    {
                        result.Remove(declaration.Key);
                    }
                    else
                    {
                        result[declaration.Key] = declaration.Value;
                    }
                }
            }
            result["xml"] = XmlNamespaces.Xml;
            return result;
        }


        /// <summary>
        /// Returns the value of the attribute with the given name or null.
        /// </summary>
        public string GetAttribute(string localName, string namespaceUri = "")
        {
            foreach (XdmNode attribute in Attributes)
            {
                if (attribute.LocalName == localName && attribute.NamespaceUri == namespaceUri)
                {
                    return attribute.Value;
                }
            }
            return null;
        }


        public override string ToString()
        {
            switch (Kind)
            {
                case XdmNodeKind.Document:
                    return "document-node()";
                case XdmNodeKind.Element:
                    return "<" + Name + ">";
                case XdmNodeKind.Attribute:
                    return "@" + Name + "=\"" + Value + "\"";
                case XdmNodeKind.Text:
                    return "text(\"" + Value + "\")";
                default:
                    return Kind.ToString();
            }
        }
    }


    /// <summary>
    /// A tree of <see cref="XdmNode"/>s with its document node, document URI and an index of element and attribute
    /// names that speeds up the descendant axis.
    /// </summary>
    internal sealed class XdmDocument
    {
        private static int _NextId;
        private Dictionary<NameKey, List<XdmNode>> _ElementIndex;
        private Dictionary<NameKey, List<XdmNode>> _AttributeIndex;
        private readonly object _IndexLock = new object();

        public readonly int Id;

        /// <summary>
        /// The document (or, for parentless constructed nodes, the root) node.
        /// </summary>
        public XdmNode Root;

        /// <summary>
        /// All nodes of the tree, indexed by <see cref="XdmNode.Order"/>.
        /// </summary>
        public XdmNode[] AllNodes = XdmNode.NoNodes;

        public string DocumentUri;


        public XdmDocument(string documentUri)
        {
            Id = System.Threading.Interlocked.Increment(ref _NextId);
            DocumentUri = documentUri;
        }


        /// <summary>
        /// Returns all elements (or attributes) with the given expanded name in document order.
        /// </summary>
        public List<XdmNode> GetNodesByName(string namespaceUri, string localName, bool attributes)
        {
            _EnsureIndex();
            Dictionary<NameKey, List<XdmNode>> index = attributes ? _AttributeIndex : _ElementIndex;
            List<XdmNode> list;
            return index.TryGetValue(new NameKey(namespaceUri, localName), out list) ? list : null;
        }


        private void _EnsureIndex()
        {
            if (_ElementIndex != null)
            {
                return;
            }
            lock (_IndexLock)
            {
                if (_ElementIndex != null)
                {
                    return;
                }
                Dictionary<NameKey, List<XdmNode>> elements = new Dictionary<NameKey, List<XdmNode>>();
                Dictionary<NameKey, List<XdmNode>> attributes = new Dictionary<NameKey, List<XdmNode>>();
                foreach (XdmNode node in AllNodes)
                {
                    Dictionary<NameKey, List<XdmNode>> target;
                    if (node.Kind == XdmNodeKind.Element)
                    {
                        target = elements;
                    }
                    else if (node.Kind == XdmNodeKind.Attribute)
                    {
                        target = attributes;
                    }
                    else
                    {
                        continue;
                    }
                    NameKey key = new NameKey(node.NamespaceUri, node.LocalName);
                    List<XdmNode> list;
                    if (!target.TryGetValue(key, out list))
                    {
                        list = new List<XdmNode>();
                        target[key] = list;
                    }
                    list.Add(node);
                }
                _AttributeIndex = attributes;
                _ElementIndex = elements;
            }
        }


        private readonly struct NameKey : IEquatable<NameKey>
        {
            private readonly string _Namespace;
            private readonly string _Local;


            public NameKey(string ns, string local)
            {
                _Namespace = ns ?? "";
                _Local = local;
            }


            public bool Equals(NameKey other)
            {
                return String.Equals(_Local, other._Local, StringComparison.Ordinal) && String.Equals(_Namespace, other._Namespace, StringComparison.Ordinal);
            }


            public override bool Equals(object obj)
            {
                return obj is NameKey other && Equals(other);
            }


            public override int GetHashCode()
            {
                return (_Local.GetHashCode() * 397) ^ _Namespace.GetHashCode();
            }
        }
    }


    internal static class XmlNamespaces
    {
        public const string Xml = "http://www.w3.org/XML/1998/namespace";
        public const string Xmlns = "http://www.w3.org/2000/xmlns/";
        public const string Xs = "http://www.w3.org/2001/XMLSchema";
        public const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";
        public const string Fn = "http://www.w3.org/2005/xpath-functions";
        public const string Math = "http://www.w3.org/2005/xpath-functions/math";
        public const string Err = "http://www.w3.org/2005/xqt-errors";
        public const string Xsl = "http://www.w3.org/1999/XSL/Transform";
        public const string Schematron = "http://purl.oclc.org/dsdl/schematron";
        public const string Svrl = "http://purl.oclc.org/dsdl/svrl";
    }
}
