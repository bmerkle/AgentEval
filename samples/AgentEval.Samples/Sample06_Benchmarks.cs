// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

namespace AgentEval.Samples;

/// <summary>
/// Sample 06: Benchmarks - Running performance and agentic benchmarks
/// 
/// This demonstrates:
/// - Using PerformanceBenchmark concepts for latency/throughput testing
/// - Using AgenticBenchmark concepts for tool accuracy evaluation
/// - Statistical analysis (mean, p50, p90, p99)
/// - Comparing benchmark results
/// 
/// ⏱️ Time to understand: 5 minutes
/// </summary>
public static class Sample06_Benchmarks
{
    public static async Task RunAsync()
    {
        PrintHeader();

        // ═══════════════════════════════════════════════════════════════
        // STEP 1: Performance Benchmark Demonstration
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 1: Performance Benchmark Concepts...\n");
        
        await DemonstratePerformanceBenchmark();

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Agentic Benchmark Demonstration
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 2: Agentic Benchmark Concepts...\n");
        
        await DemonstrateAgenticBenchmark();

        // ═══════════════════════════════════════════════════════════════
        // STEP 3: Show the actual code patterns
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 3: Code patterns for benchmarking...\n");
        
        ShowCodePatterns();

        // ═══════════════════════════════════════════════════════════════
        // KEY TAKEAWAYS
        // ═══════════════════════════════════════════════════════════════
        PrintKeyTakeaways();
    }

    private static async Task DemonstratePerformanceBenchmark()
    {
        // Simulate benchmark results (in real usage, you'd use PerformanceBenchmark class)
        var prompts = new[]
        {
            "What is 2 + 2?",
            "Explain quantum computing in one sentence.",
            "List three benefits of exercise.",
            "What is the capital of France?",
            "Summarize the plot of Romeo and Juliet."
        };

        Console.WriteLine($"   Running {prompts.Length} prompts, 3 iterations each...\n");
        
        // Simulate running benchmarks
        var latencies = new List<double>();
        var random = new Random(42);
        
        for (int i = 0; i < prompts.Length * 3; i++)
        {
            await Task.Delay(10); // Simulate work
            latencies.Add(50 + random.NextDouble() * 100); // 50-150ms
        }

        latencies.Sort();
        
        var totalRuns = latencies.Count;
        var meanLatency = TimeSpan.FromMilliseconds(latencies.Average());
        var p50Latency = TimeSpan.FromMilliseconds(latencies[(int)(latencies.Count * 0.50)]);
        var p90Latency = TimeSpan.FromMilliseconds(latencies[(int)(latencies.Count * 0.90)]);
        var p99Latency = TimeSpan.FromMilliseconds(latencies[Math.Min(latencies.Count - 1, (int)(latencies.Count * 0.99))]);
        var totalTokens = 2500;
        var tokensPerSecond = 312.5;
        var estimatedCost = 0.0125m;

        // Display results
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │              PERFORMANCE BENCHMARK RESULTS              │");
        Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
        Console.WriteLine($"   │  Total Runs:           {totalRuns,30} │");
        Console.WriteLine($"   │  Mean Latency:         {meanLatency.TotalMilliseconds,27:F1} ms │");
        Console.WriteLine($"   │  P50 Latency:          {p50Latency.TotalMilliseconds,27:F1} ms │");
        Console.WriteLine($"   │  P90 Latency:          {p90Latency.TotalMilliseconds,27:F1} ms │");
        Console.WriteLine($"   │  P99 Latency:          {p99Latency.TotalMilliseconds,27:F1} ms │");
        Console.WriteLine($"   │  Total Tokens:         {totalTokens,30} │");
        Console.WriteLine($"   │  Tokens/Second:        {tokensPerSecond,27:F1} t/s │");
        Console.WriteLine($"   │  Estimated Cost:       ${estimatedCost,28:F4} │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");
    }

    private static async Task DemonstrateAgenticBenchmark()
    {
        // Define test cases
        var testCases = new[]
        {
            ("Weather Query", "What's the weather in Seattle?", new[] { "get_weather" }),
            ("Flight Search", "Find flights from NYC to LA", new[] { "search_flights" }),
            ("Multi-Step Task", "Book a hotel and check weather in Paris", new[] { "search_hotels", "get_weather" })
        };

        Console.WriteLine($"   Running {testCases.Length} agentic test cases...\n");

        // Simulate running tests
        await Task.Delay(50);

        var totalTests = 3;
        var passedTests = 3;
        var toolSelectionAccuracy = 0.95;
        var argumentAccuracy = 0.88;
        var taskCompletionRate = 1.0;
        var meanToolCalls = 1.67;

        // Display results
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │               AGENTIC BENCHMARK RESULTS                 │");
        Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
        Console.WriteLine($"   │  Total Tests:          {totalTests,30} │");
        Console.WriteLine($"   │  Passed:               {passedTests,30} │");
        Console.WriteLine($"   │  Tool Accuracy:        {toolSelectionAccuracy * 100,27:F1}% │");
        Console.WriteLine($"   │  Argument Accuracy:    {argumentAccuracy * 100,27:F1}% │");
        Console.WriteLine($"   │  Task Completion:      {taskCompletionRate * 100,27:F1}% │");
        Console.WriteLine($"   │  Mean Tool Calls:      {meanToolCalls,27:F1} │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

        // Show per-test details
        Console.WriteLine("\n   📋 Per-Test Results:");
        foreach (var (name, _, tools) in testCases)
        {
            Console.WriteLine($"      ✅ {name}: {tools.Length} tools, ~85ms");
        }
    }

    private static void ShowCodePatterns()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"   // ════════════════════════════════════════════════════════════
   // PERFORMANCE BENCHMARK PATTERN
   // ════════════════════════════════════════════════════════════
   
   var benchmark = new PerformanceBenchmark();
   var results = await benchmark.RunAsync(
       chatClient: myClient,
       prompts: testPrompts,
       iterations: 10,
       warmupIterations: 2);

   // Access statistical metrics
   Console.WriteLine($""Mean: {results.MeanLatency}"");
   Console.WriteLine($""P90:  {results.P90Latency}"");
   Console.WriteLine($""P99:  {results.P99Latency}"");
   Console.WriteLine($""Cost: ${results.EstimatedTotalCost}"");

   // ════════════════════════════════════════════════════════════
   // AGENTIC BENCHMARK PATTERN  
   // ════════════════════════════════════════════════════════════
   
   var benchmark = new AgenticBenchmark();
   var results = await benchmark.RunAsync(myAgent, testCases);

   // Evaluate tool accuracy
   Assert.True(results.ToolSelectionAccuracy >= 0.9);
   Assert.True(results.TaskCompletionRate >= 0.95);

   // ════════════════════════════════════════════════════════════
   // CI/CD REGRESSION PATTERN
   // ════════════════════════════════════════════════════════════
   
   var current = await benchmark.RunAsync(agent, testCases);
   var baseline = await LoadBaseline(""benchmark-baseline.json"");

   // Fail if latency regressed more than 20%
   Assert.True(current.P90Latency <= baseline.P90Latency * 1.2,
       $""Latency regressed: {current.P90Latency} vs {baseline.P90Latency}"");
");
        Console.ResetColor();
    }

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║              Sample 06: Performance & Agentic Benchmarks                      ║
║                                                                               ║
║   Learn how to:                                                               ║
║   • Run latency and throughput benchmarks                                     ║
║   • Measure tool selection accuracy                                           ║
║   • Get statistical analysis (p50, p90, p99)                                  ║
║   • Compare benchmark results across runs                                     ║
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
│  1. PerformanceBenchmark measures latency, throughput, and cost                 │
│     var results = await benchmark.RunAsync(client, prompts, iterations: 10);    │
│                                                                                 │
│  2. AgenticBenchmark measures tool accuracy and task completion                 │
│     var results = await benchmark.RunAsync(agent, testCases);                   │
│                                                                                 │
│  3. Statistical analysis provides p50, p90, p99 percentiles                     │
│     results.P90Latency  // 90th percentile latency                              │
│                                                                                 │
│  4. Use benchmarks for:                                                         │
│     • Baseline establishment before changes                                     │
│     • Regression detection in CI/CD                                             │
│     • Model comparison (GPT-4 vs Claude vs Gemini)                              │
│     • Cost optimization                                                         │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
");
        Console.ResetColor();
    }
}
