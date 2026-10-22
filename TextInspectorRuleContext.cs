namespace TextInspector;

/// <summary>
/// Optional context gate for a rule. The primary regex still produces evidence, but the
/// occurrence only contributes to scoring when its required nearby context is present and
/// no exclusion matches.
/// </summary>
public sealed class TextInspectorRuleContext
{
    public TextInspectorRuleContext() { }
    /// <summary>
    /// Maximum normalized-character gap on either side of the primary match used for nearby
    /// rule/category evidence and context-regex checks.
    /// </summary>
    public int WindowCharacters { get; set; } = 80;

    /// <summary>Number of distinct positive selectors that must match when requirements exist.</summary>
    public int MinimumRequiredMatches { get; set; } = 1;

    /// <summary>
    /// Positive selectors across rule keys, categories, and regex patterns contribute to the
    /// requirement match count. If all requirement collections are empty, there is no positive
    /// requirement.
    /// </summary>
    public string[] RequireAnyRuleKeys { get; set; } = Array.Empty<string>();
    public TextInspectorRuleCategory[] RequireAnyCategories { get; set; } = Array.Empty<TextInspectorRuleCategory>();
    public string[] RequireAnyRegexPatterns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Any configured exclusion matching within the context window suppresses scoring for
    /// the occurrence while retaining it as evidence.
    /// </summary>
    public string[] ExcludeAnyRuleKeys { get; set; } = Array.Empty<string>();
    public TextInspectorRuleCategory[] ExcludeAnyCategories { get; set; } = Array.Empty<TextInspectorRuleCategory>();
    public string[] ExcludeAnyRegexPatterns { get; set; } = Array.Empty<string>();
}
