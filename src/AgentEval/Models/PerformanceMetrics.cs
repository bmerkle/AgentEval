// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

using System.Collections.Concurrent;
using AgentEval.Assertions;

namespace AgentEval.Models;

/// <summary>
/// Performance metrics captured during an agent run.
/// </summary>
public class PerformanceMetrics
{
    /// <summary>When the request started.</summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>When the request completed.</summary>
    public DateTimeOffset EndTime { get; set; }
    
    /// <summary>Total duration of the request.</summary>
    public TimeSpan TotalDuration => EndTime - StartTime;
    
    /// <summary>Time to receive first token (streaming only).</summary>
    public TimeSpan? TimeToFirstToken { get; set; }
    
    /// <summary>Number of prompt/input tokens used.</summary>
    public int? PromptTokens { get; set; }
    
    /// <summary>Number of completion/output tokens used.</summary>
    public int? CompletionTokens { get; set; }
    
    /// <summary>Total tokens used.</summary>
    public int? TotalTokens => (PromptTokens ?? 0) + (CompletionTokens ?? 0);
    
    /// <summary>Number of tool calls made.</summary>
    public int ToolCallCount { get; set; }
    
    /// <summary>Total time spent in tool execution.</summary>
    public TimeSpan TotalToolTime { get; set; }
    
    /// <summary>Average time per tool call.</summary>
    public TimeSpan AverageToolTime => ToolCallCount > 0 
        ? TimeSpan.FromTicks(TotalToolTime.Ticks / ToolCallCount) 
        : TimeSpan.Zero;
    
    /// <summary>Estimated cost in USD (based on model pricing).</summary>
    public decimal? EstimatedCost { get; set; }
    
    /// <summary>Model used for the request.</summary>
    public string? ModelUsed { get; set; }
    
    /// <summary>Whether streaming was used.</summary>
    public bool WasStreaming { get; set; }
    
    /// <summary>Start fluent assertions.</summary>
    public PerformanceAssertions Should() => new(this);
    
    public override string ToString()
    {
        var parts = new List<string>
        {
            $"Duration: {TotalDuration.TotalMilliseconds:F0}ms"
        };
        
        if (TimeToFirstToken.HasValue)
            parts.Add($"TTFT: {TimeToFirstToken.Value.TotalMilliseconds:F0}ms");
        
        if (TotalTokens > 0)
            parts.Add($"Tokens: {TotalTokens}");
        
        if (EstimatedCost.HasValue)
            parts.Add($"Cost: ${EstimatedCost:F4}");
        
        if (ToolCallCount > 0)
            parts.Add($"Tools: {ToolCallCount}");
        
        return string.Join(" | ", parts);
    }
}

/// <summary>
/// Model pricing information for cost estimation.
/// Thread-safe for concurrent access.
/// </summary>
public static class ModelPricing
{
    private static readonly ConcurrentDictionary<string, (decimal InputPer1K, decimal OutputPer1K)> _pricing = 
        new(StringComparer.OrdinalIgnoreCase)
    {
        // OpenAI models
        ["gpt-5-mini"] = (0.0001m, 0.0004m),  // gpt-5-mini pricing (placeholder, similar to gpt-4o-mini)
        ["gpt-4o"] = (0.005m, 0.015m),
        ["gpt-4o-2024-11-20"] = (0.0025m, 0.01m),
        ["gpt-4o-mini"] = (0.00015m, 0.0006m),
        ["gpt-4.1"] = (0.01m, 0.03m),  // gpt-4.1 pricing (placeholder, similar to gpt-4-turbo)
        ["gpt-4-turbo"] = (0.01m, 0.03m),
        ["gpt-4"] = (0.03m, 0.06m),
        ["gpt-3.5-turbo"] = (0.0005m, 0.0015m),
        ["o1"] = (0.015m, 0.06m),
        ["o1-mini"] = (0.003m, 0.012m),
        ["o3-mini"] = (0.00165m, 0.0066m),
        
        // Anthropic models
        ["claude-3-5-sonnet"] = (0.003m, 0.015m),
        ["claude-3-5-sonnet-20241022"] = (0.003m, 0.015m),
        ["claude-3-opus"] = (0.015m, 0.075m),
        ["claude-3-sonnet"] = (0.003m, 0.015m),
        ["claude-3-haiku"] = (0.00025m, 0.00125m),
    };
    
    /// <summary>Estimate cost for a request.</summary>
    public static decimal? EstimateCost(string? modelName, int inputTokens, int outputTokens)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;
        
        var pricing = GetPricing(modelName);
        if (pricing == null)
            return null;
        
        return (inputTokens / 1000m * pricing.Value.InputPer1K) + (outputTokens / 1000m * pricing.Value.OutputPer1K);
    }
    
    /// <summary>Get pricing for a model.</summary>
    public static (decimal InputPer1K, decimal OutputPer1K, decimal InputPricePerMillion, decimal OutputPricePerMillion)? GetPricing(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;
        
        // Try exact match first, then partial match
        if (!_pricing.TryGetValue(modelName, out var price))
        {
            var match = _pricing.Keys.FirstOrDefault(k => 
                modelName.Contains(k, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                return null;
            price = _pricing[match];
        }
        
        return (price.InputPer1K, price.OutputPer1K, price.InputPer1K * 1000, price.OutputPer1K * 1000);
    }
    
    /// <summary>Add or update model pricing.</summary>
    /// <param name="modelName">The model name (case-insensitive).</param>
    /// <param name="inputPer1K">Price per 1000 input tokens in USD.</param>
    /// <param name="outputPer1K">Price per 1000 output tokens in USD.</param>
    /// <exception cref="ArgumentNullException">Thrown when modelName is null.</exception>
    public static void SetPricing(string modelName, decimal inputPer1K, decimal outputPer1K)
    {
        _ = modelName ?? throw new ArgumentNullException(nameof(modelName));
        _pricing.AddOrUpdate(
            modelName.ToLowerInvariant(),
            (inputPer1K, outputPer1K),
            (_, _) => (inputPer1K, outputPer1K));
    }
    
    /// <summary>Get all known model names.</summary>
    public static IEnumerable<string> KnownModels => _pricing.Keys;
}
