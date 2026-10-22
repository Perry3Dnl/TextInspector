using TextInspector;

namespace TextInspector.Tests;

/// <summary>Portable data contracts are usable in Free even though Pro execution is unavailable.</summary>
public sealed class ProMetadataContractCoverageTests
{
    [Fact]
    public void PolicyConditionFactoriesPreserveTheirPublicConfiguration()
    {
        var rule = TextInspectorPolicyConditions.Rule("PII_Email_Address", scoredOnly: false, minimumOccurrences: 2);
        Assert.Equal(TextInspectorPolicyConditionKind.Rule, rule.Kind);
        Assert.Equal("PII_Email_Address", rule.RuleKey);
        Assert.False(rule.ScoredOnly);
        Assert.Equal(2, rule.MinimumOccurrences);

        var category = TextInspectorPolicyConditions.Category(TextInspectorRuleCategory.Fraud,
            scoredOnly: false, minimumOccurrences: 3);
        Assert.Equal(TextInspectorPolicyConditionKind.Category, category.Kind);
        Assert.Equal(TextInspectorRuleCategory.Fraud, category.Category);
        Assert.False(category.ScoredOnly);
        Assert.Equal(3, category.MinimumOccurrences);

        var all = TextInspectorPolicyConditions.AllOf(rule, category);
        var any = TextInspectorPolicyConditions.AnyOf(rule, category);
        var atLeast = TextInspectorPolicyConditions.AtLeast(2, rule, category);
        var proximity = TextInspectorPolicyConditions.Proximity(100, rule, category);
        Assert.Equal(TextInspectorPolicyConditionKind.AllOf, all.Kind);
        Assert.Equal(TextInspectorPolicyConditionKind.AnyOf, any.Kind);
        Assert.Equal(TextInspectorPolicyConditionKind.AtLeast, atLeast.Kind);
        Assert.Equal(2, atLeast.MinimumSatisfied);
        Assert.Equal(TextInspectorPolicyConditionKind.Proximity, proximity.Kind);
        Assert.Equal(100, proximity.WindowCharacters);
        Assert.Equal(2, all.Conditions.Length);
        Assert.Equal(2, any.Conditions.Length);
        Assert.Equal(2, proximity.Conditions.Length);
        Assert.Empty(TextInspectorPolicyConditions.AllOf(null!).Conditions);
        Assert.Empty(TextInspectorPolicyConditions.AnyOf(null!).Conditions);
        Assert.Empty(TextInspectorPolicyConditions.AtLeast(1, null!).Conditions);
        Assert.Empty(TextInspectorPolicyConditions.Proximity(1, null!).Conditions);
    }

    [Fact]
    public void UrlSignalKeysArePublicWithoutEnablingUrlAnalysis()
    {
        Assert.Equal(8, TextInspectorUrlSignalKeys.All.Count);
        Assert.Contains(TextInspectorUrlSignalKeys.ExternalDomain, TextInspectorUrlSignalKeys.All);
        Assert.Contains(TextInspectorUrlSignalKeys.FirstPartyLookalike, TextInspectorUrlSignalKeys.All);
        Assert.Contains(TextInspectorUrlSignalKeys.IpHost, TextInspectorUrlSignalKeys.All);
        Assert.Contains(TextInspectorUrlSignalKeys.PunycodeHost, TextInspectorUrlSignalKeys.All);
        Assert.Contains(TextInspectorUrlSignalKeys.UserInfo, TextInspectorUrlSignalKeys.All);
        Assert.Contains(TextInspectorUrlSignalKeys.UnusualPort, TextInspectorUrlSignalKeys.All);
        Assert.Contains(TextInspectorUrlSignalKeys.LoginOrVerificationPath, TextInspectorUrlSignalKeys.All);
        Assert.Contains(TextInspectorUrlSignalKeys.KnownShortener, TextInspectorUrlSignalKeys.All);
        Assert.Empty(TextInspectorEngine.Create().Inspect("https://example.org").UrlObservations);
    }

    [Fact]
    public void MatchFilterAndPolicyDtosRetainConfigurationMetadata()
    {
        var id = Guid.NewGuid();
        var filter = new TextRuleMatchFilter
        {
            RuleIds = new[] { id },
            RuleKeys = new[] { "PII_Email_Address" },
            Categories = new[] { TextInspectorRuleCategory.ExternalContact },
            Tags = new[] { "sample" },
            RequireAllTags = true,
            MinimumSeverityScore = 2,
            MinimumPriority = 3
        };
        Assert.Equal(id, Assert.Single(filter.RuleIds));
        Assert.Equal("PII_Email_Address", Assert.Single(filter.RuleKeys));
        Assert.Equal(TextInspectorRuleCategory.ExternalContact, Assert.Single(filter.Categories));
        Assert.Equal("sample", Assert.Single(filter.Tags));
        Assert.True(filter.RequireAllTags);
        Assert.Equal(2, filter.MinimumSeverityScore);
        Assert.Equal(3, filter.MinimumPriority);

        var condition = TextInspectorPolicyConditions.Rule("PII_Email_Address");
        var policy = new TextInspectorPolicy
        {
            TextInspectorPolicyId = id,
            Key = "example",
            Description = "Example only",
            Tags = new[] { "example" },
            Category = TextInspectorRuleCategory.Fraud,
            SeverityScore = 5,
            RiskPoints = 8,
            Condition = condition,
            IsEnabled = false
        };
        Assert.Equal(id, policy.TextInspectorPolicyId);
        Assert.Equal("example", policy.Key);
        Assert.Equal("Example only", policy.Description);
        Assert.Equal("example", Assert.Single(policy.Tags));
        Assert.Equal(TextInspectorRuleCategory.Fraud, policy.Category);
        Assert.Equal(5, policy.SeverityScore);
        Assert.Equal(8, policy.RiskPoints);
        Assert.Same(condition, policy.Condition);
        Assert.False(policy.IsEnabled);
    }
}
