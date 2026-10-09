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
using System.Collections;
using System.Collections.Generic;

namespace Envisia.InvoiceXml.Validation.XPath
{
    /// <summary>
    /// An immutable sequence of items.
    /// </summary>
    internal sealed class Sequence : IReadOnlyList<Item>
    {
        private static readonly Item[] _NoItems = new Item[0];
        public static readonly Sequence Empty = new Sequence(_NoItems, 0);
        public static readonly Sequence True = new Sequence(new Item[] { BooleanValue.True }, 1);
        public static readonly Sequence False = new Sequence(new Item[] { BooleanValue.False }, 1);
        public static readonly Sequence EmptyString = new Sequence(new Item[] { StringAtomic.Empty }, 1);

        private readonly Item[] _Items;
        private readonly int _Count;


        private Sequence(Item[] items, int count)
        {
            _Items = items;
            _Count = count;
        }


        public static Sequence Of(Item item)
        {
            if (item == null)
            {
                return Empty;
            }
            return new Sequence(new[] { item }, 1);
        }


        public static Sequence Of(bool value)
        {
            return value ? True : False;
        }


        public static Sequence Of(string value)
        {
            return value.Length == 0 ? EmptyString : new Sequence(new Item[] { new StringAtomic(value) }, 1);
        }


        /// <summary>
        /// Wraps a list without copying it; the caller must not modify the list afterwards.
        /// </summary>
        public static Sequence FromList(List<Item> items)
        {
            if (items.Count == 0)
            {
                return Empty;
            }
            return new Sequence(items.ToArray(), items.Count);
        }


        public static Sequence FromArray(Item[] items)
        {
            return items.Length == 0 ? Empty : new Sequence(items, items.Length);
        }


        public static Sequence FromNodes(List<XdmNode> nodes)
        {
            if (nodes.Count == 0)
            {
                return Empty;
            }
            Item[] items = new Item[nodes.Count];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = nodes[i];
            }
            return new Sequence(items, items.Length);
        }


        public int Count => _Count;


        public bool IsEmpty => _Count == 0;


        public Item this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return _Items[index];
            }
        }


        public Item First => _Count > 0 ? _Items[0] : null;


        public IEnumerator<Item> GetEnumerator()
        {
            for (int i = 0; i < _Count; i++)
            {
                yield return _Items[i];
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Returns a sequence with the items in the range [start, start + length).
        /// </summary>
        public Sequence Slice(int start, int length)
        {
            if (length <= 0 || start >= _Count)
            {
                return Empty;
            }
            if (start == 0 && length >= _Count)
            {
                return this;
            }
            length = Math.Min(length, _Count - start);
            Item[] items = new Item[length];
            Array.Copy(_Items, start, items, 0, length);
            return new Sequence(items, length);
        }


        public override string ToString()
        {
            return "(" + String.Join(", ", this) + ")";
        }
    }


    /// <summary>
    /// A dynamic or static error of the XPath / XSLT engine, identified by its W3C error code.
    /// </summary>
    internal sealed class XPathException : Exception
    {
        public string Code { get; }


        public XPathException(string code, string message) : base(code + ": " + message)
        {
            Code = code;
        }


        public XPathException(string code, string message, Exception inner) : base(code + ": " + message, inner)
        {
            Code = code;
        }
    }
}
