# Changelog

## 1.1.0 — 2026-10-15

- Add the shared numeric capture-condition evaluator and boundary regression tests used by Pro's configurable rules. The fixed Free preset and public configuration surface remain unchanged.
- Add the English/Dutch lexical evidence port from the validated v0.8.5 source set, bringing the built-in preset to 79 rules while keeping imported lexical signals evidence-only.
- Add exact, rule-scoped false-positive exclusions for scored inspection without changing raw matching behavior.
- Add calibrated handling for locally negated credential requests and attributed/quoted threat examples.
- Add adversarial, context, multilingual-boundary, and frozen lexical regression corpora to protect precision as the detector evolves.
- Keep TextInspector and TextInspector.AspNetCore on the same 1.1.0 package version and shared engine line.

# TextInspector changelog

## 1.0.2 — 2026-10-13

- Release TextInspector and TextInspector.AspNetCore at matching version 1.0.2.
- Refresh both NuGet READMEs and installation examples.
- Add release-time package identity/version and NuGet publication verification so a partial release cannot report success.
- No changes to detection, scoring, public APIs or the 76-rule preset.


## 1.0.1 — 2026-10-13

- Publish separate GitHub and NuGet READMEs for both the core and ASP.NET Core packages.
- Correct image rendering in NuGet with an absolute, versioned image URL.
- Present TextInspector as a fully functional standalone product; remove premature Pro comparison and promotion from package documentation.
- Update package version and documentation examples to 1.0.1 without changing inspection behavior, public APIs or the 76-rule preset.


## 1.0.0 — 2026-10-10

- Rename the free product to TextInspector and use distinct TextInspector / TextInspector.Pro NuGet package identities; preserve namespaces, assemblies, presets, and scoring.

- Set TextInspector packages to stable version 1.0.0.
- Preserve the established 76-rule preset, public API naming, and report schema.
- Retain 95% core line coverage enforcement and clean-consumer package validation.
- Publication remains subject to final release-commit CI and package review.

## 0.9.0-preview.1

- Select 76 embedded rules: the 70-rule original standalone baseline with current calibration, plus six explicit basic rules.
- Preserve deterministic local inspection, independent detection/severity/risk values, exact spans, contextual safety gates, base/bonus scoring, category reports and linked highlights.
- Add TextInspectorEngine.Create and a small TextInspectorOptions surface with shipped-rule/category opt-outs and basic all-match masking.
- Keep raw HasMatch/FindMatches and sequential batch inspection.
- Remove composite policy evaluation, full URL intelligence, filtered masking, custom authoring helpers, targeted scans, parallel helper and repeated-capture configuration.
- Remove file/EF Core/EF6 repositories, mutation/reload APIs, atomic persistence and refresh helpers.
- Publishable package IDs are TextInspector and TextInspector.AspNetCore; retain TextInspector namespaces and assembly names.
- Provide independent TextInspector tests, package smoke checks, documentation, and a preset manifest. Pro is maintained in its own repository.

The release workflow builds packages for verification; it does not publish them to NuGet.
