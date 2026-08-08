---
type: Process
title: Knowledge policy
description: What belongs in the Yurai knowledge bundle, how its documents are structured, and how they change.
tags: [process, knowledge, okf]
status: stable
generated: { by: claude-code/2026-08, at: 2026-08-08T22:49:00Z }
sources:
  - id: issue-8
    resource: https://github.com/urario/Yurai/issues/8
    title: "Issue #8: bootstrap the knowledge base"
  - id: okf-spec
    resource: https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md
    title: Open Knowledge Format (OKF) v0.2
---

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

## The format

`knowledge/` is an [Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md)
bundle targeting v0.2, declared as `okf_version` in the bundle root
[`index.md`](../index.md). The reasoning, and what Yurai gets from an external format
rather than a house style, is in
[ADR-0003](../decisions/ADR-0003-adopt-open-knowledge-format.md).

Practically, OKF asks for two things and leaves the rest to us: every document is
markdown with a YAML frontmatter block, and every frontmatter block has a non-empty
`type`. Everything below is either that requirement made concrete or a Yurai choice
within the room OKF leaves.

### Frontmatter

```yaml
---
type: ADR
title: "Rounding mode for Round(digits, reason)"
description: One sentence a reader can use to decide whether to open the file.
tags: [rounding, decimal]
status: stable
requirements: [RQ-NNN, RQ-MMM]
generated: { by: claude-code/2026-08, at: 2026-09-01T10:00:00Z }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: design decisions for the open questions"
---
```

`ADR-NNNN`, `RQ-NNN`, and `RQ-MMM` in examples are placeholders, deliberately written so
that they do not match a real identifier — see
[traceability](traceability.md#the-identifier). The rule holds for both identifier
schemes and in every document: a plausible-looking identifier in an example is a record
that does not exist, and a search cannot tell the difference between it and a genuine
reference.

**`type`** is the only field OKF requires. Yurai uses five values:

| `type` | Used for |
|---|---|
| `ADR` | A decision record in `decisions/` |
| `Process` | A convention in `process/` — this policy, traceability, the testing strategy |
| `Requirements Registry` | The requirement registry in `requirements/` |
| `Design` | An architecture document in `design/` |
| `Template` | A skeleton to copy, such as the ADR template |

**`status`** uses the OKF vocabulary, which maps onto the states an ADR moves through:

| `status` | Meaning | For an ADR |
|---|---|---|
| `draft` | Not yet reviewed; possibly incomplete | Proposed — written, not yet decided |
| `stable` | Ready for consumption (the default when absent) | Accepted — decided and in force |
| `deprecated` | Kept for links and history, no longer current | Superseded — `superseded_by` names the replacement |

**Producer keys.** OKF tolerates additional fields, and Yurai defines two: `requirements`
lists the `RQ-###` identifiers a document serves, and `superseded_by` names the record
that replaced this one. Omit `requirements` rather than writing an empty list when none
apply yet.

**Actors** follow the OKF convention: `claude-code/<YYYY-MM>` and `codex/<YYYY-MM>` for
the agents, `human:<github-id>` for a person, `process:<id>` for automation. The month
is the version — the agents are continuously updated products, and pinning a model
version in a document that outlives it would be precision without meaning.

**`verified` is not written in advance.** The maintainer's merge is what confirms a
document, and that happens after the file is written; asserting it beforehand would be
untrue. Absence means unverified, which is accurate. A later pull request may add
`verified: { by: human:<id>, at: <date> }` where the confirmation itself matters — a
requirement signed off at a gate issue, for instance.

**`stale_after` is unused for now.** Decisions do not expire, and requirements are
checked at gate issues rather than on a clock. Add it to a document that genuinely has a
shelf life.

**`sources`** records what a document derives from: the issue it came from, the project
proposal, an external specification. To attribute one claim rather than the document,
use a markdown footnote whose label is a `sources` entry's `id`.

### Body

**An H1 title, then a paragraph that says what the document is for**, before any
structure. A reader who opened the wrong file should discover that in one sentence.

**Language is English**, including in documents that summarise a Japanese issue. Quote
the issue in translation and link it.

### Files and links

**File names** are lowercase kebab-case, ending in `.md`. ADRs are the exception: they
carry their identifier so it survives grep and file listings —
`ADR-NNNN-round-half-away-from-zero.md`, with `NNNN` replaced by the next unused number.

**Links between documents are relative** (`../decisions/index.md`), not the
bundle-absolute form OKF also allows. This bundle is a subdirectory of a larger
repository, so a link beginning with `/` resolves against the repository root in
GitHub's renderer and breaks for every human reader.

**`index.md` is a listing, not an essay.** One or more sections, each a heading followed
by links with one-line descriptions, so that a reader or an agent can see what a
directory holds before opening anything in it. Explanation belongs in a document the
index links to. Only the bundle root `index.md` carries frontmatter, and only to declare
`okf_version`.

**There is no `log.md`.** OKF allows one; git already records what changed, when, and by
whom, and a hand-written history would be the change narration this policy excludes.

## Status and lifecycle

**The maintainer's merge is the acceptance.** The `status` written in a pull request is
the status the document will have once merged — so an ADR that the pull request intends
to put in force is written `stable`, and one opened to collect feedback before a decision
is written `draft` in a draft pull request. Which decisions are the maintainer's to make
is [AGENTS.md §2](../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides).

**A decision is never edited into a different decision.** Correct a typo freely; when the
decision itself changes, write a new ADR, set the old one to `deprecated` with
`superseded_by`, and add a line at the top of its body pointing forward. Everything else
in the superseded record stays as written — the reasoning that turned out to be wrong is
the most useful part of it.

**One decision per record**, and the test is supersession: if part of a record could
plausibly be reversed while the rest stands, that part is its own ADR. Several aspects of
one decision may share a record when removing any of them would break the others;
several decisions that merely arrived together may not.

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
- **No conformance check until CI exists.** The conventions above are checked by review
  for now. What is checked mechanically once CI arrives, and why waiting is the cheaper
  order, is in [ADR-0004](../decisions/ADR-0004-conformance-check-in-ci.md).
