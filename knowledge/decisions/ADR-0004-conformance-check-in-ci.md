---
type: ADR
title: Check knowledge base conformance in CI, not with a ported validator
description: Supersedes ADR-0002 — the deferral now ends when CI lands rather than when the conventions have proven themselves.
tags: [knowledge, tooling, ci, adr]
status: draft
generated: { by: claude-code/2.1.226, at: 2026-08-08T23:04:15Z }
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

Four properties are checked mechanically, in CI, once CI exists. The first three are OKF
v0.2 §11 in full — all three of its clauses, not the two that are easiest to assert:

1. **Frontmatter parses.** Every non-reserved `.md` file under `knowledge/` opens with a
   YAML block that a parser accepts (§11.1).
2. **`type` is present and non-empty** in each of them (§11.2).
3. **Reserved files keep their shape** (§11.3). An `index.md` carries no frontmatter
   except the bundle root's `okf_version`, and its body is a listing: at least one
   heading, with link entries beneath it. That is the difference between a directory
   index and the essay `requirements/index.md` was before
   [ADR-0003](ADR-0003-adopt-open-knowledge-format.md) — a sentence of framing around
   the entries is fine, a document with no entries is not an index. A `log.md`, if one is
   ever added, uses `YYYY-MM-DD` date headings.

The fourth is Yurai's own invariant, not OKF's:

4. **No phantom identifiers.** Every string matching the `RQ-###` format anywhere in the
   repository names a requirement registered in
   [`requirements/registry.md`](../requirements/registry.md) — the invariant that makes
   traceability-by-search mean anything
   ([traceability](../process/traceability.md#the-identifier)). The search is `git grep`
   over tracked files, so its scope matches the words "anywhere in the repository" rather
   than a list of extensions.

All four are a few lines of shell and a YAML parse added to the existing workflow.
None is a port of the PowerShell validator, and none is a general-purpose OKF linter:
this repository needs four assertions, not a tool.

**What the check does not assert**, and what therefore stays with review: that a
`description` is useful, that an index lists everything in its directory, that `type`
values are used consistently, that `generated.at` was moved when the content changed.
Those are judgements. The check covers the conformance clauses and the identifier
invariant — everything a machine can decide — and claims nothing beyond them.

Until CI exists, all four stay review's job. The identifier one is already a search a
reviewer can run in a single line ([traceability](../process/traceability.md)); the
frontmatter and index-shape ones need a parser, which is exactly why they belong in a
workflow rather than in a reviewer's habits.

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
