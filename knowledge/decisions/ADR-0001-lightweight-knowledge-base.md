---
type: ADR
title: A lightweight knowledge base with RQ-ID traceability
description: Requirements are the only identified artifacts, traceability is derived by search, and decision records are immutable.
tags: [knowledge, traceability, adr]
status: stable
generated: { by: claude-code/2026-08, at: 2026-08-08T22:20:00Z }
sources:
  - id: issue-8
    resource: https://github.com/urario/Yurai/issues/8
    title: "Issue #8: bootstrap the knowledge base"
---

# ADR-0001: A lightweight knowledge base with RQ-ID traceability

## Context

Yurai is developed by a maintainer working with two AI agents that never talk to each
other: everything one knows, the other has to read off GitHub
([AGENTS.md §3](../../AGENTS.md#3-agents-communicate-through-github-never-directly)).
That makes written records load-bearing rather than ceremonial. An agent picking up
[#17](https://github.com/urario/Yurai/issues/17) six weeks from now sees the repository
and the issue tracker, and nothing else.

Issues and pull requests alone do not carry that weight. An issue is a working surface
and stops being read once it closes; a pull request describes one change and is frozen
the moment it merges. Neither answers "why is the API shaped like this" for someone who
arrives later.

The process assets being adapted here come from a larger internal project (Surveyor),
which identifies every artifact class — design, implementation, unit test, integration
test, traceability record — and validates the result with a script. That system is built
for a much bigger surface than a dependency-free `netstandard2.0` library with a
deliberately small public API. Yurai's constraints cut the other way: the whole project
is meant to be understandable in an afternoon, and process that outweighs the code it
governs would contradict what the library is trying to be.

So the question is not whether to write things down. It is how little structure is
enough to keep design decisions findable and requirements connected to the tests that
prove them.

## Decision

Yurai keeps a `knowledge/` directory with four subdirectories — `requirements/`,
`decisions/`, `design/`, `process/` — described by
[`knowledge/index.md`](../index.md) and governed by
[the knowledge policy](../process/knowledge-policy.md). Information is split three ways:
issues hold working state, pull requests hold changes and their verification, and
`knowledge/` holds what stays true after both are closed.

Three choices inside that, each of which had a heavier alternative. They are recorded
together because they are one structural decision seen from three sides — take away any
one of them and the other two stop making sense:

**Requirements are the only identified artifacts.** `RQ-###` identifies a requirement;
design documents, modules, and test cases have no identifiers of their own. They refer
to requirements instead, by the mechanism in
[traceability](../process/traceability.md). The full artifact-identifier system
(`DES`/`IMP`/`UT`/`IT`/`TRC`) is not adopted. It buys precise cross-referencing at the
cost of an identifier on every artifact and a discipline to keep them consistent; at
this size, the pull request and the ADR already say what those identifiers would say,
and a scheme nobody maintains is worse than no scheme.

**Traceability is derived, not maintained.** No hand-written matrix. The link from a
requirement to the test that establishes it is a requirement trait on the test, found by
search or by `dotnet test --filter`. A matrix is a second source of truth that is only
correct on the day it is written.

**Decisions are recorded one per file, and are never rewritten.** An ADR that no longer
holds is superseded by a new one and keeps its text. The abandoned reasoning is the part
future readers need most — it is what tells them whether the context has changed.

Whether these conventions are also enforced by a script is a separate question, decided
separately in [ADR-0002](ADR-0002-defer-format-validator.md) — it is the part of this
design most likely to change on its own, and binding it to the structure above would
mean superseding the structure to revisit the tooling.

## Consequences

Design rationale gets a home that survives issue closure, and a newcomer has one
entry point instead of an issue archive to read. Requirements connect to tests through a
single convention that costs one attribute per test.

The costs are real and accepted:

- **Coverage is only as good as the tagging.** Nothing forces a test to carry its
  requirement trait, so a P0 requirement can look untested when it is merely untagged.
  The gate reviews ([#29](https://github.com/urario/Yurai/issues/29)) are where that is
  caught, which means it is caught late rather than continuously.
- **Cross-referencing is coarser** than an identifier-per-artifact scheme would give.
  Asking which code satisfies a given requirement is answered by reading the tests
  tagged with it, not by a lookup.
- **Identifiers only work if nothing counterfeits them.** Because a search for
  `RQ-` followed by three digits is the retrieval mechanism, an example identifier in a
  document is indistinguishable from a real reference. The placeholder convention in
  [traceability](../process/traceability.md#the-identifier) exists for that reason and
  has to hold in every document written from here on.
- **`knowledge/` is now a maintenance obligation.** A stale knowledge base is worse than
  none, because it is believed. The policy's rule — the pull request that changes
  reality updates the document in the same pull request — is what keeps that from
  happening, and it only works if reviewers hold it.

The residual risk named in [#8](https://github.com/urario/Yurai/issues/8) stands: this
may prove too thin. The Phase 2 gate review is where that gets examined against how the
project actually ran, with the heavier options above still available one ADR at a time.
