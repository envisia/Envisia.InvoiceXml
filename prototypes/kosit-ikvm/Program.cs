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
using de.kosit.validationtool.api;
using de.kosit.validationtool.impl;
using de.kosit.validationtool.impl.xml;

// Validates invoices with the KoSIT validator running on .NET through IKVM, using the KoSIT Java API
// directly (Configuration, DefaultCheck, InputFactory), and writes the KoSIT XML reports.
//
// usage: KositIkvm --repository <dir> [--scenarios <file>] [--output <dir>] [--passes <n>] <invoice.xml>...
string repository = null;
string scenarios = "scenarios.xml";
string output = null;
int passes = 1;
List<string> files = new List<string>();
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--repository": repository = Path.GetFullPath(args[++i]); break;
        case "--scenarios": scenarios = args[++i]; break;
        case "--output": output = Path.GetFullPath(args[++i]); break;
        case "--passes": passes = int.Parse(args[++i]); break;
        default: files.Add(Path.GetFullPath(args[i])); break;
    }
}
if (repository == null || files.Count == 0)
{
    Console.Error.WriteLine("usage: KositIkvm --repository <dir> [--scenarios <file>] [--output <dir>] [--passes <n>] <invoice.xml>...");
    return 1;
}

Stopwatch watch = Stopwatch.StartNew();
net.sf.saxon.s9api.Processor processor = ProcessorProvider.getProcessor();
Configuration configuration = Configuration.load(new java.io.File(Path.Combine(repository, scenarios)).toURI(), new java.io.File(repository).toURI()).build(processor);
Check check = new DefaultCheck(processor, configuration);
Console.WriteLine($"configuration loaded in {watch.ElapsedMilliseconds} ms");

if (output != null)
{
    Directory.CreateDirectory(output);
}
for (int pass = 1; pass <= passes; pass++)
{
    watch.Restart();
    foreach (string file in files)
    {
        Result result = check.checkInput(InputFactory.read(new java.io.File(file)));
        Console.WriteLine($"{Path.GetFileName(file)}: {result.getAcceptRecommendation()}");
        if (output != null && pass == 1)
        {
            javax.xml.transform.Transformer transformer = javax.xml.transform.TransformerFactory.newInstance().newTransformer();
            java.io.StringWriter writer = new java.io.StringWriter();
            transformer.transform(new javax.xml.transform.dom.DOMSource(result.getReportDocument()), new javax.xml.transform.stream.StreamResult(writer));
            File.WriteAllText(Path.Combine(output, Path.GetFileNameWithoutExtension(file) + "-report.xml"), writer.toString());
        }
    }
    Console.WriteLine($"pass {pass}: {files.Count} documents in {watch.ElapsedMilliseconds} ms ({watch.Elapsed.TotalMilliseconds / files.Count:F1} ms per document)");
}
Console.WriteLine($"peak working set: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024} MB");
return 0;
