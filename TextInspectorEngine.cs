using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TextInspector;

/// <summary>
/// Immutable, thread-safe inspector for the fixed TextInspector rule set.
/// </summary>
public sealed partial class TextInspectorEngine
{
    internal TextInspectorScoringOptions Options { get; }
    public IReadOnlyList<TextInspectorDiagnostic> CompilationDiagnostics { get; }
    public int CompiledRuleCount => _rules.Length;
    public int CompiledPrefilteredRuleCount => _rules.Count(static rule => rule.PrefilterTerms.Length > 0);
    /// <summary>TextInspector does not evaluate composite policies.</summary>
    public int CompiledPolicyCount => 0;

    public int CompiledLinkedRuleCount => _linkedRules.Length;
    private readonly RuntimeLinkedRule[] _linkedRules;
    private readonly RuntimeRule[] _rules;

    private TextInspectorEngine(
        TextInspectorScoringOptions options,
        RuntimeRule[] rules,
        RuntimeLinkedRule[] linkedRules,
        IReadOnlyList<TextInspectorDiagnostic> diagnostics)
    {
        Options = options;
        _rules = rules;
        _linkedRules = linkedRules;
        CompilationDiagnostics = diagnostics;
    }

    public TextInspectionResult Inspect(string? text,bool includeMarkedHtml = false)
        => Inspect(text,new TextInspectionOptions { IncludeMarkedHtml = includeMarkedHtml });

    public TextInspectionResult Inspect(string? text,TextInspectionOptions? inspectionOptions)
    {
        text ??= "";
        inspectionOptions ??= new TextInspectionOptions();

        if(Options.MaxInputLength > 0 && text.Length > Options.MaxInputLength)
            throw new ArgumentException($"Input length {text.Length} exceeds configured MaxInputLength {Options.MaxInputLength}.",nameof(text));

        if(inspectionOptions.MaxReturnedEvidence is < 0)
            throw new ArgumentOutOfRangeException(nameof(inspectionOptions),"MaxReturnedEvidence cannot be negative.");
        if(inspectionOptions.MaxReturnedHighlightSpans is < 0)
            throw new ArgumentOutOfRangeException(nameof(inspectionOptions),"MaxReturnedHighlightSpans cannot be negative.");

        var runtimeDiagnostics = new List<TextInspectorDiagnostic>();
        var normalized = NormalizeWithMap(text);
        var evidence = BuildEvidence(text,normalized,runtimeDiagnostics);
        var contextualEvidence = ApplyContext(evidence,normalized.Text,runtimeDiagnostics);
        var evidenceWithCounts = ApplyCountsForScoring(contextualEvidence);
        var linkedMatches = ApplyLinkedRules(evidenceWithCounts);
        var summary = Summarize(evidenceWithCounts,linkedMatches,text.Length);

        IReadOnlyList<TextHighlightSpan> highlightSpans = Array.Empty<TextHighlightSpan>();
        if(inspectionOptions.IncludeMarkedHtml || inspectionOptions.IncludeHighlightSpans)
            highlightSpans = BuildHighlightSpans(evidenceWithCounts,inspectionOptions.HighlightScoredEvidenceOnly);

        string? markedHtml = inspectionOptions.IncludeMarkedHtml
            ? RenderMarkedHtml(text,highlightSpans)
            : null;

        string? maskedText = Options.Masking.Enabled
            ? RenderMaskedText(text,evidenceWithCounts,Options.Masking)
            : null;

        var publicEvidence = BuildPublicEvidence(text,evidenceWithCounts,inspectionOptions);
        var publicHighlights = inspectionOptions.IncludeHighlightSpans
            ? LimitHighlightSpans(highlightSpans,inspectionOptions.MaxReturnedHighlightSpans)
            : Array.Empty<TextHighlightSpan>();

        return new TextInspectionResult
        {
            LinkedRuleMatches = BuildPublicLinkedRules(text,linkedMatches,evidenceWithCounts,inspectionOptions),
            RiskAssessment = summary.RiskAssessment,
            MaskedText = maskedText,
            MarkedHtml = markedHtml,
            Evidence = publicEvidence,
            HighlightSpans = publicHighlights,
            CategorySummaries = summary.Categories,
            RiskContributions = summary.Contributions,
            Diagnostics = runtimeDiagnostics
        };
    }

    /// <summary>
    /// Produces a UI-ready report that correlates low-level rules into logical findings while
    /// retaining exact evidence/highlight spans from the original input.
    /// </summary>
    public TextInspectionReport CreateReport(string? text,TextInspectionReportOptions? reportOptions = null)
    {
        text ??= "";
        reportOptions ??= new TextInspectionReportOptions();
        reportOptions.Validate();

        var result = Inspect(text,new TextInspectionOptions
        {
            IncludeMatchedText = true,
            IncludeSnippets = true,
            IncludeContextDetails = true,
            IncludeHighlightSpans = reportOptions.IncludeHighlights,
            HighlightScoredEvidenceOnly = false,
            IncludeSourceLocations = reportOptions.IncludeSourceLocations,
            IncludeNonScoringEvidence = true
        });

        return TextInspectionReportBuilder.Build(text,result,Options,reportOptions);
    }

    public IReadOnlyList<TextInspectionResult> InspectMany(
        IEnumerable<string?> texts,
        TextInspectionOptions? inspectionOptions = null)
    {
        if(texts is null) throw new ArgumentNullException(nameof(texts));
        return texts.Select(text => Inspect(text,inspectionOptions)).ToArray();
    }

    /// <summary>
    /// Fast raw-rule pre-screen. Returns true on the first regex/evidence-group hit satisfying numeric conditions.
    /// Context gates, scoring, snippets, and category aggregation are intentionally skipped.
    /// Regex timeouts and invalid numeric captures are skipped; use Inspect for runtime diagnostics.
    /// </summary>

    public bool HasMatch(string? text)
    {
        text ??= "";
        if(Options.MaxInputLength > 0 && text.Length > Options.MaxInputLength)
            throw new ArgumentException($"Input length {text.Length} exceeds configured MaxInputLength {Options.MaxInputLength}.",nameof(text));
        if(text.Length == 0)
            return false;

        string normalized = NormalizeForMatchOnly(text);
        if(normalized.Length == 0)
            return false;

        foreach(var rule in _rules)
        {
            if(!PassesPrefilter(rule,normalized))
                continue;

            try
            {
                for(var match = rule.Regex.Match(normalized);match.Success;match = match.NextMatch())
                {
                    if(!PassesNumericConditions(rule,match))
                        continue;

                    if(rule.EvidenceGroup is null)
                    {
                        if(match.Length > 0)
                            return true;
                    }
                    else
                    {
                        var group = match.Groups[rule.EvidenceGroup];
                        if(group.Success && group.Length > 0)
                            return true;
                    }
                }
            }
            catch(RegexMatchTimeoutException)
            {
                // Fast path intentionally skips diagnostics allocation.
            }
        }

        return false;
    }

    /// <summary>
    /// Returns raw matches in priority/compile order with exact original-text spans.
    /// Numeric conditions apply; contextual scoring is skipped.
    /// </summary>

    public IReadOnlyList<TextRuleMatch> FindMatches(string? text,int maxMatches = int.MaxValue)
    {
        text ??= "";
        if(maxMatches < 0)
            throw new ArgumentOutOfRangeException(nameof(maxMatches),"maxMatches cannot be negative.");
        if(maxMatches == 0 || text.Length == 0)
            return Array.Empty<TextRuleMatch>();
        if(Options.MaxInputLength > 0 && text.Length > Options.MaxInputLength)
            throw new ArgumentException($"Input length {text.Length} exceeds configured MaxInputLength {Options.MaxInputLength}.",nameof(text));

        var normalized = NormalizeWithMap(text);
        if(normalized.Text.Length == 0)
            return Array.Empty<TextRuleMatch>();

        var output = new List<TextRuleMatch>(Math.Min(maxMatches,16));
        foreach(var rule in _rules)
        {
            if(!PassesPrefilter(rule,normalized.Text))
                continue;

            int ruleOutputStart = output.Count;
            try
            {
                for(var match = rule.Regex.Match(normalized.Text);match.Success;match = match.NextMatch())
                {
                    if(!PassesNumericConditions(rule,match))
                        continue;

                    if(TryGetFirstEvidenceCapture(rule,match,out var capture)
                            && TryCreateRuleMatch(rule,capture,normalized,text.Length,out var captured))
                    {
                        output.Add(captured);
                        if(output.Count >= maxMatches)
                            return output;
                    }
                }
            }
            catch(RegexMatchTimeoutException)
            {
                if(output.Count > ruleOutputStart)
                    output.RemoveRange(ruleOutputStart,output.Count - ruleOutputStart);
                // Fast path skips the timed-out rule. Use Inspect when runtime diagnostics are required.
            }
        }

        return output;
    }

    private static bool TryGetFirstEvidenceCapture(RuntimeRule rule,Match match,out Capture capture)
    {
        if(rule.EvidenceGroup is null)
        {
            capture = match.Groups[0];
            return capture.Length > 0;
        }

        var group = match.Groups[rule.EvidenceGroup];
        if(!group.Success || group.Length <= 0)
        {
            capture = group;
            return false;
        }

        capture = group;
        return capture.Length > 0;
    }

    private TextRuleMatch CreateRuleMatch(RuntimeRule rule,int start,int length)
        => new TextRuleMatch
        {
            RuleId = rule.RuleId,
            RuleKey = rule.Key,
            Category = rule.Category,
            SeverityScore = rule.SeverityScore,
            Priority = rule.Priority,
            DetectionScore = rule.DetectionScore,
            HighlightIntensity = NormalizeSeverity(rule.SeverityScore),
            Start = start,
            Length = length
        };

    private bool TryCreateRuleMatch(
        RuntimeRule rule,
        Capture capture,
        NormalizedText normalized,
        int originalLength,
        out TextRuleMatch result)
    {
        result = default!;
        int normStart = capture.Index;
        int normEndExclusive = capture.Index + capture.Length;
        if(capture.Length <= 0 || normStart < 0 || normEndExclusive > normalized.Text.Length)
            return false;

        int origStart = normalized.StartMap[normStart];
        int origEndExclusive = normalized.EndMap[normEndExclusive - 1];
        if(origStart < 0 || origStart >= originalLength || origEndExclusive <= origStart || origEndExclusive > originalLength)
            return false;

        result = CreateRuleMatch(rule,origStart,origEndExclusive - origStart);
        return true;
    }

    private static bool PassesPrefilter(RuntimeRule rule,string normalizedText)
    {
        if(rule.PrefilterTerms.Length == 0)
            return true;

        if(rule.PrefilterMode == TextInspectorPrefilterMode.All)
        {
            for(int i = 0;i < rule.PrefilterTerms.Length;i++)
                if(normalizedText.IndexOf(rule.PrefilterTerms[i],StringComparison.Ordinal) < 0)
                    return false;
            return true;
        }

        for(int i = 0;i < rule.PrefilterTerms.Length;i++)
            if(normalizedText.IndexOf(rule.PrefilterTerms[i],StringComparison.Ordinal) >= 0)
                return true;
        return false;
    }

    private List<EvidenceHit> BuildEvidence(
        string originalText,
        NormalizedText normalized,
        List<TextInspectorDiagnostic> runtimeDiagnostics)
    {
        if(normalized.Text.Length == 0)
            return new List<EvidenceHit>(0);

        var evidence = new List<EvidenceHit>(32);

        foreach(var rule in _rules)
        {
            if(!PassesPrefilter(rule,normalized.Text))
                continue;

            int evidenceStart = evidence.Count;
            try
            {
                for(var match = rule.Regex.Match(normalized.Text);match.Success;match = match.NextMatch())
                {
                    if(!PassesNumericConditions(rule,match,runtimeDiagnostics))
                        continue;

                    if(TryGetFirstEvidenceCapture(rule,match,out var capture))
                    {
                        TryAddEvidenceHit(evidence,rule,capture,originalText,normalized);
                    }
                }
            }
            catch(RegexMatchTimeoutException)
            {
                if(evidence.Count > evidenceStart)
                    evidence.RemoveRange(evidenceStart,evidence.Count - evidenceStart);

                runtimeDiagnostics.Add(new TextInspectorDiagnostic
                {
                    Kind = TextInspectorDiagnosticKind.RegexTimeout,
                    RuleId = rule.RuleId,
                    RuleKey = rule.Key,
                    Message = $"Rule exceeded the {Options.RegexTimeoutMilliseconds} ms regex timeout and was skipped for this inspection."
                });
            }
        }

        return evidence;
    }

    private bool TryAddEvidenceHit(
        List<EvidenceHit> evidence,
        RuntimeRule rule,
        Capture capture,
        string originalText,
        NormalizedText normalized)
    {
        int originalLength = originalText.Length;
        int normStart = capture.Index;
        int normEndExclusive = capture.Index + capture.Length;
        if(capture.Length <= 0 || normStart < 0 || normEndExclusive > normalized.Text.Length)
            return false;

        int origStart = normalized.StartMap[normStart];
        int origEndExclusive = normalized.EndMap[normEndExclusive - 1];
        if(origStart < 0 || origStart >= originalLength || origEndExclusive <= origStart || origEndExclusive > originalLength)
            return false;
        // Only a precise, rule-scoped known benign match is excluded. The remaining
        // evidence from this and other rules must continue through the engine.
        foreach(var exclusion in Options.FalsePositiveExclusions)
        {
            if(string.Equals(exclusion.RuleKey,rule.Key,StringComparison.OrdinalIgnoreCase)
               && exclusion.MatchedText.Length == origEndExclusive - origStart
               && string.Compare(originalText,origStart,exclusion.MatchedText,0,
                   exclusion.MatchedText.Length,StringComparison.OrdinalIgnoreCase) == 0)
                return false;
        }


        evidence.Add(new EvidenceHit(
            RuleId: rule.RuleId,
            RuleKey: rule.Key,
            RuleDescription: rule.Description,
            Tags: rule.Tags,
            Category: rule.Category,
            DetectionScore: rule.DetectionScore,
            SeverityScore: rule.SeverityScore,
            RuleWeight: rule.RuleWeight,
            MaxCountForScoring: rule.MaxCountForScoring,
            MaxEvidenceSnippets: rule.MaxEvidenceSnippets,
            Start: origStart,
            Length: origEndExclusive - origStart,
            NormalizedStart: normStart,
            NormalizedLength: capture.Length));
        return true;
    }

    private List<ContextualEvidenceHit> ApplyContext(
        List<EvidenceHit> evidence,
        string normalizedText,
        List<TextInspectorDiagnostic> runtimeDiagnostics)
    {
        if(evidence.Count == 0)
            return new List<ContextualEvidenceHit>(0);

        var rulesById = _rules.ToDictionary(static rule => rule.RuleId);
        var output = new List<ContextualEvidenceHit>(evidence.Count);

        for(int i = 0;i < evidence.Count;i++)
        {
            var hit = evidence[i];
            // Scope an explicit grammatical negation to this one credential request.
            // Detection evidence survives; only its risk contribution is suppressed.
            if(hit.RuleKey.StartsWith("Credential_",StringComparison.OrdinalIgnoreCase)
               && IsLocallyNegatedCredentialRequest(hit,normalizedText))
            {
                output.Add(new ContextualEvidenceHit(
                    hit,false,TextEvidenceScoringDisposition.ExcludedByContext,null));
                continue;
            }

            // Attribution is scoped to the matched quoted span, not the whole message.
            // Preserve the evidence while preventing a documented quotation from
            // contributing the same risk as a direct threat.
            if(hit.Category == TextInspectorRuleCategory.Threats &&
               IsAttributedQuotedEvidence(hit,normalizedText))
            {
                output.Add(new ContextualEvidenceHit(
                    hit,false,TextEvidenceScoringDisposition.ExcludedByContext,null));
                continue;
            }
            if(!rulesById.TryGetValue(hit.RuleId,out var rule) || rule.Context is null)
            {
                output.Add(new ContextualEvidenceHit(hit,true,null,null));
                continue;
            }

            var context = rule.Context;
            var matchedRequirements = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var matchedExclusions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for(int j = 0;j < evidence.Count;j++)
            {
                if(i == j)
                    continue;

                var nearby = evidence[j];
                if(NormalizedSpanDistance(hit,nearby) > context.WindowCharacters)
                    continue;

                if(context.RequireAnyRuleKeys.Contains(nearby.RuleKey,StringComparer.OrdinalIgnoreCase))
                    matchedRequirements.Add("rule:" + nearby.RuleKey);
                if(context.RequireAnyCategories.Contains(nearby.Category))
                    matchedRequirements.Add("category:" + nearby.Category);

                if(context.ExcludeAnyRuleKeys.Contains(nearby.RuleKey,StringComparer.OrdinalIgnoreCase))
                    matchedExclusions.Add("rule:" + nearby.RuleKey);
                if(context.ExcludeAnyCategories.Contains(nearby.Category))
                    matchedExclusions.Add("category:" + nearby.Category);
            }

            int windowStart = Math.Max(0,hit.NormalizedStart - context.WindowCharacters);
            int windowEnd = Math.Min(
                normalizedText.Length,
                hit.NormalizedStart + hit.NormalizedLength + context.WindowCharacters);
            string windowText = normalizedText.Substring(windowStart,windowEnd - windowStart);

            bool contextRegexFailed = false;
            if(!TryMatchContextRegexes(
                context.RequireAnyRegexes,
                windowText,
                matchedRequirements,
                runtimeDiagnostics,
                rule,
                "requirement"))
                contextRegexFailed = true;

            if(!TryMatchContextRegexes(
                context.ExcludeAnyRegexes,
                windowText,
                matchedExclusions,
                runtimeDiagnostics,
                rule,
                "exclusion"))
                contextRegexFailed = true;

            bool hasExclusion = matchedExclusions.Count > 0;
            bool requirementSatisfied = !context.HasRequirements
                || matchedRequirements.Count >= context.MinimumRequiredMatches;
            bool satisfied = !contextRegexFailed && !hasExclusion && requirementSatisfied;

            TextEvidenceScoringDisposition? disposition = null;
            if(contextRegexFailed)
                disposition = TextEvidenceScoringDisposition.ContextEvaluationFailed;
            else if(hasExclusion)
                disposition = TextEvidenceScoringDisposition.ExcludedByContext;
            else if(!requirementSatisfied)
                disposition = TextEvidenceScoringDisposition.MissingRequiredContext;

            output.Add(new ContextualEvidenceHit(
                hit,
                satisfied,
                disposition,
                new TextEvidenceContext
                {
                    WindowCharacters = context.WindowCharacters,
                    MinimumRequiredMatches = context.MinimumRequiredMatches,
                    IsSatisfied = satisfied,
                    RequiredSelectors = DescribeRequiredSelectors(context),
                    ExcludedSelectors = DescribeExcludedSelectors(context),
                    MatchedRequirements = matchedRequirements.OrderBy(static value => value,StringComparer.Ordinal).ToArray(),
                    MatchedExclusions = matchedExclusions.OrderBy(static value => value,StringComparer.Ordinal).ToArray()
                }));
        }

        return output;
    }

    private static bool IsLocallyNegatedCredentialRequest(EvidenceHit hit,string text)
    {
        int start = hit.NormalizedStart;
        if(start <= 0 || start > text.Length) return false;
        int from = Math.Max(0,start - 40);
        string before = text.Substring(from,start - from);
        // No general keyword allowlist: the negation must grammatically
        // precede this occurrence with at most one common intensifier.
        return Regex.IsMatch(before,
            @"\b(?:do\s+not|don't|never|should\s+not|must\s+not)\s+(?:ever\s+)?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(50));
    }

    private static bool IsAttributedQuotedEvidence(EvidenceHit hit,string text)
    {
        int start = hit.NormalizedStart;
        int end = start + hit.NormalizedLength;
        if(start < 0 || end > text.Length || end >= text.Length)
            return false;

        // Require matching quotation punctuation next to the exact hit, permitting
        // sentence punctuation just before the closing quote.
        int close = end;
        while(close < text.Length && (text[close] == '.' || text[close] == '!' ||
              text[close] == '?' || char.IsWhiteSpace(text[close])))
            close++;
        if(close >= text.Length || (text[close] != '"' && text[close] != '”'))
            return false;

        int opening = start - 1;
        while(opening >= 0 && char.IsWhiteSpace(text[opening])) opening--;
        if(opening < 0 || (text[opening] != '"' && text[opening] != '“'))
            return false;

        int prefixStart = Math.Max(0,opening - 100);
        var prefix = text.Substring(prefixStart,opening - prefixStart);
        // Cues must directly introduce the quote. An unrelated word 'report'
        // elsewhere in the message cannot suppress a direct threat.
        return Regex.IsMatch(prefix,
            @"\b(?:report|transcript|script|moderator|fictional|documented|quoted|archived|citation|example)\b[^\r\n:]{0,65}:\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(50));
    }

    private List<EvidenceHitWithCounts> ApplyCountsForScoring(List<ContextualEvidenceHit> evidence)
    {
        if(evidence.Count == 0)
            return new List<EvidenceHitWithCounts>(0);

        var perRuleCount = new Dictionary<Guid,int>(32);
        var perRuleSnippetCount = new Dictionary<Guid,int>(32);
        var output = new List<EvidenceHitWithCounts>(evidence.Count);

        foreach(var contextual in evidence)
        {
            var e = contextual.Hit;
            perRuleSnippetCount.TryGetValue(e.RuleId,out int snippetsSeen);
            bool includeSnippet = snippetsSeen < e.MaxEvidenceSnippets;
            if(includeSnippet)
                perRuleSnippetCount[e.RuleId] = snippetsSeen + 1;

            bool countsForScoring = false;
            TextEvidenceScoringDisposition disposition;

            if(!contextual.ContextEligible)
            {
                disposition = contextual.ContextDisposition ?? TextEvidenceScoringDisposition.ContextEvaluationFailed;
            }
            else if(e.MaxCountForScoring == 0)
            {
                disposition = TextEvidenceScoringDisposition.EvidenceOnly;
            }
            else
            {
                perRuleCount.TryGetValue(e.RuleId,out int seen);
                seen++;
                perRuleCount[e.RuleId] = seen;
                countsForScoring = seen <= e.MaxCountForScoring;
                disposition = countsForScoring
                    ? TextEvidenceScoringDisposition.Counted
                    : TextEvidenceScoringDisposition.RuleLimitExceeded;
            }

            output.Add(new EvidenceHitWithCounts(
                e.RuleId,e.RuleKey,e.RuleDescription,e.Tags,e.Category,e.DetectionScore,e.SeverityScore,e.RuleWeight,
                e.MaxCountForScoring,e.Start,e.Length,
                ContributesToRisk: countsForScoring,
                ScoringStatus: disposition,
                IncludeSnippet: includeSnippet,
                Context: contextual.Context));
        }

        if(Options.DeduplicateExactMatchesForScoring)
        {
            foreach(var group in output
                .Select((hit,index) => (hit,index))
                .Where(static item => item.hit.ContributesToRisk)
                .GroupBy(static item => (item.hit.Category,item.hit.Start,item.hit.Length)))
            {
                var winner = group
                    .OrderByDescending(static item => item.hit.RuleWeight)
                    .ThenByDescending(static item => item.hit.SeverityScore)
                    .ThenBy(static item => item.hit.RuleKey,StringComparer.Ordinal)
                    .First();

                foreach(var item in group)
                {
                    if(item.index == winner.index)
                        continue;

                    output[item.index] = item.hit with
                    {
                        ContributesToRisk = false,
                        ScoringStatus = TextEvidenceScoringDisposition.ExactDuplicateSuppressed
                    };
                }
            }
        }

        return output;
    }

    private IReadOnlyList<TextEvidence> BuildPublicEvidence(
        string originalText,
        List<EvidenceHitWithCounts> evidence,
        TextInspectionOptions inspectionOptions)
    {
        int take = inspectionOptions.MaxReturnedEvidence ?? evidence.Count;
        if(take == 0 || evidence.Count == 0)
            return Array.Empty<TextEvidence>();

        var output = new List<TextEvidence>(Math.Min(take,evidence.Count));
        int[]? lineStarts = inspectionOptions.IncludeSourceLocations ? BuildLineStarts(originalText) : null;
        for(int i = 0;i < evidence.Count && output.Count < take;i++)
        {
            var e = evidence[i];
            if(!inspectionOptions.IncludeNonScoringEvidence && !e.ContributesToRisk)
                continue;

            var snippet = inspectionOptions.IncludeSnippets && e.IncludeSnippet
                ? CreateSnippet(originalText,e.Start,e.Length)
                : default;

            output.Add(new TextEvidence
            {
                RuleId = e.RuleId,
                RuleKey = e.RuleKey,
                RuleDescription = e.RuleDescription,
                Tags = e.Tags,
                Category = e.Category,
                DetectionScore = e.DetectionScore,
                SeverityScore = e.SeverityScore,
                EffectiveSeverity = e.EffectiveSeverity,
                EffectiveRiskPoints = e.EffectiveRiskPoints,
                RiskMultiplier = e.RiskMultiplier,
                LinkedRuleKeys = e.LinkedRuleKeys,
                RuleWeight = e.RuleWeight,
                MaxCountForScoring = e.MaxCountForScoring,
                Start = e.Start,
                Length = e.Length,
                SourceRange = inspectionOptions.IncludeSourceLocations ? CreateSourceRange(lineStarts!,e.Start,e.Length) : null,
                MatchedText = inspectionOptions.IncludeMatchedText ? originalText.Substring(e.Start,e.Length) : "",
                Snippet = snippet.Text,
                SnippetStart = snippet.Start,
                HighlightIntensity = NormalizeSeverity(e.EffectiveSeverity),
                Context = inspectionOptions.IncludeContextDetails ? e.Context : null,
                ContributesToRisk = e.ContributesToRisk,
                ScoringStatus = e.ScoringStatus
            });
        }

        return output;
    }

    private string RenderMaskedText(
        string originalText,
        IReadOnlyList<EvidenceHitWithCounts> evidence,
        TextInspectorMaskingOptions masking)
    {
        if(originalText.Length == 0 || evidence.Count == 0)
            return originalText;

        char[]? masked = null;
        foreach(var hit in evidence)
        {
            if(hit.Length <= 0)
                continue;

            int start = Clamp(hit.Start,0,originalText.Length);
            int end = Clamp(hit.Start + hit.Length,start,originalText.Length);
            if(end <= start)
                continue;

            masked ??= originalText.ToCharArray();
            for(int i = start;i < end;i++)
                masked[i] = masking.MaskCharacter;
        }

        return masked is null ? originalText : new string(masked);
    }

    private string RenderMarkedHtml(string originalText,IReadOnlyList<TextHighlightSpan> spans)
    {
        if(originalText.Length == 0)
            return "";
        if(spans.Count == 0)
            return Html(originalText);

        var sb = new StringBuilder(originalText.Length * 2);
        int position = 0;
        foreach(var span in spans)
        {
            int start = Clamp(span.Start,0,originalText.Length);
            int end = Clamp(span.EndExclusive,start,originalText.Length);
            if(start > position)
                AppendPlainHtml(sb,originalText,position,start - position);
            if(end <= position)
                continue;

            start = Math.Max(start,position);
            AppendHighlightHtml(sb,originalText,start,end - start,span);
            position = end;
        }

        if(position < originalText.Length)
            AppendPlainHtml(sb,originalText,position,originalText.Length - position);

        return sb.ToString().Replace("\r\n","\n").Replace("\n","<br/>");
    }

    private static void AppendPlainHtml(StringBuilder sb,string text,int start,int length)
        => sb.Append(WebUtility.HtmlEncode(text.Substring(start,length)));

    private static void AppendHighlightHtml(
        StringBuilder sb,
        string text,
        int start,
        int length,
        TextHighlightSpan span)
    {
        sb.Append("<span class=\"text-inspector-hit\" data-score=\"");
        sb.Append(span.HighlightIntensity);
        sb.Append("\" data-severity=\"");
        sb.Append(span.HighestDetectedSeverity);
        sb.Append("\" data-scored=\"");
        sb.Append(span.HasScoringEvidence ? "true" : "false");
        sb.Append("\" data-rules=\"");
        sb.Append(WebUtility.HtmlEncode(string.Join(",",span.RuleKeys)));
        sb.Append("\" data-categories=\"");
        sb.Append(WebUtility.HtmlEncode(string.Join(",",span.Categories)));
        if(span.IsLinked)
        {
            sb.Append("\" data-linked-rules=\"");
            sb.Append(WebUtility.HtmlEncode(string.Join(",",span.LinkedRuleKeys)));
            sb.Append("\" data-effective-severity=\"");
            sb.Append(span.EffectiveSeverity);
        }
        sb.Append("\" style=\"");
        sb.Append(ScoreToHighlightStyle(span.HighlightIntensity));
        sb.Append("\">");
        sb.Append(WebUtility.HtmlEncode(text.Substring(start,length)));
        sb.Append("</span>");
    }

    private static string Html(string text)
        => WebUtility.HtmlEncode(text).Replace("\r\n","\n").Replace("\n","<br/>");

    private int NormalizeSeverity(int severity)
    {
        if(severity <= 0)
            return 0;
        double normalized = (double)severity / Options.SeverityScaleMax * 100d;
        return Clamp((int)Math.Round(normalized),0,100);
    }

    private int SaturatingScore(int raw)
    {
        if(raw <= 0)
            return Options.ScoreMin;
        if(raw >= Options.RawCapFor100)
            return Options.ScoreMax;

        long range = Options.ScoreMax - Options.ScoreMin;
        int relative = (int)((range * raw) / (raw + Options.SaturationK));
        int score = Options.ScoreMin + relative;

        if(score <= Options.ScoreMin)
            score = Math.Max(Options.MinNonZeroScore,Options.ScoreMin);

        return Clamp(score,Options.ScoreMin,Options.ScoreMax);
    }

    private static int Clamp(int value,int minimum,int maximum)
    {
        if(value < minimum) return minimum;
        if(value > maximum) return maximum;
        return value;
    }

    private static int AddRaw(int current,int addition)
        => (int)Math.Min(int.MaxValue,(long)current + addition);

    private static TextInspectorDiagnostic Diagnostic(
        TextInspectorDiagnosticKind kind,
        TextInspectorRulePattern rule,
        string message)
        => new()
        {
            Kind = kind,
            RuleId = rule.TextInspectorRuleId == Guid.Empty ? null : rule.TextInspectorRuleId,
            RuleKey = string.IsNullOrWhiteSpace(rule.Key) ? null : rule.Key,
            Message = message
        };

    private static (int r,int g,int b) InterpolateGradient(double t)
    {
        var stops = GradientStops;
        int segments = stops.Length - 1;
        double scaled = t * segments;
        int index = (int)Math.Floor(scaled);
        if(index >= segments)
            return stops[stops.Length - 1];

        double localT = scaled - index;
        var start = stops[index];
        var end = stops[index + 1];
        return (Lerp(start.r,end.r,localT),Lerp(start.g,end.g,localT),Lerp(start.b,end.b,localT));
    }

    private static int Lerp(int a,int b,double t) => (int)Math.Round(a + (b - a) * t);

    private static readonly (int r,int g,int b)[] GradientStops =
    {
        (23,162,184),
        (255,193,7),
        (253,126,20),
        (220,53,69),
        (220,38,38)
    };

    private static string ScoreToHighlightStyle(int score)
    {
        score = Clamp(score,0,100);
        var rgb = InterpolateGradient(score / 100.0);
        double luminance = 0.2126 * rgb.r + 0.7152 * rgb.g + 0.0722 * rgb.b;
        string textColor = luminance < 140 ? "#ffffff" : "#212529";

        return score > 80
            ? $"background-color:rgb({rgb.r},{rgb.g},{rgb.b});color:{textColor};box-shadow:0 0 4px rgba(220,53,69,.6);"
            : $"background-color:rgb({rgb.r},{rgb.g},{rgb.b});color:{textColor};border:1px solid rgba(0,0,0,.1);";
    }

    private readonly record struct NormalizedText(string Text,int[] StartMap,int[] EndMap);

    private readonly record struct EvidenceHit(
        Guid RuleId,
        string RuleKey,
        string? RuleDescription,
        string[] Tags,
        TextInspectorRuleCategory Category,
        int DetectionScore,
        int SeverityScore,
        int RuleWeight,
        int MaxCountForScoring,
        int MaxEvidenceSnippets,
        int Start,
        int Length,
        int NormalizedStart,
        int NormalizedLength);

    private sealed record EvidenceHitWithCounts(
        Guid RuleId,
        string RuleKey,
        string? RuleDescription,
        string[] Tags,
        TextInspectorRuleCategory Category,
        int DetectionScore,
        int SeverityScore,
        int RuleWeight,
        int MaxCountForScoring,
        int Start,
        int Length,
        bool ContributesToRisk,
        TextEvidenceScoringDisposition ScoringStatus,
        bool IncludeSnippet,
        TextEvidenceContext? Context)
    {
        public int RiskMultiplier { get; init; } = 1;
        public int? LinkedEffectiveSeverity { get; init; }
        public int EffectiveSeverity => LinkedEffectiveSeverity ?? SeverityScore;
        public int EffectiveRiskPoints => ContributesToRisk ? MultiplyCapped(RuleWeight, RiskMultiplier, int.MaxValue) : 0;
        public string[] LinkedRuleKeys { get; init; } = Array.Empty<string>();
    }

    private readonly record struct HighlightSpanEvent(int Position,int EvidenceIndex,bool IsStart);

    private sealed class CategoryAccumulator
    {
        public int TotalEvidenceCount { get; set; }
        public int ScoringEvidenceCount { get; set; }
        public int TotalRiskPoints { get; set; }
        public int HighestDetectedSeverity { get; set; }
        public int HighestScoredSeverity { get; set; }
    }

    private sealed record RuntimeRule(
        Guid RuleId,
        string Key,
        string? Description,
        string[] Tags,
        TextInspectorRuleCategory Category,
        int DetectionScore,
        int SeverityScore,
        int Priority,
        int SourceOrder,
        Regex Regex,
        RuntimeNumericCondition[] NumericConditions,
        string? EvidenceGroup,
        string[] PrefilterTerms,
        TextInspectorPrefilterMode PrefilterMode,
        RuntimeRuleContext? Context,
        int RuleWeight,
        int MaxCountForScoring,
        int MaxEvidenceSnippets);

    private sealed record ContextualEvidenceHit(
        EvidenceHit Hit,
        bool ContextEligible,
        TextEvidenceScoringDisposition? ContextDisposition,
        TextEvidenceContext? Context);

    private sealed record RuntimeContextRegex(string Pattern,Regex Regex);

    private sealed record RuntimeRuleContext(
        int WindowCharacters,
        int MinimumRequiredMatches,
        string[] RequireAnyRuleKeys,
        TextInspectorRuleCategory[] RequireAnyCategories,
        RuntimeContextRegex[] RequireAnyRegexes,
        string[] ExcludeAnyRuleKeys,
        TextInspectorRuleCategory[] ExcludeAnyCategories,
        RuntimeContextRegex[] ExcludeAnyRegexes)
    {
        public bool HasRequirements => RequireAnyRuleKeys.Length > 0
            || RequireAnyCategories.Length > 0
            || RequireAnyRegexes.Length > 0;

        public IEnumerable<string> ReferencedRuleKeys
            => RequireAnyRuleKeys.Concat(ExcludeAnyRuleKeys);
    }

    private sealed record SummaryResult(
        TextSafetyInfo RiskAssessment,
        IReadOnlyList<TextCategorySummary> Categories,
        IReadOnlyList<TextScoreContribution> Contributions);

    /// <summary>
    /// Pro-only parallel batch inspection. Use InspectMany in Free or update to TextInspector.Pro.
    /// </summary>
    /// <exception cref="NotSupportedException">Parallel batch processing requires Pro.</exception>
    public IReadOnlyList<TextInspectionResult> InspectManyParallel(
        IEnumerable<string?> texts, TextInspectionOptions? options = null,
        int maxDegreeOfParallelism = -1)
        => throw new NotSupportedException("Update to TextInspector.Pro to use parallel batch inspection.");


    /// <summary>Pro-only targeted raw matching. Upgrade to TextInspector.Pro to use filters.</summary>
    public bool HasMatch(string? text, TextRuleMatchFilter? filter)
        => throw new NotSupportedException("Update to TextInspector.Pro to use filtered matching.");

    /// <summary>Pro-only targeted first-match lookup.</summary>
    public TextRuleMatch? FindFirstMatch(string? text, TextRuleMatchFilter? filter = null)
        => throw new NotSupportedException("Update to TextInspector.Pro to use filtered matching.");

    /// <summary>Pro-only targeted raw matching with filters.</summary>
    public IReadOnlyList<TextRuleMatch> FindMatches(string? text, TextRuleMatchFilter? filter, int maxMatches = int.MaxValue)
        => throw new NotSupportedException("Update to TextInspector.Pro to use filtered matching.");


}
