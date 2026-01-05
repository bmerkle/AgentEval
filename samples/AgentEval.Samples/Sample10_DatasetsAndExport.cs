// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

using System.Text.Json;

namespace AgentEval.Samples;

/// <summary>
/// Sample 10: Datasets and Export - Batch testing with data files
/// 
/// This demonstrates:
/// - Loading test datasets from JSON, JSONL, CSV, YAML
/// - Field aliasing for different data formats
/// - Running batch evaluations
/// - Exporting results to JUnit XML for CI/CD
/// 
/// ⏱️ Time to understand: 5 minutes
/// </summary>
public static class Sample10_DatasetsAndExport
{
    public static async Task RunAsync()
    {
        PrintHeader();

        // ═══════════════════════════════════════════════════════════════
        // STEP 1: Supported dataset formats
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 1: Supported dataset formats...\n");
        
        Console.WriteLine("   AgentEval supports multiple data formats:\n");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   📄 JSON (TestCases.json):");
        Console.WriteLine(@"   [
     { ""input"": ""What is 2+2?"", ""expectedOutput"": ""4"" },
     { ""input"": ""Capital of France?"", ""expectedOutput"": ""Paris"" }
   ]");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   📄 JSONL (TestCases.jsonl) - one per line:");
        Console.WriteLine(@"   {""input"": ""What is 2+2?"", ""expectedOutput"": ""4""}
   {""input"": ""Capital of France?"", ""expectedOutput"": ""Paris""}");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   📄 CSV (TestCases.csv):");
        Console.WriteLine(@"   input,expectedOutput,tags
   What is 2+2?,4,math
   Capital of France?,Paris,geography");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   📄 YAML (TestCases.yaml):");
        Console.WriteLine(@"   - input: What is 2+2?
     expectedOutput: ""4""
   - input: Capital of France?
     expectedOutput: Paris");
        Console.ResetColor();

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Loading datasets with aliasing
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 2: Loading datasets with field aliasing...\n");
        
        Console.WriteLine("   When your data uses different field names, use aliases:\n");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"   // Your data has ""question"" and ""answer"" but AgentEval expects ""input"" and ""expectedOutput""
   var loader = new JsonDatasetLoader(new DatasetOptions
   {
       FieldAliases = new Dictionary<string, string>
       {
           [""question""] = ""input"",
           [""answer""] = ""expectedOutput"",
           [""category""] = ""tags""
       }
   });
   
   var testCases = await loader.LoadAsync(""my-dataset.json"");");
        Console.ResetColor();
        Console.WriteLine();

        // Simulate loading
        var testCases = new List<TestCaseData>
        {
            new() { Input = "What is 2+2?", ExpectedOutput = "4", Tags = ["math"] },
            new() { Input = "Capital of France?", ExpectedOutput = "Paris", Tags = ["geography"] },
            new() { Input = "Who wrote Hamlet?", ExpectedOutput = "Shakespeare", Tags = ["literature"] }
        };
        
        Console.WriteLine($"   Loaded {testCases.Count} test cases:");
        foreach (var tc in testCases)
        {
            Console.WriteLine($"      • \"{tc.Input}\" → \"{tc.ExpectedOutput}\" [{string.Join(", ", tc.Tags)}]");
        }

        // ═══════════════════════════════════════════════════════════════
        // STEP 3: Running batch evaluation
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 3: Running batch evaluation...\n");
        
        var results = new List<TestResultData>();
        
        foreach (var testCase in testCases)
        {
            await Task.Delay(50); // Simulate agent call
            
            // Simulate agent response
            var passed = testCase.ExpectedOutput.Length > 0;
            results.Add(new TestResultData
            {
                TestName = testCase.Input,
                Passed = passed,
                Score = passed ? 100 : 0,
                ActualOutput = testCase.ExpectedOutput,
                Duration = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 200))
            });
            
            Console.Write($"   Running: \"{testCase.Input[..Math.Min(30, testCase.Input.Length)]}...\" ");
            Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(passed ? "✅" : "❌");
            Console.ResetColor();
        }

        // ═══════════════════════════════════════════════════════════════
        // STEP 4: JUnit XML export for CI/CD
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 4: JUnit XML export for CI/CD...\n");
        
        var junitXml = GenerateJUnitXml(results);
        
        Console.WriteLine("   Generated JUnit XML for CI integration:\n");
        Console.ForegroundColor = ConsoleColor.Cyan;
        foreach (var line in junitXml.Split('\n').Take(15))
        {
            Console.WriteLine($"   {line}");
        }
        Console.ResetColor();
        Console.WriteLine("   ...\n");
        
        Console.WriteLine("   💡 CI/CD Integration Examples:");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(@"
   # GitHub Actions
   - name: Run AgentEval tests
     run: dotnet run -- eval --export junit --output results.xml
   
   - name: Publish Test Results
     uses: dorny/test-reporter@v1
     with:
       name: AgentEval Results
       path: results.xml
       reporter: java-junit

   # Azure DevOps
   - task: PublishTestResults@2
     inputs:
       testResultsFormat: 'JUnit'
       testResultsFiles: '**/results.xml'");
        Console.ResetColor();

        // ═══════════════════════════════════════════════════════════════
        // STEP 5: Other export formats
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 5: Other export formats...\n");
        
        Console.WriteLine("   AgentEval supports multiple export formats:\n");
        
        Console.WriteLine("   📄 JSON     - Machine-readable results");
        Console.WriteLine("   📄 JUnit    - CI/CD integration (GitHub Actions, Azure DevOps)");
        Console.WriteLine("   📄 TRX      - Visual Studio Test Results");
        Console.WriteLine("   📄 Markdown - Human-readable reports\n");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"   // Code usage
   var exporter = new JUnitExporter();
   await exporter.ExportAsync(summary, ""results.xml"");
   
   var mdExporter = new MarkdownExporter();
   await mdExporter.ExportAsync(summary, ""report.md"");");
        Console.ResetColor();

        // ═══════════════════════════════════════════════════════════════
        // STEP 6: CLI usage
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 6: CLI usage for batch evaluation...\n");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"   # Initialize a test project
   agenteval init
   
   # Run evaluation on a dataset
   agenteval eval --dataset tests.jsonl --model gpt-4o-mini
   
   # Export to JUnit for CI
   agenteval eval --dataset tests.jsonl --export junit --output results.xml
   
   # Run with multiple formats
   agenteval eval --dataset tests.jsonl --export json,junit,markdown
   
   # List available metrics
   agenteval list metrics
   
   # Run specific metrics
   agenteval eval --dataset tests.jsonl --metrics relevance,faithfulness");
        Console.ResetColor();

        await Task.Delay(1); // Keep async signature
        
        // ═══════════════════════════════════════════════════════════════
        // KEY TAKEAWAYS
        // ═══════════════════════════════════════════════════════════════
        PrintKeyTakeaways();
    }

    private static string GenerateJUnitXml(List<TestResultData> results)
    {
        var passed = results.Count(r => r.Passed);
        var failed = results.Count - passed;
        var totalTime = results.Sum(r => r.Duration.TotalSeconds);
        
        var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<testsuites tests=""{results.Count}"" failures=""{failed}"" time=""{totalTime:F3}"">
  <testsuite name=""AgentEval"" tests=""{results.Count}"" failures=""{failed}"" time=""{totalTime:F3}"">";

        foreach (var result in results)
        {
            xml += $@"
    <testcase name=""{EscapeXml(result.TestName)}"" time=""{result.Duration.TotalSeconds:F3}"" classname=""AgentEval.Tests"">";
            
            if (!result.Passed)
            {
                xml += $@"
      <failure message=""Test failed"">Score: {result.Score}/100</failure>";
            }
            
            xml += @"
    </testcase>";
        }

        xml += @"
  </testsuite>
</testsuites>";
        
        return xml;
    }

    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║                   Sample 10: Datasets and Export                              ║
║                                                                               ║
║   Learn how to:                                                               ║
║   • Load test datasets from JSON, JSONL, CSV, YAML                            ║
║   • Use field aliasing for different data formats                             ║
║   • Run batch evaluations on datasets                                         ║
║   • Export to JUnit XML for CI/CD integration                                 ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    private static void PrintKeyTakeaways()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              🎯 KEY TAKEAWAYS                                   │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  1. Multiple dataset formats supported:                                         │
│     - JSON, JSONL (line-delimited), CSV, YAML                                   │
│     - Use IDatasetLoader interface for custom formats                           │
│                                                                                 │
│  2. Field aliasing for flexibility:                                             │
│     new DatasetOptions { FieldAliases = { [""question""] = ""input"" } }           │
│                                                                                 │
│  3. Export formats for CI/CD:                                                   │
│     - JUnit XML: GitHub Actions, Azure DevOps, Jenkins                          │
│     - TRX: Visual Studio Test Results                                           │
│     - JSON: Custom dashboards                                                   │
│     - Markdown: Human-readable reports                                          │
│                                                                                 │
│  4. CLI for automation:                                                         │
│     agenteval eval --dataset tests.jsonl --export junit                         │
│                                                                                 │
│  5. Integrate with CI pipelines:                                                │
│     GitHub Actions: dorny/test-reporter@v1                                      │
│     Azure DevOps: PublishTestResults@2                                          │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
");
        Console.ResetColor();
    }

    // Local types for this sample
    private record TestCaseData
    {
        public required string Input { get; init; }
        public required string ExpectedOutput { get; init; }
        public List<string> Tags { get; init; } = [];
    }

    private record TestResultData
    {
        public required string TestName { get; init; }
        public bool Passed { get; init; }
        public int Score { get; init; }
        public string? ActualOutput { get; init; }
        public TimeSpan Duration { get; init; }
    }
}
