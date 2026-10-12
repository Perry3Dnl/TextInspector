using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TextInspector;

// Shared, behavior-preserving engine component. Keep this file identical across Lite and Pro.
public sealed partial class TextInspectorEngine
{
    private IReadOnlyList<TextHighlightSpan> BuildHighlightSpans(
        List<EvidenceHitWithCounts> evidence,
        bool scoredOnly)
    {
        if(evidence.Count == 0)
            return Array.Empty<TextHighlightSpan>();

        var eligible = evidence
            .Select((hit,index) => (hit,index))
            .Where(item => !scoredOnly || item.hit.ContributesToRisk)
            .ToArray();
        if(eligible.Length == 0)
            return Array.Empty<TextHighlightSpan>();

        var events = new List<HighlightSpanEvent>(eligible.Length * 2);
        foreach(var item in eligible)
        {
            if(item.hit.Length <= 0)
                continue;
            events.Add(new HighlightSpanEvent(item.hit.Start,item.index,true));
            events.Add(new HighlightSpanEvent(item.hit.Start + item.hit.Length,item.index,false));
        }

        if(events.Count == 0)
            return Array.Empty<TextHighlightSpan>();

        events.Sort(static (a,b) => a.Position != b.Position
            ? a.Position.CompareTo(b.Position)
            : a.IsStart.CompareTo(b.IsStart));

        var active = new HashSet<int>();
        var output = new List<TextHighlightSpan>();
        int position = events[0].Position;
        int eventIndex = 0;

        while(eventIndex < events.Count)
        {
            int boundary = events[eventIndex].Position;
            if(boundary > position && active.Count > 0)
                AddHighlightSegment(output,evidence,active,position,boundary - position);

            while(eventIndex < events.Count && events[eventIndex].Position == boundary)
            {
                var evt = events[eventIndex++];
                if(evt.IsStart)
                    active.Add(evt.EvidenceIndex);
                else
                    active.Remove(evt.EvidenceIndex);
            }

            position = boundary;
        }

        return output;
    }

    private void AddHighlightSegment(
        List<TextHighlightSpan> output,
        IReadOnlyList<EvidenceHitWithCounts> evidence,
        HashSet<int> active,
        int start,
        int length)
    {
        if(length <= 0 || active.Count == 0)
            return;

        int maxSeverity = active.Max(index => evidence[index].SeverityScore);
        int detectionScore = active.Max(index => evidence[index].DetectionScore);
        int highlightScore = NormalizeSeverity(maxSeverity);
        bool hasScoringEvidence = active.Any(index => evidence[index].ContributesToRisk);
        var ruleKeys = active.Select(index => evidence[index].RuleKey)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static key => key,StringComparer.Ordinal)
            .ToArray();
        var categories = active.Select(index => evidence[index].Category)
            .Distinct()
            .OrderBy(static category => category)
            .ToArray();

        if(output.Count > 0)
        {
            var previous = output[output.Count - 1];
            if(previous.EndExclusive == start
                && previous.DetectionScore == detectionScore
                && previous.HighlightIntensity == highlightScore
                && previous.HighestDetectedSeverity == maxSeverity
                && previous.HasScoringEvidence == hasScoringEvidence
                && previous.RuleKeys.SequenceEqual(ruleKeys,StringComparer.Ordinal)
                && previous.Categories.SequenceEqual(categories))
            {
                output[output.Count - 1] = new TextHighlightSpan
                {
                    Start = previous.Start,
                    Length = previous.Length + length,
                    DetectionScore = detectionScore,
                    HighlightIntensity = highlightScore,
                    HighestDetectedSeverity = maxSeverity,
                    HasScoringEvidence = hasScoringEvidence,
                    RuleKeys = ruleKeys,
                    Categories = categories
                };
                return;
            }
        }

        output.Add(new TextHighlightSpan
        {
            Start = start,
            Length = length,
            DetectionScore = detectionScore,
            HighlightIntensity = highlightScore,
            HighestDetectedSeverity = maxSeverity,
            HasScoringEvidence = hasScoringEvidence,
            RuleKeys = ruleKeys,
            Categories = categories
        });
    }

    private static IReadOnlyList<TextHighlightSpan> LimitHighlightSpans(
        IReadOnlyList<TextHighlightSpan> spans,
        int? maximum)
    {
        if(maximum is null || spans.Count <= maximum.Value)
            return spans;
        if(maximum.Value == 0)
            return Array.Empty<TextHighlightSpan>();
        return spans.Take(maximum.Value).ToArray();
    }

}
