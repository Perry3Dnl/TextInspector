namespace TextInspector;

/// <summary>Compares a numeric capture from the same regex match with a configured value.</summary>
internal sealed class TextInspectorNumericCondition
{
    public TextInspectorNumericCondition() { }

    /// <summary>Named or numbered regex group. It must capture exactly once per match.</summary>
    public string CaptureGroup { get; set; } = "";

    public TextInspectorNumericComparison Comparison { get; set; } = TextInspectorNumericComparison.GreaterThan;
    public decimal Value { get; set; }

    /// <summary>Optional regex capture naming the unit associated with the numeric capture.</summary>
    public string? UnitCaptureGroup { get; set; }

    /// <summary>Optional conversion factors to the units used by Value. Null means no unit conversion.</summary>
    public Dictionary<string,decimal>? UnitFactors { get; set; }

    /// <summary>Explicit "." or "," decimal separator. Grouping, exponents and units are not accepted.</summary>
    public string DecimalSeparator { get; set; } = ".";
}
