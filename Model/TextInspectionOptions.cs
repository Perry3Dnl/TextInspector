namespace TextInspector;

public sealed class TextInspectionOptions
{
    public bool IncludeMarkedHtml { get; init; }
    public bool IncludeMatchedText { get; init; } = true;
    public bool IncludeSnippets { get; init; } = true;
    public bool IncludeContextDetails { get; init; } = true;

    /// <summary>Return non-overlapping highlight spans mapped to the original text.</summary>
    public bool IncludeHighlightSpans { get; init; }

    /// <summary>When true, marked HTML/highlight spans include only evidence that contributes to scoring.</summary>
    public bool HighlightScoredEvidenceOnly { get; init; }

    /// <summary>Populate 1-based line/column coordinates for returned evidence.</summary>
    public bool IncludeSourceLocations { get; init; }

    /// <summary>
    /// When false, evidence suppressed by context/count limits/de-duplication and evidence-only
    /// rules are omitted from the returned Evidence collection. Scoring still uses the complete set.
    /// </summary>
    public bool IncludeNonScoringEvidence { get; init; } = true;

    /// <summary>
    /// Optional cap on evidence returned to the caller. Scoring always uses the complete match set.
    /// Null means no cap.
    /// </summary>
    public int? MaxReturnedEvidence { get; init; }

    /// <summary>Optional cap on returned highlight spans. Null means no cap.</summary>
    public int? MaxReturnedHighlightSpans { get; init; }
}
