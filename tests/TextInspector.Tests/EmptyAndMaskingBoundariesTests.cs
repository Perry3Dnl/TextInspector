using TextInspector;

namespace TextInspector.Tests;

public sealed class EmptyAndMaskingBoundariesTests
{
    [Fact]
    public void EmptyInputReturnsUnchangedMaskedTextAndSafeHtml()
    {
        var engine = TextInspectorEngine.Create(new TextInspectorOptions { EnableMasking = true });
        var empty = engine.Inspect("", includeMarkedHtml: true);
        Assert.Equal("", empty.MarkedHtml);
        Assert.Equal("", empty.MaskedText);
        Assert.False(empty.HasDetections);
    }

    [Fact]
    public void NonmatchingTextPreservesMaskAndEncodesPlainMarkup()
    {
        var engine = TextInspectorEngine.Compile(
            new TextInspectorScoringOptions { Masking = new TextInspectorMaskingOptions { Enabled = true } },
            Array.Empty<TextInspectorRulePattern>());
        var result = engine.Inspect("<strong>nothing</strong>", includeMarkedHtml: true);
        Assert.Equal("<strong>nothing</strong>", result.MaskedText);
        Assert.Contains("&lt;strong&gt;", result.MarkedHtml);
        Assert.Empty(result.Evidence);
    }

    [Fact]
    public void ReportFlaggingCanBeSuppressedByCriticalThreshold()
    {
        var engine = TextInspectorEngine.Create();
        var report = engine.CreateReport("I am going to kill you.", new TextInspectionReportOptions
        {
            FlaggedMinimumRiskLevel = TextInspectionRiskLevel.Critical
        });
        Assert.True(report.HasDetections);
        Assert.NotEmpty(report.Findings);
        Assert.Equal(report.RiskAssessment.RiskScore, engine.Inspect("I am going to kill you.").RiskAssessment.RiskScore);
    }
}
