// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

using AgentEval.Models;

namespace AgentEval.Assertions;

/// <summary>
/// Fluent assertion builder for workflow execution results.
/// </summary>
/// <remarks>
/// <para>
/// Provides a fluent API for asserting on workflow execution:
/// </para>
/// <code>
/// result.Should()
///     .HaveStepCount(3)
///     .HaveExecutedInOrder("researcher", "writer", "editor")
///     .HaveNoErrors()
///     .ForExecutor("writer")
///         .HaveOutputContaining("article")
///         .HaveCompletedWithin(TimeSpan.FromSeconds(30))
///     .And()
///     .Validate();
/// </code>
/// </remarks>
public static class WorkflowAssertionsExtensions
{
    /// <summary>
    /// Begin fluent assertions on a workflow execution result.
    /// </summary>
    /// <param name="result">The workflow execution result to assert on.</param>
    /// <returns>A fluent assertion builder.</returns>
    public static WorkflowAssertionBuilder Should(this WorkflowExecutionResult result)
        => new(result);
}

/// <summary>
/// Fluent assertion builder for workflow execution results.
/// </summary>
public class WorkflowAssertionBuilder
{
    private readonly WorkflowExecutionResult _result;
    private readonly List<string> _failures = [];

    /// <summary>
    /// Creates a new assertion builder for a workflow result.
    /// </summary>
    internal WorkflowAssertionBuilder(WorkflowExecutionResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>
    /// Assert the workflow has a specific number of executor steps.
    /// </summary>
    /// <param name="expected">Expected number of steps.</param>
    public WorkflowAssertionBuilder HaveStepCount(int expected)
    {
        if (_result.Steps.Count != expected)
        {
            _failures.Add($"Expected {expected} steps but found {_result.Steps.Count}");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow has at least a minimum number of steps.
    /// </summary>
    /// <param name="minimum">Minimum number of steps.</param>
    public WorkflowAssertionBuilder HaveAtLeastSteps(int minimum)
    {
        if (_result.Steps.Count < minimum)
        {
            _failures.Add($"Expected at least {minimum} steps but found {_result.Steps.Count}");
        }
        return this;
    }

    /// <summary>
    /// Assert a specific executor was invoked.
    /// </summary>
    /// <param name="executorId">The executor ID to check.</param>
    public WorkflowAssertionBuilder HaveInvokedExecutor(string executorId)
    {
        if (!_result.Steps.Any(s => s.ExecutorId.Equals(executorId, StringComparison.OrdinalIgnoreCase)))
        {
            _failures.Add($"Expected executor '{executorId}' was not invoked. " +
                          $"Invoked executors: [{string.Join(", ", _result.Steps.Select(s => s.ExecutorId))}]");
        }
        return this;
    }

    /// <summary>
    /// Assert executors were invoked in a specific order.
    /// </summary>
    /// <param name="executorIds">Expected executor order.</param>
    public WorkflowAssertionBuilder HaveExecutedInOrder(params string[] executorIds)
    {
        var actualOrder = _result.Steps.Select(s => s.ExecutorId).ToList();

        if (actualOrder.Count != executorIds.Length)
        {
            _failures.Add($"Expected executor order [{string.Join(" → ", executorIds)}] " +
                          $"but found [{string.Join(" → ", actualOrder)}] (different count)");
            return this;
        }

        for (int i = 0; i < executorIds.Length; i++)
        {
            if (!actualOrder[i].Equals(executorIds[i], StringComparison.OrdinalIgnoreCase))
            {
                _failures.Add($"Expected executor order [{string.Join(" → ", executorIds)}] " +
                              $"but found [{string.Join(" → ", actualOrder)}]");
                break;
            }
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow completed within a time limit.
    /// </summary>
    /// <param name="maxDuration">Maximum acceptable duration.</param>
    public WorkflowAssertionBuilder HaveCompletedWithin(TimeSpan maxDuration)
    {
        if (_result.TotalDuration > maxDuration)
        {
            _failures.Add($"Expected completion within {maxDuration.TotalSeconds:F1}s " +
                          $"but took {_result.TotalDuration.TotalSeconds:F1}s");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow had no errors.
    /// </summary>
    public WorkflowAssertionBuilder HaveNoErrors()
    {
        if (_result.Errors?.Any() == true)
        {
            _failures.Add($"Expected no errors but found {_result.Errors.Count}: " +
                          $"{string.Join(", ", _result.Errors.Select(e => $"[{e.ExecutorId}] {e.Message}"))}");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow completed successfully.
    /// </summary>
    public WorkflowAssertionBuilder HaveSucceeded()
    {
        if (!_result.IsSuccess)
        {
            var errorMsg = _result.Errors?.FirstOrDefault()?.Message ?? "Unknown error";
            _failures.Add($"Expected workflow to succeed but it failed: {errorMsg}");
        }
        return this;
    }

    /// <summary>
    /// Assert the final output contains a string.
    /// </summary>
    /// <param name="expected">Expected substring.</param>
    /// <param name="caseSensitive">Whether comparison is case-sensitive.</param>
    public WorkflowAssertionBuilder HaveFinalOutputContaining(string expected, bool caseSensitive = false)
    {
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (!_result.FinalOutput.Contains(expected, comparison))
        {
            _failures.Add($"Expected final output to contain '{expected}' " +
                          $"but output was: \"{Truncate(_result.FinalOutput, 100)}\"");
        }
        return this;
    }

    /// <summary>
    /// Assert the final output matches a pattern.
    /// </summary>
    /// <param name="pattern">Regex pattern to match.</param>
    public WorkflowAssertionBuilder HaveFinalOutputMatching(string pattern)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(_result.FinalOutput, pattern))
        {
            _failures.Add($"Expected final output to match pattern '{pattern}' " +
                          $"but output was: \"{Truncate(_result.FinalOutput, 100)}\"");
        }
        return this;
    }

    /// <summary>
    /// Assert the final output is not empty.
    /// </summary>
    public WorkflowAssertionBuilder HaveNonEmptyOutput()
    {
        if (string.IsNullOrWhiteSpace(_result.FinalOutput))
        {
            _failures.Add("Expected non-empty final output but output was empty");
        }
        return this;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EDGE AND GRAPH ASSERTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Assert the workflow has a graph structure.
    /// </summary>
    public WorkflowAssertionBuilder HaveGraphStructure()
    {
        if (_result.Graph == null)
        {
            _failures.Add("Expected workflow to have graph structure but Graph was null");
        }
        return this;
    }

    /// <summary>
    /// Assert a specific edge was traversed during execution.
    /// </summary>
    /// <param name="sourceExecutorId">Source executor ID.</param>
    /// <param name="targetExecutorId">Target executor ID.</param>
    public WorkflowAssertionBuilder HaveTraversedEdge(string sourceExecutorId, string targetExecutorId)
    {
        var traversedEdges = _result.Graph?.TraversedEdges;
        if (traversedEdges == null || !traversedEdges.Any(e =>
            e.SourceExecutorId.Equals(sourceExecutorId, StringComparison.OrdinalIgnoreCase) &&
            e.TargetExecutorId.Equals(targetExecutorId, StringComparison.OrdinalIgnoreCase)))
        {
            _failures.Add($"Expected edge '{sourceExecutorId}' → '{targetExecutorId}' to be traversed but it was not. " +
                          $"Traversed edges: [{string.Join(", ", traversedEdges?.Select(e => $"{e.SourceExecutorId}→{e.TargetExecutorId}") ?? [])}]");
        }
        return this;
    }

    /// <summary>
    /// Assert a specific edge type was used.
    /// </summary>
    /// <param name="edgeType">Expected edge type.</param>
    public WorkflowAssertionBuilder HaveUsedEdgeType(EdgeType edgeType)
    {
        var traversedEdges = _result.Graph?.TraversedEdges;
        if (traversedEdges == null || !traversedEdges.Any(e => e.EdgeType == edgeType))
        {
            _failures.Add($"Expected edge type '{edgeType}' to be used but it was not. " +
                          $"Used edge types: [{string.Join(", ", traversedEdges?.Select(e => e.EdgeType.ToString()).Distinct() ?? [])}]");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow used conditional routing.
    /// </summary>
    public WorkflowAssertionBuilder HaveConditionalRouting()
    {
        if (!_result.HasConditionalRouting)
        {
            _failures.Add("Expected workflow to have conditional routing but it did not");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow had parallel execution.
    /// </summary>
    public WorkflowAssertionBuilder HaveParallelExecution()
    {
        if (!_result.HasParallelExecution)
        {
            _failures.Add("Expected workflow to have parallel execution but it did not");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow has a specific number of parallel branches.
    /// </summary>
    /// <param name="expectedCount">Expected number of parallel branches.</param>
    public WorkflowAssertionBuilder HaveParallelBranchCount(int expectedCount)
    {
        var actualCount = _result.Graph?.ParallelBranches?.Count ?? 0;
        if (actualCount != expectedCount)
        {
            _failures.Add($"Expected {expectedCount} parallel branches but found {actualCount}");
        }
        return this;
    }

    /// <summary>
    /// Assert a specific routing decision was made.
    /// </summary>
    /// <param name="deciderExecutorId">The executor that made the decision.</param>
    /// <param name="selectedEdgeId">The edge that was selected.</param>
    public WorkflowAssertionBuilder HaveRoutingDecision(string deciderExecutorId, string selectedEdgeId)
    {
        var decisions = _result.RoutingDecisions;
        if (decisions == null || !decisions.Any(d =>
            d.DeciderExecutorId.Equals(deciderExecutorId, StringComparison.OrdinalIgnoreCase) &&
            d.SelectedEdgeId.Equals(selectedEdgeId, StringComparison.OrdinalIgnoreCase)))
        {
            _failures.Add($"Expected routing decision from '{deciderExecutorId}' selecting '{selectedEdgeId}' " +
                          $"but it was not found");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow execution path matches expected sequence.
    /// </summary>
    /// <param name="expectedPath">Expected executor ID sequence.</param>
    public WorkflowAssertionBuilder HaveExecutionPath(params string[] expectedPath)
    {
        var actualPath = _result.GetExecutionPath().ToList();
        
        if (!actualPath.SequenceEqual(expectedPath, StringComparer.OrdinalIgnoreCase))
        {
            _failures.Add($"Expected execution path [{string.Join(" → ", expectedPath)}] " +
                          $"but actual path was [{string.Join(" → ", actualPath)}]");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow contains specific nodes.
    /// </summary>
    /// <param name="nodeIds">Expected node IDs.</param>
    public WorkflowAssertionBuilder HaveNodes(params string[] nodeIds)
    {
        if (_result.Graph == null)
        {
            _failures.Add("Cannot check nodes: workflow has no graph structure");
            return this;
        }

        var graphNodeIds = _result.Graph.Nodes.Select(n => n.NodeId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingNodes = nodeIds.Where(id => !graphNodeIds.Contains(id)).ToList();
        
        if (missingNodes.Count > 0)
        {
            _failures.Add($"Expected nodes [{string.Join(", ", missingNodes)}] not found in graph. " +
                          $"Available nodes: [{string.Join(", ", graphNodeIds)}]");
        }
        return this;
    }

    /// <summary>
    /// Assert the workflow graph has a specific entry point.
    /// </summary>
    /// <param name="entryNodeId">Expected entry node ID.</param>
    public WorkflowAssertionBuilder HaveEntryPoint(string entryNodeId)
    {
        if (_result.Graph == null)
        {
            _failures.Add("Cannot check entry point: workflow has no graph structure");
        }
        else if (!_result.Graph.EntryNodeId.Equals(entryNodeId, StringComparison.OrdinalIgnoreCase))
        {
            _failures.Add($"Expected entry point '{entryNodeId}' but found '{_result.Graph.EntryNodeId}'");
        }
        return this;
    }

    /// <summary>
    /// Get assertion builder for edge-related assertions.
    /// </summary>
    public EdgeAssertionBuilder ForEdge(string sourceExecutorId, string targetExecutorId)
    {
        var edge = _result.Graph?.TraversedEdges?.FirstOrDefault(e =>
            e.SourceExecutorId.Equals(sourceExecutorId, StringComparison.OrdinalIgnoreCase) &&
            e.TargetExecutorId.Equals(targetExecutorId, StringComparison.OrdinalIgnoreCase));
        return new EdgeAssertionBuilder(this, edge, sourceExecutorId, targetExecutorId);
    }

    /// <summary>
    /// Get assertion builder for a specific executor step.
    /// </summary>
    /// <param name="executorId">The executor ID to assert on.</param>
    public ExecutorStepAssertionBuilder ForExecutor(string executorId)
    {
        var step = _result.Steps.FirstOrDefault(s => 
            s.ExecutorId.Equals(executorId, StringComparison.OrdinalIgnoreCase));
        return new ExecutorStepAssertionBuilder(this, step, executorId);
    }

    /// <summary>
    /// Get assertion builder for a specific step by index.
    /// </summary>
    /// <param name="index">Zero-based step index.</param>
    public ExecutorStepAssertionBuilder ForStep(int index)
    {
        var step = index >= 0 && index < _result.Steps.Count ? _result.Steps[index] : null;
        return new ExecutorStepAssertionBuilder(this, step, $"step[{index}]");
    }

    /// <summary>
    /// Validate all assertions and throw if any failed.
    /// </summary>
    /// <exception cref="WorkflowAssertionException">Thrown if any assertions failed.</exception>
    public void Validate()
    {
        if (_failures.Count > 0)
        {
            throw new WorkflowAssertionException(
                $"Workflow assertion failed ({_failures.Count} issue(s)):\n  • {string.Join("\n  • ", _failures)}");
        }
    }

    /// <summary>
    /// Get whether all assertions have passed so far.
    /// </summary>
    public bool IsValid => _failures.Count == 0;

    /// <summary>
    /// Get the list of failure messages.
    /// </summary>
    public IReadOnlyList<string> Failures => _failures;

    internal void AddFailure(string failure) => _failures.Add(failure);

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "(empty)";
        if (text.Length <= maxLength) return text;
        return text[..(maxLength - 3)] + "...";
    }
}

/// <summary>
/// Fluent assertion builder for individual executor steps.
/// </summary>
public class ExecutorStepAssertionBuilder
{
    private readonly WorkflowAssertionBuilder _parent;
    private readonly ExecutorStep? _step;
    private readonly string _executorId;

    internal ExecutorStepAssertionBuilder(WorkflowAssertionBuilder parent, ExecutorStep? step, string executorId)
    {
        _parent = parent;
        _step = step;
        _executorId = executorId;
    }

    /// <summary>
    /// Assert the executor's output contains a string.
    /// </summary>
    /// <param name="expected">Expected substring.</param>
    /// <param name="caseSensitive">Whether comparison is case-sensitive.</param>
    public ExecutorStepAssertionBuilder HaveOutputContaining(string expected, bool caseSensitive = false)
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (!_step.Output.Contains(expected, comparison))
            {
                _parent.AddFailure($"Executor '{_executorId}' output does not contain '{expected}'. " +
                                   $"Actual output: \"{Truncate(_step.Output, 80)}\"");
            }
        }
        return this;
    }

    /// <summary>
    /// Assert the executor completed within a time limit.
    /// </summary>
    /// <param name="maxDuration">Maximum acceptable duration.</param>
    public ExecutorStepAssertionBuilder HaveCompletedWithin(TimeSpan maxDuration)
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (_step.Duration > maxDuration)
        {
            _parent.AddFailure($"Executor '{_executorId}' took {_step.Duration.TotalMilliseconds:F0}ms, " +
                               $"expected under {maxDuration.TotalMilliseconds:F0}ms");
        }
        return this;
    }

    /// <summary>
    /// Assert the executor called a specific tool.
    /// </summary>
    /// <param name="toolName">Name of the expected tool.</param>
    public ExecutorStepAssertionBuilder HaveCalledTool(string toolName)
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (_step.ToolCalls?.Any(t => t.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase)) != true)
        {
            var calledTools = _step.ToolCalls?.Select(t => t.Name).ToList() ?? [];
            _parent.AddFailure($"Executor '{_executorId}' did not call tool '{toolName}'. " +
                               $"Called tools: [{string.Join(", ", calledTools)}]");
        }
        return this;
    }

    /// <summary>
    /// Assert the executor made a specific number of tool calls.
    /// </summary>
    /// <param name="expected">Expected number of tool calls.</param>
    public ExecutorStepAssertionBuilder HaveToolCallCount(int expected)
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else
        {
            var actual = _step.ToolCalls?.Count ?? 0;
            if (actual != expected)
            {
                _parent.AddFailure($"Executor '{_executorId}' made {actual} tool calls, expected {expected}");
            }
        }
        return this;
    }

    /// <summary>
    /// Assert the executor has non-empty output.
    /// </summary>
    public ExecutorStepAssertionBuilder HaveNonEmptyOutput()
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (string.IsNullOrWhiteSpace(_step.Output))
        {
            _parent.AddFailure($"Executor '{_executorId}' produced empty output");
        }
        return this;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EDGE-RELATED STEP ASSERTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Assert the executor was reached via conditional routing.
    /// </summary>
    public ExecutorStepAssertionBuilder HaveBeenConditionallyRouted()
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (!_step.WasConditionallyRouted)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not reached via conditional routing");
        }
        return this;
    }

    /// <summary>
    /// Assert the executor is part of a parallel branch.
    /// </summary>
    public ExecutorStepAssertionBuilder BeInParallelBranch()
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (!_step.IsParallelBranch)
        {
            _parent.AddFailure($"Executor '{_executorId}' is not part of a parallel branch");
        }
        return this;
    }

    /// <summary>
    /// Assert the executor is part of a specific parallel branch.
    /// </summary>
    /// <param name="branchId">Expected branch ID.</param>
    public ExecutorStepAssertionBuilder BeInParallelBranch(string branchId)
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (!string.Equals(_step.ParallelBranchId, branchId, StringComparison.OrdinalIgnoreCase))
        {
            _parent.AddFailure($"Executor '{_executorId}' expected branch '{branchId}' but was in '{_step.ParallelBranchId ?? "(none)"}'");
        }
        return this;
    }

    /// <summary>
    /// Assert the executor has an incoming edge.
    /// </summary>
    public ExecutorStepAssertionBuilder HaveIncomingEdge()
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (_step.IncomingEdge == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' has no incoming edge");
        }
        return this;
    }

    /// <summary>
    /// Assert the executor has outgoing edges.
    /// </summary>
    /// <param name="expectedCount">Optional expected count of outgoing edges.</param>
    public ExecutorStepAssertionBuilder HaveOutgoingEdges(int? expectedCount = null)
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else
        {
            var actualCount = _step.OutgoingEdges?.Count ?? 0;
            if (actualCount == 0)
            {
                _parent.AddFailure($"Executor '{_executorId}' has no outgoing edges");
            }
            else if (expectedCount.HasValue && actualCount != expectedCount.Value)
            {
                _parent.AddFailure($"Executor '{_executorId}' expected {expectedCount} outgoing edges but had {actualCount}");
            }
        }
        return this;
    }

    /// <summary>
    /// Assert the executor's incoming edge is of a specific type.
    /// </summary>
    /// <param name="expectedType">Expected edge type.</param>
    public ExecutorStepAssertionBuilder HaveIncomingEdgeOfType(EdgeType expectedType)
    {
        if (_step == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' was not found");
        }
        else if (_step.IncomingEdge == null)
        {
            _parent.AddFailure($"Executor '{_executorId}' has no incoming edge");
        }
        else if (_step.IncomingEdge.EdgeType != expectedType)
        {
            _parent.AddFailure($"Executor '{_executorId}' incoming edge expected type '{expectedType}' but was '{_step.IncomingEdge.EdgeType}'");
        }
        return this;
    }

    /// <summary>
    /// Return to the parent builder for continued chaining.
    /// </summary>
    public WorkflowAssertionBuilder And() => _parent;

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "(empty)";
        if (text.Length <= maxLength) return text;
        return text[..(maxLength - 3)] + "...";
    }
}

/// <summary>
/// Exception thrown when workflow assertions fail.
/// </summary>
public class WorkflowAssertionException : Exception
{
    /// <summary>
    /// Creates a new workflow assertion exception.
    /// </summary>
    /// <param name="message">Failure message.</param>
    public WorkflowAssertionException(string message) : base(message) { }

    /// <summary>
    /// Creates a new workflow assertion exception with inner exception.
    /// </summary>
    /// <param name="message">Failure message.</param>
    /// <param name="inner">Inner exception.</param>
    public WorkflowAssertionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Fluent assertion builder for edge-related assertions.
/// </summary>
public class EdgeAssertionBuilder
{
    private readonly WorkflowAssertionBuilder _parent;
    private readonly EdgeExecution? _edge;
    private readonly string _sourceId;
    private readonly string _targetId;

    internal EdgeAssertionBuilder(WorkflowAssertionBuilder parent, EdgeExecution? edge, string sourceId, string targetId)
    {
        _parent = parent;
        _edge = edge;
        _sourceId = sourceId;
        _targetId = targetId;
    }

    /// <summary>
    /// Assert the edge exists.
    /// </summary>
    public EdgeAssertionBuilder Exist()
    {
        if (_edge == null)
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' was not traversed");
        }
        return this;
    }

    /// <summary>
    /// Assert the edge is of a specific type.
    /// </summary>
    /// <param name="expectedType">Expected edge type.</param>
    public EdgeAssertionBuilder BeOfType(EdgeType expectedType)
    {
        if (_edge == null)
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' was not found");
        }
        else if (_edge.EdgeType != expectedType)
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' expected type '{expectedType}' but was '{_edge.EdgeType}'");
        }
        return this;
    }

    /// <summary>
    /// Assert the conditional edge had a specific result.
    /// </summary>
    /// <param name="expectedResult">Expected condition result.</param>
    public EdgeAssertionBuilder HaveConditionResult(bool expectedResult)
    {
        if (_edge == null)
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' was not found");
        }
        else if (_edge.ConditionResult != expectedResult)
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' expected condition result '{expectedResult}' " +
                               $"but was '{_edge.ConditionResult}'");
        }
        return this;
    }

    /// <summary>
    /// Assert the switch edge matched a specific label.
    /// </summary>
    /// <param name="expectedLabel">Expected switch label.</param>
    public EdgeAssertionBuilder HaveMatchedSwitchLabel(string expectedLabel)
    {
        if (_edge == null)
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' was not found");
        }
        else if (!string.Equals(_edge.MatchedSwitchLabel, expectedLabel, StringComparison.OrdinalIgnoreCase))
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' expected switch label '{expectedLabel}' " +
                               $"but was '{_edge.MatchedSwitchLabel}'");
        }
        return this;
    }

    /// <summary>
    /// Assert the edge transferred specific data.
    /// </summary>
    /// <param name="expectedData">Expected data substring.</param>
    public EdgeAssertionBuilder HaveTransferredDataContaining(string expectedData)
    {
        if (_edge == null)
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' was not found");
        }
        else if (_edge.TransferredData == null || !_edge.TransferredData.Contains(expectedData, StringComparison.OrdinalIgnoreCase))
        {
            _parent.AddFailure($"Edge '{_sourceId}' → '{_targetId}' expected data containing '{expectedData}' " +
                               $"but transferred: '{_edge.TransferredData ?? "(null)"}'");
        }
        return this;
    }

    /// <summary>
    /// Return to the parent builder for continued chaining.
    /// </summary>
    public WorkflowAssertionBuilder And() => _parent;
}
