using System.Globalization;
using TextInspector;

namespace TextInspector.Tests;

public sealed class NumericRuleConditionTests
{
    private static TextInspectorRulePattern Rule(
        TextInspectorNumericComparison comparison = TextInspectorNumericComparison.GreaterThan,
        decimal value = 10m,
        string separator = ".") => new()
    {
        TextInspectorRuleId = Guid.NewGuid(),
        Key = "Quantity",
        Category = TextInspectorRuleCategory.PlatformIntegrity,
        RegexPattern = @"\bitem\s+a\s*:\s*(?<quantity>\S+)",
        EvidenceGroup = "quantity",
        RuleWeight = 10,
        SeverityScore = 5,
        NumericConditions = new[]
        {
            new TextInspectorNumericCondition
            {
                CaptureGroup = "quantity", Comparison = comparison, Value = value, DecimalSeparator = separator
            }
        }
    };

    private static TextInspectorEngine Compile(TextInspectorRulePattern rule) =>
        TextInspectorEngine.Compile(new TextInspectorScoringOptions(), new[] { rule });

    [Theory]
    [InlineData(1, "10", true)]
    [InlineData(1, "10.00", true)]
    [InlineData(1, "11", false)]
    [InlineData(2, "10", false)]
    [InlineData(2, "11", true)]
    [InlineData(3, "9", false)]
    [InlineData(3, "10", false)]
    [InlineData(3, "11", true)]
    [InlineData(3, "100", true)]
    [InlineData(3, "00010", false)]
    [InlineData(3, "00011", true)]
    [InlineData(3, "+10.5", true)]
    [InlineData(3, "-100", false)]
    [InlineData(3, "0", false)]
    [InlineData(3, "-0.00", false)]
    [InlineData(3, "10.000000000000000000000000001", true)]
    [InlineData(3, "79228162514264337593543950335", true)]
    [InlineData(4, "10", true)]
    [InlineData(4, "9", false)]
    [InlineData(5, "9", true)]
    [InlineData(5, "10", false)]
    [InlineData(6, "10", true)]
    [InlineData(6, "11", false)]
    public void NumericConditionsApplyToInspectionAndRawMatching(int comparison, string number, bool expected)
    {
        var engine = Compile(Rule((TextInspectorNumericComparison)comparison));
        string input = "Item A: " + number;
        Assert.Empty(engine.CompilationDiagnostics);
        Assert.Equal(expected, engine.HasMatch(input));
        Assert.Equal(expected ? 1 : 0, engine.FindMatches(input).Count);
        var result = engine.Inspect(input, new TextInspectionOptions { IncludeHighlightSpans = true });
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected ? 1 : 0, result.Evidence.Count);
        Assert.Equal(expected, engine.CreateReport(input).HasDetections);
        if (expected)
        {
            var evidence = Assert.Single(result.Evidence);
            Assert.Equal(number, evidence.MatchedText);
            Assert.Equal(number, input.Substring(evidence.Start, evidence.Length));
            Assert.Single(result.HighlightSpans);
        }
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData(".5")]
    [InlineData("10.")]
    [InlineData("1.2.3")]
    [InlineData("1,000")]
    [InlineData("1e2")]
    [InlineData("100mg")]
    [InlineData("79228162514264337593543950336")]
    [InlineData("10.00000000000000000000000000001")]
    public void UnsupportedOrRoundedNumbersProduceDiagnosticsInsteadOfAnApparentPass(string number)
    {
        var engine = Compile(Rule());
        string input = "Item A: " + number;
        Assert.False(engine.HasMatch(input));
        Assert.Empty(engine.FindMatches(input));
        var result = engine.Inspect(input);
        Assert.Empty(result.Evidence);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(TextInspectorDiagnosticKind.InvalidNumericCapture, diagnostic.Kind);
        Assert.Equal("Quantity", diagnostic.RuleKey);
        Assert.Single(engine.CreateReport(input).Diagnostics);
    }

    [Theory]
    [InlineData("nl-NL")]
    [InlineData("en-US")]
    public void SeparatorIsExplicitAndIndependentOfCurrentCulture(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var dot = Compile(Rule());
            var comma = Compile(Rule(separator: ","));
            Assert.True(dot.HasMatch("Item A: 10.5"));
            Assert.True(comma.HasMatch("Item A: 10,5"));
            Assert.False(comma.HasMatch("Item A: 10,000"));
            Assert.Single(dot.Inspect("Item A: 10,5").Diagnostics);
            Assert.Single(comma.Inspect("Item A: 10.5").Diagnostics);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void ConditionsCannotBorrowAnotherItemsNumber()
    {
        var engine = Compile(Rule());
        Assert.Empty(engine.Inspect("Item A: 10\nItem C: 100").Evidence);
        var evidence = Assert.Single(engine.Inspect("Item A: 10\nItem A: 100").Evidence);
        Assert.Equal("100", evidence.MatchedText);
        Assert.Equal(19, evidence.Start);
    }

    [Fact]
    public void CapturesAndEvidenceKeepOriginalCoordinatesAfterNormalization()
    {
        const string input = "ITEM A: １００";
        var result = Compile(Rule()).Inspect(input);
        Assert.Empty(result.Diagnostics);
        var evidence = Assert.Single(result.Evidence);
        Assert.Equal("１００", evidence.MatchedText);
        Assert.Equal(8, evidence.Start);
    }

    [Fact]
    public void MultipleConditionsAreAndedWithinTheSameMatch()
    {
        var rule = Rule(TextInspectorNumericComparison.GreaterThanOrEqual, 10);
        rule.NumericConditions = rule.NumericConditions.Concat(new[]
        {
            new TextInspectorNumericCondition
            {
                CaptureGroup = "quantity", Comparison = TextInspectorNumericComparison.LessThan, Value = 20
            }
        }).ToArray();
        var engine = Compile(rule);
        Assert.False(engine.HasMatch("Item A: 9"));
        Assert.True(engine.HasMatch("Item A: 10"));
        Assert.True(engine.HasMatch("Item A: 19.5"));
        Assert.False(engine.HasMatch("Item A: 20"));
    }

    [Fact]
    public void AFalseComparisonDoesNotHideAnInvalidSecondCapture()
    {
        var rule = Rule();
        rule.RegexPattern = @"(?<quantity>\S+) (?<other>\S+)";
        rule.NumericConditions = rule.NumericConditions.Concat(new[]
        {
            new TextInspectorNumericCondition
            {
                CaptureGroup = "other", Comparison = TextInspectorNumericComparison.LessThan, Value = 20
            }
        }).ToArray();
        var result = Compile(rule).Inspect("5 invalid");
        Assert.Empty(result.Evidence);
        Assert.Single(result.Diagnostics);
    }

    [Theory]
    [InlineData(@"item a(?:: (?<quantity>\S+))?", "item a")]
    [InlineData(@"(?<quantity>\d)+", "11")]
    [InlineData(@"(?<quantity>)x", "x")]
    public void MissingRepeatedOrEmptyCapturesAreNotSilentlySelected(string pattern, string input)
    {
        var rule = Rule();
        rule.RegexPattern = pattern;
        var engine = Compile(rule);
        Assert.False(engine.HasMatch(input));
        Assert.Empty(engine.FindMatches(input));
        Assert.Equal(TextInspectorDiagnosticKind.InvalidNumericCapture,
            Assert.Single(engine.Inspect(input).Diagnostics).Kind);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void InvalidNumericConfigurationSkipsRuleWithACompilationDiagnostic(int scenario)
    {
        var rule = Rule();
        switch (scenario)
        {
            case 0: rule.NumericConditions = null!; break;
            case 1: rule.NumericConditions = new TextInspectorNumericCondition[] { null! }; break;
            case 2: rule.NumericConditions[0].CaptureGroup = ""; break;
            case 3: rule.NumericConditions[0].CaptureGroup = "missing"; break;
            case 4: rule.NumericConditions[0].Comparison = (TextInspectorNumericComparison)999; break;
            case 5: rule.NumericConditions[0].DecimalSeparator = "/"; break;
        }
        var engine = Compile(rule);
        Assert.Equal(0, engine.CompiledRuleCount);
        Assert.Equal(TextInspectorDiagnosticKind.InvalidNumericCondition,
            Assert.Single(engine.CompilationDiagnostics).Kind);
    }

    [Fact]
    public void CompiledConditionsAreDetachedFromMutableConfiguration()
    {
        var rule = Rule();
        rule.NumericConditions[0].CaptureGroup = " quantity ";
        var engine = Compile(rule);
        rule.NumericConditions[0].Value = 1000;
        rule.NumericConditions[0].DecimalSeparator = ",";
        rule.NumericConditions = Array.Empty<TextInspectorNumericCondition>();
        Assert.True(engine.HasMatch("Item A: 10.5"));
        Assert.False(engine.HasMatch("Item A: 10"));
    }

    [Fact]
    public void NoConditionsPreserveExistingRegexBehavior()
    {
        var rule = Rule();
        rule.NumericConditions = Array.Empty<TextInspectorNumericCondition>();
        var result = Compile(rule).Inspect("Item A: text");
        Assert.Single(result.Evidence);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void NumericConditionsRoundTripThroughRuleJson()
    {
        var rule = Rule(separator: ",");
        string json = System.Text.Json.JsonSerializer.Serialize(rule, TextInspectorPresets.Json);
        var restored = System.Text.Json.JsonSerializer.Deserialize<TextInspectorRulePattern>(json, TextInspectorPresets.Json)!;
        var engine = Compile(restored);
        Assert.True(engine.HasMatch("Item A: 10,5"));
        Assert.False(engine.HasMatch("Item A: 10"));
        Assert.Equal(",", restored.NumericConditions[0].DecimalSeparator);
    }

    [Fact]
    public void NumberedGroupsAndNegativeThresholdsWork()
    {
        var rule = Rule(TextInspectorNumericComparison.LessThan, -10);
        rule.RegexPattern = @"item a: (\S+)";
        rule.EvidenceGroup = "1";
        rule.NumericConditions[0].CaptureGroup = "1";
        var engine = Compile(rule);
        Assert.True(engine.HasMatch("Item A: -10.5"));
        Assert.False(engine.HasMatch("Item A: -10"));
    }
    [Fact]
    public void UnitAwareNumericConditionsReuseTheNormalEngine()
    {
        var rule = new TextInspectorRulePattern
        {
            TextInspectorRuleId = Guid.NewGuid(),Key = "DoseOverLimit",
            Category = TextInspectorRuleCategory.PlatformIntegrity,
            RegexPattern = @"\bparacetamol\s+(?<amount>\d+(?:\.\d+)?)\s*(?<unit>mg|g)\s+at\s+once\b",
            NumericConditions = new[]
            {
                new TextInspectorNumericCondition
                {
                    CaptureGroup = "amount",
                    UnitCaptureGroup = "unit",
                    UnitFactors = new Dictionary<string,decimal>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["mg"] = 1m, ["g"] = 1000m
                    },
                    Comparison = TextInspectorNumericComparison.GreaterThan,
                    Value = 1000m
                }
            },
            RuleWeight = 10,SeverityScore = 2
        };
        var engine = TextInspectorEngine.Compile(new TextInspectorScoringOptions(),new[] { rule });
        Assert.False(engine.Inspect("paracetamol 500 mg at once").HasDetections);
        Assert.False(engine.Inspect("paracetamol 1 g at once").HasDetections);
        Assert.True(engine.Inspect("paracetamol 1.5 g at once").HasDetections);
        Assert.True(engine.Inspect("paracetamol 1500 mg at once").HasDetections);
    }

    [Fact]
    public void InvalidUnitDefinitionIsReportedAtCompilation()
    {
        var rule = Rule();
        rule.NumericConditions[0].UnitCaptureGroup = "missing";
        rule.NumericConditions[0].UnitFactors = new Dictionary<string,decimal> { ["g"] = 1000m };
        var compiled = TextInspectorEngine.CompileWithDiagnostics(new TextInspectorScoringOptions(),new[] { rule });
        Assert.Equal(0,compiled.Engine.CompiledRuleCount);
    }

}
