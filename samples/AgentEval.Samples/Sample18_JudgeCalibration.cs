// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

using AgentEval.Calibration;
using AgentEval.Core;
using AgentEval.Metrics.RAG;
using AgentEval.Testing;
using Microsoft.Extensions.AI;

namespace AgentEval.Samples;

/// <summary>
/// Sample 18: Judge Calibration - Multi-model consensus for reliable LLM-as-judge evaluations.
/// 
/// This demonstrates:
/// - Using CalibratedJudge for multi-model evaluation
/// - Different voting strategies (Median, Mean, Unanimous, Weighted)
/// - Agreement scores and confidence intervals
/// - Graceful degradation when judges fail
/// 
/// ⏱️ Time to understand: 8 minutes
/// </summary>
public static class Sample18_JudgeCalibration
{
    public static async Task RunAsync()
    {
        PrintHeader();
        
        // ═══════════════════════════════════════════════════════════════
        // STEP 1: Why Calibrated Judges?
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 1: Why use CalibratedJudge?\n");
        
        Console.WriteLine(@"   LLM-as-judge evaluations have inherent variance:
   • A single LLM may give inconsistent scores across runs
   • Different models have different biases
   • Single judge errors can skew entire evaluations
   
   CalibratedJudge solves this by:
   ✓ Running the same evaluation with multiple LLM judges
   ✓ Aggregating scores using configurable voting strategies
   ✓ Calculating agreement percentages and confidence intervals
   ✓ Providing graceful degradation if individual judges fail
");

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Create Fake Judges for Demo
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 2: Creating mock judges for demonstration...\n");
        
        // Simulate 3 different judges with slightly different scoring patterns
        var gpt4oClient = new FakeChatClient("""{"score": 85, "explanation": "Good faithfulness, well grounded in context."}""");
        var claudeClient = new FakeChatClient("""{"score": 88, "explanation": "Response accurately reflects source material."}""");
        var geminiClient = new FakeChatClient("""{"score": 82, "explanation": "Mostly faithful with minor unsupported claims."}""");
        
        // Create named judge dictionary for the factory pattern
        var judges = new Dictionary<string, IChatClient>
        {
            ["GPT-4o"] = gpt4oClient,
            ["Claude-3.5"] = claudeClient,
            ["Gemini-Pro"] = geminiClient
        };
        
        Console.WriteLine("   ✓ GPT-4o judge (will score ~85)");
        Console.WriteLine("   ✓ Claude-3.5 judge (will score ~88)");
        Console.WriteLine("   ✓ Gemini-Pro judge (will score ~82)\n");

        // ═══════════════════════════════════════════════════════════════
        // STEP 3: Create CalibratedJudge with Median Strategy
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 3: Creating CalibratedJudge with Median voting...\n");
        
        var calibratedJudge = CalibratedJudge.Create(
            ("GPT-4o", gpt4oClient),
            ("Claude-3.5", claudeClient),
            ("Gemini-Pro", geminiClient));
        
        Console.WriteLine($"   Judges: {string.Join(", ", calibratedJudge.JudgeNames)}");
        Console.WriteLine($"   Strategy: {calibratedJudge.Options.Strategy}");
        Console.WriteLine($"   Max Parallel: {calibratedJudge.Options.MaxParallelJudges}");
        Console.WriteLine($"   Timeout: {calibratedJudge.Options.Timeout.TotalSeconds}s\n");

        // ═══════════════════════════════════════════════════════════════
        // STEP 4: Run Calibrated Evaluation
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 4: Running calibrated evaluation...\n");
        
        var context = new EvaluationContext
        {
            Input = "What are the main benefits of renewable energy?",
            Output = "Renewable energy reduces carbon emissions and provides sustainable power sources.",
            Context = "Renewable energy comes from natural sources that replenish themselves. Key benefits include: reduced greenhouse gas emissions, energy independence, lower long-term costs, and sustainable power generation.",
            GroundTruth = "Benefits include reduced emissions, sustainability, and energy independence."
        };
        
        // Use factory pattern - each judge gets its own metric with its own client
        var result = await calibratedJudge.EvaluateAsync(context, judgeName =>
        {
            return new FaithfulnessMetric(judges[judgeName]);
        });
        
        // Display results
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("   ┌──────────────────────────────────────────────┐");
        Console.WriteLine($"   │  Final Score: {result.Score,6:F1} (Median)              │");
        Console.WriteLine($"   │  Agreement:   {result.Agreement,6:F1}%                      │");
        Console.WriteLine($"   │  Std Dev:     {result.StandardDeviation,6:F2}                       │");
        Console.WriteLine($"   │  Consensus:   {(result.HasConsensus ? "Yes ✓" : "No ✗"),-6}                      │");
        Console.WriteLine("   └──────────────────────────────────────────────┘");
        Console.ResetColor();
        
        Console.WriteLine("\n   Individual Judge Scores:");
        foreach (var (judgeName, score) in result.JudgeScores)
        {
            Console.WriteLine($"   • {judgeName}: {score:F1}");
        }
        
        if (result.ConfidenceLower.HasValue && result.ConfidenceUpper.HasValue)
        {
            Console.WriteLine($"\n   95% CI: [{result.ConfidenceLower:F1}, {result.ConfidenceUpper:F1}]");
        }
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════
        // STEP 5: Different Voting Strategies
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 5: Comparing voting strategies...\n");
        
        // Mean strategy
        var meanJudge = new CalibratedJudge(
            [("GPT-4o", gpt4oClient), ("Claude-3.5", claudeClient), ("Gemini-Pro", geminiClient)],
            new CalibratedJudgeOptions { Strategy = VotingStrategy.Mean });
        
        var meanResult = await meanJudge.EvaluateAsync(context, jn => new FaithfulnessMetric(judges[jn]));
        
        // Unanimous strategy (requires consensus)
        var unanimousJudge = new CalibratedJudge(
            [("GPT-4o", gpt4oClient), ("Claude-3.5", claudeClient), ("Gemini-Pro", geminiClient)],
            new CalibratedJudgeOptions { Strategy = VotingStrategy.Unanimous, ConsensusTolerance = 10 });
        
        CalibratedResult? unanimousResult = null;
        try
        {
            unanimousResult = await unanimousJudge.EvaluateAsync(context, jn => new FaithfulnessMetric(judges[jn]));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"   ⚠️ Unanimous strategy failed: {ex.Message}");
        }
        
        Console.WriteLine("   Strategy Comparison:");
        Console.WriteLine("   ┌─────────────┬─────────┬───────────┐");
        Console.WriteLine("   │ Strategy    │  Score  │ Agreement │");
        Console.WriteLine("   ├─────────────┼─────────┼───────────┤");
        Console.WriteLine($"   │ Median      │  {result.Score,5:F1}  │   {result.Agreement,5:F1}%  │");
        Console.WriteLine($"   │ Mean        │  {meanResult.Score,5:F1}  │   {meanResult.Agreement,5:F1}%  │");
        if (unanimousResult != null)
        {
            Console.WriteLine($"   │ Unanimous   │  {unanimousResult.Score,5:F1}  │   {unanimousResult.Agreement,5:F1}%  │");
        }
        else
        {
            Console.WriteLine("   │ Unanimous   │  N/A    │   N/A     │");
        }
        Console.WriteLine("   └─────────────┴─────────┴───────────┘\n");

        // ═══════════════════════════════════════════════════════════════
        // STEP 6: Weighted Strategy
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 6: Weighted voting (trust GPT-4o more)...\n");
        
        var weightedJudge = new CalibratedJudge(
            [("GPT-4o", gpt4oClient), ("Claude-3.5", claudeClient), ("Gemini-Pro", geminiClient)],
            new CalibratedJudgeOptions
            {
                Strategy = VotingStrategy.Weighted,
                JudgeWeights = new Dictionary<string, double>
                {
                    ["GPT-4o"] = 2.0,      // Trust GPT-4o twice as much
                    ["Claude-3.5"] = 1.0,
                    ["Gemini-Pro"] = 1.0
                }
            });
        
        var weightedResult = await weightedJudge.EvaluateAsync(context, jn => new FaithfulnessMetric(judges[jn]));
        
        Console.WriteLine($"   Weighted Score: {weightedResult.Score:F1}");
        Console.WriteLine("   Weights: GPT-4o=2.0, Claude=1.0, Gemini=1.0");
        Console.WriteLine($"   (Biased toward GPT-4o's score of {result.JudgeScores["GPT-4o"]:F1})\n");

        // ═══════════════════════════════════════════════════════════════
        // STEP 7: Real-World Usage Pattern
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 7: Real-world usage pattern...\n");
        
        ShowCodeExample();

        PrintKeyTakeaways();
    }
    
    private static void ShowCodeExample()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"   // Production usage with real Azure OpenAI clients:
   
   var judge = CalibratedJudge.Create(
       (""GPT-4o"", azureClient.GetChatClient(""gpt-4o"").AsIChatClient()),
       (""Claude"", claudeClient),
       (""Gemini"", geminiClient));
   
   // Factory pattern ensures each judge uses its own client
   var result = await judge.EvaluateAsync(context, judgeName =>
   {
       return judgeName switch
       {
           ""GPT-4o"" => new FaithfulnessMetric(gpt4oClient),
           ""Claude"" => new FaithfulnessMetric(claudeClient),
           ""Gemini"" => new FaithfulnessMetric(geminiClient),
           _ => throw new ArgumentException($""Unknown judge: {judgeName}"")
       };
   });
   
   Console.WriteLine($""Score: {result.Score:F1}, Agreement: {result.Agreement:F0}%"");
   Console.WriteLine($""95% CI: [{result.ConfidenceLower:F1}, {result.ConfidenceUpper:F1}]"");
");
        Console.ResetColor();
    }
    
    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║            Sample 18: Judge Calibration (Multi-Model Consensus)               ║
║                                                                               ║
║   Learn how to:                                                               ║
║   • Use multiple LLM judges for reliable evaluations                          ║
║   • Apply different voting strategies (Median, Mean, Weighted)                ║
║   • Interpret agreement scores and confidence intervals                       ║
║   • Handle graceful degradation when judges fail                              ║
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
│  1. USE FACTORY PATTERN for per-judge metric instantiation:                     │
│     judge.EvaluateAsync(ctx, judgeName => new Metric(clients[judgeName]))      │
│                                                                                 │
│  2. MEDIAN STRATEGY is robust to outliers and biased judges                     │
│                                                                                 │
│  3. AGREEMENT SCORE shows how much judges agree (100% = identical)              │
│                                                                                 │
│  4. CONFIDENCE INTERVALS quantify uncertainty in the score                      │
│                                                                                 │
│  5. GRACEFUL DEGRADATION continues if some judges fail                          │
│                                                                                 │
│  6. USE WEIGHTED voting when you trust certain models more                      │
│                                                                                 │
│  7. USE UNANIMOUS for high-stakes decisions requiring consensus                 │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
");
        Console.ResetColor();
    }
}
