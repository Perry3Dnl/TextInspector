namespace TextInspector;

/// <summary>Explainable result of evaluating a rule's optional context policy.</summary>
public sealed class TextEvidenceContext
{
    public int WindowCharacters { get; init; }
    public int MinimumRequiredMatches { get; init; }
    public bool IsSatisfied { get; init; }
    public IReadOnlyList<string> RequiredSelectors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ExcludedSelectors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MatchedRequirements { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MatchedExclusions { get; init; } = Array.Empty<string>();
}
