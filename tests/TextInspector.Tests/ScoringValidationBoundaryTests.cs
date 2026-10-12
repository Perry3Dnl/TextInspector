using TextInspector;

namespace TextInspector.Tests;

/// <summary>Boundary tests for configuration validation, the same in Lite and Pro.</summary>
public sealed class ScoringValidationBoundaryTests
{
    [Theory]
    [InlineData("version")]
    [InlineData("range")]
    [InlineData("minimum-below")]
    [InlineData("minimum-above")]
    [InlineData("saturation")]
    [InlineData("raw-cap")]
    [InlineData("severity-scale")]
    [InlineData("severity-low")]
    [InlineData("severity-high")]
    [InlineData("regex-timeout")]
    [InlineData("input-limit")]
    [InlineData("snippet-pad")]
    [InlineData("snippet-count")]
    [InlineData("masking-null")]
    [InlineData("negative-threshold")]
    [InlineData("negative-combination")]
    public void InvalidScoringConfigurationIsRejected(string issue)
    {
        var defaults = new TextInspectorScoringOptions();
        TextInspectorScoringOptions options = issue switch
        {
            "version" => new() { Version = 0 },
            "range" => new() { ScoreMin = 100, ScoreMax = 100 },
            "minimum-below" => new() { ScoreMin = 10, MinNonZeroScore = 1 },
            "minimum-above" => new() { MinNonZeroScore = 101 },
            "saturation" => new() { SaturationK = 0 },
            "raw-cap" => new() { RawCapFor100 = 0 },
            "severity-scale" => new() { SeverityScaleMax = 0 },
            "severity-low" => new() { HighSeverityThreshold = -1 },
            "severity-high" => new() { HighSeverityThreshold = 11 },
            "regex-timeout" => new() { RegexTimeoutMilliseconds = 0 },
            "input-limit" => new() { MaxInputLength = -1 },
            "snippet-pad" => new() { EvidenceSnippetPad = -1 },
            "snippet-count" => new() { DefaultMaxEvidenceSnippets = -1 },
            "masking-null" => new() { Masking = null! },
            "negative-threshold" => new() { Combo_ChildPlusAnyObf_AtObfCount = -1 },
            "negative-combination" => new() { Combo_ChildPlusCSAM_Bonus = -1 },
            _ => defaults
        };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void DefaultScoringConfigurationIsValid()
    {
        new TextInspectorScoringOptions().Validate();
    }

    [Theory]
    [InlineData("risk-level")]
    [InlineData("negative-reasons")]
    [InlineData("negative-findings")]
    public void InvalidReportOptionsAreRejected(string issue)
    {
        var options = issue switch
        {
            "risk-level" => new TextInspectionReportOptions { FlaggedMinimumRiskLevel = (TextInspectionRiskLevel)999 },
            "negative-reasons" => new TextInspectionReportOptions { MaxReasonsPerFinding = -1 },
            _ => new TextInspectionReportOptions { MaxFindings = -1 }
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }

    [Fact]
    public void EmptyReportOptionsAreValid()
    {
        new TextInspectionReportOptions { MaxFindings = 0, MaxReasonsPerFinding = 0 }.Validate();
    }
}
