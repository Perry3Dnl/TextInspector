# 1.1.0 edition interchangeability and release gates

## Product contract

- **Free is a complete, usable inspector** with its fixed embedded preset. Pro is an extension for custom rules, persistence, advanced evaluation and commercial licensing.
- Do **not** silently return empty results for unsupported Pro-only operations in Free. Use `NotSupportedException` with a clear upgrade message; methods already implemented by Free continue to operate normally.
- Preserve public constructor and method signatures, interface contracts, serialized DTOs, detection spans, and scores for **the same rule configuration and engine options** whenever the new features are absent.
- A package swap requires rebuilding against the other package (both use assembly name `TextInspector`). It cannot be promised as a transparent runtime binary swap.
- **Identical default detection outputs are not guaranteed:** Free and Pro ship different presets; Pro additionally has composite policies and URL intelligence. Making defaults identical would change established Pro behavior and must be designed and versioned separately, not silently applied in a release-hardening patch.
- Pro-only custom-rule APIs are not all exposed in Free. Exact compile-time interchangeability of every Pro application is **not yet supported**. Applications using Pro-only types or repositories cannot be expected to downgrade without adapting their use of those features.

## Implemented safeguards

- Original `TextSafetyInfo` constructor preserved alongside the extended linked-score constructor in both editions.
- `TextInspectorService()`, `TextInspectorService(TextInspectorOptions)`, and `TextInspectorEngine.Create(...)` provided in Pro for Free-style initialization. Existing repository constructors retained.
- Free offers `Analyze` and `MarkText`. Free `ReloadAsync` explicitly reports unavailable repository refresh without pretending the operation was successful.
- Numeric evaluator and LinkedRule evaluator are compared against the matching Free branch by Pro CI. Mainline validation keeps comparing main-to-main.
- Backwards compatibility tests cover legacy JSON, unchanged plain-rule scoring, and constructor availability; Pro additionally exercises Free-compatible construction and ASP.NET Core dependency injection.
- The earlier `RuleType.Quantity` prototype in medicine PR #8 is superseded for the numeric engine. Do not merge its competing implementation unchanged.

## Mandatory pre-release checks (not all completed)

- [ ] Merge or rebase the feature branches safely, including Free/Pro parity, with no remaining competing quantity engine.
- [ ] Compare full public API signatures of both packages to each previous published version; do not break existing external implementations of interfaces.
- [ ] Define and test exactly which common API calls must produce identical outputs with the **same config**; compare the old published baseline and candidate with a stored fixture corpus.
- [ ] Define a versioned shared baseline preset or an explicitly named compatibility mode **if identical default outputs across editions are required**. This is not yet implemented.
- [ ] Decide whether Pro-only API placeholders belong in Free: stubs must retain signatures and return types, throw on use, and must not compromise Free package completeness or third-party interface implementations.
- [ ] Test clean packed consumers for both directions (Free -> Pro and Pro -> Free using common API only) across supported target frameworks, plus ASP.NET Core and EF6 integration.
- [ ] Run complete tests, coverage >=95%, build, pack, and shared-source parity gates on the final combined revision in **both** repositories.
- [ ] Review product documentation and update changelog without publishing.

**Status:** Development branches only. This document does not certify edition parity, release readiness, or identical default scores.

## Historical public interface compatibility blocker

The Free `ITextInspectorService` and `ITextInspectorDetectionService` interfaces have gained members for Pro parity. Since the core targets `netstandard2.0`, existing third-party implementations may stop compiling or loading against this candidate. Bidirectional reflection parity between the two 1.1.0 editions does **not** establish binary compatibility with previously published packages. Treat this as a release blocker until a historical consumer ABI check passes or the API design is revised without breaking older implementations.
