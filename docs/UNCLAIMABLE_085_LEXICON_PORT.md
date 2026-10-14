# Unclaimable 0.8.5 lexical data adaptation for TextInspector

## Scope and provenance

This adapts **v0.8.5 vocabulary data**, not Unclaimable's reserved-name blocking/matching engine. TextInspector remains an evidence-producing inspection system with original text spans, contextual gating, explainable scoring and UI-ready reports.

Source vocabulary: `Perry3Dnl/Unclaimable` at v0.8.5, `platforms/dotnet/src/Unclaimable.Extended/Data/slangprofanity-*.json` and `hatespeech.json`. All imported source entries are captured in the checked-in test fixture `tests/TextInspector.Tests/lexicon-085-fixtures.json` for reproducibility.

## Edition split

| Dimension | TextInspector | TextInspector Pro |
| --- | --- | --- |
| Original preset rules and scoring | Retained | Retained and extended |
| English slang/profanity lexical terms | 300 | 300 |
| Dutch slang/profanity lexical terms | 302 | 302 |
| Other slang/profanity languages | None | 13 extra; 3,433 terms |
| Supplementary hateful expressions | Curated 20 phrase subset | Full 112 expressions |
| Imported lexical evidence scoring | Evidence-only | Evidence-only |
| Existing custom rules and composite policies | No | Yes |

The 15 Pro languages: EN, NL, DE, FR, ES, IT, PT, PL, TR, ID, CS, VI, HU, SV, RO.

The imported profanity lexicons use `AbusiveLanguage`, a new category with stable numeric enum value 21. Hateful expressions use `HateSpeech`. Merely detecting ordinary profanity is *not* equivalent to hateful content, a threat, or policy breach.

The entries are part of the **existing fixed rule/evidence pipeline**, stored in `Presets/rules.json` with stable keys and GUIDs. We do not import Unclaimable's false-positive sensitivity API or partial-username indexing. The lexical rules are explicitly `MaxCountForScoring: 0` and `RuleWeight: 0`; they cannot add risk points directly.

The expression boundary `(?<![\\p{L}\\p{N}_])` / `(?![\\p{L}\\p{N}_])` avoids triggering on the interior of ordinary Unicode identifiers. Terms are sorted longest-first before generating regex alternatives. The existing normalization-to-original-text mapping remains responsible for exact highlighting and offsets.

## Test coverage and safe rollout

- Per-edition fixture audit exercises **every imported source entry** against the compiled expression and checks the expected rule, category, and non-scoring contract.
- Smoke tests verify full inspection, source offsets, highlight spans, no-risk semantics and an embedded-identifier negative case.
- Existing category and rule manifest tests are updated, without deleting the baseline rule contract.
- CI must pass on net8.0 and net10.0, and packages must build for netstandard2.0, net8.0 and net10.0.
- Benchmark cold startup, warm engine creation, allocations, throughput for no-match and multilingual inputs, and regex timeout incidence before approving this feature.
- Run a separate calibration against benign multilingual content and quotation/research contexts; lexical evidence should not falsely be interpreted as automatic enforcement.
- Verify cross-project free baseline parity and redistribution/license compliance before merge or NuGet publication.

## Follow-on architecture

The current import intentionally reuses TextInspector's regex-based rule interface. This is a complete **data port for the selected categories**, but it is not an optimized indexed lexicon engine. A dedicated literal matcher may be justified by profiling, but should only be integrated if it preserves raw matching, exact UTF-16 source spans, normalized-source mapping, first-match fast paths, score accounting, context and reports. It must not be implemented by copying Unclaimable's checker.

**Status:** draft PR, not production release approval.
