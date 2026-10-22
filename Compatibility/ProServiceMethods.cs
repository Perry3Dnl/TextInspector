namespace TextInspector;

/// <summary>Pro API facades on the fully functional Free service. Unsupported operations throw clearly.</summary>
public sealed partial class TextInspectorService
{
    public IReadOnlyList<TextInspectorDiagnostic> CompilationDiagnostics => _engine.CompilationDiagnostics;


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public TextInspectorService(ITextInspectorRepository repository) : this()
        => throw ProRequired();

    public TextSafetyInfo Analyze(string? text) => Inspect(text).RiskAssessment;

    public string MarkText(string? text) => Inspect(text, includeMarkedHtml: true).MarkedHtml ?? "";


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public Task ReloadAsync(CancellationToken ct = default)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public Task<TextInspectorScoringOptions> GetOptionsAsync(CancellationToken ct = default)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public Task<IReadOnlyList<TextInspectorPolicy>> GetAllPoliciesAsync(CancellationToken ct = default)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public Task SaveAsync(TextInspectorScoringOptions options, IReadOnlyList<TextInspectorRulePattern> rules,
        Guid userId, CancellationToken ct = default)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public Task SaveAsync(TextInspectorScoringOptions options, IReadOnlyList<TextInspectorRulePattern> rules,
        IReadOnlyList<TextInspectorPolicy> policies, Guid userId, CancellationToken ct = default)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public bool HasMatch(string? text, TextRuleMatchFilter? filter)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public TextRuleMatch? FindFirstMatch(string? text, TextRuleMatchFilter? filter = null)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public IReadOnlyList<TextRuleMatch> FindMatches(string? text, TextRuleMatchFilter? filter,
        int maxMatches = int.MaxValue)
        => throw ProRequired();


    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]    public IReadOnlyList<TextInspectionResult> InspectManyParallel(
        IEnumerable<string?> texts, TextInspectionOptions? options = null,
        int maxDegreeOfParallelism = -1)
        => throw ProRequired();

    private static NotSupportedException ProRequired()
        => new("Custom rule or policy configuration, persistence, targeted matching, and parallel inspection require TextInspector Pro.");
}
