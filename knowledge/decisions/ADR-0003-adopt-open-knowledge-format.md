---
type: ADR
title: Adopt the Open Knowledge Format for the knowledge base
description: knowledge/ is an OKF v0.2 bundle rather than a house style, conforming at the required level and adopting the optional families selectively.
tags: [knowledge, okf, format, adr]
status: draft
generated: { by: claude-code/2.1.226, at: 2026-08-08T23:04:15Z }
sources:
  - id: okf-spec
    resource: https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md
    title: Open Knowledge Format (OKF) v0.2
  - id: issue-8
    resource: https://github.com/urario/Yurai/issues/8
    title: "Issue #8: bootstrap the knowledge base"
---

# ADR-0003: Adopt the Open Knowledge Format for the knowledge base

## Context

[ADR-0001](ADR-0001-lightweight-knowledge-base.md) established `knowledge/` with a house
style: an H1 followed by a metadata bullet list, directory indexes written as essays, and
an ADR status vocabulary of our own naming. That style was invented in the pull request
that created the bundle, and it was reasonable only because nothing else was assumed to
exist.

Something else does exist. The Open Knowledge Format (OKF) v0.2[^okf-spec] specifies a
minimal convention for exactly this problem — knowledge authored by people and agents,
exchanged as a directory of markdown files with YAML frontmatter, with no schema
registry and no required tooling. Its conformance bar is three clauses: parseable
frontmatter on every non-reserved document, a non-empty `type` in each, and the two
reserved filenames (`index.md`, `log.md`) shaped as specified when present. Everything
richer — provenance, trust, lifecycle, attestation — is optional and additive.

Checked against v0.2, the bundle as first written failed all three clauses: no document
carried frontmatter at all, and the indexes were prose. The house-style metadata list was
a hand-rolled version of frontmatter that no tool can read.

The question is whether Yurai's knowledge base is better served by its own conventions or
by an external format. ADR-0001 already answered the general form of this question when
it rejected an artifact-identifier scheme: *a scheme nobody maintains is worse than no
scheme*. That argument runs against a private style as hard as it ran against a heavy
one.

## Decision

`knowledge/` is an OKF v0.2 bundle. The bundle root [`index.md`](../index.md) declares
`okf_version: "0.2"`, and the concrete conventions are written up in
[the knowledge policy](../process/knowledge-policy.md).

**Conform at the required level.** Every non-reserved document carries YAML frontmatter
with a `type`; `index.md` files are directory listings rather than essays, with the
explanation they used to carry moved into the documents they link to.

**Adopt the optional families where they say something true.** `status` (whose
`draft`/`stable`/`deprecated` vocabulary maps onto an ADR's proposed, accepted, and
superseded states, and which describes the branch a reader is on rather than the state
the pull request hopes to reach), `generated` with the actor convention, `tags`,
`description`, and `sources` — which is where the issue a document came from now lives.

**Leave out the families that would assert something false or unused.** `verified` is not
written in advance, because the maintainer's merge is the confirmation and it happens
after the file is written; absence correctly reads as unverified. `stale_after` goes
unused until a document genuinely has a shelf life. There is no `log.md`: git already
records what changed and when, and a hand-written history would be the change narration
the policy excludes.

**Link relatively, not bundle-absolutely.** OKF permits both and recommends the
`/`-prefixed form; this bundle is a subdirectory of a larger repository, where such links
resolve against the repository root and break for every human reader on GitHub. This is a
choice inside the format, not a deviation from it.

**What this buys, stated narrowly.** OKF standardises the container and the metadata
vocabulary: a consumer knows where frontmatter is, that `type` exists, and what `status`
and `generated` mean. It does not standardise meaning. `type` values are producer-defined
and registered nowhere, links carry no relationship type, and `requirements` and
`superseded_by` are Yurai extensions. So the gain is structural — less bespoke parsing
for any agent or tool that reads this bundle, and less vocabulary Yurai has to invent —
while the semantics of a Yurai requirement or a Yurai decision stay Yurai's to own and to
argue about.

Two alternatives were rejected. **Keeping the house style** costs nothing today and keeps
every reader on a bespoke parse, while re-deriving a vocabulary that already exists.
**Borrowing the vocabulary without conforming** — frontmatter but essay indexes, say —
pays most of the cost for none of the guarantee, since a consumer can then rely on
nothing. Deferring the whole thing until [#12](https://github.com/urario/Yurai/issues/12)
and [#17](https://github.com/urario/Yurai/issues/17) land was rejected on arithmetic:
nine documents convert in one pull request, and several dozen do not.

## Consequences

An agent can now filter this bundle by `type`, `status`, or `tags` without bespoke
parsing, and a reader gets a one-line `description` before opening anything. The
structural conventions are specified and versioned elsewhere, so the shape of a document
is one fewer thing for Yurai to design and defend — the format is young, so the value
today is a vendor-neutral convention to stand on rather than an ecosystem of tools
already waiting to read it.

The costs:

- **A dependency on a pre-1.0 external format.** v0.2 already renamed fields from v0.1,
  and a major bump may break more. The lock-in is weak — the bundle is plain markdown,
  and abandoning OKF costs deleting frontmatter — but tracking it is now a small
  standing obligation.
- **Every document must carry frontmatter**, which is a mechanical rule that review has
  to catch on every pull request. That changes the reasoning behind
  [ADR-0002](ADR-0002-defer-format-validator.md), which deferred any enforcement partly
  because the conventions were unproven; [ADR-0004](ADR-0004-conformance-check-in-ci.md)
  re-decides it on the new footing.
- **The requirements registry moved.** In OKF an `index.md` is a directory listing, so
  the registry is now [`requirements/registry.md`](../requirements/registry.md) with
  `type: Requirements Registry`. #12 writes into that file.
- **`generated.at` is a maintenance duty.** A meaningful content change updates it, and
  nothing but review will notice when it does not.

[^okf-spec]: Open Knowledge Format (OKF) v0.2
