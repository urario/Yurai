---
type: Design
title: Yurai core architecture
description: Architecture drivers, evidence model, runtime boundaries, and implementation seams for Yurai's 0.1.x decimal surface and approved 0.2.0 decimal-plus-Int64 carrier.
tags: [architecture, evidence, computation-lineage, decimal, int64, generics]
status: stable
requirements: [RQ-001, RQ-002, RQ-003, RQ-004, RQ-005, RQ-007, RQ-008, RQ-009, RQ-010, RQ-011, RQ-012, RQ-013, RQ-014, RQ-015, RQ-016, RQ-017, RQ-018, RQ-019, RQ-020, RQ-021, RQ-022, RQ-023, RQ-024, RQ-025, RQ-026, RQ-027, RQ-028, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-11T18:35:01+09:00 }
sources:
  - id: issue-17
    resource: https://github.com/urario/Yurai/issues/17
    title: "Issue #17: core architecture and ADRs"
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: decisions for open questions Q2-Q6"
  - id: issue-67
    resource: https://github.com/urario/Yurai/issues/67
    title: "Issue #67: Traced<T> and value-type policy architecture"
  - id: requirements
    resource: ../requirements/registry.md
    title: Yurai requirements registry
---

# Yurai core architecture

This document is the implementation-facing architecture for Yurai's 0.1.x decimal
surface and the approved 0.2.0 transition to decimal plus Int64. It derives the core
shape from the requirements and records the boundaries shared by implementation slices.
ADR-0018 owns the 0.2.0 public carrier and type-specific contracts.

## 1. Architecture drivers

Yurai attaches queryable derivation evidence to eagerly evaluated domain values. The
value remains the result of ordinary C# arithmetic; Yurai adds evidence and never
substitutes its own numeric interpretation.

| Driver | Architectural response |
| --- | --- |
| Value fidelity (RQ-001) | Perform the supported type's recorded native operation before constructing result evidence; propagate native failures without producing a result. |
| Explainability and queryability (RQ-002, RQ-003, RQ-012, RQ-014) | Preserve named inputs, named results, operation metadata, selected branches, and shared dependency structure in a traversable model. |
| Deployment reach (RQ-004) | Keep `src/Yurai` on `netstandard2.0`, BCL-only, with no runtime package dependency. |
| Immutable shared evidence (RQ-011) | Use an immutable DAG in which a new operation creates a parent and reuses existing children. |
| Familiar C# usage (RQ-008, RQ-009) | Keep arithmetic eager and operator-based inside an explicitly traced region; unwrap to a plain value at its boundary. |
| Minimal, coherent API (RQ-015) | Keep node and traversal types internal, map every public operation to user value, and prefer explicit boundaries over count-driven compression. |
| Closed value-type evolution (RQ-023, RQ-028, RQ-029) | Keep 0.1.x decimal-only; introduce decimal + Int64 in 0.2.0 through `Traced<T>` with library-owned closed bindings, without an open generic factory or public policy framework. |
| Large and deep graphs | Use structural sharing and iterative traversals; measure, rather than guess, allocation and throughput in issue #27. |

The permanent non-goals are architectural boundaries: no symbolic evaluation, automatic
differentiation, sensitivity calculation, attribution, whole-program data-flow
tracking, evidence storage, rich rendering, or string-expression evaluation
(RQ-016 through RQ-022). The computation a developer explicitly places inside a traced
region is the complete scope of Yurai's evidence.

## 2. Vocabulary and semantic boundary

- A **value** is the already evaluated domain result. In 0.1.x it is a `decimal`; the
  approved 0.2.0 supported set is `decimal` plus Int64.
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

For arithmetic and Min/Max, dependencies are the recorded operand values consumed by
the operation, including both compared Min/Max operands. A branch condition is different
in the current API sketch: reading `.Value` and evaluating a comparison produces an
ordinary `bool` outside the evidence graph. An input used only by that condition is
therefore not discoverable as a dependency unless Yurai introduces traced predicates.
Q13 makes that absence the documented v1 boundary; any traced-predicate expansion is a
separate future public capability.

## 3. Architecture viewpoints

### 3.1 Context and responsibility view

```mermaid
flowchart LR
    Caller[Caller code] -->|supported plain value + domain name| Facade[Public entry points]
    Facade --> Carrier[Traced value carrier]
    Carrier -->|ordinary operators and named operations| Native[Bound value evaluation]
    Native -->|success only| Graph[Immutable evidence DAG]
    Graph --> Query[Dependency queries]
    Graph --> Explain[Text representation]
    Graph --> Json[JSON representation]
    Carrier -->|plain supported value| Caller
```

| Component | Responsibility | Explicitly does not own |
| --- | --- | --- |
| Public entry points | Introduce plain values and express operations in ordinary C# syntax. | Evidence storage, global configuration, or expression parsing. |
| Traced value carrier | Hold one root reference and expose the root's evaluated value. | Mutable state, caches, or public node access. |
| Native operation boundary | Invoke the supported type's recorded operation with the same operands and parameters. | Unrecorded coercion, runtime registration, or consumer-defined semantics. |
| Evidence nodes | Preserve evaluated values, operation metadata, and child references. | Text/JSON formatting and dependency-query result shapes. |
| Traversal kernel | Walk a DAG iteratively with reference-identity tracking and deterministic child order. | User-facing formatting policy. |
| Query, text, and JSON adapters | Interpret one immutable model for a specific output. | Mutation or re-evaluation of the calculation. |

### 3.2 Information view

The 0.1.x internal model is an abstract `EvidenceNode` with an evaluated `decimal` value.
In 0.2.0 it becomes a homogeneous `EvidenceNode<T>` DAG for one supported closed `T`,
with the same closed family of sealed node kinds. Names are conceptual here; private
implementation names may differ without changing the contract.

| Node kind | Required data | Children |
| --- | --- | --- |
| `Input` | Evaluated value and optional developer-supplied name. | None. |
| `BinaryOperation` | Evaluated value and operation kind. Min/Max also preserve which operand was selected so equal operands remain distinguishable. | Left and right, in source order. |
| `Round` | Evaluated value, digits, `MidpointRounding.ToEven`, and reason. | The unrounded value. |
| `Branch` | Evaluated value, decision name, condition outcome, selected branch label. | The selected result only; a plain condition creates no evidence edge. |
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
2. Execute the corresponding bound operation for the graph's supported `T` with the
   same operands and parameters.
3. If native evaluation throws, propagate that exception type and construct no result
   node. Existing operands remain valid and unchanged.
4. If it succeeds, create one `BinaryOperation` node containing the exact result and
   references to the existing roots.
5. Return a new carrier holding only that parent reference.

Mixed operations first introduce the plain operand as an anonymous `Input` and then use
the same flow. Its display is the invariant-formatted value. `Round(digits, reason)` invokes
`decimal.Round(value, digits)`, whose midpoint mode is `ToEven`, and records that mode
explicitly. Evidence-only metadata validation must not replace a native arithmetic
failure: an operation first invokes native evaluation and its parameter validation,
then validates evidence-only metadata, then allocates evidence. Operations that only
add metadata validate it at entry.

The supported surfaces provide no implicit conversion from `double` or `float`. Such a
conversion would silently add a precision-changing step before the native `decimal`
operation and make RQ-001 impossible to state against the expression the caller wrote.

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
- Collapsing traversals use an explicit stack and a reference-identity visited set. The
  expanding path traversal also uses an explicit stack but revisits shared nodes once
  per path. Neither uses call-stack recursion, so a graph beyond 10,000 nodes does not
  fail merely because it is deep.
- Construction performs one node allocation per recorded operation. The carrier is a
  `readonly struct`, avoiding a carrier allocation in ordinary usage;
  boxing still occurs if callers place it in `object` or a non-generic interface.
- Each collapsing traversal costs `O(V + E)` time and `O(V)` temporary identity state.
  `Trace` is output-sensitive: its time and result size are proportional to the total
  returned path data, whose path count can be exponential in `V` for a heavily shared
  DAG. No result is cached in 0.1.x: repeated calls recompute representations and
  queries from immutable evidence rather than retaining large strings or synchronized
  caches.
- Serialization writes directly to a `StringBuilder` or equivalent BCL buffer. It does
  not build a second public object model and does not add a JSON package dependency.

Allocation size and throughput are not guessed into a gate here. Issue #27 measures
native-versus-traced operations, graph construction beyond 10,000 nodes, and each
traversal before a performance threshold is proposed.

### 3.5 Public API and internal-model boundary

The 0.1.x public carrier is the non-generic `Traced`. ADR-0018 supersedes that placement
for 0.2.0: `Traced<T>` is the only carrier, while arity-different non-generic `Traced`
is a static inference companion. Only decimal and Int64 have public construction paths;
there is no generic `Of<T>`, public policy, or registration lifecycle.

The public surface may expose entry, value extraction, arithmetic composition, naming,
rounding, branch selection, text/JSON output, and dependency queries. It must not expose
nodes, node kinds, visitors, graph identifiers, or traversal collections merely to make
tests convenient. Internal graph inspection for S1 tests is provided through a friend
test assembly, not a production API.

RQ-015 is reviewed against purpose rather than an arbitrary metadata ceiling. The
architecture maintains two inventories:

- **Logical operations:** the concepts a user must learn, with an overload family such
  as addition counted once when every overload has the same documented semantics.
- **Declared CLR members:** each public type, constructor, method/operator overload,
  property, event, and field counted separately; accessors, inherited members, and
  compiler-generated members are excluded.

Both counts are reported at the Phase 2 gate, but neither is a standalone pass/fail
limit. Every logical operation must map to a registered capability or necessary .NET
value behavior, and every overload must exist for natural, type-safe C# use rather than
as an alias. `ToString`, equality, and hashing are reviewed explicitly instead of being
hidden outside a numeric budget.

Mixed arithmetic therefore uses explicit left/right plain-value overloads for the
carrier's closed `T`. An implicit conversion from a plain value would generate evidence
at an invisible conversion site, admit accidental entry into a traced region, and
complicate overload resolution. `Traced.Of(1, ...)` continues to mean decimal; Int64
entry is explicit through `Traced.OfInt64(...)`.

For 0.2.0, the public CLR type-definition inventory grows from one to two:
`Traced<T>` is the carrier and non-generic `Traced` is the inference companion. ADR-0018
owns the member-level inventory and the RQ-015 trade-off it accepts; the architectural
rule here is unchanged — arithmetic, naming, selection, rendering, and query concepts
must not multiply into public policy types.

### 3.6 Representation separation

The exact text and schema-v1 contracts in this section describe the shipped 0.1.x
decimal adapters. ADR-0018 owns the 0.2.0 `Value != Representation != Presentation`
boundary and schema-v2 logical contract; its implementation and schema document must
add type-specific output evidence without changing the shared traversal rules below.

The traversal kernel exposes only internal node-kind and child access to internal
consumers. Each consumer owns its own policy:

- `Explain` owns indentation, number formatting, labels, and shared-node reference
  presentation.
- `ToJson` owns schema fields, escaping, value representation, and document-local IDs.
  Schema v1 uses decimal encoding; schema v2 adds a document logical type and per-node
  lossless representations as defined by ADR-0018.
- `DependsOn`, `Trace`, and `Inputs` own name matching, path construction, ordering, and
  result projection.

No consumer parses another consumer's output. JSON does not parse Explain text, queries
do not inspect rendered labels, and renderers do not mutate or annotate nodes. This
keeps a future output format or schema revision from becoming a graph-model migration.

RQ-011 and RQ-012 already require a shared node to expand once and appear as a reference
afterwards. Q14 uses one deterministic document-local numeric identity mapping for text
and JSON. The text token and JSON field spelling remain representation contracts, but
neither may introduce a different identity model.

The collapsing traversal kernel therefore owns root-first, left-to-right visitation,
reference identity, first-encounter numeric IDs, depth, and revisit detection. Explain,
JSON, `DependsOn`, and `Inputs` consume that shared traversal discipline. `Trace` must
retain every root-to-match route, so it uses a separate expanding traversal with the same
child order but without global revisit suppression; using the collapsing result would
silently discard paths through shared nodes. The closed node family exposes an internal
visitor seam so adding a node kind requires every representation adapter to handle it at
compile time; adapters still own the content they produce for each kind.

#### 3.6.1 0.1.x Explain text contract

`Explain()` emits `Result`, the invariant decimal result indented by two spaces,
`Derivation`, and then the complete evidence tree. Each derivation depth adds two spaces,
and every node kind has one line containing its domain name or operation metadata and
invariant decimal value. Output uses `\n` regardless of platform and has no trailing
newline.

Names remain unquoted to keep domain vocabulary prominent. Literal line breaks, tabs,
and control characters in names use backslash escapes; quote and backslash characters
remain literal because they have no delimiter role there. Round reasons and branch names
are quoted, so quote, backslash, line-break, tab, and control characters are escaped.

Only nodes encountered more than once display their document-local ID. Their first
expanded line starts with `[#N]`; each later occurrence is the complete line `<ref #N>`.
IDs are assigned to all distinct nodes on first encounter, so the displayed numbers may
have gaps and are not stable across calls or graph revisions. An uninitialized carrier
returns the deterministic diagnostic `Uninitialized Traced`.

### 3.7 Value-type extension policy

ADR-0018 activates value-type extension for a deliberately closed 0.2.0 supported set:
`decimal` and Int64. The public carrier is `Traced<T>`, but valid initialized values are
created only through decimal `Of` and explicit `OfInt64` companion methods.

Each supported `T` has one immutable, library-owned internal binding for arithmetic,
selection, and lossless representation. Dispatch is localized to that boundary; there
is no mutable registry, consumer registration, or public policy/profile. The evidence
graph is homogeneous `EvidenceNode<T>` and never stores values as `object`.

Decimal retains its native fidelity, total ordering, digits-based rounding, and exact
representation. Int64 uses checked addition/subtraction/multiplication, native truncating
division, left-biased equal Min/Max ties, invariant base-10 representation, and no
rounding member. JSON schema v2 records one logical value type per homogeneous document
and each node's lossless representation; schema v1 remains frozen.

Floating-point and user-defined values remain deferred. Each requires a new reserved
decision covering its capabilities, fidelity, representation, and graph result types;
the syntactic existence of `Traced<T>` does not approve arbitrary `T`.

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

- **Options:** cache text/query results on nodes; external weak cache; no 0.1.x cache.
- **Evaluation axes:** repeated-call speed, retained memory, synchronization,
  complexity.
- **Recommendation:** no cache until issue #27 demonstrates a material repeated-call
  cost.
- **Reason:** immutable input makes recomputation predictable; caching would add memory
  retention and thread-safe publication before a measured need exists.

### 4.7 Type abstraction

- **Options:** open public policy/registration; closed generic carrier with internal
  per-type binding; generic math and multi-targeting.
- **Evaluation axes:** supported-type fidelity, public surface, dispatch cost,
  `netstandard2.0`, and zero runtime dependencies.
- **Recommendation:** the ADR-0018 closed generic carrier with immutable internal
  decimal/Int64 bindings and a homogeneous typed graph.
- **Reason:** it supports the approved second type without claiming arbitrary numeric
  or user-defined capability and without changing targets or dependencies.

## 5. Approved design directions

Items Q2 through Q6 retain the numbering from issue #18. Q7 onward are gaps found by
this architecture review. The maintainer selected these directions after reviewing the
options, evaluation axes, and trade-offs. They are implementation authority for the
behavior stated here; exact signatures still receive the normal public-surface review.

| Question | Approved direction | Consequence |
| --- | --- | --- |
| Q2 anonymous-input display | Display the invariant-formatted value without inventing a domain name. | Q14 identity, rather than the display label, distinguishes equal anonymous inputs. |
| Q3 conditional API | Accept lazy alternatives, evaluate the selected delegate exactly once, and neither evaluate nor record the unselected alternative. | Preserves native short-circuit behavior at the cost of delegate allocation. |
| Q4 future type strategy | Superseded for 0.2.0 by ADR-0018: keep `netstandard2.0`, use a closed `Traced<T>` carrier for decimal + Int64, and bind semantics internally. | No generic math, multi-targeting, public policy, or registration lifecycle is introduced. |
| Q5 JSON schema and stability | Publish a versioned stable schema. Encode decimal as invariant text so value and scale remain exact. | Consumers parse decimal explicitly; breaking schema changes require a new schema version. |
| Q6 public carrier name | `Traced` remains the 0.1.x carrier; ADR-0018 makes `Traced<T>` the 0.2.0 carrier and non-generic `Traced` an inference companion. | Decimal call spelling remains `Traced.Of(...)`; Int64 entry is explicit. |
| Q7 default carrier value | Treat default as invalid: `Value`, operations, queries, and JSON throw `InvalidOperationException`; `ToString()` and `Explain()` return a deterministic uninitialized diagnostic. | Initialization bugs cannot silently become zero evidence, while logs and debuggers remain useful. |
| Q8 name and reason validation | Separate anonymous-input entry from named entry. Supplied names and rounding reasons reject null, empty, and whitespace-only text; accepted text is preserved without trimming or substitution. | Incomplete evidence fails early and Yurai does not invent metadata. |
| Q9 duplicate names and paths | Permit duplicate names and return all matches and paths in deterministic traversal order. | Independent graphs remain composable; callers must handle multiple results. |
| Q10 text culture and format | Use invariant culture for v1 and defer configuration to RQ-026. | Output and snapshot tests are reproducible across environments. |
| Q12 mixed-operation surface | Use explicit left/right plain-`T` overloads for the closed supported carrier and no conversion operator. | Both decimal and Int64 retain ordinary syntax without an invisible traced-region entry. |
| Q13 branch-condition dependency | Do not create a dependency edge from a plain `bool`; document and test that condition-only inputs are outside v1 dependency queries. | Control dependencies require a separately approved traced-predicate capability. |
| Q14 shared-reference notation | Assign deterministic document-local numeric IDs; text uses a concise ID reference and JSON uses the same identity mapping. | IDs are unique within one output and explicitly unstable across outputs or graph revisions. |

For 0.1.x, Q11 is folded into Q5 because decimal encoding is part of the JSON schema
v1 decision.
The architecture-significant directions are separated into ADRs so that each can be
revisited independently. Q2 and Q7-Q10 are public behavior contracts recorded here and
in issue #18; their exact spellings and signatures remain reviewable implementation
detail within the approved semantics.

## 6. 0.1.x implementation slices and test seams

The completed slices below describe the shipped decimal implementation. ADR-0018's
0.2.0 carrier, Int64, schema-v2, and migration work are split into new slices only after
the draft decision is approved.

| Slice | Input contract | Output contract | Test seam and required scenarios |
| --- | --- | --- | --- |
| S0 internal foundation (within issue #19) | Evaluated decimal values, node metadata, and already constructed children; no public API. | Immutable node family, structural sharing, reference-identity traversal kernel, and deterministic child ordering. | Friend-assembly construction/inspection; immutability, sharing, cycle-prevention, node-count, diamond, concurrency, and a chain beyond 10,000 nodes. RQ-011. |
| S1 public core boundary (remainder of issue #19) | Plain decimal with optional name; two initialized carriers for arithmetic. | New immutable root for input, naming, or native `+ - * /`; plain value equals root value. | Representative value/exception parity, default-carrier diagnostics, explicit-boundary tests, and S0 graph inspection. RQ-001, RQ-007, RQ-008. |
| S2 mixed and selection | One carrier and one plain decimal in either order, or two values for Min/Max. | Anonymous input plus binary evidence; Min/Max records selected operand even when values compare equal. | Both operand orders, all operations, equal Min/Max operands, sharing, and property-test hooks. RQ-008, RQ-009. |
| S3 rounding | Initialized carrier, valid digits, and reason. | Native `decimal.Round(value, digits)` result plus explicit digits, `ToEven`, and reason evidence. | Midpoints, negative values, scale, digits bounds, native exception parity, reason retention. RQ-001, RQ-010. |
| S4 branching | Condition, alternatives, and decision name in the Q3-approved shape. | Value of the selected alternative and branch evidence containing the actual outcome; condition dependency follows Q13. | Both outcomes, nested decisions, short-circuit behavior, selected-only evidence, and condition-only input boundary. RQ-001, RQ-005, RQ-014. |
| S5 text | Any initialized evidence root. | Deterministic complete text; each shared node expands once and later occurrences use the Q14 reference notation. | Domain-sample expectations, every node kind, escaping, shared references, duplicate names, and a chain beyond 10,000 nodes. RQ-002, RQ-012. |
| S6 JSON | Any initialized evidence root. | Valid dependency-free JSON preserving every evidence field and graph edge under the Q5 schema and Q14 identity mapping. | Parse-and-compare with a standard parser in tests, all kinds, escaping, decimal value/scale, deterministic IDs, and shared references. RQ-004, RQ-013, RQ-027. |
| S7 queries | Initialized root and, where needed, an input name. | Dependency boolean, dependency path result, or deterministic named-input collection under Q9/Q13. | No match, anonymous inputs, duplicate names, multiple paths, condition-only inputs, diamonds, cycles impossible, and deep graphs. RQ-003, RQ-014. |

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
| Simplicity and usability | One eager carrier per supported `T`, ordinary operators, no public graph API, and an explicit traced boundary. |
| Maintainability and testability | Closed internal node kinds, representation adapters, friend-assembly structural seam, and RQ-tagged behavior tests. |
| Extensibility | 0.1.x remains decimal-concrete; 0.2.0 exposes only the ADR-0018 closed generic contract and no open generic-math or registration framework. |
| Performance and memory | One node per recorded action, fixed child fields, structural sharing, iterative `O(V+E)` walks; benchmark before optimization. |
| Thread safety | Immutable reachable state, no cache or global identity service; concurrent-read tests. |
| API stability | Internal nodes stay hidden; logical operations and CLR members are inventoried, requirement-mapped, and reviewed for redundant or surprising routes. |
| Portability | `netstandard2.0`, BCL-only, no serialization package, no newer runtime features in shipped code. |
| Failure observability | Native exceptions propagate; invalid carrier and metadata behavior follows Q7/Q8; no partial result is returned. |

## 8. Traceability

| Requirements | Architecture coverage |
| --- | --- |
| RQ-001 | Native-first evaluation, no result node on native failure, parity tests. |
| RQ-002, RQ-007, RQ-012 | Named evidence, representation adapter, sample-based text tests. |
| RQ-003, RQ-014, RQ-017, RQ-018 | Dependency-only vocabulary, query boundary, and explicit Q13 condition-dependency limit. |
| RQ-004 | BCL-only serialization and `netstandard2.0` portability boundary. |
| RQ-005 | Lazy selected-only branch evaluation and recorded outcome. |
| RQ-008, RQ-009, RQ-010 | Native operators, anonymous inputs, selection metadata, and recorded native rounding. |
| RQ-011 | Immutable DAG, reference sharing, linear node growth, iterative traversal, and Q14 reference identity. |
| RQ-013, RQ-020, RQ-021, RQ-027 | Versioned JSON adapters with schema-v1 invariant decimal text and schema-v2 type-specific lossless representation, caller-owned export material, and no storage or rich renderer. |
| RQ-015 | Hidden graph model, explicit traced boundary, purpose-mapped logical operations, and transparent dual inventory. |
| RQ-016, RQ-019, RQ-022 | Eager concrete calculation inside an explicit traced region only. |
| RQ-023, RQ-028, RQ-029 | Decimal-only 0.1.x; closed decimal + Int64 0.2.0 carrier; explicit per-type contracts and no speculative open policy framework. |
| RQ-024, RQ-025 | The architecture uses the fixed vocabulary and makes no broader product claim. |
| RQ-026 | Formatting remains an adapter seam; configurability is deferred. |

RQ-006 concerns the README's related-work content and imposes no additional internal
structure; it remains a release-gate requirement outside this architecture.

## 9. Risks and readiness

- **Public contract readiness:** Q2-Q10 and Q12-Q14 now define the behavioral direction
  for their named slices. Exact public signatures and the complete API inventory still
  require normal detailed-design and public-surface review before implementation.
- **API-coherence risk:** raw member counts can reward implicit conversions or overload
  compression that obscure the traced boundary. Gate review must inspect the purpose
  mapping and both inventories rather than optimize either count.
- **Condition-dependency risk:** a plain `bool` loses which traced value decided a
  branch. Q13 accepts this documented v1 limitation; S4/S7 must not claim control
  dependency completeness.
- **Default-struct risk:** a `readonly struct` cannot prevent zero initialization. Q7
  prevents silent zero evidence; every public operation still needs the specified
  invalid-state tests.
- **Name ambiguity:** Q9 preserves all duplicate matches, so result cardinality and
  deterministic path ordering must be explicit in the query signatures and tests.
- **Large-output risk:** structural sharing bounds graph storage but text and JSON can
  still be large. Iterative traversal prevents stack failure, but full two-space
  indentation makes a depth-*D* chain's text size `O(D^2)`. `Trace` retains every path,
  so a repeatedly shared graph can produce exponentially many paths relative to its
  unique-node count even though `DependsOn` and `Inputs` remain linear. RQ-026 and issue
  #27 address depth or output limiting, usability, and measured cost; adding a public
  bound or guard remains a reserved semantic decision.
- **Hand-written JSON risk:** escaping and exact decimal representation are correctness
  hotspots. Parse-and-compare tests and mutation review are mandatory seams.
- **Future-type risk:** decimal assumptions are isolated but not eliminated. A new type
  requires a new fidelity contract and a reserved architecture/API decision.

The internal S0 evidence model and traversal kernel remain implementation-ready. The
reserved directions for later slices are now approved; each public slice next needs a
signature-level detailed design, API inventory, and test contract that implements these
directions without introducing a new reserved decision.
