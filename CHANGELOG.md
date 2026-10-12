# TextInspector Lite changelog

## 1.0.0 — 2026-10-10

- Set Lite packages to stable version 1.0.0.
- Preserve the established 76-rule preset, public API naming, and report schema.
- Retain 95% core line coverage enforcement and clean-consumer package validation.
- Publication remains subject to final release-commit CI and package review.

## 0.9.0-lite-preview.1

- Select 76 embedded rules: the 70-rule original standalone baseline with current calibration, plus six explicit basic rules.
- Preserve deterministic local inspection, independent detection/severity/risk values, exact spans, contextual safety gates, base/bonus scoring, category reports and linked highlights.
- Add TextInspectorEngine.Create and a small TextInspectorLiteOptions surface with shipped-rule/category opt-outs and basic all-match masking.
- Keep raw HasMatch/FindMatches and sequential batch inspection.
- Remove composite policy evaluation, full URL intelligence, filtered masking, custom authoring helpers, targeted scans, parallel helper and repeated-capture configuration.
- Remove file/EF Core/EF6 repositories, mutation/reload APIs, atomic persistence and refresh helpers.
- Publishable package IDs are TextInspector.Lite and TextInspector.Lite.AspNetCore; retain TextInspector namespaces and assembly names.
- Replace full-edition CI/docs with Lite-specific tests, package smoke checks and a manifest. Full implementation and history remain on main/pro.

This change prepares the Lite branch; it does not publish a NuGet release.
