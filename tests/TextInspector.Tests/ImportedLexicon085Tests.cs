using System.Linq;
using Xunit;

namespace TextInspector.Tests;

public sealed class ImportedLexicon085Tests
{
    [Theory]
    [InlineData("airhead", "Lexicon_Profanity_EN")]
    [InlineData("achterlijk", "Lexicon_Profanity_NL")]
    public void ImportedTermsProduceEvidenceWithoutScoring(string input, string expectedRule)
    {
        var inspector = TextInspectorEngine.Create();
        Assert.Contains(inspector.FindMatches(input), match => match.RuleKey == expectedRule);
        var result = inspector.Inspect(input);
        Assert.Contains(result.Evidence, hit => hit.RuleKey == expectedRule && !hit.CountsForScoring);
    }

    [Fact]
    public void LexiconDoesNotFlagEmbeddedOrdinaryIdentifiers()
    {
        var inspector = TextInspectorEngine.Create();
        Assert.DoesNotContain(inspector.FindMatches("prefixairheadsuffix"),
            match => match.RuleKey == "Lexicon_Profanity_EN");
    }
}
