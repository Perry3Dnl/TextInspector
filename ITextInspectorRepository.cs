namespace TextInspector;

public interface ITextInspectorRepository
{
    Task<int?> GetOptionsVersionAsync(CancellationToken ct = default);
    Task<TextInspectorScoringOptions?> GetOptionsAsync(CancellationToken ct = default);
    Task SaveOptionsAsync(TextInspectorScoringOptions options,Guid userId,CancellationToken ct = default);
    Task<IReadOnlyList<TextInspectorRulePattern>> GetEnabledRulesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default);
    Task UpsertRuleAsync(TextInspectorRulePattern rule,Guid userId,CancellationToken ct = default);
}
