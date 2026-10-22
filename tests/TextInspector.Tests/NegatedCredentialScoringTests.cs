using Xunit;

namespace TextInspector.Tests;

public sealed class NegatedCredentialScoringTests
{
    private static TextInspectorEngine Create()
    {
        var options = TextInspectorPresets.LoadOptions();
        return TextInspectorEngine.CompileInternal(options,new[] {
            new TextInspectorRulePattern {
                TextInspectorRuleId = Guid.Parse("a4b59731-ffae-4a11-88bb-100000000001"),
                Key = "Credential_Test", Category = TextInspectorRuleCategory.Fraud,
                RegexPattern = @"\bsend\s+your\s+password\b",
                RuleWeight = 45, MaxCountForScoring = 5, DetectionScore = 90, SeverityScore = 8
            }
        });
    }
    [Theory]
    [InlineData("Please do not ever send your password.")]
    [InlineData("You should never ever send your password.")]
    [InlineData("Please don't ever send your password.")]
    public void LocalNegationKeepsCredentialEvidenceButDoesNotScore(string input)
    {
        var result = Create().Inspect(input,new TextInspectionOptions { IncludeNonScoringEvidence = true });
        Assert.Contains(result.Evidence,e => e.RuleKey == "Credential_Test" && !e.ContributesToRisk);
        Assert.Equal(0,result.RiskAssessment.TotalRiskPoints);
    }
    [Theory]
    [InlineData("Send your password now.")]
    [InlineData("The guide says not to delay; send your password.")]
    public void IndependentDirectRequestStillScores(string input)
    {
        var result = Create().Inspect(input);
        Assert.Contains(result.Evidence,e => e.RuleKey == "Credential_Test" && e.ContributesToRisk);
    }
}
