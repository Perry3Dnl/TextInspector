# TextInspector.AspNetCore

**ASP.NET Core integration for TextInspector, the complete standalone .NET text inspection library.**

This optional companion package registers TextInspector inspection, detection and reporting services using dependency injection. It uses the same fully functional library, built-in rules and risk assessment as the core package.

## Install

```bash
dotnet add package TextInspector.AspNetCore --version 1.0.1
```

## Register services

```csharp
using TextInspector.AspNetCore;

builder.Services.AddTextInspector(options => options.EnableMasking = true);
```

Resolve `ITextInspectorService`, `ITextInspectorDetectionService` or `ITextInspectorReportService` from DI. No database integration is required.

- Shared, immutable inspection service.
- Explainable reports and exact evidence spans.
- Built-in rule/category exclusions and basic masking.
- .NET Standard 2.0, .NET 8 and .NET 10 target assets.

## Documentation and licensing

[TextInspector documentation](https://github.com/Perry3Dnl/TextInspector) · [Full license](https://github.com/Perry3Dnl/TextInspector/blob/v1.0.1/LICENSE)

Free for genuinely free, non-monetized products; monetized products and revenue-generating internal tools require separate commercial licensing. Not an OSI-approved open-source license.

**Version 1.0.1:** Package documentation update; no inspection behavior changes.
