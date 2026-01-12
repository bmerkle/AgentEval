# ADR-001: Metric Naming Prefixes

**Status:** Proposed  
**Date:** 2026-01-07  
**Decision Makers:** AgentEval Contributors

---

## Context

AgentEval provides multiple types of metrics with different cost and computation characteristics:

| Current Metric | Computation | Cost Impact |
|---------------|-------------|-------------|
| `FaithfulnessMetric` | LLM prompt evaluation | $$$ per call |
| `RelevanceMetric` | LLM prompt evaluation | $$$ per call |
| `AnswerSimilarityMetric` | Embedding comparison | $ per call |
| `ToolSelectionMetric` | LLM prompt evaluation | $$$ per call |
| `ToolSuccessMetric` | Code logic (exception check) | Free |

**Problems:**

1. **Cost Opacity** — Users cannot tell at a glance which metrics cost money
2. **Selection Difficulty** — When optimizing for speed/cost, users must check implementation
3. **Industry Divergence** — Some evaluation frameworks prefix LLM metrics with `gpt_`, making cost obvious

**Forces:**

- Users want to minimize LLM API costs
- Some CI pipelines need fast, free metrics only
- Clear naming improves developer experience
- Breaking changes should be avoided if possible

---

## Decision

**Adopt metric name prefixes indicating computation type:**

| Prefix | Computation | Cost | Example |
|--------|-------------|------|---------|
| `llm_` | LLM prompt evaluation | $$$ | `llm_faithfulness`, `llm_relevance` |
| `embed_` | Embedding similarity | $ | `embed_answer_similarity` |
| `code_` | Pure code logic | Free | `code_tool_success`, `code_latency` |

**Implementation:**

1. Add `MetricName` property that returns prefixed name
2. `Name` property returns prefixed version
3. No backward compatibility concerns — library is pre-release with no production users

**Metric Renames:**

| Current Name | New Name | Type |
|--------------|----------|------|
| `Faithfulness` | `llm_faithfulness` | LLM |
| `Relevance` | `llm_relevance` | LLM |
| `ContextPrecision` | `llm_context_precision` | LLM |
| `ContextRecall` | `llm_context_recall` | LLM |
| `AnswerCorrectness` | `llm_answer_correctness` | LLM |
| `ToolSelection` | `llm_tool_selection` | LLM |
| `ToolArguments` | `llm_tool_arguments` | LLM |
| `TaskCompletion` | `llm_task_completion` | LLM |
| `AnswerSimilarity` | `embed_answer_similarity` | Embedding |
| `ResponseContextSimilarity` | `embed_response_context` | Embedding |
| `QueryContextSimilarity` | `embed_query_context` | Embedding |
| `ToolSuccess` | `code_tool_success` | Code |
| `ToolEfficiency` | `code_tool_efficiency` | Code |

---

## Consequences

### Positive

- **Cost Transparency** — Users immediately know which metrics cost money
- **Easy Filtering** — `metrics.Where(m => m.Name.StartsWith("code_"))` for free-only
- **Industry Alignment** — Matches industry conventions
- **Better UX** — Clearer intent in test output and reports

### Negative

- **Longer Names** — `llm_faithfulness` vs `Faithfulness`
- **Learning Curve** — Users must learn prefix meanings (mitigated by documentation)

### Neutral

- **No Breaking Changes** — Library is pre-release, no production users to migrate

---

## Alternatives Considered

### Alternative A: Keep Current Names
**Rejected** — Cost opacity remains a problem.

### Alternative B: Use `gpt_` Prefix
**Rejected** — `gpt_` is OpenAI-specific; AgentEval supports multiple LLM providers.

### Alternative C: Separate Namespaces
```csharp
AgentEval.Metrics.Llm.Faithfulness
AgentEval.Metrics.Embedding.AnswerSimilarity
AgentEval.Metrics.Code.ToolSuccess
```
**Rejected** — Doesn't help with output clarity; metric names in reports still ambiguous.

### Alternative D: Cost Property Instead of Prefix
```csharp
metric.ComputationCost // "llm", "embedding", "free"
```
**Considered but insufficient** — Doesn't appear in test output/reports.

---

## Implementation

1. Update `Name` property in all metric classes
2. Use `snake_case` for metric names (consistent with JSON output)
3. Update documentation and samples
4. No deprecation needed (pre-release)

---

## References

- [ai-rag-chat-evaluator metric naming](https://github.com/Azure-Samples/ai-rag-chat-evaluator) — Uses `gpt_` prefix
