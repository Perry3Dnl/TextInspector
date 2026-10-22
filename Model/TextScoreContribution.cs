namespace TextInspector;

/// <summary>A transparent raw-score adjustment applied after rule weights.</summary>
public sealed class TextScoreContribution
{
    public TextScoreContributionKind Kind { get; init; } = TextScoreContributionKind.BuiltInBonus;
    public Guid? PolicyId { get; init; }
    public string? LinkedRuleKey { get; init; }
    public bool IsLinkedRule => LinkedRuleKey is not null;
    public string? PolicyKey { get; init; }
    public string Key { get; init; } = "";
    public string Description { get; init; } = "";
    public int RiskPoints { get; init; }
}
