namespace TextInspector;

internal sealed class TextInspectorMaskingOptions
{
    public TextInspectorMaskingOptions() { }
    public bool Enabled { get; set; }
    public char MaskCharacter { get; set; } = '*';
    internal void Validate()
    {
        if(char.IsControl(MaskCharacter) || char.IsSurrogate(MaskCharacter))
            throw new InvalidOperationException("MaskCharacter must be a visible non-surrogate character.");
    }
}
