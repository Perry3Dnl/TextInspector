namespace TextInspector;

/// <summary>A matched composite policy and the deterministic evidence witness that satisfied it.</summary>
public sealed class TextPolicyMatch
{
    public Guid PolicyId { get; init; }
    public string PolicyKey { get; init; } = "";
    public string? Description { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public TextInspectorRuleCategory? Category { get; init; }
    public int SeverityScore { get; init; }
    public int RiskPoints { get; init; }
    public bool ContributesToRisk => RiskPoints > 0;

    /// <summary>Canonical, human-readable representation of the configured condition.</summary>
    public string ConditionSummary { get; init; } = "";

    /// <summary>Atomic selectors used by the deterministic witness that satisfied the condition.</summary>
    public IReadOnlyList<string> MatchedSelectors { get; init; } = Array.Empty<string>();

    /// <summary>Underlying rule evidence used as the policy witness.</summary>
    public IReadOnlyList<TextPolicyEvidenceReference> Evidence { get; init; } = Array.Empty<TextPolicyEvidenceReference>();

    /// <summary>Smallest witness span in original-text coordinates, when the policy used evidence.</summary>
    public int? WitnessStart { get; init; }
    public int? WitnessLength { get; init; }
}
