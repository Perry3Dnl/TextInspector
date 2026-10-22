namespace TextInspector;

// Shared engine implementation. Keep identical in Free and Pro.
public sealed partial class TextInspectorEngine
{
    private static RuntimeLinkedRule[] CompileLinkedRules(
        IReadOnlyList<TextInspectorRulePattern> owners, IReadOnlyList<RuntimeRule> rules,
        List<TextInspectorDiagnostic> diagnostics)
    {
        var known = new HashSet<string>(rules.Select(rule => rule.Key), StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var output = new List<RuntimeLinkedRule>();
        foreach(var owner in owners)
        {
            if(owner.LinkedRules is null)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.InvalidLinkedRule, owner, "LinkedRules cannot be null."));
                continue;
            }
            foreach(var link in owner.LinkedRules)
            {
                if(link is not null && !link.IsEnabled) continue;
                string? error = null;
                if(link is null) error = "LinkedRules contains a null definition.";
                else if(string.IsNullOrWhiteSpace(link.Key)) error = "LinkedRule key is required.";
                else if(link.RuleKeys is null || link.RuleKeys.Any(string.IsNullOrWhiteSpace)) error = "LinkedRule requires nonblank rule keys.";
                else if(link.RuleKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count() != link.RuleKeys.Length)
                    error = "LinkedRule members must be distinct.";
                else if(link.MinimumMatchedRules < 2 || link.MinimumMatchedRules > link.RuleKeys.Length)
                    error = "MinimumMatchedRules must be between two and the member count.";
                else if(link.Multiplier < 1 || link.RiskPoints < 0 || link.WindowCharacters < 0)
                    error = "Multiplier must be at least one; points and window cannot be negative.";
                if(error is not null)
                {
                    diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.InvalidLinkedRule, owner, error));
                    continue;
                }
                var keys = link!.RuleKeys.Select(key => key.Trim()).ToArray();
                if(keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() != keys.Length)
                {
                    diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.InvalidLinkedRule, owner, "Duplicate link key or member key."));
                    continue;
                }
                var missing = keys.Where(key => !known.Contains(key)).ToArray();
                if(missing.Length > 0)
                {
                    diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.LinkedRuleReferenceNotFound, owner,
                        $"LinkedRule '{link.Key}' references missing/disabled rules: {string.Join(", ", missing)}."));
                    continue;
                }
                if(!seen.Add(link.Key.Trim()))
                {
                    diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.InvalidLinkedRule, owner, "Duplicate link key."));
                    continue;
                }
                output.Add(new RuntimeLinkedRule(link.Key.Trim(), link.Description, keys, link.MinimumMatchedRules,
                    link.Multiplier, link.RiskPoints, link.WindowCharacters, link.ScoredOnly));
            }
        }
        return output.OrderBy(link => link.Key, StringComparer.Ordinal).ToArray();
    }

    private List<RuntimeLinkedRuleMatch> ApplyLinkedRules(List<EvidenceHitWithCounts> evidence)
    {
        var matches = new List<RuntimeLinkedRuleMatch>();
        foreach(var link in _linkedRules)
        {
            var keys = new HashSet<string>(link.RuleKeys, StringComparer.OrdinalIgnoreCase);
            var eligible = Enumerable.Range(0, evidence.Count).Where(index =>
                keys.Contains(evidence[index].RuleKey) && (link.ScoredOnly
                    ? evidence[index].ContributesToRisk
                    : evidence[index].ScoringStatus is TextEvidenceScoringDisposition.Counted
                        or TextEvidenceScoringDisposition.EvidenceOnly or TextEvidenceScoringDisposition.RuleLimitExceeded))
                .ToArray();
            bool Enough(IEnumerable<int> indexes) => indexes.Select(index => evidence[index].RuleKey)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() >= link.MinimumMatchedRules;
            if(!Enough(eligible)) continue;
            var participants = new HashSet<int>();
            if(link.WindowCharacters == 0) participants.UnionWith(eligible);
            else
                foreach(int start in eligible.Select(index => evidence[index].Start).Distinct())
                {
                    long end = (long)start + link.WindowCharacters;
                    var window = eligible.Where(index => evidence[index].Start >= start
                        && (long)evidence[index].Start + evidence[index].Length <= end).ToArray();
                    if(Enough(window)) participants.UnionWith(window);
                }
            if(participants.Count == 0) continue;
            matches.Add(new RuntimeLinkedRuleMatch(link, participants.OrderBy(index => evidence[index].Start)
                .ThenBy(index => evidence[index].RuleKey, StringComparer.Ordinal).ThenBy(index => evidence[index].Length).ToArray()));
        }

        if(matches.Count == 0) return matches;

        // Evaluate against original evidence, then apply the largest multiplier once. No cascading activation.
        for(int index = 0; index < evidence.Count; index++)
        {
            var active = matches.Where(match => match.EvidenceIndexes.Contains(index)).ToArray();
            if(active.Length == 0) continue;
            var winner = active.OrderByDescending(match => match.Rule.Multiplier)
                .ThenBy(match => match.Rule.Key, StringComparer.Ordinal).First();
            var hit = evidence[index];
            int effectivePoints = hit.ContributesToRisk ? MultiplyCapped(hit.RuleWeight, winner.Rule.Multiplier, int.MaxValue) : 0;
            winner.EscalationRiskPoints = AddRaw(winner.EscalationRiskPoints,
                effectivePoints - (hit.ContributesToRisk ? hit.RuleWeight : 0));
            evidence[index] = hit with
            {
                RiskMultiplier = winner.Rule.Multiplier,
                LinkedEffectiveSeverity = Math.Max(hit.SeverityScore, MultiplyCapped(hit.SeverityScore, winner.Rule.Multiplier, RuntimeOptions.SeverityScaleMax)),
                LinkedRuleKeys = active.Select(match => match.Rule.Key).OrderBy(key => key, StringComparer.Ordinal).ToArray()
            };
        }
        return matches;
    }

    private static int MultiplyCapped(int value, int multiplier, int maximum)
        => (int)Math.Min(maximum, (long)value * multiplier);

    private IReadOnlyList<TextLinkedRuleMatch> BuildPublicLinkedRules(
        string text, IReadOnlyList<RuntimeLinkedRuleMatch> matches, IReadOnlyList<EvidenceHitWithCounts> evidence,
        TextInspectionOptions options)
        => matches.Select(match => new TextLinkedRuleMatch
        {
            LinkedRuleKey = match.Rule.Key,
            Description = match.Rule.Description,
            Multiplier = match.Rule.Multiplier,
            MinimumMatchedRules = match.Rule.MinimumMatchedRules,
            MatchedRuleKeys = match.EvidenceIndexes.Select(index => evidence[index].RuleKey)
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(key => key, StringComparer.Ordinal).ToArray(),
            FixedRiskPoints = match.Rule.RiskPoints,
            EscalationRiskPoints = match.EscalationRiskPoints,
            EffectiveSeverity = match.EvidenceIndexes.Max(index => evidence[index].EffectiveSeverity),
            Evidence = match.EvidenceIndexes.Select(index => evidence[index]).Select(hit => new TextPolicyEvidenceReference
            {
                RuleId = hit.RuleId, RuleKey = hit.RuleKey, Category = hit.Category,
                Start = hit.Start, Length = hit.Length, ContributesToRisk = hit.ContributesToRisk, ScoringStatus = hit.ScoringStatus,
                MatchedText = options.IncludeMatchedText ? text.Substring(hit.Start, hit.Length) : ""
            }).ToArray()
        }).ToArray();

    private static TextScoreContribution[] LinkedRuleContributions(IReadOnlyList<RuntimeLinkedRuleMatch> matches)
        => matches.Where(match => match.TotalRiskPoints > 0).Select(match => new TextScoreContribution
        {
            Kind = TextScoreContributionKind.LinkedRule, LinkedRuleKey = match.Rule.Key,
            Key = "linked." + match.Rule.Key, Description = match.Rule.Description ?? $"LinkedRule '{match.Rule.Key}' matched.",
            RiskPoints = match.TotalRiskPoints
        }).ToArray();

    private sealed record RuntimeLinkedRule(string Key, string? Description, string[] RuleKeys,
        int MinimumMatchedRules, int Multiplier, int RiskPoints, int WindowCharacters, bool ScoredOnly);

    private sealed class RuntimeLinkedRuleMatch
    {
        public RuntimeLinkedRuleMatch(RuntimeLinkedRule rule, int[] indexes) { Rule = rule; EvidenceIndexes = indexes; }
        public RuntimeLinkedRule Rule { get; }
        public int[] EvidenceIndexes { get; }
        public int EscalationRiskPoints { get; set; }
        public int TotalRiskPoints => AddRaw(Rule.RiskPoints, EscalationRiskPoints);
    }
}
