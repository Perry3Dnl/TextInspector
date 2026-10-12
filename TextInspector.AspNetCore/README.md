# TextInspector Lite ASP.NET Core

The free Lite integration package is `TextInspector.Lite.AspNetCore`, version **1.0** (`1.0.0`). It is included in the complete Lite edition and uses the [Lite license](../LICENSE).

```bash
dotnet add package TextInspector.Lite.AspNetCore --version 1.0.0
```

Install this package after version 1.0.0 is published on NuGet. It registers one immutable, local Lite service without Entity Framework, files or live rule management.

```csharp
using TextInspector.AspNetCore;
builder.Services.AddTextInspector(options => options.EnableMasking = true);
```

Resolve ITextInspectorService, ITextInspectorDetectionService or ITextInspectorReportService. Configuration uses TextInspectorLiteOptions: disable shipped rule keys/categories or enable basic masking. Custom regexes, policies, database persistence and refresh helpers belong to Pro.

See the [Lite README](../README.md) for reports and package compatibility, or [TextInspector Pro](https://github.com/Perry3Dnl/TextInspectorPro) for the extended paid edition.
