namespace TextInspector;

/// <summary>
/// Scoring, matching-safety, and normalization knobs for a compiled inspector.
/// </summary>
internal class TextInspectorScoringOptions
{
    public TextInspectorScoringOptions() { }

    public int TextInspectorOptionsId { get; set; }
    /// <summary>
    /// Monotonic configuration generation. Transactional persistence adapters advance this value on commit.
    /// </summary>
    public int Version { get; set; } = 5;

    public int ScoreMin { get; init; } = 0;
    public int ScoreMax { get; init; } = 100;
    public int MinNonZeroScore { get; init; } = 1;
    public int SaturationK { get; init; } = 25;
    public int RawCapFor100 { get; init; } = 800;

    /// <summary>The maximum value used by rule SeverityScore. Presets use a 1..10 scale.</summary>
    public int SeverityScaleMax { get; init; } = 10;

    /// <summary>Severity at or above this value is considered high severity for combo scoring.</summary>
    public int HighSeverityThreshold { get; init; } = 8;

    /// <summary>Per-rule regex execution timeout. Protects callers from pathological patterns/input.</summary>
    public int RegexTimeoutMilliseconds { get; init; } = 250;

    /// <summary>Optional hard input limit. 0 means unlimited.</summary>
    public int MaxInputLength { get; init; } = 0;

    public bool NormalizeFullWidthCharacters { get; init; } = true;
    public bool RemoveZeroWidthCharacters { get; init; } = true;
    public bool NormalizeSmartPunctuation { get; init; } = true;
    public bool DeduplicateExactMatchesForScoring { get; init; } = true;

    /// <summary>
    /// Optional exact-match exclusions for known benign matches, scoped to a rule key.
    /// The default is empty; this does not implement broad substring filtering.
    /// </summary>
    public TextInspectorFalsePositiveExclusion[] FalsePositiveExclusions { get; set; } = Array.Empty<TextInspectorFalsePositiveExclusion>();

    /// <summary>
    /// Optional local masking/redaction output. Detection masking is independent from risk scoring:
    /// evidence-only matches can be masked without contributing risk points.
    /// </summary>
    public TextInspectorMaskingOptions Masking { get; set; } = new();

    public int EvidenceSnippetPad { get; init; } = 28;
    public int DefaultMaxEvidenceSnippets { get; init; } = 3;

    public int Combo_ChildPlusCSAM_Bonus { get; init; } = 50;
    public int Combo_ChildPlusNonConsensual_Bonus { get; init; } = 50;
    public int Combo_ChildPlusTrafficking_Bonus { get; init; } = 50;
    public int Combo_ChildPlusExploitativePorn_Bonus { get; init; } = 50;
    public int Combo_ChildPlusExtremeViolence_Bonus { get; init; } = 50;
    public int Combo_ChildPlusThreats_Bonus { get; init; } = 50;
    public int Combo_ExtremeViolencePlusThreats_Bonus { get; init; } = 15;
    public int Combo_SnuffPlusExtremeViolence_Bonus { get; init; } = 10;
    public int Combo_WeaponsPlusThreats_Bonus { get; init; } = 15;
    public int Combo_WeaponsPlusExtremeViolence_Bonus { get; init; } = 12;
    public int Combo_WeaponsPlusSnuff_Bonus { get; init; } = 15;
    public int Combo_NonConsensualPlusTrafficking_Bonus { get; init; } = 20;
    public int Combo_NonConsensualPlusExtremeViolence_Bonus { get; init; } = 15;
    public int Combo_NonConsensualPlusSnuff_Bonus { get; init; } = 20;
    public int Combo_DrugsPlusWeapons_Bonus { get; init; } = 12;
    public int Combo_FraudPlusDrugs_Bonus { get; init; } = 10;
    public int Combo_FraudPlusWeapons_Bonus { get; init; } = 10;
    public int Combo_TraffickingPlusDrugs_Bonus { get; init; } = 12;
    public int Combo_TraffickingPlusFraud_Bonus { get; init; } = 12;
    public int Combo_TraffickingPlusWeapons_Bonus { get; init; } = 15;
    public int Combo_HighSeverityPlusHeavyObf_Bonus { get; init; } = 15;
    public int Combo_ThreatsPlusObf_Bonus { get; init; } = 8;
    public int Combo_ChildPlusAnyObf_Bonus { get; init; } = 10;
    public int Combo_VeryHeavyObfAlone_Bonus { get; init; } = 10;
    public int Combo_AnyHighSeverity_TieBreakBonus { get; init; } = 10;
    public int Combo_TwoPlusHighCategories_Bonus { get; init; } = 12;
    public int Combo_ThreePlusHighCategories_Bonus { get; init; } = 18;
    public int Combo_FourPlusHighCategories_Bonus { get; init; } = 25;
    public int Combo_HighSeverityPlusHeavyObf_AtObfCount { get; init; } = 10;
    public int Combo_ThreatsPlusObf_AtObfCount { get; init; } = 6;
    public int Combo_ChildPlusAnyObf_AtObfCount { get; init; } = 3;
    public int Combo_VeryHeavyObfAlone_AtObfCount { get; init; } = 20;
    public int Combo_TwoPlusHighCategories_AtCount { get; init; } = 2;
    public int Combo_ThreePlusHighCategories_AtCount { get; init; } = 3;
    public int Combo_FourPlusHighCategories_AtCount { get; init; } = 4;

    public int ObfLinearCap { get; init; } = 60;
    public int ObfLinearMultiplier { get; init; } = 3;
    public int ObfStep1At { get; init; } = 8;
    public int ObfStep1Bonus { get; init; } = 10;
    public int ObfStep2At { get; init; } = 15;
    public int ObfStep2Bonus { get; init; } = 15;
    public int ObfStep3At { get; init; } = 25;
    public int ObfStep3Bonus { get; init; } = 20;

    /// <summary>
    /// When the number of scored obfuscation hits reaches this value, HighestDetectedSeverity is floored
    /// to HighSeverityThreshold. Set to 0 to disable.
    /// </summary>
    public int ObfSeverityFloorAt { get; init; } = 15;

    public void Validate()
    {
        if(Version <= 0)
            throw new InvalidOperationException("Version must be > 0.");
        if(ScoreMax <= ScoreMin)
            throw new InvalidOperationException("ScoreMax must be greater than ScoreMin.");
        if(MinNonZeroScore < ScoreMin || MinNonZeroScore > ScoreMax)
            throw new InvalidOperationException("MinNonZeroScore must be within the score range.");
        if(SaturationK <= 0)
            throw new InvalidOperationException("SaturationK must be > 0.");
        if(RawCapFor100 <= 0)
            throw new InvalidOperationException("RawCapFor100 must be > 0.");
        if(SeverityScaleMax <= 0)
            throw new InvalidOperationException("SeverityScaleMax must be > 0.");
        if(HighSeverityThreshold < 0 || HighSeverityThreshold > SeverityScaleMax)
            throw new InvalidOperationException("HighSeverityThreshold must be within the severity scale.");
        if(RegexTimeoutMilliseconds <= 0)
            throw new InvalidOperationException("RegexTimeoutMilliseconds must be > 0.");
        if(MaxInputLength < 0)
            throw new InvalidOperationException("MaxInputLength cannot be negative.");
        if(FalsePositiveExclusions is null || FalsePositiveExclusions.Any(item =>
            item is null || string.IsNullOrWhiteSpace(item.RuleKey) || string.IsNullOrWhiteSpace(item.MatchedText)))
            throw new InvalidOperationException("FalsePositiveExclusions must contain nonblank rule keys and matched text.");
        if(EvidenceSnippetPad < 0)
            throw new InvalidOperationException("EvidenceSnippetPad cannot be negative.");
        if(DefaultMaxEvidenceSnippets < 0)
            throw new InvalidOperationException("DefaultMaxEvidenceSnippets cannot be negative.");
        if(Masking is null)
            throw new InvalidOperationException("Masking cannot be null.");
        Masking.Validate();
        ValidateNonNegative(
            Combo_HighSeverityPlusHeavyObf_AtObfCount, Combo_ThreatsPlusObf_AtObfCount,
            Combo_ChildPlusAnyObf_AtObfCount, Combo_VeryHeavyObfAlone_AtObfCount,
            Combo_TwoPlusHighCategories_AtCount, Combo_ThreePlusHighCategories_AtCount,
            Combo_FourPlusHighCategories_AtCount, ObfStep1At, ObfStep2At, ObfStep3At, ObfSeverityFloorAt);

        ValidateNonNegative(
            Combo_ChildPlusCSAM_Bonus, Combo_ChildPlusNonConsensual_Bonus, Combo_ChildPlusTrafficking_Bonus,
            Combo_ChildPlusExploitativePorn_Bonus, Combo_ChildPlusExtremeViolence_Bonus, Combo_ChildPlusThreats_Bonus,
            Combo_ExtremeViolencePlusThreats_Bonus, Combo_SnuffPlusExtremeViolence_Bonus, Combo_WeaponsPlusThreats_Bonus,
            Combo_WeaponsPlusExtremeViolence_Bonus, Combo_WeaponsPlusSnuff_Bonus, Combo_NonConsensualPlusTrafficking_Bonus,
            Combo_NonConsensualPlusExtremeViolence_Bonus, Combo_NonConsensualPlusSnuff_Bonus, Combo_DrugsPlusWeapons_Bonus,
            Combo_FraudPlusDrugs_Bonus, Combo_FraudPlusWeapons_Bonus, Combo_TraffickingPlusDrugs_Bonus,
            Combo_TraffickingPlusFraud_Bonus, Combo_TraffickingPlusWeapons_Bonus, Combo_HighSeverityPlusHeavyObf_Bonus,
            Combo_ThreatsPlusObf_Bonus, Combo_ChildPlusAnyObf_Bonus, Combo_VeryHeavyObfAlone_Bonus,
            Combo_AnyHighSeverity_TieBreakBonus, Combo_TwoPlusHighCategories_Bonus, Combo_ThreePlusHighCategories_Bonus,
            Combo_FourPlusHighCategories_Bonus, ObfLinearCap, ObfLinearMultiplier, ObfStep1Bonus, ObfStep2Bonus, ObfStep3Bonus);
    }

    private static void ValidateNonNegative(params int[] values)
    {
        if(values.Any(static value => value < 0))
            throw new InvalidOperationException("Scoring values cannot be negative.");
    }
}
