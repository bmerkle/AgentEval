# Benchmarks Guide

> **Running standard AI benchmarks and creating custom benchmark suites with AgentEval**

---

## Overview

AgentEval supports running industry-standard AI agent benchmarks to:

- Compare your agent against published models
- Track performance regressions across versions
- Identify specific capability gaps
- Generate credible performance reports

---

## Supported Benchmarks

| Benchmark | Focus | Status | What It Tests |
|-----------|-------|--------|---------------|
| **BFCL** | Function Calling | ✅ Ready | Tool selection, arguments, multi-turn |
| **GAIA** | General AI Assistants | ✅ Ready | Multi-step reasoning, tool use |
| **ToolBench** | API Tool Use | ✅ Ready | Complex API workflows |
| **MINT** | Multi-turn Interaction | ⚠️ Partial | Conversation handling |
| **HumanEval** | Code Generation | ❌ Out of Scope | Requires code execution |
| **WebArena** | Web Browsing | ❌ Out of Scope | Requires browser simulation |

---

## Berkeley Function Calling Leaderboard (BFCL)

BFCL is the industry standard for evaluating function/tool calling accuracy.

### Categories

| Category | Description | AgentEval Support |
|----------|-------------|-------------------|
| `simple_python` | Single function calls | `ToolAccuracyTestCase` |
| `parallel` | Multiple independent calls | `ToolAccuracyTestCase` with multi-tool |
| `multiple` | Sequential dependent calls | `MultiStepTestCase` |
| `multi_turn_base` | Multi-turn conversations | `ConversationalTestCase` (planned) |
| `live_*` | Real-world API scenarios | `ToolAccuracyTestCase` |

### Dataset Structure

BFCL test cases follow this JSON structure:

```json
{
  "id": "simple_python_001",
  "question": "Calculate the factorial of 5",
  "function": {
    "name": "calculate_factorial",
    "description": "Calculate factorial of a number",
    "parameters": {
      "type": "object",
      "properties": {
        "n": {"type": "integer", "description": "The number"}
      },
      "required": ["n"]
    }
  },
  "ground_truth": {
    "name": "calculate_factorial",
    "arguments": {"n": 5}
  }
}
```

### Running BFCL with AgentEval

```csharp
using AgentEval.Benchmarks;
using AgentEval.Core;

// 1. Load BFCL dataset (when BfclDatasetLoader is available)
var loader = new BfclDatasetLoader();
var bfclCases = await loader.LoadBenchmarkAsync("bfcl/simple_python.json");

// 2. Convert to AgentEval test cases
var testCases = bfclCases.Select(bc => new ToolAccuracyTestCase
{
    Name = bc.Id,
    Prompt = bc.Question,
    ExpectedTools = new[]
    {
        new ExpectedTool
        {
            Name = bc.GroundTruth.Name,
            RequiredParameters = bc.GroundTruth.Arguments.Keys.ToList()
        }
    },
    AllowExtraTools = false  // Strict mode for benchmarking
}).ToList();

// 3. Run benchmark
var benchmark = new AgenticBenchmark(agent);
var results = await benchmark.RunToolAccuracyBenchmarkAsync(testCases);

// 4. Report results
Console.WriteLine($"BFCL Simple Python Results");
Console.WriteLine($"  Accuracy: {results.OverallAccuracy:P1}");
Console.WriteLine($"  Passed: {results.PassedTests}/{results.TotalTests}");
```

### Manual BFCL Test Cases

If you don't have the full dataset, create representative test cases:

```csharp
var bfclTests = new List<ToolAccuracyTestCase>
{
    // Simple function call
    new ToolAccuracyTestCase
    {
        Name = "simple_math",
        Prompt = "Calculate 15 factorial",
        ExpectedTools = new[]
        {
            new ExpectedTool { Name = "factorial", RequiredParameters = new[] { "n" } }
        }
    },
    
    // Parallel calls (multiple independent tools)
    new ToolAccuracyTestCase
    {
        Name = "parallel_weather",
        Prompt = "What's the weather in NYC and LA?",
        ExpectedTools = new[]
        {
            new ExpectedTool { Name = "get_weather", RequiredParameters = new[] { "location" } },
            new ExpectedTool { Name = "get_weather", RequiredParameters = new[] { "location" } }
        }
    },
    
    // Multiple steps (sequential dependent calls)
    new ToolAccuracyTestCase
    {
        Name = "sequential_booking",
        Prompt = "Find flights to Paris and book the cheapest one",
        ExpectedTools = new[]
        {
            new ExpectedTool { Name = "search_flights", RequiredParameters = new[] { "destination" } },
            new ExpectedTool { Name = "book_flight", RequiredParameters = new[] { "flight_id" } }
        }
    }
};
```

### Comparing Against Leaderboard

BFCL publishes scores for major models. Compare your agent:

| Model | Simple | Parallel | Multiple | Multi-turn | Overall |
|-------|--------|----------|----------|------------|---------|
| GPT-4o | 94.5% | 91.2% | 88.5% | 82.1% | 89.1% |
| Claude-3.5 | 92.1% | 88.7% | 85.2% | 79.8% | 86.5% |
| Gemini-1.5 | 91.8% | 87.5% | 84.1% | 78.2% | 85.4% |
| **Your Agent** | ?% | ?% | ?% | ?% | ?% |

---

## GAIA (General AI Assistants)

GAIA tests multi-step reasoning and real-world task completion.

### Difficulty Levels

| Level | Description | Example |
|-------|-------------|---------|
| Level 1 | Simple, few steps | "What's the capital of France?" |
| Level 2 | Moderate complexity | "Find the CEO's email from their website" |
| Level 3 | Complex multi-step | "Research and compare two products, then recommend" |

### Running GAIA Benchmarks

```csharp
// GAIA uses task completion benchmarks
var gaiaCases = new List<TaskCompletionTestCase>
{
    new TaskCompletionTestCase
    {
        Name = "gaia_level1_001",
        Prompt = "What is the population of Tokyo?",
        CompletionCriteria = new[]
        {
            "Provides a specific number",
            "Number is approximately correct (within 5%)",
            "Cites a source or explains where the data comes from"
        },
        PassingScore = 70
    },
    
    new TaskCompletionTestCase
    {
        Name = "gaia_level2_015",
        Prompt = "Find the contact email for customer support at Microsoft",
        CompletionCriteria = new[]
        {
            "Provides an email address",
            "Email appears to be a valid Microsoft domain",
            "Explains how to reach support"
        },
        PassingScore = 70
    }
};

var benchmark = new AgenticBenchmark(agent, evaluator);
var results = await benchmark.RunTaskCompletionBenchmarkAsync(gaiaCases);

Console.WriteLine($"GAIA Results");
Console.WriteLine($"  Average Score: {results.AverageScore:F1}/100");
Console.WriteLine($"  Pass Rate: {(double)results.PassedTests / results.TotalTests:P1}");
```

---

## ToolBench

ToolBench tests complex API tool usage across 16,000+ scenarios.

### Categories

- **Single-tool**: One API call
- **Intra-category**: Multiple tools from same category
- **Intra-collection**: Tools from same provider
- **Inter-collection**: Tools across providers

### Running ToolBench Scenarios

```csharp
// Multi-step reasoning benchmark for ToolBench
var toolbenchCases = new List<MultiStepTestCase>
{
    new MultiStepTestCase
    {
        Name = "toolbench_weather_trip",
        Prompt = "I'm planning a trip. Check the weather in Paris for next week, then find hotels if it's sunny.",
        ExpectedSteps = new List<ExpectedStep>
        {
            new ExpectedStep { ToolName = "weather_api" },
            new ExpectedStep { ToolName = "hotel_search", DependsOnStep = 0 }
        },
        RequireSequentialExecution = true
    },
    
    new MultiStepTestCase
    {
        Name = "toolbench_research_task",
        Prompt = "Research the top 3 competitors of Tesla and summarize their market caps",
        ExpectedSteps = new List<ExpectedStep>
        {
            new ExpectedStep { ToolName = "company_search" },
            new ExpectedStep { ToolName = "financial_data" },
            new ExpectedStep { ToolName = "summarize" }
        }
    }
};

var benchmark = new AgenticBenchmark(agent);
var results = await benchmark.RunMultiStepReasoningBenchmarkAsync(toolbenchCases);

Console.WriteLine($"ToolBench Multi-Step Results");
Console.WriteLine($"  Step Completion: {results.AverageStepCompletion:P1}");
Console.WriteLine($"  Full Success: {results.PassedTests}/{results.TotalTests}");
```

---

## Performance Benchmarks

Beyond accuracy, measure speed and cost:

```csharp
using AgentEval.Benchmarks;

var benchmark = new PerformanceBenchmark(agent);

// Run latency benchmark
var latencyResults = await benchmark.RunLatencyBenchmarkAsync(
    testCases,
    iterations: 10,  // Run each test 10 times
    warmupIterations: 2);

Console.WriteLine($"Latency Benchmark");
Console.WriteLine($"  P50: {latencyResults.P50Latency.TotalMilliseconds:F0}ms");
Console.WriteLine($"  P90: {latencyResults.P90Latency.TotalMilliseconds:F0}ms");
Console.WriteLine($"  P99: {latencyResults.P99Latency.TotalMilliseconds:F0}ms");
Console.WriteLine($"  Avg: {latencyResults.AverageLatency.TotalMilliseconds:F0}ms");

// Run throughput benchmark
var throughputResults = await benchmark.RunThroughputBenchmarkAsync(
    testCases,
    durationSeconds: 60,
    maxConcurrency: 10);

Console.WriteLine($"\nThroughput Benchmark");
Console.WriteLine($"  Requests/sec: {throughputResults.RequestsPerSecond:F1}");
Console.WriteLine($"  Success Rate: {throughputResults.SuccessRate:P1}");

// Run cost benchmark
var costResults = await benchmark.RunCostBenchmarkAsync(testCases);

Console.WriteLine($"\nCost Benchmark");
Console.WriteLine($"  Total Cost: ${costResults.TotalCost:F4}");
Console.WriteLine($"  Avg per Request: ${costResults.AverageCostPerRequest:F6}");
Console.WriteLine($"  Total Tokens: {costResults.TotalTokens}");
```

---

## Creating Custom Benchmark Suites

### Define a Benchmark Suite

```csharp
public class CustomerSupportBenchmark
{
    private readonly ITestableAgent _agent;
    private readonly IEvaluator _evaluator;
    
    public CustomerSupportBenchmark(ITestableAgent agent, IEvaluator evaluator)
    {
        _agent = agent;
        _evaluator = evaluator;
    }
    
    public async Task<BenchmarkReport> RunFullSuiteAsync(CancellationToken ct = default)
    {
        var report = new BenchmarkReport
        {
            BenchmarkName = "CustomerSupport-v1",
            AgentName = _agent.Name,
            ModelVersion = "gpt-4o-2024-11-20",
            RunDate = DateTimeOffset.UtcNow
        };
        
        var benchmark = new AgenticBenchmark(_agent, _evaluator);
        var perfBenchmark = new PerformanceBenchmark(_agent);
        
        // Tool accuracy
        var toolResults = await benchmark.RunToolAccuracyBenchmarkAsync(GetToolTestCases());
        report.CategoryScores["ToolAccuracy"] = toolResults.OverallAccuracy * 100;
        
        // Task completion
        var taskResults = await benchmark.RunTaskCompletionBenchmarkAsync(GetTaskTestCases());
        report.CategoryScores["TaskCompletion"] = taskResults.AverageScore;
        
        // Multi-step reasoning
        var stepResults = await benchmark.RunMultiStepReasoningBenchmarkAsync(GetMultiStepCases());
        report.CategoryScores["MultiStep"] = stepResults.AverageStepCompletion * 100;
        
        // Latency
        var latencyResults = await perfBenchmark.RunLatencyBenchmarkAsync(GetToolTestCases());
        report.CategoryScores["P90Latency"] = latencyResults.P90Latency.TotalMilliseconds;
        
        // Overall score (weighted average)
        report.OverallScore = 
            report.CategoryScores["ToolAccuracy"] * 0.3 +
            report.CategoryScores["TaskCompletion"] * 0.4 +
            report.CategoryScores["MultiStep"] * 0.3;
        
        return report;
    }
    
    private List<ToolAccuracyTestCase> GetToolTestCases() => new()
    {
        new ToolAccuracyTestCase
        {
            Name = "password_reset",
            Prompt = "I forgot my password, help me reset it",
            ExpectedTools = new[] { new ExpectedTool { Name = "reset_password" } }
        },
        new ToolAccuracyTestCase
        {
            Name = "order_lookup",
            Prompt = "Where is my order #12345?",
            ExpectedTools = new[] { new ExpectedTool { Name = "track_order" } }
        }
        // ... more cases
    };
    
    private List<TaskCompletionTestCase> GetTaskTestCases() => new()
    {
        new TaskCompletionTestCase
        {
            Name = "refund_request",
            Prompt = "I want a refund for my last order",
            CompletionCriteria = new[]
            {
                "Acknowledges the refund request",
                "Asks for or retrieves order details",
                "Explains refund policy or timeline"
            }
        }
        // ... more cases
    };
    
    private List<MultiStepTestCase> GetMultiStepCases() => new()
    {
        new MultiStepTestCase
        {
            Name = "complex_order_issue",
            Prompt = "My order arrived damaged. I want a replacement or refund.",
            ExpectedSteps = new List<ExpectedStep>
            {
                new ExpectedStep { ToolName = "lookup_order" },
                new ExpectedStep { ToolName = "check_return_eligibility" },
                new ExpectedStep { ToolName = "create_return_request" }
            }
        }
        // ... more cases
    };
}
```

### Run and Report

```csharp
var benchmark = new CustomerSupportBenchmark(agent, evaluator);
var report = await benchmark.RunFullSuiteAsync();

Console.WriteLine($"\n{'=',-60}");
Console.WriteLine($"BENCHMARK REPORT: {report.BenchmarkName}");
Console.WriteLine($"{'=',-60}");
Console.WriteLine($"Agent: {report.AgentName}");
Console.WriteLine($"Model: {report.ModelVersion}");
Console.WriteLine($"Date: {report.RunDate:yyyy-MM-dd HH:mm}");
Console.WriteLine();
Console.WriteLine("Category Scores:");
foreach (var (category, score) in report.CategoryScores)
{
    var bar = new string('█', (int)(score / 5));
    Console.WriteLine($"  {category,-20} {score,6:F1}  {bar}");
}
Console.WriteLine();
Console.WriteLine($"OVERALL SCORE: {report.OverallScore:F1}/100");
```

Output:
```
============================================================
BENCHMARK REPORT: CustomerSupport-v1
============================================================
Agent: CustomerSupportAgent
Model: gpt-4o-2024-11-20
Date: 2026-01-05 14:30

Category Scores:
  ToolAccuracy         92.5  ██████████████████
  TaskCompletion       88.2  █████████████████
  MultiStep            85.0  █████████████████
  P90Latency          1250  (ms)

OVERALL SCORE: 88.5/100
```

---

## Regression Detection

Track benchmark scores over time:

```csharp
// Load previous baseline
var baseline = await LoadBaselineAsync("baseline-v1.0.json");

// Run current benchmark
var current = await benchmark.RunFullSuiteAsync();

// Compare
var regressions = new List<(string Category, double Delta)>();

foreach (var (category, score) in current.CategoryScores)
{
    if (baseline.CategoryScores.TryGetValue(category, out var baselineScore))
    {
        var delta = score - baselineScore;
        if (delta < -5.0)  // More than 5% regression
        {
            regressions.Add((category, delta));
        }
    }
}

if (regressions.Any())
{
    Console.WriteLine("⚠️ REGRESSIONS DETECTED:");
    foreach (var (category, delta) in regressions)
    {
        Console.WriteLine($"  {category}: {delta:+0.0;-0.0}%");
    }
    Environment.ExitCode = 1;  // Fail CI/CD
}
else
{
    Console.WriteLine("✅ No regressions detected");
}
```

---

## CI/CD Integration

### GitHub Actions

```yaml
name: Benchmark

on:
  push:
    branches: [main]
  pull_request:

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Run Benchmarks
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          dotnet run --project benchmarks/AgentBenchmarks.csproj \
            --output results.json \
            --baseline baselines/main.json \
            --fail-on-regression
      
      - name: Upload Results
        uses: actions/upload-artifact@v4
        with:
          name: benchmark-results
          path: results.json
      
      - name: Comment on PR
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v7
        with:
          script: |
            const results = require('./results.json');
            const body = `## Benchmark Results
            
            | Category | Score | Change |
            |----------|-------|--------|
            ${Object.entries(results.categoryScores).map(([k,v]) => 
              `| ${k} | ${v.toFixed(1)} | ${results.deltas[k] || 'N/A'} |`
            ).join('\n')}
            
            **Overall: ${results.overallScore.toFixed(1)}/100**`;
            
            github.rest.issues.createComment({
              owner: context.repo.owner,
              repo: context.repo.repo,
              issue_number: context.issue.number,
              body
            });
```

---

## See Also

- [Architecture Overview](architecture.md) - Understanding AgentEval structure
- [Extensibility Guide](extensibility.md) - Creating custom metrics
- [Embedding Metrics](embedding-metrics.md) - Fast similarity evaluation
