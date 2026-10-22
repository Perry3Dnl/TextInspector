namespace TextInspector;
public interface ITextInspectorService
{
    TextInspectionResult Inspect(string? text, bool includeMarkedHtml = false);
}
