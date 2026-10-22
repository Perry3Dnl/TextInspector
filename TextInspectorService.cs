namespace TextInspector;

/// <summary>Read-only, reusable service for the embedded TextInspector rules. No persistence or rule mutation.</summary>
public sealed class TextInspectorService : ITextInspectorService, ITextInspectorExtendedService, ITextInspectorParallelDetectionService, ITextInspectorDetectionService, ITextInspectorMatchService, ITextInspectorPolicyService, ITextInspectorReportService
{
    private readonly TextInspectorEngine _engine;
    public TextInspectorService(TextInspectorOptions? options = null) => _engine = TextInspectorEngine.Create(options);
    public TextInspectionResult Inspect(string? text, bool includeMarkedHtml = false) => _engine.Inspect(text, includeMarkedHtml);
    public TextInspectionResult Inspect(string? text, TextInspectionOptions? options) => _engine.Inspect(text, options);
    public bool HasMatch(string? text) => _engine.HasMatch(text);
    public IReadOnlyList<TextRuleMatch> FindMatches(string? text, int maxMatches = int.MaxValue) => _engine.FindMatches(text, maxMatches);
    public IReadOnlyList<TextInspectionResult> InspectMany(IEnumerable<string?> texts, TextInspectionOptions? options = null) => _engine.InspectMany(texts, options);
    public TextInspectionReport CreateReport(string? text, TextInspectionReportOptions? options = null) => _engine.CreateReport(text, options);
    /// <summary>Returns the same deterministic risk summary as Pro for the Free preset.</summary>
    public TextSafetyInfo Analyze(string? text) => Inspect(text).RiskAssessment;

    /// <summary>Returns marked HTML using the same inspection output contract as Pro.</summary>
    public string MarkText(string? text) => Inspect(text,includeMarkedHtml: true).MarkedHtml ?? "";

    /// <summary>
    /// Free uses an immutable embedded preset. Runtime repository refresh requires TextInspector.Pro.
    /// </summary>
    /// <exception cref="NotSupportedException">Repository refresh is a Pro-only feature.</exception>
    public Task ReloadAsync(CancellationToken ct = default)
        => Task.FromException(new NotSupportedException(
            "Update to TextInspector.Pro to use repository reload."));

    /// <summary>
    /// Pro-only parallel batch inspection. Use InspectMany in Free or update to TextInspector.Pro.
    /// </summary>
    /// <exception cref="NotSupportedException">Parallel batch processing requires Pro.</exception>
    public IReadOnlyList<TextInspectionResult> InspectManyParallel(
        IEnumerable<string?> texts, TextInspectionOptions? options = null,
        int maxDegreeOfParallelism = -1)
        => throw new NotSupportedException("Update to TextInspector.Pro to use parallel batch inspection.");


    /// <summary>Pro-only targeted raw matching. Upgrade to TextInspector.Pro to use filters.</summary>
    public bool HasMatch(string? text, TextRuleMatchFilter? filter)
        => throw new NotSupportedException("Update to TextInspector.Pro to use filtered matching.");

    /// <summary>Pro-only targeted first-match lookup.</summary>
    public TextRuleMatch? FindFirstMatch(string? text, TextRuleMatchFilter? filter = null)
        => throw new NotSupportedException("Update to TextInspector.Pro to use filtered matching.");

    /// <summary>Pro-only targeted raw matching with filters.</summary>
    public IReadOnlyList<TextRuleMatch> FindMatches(string? text, TextRuleMatchFilter? filter, int maxMatches = int.MaxValue)
        => throw new NotSupportedException("Update to TextInspector.Pro to use filtered matching.");


    /// <summary>Diagnostics from the immutable built-in Free engine.</summary>
    public IReadOnlyList<TextInspectorDiagnostic> CompilationDiagnostics => _engine.CompilationDiagnostics;

    /// <summary>Repository-backed option retrieval requires TextInspector.Pro.</summary>
    public Task<TextInspectorScoringOptions> GetOptionsAsync(CancellationToken ct = default)
        => Task.FromException<TextInspectorScoringOptions>(
            new NotSupportedException("Update to TextInspector.Pro to use configuration repositories."));

    /// <summary>Editable rule retrieval requires TextInspector.Pro.</summary>
    public Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorRulePattern>>(
            new NotSupportedException("Update to TextInspector.Pro to use editable rules."));

    /// <summary>Composite policy retrieval requires TextInspector.Pro.</summary>
    public Task<IReadOnlyList<TextInspectorPolicy>> GetAllPoliciesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorPolicy>>(
            new NotSupportedException("Update to TextInspector.Pro to use composite policies."));

    /// <summary>Persistence requires TextInspector.Pro; Free never mutates embedded rules.</summary>
    public Task SaveAsync(TextInspectorScoringOptions options,
        IReadOnlyList<TextInspectorRulePattern> rules, Guid userId, CancellationToken ct = default)
        => Task.FromException(new NotSupportedException(
            "Update to TextInspector.Pro to save rule configurations."));

    /// <summary>Persistence requires TextInspector.Pro; Free never mutates embedded policies.</summary>
    public Task SaveAsync(TextInspectorScoringOptions options,
        IReadOnlyList<TextInspectorRulePattern> rules,
        IReadOnlyList<TextInspectorPolicy> policies, Guid userId, CancellationToken ct = default)
        => Task.FromException(new NotSupportedException(
            "Update to TextInspector.Pro to save composite policies."));

    /// <summary>Pro-compatible repository constructor. Repository-backed inspection is Pro-only.</summary>
    public TextInspectorService(ITextInspectorRepository repository)
        : this(new TextInspectorOptions())
        => throw new NotSupportedException("Update to TextInspector.Pro to use repositories.");

}
