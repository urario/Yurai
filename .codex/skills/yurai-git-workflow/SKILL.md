---
name: yurai-git-workflow
description: Apply Yurai's Git and GitHub workflow when creating or updating a branch, organizing commits, preparing or opening a pull request, or handing an issue to its next owner.
---

# Yurai Git workflow

Move one coherent change from a clean branch to a reviewable pull request without
bypassing the repository quality gate.

Read [`knowledge/process/git-policy.md`](../../../knowledge/process/git-policy.md) and
[`AGENTS.md`](../../../AGENTS.md) before acting. They bind when this procedure and the
policy ever differ.

## 1. Branch safely

1. Run `git status --short --branch` before branching, staging, committing, or pushing.
2. Preserve unrelated changes and stop if the intended scope cannot be isolated.
3. Update local `main` from `origin/main` with a fast-forward-only pull.
4. Create `<prefix>/<kebab-case-summary>` from `main`: `feature`, `fix`, `chore`,
   `docs`, or `test` as defined by the Git policy.

Never commit or push directly to `main`. Keep a hosted platform-assigned branch name
when the platform owns it, as allowed by the policy.

## 2. Make reviewable commits

Use Conventional Commits: `<type>: <imperative summary>`, with no trailing period.
Allowed types are `feat`, `fix`, `test`, `docs`, `chore`, and `refactor`.

For a behavior change, prefer an observable failing `test:` commit followed by the
`feat:` or `fix:` commit that makes it pass. Do not squash away that evidence. Reference
registered requirements in the commit body when useful; use `RQ-NNN` only as an example
and never invent a real-format identifier.

Stage only files in the issue scope. Inspect `git diff --cached` before committing and
`git diff origin/main...HEAD` before pushing.

## 3. Prepare the pull request

Use [the Japanese template](../../../.github/PULL_REQUEST_TEMPLATE/japanese.md) by
default; the English template is also valid. Keep every quality-gate row and provide
commands and results, or `N/A` / `NOT RUN` with a reason.

The description must:

- summarize the change and its impact;
- link the issue with `Closes #N` when the pull request completes it, otherwise
  `Refs #N`;
- list registered requirement and durable artifact identifiers, or state `N/A`;
- preserve one accepted test-first evidence form for behavior changes;
- contain a written self-review, residual risks, and everything intentionally skipped;
- disclose when an agent worked outside its default lane.

Open a ready-for-review pull request only after local verification succeeds. Use a draft
when feedback is intentionally needed before the checks or design are complete. Never
merge the pull request; the maintainer owns that action.

## 4. Hand off through GitHub

After opening the pull request, post an issue comment for a reader with no session
context. State what changed, what remains, the verification performed, residual risks,
and what the next actor must decide or review. Move the `owner:*` label to that actor.

For the normal Codex implementation flow, hand a completed pull request to
`owner:claude` for independent review. Use `owner:human` when blocked on a reserved
decision. Leave the issue open until its pull request merges unless the maintainer says
otherwise.
