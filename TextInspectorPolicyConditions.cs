namespace TextInspector;

/// <summary>Pro-compatible policy condition DTO factories. Free does not evaluate composite policies.</summary>
public static class TextInspectorPolicyConditions
{
    public static TextInspectorPolicyCondition Rule(string ruleKey,bool scoredOnly = true,int minimumOccurrences = 1)
        => new()
        {
            Kind = TextInspectorPolicyConditionKind.Rule,
            RuleKey = ruleKey,
            ScoredOnly = scoredOnly,
            MinimumOccurrences = minimumOccurrences
        };

    public static TextInspectorPolicyCondition Category(TextInspectorRuleCategory category,bool scoredOnly = true,int minimumOccurrences = 1)
        => new()
        {
            Kind = TextInspectorPolicyConditionKind.Category,
            Category = category,
            ScoredOnly = scoredOnly,
            MinimumOccurrences = minimumOccurrences
        };

    public static TextInspectorPolicyCondition AllOf(params TextInspectorPolicyCondition[] conditions)
        => new() { Kind = TextInspectorPolicyConditionKind.AllOf, Conditions = conditions ?? Array.Empty<TextInspectorPolicyCondition>() };

    public static TextInspectorPolicyCondition AnyOf(params TextInspectorPolicyCondition[] conditions)
        => new() { Kind = TextInspectorPolicyConditionKind.AnyOf, Conditions = conditions ?? Array.Empty<TextInspectorPolicyCondition>() };

    public static TextInspectorPolicyCondition AtLeast(int minimumSatisfied,params TextInspectorPolicyCondition[] conditions)
        => new()
        {
            Kind = TextInspectorPolicyConditionKind.AtLeast,
            MinimumSatisfied = minimumSatisfied,
            Conditions = conditions ?? Array.Empty<TextInspectorPolicyCondition>()
        };

    public static TextInspectorPolicyCondition Proximity(int windowCharacters,params TextInspectorPolicyCondition[] conditions)
        => new()
        {
            Kind = TextInspectorPolicyConditionKind.Proximity,
            WindowCharacters = windowCharacters,
            Conditions = conditions ?? Array.Empty<TextInspectorPolicyCondition>()
        };
}
