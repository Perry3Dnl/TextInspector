namespace TextInspector;

/// <summary>
/// A non-overlapping source span suitable for editor/UI highlighting. Overlapping evidence is
/// merged into segments while retaining the active rule keys/categories and strongest severity.
/// Coordinates are UTF-16 string offsets into the original input.
/// </summary>
public sealed class TextHighlightSpan
{
    public int Start { get; init; }
    public int Length { get; init; }
    public int EndExclusive => Start + Length;

    /// <summary>Strongest positive detection score among evidence active on this span.</summary>
    public int DetectionScore { get; init; }

    public int HighlightIntensity { get; init; }
    public int EffectiveSeverity { get; init; }
    public IReadOnlyList<string> LinkedRuleKeys { get; init; } = Array.Empty<string>();
    public bool IsLinked => LinkedRuleKeys.Count > 0;
    public int HighestDetectedSeverity { get; init; }
    public bool HasScoringEvidence { get; init; }
    public IReadOnlyList<string> RuleKeys { get; init; } = Array.Empty<string>();
    public IReadOnlyList<TextInspectorRuleCategory> Categories { get; init; } = Array.Empty<TextInspectorRuleCategory>();
}
