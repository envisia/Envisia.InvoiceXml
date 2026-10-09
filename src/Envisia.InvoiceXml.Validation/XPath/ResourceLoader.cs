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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Envisia.InvoiceXml.Validation.XPath
{
    /// <summary>
    /// Opens the resources referenced by validation artefacts: files (file: URIs or paths) and the artefacts
    /// embedded into this assembly (embedded:/path URIs). Network resources are never loaded.
    /// </summary>
    internal static class ResourceLoader
    {
        public const string EmbeddedScheme = "embedded:";
        private static readonly Assembly _Assembly = typeof(ResourceLoader).Assembly;


        public static string EmbeddedUri(string path)
        {
            return EmbeddedScheme + "/" + path.TrimStart('/');
        }


        public static bool Exists(string uri)
        {
            if (uri.StartsWith(EmbeddedScheme, StringComparison.Ordinal))
            {
                return _Assembly.GetManifestResourceInfo(uri.Substring(EmbeddedScheme.Length).TrimStart('/')) != null;
            }
            string path = ToLocalPath(uri);
            return path != null && File.Exists(path);
        }


        public static Stream Open(string uri)
        {
            if (uri.StartsWith(EmbeddedScheme, StringComparison.Ordinal))
            {
                string name = uri.Substring(EmbeddedScheme.Length).TrimStart('/');
                Stream stream = _Assembly.GetManifestResourceStream(name);
                if (stream == null)
                {
                    throw new FileNotFoundException("Embedded validation resource not found: " + name);
                }
                return stream;
            }
            string path = ToLocalPath(uri);
            if (path == null)
            {
                throw new NotSupportedException("Only local files can be loaded, cannot open " + uri);
            }
            return File.OpenRead(path);
        }


        public static string ToLocalPath(string uri)
        {
            Uri parsed;
            if (Uri.TryCreate(uri, UriKind.Absolute, out parsed))
            {
                return parsed.IsFile ? parsed.LocalPath : null;
            }
            return uri;
        }


        /// <summary>
        /// Returns the URI of a local file path.
        /// </summary>
        public static string FileUri(string path)
        {
            return new Uri(Path.GetFullPath(path)).AbsoluteUri;
        }


        /// <summary>
        /// Resolves a relative reference against an embedded: base URI.
        /// </summary>
        public static string ResolveEmbedded(string baseUri, string relative)
        {
            string basePath = baseUri.Substring(EmbeddedScheme.Length);
            string directory = basePath.Substring(0, basePath.LastIndexOf('/') + 1);
            string combined = relative.StartsWith("/", StringComparison.Ordinal) ? relative : directory + relative;
            List<string> segments = new List<string>();
            foreach (string segment in combined.Split('/'))
            {
                if (segment.Length == 0 || segment == ".")
                {
                    continue;
                }
                if (segment == "..")
                {
                    if (segments.Count > 0)
                    {
                        segments.RemoveAt(segments.Count - 1);
                    }
                    continue;
                }
                segments.Add(Uri.UnescapeDataString(segment));
            }
            return EmbeddedScheme + "/" + String.Join("/", segments);
        }
    }


    /// <summary>
    /// Loads and caches the documents requested with doc() / document() (code lists etc.).
    /// </summary>
    internal sealed class DocumentCache : IDocumentResolver
    {
        private readonly ConcurrentDictionary<string, XdmDocument> _Documents = new ConcurrentDictionary<string, XdmDocument>();


        public XdmDocument Resolve(string absoluteUri)
        {
            return _Documents.GetOrAdd(absoluteUri, uri =>
            {
                using (Stream stream = ResourceLoader.Open(uri))
                {
                    return XdmDocumentBuilder.Load(stream, uri);
                }
            });
        }
    }
}
