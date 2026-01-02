// Copyright (c) 2025-2026 AgentEval Contributors
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace AgentEval.Assertions;

/// <summary>
/// Fluent assertions for response text.
/// </summary>
public class ResponseAssertions
{
    private readonly string _response;
    
    public ResponseAssertions(string response)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
    }
    
    /// <summary>Assert response contains a substring (case-insensitive by default).</summary>
    public ResponseAssertions Contain(string substring, bool caseSensitive = false)
    {
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (!_response.Contains(substring, comparison))
        {
            throw new ResponseAssertionException(
                $"Expected response to contain '{substring}', but it did not.\n" +
                $"Response preview: {Truncate(_response, 200)}");
        }
        return this;
    }
    
    /// <summary>Assert response contains all specified substrings.</summary>
    public ResponseAssertions ContainAll(params string[] substrings)
    {
        var missing = substrings.Where(s => 
            !_response.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (missing.Count > 0)
        {
            throw new ResponseAssertionException(
                $"Expected response to contain all of [{string.Join(", ", substrings)}], " +
                $"but missing: [{string.Join(", ", missing)}]");
        }
        return this;
    }
    
    /// <summary>Assert response contains any of the specified substrings.</summary>
    public ResponseAssertions ContainAny(params string[] substrings)
    {
        if (!substrings.Any(s => _response.Contains(s, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ResponseAssertionException(
                $"Expected response to contain at least one of [{string.Join(", ", substrings)}], but none found.");
        }
        return this;
    }
    
    /// <summary>Assert response does NOT contain a substring.</summary>
    public ResponseAssertions NotContain(string substring, bool caseSensitive = false)
    {
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (_response.Contains(substring, comparison))
        {
            throw new ResponseAssertionException(
                $"Expected response NOT to contain '{substring}', but it did.");
        }
        return this;
    }
    
    /// <summary>Assert response matches a regex pattern.</summary>
    public ResponseAssertions MatchPattern(string pattern, RegexOptions options = RegexOptions.IgnoreCase)
    {
        if (!Regex.IsMatch(_response, pattern, options))
        {
            throw new ResponseAssertionException(
                $"Expected response to match pattern '{pattern}', but it did not.\n" +
                $"Response preview: {Truncate(_response, 200)}");
        }
        return this;
    }
    
    /// <summary>Assert response has length within a range.</summary>
    public ResponseAssertions HaveLengthBetween(int min, int max)
    {
        if (_response.Length < min || _response.Length > max)
        {
            throw new ResponseAssertionException(
                $"Expected response length between {min} and {max}, but was {_response.Length}");
        }
        return this;
    }
    
    /// <summary>Assert response has at least N characters.</summary>
    public ResponseAssertions HaveLengthAtLeast(int min)
    {
        if (_response.Length < min)
        {
            throw new ResponseAssertionException(
                $"Expected response length at least {min}, but was {_response.Length}");
        }
        return this;
    }
    
    /// <summary>Assert response is not empty or whitespace.</summary>
    public ResponseAssertions NotBeEmpty()
    {
        if (string.IsNullOrWhiteSpace(_response))
        {
            throw new ResponseAssertionException("Expected response to not be empty, but it was.");
        }
        return this;
    }
    
    /// <summary>Assert response starts with a prefix.</summary>
    public ResponseAssertions StartWith(string prefix, bool caseSensitive = false)
    {
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (!_response.StartsWith(prefix, comparison))
        {
            throw new ResponseAssertionException(
                $"Expected response to start with '{prefix}', but started with '{Truncate(_response, prefix.Length + 20)}'");
        }
        return this;
    }
    
    /// <summary>Assert response ends with a suffix.</summary>
    public ResponseAssertions EndWith(string suffix, bool caseSensitive = false)
    {
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (!_response.EndsWith(suffix, comparison))
        {
            throw new ResponseAssertionException(
                $"Expected response to end with '{suffix}'");
        }
        return this;
    }
    
    /// <summary>Get the underlying response for custom assertions.</summary>
    public string Response => _response;
    
    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;
        return text[..(maxLength - 3)] + "...";
    }
}

/// <summary>
/// Extension method to start response assertions.
/// </summary>
public static class ResponseAssertionExtensions
{
    /// <summary>Start fluent assertions on a response string.</summary>
    public static ResponseAssertions Should(this string response) => new(response);
}
