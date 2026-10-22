namespace TextInspector;

/// <summary>
/// Optional repository extension for stores that keep a local snapshot. ReloadAsync calls this
/// before rebuilding the compiled engine, allowing database-backed repositories to refresh without
/// adding storage I/O to the hot inspection path.
/// </summary>
public interface ITextInspectorRefreshableRepository
{
    Task RefreshAsync(CancellationToken ct = default);
}
