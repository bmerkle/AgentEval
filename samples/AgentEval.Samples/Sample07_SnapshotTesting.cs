// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

using System.Security.Cryptography;
using System.Text;

namespace AgentEval.Samples;

/// <summary>
/// Sample 07: Snapshot Testing - Detecting regressions in agent behavior
/// 
/// This demonstrates:
/// - Creating baseline snapshots of agent responses
/// - Comparing new responses against baselines
/// - Detecting regressions (unexpected changes)
/// - Using scrubbing to ignore dynamic values
/// 
/// ⏱️ Time to understand: 5 minutes
/// </summary>
public static class Sample07_SnapshotTesting
{
    public static async Task RunAsync()
    {
        PrintHeader();

        // ═══════════════════════════════════════════════════════════════
        // STEP 1: Create a baseline snapshot
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 1: Creating baseline snapshot...\n");
        
        var baseline = new ResponseSnapshot
        {
            Query = "What is the capital of France?",
            Response = "The capital of France is Paris. Paris is also the largest city in France.",
            ToolsCalled = new[] { "lookup_capital" },
            ResponseHash = ComputeHash("The capital of France is Paris. Paris is also the largest city in France.")
        };
        
        Console.WriteLine($"   Query: \"{baseline.Query}\"");
        Console.WriteLine($"   Response: \"{baseline.Response}\"");
        Console.WriteLine($"   Tools: [{string.Join(", ", baseline.ToolsCalled)}]");
        Console.WriteLine($"   Hash: {baseline.ResponseHash[..16]}...");

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Compare identical response (should pass)
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 2: Comparing identical response...\n");
        
        var identicalResponse = new ResponseSnapshot
        {
            Query = "What is the capital of France?",
            Response = "The capital of France is Paris. Paris is also the largest city in France.",
            ToolsCalled = new[] { "lookup_capital" },
            ResponseHash = ComputeHash("The capital of France is Paris. Paris is also the largest city in France.")
        };
        
        var identicalResult = CompareSnapshots(baseline, identicalResponse);
        PrintComparisonResult("Identical Response", identicalResult);

        // ═══════════════════════════════════════════════════════════════
        // STEP 3: Compare changed response (should detect regression)
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 3: Comparing changed response (regression)...\n");
        
        var changedResponse = new ResponseSnapshot
        {
            Query = "What is the capital of France?",
            Response = "Paris is the capital of France.",  // Changed!
            ToolsCalled = new[] { "lookup_capital" },
            ResponseHash = ComputeHash("Paris is the capital of France.")
        };
        
        var changedResult = CompareSnapshots(baseline, changedResponse);
        PrintComparisonResult("Changed Response", changedResult);

        // ═══════════════════════════════════════════════════════════════
        // STEP 4: Compare with different tools (should detect)
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 4: Comparing different tool usage...\n");
        
        var differentTools = new ResponseSnapshot
        {
            Query = "What is the capital of France?",
            Response = "The capital of France is Paris. Paris is also the largest city in France.",
            ToolsCalled = new[] { "search_web", "lookup_capital" },  // Extra tool!
            ResponseHash = ComputeHash("The capital of France is Paris. Paris is also the largest city in France.")
        };
        
        var toolsResult = CompareSnapshots(baseline, differentTools);
        PrintComparisonResult("Different Tools", toolsResult);

        // ═══════════════════════════════════════════════════════════════
        // STEP 5: Using scrubbing for dynamic values
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 5: Scrubbing dynamic values...\n");
        
        var responseWithTimestamp = "The capital of France is Paris. Retrieved at 2025-01-15T10:30:00Z.";
        var scrubbed = ScrubDynamicValues(responseWithTimestamp);
        
        Console.WriteLine($"   Original:  \"{responseWithTimestamp}\"");
        Console.WriteLine($"   Scrubbed:  \"{scrubbed}\"");
        Console.WriteLine();
        
        // Now compare with different timestamps - should match after scrubbing
        var baseline2 = "The capital of France is Paris. Retrieved at 2025-01-10T08:00:00Z.";
        var current2 = "The capital of France is Paris. Retrieved at 2025-01-20T15:45:00Z.";
        
        var matchesWithoutScrub = baseline2 == current2;
        var matchesWithScrub = ScrubDynamicValues(baseline2) == ScrubDynamicValues(current2);
        
        Console.WriteLine($"   Without scrubbing: {(matchesWithoutScrub ? "✅ Match" : "❌ Different")}");
        Console.WriteLine($"   With scrubbing:    {(matchesWithScrub ? "✅ Match" : "❌ Different")}");

        // ═══════════════════════════════════════════════════════════════
        // KEY TAKEAWAYS
        // ═══════════════════════════════════════════════════════════════
        await Task.Delay(1); // Keep async signature
        PrintKeyTakeaways();
    }

    private static ComparisonResult CompareSnapshots(ResponseSnapshot baseline, ResponseSnapshot current)
    {
        var differences = new List<string>();
        
        // Compare response hash
        if (baseline.ResponseHash != current.ResponseHash)
        {
            differences.Add($"Response changed: \"{baseline.Response}\" → \"{current.Response}\"");
        }
        
        // Compare tools
        var baselineTools = new HashSet<string>(baseline.ToolsCalled);
        var currentTools = new HashSet<string>(current.ToolsCalled);
        
        if (!baselineTools.SetEquals(currentTools))
        {
            var added = currentTools.Except(baselineTools).ToList();
            var removed = baselineTools.Except(currentTools).ToList();
            
            if (added.Count > 0)
                differences.Add($"New tools called: [{string.Join(", ", added)}]");
            if (removed.Count > 0)
                differences.Add($"Tools no longer called: [{string.Join(", ", removed)}]");
        }
        
        return new ComparisonResult
        {
            Matches = differences.Count == 0,
            Differences = differences
        };
    }

    private static void PrintComparisonResult(string label, ComparisonResult result)
    {
        Console.Write($"   {label}: ");
        if (result.Matches)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ MATCHES baseline");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ REGRESSION detected!");
            Console.ResetColor();
            foreach (var diff in result.Differences)
            {
                Console.WriteLine($"      • {diff}");
            }
        }
        Console.ResetColor();
    }

    private static string ScrubDynamicValues(string text)
    {
        // Remove ISO timestamps
        var scrubbed = System.Text.RegularExpressions.Regex.Replace(
            text, 
            @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z?", 
            "[TIMESTAMP]");
        
        // Remove GUIDs
        scrubbed = System.Text.RegularExpressions.Regex.Replace(
            scrubbed,
            @"[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}",
            "[GUID]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return scrubbed;
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║                     Sample 07: Snapshot Testing                               ║
║                                                                               ║
║   Learn how to:                                                               ║
║   • Create baseline snapshots of agent responses                              ║
║   • Detect regressions when agent behavior changes                            ║
║   • Use scrubbing to ignore dynamic values (timestamps, IDs)                  ║
║   • Compare tool usage across runs                                            ║
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
│  1. SnapshotComparer captures baseline behavior:                                │
│     var baseline = await SnapshotStore.SaveAsync(""query"", result);             │
│                                                                                 │
│  2. Compare new results against baseline:                                       │
│     var comparison = await SnapshotComparer.CompareAsync(baseline, current);    │
│     Assert.True(comparison.Matches, comparison.DiffSummary);                    │
│                                                                                 │
│  3. Use scrubbing for dynamic content:                                          │
│     var options = new SnapshotOptions                                           │
│     {                                                                           │
│         Scrubbers = new[] { new TimestampScrubber(), new GuidScrubber() }       │
│     };                                                                          │
│                                                                                 │
│  4. Compare tool usage, not just output:                                        │
│     comparison.ToolDifferences.Should().BeEmpty();                              │
│                                                                                 │
│  5. Store snapshots alongside tests for versioning:                             │
│     __snapshots__/                                                              │
│       MyTest_query1.json                                                        │
│       MyTest_query2.json                                                        │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
");
        Console.ResetColor();
    }

    // Local types for this sample
    private record ResponseSnapshot
    {
        public required string Query { get; init; }
        public required string Response { get; init; }
        public required string[] ToolsCalled { get; init; }
        public required string ResponseHash { get; init; }
    }

    private record ComparisonResult
    {
        public bool Matches { get; init; }
        public List<string> Differences { get; init; } = new();
    }
}
