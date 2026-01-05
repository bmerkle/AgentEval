// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

namespace AgentEval.Samples;

/// <summary>
/// Sample 08: Conversation Testing - Multi-turn agent interactions
/// 
/// This demonstrates:
/// - Using ConversationRunner for multi-turn testing
/// - Defining conversation test cases with checkpoints
/// - Asserting behavior across turns
/// - Testing memory/context retention
/// 
/// ⏱️ Time to understand: 5 minutes
/// </summary>
public static class Sample08_ConversationTesting
{
    public static async Task RunAsync()
    {
        PrintHeader();

        // ═══════════════════════════════════════════════════════════════
        // STEP 1: Define a multi-turn conversation
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 1: Defining multi-turn conversation...\n");
        
        var conversation = new ConversationDefinition
        {
            Name = "Travel Planning Conversation",
            Description = "Test a travel agent across multiple turns",
            Turns = new[]
            {
                new TurnDefinition
                {
                    UserMessage = "I want to plan a trip to Paris",
                    ExpectedOutputContains = "Paris",
                    CheckpointDescription = "Agent acknowledges destination"
                },
                new TurnDefinition
                {
                    UserMessage = "I'll be traveling in June for 5 days",
                    ExpectedToolCalls = new[] { "check_availability" },
                    CheckpointDescription = "Agent checks availability"
                },
                new TurnDefinition
                {
                    UserMessage = "What hotels do you recommend?",
                    ExpectedToolCalls = new[] { "search_hotels" },
                    ExpectedOutputContains = "hotel",
                    CheckpointDescription = "Agent searches and recommends hotels"
                },
                new TurnDefinition
                {
                    UserMessage = "Book the first option please",
                    ExpectedToolCalls = new[] { "book_hotel" },
                    MustConfirmBefore = true,
                    CheckpointDescription = "Agent confirms before booking"
                }
            },
            MaxTotalDuration = TimeSpan.FromSeconds(30),
            MaxTurns = 10
        };

        Console.WriteLine($"   Conversation: {conversation.Name}");
        Console.WriteLine($"   Turns defined: {conversation.Turns.Length}");
        Console.WriteLine($"   Max duration: {conversation.MaxTotalDuration.TotalSeconds}s\n");

        foreach (var (turn, index) in conversation.Turns.Select((t, i) => (t, i)))
        {
            Console.WriteLine($"   Turn {index + 1}: \"{turn.UserMessage}\"");
            Console.WriteLine($"            → {turn.CheckpointDescription}");
        }

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Run the conversation (simulated)
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 2: Running conversation...\n");
        
        var result = await SimulateConversation(conversation);

        // ═══════════════════════════════════════════════════════════════
        // STEP 3: Review results
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📊 CONVERSATION RESULTS:");
        Console.WriteLine(new string('─', 60));
        
        Console.Write("   Overall: ");
        if (result.AllPassed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ PASSED");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ FAILED");
        }
        Console.ResetColor();
        
        Console.WriteLine($"   Turns completed: {result.TurnsCompleted}/{conversation.Turns.Length}");
        Console.WriteLine($"   Total duration: {result.TotalDuration.TotalMilliseconds:F0}ms");
        Console.WriteLine($"   Total tool calls: {result.TotalToolCalls}");
        Console.WriteLine();

        Console.WriteLine("   📋 Turn-by-turn results:");
        foreach (var turnResult in result.TurnResults)
        {
            var status = turnResult.Passed ? "✅" : "❌";
            Console.WriteLine($"      {status} Turn {turnResult.TurnIndex + 1}: {turnResult.Description}");
            
            if (turnResult.ToolsCalled.Any())
            {
                Console.WriteLine($"         Tools: {string.Join(", ", turnResult.ToolsCalled)}");
            }
            
            if (!turnResult.Passed && !string.IsNullOrEmpty(turnResult.FailureReason))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"         Reason: {turnResult.FailureReason}");
                Console.ResetColor();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // STEP 4: Code patterns for conversation testing
        // ═══════════════════════════════════════════════════════════════
        Console.WriteLine("\n📝 Step 4: Code patterns for conversation testing...\n");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"   // Example test code with AgentEval.Testing:
   
   [Fact]
   public async Task Agent_ShouldHandleMultiTurnBooking()
   {
       // Arrange
       var conversation = new ConversationalTestCase
       {
           Name = ""Hotel Booking Flow"",
           Turns = new[]
           {
               new ConversationTurn
               {
                   UserMessage = ""Book a hotel in Paris"",
                   Checkpoint = new TurnCheckpoint
                   {
                       ExpectedOutputContains = ""Paris"",
                       ExpectedToolCalls = new[] { ""search_hotels"" }
                   }
               },
               new ConversationTurn
               {
                   UserMessage = ""Book the Marriott"",
                   Checkpoint = new TurnCheckpoint
                   {
                       ExpectedToolCalls = new[] { ""book_hotel"" },
                       MustConfirmBefore = true  // Agent should confirm!
                   }
               }
           }
       };
       
       // Act
       var runner = new ConversationRunner();
       var result = await runner.RunAsync(agent, conversation);
       
       // Assert
       Assert.True(result.Passed, result.FailureSummary);
       Assert.True(result.TurnsCompleted >= 2);
   }

   // Advanced assertions:
   result.Should().HaveCalledToolByTurn(""search_hotels"", turnIndex: 1);
   result.Should().HaveConfirmedBefore(""book_hotel"");
   result.TurnResults[1].Output.Should().Contain(""Paris""); // Context retained");
        Console.ResetColor();

        // ═══════════════════════════════════════════════════════════════
        // KEY TAKEAWAYS
        // ═══════════════════════════════════════════════════════════════
        PrintKeyTakeaways();
    }

    private static async Task<ConversationResult> SimulateConversation(ConversationDefinition conversation)
    {
        var turnResults = new List<TurnResult>();
        var totalToolCalls = 0;
        var startTime = DateTimeOffset.UtcNow;

        // Simulated agent responses
        var responses = new Dictionary<string, (string output, string[] tools)>
        {
            ["Paris"] = ("Great choice! Paris is beautiful. When would you like to visit?", Array.Empty<string>()),
            ["June"] = ("I'm checking availability for June. Looks like there are many options!", new[] { "check_availability" }),
            ["hotel"] = ("I found several great hotel options in Paris:\n1. Hotel Le Marais\n2. Grand Hotel Paris", new[] { "search_hotels" }),
            ["book"] = ("Before I book Hotel Le Marais, please confirm: 5 nights, €850 total. Proceed?", new[] { "book_hotel" })
        };

        foreach (var (turn, index) in conversation.Turns.Select((t, i) => (t, i)))
        {
            await Task.Delay(25); // Simulate processing

            // Find matching response
            var matchedResponse = responses.FirstOrDefault(r => 
                turn.UserMessage.Contains(r.Key, StringComparison.OrdinalIgnoreCase));
            
            var output = matchedResponse.Value.output ?? "I understand. How can I help?";
            var toolsCalled = matchedResponse.Value.tools ?? Array.Empty<string>();

            // Validate checkpoint
            var passed = true;
            string? failureReason = null;

            if (turn.ExpectedOutputContains != null &&
                !output.Contains(turn.ExpectedOutputContains, StringComparison.OrdinalIgnoreCase))
            {
                passed = false;
                failureReason = $"Output missing: {turn.ExpectedOutputContains}";
            }

            if (turn.ExpectedToolCalls != null)
            {
                var missingTools = turn.ExpectedToolCalls.Except(toolsCalled).ToList();
                if (missingTools.Any())
                {
                    passed = false;
                    failureReason = $"Missing tools: {string.Join(", ", missingTools)}";
                }
            }

            turnResults.Add(new TurnResult
            {
                TurnIndex = index,
                Passed = passed,
                Description = turn.CheckpointDescription ?? $"Turn {index + 1}",
                Output = output,
                ToolsCalled = toolsCalled.ToList(),
                FailureReason = failureReason
            });

            totalToolCalls += toolsCalled.Length;
        }

        return new ConversationResult
        {
            AllPassed = turnResults.All(r => r.Passed),
            TurnsCompleted = turnResults.Count,
            TotalDuration = DateTimeOffset.UtcNow - startTime,
            TotalToolCalls = totalToolCalls,
            TurnResults = turnResults
        };
    }

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║                  Sample 08: Conversation Testing                              ║
║                                                                               ║
║   Learn how to:                                                               ║
║   • Test multi-turn agent interactions                                        ║
║   • Define checkpoints for each turn                                          ║
║   • Assert tool usage across the conversation                                 ║
║   • Verify context retention and memory                                       ║
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
│  1. ConversationalTestCase defines multi-turn scenarios:                        │
│     var conversation = new ConversationalTestCase {                             │
│         Turns = new[] { turn1, turn2, turn3 }                                   │
│     };                                                                          │
│                                                                                 │
│  2. TurnCheckpoint sets expectations per turn:                                  │
│     new TurnCheckpoint {                                                        │
│         ExpectedOutputContains = ""Paris"",                                       │
│         ExpectedToolCalls = new[] { ""search_hotels"" },                          │
│         MustConfirmBefore = true  // For destructive actions                    │
│     }                                                                           │
│                                                                                 │
│  3. ConversationRunner executes and validates:                                  │
│     var runner = new ConversationRunner();                                      │
│     var result = await runner.RunAsync(agent, conversation);                    │
│                                                                                 │
│  4. Multi-turn testing catches issues single-turn misses:                       │
│     • Memory/context drift                                                      │
│     • Tool failure recovery                                                     │
│     • Clarification loops                                                       │
│     • State corruption                                                          │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
");
        Console.ResetColor();
    }

    // Local types for this sample (to avoid conflicts with library types)
    private record ConversationDefinition
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required TurnDefinition[] Turns { get; init; }
        public TimeSpan MaxTotalDuration { get; init; } = TimeSpan.FromMinutes(1);
        public int MaxTurns { get; init; } = 20;
    }

    private record TurnDefinition
    {
        public required string UserMessage { get; init; }
        public string? ExpectedOutputContains { get; init; }
        public string[]? ExpectedToolCalls { get; init; }
        public bool MustConfirmBefore { get; init; }
        public string? CheckpointDescription { get; init; }
    }

    private record TurnResult
    {
        public int TurnIndex { get; init; }
        public bool Passed { get; init; }
        public string Description { get; init; } = "";
        public string Output { get; init; } = "";
        public List<string> ToolsCalled { get; init; } = new();
        public string? FailureReason { get; init; }
    }

    private record ConversationResult
    {
        public bool AllPassed { get; init; }
        public int TurnsCompleted { get; init; }
        public TimeSpan TotalDuration { get; init; }
        public int TotalToolCalls { get; init; }
        public List<TurnResult> TurnResults { get; init; } = new();
    }
}
