---
type: Process
title: Git policy
description: Branch naming, commit message style, and merge conditions for changes to Yurai.
tags: [process, git, governance]
status: draft
generated: { by: claude-code/2.1.226, at: 2026-08-08T00:00:00Z }
sources:
  - id: issue-4
    resource: https://github.com/urario/Yurai/issues/4
    title: "Issue #4: Git operating policy and branch protection"
---

# Git policy

How a change moves from a branch to `main`: what a branch and a commit are named, and
what has to be true before a pull request merges.

The pull request gate itself — no direct commits or pushes to `main`, review by someone
other than the author, the maintainer merges — is
[AGENTS.md §5](../../AGENTS.md#5-the-pull-request-is-the-quality-gate) and is not
repeated here. This document covers what that section leaves open: how branches and
commits are named, and the concrete conditions a merge checks.

## Branch naming

`<prefix>/<kebab-case-summary>`, branched from `main`:

| Prefix | For |
|---|---|
| `feature/` | New capability |
| `fix/` | A bug fix |
| `chore/` | Maintenance with no behavior change — tooling, dependency bumps, formatting |
| `docs/` | Documentation only, including `knowledge/` |
| `test/` | Tests only, no production code change |

`fix/negative-round-digits`, `docs/git-policy`. The prefix says what kind of change is on
the branch; the summary is a few words, not a restatement of the issue title.

## Commit messages

[Conventional Commits](https://www.conventionalcommits.org/) style: `<type>: <summary>`,
imperative mood, no trailing period.

| Type | For |
|---|---|
| `feat` | New capability |
| `fix` | A bug fix |
| `test` | Tests only |
| `docs` | Documentation only |
| `chore` | Maintenance with no behavior change |
| `refactor` | Restructuring with no behavior change |

The type set mirrors the branch prefixes above; a `fix/` branch is expected to carry
`fix:` commits, not the other way around — a branch may accumulate a `test:` commit
fixing its own test before the `fix:` lands, and that is fine.

**Referencing a requirement in a commit body** — a line such as `Refs RQ-NNN` — is part
of this policy but not active yet: the requirement registry it would point to does not
exist until [#12](https://github.com/urario/Yurai/issues/12) registers the first
`RQ-###` identifier
([traceability](traceability.md#the-identifier)). Once it does, a commit that implements
or tests a requirement names it in the body the same way a pull request does.

## Merge conditions

A pull request merges when both hold:

- **CI is green.** Required checks pass before merge, not before review — a draft pull
  request with red or absent CI is a legitimate way to ask for early feedback
  ([AGENTS.md §5](../../AGENTS.md#5-the-pull-request-is-the-quality-gate)). CI itself
  does not exist yet ([#6](https://github.com/urario/Yurai/issues/6)); until it does,
  "green" means the build and test commands in
  [CONTRIBUTING.md](../../CONTRIBUTING.md#build-and-test) were run locally and reported.
- **Review comments are resolved or explicitly accepted.** "Resolved" means addressed by
  a pushed change; "explicitly accepted" means the author replied with why not, and the
  reviewer did not press further. A comment that is silently left unanswered is neither.

The maintainer merges — never the pull request's own author
([AGENTS.md §5](../../AGENTS.md#5-the-pull-request-is-the-quality-gate)).

## Branch protection

This document is the policy; branch protection on `main` is its mechanical enforcement,
configured by the maintainer in the GitHub UI
([#4](https://github.com/urario/Yurai/issues/4)) rather than written here, because a
policy document cannot itself flip a repository setting. Until it is configured, the
"no direct commits or pushes" rule holds by agreement among the actors in
[AGENTS.md §1](../../AGENTS.md#1-actors-and-responsibilities), not by a technical
control.

Required-check selection for branch protection — which CI checks are required, and how
many approving reviews — is deferred until [#6](https://github.com/urario/Yurai/issues/6)
gives branch protection something to require; configuring it against a repository with no
CI would just require nothing.
