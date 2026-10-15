# TextInspector

<img src="assets/logo.png" alt="TextInspector logo" width="240" />

**TextInspector separates what was detected from what is actually risky.**

**TextInspector is a complete, fully functional standalone .NET library.** It provides everything needed to detect and inspect text, assess risk, mask evidence and build reports or UI highlights. It is not a trial, restricted demo or incomplete edition.

**Version 1.0.2** · .NET Standard 2.0, .NET 8 and .NET 10

## What it does

- **76 released built-in rules**, with 3 additional evidence-only lexical rules prepared for the next release (79 total on this branch).
- **Detection separate from risk:** evidence can be recognized without automatically generating a warning.
- **Upcoming lexical evidence:** complete English/Dutch v0.8.5-derived slang/profanity term sets (602 entries) and a carefully scoped hate-speech phrase supplement. These are evidence-only, not an automatic moderation verdict.
- **Explainable reports:** category findings, readable reasons, risk assessment and supporting signals.
- **Exact source evidence:** source offsets and UI-ready highlights tied to findings.
- **Basic masking**, built-in rule/category opt-outs, raw matching and sequential batch inspection.
- **ASP.NET Core dependency injection** through the companion package.
- **Local, deterministic inspection** without network analysis or artificial inspection-count quotas.

## Install

```bash
dotnet add package TextInspector --version 1.0.2
```

Optional ASP.NET Core integration:

```bash
dotnet add package TextInspector.AspNetCore --version 1.0.2
```

## Quick start

```csharp
using TextInspector;

var inspector = TextInspectorEngine.Create();
var report = inspector.CreateReport("Contact us at hello@example.com.");

Console.WriteLine(report.HasDetections);                  // true
Console.WriteLine(report.RiskAssessment.TotalRiskPoints); // 0
Console.WriteLine(report.RequiresReview);                 // false
```

An email address is detected as evidence, but an ordinary address is not inherently risky. DetectionScore is a deterministic rule signal, not a probability; detection, severity and risk contribution are distinct.

### Findings and highlights

```csharp
var report = inspector.CreateReport(input);
foreach (var finding in report.Findings)
{
    Console.WriteLine($"{finding.Title}: {finding.RiskLevel}");
    foreach (var reason in finding.Reasons)
        Console.WriteLine(reason.Message);
}

var result = inspector.Inspect(input, new TextInspectionOptions
{
    IncludeHighlightSpans = true,
    IncludeSourceLocations = true
});

bool hasRawMatch = inspector.HasMatch(input);
var rawMatches = inspector.FindMatches(input);
```

`HasMatch` and `FindMatches` perform raw matching without contextual risk scoring. Use `Inspect` or `CreateReport` for risk decisions.

### Configure built-in rules

```csharp
var inspector = TextInspectorEngine.Create(new TextInspectorOptions
{
    DisabledRuleKeys = new[] { "PII_Email_Address" },
    DisabledCategories = new[] { TextInspectorRuleCategory.ExternalContact },
    EnableMasking = true,
    MaskCharacter = '*'
});
```

Options are read when the engine is created. Shipped rule definitions are read-only through the supported public API.

### ASP.NET Core

```csharp
using TextInspector.AspNetCore;

builder.Services.AddTextInspector(options => options.EnableMasking = true);
```

Resolve `ITextInspectorService`, `ITextInspectorDetectionService` or `ITextInspectorReportService` through dependency injection.

## Editions and licensing

**The free TextInspector package is complete and usable on its own.** TextInspector Pro is an optional expanded edition with additional tools and detection capabilities, plus commercial licensing. Pro is not needed to make TextInspector functional.

TextInspector is free for personal, educational and genuinely free, non-monetized products, including free applications published by businesses. Monetized products, paid services and revenue-generating internal business tools are outside the free-product license and require the appropriate commercial license.

Read the full [LICENSE](LICENSE). TextInspector is source-available under its own free-product license, **not OSI-approved open source**.

## Documentation and verification

- [Usage examples](docs/EXAMPLES.md)
- [Built-in preset manifest](Presets/preset-manifest.json)
- [NuGet-specific README](docs/NUGET_README.md)
- [Changelog](CHANGELOG.md)

```bash
dotnet test tests/TextInspector.Tests/TextInspector.Tests.csproj -c Release
dotnet run --project tests/TextInspector.SmokeTests -c Release
dotnet run --project tests/TextInspector.AspNetCore.SmokeTests -c Release
dotnet pack TextInspector.csproj -c Release
```

CI builds supported targets, runs .NET 8 and .NET 10 tests, enforces a 95% core line-coverage threshold, tests both smoke applications, and installs the packed package into a clean consumer project.
