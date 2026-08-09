---
name: yurai-git-workflow
description: Apply Yurai's branch, commit, and pull request conventions. Use when opening a branch, writing a commit message, preparing or describing a pull request, or deciding whether a pull request is ready to merge.
---

# Git workflow

How a change gets from a working tree to a pull request that a maintainer can merge.

The policy is [`knowledge/process/git-policy.md`](../../../knowledge/process/git-policy.md)
and [AGENTS.md §5](../../../AGENTS.md#5-the-pull-request-is-the-quality-gate). Read them
there. This skill is the operating procedure that sits on top of them.

## The one rule that has no exception

**Nothing reaches `main` except through a pull request.** No direct commits, no direct
pushes, and an agent never merges its own work — the maintainer merges. Branch protection
will enforce this mechanically ([#4](https://github.com/urario/Yurai/issues/4)); until it
does, it holds by agreement, which means it holds.

Before anything else, know where you are:

```shell
git rev-parse --abbrev-ref HEAD
```

If that says `main`, stop and branch.

## 1. Branch

Branch from `main`, named `<prefix>/<kebab-case-summary>`:

| Prefix | For |
|---|---|
| `feature/` | New capability |
| `fix/` | A bug fix |
| `chore/` | Maintenance with no behavior change |
| `docs/` | Documentation only, including `knowledge/` |
| `test/` | Tests only, no production code change |

The prefix names the pull request's overall intent; the summary is a few words, not a
restatement of the issue title.

**A branch name assigned by a hosted agent platform is not a violation.** A session that
was handed a branch before it saw the task keeps that name — renaming mid-session loses
the push target. The five prefixes apply whenever the contributor is the one choosing.
The name to not repeat is a freely chosen one that matches no prefix at all.

## 2. Commit

[Conventional Commits](https://www.conventionalcommits.org/): `<type>: <summary>`,
imperative mood, no trailing period. Types: `feat`, `fix`, `test`, `docs`, `chore`,
`refactor`.

**Branch prefix and commit type are two axes, not a mirror.** A `fix/` branch normally
carries a `test:` commit pinning the failing case before the `fix:` commit that resolves
it, and may pick up a `refactor:` commit on the way. None of that is a violation —
`refactor` has no branch prefix at all.

Commit order is the preferred form of test-first evidence
([testing and quality](../../../knowledge/process/testing-and-quality.md#what-counts-as-evidence)),
so on a behavior change, sequence the commits deliberately rather than squashing at the
end: the failing `test:` commit first, then the commit that makes it pass.

Referencing a requirement in a commit body — `Refs RQ-NNN` — is part of the policy but
not active yet, because the registry it points to is empty until
[#12](https://github.com/urario/Yurai/issues/12). Do not invent an identifier to fill the
line; CI rejects any three-digit `RQ-` string that is not registered.

## 3. Pull request

- **One pull request does one thing.** Several small ones review faster than one large
  one, and a reviewer who has to hold two concerns at once catches less of both.
- **Link the issue** — `Refs #N`, or `Closes #N` when the pull request completes it.
- **Use the template.** [`.github/PULL_REQUEST_TEMPLATE/japanese.md`](../../../.github/PULL_REQUEST_TEMPLATE/japanese.md)
  is the default; [`english.md`](../../../.github/PULL_REQUEST_TEMPLATE/english.md) is
  equally fine. Fill the quality-gate table — a row that does not apply says `N/A` with
  the reason, and is never deleted.
- **Language**: Japanese by default for the pull request body, English for everything in
  the repository ([AGENTS.md §4](../../../AGENTS.md#4-language-policy)). Reply in the
  language of the thread.
- **Say what you skipped.** Partial work is fine; silently narrowed scope is not.
- **Say who wrote it.** When an agent works outside its default lane — Claude Code
  implementing, Codex proposing a design — the pull request says so
  ([AGENTS.md §1](../../../AGENTS.md#1-actors-and-responsibilities)).

A draft pull request with red or absent CI is a legitimate way to ask for design feedback
early. Required checks pass before *merge*, not before review.

Before opening one, read your own diff:

```shell
git diff origin/main...HEAD
```

## 4. Merge conditions

A pull request merges when both hold:

- **CI is green** — `.NET build, test, format` and `Knowledge base (OKF)`, both of them.
- **Review comments are resolved or explicitly accepted.** Resolved means addressed by a
  pushed change. Explicitly accepted means a GitHub-visible record that the reviewer
  agreed not to pursue it. **Silence is never acceptance** — an unanswered reply or a
  thread left open with no reviewer response blocks the merge exactly like an
  unaddressed comment.

The author does not review their own work, and the author does not merge it. Issues
labelled `gate` are closed by the maintainer only.

## 5. Handoff

The `owner:human` / `owner:claude` / `owner:codex` label marks **who takes the next
action**, not a permanent assignment. When you finish your part, post a handoff comment
on the issue and move the label. The comment says: what changed, what is left, what the
next actor has to decide or verify, and any blocker.

Write it for someone with no shared context. The next agent sees GitHub and nothing else
([AGENTS.md §3](../../../AGENTS.md#3-agents-communicate-through-github-never-directly)) —
not this session, not this reasoning, not what was obvious at the time.

If you are blocked on a reserved decision, set `owner:human` and say precisely what you
need, having first finished everything that does not depend on the answer.
