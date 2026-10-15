using Xunit;

namespace TextInspector.Tests;

/// <summary>Lexicon collisions and literal token boundaries, evaluated per shipped language rule.</summary>
public sealed class LanguageLexiconBoundaryTests
{
    private static readonly Lazy<TextInspectorEngine> Inspector = new(() => TextInspectorEngine.Create());
    public static IEnumerable<object[]> Languages()
    {
        yield return new object[] { "EN", "featherbrains" };
        yield return new object[] { "EN", "scatterbrains" };
        yield return new object[] { "EN", "bollockbrain" };
        yield return new object[] { "EN", "chowderheads" };
        yield return new object[] { "NL", "achterlijkerds" };
        yield return new object[] { "NL", "schijtlijsters" };
        yield return new object[] { "NL", "achterlijkerd" };
        yield return new object[] { "NL", "ellendelingen" };
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void RealTermIsDetectedButDoesNotIndependentlyIncreaseRisk(string language,string term)
    {
        var result = Inspector.Value.Inspect("Sample: " + term + "!",new TextInspectionOptions { IncludeNonScoringEvidence = true });
        string key = "Lexicon_Profanity_" + language;
        Assert.Contains(result.Evidence,e => e.RuleKey == key && e.MatchedText == term);
        Assert.DoesNotContain(result.Evidence,e => e.RuleKey == key && e.ContributesToRisk);
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void EmbeddedTermWithinLongerUnicodeIdentifierDoesNotMatch(string language,string term)
    {
        string key = "Lexicon_Profanity_" + language;
        var variants = new[] { "alpha" + term, term + "omega", "x_" + term, term + "_x", "3" + term, term + "3" };
        foreach(var text in variants)
        {
            var result = Inspector.Value.Inspect(text,new TextInspectionOptions { IncludeNonScoringEvidence = true });
            Assert.DoesNotContain(result.Evidence,e => e.RuleKey == key && e.MatchedText == term);
        }
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void RealTermPreservesExactOriginalOffsets(string language,string term)
    {
        const string prefix = "Reference: ";
        var result = Inspector.Value.Inspect(prefix + term + " end",new TextInspectionOptions { IncludeNonScoringEvidence = true });
        var hit = Assert.Single(result.Evidence,e => e.RuleKey == "Lexicon_Profanity_" + language && e.MatchedText == term);
        Assert.Equal(prefix.Length,hit.Start);
        Assert.Equal(term.Length,hit.Length);
    }
}
