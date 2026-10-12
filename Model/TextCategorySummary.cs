namespace TextInspector;

public sealed class TextCategorySummary
{
    public TextInspectorRuleCategory Category { get; init; }
    public int TotalEvidenceCount { get; init; }
    public int ScoringEvidenceCount { get; init; }
    public int TotalRiskPoints { get; init; }
    public int HighestDetectedSeverity { get; init; }
    public int HighestScoredSeverity { get; init; }
    public int HighlightIntensity { get; init; }
}
