namespace TextInspector;

/// <summary>Compares a numeric capture from the same regex match with a configured value.</summary>
internal sealed class TextInspectorNumericCondition
{
    public TextInspectorNumericCondition() { }

    /// <summary>Named or numbered regex group. It must capture exactly once per match.</summary>
    public string CaptureGroup { get; set; } = "";

    public TextInspectorNumericComparison Comparison { get; set; } = TextInspectorNumericComparison.GreaterThan;
    public decimal Value { get; set; }

    /// <summary>Explicit "." or "," decimal separator. Grouping, exponents and units are not accepted.</summary>
    public string DecimalSeparator { get; set; } = ".";
}
