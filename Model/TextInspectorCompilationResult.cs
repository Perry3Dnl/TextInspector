namespace TextInspector;

internal sealed class TextInspectorCompilationResult
{
    public TextInspectorEngine Engine { get; init; } = null!;
    public IReadOnlyList<TextInspectorDiagnostic> Diagnostics { get; init; } = Array.Empty<TextInspectorDiagnostic>();
}
