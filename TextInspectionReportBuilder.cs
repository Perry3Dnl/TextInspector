namespace TextInspector;

internal static class TextInspectionReportBuilder
{
    public static TextInspectionReport Build(
        string originalText,
        TextInspectionResult result,
        TextInspectorScoringOptions scoringOptions,
        TextInspectionReportOptions reportOptions)
    {
        if(originalText is null) throw new ArgumentNullException(nameof(originalText));
        if(result is null) throw new ArgumentNullException(nameof(result));
        if(scoringOptions is null) throw new ArgumentNullException(nameof(scoringOptions));
        if(reportOptions is null) throw new ArgumentNullException(nameof(reportOptions));
        reportOptions.Validate();

        var findings = new List<TextInspectionFinding>();
        foreach(var category in result.CategorySummaries
            .Where(static category => category.ScoringEvidenceCount > 0)
            .OrderByDescending(static category => category.HighestScoredSeverity)
            .ThenByDescending(static category => category.TotalRiskPoints)
            .ThenBy(static category => category.Category))
        {
            var evidence = result.Evidence
                .Where(item => item.Category == category.Category
                    && item.ContributesToRisk)
                .OrderBy(static item => item.Start)
                .ThenBy(static item => item.Length)
                .ThenBy(static item => item.RuleKey,StringComparer.Ordinal)
                .ToArray();

            if(evidence.Length == 0)
                continue;

            findings.Add(CreateCategoryFinding(category,evidence,scoringOptions,reportOptions));
        }

        findings = findings
            .OrderByDescending(static finding => finding.ContributesToRisk)
            .ThenByDescending(static finding => finding.RiskLevel)
            .ThenByDescending(static finding => finding.SeverityScore)
            .ThenBy(static finding => finding.Start ?? int.MaxValue)
            .ThenBy(static finding => finding.FindingCode,StringComparer.Ordinal)
            .ToList();

        bool hasScoredFindings = findings.Any(static finding => finding.ContributesToRisk);
        var allFindings = findings.ToArray();
        if(reportOptions.MaxFindings.HasValue && findings.Count > reportOptions.MaxFindings.Value)
            findings = findings.Take(reportOptions.MaxFindings.Value).ToList();

        var supporting = reportOptions.IncludeSupportingEvidence
            ? result.Evidence
                .Where(static evidence => !evidence.ContributesToRisk)
                .OrderBy(static evidence => evidence.Start)
                .ThenBy(static evidence => evidence.Length)
                .ThenBy(static evidence => evidence.RuleKey,StringComparer.Ordinal)
                .ToArray()
            : Array.Empty<TextEvidence>();

        var overallRisk = Max(
            RiskFromScore(result.RiskAssessment.RiskScore),
            RiskFromSeverity(
                result.RiskAssessment.EffectiveSeverity,
                scoringOptions.SeverityScaleMax));

        var highlights = reportOptions.IncludeHighlights
            ? BuildHighlights(originalText,result.HighlightSpans,findings)
            : Array.Empty<TextInspectionReportHighlightSpan>();

        bool isFlagged = hasScoredFindings
            && (int)overallRisk >= (int)reportOptions.FlaggedMinimumRiskLevel;

        return new TextInspectionReport
        {
            SchemaVersion = 1,
            ConfigurationVersion = scoringOptions.Version,
            InputLength = originalText.Length,
            RiskLevel = overallRisk,
            RequiresReview = isFlagged,
            Summary = BuildSummary(overallRisk,allFindings,supporting.Length),
            MaskedText = result.MaskedText,
            RiskAssessment = result.RiskAssessment,
            Findings = findings,
            SupportingEvidence = supporting,
            Highlights = highlights,
            CategorySummaries = result.CategorySummaries,
            RiskContributions = result.RiskContributions,
            UrlObservations = result.UrlObservations,
            Diagnostics = result.Diagnostics
        };
    }

    private static TextInspectionFinding CreateCategoryFinding(
        TextCategorySummary category,
        IReadOnlyList<TextEvidence> evidence,
        TextInspectorScoringOptions scoringOptions,
        TextInspectionReportOptions reportOptions)
    {
        string categoryName = Humanize(category.Category.ToString());
        var reasons = new List<TextInspectionReportReason>();
        AddEvidenceReasons(reasons,evidence);
        reasons = LimitReasons(reasons,reportOptions.MaxReasonsPerFinding);
        var bounds = EvidenceBounds(evidence);

        return new TextInspectionFinding
        {
            FindingId = "category:" + category.Category,
            FindingCode = "category." + Slugify(category.Category.ToString()),
            FindingType = TextInspectionFindingKind.Category,
            Title = categoryName + " detected",
            Description = "Scored evidence was detected in the " + categoryName + " category.",
            Category = category.Category,
            RiskLevel = RiskFromSeverity(category.HighestScoredSeverity,scoringOptions.SeverityScaleMax),
            SeverityScore = category.HighestScoredSeverity,
            TotalRiskPoints = category.TotalRiskPoints,
            ContributesToRisk = true,
            Start = bounds.Start,
            Length = bounds.Length,
            Tags = Array.Empty<string>(),
            RuleKeys = evidence.Select(static item => item.RuleKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static key => key,StringComparer.Ordinal)
                .ToArray(),
            Evidence = evidence,
            Reasons = reasons
        };
    }

    private static void AddEvidenceReasons(
        List<TextInspectionReportReason> reasons,
        IReadOnlyList<TextEvidence> evidence)
    {
        var seenRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var item in evidence
            .OrderByDescending(static item => item.ContributesToRisk)
            .ThenByDescending(static item => item.SeverityScore)
            .ThenBy(static item => item.Start))
        {
            if(!seenRules.Add(item.RuleKey))
                continue;

            string baseMessage = !string.IsNullOrWhiteSpace(item.RuleDescription)
                ? item.RuleDescription!
                : Humanize(item.RuleKey) + " detected.";

            reasons.Add(new TextInspectionReportReason
            {
                Kind = TextInspectionReasonKind.Rule,
                Code = "rule:" + item.RuleKey,
                Message = item.ContributesToRisk
                    ? baseMessage
                    : "Supporting signal: " + baseMessage,
                ContributesToRisk = item.ContributesToRisk,
                RuleKey = item.RuleKey,
                Start = item.Start,
                Length = item.Length
            });
        }
    }

    private static List<TextInspectionReportReason> LimitReasons(
        List<TextInspectionReportReason> reasons,
        int? maximum)
    {
        if(!maximum.HasValue || reasons.Count <= maximum.Value)
            return reasons;
        if(maximum.Value == 0)
            return new List<TextInspectionReportReason>(0);
        return reasons.Take(maximum.Value).ToList();
    }

    private static TextInspectionReportHighlightSpan[] BuildHighlights(
        string originalText,
        IReadOnlyList<TextHighlightSpan> spans,
        IReadOnlyList<TextInspectionFinding> findings)
    {
        if(spans.Count == 0)
            return Array.Empty<TextInspectionReportHighlightSpan>();

        var output = new TextInspectionReportHighlightSpan[spans.Count];
        for(int i = 0;i < spans.Count;i++)
        {
            var span = spans[i];
            var findingIds = findings
                .Where(finding => finding.Evidence.Any(evidence =>
                    Overlaps(span.Start,span.EndExclusive,evidence.Start,evidence.EndExclusive)))
                .Select(static finding => finding.FindingId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static id => id,StringComparer.Ordinal)
                .ToArray();

            var findingRisk = findings
                .Where(finding => findingIds.Contains(finding.FindingId,StringComparer.Ordinal))
                .Select(static finding => finding.RiskLevel)
                .DefaultIfEmpty(TextInspectionRiskLevel.Informational)
                .Max();

            int start = Math.Max(0,Math.Min(span.Start,originalText.Length));
            int end = Math.Max(start,Math.Min(span.EndExclusive,originalText.Length));
            output[i] = new TextInspectionReportHighlightSpan
            {
                Start = start,
                Length = end - start,
                MatchedText = end > start ? originalText.Substring(start,end - start) : "",
                RiskLevel = findingIds.Length == 0
                    ? TextInspectionRiskLevel.Informational
                    : findingRisk,
                DetectionScore = span.DetectionScore,
                HighestDetectedSeverity = span.HighestDetectedSeverity,
                HasScoringEvidence = span.HasScoringEvidence,
                FindingIds = findingIds,
                RuleKeys = span.RuleKeys,
                Categories = span.Categories
            };
        }

        return output;
    }

    private static string BuildSummary(
        TextInspectionRiskLevel riskLevel,
        IReadOnlyList<TextInspectionFinding> findings,
        int supportingSignalCount)
    {
        int scored = findings.Count(static finding => finding.ContributesToRisk);
        if(scored == 0)
        {
            if(findings.Count > 0)
                return "No scored risk findings; " + findings.Count + " classification finding(s) observed.";
            if(supportingSignalCount > 0)
                return "No scored risk findings; " + supportingSignalCount + " supporting signal(s) observed.";
            return "No findings or supporting signals were detected.";
        }

        var top = findings.First(static finding => finding.ContributesToRisk);
        int categories = findings
            .Where(static finding => finding.ContributesToRisk && finding.Category.HasValue)
            .Select(static finding => finding.Category!.Value)
            .Distinct()
            .Count();
        return riskLevel + " risk: " + scored + " scored finding(s) across "
            + categories + " category/categories. Top finding: " + top.Title + ".";
    }

    private static (int Start,int Length) EvidenceBounds(IReadOnlyList<TextEvidence> evidence)
    {
        if(evidence.Count == 0)
            return (0,0);
        int start = evidence.Min(static item => item.Start);
        int end = evidence.Max(static item => item.EndExclusive);
        return (start,end - start);
    }

    private static bool Overlaps(int firstStart,int firstEnd,int secondStart,int secondEnd)
        => firstStart < secondEnd && secondStart < firstEnd;

    private static TextInspectionRiskLevel RiskFromScore(int score)
    {
        if(score <= 0) return TextInspectionRiskLevel.None;
        if(score >= 80) return TextInspectionRiskLevel.Critical;
        if(score >= 60) return TextInspectionRiskLevel.High;
        if(score >= 40) return TextInspectionRiskLevel.Medium;
        if(score >= 20) return TextInspectionRiskLevel.Low;
        return TextInspectionRiskLevel.Informational;
    }

    private static TextInspectionRiskLevel RiskFromSeverity(int severity,int severityScaleMax)
    {
        if(severity <= 0 || severityScaleMax <= 0)
            return TextInspectionRiskLevel.None;

        int percent = (int)Math.Round((double)severity / severityScaleMax * 100d);
        if(percent >= 90) return TextInspectionRiskLevel.Critical;
        if(percent >= 70) return TextInspectionRiskLevel.High;
        if(percent >= 50) return TextInspectionRiskLevel.Medium;
        if(percent >= 30) return TextInspectionRiskLevel.Low;
        return TextInspectionRiskLevel.Informational;
    }

    private static TextInspectionRiskLevel Max(TextInspectionRiskLevel first,TextInspectionRiskLevel second)
        => (int)first >= (int)second ? first : second;

    private static string Humanize(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return "Finding";

        var output = new System.Text.StringBuilder(value.Length + 8);
        char previous = '\0';
        for(int i = 0;i < value.Length;i++)
        {
            char c = value[i];
            if(c == '_' || c == '-')
            {
                if(output.Length > 0 && output[output.Length - 1] != ' ')
                    output.Append(' ');
                previous = c;
                continue;
            }

            if(output.Length > 0
               && char.IsUpper(c)
               && (char.IsLower(previous) || char.IsDigit(previous))
               && output[output.Length - 1] != ' ')
                output.Append(' ');

            output.Append(c);
            previous = c;
        }

        string text = output.ToString().Trim();
        if(text.Length == 0)
            return "Finding";
        return char.ToUpperInvariant(text[0]) + text.Substring(1);
    }

    private static string Slugify(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return "finding";

        var output = new System.Text.StringBuilder(value.Length + 8);
        bool previousDash = false;
        char previous = '\0';
        for(int i = 0;i < value.Length;i++)
        {
            char c = value[i];
            if(char.IsLetterOrDigit(c))
            {
                if(output.Length > 0
                   && char.IsUpper(c)
                   && (char.IsLower(previous) || char.IsDigit(previous))
                   && !previousDash)
                {
                    output.Append('-');
                }

                output.Append(char.ToLowerInvariant(c));
                previousDash = false;
            }
            else if(!previousDash && output.Length > 0)
            {
                output.Append('-');
                previousDash = true;
            }

            previous = c;
        }

        while(output.Length > 0 && output[output.Length - 1] == '-')
            output.Length--;

        return output.Length == 0 ? "finding" : output.ToString();
    }
}
