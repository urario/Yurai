# CLAUDE.md

Operating instructions for Claude Code in this repository.

**Read [`AGENTS.md`](AGENTS.md) first** — it is the canonical collaboration contract
and applies to every agent. This file adds only what is specific to Claude Code.

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

- **Propose, do not decide.** Options, trade-offs, a recommendation — then stop. See
  AGENTS.md §2.
- **Write for a stranger.** Codex and future-you read GitHub, not this session. An
  issue comment that assumes context you have and they don't is a dropped handoff.

## Working rules

- Branch from `main`, never push to it. Pull requests only.
- Code, comments, README, and published docs are English. Issues and pull requests are
  Japanese by default. See AGENTS.md §4.
- Link the issue in every pull request (`Refs #N` / `Closes #N`).
- Don't close `gate` issues, and don't merge — that is the maintainer's call.
- When you touch the plan's reality (scope, ordering, dependencies), update
  `docs/project-execution-plan.md` in the same pull request.

## Reviewing code

Review against the requirements and the architecture, in that order — a correct
implementation of the wrong thing is still wrong. Then correctness, public API shape,
test quality, and only then style. Be specific: point at the line, say what breaks, and
say what you would do instead. Distinguish blocking findings from suggestions.

## Repository map

| Path | Contents |
|---|---|
| `AGENTS.md` | Collaboration contract (all agents) |
| `CONTRIBUTING.md` | Contributor-facing guide |
| `docs/project-execution-plan.md` | Phases, issue map, dependency graph |

Build and test commands do not exist yet — the solution skeleton arrives with #2 and CI
with #6. Update this section then.

## Prose the project does not use

The proposal fixes the vocabulary for how Yurai is described. Do not write, in the
README, docs, or issue prose: "show your work", "audit-ready", "first" / "world's
first", "provenance semiring". Novelty claims are limited to a single sentence, held in
the requirements specification (#12). This applies to English and Japanese alike.
