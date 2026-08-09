# Architecture decision records

* [ADR-0001: A lightweight knowledge base with RQ-ID traceability](ADR-0001-lightweight-knowledge-base.md) - Requirements are the only identified artifacts, traceability is derived by search, and decision records are immutable.
* [ADR-0002: Defer a knowledge base format validator](ADR-0002-defer-format-validator.md) - Superseded by ADR-0004. Conventions were left to review rather than enforced by a ported PowerShell validator.
* [ADR-0003: Adopt the Open Knowledge Format for the knowledge base](ADR-0003-adopt-open-knowledge-format.md) - `knowledge/` is an OKF v0.2 bundle rather than a house style.
* [ADR-0004: Check knowledge base conformance in CI, not with a ported validator](ADR-0004-conformance-check-in-ci.md) - What gets checked mechanically once CI lands, and what stays with review.
* [ADR-0005: Use CsCheck for property-based testing](ADR-0005-property-based-testing-library.md) - A C#, dependency-free library, chosen over FsCheck so the test toolchain stays in one language and pins no runner version.

Each record's current state is its `status` in frontmatter, not a word repeated here — a
status copied into a listing is wrong the first time it changes. A supersession is the
exception: it never flips back, and a reader scanning this list needs to see it.

# Writing one

* [ADR template](adr-template.md) - Skeleton to copy, with the frontmatter and the three sections to fill in.
* [Knowledge policy](../process/knowledge-policy.md) - Numbering, status values, supersession, and how much belongs in one record.
