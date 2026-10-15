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
