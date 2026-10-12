namespace TextInspector;

public enum TextEvidenceScoringDisposition
{
    Counted = 1,
    RuleLimitExceeded = 2,
    ExactDuplicateSuppressed = 3,
    EvidenceOnly = 4,
    MissingRequiredContext = 5,
    ExcludedByContext = 6,
    ContextEvaluationFailed = 7
}
