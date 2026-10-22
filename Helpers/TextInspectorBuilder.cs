namespace TextInspector;

/// <summary>Pro custom-configuration builder API compatibility. Free cannot create or compile custom rule sets.</summary>
public sealed class TextInspectorBuilder
{
    private TextInspectorBuilder() { }

    private static NotSupportedException Upgrade() => new("Update to TextInspector.Pro to configure custom rules and policies.");
    public static TextInspectorBuilder Create() => throw Upgrade();
    public static TextInspectorBuilder Empty() => throw Upgrade();
    public TextInspectorBuilder WithOptions(TextInspectorScoringOptions options) => throw Upgrade();
    public TextInspectorBuilder ConfigureOptions(Action<TextInspectorScoringOptions> configure) => throw Upgrade();
    public TextInspectorBuilder AddRule(TextInspectorRulePattern rule) => throw Upgrade();
    public TextInspectorBuilder AddRule(string key, TextInspectorRuleCategory category, Action<TextInspectorRuleBuilder> configure) => throw Upgrade();
    public TextInspectorBuilder AddPolicy(TextInspectorPolicy policy) => throw Upgrade();
    public TextInspectorBuilder AddPolicy(string key, Action<TextInspectorPolicyBuilder> configure) => throw Upgrade();
    public TextInspectorBuilder ConfigureRule(string key, Action<TextInspectorRulePattern> configure) => throw Upgrade();
    public TextInspectorBuilder ConfigurePolicy(string key, Action<TextInspectorPolicy> configure) => throw Upgrade();
    public TextInspectorConfiguration Build() => throw Upgrade();
    public TextInspectorEngine Compile() => throw Upgrade();
    public TextInspectorService CreateService() => throw Upgrade();
}
