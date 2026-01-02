// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

namespace AgentEval.Core;

/// <summary>
/// Represents an evaluation metric that can be applied to agent responses.
/// </summary>
public interface IMetric
{
    /// <summary>Gets the name of the metric.</summary>
    string Name { get; }
    
    /// <summary>Gets a description of what this metric measures.</summary>
    string Description { get; }
    
    /// <summary>
    /// Evaluate the metric for a given context.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The metric result.</returns>
    Task<MetricResult> EvaluateAsync(
        EvaluationContext context, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for RAG-specific metrics.
/// </summary>
public interface IRAGMetric : IMetric
{
    /// <summary>Whether this metric requires retrieved context.</summary>
    bool RequiresContext { get; }
    
    /// <summary>Whether this metric requires ground truth.</summary>
    bool RequiresGroundTruth { get; }
}

/// <summary>
/// Marker interface for agentic metrics.
/// </summary>
public interface IAgenticMetric : IMetric
{
    /// <summary>Whether this metric requires tool usage information.</summary>
    bool RequiresToolUsage { get; }
}

/// <summary>
/// Context for metric evaluation.
/// </summary>
public class EvaluationContext
{
    /// <summary>Unique ID for this evaluation run.</summary>
    public string EvaluationId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>When the evaluation started.</summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>The original user question/prompt.</summary>
    public required string Input { get; init; }
    
    /// <summary>The agent's response.</summary>
    public required string Output { get; init; }
    
    /// <summary>Retrieved context (for RAG metrics).</summary>
    public string? Context { get; init; }
    
    /// <summary>Ground truth answer (for accuracy metrics).</summary>
    public string? GroundTruth { get; init; }
    
    /// <summary>Tool usage report (for agentic metrics).</summary>
    public Models.ToolUsageReport? ToolUsage { get; init; }
    
    /// <summary>Expected tools to be called.</summary>
    public IReadOnlyList<string>? ExpectedTools { get; init; }
    
    /// <summary>Performance metrics.</summary>
    public Models.PerformanceMetrics? Performance { get; init; }

    /// <summary>Tool call timeline (for trace-first debugging).</summary>
    public Models.ToolCallTimeline? Timeline { get; init; }

    /// <summary>Custom properties for plugin use.</summary>
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    /// <summary>Gets a typed property value.</summary>
    public T? GetProperty<T>(string key)
    {
        if (Properties.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>Sets a property value.</summary>
    public void SetProperty<T>(string key, T value) => Properties[key] = value;
}

/// <summary>
/// Result of a metric evaluation.
/// </summary>
public class MetricResult
{
    /// <summary>Name of the metric.</summary>
    public required string MetricName { get; init; }
    
    /// <summary>Score from 0.0 to 100.0.</summary>
    public required double Score { get; init; }
    
    /// <summary>Whether the metric passed (based on threshold).</summary>
    public bool Passed { get; init; }
    
    /// <summary>Human-readable explanation of the score.</summary>
    public string? Explanation { get; init; }
    
    /// <summary>Additional details or breakdown.</summary>
    public IDictionary<string, object>? Details { get; init; }
    
    /// <summary>
    /// Create a passing metric result.
    /// </summary>
    public static MetricResult Pass(string metricName, double score, string? explanation = null, IDictionary<string, object>? details = null) =>
        new() { MetricName = metricName, Score = score, Passed = true, Explanation = explanation, Details = details };
    
    /// <summary>
    /// Create a failing metric result.
    /// </summary>
    public static MetricResult Fail(string metricName, string explanation, double score = 0, IDictionary<string, object>? details = null) =>
        new() { MetricName = metricName, Score = score, Passed = false, Explanation = explanation, Details = details };
}
