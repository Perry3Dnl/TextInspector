namespace TextInspector;

/// <summary>Portable shape of Pro configuration snapshots. Custom configuration requires Pro.</summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class TextInspectorConfiguration
{
    private TextInspectorConfiguration() { }

    public TextInspectorScoringOptions Options =>
        throw new NotSupportedException("Custom configuration requires TextInspector Pro.");
    public IReadOnlyList<TextInspectorRulePattern> Rules =>
        throw new NotSupportedException("Custom configuration requires TextInspector Pro.");
    public IReadOnlyList<TextInspectorPolicy> Policies =>
        throw new NotSupportedException("Custom configuration requires TextInspector Pro.");

    public TextInspectorEngine Compile() =>
        throw new NotSupportedException("Custom configuration requires TextInspector Pro.");
    public InMemoryTextInspectorRepository CreateRepository() =>
        throw new NotSupportedException("Persistent configuration requires TextInspector Pro.");
    public TextInspectorService CreateService() =>
        throw new NotSupportedException("Custom configuration requires TextInspector Pro.");
}
