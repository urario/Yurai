# Traceability

How a requirement stays connected to the design, code, and tests that satisfy it.

One rule carries the whole convention:

> **Requirements are identified as `RQ-###`, and anything that exists to satisfy a
> requirement names it.**

That is the entire mechanism. There is no artifact identifier for designs, modules, or
test cases, and no matrix to keep in step — see
[ADR-0001](../decisions/ADR-0001-lightweight-knowledge-base.md) for why the heavier
alternative was turned down.

## The identifier

`RQ-###` — the literal prefix `RQ-` and a zero-padded three-digit number: `RQ-001`,
`RQ-042`. Numbers are assigned in order of registration and carry no meaning: `RQ-002`
is not a sub-requirement of `RQ-001` and is not more important than `RQ-030`.

**An identifier is never reused.** A requirement that is dropped stays in the registry
with status `Withdrawn` and a sentence saying why. Gaps in the numbering are normal and
are not to be filled — a gap costs nothing, while a recycled identifier silently
invalidates every reference to it.

**Splitting and merging is expected.** When a requirement turns out to be two, the
original is superseded and two new identifiers are registered; the old entry stays with
`Superseded by RQ-051, RQ-052`. Getting the granularity wrong at first is normal
([#12](https://github.com/urario/Yurai/issues/12)) and this is how it is corrected.

## The registry

[`knowledge/requirements/`](../requirements/index.md) holds the single registry of
requirement identifiers. An identifier that is not in the registry does not exist, and
the registry is the only place an identifier is defined. Each entry carries:

| Field | Values |
|---|---|
| ID | `RQ-###` |
| Statement | What is required, in one testable sentence |
| Priority | `P0` (must ship in v1.0) / `P1` / `P2` |
| Status | `Draft` / `Accepted` / `Withdrawn` / `Superseded by RQ-###` |
| Acceptance criteria | How anyone can tell whether it holds |

Non-goals — the things Yurai deliberately will not do — are registered too, as
requirements phrased in the negative. A stated non-goal is a decision that stops
recurring debate; an unwritten one is a suggestion.

## Referring to a requirement

| From | How |
|---|---|
| **ADR / design document** | The `Requirements:` line in the document header |
| **Pull request** | The requirement identifiers in the description, alongside `Refs #N` |
| **Test** | `[Trait("RQ", "RQ-001")]` on the test that establishes the requirement |
| **Code** | A comment, only where the connection is not obvious from the surrounding code |

Two of these need a word of explanation.

**In tests**, the trait is the load-bearing reference, because it is the one a machine
can check:

```csharp
[Fact]
[Trait("RQ", "RQ-001")]
public void AdditionMatchesDecimalExactly() { /* ... */ }
```

```shell
dotnet test Yurai.sln --filter "RQ=RQ-001"
```

Tag the test that *establishes* the requirement, not every test that happens to touch
the code path — a trait on everything says nothing. The testing and quality strategy
([#7](https://github.com/urario/Yurai/issues/7)) may extend this convention; it will not
contradict it without saying so.

**In code**, restraint is the point. `// RQ-004: netstandard2.0, no runtime
dependencies` on a project property earns its place; the same comment repeated on thirty
methods is noise that stops being read. Code is traced to requirements through the
tests, not through comment density.

## What "traceable" has to mean

For every `P0` requirement, both directions have to work:

- **Forward** — from the requirement to at least one test that would fail if the
  requirement were violated.
- **Backward** — from that test to the requirement, by its identifier.

`P1` and `P2` requirements are held to the same standard once implemented, but their
absence does not block a release.

This is checked at the gate issues ([#29](https://github.com/urario/Yurai/issues/29) for
the P0 requirements), not on every pull request. Two commands are enough to see the
state of it:

```shell
# Which requirements are referenced by tests?
grep -rho 'RQ-[0-9]\{3\}' tests/ | sort -u

# Everywhere a given requirement is referenced
grep -rn 'RQ-001' knowledge/ src/ tests/
```

A P0 requirement missing from the first list is either untested or untagged. Both are
findings; the gate review decides which.

## The limits of this

Search-derived traceability tells you a link exists. It does not tell you the test is a
good one, and it will not notice a requirement that quietly stopped being true. That
judgement belongs to review and to the gate issues, which is where it was left on
purpose. If the record turns out to be too thin to answer a real question — the risk
[#8](https://github.com/urario/Yurai/issues/8) flagged — the Phase 2 gate review is the
place to say so and tighten it.
