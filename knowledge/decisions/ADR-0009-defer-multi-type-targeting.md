---
type: ADR
title: Defer multi-type targeting until a second value type is approved
description: The decimal MVP remains netstandard2.0-only while inexpensive internal boundaries preserve future options.
tags: [types, decimal, portability, extensibility, adr]
status: draft
requirements: [RQ-004, RQ-023, RQ-028, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0009: Defer multi-type targeting until a second value type is approved

## Context

Yurai must ship a small decimal MVP on `netstandard2.0`, but its purpose is not
intrinsically decimal-only. Multi-targeting or generic numeric abstractions could make a
future extension easier, but today they add compatibility and design obligations without
a second type whose semantics can validate them.

## Decision

Keep the shipped v1 library `netstandard2.0`-only and decimal-concrete. Do not introduce
public generic carriers, generic-math policies, or extra target frameworks until a
second value type is separately approved with its own fidelity contract.

Preserve future options where inexpensive: use type-neutral conceptual vocabulary,
isolate decimal evaluation policy from evidence topology and representation adapters,
and avoid exposing decimal-specific evidence node types publicly. This is option
preservation, not a promise that a future type will require no redesign.

## Consequences

The MVP has one runtime contract and one build asset. Future extension remains possible
but requires a reserved decision based on a real type and its value, exception, and
formatting semantics. The project does not pay current complexity for speculative reuse.
