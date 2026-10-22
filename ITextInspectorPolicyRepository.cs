namespace TextInspector;

/// <summary>
/// Optional repository extension for composite policies. Existing ITextInspectorRepository
/// implementations remain valid and simply operate without persisted policies.
/// </summary>
public interface ITextInspectorPolicyRepository
{
    Task<IReadOnlyList<TextInspectorPolicy>> GetEnabledPoliciesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TextInspectorPolicy>> GetAllPoliciesAsync(CancellationToken ct = default);
    Task UpsertPolicyAsync(TextInspectorPolicy policy,Guid userId,CancellationToken ct = default);
}
