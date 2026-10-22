namespace TextInspector;

/// <summary>Source-compatible API placeholder. Custom configuration requires TextInspector Pro.</summary>
public sealed class TextInspectorBuilder
{
    private TextInspectorBuilder() { }
    public static TextInspectorBuilder Create()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public static TextInspectorBuilder Empty()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder WithOptions(TextInspectorScoringOptions options)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder ConfigureOptions(Action<TextInspectorScoringOptions> configure)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder AddRule(TextInspectorRulePattern rule)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder AddRule(string key, TextInspectorRuleCategory category, Action<TextInspectorRuleBuilder> configure)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder AddPolicy(TextInspectorPolicy policy)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder AddPolicy(string key, Action<TextInspectorPolicyBuilder> configure)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder ConfigureRule(string key, Action<TextInspectorRulePattern> configure)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorBuilder ConfigurePolicy(string key, Action<TextInspectorPolicy> configure)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorConfiguration Build()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorEngine Compile()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorService CreateService()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");
}
