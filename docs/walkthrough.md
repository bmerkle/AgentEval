# Walkthrough: Testing Your First AI Agent

This walkthrough guides you through testing an AI agent with AgentEval, from setup to assertions.

---

## What You'll Learn

1. Setting up a test harness
2. Wrapping an agent for testing
3. Running a test and capturing results
4. Asserting on tool usage
5. Asserting on performance
6. Exporting results for CI/CD

---

## Prerequisites

- .NET 8.0+ SDK
- An AI agent (we'll use a mock for this tutorial)
- AgentEval installed (`dotnet add package AgentEval --prerelease`)

---

## Step 1: Create a Test Harness

The test harness runs your agent and captures all the data needed for assertions.

```csharp
using AgentEval.MAF;

// Create a test harness with optional verbose logging
var harness = new MAFTestHarness(verbose: true);
```

---

## Step 2: Wrap Your Agent

AgentEval uses adapters to wrap different agent types. For Microsoft Agent Framework agents:

```csharp
using AgentEval.MAF;

// Wrap your MAF agent
var adapter = new MAFAgentAdapter(myAgent);
```

For any `IChatClient`:

```csharp
using AgentEval.Adapters;

// Wrap an IChatClient
var adapter = new ChatClientAgentAdapter(chatClient, "MyAgent");
```

---

## Step 3: Define a Test Case

Test cases describe what to test and how to evaluate the results:

```csharp
using AgentEval.Models;

var testCase = new TestCase
{
    Name = "Travel Planning Test",
    Input = "Plan a trip to Paris for next weekend",
    
    // Optional: Expected tools the agent should use
    ExpectedTools = new[] { "SearchFlights", "SearchHotels", "GetWeather" },
    
    // Optional: Criteria for AI-powered evaluation
    EvaluationCriteria = new[] 
    { 
        "Should include flight options",
        "Should include hotel recommendations",
        "Should consider weather"
    },
    
    // Minimum score to pass (0-100)
    PassingScore = 70
};
```

---

## Step 4: Run the Test

Execute the test and capture results:

```csharp
using AgentEval.Core;

// Run the test - tool tracking and performance metrics are captured automatically
var result = await harness.RunTestAsync(adapter, testCase);

// Check if the test passed
Console.WriteLine($"Passed: {result.Passed}");
Console.WriteLine($"Score: {result.Score}");
Console.WriteLine($"Output: {result.ActualOutput}");
```

---

## Step 5: Assert on Tool Usage

Use fluent assertions to verify the agent used tools correctly:

```csharp
using AgentEval.Assertions;

// Assert specific tools were called
result.ToolUsage!
    .Should()
    .HaveCalledTool("SearchFlights")
    .HaveCalledTool("SearchHotels")
    .HaveCalledTool("GetWeather");

// Assert tool ordering
result.ToolUsage!
    .Should()
    .HaveCalledTool("GetWeather")
        .BeforeTool("SearchFlights");  // Weather check before booking

// Assert tool arguments
result.ToolUsage!
    .Should()
    .HaveCalledTool("SearchFlights")
        .WithArgument("destination", "Paris");

// Assert no errors occurred
result.ToolUsage!
    .Should()
    .HaveNoErrors();

// Assert call count limits
result.ToolUsage!
    .Should()
    .HaveTotalCallsLessThan(10);  // Efficiency check
```

---

## Step 6: Assert on Performance

Verify the agent meets performance requirements:

```csharp
using AgentEval.Assertions;

result.Performance!
    .Should()
    .HaveTotalDurationUnder(TimeSpan.FromSeconds(30))
    .HaveTimeToFirstTokenUnder(TimeSpan.FromSeconds(2))
    .HaveTokenCountUnder(4000)
    .HaveEstimatedCostUnder(0.10m);  // Max $0.10 per request
```

---

## Step 7: Assert on Response Content

Verify the response contains expected information:

```csharp
using AgentEval.Assertions;

result.Response
    .Should()
    .Contain("Paris")
    .ContainAny("flight", "airline")
    .ContainAny("hotel", "accommodation")
    .NotContain("error")
    .HaveLengthBetween(100, 5000);
```

---

## Step 8: Export Results for CI/CD

Export results in formats your CI/CD system understands:

```csharp
using AgentEval.Exporters;

// JUnit XML for GitHub Actions, Azure DevOps, Jenkins
var junitExporter = new JUnitExporter();
await junitExporter.ExportAsync(new[] { result }, "results.xml");

// Markdown for PR comments
var markdownExporter = new MarkdownExporter();
await markdownExporter.ExportAsync(new[] { result }, "results.md");

// JSON for custom dashboards
var jsonExporter = new JsonExporter();
await jsonExporter.ExportAsync(new[] { result }, "results.json");
```

---

## Complete Example

Here's the full test in one file:

```csharp
using AgentEval.MAF;
using AgentEval.Models;
using AgentEval.Core;
using AgentEval.Assertions;
using AgentEval.Exporters;

// 1. Setup
var harness = new MAFTestHarness(verbose: true);
var adapter = new MAFAgentAdapter(myAgent);

// 2. Define test
var testCase = new TestCase
{
    Name = "Travel Planning Test",
    Input = "Plan a trip to Paris for next weekend",
    ExpectedTools = new[] { "SearchFlights", "SearchHotels", "GetWeather" },
    PassingScore = 70
};

// 3. Run test
var result = await harness.RunTestAsync(adapter, testCase);

// 4. Assert
result.ToolUsage!
    .Should()
    .HaveCalledTool("SearchFlights")
    .HaveCalledTool("SearchHotels")
    .HaveNoErrors();

result.Performance!
    .Should()
    .HaveTotalDurationUnder(TimeSpan.FromSeconds(30))
    .HaveEstimatedCostUnder(0.10m);

// 5. Export
var exporter = new JUnitExporter();
await exporter.ExportAsync(new[] { result }, "results.xml");

Console.WriteLine($"✅ Test {(result.Passed ? "PASSED" : "FAILED")}");
Console.WriteLine($"   Output: {result.ActualOutput}");
```

---

## Using with xUnit/NUnit/MSTest

AgentEval integrates naturally with test frameworks:

```csharp
[Fact]
public async Task Agent_ShouldPlanTrip_WithCorrectTools()
{
    // Arrange
    var harness = new MAFTestHarness();
    var adapter = new MAFAgentAdapter(_agent);
    var testCase = new TestCase
    {
        Name = "Travel Planning",
        Input = "Plan a trip to Paris",
        ExpectedTools = new[] { "SearchFlights" }
    };

    // Act
    var result = await harness.RunTestAsync(adapter, testCase);

    // Assert
    result.ToolUsage!
        .Should()
        .HaveCalledTool("SearchFlights")
        .HaveNoErrors();
    
    Assert.True(result.Passed);
}
```

---

## Next Steps

- [Architecture](architecture.md) - Understand the framework design
- [CLI Reference](cli.md) - Run tests from command line
- [Benchmarks](benchmarks.md) - Performance testing at scale
- [Conversations](conversations.md) - Multi-turn testing
- [Snapshots](snapshots.md) - Regression testing
