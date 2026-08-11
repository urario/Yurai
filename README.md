# Yurai

[![CI](https://img.shields.io/github/actions/workflow/status/urario/Yurai/ci.yml?branch=main&label=CI)](https://github.com/urario/Yurai/actions/workflows/ci.yml)
[![Deep quality](https://img.shields.io/github/actions/workflow/status/urario/Yurai/deep.yml?branch=main&label=deep%20quality)](https://github.com/urario/Yurai/actions/workflows/deep.yml)
[![Mutation gate](https://img.shields.io/badge/mutation%20gate-%E2%89%A590%25-brightgreen)](knowledge/process/testing-and-quality.md#mutation-testing)
[![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-512BD4)](https://learn.microsoft.com/dotnet/standard/net-standard)
[![Dependencies](https://img.shields.io/badge/dependencies-0-brightgreen)](Directory.Build.targets)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

**Yurai is a lightweight computation-lineage library for explainable domain calculations in .NET.**

*The name comes from the Japanese word 由来 (yurai), "origin" — how something came to be.
Fitting, since that is exactly what the library keeps attached to every value it touches.*

```csharp
using System;
using Yurai;

var basePrice = Traced.Of(1000m, "BasePrice");
var discount = Traced.Of(0.10m, "MemberDiscount");
var taxRate = Traced.Of(0.10m, "TaxRate");
var total = (basePrice * (1 - discount) * (1 + taxRate))
    .Round(0, "Round to whole currency unit")
    .As("Total");

Console.WriteLine(total.Explain());
```

```text
Result
  990
Derivation
  Total = 990
    Round(digits: 0, reason: "Round to whole currency unit") = 990
      Multiply = 990.0000
        Multiply = 900.00
          BasePrice = 1000
          Subtract = 0.90
            1
            MemberDiscount = 0.10
        Add = 1.10
          1
          TaxRate = 0.10
```

`total` is a value you can keep using — `total.Value` is `990m`, exactly what the same
expression produces without Yurai. What Yurai adds is that the value carries the evidence of
how it was reached, and that evidence can be printed, exported as JSON, or queried in code.

> **Status: 0.1.0, not yet on NuGet.** The library is implemented and its behavior is under
> test — the output above is what it prints, pinned by a test that reads this page. The
> version is deliberately `0.x`: the public surface may still change, and a second value type
> beside `decimal` is the change most likely to move it.

**Try it:** [Follow the three-step Quick start](#quick-start) to run this example in a
new console application.

## Why this exists

Domain calculations get questioned. A support ticket asks why an invoice says 990. A
regulator asks which rate was applied. A colleague asks whether last month's fix changed the
payroll formula. The usual answers are a debugger session, a log line that records the result
but not the reasoning, or a comment in the code that may no longer be true.

Yurai keeps the reasoning attached to the value while the calculation runs, so the answer is
available afterwards without re-running anything.

## The four kinds of "why"

"Why is this value what it is?" is four different questions. Yurai answers two of them and
deliberately refuses the other two, because a dependency path read as a sensitivity or an
apportionment leads to a wrong business decision.

| Question | Example | Yurai |
|---|---|---|
| **Dependency** — which inputs does this value depend on? | Does `NetPay` depend on `SocialInsuranceRate`? | Answered |
| **Derivation** — how was it computed from those inputs? | Which operations ran, in what order, with what rounding and which branch? | Answered |
| **Sensitivity** — how much would the result change if an input changed? | What would `Total` be if the tax rate were 12%? | Out of scope, permanently |
| **Attribution** — how much of the result is owed to a given input? | How much of `Total` is owed to tax? | Out of scope, permanently |

In Yurai's vocabulary, a *trace* is the dependency path of a value. It is not an execution
log, not a diagnostic trail, and not a statement about how much any input matters.

## What Yurai records

- **Named inputs and named intermediate results.** `Traced.Of(1000m, "BasePrice")` brings a
  value in under your domain vocabulary; `.As("Total")` names a result after computing it.
  Names are what the explanation and the queries speak in.
- **Ordinary arithmetic.** `+ - * /` between traced values, plus `Min` and `Max`. Traced
  values also combine with plain `decimal` values on either side (`traced * 1.1m`). Bringing
  a calculation under Yurai means naming its inputs and taking the result back out at the
  boundary — the arithmetic expression itself does not have to be rewritten into a DSL.
  Mixed arithmetic uses explicit overloads; an implicit numeric conversion does not opt a
  plain value into tracing at an invisible call site.
- **Rounding as a recorded decision.** `Round(digits, reason)` keeps the stated reason next
  to the step that changed the number — usually the most contested step in a money
  calculation.
- **The branch actually taken.** `Traced.If` records which alternative produced the value,
  under the name you gave that decision, so "which rule fired?" is answerable.
- **Immutable evidence, shared structure.** Evidence never changes once computed, values are
  safe to share across threads, and reusing an intermediate result records its derivation
  once rather than duplicating it.

Three ways out of the evidence:

| | |
|---|---|
| `Explain()` | The human-readable derivation above — for a code review, a ticket, or a conversation with a domain expert |
| [`ToJson()`](docs/json-schema-v1.md) | The same evidence as versioned JSON, for systems outside the process. It is *material* for an audit trail kept by your systems, not an audit trail in itself |
| `DependsOn`, `Trace`, `Inputs` | The same evidence queried in code — assert in a test that a calculation still uses an input, or route a question by what a value depends on |

```csharp
var subtotal = (Traced.Of(100m, "BasePrice") - Traced.Of(10m, "Discount"))
    .As("Subtotal");
var total = (subtotal + 5m).As("Total");

bool stillUsesBasePrice = total.DependsOn("BasePrice");
IReadOnlyList<string> inputs = total.Inputs;
IReadOnlyList<IReadOnlyList<string>> paths = total.Trace("BasePrice");
// inputs: ["BasePrice", "Discount"]
// paths: [["BasePrice", "Subtotal", "Total"]]
```

Names use exact ordinal matching. `DependsOn` and `Trace` address both named inputs and
results named with `As`; `Inputs` contains distinct input names only. `Trace` returns every
matching path in deterministic order, projected from the matching name toward the result.
Anonymous nodes remain part of dependency traversal but do not add a name to a path.
`DependsOn` and `Inputs` are linear in graph size. Because `Trace` retains every path, its
result can grow exponentially relative to the number of unique nodes in a heavily shared graph.

Two worked examples, with their expected output:

- [Pricing calculation](samples/Pricing/README.md) — discount, tax, rounding.
- [Payroll calculation](samples/Payroll/README.md) — overtime, social insurance, progressive
  tax brackets, and a dependency query.

## Working with traced values

A traced value is a different type from `decimal`, and that is a question worth answering
directly: does it spread through the whole codebase?

It does not have to, and it should not. Trace the calculation you need to explain, and let
plain values cross the boundary in both directions:

```csharp
public decimal CalculateTotal(Order order)
{
    var basePrice = Traced.Of(order.BasePrice, "BasePrice");
    var total = (basePrice * (1 - Traced.Of(order.Discount, "MemberDiscount")))
        .Round(0, "Round to whole currency unit")
        .As("Total");

    _evidenceStore.Save(order.Id, total.ToJson());

    return total.Value;
}
```

Isolating the calculation that has to be explained is a design decision, not a workaround:
the traced region stays small enough to read, and everything around it keeps its own types.

For help deciding where that boundary belongs, see [Which calculations should use Yurai?](docs/which-calculations.md).

Yurai is designed for explicitly bounded regions with tens of domain calculation steps,
not for tracing every variable in an application. The published
[performance baseline](docs/performance.md) includes graphs beyond 10,000 evidence nodes as
stress cases and explains the time, allocation, and dependency-path scaling trade-offs.

Boolean conditions are another explicit boundary in the current v1 design. Reading
`.Value` and evaluating a comparison produces a plain `bool`; if that boolean is passed to
`Traced.If`, Yurai cannot recover which traced input decided it. An input used only by the
condition therefore does not appear in v1 `DependsOn` or `Trace`. Q13 in
[#18](https://github.com/urario/Yurai/issues/18) fixes this as a documented boundary:
dependency queries cover recorded value derivation, not condition-only control
dependency. A future traced-predicate API would be a separate capability.

## Related work

Several projects answer nearby questions. The differences in scope and model are worth
stating up front.

| Project | What it does | How Yurai differs |
|---|---|---|
| [Petit Poucet](https://github.com/liflab/petitpoucet) | Java library for fine-grained explainability: compose functions from primitives and ask which parts of the input produced a given part of the output ([CAV 2021](https://link.springer.com/chapter/10.1007/978-3-030-81688-9_24)) | .NET rather than Java, and narrower on purpose — arithmetic on domain values written as ordinary C#, rather than a general lineage relation over composed functions and arbitrary data. No .NET port or fork of Petit Poucet was found as of this writing |
| [Fluent.Calculations.Primitives](https://github.com/jitt-team/fluent-calculations-primitives) | .NET library (last released March 2024) for traceable business calculations: a `Number`/`Condition` value carries both an eagerly evaluated primitive and a captured expression via ordinary C# operators; its `EvaluationScope` adds named-calculation capture (readable source text) and cross-reference caching, with JSON export and optional DOT/Graphviz rendering | GPL-3.0-only, versus Yurai's MIT; targets .NET 8.0 only, versus Yurai's `netstandard2.0` reach back to .NET Framework 4.6.1; `EvaluationScope` is the library's higher-level capture/cache pattern, not a requirement for the underlying `Number` arithmetic — the sharper difference is that its structure is walked with a visitor a caller implements, versus Yurai's built-in `DependsOn`/`Trace`/`Inputs` queries |
| [compprov-core](https://github.com/compprov/compprov-core) | Java framework (Apache-2.0) that wraps `BigDecimal`/`BigInteger` operations into a tracked Calculation Provenance Graph — a DAG of variables and data-flow edges — with snapshot, replay, diff, and input-substitution support | Different ecosystem: Java has no operator overloading, so computations go through explicit wrapper calls and an `env`/context object rather than `+ - * /`. It also depends on Jackson for serialization and ships snapshot/replay/diff/input-substitution features Yurai deliberately leaves out of scope. Listed as prior art for the same problem shape, not a .NET alternative |
| [Appify.HumanReadableCalculationSteps](https://github.com/AppifySheets/human-readable-calculation-steps) | Small NuGet package (Unlicense, .NET 8.0) where captioned decimal values combined with ordinary C# operators produce a linear, human-readable calculation-steps trace, aimed at invoices and payroll | Produces formatted text, not a structure the program can query — no dependency-path API and no JSON export documented. Yurai's evidence is an immutable, structurally shared DAG that is queryable in code and exportable as JSON, not only printable |
| [handcalcs](https://github.com/connorferster/handcalcs) | Python library that renders calculation code as LaTeX with symbolic substitution, for notebooks and printed reports | Produces a structure the running program can query and export, not a typeset document. Yurai renders no LaTeX and no HTML |
| [Calcpad](https://calcpad.eu/) | Free program for mathematical and engineering worksheets, with its own scripting language and HTML report output | A library you call from an existing C# domain model, not a separate environment to author calculations in |
| [NCalc](https://github.com/ncalc/ncalc), [NTDLS.ExpressionParser](https://github.com/NTDLS/NTDLS.ExpressionParser), [MathParser.org-mXparser](https://mathparser.org/) | .NET expression evaluators: they parse and evaluate expressions supplied as strings (mXparser also does symbolic calculus) at runtime | Calculations stay compiled C# — type-checked, refactorable, reviewable. Yurai never parses an expression string, and it answers why a value is what it is rather than what a string expression evaluates to |
| [Audit.NET](https://github.com/thepirat000/Audit.NET) | .NET framework for audit trails: records operations and data changes through pluggable data providers | Audit logging records *what happened* — which operation, when, by whom. Yurai answers *why this value* — the derivation inside a single calculation. Yurai also stores nothing; it returns the evidence and stops there |

These comparisons are intentionally about each project's scope, ecosystem, license, runtime,
and query model. They are limited observations about the entries listed here, not a
uniqueness claim about Yurai. Yurai's claim is only about itself:

> Yurai combines ordinary C# arithmetic over eagerly evaluated domain values with immutable,
> structurally shared derivation evidence and first-class dependency-path queries, in a
> zero-runtime-dependency `netstandard2.0` library.

## Non-goals

These are decisions, not gaps waiting to be filled.

Permanently out of scope:

- **Symbolic algebra** — no expression rewriting, simplification, or solving. Yurai records
  how a concrete value was actually computed.
- **Sensitivity analysis and automatic differentiation** — no derivatives, no what-if deltas.
- **Attribution** — no apportionment of a result among its inputs, under any name.
- **Taint tracking and general-purpose data provenance** — the evidence covers exactly the
  computation you chose to trace, and carries no security guarantee.
- **Audit-platform features** — no storage, signing, timestamping, or retention. Yurai
  produces the material; persistence and integrity belong to your systems.
- **Evaluating expressions supplied as strings** — calculations are compiled code.

Not planned, without being a redefinition of the library:

- **LaTeX or HTML rendering** — text and JSON are the two exits. The JSON export exists so
  that richer rendering can be built outside the library.

Out of v1.0, with the future left open:

- **Value types other than `decimal`** — the v1.0 public surface is `decimal` only, because
  that is the correct type for money and rate arithmetic. Yurai is a computation-lineage
  library whose first shipped value type is `decimal`, not a `decimal` library; extending it
  is an open question rather than a promise.

## Installation

The package is not on NuGet yet. From the first release (`0.1.0`) onward:

```shell
dotnet add package Yurai
```

### Quick start

After `0.1.0` is available, the shortest path from a new console application to the
first explanation is:

1. Create a console application and add Yurai:

   ```shell
   dotnet new console -n YuraiQuickstart
   cd YuraiQuickstart
   dotnet add package Yurai --version 0.1.0
   ```

2. Replace `Program.cs` with this complete example:

   ```csharp
   using System;
   using Yurai;

   var basePrice = Traced.Of(1000m, "BasePrice");
   var discount = Traced.Of(0.10m, "MemberDiscount");
   var taxRate = Traced.Of(0.10m, "TaxRate");
   var total = (basePrice * (1 - discount) * (1 + taxRate))
       .Round(0, "Round to whole currency unit")
       .As("Total");

   Console.WriteLine(total.Explain());
   ```

3. Run it:

   ```shell
   dotnet run
   ```

The first output is the `Result` / `Derivation` explanation shown at the top of this
page.

Yurai targets `netstandard2.0` and has **zero runtime dependencies** — BCL only, including
the JSON export. NuGet compatibility therefore includes .NET Framework 4.6.1+ and .NET Core
2.0+, and every .NET release since, without pulling anything in behind it. On .NET Framework,
Microsoft [recommends 4.7.2 or later](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)
when consuming .NET Standard 2.0 libraries.

## Project

- [Contributing guide](CONTRIBUTING.md) — how to propose a change, and what a pull request
  needs.
- [Collaboration contract](AGENTS.md) — how humans and AI agents develop this repository
  together, and who decides what.
- [Knowledge base](knowledge/index.md) — requirements, architecture decision records, and
  process conventions.
- [Execution plan](docs/project-execution-plan.md) — phases, issues, and their dependencies.

## License

[MIT](LICENSE).
