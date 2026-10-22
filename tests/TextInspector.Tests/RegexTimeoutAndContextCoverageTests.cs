using TextInspector;

namespace TextInspector.Tests;

public sealed class RegexTimeoutAndContextCoverageTests
{
    private static TextInspectorRulePattern Rule(string key, string pattern) => new()
    {
        TextInspectorRuleId = Guid.NewGuid(), Key = key,
        RegexPattern = pattern, Category = TextInspectorRuleCategory.Fraud,
        DetectionScore = 80, SeverityScore = 6, RuleWeight = 20
    };

    [Fact]
    public void PathologicalRegexIsSafelySkippedOnFullAndFastPaths()
    {
        var rule = Rule("catastrophic", @"^(a+)+$");
        var engine = TextInspectorEngine.CompileInternal(
            new TextInspectorScoringOptions { RegexTimeoutMilliseconds = 1 }, new[] { rule });
        string input = new string('a', 4096) + "!";
        Assert.False(engine.HasMatch(input));
        Assert.Empty(engine.FindMatches(input));
        var result = engine.Inspect(input);
        Assert.Empty(result.Evidence);
        Assert.Contains(result.Diagnostics, x => x.Kind == TextInspectorDiagnosticKind.RegexTimeout);
    }

    [Fact]
    public void ContextCanBeSatisfiedByRuleCategoryAndRegexSelectors()
    {
        var trigger = Rule("trigger", @"\btrigger\b");
        var target = Rule("target", @"\btarget\b");
        target.Context = new TextInspectorRuleContext
        {
            WindowCharacters = 80,
            MinimumRequiredMatches = 3,
            RequireAnyRuleKeys = new[] { "trigger" },
            RequireAnyCategories = new[] { TextInspectorRuleCategory.Fraud },
            RequireAnyRegexPatterns = new[] { @"\bextra\b" }
        };
        var engine = TextInspectorEngine.CompileInternal(new TextInspectorScoringOptions(), new[] { trigger, target });
        var result = engine.Inspect("trigger extra target");
        Assert.True(Assert.Single(result.Evidence, e => e.RuleKey == "target").ContributesToRisk);
        var missing = engine.Inspect("trigger target");
        Assert.False(Assert.Single(missing.Evidence, e => e.RuleKey == "target").ContributesToRisk);
    }

    [Fact]
    public void ExclusionByRuleAndCategoryPreservesDetectionButSuppressesRisk()
    {
        var blocker = Rule("blocker", "block");
        blocker.Category = TextInspectorRuleCategory.Threats;
        var target = Rule("target", "target");
        target.Context = new TextInspectorRuleContext
        {
            WindowCharacters = 20,
            ExcludeAnyRuleKeys = new[] { "blocker" },
            ExcludeAnyCategories = new[] { TextInspectorRuleCategory.Threats }
        };
        var engine = TextInspectorEngine.CompileInternal(new TextInspectorScoringOptions(), new[] { blocker, target });
        var result = engine.Inspect("block target");
        var evidence = Assert.Single(result.Evidence, e => e.RuleKey == "target");
        Assert.False(evidence.ContributesToRisk);
        Assert.Equal(TextEvidenceScoringDisposition.ExcludedByContext, evidence.ScoringStatus);
    }
}
