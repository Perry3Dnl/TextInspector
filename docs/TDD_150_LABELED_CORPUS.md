# TDD: 150-case labeled detector baseline

The dataset in `tests/TextInspector.Tests/BuiltInLabeledCorpus150Tests.cs` was written **before** detector changes and is shared between Free and Pro.

- **150 independently executed xUnit theory cases** (`C001`–`C150`).
- **90 positive cases**, where the named shipped rule must match.
- **60 negative cases**, where the named shipped rule must not match.
- Cases span threats, administrator/support claims, normal documentation, reviews of fiction, URLs and email indicators.
- Each of 75 base phrases appears standalone and embedded in a longer sentence; thus this is **not** 150 independent linguistic phrases.
- Expected outcomes are scoped to a named rule, not an assertion that the complete message is safe or unsafe. This prevents unrelated new rules from invalidating tests.
- The corpus deliberately does not define ground-truth overall moderation risk, precision or recall. Do not present this as a statistically representative estimate of false-positive rates.
- Avoid modifying labels in response to failures. Diagnose boundary matching, normalization, rule specificity, context gating, or scoring first; add adversarial regressions for the identified issue. Apply any shared engine fix to **both** editions. Only adjust edition-specific rules if the root cause is a preset rule.
- These rows are distinct from `FalsePositiveCorpusTests` which test synthetic rule-scoped exclusions and protection of independent risky evidence.

## Reproduce

```bash
dotnet test tests/TextInspector.Tests/TextInspector.Tests.csproj --filter FullyQualifiedName~BuiltInLabeledCorpus150Tests
```

A green run establishes agreement with this curated baseline, **not** evidence that the detector has no broader false positives. Expand language and category coverage using verified human-reviewed labels before claiming measured precision.
