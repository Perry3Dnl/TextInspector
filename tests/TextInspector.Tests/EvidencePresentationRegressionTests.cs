using TextInspector;

namespace TextInspector.Tests;

public sealed class EvidencePresentationRegressionTests
{
    private static TextInspectorRulePattern Rule(string key, string pattern, int limit = 3) => new()
    {
        TextInspectorRuleId = Guid.NewGuid(), Key = key,
        RegexPattern = pattern, Category = TextInspectorRuleCategory.Fraud,
        DetectionScore = 80, SeverityScore = 6, RuleWeight = 10,
        MaxCountForScoring = limit
    };

    [Fact]
    public void EvidenceOnlyHitsCanBeHiddenFromReturnedEvidenceWithoutChangingDetection()
    {
        var scored = Rule("scored", "alpha");
        var evidenceOnly = Rule("support", "beta", 0);
        var engine = TextInspectorEngine.Compile(new TextInspectorScoringOptions(), new[] { scored, evidenceOnly });
        const string text = "alpha beta";
        var baseline = engine.Inspect(text, new TextInspectionOptions { IncludeHighlightSpans = true });
        var selected = engine.Inspect(text, new TextInspectionOptions
        {
            IncludeNonScoringEvidence = false,
            IncludeHighlightSpans = true,
            HighlightScoredEvidenceOnly = true,
            MaxReturnedHighlightSpans = 1,
            MaxReturnedEvidence = 1
        });
        Assert.Equal(2, baseline.Evidence.Count);
        Assert.Single(selected.Evidence);
        Assert.Equal("scored", selected.Evidence[0].RuleKey);
        Assert.Single(selected.HighlightSpans);
        Assert.Equal(baseline.RiskAssessment.TotalRiskPoints, selected.RiskAssessment.TotalRiskPoints);
        Assert.Equal(baseline.RiskAssessment.DetectionScore, selected.RiskAssessment.DetectionScore);
    }

    [Fact]
    public void OverlappingEvidenceProducesNonOverlappingHighlightSegments()
    {
        var first = Rule("first", "alpha");
        var second = Rule("second", "pha");
        var engine = TextInspectorEngine.Compile(new TextInspectorScoringOptions(), new[] { first, second });
        var result = engine.Inspect("alpha", new TextInspectionOptions { IncludeHighlightSpans = true });
        Assert.Equal(2, result.Evidence.Count);
        Assert.NotEmpty(result.HighlightSpans);
        for(var i = 1; i < result.HighlightSpans.Count; i++)
            Assert.True(result.HighlightSpans[i - 1].EndExclusive <= result.HighlightSpans[i].Start);
        Assert.Contains(result.HighlightSpans, span => span.RuleKeys.Count == 2);
    }

    [Fact]
    public void HighlightCapZeroSuppressesDisplayOnly()
    {
        var engine = TextInspectorEngine.Compile(
            new TextInspectorScoringOptions(), new[] { Rule("item", "alpha") });
        var result = engine.Inspect("alpha", new TextInspectionOptions
        {
            IncludeHighlightSpans = true,
            MaxReturnedHighlightSpans = 0
        });
        Assert.Empty(result.HighlightSpans);
        Assert.True(result.HasDetections);
        Assert.NotEmpty(result.Evidence);
    }

    [Fact]
    public void ReportWithoutSourceLocationsStillKeepsOriginalEvidencePositions()
    {
        var engine = TextInspectorEngine.Compile(new TextInspectorScoringOptions(),
            new[] { Rule("rule", "alpha") });
        var report = engine.CreateReport("before alpha after",
            new TextInspectionReportOptions { IncludeSourceLocations = false, MaxReasonsPerFinding = 0 });
        var finding = Assert.Single(report.Findings);
        var evidence = Assert.Single(finding.Evidence);
        Assert.Equal(7, evidence.Start);
        Assert.Null(evidence.SourceRange);
        Assert.Empty(finding.Reasons);
    }
}
