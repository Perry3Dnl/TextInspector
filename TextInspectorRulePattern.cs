namespace TextInspector;

/// <summary>
/// A single match pattern the inspector compiles.
/// </summary>
public class TextInspectorRulePattern
{
    public TextInspectorRulePattern() { }
    public Guid TextInspectorRuleId { get; set; }
    public string Key { get; set; } = "";
    /// <summary>Links declared once here; their members may come from any loaded rule set.</summary>
    public TextInspectorLinkedRule[] LinkedRules { get; set; } = Array.Empty<TextInspectorLinkedRule>();

    public string? Description { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public TextInspectorRuleCategory Category { get; set; }

    /// <summary>
    /// Independent 1..100 strength assigned to a successful detection.
    /// This is a deterministic rule/configuration value, not a statistical probability.
    /// It is deliberately separate from severity and risk contribution.
    /// </summary>
    public int DetectionScore { get; set; } = 100;

    public int SeverityScore { get; set; }
    public string RegexPattern { get; set; } = "";

    /// <summary>All numeric conditions must hold for the same regex match. Empty preserves regex-only matching.</summary>
    public TextInspectorNumericCondition[] NumericConditions { get; set; } = Array.Empty<TextInspectorNumericCondition>();

    /// <summary>
    /// Optional named or numbered regex capture group to use as the exact evidence/highlight span.
    /// The full regex still controls detection. Empty/null uses the complete match.
    /// </summary>
    public string? EvidenceGroup { get; set; }

    /// <summary>Pro-compatible evidence capture configuration; custom rule compilation requires TextInspector.Pro.</summary>
    public TextInspectorEvidenceCaptureMode EvidenceCaptureMode { get; set; } = TextInspectorEvidenceCaptureMode.LastCapture;

    /// <summary>Higher-priority rules are evaluated first. This mainly affects fast first-match APIs.</summary>
    public int Priority { get; set; }

    /// <summary>
    /// Optional literal terms used as a cheap normalized-text prefilter before the regex executes.
    /// Configure only terms that must be present whenever the regex can match, otherwise false negatives are possible.
    /// </summary>
    public string[] PrefilterTerms { get; set; } = Array.Empty<string>();

    /// <summary>Determines whether any or all configured prefilter terms must be present.</summary>
    public TextInspectorPrefilterMode PrefilterMode { get; set; } = TextInspectorPrefilterMode.Any;

    public int RuleWeight { get; set; }

    /// <summary>Optional proximity/context gate applied after the primary regex matches.</summary>
    public TextInspectorRuleContext? Context { get; set; }

    /// <summary>Maximum occurrences of this rule that may contribute RuleWeight. 0 makes the rule evidence-only.</summary>
    public int MaxCountForScoring { get; set; } = 5;

    /// <summary>Maximum occurrences of this rule that receive context snippets. 0 uses the global default.</summary>
    public int MaxEvidenceSnippets { get; set; } = 3;
    public bool IsEnabled { get; set; } = true;
}
