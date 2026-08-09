---
type: ADR
title: Encode decimal values as invariant JSON text
description: JSON export preserves decimal value and scale using an invariant string rather than a JSON number.
tags: [json, decimal, precision, serialization, adr]
status: draft
requirements: [RQ-001, RQ-013]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0014: Encode decimal values as invariant JSON text

## Context

JSON number syntax does not require binary floating point, but many consumers parse it
as IEEE 754 double and lose decimal precision or trailing scale. Emitting both a number
and text creates two representations that could disagree.

## Decision

Encode every decimal evidence value as an invariant-culture JSON string that round-trips
the exact .NET decimal value and scale. Do not emit a second numeric copy. The schema
documents the required parse behavior.

## Consequences

Consumers must parse the string explicitly, but cannot silently lose fidelity through a
binary floating-point default. Tests cover extremes, signs, zero, and trailing scale by
parsing the exported text back to decimal.
