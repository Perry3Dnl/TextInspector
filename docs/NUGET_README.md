# TextInspector

![TextInspector logo](https://raw.githubusercontent.com/Perry3Dnl/TextInspector/v1.0.2/assets/logo.png)

**TextInspector separates what was detected from what is actually risky.**

**A complete, standalone .NET text inspection library.** TextInspector detects patterns, evaluates risk, preserves exact evidence and produces UI-ready reports. The free edition is fully functional on its own—not a trial, demo or feature-incomplete package.

## Features

- **76 rules in the published 1.0.2 package**. The next release candidate adds three evidence-only lexical rules, for 79 total.
- **Separate detection and risk:** recognized signals do not automatically become violations.
- **Next release candidate:** 602 English/Dutch lexical slang/profanity terms and a limited additional hate-speech phrase set, classified as supporting evidence.
- **Explainable risk reports** with category findings, reasons and supporting evidence.
- **Source-accurate highlights** linked to the original text and report findings.
- **Built-in masking**, rule/category exclusions, raw-match helpers and sequential batches.
- **Local, deterministic execution** with no remote inspection service or usage quota.
- Supports **.NET Standard 2.0, .NET 8 and .NET 10**.

## Install

```bash
dotnet add package TextInspector --version 1.0.2
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

An ordinary email address is detected as supporting evidence without automatically increasing risk.

### Read findings

```csharp
foreach (var finding in report.Findings)
{
    Console.WriteLine($"{finding.Title}: {finding.RiskLevel}");
    foreach (var reason in finding.Reasons)
        Console.WriteLine(reason.Message);
}
```

For source-linked highlights and positions, use `Inspect` with `TextInspectionOptions.IncludeHighlightSpans` and `IncludeSourceLocations`, or use the report highlights.

## ASP.NET Core

Install the optional [TextInspector.AspNetCore](https://www.nuget.org/packages/TextInspector.AspNetCore) companion package:

```bash
dotnet add package TextInspector.AspNetCore --version 1.0.2
```

```csharp
using TextInspector.AspNetCore;

builder.Services.AddTextInspector(options => options.EnableMasking = true);
```

## Licensing

Free for personal use and genuinely free, non-monetized products, including products released by businesses. Commercial use in monetized products, paid services and revenue-generating internal systems requires a separate commercial license. This is a **source-available license, not an OSI-approved open-source license**.

[Full license](https://github.com/Perry3Dnl/TextInspector/blob/v1.0.2/LICENSE) · [Documentation and examples](https://github.com/Perry3Dnl/TextInspector)

---

**Version 1.0.2:** Synchronized core and ASP.NET Core package versions and updated package documentation. No inspection behavior changes. No changes to the inspection engine, public API or 76-rule preset.
