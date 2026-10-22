namespace TextInspector;

/// <summary>Pro-only in-memory editable configuration repository. Available for API compatibility.</summary>
public sealed class InMemoryTextInspectorRepository :
    ITextInspectorRepository, ITextInspectorPolicyRepository,
    ITextInspectorAtomicConfigurationRepository, ITextInspectorCachedVersionRepository
{
    private static NotSupportedException Upgrade() => new("Update to TextInspector.Pro to use editable configuration repositories.");
    public InMemoryTextInspectorRepository() { }
    public InMemoryTextInspectorRepository(TextInspectorScoringOptions options,
        IEnumerable<TextInspectorRulePattern> rules,
        IEnumerable<TextInspectorPolicy>? policies = null) { }

    public int? CachedOptionsVersion => throw Upgrade();
    public Task<int?> GetOptionsVersionAsync(CancellationToken ct = default) => Task.FromException<int?>(Upgrade());
    public Task<TextInspectorScoringOptions?> GetOptionsAsync(CancellationToken ct = default)
        => Task.FromException<TextInspectorScoringOptions?>(Upgrade());
    public Task<IReadOnlyList<TextInspectorRulePattern>> GetEnabledRulesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorRulePattern>>(Upgrade());
    public Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorRulePattern>>(Upgrade());
    public Task<IReadOnlyList<TextInspectorPolicy>> GetEnabledPoliciesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorPolicy>>(Upgrade());
    public Task<IReadOnlyList<TextInspectorPolicy>> GetAllPoliciesAsync(CancellationToken ct = default)
        => Task.FromException<IReadOnlyList<TextInspectorPolicy>>(Upgrade());
    public Task SaveOptionsAsync(TextInspectorScoringOptions options, Guid userId, CancellationToken ct = default)
        => Task.FromException(Upgrade());
    public Task UpsertRuleAsync(TextInspectorRulePattern rule, Guid userId, CancellationToken ct = default)
        => Task.FromException(Upgrade());
    public Task UpsertPolicyAsync(TextInspectorPolicy policy, Guid userId, CancellationToken ct = default)
        => Task.FromException(Upgrade());
    public Task<TextInspectorScoringOptions> SaveConfigurationAsync(
        TextInspectorScoringOptions options, IReadOnlyList<TextInspectorRulePattern> rules,
        IReadOnlyList<TextInspectorPolicy>? policies, Guid userId, CancellationToken ct = default)
        => Task.FromException<TextInspectorScoringOptions>(Upgrade());
}
