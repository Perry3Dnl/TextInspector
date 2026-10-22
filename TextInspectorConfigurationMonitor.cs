namespace TextInspector;

/// <summary>API-compatible placeholder. Persistent custom configuration requires TextInspector Pro.</summary>
public sealed class TextInspectorConfigurationMonitor
{
    private static NotSupportedException Unavailable() => new("Persistence and advanced configuration require TextInspector Pro.");
    public TextInspectorConfigurationMonitor( ITextInspectorPersistedVersionRepository persistedVersionRepository, ITextInspectorCachedVersionRepository cachedVersionRepository, ITextInspectorService service) => throw Unavailable();

    public async Task RunAsync( TimeSpan pollInterval, Action<Exception>? onError = null, CancellationToken ct = default) => throw Unavailable();
    public async Task<bool> CheckForUpdatesAsync(CancellationToken ct = default) => throw Unavailable();
}
