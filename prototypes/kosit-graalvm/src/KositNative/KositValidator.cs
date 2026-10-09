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
using System.Runtime.InteropServices;
using System.Text;

namespace Envisia.KositNative
{
    /// <summary>
    /// The accept recommendation of the KoSIT validator.
    /// </summary>
    public enum AcceptRecommendation
    {
        Undefined = 0,
        Acceptable = 1,
        Reject = 2
    }


    /// <summary>
    /// The result of a validation: the accept recommendation and the KoSIT report (VARL) as XML.
    /// </summary>
    public sealed record KositReport(AcceptRecommendation Recommendation, string Xml);


    /// <summary>
    /// The KoSIT validator (compiled with GraalVM native-image) with one configuration. Thread-safe.
    /// </summary>
    public sealed class KositValidator : IDisposable
    {
        private long _Handle;


        /// <summary>Loads a KoSIT configuration: the scenarios file and the repository its resources are relative to.</summary>
        public KositValidator(string scenariosFile, string repositoryDirectory)
        {
            _Handle = KositLibrary.Create(Path.GetFullPath(scenariosFile), Path.GetFullPath(repositoryDirectory));
        }


        /// <summary>Validates a document; the name is used as the document reference in the report.</summary>
        public KositReport Validate(byte[] document, string name)
        {
            if (_Handle == 0)
            {
                throw new ObjectDisposedException(nameof(KositValidator));
            }
            (int recommendation, string xml) = KositLibrary.Validate(_Handle, document, name);
            return new KositReport((AcceptRecommendation)recommendation, xml);
        }


        public void Dispose()
        {
            if (_Handle != 0)
            {
                KositLibrary.Destroy(_Handle);
                _Handle = 0;
            }
        }
    }


    /// <summary>
    /// XSLT and XPath with the Saxon contained in the native library.
    /// </summary>
    public static class KositXml
    {
        /// <summary>Applies an XSLT stylesheet (compiled once per path) and returns the serialized result.</summary>
        public static string Transform(string stylesheetFile, byte[] document)
        {
            return KositLibrary.Transform(Path.GetFullPath(stylesheetFile), document);
        }


        /// <summary>Evaluates an XPath expression on an XML document and returns its effective boolean value.</summary>
        public static bool EvaluateBoolean(byte[] xml, string xpath, IReadOnlyDictionary<string, string> namespaces)
        {
            return KositLibrary.EvaluateBoolean(xml, xpath, string.Join("\n", namespaces.Select(n => n.Key + "=" + n.Value)));
        }
    }


    /// <summary>
    /// The functions of the native library. Every operating system thread that calls into the library is attached
    /// to the GraalVM isolate once.
    /// </summary>
    internal static unsafe partial class KositLibrary
    {
        private const string Library = "kosit";
        private static readonly IntPtr _Isolate = _CreateIsolate();
        [ThreadStatic]
        private static IntPtr _Thread;


        public static long Create(string scenarios, string repository)
        {
            long handle = _Create(_CurrentThread(), scenarios, repository, out IntPtr error);
            if (handle == 0)
            {
                throw new InvalidOperationException("Loading the KoSIT configuration failed: " + _Take(error));
            }
            return handle;
        }


        public static (int Recommendation, string Report) Validate(long handle, byte[] document, string name)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(name + "\0");
            fixed (byte* data = document)
            fixed (byte* namePointer = nameBytes)
            {
                int result = _Validate(_CurrentThread(), handle, data, document.Length, namePointer, out IntPtr output);
                string text = _Take(output);
                if (result < 0)
                {
                    throw new InvalidOperationException("Validation failed: " + text);
                }
                return (result, text);
            }
        }


        public static void Destroy(long handle)
        {
            _Destroy(_CurrentThread(), handle);
        }


        public static string Transform(string stylesheet, byte[] document)
        {
            fixed (byte* data = document)
            {
                int result = _Transform(_CurrentThread(), stylesheet, data, document.Length, out IntPtr output);
                string text = _Take(output);
                if (result < 0)
                {
                    throw new InvalidOperationException("Transformation failed: " + text);
                }
                return text;
            }
        }


        public static bool EvaluateBoolean(byte[] xml, string xpath, string namespaces)
        {
            fixed (byte* data = xml)
            {
                int result = _XPath(_CurrentThread(), data, xml.Length, xpath, namespaces, out IntPtr error);
                if (result < 0)
                {
                    throw new InvalidOperationException("XPath evaluation failed: " + _Take(error));
                }
                return result == 1;
            }
        }


        private static string _Take(IntPtr pointer)
        {
            string text = Marshal.PtrToStringUTF8(pointer) ?? "";
            _Free(_CurrentThread(), pointer);
            return text;
        }


        private static IntPtr _CurrentThread()
        {
            if (_Thread == IntPtr.Zero && _AttachThread(_Isolate, out _Thread) != 0)
            {
                throw new InvalidOperationException("graal_attach_thread failed");
            }
            return _Thread;
        }


        private static IntPtr _CreateIsolate()
        {
            if (_CreateIsolate(IntPtr.Zero, out IntPtr isolate, out IntPtr thread) != 0)
            {
                throw new InvalidOperationException("graal_create_isolate failed");
            }
            _Thread = thread;
            return isolate;
        }


        [LibraryImport(Library, EntryPoint = "graal_create_isolate")]
        private static partial int _CreateIsolate(IntPtr parameters, out IntPtr isolate, out IntPtr thread);

        [LibraryImport(Library, EntryPoint = "graal_attach_thread")]
        private static partial int _AttachThread(IntPtr isolate, out IntPtr thread);

        [LibraryImport(Library, EntryPoint = "kosit_create", StringMarshalling = StringMarshalling.Utf8)]
        private static partial long _Create(IntPtr thread, string scenarios, string repository, out IntPtr error);

        [LibraryImport(Library, EntryPoint = "kosit_validate")]
        private static partial int _Validate(IntPtr thread, long handle, byte* data, int length, byte* name, out IntPtr output);

        [LibraryImport(Library, EntryPoint = "kosit_destroy")]
        private static partial void _Destroy(IntPtr thread, long handle);

        [LibraryImport(Library, EntryPoint = "kosit_transform", StringMarshalling = StringMarshalling.Utf8)]
        private static partial int _Transform(IntPtr thread, string stylesheet, byte* data, int length, out IntPtr output);

        [LibraryImport(Library, EntryPoint = "kosit_xpath", StringMarshalling = StringMarshalling.Utf8)]
        private static partial int _XPath(IntPtr thread, byte* data, int length, string expression, string namespaces, out IntPtr error);

        [LibraryImport(Library, EntryPoint = "kosit_free")]
        private static partial void _Free(IntPtr thread, IntPtr pointer);
    }
}
