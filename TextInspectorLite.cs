namespace TextInspector;

/// <summary>Read-only information about the fixed embedded Lite preset.</summary>
public static class TextInspectorLite
{
    public const string Edition = "Lite";
    public static IReadOnlyList<TextInspectorRuleInfo> Rules { get; } = Array.AsReadOnly(
        TextInspectorPresets.LoadRules().Select(rule => new TextInspectorRuleInfo(
            rule.Key, rule.Category, rule.Description)).ToArray());
}

/// <summary>Rule metadata for settings and UI; contains no editable detection configuration.</summary>
public sealed class TextInspectorRuleInfo
{
    public string Key { get; }
    public TextInspectorRuleCategory Category { get; }
    public string? Description { get; }
    internal TextInspectorRuleInfo(string key, TextInspectorRuleCategory category, string? description)
    { Key = key; Category = category; Description = description; }
}
