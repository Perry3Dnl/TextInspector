namespace TextInspector;

/// <summary>A named link stored on one rule, referencing distinct existing rules in the same inspection.</summary>
internal sealed class TextInspectorLinkedRule
{
    public TextInspectorLinkedRule() { }
    public string Key { get; set; } = "";
    public string? Description { get; set; }
    public string[] RuleKeys { get; set; } = Array.Empty<string>();
    public int MinimumMatchedRules { get; set; } = 2;
    /// <summary>Multiplier for participating risk points and effective severity; overlapping links use the strongest once.</summary>
    public int Multiplier { get; set; } = 2;
    /// <summary>Additional fixed raw points, once per inspection. Zero still produces a linked-rule marker.</summary>
    public int RiskPoints { get; set; }
    /// <summary>Zero uses the whole input; otherwise participating evidence must share a window of this size.</summary>
    public int WindowCharacters { get; set; }
    /// <summary>False also permits evidence-only and count-limited hits, but never context/duplicate-suppressed hits.</summary>
    public bool ScoredOnly { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
}
