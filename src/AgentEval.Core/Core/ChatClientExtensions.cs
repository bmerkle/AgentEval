// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using Microsoft.Extensions.AI;

namespace AgentEval.Core;

/// <summary>
/// Extension methods for <see cref="IChatClient"/> to simplify AgentEval integration.
/// Parallels the <c>.AsIChatClient()</c> convention from Microsoft.Extensions.AI.
/// </summary>
public static class ChatClientExtensions
{
    /// <summary>
    /// Wraps an <see cref="IChatClient"/> as an <see cref="IStreamableAgent"/> for evaluation.
    /// This is the primary entry point for evaluating any OpenAI-compatible endpoint,
    /// Ollama, LM Studio, vLLM, Groq, Together.ai, Mistral, or any other provider
    /// that implements <see cref="IChatClient"/>.
    /// </summary>
    /// <param name="chatClient">The chat client to wrap.</param>
    /// <param name="name">Name for the agent (used in reports and comparison tables).</param>
    /// <param name="systemPrompt">Optional system prompt prepended to each request.</param>
    /// <param name="chatOptions">Optional chat options (temperature, max tokens, tools, etc.).</param>
    /// <param name="includeHistory">Whether to maintain conversation history across calls.</param>
    /// <returns>An <see cref="IStreamableAgent"/> wrapping the chat client.</returns>
    /// <example>
    /// <code>
    /// // Azure OpenAI
    /// var agent = chatClient.AsEvaluableAgent("GPT-4o", "You are helpful.");
    ///
    /// // Ollama (local)
    /// var agent = ollamaClient.AsEvaluableAgent("Llama3");
    ///
    /// // Any OpenAI-compatible endpoint
    /// var agent = myClient.AsEvaluableAgent("MyModel", systemPrompt: "Be concise.");
    /// </code>
    /// </example>
    public static IStreamableAgent AsEvaluableAgent(
        this IChatClient chatClient,
        string name = "Agent",
        string? systemPrompt = null,
        ChatOptions? chatOptions = null,
        bool includeHistory = false)
    {
        return new ChatClientAgentAdapter(chatClient, name, systemPrompt, chatOptions, includeHistory);
    }
}
