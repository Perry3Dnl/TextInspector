namespace TextInspector;

/// <summary>
/// Shared Pro-facing API declarations. Unsupported advanced operations explicitly require Pro;
/// the complete Free inspection, reporting and masking implementation is unaffected.
/// </summary>
public sealed partial class TextInspectorEngine
{
    public static TextInspectorEngine Compile(
        TextInspectorScoringOptions options, IEnumerable<TextInspectorRulePattern> rules)
        => throw ProRequired();

    public static TextInspectorEngine Compile(
        TextInspectorScoringOptions options, IEnumerable<TextInspectorRulePattern> rules,
        IEnumerable<TextInspectorPolicy> policies)
        => throw ProRequired();

    public static TextInspectorCompilationResult CompileWithDiagnostics(
        TextInspectorScoringOptions options, IEnumerable<TextInspectorRulePattern> rules)
        => throw ProRequired();

    public static TextInspectorCompilationResult CompileWithDiagnostics(
        TextInspectorScoringOptions options, IEnumerable<TextInspectorRulePattern> rules,
        IEnumerable<TextInspectorPolicy> policies)
        => throw ProRequired();

    public bool HasMatch(string? text, TextRuleMatchFilter? filter)
        => throw ProRequired();

    public TextRuleMatch? FindFirstMatch(string? text, TextRuleMatchFilter? filter = null)
        => throw ProRequired();

    public IReadOnlyList<TextRuleMatch> FindMatches(
        string? text, TextRuleMatchFilter? filter, int maxMatches = int.MaxValue)
        => throw ProRequired();

    public IReadOnlyList<TextInspectionResult> InspectManyParallel(
        IEnumerable<string?> texts, TextInspectionOptions? options = null,
        int maxDegreeOfParallelism = -1)
        => throw ProRequired();

    private static NotSupportedException ProRequired()
        => new("Custom compilation, targeted matching, and parallel inspection require TextInspector Pro.");
}
