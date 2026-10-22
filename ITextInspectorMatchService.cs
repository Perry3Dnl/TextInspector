namespace TextInspector;

/// <summary>
/// Optional targeted fast-match surface. Kept separate from ITextInspectorDetectionService to preserve
/// compatibility for third-party implementations of the earlier interface.
/// </summary>
public interface ITextInspectorMatchService
{
    bool HasMatch(string? text,TextRuleMatchFilter? filter);

    TextRuleMatch? FindFirstMatch(string? text,TextRuleMatchFilter? filter = null);

    IReadOnlyList<TextRuleMatch> FindMatches(
        string? text,
        TextRuleMatchFilter? filter,
        int maxMatches = int.MaxValue);
}
