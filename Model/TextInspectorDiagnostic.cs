namespace TextInspector;

public enum TextInspectorDiagnosticKind
{
    RuleDisabled = 1,
    RuleSkipped = 2,
    InvalidRegex = 3,
    RegexTimeout = 4,
    InvalidContextRegex = 5,
    ContextReferenceNotFound = 6,
    PolicyDisabled = 7,
    PolicySkipped = 8,
    PolicyReferenceNotFound = 9,
    InvalidEvidenceGroup = 10
}

public sealed class TextInspectorDiagnostic
{
    public TextInspectorDiagnosticKind Kind { get; init; }
    public Guid? RuleId { get; init; }
    public string? RuleKey { get; init; }
    public Guid? PolicyId { get; init; }
    public string? PolicyKey { get; init; }
    public string Message { get; init; } = "";
}
