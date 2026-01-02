// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using AgentEval.Core;
using AgentEval.Metrics.RAG;
using AgentEval.Testing;

namespace AgentEval.Samples;

/// <summary>
/// Sample 05: RAG Evaluation - Testing retrieval-augmented generation quality
/// 
/// This demonstrates:
/// - Evaluating RAG systems with FaithfulnessMetric
/// - Using EvaluationContext for RAG testing
/// - Understanding hallucination detection
/// 
/// ⏱️ Time to understand: 5 minutes
/// </summary>
public static class Sample05_RAGEvaluation
{
    public static async Task RunAsync()
    {
        PrintHeader();

        // ═══════════════════════════════════════════════════════════════
        // Understanding RAG Evaluation
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine(@"
   📖 WHAT IS RAG EVALUATION?
   
   RAG (Retrieval-Augmented Generation) systems retrieve context from a 
   knowledge base and use it to generate responses. We need to evaluate:
   
   • Faithfulness - Is the response grounded in the context? (No hallucinations)
   • Relevance    - Is the response relevant to the question?
   • Precision    - Was the retrieved context useful?
   • Recall       - Did we retrieve all needed context?
   • Correctness  - Does the answer match ground truth?
   
   This sample focuses on FAITHFULNESS - the most critical RAG metric.
");

        // ═══════════════════════════════════════════════════════════════
        // STEP 1: Set up the evaluator
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 1: Creating FaithfulnessMetric...\n");
        
        IChatClient evaluatorClient = CreateEvaluatorClient();
        var faithfulnessMetric = new FaithfulnessMetric(evaluatorClient);
        
        Console.WriteLine($"   ✓ Metric: {faithfulnessMetric.Name}");
        Console.WriteLine($"   📋 {faithfulnessMetric.Description}\n");

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Test Case 1 - Faithful Response (No Hallucinations)
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 2: Testing a FAITHFUL response (grounded in context)...\n");
        
        var faithfulContext = new EvaluationContext
        {
            Input = "What is the capital of France?",
            Context = """
                France is a country in Western Europe. Its capital city is Paris, 
                which is also its largest city with a population of over 2 million 
                in the city proper. Paris is known for the Eiffel Tower and the Louvre.
                """,
            Output = "The capital of France is Paris. It is the largest city in France."
        };
        
        Console.WriteLine($"   Question: {faithfulContext.Input}");
        Console.WriteLine($"   Context:  \"{Truncate(faithfulContext.Context, 80)}...\"");
        Console.WriteLine($"   Response: \"{faithfulContext.Output}\"");
        Console.WriteLine();
        
        var result1 = await faithfulnessMetric.EvaluateAsync(faithfulContext);
        PrintMetricResult(result1, "Faithful Response");

        // ═══════════════════════════════════════════════════════════════
        // STEP 3: Test Case 2 - Hallucinated Response
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 3: Testing a HALLUCINATED response (makes up facts)...\n");
        
        var hallucinatedContext = new EvaluationContext
        {
            Input = "What is the capital of France?",
            Context = """
                France is a country in Western Europe. Its capital city is Paris, 
                which is also its largest city.
                """,
            Output = """
                The capital of France is Paris. It was founded by the Romans in 52 BC
                and has a population of exactly 12.5 million people. The Eiffel Tower
                was built in 1789 during the French Revolution.
                """
        };
        
        Console.WriteLine($"   Question: {hallucinatedContext.Input}");
        Console.WriteLine($"   Context:  \"{Truncate(hallucinatedContext.Context, 80)}\"");
        Console.WriteLine($"   Response: \"{Truncate(hallucinatedContext.Output, 100)}...\"");
        Console.WriteLine();
        
        var result2 = await faithfulnessMetric.EvaluateAsync(hallucinatedContext);
        PrintMetricResult(result2, "Hallucinated Response");

        // ═══════════════════════════════════════════════════════════════
        // STEP 4: Test Case 3 - Partial Faithfulness
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 4: Testing a PARTIALLY faithful response...\n");
        
        var partialContext = new EvaluationContext
        {
            Input = "Tell me about the Eiffel Tower",
            Context = """
                The Eiffel Tower is a wrought-iron lattice tower on the Champ de Mars 
                in Paris. It was constructed from 1887 to 1889 as the entrance arch 
                for the 1889 World's Fair. The tower is 330 meters tall.
                """,
            Output = """
                The Eiffel Tower is located in Paris on the Champ de Mars. It was built 
                for the 1889 World's Fair and stands 330 meters tall. It receives about 
                7 million visitors annually and is painted in a special shade of brown.
                """
            // Note: "7 million visitors" and "special shade of brown" are not in context
        };
        
        Console.WriteLine($"   Question: {partialContext.Input}");
        Console.WriteLine($"   Context:  \"{Truncate(partialContext.Context, 80)}...\"");
        Console.WriteLine($"   Response: \"{Truncate(partialContext.Output, 100)}...\"");
        Console.WriteLine();
        
        var result3 = await faithfulnessMetric.EvaluateAsync(partialContext);
        PrintMetricResult(result3, "Partially Faithful Response");

        // ═══════════════════════════════════════════════════════════════
        // Available RAG Metrics
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📊 AVAILABLE RAG METRICS:");
        Console.WriteLine(new string('─', 60));
        Console.WriteLine(@"
   ┌─────────────────────────────────────────────────────────────┐
   │  METRIC                 │ PURPOSE                           │
   ├─────────────────────────────────────────────────────────────┤
   │  FaithfulnessMetric     │ No hallucinations in response     │
   │  RelevanceMetric        │ Response addresses the question   │
   │  ContextPrecisionMetric │ Retrieved context was useful      │
   │  ContextRecallMetric    │ All needed context was retrieved  │
   │  AnswerCorrectnessMetric│ Response matches ground truth     │
   └─────────────────────────────────────────────────────────────┘

   USAGE:
   ┌─────────────────────────────────────────────────────────────┐
   │  var context = new EvaluationContext                        │
   │  {                                                          │
   │      Input = ""user question"",                               │
   │      Output = ""agent response"",                             │
   │      Context = ""retrieved context"",  // For RAG metrics    │
   │      GroundTruth = ""expected answer""  // For correctness   │
   │  };                                                         │
   │                                                             │
   │  var result = await metric.EvaluateAsync(context);          │
   │  Console.WriteLine($""Score: {result.Score}"");               │
   │  Console.WriteLine($""Passed: {result.Passed}"");             │
   └─────────────────────────────────────────────────────────────┘
");

        // ═══════════════════════════════════════════════════════════════
        // Summary
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n💡 KEY TAKEAWAYS:");
        Console.WriteLine("   • RAG metrics evaluate retrieval + generation quality");
        Console.WriteLine("   • FaithfulnessMetric detects hallucinations");
        Console.WriteLine("   • EvaluationContext holds Input, Output, Context, GroundTruth");
        Console.WriteLine("   • Metrics return score (0-100), passed, and explanation");
        Console.WriteLine("   • Use FakeChatClient for testing metrics without real LLM");
        
        Console.WriteLine("\n🎉 SAMPLES COMPLETE! You're ready to use AgentEval.\n");
    }

    private static IChatClient CreateEvaluatorClient()
    {
        if (AIConfig.IsConfigured)
        {
            var azureClient = new AzureOpenAIClient(AIConfig.Endpoint, AIConfig.KeyCredential);
            return azureClient.GetChatClient(AIConfig.ModelDeployment).AsIChatClient();
        }
        
        // Use FakeChatClient for demo without Azure
        Console.WriteLine("   ℹ️  Using FakeChatClient (no Azure credentials configured)\n");
        var fake = new FakeChatClient()
            .WithResponse("""
            {
                "score": 95,
                "faithfulClaims": ["The capital of France is Paris", "It is the largest city in France"],
                "hallucinatedClaims": [],
                "reasoning": "All claims are directly supported by the provided context."
            }
            """)
            .WithResponse("""
            {
                "score": 25,
                "faithfulClaims": ["The capital of France is Paris"],
                "hallucinatedClaims": [
                    "Founded by Romans in 52 BC",
                    "Population of exactly 12.5 million",
                    "Eiffel Tower built in 1789"
                ],
                "reasoning": "The response contains multiple fabricated facts not present in the context."
            }
            """)
            .WithResponse("""
            {
                "score": 70,
                "faithfulClaims": [
                    "Eiffel Tower is in Paris on Champ de Mars",
                    "Built for 1889 World's Fair",
                    "Stands 330 meters tall"
                ],
                "hallucinatedClaims": [
                    "7 million visitors annually",
                    "Special shade of brown"
                ],
                "reasoning": "Core facts are supported but visitor count and color details are not in context."
            }
            """);
        
        return fake;
    }

    private static void PrintMetricResult(MetricResult result, string testName)
    {
        Console.ForegroundColor = result.Passed ? ConsoleColor.Green : ConsoleColor.Red;
        var icon = result.Passed ? "✅" : "❌";
        Console.WriteLine($"   {icon} {testName}:");
        Console.ResetColor();
        
        Console.WriteLine($"      Score: {result.Score}/100");
        Console.WriteLine($"      Status: {(result.Passed ? "PASSED" : "FAILED")}");
        Console.WriteLine($"      Reason: {Truncate(result.Explanation ?? "No explanation", 80)}");
        
        if (result.Details != null && result.Details.TryGetValue("hallucinatedClaims", out var claims) && claims is List<string> claimList && claimList.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"      ⚠️ Hallucinations: {string.Join(", ", claimList.Take(3))}");
            Console.ResetColor();
        }
    }

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║   📊 SAMPLE 05: RAG EVALUATION                                               ║
║   Testing faithfulness and hallucination detection                            ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "";
        text = text.Replace("\n", " ").Replace("\r", "");
        if (text.Length <= maxLength) return text;
        return text[..maxLength] + "...";
    }
}
