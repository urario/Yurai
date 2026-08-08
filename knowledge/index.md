---
okf_version: "0.2"
---

# Yurai knowledge base

Durable knowledge about Yurai: what the library is required to do, why it is built the
way it is, and the conventions the project works by. Issues hold working state, pull
requests hold changes and their verification, and this bundle holds what stays true after
both are closed.

# Requirements

* [Requirements registry](requirements/registry.md) - The single registry of `RQ-###` identifiers, with priorities, statuses, and acceptance criteria. Empty until [#12](https://github.com/urario/Yurai/issues/12) fills it.
* [requirements/](requirements/index.md) - The directory.

# Decisions

* [ADR-0001: A lightweight knowledge base with RQ-ID traceability](decisions/ADR-0001-lightweight-knowledge-base.md) - Requirements are the only identified artifacts, traceability is derived by search, and decision records are immutable.
* [ADR-0002: Defer a knowledge base format validator](decisions/ADR-0002-defer-format-validator.md) - Superseded by ADR-0004.
* [ADR-0003: Adopt the Open Knowledge Format for the knowledge base](decisions/ADR-0003-adopt-open-knowledge-format.md) - This bundle is OKF v0.2 rather than a house style.
* [ADR-0004: Check knowledge base conformance in CI, not with a ported validator](decisions/ADR-0004-conformance-check-in-ci.md) - What gets checked mechanically, and when.
* [decisions/](decisions/index.md) - The directory, including the ADR template.

# Design

* [design/](design/index.md) - Architecture documents. Empty until [#17](https://github.com/urario/Yurai/issues/17).

# Process

* [Knowledge policy](process/knowledge-policy.md) - What belongs in this bundle, how documents are structured, and how they change.
* [Traceability](process/traceability.md) - The `RQ-###` identifiers, and how design, code, and tests refer back to them.
* [Git policy](process/git-policy.md) - Branch naming, commit message style, and merge conditions.

# Elsewhere in the repository

* [AGENTS.md](../AGENTS.md) - Roles, decision rights, agent-to-agent communication, and the language policy.
* [CONTRIBUTING.md](../CONTRIBUTING.md) - The contributor-facing guide: how to build, test, and open a pull request.
* [docs/project-execution-plan.md](../docs/project-execution-plan.md) - Phases, the issue map, and the dependency graph.

This bundle points at those rather than restating them — two copies of a rule become two
different rules. Everything here is written in English, like the rest of the published
documentation; issues and pull requests are Japanese by default
([AGENTS.md §4](../AGENTS.md#4-language-policy)).
