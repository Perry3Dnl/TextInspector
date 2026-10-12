using System.Text.Json;
using TextInspector;

namespace TextInspector.Tests;

public sealed class PublicNamingContractTests
{
    [Fact]
    public void ReportExposesTheNewRiskVocabularyWithoutLegacyJsonNames()
    {
        var report = new TextInspectionReport
        {
            RiskAssessment = new TextSafetyInfo(0, 0, 0, 0, 0),
            CategorySummaries = Array.Empty<TextCategorySummary>(),
            RiskContributions = Array.Empty<TextScoreContribution>()
        };
        var json = JsonSerializer.Serialize(report);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.True(root.TryGetProperty("RiskAssessment", out var assessment));
        Assert.True(root.TryGetProperty("CategorySummaries", out _));
        Assert.True(root.TryGetProperty("RiskContributions", out _));
        Assert.True(root.TryGetProperty("HasDetections", out _));
        Assert.True(root.TryGetProperty("RequiresReview", out _));
        Assert.True(assessment.TryGetProperty("RiskScore", out _));
        Assert.True(assessment.TryGetProperty("TotalRiskPoints", out _));
        Assert.True(assessment.TryGetProperty("RuleRiskPoints", out _));
        Assert.True(assessment.TryGetProperty("AdditionalRiskPoints", out _));
        Assert.True(assessment.TryGetProperty("HighestDetectedSeverity", out _));

        foreach(var oldName in new[] { "SafetyInfo", "Categories", "ScoreContributions", "HasDetection", "IsFlagged" })
            Assert.False(root.TryGetProperty(oldName, out _));
        foreach(var oldName in new[] { "PriorityScore", "RawScore", "BaseRawScore", "BonusRawScore", "MaxSeverityScore" })
            Assert.False(assessment.TryGetProperty(oldName, out _));
    }

    [Fact]
    public void FindingUsesStableDescriptiveNamesForIdentityAndClassification()
    {
        var finding = new TextInspectionFinding
        {
            FindingId = "category:fraud",
            FindingCode = "category.fraud",
            FindingType = TextInspectionFindingKind.Category
        };
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(finding));
        Assert.Equal("category:fraud", json.RootElement.GetProperty("FindingId").GetString());
        Assert.Equal("category.fraud", json.RootElement.GetProperty("FindingCode").GetString());
        Assert.False(json.RootElement.TryGetProperty("Id", out _));
        Assert.False(json.RootElement.TryGetProperty("Code", out _));
        Assert.False(json.RootElement.TryGetProperty("Kind", out _));
    }
}
