using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TextInspector;

// Shared, behavior-preserving engine component. Keep this file identical across TextInspector and Pro.
public sealed partial class TextInspectorEngine
{
    private static string[] DescribeRequiredSelectors(RuntimeRuleContext context)
        => context.RequireAnyRuleKeys.Select(static value => "rule:" + value)
            .Concat(context.RequireAnyCategories.Select(static value => "category:" + value))
            .Concat(context.RequireAnyRegexes.Select(static value => "regex:" + value.Pattern))
            .OrderBy(static value => value,StringComparer.Ordinal)
            .ToArray();

    private static string[] DescribeExcludedSelectors(RuntimeRuleContext context)
        => context.ExcludeAnyRuleKeys.Select(static value => "rule:" + value)
            .Concat(context.ExcludeAnyCategories.Select(static value => "category:" + value))
            .Concat(context.ExcludeAnyRegexes.Select(static value => "regex:" + value.Pattern))
            .OrderBy(static value => value,StringComparer.Ordinal)
            .ToArray();

    private static int NormalizedSpanDistance(EvidenceHit first,EvidenceHit second)
    {
        int firstEnd = first.NormalizedStart + first.NormalizedLength;
        int secondEnd = second.NormalizedStart + second.NormalizedLength;
        if(firstEnd < second.NormalizedStart)
            return second.NormalizedStart - firstEnd;
        if(secondEnd < first.NormalizedStart)
            return first.NormalizedStart - secondEnd;
        return 0;
    }

    private bool TryMatchContextRegexes(
        RuntimeContextRegex[] regexes,
        string windowText,
        HashSet<string> matches,
        List<TextInspectorDiagnostic> runtimeDiagnostics,
        RuntimeRule rule,
        string kind)
    {
        foreach(var contextRegex in regexes)
        {
            try
            {
                if(contextRegex.Regex.IsMatch(windowText))
                    matches.Add("regex:" + contextRegex.Pattern);
            }
            catch(RegexMatchTimeoutException)
            {
                runtimeDiagnostics.Add(new TextInspectorDiagnostic
                {
                    Kind = TextInspectorDiagnosticKind.RegexTimeout,
                    RuleId = rule.RuleId,
                    RuleKey = rule.Key,
                    Message = $"Context {kind} regex exceeded the {Options.RegexTimeoutMilliseconds} ms timeout; this occurrence was not scored."
                });
                return false;
            }
        }

        return true;
    }

}
