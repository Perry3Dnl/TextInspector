# TextInspector: practical examples

These examples assume the `TextInspector` NuGet package. Import `TextInspector` and keep the engine instance for reuse; compile it once, not for every request.

## 1. First result in under a minute

```csharp
using TextInspector;

var inspector = TextInspectorEngine.Create();
var report = inspector.CreateReport("Contact: hello@example.com");
Console.WriteLine($"Detection: {report.HasDetections}");
Console.WriteLine($"Risk points: {report.RiskAssessment.TotalRiskPoints}");
Console.WriteLine($"Flagged: {report.RequiresReview}");
```

An email may be detected without adding risk points. Detection is **not** an automatic moderation verdict.

## 2. Highlight exact original text in your own UI

```csharp
var input = "Please contact me at hello@example.com";
var result = inspector.Inspect(input, new TextInspectionOptions
{
    IncludeHighlightSpans = true,
    IncludeSourceLocations = true
});
foreach (var evidence in result.Evidence)
    Console.WriteLine($"{evidence.RuleKey}: {evidence.Start}, {evidence.Length} — {evidence.MatchedText}");
foreach (var span in result.HighlightSpans)
    Console.WriteLine($"Highlight original UTF-16 indexes {span.Start}..{span.Start + span.Length}");
```

Use `Start` and `Length` against the **original input**, not a separately normalized string. For HTML, `inspector.Inspect(input, includeMarkedHtml: true).MarkedHtml` contains encoded text with marked detections.

## 3. Basic masking and disabling rules

```csharp
var maskedInspector = TextInspectorEngine.Create(new TextInspectorOptions
{
    EnableMasking = true,
    MaskCharacter = '*',
    DisabledCategories = new[] { TextInspectorRuleCategory.PlatformIntegrity }
});
var masked = maskedInspector.Inspect("hello@example.com").MaskedText;
Console.WriteLine(masked);
```

TextInspector masks all matched evidence when enabled. For category/tag-filtered masking or custom rules use Pro. The supported TextInspector API has a fixed preset; you may disable built-in rules and categories.

## 4. ASP.NET Core dependency injection

```bash
dotnet add package TextInspector.AspNetCore --version 1.0.0
```

In `Program.cs`:

```csharp
using TextInspector.AspNetCore;
builder.Services.AddTextInspector();
```

Inject `ITextInspectorService` into an endpoint/service and call `CreateReport(text)`. Do not register TextInspector and Pro packages in the same project: they intentionally share assembly and namespace names.

## 5. When and how to upgrade

A free, non-monetized public app may use TextInspector, even if published by a company. If the app introduces advertising, subscriptions, paid access or becomes part of a paid service, replace TextInspector with licensed Pro **before** monetization. See [the migration checklist](MIGRATION_TO_PRO.md).

```bash
dotnet remove package TextInspector
dotnet add package TextInspector.Pro --version 1.0.0
```

Replace `TextInspectorEngine.Create()` with `TextInspectorPro.Create()`; `CreateReport()`, `Inspect()`, evidence/source spans and report contracts remain available. Pro includes more rules and policies, so *risk outputs may change*. Compare reports on representative input before enabling Pro in production.
