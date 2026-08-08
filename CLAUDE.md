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
(#12).
