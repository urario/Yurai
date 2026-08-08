# Yurai knowledge base

Durable knowledge about Yurai: what the library is required to do, why it is built the
way it is, and the conventions the project works by. If you need to understand a
decision that was made before you arrived, it is here or it was not written down.

Yurai keeps information in three places, and the split is deliberate:

| Place | Holds | Lifetime |
|---|---|---|
| **Issues** | What is being worked on now, and the discussion that shapes it | Until the issue closes |
| **Pull requests** | What changed, why, and how it was verified | Frozen at merge; read as history |
| **`knowledge/`** | Requirements, decisions, and conventions that outlive both | Maintained — kept true or explicitly retired |

A decision that only exists in an issue comment is lost the moment the issue scrolls out
of view. A decision in `knowledge/` is one a stranger can find. The rules for what goes
where are in [`process/knowledge-policy.md`](process/knowledge-policy.md).

## Directories

| Directory | Contents | Start here |
|---|---|---|
| [`requirements/`](requirements/) | The requirements specification — `RQ-###` identifiers, priorities, acceptance criteria | [requirements/index.md](requirements/index.md) |
| [`decisions/`](decisions/) | Architecture decision records (ADRs) — one decision per file | [decisions/index.md](decisions/index.md) |
| [`design/`](design/) | Architecture and design documents that span more than one ADR | [design/index.md](design/index.md) |
| [`process/`](process/) | How the project works: knowledge policy, traceability, testing strategy | [process/knowledge-policy.md](process/knowledge-policy.md) |

## Conventions in force

- [Knowledge policy](process/knowledge-policy.md) — what belongs here, file naming,
  document headers, how a document is changed or retired.
- [Traceability](process/traceability.md) — the `RQ-###` requirement identifiers and how
  design, code, and tests refer back to them.

## Not here

Collaboration rules, decision rights, and the language policy live in
[`AGENTS.md`](../AGENTS.md); the contributor-facing guide is
[`CONTRIBUTING.md`](../CONTRIBUTING.md); phase ordering and the issue map are in
[`docs/project-execution-plan.md`](../docs/project-execution-plan.md). This knowledge
base points at them rather than restating them — two copies of a rule become two
different rules.

Everything under `knowledge/` is written in English, like the rest of the published
documentation. Issues and pull requests are Japanese by default
([AGENTS.md §4](../AGENTS.md#4-language-policy)).
