// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using AgentEval.Core;
using AgentEval.MAF;
using AgentEval.Models;
using AgentEval.Tests.TestHelpers;
using Xunit;

namespace AgentEval.Tests.MAF;

/// <summary>
/// Integration tests: ChatClientAgentAdapter streaming through MAFEvaluationHarness.
/// Validates the full pipeline: IChatClient → ChatClientAgentAdapter → 
/// MAFEvaluationHarness.RunEvaluationStreamingAsync → ToolUsageReport.
/// </summary>
public class ChatClientAdapterStreamingIntegrationTests
{
    [Fact]
    public async Task RunEvaluationStreamingAsync_ChatClientWithToolCalls_PopulatesToolUsageReport()
    {
        // Arrange: ChatClientAgentAdapter wrapping mock with tool calls
        var mock = MockStreamingChatClient.WithFullScenario(
            initialText: "Let me look that up. ",
            toolCalls:
            [
                ("GetWeather", "call_int_1", "72°F, Sunny"),
                ("GetNews", "call_int_2", "No major news")
            ],
            finalText: "The weather is 72°F and there's no major news.",
            inputTokens: 200,
            outputTokens: 100);

        var adapter = new ChatClientAgentAdapter(mock, "IntegrationTestAgent");
        var harness = new MAFEvaluationHarness(verbose: false);
        var testCase = new TestCase
        {
            Name = "Streaming Tool Integration",
            Input = "What's the weather and news?"
        };
        var options = new EvaluationOptions
        {
            TrackTools = true,
            TrackPerformance = true,
            EvaluateResponse = false
        };

        // Act
        var result = await harness.RunEvaluationStreamingAsync(
            adapter, testCase, streamingOptions: null, options);

        // Assert: ToolUsageReport is populated
        Assert.NotNull(result.ToolUsage);
        Assert.Equal(2, result.ToolUsage!.Count);

        var toolNames = result.ToolUsage.ToolNames.ToList();
        Assert.Contains("GetWeather", toolNames);
        Assert.Contains("GetNews", toolNames);

        // Assert: Performance metrics reflect tool calls
        Assert.NotNull(result.Performance);
        Assert.Equal(2, result.Performance!.ToolCallCount);

        // Assert: Test produced output
        Assert.True(result.Passed, $"Test should pass, but: {result.Details}");
        Assert.Contains("Let me look that up.", result.ActualOutput!);
    }

    [Fact]
    public async Task RunEvaluationStreamingAsync_ChatClientWithToolCalls_FiresStreamingCallbacks()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithSingleToolCall(
            toolName: "Translate",
            callId: "call_cb_1",
            result: "Hola mundo",
            textBefore: "Translating... ",
            textAfter: "Translation: Hola mundo");

        var adapter = new ChatClientAgentAdapter(mock, "CallbackTestAgent");
        var harness = new MAFEvaluationHarness(verbose: false);
        var testCase = new TestCase
        {
            Name = "Callback Test",
            Input = "Translate Hello World to Spanish"
        };

        var toolStartNames = new List<string>();
        var toolCompleteNames = new List<string>();
        var textChunks = new List<string>();

        var streamingOptions = new StreamingOptions
        {
            OnToolStart = record => toolStartNames.Add(record.Name),
            OnToolComplete = record => toolCompleteNames.Add(record.Name),
            OnTextChunk = text => textChunks.Add(text)
        };

        var options = new EvaluationOptions
        {
            TrackTools = true,
            TrackPerformance = true,
            EvaluateResponse = false
        };

        // Act
        var result = await harness.RunEvaluationStreamingAsync(
            adapter, testCase, streamingOptions, options);

        // Assert: callbacks were fired
        Assert.Single(toolStartNames);
        Assert.Equal("Translate", toolStartNames[0]);

        Assert.Single(toolCompleteNames);
        Assert.Equal("Translate", toolCompleteNames[0]);

        Assert.True(textChunks.Count >= 1, "Expected at least one text chunk callback");
    }

    [Fact]
    public async Task RunEvaluationStreamingAsync_ChatClientNoTools_ReturnsEmptyToolUsage()
    {
        // Arrange: text-only streaming
        var mock = MockStreamingChatClient.WithTextOnly("Hello ", "World!");
        var adapter = new ChatClientAgentAdapter(mock, "NoToolsAgent");
        var harness = new MAFEvaluationHarness(verbose: false);
        var testCase = new TestCase
        {
            Name = "No Tools Test",
            Input = "Say hello"
        };
        var options = new EvaluationOptions
        {
            TrackTools = true,
            EvaluateResponse = false
        };

        // Act
        var result = await harness.RunEvaluationStreamingAsync(
            adapter, testCase, streamingOptions: null, options);

        // Assert: ToolUsage exists but is empty
        Assert.NotNull(result.ToolUsage);
        Assert.Equal(0, result.ToolUsage!.Count);

        // Assert: test still passes (text output present)
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task RunEvaluationStreamingAsync_ChatClientWithToolCalls_PopulatesTimeline()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithSingleToolCall(
            toolName: "SearchDB",
            callId: "call_tl_1",
            result: "Found 5 records",
            textBefore: "Searching... ",
            textAfter: "Found 5 records in the database.");

        var adapter = new ChatClientAgentAdapter(mock, "TimelineAgent");
        var harness = new MAFEvaluationHarness(verbose: false);
        var testCase = new TestCase
        {
            Name = "Timeline Test",
            Input = "Search for records"
        };
        var options = new EvaluationOptions
        {
            TrackTools = true,
            TrackPerformance = true,
            EvaluateResponse = false
        };

        // Act
        var result = await harness.RunEvaluationStreamingAsync(
            adapter, testCase, streamingOptions: null, options);

        // Assert: Timeline has tool invocations
        Assert.NotNull(result.Timeline);
        Assert.True(result.Timeline!.Invocations.Count >= 1,
            $"Expected at least 1 tool invocation in timeline, got {result.Timeline.Invocations.Count}");
        Assert.Equal("SearchDB", result.Timeline.Invocations[0].ToolName);
    }
}
