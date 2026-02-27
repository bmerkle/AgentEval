// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using AgentEval.Models;

namespace AgentEval.Assertions;

/// <summary>
/// Extension methods to start fluent assertions on model types.
/// </summary>
/// <remarks>
/// These are extension methods that decouple Models/ from Assertions/.
/// Usage: <c>result.Performance!.Should()</c> — requires <c>using AgentEval.Assertions;</c> in scope.
/// </remarks>
public static class ModelAssertionExtensions
{
    /// <summary>Start fluent assertions on performance metrics.</summary>
    public static PerformanceAssertions Should(this PerformanceMetrics metrics) => new(metrics);

    /// <summary>Start fluent assertions on a tool usage report.</summary>
    public static ToolUsageAssertions Should(this ToolUsageReport report) => new(report);
}
