---
type: ADR
title: Exclude plain boolean control dependencies from the v1 graph
description: A plain bool condition records its outcome but cannot create an evidence edge to values used to compute it.
tags: [branching, dependency, semantics, scope, adr]
status: stable
requirements: [RQ-003, RQ-005, RQ-014, RQ-019]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0011: Exclude plain boolean control dependencies from the v1 graph

## Context

A `bool` carries no derivation evidence. Inferring dependencies from it is impossible,
while asking callers to declare them manually could create evidence that disagrees with
the actual condition. A traced-predicate model would expand v1 into comparison, logical,
and short-circuit semantics.

## Decision

For v1, the conditional operation accepts a plain `bool` and records its outcome. A
value used only to compute that condition is not a dependency of the selected result.
Dependency queries are explicitly limited to recorded value-derivation edges.

Control dependencies may be proposed later as a separate traced-predicate capability;
they are not simulated with caller-supplied opaque dependencies.

## Consequences

The v1 API remains small and semantically honest, but it does not provide complete
control-flow lineage. Documentation and tests must demonstrate the boundary so users do
not mistake value dependency for control dependency.
