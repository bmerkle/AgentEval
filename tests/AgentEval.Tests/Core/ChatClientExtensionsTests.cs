// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using AgentEval.Core;
using AgentEval.Tests.TestHelpers;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgentEval.Tests.Core;

/// <summary>
/// Tests for the <c>AsEvaluableAgent</c> extension method on <see cref="IChatClient"/>.
/// Validates the one-liner DX bridge from IChatClient to IStreamableAgent.
/// </summary>
public class ChatClientExtensionsTests
{
    [Fact]
    public void AsEvaluableAgent_ReturnsChatClientAgentAdapter()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithTextOnly("Hello");

        // Act
        var agent = mock.AsEvaluableAgent("TestAgent");

        // Assert
        Assert.IsType<ChatClientAgentAdapter>(agent);
        Assert.Equal("TestAgent", agent.Name);
    }

    [Fact]
    public void AsEvaluableAgent_DefaultName_IsAgent()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithTextOnly("Hello");

        // Act
        var agent = mock.AsEvaluableAgent();

        // Assert
        Assert.Equal("Agent", agent.Name);
    }

    [Fact]
    public async Task AsEvaluableAgent_InvokeAsync_ReturnsText()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithTextOnly("The capital of France is Paris.");
        var agent = mock.AsEvaluableAgent("GPT-4o", "You are a helpful assistant.");

        // Act
        var response = await agent.InvokeAsync("What is the capital of France?");

        // Assert
        Assert.Contains("Paris", response.Text);
    }

    [Fact]
    public async Task AsEvaluableAgent_InvokeStreamingAsync_YieldsChunks()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithTextOnly("Hello ", "World!");
        var agent = mock.AsEvaluableAgent("StreamAgent");

        // Act
        var chunks = new List<AgentResponseChunk>();
        await foreach (var chunk in agent.InvokeStreamingAsync("Hi"))
        {
            chunks.Add(chunk);
        }

        // Assert: text chunks + final complete chunk
        var textChunks = chunks.Where(c => !string.IsNullOrEmpty(c.Text)).ToList();
        Assert.Equal(2, textChunks.Count);
        Assert.Equal("Hello ", textChunks[0].Text);
        Assert.Equal("World!", textChunks[1].Text);
        Assert.Contains(chunks, c => c.IsComplete);
    }

    [Fact]
    public async Task AsEvaluableAgent_WithToolCalls_StreamingYieldsToolChunks()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithSingleToolCall(
            toolName: "GetWeather",
            callId: "call_ext_1",
            result: "72°F",
            textBefore: "Let me check. ",
            textAfter: "It's 72°F.");
        var agent = mock.AsEvaluableAgent("ToolAgent");

        // Act
        var chunks = new List<AgentResponseChunk>();
        await foreach (var chunk in agent.InvokeStreamingAsync("Weather?"))
        {
            chunks.Add(chunk);
        }

        // Assert: tool chunks present
        Assert.Contains(chunks, c => c.ToolCallStarted != null);
        Assert.Contains(chunks, c => c.ToolCallCompleted != null);
        Assert.Equal("GetWeather", chunks.First(c => c.ToolCallStarted != null).ToolCallStarted!.Name);
    }

    [Fact]
    public async Task AsEvaluableAgent_WithToolCalls_NonStreamingExtractsViaRawMessages()
    {
        // Arrange
        var callId = "call_ext_ns";
        var funcCall = new FunctionCallContent(callId, "SearchDB",
            new Dictionary<string, object?> { { "query", "test" } });
        var funcResult = new FunctionResultContent(callId, "Found 3 results");

        var chunks = new List<List<AIContent>>
        {
            new List<AIContent> { funcCall },
            new List<AIContent> { funcResult },
            new List<AIContent> { new TextContent("I found 3 results.") }
        };
        var mock = new MockStreamingChatClient(chunks);
        var agent = mock.AsEvaluableAgent("NonStreamingToolAgent");

        // Act
        var response = await agent.InvokeAsync("Search for test");

        // Assert: RawMessages contain function content for ToolUsageExtractor
        Assert.NotNull(response.RawMessages);
        Assert.True(response.RawMessages.Count > 0);

        var toolReport = ToolUsageExtractor.Extract(response.RawMessages);
        Assert.Equal(1, toolReport.Count);
        Assert.Equal("SearchDB", toolReport.ToolNames.First());
    }

    [Fact]
    public void AsEvaluableAgent_NullChatClient_Throws()
    {
        // Arrange
        IChatClient? client = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => client!.AsEvaluableAgent());
    }

    [Fact]
    public void AsEvaluableAgent_ReturnsIStreamableAgent()
    {
        // Arrange
        var mock = MockStreamingChatClient.WithTextOnly("Hi");

        // Act
        IStreamableAgent agent = mock.AsEvaluableAgent("Test");

        // Assert: both IEvaluableAgent and IStreamableAgent
        Assert.IsAssignableFrom<IEvaluableAgent>(agent);
        Assert.IsAssignableFrom<IStreamableAgent>(agent);
    }
}
