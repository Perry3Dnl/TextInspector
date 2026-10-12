namespace TextInspector;

/// <summary>
/// Deterministic, UI-ready representation of one inspection. It correlates technical rule evidence
/// into logical findings while retaining exact spans and transparent score/category data.
/// </summary>
public sealed class TextInspectionReport
{
    /// <summary>Schema version for serialized report consumers.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>Configuration generation used to compile the engine that produced this report.</summary>
    public int ConfigurationVersion { get; init; }

    public int InputLength { get; init; }
    public TextInspectionRiskLevel RiskLevel { get; init; }
    public bool RequiresReview { get; init; }
    public string Summary { get; init; } = "";

    /// <summary>Locally masked input when masking is enabled for the compiled inspector.</summary>
    public string? MaskedText { get; init; }

    public bool HasDetections => RiskAssessment.DetectionScore > 0;

    public TextSafetyInfo RiskAssessment { get; init; } = new TextSafetyInfo(0,0,0,0,0);
    public IReadOnlyList<TextInspectionFinding> Findings { get; init; } = Array.Empty<TextInspectionFinding>();
    public IReadOnlyList<TextEvidence> SupportingEvidence { get; init; } = Array.Empty<TextEvidence>();
    public IReadOnlyList<TextInspectionReportHighlightSpan> Highlights { get; init; } = Array.Empty<TextInspectionReportHighlightSpan>();
    public IReadOnlyList<TextCategorySummary> CategorySummaries { get; init; } = Array.Empty<TextCategorySummary>();
    public IReadOnlyList<TextScoreContribution> RiskContributions { get; init; } = Array.Empty<TextScoreContribution>();
    public IReadOnlyList<TextUrlObservation> UrlObservations { get; init; } = Array.Empty<TextUrlObservation>();
    public IReadOnlyList<TextInspectorDiagnostic> Diagnostics { get; init; } = Array.Empty<TextInspectorDiagnostic>();

    public int FindingCount => Findings.Count;
    public int ScoringFindingCount => Findings.Count(static finding => finding.ContributesToRisk);
    public int SupportingSignalCount => SupportingEvidence.Count;
}
