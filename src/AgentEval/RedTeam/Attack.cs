// src/AgentEval/RedTeam/Attack.cs
using AgentEval.RedTeam.Attacks;

namespace AgentEval.RedTeam;

/// <summary>
/// Static factory for creating attack types.
/// Provides convenient access to all built-in attacks.
/// </summary>
/// <remarks>
/// Use this class for the simple API pattern:
/// <code>
/// var result = await agent.ScanAsync([
///     Attack.PromptInjection,
///     Attack.Jailbreak,
///     Attack.PIILeakage
/// ]);
/// </code>
/// </remarks>
public static class Attack
{
    // === Built-in Attack Types ===
    // Lazy singletons for efficient reuse

    private static readonly Lazy<PromptInjectionAttack> _promptInjection = new(() => new PromptInjectionAttack());
    private static readonly Lazy<JailbreakAttack> _jailbreak = new(() => new JailbreakAttack());
    private static readonly Lazy<PIILeakageAttack> _piiLeakage = new(() => new PIILeakageAttack());
    private static readonly Lazy<SystemPromptExtractionAttack> _systemPromptExtraction = new(() => new SystemPromptExtractionAttack());
    private static readonly Lazy<IndirectInjectionAttack> _indirectInjection = new(() => new IndirectInjectionAttack());

    /// <summary>
    /// Prompt Injection attack (OWASP LLM01).
    /// Tests for vulnerability to instruction override attacks.
    /// </summary>
    public static IAttackType PromptInjection => _promptInjection.Value;

    /// <summary>
    /// Jailbreak attack (OWASP LLM01).
    /// Tests for vulnerability to constraint bypass attacks.
    /// </summary>
    public static IAttackType Jailbreak => _jailbreak.Value;

    /// <summary>
    /// PII Leakage attack (OWASP LLM06).
    /// Tests for unauthorized disclosure of personal information.
    /// </summary>
    public static IAttackType PIILeakage => _piiLeakage.Value;

    /// <summary>
    /// System Prompt Extraction attack (OWASP LLM07).
    /// Tests for vulnerability to system prompt disclosure.
    /// </summary>
    public static IAttackType SystemPromptExtraction => _systemPromptExtraction.Value;

    /// <summary>
    /// Indirect Prompt Injection attack (OWASP LLM01).
    /// Tests for vulnerability to attacks via external data sources.
    /// </summary>
    public static IAttackType IndirectInjection => _indirectInjection.Value;

    // === Convenience Methods ===

    /// <summary>
    /// Get all MVP attack types (PromptInjection, Jailbreak, PIILeakage).
    /// </summary>
    /// <exception cref="NotImplementedException">Until attacks are implemented.</exception>
    public static IReadOnlyList<IAttackType> AllMvp =>
        [PromptInjection, Jailbreak, PIILeakage];

    /// <summary>
    /// Get all built-in attack types.
    /// </summary>
    /// <exception cref="NotImplementedException">Until attacks are implemented.</exception>
    public static IReadOnlyList<IAttackType> All =>
        [PromptInjection, Jailbreak, PIILeakage, SystemPromptExtraction, IndirectInjection];

    /// <summary>
    /// Get attack type by name (case-insensitive).
    /// </summary>
    /// <param name="name">Attack name (e.g., "PromptInjection").</param>
    /// <returns>The attack type, or null if not found.</returns>
    public static IAttackType? ByName(string name)
    {
        return name?.ToUpperInvariant() switch
        {
            "PROMPTINJECTION" or "PROMPT_INJECTION" => PromptInjection,
            "JAILBREAK" => Jailbreak,
            "PIILEAKAGE" or "PII_LEAKAGE" => PIILeakage,
            "SYSTEMPROMPTEXTRACTION" or "SYSTEM_PROMPT_EXTRACTION" => SystemPromptExtraction,
            "INDIRECTINJECTION" or "INDIRECT_INJECTION" => IndirectInjection,
            _ => null
        };
    }

    /// <summary>
    /// Get attack types by OWASP LLM ID.
    /// </summary>
    /// <param name="owaspId">OWASP LLM ID (e.g., "LLM01").</param>
    /// <returns>Attack types matching the OWASP ID.</returns>
    public static IReadOnlyList<IAttackType> ByOwaspId(string owaspId)
    {
        // Once implemented, this will filter All by OwaspLlmId property.
        // For now, we can define the mapping statically:
        return owaspId?.ToUpperInvariant() switch
        {
            "LLM01" => [PromptInjection, Jailbreak, IndirectInjection],
            "LLM06" => [PIILeakage],
            "LLM07" => [SystemPromptExtraction],
            _ => []
        };
    }

    // === Attack Names for Convenience ===

    /// <summary>Available attack names for autocomplete/validation.</summary>
    public static IReadOnlyList<string> AvailableNames =>
        ["PromptInjection", "Jailbreak", "PIILeakage", "SystemPromptExtraction", "IndirectInjection"];

    /// <summary>MVP attack names.</summary>
    public static IReadOnlyList<string> MvpNames =>
        ["PromptInjection", "Jailbreak", "PIILeakage"];
}
