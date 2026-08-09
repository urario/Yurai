---
type: Requirements Registry
title: Requirements registry
description: The single registry of Yurai's RQ-### requirement identifiers, with priorities, statuses, and acceptance criteria.
tags: [requirements, traceability]
status: draft
generated: { by: claude-code/2.1.226, at: 2026-08-09T05:30:00Z }
sources:
  - id: issue-8
    resource: https://github.com/urario/Yurai/issues/8
    title: "Issue #8: bootstrap the knowledge base"
  - id: issue-12
    resource: https://github.com/urario/Yurai/issues/12
    title: "Issue #12: write the requirements specification"
  - id: issue-15
    resource: https://github.com/urario/Yurai/issues/15
    title: "Issue #15: README draft — positioning, novelty ceiling, banned phrases"
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: ADR for open questions Q2-Q6"
  - id: issue-29
    resource: https://github.com/urario/Yurai/issues/29
    title: "Issue #29: Phase 2 gate — P0 requirements R1-R6 review"
  - id: execution-plan
    resource: ../../docs/project-execution-plan.md
    title: "Yurai project execution plan"
---

# Requirements registry

The registry of Yurai's requirements. Every `RQ-###` identifier used anywhere in the
repository is defined here, and an identifier that is not in this table does not exist.

The specification is derived from the project proposal (Draft 1.0) — its MVP scope
(§7.1), explicit non-goals (§7.2), P0 requirements R1–R6 (§8), and marketing
constraints (§9) — as those sections are quoted in
[#12](https://github.com/urario/Yurai/issues/12),
[#15](https://github.com/urario/Yurai/issues/15),
[#29](https://github.com/urario/Yurai/issues/29), and the implementation issues
[#19](https://github.com/urario/Yurai/issues/19)–[#26](https://github.com/urario/Yurai/issues/26).
Section references like §7.1 below point into that proposal.

Two phrasing rules hold throughout:

- **Requirements state what and why, not how.** API names from the proposal (`Of`,
  `Explain`, `ToJson`, …) appear as source citations and illustrations; the binding
  API shape is decided in the architecture work
  ([#17](https://github.com/urario/Yurai/issues/17),
  [#18](https://github.com/urario/Yurai/issues/18)), not here.
- **Requirements are phrased against "the underlying value type".** Yurai traces
  computations over some value type; in the MVP that type is `decimal` and no other
  (RQ-023). Where a requirement is essential to Yurai it names the underlying type;
  where it is an MVP bound it names `decimal`. Keeping the two apart is itself a
  requirement (RQ-028).

The identifier rules — three digits, never reused, split by supersession — are in
[traceability](../process/traceability.md).

## Registry

| ID | Requirement | Priority | Status | Verified by |
|---|---|---|---|---|
| RQ-001 | Value fidelity: results identical to native arithmetic (R1) | P0 | Draft | Property suite against plain `decimal` (#26); trait `RQ-001` |
| RQ-002 | Explain output readable by a first-time developer in five minutes (R2) | P0 | Draft | First-look review of the README sample (#23, gate #29) |
| RQ-003 | Trace means the dependency path of a value, nothing else (R3) | P0 | Draft | Vocabulary review of API, doc comments, docs (#25, gate #29) |
| RQ-004 | Zero runtime dependencies, `netstandard2.0` (R4) | P0 | Draft | Zero-package-reference build check (#2, CI, gate #29) |
| RQ-005 | Conditionals record the branch actually taken (R5) | P0 | Draft | Branch-recording tests (#22); trait `RQ-005` |
| RQ-006 | README names related work and states the differences (R6) | P0 | Draft | README review (#15, gate #29) |
| RQ-007 | Named inputs and named intermediate results | P0 | Draft | Unit tests (#19); trait `RQ-007` |
| RQ-008 | Arithmetic composition with ordinary operators, plus Min/Max | P0 | Draft | Unit tests (#19, #20) and property suite (#26) |
| RQ-009 | Mixed operations with plain, untraced values | P0 | Draft | Unit tests (#20) and property suite (#26) |
| RQ-010 | Rounding is an explicit, recorded operation with a reason | P0 | Draft | Unit tests (#21) and property suite (#26) |
| RQ-011 | Immutable derivation evidence with structure sharing | P0 | Draft | Structure and immutability tests (#19) |
| RQ-012 | Human-readable derivation output | P0 | Draft | Output tests against sample expectations (#23) |
| RQ-013 | Machine-readable derivation export | P0 | Draft | Serialization tests, all node kinds (#24) |
| RQ-014 | Programmatic dependency queries | P0 | Draft | Path-query tests on shared structures (#25) |
| RQ-015 | Public API surface of at most 20 members | P0 | Draft | Surface count at gate review (#29) |
| RQ-016 | Non-goal: symbolic algebra | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-017 | Non-goal: sensitivity analysis and automatic differentiation | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-018 | Non-goal: attribution | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-019 | Non-goal: taint tracking and general-purpose provenance | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-020 | Non-goal: audit-platform features | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-021 | Non-goal: LaTeX or HTML rendering | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-022 | Non-goal: evaluating expressions supplied as strings | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-023 | MVP non-goal: value types other than `decimal` | P0 | Draft | Design review against non-goals (gate #29) |
| RQ-024 | Banned phrases and replacement vocabulary | P0 | Draft | Vocabulary scan of prose and diffs (gate #29) |
| RQ-025 | Novelty claim ceiling: the single proposal §6.4 sentence | P0 | Draft | README and documentation review (#15, gate #29) |
| RQ-026 | Configurable explanation output (depth, culture, format) | P1 | Draft | Deferred; tests when a post-MVP issue implements it |
| RQ-027 | JSON export schema documented and versioned | P1 | Draft | Schema document review (#24, decision #18 Q5) |
| RQ-028 | Value-type extensibility stays open | P2 | Draft | Review of requirements, design, and API wording (#17, #18 Q4, gate #29) |

- **Priority** — `P0` must ship in v1.0; `P1` and `P2` are wanted but do not block a
  release.
- **Status** — `Draft`, `Accepted`, `Withdrawn`, or `Superseded by RQ-NNN`. This column
  tracks the requirement; the document's own `status` in frontmatter tracks the registry.
- **Verified by** — how the requirement is demonstrated: tests carrying its trait, a
  benchmark, or a review at a gate issue. Named in kind, not enumerated case by case;
  the tests themselves are found through the trait
  ([traceability](../process/traceability.md)).

Each requirement has a section below with its full statement and its acceptance
criteria — enough that two people reading it independently would agree on whether it
holds.

## Essential requirements

What Yurai is, independent of any release. These are phrased against the underlying
value type; the MVP instantiates them for `decimal` (RQ-023).

### RQ-001 — Value fidelity (R1)

- **Priority**: P0
- **What**: Every result produced through Yurai is identical to the result of the same
  computation performed directly on the underlying value type. For the MVP this means:
  bit-identical to plain `decimal` arithmetic in all cases, including rounding
  behavior, scale preservation, and the exceptions native arithmetic throws (overflow,
  division by zero) — thrown under the same conditions with the same exception types.
  The proposal states the bar plainly: this does not ship until it passes (§8).
- **Why / user value**: Yurai asks to sit in the middle of business-critical
  calculations. A developer can only accept that if adopting Yurai changes no result,
  ever — the explanation is added value on top of a number that stays exactly what it
  was. Yurai holds no opinion of its own about the value (§5.3); trust in that is the
  product.
- **Acceptance criteria**:
  - Property-based tests compare Yurai results against the same expression evaluated
    on plain `decimal` — arbitrary operand combinations and operator chains, rounding
    and scale, Min/Max, mixed operations, and composite expressions — and pass
    ([#26](https://github.com/urario/Yurai/issues/26), ADR-0005).
  - Cases where native arithmetic throws are covered: the same exception type under
    the same conditions.
  - Any discovered divergence is fixed, or explicitly accepted by the maintainer as a
    known limitation recorded here.
- **Constraints and notes**: "Identical" is defined by the underlying type. For
  `decimal` it is bit-identity including scale. Other value types define equality and
  edge cases differently (`double` has NaN and signed zeros; integers overflow by
  wrapping or throwing depending on context) — if a type beyond `decimal` is ever
  adopted (RQ-028), fidelity must be restated for that type first. The requirement
  itself — never disagree with the native computation — is type-neutral.
- **Source**: §8 R1, §5.3; [#26](https://github.com/urario/Yurai/issues/26),
  [#29](https://github.com/urario/Yurai/issues/29).

### RQ-002 — Explainability within five minutes (R2)

- **Priority**: P0
- **What**: The human-readable explanation of a value (RQ-012) is understandable by a
  developer who has never seen Yurai: reading the first ten lines of the README —
  sample code plus its explanation output — they understand what the library gives
  them within five minutes.
- **Why / user value**: The value proposition is that a computed domain value can say
  where it came from. If the explanation needs explaining, that value never arrives.
  This is the Phase 1 gate criterion ("value clear in five minutes") applied to the
  library's own output.
- **Acceptance criteria**:
  - The README opening sample and its output exist and fit the first ten lines
    ([#15](https://github.com/urario/Yurai/issues/15),
    [#23](https://github.com/urario/Yurai/issues/23)).
  - A first-look review by someone outside the implementation confirms comprehension,
    recorded at the gate ([#29](https://github.com/urario/Yurai/issues/29)). The
    judgement is subjective by nature and is the maintainer's call.
- **Source**: §2.3, §8 R2; [#23](https://github.com/urario/Yurai/issues/23),
  [#29](https://github.com/urario/Yurai/issues/29).

### RQ-003 — Trace means dependency path, nothing else (R3)

- **Priority**: P0
- **What**: Yurai answers two of the four kinds of "why" question about a value: which
  inputs it depends on (dependency) and how it was computed from them (derivation). It
  does not answer, and does not appear to answer, how much a result would change if an
  input changed (sensitivity) or how much of a result is owed to a given input
  (attribution). The word "trace" in Yurai's vocabulary means the dependency path of a
  value — not an execution log, not a diagnostic trail.
- **Why / user value**: An honest tool states what its answers mean. A user who
  mistakes a dependency path for a sensitivity or attribution statement will make a
  wrong business decision with it; keeping the vocabulary narrow protects the user
  from a wrong reading, and protects the library from scope creep re-litigated in
  every feature request.
- **Acceptance criteria**:
  - No public API name, XML doc comment, README section, or published document uses
    vocabulary suggesting sensitivity or attribution
    ([#25](https://github.com/urario/Yurai/issues/25)); the word "contribution" is not
    used in the documentation (§4).
  - The dependency-query documentation states explicitly that a trace is a dependency
    path and what that does not imply.
  - Checked as a vocabulary review at the gate
    ([#29](https://github.com/urario/Yurai/issues/29)).
- **Constraints and notes**: The four-way classification of "why" questions comes from
  §4 and is reproduced in the README ([#15](https://github.com/urario/Yurai/issues/15)).
  The permanent exclusions themselves are RQ-017 and RQ-018.
- **Source**: §4, §8 R3; [#25](https://github.com/urario/Yurai/issues/25),
  [#29](https://github.com/urario/Yurai/issues/29).

### RQ-004 — Zero runtime dependencies, netstandard2.0 (R4)

- **Priority**: P0
- **What**: The shipped library targets `netstandard2.0` and has no runtime package
  dependencies — BCL only. This includes serialization: the machine-readable export
  (RQ-013) is implemented without `System.Text.Json` or any other package.
- **Why / user value**: Yurai must be droppable into any .NET codebase — including
  legacy .NET Framework applications where domain calculations actually live — without
  a version conflict, a transitive dependency, or a supply-chain review. A lineage
  library that brings its own dependency tree fails the audit conversation it was
  bought for.
- **Acceptance criteria**:
  - `src/Yurai/` builds for `netstandard2.0` with zero `PackageReference` entries;
    enforced mechanically (`EnforceZeroDependencies`, CI, and per-PR review).
  - Changing either constraint is a reserved decision
    ([AGENTS.md §2](../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides)).
- **Source**: §8 R4; [#2](https://github.com/urario/Yurai/issues/2),
  [#24](https://github.com/urario/Yurai/issues/24).

### RQ-005 — Conditionals record the branch taken (R5)

- **Priority**: P0
- **What**: When a value is produced by choosing between alternatives, the derivation
  evidence records which branch was actually taken, under the name the developer gave
  that decision. The chosen branch's derivation is part of the evidence; the value is
  identical to what the equivalent plain conditional would produce (RQ-001).
- **Why / user value**: "Why is this zero?" is usually "which rule fired?". Arithmetic
  lineage without decision lineage explains the easy half of a calculation; recording
  the branch makes the explanation cover the part users actually ask about.
- **Acceptance criteria**:
  - Tests verify the recorded branch name for both the then-side and the else-side,
    and for nested conditionals ([#22](https://github.com/urario/Yurai/issues/22)).
  - The branch name appears in the human-readable output (RQ-012) and the
    machine-readable export (RQ-013).
- **Constraints and notes**: The API shape (lazy or eager alternatives, short-circuit
  behavior versus what is recorded) is decided in
  [#18](https://github.com/urario/Yurai/issues/18) Q3, not fixed here.
- **Source**: §5.2, §8 R5; [#22](https://github.com/urario/Yurai/issues/22).

### RQ-006 — Related work stated in the README (R6)

- **Priority**: P0
- **What**: The README carries a Related Work section naming the prior art (Petit
  Poucet, handcalcs, Calcpad, NCalc, Audit.NET) and stating Yurai's differences
  itself — including the contrast that audit logging records *what happened* while
  Yurai answers *why this value*.
- **Why / user value**: A user evaluating Yurai will find these projects anyway.
  Naming them first saves the user the comparison work, and it is what makes the
  bounded novelty claim (RQ-025) credible rather than promotional.
- **Acceptance criteria**:
  - The Related Work section exists in the README with the named projects and explicit
    differences ([#15](https://github.com/urario/Yurai/issues/15)), confirmed at the
    gate ([#29](https://github.com/urario/Yurai/issues/29)).
- **Source**: §8 R6; [#15](https://github.com/urario/Yurai/issues/15).

## MVP functional scope

What v1.0 does, derived from §7.1. The underlying value type throughout the MVP is
`decimal` (RQ-023). API names cited from §5.2 are illustrations of the proposal's
intent; their binding shape is fixed by [#17](https://github.com/urario/Yurai/issues/17)
and [#18](https://github.com/urario/Yurai/issues/18).

### RQ-007 — Named inputs and named intermediate results

- **Priority**: P0
- **What**: A developer can bring a plain value into a traced computation under a
  domain name (a named input), bring one in without a name (an anonymous input,
  RQ-009), and attach a name to an intermediate result after computing it (a named
  operation). Names are the developer's domain vocabulary, and they are what
  explanations and queries speak in.
- **Why / user value**: An explanation is only readable (RQ-002) if it says
  `BasePrice` and `DiscountedPrice` rather than positional operands. Naming is how the
  developer's intent gets into the evidence.
- **Acceptance criteria**:
  - A value can be introduced with a name, and an intermediate result can be named
    after the fact; both names appear in explanation output and are addressable by the
    dependency queries (RQ-014). Tests in
    [#19](https://github.com/urario/Yurai/issues/19).
  - The display convention for anonymous inputs follows the
    [#18](https://github.com/urario/Yurai/issues/18) Q2 decision.
- **Source**: §5.2, §7.1 (named input / named operation);
  [#19](https://github.com/urario/Yurai/issues/19).

### RQ-008 — Arithmetic composition with ordinary operators

- **Priority**: P0
- **What**: Traced values compose with the language's ordinary arithmetic syntax — the
  four arithmetic operators `+ - * /` — plus minimum and maximum selection. The
  results match native arithmetic exactly (RQ-001), and for Min/Max the evidence
  records which operand was selected.
- **Why / user value**: The proposal's identity sentence rests on "ordinary C#
  arithmetic syntax as-is": domain calculations already written as arithmetic should
  become explainable without being rewritten into a different style. That is what
  keeps adoption cost near zero.
- **Acceptance criteria**:
  - All four operators work between traced values and match plain `decimal` results,
    including exception cases ([#19](https://github.com/urario/Yurai/issues/19),
    property suite [#26](https://github.com/urario/Yurai/issues/26)).
  - Min/Max match `Math.Min`/`Math.Max` on `decimal` and the evidence shows the
    selected side ([#20](https://github.com/urario/Yurai/issues/20)).
- **Constraints and notes**: Min/Max assume a total order on the underlying value type.
  `decimal` has one; floating-point NaN and domain value objects may not. Recorded as
  a type assumption under RQ-028.
- **Source**: §5.2, §7.1; [#19](https://github.com/urario/Yurai/issues/19),
  [#20](https://github.com/urario/Yurai/issues/20).

### RQ-009 — Mixed operations with plain values

- **Priority**: P0
- **What**: Traced values combine directly with plain, untraced values of the
  underlying type on either side of an operation (`t * 1.1m`). The plain operand
  enters the evidence as an anonymous input; the result is identical to the fully
  native computation.
- **Why / user value**: Real calculations are full of literals — tax rates, factors,
  constants. Requiring every literal to be wrapped first would make traced code look
  unlike the original arithmetic and would bury the named inputs that matter under
  ceremony.
- **Acceptance criteria**:
  - Both operand orders work for every operator, with the literal recorded as an
    anonymous input ([#20](https://github.com/urario/Yurai/issues/20)); display name
    convention per [#18](https://github.com/urario/Yurai/issues/18) Q2.
  - Property suite covers mixed expressions ([#26](https://github.com/urario/Yurai/issues/26)).
- **Constraints and notes**: What "a plain value of the underlying type" means at the
  call site depends on the language's literal and conversion rules for that type;
  `decimal` literals are exact, floating-point literals are not. Recorded as a type
  assumption under RQ-028.
- **Source**: §5.3 (mixed operations allowed; literal side recorded as an anonymous
  input node), §7.1; [#20](https://github.com/urario/Yurai/issues/20).

### RQ-010 — Rounding is an explicit, recorded operation

- **Priority**: P0
- **What**: Rounding is a first-class operation that takes the number of digits and a
  reason, and the evidence records all of it — the rounding, its parameters, and the
  stated reason. The numeric result is identical to the native rounding call with the
  same parameters (RQ-001).
- **Why / user value**: Rounding is where money calculations get questioned — it is
  the one step where a value visibly stops being the "true" arithmetic result. A
  recorded reason ("regulatory rounding to cents") turns the most contested step of a
  calculation into the best documented one.
- **Acceptance criteria**:
  - Rounding results match `decimal.Round`/`Math.Round` with the same parameters,
    including midpoint and negative-value boundaries
    ([#21](https://github.com/urario/Yurai/issues/21)).
  - The reason is held in the evidence and appears in explanation output and export.
  - The default midpoint-rounding treatment is fixed by the design
    ([#17](https://github.com/urario/Yurai/issues/17)), not silently assumed.
- **Constraints and notes**: Digits-based rounding is real-number semantics. Integer
  types do not round; domain value objects may round by their own rules. If types
  beyond `decimal` are adopted (RQ-028), the shape of this operation is an open
  question to revisit — not one to answer now.
- **Source**: §5.2 (rounding policy kept as evidence), §7.1;
  [#21](https://github.com/urario/Yurai/issues/21).

### RQ-011 — Immutable evidence with structure sharing

- **Priority**: P0
- **What**: Derivation evidence is immutable: once a value is computed, nothing can
  alter its recorded derivation, and values can be shared freely — including across
  threads — without defensive copying. Reusing an intermediate result in several
  places records the shared sub-derivation once; evidence size grows linearly with the
  number of operations performed, not with the number of paths through the graph.
- **Why / user value**: Evidence that can change after the fact is not evidence.
  Immutability is what lets a developer hold on to a value and trust its explanation
  later; linear growth is what makes tracing affordable enough to leave on in
  production code paths.
- **Acceptance criteria**:
  - Immutability is verified by test; no public operation mutates existing evidence
    ([#19](https://github.com/urario/Yurai/issues/19)).
  - A reused intermediate result does not duplicate its sub-derivation: n operations
    produce O(n) recorded nodes, verified structurally.
  - The human-readable output expands a shared derivation once and refers back to it
    afterwards (RQ-012).
- **Constraints and notes**: Phrased as observable behavior. The node taxonomy and
  representation are design decisions
  ([#17](https://github.com/urario/Yurai/issues/17)).
- **Source**: §7.1 (immutable DAG, structure sharing), §10;
  [#19](https://github.com/urario/Yurai/issues/19).

### RQ-012 — Human-readable derivation output

- **Priority**: P0
- **What**: A traced value can render its complete derivation as human-readable text:
  the result, the named inputs and intermediate results with their values, recorded
  rounding reasons, and the branches taken. Shared sub-derivations appear in full once
  and as references afterwards.
- **Why / user value**: This output is the product's face — the thing a developer
  pastes into a code review, a support ticket, or a conversation with a domain expert
  to answer "why this value" without stepping through a debugger. RQ-002 sets the
  quality bar for it.
- **Acceptance criteria**:
  - Output covers every evidence element: names, values, rounding reasons (RQ-010),
    branch names (RQ-005); verified against the expected output of the two domain
    samples ([#23](https://github.com/urario/Yurai/issues/23),
    [#13](https://github.com/urario/Yurai/issues/13),
    [#14](https://github.com/urario/Yurai/issues/14)).
  - Shared-node reference display is verified by test.
- **Constraints and notes**: The proposal's §2.3 output sketch is illustrative, not a
  fixed format. Rendering numbers as text is culture- and type-dependent; the MVP may
  fix one culture and format, with configurability deferred to RQ-026. Recorded as a
  type assumption under RQ-028.
- **Source**: §2.3, §7.1, §10; [#23](https://github.com/urario/Yurai/issues/23).

### RQ-013 — Machine-readable derivation export

- **Priority**: P0
- **What**: A traced value can export its complete derivation as JSON: every evidence
  element — kind, name, value, references to what it was computed from, rounding
  reasons, branch names — in a structure a program can traverse. The export is
  *material* for an audit trail kept by the caller's systems; neither it nor the
  human-readable output is an audit trail by itself (§9.1).
- **Why / user value**: Explanations that leave the process become artifacts —
  attachable to a case record, diffable between runs, storable by the caller's audit
  tooling. JSON is the least-common-denominator exit that makes the evidence usable
  outside .NET.
- **Acceptance criteria**:
  - Every evidence element kind serializes; the structure round-trips (parseable and
    structurally faithful to the evidence), and string escaping is correct
    ([#24](https://github.com/urario/Yurai/issues/24)).
  - Implemented dependency-free (RQ-004).
  - Documentation states the §9.1 boundary: material for an audit trail, not one.
- **Constraints and notes**: How values are represented in JSON is type-sensitive:
  `decimal` must not lose precision to a binary float representation, and future types
  raise their own questions (`double` NaN/Infinity have no JSON literal). Schema
  stability and versioning are RQ-027 and the
  [#18](https://github.com/urario/Yurai/issues/18) Q5 decision.
- **Source**: §5.2, §7.1, §9.1; [#24](https://github.com/urario/Yurai/issues/24).

### RQ-014 — Programmatic dependency queries

- **Priority**: P0
- **What**: A traced value answers dependency questions in code, without parsing any
  output: whether the result depends on a given named input; the dependency path from
  a named input to the result; and the set of named inputs the result depends on.
- **Why / user value**: This is the "queryable" in queryable derivation evidence — it
  turns lineage from something a human reads into something code can assert on: tests
  that fail when a calculation silently stops using an input, and handlers that route
  a question about a value by what it depends on.
- **Acceptance criteria**:
  - The three queries work on shared structures: multiple paths, non-dependency, and
    anonymous inputs are all covered by test
    ([#25](https://github.com/urario/Yurai/issues/25)).
  - Query vocabulary complies with RQ-003; the path query's documentation states it
    returns a dependency path only.
- **Source**: §4, §5.2, §7.1; [#25](https://github.com/urario/Yurai/issues/25).

### RQ-015 — Public API surface of at most 20 members

- **Priority**: P0
- **What**: The public API surface of the shipped library stays at or below 20
  members, and public names are written out in full — no abbreviated aliases (§5.3).
- **Why / user value**: A surface a developer can hold in their head in one sitting is
  what makes the five-minute promise (RQ-002) honest, and small surfaces stay
  compatible: every member not added is a breaking change that can never happen.
- **Acceptance criteria**:
  - The count is taken and recorded at the gate
    ([#29](https://github.com/urario/Yurai/issues/29)); exceeding it means removing or
    deferring something, or a maintainer decision to raise the bound (reserved).
  - Every public API addition or change is a reserved decision
    ([AGENTS.md §2](../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides)).
- **Source**: §5.2, §5.3; [#17](https://github.com/urario/Yurai/issues/17),
  [#29](https://github.com/urario/Yurai/issues/29).

## Non-goals

Things Yurai deliberately does not do, registered as requirements phrased in the
negative — they keep the public surface small, and an unwritten non-goal gets
re-proposed every few months. Each carries a **scope** stating how permanent it is:

- **Permanent** — doing this would change what Yurai is (§4 positioning, §6.4
  identity). Reopening one is a redefinition of the library, not a feature request.
- **Standing** — no plans and no roadmap slot; a future proposal revision could
  revisit it without changing Yurai's identity.
- **MVP-bounded** — out of v1.0 deliberately, with the future explicitly held open.

All nine §7.2 non-goals hold for v1.0 regardless of scope label; the label says what
it would take to ever change the answer.

### RQ-016 — Non-goal: symbolic algebra

- **Priority**: P0 · **Scope**: Permanent
- **What**: Yurai does not do symbolic mathematics — no expression rewriting,
  simplification, or solving. It records how a concrete value was actually computed,
  eagerly, with real values at every step.
- **Why**: The identity (§6.4) is evidence attached to eager, ordinary arithmetic on
  domain values. A symbolic engine is a different product with a different user.
- **Acceptance criteria**: No public API accepts or produces symbolic expressions;
  no documentation promises simplification or solving.
- **Source**: §7.2.

### RQ-017 — Non-goal: sensitivity analysis and automatic differentiation

- **Priority**: P0 · **Scope**: Permanent
- **What**: Yurai does not compute how much a result would change if an input changed —
  no derivatives, no what-if deltas, no automatic differentiation.
- **Why**: Yurai answers dependency and derivation questions only (RQ-003). A
  dependency path presented next to a sensitivity number invites reading one as the
  other; the boundary is what keeps the evidence honest. This exclusion is stated to
  users up front (README §4 table, and the feature-request template requires
  acknowledging it).
- **Acceptance criteria**: No API, output, or documentation offers or implies
  sensitivity information; feature requests for it are declined as out of scope.
- **Source**: §4, §7.2; [#25](https://github.com/urario/Yurai/issues/25).

### RQ-018 — Non-goal: attribution

- **Priority**: P0 · **Scope**: Permanent
- **What**: Yurai does not apportion a result among its inputs — no statement of how
  much of a total is owed to which input, under any name.
- **Why**: Same boundary as RQ-017: apportionment methods are analytically loaded
  choices (there are many, with different answers), and a lineage library that picks
  one silently would be wrong more often than useful. Yurai shows the path, not a
  share.
- **Acceptance criteria**: As RQ-017, for attribution; additionally the vocabulary
  rule of RQ-003 (no such wording anywhere in API or documentation).
- **Source**: §4, §7.2; [#25](https://github.com/urario/Yurai/issues/25).

### RQ-019 — Non-goal: taint tracking and general-purpose provenance

- **Priority**: P0 · **Scope**: Permanent
- **What**: Yurai does not track data flow through an entire program, label values for
  security analysis, or serve as a general provenance framework for arbitrary data.
  Its evidence covers exactly the computation the developer chose to trace.
- **Why**: Isolating one calculation and explaining it completely is what keeps the
  library lightweight (RQ-015, RQ-004) and the type non-viral (§12: values cross the
  boundary as plain values). Whole-program guarantees demand instrumentation Yurai
  refuses on purpose.
- **Acceptance criteria**: Documentation scopes the evidence to the traced
  computation; no API implies program-wide coverage or security guarantees.
- **Source**: §7.2, §12.

### RQ-020 — Non-goal: audit-platform features

- **Priority**: P0 · **Scope**: Permanent
- **What**: Yurai does not store, sign, timestamp, retain, or manage evidence. It
  produces the material (RQ-012, RQ-013); everything after the return value —
  persistence, integrity, retention, access — belongs to the caller's systems.
- **Why**: §9.1 draws this line to keep the claim honest: an explanation is not an
  audit trail, and pretending otherwise would sell compliance the library cannot
  deliver. Platform features are also where dependencies come in (RQ-004).
- **Acceptance criteria**: No storage, signing, or retention API exists; documentation
  states the §9.1 boundary wherever export is described (see RQ-013).
- **Source**: §7.2, §9.1; [#24](https://github.com/urario/Yurai/issues/24).

### RQ-021 — Non-goal: LaTeX or HTML rendering

- **Priority**: P0 · **Scope**: Standing
- **What**: Yurai renders derivations as plain text (RQ-012) and JSON (RQ-013) only —
  no LaTeX, HTML, or other rich formats.
- **Why**: Two exits are enough for the MVP's user value, and every renderer is
  surface (RQ-015) plus formatting opinions to maintain. The JSON export exists
  precisely so that anyone who wants rich rendering can build it outside the library.
- **Acceptance criteria**: No rendering API beyond text and JSON; documentation points
  rich-format needs at the JSON export.
- **Source**: §7.2.

### RQ-022 — Non-goal: evaluating expressions supplied as strings

- **Priority**: P0 · **Scope**: Permanent
- **What**: Yurai does not parse or evaluate expressions given as strings. Computations
  are written as compiled code in the host language.
- **Why**: The identity (§6.4) is ordinary compiled arithmetic syntax — type-checked
  by the compiler, refactorable by the IDE, reviewed like any other code. A string
  evaluator (the NCalc family) trades all of that away and is already well served
  elsewhere (RQ-006).
- **Acceptance criteria**: No public API accepts an expression string for evaluation.
- **Source**: §7.2.

### RQ-023 — MVP non-goal: value types other than decimal

- **Priority**: P0 · **Scope**: MVP-bounded — see RQ-028
- **What**: In v1.0, the underlying value type is `decimal` and no other: no integer,
  floating-point, or user-defined value types, and no generic abstraction over value
  types. **This bounds the MVP; it does not define the library.** Yurai is a
  computation-lineage library whose first shipped value type is `decimal` — not a
  `decimal` library.
- **Why**: The MVP's user is doing money and rate arithmetic, where `decimal` is the
  correct and idiomatic .NET type — one type covers the entire first audience.
  Every additional type multiplies the fidelity obligation (RQ-001 must be restated
  and re-verified per type) and pulls in design questions (generic math versus
  `netstandard2.0`, [#18](https://github.com/urario/Yurai/issues/18) Q4) that would
  delay shipping the value that is already provable.
- **Acceptance criteria**:
  - The v1.0 public surface exposes `decimal` as the only underlying value type.
  - Documentation describes the `decimal` bound as the MVP's scope, not as Yurai's
    definition — the distinction this section states.
  - The extension question is kept open per RQ-028 and decided, when its time comes,
    through [#18](https://github.com/urario/Yurai/issues/18) Q4.
- **Source**: §7.2, §8 (P2), §13 Q4; [#18](https://github.com/urario/Yurai/issues/18).

## Documentation and positioning constraints

How the project is allowed to describe itself, from §9. These bind all prose — README,
published documentation, issues, and pull requests, in English and Japanese alike —
and are checked at the gate ([#29](https://github.com/urario/Yurai/issues/29)).

### RQ-024 — Banned phrases and replacement vocabulary

- **Priority**: P0
- **What**: The following are never written, as phrases, anywhere in the project's
  prose (this list is the canonical one; it exists to be checked against, which is why
  the phrases appear here at all):
  - "show your work"
  - "audit-ready"
  - "provenance semiring"
  - "first" / "world's first" / "novel approach" — as novelty claims; see RQ-025. The
    word "first" in ordinary uses (`test-first`, "the first example") is fine — the
    rule is about the claim, not the word.

  The replacement vocabulary is: *queryable derivation evidence*, *explainable*,
  *for .NET*, *computation lineage*.
- **Why / user value**: Each banned phrase over-promises ("audit-ready" sells
  compliance RQ-020 refuses), borrows academic weight the library does not claim
  ("provenance semiring"), or is a marketing register the project has chosen not to
  speak in. The replacements say what Yurai actually does; consistent vocabulary is
  what lets users repeat the claim accurately.
- **Acceptance criteria**:
  - No occurrence in README, documentation, XML doc comments, or repository prose;
    scanned in review and at the gate ([#29](https://github.com/urario/Yurai/issues/29)).
  - The operative statement of this rule for day-to-day work is
    [CLAUDE.md](../../CLAUDE.md#how-the-project-describes-itself), which follows this
    registry.
- **Source**: §9.2; [#15](https://github.com/urario/Yurai/issues/15), execution plan.

### RQ-025 — Novelty claim ceiling

- **Priority**: P0
- **What**: The project makes exactly one novelty claim, fixed by §6.4 of the
  proposal, and never a stronger one. The sentence, in the proposal's original
  Japanese:

  > 本番アプリケーションの通常のC#算術構文をそのまま用いて、eager評価済みのドメイン値と
  > 問い合わせ可能な導出証拠(DAG)を一体で提供する、依存0のNuGetライブラリは存在しない

  English rendering: *No zero-dependency NuGet library exists that, using ordinary C#
  arithmetic syntax as-is in a production application, provides eagerly evaluated
  domain values together with queryable derivation evidence (a DAG) as one thing.*

  The claim is deliberately framed as an observed absence, backed by the Related Work
  comparison (RQ-006) — never as being "the first", and never implied to be.
- **Why / user value**: Credibility compounds: one precise, checkable claim survives
  scrutiny; inflation anywhere costs trust everywhere, including in the explanations
  the library produces.
- **Acceptance criteria**:
  - Every novelty statement in README or documentation is this sentence or a direct
    translation of it; nothing stronger appears
    ([#15](https://github.com/urario/Yurai/issues/15), gate
    [#29](https://github.com/urario/Yurai/issues/29)).
  - If the observation is ever falsified (such a library exists), the sentence is
    withdrawn or corrected — the ceiling can move down, never up.
- **Source**: §6.4, §9.2; [#15](https://github.com/urario/Yurai/issues/15).

## Post-MVP requirements

Wanted, registered now so design work keeps them reachable; none blocks v1.0.

### RQ-026 — Configurable explanation output

- **Priority**: P1
- **What**: The human-readable output (RQ-012) becomes configurable: depth limit for
  large derivations, culture for number formatting, and output format options.
- **Why / user value**: Real derivations outgrow a terminal, and real users read
  numbers in more than one culture. The MVP fixes one rendering; this makes the same
  evidence readable in more contexts without the caller re-implementing rendering.
- **Acceptance criteria**: Deferred to the issue that implements it. Until then, MVP
  design keeps the option space open — the options type named in §5.2 is the
  proposal's sketch of that seam ([#23](https://github.com/urario/Yurai/issues/23)).
- **Source**: §8 (P1); [#23](https://github.com/urario/Yurai/issues/23).

### RQ-027 — JSON export schema documented and versioned

- **Priority**: P1
- **What**: The JSON export (RQ-013) has a documented schema carrying a version, so a
  consumer can build against it and detect change.
- **Why / user value**: RQ-013's value is programs outside the process consuming the
  evidence; an undocumented shape makes every consumer reverse-engineer it and breaks
  them silently when it changes.
- **Acceptance criteria**:
  - A schema document (English) is merged alongside the export implementation
    ([#24](https://github.com/urario/Yurai/issues/24)).
  - Whether the schema is a stable API — and versioned from the first release — is
    the [#18](https://github.com/urario/Yurai/issues/18) Q5 decision; this requirement
    holds whichever way that decision goes.
- **Source**: §8 (P1); [#24](https://github.com/urario/Yurai/issues/24),
  [#18](https://github.com/urario/Yurai/issues/18).

### RQ-028 — Value-type extensibility stays open

- **Priority**: P2
- **What**: Yurai's model — named inputs, arithmetic composition, recorded decisions,
  immutable shared evidence, explanation, export, and queries — is not inherently
  specific to `decimal`, and extending it to other value types (integers, floating
  point, domain value objects) must remain *possible*. Until that extension is
  actually designed, no requirement, design, or public API decision may foreclose it
  where a type-neutral alternative costs nothing now.

  Explicitly **not** required now: generic math support, multi-targeting beyond
  `netstandard2.0`, or any concrete extension architecture. Those are the
  [#18](https://github.com/urario/Yurai/issues/18) Q4 decision, reserved for when a
  second value type is actually pursued ("the minimum judgement that does not close
  the door" — §8 P2).
- **Why / user value**: Domain calculations are not only money: quantities, indexes,
  and typed domain values ask the same "why this value" question. Locking the
  library's identity to `decimal` at the requirements level would make the eventual
  answer a rewrite instead of an extension — the cheapest moment to keep a door open
  is before anything is built into it.
- **Acceptance criteria**:
  - Requirements and design documents phrase Yurai's concepts against "the underlying
    value type", naming `decimal` only where the MVP bound (RQ-023) or a genuinely
    type-specific behavior (RQ-001's fidelity definition) is meant. Reviewed on this
    registry and on the [#17](https://github.com/urario/Yurai/issues/17) design.
  - The known type-sensitive assumptions stay recorded where they arise, so a future
    extension inherits its worklist: fidelity semantics per type (RQ-001), total order
    for Min/Max (RQ-008), literal conversion at mixed-operation call sites (RQ-009),
    the shape of rounding (RQ-010), value formatting in output (RQ-012), and JSON
    value representation (RQ-013).
  - Public API naming does not encode `decimal` where the concept is type-neutral,
    unless the [#17](https://github.com/urario/Yurai/issues/17)/[#18](https://github.com/urario/Yurai/issues/18)
    design records a reason.
- **Constraints and notes**: This requirement constrains *wording and decision
  hygiene* now; it promises no implementation and no timeline. It is the positive
  counterpart of RQ-023: v1.0 ships `decimal` only, and the library stays defined by
  its model rather than by that first type.
- **Source**: §8 (P2), §13 Q4; [#18](https://github.com/urario/Yurai/issues/18).

## The proposal

The project proposal (Draft 1.0) that this specification is derived from is not yet
stored in the repository; [#12](https://github.com/urario/Yurai/issues/12) intends it
to live in this directory so the source of a requirement stays readable next to the
requirement itself, and it will be added in a follow-up once the maintainer supplies
the document. Until then, the proposal passages this registry relies on are the ones
quoted verbatim in the issues cited per requirement. Either way the rule stands: the
proposal is the origin of the specification, not a parallel copy of it — where the two
differ, this registry is what binds.
