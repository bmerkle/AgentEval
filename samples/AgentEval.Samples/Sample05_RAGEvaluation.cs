// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using AgentEval.Core;
using AgentEval.Metrics.RAG;
using AgentEval.Testing;

namespace AgentEval.Samples;

/// <summary>
/// Sample 05: RAG Evaluation - Complete guide to RAG quality metrics
/// 
/// This demonstrates:
/// - FaithfulnessMetric - Hallucination detection (is response grounded in context?)
/// - RelevanceMetric - Response quality (does it address the question?)
/// - ContextPrecisionMetric - Retrieval quality (was retrieved context useful?)
/// - ContextRecallMetric - Retrieval coverage (was all needed context retrieved?)
/// - AnswerCorrectnessMetric - Accuracy (does response match ground truth?)
/// - Using EvaluationContext for RAG testing
/// 
/// ⏱️ Time to understand: 8 minutes
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
   📖 COMPLETE RAG EVALUATION GUIDE
   
   RAG (Retrieval-Augmented Generation) systems need multi-dimensional evaluation:
   
   ┌─────────────────────────────────────────────────────────────────────┐
   │  METRIC              │ WHAT IT MEASURES                            │
   ├─────────────────────────────────────────────────────────────────────┤
   │  FaithfulnessMetric  │ No hallucinations (grounded in context)     │
   │  RelevanceMetric     │ Response addresses the question             │
   │  ContextPrecision    │ Retrieved context was useful                │
   │  ContextRecall       │ All needed context was retrieved            │
   │  AnswerCorrectness   │ Response matches expected answer            │
   └─────────────────────────────────────────────────────────────────────┘
   
   This sample demonstrates ALL five RAG metrics with examples.
");

        IChatClient evaluatorClient = CreateEvaluatorClient();

        // ═══════════════════════════════════════════════════════════════
        // PART 1: FaithfulnessMetric - Hallucination Detection
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("📝 PART 1: FAITHFULNESS METRIC");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        var faithfulnessMetric = new FaithfulnessMetric(evaluatorClient);
        Console.WriteLine($"   Metric: {faithfulnessMetric.Name}");
        Console.WriteLine($"   Categories: {faithfulnessMetric.Categories}");
        Console.WriteLine($"   📋 {faithfulnessMetric.Description}\n");

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Test Case 1 - Faithful Response (No Hallucinations)
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("   🧪 Test 1: A FAITHFUL response (grounded in context)\n");
        
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
        
        Console.WriteLine($"      Question: {faithfulContext.Input}");
        Console.WriteLine($"      Context:  \"{Truncate(faithfulContext.Context, 70)}...\"");
        Console.WriteLine($"      Response: \"{faithfulContext.Output}\"");
        
        var result1 = await faithfulnessMetric.EvaluateAsync(faithfulContext);
        PrintMetricResult(result1, "Faithful Response");

        // Test Case 2 - Hallucinated Response
        Console.WriteLine("\n   🧪 Test 2: A HALLUCINATED response (makes up facts)\n");
        
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
        
        Console.WriteLine($"      Question: {hallucinatedContext.Input}");
        Console.WriteLine($"      Response contains fabricated details ✗");
        
        var result2 = await faithfulnessMetric.EvaluateAsync(hallucinatedContext);
        PrintMetricResult(result2, "Hallucinated Response", expectFail: true);

        // ═══════════════════════════════════════════════════════════════
        // PART 2: RelevanceMetric - Does Response Address the Question?
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("📝 PART 2: RELEVANCE METRIC");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        var relevanceMetric = new RelevanceMetric(evaluatorClient);
        Console.WriteLine($"   Metric: {relevanceMetric.Name}");
        Console.WriteLine($"   Categories: {relevanceMetric.Categories}");
        Console.WriteLine($"   📋 {relevanceMetric.Description}\n");

        // Relevant response
        Console.WriteLine("   🧪 Test 3: A RELEVANT response (addresses the question)\n");
        
        var relevantContext = new EvaluationContext
        {
            Input = "What are the main benefits of exercise?",
            Output = """
                The main benefits of exercise include improved cardiovascular health,
                stronger muscles and bones, better mental health and mood, weight
                management, and increased energy levels. Regular exercise also helps
                reduce the risk of chronic diseases.
                """
        };
        
        Console.WriteLine($"      Question: {relevantContext.Input}");
        
        var result3 = await relevanceMetric.EvaluateAsync(relevantContext);
        PrintMetricResult(result3, "Relevant Response");

        // Irrelevant response
        Console.WriteLine("\n   🧪 Test 4: An IRRELEVANT response (off-topic)\n");
        
        var irrelevantContext = new EvaluationContext
        {
            Input = "What are the main benefits of exercise?",
            Output = """
                The history of the Olympics dates back to ancient Greece where the
                first games were held in 776 BC. The modern Olympics were revived
                in 1896 in Athens. The Olympic rings represent the five continents.
                """
        };
        
        Console.WriteLine($"      Question: {irrelevantContext.Input}");
        Console.WriteLine($"      Response is off-topic (Olympics history vs exercise benefits) ✗");
        
        var result4 = await relevanceMetric.EvaluateAsync(irrelevantContext);
        PrintMetricResult(result4, "Irrelevant Response", expectFail: true);

        // ═══════════════════════════════════════════════════════════════
        // PART 3: ContextPrecisionMetric - Was Retrieved Context Useful?
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("📝 PART 3: CONTEXT PRECISION METRIC");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        var precisionMetric = new ContextPrecisionMetric(evaluatorClient);
        Console.WriteLine($"   Metric: {precisionMetric.Name}");
        Console.WriteLine($"   📋 {precisionMetric.Description}\n");

        // High precision context
        Console.WriteLine("   🧪 Test 5: HIGH PRECISION context (all relevant)\n");
        
        var highPrecisionContext = new EvaluationContext
        {
            Input = "What is the boiling point of water?",
            Output = "Water boils at 100°C at standard pressure.",
            Context = """
                Water boils at 100 degrees Celsius (212 degrees Fahrenheit) at 
                standard atmospheric pressure. At higher altitudes, water boils 
                at lower temperatures due to reduced atmospheric pressure.
                """
        };
        
        Console.WriteLine($"      Question: {highPrecisionContext.Input}");
        Console.WriteLine($"      Context is focused and directly relevant ✓");
        
        var result5 = await precisionMetric.EvaluateAsync(highPrecisionContext);
        PrintMetricResult(result5, "High Precision Context");

        // Low precision context
        Console.WriteLine("\n   🧪 Test 6: LOW PRECISION context (mostly noise)\n");
        
        var lowPrecisionContext = new EvaluationContext
        {
            Input = "What is the boiling point of water?",
            Output = "Water boils at 100°C.",
            Context = """
                Water is essential for life on Earth. It covers about 71% of the 
                Earth's surface. The water cycle involves evaporation, condensation, 
                and precipitation. Fish live in water. Humans need about 8 glasses 
                of water daily. By the way, water boils at 100°C. Ocean water is 
                salty. Ice floats because it's less dense than liquid water.
                """
        };
        
        Console.WriteLine($"      Question: {lowPrecisionContext.Input}");
        Console.WriteLine($"      Context has lots of irrelevant info (water facts) ✗");
        
        var result6 = await precisionMetric.EvaluateAsync(lowPrecisionContext);
        PrintMetricResult(result6, "Low Precision Context", expectFail: true);

        // ═══════════════════════════════════════════════════════════════
        // PART 4: ContextRecallMetric - Was All Needed Info Retrieved?
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("📝 PART 4: CONTEXT RECALL METRIC");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        var recallMetric = new ContextRecallMetric(evaluatorClient);
        Console.WriteLine($"   Metric: {recallMetric.Name}");
        Console.WriteLine($"   📋 {recallMetric.Description}\n");
        Console.WriteLine("   ⚠️ Note: This metric requires GroundTruth!\n");

        // High recall context
        Console.WriteLine("   🧪 Test 7: HIGH RECALL context (contains all needed info)\n");
        
        var highRecallContext = new EvaluationContext
        {
            Input = "What are the ingredients in a margherita pizza?",
            Output = "Margherita pizza has tomato sauce, mozzarella, basil, and olive oil.",
            Context = """
                A traditional Margherita pizza consists of tomato sauce, fresh 
                mozzarella cheese, fresh basil leaves, and olive oil on a thin 
                crust. It was named after Queen Margherita of Italy in 1889.
                """,
            GroundTruth = "Margherita pizza contains tomato sauce, mozzarella cheese, basil, and olive oil."
        };
        
        Console.WriteLine($"      Question: {highRecallContext.Input}");
        Console.WriteLine($"      Ground Truth: {highRecallContext.GroundTruth}");
        
        var result7 = await recallMetric.EvaluateAsync(highRecallContext);
        PrintMetricResult(result7, "High Recall Context");

        // Low recall context
        Console.WriteLine("\n   🧪 Test 8: LOW RECALL context (missing key info)\n");
        
        var lowRecallContext = new EvaluationContext
        {
            Input = "What are the ingredients in a margherita pizza?",
            Output = "Margherita pizza was created in Naples in 1889.",
            Context = """
                Margherita pizza is a classic Italian dish. It was created in 
                Naples in 1889. The pizza represents the Italian flag colors.
                """,
            GroundTruth = "Margherita pizza contains tomato sauce, mozzarella cheese, basil, and olive oil."
        };
        
        Console.WriteLine($"      Question: {lowRecallContext.Input}");
        Console.WriteLine($"      Context missing all ingredient information ✗");
        
        var result8 = await recallMetric.EvaluateAsync(lowRecallContext);
        PrintMetricResult(result8, "Low Recall Context", expectFail: true);

        // ═══════════════════════════════════════════════════════════════
        // PART 5: AnswerCorrectnessMetric - Does Answer Match Ground Truth?
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("📝 PART 5: ANSWER CORRECTNESS METRIC");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        var correctnessMetric = new AnswerCorrectnessMetric(evaluatorClient);
        Console.WriteLine($"   Metric: {correctnessMetric.Name}");
        Console.WriteLine($"   📋 {correctnessMetric.Description}\n");
        Console.WriteLine("   ⚠️ Note: This metric requires GroundTruth!\n");

        // Correct answer
        Console.WriteLine("   🧪 Test 9: CORRECT answer (matches ground truth)\n");
        
        var correctContext = new EvaluationContext
        {
            Input = "How many planets are in our solar system?",
            Output = "There are 8 planets in our solar system: Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, and Neptune.",
            GroundTruth = "Our solar system has 8 planets."
        };
        
        Console.WriteLine($"      Question: {correctContext.Input}");
        Console.WriteLine($"      Ground Truth: {correctContext.GroundTruth}");
        
        var result9 = await correctnessMetric.EvaluateAsync(correctContext);
        PrintMetricResult(result9, "Correct Answer");

        // Incorrect answer
        Console.WriteLine("\n   🧪 Test 10: INCORRECT answer (wrong facts)\n");
        
        var incorrectContext = new EvaluationContext
        {
            Input = "How many planets are in our solar system?",
            Output = "There are 9 planets in our solar system, including Pluto.",
            GroundTruth = "Our solar system has 8 planets. Pluto was reclassified as a dwarf planet in 2006."
        };
        
        Console.WriteLine($"      Question: {incorrectContext.Input}");
        Console.WriteLine($"      Response counts Pluto as a planet (outdated) ✗");
        
        var result10 = await correctnessMetric.EvaluateAsync(incorrectContext);
        PrintMetricResult(result10, "Incorrect Answer", expectFail: true);

        // ═══════════════════════════════════════════════════════════════
        // Summary: RAG Metrics Cheat Sheet
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("📊 RAG METRICS QUICK REFERENCE");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine(@"
   ┌─────────────────────────────────────────────────────────────────────┐
   │  METRIC              │ REQUIRES        │ EVALUATES                  │
   ├─────────────────────────────────────────────────────────────────────┤
   │  FaithfulnessMetric  │ Context         │ Response ← Context         │
   │  RelevanceMetric     │ (none)          │ Response ← Question        │
   │  ContextPrecision    │ Context         │ Context usefulness         │
   │  ContextRecall       │ Context + Truth │ Context completeness       │
   │  AnswerCorrectness   │ GroundTruth     │ Response ← GroundTruth     │
   └─────────────────────────────────────────────────────────────────────┘

   EVALUATION WORKFLOW:
   ┌─────────────────────────────────────────────────────────────────────┐
   │  1. RETRIEVAL QUALITY: ContextPrecision + ContextRecall            │
   │     → Is your retriever finding the right documents?               │
   │                                                                     │
   │  2. GENERATION QUALITY: Faithfulness + Relevance                   │
   │     → Is your generator using context well?                        │
   │                                                                     │
   │  3. END-TO-END: AnswerCorrectness                                  │
   │     → Does the full system produce correct answers?                │
   └─────────────────────────────────────────────────────────────────────┘

   COMMON PATTERNS:
   ┌─────────────────────────────────────────────────────────────────────┐
   │  // Evaluate a RAG response comprehensively                         │
   │  var context = new EvaluationContext                                │
   │  {                                                                  │
   │      Input = ""user question"",                                       │
   │      Output = ""agent response"",                                     │
   │      Context = ""retrieved documents"",                               │
   │      GroundTruth = ""expected answer""                                │
   │  };                                                                 │
   │                                                                     │
   │  var metrics = new IMetric[]                                        │
   │  {                                                                  │
   │      new FaithfulnessMetric(client),                                │
   │      new RelevanceMetric(client),                                   │
   │      new ContextPrecisionMetric(client),                            │
   │      new AnswerCorrectnessMetric(client)                            │
   │  };                                                                 │
   │                                                                     │
   │  foreach (var metric in metrics)                                    │
   │  {                                                                  │
   │      var result = await metric.EvaluateAsync(context);              │
   │      Console.WriteLine($""{metric.Name}: {result.Score}/100"");       │
   │  }                                                                  │
   └─────────────────────────────────────────────────────────────────────┘
");

        Console.WriteLine("💡 KEY TAKEAWAYS:");
        Console.WriteLine("   • Use FaithfulnessMetric to detect hallucinations");
        Console.WriteLine("   • Use RelevanceMetric to ensure on-topic responses");
        Console.WriteLine("   • Use ContextPrecision/Recall to evaluate retrieval");
        Console.WriteLine("   • Use AnswerCorrectness for end-to-end accuracy");
        Console.WriteLine("   • Combine metrics for comprehensive RAG evaluation");
        Console.WriteLine("   • See Sample 17 for Quality & Safety metrics");
        
        Console.WriteLine("\n🎉 Sample complete!\n");
    }

    private static IChatClient CreateEvaluatorClient()
    {
        if (AIConfig.IsConfigured)
        {
            var azureClient = new AzureOpenAIClient(AIConfig.Endpoint, AIConfig.KeyCredential);
            return azureClient.GetChatClient(AIConfig.ModelDeployment).AsIChatClient();
        }
        
        Console.WriteLine("   ℹ️  Using FakeChatClient (no Azure credentials configured)\n");
        
        // Provide fake responses for each test case in order
        var fake = new FakeChatClient()
            // Test 1: Faithful response
            .WithResponse("""
            {
                "score": 95,
                "faithfulClaims": ["The capital of France is Paris", "It is the largest city in France"],
                "hallucinatedClaims": [],
                "reasoning": "All claims are directly supported by the provided context."
            }
            """)
            // Test 2: Hallucinated response
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
            // Test 3: Relevant response
            .WithResponse("""
            {
                "score": 92,
                "addressesQuestion": true,
                "staysOnTopic": true,
                "irrelevantParts": [],
                "reasoning": "Response directly lists exercise benefits as requested."
            }
            """)
            // Test 4: Irrelevant response
            .WithResponse("""
            {
                "score": 15,
                "addressesQuestion": false,
                "staysOnTopic": false,
                "irrelevantParts": ["History of Olympics", "Olympic rings meaning", "Athens 1896"],
                "reasoning": "Response discusses Olympic history instead of exercise benefits."
            }
            """)
            // Test 5: High precision context
            .WithResponse("""
            {
                "score": 98,
                "relevantParts": ["Water boils at 100°C", "Boiling point varies with altitude"],
                "irrelevantParts": [],
                "reasoning": "All retrieved context is directly relevant to the boiling point question."
            }
            """)
            // Test 6: Low precision context
            .WithResponse("""
            {
                "score": 35,
                "relevantParts": ["water boils at 100°C"],
                "irrelevantParts": ["Water covers 71% of Earth", "Fish live in water", "8 glasses daily", "Ice floats", "Ocean is salty"],
                "reasoning": "Only one sentence is relevant. Most content is general water facts unrelated to boiling point."
            }
            """)
            // Test 7: High recall context
            .WithResponse("""
            {
                "score": 95,
                "informationPresent": ["tomato sauce", "mozzarella cheese", "basil", "olive oil"],
                "informationMissing": [],
                "reasoning": "Context contains all ingredients mentioned in the ground truth."
            }
            """)
            // Test 8: Low recall context
            .WithResponse("""
            {
                "score": 20,
                "informationPresent": [],
                "informationMissing": ["tomato sauce", "mozzarella cheese", "basil", "olive oil"],
                "reasoning": "Context only mentions history and flag colors, no ingredient information."
            }
            """)
            // Test 9: Correct answer
            .WithResponse("""
            {
                "score": 100,
                "factsCorrect": ["8 planets", "Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune"],
                "factsIncorrect": [],
                "factsMissing": [],
                "reasoning": "Answer correctly states 8 planets and lists them all accurately."
            }
            """)
            // Test 10: Incorrect answer
            .WithResponse("""
            {
                "score": 30,
                "factsCorrect": [],
                "factsIncorrect": ["9 planets (should be 8)", "Pluto is a planet (reclassified in 2006)"],
                "factsMissing": ["Pluto was reclassified as dwarf planet"],
                "reasoning": "Answer incorrectly counts Pluto as a planet, outdated since 2006."
            }
            """);
        
        return fake;
    }

    private static void PrintMetricResult(MetricResult result, string testName, bool expectFail = false)
    {
        var passed = expectFail ? !result.Passed : result.Passed;
        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        var icon = result.Passed ? "✅" : "❌";
        Console.WriteLine($"\n      {icon} {testName}:");
        Console.ResetColor();
        
        Console.WriteLine($"         Score: {result.Score}/100");
        Console.WriteLine($"         Status: {(result.Passed ? "PASSED" : "FAILED")}");
        Console.WriteLine($"         Reason: {Truncate(result.Explanation ?? "No explanation", 60)}");
        
        if (result.Details != null && result.Details.TryGetValue("hallucinatedClaims", out var claims) && claims is List<string> claimList && claimList.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"         ⚠️ Hallucinations: {string.Join(", ", claimList.Take(2))}...");
            Console.ResetColor();
        }
        
        if (result.Details != null && result.Details.TryGetValue("missingInformation", out var missing) && missing is List<string> missingList && missingList.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"         ⚠️ Missing info: {string.Join(", ", missingList.Take(3))}");
            Console.ResetColor();
        }
    }

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║   📊 SAMPLE 05: COMPLETE RAG EVALUATION                                      ║
║   All 5 RAG metrics: Faithfulness, Relevance, Precision, Recall, Correctness ║
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
