// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AgentEval.Tests.TestHelpers;

/// <summary>
/// Mock IChatClient that supports configurable streaming responses containing
/// text, tool calls (FunctionCallContent/FunctionResultContent), and usage data.
/// Used to test ChatClientAgentAdapter's streaming tool extraction.
/// </summary>
internal sealed class MockStreamingChatClient : IChatClient
{
    private readonly List<List<AIContent>> _streamingChunks;
    private readonly string? _nonStreamingResponse;

    /// <summary>
    /// All prompts received by this mock.
    /// </summary>
    public List<IEnumerable<ChatMessage>> ReceivedMessages { get; } = new();

    /// <summary>
    /// Creates a mock that streams the given content chunks.
    /// Each inner list represents one ChatResponseUpdate's Contents.
    /// </summary>
    /// <param name="streamingChunks">
    /// Sequence of content lists. Each list becomes one <see cref="ChatResponseUpdate"/>.
    /// </param>
    /// <param name="nonStreamingResponse">
    /// Optional response text for <see cref="GetResponseAsync"/>. 
    /// If null, returns tool call messages matching the streaming chunks.
    /// </param>
    public MockStreamingChatClient(
        List<List<AIContent>> streamingChunks,
        string? nonStreamingResponse = null)
    {
        _streamingChunks = streamingChunks;
        _nonStreamingResponse = nonStreamingResponse;
    }

    /// <inheritdoc />
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ReceivedMessages.Add(messages.ToList());

        var responseMessages = new List<ChatMessage>();

        if (_nonStreamingResponse != null)
        {
            responseMessages.Add(new ChatMessage(ChatRole.Assistant, _nonStreamingResponse));
        }
        else
        {
            // Build response with tool call messages from the chunk configuration
            foreach (var chunkContents in _streamingChunks)
            {
                foreach (var content in chunkContents)
                {
                    switch (content)
                    {
                        case FunctionCallContent call:
                        {
                            var msg = new ChatMessage(ChatRole.Assistant, $"Calling {call.Name}");
                            msg.Contents.Add(call);
                            responseMessages.Add(msg);
                            break;
                        }
                        case FunctionResultContent result:
                        {
                            var msg = new ChatMessage(ChatRole.Tool, new AIContent[] { result });
                            responseMessages.Add(msg);
                            break;
                        }
                    }
                }
            }

            // Add final text response
            var textParts = _streamingChunks
                .SelectMany(c => c)
                .OfType<TextContent>()
                .Select(t => t.Text);
            var fullText = string.Join("", textParts);
            if (!string.IsNullOrEmpty(fullText))
            {
                responseMessages.Add(new ChatMessage(ChatRole.Assistant, fullText));
            }
        }

        var response = new ChatResponse(responseMessages);
        return Task.FromResult(response);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ReceivedMessages.Add(messages.ToList());

        foreach (var chunkContents in _streamingChunks)
        {
            await Task.Yield(); // Simulate async streaming

            var update = new ChatResponseUpdate
            {
                Role = ChatRole.Assistant
            };

            foreach (var content in chunkContents)
            {
                update.Contents.Add(content);
            }

            yield return update;
        }
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? key = null) => null;

    /// <inheritdoc />
    public void Dispose() { }

    // ═══════════════════════════════════════════════════════════════════
    // BUILDER HELPERS — Fluent API for constructing streaming sequences
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a mock that streams only text content.
    /// </summary>
    public static MockStreamingChatClient WithTextOnly(params string[] textChunks)
    {
        var chunks = textChunks
            .Select(t => new List<AIContent> { new TextContent(t) })
            .ToList();
        return new MockStreamingChatClient(chunks);
    }

    /// <summary>
    /// Creates a mock that streams text followed by a single tool call and result.
    /// </summary>
    public static MockStreamingChatClient WithSingleToolCall(
        string toolName,
        string callId,
        IDictionary<string, object?>? arguments = null,
        object? result = null,
        string? textBefore = null,
        string? textAfter = null)
    {
        var chunks = new List<List<AIContent>>();

        if (textBefore != null)
            chunks.Add(new List<AIContent> { new TextContent(textBefore) });

        chunks.Add(new List<AIContent> { new FunctionCallContent(callId, toolName, arguments) });
        chunks.Add(new List<AIContent> { new FunctionResultContent(callId, result ?? $"{toolName} result") });

        if (textAfter != null)
            chunks.Add(new List<AIContent> { new TextContent(textAfter) });

        return new MockStreamingChatClient(chunks);
    }

    /// <summary>
    /// Creates a mock that streams multiple sequential tool calls with results.
    /// </summary>
    public static MockStreamingChatClient WithMultipleToolCalls(
        params (string ToolName, string CallId, object? Result)[] toolCalls)
    {
        var chunks = new List<List<AIContent>>();

        foreach (var (toolName, callId, result) in toolCalls)
        {
            chunks.Add(new List<AIContent> { new FunctionCallContent(callId, toolName) });
            chunks.Add(new List<AIContent> { new FunctionResultContent(callId, result ?? $"{toolName} result") });
        }

        return new MockStreamingChatClient(chunks);
    }

    /// <summary>
    /// Creates a mock that streams text, tool calls, and usage info — a complete realistic scenario.
    /// </summary>
    public static MockStreamingChatClient WithFullScenario(
        string initialText,
        (string ToolName, string CallId, object? Result)[] toolCalls,
        string finalText,
        int inputTokens = 100,
        int outputTokens = 50)
    {
        var chunks = new List<List<AIContent>>
        {
            new List<AIContent> { new TextContent(initialText) }
        };

        foreach (var (toolName, callId, result) in toolCalls)
        {
            chunks.Add(new List<AIContent> { new FunctionCallContent(callId, toolName) });
            chunks.Add(new List<AIContent> { new FunctionResultContent(callId, result ?? $"{toolName} result") });
        }

        chunks.Add(new List<AIContent> { new TextContent(finalText) });
        chunks.Add(new List<AIContent> { new UsageContent(new UsageDetails
        {
            InputTokenCount = inputTokens,
            OutputTokenCount = outputTokens
        }) });

        return new MockStreamingChatClient(chunks);
    }
}
