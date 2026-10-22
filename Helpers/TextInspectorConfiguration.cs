namespace TextInspector;

/// <summary>Pro-compatible configuration snapshot. Custom configuration requires TextInspector.Pro.</summary>
public sealed class TextInspectorConfiguration
{
    private TextInspectorConfiguration() { }

    private static NotSupportedException Upgrade() => new("Update to TextInspector.Pro for custom configuration.");
    public TextInspectorScoringOptions Options => throw Upgrade();
    public IReadOnlyList<TextInspectorRulePattern> Rules => throw Upgrade();
    public IReadOnlyList<TextInspectorPolicy> Policies => throw Upgrade();
    public TextInspectorEngine Compile() => throw Upgrade();
    public InMemoryTextInspectorRepository CreateRepository() => throw Upgrade();
    public TextInspectorService CreateService() => throw Upgrade();
}
