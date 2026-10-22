namespace TextInspector;

/// <summary>Source-compatible API placeholder. Custom configuration requires TextInspector Pro.</summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class TextInspectorRuleBuilder
{
    private TextInspectorRuleBuilder() { }
    public static TextInspectorRuleBuilder Create(string key, TextInspectorRuleCategory category)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder WithId(Guid id)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder Match(string regex)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder Describe(string description)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder WithTags(params string[] tags)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder WithDetectionScore(int score)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder WithSeverity(int severity)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder WithScoring(int rawWeight, int maximumOccurrences = 5)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder EvidenceOnly()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder Capture(string group, TextInspectorEvidenceCaptureMode mode = TextInspectorEvidenceCaptureMode.LastCapture)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder WithPriority(int priority)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder Prefilter(TextInspectorPrefilterMode mode, params string[] literals)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRuleBuilder WithContext(Action<TextInspectorRuleContextBuilder> configure)
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");

    public TextInspectorRulePattern Build()
        => throw new NotSupportedException("Custom rule and policy authoring requires TextInspector Pro.");
}
