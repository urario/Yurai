---
name: yurai-detailed-design
description: Check whether a Yurai issue or design is implementation-ready before coding. Use when contracts, public API decisions, data flow, failure behavior, compatibility, dependencies, or test seams may be incomplete or ambiguous.
---

# Yurai detailed-design check

Decide whether implementation can proceed without inventing a requirement or taking a
reserved decision. This is an implementation-readiness check, not an authority to change
Yurai's architecture or public API.

Read the issue, [`AGENTS.md`](../../../AGENTS.md), the relevant entries under
[`knowledge/`](../../../knowledge/index.md), and the requirement registry before making
the check. Where no registered requirement exists yet, use only the issue's explicit
scope and say that the registry could not be used.

## Check the contract

Confirm that the implementation has all of the following:

- a goal and acceptance criteria that distinguish success from failure;
- explicit in-scope behavior and non-goals;
- recorded public types, members, signatures, and semantics when the public surface
  changes;
- defined inputs, outputs, ownership, ordering, and state transitions where applicable;
- failure and edge-case behavior, including overflow and compatibility constraints;
- the `netstandard2.0` and zero-runtime-dependency constraints preserved;
- a test seam and an identified example or property test strategy;
- registered `RQ-NNN` identifiers and their required test traits when available;
- no conflict with an ADR, the execution plan, or Yurai's fixed vocabulary.

Avoid speculative detail. Require only decisions necessary to implement and verify the
issue safely.

## Choose one outcome

**Ready:** State the contract, affected boundary, test approach, and relevant records in
a short implementation note, then proceed with
[`yurai-implementation`](../yurai-implementation/SKILL.md).

**Needs design clarification:** Finish every independent check, then post the precise
gap and its implementation impact to the issue. Hand the next action to `owner:claude`
when requirements or design need elaboration.

**Needs a reserved decision:** Present realistic options, trade-offs, and a
recommendation. Stop before implementation and hand the issue to `owner:human`, as
required by `AGENTS.md` section 2.

Do not resolve missing design through direct agent conversation. GitHub issues, pull
requests, and durable `knowledge/` records are the handoff surfaces.
