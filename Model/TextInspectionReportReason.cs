namespace TextInspector;

/// <summary>
/// Human-readable explanation for a finding. Source keys remain available for audit/debug use,
/// but UI consumers can normally render <see cref="Message"/> directly.
/// </summary>
public sealed class TextInspectionReportReason
{
    public TextInspectionReasonKind Kind { get; init; }
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public bool ContributesToRisk { get; init; }
    public string? RuleKey { get; init; }
    public string? LinkedRuleKey { get; init; }
    public bool IsLinkedRule => LinkedRuleKey is not null;
    public string? PolicyKey { get; init; }
    public int? Start { get; init; }
    public int? Length { get; init; }
    public int? EndExclusive => Start.HasValue && Length.HasValue ? Start.Value + Length.Value : null;
}
