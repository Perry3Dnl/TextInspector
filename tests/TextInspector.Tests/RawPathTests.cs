using TextInspector;

namespace TextInspector.Tests;

public sealed class RawPathTests
{
    private static TextInspectorRulePattern Rule(string key, string regex) => new()
    {
        TextInspectorRuleId = Guid.NewGuid(), Key = key,
        RegexPattern = regex, Category = TextInspectorRuleCategory.Fraud,
        SeverityScore = 7, RuleWeight = 20, Priority = 30
    };

    [Fact]
    public void FindMatchesReturnsOriginalCoordinatesAndPreservesRuleMetadata()
    {
        var rule = Rule("captured", @"(?<capture>target)");
        rule.EvidenceGroup = "capture";
        rule.DetectionScore = 68;
        var engine = TextInspectorEngine.CompileInternal(new TextInspectorScoringOptions(), new[] { rule });
        const string input = "ＦＯＯ\u200b TARGET and target";
        var matches = engine.FindMatches(input);
        Assert.Equal(2, matches.Count);
        var first = matches[0];
        Assert.Equal(rule.TextInspectorRuleId, first.RuleId);
        Assert.Equal("captured", first.RuleKey);
        Assert.Equal(TextInspectorRuleCategory.Fraud, first.Category);
        Assert.Equal(7, first.SeverityScore);
        Assert.Equal(68, first.DetectionScore);
        Assert.Equal(30, first.Priority);
        Assert.Equal(input.IndexOf("TARGET", StringComparison.Ordinal), first.Start);
        Assert.Equal(6, first.Length);
        Assert.Equal(first.Start + first.Length, first.EndExclusive);
        Assert.True(engine.HasMatch(input));
        Assert.Single(engine.FindMatches(input, maxMatches: 1));
        Assert.Empty(engine.FindMatches(input, maxMatches: 0));
        Assert.Empty(engine.FindMatches("unrelated"));
    }

    [Fact]
    public void FastAndFullPathsIgnoreZeroLengthOptionalCaptureGroups()
    {
        var rule = Rule("optional-capture", @"(?<token>target)?suffix");
        rule.EvidenceGroup = "token";
        var engine = TextInspectorEngine.CompileInternal(new TextInspectorScoringOptions(), new[] { rule });
        Assert.False(engine.HasMatch("suffix"));
        Assert.Empty(engine.FindMatches("suffix"));
        Assert.Empty(engine.Inspect("suffix").Evidence);
        Assert.True(engine.HasMatch("targetsuffix"));
        Assert.Single(engine.FindMatches("targetsuffix"));
        Assert.Equal("target", Assert.Single(engine.Inspect("targetsuffix").Evidence).MatchedText);
    }

    [Fact]
    public void NoInputAndWhitespaceOnlyTextAreSafeInAllFastPaths()
    {
        var engine = TextInspectorEngine.CompileInternal(new TextInspectorScoringOptions(), new[] { Rule("match", "something") });
        Assert.False(engine.HasMatch(null));
        Assert.False(engine.HasMatch(""));
        Assert.False(engine.HasMatch("\u200b\u200b"));
        Assert.Empty(engine.FindMatches(null));
        Assert.Empty(engine.FindMatches(""));
        Assert.Empty(engine.FindMatches("\u200b\u200b"));
        Assert.Empty(engine.Inspect(null).Evidence);
        Assert.False(engine.CreateReport(null).HasDetections);
        Assert.Empty(engine.InspectMany(Array.Empty<string>()));
        Assert.Throws<ArgumentNullException>(() => engine.InspectMany(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.FindMatches("something", -1));
    }

    [Fact]
    public void AllAndAnyPrefiltersBehaveIdenticallyInFastAndFullInspection()
    {
        var first = Rule("all", @"\btarget\b");
        first.PrefilterMode = TextInspectorPrefilterMode.All;
        first.PrefilterTerms = new[] { "alpha", "beta" };
        var second = Rule("any", @"\btarget\b");
        second.PrefilterMode = TextInspectorPrefilterMode.Any;
        second.PrefilterTerms = new[] { "gamma", "delta" };
        var engine = TextInspectorEngine.CompileInternal(new TextInspectorScoringOptions(), new[] { first, second });
        Assert.Equal(2, engine.CompiledPrefilteredRuleCount);
        Assert.False(engine.HasMatch("target"));
        Assert.Empty(engine.FindMatches("target"));
        Assert.Empty(engine.Inspect("target").Evidence);
        Assert.Single(engine.FindMatches("alpha beta target"));
        Assert.Single(engine.FindMatches("delta target"));
        Assert.Equal(2, engine.FindMatches("alpha beta gamma target").Count);
    }

    [Fact]
    public void InspectionBatchAndHighlightCapsHonorConfiguredLimits()
    {
        var rule = Rule("target", "target");
        var engine = TextInspectorEngine.CompileInternal(new TextInspectorScoringOptions(), new[] { rule });
        var batch = engine.InspectMany(new string?[] { null, "target", "unrelated" });
        Assert.Equal(3, batch.Count);
        Assert.True(batch[1].HasDetections);
        Assert.False(batch[0].HasDetections);
        Assert.False(batch[2].HasDetections);
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.Inspect("target",
            new TextInspectionOptions { MaxReturnedHighlightSpans = -1 }));
        var result = engine.Inspect("target and target", new TextInspectionOptions
        {
            IncludeHighlightSpans = true, MaxReturnedHighlightSpans = 1
        });
        Assert.Single(result.HighlightSpans);
        Assert.True(result.HasDetections);
    }

    [Fact]
    public void PresetExposesOnlyReadOnlyRuleDescriptorsAndNoPolicyCompilation()
    {
        var engine = TextInspectorEngine.Create();
        Assert.Equal(0, engine.CompiledPolicyCount);
        Assert.NotEmpty(engine.CompilationDiagnostics is null ? Array.Empty<TextInspectorDiagnostic>() : TextInspectorDefaults.Rules);
        Assert.Equal(79, TextInspectorDefaults.Rules.Count);
        var metadata = TextInspectorDefaults.Rules[0];
        Assert.False(string.IsNullOrWhiteSpace(metadata.Key));
        Assert.True(Enum.IsDefined(typeof(TextInspectorRuleCategory), metadata.Category));
    }
}
