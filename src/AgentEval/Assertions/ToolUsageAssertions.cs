// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

using System.Text.Json;
using AgentEval.Models;

namespace AgentEval.Assertions;

/// <summary>
/// Fluent assertions entry point for ToolUsageReport.
/// </summary>
public class ToolUsageAssertions
{
    private readonly ToolUsageReport _report;
    
    public ToolUsageAssertions(ToolUsageReport report)
    {
        _report = report ?? throw new ArgumentNullException(nameof(report));
    }
    
    /// <summary>Assert that a specific tool was called at least once.</summary>
    public ToolCallAssertion HaveCalledTool(string toolName)
    {
        if (!_report.WasToolCalled(toolName))
        {
            var called = _report.Count > 0 
                ? $"Tools called: {string.Join(", ", _report.UniqueToolNames)}"
                : "No tools were called";
            throw new ToolAssertionException(
                $"Expected tool '{toolName}' to be called, but it was not.\n{called}");
        }
        
        var call = _report.GetCallsByName(toolName).First();
        return new ToolCallAssertion(this, _report, call, toolName);
    }
    
    /// <summary>Assert that a specific tool was NOT called.</summary>
    public ToolUsageAssertions NotHaveCalledTool(string toolName)
    {
        if (_report.WasToolCalled(toolName))
        {
            throw new ToolAssertionException(
                $"Expected tool '{toolName}' NOT to be called, but it was called {_report.GetCallsByName(toolName).Count()} time(s).");
        }
        return this;
    }
    
    /// <summary>Assert exact number of tool calls.</summary>
    public ToolUsageAssertions HaveCallCount(int expectedCount)
    {
        if (_report.Count != expectedCount)
        {
            throw new ToolAssertionException(
                $"Expected {expectedCount} tool call(s), but {_report.Count} call(s) were made.\n" +
                $"Tools called: {_report}");
        }
        return this;
    }
    
    /// <summary>Assert at least N tool calls.</summary>
    public ToolUsageAssertions HaveCallCountAtLeast(int minCount)
    {
        if (_report.Count < minCount)
        {
            throw new ToolAssertionException(
                $"Expected at least {minCount} tool call(s), but only {_report.Count} call(s) were made.\n" +
                $"Tools called: {_report}");
        }
        return this;
    }
    
    /// <summary>Assert no tool calls resulted in errors.</summary>
    public ToolUsageAssertions HaveNoErrors()
    {
        var errors = _report.Calls.Where(c => c.HasError).ToList();
        if (errors.Count > 0)
        {
            var errorDetails = string.Join("\n", errors.Select(e => $"  • {e.Name}: {e.Exception?.Message}"));
            throw new ToolAssertionException(
                $"Expected no tool errors, but {errors.Count} error(s) occurred:\n{errorDetails}");
        }
        return this;
    }
    
    /// <summary>Assert that tools were called in a specific order.</summary>
    public ToolUsageAssertions HaveCallOrder(params string[] expectedOrder)
    {
        for (int i = 0; i < expectedOrder.Length; i++)
        {
            var expectedTool = expectedOrder[i];
            var actualOrder = _report.GetToolOrder(expectedTool);
            
            if (actualOrder == 0)
            {
                throw new ToolAssertionException(
                    $"Expected tool '{expectedTool}' at position {i + 1}, but it was never called.\n" +
                    $"Actual order: {_report}");
            }
            
            if (i > 0)
            {
                var previousTool = expectedOrder[i - 1];
                var previousOrder = _report.GetToolOrder(previousTool);
                
                if (actualOrder <= previousOrder)
                {
                    throw new ToolAssertionException(
                        $"Expected '{expectedTool}' to be called after '{previousTool}', but order was reversed.\n" +
                        $"Actual order: {_report}");
                }
            }
        }
        return this;
    }
    
    /// <summary>Assert that at least one tool was called.</summary>
    public ToolUsageAssertions HaveCalledAnyTool()
    {
        if (_report.Count == 0)
        {
            throw new ToolAssertionException("Expected at least one tool to be called, but no tools were called.");
        }
        return this;
    }
    
    /// <summary>Get the underlying report for custom assertions.</summary>
    public ToolUsageReport Report => _report;
}

/// <summary>
/// Fluent assertions for a specific tool call.
/// </summary>
public class ToolCallAssertion
{
    private readonly ToolUsageAssertions _parent;
    private readonly ToolUsageReport _report;
    private readonly ToolCallRecord _call;
    private readonly string _toolName;
    
    internal ToolCallAssertion(ToolUsageAssertions parent, ToolUsageReport report, ToolCallRecord call, string toolName)
    {
        _parent = parent;
        _report = report;
        _call = call;
        _toolName = toolName;
    }
    
    /// <summary>Assert this tool was called before another tool.</summary>
    public ToolCallAssertion BeforeTool(string otherToolName)
    {
        var otherOrder = _report.GetToolOrder(otherToolName);
        if (otherOrder == 0)
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' to be called before '{otherToolName}', but '{otherToolName}' was never called.");
        }
        
        if (_call.Order >= otherOrder)
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' (order {_call.Order}) to be called before '{otherToolName}' (order {otherOrder}).\n" +
                $"Actual order: {_report}");
        }
        return this;
    }
    
    /// <summary>Assert this tool was called after another tool.</summary>
    public ToolCallAssertion AfterTool(string otherToolName)
    {
        var otherOrder = _report.GetToolOrder(otherToolName);
        if (otherOrder == 0)
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' to be called after '{otherToolName}', but '{otherToolName}' was never called.");
        }
        
        if (_call.Order <= otherOrder)
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' (order {_call.Order}) to be called after '{otherToolName}' (order {otherOrder}).\n" +
                $"Actual order: {_report}");
        }
        return this;
    }
    
    /// <summary>Assert a specific argument value (equality).</summary>
    public ToolCallAssertion WithArgument(string paramName, object expectedValue)
    {
        if (_call.Arguments == null || !_call.Arguments.TryGetValue(paramName, out var actualValue))
        {
            var available = _call.Arguments?.Keys.Any() == true 
                ? string.Join(", ", _call.Arguments.Keys) 
                : "(none)";
            throw new ToolAssertionException(
                $"Expected '{_toolName}' to have argument '{paramName}', but it was not found.\n" +
                $"Available arguments: {available}");
        }
        
        var actualStr = actualValue is JsonElement je ? je.GetRawText().Trim('"') : actualValue?.ToString();
        var expectedStr = expectedValue?.ToString();
        
        if (!string.Equals(actualStr, expectedStr, StringComparison.Ordinal))
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' argument '{paramName}' to equal '{expectedValue}', but was '{actualValue}'.");
        }
        return this;
    }
    
    /// <summary>Assert an argument contains a substring (case-insensitive).</summary>
    public ToolCallAssertion WithArgumentContaining(string paramName, string substring)
    {
        if (_call.Arguments == null || !_call.Arguments.TryGetValue(paramName, out var actualValue))
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' to have argument '{paramName}' containing '{substring}', but argument was not found.");
        }
        
        var actualStr = actualValue is JsonElement je ? je.GetString() : actualValue?.ToString();
        
        if (actualStr == null || !actualStr.Contains(substring, StringComparison.OrdinalIgnoreCase))
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' argument '{paramName}' to contain '{substring}', but was '{actualStr}'.");
        }
        return this;
    }
    
    /// <summary>Assert the tool result contains a substring (case-insensitive).</summary>
    public ToolCallAssertion WithResultContaining(string substring)
    {
        var resultStr = _call.Result?.ToString();
        
        if (resultStr == null || !resultStr.Contains(substring, StringComparison.OrdinalIgnoreCase))
        {
            var display = resultStr == null ? "(null)" : (resultStr.Length > 100 ? resultStr[..100] + "..." : resultStr);
            throw new ToolAssertionException(
                $"Expected '{_toolName}' result to contain '{substring}', but result was: {display}");
        }
        return this;
    }
    
    /// <summary>Assert the tool completed without error.</summary>
    public ToolCallAssertion WithoutError()
    {
        if (_call.HasError)
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' to complete without error, but got: {_call.Exception?.Message}");
        }
        return this;
    }
    
    /// <summary>Assert tool duration is under a maximum.</summary>
    public ToolCallAssertion WithDurationUnder(TimeSpan max)
    {
        if (!_call.HasTiming)
        {
            throw new ToolAssertionException(
                $"Cannot assert duration for '{_toolName}' - timing information not available (requires streaming).");
        }
        
        if (_call.Duration > max)
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' duration under {max.TotalMilliseconds:F0}ms, but was {_call.Duration.Value.TotalMilliseconds:F0}ms.");
        }
        return this;
    }
    
    /// <summary>Assert this tool was called exactly N times total.</summary>
    public ToolCallAssertion Times(int expectedCount)
    {
        var actualCount = _report.GetCallsByName(_toolName).Count();
        if (actualCount != expectedCount)
        {
            throw new ToolAssertionException(
                $"Expected '{_toolName}' to be called {expectedCount} time(s), but was called {actualCount} time(s).");
        }
        return this;
    }
    
    /// <summary>Return to parent assertions for chaining.</summary>
    public ToolUsageAssertions And() => _parent;
}
