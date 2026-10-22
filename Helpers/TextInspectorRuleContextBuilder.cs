namespace TextInspector;
public sealed class TextInspectorRuleContextBuilder {
    private static NotSupportedException Upgrade() => new("Update to TextInspector.Pro for context configuration.");
    public TextInspectorRuleContextBuilder Within(int characters) => throw Upgrade();
    public TextInspectorRuleContextBuilder RequireAtLeast(int selectors) => throw Upgrade();
    public TextInspectorRuleContextBuilder RequireRules(params string[] keys) => throw Upgrade();
    public TextInspectorRuleContextBuilder RequireCategories(params TextInspectorRuleCategory[] categories) => throw Upgrade();
    public TextInspectorRuleContextBuilder RequirePatterns(params string[] patterns) => throw Upgrade();
    public TextInspectorRuleContextBuilder ExcludeRules(params string[] keys) => throw Upgrade();
    public TextInspectorRuleContextBuilder ExcludeCategories(params TextInspectorRuleCategory[] categories) => throw Upgrade();
    public TextInspectorRuleContextBuilder ExcludePatterns(params string[] patterns) => throw Upgrade();
    public TextInspectorRuleContext Build() => throw Upgrade();
}
