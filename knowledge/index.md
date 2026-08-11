---
okf_version: "0.2"
---

# Yurai knowledge base

Durable knowledge about Yurai: what the library is required to do, why it is built the
way it is, and the conventions the project works by. Issues hold working state, pull
requests hold changes and their verification, and this bundle holds what stays true after
both are closed.

# Requirements

* [Requirements registry](requirements/registry.md) - The single registry of `RQ-###` identifiers, with priorities, statuses, and acceptance criteria. Populated by [#12](https://github.com/urario/Yurai/issues/12).
* [requirements/](requirements/index.md) - The directory.

# Decisions

* [ADR-0001: A lightweight knowledge base with RQ-ID traceability](decisions/ADR-0001-lightweight-knowledge-base.md) - Requirements are the only identified artifacts, traceability is derived by search, and decision records are immutable.
* [ADR-0002: Defer a knowledge base format validator](decisions/ADR-0002-defer-format-validator.md) - Superseded by ADR-0004.
* [ADR-0003: Adopt the Open Knowledge Format for the knowledge base](decisions/ADR-0003-adopt-open-knowledge-format.md) - This bundle is OKF v0.2 rather than a house style.
* [ADR-0004: Check knowledge base conformance in CI, not with a ported validator](decisions/ADR-0004-conformance-check-in-ci.md) - What gets checked mechanically, and when.
* [ADR-0005: Use CsCheck for property-based testing](decisions/ADR-0005-property-based-testing-library.md) - A C#, dependency-free library, chosen over FsCheck so the test toolchain stays in one language and pins nothing.
* [ADR-0006: Represent derivation evidence as an immutable evaluated DAG](decisions/ADR-0006-immutable-evidence-dag.md) - Evidence nodes keep evaluated values and immutable child references, with structural sharing and reference identity.
* [ADR-0007: Use a root-only traced value carrier](decisions/ADR-0007-root-only-traced-value-carrier.md) - A readonly value carrier keeps one evidence root as the single source of its evaluated value and derivation.
* [ADR-0008: Evaluate native values before creating derivation evidence](decisions/ADR-0008-native-first-evaluation.md) - Each operation executes the native decimal operation once and creates evidence only after successful evaluation.
* [ADR-0009: Defer multi-type targeting until a second value type is approved](decisions/ADR-0009-defer-multi-type-targeting.md) - Superseded by ADR-0018.
* [ADR-0010: Evaluate only the selected conditional alternative](decisions/ADR-0010-use-lazy-selected-only-branches.md) - Conditional operations use lazy alternatives and record only the selected derivation.
* [ADR-0011: Exclude plain boolean control dependencies from the v1 graph](decisions/ADR-0011-exclude-plain-boolean-control-dependencies.md) - Dependency queries cover recorded value derivation, not condition-only control dependency.
* [ADR-0012: Use explicit mixed decimal operator overloads](decisions/ADR-0012-use-explicit-mixed-decimal-overloads.md) - Both operand orders retain ordinary notation without an implicit traced conversion.
* [ADR-0013: Publish a versioned stable JSON schema](decisions/ADR-0013-publish-versioned-json-schema.md) - JSON export is a versioned compatibility contract.
* [ADR-0014: Encode decimal values as invariant JSON text](decisions/ADR-0014-encode-decimal-as-invariant-json-text.md) - JSON preserves exact decimal value and scale.
* [ADR-0015: Use document-local identities for shared evidence](decisions/ADR-0015-use-document-local-output-identities.md) - Text and JSON share deterministic per-document node identity.
* [ADR-0016: Name the non-generic v1 carrier Traced](decisions/ADR-0016-name-the-v1-carrier-traced.md) - Superseded by ADR-0018.
* [ADR-0017: Fold the creation methods onto the carrier](decisions/ADR-0017-fold-creation-onto-the-carrier.md) - Superseded by ADR-0018; its namespace-collision finding remains binding.
* [ADR-0018: Introduce a closed-set generic Traced carrier for decimal and Int64](decisions/ADR-0018-introduce-closed-set-generic-traced-carrier.md) - Yurai 0.2.0 supports decimal and Int64 through one generic carrier and an inference companion, with homogeneous typed evidence and JSON schema v2.
* [decisions/](decisions/index.md) - The directory, including the ADR template.

# Design

* [Yurai core architecture](design/core-architecture.md) - Architecture drivers, evidence model, runtime boundaries, and implementation seams for the 0.1.x decimal surface and approved 0.2.0 decimal-plus-Int64 carrier.
* [design/](design/index.md) - The architecture document directory.

# Process

* [Knowledge policy](process/knowledge-policy.md) - What belongs in this bundle, how documents are structured, and how they change.
* [Traceability](process/traceability.md) - The `RQ-###` identifiers, and how design, code, and tests refer back to them.
* [Git policy](process/git-policy.md) - Branch naming, commit message style, and merge conditions.
* [Testing and quality strategy](process/testing-and-quality.md) - Test-first evidence, example and property tests, mutation testing, and the quality gate table.

# Elsewhere in the repository

* [AGENTS.md](../AGENTS.md) - Roles, decision rights, agent-to-agent communication, and the language policy.
* [CONTRIBUTING.md](../CONTRIBUTING.md) - The contributor-facing guide: how to build, test, and open a pull request.
* [docs/project-execution-plan.md](../docs/project-execution-plan.md) - Phases, the issue map, and the dependency graph.

This bundle points at those rather than restating them — two copies of a rule become two
different rules. Everything here is written in English, like the rest of the published
documentation; issues and pull requests are Japanese by default
([AGENTS.md §4](../AGENTS.md#4-language-policy)).
