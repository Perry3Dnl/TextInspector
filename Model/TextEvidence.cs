using System.Diagnostics.CodeAnalysis;

namespace TextInspector;

[ExcludeFromCodeCoverage]
public sealed class TextEvidence
{
    public Guid RuleId { get; init; }
    public string RuleKey { get; init; } = "";
    public string? RuleDescription { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public TextInspectorRuleCategory Category { get; init; }
    public int SeverityScore { get; init; }
    public int RuleWeight { get; init; }
    public int MaxCountForScoring { get; init; }

    public int Start { get; init; }
    public int Length { get; init; }
    public int EndExclusive => Start + Length;

    /// <summary>Optional 1-based line/column coordinates for this exact source span.</summary>
    public TextSourceRange? SourceRange { get; init; }

    /// <summary>The exact original text matched by this evidence item.</summary>
    public string MatchedText { get; init; } = "";

    /// <summary>Optional context around the match. Limited per rule by MaxEvidenceSnippets.</summary>
    public string? Snippet { get; init; }
    public int? SnippetStart { get; init; }

    /// <summary>
    /// Positive 1..100 score indicating that this exact evidence span was detected.
    /// Unlike risk scoring, every real match has a value greater than zero.
    /// </summary>
    public int DetectionScore { get; init; }

    /// <summary>Normalized 0..100 severity used for highlighting.</summary>
    public int HighlightIntensity { get; init; }

    /// <summary>Context-gate evaluation for this occurrence, when the rule defines one.</summary>
    public TextEvidenceContext? Context { get; init; }

    /// <summary>True when this occurrence contributes its RuleWeight to scoring.</summary>
    public bool ContributesToRisk { get; init; }
    public TextEvidenceScoringDisposition ScoringStatus { get; init; }
}
