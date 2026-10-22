namespace TextInspector;

/// <summary>
/// A recursive deterministic policy condition. Atomic Rule/Category predicates can inspect only
/// scored evidence (the default) or all observed evidence. Composite conditions combine them
/// without invoking external services or probabilistic models.
/// </summary>
public sealed class TextInspectorPolicyCondition
{
    public TextInspectorPolicyConditionKind Kind { get; set; }

    /// <summary>Used by <see cref="TextInspectorPolicyConditionKind.Rule"/>.</summary>
    public string? RuleKey { get; set; }

    /// <summary>Used by <see cref="TextInspectorPolicyConditionKind.Category"/>.</summary>
    public TextInspectorRuleCategory? Category { get; set; }

    /// <summary>
    /// Atomic predicates consider only evidence that contributes base score when true. Set false
    /// deliberately when a policy should consume supporting/evidence-only or context-suppressed hits.
    /// </summary>
    public bool ScoredOnly { get; set; } = true;

    /// <summary>Minimum matching evidence occurrences for Rule/Category predicates.</summary>
    public int MinimumOccurrences { get; set; } = 1;

    /// <summary>Used by AtLeast: number of child conditions that must be satisfied.</summary>
    public int MinimumSatisfied { get; set; } = 1;

    /// <summary>
    /// Used by Proximity. All child witnesses must fit entirely inside a deterministic original-text
    /// window of this many characters.
    /// </summary>
    public int WindowCharacters { get; set; } = 120;

    public TextInspectorPolicyCondition[] Conditions { get; set; } = Array.Empty<TextInspectorPolicyCondition>();
}
