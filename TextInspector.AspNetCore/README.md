# TextInspector.AspNetCore

**ASP.NET Core dependency injection for the complete TextInspector text inspection library.**

TextInspector is a fully functional standalone library. This optional adapter makes its existing inspection and reporting services easy to use with ASP.NET Core. No database or additional edition is required.

## Install

```bash
dotnet add package TextInspector.AspNetCore --version 1.0.2
```

## Register

```csharp
using TextInspector.AspNetCore;

builder.Services.AddTextInspector(options => options.EnableMasking = true);
```

Resolve `ITextInspectorService`, `ITextInspectorDetectionService` or `ITextInspectorReportService` from your dependency-injection container. The service uses the included built-in rules, supports rule/category exclusions and basic masking, and performs inspection locally.

For usage examples and the inspection/reporting model, see the [TextInspector README](../README.md).

## Licensing

Free for personal use and genuinely free, non-monetized products. Paid and monetized uses require separate commercial licensing. See the [TextInspector license](../LICENSE). This is not an OSI-approved open-source license.
