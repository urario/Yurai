# AGENTS.md — AI Collaboration Contract

This is the canonical description of how Yurai is developed by a mixed team of
humans and AI coding agents. Every agent working in this repository — and every
human reviewing its output — is expected to follow it.

Yurai is an OSS library, so this process is deliberately lightweight. When a rule
here starts costing more than it protects, raise it with the maintainer and cut it.

## 1. Actors and responsibilities

| Actor | Owns | Does not own |
|---|---|---|
| **Human** (maintainer) | Design decisions, priorities, acceptance, closing gate issues, merging | Producing the first draft of anything |
| **Claude Code** | Requirements analysis, architecture and design, ADRs, design review, code review, knowledge management, documentation | Final decisions; being the default implementer |
| **Codex** | Test-first implementation, verification, benchmarks, preparing pull requests | Final decisions; unilaterally changing architecture or public API |

The lanes are defaults, not fences. The maintainer may hand any task to any actor;
what never moves is who decides.

## 2. AI proposes, the human decides

Agents do not settle open questions on their own. When a decision is needed:

1. State the question and why it matters now.
2. Give the realistic options with their trade-offs.
3. Give a recommendation and the reason for it.
4. Stop and wait for the maintainer.

Decisions that reach beyond a single pull request — public API shape, dependencies,
naming, scope, release timing — are recorded (ADR or issue comment) so the next agent
inherits the reasoning instead of re-deriving it.

If a decision is already recorded, follow it. Reopen it by proposal, not by quietly
implementing something else.

## 3. Agents communicate through GitHub, never directly

**Claude Code and Codex do not talk to each other.** All information passes through
GitHub artifacts:

| Purpose | Where it goes |
|---|---|
| Requirements, design, decisions | Issue body and comments |
| Handoff of work | Issue comment stating what is done and what the next actor needs |
| Implementation | Pull request |
| Feedback on implementation | Pull request review comments |

The practical consequence: **write for a reader with no shared context.** An agent
picking up an issue sees only what is on GitHub — not another agent's session, not a
chat log. State assumptions, link the files you touched, and say explicitly what you
did *not* do.

Handoff protocol:

- The `owner:human` / `owner:claude` / `owner:codex` label marks **who takes the next
  action**, not who has been assigned forever. When you finish your part, post a
  handoff comment and move the label.
- A handoff comment says: what changed, what is left, what the next actor needs to
  decide or verify, and any blocker.
- If you are blocked on a decision, set `owner:human` and say precisely what you need.

## 4. Language policy

| Artifact | Language |
|---|---|
| Issues | Japanese by default; English is welcome from external contributors |
| Pull requests | Japanese by default; English is fine (templates exist in both — see #5) |
| Source code, identifiers, code comments | English |
| README and all published documentation | English |
| Repository-internal governance docs (this file, `CLAUDE.md`, `CONTRIBUTING.md`) | English |

The split is deliberate: internal coordination runs in the maintainer's working
language, and everything a user of the library might read is English so the library
is usable outside Japan.

Reply in the language of the thread you are in.

## 5. The pull request is the quality gate

Nothing reaches `main` except through a pull request.

- No direct commits or pushes to `main` (branch protection: #4).
- One pull request does one thing, and links its issue (`Refs #N` / `Closes #N`).
- CI must be green before review is requested (#6).
- Implementation is test-first; a pull request that changes behavior changes tests (#7).
- Review is required. An AI review is useful input; it is not the merge decision.
- **The maintainer merges.** Agents prepare pull requests and respond to review; they
  do not merge their own work.
- Issues labelled `gate` are closed by the maintainer only.

The detailed gate definition — coverage, mutation score, benchmark thresholds — lives
in the testing and quality strategy (#7), not here.

## 6. Working agreements

- **Issue first.** Non-trivial work starts from an issue so the reasoning is durable.
- **Small pull requests.** Prefer several reviewable pull requests over one large one.
- **Say what you skipped.** Partial work is fine; silently narrowed scope is not.
- **Do not invent requirements.** If the issue is ambiguous, ask in the issue.
- **Keep the plan honest.** When reality diverges from `docs/project-execution-plan.md`,
  update it in the same pull request.
- **Report failures as failures.** If tests fail or a step was skipped, say so with the
  output. Never describe unverified work as verified.

## 7. Scope of this document

Defined here: roles, decision rights, agent-to-agent communication, language policy,
and the pull request gate principle.

Defined elsewhere:

| Topic | Issue |
|---|---|
| Git branching policy and branch protection | #4 |
| Issue and pull request templates | #5 |
| Testing strategy and quality gate thresholds | #7 |
| Knowledge management structure | #8 |
| Claude Code agents and skills | #10 |
| Codex skills and operational detail | #11 |

Claude-specific operating instructions: [`CLAUDE.md`](CLAUDE.md).
Contributor-facing guide: [`CONTRIBUTING.md`](CONTRIBUTING.md).
