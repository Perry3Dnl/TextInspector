namespace TextInspector;

/// <summary>Pro-compatible raw-match filter. Filtered matching requires TextInspector.Pro.</summary>
public sealed class TextRuleMatchFilter
{
    public Guid[] RuleIds { get; init; } = Array.Empty<Guid>();
    public string[] RuleKeys { get; init; } = Array.Empty<string>();
    public TextInspectorRuleCategory[] Categories { get; init; } = Array.Empty<TextInspectorRuleCategory>();
    public string[] Tags { get; init; } = Array.Empty<string>();
    public bool RequireAllTags { get; init; }
    public int? MinimumSeverityScore { get; init; }
    public int? MinimumPriority { get; init; }
}
