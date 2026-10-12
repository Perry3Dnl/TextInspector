using TextInspector;

namespace TextInspector.Tests;

public sealed class LiteConfigurationValidationTests
{
    [Fact]
    public void UnknownRuleKeysAndCategoriesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorLiteOptions { DisabledRuleKeys = new[] { "nonexistent" } }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorLiteOptions { DisabledRuleKeys = new[] { " " } }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorLiteOptions { DisabledCategories = new[] { (TextInspectorRuleCategory)999 } }));
    }

    [Fact]
    public void InvalidMaskCharactersAndNullCollectionsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorLiteOptions { DisabledRuleKeys = null! }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorLiteOptions { DisabledCategories = null! }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorLiteOptions { MaskCharacter = '\n', EnableMasking = true }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorLiteOptions { MaskCharacter = '\ud800', EnableMasking = true }));
    }

    [Fact]
    public void DisabledRulesAndCategoriesAreNotCompiled()
    {
        var baseline = TextInspectorEngine.Create();
        var info = TextInspectorLite.Rules.First();
        var selected = TextInspectorEngine.Create(new TextInspectorLiteOptions
        {
            DisabledRuleKeys = new[] { info.Key }
        });
        Assert.True(selected.CompiledRuleCount < baseline.CompiledRuleCount);
        var withoutCategory = TextInspectorEngine.Create(new TextInspectorLiteOptions
        {
            DisabledCategories = new[] { info.Category }
        });
        Assert.True(withoutCategory.CompiledRuleCount < baseline.CompiledRuleCount);
    }
}
