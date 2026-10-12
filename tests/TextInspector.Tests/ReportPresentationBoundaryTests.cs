using TextInspector;

namespace TextInspector.Tests;

public sealed class ReportPresentationBoundaryTests
{
    [Fact]
    public void EmptyAndInformationalReportsDoNotCreateReviewFindings()
    {
        var inspector = TextInspectorEngine.Create();
        var empty = inspector.CreateReport("");
        Assert.Empty(empty.Findings);
        Assert.False(empty.RequiresReview);
        Assert.Equal(0, empty.InputLength);
        Assert.Empty(empty.Highlights);
        var signal = inspector.CreateReport("Contact us at hello@example.com", new TextInspectionReportOptions
        {
            IncludeSupportingEvidence = false,
            IncludeHighlights = false
        });
        Assert.True(signal.HasDetections);
        Assert.Empty(signal.SupportingEvidence);
        Assert.Empty(signal.Highlights);
        Assert.False(signal.RequiresReview);
    }

    [Fact]
    public void PresentationLimitsDoNotAlterRiskAssessment()
    {
        var inspector = TextInspectorEngine.Create();
        const string text = "I am going to kill you.";
        var original = inspector.CreateReport(text);
        var limited = inspector.CreateReport(text, new TextInspectionReportOptions
        {
            MaxFindings = 0, MaxReasonsPerFinding = 0,
            IncludeHighlights = false, IncludeSupportingEvidence = false,
            IncludeSourceLocations = false
        });
        Assert.Empty(limited.Findings);
        Assert.Empty(limited.Highlights);
        Assert.Equal(original.RiskAssessment.RiskScore, limited.RiskAssessment.RiskScore);
        Assert.Equal(original.RiskAssessment.TotalRiskPoints, limited.RiskAssessment.TotalRiskPoints);
        Assert.Equal(original.RiskLevel, limited.RiskLevel);
    }

    [Fact]
    public void FlagThresholdCanBeMadeMoreRestrictive()
    {
        var inspector = TextInspectorEngine.Create();
        const string text = "I am going to kill you.";
        var report = inspector.CreateReport(text, new TextInspectionReportOptions
        {
            FlaggedMinimumRiskLevel = TextInspectionRiskLevel.None
        });
        Assert.True(report.HasDetections);
        Assert.NotEmpty(report.Findings);
        Assert.True(report.RequiresReview);
    }
}
