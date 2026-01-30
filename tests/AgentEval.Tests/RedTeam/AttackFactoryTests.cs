// tests/AgentEval.Tests/RedTeam/AttackFactoryTests.cs
using AgentEval.RedTeam;
using AgentEval.RedTeam.Attacks;

namespace AgentEval.Tests.RedTeam;

/// <summary>
/// Tests for the Attack static factory class.
/// </summary>
public class AttackFactoryTests
{
    [Fact]
    public void AvailableNames_ContainsMvpAttacks()
    {
        Assert.Contains("PromptInjection", Attack.AvailableNames);
        Assert.Contains("Jailbreak", Attack.AvailableNames);
        Assert.Contains("PIILeakage", Attack.AvailableNames);
    }

    [Fact]
    public void AvailableNames_ContainsAllBuiltInAttacks()
    {
        Assert.Equal(5, Attack.AvailableNames.Count);
        Assert.Contains("SystemPromptExtraction", Attack.AvailableNames);
        Assert.Contains("IndirectInjection", Attack.AvailableNames);
    }

    [Fact]
    public void MvpNames_ContainsExactlyThreeAttacks()
    {
        Assert.Equal(3, Attack.MvpNames.Count);
        Assert.Contains("PromptInjection", Attack.MvpNames);
        Assert.Contains("Jailbreak", Attack.MvpNames);
        Assert.Contains("PIILeakage", Attack.MvpNames);
    }

    // === PromptInjection and Jailbreak are now implemented (P1A) ===

    [Theory]
    [InlineData("PromptInjection")]
    [InlineData("PROMPTINJECTION")]
    [InlineData("promptinjection")]
    [InlineData("Prompt_Injection")]
    public void ByName_PromptInjection_IsCaseInsensitive(string name)
    {
        var attack = Attack.ByName(name);
        Assert.NotNull(attack);
        Assert.Equal("PromptInjection", attack.Name);
    }

    [Theory]
    [InlineData("Jailbreak")]
    [InlineData("JAILBREAK")]
    [InlineData("jailbreak")]
    public void ByName_Jailbreak_IsCaseInsensitive(string name)
    {
        var attack = Attack.ByName(name);
        Assert.NotNull(attack);
        Assert.Equal("Jailbreak", attack.Name);
    }

    [Fact]
    public void ByName_UnknownName_ReturnsNull()
    {
        Assert.Null(Attack.ByName("UnknownAttack"));
        Assert.Null(Attack.ByName(""));
        Assert.Null(Attack.ByName(null!));
    }

    [Fact]
    public void PromptInjection_ReturnsPromptInjectionAttack()
    {
        var attack = Attack.PromptInjection;
        Assert.NotNull(attack);
        Assert.IsType<PromptInjectionAttack>(attack);
        Assert.Equal("PromptInjection", attack.Name);
        Assert.Equal("LLM01", attack.OwaspLlmId);
    }

    [Fact]
    public void PromptInjection_ReturnsSameInstance()
    {
        var attack1 = Attack.PromptInjection;
        var attack2 = Attack.PromptInjection;
        Assert.Same(attack1, attack2);
    }

    [Fact]
    public void Jailbreak_ReturnsJailbreakAttack()
    {
        var attack = Attack.Jailbreak;
        Assert.NotNull(attack);
        Assert.IsType<JailbreakAttack>(attack);
        Assert.Equal("Jailbreak", attack.Name);
        Assert.Equal("LLM01", attack.OwaspLlmId);
    }

    [Fact]
    public void Jailbreak_ReturnsSameInstance()
    {
        var attack1 = Attack.Jailbreak;
        var attack2 = Attack.Jailbreak;
        Assert.Same(attack1, attack2);
    }

    // === All MVP attacks are now implemented (P1B) ===

    [Fact]
    public void PIILeakage_ReturnsPIILeakageAttack()
    {
        var attack = Attack.PIILeakage;
        Assert.NotNull(attack);
        Assert.IsType<PIILeakageAttack>(attack);
        Assert.Equal("PIILeakage", attack.Name);
        Assert.Equal("LLM06", attack.OwaspLlmId);
    }

    [Fact]
    public void PIILeakage_ReturnsSameInstance()
    {
        var attack1 = Attack.PIILeakage;
        var attack2 = Attack.PIILeakage;
        Assert.Same(attack1, attack2);
    }

    [Fact]
    public void SystemPromptExtraction_ReturnsSystemPromptExtractionAttack()
    {
        var attack = Attack.SystemPromptExtraction;
        Assert.NotNull(attack);
        Assert.IsType<SystemPromptExtractionAttack>(attack);
        Assert.Equal("SystemPromptExtraction", attack.Name);
        Assert.Equal("LLM07", attack.OwaspLlmId);
    }

    [Fact]
    public void SystemPromptExtraction_ReturnsSameInstance()
    {
        var attack1 = Attack.SystemPromptExtraction;
        var attack2 = Attack.SystemPromptExtraction;
        Assert.Same(attack1, attack2);
    }

    [Fact]
    public void IndirectInjection_ReturnsIndirectInjectionAttack()
    {
        var attack = Attack.IndirectInjection;
        Assert.NotNull(attack);
        Assert.IsType<IndirectInjectionAttack>(attack);
        Assert.Equal("IndirectInjection", attack.Name);
        Assert.Equal("LLM01", attack.OwaspLlmId);
    }

    [Fact]
    public void IndirectInjection_ReturnsSameInstance()
    {
        var attack1 = Attack.IndirectInjection;
        var attack2 = Attack.IndirectInjection;
        Assert.Same(attack1, attack2);
    }

    [Fact]
    public void AllMvp_ReturnsThreeAttacks()
    {
        var attacks = Attack.AllMvp;
        Assert.Equal(3, attacks.Count);
        Assert.Contains(attacks, a => a.Name == "PromptInjection");
        Assert.Contains(attacks, a => a.Name == "Jailbreak");
        Assert.Contains(attacks, a => a.Name == "PIILeakage");
    }

    [Fact]
    public void All_ReturnsFiveAttacks()
    {
        var attacks = Attack.All;
        Assert.Equal(5, attacks.Count);
        Assert.Contains(attacks, a => a.Name == "PromptInjection");
        Assert.Contains(attacks, a => a.Name == "Jailbreak");
        Assert.Contains(attacks, a => a.Name == "PIILeakage");
        Assert.Contains(attacks, a => a.Name == "SystemPromptExtraction");
        Assert.Contains(attacks, a => a.Name == "IndirectInjection");
    }

    [Fact]
    public void ByOwaspId_LLM01_ReturnsInjectionAttacks()
    {
        var attacks = Attack.ByOwaspId("LLM01");
        Assert.Equal(3, attacks.Count);
        Assert.Contains(attacks, a => a.Name == "PromptInjection");
        Assert.Contains(attacks, a => a.Name == "Jailbreak");
        Assert.Contains(attacks, a => a.Name == "IndirectInjection");
    }

    [Fact]
    public void ByOwaspId_LLM06_ReturnsPIILeakage()
    {
        var attacks = Attack.ByOwaspId("LLM06");
        Assert.Single(attacks);
        Assert.Equal("PIILeakage", attacks[0].Name);
    }

    [Fact]
    public void ByOwaspId_LLM07_ReturnsSystemPromptExtraction()
    {
        var attacks = Attack.ByOwaspId("LLM07");
        Assert.Single(attacks);
        Assert.Equal("SystemPromptExtraction", attacks[0].Name);
    }

    [Fact]
    public void ByOwaspId_Unknown_ReturnsEmptyList()
    {
        // Non-existent OWASP IDs should return empty list (not throw)
        var result = Attack.ByOwaspId("LLM99");
        Assert.Empty(result);
    }

    [Fact]
    public void ByOwaspId_Null_ReturnsEmptyList()
    {
        var result = Attack.ByOwaspId(null!);
        Assert.Empty(result);
    }
}
