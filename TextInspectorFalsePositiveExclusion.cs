namespace TextInspector;

/// <summary>Suppresses one known benign matched text for one specific rule during scored inspection.</summary>
/// <remarks>Exclusions do not modify the regex, fast raw matching APIs, or other rule matches.</remarks>
public sealed class TextInspectorFalsePositiveExclusion
{
    public string RuleKey { get; set; } = "";
    public string MatchedText { get; set; } = "";
}
