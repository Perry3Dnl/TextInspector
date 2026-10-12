using TextInspector;

namespace TextInspector.Tests;

/// <summary>Exercises edition-shared compiler failure paths without touching production behaviour.</summary>
public sealed class EngineCompilationBoundaryTests
{
    private static TextInspectorRulePattern Rule() => new()
    {
        TextInspectorRuleId = Guid.NewGuid(), Key = "boundary", Category = TextInspectorRuleCategory.Fraud,
        RegexPattern = "target", SeverityScore = 5, RuleWeight = 10, MaxCountForScoring = 2,
        MaxEvidenceSnippets = 3, DetectionScore = 80
    };

    [Theory]
    [InlineData("missing-id")]
    [InlineData("missing-key")]
    [InlineData("missing-pattern")]
    [InlineData("negative-count")]
    [InlineData("negative-snippets")]
    [InlineData("negative-weight")]
    [InlineData("negative-severity")]
    [InlineData("bad-prefilter")]
    [InlineData("negative-window")]
    [InlineData("zero-required")]
    [InlineData("bad-regex")]
    [InlineData("bad-context-regex")]
    [InlineData("excess-context-selectors")]
    [InlineData("invalid-capture")]
    public void BrokenRulesAreDiagnosedWithoutCrashingOtherRules(string caseName)
    {
        var invalid = Rule();
        switch(caseName)
        {
            case "missing-id": invalid.TextInspectorRuleId = Guid.Empty; break;
            case "missing-key": invalid.Key = " "; break;
            case "missing-pattern": invalid.RegexPattern = " "; break;
            case "negative-count": invalid.MaxCountForScoring = -1; break;
            case "negative-snippets": invalid.MaxEvidenceSnippets = -1; break;
            case "negative-weight": invalid.RuleWeight = -1; break;
            case "negative-severity": invalid.SeverityScore = -1; break;
            case "bad-prefilter": invalid.PrefilterMode = (TextInspectorPrefilterMode)100; break;
            case "negative-window": invalid.Context = new TextInspectorRuleContext { WindowCharacters = -1 }; break;
            case "zero-required": invalid.Context = new TextInspectorRuleContext { MinimumRequiredMatches = 0 }; break;
            case "bad-regex": invalid.RegexPattern = "("; break;
            case "bad-context-regex": invalid.Context = new TextInspectorRuleContext { RequireAnyRegexPatterns = new[] { "(" } }; break;
            case "excess-context-selectors": invalid.Context = new TextInspectorRuleContext { RequireAnyRuleKeys = new[] { "valid" }, MinimumRequiredMatches = 2 }; break;
            case "invalid-capture": invalid.EvidenceGroup = "missing"; break;
            default: throw new ArgumentOutOfRangeException(nameof(caseName));
        }
        var valid = Rule();
        valid.Key = "valid";
        valid.RegexPattern = "working";
        var compilation = TextInspectorEngine.CompileWithDiagnostics(
            new TextInspectorScoringOptions(), new[] { invalid, valid });
        Assert.Equal(1, compilation.Engine.CompiledRuleCount);
        Assert.NotEmpty(compilation.Diagnostics);
        Assert.True(compilation.Engine.HasMatch("working"));
        Assert.False(compilation.Engine.HasMatch("target"));
    }

    [Fact]
    public void CompiledRulesKeepPriorityAndPrefilterSemantics()
    {
        var low = Rule();
        low.Key = "low"; low.Priority = -2;
        var high = Rule();
        high.Key = "high"; high.Priority = 10; high.PrefilterTerms = new[] { "MUST" };
        var compilation = TextInspectorEngine.CompileWithDiagnostics(
            new TextInspectorScoringOptions(), new[] { low, high });
        Assert.Equal(2, compilation.Engine.CompiledRuleCount);
        Assert.Equal(1, compilation.Engine.CompiledPrefilteredRuleCount);
        Assert.True(compilation.Engine.HasMatch("target"));
        Assert.True(compilation.Engine.HasMatch("must target"));
        Assert.False(compilation.Engine.HasMatch("nothing"));
    }

    [Fact]
    public void InvalidContextExclusionRegexIsReported()
    {
        var invalid = Rule();
        invalid.Context = new TextInspectorRuleContext { ExcludeAnyRegexPatterns = new[] { "(" } };
        var result = TextInspectorEngine.CompileWithDiagnostics(new TextInspectorScoringOptions(), new[] { invalid });
        Assert.Empty(result.Engine.FindMatches("target"));
        Assert.Contains(result.Diagnostics, x => x.Kind == TextInspectorDiagnosticKind.InvalidContextRegex);
    }

    [Fact]
    public void MissingContextReferenceIsDiagnosed()
    {
        var invalid = Rule();
        invalid.Context = new TextInspectorRuleContext { RequireAnyRuleKeys = new[] { "nonexistent-rule-for-boundary-tests" } };
        var result = TextInspectorEngine.CompileWithDiagnostics(new TextInspectorScoringOptions(), new[] { invalid });
        Assert.Contains(result.Diagnostics, x => x.Kind == TextInspectorDiagnosticKind.ContextReferenceNotFound);
    }
}
