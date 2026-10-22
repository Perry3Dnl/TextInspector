namespace TextInspector;

/// <summary>Source-compatible API placeholder. Custom configuration requires TextInspector Pro.</summary>
public sealed class TextInspectorPolicyBuilder
{
    private TextInspectorPolicyBuilder() { }
    public static TextInspectorPolicyBuilder Create(string key)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder WithId(Guid id)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder Describe(string description)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder WithTags(params string[] tags)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder InCategory(TextInspectorRuleCategory category)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder WithSeverity(int severity)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder WithScore(int rawPoints)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder ClassificationOnly()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicyBuilder When(TextInspectorPolicyCondition condition)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorPolicy Build()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");
}
