---
type: ADR
title: Name the non-generic v1 carrier Traced
description: The decimal MVP exposes a type-neutral non-generic Traced carrier without promising generic behavior.
tags: [api, naming, decimal, extensibility, adr]
status: draft
requirements: [RQ-015, RQ-023, RQ-028, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0016: Name the non-generic v1 carrier Traced

## Context

`TracedDecimal` exposes the MVP's current value type as the enduring concept, while
`Traced<T>` promises a generic capability that v1 neither needs nor can express cleanly
on `netstandard2.0`. The public name must be useful now without speculating about future
type mechanics.

## Decision

Name the v1 public carrier `Traced`. It is non-generic and its underlying public value
contract is decimal-only. Documentation states that decimal is the MVP scope rather than
the definition of tracing.

## Consequences

The API has a short conceptual name and no unused type parameter. The type name alone
does not reveal the decimal restriction, so signatures and documentation must. A future
generic or additional carrier remains a reserved compatibility decision and is not
promised by this name.
