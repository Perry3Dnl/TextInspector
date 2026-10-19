using System;
using System.Linq;
using TextInspector;

namespace TextInspector.Tests;

/// <summary>Guards the legacy inspection path when optional 1.1 additions are not configured.</summary>
public sealed class CompatibilityRegressionTests
{
    [Fact]
    public void NoNumericConditionsOrLinkedRulesPreservesTraditionalRuleBehavior()
    {
        var rules = new[]
        {
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("00000000-0000-0000-0000-0000000000a1"),
                Key = "alpha", RegexPattern = @"\balpha\b",
                Category = TextInspectorRuleCategory.PlatformIntegrity,
                RuleWeight = 10, SeverityScore = 3, DetectionScore = 75
            },
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("00000000-0000-0000-0000-0000000000a2"),
                Key = "beta", RegexPattern = @"\bbeta\b",
                Category = TextInspectorRuleCategory.PlatformIntegrity,
                RuleWeight = 20, SeverityScore = 4, DetectionScore = 65
            }
        };
        var engine = TextInspectorEngine.Compile(new TextInspectorScoringOptions(),rules);
        var result = engine.Inspect("alpha beta");
        Assert.Equal(2,engine.CompiledRuleCount);
        Assert.Equal(2,result.Evidence.Count);
        Assert.Equal(new[] { "alpha", "beta" },result.Evidence.Select(x => x.RuleKey).ToArray());
        Assert.Equal(new[] { 0, 6 },result.Evidence.Select(x => x.Start).ToArray());
        Assert.Equal(30,result.RiskAssessment.RuleRiskPoints);
        Assert.Equal(0,result.RiskAssessment.LinkedRuleRiskPoints);
        Assert.Equal(0,result.RiskAssessment.TriggeredLinkedRuleCount);
        Assert.Empty(result.LinkedRuleMatches);
        Assert.All(result.Evidence,hit => Assert.False(hit.IsLinked));
    }

    [Fact]
    public void OldRuleJsonWithoutNewFieldsDeserializesWithDisabledFeatures()
    {
        const string json = "{\"TextInspectorRuleId\":\"00000000-0000-0000-0000-0000000000a1\",\"Key\":\"alpha\",\"RegexPattern\":\"alpha\"}";
        var rule = System.Text.Json.JsonSerializer.Deserialize<TextInspectorRulePattern>(json)!;
        Assert.Empty(rule.LinkedRules);
        Assert.Empty(rule.NumericConditions);
    }
}
