// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors
// Licensed under the MIT License.

namespace AgentEval.RedTeam;

/// <summary>
/// Registry for <see cref="IAttackType"/> instances.
/// Enables dynamic registration of custom attack types from extension NuGet packages.
/// </summary>
/// <remarks>
/// <para>
/// Built-in attacks are pre-populated from <see cref="Attack.All"/>.
/// Extension packages can register additional attacks via DI:
/// <code>services.AddSingleton&lt;IAttackType, CustomAttack&gt;();</code>
/// </para>
/// <para>
/// The existing static <see cref="Attack.ByName(string)"/> continues
/// to work — this registry is a parallel, DI-friendly path.
/// </para>
/// </remarks>
public interface IAttackTypeRegistry
{
    /// <summary>
    /// Register an attack type. Overwrites any existing attack with the same name.
    /// </summary>
    /// <param name="attackType">The attack type to register.</param>
    /// <exception cref="ArgumentNullException"><paramref name="attackType"/> is null.</exception>
    void Register(IAttackType attackType);

    /// <summary>
    /// Get an attack type by name (case-insensitive).
    /// </summary>
    /// <param name="name">The attack name.</param>
    /// <returns>The attack type, or null if not registered.</returns>
    IAttackType? Get(string name);

    /// <summary>
    /// Get an attack type by name, throwing if not found.
    /// </summary>
    /// <param name="name">The attack name.</param>
    /// <returns>The attack type.</returns>
    /// <exception cref="KeyNotFoundException">No attack type registered with the given name.</exception>
    IAttackType GetRequired(string name);

    /// <summary>
    /// Get all registered attack types.
    /// </summary>
    IEnumerable<IAttackType> GetAll();

    /// <summary>
    /// Get attack types by OWASP LLM ID (e.g., "LLM01").
    /// </summary>
    /// <param name="owaspId">The OWASP LLM vulnerability identifier.</param>
    /// <returns>Attack types matching the OWASP ID.</returns>
    IEnumerable<IAttackType> GetByOwaspId(string owaspId);

    /// <summary>
    /// Check if an attack type is registered.
    /// </summary>
    /// <param name="name">The attack name (case-insensitive).</param>
    bool Contains(string name);
}
