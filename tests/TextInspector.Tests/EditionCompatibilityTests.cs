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
}
