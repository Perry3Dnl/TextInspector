# TextInspector

**Version 1.0** · Complete standalone package · Free distribution on NuGet

**License:** Free for personal use and genuinely free, non-monetized products — including free apps published by businesses. Monetized products, paid services, or internal revenue-generating tools require a licensed Pro edition. See [LICENSE](LICENSE). This is not an OSI open-source license.

**TextInspector separates what was detected from what is actually risky.**

TextInspector is a complete, standalone .NET text inspection package, distributed free on NuGet for the uses permitted by its license. It includes the inspection engine, explainable risk assessment, masking, reports, and ASP.NET Core integration. It has no inspection-count or text-length quota. Pro extends the same package foundation with additional rules, detection methods, configuration, and developer helpers.

The TextInspector preset includes **76 rules**: all **70 rules from the first standalone import**, using their current calibrated definitions, plus six basic contact, financial and spam signals. It provides exact evidence, explainable risk scores, category findings and UI-ready highlights.

The TextInspector preset is selected explicitly in [Presets/preset-manifest.json](Presets/preset-manifest.json). This is a baseline inventory, not a claim that every baseline rule originated in Unclaimable. No additional Unclaimable identifier rules or datasets were imported for this edition.

**[Practical TextInspector examples](docs/EXAMPLES.md)** · **[TextInspector → Pro migration guide](docs/MIGRATION_TO_PRO.md)**

## Quick start

```bash
dotnet add package TextInspector --version 1.0.0
```

```csharp
using TextInspector;

var inspector = TextInspectorEngine.Create();
var report = inspector.CreateReport("Contact us at hello@example.com.");
Console.WriteLine(report.HasDetections);        // true
Console.WriteLine(report.RiskAssessment.TotalRiskPoints); // 0
Console.WriteLine(report.RequiresReview);           // false
```

An ordinary email address is supporting evidence, not a risk verdict. DetectionScore is an independent deterministic 1..100 rule signal, not a probability. Severity and risk contribution remain separate. The risk score describes the evidence supported by the TextInspector preset; additional Pro rules and policies can produce different findings for the same input.

## TextInspector and Pro

| Capability | TextInspector (free) | TextInspector Pro |
| --- | --- | --- |
| Original imported baseline rules | Yes | Yes |
| Basic email, IBAN, Discord, Signal, Instagram and bulk-DM signals | Yes | Yes |
| Local inspection, Unicode source mapping, regex timeouts and diagnostics | Yes | Yes |
| Detection/risk separation, contextual safety gates and transparent base/bonus scoring | Yes | Yes |
| Category reports, supporting evidence and exact linked highlights | Yes | Yes |
| Sequential batches, raw HasMatch / FindMatches, basic all-match masking | Yes | Yes |
| Full specialized preset | — | Yes |
| Composite policies and policy-correlated findings | — | Yes |
| Parsed URL/domain intelligence, trust baselines and lookalikes | — | Yes |
| Filtered masking and targeted match helpers | — | Yes |
| Parallel batch helper and configurable repeated-capture mode | — | Yes |
| Public custom-rule, context and policy authoring | — | Yes |
| File/database rule management, atomic saves and live multi-instance refresh | — | Yes |

Pro also provides configuration, rule, context, and policy builders, JSON configuration helpers, and validated ASP.NET Core registration. See [TextInspector Pro](https://github.com/Perry3Dnl/TextInspectorPro) for the extended paid edition.

TextInspector has no artificial inspection-count or text-length quota. It keeps correct scoring and exact evidence for the rules it includes. Shipped context gates and normalization remain in TextInspector to preserve calibration and avoid false positives. Basic URL/domain **regex detection** is included; it does not perform Pro URL intelligence.

## Fixed rules, small configuration surface

```csharp
var inspector = TextInspectorEngine.Create(new TextInspectorOptions
{
    DisabledRuleKeys = new[] { "PII_Email_Address" },
    DisabledCategories = new[] { TextInspectorRuleCategory.ExternalContact },
    EnableMasking = true,
    MaskCharacter = '*'
});

foreach(var rule in TextInspectorDefaults.Rules)
    Console.WriteLine($"{rule.Key}: {rule.Category}");
```

Options are consumed when the engine is created. Changing them afterwards does not mutate the engine. Rule metadata is read-only. TextInspector has no supported API for adding/replacing regexes, creating context conditions, importing JSON rules, or compiling a custom preset. The internal matcher stays maintainable in source; modifying source is a separate route from the supported TextInspector API.

Basic masking replaces every detected evidence span and preserves UTF-16 length and offsets. Category/tag/rule-key masking filters belong to Pro. HTML output encodes inspected text; renderer-neutral evidence and highlight spans are also available.

## Reports and raw detection

```csharp
var report = inspector.CreateReport(input);
foreach(var finding in report.Findings)
{
    Console.WriteLine($"{finding.Title}: {finding.RiskLevel}");
    foreach(var reason in finding.Reasons)
        Console.WriteLine(reason.Message);
}

var result = inspector.Inspect(input, new TextInspectionOptions
{
    IncludeHighlightSpans = true,
    IncludeSourceLocations = true
});
var rawHit = inspector.HasMatch(input);
var rawSpans = inspector.FindMatches(input);
```

HasMatch and FindMatches are raw primary-rule checks: they skip contextual scoring. Use Inspect or CreateReport when deciding risk. Reports group scored evidence by category, retaining supporting non-scoring evidence separately. Policy and URL output DTOs remain for schema continuity; TextInspector always returns empty PolicyMatches / UrlObservations and zero policy score. No policy evaluator, domain analyzer or persistence implementation ships in TextInspector.

## ASP.NET Core

```bash
dotnet add package TextInspector.AspNetCore --version 1.0.0
```

```csharp
using TextInspector.AspNetCore;
builder.Services.AddTextInspector(options => options.EnableMasking = true);
```

Resolve ITextInspectorService, ITextInspectorDetectionService or ITextInspectorReportService. They share one immutable service. This adapter depends on DI abstractions only, not Entity Framework or hosted refresh services.

## Packages and upgrade

Core and DI packages target netstandard2.0, net8.0 and net10.0. TextInspector uses distinct NuGet package IDs while preserving the TextInspector namespace and assembly names. Replace the TextInspector package references when upgrading; do not reference both editions in one application. Shared result/report DTOs ease UI migration; the Pro initialization API and scores can differ.

This repository contains the complete TextInspector edition on `main`; the extended paid edition is maintained separately in [TextInspectorPro](https://github.com/Perry3Dnl/TextInspectorPro). The free NuGet packages are `TextInspector` and `TextInspector.AspNetCore`; the paid packages use the `TextInspector.Pro` prefix. Both editions retain the `TextInspector` namespaces and assembly names, so replace package references when upgrading.

The release target is **1.0** (`1.0.0` in NuGet metadata). The installation commands apply once that version is published; this repository migration does not publish packages to NuGet.

## Development and maintenance

Build, test, and package TextInspector independently from this repository. CI runs on pushes and pull requests to `main` and supports manual runs. It does not require the original repository or a sibling Pro branch. When changing shared engine behavior or report contracts, review the corresponding Pro implementation and port applicable fixes between repositories.

## Verification

```bash
dotnet test tests/TextInspector.Tests/TextInspector.Tests.csproj -c Release
dotnet run --project tests/TextInspector.SmokeTests -c Release
dotnet run --project tests/TextInspector.AspNetCore.SmokeTests -c Release
dotnet pack TextInspector.csproj -c Release
```

CI verifies .NET 8/10 tests, all target builds, both smoke tests, packed consumer installation and the public TextInspector API boundary. Full-edition accuracy and benchmark figures are not reused as TextInspector measurements.
