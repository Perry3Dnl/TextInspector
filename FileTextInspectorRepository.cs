namespace TextInspector;

/// <summary>Pro-only file-backed configuration store, declared for upgrade/downgrade API compatibility.</summary>
public sealed class FileTextInspectorRepository :
    ITextInspectorRepository, ITextInspectorPolicyRepository,
    ITextInspectorRefreshableRepository, ITextInspectorCachedVersionRepository
{
    private static NotSupportedException Upgrade() => new("Update to TextInspector.Pro to use file-backed configuration repositories.");
    public FileTextInspectorRepository(string directory) => throw Upgrade();
    public int? CachedOptionsVersion => throw Upgrade();
    public Task<int?> GetOptionsVersionAsync(CancellationToken ct = default) => Task.FromException<int?>(Upgrade());
    public Task<TextInspectorScoringOptions?> GetOptionsAsync(CancellationToken ct = default)
        => Task.FromException<TextInspectorScoringOptions?>(Upgrade());
    public Task SaveOptionsAsync(TextInspectorScoringOptions options, Guid userId, CancellationToken ct = default)
        => Task.FromException(Upgrade());
    public Task<IReadOnlyList<TextInspectorRulePattern>> GetEnabledRulesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorRulePattern>>(Upgrade());
    public Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorRulePattern>>(Upgrade());
    public Task UpsertRuleAsync(TextInspectorRulePattern rule, Guid userId, CancellationToken ct = default)
        => Task.FromException(Upgrade());
    public Task<IReadOnlyList<TextInspectorPolicy>> GetEnabledPoliciesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorPolicy>>(Upgrade());
    public Task<IReadOnlyList<TextInspectorPolicy>> GetAllPoliciesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorPolicy>>(Upgrade());
    public Task UpsertPolicyAsync(TextInspectorPolicy policy, Guid userId, CancellationToken ct = default)
        => Task.FromException(Upgrade());
    public Task RefreshAsync(CancellationToken ct = default) => Task.FromException(Upgrade());
}
