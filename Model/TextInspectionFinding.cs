namespace TextInspector;

/// <summary>
/// One logical report-card finding correlated from one composite policy or remaining scored evidence
/// in a single category. Rule matches are intentionally implementation detail below this layer.
/// </summary>
public sealed class TextInspectionFinding
{
    /// <summary>Deterministic report identifier, for example policy:Preset_Phishing_AccountTakeover.</summary>
    public string FindingId { get; init; } = "";

    /// <summary>Stable semantic code suitable for UI routing, analytics and serialization.</summary>
    public string FindingCode { get; init; } = "";

    public TextInspectionFindingKind FindingType { get; init; }
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public TextInspectorRuleCategory? Category { get; init; }
    public TextInspectionRiskLevel RiskLevel { get; init; }
    public int SeverityScore { get; init; }
    public int TotalRiskPoints { get; init; }
    public bool ContributesToRisk { get; init; }

    /// <summary>Smallest original-text span containing this finding's evidence, when available.</summary>
    public int? Start { get; init; }
    public int? Length { get; init; }
    public int? EndExclusive => Start.HasValue && Length.HasValue ? Start.Value + Length.Value : null;

    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuleKeys { get; init; } = Array.Empty<string>();
    public string? PolicyKey { get; init; }

    /// <summary>Exact original-text evidence correlated into this finding.</summary>
    public IReadOnlyList<TextEvidence> Evidence { get; init; } = Array.Empty<TextEvidence>();

    /// <summary>Developer/UI-friendly explanations for why this finding exists.</summary>
    public IReadOnlyList<TextInspectionReportReason> Reasons { get; init; } = Array.Empty<TextInspectionReportReason>();
}
