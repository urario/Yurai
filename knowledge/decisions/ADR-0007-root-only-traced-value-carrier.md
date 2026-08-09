---
type: ADR
title: Use a root-only traced value carrier and native-first evaluation
description: A readonly value carrier keeps one evidence root, and native decimal operations complete before evidence is created.
tags: [value, decimal, semantics, api-boundary, adr]
status: draft
requirements: [RQ-001, RQ-004, RQ-008, RQ-009, RQ-010, RQ-011, RQ-015, RQ-023, RQ-028, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-09T15:30:00+09:00 }
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

# ADR-0007: Use a root-only traced value carrier and native-first evaluation

## Context

Yurai's central compatibility promise is that adding evidence never changes a native
calculation's result, scale, rounding behavior, or exception conditions (RQ-001). The
public value must also compose with ordinary C# arithmetic while keeping the API small
and the evidence immutable (RQ-008, RQ-011, RQ-015).

A carrier could store both a value and evidence, store only an evidence root whose node
contains the value, or be a mutable builder. Duplicating value and root makes every
operation maintain an unnecessary consistency invariant. A builder adds allocation and
allows evidence to change after the value is observed.

Evaluation could build an expression for later execution, replay an operation after
capturing it, or invoke the native operation once and attach evidence only after it
succeeds. Deferred or repeated evaluation makes Yurai responsible for evaluation order
and introduces ways to disagree with the host language.

## Decision

The traced value carrier is a `readonly struct` containing exactly one reference to the
root evidence node. Its exposed value is always the evaluated value stored by that root;
the carrier does not duplicate the `decimal` in another field. The final public type
name and whether it is generic remain issue #18 Q6 decisions. This record decides the
ownership shape, not that reserved name.

Each operation follows a native-first boundary:

1. obtain operand values from their evidence roots;
2. execute the corresponding native `decimal` operation once, using the same operands
   and parameters;
3. propagate any native exception without returning a result or constructing a parent
   evidence node;
4. after success, construct the immutable parent containing that exact result; and
5. return a new carrier containing the parent reference.

The same rule applies to mixed operands after the plain operand is introduced as an
anonymous input. `Round(digits, reason)` calls `decimal.Round(value, digits)`, preserving
the native default `MidpointRounding.ToEven`, and records the digits, mode, and reason.
Yurai does not introduce an alternate overflow, division, scale, or rounding policy.

The carrier contains no cache and implements no mutable interface. It crosses out of a
traced region only by exposing the plain native value; evidence does not attach itself
to that value elsewhere in the program.

The zero-initialized struct state cannot be prevented by a constructor. Its public
failure behavior is intentionally not invented here: the architecture recommends a
defined `InvalidOperationException`, and the maintainer must approve that contract
before S1. Name/reason validation and validation ordering are likewise open public
semantics.

## Consequences

There is one source of truth for value and evidence, so they cannot diverge. Ordinary
carrier use avoids a carrier allocation, existing operands remain valid after a failed
operation, and all successfully returned values have complete immutable evidence.

The carrier may still box when converted to `object` or a non-generic interface. A root
reference also keeps the full reachable graph alive. These costs are visible and
measurable; neither justifies mutable pooling or caching before issue #27 establishes a
baseline.

Native exception type and conditions are preserved, but stack traces and allocation
timing include Yurai's operator boundary. RQ-001 requires semantic parity, not an
identical stack trace. Metadata validation must be specified carefully so it does not
mask a native failure in operations that can fail.

The v1 public API supports only `decimal` (RQ-023). This decision avoids encoding
`decimal` into type-neutral public terminology, but it does not create a public generic
abstraction or a generic-math framework. Supporting another value type remains a future
reserved decision with its own fidelity contract (RQ-028, RQ-029).
