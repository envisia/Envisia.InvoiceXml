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
namespace Envisia.InvoiceXml.Tests.KositConformance
{
    /// <summary>
    /// The test data prepared by tests/KositConformance/prepare-testdata.sh and the committed JVM reference.
    /// The build directory can be overridden with the environment variable KOSIT_TESTDATA.
    /// </summary>
    public static class KositTestData
    {
        public const string KositVersion = "1.6.3";


        public static string SuiteDirectory { get; } = Path.Combine(_FindRepositoryRoot(), "tests", "KositConformance");

        public static string BuildDirectory { get; } = Environment.GetEnvironmentVariable("KOSIT_TESTDATA") ?? Path.Combine(SuiteDirectory, "build");

        public static bool IsPrepared => File.Exists(Path.Combine(BuildDirectory, "manifest.txt"));

        public static string XRechnungConfiguration => Path.Combine(BuildDirectory, "config", "xrechnung");

        public static string FacturXConfiguration => Path.Combine(BuildDirectory, "config", "facturx");

        public static string KositTests => Path.Combine(BuildDirectory, "kosit-tests");

        public static string EN16931 => Path.Combine(BuildDirectory, "sources", "eInvoicing-EN16931");


        /// <summary>The documents of the report tests: set (configuration), id and path.</summary>
        public static IReadOnlyList<(string Set, string Id, string Path)> Manifest()
        {
            return File.ReadAllLines(Path.Combine(BuildDirectory, "manifest.txt"))
                       .Where(l => l.Length > 0)
                       .Select(l => l.Split('\t'))
                       .Select(p => (p[0], p[1], p[2]))
                       .ToList();
        }


        /// <summary>The report summaries of the KoSIT validator on the JVM, by set and id.</summary>
        public static IReadOnlyDictionary<(string Set, string Id), string> Reference { get; } = _ReadReference();


        /// <summary>The expectations of the CEN unit tests that Saxon on the JVM does not meet (stale upstream tests).</summary>
        public static IReadOnlyList<(string File, string Failure)> CenUnitTestReference { get; } =
            File.ReadAllLines(Path.Combine(SuiteDirectory, "reference", "cen-unit-tests-jvm.txt"))
                .Where(l => l.Length > 0)
                .Select(l => l.Split('\t', 2))
                .Select(p => (p[0], p[1]))
                .ToList();


        public static void RequirePrepared()
        {
            if (!IsPrepared)
            {
                Assert.Inconclusive("The test data is missing, run tests/KositConformance/prepare-testdata.sh (see its README).");
            }
        }


        private static Dictionary<(string, string), string> _ReadReference()
        {
            Dictionary<(string, string), string> reference = new Dictionary<(string, string), string>();
            string path = Path.Combine(SuiteDirectory, "reference", "kosit-" + KositVersion + "-jvm.txt");
            string? set = null, id = null;
            System.Text.StringBuilder summary = new System.Text.StringBuilder();
            foreach (string line in File.ReadAllLines(path))
            {
                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    if (set != null)
                    {
                        reference[(set, id!)] = summary.ToString();
                    }
                    int space = line.IndexOf(' ', 3);
                    set = line.Substring(3, space - 3);
                    id = line.Substring(space + 1);
                    summary.Clear();
                }
                else
                {
                    summary.Append(line).Append('\n');
                }
            }
            if (set != null)
            {
                reference[(set, id!)] = summary.ToString();
            }
            return reference;
        }


        // from the test output directory: the source paths of CI builds are mapped (ContinuousIntegrationBuild)
        private static string _FindRepositoryRoot()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Envisia.InvoiceXml.sln")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find the repository root starting at " + AppContext.BaseDirectory);
        }
    }
}
