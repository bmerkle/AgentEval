# Metrics Reference

> **Complete catalog of AgentEval evaluation metrics**

---

## Overview

AgentEval provides metrics across several categories to evaluate AI agents and RAG systems. Each metric is prefixed to indicate its computation method and cost.

### Metric Prefixes

| Prefix | Computation Method | Cost | Best For |
|--------|-------------------|------|----------|
| `llm_` | LLM-as-judge evaluation | $$ (API calls) | Detailed quality analysis, production sampling |
| `code_` | Computed by code logic | Free | CI/CD, unit tests, high-volume testing |
| `embed_` | Embedding similarity | $ (embedding API) | Cost-effective semantic checks |

---

## RAG Metrics

Metrics for evaluating Retrieval-Augmented Generation systems.

### llm_faithfulness

**Purpose:** Measures whether the response is grounded in the provided context (no hallucinations).

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ✅ Yes |
| Requires Ground Truth | ❌ No |
| Cost | LLM API call |

**When to Use:**
- Validating RAG systems don't hallucinate
- Ensuring responses are supported by retrieved documents
- Production quality checks

**Example:**
```csharp
var metric = new FaithfulnessMetric(chatClient);
var result = await metric.EvaluateAsync(new EvaluationContext
{
    Input = "What is the capital of France?",
    Output = "Paris is the capital of France.",
    Context = "France is a country in Europe. Its capital is Paris."
});
// result.Score: 100 (fully faithful)
```

---

### llm_relevance

**Purpose:** Measures how relevant the response is to the user's question.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ❌ No |
| Requires Ground Truth | ❌ No |
| Cost | LLM API call |

**When to Use:**
- Evaluating if responses address the question
- General quality checks for any LLM output
- Detecting off-topic responses

---

### llm_context_precision

**Purpose:** Measures how much of the retrieved context is actually useful for answering.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ✅ Yes |
| Requires Ground Truth | ❌ No |
| Cost | LLM API call |

**When to Use:**
- Optimizing retrieval quality
- Reducing context noise
- Improving retrieval efficiency

---

### llm_context_recall

**Purpose:** Measures whether the context contains all information needed for the correct answer.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ✅ Yes |
| Requires Ground Truth | ✅ Yes |
| Cost | LLM API call |

**When to Use:**
- Evaluating retrieval completeness
- Ensuring no critical information is missed
- Testing retrieval coverage

---

### llm_answer_correctness

**Purpose:** Measures factual correctness compared to ground truth.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ❌ No |
| Requires Ground Truth | ✅ Yes |
| Cost | LLM API call |

**When to Use:**
- QA system evaluation
- Fact-checking responses
- Regression testing with known answers

---

## Embedding Metrics

Fast, cost-effective semantic similarity metrics using embeddings.

### embed_answer_similarity

**Purpose:** Measures semantic similarity between response and ground truth.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ❌ No |
| Requires Ground Truth | ✅ Yes |
| Cost | Embedding API call |

**When to Use:**
- Fast similarity checks at scale
- When exact wording doesn't matter
- Cost-effective alternative to `llm_answer_correctness`

**Limitations:**
- Cannot detect subtle factual errors
- May miss negation differences

---

### embed_response_context

**Purpose:** Measures how semantically grounded the response is in the context.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ✅ Yes |
| Requires Ground Truth | ❌ No |
| Cost | Embedding API call |

**When to Use:**
- Fast grounding check
- Pre-filter before expensive LLM evaluation

---

### embed_query_context

**Purpose:** Measures relevance of retrieved context to the query.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ✅ Yes |
| Requires Ground Truth | ❌ No |
| Cost | Embedding API call |

**When to Use:**
- Evaluating retrieval relevance
- Optimizing search/retrieval systems

---

## Agentic Metrics

Metrics for evaluating tool-using AI agents.

### code_tool_selection

**Purpose:** Measures whether the agent selected the correct tools.

| Property | Value |
|----------|-------|
| Interface | `IAgenticMetric` |
| Requires Tool Usage | ✅ Yes |
| Cost | Free (code-based) |

**When to Use:**
- CI/CD pipeline tests
- Validating tool selection logic
- High-volume testing

**Example:**
```csharp
var metric = new ToolSelectionMetric(
    expectedTools: ["SearchTool", "CalculatorTool"],
    strictOrder: false);

var result = await metric.EvaluateAsync(context);
```

---

### code_tool_arguments

**Purpose:** Validates tool arguments are complete and correct.

| Property | Value |
|----------|-------|
| Interface | `IAgenticMetric` |
| Requires Tool Usage | ✅ Yes |
| Cost | Free (code-based) |

**When to Use:**
- Ensuring required parameters are provided
- Validating argument formats

---

### code_tool_success

**Purpose:** Measures tool execution success rate.

| Property | Value |
|----------|-------|
| Interface | `IAgenticMetric` |
| Requires Tool Usage | ✅ Yes |
| Cost | Free (code-based) |

**When to Use:**
- Monitoring agent reliability
- Detecting tool failures

---

### code_tool_efficiency

**Purpose:** Measures efficiency of tool usage (call count, retries, duration).

| Property | Value |
|----------|-------|
| Interface | `IAgenticMetric` |
| Requires Tool Usage | ✅ Yes |
| Cost | Free (code-based) |

**When to Use:**
- Optimizing agent efficiency
- Detecting excessive retries
- Cost optimization

---

### llm_task_completion

**Purpose:** Evaluates whether the agent completed the requested task.

| Property | Value |
|----------|-------|
| Interface | `IAgenticMetric` |
| Requires Tool Usage | ❌ No |
| Cost | LLM API call |

**When to Use:**
- End-to-end task validation
- Complex multi-step task evaluation

---

## Conversation Metrics

Metrics for multi-turn conversation evaluation.

### ConversationCompleteness

**Purpose:** Evaluates multi-turn conversation quality including response rate, tool usage, and flow.

| Property | Value |
|----------|-------|
| Type | Special (non-IMetric) |
| Requires | ConversationResult |
| Cost | Free (code-based) |

**Sub-scores:**
- ResponseRate (40%) - Did every turn get a response?
- ToolUsage (30%) - Were expected tools called?
- DurationCompliance (15%) - Within time limits?
- ErrorFree (15%) - No errors occurred?

---

## Quick Reference Table

| Metric | Category | Context | Ground Truth | Tool Usage | Cost |
|--------|----------|---------|--------------|------------|------|
| `llm_faithfulness` | RAG | ✅ | ❌ | ❌ | LLM |
| `llm_relevance` | RAG | ❌ | ❌ | ❌ | LLM |
| `llm_context_precision` | RAG | ✅ | ❌ | ❌ | LLM |
| `llm_context_recall` | RAG | ✅ | ✅ | ❌ | LLM |
| `llm_answer_correctness` | RAG | ❌ | ✅ | ❌ | LLM |
| `embed_answer_similarity` | Embedding | ❌ | ✅ | ❌ | Embed |
| `embed_response_context` | Embedding | ✅ | ❌ | ❌ | Embed |
| `embed_query_context` | Embedding | ✅ | ❌ | ❌ | Embed |
| `code_tool_selection` | Agentic | ❌ | ❌ | ✅ | Free |
| `code_tool_arguments` | Agentic | ❌ | ❌ | ✅ | Free |
| `code_tool_success` | Agentic | ❌ | ❌ | ✅ | Free |
| `code_tool_efficiency` | Agentic | ❌ | ❌ | ✅ | Free |
| `llm_task_completion` | Agentic | ❌ | ❌ | ❌ | LLM |

---

## See Also

- [Evaluation Guide](evaluation-guide.md) - How to choose the right metrics
- [Naming Conventions](naming-conventions.md) - Metric naming standards
- [Architecture](architecture.md) - System design

---

*Last updated: January 2026*
