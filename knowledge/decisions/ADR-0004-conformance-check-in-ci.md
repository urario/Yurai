---
type: ADR
title: Check knowledge base conformance in CI, not with a ported validator
description: Supersedes ADR-0002 — the deferral now ends when CI lands rather than when the conventions have proven themselves.
tags: [knowledge, tooling, ci, adr]
status: stable
generated: { by: claude-code/2026-08, at: 2026-08-08T22:49:00Z }
sources:
  - id: issue-8
    resource: https://github.com/urario/Yurai/issues/8
    title: "Issue #8: bootstrap the knowledge base"
  - id: issue-6
    resource: https://github.com/urario/Yurai/issues/6
    title: "Issue #6: build, test, and format CI"
---

# ADR-0004: Check knowledge base conformance in CI, not with a ported validator

## Context

[ADR-0002](ADR-0002-defer-format-validator.md) deferred all mechanical enforcement of the
knowledge base conventions, on two grounds: porting a PowerShell validator would put the
first non-`dotnet`, non-`git` tool into the repository, and enforcing conventions that had
not survived contact with real documents risked freezing a guess.

[ADR-0003](ADR-0003-adopt-open-knowledge-format.md) removed the second ground. The
conventions are no longer Yurai's guess — they are OKF v0.2, specified and versioned
elsewhere, with a conformance section written to be checked. What is left is not "should
these rules be enforced" but "when, and by what".

The timing question is real because CI does not exist yet
([#6](https://github.com/urario/Yurai/issues/6)). There is nowhere for a check to run
except a developer's shell, and a check that only runs when someone remembers is not a
check.

## Decision

Two properties are checked mechanically, in CI, once CI exists:

1. **OKF conformance.** Every non-reserved `.md` file under `knowledge/` parses as YAML
   frontmatter with a non-empty `type` (OKF v0.2 §11).
2. **No phantom identifiers.** Every string matching the `RQ-###` format anywhere in the
   repository names a requirement registered in
   [`requirements/registry.md`](../requirements/registry.md) — the invariant that makes
   traceability-by-search mean anything
   ([traceability](../process/traceability.md#the-identifier)).

Both are a few lines of shell over `grep` and a YAML parse, added to the existing
workflow. Neither is a port of the PowerShell validator, and neither is a general-purpose
OKF linter: this repository needs two assertions, not a tool.

Until CI exists, both stay review's job. The identifier half of that is already a search
a reviewer can run in one line ([traceability](../process/traceability.md)); the
frontmatter half needs a YAML parse, which is exactly why it belongs in a workflow rather
than in a reviewer's habits.

## Consequences

The window in which a malformed document can reach `main` unnoticed is now bounded by
#6 rather than open-ended, and the bound is visible instead of implicit.

The check belongs to a workflow this decision does not own. Whether it lands inside #6
or as a follow-up issue is the maintainer's call; this record fixes only what the check
has to assert, so that whoever writes the workflow does not have to re-derive it.

If #6 slips past [#12](https://github.com/urario/Yurai/issues/12), the phantom-identifier
check is the one that matters, because that is when real identifiers first appear and
become confusable with examples. It runs as a single `grep` in the meantime.

The cost carried over from ADR-0002 is unchanged: until the check runs somewhere, a
document with a malformed header reaches `main` if no reviewer notices.
