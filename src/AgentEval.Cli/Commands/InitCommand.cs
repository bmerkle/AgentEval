// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;

namespace AgentEval.Cli.Commands;

/// <summary>
/// The 'agenteval init' command — scaffold a starter evaluation dataset.
/// </summary>
internal static class InitCommand
{
    public static Command Create()
    {
        var command = new Command("init", "Initialize a starter evaluation dataset in the current directory");

        var formatOpt = new Option<string>("--format")
            { DefaultValueFactory = _ => "yaml", Description = "Output format: yaml or json" };

        var outputOpt = new Option<string?>("-o", "--output")
            { Description = "Output file path (default: agenteval.{format})" };

        var forceFlag = new Option<bool>("--force")
            { Description = "Overwrite existing file" };

        command.Options.Add(formatOpt);
        command.Options.Add(outputOpt);
        command.Options.Add(forceFlag);

        command.SetAction(async (parseResult, ct) =>
        {
            var format = parseResult.GetValue(formatOpt)!;
            var output = parseResult.GetValue(outputOpt);
            var force = parseResult.GetValue(forceFlag);

            return await ExecuteAsync(format, output, force);
        });

        return command;
    }

    /// <summary>
    /// Core execution logic — separated from command wiring for testability.
    /// </summary>
    internal static async Task<int> ExecuteAsync(string format, string? output, bool force)
    {
        var normalizedFormat = format.ToLowerInvariant();
        if (normalizedFormat is not ("yaml" or "json"))
        {
            Console.Error.WriteLine($"Error: Unsupported format '{format}'. Use 'yaml' or 'json'.");
            return ExitCodes.UsageError;
        }

        var fileName = output ?? $"agenteval.{normalizedFormat}";

        if (File.Exists(fileName) && !force)
        {
            Console.Error.WriteLine($"Error: {fileName} already exists. Use --force to overwrite.");
            return ExitCodes.UsageError;
        }

        var template = normalizedFormat switch
        {
            "yaml" => GetYamlTemplate(),
            "json" => GetJsonTemplate(),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };

        // Ensure parent directory exists
        var dir = Path.GetDirectoryName(Path.GetFullPath(fileName));
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(fileName, template);
        Console.Error.WriteLine($"  Created {fileName} with sample test cases.");
        Console.Error.WriteLine($"  Edit the file and run: agenteval eval --azure --model <model> --dataset {fileName}");
        return ExitCodes.Success;
    }

    private static string GetYamlTemplate() => """
        # AgentEval Evaluation Dataset
        # Documentation: https://agenteval.dev/docs/getting-started
        #
        # Each test case has:
        #   - id: Unique test identifier
        #   - input: The prompt sent to the agent
        #   - expectedOutput: (optional) Expected response for comparison
        #   - context: (optional) Retrieved context for RAG evaluation
        #   - groundTruth: (optional) Ground truth for faithfulness metrics
        #   - tags: (optional) Tags for filtering and grouping
        
        - id: greeting_test
          input: "Hello, how are you?"
          expectedOutput: "A friendly greeting response"
          tags: [basic, greeting]
        
        - id: knowledge_test
          input: "What is the capital of France?"
          expectedOutput: "Paris"
          context: "France is a country in Western Europe. Its capital is Paris."
          groundTruth: "The capital of France is Paris."
          tags: [knowledge, geography]
        
        - id: reasoning_test
          input: "If a train travels 60mph for 2 hours, how far does it go?"
          expectedOutput: "120 miles"
          tags: [reasoning, math]
        """;

    private static string GetJsonTemplate() => """
        [
          {
            "id": "greeting_test",
            "input": "Hello, how are you?",
            "expectedOutput": "A friendly greeting response",
            "tags": ["basic", "greeting"]
          },
          {
            "id": "knowledge_test",
            "input": "What is the capital of France?",
            "expectedOutput": "Paris",
            "context": "France is a country in Western Europe. Its capital is Paris.",
            "groundTruth": "The capital of France is Paris.",
            "tags": ["knowledge", "geography"]
          },
          {
            "id": "reasoning_test",
            "input": "If a train travels 60mph for 2 hours, how far does it go?",
            "expectedOutput": "120 miles",
            "tags": ["reasoning", "math"]
          }
        ]
        """;
}
