---
type: ADR
title: Publish a versioned stable JSON schema
description: JSON export is a compatibility contract whose breaking changes require a new schema version.
tags: [json, compatibility, schema, api, adr]
status: stable
requirements: [RQ-004, RQ-013, RQ-027]
generated: { by: codex/2026-08, at: 2026-08-09T20:23:48+09:00 }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# ADR-0013: Publish a versioned stable JSON schema

## Context

Machine-readable evidence is useful only when consumers can depend on its meaning.
Leaving a documented schema unstable preserves producer flexibility but transfers every
change risk to consumers and weakens RQ-013.

## Decision

Publish the first JSON export with an explicit schema version and treat that schema as a
stable public contract. Additive compatible evolution may remain within a version only
where the schema contract explicitly permits it; breaking semantic or structural
changes require a new schema version.

## Consequences

Consumers receive a usable compatibility boundary from v1. The initial schema needs
careful review because mistakes cannot be silently rewritten. Serialization tests and
schema documentation are release artifacts, not implementation notes.
