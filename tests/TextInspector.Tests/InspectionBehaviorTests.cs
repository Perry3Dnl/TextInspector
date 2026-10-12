using TextInspector;

namespace TextInspector.Tests;

public sealed class InspectionBehaviorTests
{
    [Fact]
    public void ExactCaptureMapsBackToOriginalUnicodeText()
    {
        var engine = Engine(new TextInspectorRulePattern
        {
            TextInspectorRuleId = Guid.NewGuid(),
            Key = "target",
            Category = TextInspectorRuleCategory.Fraud,
            SeverityScore = 7,
            RuleWeight = 20,
            RegexPattern = @"send\s+(?<target>[a-z]+)",
            EvidenceGroup = "target"
        });

        const string text = "ＳＥＮＤ\u200B TOKEN";
        var result = engine.Inspect(text, new TextInspectionOptions
        {
            IncludeHighlightSpans = true,
            IncludeSourceLocations = true
        });

        var evidence = Assert.Single(result.Evidence);
        Assert.Equal("TOKEN", evidence.MatchedText);
        Assert.Equal(text.IndexOf("TOKEN", StringComparison.Ordinal), evidence.Start);
        Assert.Equal(5, evidence.Length);
        Assert.Equal(evidence.Start, Assert.Single(result.HighlightSpans).Start);
    }

    [Fact]
    public void MarkedHtmlEncodesUntrustedInput()
    {
        var engine = Engine(new TextInspectorRulePattern
        {
            TextInspectorRuleId = Guid.NewGuid(),
            Key = "danger",
            Category = TextInspectorRuleCategory.Threats,
            SeverityScore = 8,
            RuleWeight = 20,
            RegexPattern = "danger"
        });

        var result = engine.Inspect("<script>alert(1)</script> danger", includeMarkedHtml: true);

        Assert.NotNull(result.MarkedHtml);
        Assert.DoesNotContain("<script>", result.MarkedHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", result.MarkedHtml, StringComparison.Ordinal);
        Assert.Contains("text-inspector-hit", result.MarkedHtml, StringComparison.Ordinal);
    }

    [Fact]
    public void InputLimitIsEnforcedOnFullAndFastPaths()
    {
        var options = new TextInspectorScoringOptions { MaxInputLength = 3 };
        var engine = TextInspectorEngine.Compile(options, new[]
        {
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.NewGuid(),
                Key = "x",
                Category = TextInspectorRuleCategory.Fraud,
                SeverityScore = 1,
                RuleWeight = 1,
                RegexPattern = "x"
            }
        });

        Assert.Throws<ArgumentException>(() => engine.Inspect("four"));
        Assert.Throws<ArgumentException>(() => engine.HasMatch("four"));
        Assert.Throws<ArgumentException>(() => engine.FindMatches("four"));
    }

    [Fact]
    public void ContextCanSuppressScoringWithoutRemovingEvidence()
    {
        var engine = Engine(new TextInspectorRulePattern
        {
            TextInspectorRuleId = Guid.NewGuid(),
            Key = "weapon",
            Category = TextInspectorRuleCategory.WeaponsSales,
            SeverityScore = 7,
            RuleWeight = 30,
            RegexPattern = @"\bpistol\b",
            Context = new TextInspectorRuleContext
            {
                WindowCharacters = 30,
                RequireAnyRegexPatterns = new[] { @"\b(?:buy|sell)\b" }
            }
        });

        var result = engine.Inspect("A museum displayed a pistol.");

        var evidence = Assert.Single(result.Evidence);
        Assert.False(evidence.ContributesToRisk);
        Assert.Equal(TextEvidenceScoringDisposition.MissingRequiredContext, evidence.ScoringStatus);
        Assert.Equal(0, result.RiskAssessment.RuleRiskPoints);
    }

    [Fact]
    public void EvidenceOnlyMatchHasPositiveDetectionScoreWithoutRisk()
    {
        var engine = TextInspectorEngine.Compile(
            new TextInspectorScoringOptions(),
            new[]
            {
                new TextInspectorRulePattern
                {
                    TextInspectorRuleId = Guid.NewGuid(),
                    Key = "observed",
                    Category = TextInspectorRuleCategory.ExternalContact,
                    DetectionScore = 37,
                    SeverityScore = 9,
                    RuleWeight = 10,
                    RegexPattern = "observed",
                    MaxCountForScoring = 0
                }
            });

        var result = engine.Inspect("observed");

        var evidence = Assert.Single(result.Evidence);
        Assert.Equal(37,evidence.DetectionScore);
        Assert.Equal(9,evidence.SeverityScore);
        Assert.False(evidence.ContributesToRisk);
        Assert.Equal(37,result.RiskAssessment.DetectionScore);
        Assert.True(result.RiskAssessment.HasDetections);
        Assert.True(result.HasDetections);
        Assert.Equal(0,result.RiskAssessment.RuleRiskPoints);
        Assert.Equal(0,result.RiskAssessment.TotalRiskPoints);
        Assert.Equal(0,result.RiskAssessment.RiskScore);
    }

    [Fact]
    public void GlobalMaskingMasksEvidenceOnlyDetectionWithoutChangingRisk()
    {
        var options = new TextInspectorScoringOptions
        {
            Masking = new TextInspectorMaskingOptions
            {
                Enabled = true,
                MaskCharacter = '*'
            }
        };
        var engine = TextInspectorEngine.Compile(
            options,
            new[]
            {
                new TextInspectorRulePattern
                {
                    TextInspectorRuleId = Guid.NewGuid(),
                    Key = "email",
                    Category = TextInspectorRuleCategory.ExternalContact,
                    DetectionScore = 91,
                    SeverityScore = 2,
                    RuleWeight = 10,
                    RegexPattern = @"\b[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,63}\b",
                    MaxCountForScoring = 0
                }
            });

        const string input = "Contact me at me@example.com.";
        var result = engine.Inspect(input);

        Assert.Equal("Contact me at **************.",result.MaskedText);
        Assert.True(result.HasDetections);
        Assert.Equal(91,result.RiskAssessment.DetectionScore);
        Assert.Equal(0,result.RiskAssessment.TotalRiskPoints);
        Assert.False(Assert.Single(result.Evidence).ContributesToRisk);
    }

    [Fact]
    public void NegativeReturnCapsAreRejected()
    {
        var engine = Engine(Rule("alpha", TextInspectorRuleCategory.Fraud, "alpha"));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            engine.Inspect("alpha", new TextInspectionOptions { MaxReturnedEvidence = -1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.FindMatches("alpha", -1));
    }

    private static TextInspectorEngine Engine(params TextInspectorRulePattern[] rules)
        => TextInspectorEngine.Compile(new TextInspectorScoringOptions(), rules);

    private static TextInspectorRulePattern Rule(
        string key,
        TextInspectorRuleCategory category,
        string pattern)
        => new()
        {
            TextInspectorRuleId = Guid.NewGuid(),
            Key = key,
            Category = category,
            SeverityScore = 5,
            RuleWeight = 10,
            RegexPattern = pattern,
            MaxCountForScoring = 5
        };
}
