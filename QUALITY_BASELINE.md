# Lite quality baseline

The full-edition evaluation/benchmark figures do not describe Lite. They remain in the separate [Pro repository](https://github.com/Perry3Dnl/TextInspectorPro) and are intentionally not presented here as Lite accuracy or latency claims.

Lite retains internal regression cases for benign text, scoring positives, evidence-only observations, evasion/source mapping, compiler diagnostics, HTML encoding, masking and category reports. Edition-boundary tests verify the fixed 76-rule manifest, removed Pro implementations and absence of public rule-authoring entry points.

CI builds all package target frameworks, tests on .NET 8 and .NET 10, executes core/DI smoke tests and installs the packed Lite package in a clean consumer. Code coverage is collected; no inherited percentage is claimed for this edition.
