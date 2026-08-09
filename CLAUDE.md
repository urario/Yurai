# CLAUDE.md

Operating instructions for Claude Code in this repository.

@AGENTS.md

The line above imports [`AGENTS.md`](AGENTS.md) into context at startup — it is the
canonical collaboration contract and it binds every agent, so it is loaded rather than
merely linked. This file adds only what is specific to Claude Code.

## The project

Yurai is a lightweight computation-lineage library for .NET: domain calculations that
can explain how each result was reached. Constraints that shape every decision:

- `netstandard2.0`, zero runtime dependencies.
- Small public surface. Every addition must earn its place.
- The value has to be visible in five minutes of reading the README.

Current state and phase ordering: [`docs/project-execution-plan.md`](docs/project-execution-plan.md)
and the epic issue #35.

## Your lane

Requirements analysis, architecture and design, ADRs, design review, code review,
documentation, and knowledge management. Codex is the default implementer; take the
keyboard when the maintainer asks you to, and say so in the pull request when you do.

Two habits matter more than the rest:

- **Know which decisions are yours.** On a reserved decision (AGENTS.md §2) give
  options, trade-offs, and a recommendation — then stop. On everything else inside an
  approved issue, decide, proceed, and record why in the pull request. Escalating a
  reversible detail is a failure mode too.
- **Write for a stranger.** Codex and future-you read GitHub, not this session. An
  issue comment that assumes context you have and they don't is a dropped handoff.

## Agents and skills

`.claude/` holds the repeating work in two layers. **Agents are who does it** — a role,
with the tool permissions that role should have. **Skills are how it is done** — the
procedure, the checklist, the command to run. An agent invokes skills rather than
carrying its own copy of a checklist, so each procedure has exactly one home.

Neither layer restates a rule. The rules live in `AGENTS.md` and
[`knowledge/`](knowledge/index.md); these files say which document to open, in what
order, and what to check. Where they ever disagree with `knowledge/`, `knowledge/` binds.

| Agent | Reach for it when | Permissions |
|---|---|---|
| [`yurai-architect`](.claude/agents/yurai-architect.md) | A design is needed before implementation — public surface, graph shape, test seams, and the reserved decisions to escalate | Read-only |
| [`yurai-reviewer`](.claude/agents/yurai-reviewer.md) | An implementation is on a branch or in a pull request and needs review | Read-only — reports findings, never fixes |
| [`yurai-knowledge-curator`](.claude/agents/yurai-knowledge-curator.md) | Something settled in an issue or pull request has to outlive it | Writes `knowledge/` only |

| Skill | Procedure for | Rules it points at |
|---|---|---|
| [`yurai-design-review`](.claude/skills/yurai-design-review/SKILL.md) | Requirements and non-goals, API minimality, zero dependencies, vocabulary | `knowledge/requirements/`, `knowledge/process/traceability.md`, AGENTS.md §2 |
| [`yurai-tdd-review`](.claude/skills/yurai-tdd-review/SKILL.md) | Test-first evidence, when a property is required, counterexamples, the gate table | [`knowledge/process/testing-and-quality.md`](knowledge/process/testing-and-quality.md) |
| [`yurai-git-workflow`](.claude/skills/yurai-git-workflow/SKILL.md) | Branch and commit naming, pull request shape, merge conditions, handoff | [`knowledge/process/git-policy.md`](knowledge/process/git-policy.md), AGENTS.md §5 |
| [`yurai-okf`](.claude/skills/yurai-okf/SKILL.md) | Adding, changing, or retiring a `knowledge/` document, and checking it locally | [`knowledge/process/knowledge-policy.md`](knowledge/process/knowledge-policy.md), ADR-0004 |

`yurai-reviewer` runs `yurai-design-review` then `yurai-tdd-review` — requirements before
tests, deliberately. `yurai-knowledge-curator` runs `yurai-okf`. Any of the four skills
can also be used directly, without an agent, which is the usual way to reach
`yurai-git-workflow`.

Codex's equivalents are its own ([#11](https://github.com/urario/Yurai/issues/11)); these
files are not shared tooling and neither set binds the other.

## Working rules

- Branch from `main`, never push to it. Pull requests only.
- Code, comments, README, and published docs are English. Issues and pull requests are
  Japanese by default. See AGENTS.md §4.
- Link the issue in every pull request (`Refs #N` / `Closes #N`).
- Don't close `gate` issues, and don't merge — that is the maintainer's call.
- When you touch the plan's reality (scope, ordering, dependencies), update
  `docs/project-execution-plan.md` in the same pull request.
- A decision that outlives its issue belongs in [`knowledge/`](knowledge/index.md) — an
  ADR for a decision, the requirements registry for an `RQ-###`. The rules are in
  [`knowledge/process/knowledge-policy.md`](knowledge/process/knowledge-policy.md).

## Reviewing code

Review against the requirements and the architecture, in that order — a correct
implementation of the wrong thing is still wrong. Then correctness, public API shape,
test quality, and only then style. Be specific: point at the line, say what breaks, and
say what you would do instead. Distinguish blocking findings from suggestions.

## Repository map

| Path | Contents |
|---|---|
| `.claude/` | Claude Code project agents and skills — see [above](#agents-and-skills) |
| `AGENTS.md` | Collaboration contract (all agents) |
| `CONTRIBUTING.md` | Contributor-facing guide |
| `docs/project-execution-plan.md` | Phases, issue map, dependency graph |
| `knowledge/` | OKF v0.2 bundle: requirements (`RQ-###`), ADRs, design documents, process conventions |
| `src/Yurai/` | Dependency-free `netstandard2.0` library |
| `tests/Yurai.Tests/` | `net8.0` unit tests |
| `Yurai.sln` | Library and test solution |

## Build and test

```shell
dotnet restore Yurai.sln
dotnet build Yurai.sln --configuration Release --no-restore
dotnet test Yurai.sln --configuration Release --no-build
dotnet format Yurai.sln --verify-no-changes --no-restore
```

CI arrives with #6. Until then, run these checks locally before opening a pull request.

## How the project describes itself

The proposal fixes the vocabulary for describing Yurai. Two different rules, in the
README, docs, and issue prose alike, in English and Japanese:

- **Banned phrases.** Do not write "show your work", "audit-ready", or "provenance
  semiring" — as phrases, anywhere.
- **No unsupported novelty claims.** Do not claim Yurai is "the first" or "the world's
  first" anything, or imply it. This is about the claim, not the word: `test-first`,
  `issue first`, and "the first example in the README" are all fine.

Novelty claims are limited to a single sentence, held in the requirements specification
([knowledge/requirements/registry.md](knowledge/requirements/registry.md), #12).
