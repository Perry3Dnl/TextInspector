using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TextInspector;

// Engine component extracted without changing its existing algorithm or public signatures.
public sealed partial class TextInspectorEngine
{
    /// <summary>Creates a TextInspector engine from its fixed embedded rule set.</summary>
    public static TextInspectorEngine Create(TextInspectorOptions? options = null)
    {
        options ??= new TextInspectorOptions();
        options.Validate();
        var scoring = TextInspectorPresets.LoadOptions();
        scoring.FalsePositiveExclusions = options.FalsePositiveExclusions.Select(item => new TextInspectorFalsePositiveExclusion
        {
            RuleKey = item.RuleKey, MatchedText = item.MatchedText
        }).ToArray();
        scoring.Masking.Enabled = options.EnableMasking;
        scoring.Masking.MaskCharacter = options.MaskCharacter;
        var disabledKeys = new HashSet<string>(options.DisabledRuleKeys, StringComparer.OrdinalIgnoreCase);
        var disabledCategories = new HashSet<TextInspectorRuleCategory>(options.DisabledCategories);
        var rules = TextInspectorPresets.LoadRules()
            .Where(rule => !disabledKeys.Contains(rule.Key) && !disabledCategories.Contains(rule.Category));
        return Compile(scoring, rules);
    }

    internal static TextInspectorEngine Compile(TextInspectorScoringOptions options, IEnumerable<TextInspectorRulePattern> rules)
        => CompileWithDiagnostics(options, rules).Engine;

    internal static TextInspectorCompilationResult CompileWithDiagnostics(
        TextInspectorScoringOptions options, IEnumerable<TextInspectorRulePattern> rules)
    {
        if(options is null) throw new ArgumentNullException(nameof(options));
        if(rules is null) throw new ArgumentNullException(nameof(rules));
        options.Validate();

        var compiledRules = new List<RuntimeRule>();
        var diagnostics = new List<TextInspectorDiagnostic>();
        var seenIds = new HashSet<Guid>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int sourceOrder = 0;

        foreach(var rule in rules)
        {
            int ruleSourceOrder = sourceOrder++;
            if(rule is null)
                continue;

            if(!rule.IsEnabled)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleDisabled,rule,"Rule is disabled and was not compiled."));
                continue;
            }

            if(rule.TextInspectorRuleId == Guid.Empty)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"Rule ID is empty."));
                continue;
            }

            if(seenIds.Contains(rule.TextInspectorRuleId))
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"Duplicate rule ID; later occurrence was skipped."));
                continue;
            }

            if(string.IsNullOrWhiteSpace(rule.Key))
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"Rule key is empty."));
                continue;
            }

            if(seenKeys.Contains(rule.Key.Trim()))
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"Duplicate rule key; later occurrence was skipped."));
                continue;
            }

            if(string.IsNullOrWhiteSpace(rule.RegexPattern))
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"Regex pattern is empty."));
                continue;
            }

            if(rule.MaxCountForScoring < 0)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"MaxCountForScoring cannot be negative."));
                continue;
            }

            if(rule.MaxEvidenceSnippets < 0)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"MaxEvidenceSnippets cannot be negative."));
                continue;
            }

            if(rule.RuleWeight < 0)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"RuleWeight cannot be negative."));
                continue;
            }

            if(rule.DetectionScore < 1 || rule.DetectionScore > 100)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"DetectionScore must be between 1 and 100."));
                continue;
            }

            if(rule.SeverityScore < 0)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"SeverityScore cannot be negative."));
                continue;
            }

            if(!Enum.IsDefined(typeof(TextInspectorPrefilterMode),rule.PrefilterMode))
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"PrefilterMode is invalid."));
                continue;
            }

            if(rule.Context is not null && rule.Context.WindowCharacters < 0)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"Context.WindowCharacters cannot be negative."));
                continue;
            }

            if(rule.Context is not null && rule.Context.MinimumRequiredMatches <= 0)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.RuleSkipped,rule,"Context.MinimumRequiredMatches must be greater than zero."));
                continue;
            }

            try
            {
                var regex = new Regex(
                    rule.RegexPattern!,
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                    TimeSpan.FromMilliseconds(options.RegexTimeoutMilliseconds));

                var configuredEvidenceGroup = rule.EvidenceGroup;
                string? evidenceGroup = string.IsNullOrWhiteSpace(configuredEvidenceGroup) ? null : configuredEvidenceGroup!.Trim();
                if(evidenceGroup is not null && !regex.GetGroupNames().Contains(evidenceGroup,StringComparer.Ordinal))
                {
                    diagnostics.Add(Diagnostic(
                        TextInspectorDiagnosticKind.InvalidEvidenceGroup,
                        rule,
                        $"EvidenceGroup '{evidenceGroup}' does not exist in the rule regex."));
                    continue;
                }

                if(!TryCompileContext(rule,options,diagnostics,out var context))
                    continue;

                var prefilterTerms = CleanStrings(rule.PrefilterTerms)
                    .Select(term => NormalizeForMatchOnly(term,options))
                    .Where(static term => term.Length > 0)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();

                seenIds.Add(rule.TextInspectorRuleId);
                seenKeys.Add(rule.Key.Trim());
                compiledRules.Add(new RuntimeRule(
                    RuleId: rule.TextInspectorRuleId,
                    Key: rule.Key.Trim(),
                    Description: rule.Description,
                    Tags: rule.Tags?.ToArray() ?? Array.Empty<string>(),
                    Category: rule.Category,
                    DetectionScore: rule.DetectionScore,
                    SeverityScore: rule.SeverityScore,
                    Priority: rule.Priority,
                    SourceOrder: ruleSourceOrder,
                    Regex: regex,
                    EvidenceGroup: evidenceGroup,
                    PrefilterTerms: prefilterTerms,
                    PrefilterMode: rule.PrefilterMode,
                    Context: context,
                    RuleWeight: rule.RuleWeight,
                    MaxCountForScoring: rule.MaxCountForScoring,
                    MaxEvidenceSnippets: rule.MaxEvidenceSnippets == 0 ? options.DefaultMaxEvidenceSnippets : rule.MaxEvidenceSnippets));
            }
            catch(ArgumentException ex)
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.InvalidRegex,rule,$"Invalid regex: {ex.Message}"));
            }
        }

        compiledRules = compiledRules
            .OrderByDescending(static rule => rule.Priority)
            .ThenBy(static rule => rule.SourceOrder)
            .ToList();

        AddContextReferenceDiagnostics(compiledRules,diagnostics);

        var frozenDiagnostics = diagnostics.ToArray();
        var engine = new TextInspectorEngine(options,compiledRules.ToArray(),frozenDiagnostics);
        return new TextInspectorCompilationResult { Engine = engine, Diagnostics = frozenDiagnostics };
    }

    private static bool TryCompileContext(
        TextInspectorRulePattern rule,
        TextInspectorScoringOptions options,
        List<TextInspectorDiagnostic> diagnostics,
        out RuntimeRuleContext? context)
    {
        context = null;
        var source = rule.Context;
        if(source is null)
            return true;

        var requireKeys = CleanStrings(source.RequireAnyRuleKeys);
        var requireCategories = source.RequireAnyCategories?.Distinct().ToArray() ?? Array.Empty<TextInspectorRuleCategory>();
        var excludeKeys = CleanStrings(source.ExcludeAnyRuleKeys);
        var excludeCategories = source.ExcludeAnyCategories?.Distinct().ToArray() ?? Array.Empty<TextInspectorRuleCategory>();

        if(!TryCompileContextRegexes(source.RequireAnyRegexPatterns,rule,options,diagnostics,out var requireRegexes)
           || !TryCompileContextRegexes(source.ExcludeAnyRegexPatterns,rule,options,diagnostics,out var excludeRegexes))
            return false;

        int requirementSelectorCount = requireKeys.Length + requireCategories.Length + requireRegexes.Length;
        if(requirementSelectorCount > 0 && source.MinimumRequiredMatches > requirementSelectorCount)
        {
            diagnostics.Add(Diagnostic(
                TextInspectorDiagnosticKind.RuleSkipped,
                rule,
                $"Context.MinimumRequiredMatches ({source.MinimumRequiredMatches}) exceeds the number of distinct requirement selectors ({requirementSelectorCount})."));
            return false;
        }

        if(requireKeys.Length == 0
           && requireCategories.Length == 0
           && requireRegexes.Length == 0
           && excludeKeys.Length == 0
           && excludeCategories.Length == 0
           && excludeRegexes.Length == 0)
            return true;

        context = new RuntimeRuleContext(
            source.WindowCharacters,
            source.MinimumRequiredMatches,
            requireKeys,
            requireCategories,
            requireRegexes,
            excludeKeys,
            excludeCategories,
            excludeRegexes);
        return true;
    }

    private static bool TryCompileContextRegexes(
        string[]? patterns,
        TextInspectorRulePattern rule,
        TextInspectorScoringOptions options,
        List<TextInspectorDiagnostic> diagnostics,
        out RuntimeContextRegex[] regexes)
    {
        var output = new List<RuntimeContextRegex>();
        foreach(var pattern in patterns ?? Array.Empty<string>())
        {
            if(string.IsNullOrWhiteSpace(pattern))
                continue;

            try
            {
                output.Add(new RuntimeContextRegex(
                    pattern,
                    new Regex(
                        pattern,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                        TimeSpan.FromMilliseconds(options.RegexTimeoutMilliseconds))));
            }
            catch(ArgumentException ex)
            {
                diagnostics.Add(Diagnostic(
                    TextInspectorDiagnosticKind.InvalidContextRegex,
                    rule,
                    $"Invalid context regex '{pattern}': {ex.Message}"));
                regexes = Array.Empty<RuntimeContextRegex>();
                return false;
            }
        }

        regexes = output.ToArray();
        return true;
    }

    private static string[] CleanStrings(string[]? values)
        => (values ?? Array.Empty<string>())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static void AddContextReferenceDiagnostics(
        IReadOnlyList<RuntimeRule> rules,
        List<TextInspectorDiagnostic> diagnostics)
    {
        var knownKeys = new HashSet<string>(TextInspectorPresets.LoadRules().Select(static rule => rule.Key),StringComparer.OrdinalIgnoreCase);
        knownKeys.UnionWith(rules.Select(static rule => rule.Key));
        foreach(var rule in rules)
        {
            if(rule.Context is null)
                continue;

            foreach(var referencedKey in rule.Context.ReferencedRuleKeys.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if(knownKeys.Contains(referencedKey))
                    continue;

                diagnostics.Add(new TextInspectorDiagnostic
                {
                    Kind = TextInspectorDiagnosticKind.ContextReferenceNotFound,
                    RuleId = rule.RuleId,
                    RuleKey = rule.Key,
                    Message = $"Context references rule key '{referencedKey}', but no enabled compiled rule has that key."
                });
            }
        }
    }

}
