---
type: ADR
title: Use document-local identities for shared evidence
description: Deterministic numeric IDs identify shared nodes consistently in text and JSON output.
tags: [evidence, identity, json, explain, traversal, adr]
status: stable
requirements: [RQ-011, RQ-012, RQ-013]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0015: Use document-local identities for shared evidence

## Context

Shared DAG nodes must expand once and later appear as references. Names are optional and
non-unique, while structural paths become long and ambiguous for nodes with multiple
parents. Text and JSON need one identity concept without exposing runtime node identity.

## Decision

During each output operation, assign numeric IDs in deterministic traversal order. Text
uses a concise reference marker containing that ID; JSON uses the same conceptual ID
mapping for nodes and edges. The exact text token is part of the text-format design, not
a stable runtime object identifier.

IDs are valid only within one output document. They are not stable across calls,
processes, releases, or graph revisions.

## Consequences

Anonymous and duplicate-named nodes remain unambiguous and both representations share a
testable identity policy. Small graph changes may renumber later nodes, so consumers
must not persist or compare IDs outside their document.
