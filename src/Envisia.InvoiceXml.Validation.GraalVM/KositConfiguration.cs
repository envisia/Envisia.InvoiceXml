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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Envisia.InvoiceXml.Validation.GraalVM
{
    /// <summary>
    /// A KoSIT validator configuration: a scenarios file (http://www.xoev.de/de/validator/framework/1/scenarios) and
    /// the repository directory its resource locations are relative to.
    /// </summary>
    public sealed class KositConfiguration
    {
        private static readonly Lazy<string> _BuiltIn = new Lazy<string>(_ExtractBuiltIn, LazyThreadSafetyMode.ExecutionAndPublication);


        private KositConfiguration(string name, string scenariosFile, string repositoryDirectory)
        {
            Name = name;
            ScenariosFile = scenariosFile;
            RepositoryDirectory = repositoryDirectory;
        }


        /// <summary>A name for messages.</summary>
        public string Name { get; }

        /// <summary>The scenarios file.</summary>
        public string ScenariosFile { get; }

        /// <summary>The directory the resource locations of the scenarios are relative to.</summary>
        public string RepositoryDirectory { get; }


        /// <summary>
        /// The KoSIT validator configuration for XRechnung 3.0.2 (release 2026-08-31): XRechnung UBL Invoice, UBL
        /// CreditNote and CII including Extension and CVD, and EN 16931 in UBL and CII.
        /// </summary>
        public static KositConfiguration XRechnung { get; } = new KositConfiguration("XRechnung", null, null);

        /// <summary>
        /// The configuration for ZUGFeRD 2.5.2 / Factur-X 1.09.2 (MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED) with the
        /// official XML schemas and Schematron rules.
        /// </summary>
        public static KositConfiguration FacturX { get; } = new KositConfiguration("FacturX", null, null);

        /// <summary>
        /// The built-in configurations in the order they are tried: Factur-X, XRechnung (as the default of
        /// Envisia.InvoiceXml.Validation).
        /// </summary>
        public static IReadOnlyList<KositConfiguration> Default { get; } = new[] { FacturX, XRechnung };


        /// <summary>
        /// A configuration from files, e.g. an unpacked release of a KoSIT validator configuration.
        /// </summary>
        /// <param name="scenariosFile">The scenarios file</param>
        /// <param name="repositoryDirectory">The repository directory, by default the directory of the scenarios file</param>
        public static KositConfiguration FromFiles(string scenariosFile, string repositoryDirectory = null)
        {
            if (scenariosFile == null)
            {
                throw new ArgumentNullException(nameof(scenariosFile));
            }
            string scenarios = Path.GetFullPath(scenariosFile);
            return new KositConfiguration(Path.GetFileName(scenarios), scenarios, Path.GetFullPath(repositoryDirectory ?? Path.GetDirectoryName(scenarios)));
        }


        /// <summary>The scenarios file and repository, unpacking the built-in configurations on first use.</summary>
        internal (string Scenarios, string Repository) Resolve()
        {
            if (ScenariosFile != null)
            {
                return (ScenariosFile, RepositoryDirectory);
            }
            string repository = Path.Combine(_BuiltIn.Value, ReferenceEquals(this, XRechnung) ? "xrechnung" : "facturx");
            return (Path.Combine(repository, "scenarios.xml"), repository);
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// Writes the embedded configurations to a directory below the temporary directory whose name depends on their
        /// content; an existing complete copy is reused (also by other processes).
        /// </summary>
        private static string _ExtractBuiltIn()
        {
            Assembly assembly = typeof(KositConfiguration).Assembly;
            const string prefix = "configurations/";
            List<string> names = assembly.GetManifestResourceNames().Where(n => n.StartsWith(prefix, StringComparison.Ordinal)).OrderBy(n => n, StringComparer.Ordinal).ToList();
            string hash;
            using (IncrementalHash sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                foreach (string name in names)
                {
                    sha.AppendData(Encoding.UTF8.GetBytes(name));
                    using (Stream stream = assembly.GetManifestResourceStream(name))
                    {
                        sha.AppendData(BitConverter.GetBytes(stream.Length));
                    }
                }
                hash = Convert.ToHexString(sha.GetHashAndReset()).Substring(0, 16).ToLowerInvariant();
            }
            string root = Path.Combine(Path.GetTempPath(), "Envisia.InvoiceXml.Validation.GraalVM", hash);
            if (File.Exists(Path.Combine(root, ".complete")))
            {
                return root;
            }
            string temporary = root + "." + Guid.NewGuid().ToString("N");
            foreach (string name in names)
            {
                string path = Path.Combine(temporary, name.Substring(prefix.Length).Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (Stream source = assembly.GetManifestResourceStream(name))
                using (FileStream target = File.Create(path))
                {
                    source.CopyTo(target);
                }
            }
            // the Factur-X configuration uses the default report of the XRechnung configuration
            File.Copy(Path.Combine(temporary, "xrechnung", "resources", "default-report.xsl"), Path.Combine(temporary, "facturx", "resources", "default-report.xsl"));
            File.WriteAllText(Path.Combine(temporary, ".complete"), "");
            if (Directory.Exists(root))
            {
                // left incomplete by a process that was terminated while unpacking
                Directory.Delete(root, true);
            }
            try
            {
                Directory.Move(temporary, root);
            }
            catch (IOException) when (File.Exists(Path.Combine(root, ".complete")))
            {
                // another process unpacked them at the same time
                Directory.Delete(temporary, true);
            }
            return root;
        }
    }
}
