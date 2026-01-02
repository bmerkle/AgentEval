// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

using AgentEval.Models;

namespace AgentEval.Assertions;

/// <summary>
/// Fluent assertions for PerformanceMetrics.
/// </summary>
public class PerformanceAssertions
{
    private readonly PerformanceMetrics _metrics;
    
    public PerformanceAssertions(PerformanceMetrics metrics)
    {
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
    }
    
    /// <summary>Assert total duration is under a maximum.</summary>
    public PerformanceAssertions HaveTotalDurationUnder(TimeSpan max)
    {
        if (_metrics.TotalDuration > max)
            throw new PerformanceAssertionException(
                $"Expected duration under {max.TotalMilliseconds:F0}ms, but was {_metrics.TotalDuration.TotalMilliseconds:F0}ms");
        return this;
    }
    
    /// <summary>Assert total duration is at least a minimum.</summary>
    public PerformanceAssertions HaveTotalDurationAtLeast(TimeSpan min)
    {
        if (_metrics.TotalDuration < min)
            throw new PerformanceAssertionException(
                $"Expected duration at least {min.TotalMilliseconds:F0}ms, but was {_metrics.TotalDuration.TotalMilliseconds:F0}ms");
        return this;
    }
    
    /// <summary>Assert time to first token is under a maximum (streaming only).</summary>
    public PerformanceAssertions HaveTimeToFirstTokenUnder(TimeSpan max)
    {
        if (!_metrics.TimeToFirstToken.HasValue)
            throw new PerformanceAssertionException(
                "Cannot assert TTFT - streaming was not used or TTFT was not captured");
        
        if (_metrics.TimeToFirstToken.Value > max)
            throw new PerformanceAssertionException(
                $"Expected TTFT under {max.TotalMilliseconds:F0}ms, but was {_metrics.TimeToFirstToken.Value.TotalMilliseconds:F0}ms");
        return this;
    }
    
    /// <summary>Assert total token count is under a maximum.</summary>
    public PerformanceAssertions HaveTokenCountUnder(int max)
    {
        if (_metrics.TotalTokens > max)
            throw new PerformanceAssertionException(
                $"Expected tokens under {max}, but was {_metrics.TotalTokens}");
        return this;
    }
    
    /// <summary>Assert prompt tokens under a maximum.</summary>
    public PerformanceAssertions HavePromptTokensUnder(int max)
    {
        if (_metrics.PromptTokens > max)
            throw new PerformanceAssertionException(
                $"Expected prompt tokens under {max}, but was {_metrics.PromptTokens}");
        return this;
    }
    
    /// <summary>Assert completion tokens under a maximum.</summary>
    public PerformanceAssertions HaveCompletionTokensUnder(int max)
    {
        if (_metrics.CompletionTokens > max)
            throw new PerformanceAssertionException(
                $"Expected completion tokens under {max}, but was {_metrics.CompletionTokens}");
        return this;
    }
    
    /// <summary>Assert estimated cost is under a maximum (USD).</summary>
    public PerformanceAssertions HaveEstimatedCostUnder(decimal maxUsd)
    {
        if (!_metrics.EstimatedCost.HasValue)
            throw new PerformanceAssertionException(
                "Cannot assert cost - model pricing not available or tokens not captured");
        
        if (_metrics.EstimatedCost.Value > maxUsd)
            throw new PerformanceAssertionException(
                $"Expected cost under ${maxUsd:F4}, but was ${_metrics.EstimatedCost.Value:F4}");
        return this;
    }
    
    /// <summary>Assert average tool time is under a maximum.</summary>
    public PerformanceAssertions HaveAverageToolTimeUnder(TimeSpan max)
    {
        if (_metrics.AverageToolTime > max)
            throw new PerformanceAssertionException(
                $"Expected average tool time under {max.TotalMilliseconds:F0}ms, but was {_metrics.AverageToolTime.TotalMilliseconds:F0}ms");
        return this;
    }
    
    /// <summary>Assert total tool time is under a maximum.</summary>
    public PerformanceAssertions HaveTotalToolTimeUnder(TimeSpan max)
    {
        if (_metrics.TotalToolTime > max)
            throw new PerformanceAssertionException(
                $"Expected total tool time under {max.TotalMilliseconds:F0}ms, but was {_metrics.TotalToolTime.TotalMilliseconds:F0}ms");
        return this;
    }
    
    /// <summary>Assert tool call count is exactly N.</summary>
    public PerformanceAssertions HaveToolCallCount(int expected)
    {
        if (_metrics.ToolCallCount != expected)
            throw new PerformanceAssertionException(
                $"Expected {expected} tool call(s), but was {_metrics.ToolCallCount}");
        return this;
    }
    
    /// <summary>Get the underlying metrics for custom assertions.</summary>
    public PerformanceMetrics Metrics => _metrics;
}
