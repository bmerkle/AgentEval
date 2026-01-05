# AgentEval Architecture

> **Understanding the component structure and design patterns of AgentEval**

---

## Overview

AgentEval is designed with a layered architecture that separates concerns and enables extensibility. The framework follows SOLID principles, with interface segregation being particularly important for the metric hierarchy.

---

## Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              AgentEval                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                           Core Layer                                    │ │
│  ├────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                         │ │
│  │  Interfaces:                                                            │ │
│  │  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌──────────────┐  │ │
│  │  │   IMetric   │  │ITestableAgent│  │ ITestHarness│  │  IEvaluator  │  │ │
│  │  └─────────────┘  └──────────────┘  └─────────────┘  └──────────────┘  │ │
│  │                                                                         │ │
│  │  Utilities:                                                             │ │
│  │  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌──────────────┐  │ │
│  │  │MetricRegistry│ │ScoreNormalizer│ │LlmJsonParser│  │ RetryPolicy  │  │ │
│  │  └─────────────┘  └──────────────┘  └─────────────┘  └──────────────┘  │ │
│  │                                                                         │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                          Metrics Layer                                  │ │
│  ├────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                         │ │
│  │  RAG Metrics:              Agentic Metrics:         Embedding Metrics:  │ │
│  │  ┌─────────────────┐       ┌─────────────────┐      ┌────────────────┐  │ │
│  │  │  Faithfulness   │       │  ToolSelection  │      │AnswerSimilarity│  │ │
│  │  │  Relevance      │       │  ToolArguments  │      │ContextSimilarity│ │ │
│  │  │  ContextPrecision│      │  ToolSuccess    │      │ QuerySimilarity│  │ │
│  │  │  ContextRecall  │       │  TaskCompletion │      └────────────────┘  │ │
│  │  │  AnswerCorrectness│     │  ToolEfficiency │                          │ │
│  │  └─────────────────┘       └─────────────────┘                          │ │
│  │                                                                         │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                        Assertions Layer                                 │ │
│  ├────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                         │ │
│  │  ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────┐  │ │
│  │  │ToolUsageAssertions  │  │PerformanceAssertions│  │ResponseAssertions│ │ │
│  │  │  .HaveCalledTool()  │  │  .HaveDurationUnder()│ │  .Contain()     │  │ │
│  │  │  .BeforeTool()      │  │  .HaveTTFTUnder()   │  │  .MatchPattern()│  │ │
│  │  │  .WithArguments()   │  │  .HaveCostUnder()   │  │  .HaveLength()  │  │ │
│  │  └─────────────────────┘  └─────────────────────┘  └─────────────────┘  │ │
│  │                                                                         │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                        Benchmarks Layer                                 │ │
│  ├────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                         │ │
│  │  ┌─────────────────────────┐  ┌─────────────────────────────────────┐   │ │
│  │  │   PerformanceBenchmark  │  │        AgenticBenchmark             │   │ │
│  │  │   • Latency             │  │   • ToolAccuracy                    │   │ │
│  │  │   • Throughput          │  │   • TaskCompletion                  │   │ │
│  │  │   • Cost                │  │   • MultiStepReasoning              │   │ │
│  │  └─────────────────────────┘  └─────────────────────────────────────┘   │ │
│  │                                                                         │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                       Integration Layer                                 │ │
│  ├────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                         │ │
│  │  ┌─────────────────┐  ┌────────────────────────┐  ┌─────────────────┐   │ │
│  │  │  MAFTestHarness │  │MicrosoftEvaluatorAdapter│ │ChatClientAdapter│   │ │
│  │  │  (MAF support)  │  │(MS.Extensions.AI.Eval) │  │ (Generic)       │   │ │
│  │  └─────────────────┘  └────────────────────────┘  └─────────────────┘   │ │
│  │                                                                         │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                   Production Infrastructure (Planned)                   │ │
│  ├────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                         │ │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │ │
│  │  │IResultExporter│ │IDatasetLoader│ │SnapshotTest │  │AgentEval.CLI│   │ │
│  │  │ JUnit/MD/JSON│ │JSONL/BFCL   │  │  Harness    │  │dotnet tool  │    │ │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │ │
│  │                                                                         │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Metric Hierarchy

AgentEval uses interface segregation to organize metrics by their requirements:

```
IMetric (base interface)
│
├── Properties:
│   ├── Name: string
│   └── Description: string
│
├── Methods:
│   └── EvaluateAsync(EvaluationContext, CancellationToken) -> MetricResult
│
├── IRAGMetric : IMetric
│   ├── RequiresContext: bool
│   ├── RequiresGroundTruth: bool
│   │
│   └── Implementations:
│       ├── FaithfulnessMetric      - Is response supported by context?
│       ├── RelevanceMetric         - Is response relevant to query?
│       ├── ContextPrecisionMetric  - Was context useful for the answer?
│       ├── ContextRecallMetric     - Does context cover ground truth?
│       └── AnswerCorrectnessMetric - Is response factually correct?
│
├── IAgenticMetric : IMetric
│   ├── RequiresToolUsage: bool
│   │
│   └── Implementations:
│       ├── ToolSelectionMetric   - Were correct tools called?
│       ├── ToolArgumentsMetric   - Were tool arguments correct?
│       ├── ToolSuccessMetric     - Did tool calls succeed?
│       ├── ToolEfficiencyMetric  - Were tools used efficiently?
│       └── TaskCompletionMetric  - Was the task completed?
│
└── IEmbeddingMetric : IMetric (implicit)
    ├── RequiresEmbeddings: bool
    │
    └── Implementations:
        ├── AnswerSimilarityMetric         - Response vs ground truth similarity
        ├── ResponseContextSimilarityMetric - Response vs context similarity
        └── QueryContextSimilarityMetric    - Query vs context similarity
```

---

## Data Flow

```
┌─────────────┐    ┌──────────────┐    ┌─────────────┐    ┌──────────────┐
│  Test Case  │───▶│ ITestHarness │───▶│ Agent Under │───▶│   Response   │
│   (Input)   │    │              │    │    Test     │    │   (Output)   │
└─────────────┘    └──────────────┘    └─────────────┘    └──────────────┘
                          │                                       │
                          │                                       │
                          ▼                                       ▼
                   ┌──────────────┐                       ┌──────────────┐
                   │Tool Tracking │                       │  Evaluation  │
                   │ (timeline,   │                       │   Context    │
                   │  arguments)  │                       │              │
                   └──────────────┘                       └──────────────┘
                          │                                       │
                          └───────────────────┬───────────────────┘
                                              │
                                              ▼
                                    ┌──────────────────┐
                                    │  Metric Runner   │
                                    │  (evaluates all  │
                                    │   configured     │
                                    │   metrics)       │
                                    └──────────────────┘
                                              │
                                              ▼
                                    ┌──────────────────┐
                                    │   Test Result    │
                                    │  • Score         │
                                    │  • Passed/Failed │
                                    │  • ToolUsage     │
                                    │  • Performance   │
                                    │  • FailureReport │
                                    └──────────────────┘
                                              │
                                              ▼
                                    ┌──────────────────┐
                                    │  Result Exporter │
                                    │  • JUnit XML     │
                                    │  • Markdown      │
                                    │  • JSON          │
                                    └──────────────────┘
```

---

## Key Models

### EvaluationContext

The central data structure passed to all metrics:

```csharp
public class EvaluationContext
{
    // Identification
    public string EvaluationId { get; init; }
    public DateTimeOffset StartedAt { get; init; }

    // Core data
    public required string Input { get; init; }      // User query
    public required string Output { get; init; }     // Agent response
    
    // RAG-specific
    public string? Context { get; init; }            // Retrieved context
    public string? GroundTruth { get; init; }        // Expected answer
    
    // Agentic-specific
    public ToolUsageReport? ToolUsage { get; init; } // Tool calls made
    public IReadOnlyList<string>? ExpectedTools { get; init; }
    
    // Performance
    public PerformanceMetrics? Performance { get; init; }
    public ToolCallTimeline? Timeline { get; init; } // Execution trace
    
    // Extensibility
    public IDictionary<string, object?> Properties { get; }
}
```

### MetricResult

The result of evaluating a single metric:

```csharp
public class MetricResult
{
    public required string MetricName { get; init; }
    public required double Score { get; init; }       // 0-100 scale
    public bool Passed { get; init; }
    public string? Explanation { get; init; }
    public IDictionary<string, object>? Details { get; init; }
    
    // Factory methods
    public static MetricResult Pass(string name, double score, string? explanation = null);
    public static MetricResult Fail(string name, string explanation, double score = 0);
}
```

### ToolUsageReport

Tracks all tool calls made during an agent run:

```csharp
public class ToolUsageReport
{
    public IReadOnlyList<ToolCallRecord> Calls { get; }
    public int Count { get; }
    public int SuccessCount { get; }
    public int FailureCount { get; }
    public TimeSpan TotalDuration { get; }
    
    // Fluent assertions
    public ToolUsageAssertions Should();
}
```

### PerformanceMetrics

Captures timing and cost information:

```csharp
public class PerformanceMetrics
{
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan? TimeToFirstToken { get; set; }
    public TokenUsage? Tokens { get; set; }
    public decimal? EstimatedCost { get; set; }
    
    // Fluent assertions
    public PerformanceAssertions Should();
}
```

---

## Design Patterns

### 1. Interface Segregation (ISP)

Metrics only require what they need:

```csharp
// RAG metrics need context
public interface IRAGMetric : IMetric
{
    bool RequiresContext { get; }
    bool RequiresGroundTruth { get; }
}

// Agentic metrics need tool usage
public interface IAgenticMetric : IMetric
{
    bool RequiresToolUsage { get; }
}
```

### 2. Adapter Pattern

Enables integration with different frameworks:

```csharp
// Adapt any IChatClient to ITestableAgent
public class ChatClientAgentAdapter : ITestableAgent
{
    private readonly IChatClient _chatClient;
    
    public async Task<AgentResponse> InvokeAsync(string input, CancellationToken ct)
    {
        var response = await _chatClient.GetResponseAsync(
            new[] { new ChatMessage(ChatRole.User, input) }, ct);
        return new AgentResponse { Text = response.Message.Text };
    }
}

// Wrap Microsoft's evaluators for AgentEval
public class MicrosoftEvaluatorAdapter : IMetric
{
    private readonly IEvaluator _msEvaluator;
    
    public async Task<MetricResult> EvaluateAsync(EvaluationContext context, CancellationToken ct)
    {
        var msResult = await _msEvaluator.EvaluateAsync(...);
        return new MetricResult
        {
            Score = ScoreNormalizer.From1To5(msResult.Score),
            ...
        };
    }
}
```

### 3. Fluent API

Intuitive assertion chaining:

```csharp
result.ToolUsage!
    .Should()
    .HaveCalledTool("SearchTool")
        .BeforeTool("AnalyzeTool")
        .WithArguments(args => args.ContainsKey("query"))
    .And()
    .HaveNoErrors()
    .And()
    .HaveToolCountBetween(1, 5);

result.Performance!
    .Should()
    .HaveTotalDurationUnder(TimeSpan.FromSeconds(5))
    .HaveTimeToFirstTokenUnder(TimeSpan.FromSeconds(1))
    .HaveEstimatedCostUnder(0.10m);
```

### 4. Registry Pattern

Centralized metric management:

```csharp
var registry = new MetricRegistry();
registry.Register(new FaithfulnessMetric(chatClient));
registry.Register(new ToolSelectionMetric(expectedTools));

// Run all registered metrics
foreach (var metric in registry.GetAll())
{
    var result = await metric.EvaluateAsync(context);
}
```

---

## Package Structure

```
AgentEval/
├── Core/                    # Core interfaces and utilities
│   ├── IMetric.cs
│   ├── ITestableAgent.cs
│   ├── ITestHarness.cs
│   ├── IEvaluator.cs
│   ├── IAgentEvalLogger.cs
│   ├── MetricRegistry.cs
│   ├── ScoreNormalizer.cs
│   ├── RetryPolicy.cs
│   └── EvaluationDefaults.cs
│
├── Models/                  # Data models
│   ├── TestModels.cs        # TestCase, TestResult, TestSummary
│   ├── ToolCallRecord.cs
│   ├── ToolUsageReport.cs
│   ├── ToolCallTimeline.cs
│   ├── PerformanceMetrics.cs
│   └── FailureReport.cs
│
├── Metrics/                 # Metric implementations
│   ├── RAG/
│   │   ├── RAGMetrics.cs    # Faithfulness, Relevance, etc.
│   │   └── EmbeddingMetrics.cs
│   └── Agentic/
│       └── AgenticMetrics.cs # ToolSelection, ToolSuccess, etc.
│
├── Assertions/              # Fluent assertions
│   ├── ToolUsageAssertions.cs
│   ├── PerformanceAssertions.cs
│   └── ResponseAssertions.cs
│
├── Benchmarks/              # Benchmarking infrastructure
│   ├── PerformanceBenchmark.cs
│   └── AgenticBenchmark.cs
│
├── Adapters/                # Framework integrations
│   └── MicrosoftEvaluatorAdapter.cs
│
├── MAF/                     # Microsoft Agent Framework
│   └── MAFTestHarness.cs
│
├── Embeddings/              # Embedding utilities
│   ├── IAgentEvalEmbeddings.cs
│   └── EmbeddingSimilarity.cs
│
└── Testing/                 # Test utilities
    └── FakeChatClient.cs
```

---

## Future Architecture (Planned)

### Production Infrastructure

```
AgentEval/
├── ... (existing)
│
├── Exporters/               # Result exporters (planned)
│   ├── IResultExporter.cs
│   ├── JUnitXmlExporter.cs
│   ├── MarkdownExporter.cs
│   └── JsonExporter.cs
│
├── DataLoaders/             # Dataset loaders (planned)
│   ├── IDatasetLoader.cs
│   ├── JsonLinesLoader.cs
│   └── BfclDatasetLoader.cs
│
└── Snapshots/               # Snapshot testing (planned)
    ├── SnapshotTestHarness.cs
    └── PropertyMatchers.cs

AgentEval.Cli/               # CLI tool (planned)
├── Program.cs
├── Commands/
│   ├── EvalCommand.cs
│   ├── SnapshotCommand.cs
│   └── CompareCommand.cs
└── Configuration/
    └── YamlConfigLoader.cs
```

---

## See Also

- [Extensibility Guide](extensibility.md) - Creating custom metrics and plugins
- [Embedding Metrics](embedding-metrics.md) - Semantic similarity evaluation
- [Benchmarks Guide](benchmarks.md) - Running standard benchmarks
