# Requirements

- **Status:** Registry established; entries pending [#12](https://github.com/urario/Yurai/issues/12)
- **Issue:** [#8](https://github.com/urario/Yurai/issues/8)

The registry of Yurai's requirements. Every `RQ-###` identifier used anywhere in the
repository is defined here, and an identifier that is not in this table does not exist.

It is empty on purpose. The requirements specification is derived from the project
proposal in [#12](https://github.com/urario/Yurai/issues/12) — the MVP scope, the
explicit non-goals, and the P0 requirements R1–R6 — and lands as rows in the table
below plus the detail sections beneath it. What this document fixes now is the shape
those rows take, so that the work in [#12](https://github.com/urario/Yurai/issues/12)
has a target and the identifier convention has
somewhere to point.

The identifier rules — three digits, never reused, split by supersession — are in
[traceability](../process/traceability.md).

## Registry

| ID | Requirement | Priority | Status | Verified by |
|---|---|---|---|---|
| _(none yet)_ | | | | |

- **Priority** — `P0` must ship in v1.0; `P1` and `P2` are wanted but do not block a
  release.
- **Status** — `Draft`, `Accepted`, `Withdrawn`, or `Superseded by RQ-###`.
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

## Also here

The project proposal that the requirements are derived from is stored in this directory
by [#12](https://github.com/urario/Yurai/issues/12), so that the source of a requirement
stays readable next to the requirement itself. The proposal is the origin of the
specification, not a parallel copy of it: where the two differ, this registry is what
binds.
