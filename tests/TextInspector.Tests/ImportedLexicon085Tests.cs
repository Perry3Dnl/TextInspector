using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace TextInspector.Tests;

public sealed class ImportedLexicon085Tests
{
    private static readonly Lazy<TextInspectorEngine> Engine = new(() => TextInspectorEngine.Create());

    [Theory]
    [InlineData("airhead", "Lexicon_Profanity_EN")]
    [InlineData("achterlijk", "Lexicon_Profanity_NL")]
    public void ImportedTermsProduceExactEvidenceWithoutRisk(string text, string ruleKey)
    {
        var result = Engine.Value.Inspect("prefix " + text + " suffix", new TextInspectionOptions
        {
            IncludeHighlightSpans = true,
            IncludeSourceLocations = true
        });
        var evidence = Assert.Single(result.Evidence, e => e.RuleKey == ruleKey);
        Assert.Equal(text, evidence.MatchedText);
        Assert.Equal(7, evidence.Start);
        Assert.Equal(text.Length, evidence.Length);
        Assert.Equal(TextInspectorRuleCategory.AbusiveLanguage, evidence.Category);
        Assert.False(evidence.ContributesToRisk);
        Assert.Equal(0, evidence.RuleWeight);
        Assert.Contains(result.HighlightSpans, highlight => highlight.Start == evidence.Start && highlight.Length == evidence.Length);
    }

    [Fact]
    public void ImportedLexiconsAreCompleteAndMatchAllRecordedSourceEntries()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "lexicon-085-fixtures.json");
        using var fixture = JsonDocument.Parse(File.ReadAllText(file));
        var rules = TextInspectorPresets.LoadRules().ToDictionary(r => r.Key, StringComparer.Ordinal);
        var languageCount = 0;
        var termCount = 0;

        foreach(var language in fixture.RootElement.GetProperty("languages").EnumerateObject())
        {
            languageCount++;
            var key = "Lexicon_Profanity_" + language.Name.ToUpperInvariant();
            var rule = rules[key];
            Assert.Equal(TextInspectorRuleCategory.AbusiveLanguage, rule.Category);
            Assert.Equal(0, rule.MaxCountForScoring);
            Assert.Equal(0, rule.RuleWeight);
            var regex = new Regex(rule.RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));

            foreach(var termElement in language.Value.EnumerateArray())
            {
                var term = termElement.GetString()!;
                termCount++;
                Assert.True(regex.IsMatch(term), $"Missing {language.Name} lexicon entry: {term}");
            }
        }

        Assert.Equal(2, languageCount);
        Assert.Equal(602, termCount);

        var hate = rules["Lexicon_HateSpeech_Expanded"];
        Assert.Equal(0, hate.MaxCountForScoring);
        Assert.Equal(0, hate.RuleWeight);
        var hateRegex = new Regex(hate.RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
        var hateTerms = fixture.RootElement.GetProperty("hateSpeech").EnumerateArray().ToArray();
        Assert.Equal(20, hateTerms.Length);
        foreach(var value in hateTerms)
            Assert.True(hateRegex.IsMatch(value.GetString()!), "Missing supplementary hate-speech lexicon expression.");
    }

    [Fact]
    public void LexiconDoesNotFlagEmbeddedOrdinaryIdentifiers()
    {
        Assert.DoesNotContain(Engine.Value.FindMatches("prefixairheadsuffix"),
            match => match.RuleKey == "Lexicon_Profanity_EN");
    }

    [Fact]
    public void ImportedLexiconOnlyDetectionDoesNotCreateRisk()
    {
        var result = Engine.Value.Inspect("prefix airhead suffix", new TextInspectionOptions { IncludeNonScoringEvidence = true });
        Assert.Equal(0, result.RiskAssessment.RuleRiskPoints);
        Assert.Equal(0, result.RiskAssessment.TotalRiskPoints);
        Assert.Equal(0, result.RiskAssessment.RiskScore);
    }
}
