namespace TextInspector;

/// <summary>Optional service surface for callers that persist composite policies.</summary>
public interface ITextInspectorPolicyService
{
    Task<IReadOnlyList<TextInspectorPolicy>> GetAllPoliciesAsync(CancellationToken ct = default);

    Task SaveAsync(
        TextInspectorScoringOptions options,
        IReadOnlyList<TextInspectorRulePattern> rules,
        IReadOnlyList<TextInspectorPolicy> policies,
        Guid userId,
        CancellationToken ct = default);
}
