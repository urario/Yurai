---
type: ADR
title: Use explicit mixed decimal operator overloads
description: Mixed arithmetic supports both operand orders without an implicit conversion into the traced region.
tags: [api, decimal, operators, boundaries, adr]
status: draft
requirements: [RQ-008, RQ-009, RQ-015]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0012: Use explicit mixed decimal operator overloads

## Context

Mixed arithmetic should retain ordinary C# notation in both operand orders. An implicit
conversion from `decimal` reduces the raw member count, but also permits invisible entry
into traced regions from assignments and method calls and complicates overload
resolution. RQ-015 evaluates API purpose rather than optimizing a raw count.

## Decision

Define explicit left- and right-hand `decimal` operator overloads for supported mixed
arithmetic. Each overload introduces the plain operand as an anonymous input at the
operation site. Provide no implicit numeric conversion into `Traced`, including from
`decimal`, `double`, or `float`.

## Consequences

The CLR surface contains more members than an implicit-conversion design, but no extra
logical operations. Traced-region entry stays visible and mixed-expression evidence is
created at a predictable site. API review counts and reports both measures.
