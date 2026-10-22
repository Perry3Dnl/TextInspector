namespace TextInspector;

/// <summary>The deterministic predicate/combinator used by a composite policy condition.</summary>
public enum TextInspectorPolicyConditionKind
{
    Rule = 1,
    Category = 2,
    AllOf = 3,
    AnyOf = 4,
    AtLeast = 5,
    Proximity = 6
}
