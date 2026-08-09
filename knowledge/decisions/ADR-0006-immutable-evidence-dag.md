---
type: ADR
title: Represent derivation evidence as an immutable evaluated DAG
description: Evidence nodes keep evaluated values and immutable child references, with structural sharing and reference identity.
tags: [evidence, dag, immutability, concurrency, adr]
status: draft
requirements: [RQ-005, RQ-007, RQ-008, RQ-010, RQ-011, RQ-012, RQ-013, RQ-014]
generated: { by: codex/2026-08, at: 2026-08-09T19:15:55+09:00 }
sources:
  - id: issue-17
    resource: https://github.com/urario/Yurai/issues/17
    title: "Issue #17: core architecture and ADRs"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0006: Represent derivation evidence as an immutable evaluated DAG

## Context

Yurai must preserve how an eagerly evaluated value was derived, allow dependency-path
queries, reuse intermediate derivations without copying them, and remain safe to read
across threads (RQ-005, RQ-007, RQ-011, RQ-014). The evidence must also support text and
JSON representations without making either representation the source model
(RQ-012, RQ-013).

Three credible shapes are available. A copied tree is easy to traverse but duplicates a
reused intermediate for every path. An append-only event list is compact during capture
but does not itself describe dependencies and requires a second model for queries. A
DAG records the actual parent-child relationships and shares reused children, but needs
identity-aware traversal.

The node model must also choose between one tagged object with optional fields, a sealed
family with kind-specific fields, and public interfaces. A tagged object permits invalid
field combinations. Public graph interfaces permanently enlarge the compatibility
surface even though no requirement asks callers to construct or extend evidence.

## Decision

Yurai represents derivation evidence as an internal immutable directed acyclic graph.
Every successful recorded action creates one new parent node containing its already
evaluated `decimal` value and references to already constructed children. Existing nodes
are never modified or copied. Naming creates a parent rather than changing its child.

The implementation uses an internal abstract base with sealed `Input`,
`BinaryOperation`, `Round`, `Branch`, and `Named` node kinds. Each kind has fixed,
read-only fields for its arity and metadata; no per-node child array is required.
Min/Max remain binary operations and store the selected operand explicitly. Branch
evidence stores the condition outcome and selected branch information; issue #18 Q3
decides the public evaluation contract and whether an unselected alternative is ever
represented. A plain `bool` carries no evidence edge back to the values used to compute
it. Q13 must either fix that limitation as the documented v1 boundary or authorize a
separate traced-predicate model; this DAG does not invent a condition dependency from a
boolean alone.

Supported construction cannot produce a cycle: child references must be non-null,
children already exist before the parent, and there is no mutation or rewiring API.
Memory identity is object reference identity. Nodes carry no global or persistent ID.
Each representation or query walks the graph iteratively, tracks visited references,
and may assign deterministic document-local IDs in root-first, left-to-right encounter
order.

Thread safety follows from the graph constraint. Once a root is returned, every object
reachable from it is immutable and there is no shared cache or global identity state to
synchronize.

The internal nodes store `decimal` in the MVP. Their concepts and names remain neutral,
but the node family is not made generic until another value type is approved. The
public model remains hidden, so that future change is an internal migration rather than
a compatibility promise.

## Consequences

Reusing an intermediate preserves one subgraph, and the node count grows linearly with
recorded actions rather than with the number of dependency paths. Dependency queries,
Explain, and JSON can all consume the same authoritative structure without parsing one
another. Concurrent reads require no locks or defensive copies.

Every recorded action allocates a node, and every complete traversal needs temporary
reference-identity state proportional to the reachable graph. A root also keeps all its
reachable evidence alive. These are accepted costs and are measured in issue #27.

Iterative traversal is more code than a recursive visitor, and each adapter must handle
the closed node family. That cost prevents stack exhaustion on deep calculations and
keeps representation policy out of the model. Adding a node kind requires coordinated
updates to all adapters and their exhaustive tests.

Reference identity is deliberately process-local. JSON or text references are stable
within one output call, not across calls, processes, or releases. A stable external ID
would require a separate approved contract.

RQ-011 and RQ-012 require shared derivations to expand once in text and appear as
references afterwards. Q14 decides the reference notation and its correspondence to
JSON document-local IDs; it does not change the in-memory reference-identity decision.
