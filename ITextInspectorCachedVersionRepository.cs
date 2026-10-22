namespace TextInspector;

/// <summary>
/// Optional repository optimization for stores that already maintain an in-memory configuration
/// snapshot. TextInspectorService uses this synchronous value on the inspection hot path instead of
/// allocating/waiting on GetOptionsVersionAsync(). It must never perform storage I/O.
/// </summary>
public interface ITextInspectorCachedVersionRepository
{
    int? CachedOptionsVersion { get; }
}
