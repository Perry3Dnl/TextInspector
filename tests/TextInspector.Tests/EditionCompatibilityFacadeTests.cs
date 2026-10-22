using TextInspector;

namespace TextInspector.Tests;

public sealed class EditionCompatibilityFacadeTests
{
    [Fact]
    public void ProFactoriesAndBuildersProduceUpgradeRequiredErrors()
    {
        var error = Assert.Throws<NotSupportedException>(() => TextInspectorPro.Create());
        Assert.Contains("TextInspector Pro", error.Message);
        Assert.Throws<NotSupportedException>(() => TextInspectorPro.CreateService());
        Assert.Throws<NotSupportedException>(() => TextInspectorBuilder.Create());
        Assert.Throws<NotSupportedException>(() => TextInspectorRuleBuilder.Create("custom", TextInspectorRuleCategory.Fraud));
        Assert.Throws<NotSupportedException>(() => TextInspectorPolicyBuilder.Create("custom"));
    }

    [Fact]
    public void ProRepositorySurfacesAreDeclaredButUnavailableInFree()
    {
        Assert.Throws<NotSupportedException>(() => new InMemoryTextInspectorRepository());
        Assert.Throws<NotSupportedException>(() => new FileTextInspectorRepository("test"));
        Assert.Throws<NotSupportedException>(() => TextInspectorPresets.WriteMissing("test"));
    }

    [Fact]
    public void AdvancedEngineSurfaceIsExplicitAndDoesNotChangeFreeInspection()
    {
        var options = TextInspectorPresets.LoadOptions();
        var rules = TextInspectorPresets.LoadRules();
        var engine = TextInspectorEngine.Compile(options, rules);
        Assert.True(engine.HasMatch("hello@example.com"));
        Assert.Throws<NotSupportedException>(() => TextInspectorEngine.Compile(options, rules, Array.Empty<TextInspectorPolicy>()));
        Assert.Throws<NotSupportedException>(() => engine.FindFirstMatch("hello@example.com"));
        Assert.Throws<NotSupportedException>(() => engine.InspectManyParallel(new[] { "hello@example.com" }));
    }

    [Fact]
    public void BasicServiceHelpersRemainFunctional()
    {
        var service = new TextInspectorService();
        Assert.True(service.HasMatch("hello@example.com"));
        Assert.True(service.Analyze("hello@example.com").TotalRiskPoints >= 0);
        Assert.Contains("<mark", service.MarkText("hello@example.com"), StringComparison.OrdinalIgnoreCase);
        Assert.Throws<NotSupportedException>(() => { _ = service.GetOptionsAsync(); });
        Assert.Throws<NotSupportedException>(() => { _ = service.ReloadAsync(); });
    }

    [Fact]
    public void FreeRuleAndPolicyDtosAreSourcePortableWithoutRunningProEngine()
    {
        var options = new TextInspectionOptions { IncludePolicyDetails = true, IncludeUrlDetails = true, MaxReturnedPolicyMatches = 1 };
        var result = TextInspectorEngine.Create().Inspect("hello@example.com", options);
        Assert.Empty(result.PolicyMatches);
        Assert.Empty(result.UrlObservations);
        Assert.NotNull(new TextInspectorPolicy { Key = "Example", Condition = TextInspectorPolicyConditions.Rule("PII_Email_Address") });
    }
}
