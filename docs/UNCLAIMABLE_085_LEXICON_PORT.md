# Unclaimable 0.8.5 lexical evidence adaptation (review candidate)

This is a **TextInspector-specific adaptation**, not a copy of Unclaimable's identifier rejection pipeline. Source vocabulary was taken from the Unclaimable v0.8.5 language JSON datasets, then filtered to a small initial, reproducible subset (terms containing at least five UTF-16 code units, first 45 per language entries in source order). This is a deliberately incomplete initial import, not a claim of full 0.8.5 dataset coverage.

Original source: https://github.com/Perry3Dnl/Unclaimable

## Edition boundary

- TextInspector: English and Dutch evidence-only vocabulary.
- TextInspector Pro: the same English/Dutch vocabulary, plus 13 additional language groups (DE, FR, ES, IT, PT, PL, TR, ID, CS, VI, HU, SV, RO).
- Both retain their existing scoring and report machinery. A lexical match has `MaxCountForScoring: 0`, so it contributes no risk points by itself.
- New `AbusiveLanguage = 21` is an additive category; do **not** mix generic swearing into `HateSpeech`.

## Review gates before merging

1. Verify dataset provenance and redistribution rights in both package licenses.
2. Build/test all supported frameworks and run packaged-consumer smoke tests.
3. Run category/score regression tests against ordinary multilingual prose, names, quotations, and safe false-positive controls.
4. Benchmark cold-start, allocation, compiled regex construction, common no-match throughput, and peak working set.
5. Evaluate a dedicated literal lexicon matcher before expanding beyond this small candidate set; do not insert tens of thousands of literals into regex or silently change the core engine.
6. Preserve source spans and report highlighting across every imported language; test Unicode and multiword terms.
7. Synchronize the common rules and public category enum across both editions. Publish only with a separately approved matching version policy.

These are feature branches pending CI and design review, **not release-ready changes**.
