using TextInspector;

namespace TextInspector.Tests;

public sealed class InspectionReportTests
{
    private static readonly TextInspectorEngine Engine = TextInspectorEngine.CompileInternal(
        TextInspectorPresets.LoadOptions(),
        TextInspectorPresets.LoadRules());

    [Fact]
    public void StandaloneThreatBecomesCategoryFinding()
    {
        const string text = "I am going to kill you.";

        var report = Engine.CreateReport(text);
        var finding = Assert.Single(report.Findings,item =>
            item.FindingType == TextInspectionFindingKind.Category
            && item.Category == TextInspectorRuleCategory.Threats);

        Assert.Equal("category.threats",finding.FindingCode);
        Assert.Equal("Threats detected",finding.Title);
        Assert.True(finding.ContributesToRisk);
        Assert.NotEmpty(finding.Evidence);
        Assert.NotEmpty(finding.Reasons);
        Assert.Contains(report.Highlights,span => span.FindingIds.Contains(finding.FindingId,StringComparer.Ordinal));
    }

    [Fact]
    public void EvidenceOnlyContactSignalIsVisibleWithoutCreatingRiskFinding()
    {
        const string text = "Contact us at hello@example.com.";

        var report = Engine.CreateReport(text);

        Assert.Empty(report.Findings);
        var email = Assert.Single(report.SupportingEvidence,evidence => evidence.RuleKey == "PII_Email_Address");
        Assert.True(email.DetectionScore > 0);
        Assert.True(report.RiskAssessment.DetectionScore > 0);
        Assert.True(report.HasDetections);
        Assert.Equal(0,report.RiskAssessment.TotalRiskPoints);
        Assert.Equal(TextInspectionRiskLevel.None,report.RiskLevel);
        Assert.False(report.RequiresReview);
        Assert.Null(report.MaskedText);
        Assert.Contains("supporting signal",report.Summary,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReportEvidenceCanIncludeExactSourceLocations()
    {
        const string text = "Header\r\nI am going to kill you.\r\nFooter";

        var report = Engine.CreateReport(text,new TextInspectionReportOptions
        {
            IncludeSourceLocations = true
        });

        var threat = Assert.Single(report.Findings,item =>
            item.Category == TextInspectorRuleCategory.Threats);
        var evidence = Assert.Single(threat.Evidence,item => item.RuleKey == "Threat_Violence");

        Assert.NotNull(evidence.SourceRange);
        Assert.Equal(2,evidence.SourceRange!.StartLine);
        Assert.Equal(evidence.MatchedText,text.Substring(evidence.Start,evidence.Length));
    }

    [Fact]
    public void ReportOptionsCanLimitPresentationWithoutChangingRisk()
    {
        const string text = "I'm going to kill you. Contact hello@example.com.";

        var full = Engine.CreateReport(text);
        var limited = Engine.CreateReport(text,new TextInspectionReportOptions
        {
            IncludeSupportingEvidence = false,
            IncludeHighlights = false,
            MaxReasonsPerFinding = 1,
            MaxFindings = 1
        });

        Assert.Equal(full.RiskLevel,limited.RiskLevel);
        Assert.Equal(full.RequiresReview,limited.RequiresReview);
        Assert.Empty(limited.SupportingEvidence);
        Assert.Empty(limited.Highlights);
        Assert.Single(limited.Findings);
        Assert.True(limited.Findings[0].Reasons.Count <= 1);
    }

    [Fact]
    public void ReportMetadataIsDeterministicAndConfigurationAware()
    {
        const string text = "I am going to kill you.";

        var first = Engine.CreateReport(text);
        var second = Engine.CreateReport(text);

        Assert.Equal(1,first.SchemaVersion);
        Assert.Equal(TextInspectorPresets.LoadOptions().Version,first.ConfigurationVersion);
        Assert.Equal(text.Length,first.InputLength);
        Assert.Equal(first.RiskLevel,second.RiskLevel);
        Assert.Equal(first.RequiresReview,second.RequiresReview);
        Assert.Equal(first.Summary,second.Summary);
        Assert.Equal(
            first.Findings.Select(item => item.FindingId),
            second.Findings.Select(item => item.FindingId));
    }

    [Fact]
    public void InvalidReportLimitsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Engine.CreateReport("test",new TextInspectionReportOptions { MaxFindings = -1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Engine.CreateReport("test",new TextInspectionReportOptions { MaxReasonsPerFinding = -1 }));
    }
}
