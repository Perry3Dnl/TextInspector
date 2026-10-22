using TextInspector;

namespace TextInspector.Tests;

public sealed class EditionCompatibilityTests
{
    [Fact]
    public void FreeExposesProStyleAnalysisAndMarkupWithoutChangingInspection()
    {
        var inspector = new TextInspectorService();
        var text = "hello@example.com";
        var ordinary = inspector.Inspect(text,includeMarkedHtml: true);
        Assert.Equal(ordinary.RiskAssessment.RiskScore,inspector.Analyze(text).RiskScore);
        Assert.Equal(ordinary.MarkedHtml ?? "",inspector.MarkText(text));
    }

    [Fact]
    public async Task FreeRepositoryRefreshReportsProUpgradeRequirement()
    {
        var service = new TextInspectorService();
        var error = await Assert.ThrowsAsync<NotSupportedException>(() => service.ReloadAsync());
        Assert.Contains("TextInspector.Pro",error.Message);
    }
    [Fact]
    public void ProOnlyBatchApiIsVisibleButFailsClearlyInFree()
    {
        var service = new TextInspectorService();
        var engine = TextInspectorEngine.Create();
        var sample = new string?[] { "sample" };
        Assert.Contains("TextInspector.Pro",
            Assert.Throws<NotSupportedException>(() => service.InspectManyParallel(sample)).Message);
        Assert.Contains("TextInspector.Pro",
            Assert.Throws<NotSupportedException>(() => engine.InspectManyParallel(sample)).Message);
    }

    [Fact]
    public void ProCompatibleOutputOptionsDoNotChangeFreeInspection()
    {
        var engine = TextInspectorEngine.Create();
        const string text = "hello@example.com";
        var baseline = engine.Inspect(text);
        var compatible = engine.Inspect(text, new TextInspectionOptions
        {
            IncludePolicyDetails = true,
            IncludeUrlDetails = true,
            MaxReturnedPolicyMatches = 0
        });
        Assert.Equal(baseline.RiskAssessment.RiskScore,compatible.RiskAssessment.RiskScore);
        Assert.Equal(baseline.Evidence.Count,compatible.Evidence.Count);
    }

}
