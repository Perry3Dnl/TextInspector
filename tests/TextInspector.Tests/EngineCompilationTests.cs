using TextInspector;

namespace TextInspector.Tests;

public sealed class EngineCompilationTests
{
    [Fact]
    public void PresetsCompileWithoutStructuralErrors()
    {
        var result = TextInspectorEngine.CompileWithDiagnosticsInternal(
            TextInspectorPresets.LoadOptions(),
            TextInspectorPresets.LoadRules());

        Assert.True(result.Engine.CompiledRuleCount > 0);
        Assert.DoesNotContain(result.Diagnostics, d =>
            d.Kind is TextInspectorDiagnosticKind.InvalidRegex
                or TextInspectorDiagnosticKind.InvalidContextRegex
                or TextInspectorDiagnosticKind.InvalidEvidenceGroup
                or TextInspectorDiagnosticKind.ContextReferenceNotFound);
    }

    [Fact]
    public void DuplicateRuleIdsAreRejected()
    {
        var id = Guid.NewGuid();
        var result = TextInspectorEngine.CompileWithDiagnosticsInternal(
            new TextInspectorScoringOptions(),
            new[]
            {
                Rule(id, "first", "alpha"),
                Rule(id, "second", "beta")
            });

        Assert.Equal(1, result.Engine.CompiledRuleCount);
        Assert.Contains(result.Diagnostics, d =>
            d.Kind == TextInspectorDiagnosticKind.RuleSkipped
            && d.Message.Contains("Duplicate rule ID", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicateRuleKeysAreRejectedCaseInsensitively()
    {
        var result = TextInspectorEngine.CompileWithDiagnosticsInternal(
            new TextInspectorScoringOptions(),
            new[]
            {
                Rule(Guid.NewGuid(), "SharedKey", "alpha"),
                Rule(Guid.NewGuid(), "sharedkey", "beta")
            });

        Assert.Equal(1, result.Engine.CompiledRuleCount);
        Assert.Contains(result.Diagnostics, d =>
            d.Kind == TextInspectorDiagnosticKind.RuleSkipped
            && d.Message.Contains("Duplicate rule key", StringComparison.Ordinal));
    }

    [Fact]
    public void InvalidEvidenceGroupSkipsRule()
    {
        var rule = Rule(Guid.NewGuid(), "bad-group", "(?<valid>alpha)");
        rule.EvidenceGroup = "missing";

        var result = TextInspectorEngine.CompileWithDiagnosticsInternal(
            new TextInspectorScoringOptions(),
            new[] { rule });

        Assert.Equal(0, result.Engine.CompiledRuleCount);
        Assert.Contains(result.Diagnostics, d => d.Kind == TextInspectorDiagnosticKind.InvalidEvidenceGroup);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void DetectionScoreOutsideSupportedRangeSkipsRule(int detectionScore)
    {
        var rule = Rule(Guid.NewGuid(),"invalid-detection-score","alpha");
        rule.DetectionScore = detectionScore;

        var result = TextInspectorEngine.CompileWithDiagnosticsInternal(
            new TextInspectorScoringOptions(),
            new[] { rule });

        Assert.Equal(0,result.Engine.CompiledRuleCount);
        Assert.Contains(result.Diagnostics,d =>
            d.Kind == TextInspectorDiagnosticKind.RuleSkipped
            && d.Message.Contains("DetectionScore",StringComparison.Ordinal));
    }

    [Fact]
    public void InvalidScoringOptionsFailFast()
    {
        var options = new TextInspectorScoringOptions
        {
            ScoreMin = 100,
            ScoreMax = 100
        };

        Assert.Throws<InvalidOperationException>(() =>
            TextInspectorEngine.CompileInternal(options, Array.Empty<TextInspectorRulePattern>()));
    }

    private static TextInspectorRulePattern Rule(Guid id, string key, string pattern)
        => new()
        {
            TextInspectorRuleId = id,
            Key = key,
            Category = TextInspectorRuleCategory.Fraud,
            SeverityScore = 5,
            RuleWeight = 10,
            RegexPattern = pattern
        };
}
