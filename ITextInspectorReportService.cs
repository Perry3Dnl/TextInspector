namespace TextInspector;

/// <summary>
/// UI-oriented reporting surface that correlates low-level rule evidence and policies into logical findings.
/// </summary>
public interface ITextInspectorReportService
{
    /// <summary>
    /// Produces a deterministic, UI-ready inspection report while retaining exact original-text evidence spans.
    /// </summary>
    TextInspectionReport CreateReport(string? text,TextInspectionReportOptions? options = null);
}
