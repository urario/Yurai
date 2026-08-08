# ADR-0002: Defer a knowledge base format validator

- **Status:** Accepted
- **Date:** 2026-08-08
- **Requirements:** —
- **Issue:** [#8](https://github.com/urario/Yurai/issues/8)

## Context

[ADR-0001](ADR-0001-lightweight-knowledge-base.md) establishes conventions that a script
could check mechanically: document headers, ADR status values, identifiers that match
`RQ-` followed by three digits being references to registered requirements. The larger
internal project these conventions are adapted from (Surveyor) enforces its equivalents
with a PowerShell validator, and [#8](https://github.com/urario/Yurai/issues/8) leaves
porting it open as a decision for the maintainer.

The conventions are a day old and have been applied to eight documents, all written in
one pull request by one author. Nothing yet says whether they are the right conventions.

## Decision

No validator is added now. The conventions are enforced by review, and the searches in
[traceability](../process/traceability.md) are run by hand at the gate issues.

Two things argue against automating now, and one argues for revisiting later.

**Cost of the dependency.** Yurai's toolchain is plain cross-platform `dotnet` and
nothing else, deliberately — it is a zero-dependency library and the build instructions
fit in four lines. A PowerShell script would be the first tool in the repository that is
neither `dotnet` nor `git`, added to check markdown formatting.

**Cost of freezing the wrong rule.** A validator does not merely enforce conventions, it
fixes them: changing a rule then means changing the script, and rules that are annoying
to change stop being questioned. Enforcing conventions before they have survived contact
with [#12](https://github.com/urario/Yurai/issues/12) and
[#17](https://github.com/urario/Yurai/issues/17) is likely to entrench a guess.

**What would change the answer.** Two of these conventions are worth checking
mechanically the moment they are load-bearing rather than aspirational: that every
`RQ-###`-shaped string is a registered identifier, and that every P0 requirement is
referenced by at least one test. Both are one `grep` and one exit code — they belong in
CI ([#6](https://github.com/urario/Yurai/issues/6)) as a few lines of shell, not in a
ported PowerShell validator. The trigger is the requirements registry gaining real
entries in #12, or drift appearing in review before then.

## Consequences

The knowledge base stays as cheap to change as the conventions governing it, and nothing
new appears in the toolchain.

The cost is that a malformed document reaches `main` whenever no reviewer catches it.
While `knowledge/` holds a handful of documents and every change is reviewed, that is a
small exposure; it grows with the number of documents, which is exactly the signal to
reopen this.

Reopening is a new ADR superseding this one, not an edit — and because this decision is
recorded separately from the structure it is about, superseding it leaves ADR-0001
standing.
