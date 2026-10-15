namespace TextInspector;

/// <summary>Controls the shipped TextInspector rules and basic output; cannot add or replace rules.</summary>
public sealed class TextInspectorOptions
{
    public string[] DisabledRuleKeys { get; set; } = Array.Empty<string>();
    public TextInspectorRuleCategory[] DisabledCategories { get; set; } = Array.Empty<TextInspectorRuleCategory>();
    /// <summary>Known benign matches to suppress during full scored inspection. Each applies to one rule key.</summary>
    public TextInspectorFalsePositiveExclusion[] FalsePositiveExclusions { get; set; } = Array.Empty<TextInspectorFalsePositiveExclusion>();
    public bool EnableMasking { get; set; }
    public char MaskCharacter { get; set; } = '*';

    internal void Validate()
    {
        if(DisabledRuleKeys is null || DisabledCategories is null)
            throw new ArgumentException("Disabled rule keys and categories cannot be null.");
        var known = new HashSet<string>(TextInspectorDefaults.Rules.Select(rule => rule.Key), StringComparer.OrdinalIgnoreCase);
        if(DisabledRuleKeys.Any(key => string.IsNullOrWhiteSpace(key) || !known.Contains(key)))
            throw new ArgumentException("DisabledRuleKeys must contain shipped TextInspector rule keys.");
        if(FalsePositiveExclusions is null || FalsePositiveExclusions.Any(item => item is null ||
            string.IsNullOrWhiteSpace(item.RuleKey) || string.IsNullOrWhiteSpace(item.MatchedText) || !known.Contains(item.RuleKey)))
            throw new ArgumentException("FalsePositiveExclusions must contain valid rule keys and nonblank matched text.");
        if(DisabledCategories.Any(category => !Enum.IsDefined(typeof(TextInspectorRuleCategory), category)))
            throw new ArgumentException("DisabledCategories contains an unknown category.");
        if(char.IsControl(MaskCharacter) || char.IsSurrogate(MaskCharacter))
            throw new ArgumentException("MaskCharacter must be a visible non-surrogate character.");
    }
}
