---
type: Process
title: Git policy
description: Branch naming, commit message style, and merge conditions for changes to Yurai.
tags: [process, git, governance]
status: draft
generated: { by: codex/2026-08, at: 2026-08-09T09:11:32+09:00 }
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

When the person or agent opening the branch chooses its name, it is
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

**Platform-assigned branch names are an exception, not a violation.** A hosted agent
session (for example, Claude Code on the web) creates its working branch before the
agent sees the task, names it outside the agent's control — `claude/<slug>-<random>`,
as on this pull request itself — and the agent cannot rename it mid-session without
losing the session's push target. Such names are accepted as-is; the five prefixes above
apply whenever the contributor, human or agent, is the one choosing the name. A branch
name a validator or branch rule would reject on sight is one a *free* choice produced —
`agent/issue-5-templates` (#41) is the example to not repeat, not `claude/...`. Any
future automated check on branch names allows both: the five prefixes, and the naming
scheme of a known hosted-agent platform.

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

Branch prefix and commit type are two different axes, not a mirror of each other: the
branch prefix names the pull request's overall intent, and the commit type names one
commit's own. A `fix/` branch commonly carries a `test:` commit that pins down the
failing case before the `fix:` commit that resolves it, and may pick up a `refactor:`
commit along the way — none of that is a policy violation, because `refactor` has no
corresponding branch prefix at all. A branch's prefix is chosen once, when it is opened;
what it constrains is the pull request's title and scope, not the type of every commit
inside it.

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
  ([AGENTS.md §5](../../AGENTS.md#5-the-pull-request-is-the-quality-gate)). The
  `.NET build, test, format` and `Knowledge base (OKF)` checks run for every pull request
  targeting `main` and every push to `main`; both must pass.
- **Review comments are resolved or explicitly accepted.** "Resolved" means addressed by
  a pushed change. "Explicitly accepted" means a positive, GitHub-visible record that the
  reviewer agreed not to pursue it further — the reviewer replies saying so and resolves
  the thread, or, failing that, the maintainer overrides with the reason recorded in the
  thread. Silence is never acceptance: an unanswered reply from the author, or a thread
  left unresolved with no reviewer response, blocks merge exactly like an unaddressed
  comment. The state has to be one a validator could read off GitHub, not one that
  depends on knowing nobody objected.

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

Selecting the two CI checks above as required branch-protection checks, and choosing how
many approving reviews are required, remains a maintainer follow-up under
[#4](https://github.com/urario/Yurai/issues/4). Their displayed job names are kept stable
so branch protection can refer to them reliably.
