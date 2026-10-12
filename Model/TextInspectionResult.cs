using System.Diagnostics.CodeAnalysis;

namespace TextInspector;

[ExcludeFromCodeCoverage]
public sealed class TextInspectionResult
{
    public TextSafetyInfo RiskAssessment { get; init; } = new TextSafetyInfo(0,0,0,0,0);

    /// <summary>
    /// Locally masked copy of the original input when TextInspectorScoringOptions.Masking.Enabled is true.
    /// Null when masking is disabled.
    /// </summary>
    public string? MaskedText { get; init; }

    public bool HasDetections => RiskAssessment.DetectionScore > 0;

    public string? MarkedHtml { get; init; }
    public IReadOnlyList<TextEvidence> Evidence { get; init; } = Array.Empty<TextEvidence>();
    public IReadOnlyList<TextHighlightSpan> HighlightSpans { get; init; } = Array.Empty<TextHighlightSpan>();
    public IReadOnlyList<TextCategorySummary> CategorySummaries { get; init; } = Array.Empty<TextCategorySummary>();
    public IReadOnlyList<TextScoreContribution> RiskContributions { get; init; } = Array.Empty<TextScoreContribution>();
    public IReadOnlyList<TextPolicyMatch> PolicyMatches { get; init; } = Array.Empty<TextPolicyMatch>();
    public IReadOnlyList<TextUrlObservation> UrlObservations { get; init; } = Array.Empty<TextUrlObservation>();
    public IReadOnlyList<TextInspectorDiagnostic> Diagnostics { get; init; } = Array.Empty<TextInspectorDiagnostic>();
}
