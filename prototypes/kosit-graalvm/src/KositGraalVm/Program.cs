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
using System.Diagnostics;
using Envisia.KositNative;

// Validates invoices with the KoSIT validator compiled with GraalVM native-image and writes the KoSIT reports.
//
// usage: KositGraalVm --repository <dir> [--scenarios <file>] [--output <dir>] [--passes <n>] [--threads <n>] <invoice.xml>...
string? repository = null;
string scenarios = "scenarios.xml";
string? output = null;
int passes = 1, threads = 1;
List<string> files = new List<string>();
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--repository": repository = Path.GetFullPath(args[++i]); break;
        case "--scenarios": scenarios = args[++i]; break;
        case "--output": output = Path.GetFullPath(args[++i]); break;
        case "--passes": passes = int.Parse(args[++i]); break;
        case "--threads": threads = int.Parse(args[++i]); break;
        default: files.Add(Path.GetFullPath(args[i])); break;
    }
}
if (repository == null || files.Count == 0)
{
    Console.Error.WriteLine("usage: KositGraalVm --repository <dir> [--scenarios <file>] [--output <dir>] [--passes <n>] [--threads <n>] <invoice.xml>...");
    return 1;
}

Stopwatch watch = Stopwatch.StartNew();
using KositValidator validator = new KositValidator(Path.Combine(repository, scenarios), repository);
Console.WriteLine($"configuration loaded in {watch.ElapsedMilliseconds} ms");
if (output != null)
{
    Directory.CreateDirectory(output);
}
for (int pass = 1; pass <= passes; pass++)
{
    watch.Restart();
    int current = pass;
    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = threads }, file =>
    {
        KositReport report = validator.Validate(File.ReadAllBytes(file), Path.GetFileName(file));
        if (current == 1)
        {
            Console.WriteLine($"{Path.GetFileName(file)}: {report.Recommendation}");
            if (output != null)
            {
                File.WriteAllText(Path.Combine(output, Path.GetFileNameWithoutExtension(file) + "-report.xml"), report.Xml);
            }
        }
    });
    Console.WriteLine($"pass {pass}: {files.Count} documents in {watch.ElapsedMilliseconds} ms ({watch.Elapsed.TotalMilliseconds / files.Count:F1} ms per document, {threads} thread(s))");
}
Console.WriteLine($"peak working set: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024} MB");
return 0;
