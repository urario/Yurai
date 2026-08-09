---
type: ADR
title: Use a root-only traced value carrier
description: A readonly value carrier keeps one evidence root as the single source of its evaluated value and derivation.
tags: [value, evidence, memory, api-boundary, adr]
status: draft
requirements: [RQ-011, RQ-015, RQ-023, RQ-028, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-17
    resource: https://github.com/urario/Yurai/issues/17
    title: "Issue #17: core architecture and ADRs"
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions Q2-Q6"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0007: Use a root-only traced value carrier

## Context

Each traced value needs to expose its evaluated `decimal` and retain the immutable
evidence that derives it. The carrier representation affects copy cost, allocation,
thread safety, and the risk that the exposed value disagrees with the evidence root.

A carrier could store both a value and evidence, store only an evidence root whose node
contains the value, or be a mutable builder. Duplicating value and root makes every
operation maintain an unnecessary consistency invariant. A builder adds allocation and
allows evidence to change after the value is observed.

## Decision

The traced value carrier is a `readonly struct` containing exactly one reference to the
root evidence node. Its exposed value is always the evaluated value stored by that root;
the carrier does not duplicate the `decimal` in another field. ADR-0016 separately names
the public non-generic carrier `Traced`; this record decides its ownership shape.

The carrier contains no cache and implements no mutable interface. It crosses out of a
traced region only by exposing the plain native value; evidence does not attach itself
to that value elsewhere in the program.

The zero-initialized struct state cannot be prevented by a constructor. Q7 therefore
defines it as invalid: value access, operations, queries, and JSON throw
`InvalidOperationException`, while `ToString()` and `Explain()` return a deterministic
uninitialized diagnostic.

## Consequences

There is one source of truth for value and evidence, so they cannot diverge. Ordinary
carrier use avoids a carrier allocation, and copying the struct shares the same
immutable root rather than cloning or detaching its derivation.

The carrier may still box when converted to `object` or a non-generic interface. A root
reference also keeps the full reachable graph alive. These costs are visible and
measurable; neither justifies mutable pooling or caching before issue #27 establishes a
baseline.

The v1 public API supports only `decimal` (RQ-023). This decision avoids encoding
`decimal` into type-neutral public terminology, but it does not create a public generic
abstraction or a generic-math framework. Supporting another value type remains a future
reserved decision with its own fidelity contract (RQ-028, RQ-029).

This representation is intentionally reopenable if issue #27 demonstrates that another
private carrier shape materially improves measured allocation or copy cost. Reopening it
does not reopen the native evaluation semantics recorded separately in ADR-0008.
