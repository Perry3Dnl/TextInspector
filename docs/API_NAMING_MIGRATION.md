# Breaking API naming migration

TextInspector Lite and Pro use the same reporting vocabulary. This change **renames actual C# members**, not display labels or forwarding aliases, and is intentionally source- and JSON-schema-breaking. Recompile consumers and update serialized report readers.

## Main property changes

| Previous name | New name |
| --- | --- |
| `SafetyInfo` | `RiskAssessment` |
| `HasDetection` | `HasDetections` |
| `IsFlagged` | `RequiresReview` |
| `PriorityScore` | `RiskScore` |
| `RawScore` | `TotalRiskPoints` |
| `BaseRawScore` | `RuleRiskPoints` |
| `BonusRawScore` | `AdditionalRiskPoints` |
| `BuiltInBonusRawScore` | `BuiltInBonusPoints` |
| `PolicyRawScore` | `PolicyRiskPoints` |
| `MaxSeverityScore` | `HighestDetectedSeverity` |
| `MaxScoredSeverityScore` | `HighestScoredSeverity` |
| `MaxPolicySeverityScore` | `HighestPolicySeverity` |
| `EffectiveSeverityScore` | `EffectiveSeverity` |
| `EvidenceCount` | `TotalEvidenceCount` |
| `ScoredEvidenceCount` | `ScoringEvidenceCount` |
| `CategoryHitCount` | `DetectedCategoryCount` |
| `ScoredCategoryCount` | `ScoringCategoryCount` |
| `MatchedPolicyCount` | `TriggeredPolicyCount` |
| `ScoredPolicyCount` | `ScoringPolicyCount` |
| `ScoredFindingCount` | `ScoringFindingCount` |
| `CountsForScoring` | `ContributesToRisk` |
| `ScoringDisposition` | `ScoringStatus` |
| `HighlightScore` | `HighlightIntensity` |
| `BaseWeight` | `RuleWeight` |
| `RawPoints` | `RiskPoints` |
| `ScoreContributions` | `RiskContributions` |
| `Urls` | `UrlObservations` |
| `TextInspectionReport.Findings[].Id` | `FindingId` |
| `TextInspectionReport.Findings[].Code` | `FindingCode` |
| `TextInspectionReport.Findings[].Kind` | `FindingType` |
| `TextInspectionReport.Categories` | `CategorySummaries` |
| `TextInspectionResult.Categories` | `CategorySummaries` |

Properties such as `Start`, `Length`, `Title`, `RuleKey`, `RiskLevel`, `Summary`, `Findings`, and `Highlights` retain their conventional names. Category-selector arrays in configuration and highlight span metadata continue to be called `Categories` because they are **not summary collections**. Policy condition `Kind` and diagnostic `Kind` also retain those terms.

## Before and after

```csharp
// Previous
var report = inspector.CreateReport(input);
// report.IsFlagged;
// report.SafetyInfo.PriorityScore;
// report.Categories;

// Updated
var report = inspector.CreateReport(input);
Console.WriteLine(report.RequiresReview);
Console.WriteLine(report.RiskAssessment.RiskScore);
foreach(var category in report.CategorySummaries)
    Console.WriteLine(category.Category);
```

## Serialization and persistence

The DTO/property names are emitted in JSON using the new C# names unless the application supplies its own naming policy. Old JSON payloads and stored rule/policy configurations should be explicitly migrated before loading under this API version. Update existing `rules.json` entries (`BaseWeight` to `RuleWeight`) and `policies.json` entries (`RawPoints` to `RiskPoints`); old fields are not maintained as API aliases. Also update database/export contracts and frontend TypeScript bindings that depend on these names.

This is a **naming migration**, not a scoring formula change. Pro may still produce a different assessment from Lite because it ships more rules and policies.
