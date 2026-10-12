# Migrating to TextInspector

TextInspector is intentionally a smaller edition, not a source-compatible replacement for the full authoring and persistence API.

- Reference TextInspector and optionally TextInspector.AspNetCore.
- Replace Compile(LoadOptions(), LoadRules(), LoadPolicies()) with TextInspectorEngine.Create().
- Replace custom repositories/service constructors with TextInspectorService(TextInspectorOptions).
- AddTextInspector accepts TextInspectorOptions; rule and policy collections cannot be supplied.
- Remove file/EF registrations, reload/save calls, targeted match helpers and parallel batch helper calls; those remain in the full edition.
- Use EnableMasking and MaskCharacter for basic all-match masking. Filtered masking belongs to Pro.
- PolicyMatches and UrlObservations remain empty for schema continuity. Category reports and exact evidence/highlights remain supported.
- The 76-rule TextInspector preset preserves the current calibrated baseline; the full preset has broader coverage. Scores and findings can therefore differ between editions.
- main and pro retain the complete implementation. Replace TextInspector package references to upgrade; the editions share assembly names and should not be referenced together.

The manifest records the first standalone imported baseline and the explicit basic additions. No new Unclaimable port is performed by this split.
