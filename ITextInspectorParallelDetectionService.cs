namespace TextInspector;

/// <summary>Optional parallel batch surface; the published detection service contract remains unchanged.</summary>
public interface ITextInspectorParallelDetectionService : ITextInspectorDetectionService
{
    IReadOnlyList<TextInspectionResult> InspectManyParallel(
        IEnumerable<string?> texts, TextInspectionOptions? options = null,
        int maxDegreeOfParallelism = -1);
}
