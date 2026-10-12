# Upgrade TextInspector Lite to Pro

The editions share the `TextInspector` namespace and report model, but **must not** be referenced together because the assemblies have the same name.

| Step | Lite | Pro |
| --- | --- | --- |
| NuGet core | `TextInspector.Lite` | `TextInspector` |
| NuGet ASP.NET Core | `TextInspector.Lite.AspNetCore` | `TextInspector.AspNetCore` |
| Quick-start factory | `TextInspectorEngine.Create()` | `TextInspectorPro.Create()` |
| Inspect / report | `Inspect(text)` / `CreateReport(text)` | Same method calls |
| DI registration | `builder.Services.AddTextInspector()` | Same method call |
| Rules | Fixed 76, selectively disabled | Full 157 and optional custom rules |
| Policies | Not evaluated | 15 presets and optional custom policies |

## Minimal migration

**Before (Lite)**

```csharp
using TextInspector;
var inspector = TextInspectorEngine.Create();
var report = inspector.CreateReport(input);
```

**After (Pro)**

```csharp
using TextInspector;
var inspector = TextInspectorPro.Create();
var report = inspector.CreateReport(input);
```

For ASP.NET Core, replace the NuGet references but keep `builder.Services.AddTextInspector()` and the same injected `ITextInspectorService` contract. If Lite used `TextInspectorLiteOptions`, port the **intent** of those options rather than copying the options type directly; Pro has a richer configuration surface.

## Acceptance tests before switching traffic

Run the old Lite and new Pro applications separately over the same test corpus. Assert:
- No exceptions or timeouts for benign, malformed, very long, and Unicode input.
- Returned evidence `Start` / `Length` still point to the correct original substring.
- Your UI renders `Findings`, `SupportingEvidence`, and highlights safely.
- Thresholds have been reviewed: Pro intentionally has more built-in rules and composite policies, so **identical risk scores are not promised**.

## Licensing

Lite is free for genuinely free, non-monetized products, including those offered by a company. A commercial Pro license is required when the product is monetized or forms part of a paid service; obtain the license **before** deploying that use. See the license shipped with each edition.
