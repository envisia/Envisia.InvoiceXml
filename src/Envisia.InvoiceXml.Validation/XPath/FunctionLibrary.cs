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
using System.Linq;

namespace Envisia.InvoiceXml.Validation.XPath
{
    internal delegate Sequence FunctionImplementation(EvalContext context, Sequence[] arguments, FunctionCallExpr call);


    /// <summary>
    /// A built-in function with its signature.
    /// </summary>
    internal sealed class FunctionDefinition
    {
        public string NamespaceUri;
        public string LocalName;
        public int MinArity;
        public int MaxArity;

        /// <summary>Parameter types; the last one is repeated for variadic functions. Null entries: no conversion.</summary>
        public SequenceType[] ParameterTypes;
        public FunctionImplementation Implementation;
        public ResultKind ResultKind;

        /// <summary>True for position() and last().</summary>
        public bool DependsOnPosition;


        public SequenceType GetParameterType(int index)
        {
            if (ParameterTypes.Length == 0)
            {
                return null;
            }
            return ParameterTypes[Math.Min(index, ParameterTypes.Length - 1)];
        }
    }


    /// <summary>
    /// A function defined with xsl:function.
    /// </summary>
    internal sealed class UserFunction
    {
        public string NamespaceUri;
        public string LocalName;
        public SequenceType[] ParameterTypes;
        public int[] ParameterSlots;
        public SequenceType ResultType;
        public XslInstruction Body;
        public int SlotCount;


        public int Arity => ParameterSlots.Length;
    }


    internal sealed class FunctionCallExpr : Expr
    {
        public readonly FunctionDefinition Function;
        public readonly Expr[] Arguments;

        /// <summary>The static context of the call (base URI for doc() and document(), namespaces, functions).</summary>
        public readonly StaticContext StaticContext;


        public FunctionCallExpr(FunctionDefinition function, Expr[] arguments, StaticContext staticContext)
        {
            Function = function;
            Arguments = arguments;
            StaticContext = staticContext;
        }


        public bool IsLast => Function.DependsOnPosition && Function.LocalName == "last" && Arguments.Length == 0;


        public override IEnumerable<Expr> Children => Arguments;


        public override bool UsesPosition => Function.DependsOnPosition || base.UsesPosition;


        public override ResultKind ResultKind => Function.ResultKind;


        public override Sequence Evaluate(EvalContext context)
        {
            Sequence[] arguments = new Sequence[Arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                Sequence value = Arguments[i].Evaluate(context);
                SequenceType type = Function.GetParameterType(i);
                if (type != null)
                {
                    value = type.Convert(value, _ArgumentRole(i));
                }
                arguments[i] = value;
            }
            return Function.Implementation(context, arguments, this);
        }


        public override bool EffectiveBooleanValue(EvalContext context)
        {
            Sequence result = Evaluate(context);
            if (result.Count == 1 && result[0] is BooleanValue b)
            {
                return b.Value;
            }
            return Operations.EffectiveBooleanValue(result);
        }


        private string _ArgumentRole(int index)
        {
            string ordinal;
            switch (index)
            {
                case 0:
                    ordinal = "first";
                    break;
                case 1:
                    ordinal = "second";
                    break;
                case 2:
                    ordinal = "third";
                    break;
                default:
                    ordinal = (index + 1) + "th";
                    break;
            }
            return "the " + ordinal + " argument of " + Function.LocalName + "()";
        }
    }


    internal sealed class UserFunctionCallExpr : Expr
    {
        private const int _MaxDepth = 400;
        public readonly UserFunction Function;
        public readonly Expr[] Arguments;


        public UserFunctionCallExpr(UserFunction function, Expr[] arguments)
        {
            Function = function;
            Arguments = arguments;
        }


        public override IEnumerable<Expr> Children => Arguments;


        public override ResultKind ResultKind
        {
            get
            {
                if (Function.ResultType != null && Function.ResultType.ItemType is AtomicItemType atomic && !Function.ResultType.AllowsMany)
                {
                    if (atomic.Type == XsType.Boolean)
                    {
                        return ResultKind.Boolean;
                    }
                    if (atomic.Type.IsNumeric)
                    {
                        return ResultKind.Numeric;
                    }
                }
                return ResultKind.Unknown;
            }
        }


        public override Sequence Evaluate(EvalContext context)
        {
            Sequence[] arguments = new Sequence[Arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                Sequence value = Arguments[i].Evaluate(context);
                SequenceType type = Function.ParameterTypes[i];
                if (type != null)
                {
                    value = type.Convert(value, "argument " + (i + 1) + " of " + Function.LocalName + "()");
                }
                arguments[i] = value;
            }

            if (context.CallDepth > _MaxDepth)
            {
                throw new XPathException("XTDE0000", "Too many nested function calls (infinite recursion?) in " + Function.LocalName + "()");
            }
            Item savedItem = context.ContextItem;
            int savedPosition = context.Position;
            int savedSize = context.Size;
            Sequence[] savedLocals = context.Locals;
            context.CallDepth++;
            try
            {
                context.ContextItem = null;
                context.Position = 0;
                context.Size = 0;
                context.Locals = new Sequence[Math.Max(1, Function.SlotCount)];
                for (int i = 0; i < arguments.Length; i++)
                {
                    context.Locals[Function.ParameterSlots[i]] = arguments[i];
                }
                Sequence result = Function.Body.Evaluate(context);
                if (Function.ResultType != null)
                {
                    result = Function.ResultType.Convert(result, "the result of " + Function.LocalName + "()");
                }
                return result;
            }
            finally
            {
                context.CallDepth--;
                context.ContextItem = savedItem;
                context.Position = savedPosition;
                context.Size = savedSize;
                context.Locals = savedLocals;
            }
        }
    }


    /// <summary>
    /// The functions known to the compiler: the built-in functions plus the functions declared with xsl:function.
    /// </summary>
    internal sealed class FunctionLibrary
    {
        private static readonly Dictionary<string, List<FunctionDefinition>> _BuiltIns = BuiltInFunctions.Create();
        private readonly Dictionary<string, List<UserFunction>> _UserFunctions = new Dictionary<string, List<UserFunction>>();


        private static string _Key(string namespaceUri, string localName)
        {
            return "{" + namespaceUri + "}" + localName;
        }


        public void Add(UserFunction function)
        {
            string key = _Key(function.NamespaceUri, function.LocalName);
            List<UserFunction> list;
            if (!_UserFunctions.TryGetValue(key, out list))
            {
                list = new List<UserFunction>();
                _UserFunctions[key] = list;
            }
            if (list.Any(f => f.Arity == function.Arity))
            {
                throw new XPathException("XTSE0770", "Duplicate function declaration " + function.LocalName + "#" + function.Arity);
            }
            list.Add(function);
        }


        public UserFunction FindUserFunction(string namespaceUri, string localName, int arity)
        {
            List<UserFunction> list;
            if (_UserFunctions.TryGetValue(_Key(namespaceUri, localName), out list))
            {
                return list.FirstOrDefault(f => f.Arity == arity);
            }
            return null;
        }


        public static FunctionDefinition FindBuiltIn(string namespaceUri, string localName, int arity)
        {
            List<FunctionDefinition> list;
            if (_BuiltIns.TryGetValue(_Key(namespaceUri, localName), out list))
            {
                return list.FirstOrDefault(f => arity >= f.MinArity && arity <= f.MaxArity);
            }
            return null;
        }


        public bool IsAvailable(string namespaceUri, string localName, int arity)
        {
            if (arity < 0)
            {
                return _BuiltIns.ContainsKey(_Key(namespaceUri, localName)) || _UserFunctions.ContainsKey(_Key(namespaceUri, localName));
            }
            return FindBuiltIn(namespaceUri, localName, arity) != null || FindUserFunction(namespaceUri, localName, arity) != null;
        }


        public static bool IsBuiltInName(string namespaceUri, string localName)
        {
            return _BuiltIns.ContainsKey(_Key(namespaceUri, localName));
        }
    }
}
