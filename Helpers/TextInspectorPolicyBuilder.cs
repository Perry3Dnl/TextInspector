namespace TextInspector;

/// <summary>Pro policy-authoring API compatibility. Free does not evaluate custom policies.</summary>
public sealed class TextInspectorPolicyBuilder
{
    private TextInspectorPolicyBuilder() { }

    private static NotSupportedException Upgrade() => new("Update to TextInspector.Pro to author policies.");
    public static TextInspectorPolicyBuilder Create(string key) => throw Upgrade();
    public TextInspectorPolicyBuilder WithId(Guid id) => throw Upgrade();
    public TextInspectorPolicyBuilder Describe(string description) => throw Upgrade();
    public TextInspectorPolicyBuilder WithTags(params string[] tags) => throw Upgrade();
    public TextInspectorPolicyBuilder InCategory(TextInspectorRuleCategory category) => throw Upgrade();
    public TextInspectorPolicyBuilder WithSeverity(int severity) => throw Upgrade();
    public TextInspectorPolicyBuilder WithScore(int rawPoints) => throw Upgrade();
    public TextInspectorPolicyBuilder ClassificationOnly() => throw Upgrade();
    public TextInspectorPolicyBuilder When(TextInspectorPolicyCondition condition) => throw Upgrade();
    public TextInspectorPolicy Build() => throw Upgrade();
}
