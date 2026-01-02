// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

namespace AgentEval.Assertions;

/// <summary>
/// Base exception for all AgentEval assertion failures.
/// </summary>
public class AgentEvalAssertionException : Exception
{
    public AgentEvalAssertionException(string message) : base(message) { }
    public AgentEvalAssertionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception thrown when a tool usage assertion fails.
/// </summary>
public class ToolAssertionException : AgentEvalAssertionException
{
    public ToolAssertionException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a performance assertion fails.
/// </summary>
public class PerformanceAssertionException : AgentEvalAssertionException
{
    public PerformanceAssertionException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a response assertion fails.
/// </summary>
public class ResponseAssertionException : AgentEvalAssertionException
{
    public ResponseAssertionException(string message) : base(message) { }
}
