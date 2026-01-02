# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Repository restructuring with standard .NET layout
- GitHub Actions CI/CD workflows for build, test, and NuGet publishing
- Central package management with `Directory.Packages.props`
- Shared build configuration with `Directory.Build.props`
- DocFX documentation scaffolding

---

## [0.1.0-alpha] - 2026-01-02

### Added

#### Core Framework
- First .NET-native AI agent testing, evaluation, and benchmarking framework
- Full Microsoft Agent Framework (MAF) integration via `MAFAgentAdapter` and `MAFTestHarness`
- Extensible adapter pattern supporting `IChatClient` and other frameworks
- Plugin system with `IAgentEvalPlugin` interface

#### Tool Usage Tracking & Assertions
- `ToolCallRecord` for capturing tool invocations with timing, arguments, results, and errors
- `ToolCallTimeline` for visualizing parallel tool execution
- Fluent assertions: `HaveCalledTool()`, `BeforeTool()`, `WithArgument()`, `HaveNoErrors()`
- Tool usage reports with success/failure metrics

#### Performance Metrics
- Real-time performance tracking with TTFT (Time To First Token)
- Per-tool timing and execution waterfall data
- Token counting (prompt/completion/total)
- Cost estimation for 8+ models (GPT-4o, GPT-4o-mini, Claude 3.5, Claude 3 Opus, GPT-4 Turbo, GPT-3.5 Turbo, o1-preview, o1-mini)
- Performance assertions: `HaveTotalDurationUnder()`, `HaveTimeToFirstTokenUnder()`, `HaveEstimatedCostUnder()`

#### RAG Metrics
- Faithfulness metric (grounded in context)
- Relevance metric (response addresses query)
- Context Precision metric
- Context Recall metric
- Answer Correctness metric

#### Agentic Metrics
- Tool Selection metric (chose appropriate tools)
- Tool Arguments metric (correct arguments passed)
- Tool Success metric (tools executed successfully)
- Task Completion metric (agent completed the task)
- Efficiency metric (minimal steps, tokens, time)

#### Benchmarks
- `PerformanceBenchmark` for latency/throughput/cost analysis
- `AgenticBenchmark` for multi-step agentic task evaluation
- Percentile statistics (p50, p90, p95, p99)
- Summary statistics (mean, min, max, standard deviation)

#### Testing Infrastructure
- `FakeChatClient` for zero-dependency unit testing
- `TestCase` model with inputs, expected outputs, evaluation criteria
- `TestResult` with comprehensive run data
- Trace-first failure reporting with structured diagnostics

#### Observability
- `IAgentEvalLogger` abstraction with console and Microsoft.Extensions.Logging adapters
- Run artifacts for debugging and "time travel" inspection
- Designed for OpenTelemetry (OTel) integration

### Technical Details
- 210+ unit tests with comprehensive coverage
- Multi-target framework support: .NET 8.0, 9.0, 10.0
- Zero-dependency core (optional integrations for MAF, Azure OpenAI)

---

## Future Releases

### Planned Packages
- `AgentEval` (core) ✅ This release
- `AgentEval.Maf` (MAF integration) - planned
- `AgentEval.TestKit` (fixtures/builders/helpers) - planned
- `AgentEval.Tracing` (OTel + run artifacts) - planned
- `AgentEval.Studio` (workflow visualizer / time-travel UI) - future

[Unreleased]: https://github.com/joslat/AgentEval/compare/v0.1.0-alpha...HEAD
[0.1.0-alpha]: https://github.com/joslat/AgentEval/releases/tag/v0.1.0-alpha
