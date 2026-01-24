// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using AgentEval.Core;
using AgentEval.MAF;
using AgentEval.Models;

namespace AgentEval.Samples;

/// <summary>
/// Sample 19: Streaming vs Non-Streaming Performance Comparison
/// 
/// This demonstrates:
/// - Running the SAME test with streaming and non-streaming (async)
/// - Comparing performance metrics between both approaches
/// - Understanding token/cost tracking in each mode
/// - Using TestOptions.ModelName for accurate cost calculation
/// 
/// ⏱️ Time to understand: 8 minutes
/// </summary>
public static class Sample19_StreamingVsAsyncPerformance
{
    public static async Task RunAsync()
    {
        PrintHeader();

        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  This sample requires Azure OpenAI configuration.");
            Console.WriteLine("    Set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY\n");
            Console.ResetColor();
            RunMockDemo();
            return;
        }

        await RunRealDemo(endpoint, apiKey, deployment);
    }

    private static async Task RunRealDemo(string endpoint, string apiKey, string deployment)
    {
        Console.WriteLine("🚀 Running with REAL Azure OpenAI...\n");

        // Create agent
        var agent = CreateAgent(endpoint, apiKey, deployment);
        var adapter = new MAFAgentAdapter(agent);
        var harness = new MAFTestHarness(verbose: true);

        // TestOptions with ModelName (KEY for cost calculation!)
        var options = new TestOptions 
        { 
            TrackPerformance = true,
            ModelName = deployment  // ← KEY: Enables accurate cost calculation!
        };

        var testCase = new TestCase
        {
            Name = "Streaming vs Async Comparison",
            Input = "Explain in 2-3 sentences why software testing is important.",
        };

        Console.WriteLine($"   📋 TestOptions.ModelName = '{options.ModelName}'\n");

        // ═══════════════════════════════════════════════════════════════
        // PART 1: NON-STREAMING (Async)
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("   ⏳ PART 1: NON-STREAMING (Async) Mode");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        var asyncResult = await harness.RunTestAsync(adapter, testCase, options);

        PrintResults("ASYNC", asyncResult);

        // ═══════════════════════════════════════════════════════════════
        // PART 2: STREAMING
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("   🌊 PART 2: STREAMING Mode");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.Write("   Response: ");
        Console.ForegroundColor = ConsoleColor.DarkGray;

        var streamingResult = await harness.RunTestStreamingAsync(adapter, testCase,
            streamingOptions: new StreamingOptions
            {
                OnTextChunk = chunk => Console.Write(chunk),
                OnFirstToken = _ => Console.ForegroundColor = ConsoleColor.White
            },
            options: options);

        Console.ResetColor();
        Console.WriteLine("\n");

        PrintResults("STREAMING", streamingResult);

        // ═══════════════════════════════════════════════════════════════
        // PART 3: COMPARISON
        // ═══════════════════════════════════════════════════════════════
        PrintComparison(asyncResult, streamingResult);
    }

    private static void RunMockDemo()
    {
        Console.WriteLine("📊 MOCK DATA DEMONSTRATION\n");
        Console.WriteLine("┌──────────────────┬───────────────┬───────────────┐");
        Console.WriteLine("│ Metric           │ Non-Streaming │ Streaming     │");
        Console.WriteLine("├──────────────────┼───────────────┼───────────────┤");
        Console.WriteLine("│ Duration         │ ~1500ms       │ ~1800ms       │");
        Console.WriteLine("│ Time to First    │ N/A           │ ~350ms        │");
        Console.WriteLine("│ Input Tokens     │ ✓ Captured    │ ✓ Captured    │");
        Console.WriteLine("│ Output Tokens    │ ✓ Captured    │ ✓ Captured    │");
        Console.WriteLine("│ Estimated Cost   │ ✓ Calculated  │ ✓ Calculated  │");
        Console.WriteLine("└──────────────────┴───────────────┴───────────────┘");
        Console.WriteLine("\n💡 BOTH methods now capture tokens and costs!\n");
    }

    private static void PrintResults(string mode, TestResult result)
    {
        Console.WriteLine($"   📊 {mode} RESULTS:");
        Console.WriteLine(new string('─', 50));

        if (result.Performance != null)
        {
            var p = result.Performance;
            Console.WriteLine($"   Duration:      {p.TotalDuration.TotalMilliseconds:F0}ms");

            if (p.TimeToFirstToken.HasValue)
                Console.WriteLine($"   TTFT:          {p.TimeToFirstToken.Value.TotalMilliseconds:F0}ms");
            else
                Console.WriteLine($"   TTFT:          N/A");

            Console.WriteLine($"   Input Tokens:  {p.PromptTokens ?? 0}");
            Console.WriteLine($"   Output Tokens: {p.CompletionTokens ?? 0}");
            Console.WriteLine($"   Total Tokens:  {p.TotalTokens}");
            Console.WriteLine($"   Model:         {p.ModelUsed ?? "unknown"}");
            Console.WriteLine($"   Est. Cost:     ${p.EstimatedCost:F6}");
        }
        Console.WriteLine();
    }

    private static void PrintComparison(TestResult asyncResult, TestResult streamingResult)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("   📊 COMPARISON TABLE");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        var ap = asyncResult.Performance;
        var sp = streamingResult.Performance;

        Console.WriteLine("┌──────────────────┬───────────────┬───────────────┐");
        Console.WriteLine("│ Metric           │ Non-Streaming │ Streaming     │");
        Console.WriteLine("├──────────────────┼───────────────┼───────────────┤");
        Console.WriteLine($"│ Duration         │ {Fmt(ap?.TotalDuration),-13} │ {Fmt(sp?.TotalDuration),-13} │");
        Console.WriteLine($"│ TTFT             │ {"N/A",-13} │ {Fmt(sp?.TimeToFirstToken),-13} │");
        Console.WriteLine($"│ Input Tokens     │ {ap?.PromptTokens ?? 0,-13} │ {sp?.PromptTokens ?? 0,-13} │");
        Console.WriteLine($"│ Output Tokens    │ {ap?.CompletionTokens ?? 0,-13} │ {sp?.CompletionTokens ?? 0,-13} │");
        Console.WriteLine($"│ Est. Cost        │ {FmtCost(ap?.EstimatedCost),-13} │ {FmtCost(sp?.EstimatedCost),-13} │");
        Console.WriteLine("└──────────────────┴───────────────┴───────────────┘");

        Console.WriteLine("\n💡 KEY INSIGHTS:");
        Console.WriteLine("   ✅ Both methods capture token usage and costs!");
        Console.WriteLine("   ✅ Streaming provides Time-to-First-Token (TTFT)");
        Console.WriteLine("   ✅ TestOptions.ModelName enables accurate cost calculation");;
    }

    private static string Fmt(TimeSpan? t) => t.HasValue ? $"{t.Value.TotalMilliseconds:F0}ms" : "N/A";
    private static string FmtCost(decimal? c) => c > 0 ? $"${c:F6}" : "N/A";

    private static AIAgent CreateAgent(string endpoint, string apiKey, string deployment)
    {
        var azureClient = new AzureOpenAIClient(
            new Uri(endpoint),
            new System.ClientModel.ApiKeyCredential(apiKey));

        var chatClient = azureClient.GetChatClient(deployment).AsIChatClient();

        return new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "PerformanceTestAgent",
            Instructions = "You are a helpful assistant. Answer concisely."
        });
    }

    private static void PrintHeader()
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("   Sample 19: Streaming vs Non-Streaming Performance");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("   This sample demonstrates:");
        Console.WriteLine("   • Running the same test with streaming AND non-streaming");
        Console.WriteLine("   • Comparing token/cost capture between methods");
        Console.WriteLine("   • Using TestOptions.ModelName for accurate cost calculation");
        Console.WriteLine();
    }
}
