using System.Reflection;
using System.Text.Json;
using TextInspector;

namespace TextInspector.Tests;

public sealed class PresetEditionTests
{
    [Fact]
    public void ShippedPresetPreservesEveryBaselineAndBasicRule()
    {
        var rules = TextInspectorDefaults.Rules.Select(rule => rule.Key).ToArray();
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "preset-manifest.json")));
        var expected = manifest.RootElement.GetProperty("baselineRuleKeys").EnumerateArray()
            .Concat(manifest.RootElement.GetProperty("basicRuleKeys").EnumerateArray()).Select(value => value.GetString()).ToArray();
        Assert.Equal(70, manifest.RootElement.GetProperty("baselineRuleKeys").GetArrayLength());
        Assert.Equal(79, rules.Length);
        Assert.Equal(expected.OrderBy(key => key), rules.Where(key => !key.StartsWith("Lexicon_", StringComparison.Ordinal)).OrderBy(key => key));
        Assert.Contains("Lexicon_Profanity_EN", rules);
        Assert.Contains("Lexicon_Profanity_NL", rules);
        Assert.Contains("Lexicon_HateSpeech_Expanded", rules);
        Assert.DoesNotContain("Credential_SeedPhrase_Request", rules);
        Assert.DoesNotContain("Secret_AWS_AccessKey", rules);
    }

    [Fact]
    public void PublicCompatibilityDtosDoNotEnableProImplementations()
    {
        var assembly = typeof(TextInspectorEngine).Assembly;
        var names = assembly.GetExportedTypes().Select(type => type.Name).ToArray();
        Assert.Contains("TextInspectorRulePattern", names);
        Assert.Contains("TextInspectorRuleContext", names);
        Assert.DoesNotContain("TextInspectorPresets", names);
        Assert.DoesNotContain(assembly.GetTypes(), type => type.IsClass && (type.Name.Contains("Repository") || type.Name.Contains("UrlAnalyzer") || type.Name.Contains("RuntimePolicy")));
        var methods = typeof(TextInspectorEngine).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        Assert.DoesNotContain(methods, method => method.Name is "Compile" or "CompileWithDiagnostics" or "FindFirstMatch" or "InspectManyParallel");
        Assert.DoesNotContain(assembly.GetManifestResourceNames(), name => name.EndsWith("policies.json"));
    }

    [Fact]
    public void SupportingContactDetectionDoesNotBecomeRisk()
    {
        const string text = "Contact us at hello@example.com.";
        var report = TextInspectorEngine.Create().CreateReport(text);
        Assert.True(report.HasDetections);
        Assert.Equal(0, report.RiskAssessment.TotalRiskPoints);
        Assert.False(report.RequiresReview);
        Assert.Empty(report.Findings);
        Assert.Contains(report.SupportingEvidence, evidence => evidence.RuleKey == "PII_Email_Address" && text.Substring(evidence.Start, evidence.Length) == "hello@example.com");
    }

    [Fact]
    public void ProMetadataRemainsEmptyAndScoresRemainTransparent()
    {
        var result = TextInspectorEngine.Create().Inspect("Official support team says send me the verification code at https://example.org/login");
        Assert.Empty(result.PolicyMatches);
        Assert.Empty(result.UrlObservations);
        Assert.Equal(0, result.RiskAssessment.PolicyRiskPoints);
        Assert.Equal(result.RiskAssessment.RuleRiskPoints + result.RiskAssessment.BuiltInBonusPoints, result.RiskAssessment.TotalRiskPoints);
    }

    [Fact]
    public void PhishingSupportIsVisibleWithoutPretendingToEvaluateProPolicies()
    {
        const string text = "Official support team says verify your bank account and send me the verification code.";
        var report = TextInspectorEngine.Create().CreateReport(text);
        Assert.True(report.HasDetections);
        Assert.Contains(report.SupportingEvidence, evidence => evidence.RuleKey == "Fraud_Staff_Impersonation");
        Assert.Contains("supporting signal", report.Summary);
        Assert.Empty(report.Findings);
        Assert.Equal(0, report.RiskAssessment.PolicyRiskPoints);
    }

    [Fact]
    public void DisableExistingRulesAndSnapshotOptionsAtCreation()
    {
        var options = new TextInspectorOptions { DisabledRuleKeys = new[] { "PII_Email_Address" } };
        var engine = TextInspectorEngine.Create(options);
        options.DisabledRuleKeys[0] = "Threat_Violence";
        var result = engine.Inspect("hello@example.com");
        Assert.DoesNotContain(result.Evidence, evidence => evidence.RuleKey == "PII_Email_Address");
        Assert.Equal(78, engine.CompiledRuleCount);
        Assert.Contains(engine.Inspect("I'm going to kill you.").Evidence, evidence => evidence.RuleKey == "Threat_Violence");
    }

    [Fact]
    public void DisableCategoryKeepsUnrelatedDetection()
    {
        var engine = TextInspectorEngine.Create(new TextInspectorOptions { DisabledCategories = new[] { TextInspectorRuleCategory.ExternalContact } });
        Assert.DoesNotContain(engine.Inspect("Contact hello@example.com on WhatsApp").Evidence, evidence => evidence.Category == TextInspectorRuleCategory.ExternalContact);
        Assert.True(engine.HasMatch("I'm going to kill you."));
    }

    [Fact]
    public void BasicMaskingPreservesLengthOffsetsAndRisk()
    {
        const string text = "Contact hello@example.com.";
        var plain = TextInspectorEngine.Create().Inspect(text);
        var masked = TextInspectorEngine.Create(new TextInspectorOptions { EnableMasking = true }).Inspect(text);
        Assert.NotNull(masked.MaskedText);
        Assert.Equal(text.Length, masked.MaskedText!.Length);
        Assert.DoesNotContain("hello@example.com", masked.MaskedText);
        Assert.Equal(plain.RiskAssessment.TotalRiskPoints, masked.RiskAssessment.TotalRiskPoints);
        Assert.Equal(plain.Evidence.Select(e => (e.Start,e.Length)), masked.Evidence.Select(e => (e.Start,e.Length)));
    }

    [Fact]
    public void ConfigurationRejectsUnknownRulesAndInvalidMaskCharacters()
    {
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(new TextInspectorOptions { DisabledRuleKeys = new[] { "custom-rule" } }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(new TextInspectorOptions { DisabledCategories = new[] { (TextInspectorRuleCategory)999 } }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(new TextInspectorOptions { MaskCharacter = '\n' }));
        Assert.Throws<ArgumentException>(() => TextInspectorEngine.Create(new TextInspectorOptions { MaskCharacter = '\ud800' }));
    }

    [Fact]
    public void HidingFindingsDoesNotChangeTheRiskVerdict()
    {
        const string text = "I'm going to kill you.";
        var engine = TextInspectorEngine.Create();
        var full = engine.CreateReport(text);
        var hidden = engine.CreateReport(text, new TextInspectionReportOptions { MaxFindings = 0 });
        Assert.True(full.RequiresReview);
        Assert.Empty(hidden.Findings);
        Assert.Equal(full.RiskLevel, hidden.RiskLevel);
        Assert.Equal(full.RequiresReview, hidden.RequiresReview);
        Assert.Equal(full.RiskAssessment.TotalRiskPoints, hidden.RiskAssessment.TotalRiskPoints);
        Assert.Equal(full.Summary, hidden.Summary);
    }

    [Fact]
    public void ReportsKeepStableFindingsAndExactHighlights()
    {
        const string text = "I’m going to kill you.";
        var engine = TextInspectorEngine.Create();
        var first = engine.CreateReport(text);
        var second = engine.CreateReport(text);
        Assert.NotEmpty(first.Findings);
        Assert.All(first.Findings, finding => Assert.Equal(TextInspectionFindingKind.Category, finding.FindingType));
        Assert.Equal(first.Findings.Select(f => f.FindingId), second.Findings.Select(f => f.FindingId));
        Assert.All(first.Highlights, span => Assert.Equal(text.Substring(span.Start, span.Length), span.MatchedText));
    }
}
