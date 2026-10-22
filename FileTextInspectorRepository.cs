namespace TextInspector;

/// <summary>API-compatible placeholder. Persistent custom configuration requires TextInspector Pro.</summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class FileTextInspectorRepository : ITextInspectorRepository, ITextInspectorPolicyRepository, ITextInspectorRefreshableRepository, ITextInspectorCachedVersionRepository
{
    private static NotSupportedException Unavailable() => new("Persistence and advanced configuration require TextInspector Pro.");
    public FileTextInspectorRepository(string directory) => throw Unavailable();
    public int? CachedOptionsVersion => throw Unavailable();

    public Task<int?> GetOptionsVersionAsync(CancellationToken ct = default) => throw Unavailable();
    public Task<TextInspectorScoringOptions?> GetOptionsAsync(CancellationToken ct = default) => throw Unavailable();
    public Task SaveOptionsAsync(TextInspectorScoringOptions options,Guid userId,CancellationToken ct = default) => throw Unavailable();
    public Task<IReadOnlyList<TextInspectorRulePattern>> GetEnabledRulesAsync(CancellationToken ct = default) => throw Unavailable();
    public Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default) => throw Unavailable();
    public Task UpsertRuleAsync(TextInspectorRulePattern rule,Guid userId,CancellationToken ct = default) => throw Unavailable();
    public Task<IReadOnlyList<TextInspectorPolicy>> GetEnabledPoliciesAsync(CancellationToken ct = default) => throw Unavailable();
    public Task<IReadOnlyList<TextInspectorPolicy>> GetAllPoliciesAsync(CancellationToken ct = default) => throw Unavailable();
    public Task UpsertPolicyAsync(TextInspectorPolicy policy,Guid userId,CancellationToken ct = default) => throw Unavailable();
    public Task RefreshAsync(CancellationToken ct = default) => throw Unavailable();
}
