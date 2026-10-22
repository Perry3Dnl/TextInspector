using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TextInspector;

// Engine component extracted without changing its existing algorithm or public signatures.
public sealed partial class TextInspectorEngine
{
    private SummaryResult Summarize(
        List<EvidenceHitWithCounts> evidence,
        List<RuntimeLinkedRuleMatch> linkedMatches,
        int originalLength)
    {
        if(evidence.Count == 0)
        {
            return new SummaryResult(
                new TextSafetyInfo(RuntimeOptions.ScoreMin,0,0,0,originalLength),
                Array.Empty<TextCategorySummary>(),
                Array.Empty<TextScoreContribution>());
        }

        var categories = new Dictionary<TextInspectorRuleCategory,CategoryAccumulator>();
        int maxSeverity = 0;
        int maxScoredSeverity = 0;
        int baseRawScore = 0;
        int scoredEvidenceCount = 0;

        foreach(var e in evidence)
        {
            if(!categories.TryGetValue(e.Category,out var category))
            {
                category = new CategoryAccumulator();
                categories[e.Category] = category;
            }

            category.TotalEvidenceCount++;
            category.HighestDetectedSeverity = Math.Max(category.HighestDetectedSeverity,e.SeverityScore);
            maxSeverity = Math.Max(maxSeverity,e.SeverityScore);

            if(e.ContributesToRisk)
            {
                category.ScoringEvidenceCount++;
                category.TotalRiskPoints = AddRaw(category.TotalRiskPoints,e.RuleWeight);
                category.HighestScoredSeverity = Math.Max(category.HighestScoredSeverity,e.SeverityScore);
                maxScoredSeverity = Math.Max(maxScoredSeverity,e.SeverityScore);
                baseRawScore = AddRaw(baseRawScore,e.RuleWeight);
                scoredEvidenceCount++;
            }
        }

        var builtInContributions = BuildScoreContributions(categories).ToList();
        int builtInBonusRawScore = builtInContributions.Aggregate(0,(sum,item) => AddRaw(sum,item.RiskPoints));

        int policyRawScore = 0;
        int maxPolicySeverity = 0;
        int bonusRawScore = builtInBonusRawScore;
        var linkedContributions = LinkedRuleContributions(linkedMatches);
        int linkedPoints = linkedContributions.Aggregate(0, (sum, item) => AddRaw(sum, item.RiskPoints));
        bonusRawScore = AddRaw(bonusRawScore, linkedPoints);
        int rawScore = AddRaw(baseRawScore,bonusRawScore);
        int effectiveSeverity = maxScoredSeverity;
        effectiveSeverity = Math.Max(effectiveSeverity, evidence.Where(hit => hit.ContributesToRisk).Select(hit => hit.EffectiveSeverity).DefaultIfEmpty(0).Max());
        int linkedSeverity = linkedMatches.Where(match => match.TotalRiskPoints > 0)
            .SelectMany(match => match.EvidenceIndexes).Select(index => evidence[index].EffectiveSeverity).DefaultIfEmpty(0).Max();
        effectiveSeverity = Math.Max(effectiveSeverity, linkedSeverity);
        int obfuscationCount = GetScoredCount(categories,TextInspectorRuleCategory.Obfuscation);
        if(RuntimeOptions.ObfSeverityFloorAt > 0 && obfuscationCount >= RuntimeOptions.ObfSeverityFloorAt)
            effectiveSeverity = Math.Max(effectiveSeverity,RuntimeOptions.HighSeverityThreshold);

        int priorityScore = SaturatingScore(rawScore);
        int scoredCategoryCount = categories.Count(static pair => pair.Value.ScoringEvidenceCount > 0);
        int scoredPolicyCount = 0;
        int detectionScore = evidence.Count == 0
            ? 0
            : evidence.Max(static item => item.DetectionScore);

        var categorySummaries = categories
            .OrderBy(pair => pair.Key)
            .Select(pair => new TextCategorySummary
            {
                Category = pair.Key,
                TotalEvidenceCount = pair.Value.TotalEvidenceCount,
                ScoringEvidenceCount = pair.Value.ScoringEvidenceCount,
                TotalRiskPoints = pair.Value.TotalRiskPoints,
                HighestDetectedSeverity = pair.Value.HighestDetectedSeverity,
                HighestScoredSeverity = pair.Value.HighestScoredSeverity,
                HighlightIntensity = NormalizeSeverity(pair.Value.HighestDetectedSeverity)
            })
            .ToArray();

        var contributions = builtInContributions.Concat(linkedContributions).ToArray();

        return new SummaryResult(
            new TextSafetyInfo(
                priorityScore,
                maxSeverity,
                categories.Count,
                evidence.Count,
                originalLength,
                rawScore,
                baseRawScore,
                bonusRawScore,
                scoredEvidenceCount,
                maxScoredSeverity,
                scoredCategoryCount,
                effectiveSeverity,
                builtInBonusRawScore,
                policyRawScore,
                0,
                scoredPolicyCount,
                maxPolicySeverity,
                detectionScore,
                linkedPoints,
                linkedMatches.Count),
            categorySummaries,
            contributions);
    }

    private IReadOnlyList<TextScoreContribution> BuildScoreContributions(
        IReadOnlyDictionary<TextInspectorRuleCategory,CategoryAccumulator> categories)
    {
        var output = new List<TextScoreContribution>();

        bool Has(TextInspectorRuleCategory category)
            => categories.TryGetValue(category,out var value) && value.ScoringEvidenceCount > 0;

        bool child = Has(TextInspectorRuleCategory.ChildMinor) || Has(TextInspectorRuleCategory.UnderageContext);
        bool csam = Has(TextInspectorRuleCategory.Csam);
        bool nonConsensual = Has(TextInspectorRuleCategory.NonConsensualSex);
        bool trafficking = Has(TextInspectorRuleCategory.Trafficking);
        bool exploitativePorn = Has(TextInspectorRuleCategory.ExploitativePorn);
        bool extremeViolence = Has(TextInspectorRuleCategory.ExtremeViolence);
        bool threats = Has(TextInspectorRuleCategory.Threats);
        bool snuff = Has(TextInspectorRuleCategory.Snuff);
        bool weapons = Has(TextInspectorRuleCategory.WeaponsSales);
        bool drugs = Has(TextInspectorRuleCategory.DrugSales);
        bool fraud = Has(TextInspectorRuleCategory.Fraud);

        int obfCount = GetScoredCount(categories,TextInspectorRuleCategory.Obfuscation);
        int maxScoredSeverity = categories
            .Where(static pair => pair.Value.ScoringEvidenceCount > 0)
            .Select(static pair => pair.Value.HighestScoredSeverity)
            .DefaultIfEmpty(0)
            .Max();
        int nonObfCategoryCount = categories.Count(pair => pair.Key != TextInspectorRuleCategory.Obfuscation && pair.Value.ScoringEvidenceCount > 0);
        int highCategoryCount = categories.Count(pair =>
            pair.Key != TextInspectorRuleCategory.Obfuscation
            && pair.Value.ScoringEvidenceCount > 0
            && pair.Value.HighestScoredSeverity >= RuntimeOptions.HighSeverityThreshold);
        bool anyHighSeverity = maxScoredSeverity >= RuntimeOptions.HighSeverityThreshold;

        Add(child && csam,"combo.child+csam","Child/minor context plus CSAM evidence.",RuntimeOptions.Combo_ChildPlusCSAM_Bonus);
        Add(child && nonConsensual,"combo.child+nonconsensual","Child/minor context plus non-consensual sexual content.",RuntimeOptions.Combo_ChildPlusNonConsensual_Bonus);
        Add(child && trafficking,"combo.child+trafficking","Child/minor context plus trafficking evidence.",RuntimeOptions.Combo_ChildPlusTrafficking_Bonus);
        Add(child && exploitativePorn,"combo.child+exploitative-porn","Child/minor context plus exploitative pornography evidence.",RuntimeOptions.Combo_ChildPlusExploitativePorn_Bonus);
        Add(child && extremeViolence,"combo.child+extreme-violence","Child/minor context plus extreme violence evidence.",RuntimeOptions.Combo_ChildPlusExtremeViolence_Bonus);
        Add(child && threats,"combo.child+threats","Child/minor context plus threats evidence.",RuntimeOptions.Combo_ChildPlusThreats_Bonus);
        Add(extremeViolence && threats,"combo.extreme-violence+threats","Extreme violence plus threats evidence.",RuntimeOptions.Combo_ExtremeViolencePlusThreats_Bonus);
        Add(snuff && extremeViolence,"combo.snuff+extreme-violence","Snuff plus extreme violence evidence.",RuntimeOptions.Combo_SnuffPlusExtremeViolence_Bonus);
        Add(weapons && threats,"combo.weapons+threats","Weapons sales plus threats evidence.",RuntimeOptions.Combo_WeaponsPlusThreats_Bonus);
        Add(weapons && extremeViolence,"combo.weapons+extreme-violence","Weapons sales plus extreme violence evidence.",RuntimeOptions.Combo_WeaponsPlusExtremeViolence_Bonus);
        Add(weapons && snuff,"combo.weapons+snuff","Weapons sales plus snuff evidence.",RuntimeOptions.Combo_WeaponsPlusSnuff_Bonus);
        Add(nonConsensual && trafficking,"combo.nonconsensual+trafficking","Non-consensual sexual content plus trafficking evidence.",RuntimeOptions.Combo_NonConsensualPlusTrafficking_Bonus);
        Add(nonConsensual && extremeViolence,"combo.nonconsensual+extreme-violence","Non-consensual sexual content plus extreme violence evidence.",RuntimeOptions.Combo_NonConsensualPlusExtremeViolence_Bonus);
        Add(nonConsensual && snuff,"combo.nonconsensual+snuff","Non-consensual sexual content plus snuff evidence.",RuntimeOptions.Combo_NonConsensualPlusSnuff_Bonus);
        Add(drugs && weapons,"combo.drugs+weapons","Drug sales plus weapons sales evidence.",RuntimeOptions.Combo_DrugsPlusWeapons_Bonus);
        Add(fraud && drugs,"combo.fraud+drugs","Fraud plus drug sales evidence.",RuntimeOptions.Combo_FraudPlusDrugs_Bonus);
        Add(fraud && weapons,"combo.fraud+weapons","Fraud plus weapons sales evidence.",RuntimeOptions.Combo_FraudPlusWeapons_Bonus);
        Add(trafficking && drugs,"combo.trafficking+drugs","Trafficking plus drug sales evidence.",RuntimeOptions.Combo_TraffickingPlusDrugs_Bonus);
        Add(trafficking && fraud,"combo.trafficking+fraud","Trafficking plus fraud evidence.",RuntimeOptions.Combo_TraffickingPlusFraud_Bonus);
        Add(trafficking && weapons,"combo.trafficking+weapons","Trafficking plus weapons sales evidence.",RuntimeOptions.Combo_TraffickingPlusWeapons_Bonus);

        if(obfCount > 0 && RuntimeOptions.ObfLinearMultiplier > 0 && RuntimeOptions.ObfLinearCap > 0)
        {
            int points = Math.Min(RuntimeOptions.ObfLinearCap,obfCount * RuntimeOptions.ObfLinearMultiplier);
            Add(points > 0,"obfuscation.linear",$"{obfCount} scored obfuscation hit(s).",points);
        }

        Add(obfCount > 0 && obfCount >= RuntimeOptions.ObfStep1At,"obfuscation.step1",$"Obfuscation count reached {RuntimeOptions.ObfStep1At}.",RuntimeOptions.ObfStep1Bonus);
        Add(obfCount > 0 && obfCount >= RuntimeOptions.ObfStep2At,"obfuscation.step2",$"Obfuscation count reached {RuntimeOptions.ObfStep2At}.",RuntimeOptions.ObfStep2Bonus);
        Add(obfCount > 0 && obfCount >= RuntimeOptions.ObfStep3At,"obfuscation.step3",$"Obfuscation count reached {RuntimeOptions.ObfStep3At}.",RuntimeOptions.ObfStep3Bonus);

        Add(anyHighSeverity && obfCount > 0 && obfCount >= RuntimeOptions.Combo_HighSeverityPlusHeavyObf_AtObfCount,
            "combo.high-severity+heavy-obfuscation","High-severity evidence plus heavy obfuscation.",RuntimeOptions.Combo_HighSeverityPlusHeavyObf_Bonus);
        Add(threats && obfCount > 0 && obfCount >= RuntimeOptions.Combo_ThreatsPlusObf_AtObfCount,
            "combo.threats+obfuscation","Threat evidence plus obfuscation.",RuntimeOptions.Combo_ThreatsPlusObf_Bonus);
        Add(child && obfCount > 0 && obfCount >= RuntimeOptions.Combo_ChildPlusAnyObf_AtObfCount,
            "combo.child+obfuscation","Child/minor context plus obfuscation.",RuntimeOptions.Combo_ChildPlusAnyObf_Bonus);
        Add(nonObfCategoryCount == 0 && obfCount > 0 && obfCount >= RuntimeOptions.Combo_VeryHeavyObfAlone_AtObfCount,
            "combo.obfuscation-only","Very heavy obfuscation without another scored category.",RuntimeOptions.Combo_VeryHeavyObfAlone_Bonus);
        Add(anyHighSeverity,"combo.high-severity","At least one high-severity rule matched.",RuntimeOptions.Combo_AnyHighSeverity_TieBreakBonus);

        // High-category bonuses are tiers: only the highest reached tier applies.
        if(highCategoryCount >= RuntimeOptions.Combo_FourPlusHighCategories_AtCount)
            Add(true,"combo.high-categories.4+",$"{highCategoryCount} high-severity categories matched.",RuntimeOptions.Combo_FourPlusHighCategories_Bonus);
        else if(highCategoryCount >= RuntimeOptions.Combo_ThreePlusHighCategories_AtCount)
            Add(true,"combo.high-categories.3+",$"{highCategoryCount} high-severity categories matched.",RuntimeOptions.Combo_ThreePlusHighCategories_Bonus);
        else if(highCategoryCount >= RuntimeOptions.Combo_TwoPlusHighCategories_AtCount)
            Add(true,"combo.high-categories.2+",$"{highCategoryCount} high-severity categories matched.",RuntimeOptions.Combo_TwoPlusHighCategories_Bonus);

        return output;

        void Add(bool condition,string key,string description,int rawPoints)
        {
            if(!condition || rawPoints <= 0)
                return;

            output.Add(new TextScoreContribution
            {
                Key = key,
                Description = description,
                RiskPoints = rawPoints
            });
        }
    }

    private static int GetScoredCount(
        IReadOnlyDictionary<TextInspectorRuleCategory,CategoryAccumulator> categories,
        TextInspectorRuleCategory category)
        => categories.TryGetValue(category,out var value) ? value.ScoringEvidenceCount : 0;

}
