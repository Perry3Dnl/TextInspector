namespace TextInspector;

/// <summary>Shared inspection service contract. Pro additionally implements repository persistence.</summary>
public interface ITextInspectorService
{
    TextInspectionResult Inspect(string? text, bool includeMarkedHtml = false);

    /// <summary>Inspection diagnostics for the compiled rule set.</summary>
    IReadOnlyList<TextInspectorDiagnostic> CompilationDiagnostics { get; }

    /// <summary>Analyze the inspection risk without markup.</summary>
    TextSafetyInfo Analyze(string? text);

    /// <summary>Return the inspection's highlighted HTML.</summary>
    string MarkText(string? text);

    /// <summary>Pro-only repository reload in the Free edition.</summary>
    Task ReloadAsync(CancellationToken ct = default);

    /// <summary>Pro-only editable scoring configuration in the Free edition.</summary>
    Task<TextInspectorScoringOptions> GetOptionsAsync(CancellationToken ct = default);

    /// <summary>Pro-only editable rules in the Free edition.</summary>
    Task<IReadOnlyList<TextInspectorRulePattern>> GetAllRulesAsync(CancellationToken ct = default);

    /// <summary>Pro-only rule persistence in the Free edition.</summary>
    Task SaveAsync(TextInspectorScoringOptions options,
        IReadOnlyList<TextInspectorRulePattern> rules, Guid userId,
        CancellationToken ct = default);
}
