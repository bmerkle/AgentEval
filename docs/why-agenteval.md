# Use Cases

> **Who is AgentEval for and what can you achieve?**

---

## Target Users

### 🏢 .NET Teams Building AI Agents

If you're building production AI agents in .NET and need to:

- **Verify tool usage** - Did the agent call the right tools in the right order?
- **Enforce SLAs** - Is response time and cost within acceptable limits?
- **Handle non-determinism** - How often does the agent actually succeed?
- **Compare models** - Which model gives the best quality for the cost?

### 🚀 Microsoft Agent Framework (MAF) Developers

Native integration with MAF concepts:

- `AIAgent`, `IChatClient`, `IStreamingChatClient` support
- Automatic tool call tracking from `AIFunctionContext`
- Performance metrics with token usage and cost estimation

### 📊 ML Engineers Evaluating LLM Quality

Rigorous evaluation capabilities:

- RAG metrics: Faithfulness, Relevance, Context Precision
- Embedding-based similarity metrics
- Calibrated judge patterns for consistent evaluation

---

## Common Scenarios

### Scenario 1: Model Upgrade Testing

You're upgrading from GPT-4 to GPT-4o. Will it break anything?

```csharp
// Run tests across both models with statistical significance
var result = await stochasticRunner.RunStochasticTestAsync(
    gpt4oAgent, testCase, 
    new StochasticOptions(Runs: 20, SuccessRateThreshold: 0.85));

// Compare: Did success rate drop?
Console.WriteLine($"Success rate: {result.Statistics.SuccessRate:P1}");
```

### Scenario 2: Tool Chain Verification

Your financial agent must verify identity BEFORE transferring funds:

```csharp
result.ToolUsage!.Should()
    .HaveCalledTool("VerifyIdentity")
        .BeforeTool("TransferFunds")  // Order enforcement
        .WithArgument("method", "TwoFactor")
    .HaveNoErrors();
```

### Scenario 3: Performance SLA Enforcement

Production requires responses under 5 seconds and cost under $0.05:

```csharp
result.Performance!.Should()
    .HaveTotalDurationUnder(TimeSpan.FromSeconds(5))
    .HaveEstimatedCostUnder(0.05m);
```

### Scenario 4: Compliance Testing

Your agent must never call certain tools or reveal sensitive data:

```csharp
result.Should()
    .NeverCallTool("DeleteAccount")
    .NeverPassArgumentMatching(@"\b\d{3}-\d{2}-\d{4}\b");  // No SSNs
```

### Scenario 5: Debug Past Failures

Something went wrong. Record and analyze:

```csharp
// Record execution
var recorder = new TraceRecordingAgent(agent);
await recorder.ExecuteAsync(userInput);
TraceSerializer.Save(recorder.GetTrace(), "incident-trace.json");

// Later: load and inspect tool calls, timing, responses
var trace = TraceSerializer.Load("incident-trace.json");
```

---

## What AgentEval Evaluates

| Category | What It Measures |
|----------|------------------|
| **Tool Usage** | Tools called, order, arguments, errors, retries |
| **Performance** | Duration, TTFT, token usage, cost estimation |
| **RAG Quality** | Faithfulness, Relevance, Context Precision/Recall |
| **Compliance** | Forbidden tools, PII detection, policy violations |

---

## Next Steps

<div class="grid cards" markdown>

-   :rocket: **[Get Started](getting-started.md)**

    Install and write your first test in 5 minutes

-   :books: **[Migration Guide](comparison.md)**

    Coming from Python? CLI tools? We've got you covered.

-   :test_tube: **[Assertions Reference](assertions.md)**

    Complete fluent assertion API

</div>
