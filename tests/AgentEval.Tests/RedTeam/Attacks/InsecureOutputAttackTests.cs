// tests/AgentEval.Tests/RedTeam/Attacks/InsecureOutputAttackTests.cs
using AgentEval.RedTeam;
using AgentEval.RedTeam.Attacks;

namespace AgentEval.Tests.RedTeam.Attacks;

/// <summary>
/// Tests for <see cref="InsecureOutputAttack"/>.
/// </summary>
public sealed class InsecureOutputAttackTests
{
    private readonly InsecureOutputAttack _attack = new();

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("InsecureOutput", _attack.Name);
    }

    [Fact]
    public void DisplayName_ReturnsCorrectValue()
    {
        Assert.Equal("Insecure Output Handling", _attack.DisplayName);
    }

    [Fact]
    public void OwaspLlmId_ReturnsLLM02()
    {
        Assert.Equal("LLM02", _attack.OwaspLlmId);
    }

    [Fact]
    public void MitreAtlasIds_ContainsExpectedTechniques()
    {
        Assert.Contains("AML.T0051", _attack.MitreAtlasIds);
    }

    [Fact]
    public void DefaultSeverity_IsHigh()
    {
        Assert.Equal(Severity.High, _attack.DefaultSeverity);
    }

    [Fact]
    public void Description_DescribesInjectionDetection()
    {
        Assert.Contains("injection", _attack.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("XSS", _attack.Description);
        Assert.Contains("SQL", _attack.Description);
    }

    [Fact]
    public void GetEvaluator_ReturnsInsecureOutputEvaluator()
    {
        var evaluator = _attack.GetEvaluator();
        Assert.NotNull(evaluator);
        Assert.IsType<AgentEval.RedTeam.Evaluators.InsecureOutputEvaluator>(evaluator);
    }

    // === Probe Count Tests by Intensity ===

    [Fact]
    public void GetProbes_QuickIntensity_Returns4Probes()
    {
        var probes = _attack.GetProbes(Intensity.Quick);
        Assert.Equal(4, probes.Count); // XSS probes only (Easy)
    }

    [Fact]
    public void GetProbes_ModerateIntensity_Returns12Probes()
    {
        var probes = _attack.GetProbes(Intensity.Moderate);
        Assert.Equal(12, probes.Count); // XSS (4) + SQL (4) + Command (4)
    }

    [Fact]
    public void GetProbes_ComprehensiveIntensity_Returns18Probes()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        Assert.Equal(18, probes.Count); // All probes
    }

    // === Probe ID Tests ===

    [Fact]
    public void GetProbes_ProbeIdsStartWithIO()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        Assert.All(probes, p => Assert.StartsWith("IO-", p.Id));
    }

    [Fact]
    public void GetProbes_ProbeIdsAreUnique()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        var ids = probes.Select(p => p.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void GetProbes_ProbeIdsAreSequential()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        var expectedIds = Enumerable.Range(1, 18).Select(i => $"IO-{i:D3}").ToList();
        var actualIds = probes.Select(p => p.Id).OrderBy(id => id).ToList();
        Assert.Equal(expectedIds, actualIds);
    }

    // === Technique Tests ===

    [Theory]
    [InlineData("xss_injection")]
    [InlineData("sql_injection")]
    [InlineData("command_injection")]
    [InlineData("format_injection")]
    [InlineData("code_injection")]
    public void GetProbes_ContainsTechnique(string technique)
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        Assert.Contains(probes, p => p.Technique == technique);
    }

    [Fact]
    public void GetProbes_XssInjectionProbesHaveCorrectTechnique()
    {
        var probes = _attack.GetProbes(Intensity.Quick);
        Assert.All(probes, p => Assert.Equal("xss_injection", p.Technique));
    }

    [Fact]
    public void GetProbes_SqlInjectionProbesAtModerateIntensity()
    {
        var probes = _attack.GetProbes(Intensity.Moderate);
        var sqlProbes = probes.Where(p => p.Technique == "sql_injection").ToList();
        Assert.Equal(4, sqlProbes.Count);
    }

    [Fact]
    public void GetProbes_CommandInjectionProbesAtModerateIntensity()
    {
        var probes = _attack.GetProbes(Intensity.Moderate);
        var cmdProbes = probes.Where(p => p.Technique == "command_injection").ToList();
        Assert.Equal(4, cmdProbes.Count);
    }

    // === Difficulty Level Tests ===

    [Fact]
    public void GetProbes_QuickIntensity_AllEasyDifficulty()
    {
        var probes = _attack.GetProbes(Intensity.Quick);
        Assert.All(probes, p => Assert.Equal(Difficulty.Easy, p.Difficulty));
    }

    [Fact]
    public void GetProbes_ModerateIntensity_HasModerateProbes()
    {
        var probes = _attack.GetProbes(Intensity.Moderate);
        Assert.Contains(probes, p => p.Difficulty == Difficulty.Moderate);
    }

    [Fact]
    public void GetProbes_ComprehensiveIntensity_HasHardProbes()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        Assert.Contains(probes, p => p.Difficulty == Difficulty.Hard);
    }

    // === Probe Content Tests ===

    [Fact]
    public void GetProbes_AllProbesHaveAttackName()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        Assert.All(probes, p => Assert.Equal("InsecureOutput", p.AttackName));
    }

    [Fact]
    public void GetProbes_AllProbesHavePrompt()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        Assert.All(probes, p => Assert.False(string.IsNullOrWhiteSpace(p.Prompt)));
    }

    [Fact]
    public void GetProbes_AllProbesHaveSource()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive);
        Assert.All(probes, p => Assert.False(string.IsNullOrWhiteSpace(p.Source)));
    }

    // === XSS Probe Content Tests ===

    [Fact]
    public void GetProbes_XssProbesContainXssPatterns()
    {
        var probes = _attack.GetProbes(Intensity.Quick);
        var xssPatterns = new[] { "<script", "onclick", "onerror", "onload", "svg" };
        Assert.All(probes, p => 
            Assert.True(xssPatterns.Any(pattern => 
                p.Prompt.Contains(pattern, StringComparison.OrdinalIgnoreCase))));
    }

    // === SQL Probe Content Tests ===

    [Fact]
    public void GetProbes_SqlProbesContainSqlPatterns()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive)
            .Where(p => p.Technique == "sql_injection")
            .ToList();
        
        var sqlPatterns = new[] { "OR", "DROP", "UPDATE", "UNION", "--" };
        Assert.All(probes, p => 
            Assert.True(sqlPatterns.Any(pattern => 
                p.Prompt.Contains(pattern, StringComparison.OrdinalIgnoreCase))));
    }

    // === Command Injection Probe Content Tests ===

    [Fact]
    public void GetProbes_CommandProbesContainShellPatterns()
    {
        var probes = _attack.GetProbes(Intensity.Comprehensive)
            .Where(p => p.Technique == "command_injection")
            .ToList();
        
        var cmdPatterns = new[] { "rm", "cat", "wget", "cmd.exe", "$(", "`" };
        Assert.All(probes, p => 
            Assert.True(cmdPatterns.Any(pattern => 
                p.Prompt.Contains(pattern, StringComparison.OrdinalIgnoreCase))));
    }
}
