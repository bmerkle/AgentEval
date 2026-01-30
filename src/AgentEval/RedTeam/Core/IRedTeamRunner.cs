// src/AgentEval/RedTeam/Core/IRedTeamRunner.cs
using AgentEval.Core;

namespace AgentEval.RedTeam;

/// <summary>
/// Core interface for executing red team attacks against an agent.
/// </summary>
public interface IRedTeamRunner
{
    /// <summary>
    /// Runs a red team scan using specified options.
    /// </summary>
    /// <param name="agent">The agent to scan.</param>
    /// <param name="options">Scan configuration options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregate result of all attacks.</returns>
    Task<RedTeamResult> ScanAsync(
        IEvaluableAgent agent,
        ScanOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a red team scan with progress reporting.
    /// </summary>
    /// <param name="agent">The agent to scan.</param>
    /// <param name="options">Scan configuration options.</param>
    /// <param name="progress">Progress reporter for UI updates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregate result of all attacks.</returns>
    Task<RedTeamResult> ScanAsync(
        IEvaluableAgent agent,
        ScanOptions options,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Progress update during a red team scan.
/// </summary>
/// <param name="CompletedProbes">Number of probes completed so far.</param>
/// <param name="TotalProbes">Total number of probes to execute.</param>
/// <param name="CurrentAttack">Name of the current attack being executed.</param>
/// <param name="CurrentProbe">ID of the current probe being executed.</param>
/// <param name="Elapsed">Time elapsed since scan started.</param>
public readonly record struct ScanProgress(
    int CompletedProbes,
    int TotalProbes,
    string CurrentAttack,
    string CurrentProbe,
    TimeSpan Elapsed)
{
    /// <summary>Percentage of scan completed (0-100).</summary>
    public double PercentComplete => TotalProbes > 0
        ? (CompletedProbes * 100.0 / TotalProbes)
        : 0.0;

    /// <summary>Estimated time remaining based on current pace.</summary>
    public TimeSpan? EstimatedRemaining => CompletedProbes > 0
        ? TimeSpan.FromTicks(Elapsed.Ticks * (TotalProbes - CompletedProbes) / CompletedProbes)
        : null;
}
