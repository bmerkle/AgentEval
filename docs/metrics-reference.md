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

## Information Retrieval Metrics

Code-based metrics for evaluating retrieval quality in RAG systems. These are **FREE** (no API calls) and fast.

### code_recall_at_k

**Purpose:** Measures what proportion of relevant documents were found in the top K retrieved results.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ❌ No |
| Requires Ground Truth | ❌ No |
| Requires Document IDs | ✅ Yes (`RelevantDocumentIds`, `RetrievedDocumentIds`) |
| Cost | Free (code-based) |
| Formula | `Recall@K = |Relevant ∩ Retrieved@K| / |Relevant|` |

**When to Use:**
- Evaluating retrieval coverage in RAG pipelines
- CI/CD testing of vector search
- Optimizing retrieval parameters (K value)
- High-volume testing (free - no API costs)

**Parameters:**
- `k` - Number of top results to consider (default: 10)
- `passThreshold` - Minimum score to pass (default: 0.7 = 70%)

**Example:**
```csharp
var metric = new RecallAtKMetric(k: 5);
var result = await metric.EvaluateAsync(new EvaluationContext
{
    RelevantDocumentIds = ["doc1", "doc2", "doc3"],
    RetrievedDocumentIds = ["doc1", "doc4", "doc2", "doc5", "doc6"]
});
// Score: 67 (2 of 3 relevant docs found in top 5)
```

**AdditionalData:**
- `k` - K value used
- `relevant_count` - Total relevant documents
- `retrieved_at_k` - Documents retrieved at K
- `relevant_found_at_k` - Relevant documents found

---

### code_mrr

**Purpose:** Mean Reciprocal Rank - measures how early the first relevant document appears in results.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric` |
| Requires Context | ❌ No |
| Requires Ground Truth | ❌ No |
| Requires Document IDs | ✅ Yes (`RelevantDocumentIds`, `RetrievedDocumentIds`) |
| Cost | Free (code-based) |
| Formula | `MRR = 1 / rank_of_first_relevant` |

**When to Use:**
- Evaluating retrieval ranking quality
- User experience optimization (users want relevant docs first)
- Search result quality assessment
- CI/CD testing of ranking algorithms

**Parameters:**
- `maxRank` - Maximum rank to consider (default: unlimited, 0 = unlimited)
- `passThreshold` - Minimum score to pass (default: 0.33 = first relevant in top 3)

**Scoring:**
- First relevant at rank 1 → Score = 100 (1/1)
- First relevant at rank 2 → Score = 50 (1/2)
- First relevant at rank 3 → Score = 33 (1/3)
- First relevant at rank 10 → Score = 10 (1/10)
- No relevant found → Score = 0

**Example:**
```csharp
var metric = new MRRMetric(maxRank: 10);
var result = await metric.EvaluateAsync(new EvaluationContext
{
    RelevantDocumentIds = ["doc1", "doc2"],
    RetrievedDocumentIds = ["docA", "docB", "doc1", "docC"]  // doc1 at rank 3
});
// Score: 33 (1/3 = 0.333)
```

**AdditionalData:**
- `first_relevant_rank` - Position of first relevant doc (0 if none found)
- `max_rank` - Maximum rank considered (0 = unlimited)
- `reciprocal_rank` - The 1/rank value

---

## Quality Metrics

Metrics for evaluating response quality, safety, and language characteristics.

### llm_groundedness

**Purpose:** Measures whether the response is grounded and avoids unsubstantiated claims, fabricated sources, or presenting speculation as fact.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric`, `ISafetyMetric` |
| Requires Context | ✅ Yes |
| Requires Ground Truth | ❌ No |
| Cost | LLM API call |

**When to Use:**
- Safety validation for AI responses
- Detecting fabricated sources, citations, or statistics
- Ensuring claims are substantiated by context
- High-stakes applications requiring factual accuracy

**Example:**
```csharp
var metric = new GroundednessMetric(chatClient);
var result = await metric.EvaluateAsync(new EvaluationContext
{
    Input = "What are the sales numbers?",
    Output = "According to our Q3 report, sales increased 15%.",
    Context = "Q3 sales report shows 15% growth year-over-year."
});
// result.Score: High (claim supported by context)
```

---

### llm_coherence

**Purpose:** Measures the logical coherence and internal consistency of the response.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric`, `IQualityMetric` |
| Requires Context | ❌ No |
| Requires Ground Truth | ❌ No |
| Cost | LLM API call |

**When to Use:**
- Detecting self-contradictions in responses
- Evaluating logical flow and structure
- Quality assurance for complex, multi-part responses
- Ensuring ideas connect naturally

**Example:**
```csharp
var metric = new CoherenceMetric(chatClient);
var result = await metric.EvaluateAsync(new EvaluationContext
{
    Input = "Explain the process",
    Output = "First, prepare the ingredients. The final step is preparation..."
});
// result.Score: Low (contradictory: preparation both first and last)
```

---

### llm_fluency

**Purpose:** Measures grammar, readability, and natural language quality of the response.

| Property | Value |
|----------|-------|
| Interface | `IRAGMetric`, `IQualityMetric` |
| Requires Context | ❌ No |
| Requires Ground Truth | ❌ No |
| Cost | LLM API call |

**When to Use:**
- Grammar and style checking
- Readability assessment
- Language quality assurance
- Customer-facing content validation

**Example:**
```csharp
var metric = new FluencyMetric(chatClient);
var result = await metric.EvaluateAsync(new EvaluationContext
{
    Output = "The product is very good and works excellent for many purpose."
});
// result.Score: Moderate (minor grammar issues)
```

---

## Workflow Metrics

Metrics for evaluating multi-agent workflows and complex execution pipelines.

### code_workflow_structure_validity

**Purpose:** Validates workflow graph structure (nodes, edges, entry/exit points).

| Property | Value |
|----------|-------|
| Interface | `IWorkflowMetric` |
| Requires Workflow Graph | ✅ Yes |
| Cost | Free (code validation) |

**When to Use:**
- Ensuring workflow topology is correct
- Validating graph connectivity
- CI/CD workflow validation
- Detecting broken workflow definitions

**Example:**
```csharp
var metric = new WorkflowStructureValidityMetric();
var result = await metric.EvaluateAsync(new WorkflowEvaluationContext
{
    WorkflowGraph = workflowDefinition,
    ExpectedNodes = new[] { "Planner", "Writer", "Editor" }
});
// result.Score: 100 (valid structure)
```

---

### code_workflow_execution_order

**Purpose:** Verifies agents executed in the expected order.

| Property | Value |
|----------|-------|
| Interface | `IWorkflowMetric` |
| Requires Execution Result | ✅ Yes |
| Cost | Free (sequence validation) |

**When to Use:**
- Sequential pipeline validation
- Dependency checking
- Ensuring proper workflow orchestration
- Debugging execution flow

**Example:**
```csharp
var metric = new WorkflowExecutionOrderMetric(
    expectedOrder: new[] { "Planner", "Researcher", "Writer", "Editor" }
);
var result = await metric.EvaluateAsync(context);
// result.Score: Based on sequence correctness
```

---

### code_workflow_executor_success

**Purpose:** Measures the success rate of individual executors within the workflow.

| Property | Value |
|----------|-------|
| Interface | `IWorkflowMetric` |
| Requires Executor Results | ✅ Yes |
| Cost | Free (success rate calculation) |

**When to Use:**
- Per-agent reliability tracking
- Identifying problematic workflow steps
- Quality assurance for complex pipelines
- Performance monitoring

**Example:**
```csharp
var metric = new WorkflowExecutorSuccessMetric();
var result = await metric.EvaluateAsync(context);
// result.Score: Percentage of successful executor completions
```

---

### llm_workflow_output_quality

**Purpose:** LLM-based evaluation of final workflow output quality.

| Property | Value |
|----------|-------|
| Interface | `IWorkflowMetric` |
| Requires Workflow Output | ✅ Yes |
| Cost | LLM API call |

**When to Use:**
- End-to-end workflow quality assessment
- Complex pipeline output validation
- Production quality gates
- Multi-agent collaboration evaluation

**Example:**
```csharp
var metric = new WorkflowOutputQualityMetric(chatClient, criteria: 
    "Evaluate if the content creation workflow produced high-quality, coherent output"
);
var result = await metric.EvaluateAsync(context);
// result.Score: LLM assessment of final output quality
```

---

### code_workflow_tool_chain_validity

**Purpose:** Validates tool usage patterns across multiple agents in the workflow.

| Property | Value |
|----------|-------|
| Interface | `IWorkflowMetric` |
| Requires Tool Usage | ✅ Yes |
| Cost | Free (tool pattern analysis) |

**When to Use:**
- Multi-agent tool coordination
- Tool dependency validation
- Complex tool chain verification
- Cross-agent tool state management

**Example:**
```csharp
var metric = new WorkflowToolChainValidityMetric(
    requiredPatterns: new[] {
        new ToolPattern("Search", beforeTool: "Book"),
        new ToolPattern("Confirm", beforeTool: "Execute")
    }
);
var result = await metric.EvaluateAsync(context);
// result.Score: Tool chain pattern compliance
```

---

### code_workflow_performance_efficiency

**Purpose:** Measures overall workflow performance (timing, cost, resource usage).

| Property | Value |
|----------|-------|
| Interface | `IWorkflowMetric` |
| Requires Performance Data | ✅ Yes |
| Cost | Free (performance calculation) |

**When to Use:**
- Workflow optimization
- Cost monitoring across multiple agents
- SLA validation for complex pipelines
- Performance regression detection

**Example:**
```csharp
var metric = new WorkflowPerformanceEfficiencyMetric(
    maxDuration: TimeSpan.FromMinutes(5),
    maxTotalCost: 0.50m
);
var result = await metric.EvaluateAsync(context);
// result.Score: Based on performance vs. thresholds
```

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
| `code_recall_at_k` | IR | ❌ | ❌ | ❌ | Free |
| `code_mrr` | IR | ❌ | ❌ | ❌ | Free |
| `code_tool_selection` | Agentic | ❌ | ❌ | ✅ | Free |
| `code_tool_arguments` | Agentic | ❌ | ❌ | ✅ | Free |
| `code_tool_success` | Agentic | ❌ | ❌ | ✅ | Free |
| `code_tool_efficiency` | Agentic | ❌ | ❌ | ✅ | Free |
| `llm_task_completion` | Agentic | ❌ | ❌ | ❌ | LLM |
| `llm_groundedness` | Quality | ✅ | ❌ | ❌ | LLM |
| `llm_coherence` | Quality | ❌ | ❌ | ❌ | LLM |
| `llm_fluency` | Quality | ❌ | ❌ | ❌ | LLM |
| `code_workflow_structure_validity` | Workflow | ❌ | ❌ | ❌ | Free |
| `code_workflow_execution_order` | Workflow | ❌ | ❌ | ❌ | Free |
| `code_workflow_executor_success` | Workflow | ❌ | ❌ | ❌ | Free |
| `llm_workflow_output_quality` | Workflow | ❌ | ❌ | ❌ | LLM |
| `code_workflow_tool_chain_validity` | Workflow | ❌ | ❌ | ✅ | Free |
| `code_workflow_performance_efficiency` | Workflow | ❌ | ❌ | ❌ | Free |

---

## See Also

- [RAG Metrics](rag-metrics.md) - Complete RAG evaluation guide
- [Workflows](workflows.md) - Multi-agent workflow evaluation guide
- [Evaluation Guide](evaluation-guide.md) - How to choose the right metrics
- [Naming Conventions](naming-conventions.md) - Metric naming standards
- [Architecture](architecture.md) - System design

---

*Last updated: January 2026*
