namespace TextInspector;

/// <summary>
/// Optional extended inspection and configuration API. The published ITextInspectorService
/// stays unchanged so applications implementing its original Inspect method continue to work.
/// Both Free and Pro expose this interface; repository mutation requires Pro.
/// </summary>
public interface ITextInspectorExtendedService : ITextInspectorService
{
    IReadOnlyList<TextInspectorDiagnostic> CompilationDiagnostics { get; }
    TextSafetyInfo Analyze(string? text);
    string MarkText(string? text);
    Task ReloadAsync(CancellationToken ct = default);
    Task<TextInspectorScoringOptions> GetOptionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default);
    Task SaveAsync(TextInspectorScoringOptions options,
        IReadOnlyList<TextInspectorRulePattern> rules, Guid userId, CancellationToken ct = default);
}
