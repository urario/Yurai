---
type: Requirements Registry
title: Requirements registry
description: The single registry of Yurai's RQ-### requirement identifiers, with priorities, statuses, and acceptance criteria.
tags: [requirements, traceability]
status: draft
generated: { by: claude-code/2026-08, at: 2026-08-08T22:49:00Z }
sources:
  - id: issue-8
    resource: https://github.com/urario/Yurai/issues/8
    title: "Issue #8: bootstrap the knowledge base"
  - id: issue-12
    resource: https://github.com/urario/Yurai/issues/12
    title: "Issue #12: write the requirements specification"
---

# Requirements registry

The registry of Yurai's requirements. Every `RQ-###` identifier used anywhere in the
repository is defined here, and an identifier that is not in this table does not exist.

It is empty on purpose. The requirements specification is derived from the project
proposal in [#12](https://github.com/urario/Yurai/issues/12) — the MVP scope, the
explicit non-goals, and the P0 requirements R1–R6 — and lands as rows in the table below
plus the detail sections beneath it. What this document fixes now is the shape those
rows take, so that the work in #12 has a target and the identifier convention has
somewhere to point. Its `status` stays `draft` until it holds real entries.

The identifier rules — three digits, never reused, split by supersession — are in
[traceability](../process/traceability.md).

## Registry

| ID | Requirement | Priority | Status | Verified by |
|---|---|---|---|---|
| _(none yet)_ | | | | |

- **Priority** — `P0` must ship in v1.0; `P1` and `P2` are wanted but do not block a
  release.
- **Status** — `Draft`, `Accepted`, `Withdrawn`, or `Superseded by RQ-NNN`. This column
  tracks the requirement; the document's own `status` in frontmatter tracks the registry.
- **Verified by** — how the requirement is demonstrated: tests carrying its trait, a
  benchmark, or a review at a gate issue. Named in kind, not enumerated case by case;
  the tests themselves are found through the trait
  ([traceability](../process/traceability.md)).

Each requirement then gets a section below the table with its full statement and its
acceptance criteria — enough that two people reading it independently would agree on
whether it holds.

## Non-goals

Things Yurai deliberately does not do are registered here as well, as requirements
phrased in the negative, and carry identifiers like any other. They are a large part of
what keeps the public surface small, and an unwritten non-goal gets re-proposed every
few months.

## The proposal

The project proposal that the requirements are derived from is stored in this directory
by [#12](https://github.com/urario/Yurai/issues/12), so that the source of a requirement
stays readable next to the requirement itself. The proposal is the origin of the
specification, not a parallel copy of it: where the two differ, this registry is what
binds.
