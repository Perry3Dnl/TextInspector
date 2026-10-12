namespace TextInspector;

/// <summary>Read-only, reusable service for the embedded Lite rules. No persistence or rule mutation.</summary>
public sealed class TextInspectorService : ITextInspectorService, ITextInspectorDetectionService, ITextInspectorReportService
{
    private readonly TextInspectorEngine _engine;
    public TextInspectorService(TextInspectorLiteOptions? options = null) => _engine = TextInspectorEngine.Create(options);
    public TextInspectionResult Inspect(string? text, bool includeMarkedHtml = false) => _engine.Inspect(text, includeMarkedHtml);
    public TextInspectionResult Inspect(string? text, TextInspectionOptions? options) => _engine.Inspect(text, options);
    public bool HasMatch(string? text) => _engine.HasMatch(text);
    public IReadOnlyList<TextRuleMatch> FindMatches(string? text, int maxMatches = int.MaxValue) => _engine.FindMatches(text, maxMatches);
    public IReadOnlyList<TextInspectionResult> InspectMany(IEnumerable<string?> texts, TextInspectionOptions? options = null) => _engine.InspectMany(texts, options);
    public TextInspectionReport CreateReport(string? text, TextInspectionReportOptions? options = null) => _engine.CreateReport(text, options);
}
