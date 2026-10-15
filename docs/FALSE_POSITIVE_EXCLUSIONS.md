# TextInspector false-positive exclusions

TextInspector supports a small, deterministic false-positive filter for **verified benign matches**. This is distinct from Unclaimable's identifier-oriented sensitivity filter.

An exclusion contains a `RuleKey` and an **exact matched-text value** (`MatchedText`). During full `Inspect`, the engine compares the original evidence span using case-insensitive ordinal matching. Only that exact hit from that rule is excluded. Other matches, other rules, and all associated source offsets remain unchanged. Excluded hits do not reach context evaluation, policies, scoring or highlighting.

No exclusion is enabled by default. The application chooses exceptions after testing them against legitimate risky examples. This does not attempt to infer message intent from punctuation, quotation marks, or a blanket benign-words catalog.

## Configuration

```csharp
// Free
var free = TextInspectorEngine.Create(new TextInspectorOptions
{
    FalsePositiveExclusions = new[]
    {
        new TextInspectorFalsePositiveExclusion
        {
            RuleKey = "YourBuiltInRuleKey",
            MatchedText = "known benign phrase"
        }
    }
});

// Pro
var settings = TextInspectorPresets.LoadOptions();
settings.FalsePositiveExclusions = new[]
{
    new TextInspectorFalsePositiveExclusion
    {
        RuleKey = "YourBuiltInRuleKey",
        MatchedText = "known benign phrase"
    }
};
var pro = TextInspectorEngine.Compile(settings,TextInspectorPresets.LoadRules(),TextInspectorPresets.LoadPolicies());
```

Replace illustrative rule keys with keys in your actual installed preset. A rule key is mandatory to avoid suppressing an unrelated signal with the same text.

**Scope:** This first implementation filters evidence returned by full `Inspect`, `CreateReport`, and their scoring/highlighting pipeline. Fast raw `HasMatch`, `FindFirstMatch`, and `FindMatches` deliberately report primary regex hits without this post-match filtering; they have always skipped full inspection scoring/context. Use `Inspect` for filtered decisions, not raw matching shortcuts.

Both editions implement the same shared matching change. The free public `TextInspectorOptions` exposes exclusions while Pro also supports its existing scoring-options configuration. No language sensitivity switch or substantial engine rewrite is introduced.

## Attributed quotation scoring (TDD follow-up)

The shared context-scoring pipeline now distinguishes a **direct threat** from a threat reproduced in an explicitly attributed quotation. A bounded nearby attribution cue (e.g., a report, transcript, fictional script, moderator record, or similar documentation) must introduce the opening quotation mark, and the matched span must end at the closing quotation mark. Only the quoted threat evidence is marked context-excluded for scoring.

The evidence and source offsets remain available in `Inspect` and highlighting; a separate direct threat in the same message remains scored. A quotation alone, without an attribution cue, is **not** treated as innocent. This is a deliberately narrow heuristic, not a general classifier of intent or a blanket suppression of harmful text. Applications should not equate suppressed scoring with a safety guarantee.

The TDD cases are in `tests/TextInspector.Tests/ReportedThreatScoringTests.cs`: three previously failing direct-versus-reported comparisons, direct threat controls, unattributed quotation, and simultaneous reported/direct threats. The fix is shared between Free and Pro.

## Context calibration and multilingual checks

The TDD sequence now includes `ContextCalibrationTests`, `NegatedCredentialScoringTests`, and `MultilingualCalibration120Tests` in both Free and Pro. An explicit negated credential request (for example, "do not ever send your password") originally produced risk from the matched request. The shared context-scoring code now preserves that precise evidence but marks the occurrence context-excluded when a local grammatical negation immediately precedes a `Credential_*` rule match. The bounded regex accepts "do not", "don't", "never", "should not", and "must not", with one optional "ever". A separate non-negated request still scores. This is not a broad promise to infer all negation or intent, especially across languages.

Explicitly attributed educational quotations of threat phrases are covered by the existing bounded quotation handling; unquoted educational prose is not automatically exempt. Attribution can be manipulated, so the result is an interpretable scoring heuristic, not a guarantee that the message is safe.

The multilingual corpus contains **120 named-rule cases**: 80 non-threatening examples spanning Dutch, German, Spanish, French, Italian, Portuguese, Swedish and Polish, plus 40 mixed-language controls containing a direct English threat. These measure unintended matches of the *English* `Threat_Violence` rule against other languages and ensure a clear English threat remains visible amid them. They **do not** establish native-language threat recall, nor statistically representative precision or false-positive rates. Additional labeled vocabulary-specific cases are needed before making those claims.
