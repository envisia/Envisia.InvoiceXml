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
using System.Globalization;
using System.Linq;
using System.Text;

namespace Envisia.InvoiceXml.Validation.XPath
{
    /// <summary>
    /// An XSLT instruction of the body of an xsl:function (the subset that is used in Schematron files:
    /// xsl:variable, xsl:sequence, xsl:value-of, xsl:choose, xsl:if and literal text).
    /// </summary>
    internal abstract class XslInstruction
    {
        public abstract Sequence Evaluate(EvalContext context);
    }


    internal sealed class XslSequence : XslInstruction
    {
        private readonly Expr _Select;


        public XslSequence(Expr select)
        {
            _Select = select;
        }


        public override Sequence Evaluate(EvalContext context)
        {
            return _Select.Evaluate(context);
        }
    }


    internal sealed class XslValueOf : XslInstruction
    {
        private readonly Expr _Select;
        private readonly XslInstruction _Content;
        private readonly string _Separator;


        public XslValueOf(Expr select, XslInstruction content, string separator)
        {
            _Select = select;
            _Content = content;
            _Separator = separator;
        }


        public override Sequence Evaluate(EvalContext context)
        {
            Sequence value = _Select != null ? _Select.Evaluate(context) : _Content.Evaluate(context);
            string separator = _Separator ?? (_Select != null ? " " : "");
            string text = String.Join(separator, Operations.Atomize(value).Select(i => ((AtomicValue)i).StringValue));
            return Sequence.Of(XdmDocumentBuilder.CreateTextNode(text));
        }
    }


    internal sealed class XslText : XslInstruction
    {
        private readonly string _Text;


        public XslText(string text)
        {
            _Text = text;
        }


        public override Sequence Evaluate(EvalContext context)
        {
            return Sequence.Of(XdmDocumentBuilder.CreateTextNode(_Text));
        }
    }


    internal sealed class XslVariable : XslInstruction
    {
        private readonly int _Slot;
        private readonly Expr _Select;
        private readonly XslInstruction _Content;
        private readonly SequenceType _As;
        private readonly string _Name;


        public XslVariable(string name, int slot, Expr select, XslInstruction content, SequenceType asType)
        {
            _Name = name;
            _Slot = slot;
            _Select = select;
            _Content = content;
            _As = asType;
        }


        public override Sequence Evaluate(EvalContext context)
        {
            Sequence value;
            if (_Select != null)
            {
                value = _Select.Evaluate(context);
            }
            else if (_Content == null)
            {
                value = _As != null ? Sequence.Empty : Sequence.EmptyString;
            }
            else if (_As != null)
            {
                value = _Content.Evaluate(context);
            }
            else
            {
                // a variable with content and without "as" is a temporary tree
                StringBuilder sb = new StringBuilder();
                foreach (Item item in _Content.Evaluate(context))
                {
                    sb.Append(item is XdmNode node ? node.StringValue : ((AtomicValue)item).StringValue);
                }
                value = Sequence.Of(XdmDocumentBuilder.CreateDocumentWithText(sb.ToString()));
            }
            if (_As != null)
            {
                value = _As.Convert(value, "the value of variable $" + _Name);
            }
            context.Locals[_Slot] = value;
            return Sequence.Empty;
        }
    }


    internal sealed class XslChoose : XslInstruction
    {
        private readonly List<KeyValuePair<Expr, XslInstruction>> _Whens;
        private readonly XslInstruction _Otherwise;


        public XslChoose(List<KeyValuePair<Expr, XslInstruction>> whens, XslInstruction otherwise)
        {
            _Whens = whens;
            _Otherwise = otherwise;
        }


        public override Sequence Evaluate(EvalContext context)
        {
            foreach (KeyValuePair<Expr, XslInstruction> when in _Whens)
            {
                if (when.Key.EffectiveBooleanValue(context))
                {
                    return when.Value.Evaluate(context);
                }
            }
            return _Otherwise != null ? _Otherwise.Evaluate(context) : Sequence.Empty;
        }
    }


    internal sealed class XslIf : XslInstruction
    {
        private readonly Expr _Test;
        private readonly XslInstruction _Body;


        public XslIf(Expr test, XslInstruction body)
        {
            _Test = test;
            _Body = body;
        }


        public override Sequence Evaluate(EvalContext context)
        {
            return _Test.EffectiveBooleanValue(context) ? _Body.Evaluate(context) : Sequence.Empty;
        }
    }


    /// <summary>
    /// A sequence constructor: the concatenation of the results of its instructions.
    /// </summary>
    internal sealed class XslBlock : XslInstruction
    {
        private readonly XslInstruction[] _Instructions;


        public XslBlock(XslInstruction[] instructions)
        {
            _Instructions = instructions;
        }


        public override Sequence Evaluate(EvalContext context)
        {
            if (_Instructions.Length == 1)
            {
                return _Instructions[0].Evaluate(context);
            }
            List<Item> result = new List<Item>();
            foreach (XslInstruction instruction in _Instructions)
            {
                result.AddRange(instruction.Evaluate(context));
            }
            return Sequence.FromList(result);
        }
    }


    /// <summary>
    /// The key() function: builds an index of the nodes matched by an xsl:key per document on first use.
    /// </summary>
    internal static class KeyLookup
    {
        public static Sequence Evaluate(EvalContext context, string name, Sequence values, XdmNode top, FunctionCallExpr call)
        {
            string expanded = name;
            int colon = name.IndexOf(':');
            if (colon >= 0)
            {
                string ns;
                if (!call.StaticContext.Namespaces.TryGetValue(name.Substring(0, colon), out ns))
                {
                    throw new XPathException("XTDE1260", "Undeclared prefix in key name " + name);
                }
                expanded = "{" + ns + "}" + name.Substring(colon + 1);
            }
            KeyDefinition key;
            if (context.Environment.Keys == null || !context.Environment.Keys.TryGetValue(expanded, out key))
            {
                throw new XPathException("XTDE1260", "No key named " + name + " has been declared");
            }
            XdmNode origin = top ?? context.ContextNode;
            XdmNode root = origin.Root;
            string indexKey = root.Document.Id.ToString(CultureInfo.InvariantCulture) + "|" + expanded;
            Dictionary<string, List<XdmNode>> index;
            if (!context.Environment.KeyIndexes.TryGetValue(indexKey, out index))
            {
                index = _Build(context, key, root);
                context.Environment.KeyIndexes[indexKey] = index;
            }
            List<XdmNode> result = new List<XdmNode>();
            foreach (Item value in values)
            {
                List<XdmNode> nodes;
                if (index.TryGetValue(((AtomicValue)value).StringValue, out nodes))
                {
                    result.AddRange(nodes);
                }
            }
            if (top != null)
            {
                result = result.Where(n => n == top || top.IsAncestorOf(n)).ToList();
            }
            NodeSorting.SortAndDeduplicate(result);
            return Sequence.FromNodes(result);
        }


        private static Dictionary<string, List<XdmNode>> _Build(EvalContext context, KeyDefinition key, XdmNode root)
        {
            Dictionary<string, List<XdmNode>> index = new Dictionary<string, List<XdmNode>>();
            EvalContext keyContext = new EvalContext(context.Environment) { Variables = context.Environment.GlobalVariables };
            foreach (Item item in key.Match.Evaluate(keyContext, root))
            {
                XdmNode node = (XdmNode)item;
                foreach (Item value in Operations.Atomize(key.Use.Evaluate(keyContext, node)))
                {
                    string s = ((AtomicValue)value).StringValue;
                    List<XdmNode> list;
                    if (!index.TryGetValue(s, out list))
                    {
                        list = new List<XdmNode>();
                        index[s] = list;
                    }
                    list.Add(node);
                }
            }
            return index;
        }
    }


    /// <summary>
    /// format-number() with the default decimal format.
    /// </summary>
    internal static class NumberPicture
    {
        public static string Format(NumericValue value, string picture)
        {
            if (value.IsNaN)
            {
                return "NaN";
            }
            if (value is DoubleValue || value is FloatValue)
            {
                return Format(value.ToDouble(), picture);
            }
            return _Format(Operations.ToDecimal(value), picture);
        }


        public static string Format(double value, string picture)
        {
            if (double.IsNaN(value))
            {
                return "NaN";
            }
            string[] parts = picture.Split(';');
            if (double.IsInfinity(value))
            {
                string sub = value < 0 && parts.Length > 1 ? parts[1] : parts[0];
                _Split(sub, out string prefix, out string mantissa, out string suffix);
                return (value < 0 && parts.Length == 1 ? "-" : "") + prefix + "Infinity" + suffix;
            }
            return _Format(NumberFormatting.DoubleToDecimal(value), picture);
        }


        private static void _Split(string picture, out string prefix, out string mantissa, out string suffix)
        {
            int first = -1;
            int last = -1;
            for (int i = 0; i < picture.Length; i++)
            {
                char ch = picture[i];
                if (Char.IsDigit(ch) || ch == '#' || ch == '.' || ch == ',')
                {
                    if (first < 0)
                    {
                        first = i;
                    }
                    last = i;
                }
            }
            if (first < 0)
            {
                prefix = picture;
                mantissa = "";
                suffix = "";
                return;
            }
            prefix = picture.Substring(0, first);
            mantissa = picture.Substring(first, last - first + 1);
            suffix = picture.Substring(last + 1);
        }


        private static string _Format(decimal value, string picture)
        {
            string[] parts = picture.Split(';');
            bool negative = value < 0;
            string sub = negative && parts.Length > 1 ? parts[1] : parts[0];
            _Split(sub, out string prefix, out string mantissa, out string suffix);
            decimal abs = Math.Abs(value);
            if ((prefix + suffix).Contains("%"))
            {
                abs *= 100;
            }
            else if ((prefix + suffix).Contains("‰"))
            {
                abs *= 1000;
            }
            int point = mantissa.IndexOf('.');
            string integerPart = point >= 0 ? mantissa.Substring(0, point) : mantissa;
            string fractionPart = point >= 0 ? mantissa.Substring(point + 1) : "";
            int minInteger = integerPart.Count(Char.IsDigit);
            int lastComma = integerPart.LastIndexOf(',');
            int grouping = lastComma >= 0 ? integerPart.Length - lastComma - 1 : 0;
            int minFraction = fractionPart.Count(Char.IsDigit);
            int maxFraction = minFraction + fractionPart.Count(ch => ch == '#');

            decimal rounded = Math.Round(abs, Math.Min(maxFraction, 28), MidpointRounding.ToEven);
            string digits = rounded.ToString("F" + maxFraction, CultureInfo.InvariantCulture);
            string intDigits = maxFraction > 0 ? digits.Substring(0, digits.IndexOf('.')) : digits;
            string fracDigits = maxFraction > 0 ? digits.Substring(digits.IndexOf('.') + 1) : "";
            while (fracDigits.Length > minFraction && fracDigits.EndsWith("0", StringComparison.Ordinal))
            {
                fracDigits = fracDigits.Substring(0, fracDigits.Length - 1);
            }
            intDigits = intDigits.TrimStart('0');
            if (intDigits.Length < minInteger)
            {
                intDigits = new string('0', minInteger - intDigits.Length) + intDigits;
            }
            if (intDigits.Length == 0 && fracDigits.Length == 0)
            {
                intDigits = "0";
            }
            if (grouping > 0 && intDigits.Length > grouping)
            {
                StringBuilder grouped = new StringBuilder();
                for (int i = 0; i < intDigits.Length; i++)
                {
                    if (i > 0 && (intDigits.Length - i) % grouping == 0)
                    {
                        grouped.Append(',');
                    }
                    grouped.Append(intDigits[i]);
                }
                intDigits = grouped.ToString();
            }
            string result = prefix + intDigits + (fracDigits.Length > 0 ? "." + fracDigits : "") + suffix;
            if (negative && parts.Length == 1 && rounded != 0)
            {
                result = "-" + result;
            }
            return result;
        }
    }
}
