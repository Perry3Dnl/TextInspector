using Xunit;

namespace TextInspector.Tests;

/// <summary>Shared safety regression corpus: benign exceptions must not hide independent risky evidence.</summary>
public sealed class FalsePositiveCorpusTests
{
    private static TextInspectorEngine Build(bool suppressBenign)
    {
        var options = TextInspectorPresets.LoadOptions();
        if(suppressBenign)
            options.FalsePositiveExclusions = new[]
            {
                new TextInspectorFalsePositiveExclusion { RuleKey = "Support_Claim", MatchedText = "support" },
                new TextInspectorFalsePositiveExclusion { RuleKey = "Payment_Reference", MatchedText = "transfer" }
            };
        var rules = new[]
        {
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("55c0aee1-1111-4000-8000-000000000001"),
                Key = "Support_Claim",
                Category = TextInspectorRuleCategory.Fraud,
                RegexPattern = @"\bsupport\b",
                RuleWeight = 12, MaxCountForScoring = 10, DetectionScore = 75, SeverityScore = 4
            },
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("55c0aee1-1111-4000-8000-000000000002"),
                Key = "Credential_Request",
                Category = TextInspectorRuleCategory.Fraud,
                RegexPattern = @"\bsend\s+(?:your\s+)?verification\s+code\b",
                RuleWeight = 50, MaxCountForScoring = 10, DetectionScore = 95, SeverityScore = 9
            },
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("55c0aee1-1111-4000-8000-000000000003"),
                Key = "Payment_Reference",
                Category = TextInspectorRuleCategory.Fraud,
                RegexPattern = @"\btransfer\b",
                RuleWeight = 12, MaxCountForScoring = 10, DetectionScore = 75, SeverityScore = 4
            },
            new TextInspectorRulePattern
            {
                TextInspectorRuleId = Guid.Parse("55c0aee1-1111-4000-8000-000000000004"),
                Key = "Urgent_Payment",
                Category = TextInspectorRuleCategory.Fraud,
                RegexPattern = @"\bwire\s+the\s+money\s+now\b",
                RuleWeight = 50, MaxCountForScoring = 10, DetectionScore = 95, SeverityScore = 9
            }
        };
        return TextInspectorEngine.Compile(options,rules);
    }

    [Theory]
    [InlineData("The customer support article explains the procedure.", "support")]
    [InlineData("Our accounting guide explains the transfer process.", "transfer")]
    [InlineData("SUPPORT team wrote a tutorial.", "SUPPORT")]
    public void BenignMatchedTermIsExcludedButOtherTextRemainsUntouched(string message,string excluded)
    {
        var baseline = Build(false).Inspect(message);
        var filtered = Build(true).Inspect(message,new TextInspectionOptions { IncludeHighlightSpans = true });
        Assert.Contains(baseline.Evidence,e => e.MatchedText.Equals(excluded,StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(filtered.Evidence,e => e.MatchedText.Equals(excluded,StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0,filtered.RiskAssessment.TotalRiskPoints);
        Assert.Empty(filtered.HighlightSpans);
    }

    [Theory]
    [InlineData("Contact support and send your verification code.", "Credential_Request")]
    [InlineData("Please transfer; wire the money now.", "Urgent_Payment")]
    [InlineData("SUPPORT insists: send your verification code.", "Credential_Request")]
    public void DangerousIndependentSignalSurvivesBenignExclusion(string message,string criticalKey)
    {
        var baseline = Build(false).Inspect(message);
        var filtered = Build(true).Inspect(message,new TextInspectionOptions { IncludeHighlightSpans = true });
        Assert.Contains(filtered.Evidence,e => e.RuleKey == criticalKey);
        Assert.True(filtered.RiskAssessment.TotalRiskPoints > 0);
        Assert.True(filtered.RiskAssessment.TotalRiskPoints < baseline.RiskAssessment.TotalRiskPoints);
        Assert.NotEmpty(filtered.HighlightSpans);
    }

    [Fact]
    public void FalsePositiveFilteringIsRuleScopedAndPreservesOriginalSourceOffsets()
    {
        const string message = "Support said: send your verification code.";
        var result = Build(true).Inspect(message,new TextInspectionOptions { IncludeHighlightSpans = true });
        var hit = Assert.Single(result.Evidence);
        Assert.Equal("Credential_Request",hit.RuleKey);
        Assert.Equal("send your verification code",hit.MatchedText);
        Assert.Equal(message.IndexOf("send your",StringComparison.Ordinal),hit.Start);
    }

    [Fact]
    public void ExclusionDoesNotSuppressSimilarButNotExactMatchedText()
    {
        var engine = Build(true);
        Assert.Contains(engine.Inspect("send your verification code").Evidence,
            e => e.RuleKey == "Credential_Request");
    }
}
