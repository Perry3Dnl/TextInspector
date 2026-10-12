namespace TextInspector;

/// <summary>An underlying rule-evidence witness that caused a composite policy to match.</summary>
public sealed class TextPolicyEvidenceReference
{
    public Guid RuleId { get; init; }
    public string RuleKey { get; init; } = "";
    public TextInspectorRuleCategory Category { get; init; }
    public int Start { get; init; }
    public int Length { get; init; }
    public int EndExclusive => Start + Length;
    public string MatchedText { get; init; } = "";
    public bool ContributesToRisk { get; init; }
    public TextEvidenceScoringDisposition ScoringStatus { get; init; }
}
