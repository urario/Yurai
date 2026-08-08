# Architecture decision records

One decision per file, kept for as long as the project lasts. An ADR explains why Yurai
is the way it is — the context that forced a choice, the choice, and the consequences
accepted with it.

Read them when you are about to change something and want to know whether the reasoning
behind it still holds.

## Records

| ID | Decision | Status | Date |
|---|---|---|---|
| [ADR-0001](ADR-0001-lightweight-knowledge-base.md) | A lightweight knowledge base with RQ-ID traceability | Accepted | 2026-08-08 |
| [ADR-0002](ADR-0002-defer-format-validator.md) | Defer a knowledge base format validator | Accepted | 2026-08-08 |

Add a row when you add a record. This table is the index; the files are the record.

## Writing one

1. Take the next unused number and copy [`adr-template.md`](adr-template.md) to
   `ADR-NNNN-short-slug.md`. The identifier is in the file name so that grep and a
   directory listing both find it.
2. Fill in Context, Decision, and Consequences. Say what was decided against and why —
   an ADR without a rejected alternative usually records a habit rather than a decision.
3. Open a pull request that links the issue, and add the row above in the same pull
   request.

**One decision per record**, and the test for whether you have one is supersession: if
part of a record could plausibly be reversed while the rest stands, that part is its own
ADR. ADR-0001 and ADR-0002 came out of a single pull request and were split on exactly
that basis — the tooling question will be revisited long before the structure is. Several
aspects of one decision may share a record when removing any of them would break the
others; several decisions that merely arrived together may not.

`Status` in the pull request is the status the record will have once merged; the
maintainer's merge is the acceptance. Which decisions are the maintainer's is
[AGENTS.md §2](../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides), and
the rest of the mechanics are in
[the knowledge policy](../process/knowledge-policy.md).

## When a decision changes

Write a new ADR and mark the old one `Superseded by ADR-NNNN`, with a link forward. Do
not edit the original into agreement with the new one — the superseded reasoning is what
tells the next reader which assumption stopped being true.
