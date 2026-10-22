namespace TextInspector;

public sealed class TextInspectorMaskingOptions
{
    public TextInspectorMaskingOptions() { }
    public bool Enabled { get; set; }
    /// <summary>Pro-only advanced masking filter; Free uses its fixed masking behavior.</summary>
    public int MinimumDetectionScore { get; set; } = 1;
    public TextInspectorRuleCategory[] Categories { get; set; } = Array.Empty<TextInspectorRuleCategory>();
    public string[] RuleKeys { get; set; } = Array.Empty<string>();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public bool RequireAllTags { get; set; }
    public bool ScoredEvidenceOnly { get; set; }
    public char MaskCharacter { get; set; } = '*';
    internal void Validate()
    {
        if(char.IsControl(MaskCharacter) || char.IsSurrogate(MaskCharacter))
            throw new InvalidOperationException("MaskCharacter must be a visible non-surrogate character.");
    }
}
