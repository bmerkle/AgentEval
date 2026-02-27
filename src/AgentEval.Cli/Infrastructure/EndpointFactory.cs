// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgentEval.Cli.Infrastructure;

/// <summary>
/// Creates <see cref="IChatClient"/> instances from CLI endpoint arguments.
/// Supports any OpenAI-compatible endpoint and Azure OpenAI.
/// </summary>
internal static class EndpointFactory
{
    /// <summary>
    /// Creates an <see cref="IChatClient"/> for any OpenAI-compatible endpoint.
    /// Works with OpenAI, Ollama, Groq, vLLM, LM Studio, Together.ai, Fireworks, Mistral, etc.
    /// </summary>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="model">The model/deployment name.</param>
    /// <param name="apiKey">Optional API key (some local providers like Ollama don't require one).</param>
    /// <returns>An <see cref="IChatClient"/> ready for evaluation.</returns>
    public static IChatClient CreateOpenAICompatible(string endpoint, string model, string? apiKey)
    {
        var credential = new ApiKeyCredential(apiKey ?? "no-key-needed");
        var options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
        var client = new OpenAIClient(credential, options);
        return client.GetChatClient(model).AsIChatClient();
    }

    /// <summary>
    /// Creates an <see cref="IChatClient"/> for Azure OpenAI.
    /// Reads from environment variables if endpoint/key not provided.
    /// </summary>
    /// <param name="endpoint">Optional endpoint URL (falls back to AZURE_OPENAI_ENDPOINT env var).</param>
    /// <param name="model">The deployment name.</param>
    /// <param name="apiKey">Optional API key (falls back to AZURE_OPENAI_API_KEY env var).</param>
    /// <returns>An <see cref="IChatClient"/> for Azure OpenAI.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required endpoint or API key is missing.</exception>
    public static IChatClient CreateAzure(string? endpoint, string model, string? apiKey)
    {
        var resolvedEndpoint = endpoint
            ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException(
                "Azure endpoint required. Set --endpoint or AZURE_OPENAI_ENDPOINT env var.");

        var resolvedKey = apiKey
            ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
            ?? throw new InvalidOperationException(
                "Azure API key required. Set --api-key or AZURE_OPENAI_API_KEY env var.");

        var client = new AzureOpenAIClient(
            new Uri(resolvedEndpoint),
            new AzureKeyCredential(resolvedKey));

        return client.GetChatClient(model).AsIChatClient();
    }
}
