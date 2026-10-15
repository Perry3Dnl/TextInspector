using System.Text.Json;
using Xunit;

namespace TextInspector.Tests;

/// <summary>
/// Deterministic multilingual lexical calibration harness. The checked-in JSON
/// explicitly identifies these as synthetic, unreviewed boundary cases, NOT
/// human-validated semantic accuracy data.
/// </summary>
public sealed class MultilingualAccuracyCorpusTests
{
    public sealed class LabeledCase
    {
        public string Id { get; set; } = "";
        public string Language { get; set; } = "";
        public string Split { get; set; } = "";
        public string Text { get; set; } = "";
        public string RuleKey { get; set; } = "";
        public bool ExpectedDetection { get; set; }
        public bool ExpectedScoring { get; set; }
        public string? ExpectedEvidence { get; set; }
        public string Origin { get; set; } = "";
        public string ReviewStatus { get; set; } = "";
    }

    private static readonly Lazy<TextInspectorEngine> Engine = new(() =>
        TextInspectorEngine.Create());

    private static readonly Lazy<LabeledCase[]> Corpus = new(() =>
        JsonSerializer.Deserialize<LabeledCase[]>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"accuracy-corpus-free.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("Accuracy corpus is empty."));

    public static IEnumerable<object[]> Cases() =>
        Corpus.Value.Select(item => new object[] { item });

    [Fact]
    public void CorpusIsCompleteAndLabelsAreFrozen()
    {
        var items = Corpus.Value;
        Assert.Equal(200,items.Length);
        Assert.Equal(200,items.Select(x => x.Id).Distinct().Count());
        Assert.Equal(2,items.Select(x => x.Language).Distinct().Count());
        Assert.All(items.GroupBy(x => x.Language),group =>
        {
            Assert.Equal(100,group.Count());
            Assert.Equal(80,group.Count(x => x.Split == "development"));
            Assert.Equal(20,group.Count(x => x.Split == "evaluation"));
        });
        Assert.All(items,x =>
        {
            Assert.Equal("synthetic-boundary",x.Origin);
            Assert.Equal("unreviewed",x.ReviewStatus);
            Assert.True(x.ExpectedDetection == (x.ExpectedEvidence is not null));
        });
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void RuleDetectionAndOriginalSpanMatchFrozenLabels(LabeledCase item)
    {
        var result = Engine.Value.Inspect(item.Text,
            new TextInspectionOptions { IncludeNonScoringEvidence = true });
        var hits = result.Evidence.Where(e => e.RuleKey == item.RuleKey).ToArray();
        var matching = hits.Where(e => item.ExpectedEvidence is not null &&
            string.Equals(e.MatchedText,item.ExpectedEvidence,StringComparison.Ordinal)).ToArray();
        Assert.True(item.ExpectedDetection ? matching.Length > 0 : hits.Length == 0,
            $"{item.Id} ({item.Language}/{item.Split}) expected={item.ExpectedDetection}; hits={string.Join(",",hits.Select(x => x.MatchedText))}");
        foreach(var hit in matching)
        {
            Assert.InRange(hit.Start,0,item.Text.Length);
            Assert.InRange(hit.Length,1,item.Text.Length-hit.Start);
            Assert.Equal(item.ExpectedEvidence,item.Text.Substring(hit.Start,hit.Length));
            Assert.Equal(item.ExpectedScoring,hit.ContributesToRisk);
        }
    }
}
