# AgentEval - Forward Plan & Roadmap

> **Strategic roadmap to make AgentEval THE .NET AI Agent Evaluation Framework**

**Created:** January 2026  
**Version:** 1.1  
**Status:** Strategic Planning  
**Last Updated:** January 2, 2026

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [External Framework Analysis](#external-framework-analysis)
3. [Current State Analysis](#current-state-analysis)
4. [Code Quality Review](#code-quality-review)
5. [Strategic Vision](#strategic-vision)
6. [Phase 1: Polish & Publication](#phase-1-polish--publication)
7. [Phase 2: Enterprise Features](#phase-2-enterprise-features)
8. [Phase 3: Ecosystem & Community](#phase-3-ecosystem--community)
9. [Phase 4: Advanced Capabilities](#phase-4-advanced-capabilities)
10. [Technical Debt & Optimizations](#technical-debt--optimizations)
11. [Competitive Positioning](#competitive-positioning)
12. [Success Metrics](#success-metrics)

---

## Executive Summary

AgentEval is positioned to become **THE** .NET AI agent evaluation framework. With core functionality complete (210 tests passing, all major features implemented), the focus shifts to:

1. **Polish & Publish** - Make it production-ready and ship to NuGet
2. **Enterprise** - Add features large organizations need
3. **Community** - Build ecosystem and developer experience
4. **Innovate** - Red-teaming, synthetic data, advanced safety

**Key Insight:** No other framework offers native .NET integration with this level of completeness. This is our moat.

**New Insight (Jan 2026):** After analyzing Microsoft.Extensions.AI.Evaluation and kbeaugrand/KernelMemory.Evaluation, we have a unique opportunity to be the bridge between Microsoft's official evaluators and the agentic/tool-tracking features they lack.

**Recent Additions (Phase 5-6 Complete):**
- ✅ Trace-first failure reporting with FailureReport and ToolCallTimeline
- ✅ IAgentEvalLogger abstraction with ConsoleAgentEvalLogger, NullAgentEvalLogger, MicrosoftExtensionsLoggingAdapter
- ✅ FakeChatClient for zero-dependency testing of LLM metrics
- ✅ Metric tests for FaithfulnessMetric, ToolSelectionMetric, ToolSuccessMetric (24 tests)

---

## External Framework Analysis

### 1. Microsoft.Extensions.AI.Evaluation (Official Microsoft Package)

**Location:** `dotnet/extensions` repository  
**Package:** `Microsoft.Extensions.AI.Evaluation` + `Microsoft.Extensions.AI.Evaluation.Quality`  
**Status:** Active development (marked `[Experimental("AIEVAL001")]`)

#### Key Design Patterns

```csharp
// Microsoft's IEvaluator interface
public interface IEvaluator
{
    IReadOnlyCollection<string> EvaluationMetricNames { get; }
    
    ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default);
}
```

#### Available Evaluators

| Evaluator | Type | Description |
|-----------|------|-------------|
| `FluencyEvaluator` | Quality | Linguistic correctness (1-5 score) |
| `CoherenceEvaluator` | Quality | Logical flow and consistency |
| `RelevanceEvaluator` | Quality | Answer relevance to question |
| `GroundednessEvaluator` | RAG | Answer grounded in context |
| `EquivalenceEvaluator` | RAG | Similarity to ground truth |
| `CompletenessEvaluator` | RAG | Completeness vs expected answer |
| `RetrievalEvaluator` | RAG | Context retrieval quality |
| `TaskAdherenceEvaluator` | Agentic | Task completion (1-5 score) |
| `IntentResolutionEvaluator` | Agentic | Intent detection/resolution |
| `ToolCallAccuracyEvaluator` | Agentic | Tool call correctness |
| `RelevanceTruthAndCompletenessEvaluator` | Combined | Multi-metric evaluator |
| `BLEUEvaluator` | NLP | BLEU score for translation quality |

#### Key Insights

1. **Uses 1-5 scoring** - We use 0-100; need score normalization utilities
2. **Requires ChatConfiguration** - All AI-based evaluators need IChatClient
3. **Context pattern** - Uses `EvaluationContext` classes per evaluator type
4. **Response parsing** - Uses `<S0>`, `<S1>`, `<S2>` tags for structured output
5. **Interpretation property** - `metric.Interpretation = metric.InterpretScore()`

#### What They DON'T Have (Our Opportunity)

| Feature | Microsoft.Extensions.AI.Evaluation | AgentEval |
|---------|-----------------------------------|-----------|
| Tool timing/duration tracking | ❌ | ✅ |
| Per-tool performance metrics | ❌ | ✅ |
| Tool call ordering assertions | ❌ | ✅ |
| Tool argument validation | ❌ | ✅ |
| Streaming TTFT metrics | ❌ | ✅ |
| Cost estimation | ❌ | ✅ |
| Fluent assertion API | ❌ | ✅ |
| Benchmark infrastructure | ❌ | ✅ |
| MAF integration | ❌ | ✅ |

### 2. kbeaugrand/KernelMemory.Evaluation (RAGAS for .NET)

**Location:** `github.com/kbeaugrand/KernelMemory.Evaluation`  
**Focus:** RAG evaluation with Kernel Memory

#### Architecture

```csharp
// Base class for all evaluators
public abstract class EvaluationEngine
{
    protected string GetSKPrompt(string pluginName, string functionName);
    protected async Task<T> Try<T>(int maxCount, Func<int, Task<T>> action);
}

// Example evaluator structure
public class FaithfulnessEvaluator : EvaluationEngine
{
    private readonly Kernel kernel;
    
    // Uses SK KernelFunction for prompt execution
    private KernelFunction ExtractStatements => kernel.CreateFunctionFromPrompt(...);
    private KernelFunction FaithfulnessEvaluation => kernel.CreateFunctionFromPrompt(...);
    
    public async Task<(float Score, IEnumerable<StatementEvaluation>? Evaluations)> EvaluateAsync(...)
}
```

#### Key Metrics Implemented

| Metric | Approach | Score Calculation |
|--------|----------|-------------------|
| **Faithfulness** | Extract statements → Verify each against context | Count(supported) / Count(total) |
| **Relevance** | Generate questions answer could address → Compare embeddings | Cosine similarity * committal |
| **Context Precision** | Check if context was useful for arriving at answer | Verdict (0/1) per context chunk |
| **Context Recall** | Classify ground truth statements as attributed or not | Count(attributed) / Count(total) |
| **Answer Correctness** | Extract statements → Classify as TP/FP/FN | TP / (TP + 0.5*(FP + FN)) |
| **Answer Similarity** | Generate embeddings → Cosine similarity | Direct cosine similarity |

#### Key Insights

1. **Prompt-based extraction** - Uses structured prompts in .txt files
2. **Statement-level evaluation** - Extracts individual claims for verification
3. **Retry logic** - `Try(3, async (remainingTry) => ...)` pattern
4. **Temperature near zero** - `Temperature = 1e-8f` for deterministic outputs
5. **JSON response format** - `ResponseFormat = "json_object"`
6. **Embedding integration** - Uses `ITextEmbeddingGenerationService`

#### What They DON'T Have (Our Opportunity)

| Feature | KernelMemory.Evaluation | AgentEval |
|---------|------------------------|-----------|
| Agentic tool metrics | ❌ | ✅ |
| Performance benchmarking | ❌ | ✅ |
| Streaming support | ❌ | ✅ |
| Cost tracking | ❌ | ✅ |
| Fluent assertions | ❌ | ✅ |
| MAF support | ❌ | ✅ |
| Non-SK framework support | ❌ | ✅ |

### 3. Integration Strategy

#### Option A: Use Microsoft.Extensions.AI.Evaluation as Dependency ⭐ RECOMMENDED

```csharp
// Create adapter to use Microsoft evaluators
public class MicrosoftEvaluatorAdapter : IMetric
{
    private readonly IEvaluator _msEvaluator;
    private readonly IChatClient _chatClient;
    
    public async Task<MetricResult> EvaluateAsync(EvaluationContext context, CancellationToken ct)
    {
        var chatConfig = new ChatConfiguration(_chatClient);
        var messages = new[] { new ChatMessage(ChatRole.User, context.Input) };
        var response = new ChatResponse([new ChatMessage(ChatRole.Assistant, context.Output)]);
        
        var result = await _msEvaluator.EvaluateAsync(messages, response, chatConfig, ct: ct);
        
        // Normalize 1-5 to 0-100
        var msScore = result.Metrics.First().Value is NumericMetric nm ? nm.Value : 0;
        var normalizedScore = (msScore - 1) * 25; // 1->0, 5->100
        
        return new MetricResult { Score = normalizedScore, ... };
    }
}
```

**Benefits:**
- Leverage Microsoft's tested prompts
- Get updates automatically
- Reduce maintenance burden
- Enterprise credibility

**Risks:**
- Experimental API may change
- Extra dependency
- Less control over prompts

#### Option B: Port Best Patterns from Both

Take the best from each:
- **From Microsoft:** Tag-based response parsing, interpretation patterns
- **From kbeaugrand:** Statement extraction, retry logic, F1-score formulas

---

## Current State Analysis

### What's Complete ✅

| Category | Features | Quality |
|----------|----------|---------|
| **Core Interfaces** | IMetric, IRAGMetric, IAgenticMetric, ITestableAgent, IStreamableAgent, ITestHarness, IEvaluator | ⭐⭐⭐⭐⭐ |
| **Models** | ToolCallRecord, ToolUsageReport, PerformanceMetrics, TestCase, TestResult, TestSummary, TokenUsage, FailureReport, ToolCallTimeline | ⭐⭐⭐⭐⭐ |
| **Logging** | IAgentEvalLogger, ConsoleAgentEvalLogger, NullAgentEvalLogger, MicrosoftExtensionsLoggingAdapter | ⭐⭐⭐⭐⭐ |
| **Assertions** | Tool (12+ methods), Performance (10+ methods), Response (10+ methods) | ⭐⭐⭐⭐⭐ |
| **RAG Metrics** | Faithfulness, Relevance, ContextPrecision, ContextRecall, AnswerCorrectness | ⭐⭐⭐⭐ |
| **Agentic Metrics** | ToolSelection, ToolArguments, ToolSuccess, TaskCompletion, ToolEfficiency | ⭐⭐⭐⭐ |
| **Benchmarks** | Latency, Throughput, Cost, ToolAccuracy, TaskCompletion, MultiStep | ⭐⭐⭐⭐ |
| **MAF Integration** | MAFAgentAdapter, MAFTestHarness with streaming, trace-first failure reporting | ⭐⭐⭐⭐⭐ |
| **Testing Infrastructure** | FakeChatClient for mocking IChatClient, no external dependencies | ⭐⭐⭐⭐⭐ |
| **Tests** | 210 unit tests, all passing | ⭐⭐⭐⭐⭐ |

### Architecture Strengths

1. **Interface Segregation** - Clean separation (IMetric → IRAGMetric → IAgenticMetric)
2. **Adapter Pattern** - MAFAgentAdapter enables easy framework support
3. **Fluent API** - Intuitive `Should()` assertions with chaining
4. **SOLID Principles** - Single responsibility, open for extension
5. **Multi-targeting** - net8.0, net9.0, net10.0 support

### Lines of Code Summary

| File | Lines | Complexity |
|------|-------|------------|
| MAFTestHarness.cs | ~350 | High |
| AgenticBenchmark.cs | ~300 | High |
| PerformanceBenchmark.cs | ~250 | Medium |
| RAGMetrics.cs | ~350 | Medium |
| AgenticMetrics.cs | ~300 | Medium |
| ToolUsageAssertions.cs | ~200 | Low |
| PerformanceAssertions.cs | ~100 | Low |
| **Total** | ~2,500+ | - |

---

## Code Quality Review

### Identified Issues & Fixes

#### 1. ✅ FIXED - Duplicate Code - ExtractToolUsage (Priority: Medium)

**Problem:** Same `ExtractToolUsage` method exists in both `MAFTestHarness.cs` and `AgenticBenchmark.cs`.

**Solution:** Created `Core/ToolUsageExtractor.cs` with shared implementation.
```csharp
// Implemented in Core/ToolUsageExtractor.cs
public static class ToolUsageExtractor
{
    public static ToolUsageReport Extract(IReadOnlyList<object>? rawMessages) { ... }
    public static ToolUsageReport Extract(AgentResponse response) { ... }
}
```

#### 2. ✅ FIXED - Thread Safety - ModelPricing (Priority: High)

**Problem:** `SetPricing` modifies a dictionary that could be read concurrently.

**Solution:** Changed to `ConcurrentDictionary` in `Models/PerformanceMetrics.cs`.
```csharp
public static class ModelPricing
{
    private static readonly ConcurrentDictionary<string, (decimal InputPer1K, decimal OutputPer1K)> Pricing = new();
    
    public static void SetPricing(string modelName, decimal inputPer1K, decimal outputPer1K)
    {
        ArgumentNullException.ThrowIfNull(modelName);
        Pricing.AddOrUpdate(modelName.ToLowerInvariant(), (inputPer1K, outputPer1K), (_, _) => (inputPer1K, outputPer1K));
    }
}
```

#### 3. ✅ FIXED - Magic Numbers (Priority: Low)

**Problem:** Hardcoded values like `70` (passing score), `10` (penalty per extra tool).

**Solution:** Created `Core/EvaluationDefaults.cs` with all constants.
```csharp
// Implemented in Core/EvaluationDefaults.cs
public static class EvaluationDefaults
{
    public const int DefaultPassingScore = 70;
    public const double PassingScoreThreshold = 70.0;
    public const double ExtraToolPenaltyPercent = 10.0;
    public const double OrderPenaltyPercent = 20.0;
    public const int DefaultMaxExpectedToolCalls = 10;
    public static readonly TimeSpan DefaultMaxToolDuration = TimeSpan.FromSeconds(30);
    public const double DuplicateToolPenaltyPercent = 5.0;
}
```

#### 4. ✅ FIXED - JSON Parsing Foundation (Priority: Low)

**Problem:** Using `System.Text.Json.JsonDocument` in hot paths with repetitive extraction code.

**Solution:** Created `Core/LlmJsonParser.cs` helper utilities.
```csharp
// Implemented in Core/LlmJsonParser.cs
public static class LlmJsonParser
{
    public static string? ExtractJson(string text) { ... }
    public static T? ParseMetricResponse<T>(string response) { ... }
    public static List<string> ExtractStringArray(JsonElement element, string propertyName) { ... }
    public static bool GetBoolean(JsonElement element, string propertyName, bool defaultValue) { ... }
}
```
**Note:** Source generators for full optimization are a future enhancement.

#### 5. ✅ FIXED - Missing Null Checks (Priority: Medium)

**Problem:** Some methods don't validate required parameters.

**Solution:** Added `ArgumentNullException.ThrowIfNull()` to:
- `ToolSelectionMetric` constructor
- `ToolArgumentsMetric` constructor  
- `ModelPricing.SetPricing` method
- `ToolUsageExtractor.Extract` method
```csharp
public ToolSelectionMetric(IEnumerable<string> expectedTools, bool strictOrder = false)
{
    ArgumentNullException.ThrowIfNull(expectedTools);
    _expectedTools = expectedTools.ToList();
    _strictOrder = strictOrder;
}
```

---

## Strategic Vision

### What Makes "THE" Framework?

To become the definitive .NET AI agent evaluation framework, we need:

| Attribute | Current State | Target State |
|-----------|---------------|--------------|
| **Completeness** | Core complete | All features of DeepEval + RAGAS + Promptfoo |
| **Adoption** | Internal only | 10K+ monthly NuGet downloads |
| **Framework Support** | MAF complete | MAF + future frameworks via adapters |
| **Enterprise Ready** | Alpha | SOC2-compatible, enterprise features |
| **Community** | None | Active contributors, Discord, examples |
| **Documentation** | Basic | Comprehensive docs site |
| **Tooling** | CLI only | VS extension, GitHub Actions, dashboard |

### Competitive Advantages to Maintain

1. **First-mover in .NET** - No real competition exists
2. **Microsoft alignment** - Built on Microsoft.Extensions.AI
3. **Fluent API** - Best-in-class developer experience
4. **Performance focus** - Streaming, benchmarking, cost tracking

---

## Phase 1: Polish & Publication

**Timeline:** 2-3 weeks  
**Goal:** Ship v1.0.0-alpha to NuGet

### 1.1 Code Cleanup (Week 1)

- [ ] Extract duplicate `ExtractToolUsage` to shared utility
- [ ] Add `ConcurrentDictionary` for ModelPricing
- [ ] Extract magic numbers to constants class
- [ ] Add XML documentation to ALL public APIs
- [ ] Add nullable reference type annotations throughout
- [ ] Create `AgentEval.ruleset` for consistent code style

### 1.2 Testing Enhancement (Week 1)

- [ ] Add integration tests with mock LLM responses
- [ ] Add edge case tests (empty responses, network errors, timeouts)
- [ ] Add benchmark comparison tests (regression detection)
- [ ] Achieve 90%+ code coverage
- [ ] Add test categories: Unit, Integration, Performance

### 1.3 Package Preparation (Week 2)

- [ ] Create proper `README.md` for NuGet
- [ ] Add `CHANGELOG.md`
- [ ] Configure SourceLink for debugging
- [ ] Set up deterministic builds
- [ ] Add license header to all files
- [ ] Create logo/icon for NuGet
- [ ] Add package validation

```xml
<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>
</PropertyGroup>
```

### 1.4 Repository Setup (Week 2)

- [ ] Create standalone GitHub repository: `AgentEval/AgentEval`
- [ ] Set up GitHub Actions for CI/CD
- [ ] Configure Dependabot
- [ ] Add issue/PR templates
- [ ] Create CONTRIBUTING.md
- [ ] Set up discussions

### 1.5 Initial Publication (Week 3)

- [ ] Publish to NuGet as `1.0.0-alpha`
- [ ] Create GitHub release
- [ ] Write announcement blog post
- [ ] Share on .NET community channels

---

## Phase 2: Enterprise Features

**Timeline:** 6-8 weeks  
**Goal:** Make AgentEval enterprise-ready

### 2.1 Tracing & Observability

```csharp
public class TracingTestHarness : ITestHarness
{
    private readonly ActivitySource _activitySource = new("AgentEval");
    
    public async Task<TestResult> RunTestAsync(...)
    {
        using var activity = _activitySource.StartActivity("AgentEval.Test");
        activity?.SetTag("test.name", testCase.Name);
        activity?.SetTag("agent.name", agent.Name);
        
        try
        {
            var result = await _inner.RunTestAsync(...);
            activity?.SetTag("test.passed", result.Passed);
            activity?.SetTag("test.score", result.Score);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

**Integrations:**
- [ ] OpenTelemetry export
- [ ] Application Insights integration
- [ ] Jaeger/Zipkin support
- [ ] Custom span attributes for all metrics

### 3.2 Dataset Management

```csharp
public interface ITestDataset
{
    string Name { get; }
    IAsyncEnumerable<TestCase> GetTestCasesAsync(CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}

public class JsonFileDataset : ITestDataset
{
    public static JsonFileDataset Load(string path) { ... }
}

public class CsvDataset : ITestDataset
{
    public static CsvDataset Load(string path, CsvDatasetOptions options) { ... }
}

public class HuggingFaceDataset : ITestDataset
{
    public HuggingFaceDataset(string datasetId, string split = "test") { ... }
}
```

### 3.3 Results Persistence

```csharp
public interface IResultStore
{
    Task SaveAsync(TestSummary summary, CancellationToken ct = default);
    Task<TestSummary?> LoadAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<TestSummary>> ListAsync(ResultFilter filter, CancellationToken ct = default);
}

public class SqlResultStore : IResultStore { ... }
public class CosmosResultStore : IResultStore { ... }
public class FileResultStore : IResultStore { ... }
```

### 3.4 Regression Detection

```csharp
public class RegressionDetector
{
    public RegressionReport Compare(TestSummary baseline, TestSummary current)
    {
        var regressions = new List<Regression>();
        
        foreach (var result in current.Results)
        {
            var baselineResult = baseline.Results.FirstOrDefault(r => r.TestName == result.TestName);
            if (baselineResult != null)
            {
                if (result.Score < baselineResult.Score - threshold)
                {
                    regressions.Add(new Regression
                    {
                        TestName = result.TestName,
                        BaselineScore = baselineResult.Score,
                        CurrentScore = result.Score,
                        Delta = result.Score - baselineResult.Score
                    });
                }
            }
        }
        
        return new RegressionReport { Regressions = regressions };
    }
}
```

### 3.5 CI/CD Integration

**GitHub Actions:**
```yaml
- name: Run Agent Evaluation
  uses: AgentEval/evaluate-action@v1
  with:
    test-project: ./tests/AgentTests.csproj
    baseline-results: ./baselines/main.json
    fail-on-regression: true
    regression-threshold: 5
    
- name: Upload Results
  uses: AgentEval/upload-results@v1
  with:
    results: ./test-results.json
    dashboard-url: ${{ secrets.AGENTEVAL_DASHBOARD }}
```

**Azure DevOps:**
```yaml
- task: AgentEval@1
  inputs:
    projectPath: '$(System.DefaultWorkingDirectory)/tests'
    publishResults: true
```

---

## Phase 3: Ecosystem & Community

**Timeline:** Ongoing  
**Goal:** Build thriving community

### 3.1 Documentation Site

- [ ] Create docs site with Docusaurus/VitePress
- [ ] Getting Started guide
- [ ] API Reference (auto-generated)
- [ ] Tutorials:
  - Testing MAF agents
  - RAG evaluation guide
  - Benchmarking guide
  - CI/CD integration
- [ ] Examples repository

### 3.2 Visual Studio Extension

```
AgentEval.VisualStudio/
├── TestExplorer/           # Custom test explorer for agent tests
├── ResultsViewer/          # Rich results visualization
├── Snippets/               # Code snippets for assertions
└── Analyzers/              # Roslyn analyzers for best practices
```

### 3.3 CLI Tool

```bash
# Install
dotnet tool install -g AgentEval.CLI

# Initialize project
agenteval init --framework maf

# Run tests
agenteval test --project ./tests

# Benchmark
agenteval benchmark --iterations 20 --output results.json

# Compare
agenteval compare baseline.json current.json --threshold 5

# Generate report
agenteval report results.json --format html --output report.html
```

### 3.4 Dashboard (Optional SaaS)

```
AgentEval.Dashboard/
├── Frontend/               # React dashboard
│   ├── TestResults/        # Results visualization
│   ├── Benchmarks/         # Performance charts
│   ├── Regressions/        # Regression tracking
│   └── Comparisons/        # A/B testing
└── Backend/                # ASP.NET Core API
    ├── Results/            # Store results
    ├── Webhooks/           # CI/CD integration
    └── Analytics/          # Usage analytics
```

### 3.5 Community Building

- [ ] Create Discord server
- [ ] Write blog posts:
  - "Introducing AgentEval"
  - "Testing AI Agents in .NET"
  - "RAG Evaluation Best Practices"
- [ ] Conference talks (NDC, .NET Conf)
- [ ] Twitter/X engagement
- [ ] Respond to issues within 24h

---

## Phase 4: Advanced Capabilities

**Timeline:** 3-6 months  
**Goal:** Feature parity with Python frameworks + innovation

### 4.1 Red-Teaming & Safety

```csharp
public interface IRedTeamingPlugin
{
    string Name { get; }
    IAsyncEnumerable<string> GenerateAttackPromptsAsync(RedTeamingContext context);
}

public class JailbreakPlugin : IRedTeamingPlugin { ... }
public class PromptInjectionPlugin : IRedTeamingPlugin { ... }
public class ContentFilterBypassPlugin : IRedTeamingPlugin { ... }
public class PIIExtractionPlugin : IRedTeamingPlugin { ... }

public class RedTeamingBenchmark
{
    public async Task<RedTeamingResult> RunAsync(
        ITestableAgent agent,
        IEnumerable<IRedTeamingPlugin> plugins)
    {
        // Run attacks, evaluate responses
    }
}
```

### 4.2 Synthetic Data Generation

```csharp
public class SyntheticDataGenerator
{
    private readonly IChatClient _generator;
    
    public async IAsyncEnumerable<TestCase> GenerateAsync(
        SyntheticDataSpec spec,
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Use LLM to generate test cases based on spec
        var prompt = BuildGenerationPrompt(spec);
        
        for (int i = 0; i < count; i++)
        {
            var response = await _generator.GetResponseAsync(prompt, cancellationToken: ct);
            yield return ParseTestCase(response.Text);
        }
    }
}

// Usage
var generator = new SyntheticDataGenerator(chatClient);
var testCases = await generator.GenerateAsync(new SyntheticDataSpec
{
    Domain = "Customer support",
    Difficulty = Difficulty.Hard,
    RequireToolUsage = true,
    ExpectedTools = new[] { "SearchKnowledgeBase", "CreateTicket" }
}, count: 100).ToListAsync();
```

### 4.3 Multi-Agent Evaluation

```csharp
public interface IMultiAgentScenario
{
    IReadOnlyList<ITestableAgent> Agents { get; }
    Task<MultiAgentResult> RunAsync(CancellationToken ct = default);
}

public class ConversationScenario : IMultiAgentScenario
{
    public async Task<MultiAgentResult> RunAsync(CancellationToken ct)
    {
        var conversation = new List<ConversationTurn>();
        var currentAgent = 0;
        
        while (!IsConversationComplete())
        {
            var agent = Agents[currentAgent];
            var response = await agent.InvokeAsync(GetNextPrompt(), ct);
            conversation.Add(new ConversationTurn(agent.Name, response));
            currentAgent = (currentAgent + 1) % Agents.Count;
        }
        
        return EvaluateConversation(conversation);
    }
}
```

### 4.4 Human-in-the-Loop Evaluation

```csharp
public interface IHumanEvaluator
{
    Task<HumanEvaluation> RequestEvaluationAsync(
        EvaluationContext context,
        IReadOnlyList<string> criteria,
        CancellationToken ct = default);
}

public class SlackHumanEvaluator : IHumanEvaluator { ... }
public class TeamsHumanEvaluator : IHumanEvaluator { ... }
public class WebUIHumanEvaluator : IHumanEvaluator { ... }
```

### 4.5 Model Comparison Framework

```csharp
public class ModelComparer
{
    public async Task<ComparisonResult> CompareAsync(
        IEnumerable<ITestableAgent> agents,
        IEnumerable<TestCase> testCases,
        ComparisonOptions options)
    {
        var results = new Dictionary<string, TestSummary>();
        
        foreach (var agent in agents)
        {
            var harness = new MAFTestHarness();
            var summary = await harness.RunTestSuiteAsync(
                agent.Name, agent, testCases, options.TestOptions);
            results[agent.Name] = summary;
        }
        
        return AnalyzeComparison(results);
    }
}
```

---

## Technical Debt & Optimizations

### Performance Optimizations

| Optimization | Impact | Effort |
|--------------|--------|--------|
| JSON source generators | Medium | Low |
| ArrayPool for string building | Low | Low |
| Parallel metric evaluation | High | Medium |
| Connection pooling for benchmarks | Medium | Low |
| Caching for repeated evaluations | Medium | Medium |

### Code Health

| Task | Priority | Effort |
|------|----------|--------|
| Extract ExtractToolUsage | High | Low |
| ConcurrentDictionary for ModelPricing | High | Low |
| Constants for magic numbers | Medium | Low |
| Nullable annotations | Medium | Medium |
| XML docs on all public APIs | High | Medium |
| Roslyn analyzers for best practices | Low | High |

### Testing Improvements

| Task | Priority | Effort |
|------|----------|--------|
| Integration tests with mock LLM | High | Medium |
| Edge case coverage | Medium | Medium |
| Performance regression tests | Medium | Medium |
| Fuzz testing for parsing | Low | High |

---

## Competitive Positioning

### Feature Comparison After All Phases

| Feature | DeepEval | Promptfoo | RAGAS | AgentEval |
|---------|----------|-----------|-------|-----------|
| .NET Native | ❌ | ❌ | ❌ | ✅ |
| Python | ✅ | ❌ | ✅ | ⏳ Consider |
| Fluent Assertions | ❌ | ❌ | ❌ | ✅ |
| MAF Integration | ❌ | ❌ | ❌ | ✅ |
| Tool Tracking | ✅ | ❌ | ❌ | ✅ |
| Streaming Metrics | ❌ | ❌ | ❌ | ✅ |
| RAG Metrics | Partial | ❌ | ✅ | ✅ |
| Agentic Metrics | ✅ | ❌ | ❌ | ✅ |
| Red-Teaming | ✅ | ✅ | ❌ | ✅ Planned |
| Synthetic Data | ✅ | ❌ | ✅ | ✅ Planned |
| CI/CD Integration | ✅ | ✅ | ❌ | ✅ Planned |
| Dashboard | ✅ (SaaS) | ✅ | ❌ | ✅ Planned |
| Cost Tracking | Partial | ❌ | ❌ | ✅ |
| VS Integration | ❌ | ❌ | ❌ | ✅ Planned |

### Messaging

**Tagline:** "The first .NET-native AI agent testing framework"

**Key Messages:**
1. Built for .NET developers, by .NET developers
2. Native integration with xUnit/NUnit/MSTest
3. First-class support for Microsoft Agent Framework
4. Fluent API that feels like FluentAssertions
5. Enterprise-ready with full observability

---

## Success Metrics

### Year 1 Targets

| Metric | Q1 | Q2 | Q3 | Q4 |
|--------|----|----|----|----|
| NuGet Downloads | 1K | 5K | 15K | 30K |
| GitHub Stars | 100 | 300 | 700 | 1,500 |
| Contributors | 2 | 5 | 10 | 20 |
| Framework Adapters | 1 (MAF) | 3 | 4 | 5 |
| Enterprise Users | 0 | 2 | 5 | 10 |

### Quality Metrics

| Metric | Target |
|--------|--------|
| Test Coverage | 90%+ |
| Documentation Coverage | 100% public API |
| Issue Response Time | < 24 hours |
| Release Cadence | Monthly |
| Breaking Changes | 0 per minor version |

---

## Missing Features & Enhancement Proposals

Based on analysis of Microsoft.Extensions.AI.Evaluation and kbeaugrand/KernelMemory.Evaluation, here are features we should consider adding:

### High Priority - Unique Value

| Feature | Benefit | Effort | Priority |
|---------|---------|--------|----------|
| **Microsoft Evaluator Adapters** | Use official MS evaluators via our API | Medium | ⭐⭐⭐⭐⭐ |
| **Embedding-based Similarity** | More accurate relevance/equivalence scoring | Medium | ⭐⭐⭐⭐ |
| **Statement Extraction** | Better faithfulness via claim-level analysis | Medium | ⭐⭐⭐⭐ |
| **Retry Logic** | Resilience for LLM evaluation calls | Low | ⭐⭐⭐⭐ |
| **Score Normalization** | Convert between 1-5 and 0-100 scales | Low | ⭐⭐⭐⭐ |

### Medium Priority - Parity Features

| Feature | Source | Description |
|---------|--------|-------------|
| **Test Set Generation** | kbeaugrand | Synthetic question generation from documents |
| **Multi-context Questions** | kbeaugrand | Questions requiring multiple context chunks |
| **Reasoning Questions** | kbeaugrand | Questions requiring inference |
| **BLEU Score** | Microsoft | Standard NLP metric for translation quality |

### Lower Priority - Nice to Have

| Feature | Description |
|---------|-------------|
| **Prompt Templates as Resources** | Store prompts in .txt files like kbeaugrand |
| **Temperature=0 Enforcement** | Deterministic evaluation outputs |
| **Tag-based Response Parsing** | `<S0>`, `<S1>` parsing like Microsoft |
| **EvaluationDiagnostic** | Error/warning collection per metric |
| **Metric Interpretation** | Human-readable score interpretation |

### Proposed New Metrics

```csharp
// 1. Answer Semantic Similarity (embedding-based)
public class AnswerSimilarityMetric : IRAGMetric
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddings;
    
    public async Task<MetricResult> EvaluateAsync(EvaluationContext context, CancellationToken ct)
    {
        var embeddings = await _embeddings.GenerateEmbeddingsAsync(
            [context.GroundTruth!, context.Output], ct);
        
        var similarity = TensorPrimitives.CosineSimilarity(
            embeddings[0].Vector.Span, 
            embeddings[1].Vector.Span);
        
        return MetricResult.Pass(Name, similarity * 100, $"Similarity: {similarity:P1}");
    }
}

// 2. Fluency Metric (linguistic correctness)
public class FluencyMetric : IMetric
{
    // Adapted from Microsoft.Extensions.AI.Evaluation.Quality.FluencyEvaluator
}

// 3. Coherence Metric (logical flow)
public class CoherenceMetric : IMetric
{
    // Adapted from Microsoft.Extensions.AI.Evaluation.Quality.CoherenceEvaluator
}
```

### Integration Decision Matrix

| Approach | Pros | Cons | Recommendation |
|----------|------|------|----------------|
| **Use MS.Extensions.AI.Evaluation directly** | Official, maintained, tested | Experimental API, less control | For quality metrics |
| **Port patterns only** | Full control, no dependency | More work, maintenance | For agentic-specific |
| **Dual support** | Best of both worlds | Complexity | ⭐ RECOMMENDED |

---

## Immediate Next Steps (This Week)

1. [x] **Fix critical issues:** ✅ COMPLETE
   - ✅ Extract duplicate ExtractToolUsage → `Core/ToolUsageExtractor.cs`
   - ✅ Add ConcurrentDictionary to ModelPricing
   - ✅ Extract magic numbers → `Core/EvaluationDefaults.cs`
   - ✅ Add null checks to constructors
   - ✅ Create JSON parsing helper → `Core/LlmJsonParser.cs`

2. [x] **Trace-First Failure Reporting:** ✅ COMPLETE
   - ✅ FailureReport model with structured failure information
   - ✅ ToolCallTimeline for detailed tool execution traces
   - ✅ IAgentEvalLogger abstraction (Console, Null, MicrosoftExtensions adapters)
   - ✅ MAFTestHarness integration - 4 call sites creating FailureReports
   - ✅ LogFailure method for trace-first output

3. [x] **Metric Tests (Phase 6):** ✅ COMPLETE
   - ✅ FakeChatClient for zero-dependency testing (no NSubstitute needed)
   - ✅ FaithfulnessMetricTests (7 tests)
   - ✅ ToolSelectionMetricTests (9 tests)
   - ✅ ToolSuccessMetricTests (8 tests)
   - ✅ All 210 tests passing

4. [ ] **Integration decision:**
   - Decide: Use Microsoft.Extensions.AI.Evaluation as optional dependency?
   - Create adapter layer if yes

5. [ ] **Result Exporters:** (Next Priority)
   - [ ] MarkdownResultExporter
   - [ ] JUnitResultExporter
   - [ ] HtmlResultExporter

6. [ ] **Prepare for publication:**
   - Add complete XML documentation
   - Create NuGet README
   - Set up GitHub repository

7. [ ] **Start documentation:**
   - Getting Started guide
   - API overview

8. [ ] **Community prep:**
   - Draft announcement blog post
   - Create Twitter/X account
   - Identify early adopters

---

*This is a living document. Update as priorities evolve.*

**Last Updated:** January 2026  
**Next Review:** 2 weeks  
**Owner:** AgentEval Team
