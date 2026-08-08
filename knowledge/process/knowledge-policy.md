# Knowledge policy

How `knowledge/` is kept: what belongs in it, what does not, and how a document is
added, changed, or retired.

The policy is short on purpose. Yurai is a small library maintained by a small mixed
team of humans and AI agents, and a process that costs more than it protects is a bug —
raise it with the maintainer and cut it ([AGENTS.md](../../AGENTS.md)).

## The three places

| Place | Answers | Written by |
|---|---|---|
| **Issue** | What are we doing, and what is the state of it? | Whoever takes the next action |
| **Pull request** | What changed, why, how was it verified? | The author; reviewed by someone else |
| **`knowledge/`** | What is true about this project independent of any one task? | Whoever makes the change, in a pull request |

Working state belongs in the issue. A record of a change belongs in the pull request
that made it. Only what a newcomer needs in order to understand the project — without
reading a single issue — belongs here.

The practical test: **if the answer would still matter after every open issue is
closed, it belongs in `knowledge/`.**

## What belongs here

- **Requirements** — the `RQ-###` specification, with priorities and acceptance
  criteria. One registry, in `requirements/`.
- **Decisions** — ADRs in `decisions/`. One decision per file, with the context that
  made it necessary and the consequences accepted along with it.
- **Design** — architecture documents in `design/` that describe structure spanning
  several decisions, when an ADR is the wrong shape for it.
- **Process conventions** — in `process/`: this policy, traceability, and the testing
  and quality strategy ([#7](https://github.com/urario/Yurai/issues/7)).

## What does not belong here

- **Task state.** Progress, blockers, and to-do lists live on issues and labels.
- **Change narration.** "Renamed X to Y" is a pull request, not a knowledge document.
- **Rules that already exist elsewhere.** Roles, decision rights, the pull request gate,
  and the language policy are in [`AGENTS.md`](../../AGENTS.md); the build and
  contribution steps are in [`CONTRIBUTING.md`](../../CONTRIBUTING.md); phase ordering is
  in [`docs/project-execution-plan.md`](../../docs/project-execution-plan.md). Link to
  them. A second copy drifts, and then nobody knows which one binds.
- **User documentation.** What a user of the library reads is the README and the
  published docs. `knowledge/` is for the people building it.

## Document conventions

**File names** are lowercase kebab-case, ending in `.md`. ADRs are the exception: they
carry their identifier so it survives grep and file listings —
`ADR-0007-round-half-away-from-zero.md`.

**Every document starts with an H1 title.** ADRs, design documents, and the requirements
registry then carry a short metadata list, because for those the status, the date, and
the requirements served are part of the content:

```markdown
# ADR-0007: Rounding mode for Round(digits, reason)

- **Status:** Accepted
- **Date:** 2026-09-01
- **Requirements:** RQ-014, RQ-015
- **Issue:** [#18](https://github.com/urario/Yurai/issues/18)
```

`Requirements` lists the `RQ-###` identifiers the document serves, or `—` when none
apply yet (see [traceability](traceability.md)). `Status` is meaningful for ADRs; a
document whose only state is "current" — this policy, the directory indexes — omits it
and the metadata list with it.

**Then a paragraph that says what the document is for**, before any structure. A reader
who opened the wrong file should discover that in one sentence.

**Language is English**, including in documents that summarise a Japanese issue. Quote
the issue in translation and link it.

## Status and lifecycle

ADRs carry a status:

| Status | Meaning |
|---|---|
| `Proposed` | Written, not yet decided. Lives in an open pull request. |
| `Accepted` | Decided and in force. |
| `Superseded by ADR-NNNN` | Replaced. The file stays, with a link forward. |

**The maintainer's merge is the acceptance.** The status written in a pull request is
the status the document will have once merged — so an ADR that the pull request intends
to put in force is written `Accepted`, and one opened to collect feedback before a
decision is written `Proposed` in a draft pull request. Which decisions are the
maintainer's to make is [AGENTS.md §2](../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides).

**A decision is never edited into a different decision.** Correct a typo freely; when
the decision itself changes, write a new ADR and mark the old one superseded. The
reasoning that was wrong is the most useful part of the record.

Other documents are simply kept true. When a requirement or a convention changes, the
pull request that changes reality updates the document in the same pull request.

## Changing a document

Same gate as code: a branch, a pull request, an issue link, and a review from someone
other than the author — with the same exception, that a typo or a broken link does not
need the ceremony
([AGENTS.md §5](../../AGENTS.md#5-the-pull-request-is-the-quality-gate)). Nothing here is
edited on `main` directly.

## What this policy deliberately does not do

- **No artifact identifier system beyond `RQ-###`.** Design, implementation, unit test,
  and integration test artifacts do not get their own identifiers. The reasoning is in
  [ADR-0001](../decisions/ADR-0001-lightweight-knowledge-base.md).
- **No hand-maintained traceability matrix.** A matrix that has to be updated by hand is
  wrong within a month and misleading thereafter. Traceability is derived by search —
  see [traceability](traceability.md).
- **No format validator, for now.** The conventions above are checked by review. A
  script that enforces them is worth adding only if drift actually appears; the question
  is recorded in [ADR-0001](../decisions/ADR-0001-lightweight-knowledge-base.md) and is
  the maintainer's to reopen.
