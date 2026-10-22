namespace TextInspector;

/// <summary>Pro edition convenience facade. Free exposes matching signatures for source compatibility.</summary>
public static class TextInspectorPro
{
    public static TextInspectorEngine Create(Action<TextInspectorBuilder>? configure = null)
        => throw new NotSupportedException("Update to TextInspector.Pro to use the Pro builder facade.");

    public static TextInspectorService CreateService(Action<TextInspectorBuilder>? configure = null)
        => throw new NotSupportedException("Update to TextInspector.Pro to create custom Pro services.");
}
