namespace TextInspector;

/// <summary>
/// Parsed URL/domain observation with exact original-text coordinates. These properties are
/// descriptive metadata; they do not introduce a separate URL risk score.
/// </summary>
public sealed class TextUrlObservation
{
    public int Start { get; init; }
    public int Length { get; init; }
    public int EndExclusive => Start + Length;
    public string MatchedText { get; init; } = "";
    public string Scheme { get; init; } = "";
    public string Host { get; init; } = "";
    public int? Port { get; init; }
    public string PathAndQuery { get; init; } = "";

    public TextUrlTrustClassification Trust { get; init; }
    public string? MatchedConfiguredDomain { get; init; }

    public bool LooksLikeFirstParty { get; init; }
    public bool HasIpHost { get; init; }
    public bool HasPunycodeHost { get; init; }
    public bool HasUserInfo { get; init; }
    public bool HasUnusualPort { get; init; }
    public bool IsKnownShortener { get; init; }

    /// <summary>Configured URL-intelligence keywords observed in the path/query.</summary>
    public IReadOnlyList<string> SuspiciousPathTerms { get; init; } = Array.Empty<string>();
}
