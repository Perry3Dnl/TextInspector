# Multilingual detection accuracy benchmark — initial infrastructure

This is a **synthetic regression baseline**, not a certified accuracy benchmark.

## Files
- `tests/TextInspector.Tests/MultilingualAccuracyCorpusTests.cs` — xUnit JSON-driven harness
- `tests/TextInspector.Tests/accuracy-corpus-free.json` — 200 Free cases (EN/NL only)

Each case fixes an ID, language, development/evaluation split, input, rule key, expected lexical detection, expected scoring, expected original evidence text, provenance, and review status. The harness checks unique IDs, per-language counts and splits, literal evidence, risk-disposition, and source offsets.

Each language has ten ten-item groups: four positive literal-in-context patterns (40) and six Unicode word-boundary negative constructions (60). Each language contains an 80/20 deterministic partition. All inputs are `origin=synthetic-boundary` and `reviewStatus=unreviewed`; **neither partition is valid as an independent natural-language holdout**. Text templates and repeated vocabulary are shared across partitions; do not advertise 95% accuracy or any measured operational false-positive rate from their results.

## Human-labeling phase (not yet complete)
1. Independently write natural, language-specific sentences rather than expanding templated seed words. Include benign administrative, identity, documentation, slang, quoted, educational, negation, and genuine abusive contexts.
2. Use fluent reviewers for every language, with at least two separate judgments for disputed terms; record disagreements and adjudication.
3. Record per-rule *lexical evidence*, *contextual risk decision*, and *exact span* separately. Note that profanity lexicons intentionally produce evidence-only matches: offensive/benign usage cannot be inferred from a zero risk contribution.
4. Split by word families, source and speaker/context, not random paraphrase, to prevent leakage. Keep a sealed evaluation set.
5. Run against compiled package outputs, audit false positives and false negatives by language/rule, and calculate TP/FP/FN/TN, precision, recall, FPR and sample-size uncertainty.
6. Document failures first, then change generalized rule/boundary/context mechanisms. Re-run full existing regressions and sealed evaluation set. Do not tune against evaluation labels.

## Release gate
This scaffold is suitable to guard against lexical-boundary regressions. **Semantic multilingual accuracy remains unmeasured** until reviewed natural-language labels have been added and an independent holdout has been evaluated.
