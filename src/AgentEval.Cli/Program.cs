// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using AgentEval.Cli.Commands;

var root = new RootCommand("AgentEval — evaluate AI agents from the command line")
{
    EvalCommand.Create(),
    // Phase 2: CompareCommand.Create(),
    // Phase 3: QuickCommand.Create(),
};

var parseResult = root.Parse(args);
return await parseResult.InvokeAsync();
