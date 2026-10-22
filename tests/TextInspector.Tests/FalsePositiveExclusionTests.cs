using Xunit;

namespace TextInspector.Tests;

public sealed class FalsePositiveExclusionTests
{
    private static TextInspectorEngine Build(params TextInspectorFalsePositiveExclusion[] exclusions)
    {
        var options = TextInspectorPresets.LoadOptions();
        options.FalsePositiveExclusions = exclusions;
        var rules = new[]
        {
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("ba51f0b0-7777-4444-8888-000000000001"),
                Key = "Example_Word", Category = TextInspectorRuleCategory.Fraud,
                RegexPattern = @"\b(?:foo|bar)\b",
                DetectionScore = 80, SeverityScore = 5, RuleWeight = 10
            },
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("ba51f0b0-7777-4444-8888-000000000002"),
                Key = "Other_Word", Category = TextInspectorRuleCategory.Fraud,
                RegexPattern = @"\bfoo\b",
                DetectionScore = 80, SeverityScore = 5, RuleWeight = 10
            }
        };
        return TextInspectorEngine.CompileInternal(options,rules);
    }

    [Fact]
    public void ByDefaultNoEvidenceIsSuppressed()
    {
        var report = Build().Inspect("foo and bar");
        Assert.Contains(report.Evidence,e => e.RuleKey == "Example_Word" && e.MatchedText == "foo");
        Assert.Contains(report.Evidence,e => e.RuleKey == "Example_Word" && e.MatchedText == "bar");
    }

    [Fact]
    public void ExactExclusionAffectsOnlySpecifiedRuleAndText()
    {
        var engine = Build(new TextInspectorFalsePositiveExclusion
        {
            RuleKey = "example_word", MatchedText = "FOO"
        });
        var result = engine.Inspect("foo and bar");
        Assert.DoesNotContain(result.Evidence,e => e.RuleKey == "Example_Word" && e.MatchedText == "foo");
        Assert.Contains(result.Evidence,e => e.RuleKey == "Example_Word" && e.MatchedText == "bar");
        Assert.Contains(result.Evidence,e => e.RuleKey == "Other_Word" && e.MatchedText == "foo");
    }

    [Fact]
    public void PublicOptionsCanConfigureExclusions()
    {
        var key = TextInspectorPresets.LoadRules().First().Key;
        var inspector = TextInspectorEngine.Create(new TextInspectorOptions
        {
            FalsePositiveExclusions = new[] {
                new TextInspectorFalsePositiveExclusion {
                    RuleKey = key, MatchedText = "innocent-case"
                }
            }
        });
        Assert.True(inspector.CompiledRuleCount > 0);
    }

    [Fact]
    public void SuppressedMatchesDoNotAffectRiskOrHighlights()
    {
        var engine = Build(
            new TextInspectorFalsePositiveExclusion { RuleKey = "Example_Word", MatchedText = "foo" },
            new TextInspectorFalsePositiveExclusion { RuleKey = "Other_Word", MatchedText = "foo" });
        var result = engine.Inspect("foo",new TextInspectionOptions { IncludeHighlightSpans = true });
        Assert.Empty(result.Evidence);
        Assert.Empty(result.HighlightSpans);
        Assert.Equal(0,result.RiskAssessment.TotalRiskPoints);
    }

    [Fact]
    public void NoSubstringExclusionsAndInvalidConfigurationsRejected()
    {
        var engine = Build(new TextInspectorFalsePositiveExclusion
        {
            RuleKey = "Example_Word", MatchedText = "fo"
        });
        Assert.Contains(engine.Inspect("foo").Evidence,e => e.RuleKey == "Example_Word");
        Assert.Throws<InvalidOperationException>(() => Build(new TextInspectorFalsePositiveExclusion
        {
            RuleKey = "", MatchedText = "foo"
        }));
    }
}
