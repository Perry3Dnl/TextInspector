namespace TextInspector;

public class TextSafetyInfo
{
    private TextSafetyInfo() { }

    public TextSafetyInfo(
        int riskScore,
        int highestDetectedSeverity,
        int detectedCategoryCount,
        int totalEvidenceCount,
        int length,
        int totalRiskPoints = 0,
        int ruleRiskPoints = 0,
        int additionalRiskPoints = 0,
        int scoringEvidenceCount = 0,
        int highestScoredSeverity = 0,
        int scoringCategoryCount = 0,
        int effectiveSeverity = 0,
        int builtInBonusPoints = 0,
        int policyRiskPoints = 0,
        int triggeredPolicyCount = 0,
        int scoringPolicyCount = 0,
        int highestPolicySeverity = 0,
        int detectionScore = 0,
        int linkedRuleRiskPoints = 0,
        int triggeredLinkedRuleCount = 0)
    {
        RiskScore = riskScore;
        HighestDetectedSeverity = highestDetectedSeverity;
        DetectedCategoryCount = detectedCategoryCount;
        TotalEvidenceCount = totalEvidenceCount;
        Length = length;
        TotalRiskPoints = totalRiskPoints;
        RuleRiskPoints = ruleRiskPoints;
        AdditionalRiskPoints = additionalRiskPoints;
        ScoringEvidenceCount = scoringEvidenceCount;
        HighestScoredSeverity = highestScoredSeverity;
        ScoringCategoryCount = scoringCategoryCount;
        EffectiveSeverity = effectiveSeverity;
        BuiltInBonusPoints = builtInBonusPoints;
        PolicyRiskPoints = policyRiskPoints;
        TriggeredPolicyCount = triggeredPolicyCount;
        ScoringPolicyCount = scoringPolicyCount;
        HighestPolicySeverity = highestPolicySeverity;
        DetectionScore = detectionScore;
        LinkedRuleRiskPoints = linkedRuleRiskPoints;
        TriggeredLinkedRuleCount = triggeredLinkedRuleCount;
    }

    /// <summary>
    /// Strongest 0..100 detection value across observed evidence. 0 means no evidence was detected;
    /// every real evidence match contributes a value of at least 1 independently of risk scoring.
    /// </summary>
    public int DetectionScore { get; private set; }

    public bool HasDetections => DetectionScore > 0;

    /// <summary>Final configured priority score after saturation.</summary>
    public int RiskScore { get; private set; } = -1;

    /// <summary>Highest severity among all rule evidence, including context-suppressed evidence.</summary>
    public int HighestDetectedSeverity { get; private set; } = -1;

    /// <summary>Highest severity among rule evidence that actually contributed base score.</summary>
    public int HighestScoredSeverity { get; private set; }

    /// <summary>Highest severity among matched composite policies.</summary>
    public int HighestPolicySeverity { get; private set; }

    /// <summary>Scored severity after policy severity and configured severity floors are considered.</summary>
    public int EffectiveSeverity { get; private set; }

    /// <summary>Number of categories containing any rule evidence.</summary>
    public int DetectedCategoryCount { get; private set; } = -1;

    /// <summary>Number of categories containing at least one scored rule-evidence item.</summary>
    public int ScoringCategoryCount { get; private set; }

    public int TotalEvidenceCount { get; private set; } = -1;
    public int ScoringEvidenceCount { get; private set; } = -1;
    public int Length { get; private set; } = -1;

    public int TriggeredPolicyCount { get; private set; }
    public int ScoringPolicyCount { get; private set; }

    /// <summary>Total raw score before saturation.</summary>
    public int TotalRiskPoints { get; private set; }

    /// <summary>Raw score contributed directly by rule evidence.</summary>
    public int RuleRiskPoints { get; private set; }

    /// <summary>
    /// All post-rule raw points. Preserved for compatibility and equals BuiltInBonusPoints + PolicyRiskPoints + LinkedRuleRiskPoints.
    /// </summary>
    public int AdditionalRiskPoints { get; private set; }

    /// <summary>Raw score from the built-in deterministic combo/obfuscation bonuses.</summary>
    public int BuiltInBonusPoints { get; private set; }

    /// <summary>Raw score contributed by matched composite policies.</summary>
    public int LinkedRuleRiskPoints { get; private set; }
    public int TriggeredLinkedRuleCount { get; private set; }

    public int PolicyRiskPoints { get; private set; }

    public bool MeetsThreshold(int minimumPriorityScore) => RiskScore >= minimumPriorityScore;
}
