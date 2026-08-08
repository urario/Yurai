---
type: Template
title: ADR template
description: Skeleton for a new architecture decision record, with the frontmatter and the three sections to fill in.
tags: [template, adr]
status: draft
generated: { by: claude-code/2.1.226, at: 2026-08-08T23:04:15Z }
---

# ADR template

Copy the block below into `knowledge/decisions/ADR-NNNN-short-slug.md`, taking the next
unused number from [the index](index.md), and fill it in. Delete any section that has
nothing to say — an empty heading is worse than a missing one.

`status` stays `draft` for as long as the pull request is open — nobody has reviewed the
record yet, and that is exactly what `draft` means. When the maintainer approves, one
more commit raises it to `stable`, and that commit is the one that merges. The rest of
the conventions — numbering, actors, supersession, granularity — are in
[the knowledge policy](../process/knowledge-policy.md).

````markdown
---
type: ADR
title: Short statement of the decision
description: One sentence a reader can use to decide whether to open the file.
tags: [area, adr]
status: draft
requirements: [RQ-NNN]
generated: { by: claude-code/2.1.226, at: 2026-08-08T23:04:15Z }
sources:
  - id: issue-N
    resource: https://github.com/urario/Yurai/issues/N
    title: "Issue #N: what it asked for"
---

# ADR-NNNN: Short statement of the decision

## Context

What forces the decision now. The constraints in play, the requirement or issue that
raised it, and what is already fixed by earlier decisions. Write it so that someone who
was not in the discussion can tell whether the decision still applies when the context
changes — that is the only reason this section exists.

## Decision

What was decided, in the present tense and in a form specific enough to be followed:
"Yurai does X" rather than "we should probably do X". Name what was decided *against*
where the alternative is one a reasonable person would have picked, and say what made
the difference. One decision per record; if the "and" in the title is load-bearing, this
is two ADRs.

## Consequences

What follows from this, good and bad. Include the costs accepted — a record with only
benefits is one where the trade-off was not examined. Note anything this constrains
later, and what would have to be true to reopen it.
````

Omit `requirements` entirely when no requirement applies yet, rather than writing an
empty list. `RQ-NNN` above is a placeholder that deliberately does not match a real
identifier ([traceability](../process/traceability.md#the-identifier)).
