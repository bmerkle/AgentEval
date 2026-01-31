# Workflow Evaluation

> **Comprehensive guide to evaluating multi-agent workflows with AgentEval**

AgentEval provides first-class support for evaluating multi-agent workflows, including graph-based execution, conditional routing, parallel branches, and edge traversal assertions.

## Overview

Modern AI applications often orchestrate multiple agents in complex workflows:
- **Sequential chains**: One agent's output feeds the next
- **Conditional routing**: Different agents handle different scenarios
- **Parallel execution**: Multiple agents work simultaneously
- **Switch patterns**: Route to specific handlers based on classification

AgentEval captures the full execution graph, enabling you to:
- Assert on which edges were traversed
- Verify routing decisions
- Evaluate parallel branch completion
- Replay and visualize workflow execution

## Quick Start

```csharp
using AgentEval.MAF;
using AgentEval.Models;
using AgentEval.Assertions;

// Create a workflow adapter
var adapter = MAFWorkflowAdapter.FromSteps(
    "support-workflow",
    ("classifier", "This is a billing inquiry"),
    ("billing-handler", "I'll help with your billing question"),
    ("response-formatter", "Here's your formatted response"));

// Execute the workflow
var result = await adapter.ExecuteWorkflowAsync("I need help with my bill");

// Assert on execution
result.Should()
    .HaveStepCount(3)
    .HaveExecutedStep("classifier")
    .HaveExecutedStep("billing-handler")
    .HaveTraversedEdge("classifier", "billing-handler");
```

## Core Concepts

### Workflow Execution Result

The `WorkflowExecutionResult` captures everything about a workflow run:

```csharp
public record WorkflowExecutionResult
{
    // Final aggregated output
    public required string FinalOutput { get; init; }
    
    // Individual steps executed
    public required IReadOnlyList<ExecutorStep> Steps { get; init; }
    
    // Total execution time
    public TimeSpan TotalDuration { get; init; }
    
    // Graph structure and traversed edges
    public WorkflowGraphSnapshot? Graph { get; init; }
    
    // Routing decisions made during execution
    public IReadOnlyList<RoutingDecision>? RoutingDecisions { get; init; }
    
    // Helper properties
    public bool HasConditionalRouting { get; }
    public bool HasParallelExecution { get; }
    public IEnumerable<string> GetExecutionPath() { }
}
```

### Executor Steps

Each step in the workflow is captured as an `ExecutorStep`:

```csharp
public record ExecutorStep
{
    // Identification
    public required string ExecutorId { get; init; }
    public string? ExecutorName { get; init; }
    
    // Output
    public required string Output { get; init; }
    
    // Timing
    public TimeSpan StartOffset { get; init; }
    public TimeSpan Duration { get; init; }
    public int StepIndex { get; init; }
    
    // Edge information
    public EdgeExecution? IncomingEdge { get; init; }
    public IReadOnlyList<EdgeExecution>? OutgoingEdges { get; init; }
    
    // Parallel execution
    public string? ParallelBranchId { get; init; }
    public bool IsParallelBranch { get; }
    public bool WasConditionallyRouted { get; }
    
    // Tool tracking
    public IReadOnlyList<ToolCallRecord>? ToolCalls { get; init; }
}
```

### Edge Types

AgentEval supports 8 different edge types:

| Edge Type | Description | Use Case |
|-----------|-------------|----------|
| `Sequential` | Direct linear flow | A → B → C |
| `Conditional` | Flow based on condition evaluation | If approved → proceed |
| `Switch` | Route to one of many targets based on value | Classify → handler |
| `ParallelFanOut` | Split to multiple parallel branches | Orchestrator → workers |
| `ParallelFanIn` | Merge parallel branches | Workers → aggregator |
| `Loop` | Return to previous step | Retry logic |
| `Error` | Error handling path | Handler → error-handler |
| `Terminal` | Exit the workflow | Final step |

## Creating Workflow Adapters

### From Predefined Steps

The simplest way to create a testable workflow:

```csharp
// Sequential workflow with 3 steps
var adapter = MAFWorkflowAdapter.FromSteps(
    "my-workflow",
    ("step-1", "output from step 1"),
    ("step-2", "output from step 2"),
    ("step-3", "final output"));
```

### With Conditional Edges

For workflows with routing logic:

```csharp
var adapter = MAFWorkflowAdapter.FromConditionalSteps(
    "conditional-workflow",
    steps: [
        ("classifier", "billing"),
        ("billing-handler", "Handled billing request"),
        ("tech-handler", "Handled tech request")
    ],
    edges: [
        ("classifier", "billing-handler", EdgeType.Conditional, "output.Contains('billing')"),
        ("classifier", "tech-handler", EdgeType.Conditional, "output.Contains('tech')")
    ]);
```

### With Predefined Graph

For complex workflows with full graph control:

```csharp
var graph = new WorkflowGraphSnapshot
{
    Nodes = [
        new WorkflowNode { NodeId = "router", IsEntryPoint = true },
        new WorkflowNode { NodeId = "handler-a" },
        new WorkflowNode { NodeId = "handler-b" },
        new WorkflowNode { NodeId = "merger", IsExitNode = true }
    ],
    Edges = [
        new WorkflowEdge { 
            EdgeId = "e1", 
            SourceExecutorId = "router", 
            TargetExecutorId = "handler-a", 
            EdgeType = EdgeType.ParallelFanOut 
        },
        new WorkflowEdge { 
            EdgeId = "e2", 
            SourceExecutorId = "router", 
            TargetExecutorId = "handler-b", 
            EdgeType = EdgeType.ParallelFanOut 
        },
        new WorkflowEdge { 
            EdgeId = "e3", 
            SourceExecutorId = "handler-a", 
            TargetExecutorId = "merger", 
            EdgeType = EdgeType.ParallelFanIn 
        },
        new WorkflowEdge { 
            EdgeId = "e4", 
            SourceExecutorId = "handler-b", 
            TargetExecutorId = "merger", 
            EdgeType = EdgeType.ParallelFanIn 
        }
    ],
    EntryNodeId = "router",
    ExitNodeIds = ["merger"]
};

var adapter = MAFWorkflowAdapter.WithGraph(
    "parallel-workflow",
    graph,
    MyWorkflowExecutor);
```

### Custom Workflow Executor

For full control, provide a custom executor function:

```csharp
static async IAsyncEnumerable<WorkflowEvent> MyWorkflowExecutor(
    string prompt, 
    [EnumeratorCancellation] CancellationToken ct)
{
    // Emit events as the workflow executes
    yield return new ExecutorOutputEvent("step-1", "processing...");
    yield return new EdgeTraversedEvent("step-1", "step-2", EdgeType.Sequential);
    yield return new ExecutorOutputEvent("step-2", "completed");
    yield return new WorkflowCompleteEvent();
}
```

## Workflow Events

The workflow adapter recognizes these event types:

| Event | Purpose |
|-------|---------|
| `ExecutorOutputEvent` | Step produced output |
| `EdgeTraversedEvent` | Edge was traversed |
| `RoutingDecisionEvent` | Routing decision was made |
| `ParallelBranchStartEvent` | Parallel branch started |
| `ParallelBranchEndEvent` | Parallel branch completed |
| `WorkflowCompleteEvent` | Workflow finished |
| `WorkflowErrorEvent` | Error occurred |

### Event Examples

```csharp
// Simple output
yield return new ExecutorOutputEvent("agent-id", "output text");

// Conditional edge traversal
yield return new EdgeTraversedEvent(
    sourceExecutorId: "router",
    targetExecutorId: "handler",
    edgeType: EdgeType.Conditional,
    conditionResult: true,
    routingReason: "Output matched billing pattern");

// Routing decision (switch pattern)
yield return new RoutingDecisionEvent(
    deciderExecutorId: "classifier",
    possibleEdgeIds: ["billing", "tech", "general"],
    selectedEdgeId: "billing",
    evaluatedValue: "billing inquiry",
    selectionReason: "Matched billing keywords");

// Parallel execution
yield return new ParallelBranchStartEvent("branch-1", ["worker-a", "worker-b"]);
yield return new ExecutorOutputEvent("worker-a", "result A");
yield return new ExecutorOutputEvent("worker-b", "result B");
yield return new ParallelBranchEndEvent(
    branchId: "branch-1",
    executorIds: ["worker-a", "worker-b"],
    startTime: TimeSpan.Zero,
    endTime: TimeSpan.FromSeconds(2),
    isSuccess: true,
    output: "merged results");
```

## Workflow Assertions

### Basic Step Assertions

```csharp
result.Should()
    .HaveStepCount(3)
    .HaveExecutedStep("classifier")
    .HaveExecutedStep("handler")
    .HaveNoErrors();
```

### Edge Assertions

```csharp
result.Should()
    .HaveGraphStructure()
    .HaveTraversedEdge("classifier", "billing-handler")
    .NotHaveTraversedEdge("classifier", "tech-handler");
```

### Conditional Routing Assertions

```csharp
result.Should()
    .HaveConditionalRouting()
    .HaveRoutingDecision("classifier", "edge-billing");
```

### Parallel Execution Assertions

```csharp
result.Should()
    .HaveParallelExecution()
    .HaveParallelBranch("branch-1")
    .HaveCompletedAllParallelBranches();
```

### Edge-Level Assertions

For detailed edge verification:

```csharp
result.Should()
    .ForEdge("classifier", "billing-handler")
        .BeOfType(EdgeType.Conditional)
        .HaveConditionResult(true)
        .HaveRoutingReason("matched billing keywords")
    .And()
    .ForEdge("billing-handler", "response-formatter")
        .BeOfType(EdgeType.Sequential);
```

### Step-Level Assertions

```csharp
result.Should()
    .ForStep("classifier")
        .HaveOutput("billing inquiry")
        .HaveDurationUnder(TimeSpan.FromSeconds(5))
        .NotBeParallelBranch()
    .And()
    .ForStep("parallel-worker")
        .BeInParallelBranch("branch-1")
        .HaveToolCall("ProcessData");
```

### Execution Path Assertions

```csharp
result.Should()
    .HaveExecutionPath(["classifier", "billing-handler", "formatter"])
    .HaveExecutionPathContaining("billing-handler");
```

## JSON Export for Visualization

AgentEval can export workflow execution to JSON for visualization tools:

```csharp
using AgentEval.Models.Serialization;

// Execute workflow
var result = await adapter.ExecuteWorkflowAsync("test prompt");

// Export for visualization
var json = WorkflowSerializer.ToJson(result);
File.WriteAllText("workflow-trace.json", json);

// Export just the graph structure
var graphJson = WorkflowSerializer.ToGraphJson(result);

// Export for Mermaid diagram generation
var mermaid = WorkflowSerializer.ToMermaid(result);
```

### JSON Structure

The exported JSON includes both the static graph structure and the dynamic execution trace:

```json
{
  "workflowId": "support-workflow",
  "executedAt": "2026-01-05T10:30:00Z",
  "totalDuration": "00:00:02.345",
  "finalOutput": "Here's your formatted response",
  
  "graph": {
    "nodes": [
      { "nodeId": "classifier", "isEntryPoint": true },
      { "nodeId": "billing-handler" },
      { "nodeId": "response-formatter", "isExitNode": true }
    ],
    "edges": [
      { "edgeId": "e1", "source": "classifier", "target": "billing-handler", "type": "Conditional" },
      { "edgeId": "e2", "source": "billing-handler", "target": "response-formatter", "type": "Sequential" }
    ]
  },
  
  "executionTrace": {
    "steps": [
      {
        "stepIndex": 0,
        "executorId": "classifier",
        "output": "This is a billing inquiry",
        "startOffset": "00:00:00",
        "duration": "00:00:00.500",
        "incomingEdge": null
      },
      {
        "stepIndex": 1,
        "executorId": "billing-handler",
        "output": "I'll help with your billing question",
        "startOffset": "00:00:00.500",
        "duration": "00:00:01.200",
        "incomingEdge": {
          "source": "classifier",
          "target": "billing-handler",
          "type": "Conditional",
          "conditionResult": true,
          "routingReason": "matched billing keywords"
        }
      }
    ],
    "traversedEdges": [
      {
        "source": "classifier",
        "target": "billing-handler",
        "traversedAt": "00:00:00.500",
        "type": "Conditional"
      }
    ],
    "routingDecisions": [
      {
        "decider": "classifier",
        "possibleEdges": ["billing-handler", "tech-handler", "general-handler"],
        "selectedEdge": "billing-handler",
        "reason": "matched billing keywords"
      }
    ]
  }
}
```

### Mermaid Diagram Export

Generate flowchart diagrams:

```csharp
var mermaid = WorkflowSerializer.ToMermaid(result);
// Output:
// ```mermaid
// graph TD
//     classifier([classifier])
//     billing-handler([billing-handler])
//     response-formatter([response-formatter])
//     
//     classifier -->|Conditional| billing-handler
//     billing-handler --> response-formatter
//     
//     classDef executed fill:#90EE90
//     class classifier,billing-handler,response-formatter executed
// ```
```

## Time-Travel Debugging

The execution trace captures timing for every step and edge, enabling replay:

```csharp
// Get execution timeline
foreach (var step in result.Steps.OrderBy(s => s.StartOffset))
{
    Console.WriteLine($"[{step.StartOffset}] {step.ExecutorId} started");
    Console.WriteLine($"[{step.StartOffset + step.Duration}] {step.ExecutorId} completed");
}

// Replay edges
foreach (var edge in result.Graph?.TraversedEdges ?? [])
{
    Console.WriteLine($"[{edge.TraversedAt}] {edge.SourceExecutorId} → {edge.TargetExecutorId}");
}
```

## Evaluation Patterns

### Evaluate Sequential Workflow

```csharp
[Fact]
public async Task SequentialWorkflow_Should_ExecuteInOrder()
{
    var adapter = MAFWorkflowAdapter.FromSteps(
        "pipeline",
        ("extract", "data extracted"),
        ("transform", "data transformed"),
        ("load", "data loaded"));

    var result = await adapter.ExecuteWorkflowAsync("process data");

    result.Should()
        .HaveStepCount(3)
        .HaveExecutionPath(["extract", "transform", "load"])
        .HaveTraversedEdge("extract", "transform")
        .HaveTraversedEdge("transform", "load");
}
```

### Evaluate Conditional Routing

```csharp
[Fact]
public async Task ConditionalWorkflow_Should_RouteCorrectly()
{
    var adapter = MAFWorkflowAdapter.FromConditionalSteps(
        "router",
        [("classifier", "billing"), ("billing-agent", "handled")],
        [("classifier", "billing-agent", EdgeType.Conditional, null)]);

    var result = await adapter.ExecuteWorkflowAsync("I need billing help");

    result.Should()
        .HaveConditionalRouting()
        .HaveExecutedStep("billing-agent")
        .ForEdge("classifier", "billing-agent")
            .BeOfType(EdgeType.Conditional)
            .HaveConditionResult(true);
}
```

### Evaluate Parallel Execution

```csharp
[Fact]
public async Task ParallelWorkflow_Should_ExecuteConcurrently()
{
    var adapter = new MAFWorkflowAdapter("parallel-test", EmitParallelEvents);

    var result = await adapter.ExecuteWorkflowAsync("parallel task");

    result.Should()
        .HaveParallelExecution()
        .HaveParallelBranch("branch-1")
        .HaveCompletedAllParallelBranches();
    
    var branch = result.Graph?.ParallelBranches?.First();
    Assert.Contains("worker-a", branch!.ExecutorIds);
    Assert.Contains("worker-b", branch.ExecutorIds);
}
```

### Evaluate Error Handling

```csharp
[Fact]
public async Task Workflow_Should_CaptureErrors()
{
    var adapter = new MAFWorkflowAdapter("error-test", EmitErrorEvents);

    var result = await adapter.ExecuteWorkflowAsync("will fail");

    Assert.False(result.IsSuccess);
    Assert.NotNull(result.Errors);
    Assert.Contains(result.Errors, e => e.ExecutorId == "failing-step");
}
```

## Best Practices

### 1. Use Descriptive Executor IDs

```csharp
// Good
("intent-classifier", "...")
("billing-inquiry-handler", "...")

// Avoid
("step1", "...")
("agent", "...")
```

### 2. Test Edge Types Explicitly

```csharp
result.Should()
    .ForEdge("classifier", "handler")
        .BeOfType(EdgeType.Conditional);  // Be specific about edge type
```

### 3. Assert on Timing for Performance

```csharp
result.Should()
    .ForStep("slow-step")
        .HaveDurationUnder(TimeSpan.FromSeconds(5));

Assert.True(result.TotalDuration < TimeSpan.FromSeconds(30));
```

### 4. Use Parallel Assertions Carefully

```csharp
// Verify all parallel branches completed
result.Should().HaveCompletedAllParallelBranches();

// Check specific branch
result.Should()
    .HaveParallelBranch("branch-1")
    .ForStep("worker-a")
        .BeInParallelBranch("branch-1");
```

### 5. Export for Debugging

```csharp
// In test setup/teardown
[Fact]
public async Task ComplexWorkflow_Test()
{
    var result = await adapter.ExecuteWorkflowAsync("test");
    
    // Export on failure for debugging
    if (!result.IsSuccess)
    {
        var json = WorkflowSerializer.ToJson(result);
        File.WriteAllText($"failed-workflow-{DateTime.Now:yyyyMMdd-HHmmss}.json", json);
    }
    
    result.Should().HaveNoErrors();
}
```

## Integration with CI/CD

Export workflow results for CI/CD visibility:

```csharp
// In your test setup
var result = await adapter.ExecuteWorkflowAsync(prompt);

// Export as JUnit-compatible artifact
var report = new EvaluationReport
{
    Name = "Workflow Tests",
    Results = [new TestResult { 
        Name = "support-workflow",
        Passed = result.IsSuccess,
        Duration = result.TotalDuration
    }]
};

await new JUnitXmlExporter().ExportAsync(report, outputStream);
```

## See Also

- [Architecture Overview](architecture.md)
- [Benchmarks](benchmarks.md) - Including BFCL workflow evaluation
- [CLI Usage](cli.md) - Running workflow evaluations from command line
- [Conversations](conversations.md) - Multi-turn conversation evaluation
