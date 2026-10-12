using TextInspector;

namespace TextInspector.Tests;

public sealed class ConfigurationValidationTests
{
    [Fact]
    public void UnknownRuleKeysAndCategoriesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorOptions { DisabledRuleKeys = new[] { "nonexistent" } }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorOptions { DisabledRuleKeys = new[] { " " } }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorOptions { DisabledCategories = new[] { (TextInspectorRuleCategory)999 } }));
    }

    [Fact]
    public void InvalidMaskCharactersAndNullCollectionsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorOptions { DisabledRuleKeys = null! }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorOptions { DisabledCategories = null! }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorOptions { MaskCharacter = '\n', EnableMasking = true }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(
            new TextInspectorOptions { MaskCharacter = '\ud800', EnableMasking = true }));
    }

    [Fact]
    public void DisabledRulesAndCategoriesAreNotCompiled()
    {
        var baseline = TextInspectorEngine.Create();
        var info = TextInspectorDefaults.Rules.First();
        var selected = TextInspectorEngine.Create(new TextInspectorOptions
        {
            DisabledRuleKeys = new[] { info.Key }
        });
        Assert.True(selected.CompiledRuleCount < baseline.CompiledRuleCount);
        var withoutCategory = TextInspectorEngine.Create(new TextInspectorOptions
        {
            DisabledCategories = new[] { info.Category }
        });
        Assert.True(withoutCategory.CompiledRuleCount < baseline.CompiledRuleCount);
    }
}
