// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using AgentEval.Core;

namespace AgentEval.MAF;

/// <summary>
/// Adapts a Microsoft Agent Framework (MAF) AIAgent for testing with AgentEval.
/// </summary>
public class MAFAgentAdapter : IStreamableAgent
{
    private readonly AIAgent _agent;
    private AgentThread? _thread;
    
    /// <summary>
    /// Create an adapter for an AIAgent.
    /// </summary>
    /// <param name="agent">The MAF agent to adapt.</param>
    /// <param name="thread">Optional thread for conversation context. If null, a new thread is created per invocation.</param>
    public MAFAgentAdapter(AIAgent agent, AgentThread? thread = null)
    {
        _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        _thread = thread;
    }
    
    /// <inheritdoc/>
    public string Name => _agent.Name ?? string.Empty;
    
    /// <inheritdoc/>
    public async Task<AgentResponse> InvokeAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var thread = _thread ?? _agent.GetNewThread();
        var response = await _agent.RunAsync(prompt, thread, cancellationToken: cancellationToken);
        
        // Extract token usage from AgentRunResponse.Usage property
        TokenUsage? tokenUsage = null;
        if (response.Usage != null)
        {
            tokenUsage = new TokenUsage
            {
                PromptTokens = (int)(response.Usage.InputTokenCount ?? 0),
                CompletionTokens = (int)(response.Usage.OutputTokenCount ?? 0)
            };
        }
        
        return new AgentResponse
        {
            Text = response.Text,
            RawMessages = response.Messages.ToList(),
            TokenUsage = tokenUsage
        };
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<AgentResponseChunk> InvokeStreamingAsync(
        string prompt, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var thread = _thread ?? _agent.GetNewThread();
        TokenUsage? capturedUsage = null;
        
        await foreach (var update in _agent.RunStreamingAsync(prompt, thread, cancellationToken: cancellationToken))
        {
            foreach (var content in update.Contents)
            {
                switch (content)
                {
                    case TextContent text when !string.IsNullOrEmpty(text.Text):
                        yield return new AgentResponseChunk { Text = text.Text };
                        break;
                    
                    case FunctionCallContent call:
                        yield return new AgentResponseChunk
                        {
                            ToolCallStarted = new ToolCallInfo
                            {
                                Name = call.Name,
                                CallId = call.CallId,
                                Arguments = call.Arguments
                            }
                        };
                        break;
                    
                    case FunctionResultContent result:
                        yield return new AgentResponseChunk
                        {
                            ToolCallCompleted = new ToolResultInfo
                            {
                                CallId = result.CallId,
                                Result = result.Result,
                                Exception = result.Exception
                            }
                        };
                        break;
                    
                    // Check for UsageContent if provider sends it in streaming
                    case UsageContent usage:
                        capturedUsage = new TokenUsage
                        {
                            PromptTokens = (int)(usage.Details.InputTokenCount ?? 0),
                            CompletionTokens = (int)(usage.Details.OutputTokenCount ?? 0)
                        };
                        break;
                }
            }
        }
        
        yield return new AgentResponseChunk { IsComplete = true, Usage = capturedUsage };
    }
    
    /// <summary>
    /// Reset the conversation thread.
    /// </summary>
    public void ResetThread()
    {
        _thread = _agent.GetNewThread();
    }
    
    /// <summary>
    /// Get a new thread for fresh conversations.
    /// </summary>
    public AgentThread GetNewThread() => _agent.GetNewThread();
}
