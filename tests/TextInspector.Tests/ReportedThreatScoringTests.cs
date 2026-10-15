using Xunit;

namespace TextInspector.Tests;

/// <summary>Context-scoring TDD: keep threat evidence while distinguishing quotation/reporting from direct threats.</summary>
public sealed class ReportedThreatScoringTests
{
    private static readonly Lazy<TextInspectorEngine> Engine = new(() => TextInspectorEngine.Create());

    [Theory]
    [InlineData("I'm going to kill you.", "The report quotes: \"I'm going to kill you.\"")]
    [InlineData("I'm gonna hurt you.", "The fictional script says: \"I'm gonna hurt you.\"")]
    [InlineData("I am going to harm you.", "The moderator documented: \"I am going to harm you.\"")]
    public void ReportedThreatRetainsEvidenceButContributesLessRisk(string direct,string reported)
    {
        var a = Engine.Value.Inspect(direct);
        var b = Engine.Value.Inspect(reported,new TextInspectionOptions { IncludeNonScoringEvidence = true, IncludeHighlightSpans = true });
        Assert.Contains(a.Evidence,e => e.RuleKey == "Threat_Violence" && e.ContributesToRisk);
        Assert.Contains(b.Evidence,e => e.RuleKey == "Threat_Violence");
        Assert.True(b.RiskAssessment.TotalRiskPoints < a.RiskAssessment.TotalRiskPoints,
            $"Quoted/reported threat scored {b.RiskAssessment.TotalRiskPoints} versus direct {a.RiskAssessment.TotalRiskPoints}");
        Assert.Contains(b.HighlightSpans,s => s.Length > 0);
    }

    [Fact]
    public void UnattributedQuotationIsNotAutomaticallyExcused()
    {
        var result = Engine.Value.Inspect("He shouted \"I'm going to kill you.\"");
        Assert.Contains(result.Evidence,e => e.RuleKey == "Threat_Violence" && e.ContributesToRisk);
    }

    [Fact]
    public void AttributedQuoteDoesNotHideSeparateDirectThreat()
    {
        var result = Engine.Value.Inspect(
            "The transcript says: \"I'm going to kill you.\" Later, I am going to harm you.");
        var threats = result.Evidence.Where(e => e.RuleKey == "Threat_Violence").ToArray();
        Assert.Equal(2,threats.Length);
        Assert.Contains(threats,e => e.ContributesToRisk);
        Assert.Contains(threats,e => !e.ContributesToRisk);
    }

    [Theory]
    [InlineData("I'm going to kill you.")]
    [InlineData("I'm gonna hurt you.")]
    public void DirectThreatStillContributesRisk(string input)
    {
        var result = Engine.Value.Inspect(input);
        Assert.Contains(result.Evidence,e => e.RuleKey == "Threat_Violence" && e.ContributesToRisk);
    }
}
