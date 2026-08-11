---
type: ADR
title: Fold the creation methods onto the carrier and remove the facade type
description: A type named after its own namespace cannot be reached from a consumer's namespace, so Traced gains the creation methods and the Yurai facade is deleted.
tags: [api, naming, usability, adr]
status: deprecated
superseded_by: ADR-0018
requirements: [RQ-002, RQ-015]
generated: { by: codex/2026-08, at: 2026-08-11T15:32:43+09:00 }
sources:
  - id: issue-66
    resource: https://github.com/urario/Yurai/issues/66
    title: "Issue #66: sync the README with the implementation"
  - id: ci-run
    resource: https://github.com/urario/Yurai/actions/runs/31381485538
    title: "CI run 31381485538: five CS0234 errors from a consumer namespace"
  - id: adr-0016
    resource: ADR-0016-name-the-v1-carrier-traced.md
    title: "ADR-0016: Name the non-generic v1 carrier Traced"
---

# ADR-0017: Fold the creation methods onto the carrier and remove the facade type

> **Superseded by [ADR-0018](ADR-0018-introduce-closed-set-generic-traced-carrier.md).**
> The namespace-collision finding remains binding, but 0.2.0 places creation methods on
> the arity-different non-generic inference companion rather than the generic carrier.
> The original reasoning remains below.

## Context

The library shipped two public types: the carrier `Traced` and a static class `Yurai`
holding the five creation methods — `Of` (two overloads), `Min`, `Max`, and `If`. Both
live in namespace `Yurai`.

A static class named `Yurai` inside a namespace named `Yurai` cannot be reached the way
the documentation showed. A consumer writing `using Yurai;` in their own namespace and
then `Yurai.Of(1000m, "BasePrice")` does not get the type: the simple name `Yurai`
resolves to the namespace, which is a member of the global namespace, and the compiler
then looks for a type called `Of` inside it. The result is `CS0234`, not a call.

This went unnoticed for the whole of Phase 2 because every test lives in
`namespace Yurai.Tests` and aliases `global::Yurai.Yurai` to work around the same
shadowing from the inside. The alias made the tests compile and hid the fact that nobody
outside could. It surfaced only when [#66](https://github.com/urario/Yurai/issues/66)
added tests in a namespace outside `Yurai.*`, which failed with five `CS0234` errors in
[CI run 31381485538](https://github.com/urario/Yurai/actions/runs/31381485538).

The facade's name was never a recorded decision. [ADR-0016](ADR-0016-name-the-v1-carrier-traced.md)
fixed the *carrier* name; the facade came along from the proposal's API sketch and no ADR
covers it. The general form of the problem is the .NET design guideline against giving a
type the same name as a namespace.

The library is at 0.1.0 and has not been published. A breaking change costs nothing today
and costs a major version tomorrow.

## Decision

Delete the `Yurai` static class. `Traced` gains the five creation methods as public static
members: `Traced.Of`, `Traced.Min`, `Traced.Max`, and `Traced.If`. The method bodies move
unchanged — this decision is about reachability and naming, not behavior.

`Traced` does not collide with the namespace, so the entry point is reachable from any
consumer namespace with a single `using Yurai;`. Static factory methods on the type they
construct are the ordinary .NET shape for this.

## Consequences

The assembly exports one public type instead of two. RQ-015 asks for a minimal and
coherent public surface reviewed against purpose rather than a count, and one fewer type
carrying the same five logical operations is a straightforward improvement against it.
The declared CLR member inventory moves the five methods from the facade onto the carrier
rather than adding any.

This is a breaking change for anyone already compiled against the facade. Nobody is: the
package has never been published. The change is taken now precisely because that is true.

Documentation that showed `Yurai.Of` is wrong and is corrected in the same pull request —
the README, both samples, and the JSON schema document. The consumer-namespace tests added
in #66 stay outside `Yurai.*` deliberately: they are the regression test for this decision,
and moving them into `Yurai.Tests` would silently restore the blind spot.

RQ-002 asks that the explanation be readable by a first-time developer within five
minutes. An entry point that cannot be written as documented fails that before the
explanation is ever reached.

This supersedes nothing. No ADR named the facade.

A future generic carrier — the value-type work in
[#67](https://github.com/urario/Yurai/issues/67) — inherits the creation methods as part
of the carrier rather than as a separate type to genericize alongside it. That is a
simplification for the later decision, not a constraint on it.
