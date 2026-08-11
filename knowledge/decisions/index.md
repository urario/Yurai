# Architecture decision records

* [ADR-0001: A lightweight knowledge base with RQ-ID traceability](ADR-0001-lightweight-knowledge-base.md) - Requirements are the only identified artifacts, traceability is derived by search, and decision records are immutable.
* [ADR-0002: Defer a knowledge base format validator](ADR-0002-defer-format-validator.md) - Superseded by ADR-0004. Conventions were left to review rather than enforced by a ported PowerShell validator.
* [ADR-0003: Adopt the Open Knowledge Format for the knowledge base](ADR-0003-adopt-open-knowledge-format.md) - `knowledge/` is an OKF v0.2 bundle rather than a house style.
* [ADR-0004: Check knowledge base conformance in CI, not with a ported validator](ADR-0004-conformance-check-in-ci.md) - What gets checked mechanically once CI lands, and what stays with review.
* [ADR-0005: Use CsCheck for property-based testing](ADR-0005-property-based-testing-library.md) - A C#, dependency-free library, chosen over FsCheck so the test toolchain stays in one language and pins no runner version.
* [ADR-0006: Represent derivation evidence as an immutable evaluated DAG](ADR-0006-immutable-evidence-dag.md) - Evidence nodes keep evaluated values and immutable child references, with structural sharing and reference identity.
* [ADR-0007: Use a root-only traced value carrier](ADR-0007-root-only-traced-value-carrier.md) - A readonly value carrier keeps one evidence root as the single source of its evaluated value and derivation.
* [ADR-0008: Evaluate native values before creating derivation evidence](ADR-0008-native-first-evaluation.md) - Each operation executes the native decimal operation once and creates evidence only after successful evaluation.
* [ADR-0009: Defer multi-type targeting until a second value type is approved](ADR-0009-defer-multi-type-targeting.md) - Superseded by ADR-0018.
* [ADR-0010: Evaluate only the selected conditional alternative](ADR-0010-use-lazy-selected-only-branches.md) - Conditional operations use lazy alternatives and record only the selected derivation.
* [ADR-0011: Exclude plain boolean control dependencies from the v1 graph](ADR-0011-exclude-plain-boolean-control-dependencies.md) - Dependency queries cover recorded value derivation, not condition-only control dependency.
* [ADR-0012: Use explicit mixed decimal operator overloads](ADR-0012-use-explicit-mixed-decimal-overloads.md) - Both operand orders retain ordinary notation without an implicit conversion into the traced region.
* [ADR-0013: Publish a versioned stable JSON schema](ADR-0013-publish-versioned-json-schema.md) - JSON export is a compatibility contract whose breaking changes require a new schema version.
* [ADR-0014: Encode decimal values as invariant JSON text](ADR-0014-encode-decimal-as-invariant-json-text.md) - JSON preserves decimal value and scale without relying on consumer number precision.
* [ADR-0015: Use document-local identities for shared evidence](ADR-0015-use-document-local-output-identities.md) - Deterministic numeric IDs identify shared nodes consistently in text and JSON.
* [ADR-0016: Name the non-generic v1 carrier Traced](ADR-0016-name-the-v1-carrier-traced.md) - Superseded by ADR-0018.
* [ADR-0017: Fold the creation methods onto the carrier](ADR-0017-fold-creation-onto-the-carrier.md) - Superseded by ADR-0018; its namespace-collision finding remains binding.
* [ADR-0018: Introduce a closed-set generic Traced carrier for decimal and Int64](ADR-0018-introduce-closed-set-generic-traced-carrier.md) - Yurai 0.2.0 supports decimal and Int64 through one generic carrier and an inference companion, with homogeneous typed evidence and JSON schema v2.

Each record's current state is its `status` in frontmatter, not a word repeated here — a
status copied into a listing is wrong the first time it changes. A supersession is the
exception: it never flips back, and a reader scanning this list needs to see it.

# Writing one

* [ADR template](adr-template.md) - Skeleton to copy, with the frontmatter and the three sections to fill in.
* [Knowledge policy](../process/knowledge-policy.md) - Numbering, status values, supersession, and how much belongs in one record.
