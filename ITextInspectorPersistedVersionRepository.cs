namespace TextInspector;

/// <summary>
/// Optional repository capability that reads the configuration generation from the authoritative store,
/// bypassing any local repository cache. Intended for background multi-instance invalidation checks.
/// </summary>
public interface ITextInspectorPersistedVersionRepository
{
    Task<int?> GetPersistedOptionsVersionAsync(CancellationToken ct = default);
}
