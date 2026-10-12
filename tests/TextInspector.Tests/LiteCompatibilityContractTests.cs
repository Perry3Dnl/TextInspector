using System.Text.Json;
using TextInspector;

namespace TextInspector.Tests;

/// <summary>
/// Lite keeps these DTOs for cross-edition report compatibility even though its
/// evaluator never emits URL intelligence or composite policy matches.
/// Ensure their serialized shape stays usable for shared UIs and upgrade paths.
/// </summary>
public sealed class LiteCompatibilityContractTests
{
    [Fact]
    public void UrlObservationRoundTripsEveryPublicMetadataField()
    {
        var original = new TextUrlObservation
        {
            Start = 3, Length = 19, MatchedText = "https://example.org",
            Scheme = "https", Host = "example.org", Port = 8443,
            PathAndQuery = "/login?next=home",
            Trust = TextUrlTrustClassification.Unknown,
            MatchedConfiguredDomain = "example.org",
            LooksLikeFirstParty = true, HasIpHost = true, HasPunycodeHost = true,
            HasUserInfo = true, HasUnusualPort = true, IsKnownShortener = true,
            SuspiciousPathTerms = new[] { "login" }
        };
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<TextUrlObservation>(json)!;
        Assert.Equal(3, restored.Start);
        Assert.Equal(22, restored.EndExclusive);
        Assert.Equal(original.MatchedText, restored.MatchedText);
        Assert.Equal(original.Scheme, restored.Scheme);
        Assert.Equal(original.Host, restored.Host);
        Assert.Equal(8443, restored.Port);
        Assert.Equal(original.PathAndQuery, restored.PathAndQuery);
        Assert.Equal(original.Trust, restored.Trust);
        Assert.Equal(original.MatchedConfiguredDomain, restored.MatchedConfiguredDomain);
        Assert.True(restored.LooksLikeFirstParty);
        Assert.True(restored.HasIpHost);
        Assert.True(restored.HasPunycodeHost);
        Assert.True(restored.HasUserInfo);
        Assert.True(restored.HasUnusualPort);
        Assert.True(restored.IsKnownShortener);
        Assert.Equal("login", Assert.Single(restored.SuspiciousPathTerms));
    }

    [Fact]
    public void PolicyEvidenceAndScoreContractsRoundTripWithoutRunningPolicies()
    {
        var id = Guid.NewGuid();
        var witness = new TextPolicyEvidenceReference
        {
            RuleId = id, RuleKey = "contact", Category = TextInspectorRuleCategory.ExternalContact,
            Start = 5, Length = 7, MatchedText = "contact",
            ContributesToRisk = true, ScoringStatus = TextEvidenceScoringDisposition.Counted
        };
        var original = new TextPolicyMatch
        {
            PolicyId = id, PolicyKey = "policy", Description = "test",
            Tags = new[] { "compat" }, Category = TextInspectorRuleCategory.Fraud,
            SeverityScore = 7, RiskPoints = 12, ConditionSummary = "contact",
            MatchedSelectors = new[] { "contact" }, Evidence = new[] { witness },
            WitnessStart = 5, WitnessLength = 7
        };
        var restored = JsonSerializer.Deserialize<TextPolicyMatch>(JsonSerializer.Serialize(original))!;
        Assert.Equal(id, restored.PolicyId);
        Assert.Equal("policy", restored.PolicyKey);
        Assert.Equal("test", restored.Description);
        Assert.Equal("compat", Assert.Single(restored.Tags));
        Assert.Equal(TextInspectorRuleCategory.Fraud, restored.Category);
        Assert.Equal(7, restored.SeverityScore);
        Assert.Equal(12, restored.RiskPoints);
        Assert.True(restored.ContributesToRisk);
        Assert.Equal("contact", restored.ConditionSummary);
        Assert.Equal("contact", Assert.Single(restored.MatchedSelectors));
        Assert.Equal(5, restored.WitnessStart);
        Assert.Equal(7, restored.WitnessLength);
        var evidence = Assert.Single(restored.Evidence);
        Assert.Equal(id, evidence.RuleId);
        Assert.Equal("contact", evidence.RuleKey);
        Assert.Equal(TextInspectorRuleCategory.ExternalContact, evidence.Category);
        Assert.Equal(5, evidence.Start);
        Assert.Equal(7, evidence.Length);
        Assert.Equal(12, evidence.EndExclusive);
        Assert.Equal("contact", evidence.MatchedText);
        Assert.True(evidence.ContributesToRisk);
        Assert.Equal(TextEvidenceScoringDisposition.Counted, evidence.ScoringStatus);

        var contribution = new TextScoreContribution
        {
            Kind = TextScoreContributionKind.BuiltInBonus,
            PolicyId = id, PolicyKey = "policy", Key = "bonus",
            Description = "Additional evidence", RiskPoints = 12
        };
        var roundTrip = JsonSerializer.Deserialize<TextScoreContribution>(JsonSerializer.Serialize(contribution))!;
        Assert.Equal(contribution.Kind, roundTrip.Kind);
        Assert.Equal(id, roundTrip.PolicyId);
        Assert.Equal("policy", roundTrip.PolicyKey);
        Assert.Equal("bonus", roundTrip.Key);
        Assert.Equal("Additional evidence", roundTrip.Description);
        Assert.Equal(12, roundTrip.RiskPoints);
    }

    [Fact]
    public void LightweightRuleMatchRemainsPortableAcrossEditions()
    {
        var original = new TextRuleMatch
        {
            RuleId = Guid.NewGuid(), RuleKey = "rule",
            Category = TextInspectorRuleCategory.Fraud,
            SeverityScore = 7, Priority = 30, DetectionScore = 91,
            HighlightIntensity = 67, Start = 6, Length = 4
        };
        var copy = JsonSerializer.Deserialize<TextRuleMatch>(JsonSerializer.Serialize(original))!;
        Assert.Equal(original.RuleId, copy.RuleId);
        Assert.Equal(original.RuleKey, copy.RuleKey);
        Assert.Equal(original.Category, copy.Category);
        Assert.Equal(7, copy.SeverityScore);
        Assert.Equal(30, copy.Priority);
        Assert.Equal(91, copy.DetectionScore);
        Assert.Equal(67, copy.HighlightIntensity);
        Assert.Equal(6, copy.Start);
        Assert.Equal(4, copy.Length);
        Assert.Equal(10, copy.EndExclusive);
    }
}
