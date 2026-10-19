using TextInspector;

namespace TextInspector.Tests;

public sealed class LinkedRuleTests
{
    private static TextInspectorRulePattern Rule(string key, int weight = 10, int severity = 3) => new()
    {
        TextInspectorRuleId = Guid.NewGuid(), Key = key, RegexPattern = @"\b" + key + @"\b",
        Category = TextInspectorRuleCategory.PlatformIntegrity, RuleWeight = weight, SeverityScore = severity,
        DetectionScore = 73, MaxCountForScoring = 5
    };
    private static TextInspectorLinkedRule Link(string key = "AB", params string[] members) => new()
    {
        Key = key, RuleKeys = members.Length == 0 ? new[] { "a", "b" } : members
    };
    private static TextInspectorScoringOptions Options() => new()
    {
        Combo_AnyHighSeverity_TieBreakBonus = 0, Combo_TwoPlusHighCategories_Bonus = 0,
        Combo_ThreePlusHighCategories_Bonus = 0, Combo_FourPlusHighCategories_Bonus = 0
    };
    private static TextInspectorEngine Compile(TextInspectorLinkedRule link, params TextInspectorRulePattern[] rules)
    {
        if(rules.Length == 0) rules = new[] { Rule("a"), Rule("b"), Rule("c"), Rule("d") };
        rules[0].LinkedRules = new[] { link };
        return TextInspectorEngine.Compile(Options(), rules);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("a a a")]
    [InlineData("c d")]
    public void FewerThanTwoDistinctMembersDoNotActivate(string text)
    {
        var result = Compile(Link()).Inspect(text);
        Assert.Empty(result.LinkedRuleMatches);
        Assert.Equal(0, result.RiskAssessment.LinkedRuleRiskPoints);
        Assert.All(result.Evidence, item => Assert.False(item.IsLinked));
    }

    [Fact]
    public void OnlyParticipantsEscalateAndOriginalValuesRemainVisible()
    {
        var engine = Compile(Link(), Rule("a", 10, 3), Rule("b", 20, 4), Rule("c", 5, 2), Rule("d", 5, 3));
        var result = engine.Inspect("a c b d", new TextInspectionOptions { IncludeHighlightSpans = true, IncludeMarkedHtml = true });
        Assert.Equal(40, result.RiskAssessment.RuleRiskPoints);
        Assert.Equal(30, result.RiskAssessment.LinkedRuleRiskPoints);
        Assert.Equal(70, result.RiskAssessment.TotalRiskPoints);
        var link = Assert.Single(result.LinkedRuleMatches);
        Assert.True(link.IsLinkedRule);
        Assert.Equal(new[] { "a", "b" }, link.MatchedRuleKeys);
        Assert.Equal(30, link.EscalationRiskPoints);
        var a = Assert.Single(result.Evidence, item => item.RuleKey == "a");
        Assert.Equal(10, a.RuleWeight);
        Assert.Equal(20, a.EffectiveRiskPoints);
        Assert.Equal(3, a.SeverityScore);
        Assert.Equal(6, a.EffectiveSeverity);
        Assert.Equal(73, a.DetectionScore);
        Assert.Equal(2, a.RiskMultiplier);
        Assert.All(result.Evidence.Where(item => item.RuleKey is "c" or "d"), item =>
        {
            Assert.False(item.IsLinked);
            Assert.Equal(item.SeverityScore, item.EffectiveSeverity);
            Assert.Equal(item.RuleWeight, item.EffectiveRiskPoints);
        });
        Assert.All(result.HighlightSpans.Where(span => span.IsLinked), span =>
            Assert.True(span.RuleKeys.All(key => key is "a" or "b")));
        Assert.Contains("data-linked-rules=\"AB\"", result.MarkedHtml);
        var report = engine.CreateReport("a c b d");
        Assert.True(report.HasLinkedRules);
        var finding = Assert.Single(report.Findings, item => item.IsLinkedRule);
        Assert.Equal(TextInspectionFindingKind.LinkedRule, finding.FindingType);
        Assert.Equal(2, finding.Evidence.Count);
        Assert.All(report.Highlights.Where(span => span.RuleKeys.Contains("c")), span => Assert.False(span.IsLinked));
    }

    [Fact]
    public void TwoOfThreeCountsAllParticipatingMembersEvenWhenOwnerDidNotMatch()
    {
        var engine = Compile(Link("ABC", "a", "b", "c"));
        Assert.Equal(3, Assert.Single(engine.Inspect("c b a").LinkedRuleMatches).MatchedRuleKeys.Count);
        Assert.Equal(2, Assert.Single(engine.Inspect("b c").LinkedRuleMatches).MatchedRuleKeys.Count);
        Assert.Equal(60, engine.Inspect("a b c").RiskAssessment.TotalRiskPoints);
        var three = Link("ABC", "a", "b", "c"); three.MinimumMatchedRules = 3;
        Assert.Empty(Compile(three).Inspect("a b").LinkedRuleMatches);
    }

    [Fact]
    public void OverlappingLinksUseStrongestMultiplierOnceWithDeterministicAttribution()
    {
        var a = Rule("a"); var b = Rule("b"); var c = Rule("c");
        var ab = Link(); var ac = Link("AC", "a", "c"); ac.Multiplier = 3;
        a.LinkedRules = new[] { ac, ab };
        var result = TextInspectorEngine.Compile(Options(), new[] { c, b, a }).Inspect("a b c");
        Assert.Equal(80, result.RiskAssessment.TotalRiskPoints); // 30 + 20 + 30
        Assert.Equal(50, result.RiskAssessment.LinkedRuleRiskPoints);
        Assert.Equal(2, result.LinkedRuleMatches.Count);
        var evidence = Assert.Single(result.Evidence, item => item.RuleKey == "a");
        Assert.Equal(new[] { "AB", "AC" }, evidence.LinkedRuleKeys);
        Assert.Equal(3, evidence.RiskMultiplier);
        Assert.Equal(50, result.RiskContributions.Where(item => item.IsLinkedRule).Sum(item => item.RiskPoints));
        ab.Multiplier = 3;
        var tied = TextInspectorEngine.Compile(Options(), new[] { a, b, c }).Inspect("a b c");
        Assert.Equal(40, Assert.Single(tied.LinkedRuleMatches, item => item.LinkedRuleKey == "AB").EscalationRiskPoints);
        Assert.Equal(20, Assert.Single(tied.LinkedRuleMatches, item => item.LinkedRuleKey == "AC").EscalationRiskPoints);
    }

    [Fact]
    public void WindowIncludesOnlyEvidenceParticipatingInACompleteCluster()
    {
        var link = Link(); link.WindowCharacters = 5;
        var engine = Compile(link);
        var result = engine.Inspect("a b ----- a");
        Assert.Equal(2, Assert.Single(result.LinkedRuleMatches).Evidence.Count);
        Assert.False(result.Evidence.Last(item => item.RuleKey == "a").IsLinked);
        Assert.Empty(engine.Inspect("a ----- b").LinkedRuleMatches);
    }

    [Fact]
    public void IndependentInspectionsNeverCombineTheirEvidence()
    {
        var engine = Compile(Link());
        Assert.Empty(engine.Inspect("a").LinkedRuleMatches);
        Assert.Empty(engine.Inspect("b").LinkedRuleMatches);
        Assert.Single(engine.Inspect("b a").LinkedRuleMatches);
    }

    [Fact]
    public void SeverityCapsButDetectionStrengthAndPositionsDoNotChange()
    {
        var result = Compile(Link(), Rule("a", 10, 8), Rule("b", 10, 9)).Inspect("a b");
        Assert.Equal(10, result.RiskAssessment.EffectiveSeverity);
        Assert.All(result.Evidence, item =>
        {
            Assert.Equal(10, item.EffectiveSeverity);
            Assert.Equal(73, item.DetectionScore);
            Assert.Equal(1, item.Length);
        });
    }

    [Fact]
    public void EvidenceOnlyLinksHaveFlagsWithoutInventingRiskPoints()
    {
        var link = Link(); link.ScoredOnly = false;
        var engine = Compile(link, Rule("a", 0), Rule("b", 0));
        var result = engine.Inspect("a b");
        Assert.True(result.HasLinkedRules);
        Assert.Equal(0, result.RiskAssessment.TotalRiskPoints);
        Assert.False(Assert.Single(result.LinkedRuleMatches).ContributesToRisk);
        Assert.Single(engine.CreateReport("a b").Findings, item => item.IsLinkedRule);
        link.RiskPoints = 25;
        var scored = Compile(link, Rule("a", 0), Rule("b", 0)).Inspect("a b a b");
        Assert.Equal(25, scored.RiskAssessment.TotalRiskPoints);
        Assert.Equal(25, Assert.Single(scored.LinkedRuleMatches).FixedRiskPoints);
        Assert.Empty(Compile(Link(), Rule("a", 0), Rule("b", 0)).Inspect("a b").LinkedRuleMatches);
    }

    [Fact]
    public void ContextAndDuplicateSuppressionCannotBeUndoneByALink()
    {
        var link = Link(); link.ScoredOnly = false;
        var a = Rule("a");
        a.Context = new TextInspectorRuleContext { RequireAnyRegexPatterns = new[] { "required" } };
        Assert.Empty(Compile(link, a, Rule("b")).Inspect("a b").LinkedRuleMatches);
        var duplicate = Rule("b"); duplicate.RegexPattern = @"\ba\b";
        Assert.Empty(Compile(link, Rule("a"), duplicate).Inspect("a").LinkedRuleMatches);
    }

    [Fact]
    public void OccurrenceLimitsStillControlPoints()
    {
        var a = Rule("a"); a.MaxCountForScoring = 1;
        var result = Compile(Link(), a, Rule("b")).Inspect("a a a b");
        Assert.Equal(40, result.RiskAssessment.TotalRiskPoints);
        Assert.Equal(2, result.Evidence.Count(item => item.ContributesToRisk));
        Assert.All(result.Evidence.Where(item => !item.ContributesToRisk), item => Assert.Equal(0, item.EffectiveRiskPoints));
    }

    [Fact]
    public void OutputLimitsAndRawHelpersDoNotChangeEvaluation()
    {
        var engine = Compile(Link());
        Assert.True(engine.HasMatch("a"));
        Assert.Single(engine.FindMatches("a"));
        var result = engine.Inspect("a b", new TextInspectionOptions { MaxReturnedEvidence = 0, IncludeMatchedText = false });
        Assert.Empty(result.Evidence);
        Assert.Equal(40, result.RiskAssessment.TotalRiskPoints);
        Assert.All(Assert.Single(result.LinkedRuleMatches).Evidence, item => Assert.Equal("", item.MatchedText));
    }

    [Fact]
    public void CompiledConfigurationIsDetachedAndDisabledLinksDoNothing()
    {
        var link = Link(); var engine = Compile(link);
        link.Multiplier = 100;
        link.RuleKeys[1] = "unknown";
        Assert.Equal(40, engine.Inspect("a b").RiskAssessment.TotalRiskPoints);
        var disabled = Link(); disabled.IsEnabled = false;
        Assert.Equal(20, Compile(disabled).Inspect("a b").RiskAssessment.TotalRiskPoints);
    }

    [Theory]
    [InlineData("threshold")]
    [InlineData("members")]
    [InlineData("blank")]
    [InlineData("multiplier")]
    [InlineData("points")]
    [InlineData("window")]
    [InlineData("null")]
    public void InvalidDefinitionsProduceDiagnostics(string invalid)
    {
        var link = Link();
        switch(invalid)
        {
            case "threshold": link.MinimumMatchedRules = 1; break;
            case "members": link.RuleKeys = new[] { "a", " A " }; break;
            case "blank": link.Key = " "; break;
            case "multiplier": link.Multiplier = 0; break;
            case "points": link.RiskPoints = -1; break;
            case "window": link.WindowCharacters = -1; break;
            case "null": link.RuleKeys = null!; break;
        }
        var engine = Compile(link);
        Assert.Equal(0, engine.CompiledLinkedRuleCount);
        Assert.Contains(engine.CompilationDiagnostics, item => item.Kind == TextInspectorDiagnosticKind.InvalidLinkedRule);
    }

    [Fact]
    public void MissingDisabledAndDuplicateReferencesAreRejected()
    {
        var missing = Compile(Link("AB", "a", "missing"));
        Assert.Contains(missing.CompilationDiagnostics, item => item.Kind == TextInspectorDiagnosticKind.LinkedRuleReferenceNotFound);
        var b = Rule("b"); b.IsEnabled = false;
        Assert.Equal(0, Compile(Link(), Rule("a"), b).CompiledLinkedRuleCount);
        var a = Rule("a"); a.LinkedRules = new[] { Link(), Link() };
        var duplicate = TextInspectorEngine.Compile(Options(), new[] { a, Rule("b") });
        Assert.Equal(1, duplicate.CompiledLinkedRuleCount);
        Assert.Contains(duplicate.CompilationDiagnostics, item => item.Kind == TextInspectorDiagnosticKind.InvalidLinkedRule);
    }

    [Fact]
    public void LargeMultipliersSaturateWithoutOverflow()
    {
        var link = Link(); link.Multiplier = int.MaxValue; link.RiskPoints = int.MaxValue;
        var result = Compile(link, Rule("a", int.MaxValue), Rule("b", int.MaxValue)).Inspect("a b");
        Assert.Equal(int.MaxValue, result.RiskAssessment.TotalRiskPoints);
        Assert.Equal(100, result.RiskAssessment.RiskScore);
        Assert.All(result.Evidence, item => Assert.Equal(int.MaxValue, item.EffectiveRiskPoints));
    }
}
