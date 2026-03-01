// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

#if NET9_0_OR_GREATER

using System.CommandLine;
using AgentEval.Cli.Commands;

namespace AgentEval.Tests.Cli;

/// <summary>
/// Tests for the CLI root command (Program.cs) — verifies all subcommands are registered.
/// </summary>
public class ProgramTests
{
    private static RootCommand CreateRootCommand()
    {
        // Mirror exactly what Program.cs does
        return new RootCommand("AgentEval — evaluate AI agents from the command line")
        {
            EvalCommand.Create(),
            InitCommand.Create(),
            ListCommand.Create(),
            RedTeamCommand.Create(),
        };
    }

    [Fact]
    public void RootCommand_Has4Subcommands()
    {
        var root = CreateRootCommand();
        Assert.Equal(4, root.Subcommands.Count);
    }

    [Fact]
    public void RootCommand_HasEvalCommand()
    {
        var root = CreateRootCommand();
        Assert.Contains(root.Subcommands, c => c.Name == "eval");
    }

    [Fact]
    public void RootCommand_HasInitCommand()
    {
        var root = CreateRootCommand();
        Assert.Contains(root.Subcommands, c => c.Name == "init");
    }

    [Fact]
    public void RootCommand_HasListCommand()
    {
        var root = CreateRootCommand();
        Assert.Contains(root.Subcommands, c => c.Name == "list");
    }

    [Fact]
    public void RootCommand_HasRedTeamCommand()
    {
        var root = CreateRootCommand();
        Assert.Contains(root.Subcommands, c => c.Name == "redteam");
    }

    [Fact]
    public void RootCommand_Description_ContainsAgentEval()
    {
        var root = CreateRootCommand();
        Assert.Contains("AgentEval", root.Description);
    }

    [Theory]
    [InlineData("eval")]
    [InlineData("init")]
    [InlineData("list")]
    [InlineData("redteam")]
    public void AllCommands_HaveDescriptions(string commandName)
    {
        var root = CreateRootCommand();
        var command = root.Subcommands.First(c => c.Name == commandName);
        Assert.False(string.IsNullOrWhiteSpace(command.Description),
            $"Command '{commandName}' should have a description");
    }
}

#endif
