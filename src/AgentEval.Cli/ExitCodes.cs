// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

namespace AgentEval.Cli;

/// <summary>
/// Exit codes for CI/CD integration.
/// </summary>
internal static class ExitCodes
{
    /// <summary>All tests passed.</summary>
    public const int Success = 0;

    /// <summary>One or more tests failed.</summary>
    public const int TestFailure = 1;

    /// <summary>CLI usage error (bad arguments) — set by System.CommandLine automatically.</summary>
    public const int UsageError = 2;

    /// <summary>Runtime error (connection failure, file not found, etc.).</summary>
    public const int RuntimeError = 3;
}
