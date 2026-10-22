using System.Reflection;
using TextInspector;

namespace TextInspector.Tests;

/// <summary>
/// Guards the 1.0.x-compatible Free entry points against accidental API removal.
/// This is not a substitute for cross-package and binary compatibility verification.
/// </summary>
public sealed class FreePublicApiStabilityTests
{
    [Fact]
    public void FreeFactoryAndReportCallsStayUsable()
    {
        var inspector = TextInspectorEngine.Create(new TextInspectorOptions
        {
            EnableMasking = true,
            MaskCharacter = '#',
            DisabledRuleKeys = Array.Empty<string>(),
            DisabledCategories = Array.Empty<TextInspectorRuleCategory>()
        });
        var report = inspector.CreateReport("Contact hello@example.com");
        Assert.NotNull(report);
        var result = inspector.Inspect("hello@example.com");
        Assert.NotNull(result.MaskedText);
    }

    [Fact]
    public void FreeEntryPointSignaturesStayPublic()
    {
        var engine = typeof(TextInspectorEngine);
        Assert.NotNull(engine.GetMethod("Create",
            BindingFlags.Public | BindingFlags.Static,
            binder: null, types: new[] { typeof(TextInspectorOptions) }, modifiers: null));
        Assert.NotNull(engine.GetMethod("Inspect", new[] { typeof(string), typeof(TextInspectionOptions) }));
        Assert.NotNull(engine.GetMethod("CreateReport", new[] { typeof(string), typeof(TextInspectionReportOptions) }));
        Assert.NotNull(engine.GetMethod("FindMatches", new[] { typeof(string), typeof(int) }));
        Assert.NotNull(engine.GetMethod("HasMatch", new[] { typeof(string) }));
    }
}
