---
type: ADR
title: Evaluate only the selected conditional alternative
description: Conditional operations accept lazy alternatives and record only the selected derivation.
tags: [branching, semantics, evidence, api, adr]
status: stable
requirements: [RQ-001, RQ-005, RQ-014]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0010: Evaluate only the selected conditional alternative

## Context

Accepting two already evaluated values makes a Yurai conditional execute work and
possibly throw from the unselected alternative, unlike native `if` and `?:` behavior.
It also leaves the evidence model unable to distinguish selection from eager evaluation.

## Decision

The conditional public operation accepts lazy alternatives. After receiving the plain
boolean condition, it invokes the selected alternative exactly once. It neither invokes
nor records the unselected alternative. The branch node records the condition outcome,
the selected branch label, and the selected derivation.

## Consequences

Exception, side-effect, and cost behavior matches short-circuit selection. Calls require
lambdas or delegates and may allocate. Tests must prove single invocation and zero
invocation of the unselected delegate, including when that delegate would throw.
