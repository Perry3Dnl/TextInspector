namespace TextInspector;

/// <summary>
/// 1-based line/column coordinates for an evidence span. Columns are UTF-16 code-unit based so
/// they map exactly to .NET string offsets. End coordinates are exclusive.
/// </summary>
public sealed class TextSourceRange
{
    public int StartLine { get; init; }
    public int StartColumn { get; init; }
    public int EndLine { get; init; }
    public int EndColumn { get; init; }
}
