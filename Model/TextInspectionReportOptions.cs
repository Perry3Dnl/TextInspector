namespace TextInspector;

/// <summary>Controls presentation-oriented report output without changing inspection/scoring semantics.</summary>
public sealed class TextInspectionReportOptions
{
    /// <summary>Risk level at or above which <see cref="TextInspectionReport.RequiresReview"/> becomes true.</summary>
    public TextInspectionRiskLevel FlaggedMinimumRiskLevel { get; init; } = TextInspectionRiskLevel.Medium;

    /// <summary>Include evidence that was observed but deliberately did not contribute to scoring.</summary>
    public bool IncludeSupportingEvidence { get; init; } = true;

    /// <summary>Return UI highlight spans associated with logical finding IDs.</summary>
    public bool IncludeHighlights { get; init; } = true;

    /// <summary>Populate exact 1-based line/column coordinates on report evidence.</summary>
    public bool IncludeSourceLocations { get; init; } = true;

    /// <summary>Maximum reasons returned per finding. Null means unlimited.</summary>
    public int? MaxReasonsPerFinding { get; init; } = 8;

    /// <summary>Maximum logical findings returned after risk/severity ordering. Null means unlimited.</summary>
    public int? MaxFindings { get; init; }

    internal void Validate()
    {
        if(!Enum.IsDefined(typeof(TextInspectionRiskLevel),FlaggedMinimumRiskLevel))
            throw new ArgumentOutOfRangeException(nameof(FlaggedMinimumRiskLevel));
        if(MaxReasonsPerFinding is < 0)
            throw new ArgumentOutOfRangeException(nameof(MaxReasonsPerFinding));
        if(MaxFindings is < 0)
            throw new ArgumentOutOfRangeException(nameof(MaxFindings));
    }
}
