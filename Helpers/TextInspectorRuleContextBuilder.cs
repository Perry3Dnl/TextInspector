namespace TextInspector;

/// <summary>Source-compatible API placeholder. Custom configuration requires TextInspector Pro.</summary>
public sealed class TextInspectorRuleContextBuilder
{
    public TextInspectorRuleContextBuilder Within(int characters)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContextBuilder RequireAtLeast(int selectors)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContextBuilder RequireRules(params string[] keys)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContextBuilder RequireCategories(params TextInspectorRuleCategory[] categories)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContextBuilder RequirePatterns(params string[] patterns)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContextBuilder ExcludeRules(params string[] keys)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContextBuilder ExcludeCategories(params TextInspectorRuleCategory[] categories)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContextBuilder ExcludePatterns(params string[] patterns)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleContext Build()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");
}
