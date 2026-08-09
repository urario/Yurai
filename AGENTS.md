# AGENTS.md — AI Collaboration Contract

This is the canonical description of how Yurai is developed by a mixed team of
humans and AI coding agents. Every agent working in this repository — and every
human reviewing its output — is expected to follow it.

Yurai is an OSS library, so this process is deliberately lightweight. When a rule
here starts costing more than it protects, raise it with the maintainer and cut it.

## 1. Actors and responsibilities

| Actor | Owns | Not their default lane |
|---|---|---|
| **Human** (maintainer) | Reserved decisions (§2), priorities, acceptance, closing gate issues, merging | Producing first drafts — nothing should be waiting on the maintainer to write one |
| **Claude Code** | Requirements analysis, architecture and design, ADRs, design review, code review, knowledge management, documentation | Reserved decisions (§2); being the default implementer |
| **Codex** | Test-first implementation, verification, benchmarks, preparing pull requests | Reserved decisions (§2); changing architecture or public API on its own initiative |

The third column is default responsibility, not permission — it says what a role is not
*expected* to do, not what it is forbidden to do. The maintainer may write a draft,
Claude Code may implement, Codex may propose a design; whoever does it says so in the
pull request. The lanes are defaults, not fences. What never moves is who decides.

## 2. Reserved decisions: AI proposes, the human decides

Not every question is the maintainer's. Routing all of them there makes one person the
serial bottleneck for work that has already been approved, which is the opposite of
what human-in-the-loop is for. What matters is *which* decisions are reserved.

**Reserved — stop and ask.** Before:

- changing the public API surface (new type or member, changed signature or semantics);
- adding or upgrading a runtime dependency, or changing the target framework;
- architectural changes — anything an ADR records, or should;
- changing requirements, scope, or the phase ordering in the execution plan;
- anything with a security or licensing dimension;
- naming, versioning, and release timing.

**Not reserved — decide and proceed.** Everything else inside an approved issue or ADR:
internal structure, test design and coverage choices, private naming, refactoring,
wording. These are reversible and they are visible in review, so record the reasoning in
the pull request and keep moving. Getting one wrong costs a review comment, not a release.

When a decision *is* reserved, present it rather than pre-empt it:

1. State the question and why it matters now.
2. Give the realistic options with their trade-offs.
3. Give a recommendation and the reason for it.
4. Stop, set `owner:human`, and wait.

**Reversibility is the test.** If undoing the choice later would mean a breaking change,
a rewrite, or a retracted release, it is reserved. When you genuinely cannot tell which
side of the line you are on, treat it as reserved — but do the rest of the work first
and ask about the one thing you are actually blocked on, not the whole task.

Reserved decisions are recorded (ADR or issue comment) so the next agent inherits the
reasoning instead of re-deriving it. If a decision is already recorded, follow it.
Reopen it by proposal, not by quietly implementing something else.

## 3. Agents communicate through GitHub, never directly

**Claude Code and Codex do not talk to each other.** All information passes through
GitHub artifacts:

| Purpose | Where it goes |
|---|---|
| Requirements, design, decisions — while they are being worked out | Issue body and comments |
| Requirements, design, decisions — once settled | [`knowledge/`](knowledge/index.md) — the ADR or requirement outlives the issue |
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

- No direct commits or pushes to `main`. The rule holds now; branch protection will
  enforce it mechanically once configured (#4).
- One pull request does one thing, and links its issue (`Refs #N` / `Closes #N`).
- Once CI exists (#6), all required checks pass **before merge**. Not before review —
  a draft pull request with red or absent CI is a legitimate way to ask for design
  feedback early, and blocking that only makes the feedback arrive later.
- Implementation is test-first; a pull request that changes behavior changes tests (#7).
- **The author does not review their own work.** Anything non-trivial, and every change
  to the public API, gets a review from someone other than whoever wrote it — in
  practice Codex implements, Claude Code reviews, the maintainer merges. A typo or a
  broken link does not need the ceremony.
- An AI review is input, not the merge decision.
- **The maintainer merges.** Agents prepare pull requests and respond to review; they
  do not merge their own work.
- Issues labelled `gate` are closed by the maintainer only.

The detailed gate definition — coverage, mutation score, benchmark thresholds — lives
in the [testing and quality strategy](knowledge/process/testing-and-quality.md), not
here.

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

| Topic | Defined in |
|---|---|
| Git branching policy and branch protection | #4 |
| Issue and pull request templates | #5 |
| Testing strategy and quality gate thresholds | [`knowledge/process/testing-and-quality.md`](knowledge/process/testing-and-quality.md) (#7) |
| Knowledge management structure | [`knowledge/process/knowledge-policy.md`](knowledge/process/knowledge-policy.md) (#8) |
| Claude Code agents and skills | #10 |
| Codex skills and operational detail | #11 |

Claude-specific operating instructions: [`CLAUDE.md`](CLAUDE.md).
Contributor-facing guide: [`CONTRIBUTING.md`](CONTRIBUTING.md).
