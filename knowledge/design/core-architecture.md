---
type: Design
title: Yurai core architecture
description: Architecture drivers, evidence model, runtime boundaries, and implementation seams for Yurai's decimal MVP.
tags: [architecture, evidence, computation-lineage, decimal]
status: draft
requirements: [RQ-001, RQ-002, RQ-003, RQ-004, RQ-005, RQ-007, RQ-008, RQ-009, RQ-010, RQ-011, RQ-012, RQ-013, RQ-014, RQ-015, RQ-016, RQ-017, RQ-018, RQ-019, RQ-020, RQ-021, RQ-022, RQ-023, RQ-024, RQ-025, RQ-026, RQ-027, RQ-028, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-09T15:30:00+09:00 }
sources:
  - id: issue-17
    resource: https://github.com/urario/Yurai/issues/17
    title: "Issue #17: core architecture and ADRs"
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions Q2-Q6"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# Yurai core architecture

This document is the implementation-facing architecture for Yurai's decimal MVP. It
derives the core shape from the requirements, records the boundaries that all slices
share, and identifies decisions that still require maintainer approval before the
affected slice can be implemented. It does not make the undecided public API choices
owned by [issue #18](https://github.com/urario/Yurai/issues/18).

## 1. Architecture drivers

Yurai attaches queryable derivation evidence to eagerly evaluated domain values. The
value remains the result of ordinary C# arithmetic; Yurai adds evidence and never
substitutes its own numeric interpretation.

| Driver | Architectural response |
| --- | --- |
| Value fidelity (RQ-001) | Perform the native `decimal` operation before constructing evidence; propagate native failures without producing a result. |
| Explainability and queryability (RQ-002, RQ-003, RQ-012, RQ-014) | Preserve named inputs, named results, operation metadata, selected branches, and shared dependency structure in a traversable model. |
| Deployment reach (RQ-004) | Keep `src/Yurai` on `netstandard2.0`, BCL-only, with no runtime package dependency. |
| Immutable shared evidence (RQ-011) | Use an immutable DAG in which a new operation creates a parent and reuses existing children. |
| Familiar C# usage (RQ-008, RQ-009) | Keep arithmetic eager and operator-based inside an explicitly traced region; unwrap to a plain value at its boundary. |
| Small, stable API (RQ-015) | Keep all node and traversal types internal and count every public type and declared member against one fixed budget. |
| Decimal MVP without redefining the product (RQ-023, RQ-028, RQ-029) | Ship only `decimal`, use type-neutral concepts and private names where they cost nothing, and avoid a generic-math framework before another value type is approved. |
| Large and deep graphs | Use structural sharing and iterative traversals; measure, rather than guess, allocation and throughput in issue #27. |

The permanent non-goals are architectural boundaries: no symbolic evaluation, automatic
differentiation, sensitivity calculation, attribution, whole-program data-flow
tracking, evidence storage, rich rendering, or string-expression evaluation
(RQ-016 through RQ-022). The computation a developer explicitly places inside a traced
region is the complete scope of Yurai's evidence.

## 2. Vocabulary and semantic boundary

- A **value** is the already evaluated domain result. In the MVP it is a `decimal`.
- **Derivation evidence** records the operations and named decisions that produced a
  value. It is not an expression awaiting evaluation.
- A **dependency** exists when an evidence path from the result reaches an input.
- A **derivation** is the recorded sequence of operations and decisions along those
  paths.
- A **trace** is a dependency path. It is not an execution log and carries no
  sensitivity or apportionment meaning (RQ-003).
- A **traced region** begins when a plain value is introduced through the public entry
  point and ends when the caller reads the plain value. Evidence does not follow that
  plain value through code outside the region (RQ-019).

The value and its evidence are related by one invariant: the root evidence node stores
the exact value exposed by the carrier. There is no separately mutable value field that
can diverge from the root.

## 3. Architecture viewpoints

### 3.1 Context and responsibility view

```mermaid
flowchart LR
    Caller[Caller code] -->|plain decimal + domain name| Facade[Public entry points]
    Facade --> Carrier[Traced value carrier]
    Carrier -->|ordinary operators and named operations| Native[Native decimal evaluation]
    Native -->|success only| Graph[Immutable evidence DAG]
    Graph --> Query[Dependency queries]
    Graph --> Explain[Text representation]
    Graph --> Json[JSON representation]
    Carrier -->|plain decimal| Caller
```

| Component | Responsibility | Explicitly does not own |
| --- | --- | --- |
| Public entry points | Introduce plain values and express operations in ordinary C# syntax. | Evidence storage, global configuration, or expression parsing. |
| Traced value carrier | Hold one root reference and expose the root's evaluated value. | Mutable state, caches, or public node access. |
| Native operation boundary | Invoke the corresponding `decimal` operation with the same operands and parameters. | Alternate rounding, overflow, ordering, or exception rules. |
| Evidence nodes | Preserve evaluated values, operation metadata, and child references. | Text/JSON formatting and dependency-query result shapes. |
| Traversal kernel | Walk a DAG iteratively with reference-identity tracking and deterministic child order. | User-facing formatting policy. |
| Query, text, and JSON adapters | Interpret one immutable model for a specific output. | Mutation or re-evaluation of the calculation. |

### 3.2 Information view

The internal model is an abstract `EvidenceNode` with an evaluated `decimal` value and
a closed family of sealed node kinds. Names are conceptual here; private implementation
names may differ without changing the contract.

| Node kind | Required data | Children |
| --- | --- | --- |
| `Input` | Evaluated value and optional developer-supplied name. | None. |
| `BinaryOperation` | Evaluated value and operation kind. Min/Max also preserve which operand was selected so equal operands remain distinguishable. | Left and right, in source order. |
| `Round` | Evaluated value, digits, `MidpointRounding.ToEven`, and reason. | The unrounded value. |
| `Branch` | Evaluated value, decision name, condition outcome, selected branch label. | At least the selected result; treatment of the unselected alternative is Q3. |
| `Named` | Evaluated value and developer-supplied result name. | The value being named. |

Fixed child fields are preferred over an array per node. They express arity, prevent an
extra allocation for common operations, and make malformed graphs harder to construct.
Constructors reject null children internally. A parent can only receive already
constructed children, all fields are read-only, and no rewiring operation exists;
cycles are therefore impossible through supported construction.

Naming an intermediate creates a `Named` parent rather than editing the child. Reusing
the same intermediate in two expressions gives both parents the same child reference.
For *n* successful input/naming/operation calls, the number of nodes is at most linear
in *n* even when the number of root-to-input paths is much larger (RQ-011).

### 3.3 Computation and data-flow view

For a binary operation:

1. Read the already evaluated values from the two roots.
2. Execute the corresponding native `decimal` operator in the same checked context the
   public contract specifies.
3. If native evaluation throws, propagate that exception type and construct no result
   node. Existing operands remain valid and unchanged.
4. If it succeeds, create one `BinaryOperation` node containing the exact result and
   references to the existing roots.
5. Return a new carrier holding only that parent reference.

Mixed operations first introduce the plain operand as an anonymous `Input` and then use
the same flow. Its display convention is Q2. `Round(digits, reason)` invokes
`decimal.Round(value, digits)`, whose midpoint mode is `ToEven`, and records that mode
explicitly. Parameter validation for names/reasons must not accidentally replace a
native arithmetic failure; the exact validation order is an open public-contract
question below.

The same evidence instance is safe to read concurrently because every reachable object
is immutable. Thread safety is a property of the object graph, not a lock protocol.

### 3.4 Lifecycle and runtime view

- Node identity in memory is reference identity. There is no process-wide counter,
  registry, interning table, or stable identifier on a node.
- A traversal assigns local deterministic identifiers on first encounter, in root-first
  and left-to-right child order. Explain and JSON may use those identifiers without
  changing the model or claiming identity across calls.
- A carrier keeps its entire reachable graph alive. When no carrier or traversal refers
  to a root, normal garbage collection reclaims the root and any children not shared by
  another live root.
- Traversals use an explicit stack and a reference-identity visited set. They do not use
  call-stack recursion, so a graph beyond 10,000 nodes does not fail merely because it
  is deep.
- Construction performs one node allocation per recorded operation. The carrier is a
  `readonly struct`, avoiding a carrier allocation in ordinary generic-free usage;
  boxing still occurs if callers place it in `object` or a non-generic interface.
- Each full traversal costs `O(V + E)` time and `O(V)` temporary identity state. No
  result is cached in the MVP: repeated calls recompute representations and queries
  from immutable evidence rather than retaining large strings or synchronized caches.
- Serialization writes directly to a `StringBuilder` or equivalent BCL buffer. It does
  not build a second public object model and does not add a JSON package dependency.

Allocation size and throughput are not guessed into a gate here. Issue #27 measures
native-versus-traced operations, graph construction beyond 10,000 nodes, and each
traversal before a performance threshold is proposed.

### 3.5 Public API and internal-model boundary

This design fixes behavior boundaries, not the names and signatures still owned by
issue #18. In particular, the proposal's `Traced<T>` sketch conflicts with RQ-023's
current rule that the v1 public surface exposes no generic abstraction over value types.
Until Q6 is decided, this document uses **traced value carrier** rather than treating a
type name as settled.

The public surface may expose entry, value extraction, arithmetic composition, naming,
rounding, branch selection, text/JSON output, and dependency queries. It must not expose
nodes, node kinds, visitors, graph identifiers, or traversal collections merely to make
tests convenient. Internal graph inspection for S1 tests is provided through a friend
test assembly, not a production API.

The RQ-015 count is fixed as follows:

- count each public type once;
- count each declared public constructor, method overload, operator overload, property,
  event, and field once;
- count a property rather than its individual accessors;
- exclude inherited and compiler-generated members.

Logical grouping does not reduce the count: eight operator overloads are eight members.
The architecture review maintains a provisional count, and the Phase 2 gate repeats it
against the compiled assembly.

The illustrative MVP surface only fits the limit if mixed arithmetic reuses the four
carrier-to-carrier operators through one approved implicit conversion from `decimal` to
an anonymous traced input:

| Public surface group | Count |
| --- | ---: |
| Static facade and traced value carrier types | 2 |
| Named and anonymous entry methods | 2 |
| Min and Max methods | 2 |
| Conditional method | 1 |
| Value, naming, rounding, text, JSON, and three query members | 8 |
| Carrier-to-carrier arithmetic operators | 4 |
| Plain-decimal-to-carrier implicit conversion | 1 |
| **Provisional total** | **20** |

Implementing explicit left and right mixed overloads for all four operators replaces
the one conversion with eight members and raises this total to 27. The table is a budget
analysis, not an approved API: Q6, Q9, and Q12 still decide the actual types, signatures,
and projections. Any additional public options or result type requires removing or
deferring another member, or a reserved decision to revise RQ-015.

### 3.6 Representation separation

The traversal kernel exposes only internal node-kind and child access to internal
consumers. Each consumer owns its own policy:

- `Explain` owns indentation, number formatting, labels, and shared-node reference
  presentation.
- `ToJson` owns schema fields, escaping, decimal encoding, and document-local IDs.
- `DependsOn`, `Trace`, and `Inputs` own name matching, path construction, ordering, and
  result projection.

No consumer parses another consumer's output. JSON does not parse Explain text, queries
do not inspect rendered labels, and renderers do not mutate or annotate nodes. This
keeps a future output format or schema revision from becoming a graph-model migration.

### 3.7 Value-type extension policy

The MVP implementation remains concrete where semantics genuinely are concrete:
`decimal` evaluation, total ordering for Min/Max, digits-based rounding, formatting,
and JSON precision. The internal node family uses type-neutral names but stores
`decimal` directly. Making every node and traversal generic now would thread a type
parameter through the design while still requiring decimal-only policies, without
shipping a second type; that is speculative generalization.

The inexpensive future options preserved now are:

- no public node or representation type contains `Decimal` in its name;
- native value operations are isolated from evidence construction;
- format and serialization policy belong to adapters rather than nodes;
- no global node service assumes one value type;
- changing the private storage to `EvidenceNode<TValue>` later is an internal migration,
  while any public generic API remains a separately approved decision.

A second value type starts by restating fidelity, ordering, literal, rounding, text, and
JSON semantics for that type (RQ-029). It does not inherit decimal behavior by analogy.

## 4. Decision analysis

### 4.1 Evidence topology

- **Options:** immutable DAG with structural sharing; copied tree per result; append-only
  event list.
- **Evaluation axes:** derivation fidelity, sharing, query cost, memory growth,
  simplicity.
- **Recommendation:** immutable DAG with structural sharing.
- **Reason:** it preserves the actual dependency topology, makes reuse observable, and
  bounds node growth by recorded operations. A tree duplicates reused derivations; an
  event list needs a second relationship model before it can answer path queries.

### 4.2 Node representation

- **Options:** sealed subclasses under an internal base; one tagged node with nullable
  fields; public graph interfaces.
- **Evaluation axes:** invalid states, allocation, extensibility, public surface.
- **Recommendation:** sealed internal subclasses with fixed child fields.
- **Reason:** each kind carries only valid data and known arity without publishing an
  extension contract or allocating a child array for every operation.

### 4.3 Evaluation timing

- **Options:** native eager evaluation followed by evidence creation; build an
  expression then evaluate it; evaluate twice and compare.
- **Evaluation axes:** RQ-001 fidelity, exception behavior, complexity, cost.
- **Recommendation:** native eager evaluation once, then evidence creation on success.
- **Reason:** it adds evidence to the language operation that actually ran and cannot
  reinterpret or replay the calculation differently.

### 4.4 Carrier state

- **Options:** root reference only; duplicate value plus root; mutable builder handle.
- **Evaluation axes:** divergence risk, size, allocation, thread safety.
- **Recommendation:** one root reference in a `readonly struct`.
- **Reason:** the root is the single source of value truth. Duplication creates an
  invariant with no user value, while a mutable builder breaks safe sharing.

### 4.5 Identity and traversal

- **Options:** process-global IDs; persistent IDs stored in nodes; reference identity
  plus traversal-local IDs. Traversal may be recursive or iterative.
- **Evaluation axes:** determinism, concurrency, lifetime coupling, deep-graph safety.
- **Recommendation:** reference identity, deterministic traversal-local IDs, and an
  iterative walk.
- **Reason:** this avoids global synchronization and identity lifetime promises while
  still representing sharing deterministically and safely beyond 10,000 nodes.

### 4.6 Caching

- **Options:** cache text/query results on nodes; external weak cache; no MVP cache.
- **Evaluation axes:** repeated-call speed, retained memory, synchronization,
  complexity.
- **Recommendation:** no cache until issue #27 demonstrates a material repeated-call
  cost.
- **Reason:** immutable input makes recomputation predictable; caching would add memory
  retention and thread-safe publication before a measured need exists.

### 4.7 Type abstraction

- **Options:** decimal-concrete internals with neutral boundaries; generic evidence
  storage; generic-math operation framework and multi-targeting.
- **Evaluation axes:** MVP simplicity, RQ-023 compliance, RQ-029 option preservation,
  target compatibility.
- **Recommendation:** decimal-concrete internals with neutral names and isolated
  policies.
- **Reason:** it avoids building unused abstractions while keeping the private migration
  path inexpensive. Generic math would change targets and belongs to Q4.

## 5. Open questions and ADR candidates

Items Q2 through Q6 retain the numbering from issue #18. Q7 onward are gaps found by
this architecture review; they require a durable decision before the named slice starts.

| Question | Options and evaluation | Recommendation | Blocks |
| --- | --- | --- | --- |
| Q2 anonymous-input display | Raw value, fixed anonymous marker, or generated ordinal; compare readability, determinism, and name collisions. | Display the invariant-formatted value without inventing a domain name. | S2, S5 |
| Q3 conditional API | Eager values or lazy delegates; compare short-circuit equivalence, allocation, and readability. | Lazy alternatives, recording only the selected derivation and condition outcome. | S4 |
| Q4 future type strategy | Remain `netstandard2.0` only, multi-target later, or multi-target now; compare reach and unused complexity. | Do not multi-target until a second value type is approved. | Project evolution, not decimal MVP |
| Q5 JSON stability | Stable and versioned from v1, documented but unstable, or internal-only format; compare consumer trust and evolution cost. | Publish a versioned stable schema because external consumption is the purpose of JSON export. | S6 |
| Q6 public carrier name | Generic `Traced<T>`, non-generic `Traced`, or decimal-named type; compare RQ-023, future compatibility, and usability. | Use a non-generic type-neutral name for v1; reserve genericity for an approved multi-type API. | S1 and all public API |
| Q7 default carrier value | Throw a defined exception, represent zero input, or prevent with a reference type; compare struct ergonomics and silent corruption. | A default carrier is invalid and every public use throws `InvalidOperationException` with a stable message. | S1 |
| Q8 name and reason validation | Allow null/empty, normalize, or reject; compare explanation integrity and compatibility. | Reject null names/reasons, reject empty names, and allow an empty reason only if Human explicitly accepts its loss of evidence. | S1, S3 |
| Q9 duplicate names and paths | Treat names as unique, match all, or match first; compare composability, ambiguity, and query shape. | Permit duplicate names and make queries operate on all matches; define deterministic path ordering. | S7 |
| Q10 text culture and format | Current culture, invariant culture, or configurable options; compare determinism and locality. | Use invariant culture for MVP and defer options to RQ-026. | S5 |
| Q11 JSON decimal encoding | JSON number, invariant string, or both; compare exactness, interoperability, and schema size. | Encode the invariant decimal text as a JSON string unless Q5 adopts a schema with an exact numeric contract. | S6 |
| Q12 mixed-operation surface | Eight explicit mixed overloads, one implicit anonymous-input conversion, or a revised API budget; compare overload resolution, discoverability, and RQ-015. | Use one implicit `decimal` conversion and four carrier-to-carrier operators; test ambiguous overload contexts explicitly. | S1, S2 |

Q2-Q6 should be decided by the maintainer in issue #18. Q7-Q12 may be added to that
issue or split into one-decision ADRs under the knowledge policy. Until then the
recommendations above are proposals, not implementation authority.

## 6. Implementation slices and test seams

| Slice | Input contract | Output contract | Test seam and required scenarios |
| --- | --- | --- | --- |
| S1 core | Plain decimal with optional name; two initialized carriers for arithmetic. | New immutable root for input, naming, or native `+ - * /`; plain value equals root value. | Friend-assembly graph inspection; representative value/exception parity, default carrier, immutability, sharing, and node-count tests. RQ-001, RQ-007, RQ-008, RQ-011. |
| S2 mixed and selection | One carrier and one plain decimal in either order, or two values for Min/Max. | Anonymous input plus binary evidence; Min/Max records selected operand even when values compare equal. | Both operand orders, all operations, equal Min/Max operands, sharing, and property-test hooks. RQ-008, RQ-009. |
| S3 rounding | Initialized carrier, valid digits, and reason. | Native `decimal.Round(value, digits)` result plus explicit digits, `ToEven`, and reason evidence. | Midpoints, negative values, scale, digits bounds, native exception parity, reason retention. RQ-001, RQ-010. |
| S4 branching | Condition, alternatives, and decision name in the Q3-approved shape. | Value of the selected alternative and branch evidence containing the actual outcome. | Both outcomes, nested decisions, short-circuit behavior, selected-only evidence. RQ-001, RQ-005. |
| S5 text | Any initialized evidence root. | Deterministic complete text; each shared node expands once and later occurrences refer to it. | Domain-sample expectations, every node kind, escaping, shared references, and a chain beyond 10,000 nodes. RQ-002, RQ-012. |
| S6 JSON | Any initialized evidence root. | Valid dependency-free JSON preserving every evidence field and graph edge under the Q5 schema. | Parse-and-compare with a standard parser in tests, all kinds, escaping, decimal precision, deterministic IDs, and shared references. RQ-004, RQ-013, RQ-027. |
| S7 queries | Initialized root and, where needed, an input name. | Dependency boolean, dependency path result, or deterministic named-input collection under Q9. | No match, anonymous inputs, duplicate names, multiple paths, diamonds, cycles impossible, and deep graphs. RQ-003, RQ-014. |

Cross-cutting concurrency tests read the same root from multiple tasks and compare value,
query, text, and JSON results. Property tests compare arithmetic behavior with direct
`decimal`; every discovered counterexample becomes a named example test under the
testing strategy. Representation tests inspect internal state only where behavior is
not yet visible through a public output slice.

## 7. Quality-attribute evaluation

| Attribute | Design position and verification |
| --- | --- |
| Correctness | Native operation is the oracle; example and property tests compare values, scale, and exception types. |
| Explainability | Domain names and decision metadata survive independently of formatting; domain samples provide acceptance examples. |
| Simplicity and usability | One eager value carrier, ordinary operators, no public graph API, and an explicit traced boundary. |
| Maintainability and testability | Closed internal node kinds, representation adapters, friend-assembly structural seam, and RQ-tagged behavior tests. |
| Extensibility | Private topology and adapters can evolve; no public generic promise or generic-math framework in v1. |
| Performance and memory | One node per recorded action, fixed child fields, structural sharing, iterative `O(V+E)` walks; benchmark before optimization. |
| Thread safety | Immutable reachable state, no cache or global identity service; concurrent-read tests. |
| API stability | Internal nodes stay hidden; the conservative count exposes growth before release. |
| Portability | `netstandard2.0`, BCL-only, no serialization package, no newer runtime features in shipped code. |
| Failure observability | Native exceptions propagate; invalid carrier/metadata behavior is explicit once Q7/Q8 are decided; no partial result is returned. |

## 8. Traceability

| Requirements | Architecture coverage |
| --- | --- |
| RQ-001 | Native-first evaluation, no result node on native failure, parity tests. |
| RQ-002, RQ-007, RQ-012 | Named evidence, representation adapter, sample-based text tests. |
| RQ-003, RQ-014, RQ-017, RQ-018 | Dependency-only vocabulary and query boundary. |
| RQ-004 | BCL-only serialization and `netstandard2.0` portability boundary. |
| RQ-005 | Branch outcome and selected derivation evidence, pending Q3 API shape. |
| RQ-008, RQ-009, RQ-010 | Native operators, anonymous inputs, selection metadata, and recorded native rounding. |
| RQ-011 | Immutable DAG, reference sharing, linear node growth, iterative traversal. |
| RQ-013, RQ-020, RQ-021, RQ-027 | JSON adapter as caller-owned export material, no storage or rich renderer, pending Q5 schema decision. |
| RQ-015 | Hidden graph model and fixed public-surface counting rule. |
| RQ-016, RQ-019, RQ-022 | Eager concrete calculation inside an explicit traced region only. |
| RQ-023, RQ-028, RQ-029 | Decimal-only public MVP, neutral private boundaries, no speculative multi-type framework. |
| RQ-024, RQ-025 | The architecture uses the fixed vocabulary and makes no broader product claim. |
| RQ-026 | Formatting remains an adapter seam; configurability is deferred. |

RQ-006 concerns the README's related-work content and imposes no additional internal
structure; it remains a release-gate requirement outside this architecture.

## 9. Risks and readiness

- **Public contract blockers:** Q6 blocks the S1 public type; Q2, Q3, Q5, and Q7-Q12
  block their named slices. None may be inferred from the sketches in the samples.
- **API-budget risk:** the complete illustrative surface consumes all 20 slots only
  with the unapproved implicit-conversion strategy. Explicit mixed overloads exceed the
  limit, and a new public query result or options type requires rebalancing the surface.
- **Default-struct risk:** a `readonly struct` cannot prevent zero initialization. A
  silent zero value would make evidence untrustworthy, so S1 needs the Q7 decision and
  explicit tests.
- **Name ambiguity:** name-based queries cannot be specified safely until duplicate
  semantics and path ordering are fixed.
- **Large-output risk:** structural sharing bounds graph storage but text and JSON can
  still be large. Iterative traversal prevents stack failure; RQ-026 and issue #27
  address usability and measured cost.
- **Hand-written JSON risk:** escaping and exact decimal representation are correctness
  hotspots. Parse-and-compare tests and mutation review are mandatory seams.
- **Future-type risk:** decimal assumptions are isolated but not eliminated. A new type
  requires a new fidelity contract and a reserved architecture/API decision.

The internal architecture is ready for review. Implementation is not ready until the
reserved decisions affecting each slice are approved and recorded on GitHub.
