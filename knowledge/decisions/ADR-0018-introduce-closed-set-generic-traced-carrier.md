---
type: ADR
title: Introduce a closed-set generic Traced carrier for decimal and Int64
description: Yurai 0.2.0 introduces Traced<T> for decimal and Int64 with library-owned type semantics, a homogeneous typed evidence DAG, and JSON schema v2.
tags: [api, types, generics, evidence, json, compatibility, adr]
status: draft
requirements: [RQ-001, RQ-004, RQ-008, RQ-009, RQ-010, RQ-011, RQ-012, RQ-013, RQ-015, RQ-023, RQ-027, RQ-028, RQ-029]
generated: { by: codex/2026-08, at: 2026-08-11T18:35:01+09:00 }
sources:
  - id: issue-67
    resource: https://github.com/urario/Yurai/issues/67
    title: "Issue #67: Traced<T> and value-type policy architecture"
---

# ADR-0018: Introduce a closed-set generic Traced carrier for decimal and Int64

Yurai 0.2.0 introduces a generic carrier for exactly two library-supported value types,
without exposing an open-ended numeric policy or weakening the existing portability and
dependency constraints.

## Context

Yurai 0.1.x has one public value carrier, `Traced`, whose value, arithmetic, rounding,
evidence nodes, text formatting, and JSON representation are all concrete `decimal`
semantics. Issue #67 approves a second value type for 0.2.0 while keeping
`netstandard2.0`, zero runtime dependencies, and no dependency on `INumber<T>` or
multi-targeting.

The pre-design investigation showed that a single open-ended numeric policy does not
model all candidate families correctly. Integers have integral division and an explicit
overflow policy. Floating-point values add NaN, infinities, signed zero, and non-total
selection semantics. User-defined values may have partial operations or heterogeneous
results such as `Money / Money -> decimal` or `Quantity * Rate -> Measure`. A single
`Traced<T>` also cannot make operators and members appear or disappear according to an
optional runtime capability.

The architecture review therefore triggered the issue's documented escape condition:
user-defined values and floating-point values require a separate later decision rather
than forcing their semantics into the first generic carrier.

0.2.0 may make a source and binary breaking change because the initial release is
explicitly SemVer 0.x and the public API is not frozen before 1.0. The earlier decision
was inexpensive while 0.1.0 was unpublished, but 0.2.0 still has a real migration cost
for 0.1.x users; release documentation must account for it. Permanent compatibility
carriers are not justified, while source ergonomics that can be preserved without a
second carrier are preserved.

The direction recorded here was settled with the maintainer in issue #67. The `draft`
status means that this record and its synchronization changes have not yet completed
pull-request review; it does not reopen the issue-level direction.

## Decision

### Supported value types and public boundary

Yurai 0.2.0 introduces:

```csharp
public readonly struct Traced<T>
```

The public supported set is deliberately closed to `decimal` and `System.Int64`
(`long`). `Traced<T>` has no public constructor and there is no generic public `Of<T>`
factory. Naming `Traced<TOther>` in source does not make `TOther` a supported Yurai
value type. A valid initialized carrier can be created only through library-provided
entry points or operations over already valid carriers.

The non-generic `Traced` name remains as an arity-different static inference companion,
not as a second carrier:

```csharp
public static class Traced
{
    public static Traced<decimal> Of(decimal value);
    public static Traced<decimal> Of(decimal value, string name);

    public static Traced<long> OfInt64(long value);
    public static Traced<long> OfInt64(long value, string name);

    public static Traced<T> If<T>(
        bool condition,
        Func<Traced<T>> whenTrue,
        Func<Traced<T>> whenFalse,
        string branchName);

    public static Traced<decimal> Min(Traced<decimal> left, Traced<decimal> right);
    public static Traced<long> Min(Traced<long> left, Traced<long> right);
    public static Traced<decimal> Max(Traced<decimal> left, Traced<decimal> right);
    public static Traced<long> Max(Traced<long> left, Traced<long> right);

    public static Traced<decimal> Round(
        this Traced<decimal> value,
        int digits,
        string reason);

    [Obsolete("JSON schema v1 is a 0.2.x compatibility bridge. Use ToJson() for v2.")]
    public static string ToJsonV1(this Traced<decimal> value);
}
```

`Traced<T>` retains the type-independent evidence surface: `Value`, `As`,
`DependsOn`, `Inputs`, `Trace`, `Explain`, and `ToJson`.

No implicit or explicit conversion operators are added between `Traced<T>` and `T`.
The existing explicit boundary remains: factories introduce evidence and `.Value`
leaves it.

### Arithmetic capability

`Traced<T>` declares the existing closed arithmetic forms for `+`, `-`, `*`, and `/`:

```text
Traced<T> op Traced<T>
Traced<T> op T
T         op Traced<T>
```

These operators are part of the closed 0.2.0 supported-type contract, not an assertion
that arbitrary `T` is arithmetic. No supported initialized carrier lacks these four
operations.

The implementation does not attempt `T op T` directly. Semantics are bound internally,
immutably, once per supported closed `T`. There is no public profile, mutable registry,
runtime registration, or consumer-supplied semantics in 0.2.0. Type dispatch, if needed,
is localized to one internal binding boundary rather than spread across operators,
formatters, and nodes.

Internal responsibilities remain separated conceptually into arithmetic, selection,
representation, and decimal-only rounding, but 0.2.0 does not create public extension
interfaces or require interface-based implementation where no substitution point exists.

### Int64 fidelity

`Traced<long>` uses one library-owned semantics:

- addition, subtraction, and multiplication use checked Int64 arithmetic;
- overflow propagates `OverflowException`;
- division is Int64 division and truncates toward zero;
- division by zero propagates `DivideByZeroException`;
- Yurai does not widen an Int64 division result to decimal;
- mixed plain operands rely only on normal C# overload resolution and implicit numeric
  conversions; Yurai performs no additional coercion.

For Int64 `Min` and `Max`, equal values select the left operand. Result value and
`SelectedOperand` are produced by the same selection operation so evidence cannot claim
a different selected value from the recorded result.

`Round` is not an Int64 capability and is absent from the Int64 receiver surface.

Int64 support ships only after example and property tests cover this contract: integer
division (`5 / 2 == 2`), both overflow edges (`long.MaxValue + 1` and
`long.MinValue / -1`), division by zero, mixed plain operands, equal `Min` and `Max`
ties, and lossless invariant base-10 round-trip at `long.MinValue` and `long.MaxValue`.
Issue #67 records measured evidence that native Int64 behaves as stated here; the
shipping tests keep Yurai's implementation aligned with that contract.

### Native-first evaluation and failures

The existing native-first rule remains in force:

1. validate all traced operands are initialized;
2. execute the type's native or library-defined value operation exactly once;
3. create result evidence only after successful evaluation.

Arithmetic exceptions are not wrapped in Yurai-specific exceptions. Failed operations
do not create result evidence.

`default(Traced<T>)` remains an invalid, uninitialized carrier. APIs that require a value
or graph fail consistently with `InvalidOperationException`; diagnostic behavior already
established for `Explain()` may remain. This decision does not add `IsInitialized`.

### Evidence model

The internal graph becomes a homogeneous typed DAG, such as `EvidenceNode<decimal>` or
`EvidenceNode<long>`. A single `Traced<T>` graph contains value-bearing nodes of one
`T`. Existing immutable structural sharing, reference identity, selected-only branches,
document-local output identities, and acyclic construction remain governed by ADR-0006,
ADR-0010, and ADR-0015.

Evidence nodes store the evaluated native `T` once. They do not store a profile, codec,
JSON string, serialization ID, or mutable cache. Representation is a deterministic
projection of the native value at the output boundary.

0.2.0 does not add a heterogeneous graph, `object` value storage, automatic node
interning, or a new Literal node kind. Existing mixed-plain-value evidence semantics are
preserved: a plain operand is represented by the existing anonymous input form.

### Value, representation, and presentation

Yurai distinguishes three layers:

```text
Value          native runtime value
Representation lossless evidence encoding of that native value
Presentation   human-oriented formatting
```

`Value != Representation != Presentation` is a design boundary.

For decimal, representation preserves the native decimal representation relevant to the
existing fidelity contract, including scale and signed zero. `1m` and `1.00m` therefore
need not have the same evidence representation even though they are numerically equal.
For Int64, the representation is an invariant base-10 integer string.

Source-code lexical spelling is outside the fidelity boundary. Yurai records the runtime
value it receives, not whether the caller wrote an equivalent literal in decimal,
hexadecimal, or another lexical form.

`Explain()` remains deterministic and human-readable and preserves value representation
fidelity, but its entire text layout is not a machine-readable schema contract.

### JSON schema v2

JSON schema v1 remains frozen under ADR-0013 and ADR-0014. Generic carriers use a new
schema v2.

Because 0.2.0 evidence DAGs are homogeneous, schema v2 records one stable logical value
type for the document and records each value-bearing node's lossless `representation`.
The schema document owns exact field names, but its logical shape is:

```json
{
  "schemaVersion": 2,
  "valueType": "yurai.int64",
  "root": 2,
  "nodes": [
    { "id": 0, "kind": "input", "representation": "10", "name": "Count" },
    { "id": 1, "kind": "input", "representation": "2", "name": null },
    { "id": 2, "kind": "binaryOperation", "representation": "12", "operation": "add", "left": 0, "right": 1 }
  ]
}
```

Built-in logical type identifiers are `yurai.decimal` and `yurai.int64`. A JSON number
is not used as the authoritative Int64 representation because consumers may parse
numbers through a narrower numeric type. The representation string is the lossless
contract.

Every value-bearing node records the result actually observed at that node. Consumers
must not reconstruct the authoritative result by re-executing child operations. Such
recomputation may be used later for validation only.

Document-local node IDs and deterministic traversal continue to follow ADR-0015; they
are topology references, not persistent evidence identities.

`Traced<T>.ToJson()` emits v2. `Traced<decimal>.ToJsonV1()` is retained only throughout
0.2.x as an obsolete migration bridge and may be removed in 0.3.0. The v1 schema itself
remains a stable historical contract regardless of emitter lifetime.

### Deferred families

The other candidate families were evaluated rather than silently dropped:

- `double` and `float` require a separate fidelity decision covering NaN, infinities,
  signed zero, non-total selection behavior, and exact round-trip representation;
- user-defined values require a separate decision covering partial capability sets,
  domain invariant failures, stable logical type ownership, and potentially
  heterogeneous `TLeft x TRight -> TResult` evidence.

Neither family is made public in 0.2.0. Adding a generic public factory, public policy,
consumer registration, floating-point factory, or heterogeneous operation requires a
new reserved architecture decision. The 0.2.0 operator surface must not be automatically
generalized to those types.

## Consequences

The second real value type removes decimal assumptions from the carrier and evidence
model without pretending Yurai has solved arbitrary numeric or domain values. The public
abstraction stays small: one generic carrier plus one inference companion, with no policy
types or registration lifecycle exposed to consumers.

The public CLR type-definition inventory nevertheless grows from one to two: the
non-generic 0.1.x carrier is replaced by `Traced<T>` plus static `Traced`. The companion
adds two explicit `OfInt64` overloads and one Int64 overload each for `Min` and `Max`;
`Round` moves to an exact-receiver decimal extension, and `ToJsonV1` is a temporary
schema-v1 migration route. These are additional declared members, but not an open policy
surface: each route either selects one of the two approved type contracts or preserves a
bounded compatibility exit. This is the accepted RQ-015 trade-off.

The cost is an intentional closed-world generic API. C# still shows the generic carrier
members for a syntactically nameable unsupported `T`; Yurai prevents supported
initialized instances of such types by controlling all public construction. This
compromise preserves ordinary arithmetic syntax for decimal and Int64 without leaking
policy types into every consumer signature.

`Traced.Of(1000m, "Price")` keeps its call spelling but now returns
`Traced<decimal>`. Code that explicitly names the old non-generic carrier, stores
`Func<Traced>`, or depends on its binary identity must migrate to `Traced<decimal>`.
This is an accepted 0.2.0 breaking change. No compatibility wrapper carrier is retained.
Yurai does not ship a decimal alias type; consumers that want a domain-local shorthand
may define a closed-type alias such as `using Money = Yurai.Traced<decimal>;`.

Inferred `var` examples generally remain source-compatible, while explicit `Traced`
declarations, fields, properties, delegate types, and generic arguments require the
closed type. The compiler may report CS0723 or CS0718 because non-generic `Traced` is now
static, without suggesting `Traced<decimal>`. Migration notes must map those diagnostics
to the required type change.

`Traced.Of(1, "Count")` continues to select the decimal factory by the existing
implicit conversion. Int64 adoption is explicit through `Traced.OfInt64(...)`,
preventing a source-compatible call from silently acquiring integer division and
overflow semantics.

That compatibility choice has a reverse hazard: passing a plain `long` or `int` to
`Traced.Of(...)` still creates `Traced<decimal>` through implicit conversion. A caller
who intended Int64 semantics must select `OfInt64` explicitly, and the compiler does not
diagnose the omission. The `Of(decimal)` XML documentation, README, migration guide, and
release notes must call out this behavior; an analyzer or another API mitigation would
require a separate reserved decision.

The non-generic companion means the assembly again contains two public CLR type
definitions, `Traced` and `Traced<T>`. That is the accepted cost of retaining
inference-friendly `Traced.Of(...)` and `Traced.If(...)` syntax on C#, where a generic
containing type cannot infer its own type argument from a static method call.

This decision supersedes ADR-0009 and ADR-0016. It also supersedes ADR-0017's placement
rule that creation methods live on the carrier: the namespace-collision lesson remains,
but 0.2.0 moves those methods to the arity-different `Traced` inference companion.
ADR-0006's immutable DAG decision remains valid; this decision activates the generic
internal migration that ADR-0006 deliberately deferred until a second type was approved.
ADR-0013, ADR-0014, and ADR-0015 remain valid; schema v2 extends their compatibility and
representation principles rather than rewriting schema v1.

The requirements registry, core architecture, execution plan, and release documentation
must reflect that 0.2.0 supports decimal and Int64 only. The original three-family,
single-policy assumption is no longer current.

Before production implementation, compile and benchmark spikes must verify the three
generic operator forms and the chosen internal closed-type dispatch on Yurai's runtime
targets. They must report dispatch, boxing, and allocation separately from evidence-node
allocation. If the approved public signatures cannot be implemented without changing
the public API, implementation stops and returns to a reserved decision rather than
introducing a registry or public policy implicitly.
