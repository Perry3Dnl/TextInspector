namespace TextInspector;

/// <summary>Read-only, reusable service for the embedded TextInspector rules. No persistence or rule mutation.</summary>
public sealed class TextInspectorService : ITextInspectorService, ITextInspectorDetectionService, ITextInspectorReportService
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


}
