namespace TextInspector;

/// <summary>Pro-only custom regex-rule builder signatures. Free's built-in rule set remains immutable.</summary>
public sealed class TextInspectorRuleBuilder
{
    private static NotSupportedException Upgrade() => new("Update to TextInspector.Pro to create custom detection rules.");
    public static TextInspectorRuleBuilder Create(string key, TextInspectorRuleCategory category) => throw Upgrade();
    public TextInspectorRuleBuilder WithId(Guid id) => throw Upgrade();
    public TextInspectorRuleBuilder Match(string regex) => throw Upgrade();
    public TextInspectorRuleBuilder WhenNumber(string captureGroup,
        TextInspectorNumericComparison comparison, decimal value, string decimalSeparator = ".") => throw Upgrade();
    public TextInspectorRuleBuilder WhenNumberWithUnit(string captureGroup,
        TextInspectorNumericComparison comparison, decimal value, string unitCaptureGroup,
        IReadOnlyDictionary<string,decimal> unitFactors, string decimalSeparator = ".") => throw Upgrade();
    public TextInspectorRuleBuilder Link(TextInspectorLinkedRule linkedRule) => throw Upgrade();
    public TextInspectorRuleBuilder Describe(string description) => throw Upgrade();
    public TextInspectorRuleBuilder WithTags(params string[] tags) => throw Upgrade();
    public TextInspectorRuleBuilder WithDetectionScore(int score) => throw Upgrade();
    public TextInspectorRuleBuilder WithSeverity(int severity) => throw Upgrade();
    public TextInspectorRuleBuilder WithScoring(int rawWeight, int maximumOccurrences = 5) => throw Upgrade();
    public TextInspectorRuleBuilder EvidenceOnly() => throw Upgrade();
    public TextInspectorRuleBuilder Capture(string group,
        TextInspectorEvidenceCaptureMode mode = TextInspectorEvidenceCaptureMode.LastCapture) => throw Upgrade();
    public TextInspectorRuleBuilder WithPriority(int priority) => throw Upgrade();
    public TextInspectorRuleBuilder Prefilter(TextInspectorPrefilterMode mode, params string[] literals) => throw Upgrade();
    public TextInspectorRuleBuilder WithContext(Action<TextInspectorRuleContextBuilder> configure) => throw Upgrade();
    public TextInspectorRulePattern Build() => throw Upgrade();
}
