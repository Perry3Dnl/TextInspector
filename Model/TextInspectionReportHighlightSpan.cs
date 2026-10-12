namespace TextInspector;

/// <summary>
/// Non-overlapping UI highlight span associated with zero or more logical finding IDs.
/// Spans without a finding are supporting/observed signals.
/// </summary>
public sealed class TextInspectionReportHighlightSpan
{
    public int Start { get; init; }
    public int Length { get; init; }
    public int EndExclusive => Start + Length;
    public string MatchedText { get; init; } = "";

    /// <summary>Strongest positive detection score among evidence active on this span.</summary>
    public int DetectionScore { get; init; }

    public TextInspectionRiskLevel RiskLevel { get; init; }
    public int HighestDetectedSeverity { get; init; }
    public bool HasScoringEvidence { get; init; }
    public IReadOnlyList<string> FindingIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuleKeys { get; init; } = Array.Empty<string>();
    public IReadOnlyList<TextInspectorRuleCategory> Categories { get; init; } = Array.Empty<TextInspectorRuleCategory>();
}
