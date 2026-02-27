# Roadmap

AgentEval is actively developed. This page outlines completed features and what's coming next.

## Current Status

AgentEval is in **beta** with all core features complete and ready for production use.

---

## What's Shipped

### Core Evaluation
- **evaluation harness** for AI agents (`MAFEvaluationHarness`, `IEvaluationHarness`)
- **Fluent assertions** for tool usage, performance, and responses
- **stochastic evaluation** - run N times, assert on pass rates
- **Model comparison** with statistical significance
- **Multi-turn conversation evaluation** (`ConversationRunner`)
- **Multi-agent workflow evaluation** - comprehensive pipeline evaluation with MAF integration
  - Sequential and parallel workflow orchestration
  - Per-executor performance analysis and assertions
  - Workflow graph structure validation and visualization
  - Tool chain coordination across multiple agents
  - Event streaming and timeout handling
  - `WorkflowResultAssertions` with hierarchical validation
- **Snapshot evaluation** for regression detection

### Metrics & Evaluation
- **RAG metrics:** Faithfulness, Relevance, Context Precision/Recall, Answer Correctness
- **Agentic metrics:** Tool Selection, Tool Arguments, Tool Success, Task Completion, Efficiency
- **Workflow metrics:** Structure validity, execution order, executor success, tool chain validation, output quality
- **Embedding-based similarity** metrics
- **Calibrated judges** with confidence intervals and voting strategies

### Performance & Observability
- **Streaming support** with real-time callbacks
- **Time to First Token (TTFT)** tracking
- **Per-tool timing** and execution waterfall
- **Token counting and cost estimation** (8+ models)
- **Performance benchmarks** (latency, throughput, cost)
- **Workflow performance benchmarks** - end-to-end latency, per-executor analysis, scaling characteristics, quality vs performance trade-offs
- **Trace record/replay** - capture and reproduce executions deterministically

### Behavioral Policies
- `NeverCallTool()` - forbid specific tool calls
- `NeverPassArgumentMatching()` - forbid argument patterns (PII, etc.)
- `MustConfirmBefore()` - require confirmation before sensitive operations

### CI/CD Integration
- **Result exporters:** JSON, JUnit XML, Markdown, TRX, CSV, Mermaid (workflow graphs)
- **Dataset loaders:** JSON, JSONL, CSV, YAML (including workflow test cases)
- **Rich test output** with verbosity levels and trace artifacts

### Security & Responsible AI
- **Red Team security scanning** - 192 probes across 9 attack types
  - 60% OWASP LLM Top 10 2025 coverage (6/10) and 6 MITRE ATLAS techniques
  - 6 export formats: JSON, JUnit XML, SARIF, Markdown, PDF, CSV
  - 4 compliance reports: OWASP, MITRE, SOC2, ISO27001
  - Fluent assertions: `result.Should().HaveOverallScoreAbove(85)`
  - Baseline comparison for CI/CD regression tracking
- **Responsible AI metrics:** Toxicity, Bias, Misinformation detection
- **Calibrated Evaluator:** Multi-model consensus evaluation with voting strategies

### CLI Tool
- **`agenteval eval` command** — Evaluate any OpenAI-compatible AI agent from the command line
  - 15 CLI options, 7 export formats, LLM-as-judge, system prompt from file
  - CI/CD exit codes (0=pass, 1=fail, 2=usage error, 3=runtime error)
  - Packaged as `dotnet tool install AgentEval.Cli`

### Cross-Framework & DI
- **`IChatClient.AsEvaluableAgent()` one-liner** — Universal adapter for any AI provider
- **Dependency Injection architecture** — `services.AddAgentEval()`, `services.AddAgentEvalAll()`
- **NuGetConsumer Semantic Kernel demo** — Real SK with `[KernelFunction]` plugins evaluated by AgentEval

### Framework Support
- **Microsoft Agent Framework (MAF)** adapter with comprehensive workflow support
  - `MAFWorkflowAdapter` for multi-agent pipeline evaluation
  - `MAFWorkflowEventBridge` for event processing and timeout handling
  - Native MAF `WorkflowBuilder` integration with `BindAsExecutor`
- **Generic `IChatClient`** adapter
- **Microsoft.Extensions.AI.Evaluation** integration

---

## What's Next

We're focused on making AgentEval the most comprehensive evaluation toolkit for AI agents in .NET. Upcoming areas include:

- **CLI Phase 2** — `compare`, `replay`, `record`, and `stochastic` commands (the `eval` command shipped in 0.6.0-beta)
- **Additional framework adapters** — Semantic Kernel native adapter, LangChain.NET
- **Visual reporting** — HTML reports, interactive diagrams
- **Experiment management** — A/B evaluation, baseline comparison, run metadata and tagging
- **MCP Server** — AI coding assistant integration for discoverability
- **Benchmark runner** — `IBenchmark`/`BenchmarkRunner` orchestration abstraction
- **Verify.Xunit integration** — Snapshot testing with Verify library

---

## Feature Requests

Have a feature request? [Open an issue](https://github.com/joslat/AgentEval/issues/new?template=feature_request.md) on GitHub!

We prioritize based on community needs and contributions.

---

## Version History

| Version | Date | Highlights |
|---------|------|------------|
| 0.6.0-beta | Feb 2026 | CLI `eval` command, `AsEvaluableAgent()` one-liner, DI/IOC architecture, NuGetConsumer SK demo, 27 samples, 7,345 tests |
| 0.4.0-beta | Feb 2026 | Red Team security (192 probes, OWASP LLM 2025), Responsible AI metrics, Calibrated Evaluator, MAF RC1, 6,573 tests |
| 0.3.0-beta | Jan 2026 | Brand rename: evaluation-first naming (Test→Evaluation across all APIs) |
| 0.2.1-beta | Jan 2026 | Enhanced token tracking, Sample 19, positioning refresh |
| 0.2.0-beta | Jan 2026 | Public beta, NuGet consumer sample, comprehensive documentation site |
| 0.1.x-alpha | Jan 2026 | Core evaluation toolkit, stochastic evaluation, model comparison, trace replay, security scanning |

See [CHANGELOG.md](https://github.com/joslat/AgentEval/blob/main/CHANGELOG.md) for detailed release notes.
