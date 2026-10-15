using Xunit;

namespace TextInspector.Tests;

/// <summary>
/// Real preset calibration checks, separate from synthetic exclusion-rule tests.
/// Assertions are scoped to named signals so unrelated preset expansions do not
/// incorrectly turn incidental evidence into a corpus failure.
/// </summary>
public sealed class BuiltInFalsePositiveCalibrationTests
{
    private static readonly Lazy<TextInspectorEngine> Inspector = new(
        () => TextInspectorEngine.Create());

    [Theory]
    [InlineData("I am going to kill you.", "Threat_Violence")]
    [InlineData("I'm gonna hurt you.", "Threat_Violence")]
    [InlineData("Official support team here.", "Fraud_Staff_Impersonation")]
    public void HighConfidencePatternsStillProduceEvidence(string input,string key)
    {
        var result = Inspector.Value.Inspect(input);
        Assert.Contains(result.Evidence,e => e.RuleKey == key);
    }

    [Theory]
    [InlineData("The weather forecast looks good today.", "Threat_Violence")]
    [InlineData("This tutorial explains how moderators review requests.", "Fraud_Staff_Impersonation")]
    [InlineData("The design team updated our style guide.", "Fraud_Staff_Impersonation")]
    [InlineData("We discussed murder mysteries in a book club.", "Threat_Violence")]
    [InlineData("Someone emailed the customer-support department.", "Fraud_Staff_Impersonation")]
    public void InnocentLanguageDoesNotMatchUnrelatedHighConfidencePatterns(string input,string key)
    {
        var result = Inspector.Value.Inspect(input);
        Assert.DoesNotContain(result.Evidence,e => e.RuleKey == key);
    }

    [Theory]
    [InlineData("You can read the public documentation at example.com.")]
    [InlineData("Our contact address is docs@example.com.")]
    public void OrdinaryLinksAndAddressesAreNotIndependentRuleRisk(string input)
    {
        var result = Inspector.Value.Inspect(input);
        Assert.DoesNotContain(result.Evidence,e =>
            (e.RuleKey == "Platform_Domain_Link" || e.RuleKey == "PII_Email_Address")
            && e.ContributesToRisk);
    }

    [Fact]
    public void ThreatEvidenceRemainsCorrectlyMappedToOriginalText()
    {
        const string text = "Report: I'm going to kill you.";
        var result = Inspector.Value.Inspect(text,new TextInspectionOptions
        {
            IncludeHighlightSpans = true,
            IncludeNonScoringEvidence = true
        });
        var hit = Assert.Single(result.Evidence.Where(e => e.RuleKey == "Threat_Violence"));
        Assert.Equal("I'm going to kill you",hit.MatchedText);
        Assert.Equal(text.IndexOf("I'm going",StringComparison.Ordinal),hit.Start);
        Assert.Contains(result.HighlightSpans,span => span.Start <= hit.Start &&
            span.Start + span.Length > hit.Start);
    }

    [Fact]
    public void BaselinePresetHasAnExplicitScoringOnlyAndEvidenceOnlySplit()
    {
        var rules = TextInspectorPresets.LoadRules();
        Assert.Contains(rules,rule => rule.MaxCountForScoring == 0);
        Assert.Contains(rules,rule => rule.MaxCountForScoring > 0);
        Assert.Equal(0,rules.Single(r => r.Key == "PII_Email_Address").MaxCountForScoring);
    }
}
