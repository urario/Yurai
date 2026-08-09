---
name: yurai-tdd-review
description: Check that a change was written test-first and that its tests would notice if the code were wrong. Use when reviewing any pull request that changes behavior, adds or edits tests, or reports a mutation or property-test result.
---

# Test-first review

Check the evidence that the tests came first, and that they are tests worth having.

The strategy this checks against is
[`knowledge/process/testing-and-quality.md`](../../../knowledge/process/testing-and-quality.md).
Read it there. This skill says what to look at and in what order; it does not restate the
rules, and where the two ever disagree the strategy document is what binds.

## 1. Does the test-first rule apply at all?

It applies to a change in behavior. Documentation, tooling, formatting, and a pure
refactor carry no test-first obligation.

The case worth stopping on: **a refactor that also changes tests.** Either the behavior
moved — in which case it is not a refactor and the rule applies — or the tests were
coupled to structure rather than behavior. Ask which, in the review. Both answers are
useful and neither is obvious from the diff.

## 2. The evidence

Three forms are accepted, and the strategy ranks them. Find which one the pull request is
offering, then verify it rather than accepting the claim.

**Commit order.** A `test:` commit that fails, followed by the `feat:` or `fix:` commit
that makes it pass. Check the shape of the history:

```shell
git log --oneline --reverse origin/main..HEAD
```

**A named test.** When the branch was squashed or reordered, the description names the
test that fails without the change. Verify it in three commands — this is the check that
most often turns up an assertion written by reading the implementation:

```shell
git checkout origin/main -- src/
dotnet test Yurai.sln --filter "FullyQualifiedName~<TheNamedTest>"
git checkout HEAD -- src/
```

For a new public member the test will not compile against `main` at all. That is a pass.
It should be stated in the description; if it is not, it is a review comment, not a
failure.

**A counterexample with its origin.** Literal values that came from a property failure or
a bug report are test-first by construction. What to check is that the origin is cited —
the seed, the issue — because values with no stated origin are indistinguishable from
values chosen to match the code.

What does not count: "added tests" naming nothing, and a test whose assertions were
written from the implementation. The second looks exactly like a good test. The tell is
that it asserts what the code does rather than what the behavior is required to be —
including its off-by-one, if it has one.

## 3. The kind of test

Example tests are the default; a property answers a question about a space too large to
enumerate. They are not substitutes for one another.

**A property is required, not optional, when a P0 requirement's acceptance criterion is
about every input rather than some.** Decimal parity is the case the project already
knows about — every arithmetic result matching `decimal` exactly, including rounding,
scale, and the overflow exception. A pull request that implements such a requirement with
a hand-written list of cases has not met the gate.

Then check the properties themselves:

- **The name states the law.** `AdditionMatchesDecimal`, not `TestAdd`. A property whose
  name does not say what is invariant is one nobody will be able to interpret when it
  fails.
- **No frozen seed.** A property pinned to one seed is an example test with extra
  machinery: it stops finding anything the day it is written. What the project requires
  instead is that a failure be reproducible from the seed the failing run reports.
- **Iteration count is a lane setting.** Raising it inside one test because that test is
  flaky is a bug being hidden — say so.

**Every counterexample graduates.** When a property finds a failure, the fix lands with
an example test carrying the literal values. If the pull request fixes a property failure
and leaves no example behind, it threw away the most expensive thing the run produced.
That is blocking.

## 4. Traits

A test that establishes a registered requirement carries
`[Trait("RQ", "RQ-NNN")]` with the registered identifier
([traceability](../../../knowledge/process/traceability.md#referring-to-a-requirement)) —
on the test that *establishes* the requirement, not on every test that touches the code
path. A trait on everything says nothing.

Write `RQ-NNN` in review prose and examples, never a real-format identifier: CI rejects
any three-digit `RQ-` string that is not in the registry, and the registry is empty until
[#12](https://github.com/urario/Yurai/issues/12).

## 5. Mutation and benchmarks

Both are advisory today and neither blocks a merge. They run in the deep lane after
merge, not on the pull request.

If the pull request reports a mutation score, it reports **both** numbers — before and
after. If it reports a surviving mutant, exactly three answers are legitimate: write the
test, suppress it as equivalent with a reason at the line, or delete the code. "Raise the
threshold until it passes" is not one of them, and neither is silence.

The running score is task state and belongs on
[#28](https://github.com/urario/Yurai/issues/28), not in `knowledge/`.

## 6. Line coverage

Never a gate. If a pull request offers a coverage percentage as evidence that its tests
are good, that is a finding about the argument, not about the number.

## Reporting

Work the quality gate table in the strategy document and say, row by row, which rows the
pull request satisfies and which it does not. Separate blocking from suggestion:

- **Blocking** — a behavior change with no accepted evidence form; a P0 requirement whose
  criterion is universal implemented without a property; a property failure fixed without
  its counterexample landing as an example test.
- **Suggestion** — naming, placement, a property that could be sharper.

State plainly when a row is not applicable rather than leaving it unmentioned. An
unmentioned row reads as a row nobody checked.
