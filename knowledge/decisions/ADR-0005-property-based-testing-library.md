---
type: ADR
title: Use CsCheck for property-based testing
description: A C#, dependency-free property-based testing library, chosen over FsCheck so the test toolchain stays in one language and pins nothing.
tags: [testing, property-based, tooling, adr]
status: stable
generated: { by: claude-code/2.1.226, at: 2026-08-09T00:41:34Z }
sources:
  - id: issue-7
    resource: https://github.com/urario/Yurai/issues/7
    title: "Issue #7: testing strategy and quality gate conventions"
  - id: cscheck
    resource: https://github.com/AnthonyLloyd/CsCheck
    title: "CsCheck"
  - id: fscheck
    resource: https://github.com/fscheck/FsCheck
    title: "FsCheck"
---

# ADR-0005: Use CsCheck for property-based testing

## Context

Yurai's central requirement is that a traced calculation produces exactly what plain
`decimal` arithmetic produces — every operation, every rounding mode, every scale, and
the overflow exception too (R1 in the proposal, registered as an `RQ-###` identifier in
[#12](https://github.com/urario/Yurai/issues/12)). That space cannot be covered by a
hand-written list of cases, which is why the
[testing strategy](../process/testing-and-quality.md#property-based-testing) makes
property tests part of the gate for it, and why
[#7](https://github.com/urario/Yurai/issues/7) asks for a library to be picked.

Four constraints shape the choice, and none of them is about the runtime package. A
property-based testing library is a test-only dependency: `src/Directory.Build.props`
turns on the guard in
[`Directory.Build.targets`](../../Directory.Build.targets) that fails the build if the
shipped library ever declares a `PackageReference`, so nothing chosen here can reach a
consumer. What is at stake is the test project's ergonomics and its freedom to move.

- **The codebase is C#.** Source, identifiers, and comments are English C#
  ([AGENTS.md §4](../../AGENTS.md#4-language-policy)), and most of the tests will be
  written by agents working from the surrounding code.
- **The test project is `net8.0`**, while the shipped library is `netstandard2.0`. The
  library under test constrains nothing about the test project's dependencies.
- **The toolchain has to stay unpinned.** xUnit v2 is the current runner
  ([testing strategy](../process/testing-and-quality.md#the-test-toolchain)); a package
  that hard-pins a runner major version decides for the project when it may move.
- **A CI failure has to be reproducible locally.** Properties run with a fresh seed each
  time, so the run's output is the only route back to the failing case.

Choosing a dependency is the maintainer's decision
([AGENTS.md §2](../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides)). This
record was written as a recommendation and approved on
[#43](https://github.com/urario/Yurai/pull/43); what follows is the decision, not a
proposal.

## Decision

Yurai uses **[CsCheck](https://github.com/AnthonyLloyd/CsCheck)** (4.8.0 at the time of
writing) for property-based tests. The version is fixed centrally in
`Directory.Packages.props` alongside the other test packages, and the tests land with
[#9](https://github.com/urario/Yurai/issues/9).

The realistic alternative was **[FsCheck](https://github.com/fscheck/FsCheck)** (3.3.4),
the QuickCheck port most .NET developers have heard of and the one with the larger
community — `FsCheck.Xunit` has roughly 7.2M downloads against CsCheck's 596K. Three
differences decided it against the safer-looking option:

1. **One language in the test project.** FsCheck is an F# library with a C# surface;
   CsCheck is C#. The gap shows up where it costs most — writing a custom generator.
   `decimal` needs one: the default generators of either library sample a convenient
   subset, and the requirement is about the whole space, including `MaxValue`, the
   96-bit mantissa boundary, and every scale. Building that from `Gen` combinators in
   idiomatic C# is ordinary work; building it through a fluent shim over F# types, from
   documentation and answers written in F#, is where an agent-written test quietly ends
   up covering less than it claims to.
2. **No pin, no transitive dependency.** CsCheck has none. `FsCheck.Xunit` depends on
   `FSharp.Core (>= 5.0.2)` and constrains `xunit.extensibility.execution` to
   `< 3.0.0` — so adopting it would hand the property-based testing library a veto over
   when Yurai's test project may move to xUnit v3. CsCheck is framework-agnostic and
   holds no opinion about the runner.
3. **The shapes match the requirement.** CsCheck has model-based and metamorphic testing
   as first-class forms, and R1 is precisely a model-based property: `Traced<decimal>`
   against `decimal` as the reference implementation. Its concurrency testing is
   relevant later, when the thread-safety model designed in
   [#17](https://github.com/urario/Yurai/issues/17) needs evidence rather than assertion.

Reproducibility is a wash and both are adequate: CsCheck prints a seed that replays the
exact case, FsCheck prints a shrunk counterexample and supports a replay seed.

Rejected without much deliberation: **Hedgehog**, which has the best shrinking design of
the three but the same F#-first problem as FsCheck and a smaller C# presence than either;
and **hand-rolled generators**, because shrinking and replay are the parts that are hard
to write and the parts that make a failure useful.

## Consequences

Property tests are written in C# against a dependency-free package, with no transitive
`FSharp.Core` and nothing constraining the runner version. The move to xUnit v3, when the
[testing strategy](../process/testing-and-quality.md#the-test-toolchain)'s trigger fires,
is decided on its own merits.

**The cost is bus factor.** CsCheck is essentially one maintainer's project, and it is
the less-known library — a contributor arriving with property-based testing experience is
more likely to know FsCheck, and the answer to a question is more likely to be findable
for FsCheck. That is accepted rather than dismissed, and two things bound it. The tests
are ordinary xUnit tests that call generators from a small number of places, so the
surface that would have to be ported is small and known. And every counterexample a
property finds
[graduates into an example test](../process/testing-and-quality.md#property-based-testing)
with literal values, so the findings — the expensive part — outlive the library that
found them.

**CsCheck targets `net8.0` and above.** That is invisible today, since the test project
is `net8.0`, and it would only matter if Yurai ever wanted to run its test suite on
.NET Framework. Nothing plans to; if that changes, this record is what gets reopened.

**Random shrinking gives a less minimal counterexample** than FsCheck's integrated
shrinking. In exchange the seed replays the run exactly. Neither property matters much
once the counterexample is a literal in an example test, which is what the strategy
requires of it.

Reopening this is cheap while the number of property tests is small, and expensive once
they are load-bearing. If the choice is going to be revisited, the moment is the first
few properties written under
[#26](https://github.com/urario/Yurai/issues/26) — if writing the `decimal` generator
against CsCheck is worse than this record predicts, that is the evidence, and it arrives
early enough to act on.
