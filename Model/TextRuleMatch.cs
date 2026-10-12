namespace TextInspector;

/// <summary>
/// Lightweight raw rule match returned by FindMatches. This bypasses context gates, policies,
/// scoring, snippets, and category aggregation and is intended for fast pre-screening/highlighting.
/// </summary>
public sealed class TextRuleMatch
{
    public Guid RuleId { get; init; }
    public string RuleKey { get; init; } = "";
    public TextInspectorRuleCategory Category { get; init; }
    public int SeverityScore { get; init; }
    public int Priority { get; init; }

    /// <summary>Positive 1..100 detection value for every returned raw match.</summary>
    public int DetectionScore { get; init; }

    public int HighlightIntensity { get; init; }
    public int Start { get; init; }
    public int Length { get; init; }
    public int EndExclusive => Start + Length;
}
