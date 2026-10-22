namespace TextInspector;

/// <summary>
/// Optional repository capability for atomically persisting one configuration generation.
/// Implementations must commit options/version and all supplied rule/policy writes as one transaction.
/// </summary>
public interface ITextInspectorAtomicConfigurationRepository
{
    /// <summary>
    /// Atomically persists options plus the supplied rules. When <paramref name="policies"/> is null,
    /// persisted policies are left unchanged. The returned options contain the committed generation version.
    /// </summary>
    Task<TextInspectorScoringOptions> SaveConfigurationAsync(
        TextInspectorScoringOptions options,
        IReadOnlyList<TextInspectorRulePattern> rules,
        IReadOnlyList<TextInspectorPolicy>? policies,
        Guid userId,
        CancellationToken ct = default);
}
