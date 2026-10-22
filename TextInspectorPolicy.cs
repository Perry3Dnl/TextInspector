namespace TextInspector;

/// <summary>
/// A named deterministic policy evaluated after rule context, count limits, and exact-match
/// de-duplication. A matched policy can add raw score and emits an explainable policy match.
/// </summary>
public sealed class TextInspectorPolicy
{
    public Guid TextInspectorPolicyId { get; set; }
    public string Key { get; set; } = "";
    public string? Description { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>Optional semantic category for the derived policy signal.</summary>
    public TextInspectorRuleCategory? Category { get; set; }

    public int SeverityScore { get; set; }

    /// <summary>Raw score added once when the policy matches. 0 makes the policy classification-only.</summary>
    public int RiskPoints { get; set; }

    public TextInspectorPolicyCondition Condition { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}
