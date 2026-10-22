namespace TextInspector;

/// <summary>One triggered link with exact participating evidence and the points actually attributed to it.</summary>
public sealed class TextLinkedRuleMatch
{
    public string LinkedRuleKey { get; init; } = "";
    public string? Description { get; init; }
    public bool IsLinkedRule => true;
    public int Multiplier { get; init; }
    public int MinimumMatchedRules { get; init; }
    public IReadOnlyList<string> MatchedRuleKeys { get; init; } = Array.Empty<string>();
    public int FixedRiskPoints { get; init; }
    public int EscalationRiskPoints { get; init; }
    public int RiskPoints => (int)Math.Min(int.MaxValue, (long)FixedRiskPoints + EscalationRiskPoints);
    public bool ContributesToRisk => RiskPoints > 0;
    public int EffectiveSeverity { get; init; }
    public IReadOnlyList<TextPolicyEvidenceReference> Evidence { get; init; } = Array.Empty<TextPolicyEvidenceReference>();
}
