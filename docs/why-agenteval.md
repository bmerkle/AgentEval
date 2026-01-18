# Success Stories & Use Cases

> **Real teams, real results.** See how organizations use AgentEval to ship AI agents with confidence.

---

## Who Uses AgentEval?

### 🏢 Enterprise AI Teams

Teams building production AI agents for customer service, internal automation, and document processing. AgentEval helps them:

- **Catch regressions** before they hit production
- **Enforce SLAs** on response time and cost
- **Compare models** to make data-driven decisions
- **Run tests in CI/CD** without paying for API calls every build

### 🚀 Microsoft Agent Framework (MAF) Developers

Developers using MAF who need native tooling that understands their stack:

- First-class integration with `AIAgent`, `IChatClient`, `IStreamingChatClient`
- Automatic tool call tracking from `AIFunctionContext`
- Performance metrics with token usage and cost estimation

### 📊 ML Engineers Evaluating LLM Quality

Data scientists and ML engineers who need rigorous evaluation:

- RAG metrics: Faithfulness, Relevance, Context Precision
- Embedding-based similarity metrics
- Calibrated judge patterns for consistent evaluation

---

## What You Can Achieve

### Catch Regressions Before Production

When upgrading models (GPT-4 → GPT-4o), stochastic testing reveals the true impact:

```csharp
// Run same test 20 times, measure actual success rate
var result = await stochasticRunner.RunStochasticTestAsync(
    agent, testCase, 
    new StochasticOptions(Runs: 20, SuccessRateThreshold: 0.85));

// See if the new model maintains quality
Console.WriteLine($"Success rate: {result.Statistics.SuccessRate:P1}");
// Output: "Success rate: 72.0%" ← Regression detected!
```

### Express Tests as Requirements

AgentEval's fluent syntax reads like requirements, making tests self-documenting:

```csharp
result.ToolUsage!.Should()
    .HaveCalledTool("VerifyIdentity")
        .BeforeTool("TransferFunds")
        .WithArgument("method", "TwoFactor")
    .HaveNoErrors();
```

### Debug with Trace Recording

Record agent executions for later analysis and debugging:

```csharp
// Record execution for debugging
var recorder = new TraceRecordingAgent(realAgent);
await recorder.ExecuteAsync("Book a flight to Paris");
TraceSerializer.Save(recorder.GetTrace(), "debug-trace.json");

// Later: replay and inspect what happened
var trace = TraceSerializer.Load("debug-trace.json");
// Examine tool calls, timing, responses without re-running
```

---

## From "It Works on My Machine" to "It Works in Production"

| Stage | Without AgentEval | With AgentEval |
|-------|-------------------|----------------|
| **Development** | Manual testing, hope for the best | Fluent assertions, immediate feedback |
| **PR Review** | "Did you test it?" | CI runs 1,000+ tests automatically |
| **Model Upgrade** | 🙏 Fingers crossed | Stochastic tests reveal true impact |
| **Production** | Users report bugs | Regressions caught before deployment |
| **Cost Management** | Surprise bills | Cost SLAs in every test |

---

## What Teams Evaluate

### 🛠️ Tool Usage
- Did the agent call the right tools?
- In the right order?
- With the right arguments?
- How many retries did it need?

### 📊 RAG Quality
- **Faithfulness**: Is the response grounded in the provided context?
- **Relevance**: Does the response actually answer the question?
- **Context Precision**: Did we retrieve the right documents?

### ⚡ Performance
- **TTFT**: Time to first token (streaming responsiveness)
- **Total Duration**: End-to-end response time
- **Token Usage**: Input/output token counts
- **Cost Estimation**: Dollars per request

### 🛡️ Behavioral Compliance

```csharp
// Enforce behavioral guardrails
result.Should()
    .NeverMentionCompetitors()
    .NotRevealSystemPrompt()
    .FollowPolicy(HIPAAPolicy);
```

---

## Start Your Success Story

<div class="grid cards" markdown>

-   :rocket: **[Get Started in 60 Seconds](getting-started.md)**

    From zero to running tests in minutes

-   :books: **[Migration Guide](comparison.md)**

    Coming from Python? CLI tools? We've got you covered.

-   :test_tube: **[Assertion Reference](assertions.md)**

    Complete guide to fluent assertions

-   :art: **[Code Gallery](showcase/code-gallery.md)**

    "Code You've Been Dreaming Of"

</div>

---

<div align="center">

**Join the teams shipping AI agents with confidence.**

[Get Started →](getting-started.md){ .md-button .md-button--primary }

</div>
