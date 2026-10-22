using TextInspector;

namespace TextInspector.Tests;

public sealed class EditionCompatibilityTests
{
    [Fact]
    public void FreeExposesProStyleAnalysisAndMarkupWithoutChangingInspection()
    {
        var inspector = new TextInspectorService();
        var text = "hello@example.com";
        var ordinary = inspector.Inspect(text,includeMarkedHtml: true);
        Assert.Equal(ordinary.RiskAssessment.RiskScore,inspector.Analyze(text).RiskScore);
        Assert.Equal(ordinary.MarkedHtml ?? "",inspector.MarkText(text));
    }

    [Fact]
    public async Task FreeRepositoryRefreshReportsProUpgradeRequirement()
    {
        var service = new TextInspectorService();
        var error = await Assert.ThrowsAsync<NotSupportedException>(() => service.ReloadAsync());
        Assert.Contains("TextInspector.Pro",error.Message);
    }
    [Fact]
    public void ProOnlyBatchApiIsVisibleButFailsClearlyInFree()
    {
        var service = new TextInspectorService();
        var engine = TextInspectorEngine.Create();
        var sample = new string?[] { "sample" };
        Assert.Contains("TextInspector.Pro",
            Assert.Throws<NotSupportedException>(() => service.InspectManyParallel(sample)).Message);
        Assert.Contains("TextInspector.Pro",
            Assert.Throws<NotSupportedException>(() => engine.InspectManyParallel(sample)).Message);
    }

    [Fact]
    public void ProCompatibleOutputOptionsDoNotChangeFreeInspection()
    {
        var engine = TextInspectorEngine.Create();
        const string text = "hello@example.com";
        var baseline = engine.Inspect(text);
        var compatible = engine.Inspect(text, new TextInspectionOptions
        {
            IncludePolicyDetails = true,
            IncludeUrlDetails = true,
            MaxReturnedPolicyMatches = 0
        });
        Assert.Equal(baseline.RiskAssessment.RiskScore,compatible.RiskAssessment.RiskScore);
        Assert.Equal(baseline.Evidence.Count,compatible.Evidence.Count);
    }

    [Fact]
    public void FilteredMatchApiRejectsPaidOperationsWithoutSuppressingNormalInspection()
    {
        var filter = new TextRuleMatchFilter
        {
            RuleIds = new[] { Guid.NewGuid() },
            RuleKeys = new[] { "demo" },
            Categories = new[] { TextInspectorRuleCategory.PlatformIntegrity },
            Tags = new[] { "demo" },
            RequireAllTags = true,
            MinimumSeverityScore = 3,
            MinimumPriority = 1
        };
        Assert.Single(filter.RuleIds);
        Assert.Single(filter.RuleKeys);
        Assert.Single(filter.Categories);
        Assert.Single(filter.Tags);
        Assert.True(filter.RequireAllTags);
        Assert.Equal(3,filter.MinimumSeverityScore);
        Assert.Equal(1,filter.MinimumPriority);
        var service = new TextInspectorService();
        var engine = TextInspectorEngine.Create();
        Assert.Throws<NotSupportedException>(() => service.HasMatch("hello",filter));
        Assert.Throws<NotSupportedException>(() => service.FindFirstMatch("hello",filter));
        Assert.Throws<NotSupportedException>(() => service.FindMatches("hello",filter));
        Assert.Throws<NotSupportedException>(() => engine.HasMatch("hello",filter));
        Assert.Throws<NotSupportedException>(() => engine.FindFirstMatch("hello",filter));
        Assert.Throws<NotSupportedException>(() => engine.FindMatches("hello",filter));
        Assert.NotNull(service.Inspect("hello"));
    }

    [Fact]
    public void ProCompatibleDtoFieldsRoundTripWithoutActivatingPaidFeatures()
    {
        var options = new TextInspectorScoringOptions
        {
            MaxPolicyConditionDepth = 20,
            MaxPolicyConditionNodes = 300,
            UrlAnalysis = new TextInspectorUrlOptions
            {
                Enabled = false,
                FirstPartyDomains = new[] { "example.org" },
                TrustedDomains = new[] { "example.com" },
                ShortenerDomains = new[] { "short.test" },
                SuspiciousPathTerms = new[] { "login" }
            },
            Masking = new TextInspectorMaskingOptions
            {
                MinimumDetectionScore = 4,
                Categories = new[] { TextInspectorRuleCategory.PlatformIntegrity },
                RuleKeys = new[] { "rule" },
                Tags = new[] { "tag" },
                RequireAllTags = true,
                ScoredEvidenceOnly = true
            }
        };
        Assert.Equal(20,options.MaxPolicyConditionDepth);
        Assert.Equal(300,options.MaxPolicyConditionNodes);
        Assert.False(options.UrlAnalysis.Enabled);
        Assert.Single(options.UrlAnalysis.FirstPartyDomains);
        Assert.Single(options.UrlAnalysis.TrustedDomains);
        Assert.Single(options.UrlAnalysis.ShortenerDomains);
        Assert.Single(options.UrlAnalysis.SuspiciousPathTerms);
        Assert.Equal(4,options.Masking.MinimumDetectionScore);
        Assert.Single(options.Masking.Categories);
        Assert.Single(options.Masking.RuleKeys);
        Assert.Single(options.Masking.Tags);
        Assert.True(options.Masking.RequireAllTags);
        Assert.True(options.Masking.ScoredEvidenceOnly);
    }

    [Fact]
    public async Task UnavailableConfigurationOperationsGiveUpgradeGuidance()
    {
        var service = new TextInspectorService();
        Assert.NotNull(service.CompilationDiagnostics);
        await Assert.ThrowsAsync<NotSupportedException>(() => service.GetOptionsAsync());
        await Assert.ThrowsAsync<NotSupportedException>(() => service.GetAllRulesAsync());
        await Assert.ThrowsAsync<NotSupportedException>(() => service.GetAllPoliciesAsync());
        var options = new TextInspectorScoringOptions();
        var rules = Array.Empty<TextInspectorRulePattern>();
        var policies = Array.Empty<TextInspectorPolicy>();
        await Assert.ThrowsAsync<NotSupportedException>(() => service.SaveAsync(options,rules,Guid.Empty));
        await Assert.ThrowsAsync<NotSupportedException>(() => service.SaveAsync(options,rules,policies,Guid.Empty));
    }

    [Fact]
    public void ProCompatiblePolicyMetadataRoundTripsWithoutRunningPolicies()
    {
        var policy = new TextInspectorPolicy
        {
            TextInspectorPolicyId = Guid.NewGuid(),
            Key = "testing",
            Description = "metadata",
            Tags = new[] { "compat" },
            Category = TextInspectorRuleCategory.PlatformIntegrity,
            SeverityScore = 6,
            RiskPoints = 15,
            Condition = TextInspectorPolicyConditions.AllOf(
                TextInspectorPolicyConditions.Rule("one"),
                TextInspectorPolicyConditions.Category(TextInspectorRuleCategory.PlatformIntegrity)),
            IsEnabled = true
        };
        var json = System.Text.Json.JsonSerializer.Serialize(policy);
        var restored = System.Text.Json.JsonSerializer.Deserialize<TextInspectorPolicy>(json)!;
        Assert.Equal(policy.TextInspectorPolicyId,restored.TextInspectorPolicyId);
        Assert.Equal("testing",restored.Key);
        Assert.Equal("metadata",restored.Description);
        Assert.Single(restored.Tags);
        Assert.Equal(TextInspectorRuleCategory.PlatformIntegrity,restored.Category);
        Assert.Equal(6,restored.SeverityScore);
        Assert.Equal(15,restored.RiskPoints);
        Assert.True(restored.IsEnabled);
        Assert.Equal(2,restored.Condition.Conditions.Length);
        Assert.Throws<NotSupportedException>(() => TextInspectorPresets.LoadPolicies());
        Assert.Throws<NotSupportedException>(() => TextInspectorPresets.WriteMissing("test-directory"));
    }

}
