namespace TextInspector;

/// <summary>
/// Optional raw-match filter evaluated before regex execution. Multiple populated dimensions are ANDed;
/// values inside a dimension are ORed, except Tags when RequireAllTags is true.
/// </summary>
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
