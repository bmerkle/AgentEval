# AgentEval Launch & Development TODO

> **Created:** January 5, 2026  
> **Status:** Pre-release published to NuGet, ready for launch activities  
> **Version:** 1.0.0-alpha

---

## ✅ Completed (Reference)

These items are already implemented and tested (707 tests passing):

| Category | Features |
|----------|----------|
| **Core** | IMetric, ITestableAgent, IStreamableAgent, ITestHarness, IEvaluator |
| **Tool Tracking** | ToolCallRecord, ToolUsageReport, ToolCallTimeline with timing |
| **Assertions** | Fluent tool assertions, performance assertions, response assertions |
| **Streaming** | TTFT tracking, real-time callbacks, per-tool timing |
| **Cost Estimation** | ModelPricing with 8+ models, custom pricing support |
| **RAG Metrics** | Faithfulness, Relevance, ContextPrecision, ContextRecall, AnswerCorrectness |
| **Agentic Metrics** | ToolSelection, ToolArguments, ToolSuccess, TaskCompletion, ToolEfficiency |
| **Benchmarks** | PerformanceBenchmark, AgenticBenchmark with statistical analysis |
| **MAF Integration** | MAFAgentAdapter, MAFTestHarness, MAFEvaluatorAdapter |
| **CLI** | `agenteval eval`, `init`, `list` commands with NO_COLOR support |
| **Exporters** | JSON, JUnit XML, Markdown, TRX formats |
| **Dataset Loaders** | JSON, JSONL, CSV, YAML with field aliasing |
| **Snapshot Testing** | SnapshotComparer, SnapshotStore with JSON diff and regex scrubbing |
| **Multi-turn** | ConversationRunner, ConversationalTestCase, ConversationCompletenessMetric |
| **Workflow Testing** | Edge assertions, WorkflowSerializer (JSON/Mermaid/Timeline export) |
| **Testing Infrastructure** | FakeChatClient for mocking without external dependencies |
| **NuGet** | ✅ Pre-release published |
| **Logo** | ✅ Designed |

---

## 🚀 Phase A: Publication & Repository (COMPLETED)

- [x] NuGet workflow configured
- [x] Pre-release 1.0.0-alpha published
- [x] Logo designed
- [ ] Create standalone GitHub repo (`AgentEval/AgentEval`)
- [ ] Configure SourceLink + deterministic builds
- [ ] Add GitHub Actions CI/CD workflow (build, test, publish)
- [ ] Create CONTRIBUTING.md
- [ ] Create issue templates (bug, feature request)
- [ ] Add CODE_OF_CONDUCT.md
- [ ] Add SECURITY.md

---

## 📚 Phase B: Documentation Site (Priority: HIGH)

### B.1 DocFX Setup
- [ ] Run DocFX to generate API reference from XML docs
- [ ] Deploy docs to GitHub Pages
- [ ] Add version selector (for future versions)

### B.2 Core Documentation
- [x] **Getting Started** (5-min quickstart tutorial) — CRITICAL ✅ Created `docs/getting-started.md`
  - Install package
  - Create first test
  - Run with xUnit
  - See results
- [ ] **CI/CD Integration Guide**
  - GitHub Actions example
  - Azure DevOps example
  - JUnit export for CI
- [ ] **Migration Guide** (from other frameworks)
  - From DeepEval (Python)
  - From Promptfoo (Node.js)

### B.3 Feature Guides (enhance existing)
- [ ] Review and polish `architecture.md`
- [ ] Review and polish `benchmarks.md`
- [ ] Review and polish `cli.md`
- [ ] Review and polish `conversations.md`
- [ ] Review and polish `embeddings.md`
- [ ] Review and polish `extensibility.md`
- [ ] Review and polish `snapshots.md`
- [ ] Review and polish `workflows.md`

---

## 🎯 Phase C: Samples Enhancement (Priority: HIGH)

### C.1 New Samples
- [x] **Sample06_Benchmarks.cs** — PerformanceBenchmark + AgenticBenchmark usage ✅ Created
- [ ] **Sample07_CLIUsage.cs** — Dataset loading, batch evaluation, JUnit export
- [x] **Sample08_SnapshotTesting.cs** — Regression workflow with scrubbing ✅ Created (as Sample07)
- [x] **Sample09_ConversationTesting.cs** — Multi-turn with ConversationRunner ✅ Created (as Sample08)
- [ ] **Sample10_WorkflowTesting.cs** — Edge assertions, graph testing

### C.2 Sample Polish
- [ ] Add runnable "no Azure credentials needed" mode to all samples
- [ ] Ensure consistent formatting and structure
- [ ] Add "Expected Output" comments

---

## 📢 Phase D: Announce & Present (Priority: HIGH)

### D.1 Videos
- [ ] **Intro Video** (5-10 min) — What is AgentEval, why it matters
  - Problem: No .NET-native agent testing
  - Solution: AgentEval differentiators
  - Quick demo
- [ ] **Demo Video** (10-15 min) — Walk through samples
  - Sample 01-05 walkthrough
  - CLI usage
  - CI integration

### D.2 Written Content
- [ ] **Launch Blog Post** — Announce 1.0.0-alpha
  - Dev.to / Medium / personal blog
  - Cross-post to LinkedIn
- [ ] **Technical Deep-Dive Post** — Tool tracking + TTFT architecture

### D.3 Community Outreach
- [ ] Post to r/dotnet
- [ ] Post to Twitter/X (.NET community)
- [ ] Post to LinkedIn
- [ ] Submit to Hacker News
- [ ] Add to awesome-dotnet list
- [ ] Add to awesome-ai list
- [ ] .NET user group presentations
- [ ] Consider dotnetConf submission

---

## 🔧 Phase E: Missing Features (Priority: MEDIUM)

Based on competitor analysis, these features would significantly boost adoption:

### E.1 Record & Replay (Determinism)
> *"One of the most underappreciated differentiators in serious benchmarks"*

- [ ] Record tool calls + results + timing + model config to trace file
- [ ] Replay traces for deterministic testing (no external services)
- [ ] Optionally replay streaming chunks for TTFT/pacing tests
- [ ] Add `ITraceRecorder` and `ITraceReplayer` interfaces

### E.2 Experiment Management
> *"Without this, teams struggle to trust benchmark deltas"*

- [ ] Capture run metadata (model, temperature, top_p, seed, tool schema hash)
- [ ] Add tagging support (branch, commit SHA, environment)
- [ ] Compare runs (diff two TestSummary objects)
- [ ] Store prompt version hashes in `TestResult.Metadata`

### E.3 Chaos Testing for Tool Failures
> *"Agents break on retries, timeouts, partial results"*

- [ ] `IChaosPolicy` interface for tool failure injection
- [ ] Inject failures at configurable rate N%
- [ ] Inject latency spikes
- [ ] Drop tool results randomly
- [ ] Assert recovery behavior

### E.4 Stochastic Pass Criteria
> *"Matches how serious benchmarks report results"*

- [ ] Run test N times with different seeds
- [ ] Pass if success-rate ≥ X% and p95 latency ≤ Y
- [ ] Report confidence intervals
- [ ] Add `[StochasticTest(runs: 10, successRate: 0.8)]` attribute

### E.5 Judge Calibration
> *"If you rely on LLM-as-judge"*

- [ ] Support multi-judge voting (3 judges, majority wins)
- [ ] Calculate confidence intervals on scores
- [ ] Judge drift detection (baseline judge vs current)
- [ ] Add `CalibratedJudge` wrapper

### E.6 Behavioral Policy Assertions
> *"Agent unit tests, not just quality scoring"*

- [ ] `NeverCallTool("dangerous_tool")` assertion
- [ ] `NeverPassArgumentMatching(regex)` for PII detection
- [ ] `MustConfirmBefore("destructive_action")` assertion
- [ ] `MustCiteSources()` when tool provides docs

---

## 🏆 Phase F: Benchmark Suite Adapters (Priority: MEDIUM)

Enable running famous agentic benchmarks:

### F.1 Benchmark Adapter Architecture
- [ ] `IBenchmarkSuite` interface — yields tasks + knows how to score
- [ ] `IEnvironment` interface — optional (browser/docker)
- [ ] `IScorer` interface — deterministic or LLM-judge
- [ ] `BenchmarkRunner` — orchestrates suite execution

### F.2 Initial Benchmark Suites
- [ ] **ToolBenchSuite** — Tool-use correctness (good fit, minimal work)
- [ ] **GAIASuite** — General AI assistant tasks (moderate work)
- [ ] **BFCLSuite** — Berkeley Function Calling Leaderboard (exists in docs)

### F.3 Advanced Benchmark Suites (Future)
- [ ] **SWEbenchSuite** — Requires Docker + patch application
- [ ] **WebArenaSuite** — Requires Playwright environment
- [ ] **WorkArenaSuite** — Enterprise workflow tasks

---

## 🏢 Phase G: Enterprise Features (Priority: LOW)

### G.1 Observability
- [ ] OpenTelemetry integration for tracing
- [ ] Metrics export (Prometheus format)
- [ ] Structured logging with correlation IDs

### G.2 Results Persistence
- [ ] SQL Server result store
- [ ] Azure Cosmos DB result store
- [ ] Result query API

### G.3 Dashboard (Future)
- [ ] Web UI for viewing results
- [ ] Trend analysis
- [ ] Regression alerts

---

## 🛡️ Phase H: Safety & Red-Teaming (Priority: LOW)

### H.1 Red-Team Plugins
- [ ] Prompt injection detection
- [ ] Jailbreak attempt detection
- [ ] PII leakage detection
- [ ] Harmful content detection

### H.2 Safety Metrics
- [ ] `SafetyMetric` — LLM-judge for harmful outputs
- [ ] `BiasMetric` — Detect biased responses
- [ ] `ToxicityMetric` — Detect toxic language

---

## 🐛 Technical Debt & Optimizations

### Code Quality
- [ ] JSON source generators for performance
- [ ] Parallel metric evaluation
- [ ] Complete XML docs on all public APIs

### Developer Experience
- [ ] Roslyn analyzers for best practices
- [ ] Visual Studio test explorer integration
- [ ] VS Code extension (future)

### Testing
- [ ] Fuzz testing for parsing
- [ ] Load testing for benchmark runner
- [ ] Memory profiling

---

## 📅 Suggested Timeline

### Week 1-2: Documentation & Samples
Focus: Make it easy to adopt
- [ ] DocFX API reference
- [ ] Getting Started tutorial
- [ ] CI/CD integration guide
- [ ] Samples 06-10

### Week 3: Videos & Content
Focus: Build visibility
- [ ] Record intro video
- [ ] Record demo video
- [ ] Write launch blog post

### Week 4: Announce
Focus: Get users
- [ ] Publish videos
- [ ] Publish blog post
- [ ] Community outreach (Reddit, Twitter, LinkedIn)
- [ ] Submit to awesome lists

### Week 5-8: Missing Features
Focus: Competitive parity
- [ ] Record & Replay
- [ ] Experiment Management
- [ ] Chaos Testing
- [ ] Stochastic Pass Criteria

### Week 9+: Advanced Features
Focus: Enterprise & benchmarks
- [ ] Benchmark Suite Adapters
- [ ] Enterprise features
- [ ] Safety & Red-Teaming

---

## 📊 Success Metrics

| Metric | Target (3 months) | Target (6 months) |
|--------|-------------------|-------------------|
| NuGet downloads | 1,000 | 10,000 |
| GitHub stars | 100 | 500 |
| Contributors | 5 | 15 |
| Documentation pages | 15 | 25 |
| Video views | 1,000 | 5,000 |
| Issues resolved | 20 | 50 |

---

## 🔗 References

### Competitor Analysis
- [DeepEval](https://github.com/confident-ai/deepeval) — 14+ agentic metrics, red-teaming
- [Promptfoo](https://github.com/promptfoo/promptfoo) — YAML config, 30+ red-team plugins
- [RAGAS](https://github.com/explodinggradients/ragas) — RAG-specific metrics
- [BenchmarkLlm](https://github.com/dotnetagents/patterns) — .NET LLM benchmarking
- [Microsoft.Extensions.AI.Evaluation](https://github.com/dotnet/extensions) — Official .NET evaluators

### Agentic Benchmarks
- [SWE-bench](https://swebench.com) — GitHub issue resolution
- [WebArena](https://webarena.dev) — Web navigation
- [GAIA](https://arxiv.org/abs/2311.12983) — General AI assistant
- [ToolBench](https://github.com/OpenBMB/ToolBench) — Tool reasoning
- [AgentBench](https://github.com/THUDM/AgentBench) — Multi-environment

---

*Last Updated: January 5, 2026*
