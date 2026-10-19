namespace TextInspector;

internal static partial class TextInspectionReportBuilder
{
    private static IEnumerable<TextInspectionFinding> CreateLinkedFindings(TextInspectionResult result,
        TextInspectorScoringOptions scoringOptions, TextInspectionReportOptions reportOptions)
    {
        foreach(var link in result.LinkedRuleMatches)
        {
            var evidence = result.Evidence.Where(hit => link.Evidence.Any(reference =>
                reference.RuleId == hit.RuleId && reference.Start == hit.Start && reference.Length == hit.Length)).ToArray();
            var reasons = new List<TextInspectionReportReason>
            {
                new TextInspectionReportReason
                {
                    Kind = TextInspectionReasonKind.LinkedRule, LinkedRuleKey = link.LinkedRuleKey,
                    Code = "linked:" + link.LinkedRuleKey, ContributesToRisk = link.ContributesToRisk,
                    Message = link.Description ?? $"{link.MatchedRuleKeys.Count} distinct linked rules matched; multiplier {link.Multiplier}."
                }
            };
            AddEvidenceReasons(reasons, evidence);
            var bounds = EvidenceBounds(evidence);
            yield return new TextInspectionFinding
            {
                FindingId = "linked:" + link.LinkedRuleKey, FindingCode = "linked." + Slugify(link.LinkedRuleKey),
                FindingType = TextInspectionFindingKind.LinkedRule, LinkedRuleKey = link.LinkedRuleKey,
                Title = Humanize(link.LinkedRuleKey), Description = link.Description,
                RiskLevel = link.ContributesToRisk ? RiskFromSeverity(link.EffectiveSeverity, scoringOptions.SeverityScaleMax) : TextInspectionRiskLevel.Informational,
                SeverityScore = link.EffectiveSeverity, TotalRiskPoints = link.RiskPoints, ContributesToRisk = link.ContributesToRisk,
                Start = bounds.Start, Length = bounds.Length, RuleKeys = link.MatchedRuleKeys, Evidence = evidence,
                Reasons = LimitReasons(reasons, reportOptions.MaxReasonsPerFinding)
            };
        }
    }
}
