---
type: Process
title: Testing and quality strategy
description: How Yurai is tested — test-first, example tests, property tests, mutation testing — and what has to be true before a pull request merges.
tags: [process, testing, quality, tdd, mutation, property-based]
status: stable
generated: { by: claude-code/2.1.226, at: 2026-08-09T00:41:34Z }
sources:
  - id: issue-7
    resource: https://github.com/urario/Yurai/issues/7
    title: "Issue #7: testing strategy and quality gate conventions"
  - id: issue-9
    resource: https://github.com/urario/Yurai/issues/9
    title: "Issue #9: Stryker.NET and property-based testing infrastructure"
---

# Testing and quality strategy

How Yurai is tested, and what a reviewer checks before a pull request merges. It covers
the test-first rule, the three kinds of test the project writes and when to reach for
each, mutation testing, and the quality gate table that
[AGENTS.md §5](../../AGENTS.md#5-the-pull-request-is-the-quality-gate) delegates here.

Two things this document is not. It is not the merge policy — the conditions a merge
checks and who merges are [git policy](git-policy.md#merge-conditions) and AGENTS.md §5,
and they are not repeated here. And it is not a tutorial: it says what has to hold, not
how to write a good test.

Yurai is a small library with no runtime dependencies, so the whole strategy is sized for
that. A rule that costs more than it protects is a bug — raise it with the maintainer and
cut it ([AGENTS.md](../../AGENTS.md)).

## Test-first

The loop is the ordinary one: **write a test that fails, make it pass with the smallest
change that does, then clean up with the test still passing.** The value is not ceremony.
A test written before the code is a test that has been seen to fail, and a test that has
never failed is a test nobody has any reason to trust.

**A bug fix starts with a test that reproduces the bug.** It is the same rule, and it is
the case where skipping it costs most: a fix with no failing test behind it is a claim
that the bug is gone, and nothing more.

### What counts as evidence

A pull request that changes behavior has to make it visible that the test came first.
Three forms are accepted, in order of preference:

1. **Commit order.** A `test:` commit that fails, followed by the `feat:` or `fix:`
   commit that makes it pass. The history is the evidence and nobody has to write
   anything. This is the default and it is what
   [git policy](git-policy.md#commit-messages) already anticipates.
2. **A named test in the pull request description.** When the branch was squashed,
   amended, or reordered, name the test that fails without the change —
   `RoundTests.NegativeDigitsThrows`, not "tests added". A reviewer can check it in one
   command:

   ```shell
   git checkout origin/main -- src/
   dotnet test Yurai.sln --filter "FullyQualifiedName~RoundTests.NegativeDigitsThrows"
   git checkout HEAD -- src/
   ```

   For a new public member the test will not compile against `main` at all. That is a
   pass, not a failure of the evidence — say so in the description rather than leaving
   the reviewer to discover it.
3. **A counterexample, with its origin.** A test whose literal values came from a
   property failure or a bug report is test-first by construction: the input existed
   before the fix did. Cite where it came from (the seed, the issue) so the next reader
   knows the values are not arbitrary.

What does not count: "added tests" with no test named, and a test whose assertions were
written by reading the implementation. The second is the one to watch for, because it
passes review by looking exactly like the first.

### What is exempt

A pull request that changes no behavior — documentation, tooling, formatting, a pure
refactor — carries no test-first obligation. On a refactor the *existing* tests are the
evidence, which is why a refactor that also changes tests deserves a question in review:
either the behavior moved, or the tests were coupled to structure rather than behavior,
and both are worth knowing.

## Three kinds of test

| Kind | Answers | Where it belongs |
|---|---|---|
| **Example test** | Does this specific case produce this specific result? | The default. Every behavior change. |
| **Property test** | Does this hold for every input in a space too large to enumerate? | Arithmetic, rounding, and overflow behavior against `decimal`. |
| **Benchmark** | Is it fast enough, and does it stay that way? | The hot paths, from [#27](https://github.com/urario/Yurai/issues/27). |

The kinds are not ranked and they are not substitutes. A property test says a law holds
without saying what the code does in the case a reader cares about; an example test says
exactly what happens in one case and nothing about the next one. A public behavior is
documented by its example tests and defended by its properties.

**Example tests are the default.** Reach for a property when the input space is the
point — the decimal-parity requirement (R1 in the proposal, registered as an `RQ-###`
identifier in [#12](https://github.com/urario/Yurai/issues/12)) is the case the project
already knows about: every arithmetic result has to match `decimal` exactly, including
rounding, scale, and the overflow exception, and no hand-written list of cases covers
that.

## Property-based testing

The library is CsCheck, decided in
[ADR-0005](../decisions/ADR-0005-property-based-testing-library.md). The conventions below
are written to hold whichever library is in use, so that a change of library is a change
to one record rather than to this document. The tooling itself lands in
[#9](https://github.com/urario/Yurai/issues/9).

**A property states a law, and its name states the law.** `AdditionMatchesDecimal`, not
`TestAdd`. When a property establishes a registered requirement it carries the trait that
[traceability](traceability.md#referring-to-a-requirement) defines — the same convention
as any other test, on the property that establishes the requirement rather than on every
property that touches the code.

**Properties are not run with a frozen seed.** A property pinned to one seed is an
example test with extra machinery: it stops finding anything the day it is written. The
run picks its own seed, and what the project requires instead is that a failure be
reproducible — the failing run reports a seed that reproduces the case locally.

The consequence is that a property can fail on a pull request that did not cause it. That
is a finding, not flakiness, and it is handled the same way as any other finding: take the
seed from the failed run, reproduce it, and fix it or open an issue for it. **Re-running
until the lane is green is not a fix** — it discards the one input that was hard to find,
and the next run will not find it again.

**Every counterexample graduates.** When a property finds a failure, the fix comes with
an example test carrying the literal values it found. The property keeps looking for the
next one; the example makes sure this one never comes back, and it survives a change of
library. A property failure that is fixed without an example test left behind has thrown
away the most expensive thing the run produced.

**Iteration count is a lane setting, not a per-test one.** The pull request lane runs the
library's default so the suite stays fast; the [deep lane](#two-lanes) raises it. Raising
the count inside one test, because that test is flaky, is a bug being hidden.

## Mutation testing

Coverage tells you a line ran. A mutation score tells you a test would have noticed if
that line had been wrong, which is the question the project actually has about its tests.
Yurai uses [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/) —
fixed to a version in `.config/dotnet-tools.json`, configured and wired up in
[#9](https://github.com/urario/Yurai/issues/9).

**Scope is `src/Yurai/`.** The test project is not mutated, and neither is anything
outside the shipped library.

**It does not block a pull request, and it does not run on one.** A mutation run is
minutes of CPU where the pull request lane is seconds, and a score computed on an
unfinished branch is noise. It runs in the [deep lane](#two-lanes).

**Thresholds.** Stryker reads three: `high` and `low` colour the report, `break` fails
the run.

| Setting | Value now | At the P0 gate |
|---|---|---|
| `break` | `0` — the run never fails | Set by the maintainer at [#29](https://github.com/urario/Yurai/issues/29) |
| `low` | `70` | `70` |
| `high` | `80` | `80` |

The target for `src/Yurai/` is **80%**, and it is deliberately not enforced yet. There is
no implementation to measure — the number would be a guess dressed as a threshold. The
cycle is: measure a baseline once the MVP exists
([#28](https://github.com/urario/Yurai/issues/28)), then the maintainer sets `break` at
the P0 gate ([#29](https://github.com/urario/Yurai/issues/29)) against what the baseline
says is real. The number they choose lands as an ordinary pull request against the Stryker
configuration and this table, because a threshold is a convention that outlives the issue
that produced it.

**A surviving mutant is a finding, not a failure.** Three answers are legitimate, and
which one applies is a review judgement:

- the test suite has a hole → write the test;
- the mutant is equivalent — the mutated program behaves identically, so no test could
  kill it → suppress it with Stryker's disable comment at the line, which takes a reason,
  so the next reader sees the argument and not just the suppression;
- the code is unreachable or redundant → delete the code.

"Raise the threshold until it passes" is not on the list.

**A score belongs to a run, not to this document.** The lane's report is the record. A
pull request that moves the score says so in its description with both numbers; the
running value is task state and lives on
[#28](https://github.com/urario/Yurai/issues/28) and in the run's artifacts, not in
`knowledge/` ([knowledge policy](knowledge-policy.md#what-does-not-belong-here)).

## Coverage

Line coverage is a diagnostic, available to anyone who wants it, and it is **never a
gate**. It answers "which code did no test touch at all", which is worth knowing and is
the only thing it answers honestly. As a threshold it measures the wrong thing and
rewards the wrong work: a percentage can be moved by tests that execute code without
asserting anything about it, and on a library this small the last few points cost more
than they find. Mutation score is the structural signal this project gates on, and it
subsumes what a coverage gate would have caught.

No tooling is mandated for it. If a coverage number is ever wanted as a required check,
that is a change to this document and to whichever issue owns the tooling — not something
to add to a pull request in passing.

## Benchmarks

Performance is a requirement in the proposal (graphs beyond ten thousand nodes) and it
gets its baseline in [#27](https://github.com/urario/Yurai/issues/27). Until that
baseline exists there is no threshold to state, and stating one now would be the same
guess the mutation threshold is avoiding. Benchmarks are advisory, they run in the deep
lane, and the numbers that turn into a gate are written here when #27 produces them.

## Two lanes

| Lane | Runs on | Contains | Blocks a merge |
|---|---|---|---|
| **Pull request lane** | Every pull request to `main`, and every push to `main` | Build, example and property tests, format, knowledge base conformance | Yes |
| **Deep lane** | Push to `main` touching `src/**` or `tests/**`, and `workflow_dispatch` | Mutation run, long property runs, benchmarks | No |

The split is what keeps both useful. Fast feedback stays fast, and the slow signals stay
honest instead of being trimmed until they fit in a pull request's patience.

The deep lane runs after merge rather than on a nightly schedule so that a score change
is attributable to the merge that caused it. A weekly run over a week of merges tells you
the number moved and leaves you to work out why.

**Non-blocking is not optional.** A lane nobody reads should be deleted, not left running
as decoration, so its output has two fixed readers: a failing deep lane on `main` gets an
issue like any other failure, and its results are what the gate issues
([#28](https://github.com/urario/Yurai/issues/28),
[#29](https://github.com/urario/Yurai/issues/29)) review. "Advisory" means it does not
stop a merge; it does not mean nobody has to look.

Both lanes' job names are kept stable, because branch protection refers to them by name
([git policy](git-policy.md#branch-protection)).

## The quality gate

What has to hold before a pull request merges. The first row is
[git policy](git-policy.md#merge-conditions)'s merge condition seen from the testing side;
the rest is what this document adds.

| Check | Applies to | Blocks merge | Judged by |
|---|---|---|---|
| The required CI checks, named in [git policy](git-policy.md#merge-conditions) | Every pull request | **Yes** | CI |
| Test-first evidence, in one of the [three accepted forms](#what-counts-as-evidence) | A pull request that changes behavior | **Yes** | Review |
| A property, where the requirement's acceptance criterion is about every input rather than some | A pull request implementing a P0 requirement | **Yes** | Review |
| A counterexample from a property failure landed as an example test | A pull request fixing a property failure | **Yes** | Review |
| Mutation score | Changes under `src/Yurai/` | No — advisory until the P0 gate sets `break` | Deep lane, then review |
| Benchmark numbers | Changes to a hot path | No — advisory until [#27](https://github.com/urario/Yurai/issues/27) | Deep lane, then review |
| Line coverage | — | **Never** | Diagnostic only |

"Blocks merge: Review" means a human or an agent reviewer says so in the pull request,
and an unaddressed comment blocks the merge exactly as
[git policy](git-policy.md#merge-conditions) describes. There is no automation for the
review rows, and inventing one — a bot that parses commit prefixes and calls it test-first
— would produce a check that is easy to satisfy without doing the thing it is named after.

## The test toolchain

The test project stays on **xUnit v2** for now — the decision the note in
[`Directory.Packages.props`](../../Directory.Packages.props) points at.

v2 is the version the tooling around it has been exercised against for years, and being an
early adopter here would be paid for in a place with no upside for Yurai: a mutation run
that cannot discover tests is a day of debugging that buys the project nothing. The
trigger to revisit is a concrete one — when the chosen property-based testing library,
Stryker.NET, and the runner all support v3 with none of them holding the others back.
`FsCheck.Xunit` is the current example of exactly that kind of hold
([ADR-0005](../decisions/ADR-0005-property-based-testing-library.md)). Until the trigger
fires, v2 is not technical debt; it is the version the tools work with.

Test project layout is otherwise left to whoever writes the tests: one test class per
behavior area, names that describe the behavior rather than the method under test, and
property tests kept in files a filter can select. These are review judgements, not rules
([AGENTS.md §2](../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides)).

## What this strategy deliberately does not do

- **No coverage threshold.** Explained [above](#coverage); it is a decision, not an
  omission.
- **No mechanical test-first check.** The evidence forms are checkable by a reviewer in
  one command, and that is where the judgement belongs.
- **No test-artifact identifiers.** Tests are traced to requirements by the trait, not by
  an identifier of their own — [ADR-0001](../decisions/ADR-0001-lightweight-knowledge-base.md).
- **No integration or end-to-end tier.** Yurai is a library with no I/O. Everything is a
  unit test until something in it talks to the outside world, and nothing is planned that
  does.
- **No mutation threshold before there is code to measure.** The number arrives with the
  baseline, from [#28](https://github.com/urario/Yurai/issues/28).
