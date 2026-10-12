namespace TextInspector;

/// <summary>Describes how a UI-ready finding was correlated.</summary>
public enum TextInspectionFindingKind
{
    /// <summary>A composite policy correlated multiple underlying signals.</summary>
    Policy = 0,

    /// <summary>Scored standalone evidence was grouped by category because no scoring policy consumed it.</summary>
    Category = 1
}
