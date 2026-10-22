namespace TextInspector;

/// <summary>
/// Optional high-performance/advanced inspection surface implemented by TextInspectorService.
/// Kept separate from ITextInspectorService so existing third-party service implementations remain
/// source-compatible while newer consumers can access raw fast paths and exact output controls.
/// </summary>
public interface ITextInspectorDetectionService
{
    /// <summary>Runs full deterministic inspection using explicit evidence/highlight output options.</summary>
    TextInspectionResult Inspect(string? text,TextInspectionOptions? options);

    /// <summary>
    /// Cheap raw primary-rule pre-screen. It skips context,  scoring, snippets and source mapping.
    /// </summary>
    bool HasMatch(string? text);

    /// <summary>
    /// Returns exact original-string spans for raw primary rule matches without full scoring/policy work.
    /// </summary>
    IReadOnlyList<TextRuleMatch> FindMatches(string? text,int maxMatches = int.MaxValue);

    /// <summary>Inspects a batch against one stable compiled snapshot and preserves input order.</summary>
    IReadOnlyList<TextInspectionResult> InspectMany(
        IEnumerable<string?> texts,
        TextInspectionOptions? options = null);

}
