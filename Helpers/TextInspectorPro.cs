namespace TextInspector;

/// <summary>Setup helpers for the extended TextInspector Pro edition.</summary>
public static class TextInspectorPro
{
    /// <summary>Creates a reusable inspector with the full preset and optional application configuration.</summary>
    public static TextInspectorEngine Create(Action<TextInspectorBuilder>? configure = null)
    => throw new NotSupportedException("This operation requires TextInspector Pro.");

    /// <summary>Creates a configurable in-memory service with the full preset and optional custom rules/policies.</summary>
    public static TextInspectorService CreateService(Action<TextInspectorBuilder>? configure = null)
    => throw new NotSupportedException("This operation requires TextInspector Pro.");
}
