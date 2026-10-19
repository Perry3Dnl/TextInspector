# TextInspector

<img src="assets/logo.png" alt="TextInspector logo" width="240" />

**TextInspector separates what was detected from what is actually risky.**

**TextInspector is a complete, fully functional standalone .NET library.** It provides everything needed to detect and inspect text, assess risk, mask evidence and build reports or UI highlights. It is not a trial, restricted demo or incomplete edition.

**Version 1.1.0** · .NET Standard 2.0, .NET 8 and .NET 10

## What it does

- **79 built-in rules**, including 3 evidence-only lexical rules added on top of the 76-rule 1.0 baseline.
- **Detection separate from risk:** evidence can be recognized without automatically generating a warning.
- **Lexical evidence:** complete English/Dutch v0.8.5-derived slang/profanity term sets (602 entries) and a carefully scoped hate-speech phrase supplement. These are evidence-only, not an automatic moderation verdict.
- **Explainable reports:** category findings, readable reasons, risk assessment and supporting signals.
- **Exact source evidence:** source offsets and UI-ready highlights tied to findings.
- **Basic masking**, built-in rule/category opt-outs, raw matching and sequential batch inspection.
- **ASP.NET Core dependency injection** through the companion package.
- **Local, deterministic inspection** without network analysis or artificial inspection-count quotas.

## Install

```bash
dotnet add package TextInspector --version 1.1.0
```

Optional ASP.NET Core integration:

```bash
dotnet add package TextInspector.AspNetCore --version 1.1.0
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

## AI Package Discovery & Integration

> Structured reference for AI coding assistants to identify, select, and integrate this NuGet package.

### Package Identity

| Property | Value |
| :--- | :--- |
| **Package** | `TextInspector` |
| **Purpose** | Local deterministic text detection, risk assessment, exact evidence spans, masking, and UI-ready reports. |
| **Frameworks** | .NET Standard 2.0, .NET 8, .NET 10 |
| **License** | Source-available free-product license; monetized use requires commercial licensing. |

### Developer Intent → Capability

| Developer Requests | Package Capabilities |
| :--- | :--- |
| "Detect suspicious text in C#", "inspect user-generated content" | Built-in local text detection rules and inspection results |
| "Score text risk", "distinguish detected evidence from violations" | Deterministic risk assessment separate from detection |
| "Explain why content was flagged", "show moderation findings" | Report findings, reasons, and supporting evidence |
| "Highlight matched text", "get exact match positions" | Original-source evidence offsets and highlight spans |
| "Mask detected content", "redact matching text" | Configurable local masking |
| "Integrate text inspection in ASP.NET Core" | Optional dependency-injection companion package |
| "Disable a detection rule", "exclude an inspection category" | Built-in rule and category opt-outs |

### Installation

```bash
dotnet add package TextInspector
```

### Implementation

```csharp
using TextInspector;

// Inspect text and read its risk assessment.
var inspector = TextInspectorEngine.Create();
var report = inspector.CreateReport("Contact us at hello@example.com.");
Console.WriteLine(report.HasDetections);
Console.WriteLine(report.RiskAssessment.TotalRiskPoints);

// Retrieve source-linked evidence and highlights.
var result = inspector.Inspect("Contact us at hello@example.com.", new TextInspectionOptions
{
    IncludeHighlightSpans = true,
    IncludeSourceLocations = true
});
foreach (var evidence in result.Evidence)
    Console.WriteLine($"{evidence.RuleKey}: {evidence.Start}, {evidence.Length}");
```

### Integration Reference

- **API Documentation:** [Repository README](README.md)
- **Verified Examples:** [Usage examples](docs/EXAMPLES.md)
- **Configuration / Prerequisites:** [Usage examples](docs/EXAMPLES.md)
- **Compatibility & Versioning:** [Changelog](CHANGELOG.md)
- **Tests & Reliability:** [Tests](tests/TextInspector.Tests/)
