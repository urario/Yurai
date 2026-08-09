---
type: ADR
title: Evaluate native values before creating derivation evidence
description: Each operation executes the native decimal operation once and creates evidence only after successful evaluation.
tags: [decimal, semantics, correctness, evidence, adr]
status: draft
requirements: [RQ-001, RQ-004, RQ-008, RQ-009, RQ-010, RQ-015, RQ-023, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-09T19:15:55+09:00 }
sources:
  - id: issue-17
    resource: https://github.com/urario/Yurai/issues/17
    title: "Issue #17: core architecture and ADRs"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0008: Evaluate native values before creating derivation evidence

## Context

Yurai's central compatibility promise is that adding evidence never changes a native
calculation's value, scale, rounding behavior, or exception conditions (RQ-001).
Evaluation could build an expression for later execution, replay an operation after
capturing it, or invoke the native operation once and attach evidence only after it
succeeds. Deferred or repeated evaluation makes Yurai responsible for evaluation order
and creates ways to disagree with the host language.

The rule must also cover failures. Constructing a result node before a native operation
completes could expose partial or false evidence when decimal arithmetic throws.

## Decision

Each recorded value operation follows a native-first boundary:

1. obtain operand values from their evidence roots;
2. execute the corresponding native `decimal` operation once, using the same operands
   and parameters;
3. propagate any native exception without returning a result or constructing a parent
   evidence node;
4. after success, construct the immutable parent containing that exact result; and
5. return a new carrier containing the parent reference.

The same rule applies to mixed operands after the plain `decimal` is introduced as an
anonymous input. Mixed arithmetic uses explicit left/right overloads; no implicit
numeric conversion creates evidence at an invisible call-site boundary. In particular,
Yurai provides no implicit conversion from `double` or `float`, because a
precision-changing conversion before decimal arithmetic would make the reference native
expression ambiguous.

`Round(digits, reason)` calls `decimal.Round(value, digits)`, preserving the native
default `MidpointRounding.ToEven`, and records the digits, mode, and reason. Yurai does
not introduce an alternate overflow, division, scale, or rounding policy.

Metadata validation order is specified with each approved public operation. It must not
mask a native arithmetic failure with a different Yurai-created failure when both are
possible for the same call.

## Consequences

Every successfully returned carrier has complete evidence for the exact value it
exposes. Existing operands remain valid after a failed operation, and no failed result
leaves a parent node reachable by callers.

Native exception type and conditions are preserved, but stack traces and allocation
timing include Yurai's operator boundary. RQ-001 requires semantic parity, not an
identical stack trace.

This rule is deliberately harder to reopen than the private carrier representation in
ADR-0007: replacing native-first evaluation would change Yurai's correctness promise,
not merely its storage layout. Supporting another value type requires a type-specific
restatement of native fidelity before the rule is generalized (RQ-029).
