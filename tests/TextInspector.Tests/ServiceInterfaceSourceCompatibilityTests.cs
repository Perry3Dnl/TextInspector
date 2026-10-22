using TextInspector;

namespace TextInspector.Tests;

public sealed class ServiceInterfaceSourceCompatibilityTests
{
    [Fact]
    public void ProMethodCallsOnFreeServiceInterfaceCompileAndBehaveClearly()
    {
        ITextInspectorService service = new TextInspectorService();
        Assert.Equal(service.Inspect("hello@example.com").RiskAssessment.TotalRiskPoints,
            service.Analyze("hello@example.com").TotalRiskPoints);
        Assert.Contains("text-inspector-hit", service.MarkText("hello@example.com"));

        Assert.Throws<NotSupportedException>(() => { _ = service.GetOptionsAsync(); });
        Assert.Throws<NotSupportedException>(() => { _ = service.GetAllRulesAsync(); });
        Assert.Throws<NotSupportedException>(() => { _ = service.ReloadAsync(); });
        Assert.Throws<NotSupportedException>(() =>
        {
            _ = service.SaveAsync(TextInspectorPresets.LoadOptions(), TextInspectorPresets.LoadRules(), Guid.NewGuid());
        });
    }

    [Fact]
    public void ProParallelCallOnDetectionInterfaceCompilesButRequiresPro()
    {
        ITextInspectorDetectionService service = new TextInspectorService();
        Assert.Throws<NotSupportedException>(() =>
        {
            _ = service.InspectManyParallel(new[] { "hello@example.com" });
        });
    }

    private sealed class LegacyConsumerImplementation : ITextInspectorService
    {
        public TextInspectionResult Inspect(string? text, bool includeMarkedHtml = false)
            => TextInspectorEngine.Create().Inspect(text, includeMarkedHtml);
    }

    [Fact]
    public void ExistingThirdPartyServiceImplementationsRemainValid()
    {
        ITextInspectorService service = new LegacyConsumerImplementation();
        Assert.NotNull(service.Inspect("hello@example.com"));
        Assert.NotNull(service.Analyze("hello@example.com"));
    }
}
