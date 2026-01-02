# AgentEval

> **The first .NET-native AI agent testing, evaluation, and benchmarking framework**

## Features

- 🎯 **Tool Tracking** - Monitor tool/function calls with timing and arguments
- 📊 **Performance Metrics** - TTFT, latency, tokens, cost estimation
- ✅ **Fluent Assertions** - Expressive test assertions for tools and performance
- 🔬 **RAG Metrics** - Faithfulness, relevance, context precision
- 📈 **Benchmarking** - Performance and agentic benchmarks
- 🔌 **Extensible** - Adapter pattern for multiple agent frameworks
- 📋 **Trace-First Failure Reporting** - Structured failure reports with tool timelines
- 🔧 **Testing Infrastructure** - FakeChatClient for mocking LLM responses

## Quick Start

```csharp
using AgentEval;
using AgentEval.MAF;

// Create test harness
var harness = new MAFTestHarness(evaluatorClient);

// Run test with tool tracking
var result = await harness.RunTestAsync(agent, new TestCase
{
    Name = "Feature Planning Test",
    Input = "Plan a user authentication feature",
    EvaluationCriteria = ["Should include security considerations"]
});

// Assert tool usage
result.ToolUsage!
    .Should()
    .HaveCalledTool("SecurityTool")
        .BeforeTool("FeatureTool")
        .WithoutError()
    .And()
    .HaveNoErrors();

// Assert performance
result.Performance!
    .Should()
    .HaveTotalDurationUnder(TimeSpan.FromSeconds(10))
    .HaveEstimatedCostUnder(0.10m);

// Check failure details
if (!result.Passed && result.Failure != null)
{
    Console.WriteLine($"Failure: {result.Failure.Summary}");
    Console.WriteLine($"Tool Timeline: {result.Timeline?.TotalToolCalls} calls");
}
```

## Test Coverage

- **210 unit tests** covering all core functionality
- Tests for all assertions, metrics, models, and adapters
- All tests passing ✅

## Installation

```bash
dotnet add package AgentEval
```

## Documentation

See the [AgentEval Design Document](AgentEval-Design.md) for full documentation.

## License

MIT License - See LICENSE file for details.
