namespace TextInspector;

/// <summary>Pro-only persisted configuration monitor API.</summary>
public sealed class TextInspectorConfigurationMonitor
{
    public TextInspectorConfigurationMonitor(
        ITextInspectorPersistedVersionRepository persistedVersionRepository,
        ITextInspectorCachedVersionRepository cachedVersionRepository,
        ITextInspectorService service) { }

    public Task RunAsync(TimeSpan pollInterval, Action<Exception>? onError = null,
        CancellationToken ct = default) =>
        Task.FromException(new NotSupportedException("Update to TextInspector.Pro for persisted configuration monitoring."));

    public Task<bool> CheckForUpdatesAsync(CancellationToken ct = default) =>
        Task.FromException<bool>(new NotSupportedException("Update to TextInspector.Pro for persisted configuration monitoring."));
}
