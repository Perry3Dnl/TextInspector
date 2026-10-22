namespace TextInspector;

/// <summary>
/// Source-compatible extension methods for Pro service operations without adding mandatory
/// members to the historical Free service interfaces. Existing third-party implementations
/// of ITextInspectorService and ITextInspectorDetectionService remain source-compatible.
/// </summary>
public static class ProInterfaceExtensions
{
    public static TextSafetyInfo Analyze(this ITextInspectorService service, string? text)
        => service.Inspect(text).RiskAssessment;

    public static string MarkText(this ITextInspectorService service, string? text)
        => service.Inspect(text, includeMarkedHtml: true).MarkedHtml ?? "";

    public static Task ReloadAsync(this ITextInspectorService service, CancellationToken ct = default)
        => throw ProRequired();

    public static Task<TextInspectorScoringOptions> GetOptionsAsync(
        this ITextInspectorService service, CancellationToken ct = default)
        => throw ProRequired();

    public static Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(
        this ITextInspectorService service, CancellationToken ct = default)
        => throw ProRequired();

    public static Task SaveAsync(this ITextInspectorService service,
        TextInspectorScoringOptions options, IReadOnlyList<TextInspectorRulePattern> rules,
        Guid userId, CancellationToken ct = default)
        => throw ProRequired();

    public static IReadOnlyList<TextInspectionResult> InspectManyParallel(
        this ITextInspectorDetectionService service, IEnumerable<string?> texts,
        TextInspectionOptions? options = null, int maxDegreeOfParallelism = -1)
        => throw ProRequired();

    private static NotSupportedException ProRequired() =>
        new("Pro service configuration, persistence, and parallel inspection require TextInspector Pro.");
}
