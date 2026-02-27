// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

using AgentEval.Core;
using AgentEval.Exporters;
using AgentEval.Models;

namespace AgentEval.Cli.Infrastructure;

/// <summary>
/// Routes evaluation reports to the correct exporter and output destination.
/// Supports format aliases (e.g., "xml" → JUnit, "md" → Markdown).
/// </summary>
internal static class ExportHandler
{
    private static readonly Dictionary<string, ExportFormat> s_formatMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["json"] = ExportFormat.Json,
        ["junit"] = ExportFormat.Junit,
        ["xml"] = ExportFormat.Junit,       // alias
        ["markdown"] = ExportFormat.Markdown,
        ["md"] = ExportFormat.Markdown,     // alias
        ["trx"] = ExportFormat.Trx,
        ["csv"] = ExportFormat.Csv,
    };

    /// <summary>
    /// Exports the evaluation report to the specified format and destination.
    /// When <paramref name="outputFile"/> is null, writes to stdout for piping.
    /// </summary>
    /// <param name="report">The evaluation report to export.</param>
    /// <param name="format">Format name (json, junit, xml, markdown, md, trx, csv).</param>
    /// <param name="outputFile">Optional output file. When null, writes to stdout.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when format is unknown.</exception>
    public static async Task ExportAsync(
        EvaluationReport report,
        string format,
        FileInfo? outputFile,
        CancellationToken ct)
    {
        if (!s_formatMap.TryGetValue(format, out var exportFormat))
            throw new ArgumentException(
                $"Unknown format '{format}'. Valid formats: {string.Join(", ", s_formatMap.Keys.Order())}",
                nameof(format));

        var exporter = ResultExporterFactory.Create(exportFormat);

        if (outputFile is not null)
        {
            // Write to file — ensure parent directory exists
            outputFile.Directory?.Create();
            await using var stream = outputFile.Create();
            await exporter.ExportAsync(report, stream, ct);
        }
        else
        {
            // Write to stdout for piping (agenteval eval ... | jq .)
            await using var stream = Console.OpenStandardOutput();
            await exporter.ExportAsync(report, stream, ct);
        }
    }

    /// <summary>
    /// Returns the list of supported format names (for error messages).
    /// </summary>
    internal static IReadOnlyCollection<string> SupportedFormats => s_formatMap.Keys;
}
