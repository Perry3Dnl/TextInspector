# TextInspector ASP.NET Core

The free TextInspector integration package is `TextInspector.AspNetCore`, version **1.0** (`1.0.0`). It is included in the complete TextInspector edition and uses the [TextInspector license](../LICENSE).

```bash
dotnet add package TextInspector.AspNetCore --version 1.0.0
```

Install this package after version 1.0.0 is published on NuGet. It registers one immutable, local TextInspector service without Entity Framework, files or live rule management.

```csharp
using TextInspector.AspNetCore;
builder.Services.AddTextInspector(options => options.EnableMasking = true);
```

Resolve ITextInspectorService, ITextInspectorDetectionService or ITextInspectorReportService. Configuration uses TextInspectorOptions: disable shipped rule keys/categories or enable basic masking. Custom regexes, policies, database persistence and refresh helpers belong to Pro.

See the [TextInspector README](../README.md) for reports and package compatibility, or [TextInspector Pro](https://github.com/Perry3Dnl/TextInspectorPro) for the extended paid edition.
