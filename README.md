# Yurai

**Yurai is a lightweight computation-lineage library for explainable domain calculations in .NET.**

> **Status: pre-release.** The library is not implemented or published yet. The code below is
> the intended shape of the API; its final form and the exact explanation output are being
> designed in [#17](https://github.com/urario/Yurai/issues/17) and
> [#18](https://github.com/urario/Yurai/issues/18).

```csharp
var basePrice = Yurai.Of(1000m, "BasePrice");
var discount = Yurai.Of(0.10m, "MemberDiscount");
var taxRate = Yurai.Of(0.10m, "TaxRate");

var total = (basePrice * (1 - discount) * (1 + taxRate))
    .Round(0, "Round to whole currency unit")
    .As("Total");

Console.WriteLine(total.Explain());
```

```text
Total = 990
└─ Round(digits: 0, reason: "Round to whole currency unit") = 990
   └─ Multiply = 990.0000
      ├─ Multiply = 900.00
      │  ├─ BasePrice = 1000
      │  └─ Subtract = 0.90
      │     ├─ 1
      │     └─ MemberDiscount = 0.10
      └─ Add = 1.10
         ├─ 1
         └─ TaxRate = 0.10
```

`total` is a value you can keep using — `total.Value` is `990m`, exactly what the same
expression produces without Yurai. What Yurai adds is that the value carries the evidence of
how it was reached, and that evidence can be printed, exported as JSON, or queried in code.

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

- **Named inputs and named intermediate results.** `Yurai.Of(1000m, "BasePrice")` brings a
  value in under your domain vocabulary; `.As("Total")` names a result after computing it.
  Names are what the explanation and the queries speak in.
- **Ordinary arithmetic.** `+ - * /` between traced values, plus `Min` and `Max`. Traced
  values also combine with plain `decimal` values on either side (`traced * 1.1m`), so
  existing formulas do not have to be rewritten to become explainable.
- **Rounding as a recorded decision.** `Round(digits, reason)` keeps the stated reason next
  to the step that changed the number — usually the most contested step in a money
  calculation.
- **The branch actually taken.** `Yurai.If` records which alternative produced the value,
  under the name you gave that decision, so "which rule fired?" is answerable.
- **Immutable evidence, shared structure.** Evidence never changes once computed, values are
  safe to share across threads, and reusing an intermediate result records its derivation
  once rather than duplicating it.

Three ways out of the evidence:

| | |
|---|---|
| `Explain()` | The human-readable derivation above — for a code review, a ticket, or a conversation with a domain expert |
| `ToJson()` | The same evidence as JSON, for systems outside the process. It is *material* for an audit trail kept by your systems, not an audit trail in itself |
| `DependsOn`, `Trace`, `Inputs` | The same evidence queried in code — assert in a test that a calculation still uses an input, or route a question by what a value depends on |

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
    var basePrice = Yurai.Of(order.BasePrice, "BasePrice");
    var total = (basePrice * (1 - Yurai.Of(order.Discount, "MemberDiscount")))
        .Round(0, "Round to whole currency unit")
        .As("Total");

    _evidenceStore.Save(order.Id, total.ToJson());

    return total.Value;
}
```

Isolating the calculation that has to be explained is a design decision, not a workaround:
the traced region stays small enough to read, and everything around it keeps its own types.

## Related work

Several projects answer nearby questions. None of them is what Yurai is, and the differences
are worth stating up front.

| Project | What it does | How Yurai differs |
|---|---|---|
| [Petit Poucet](https://github.com/liflab/petitpoucet) | Java library for fine-grained explainability: compose functions from primitives and ask which parts of the input produced a given part of the output ([CAV 2021](https://link.springer.com/chapter/10.1007/978-3-030-81688-9_24)) | .NET rather than Java, and narrower on purpose — arithmetic on domain values written as ordinary C#, rather than a general lineage relation over composed functions and arbitrary data |
| [handcalcs](https://github.com/connorferster/handcalcs) | Python library that renders calculation code as LaTeX with symbolic substitution, for notebooks and printed reports | Produces a structure the running program can query and export, not a typeset document. Yurai renders no LaTeX and no HTML |
| [Calcpad](https://calcpad.eu/) | Free program for mathematical and engineering worksheets, with its own scripting language and HTML report output | A library you call from an existing C# domain model, not a separate environment to author calculations in |
| [NCalc](https://github.com/ncalc/ncalc) | .NET evaluator for expressions supplied as strings at runtime | Calculations stay compiled C# — type-checked, refactorable, reviewable. Yurai never parses an expression string, and it answers why a value is what it is rather than what an expression evaluates to |
| [Audit.NET](https://github.com/thepirat000/Audit.NET) | .NET framework for audit trails: records operations and data changes through pluggable data providers | Audit logging records *what happened* — which operation, when, by whom. Yurai answers *why this value* — the derivation inside a single calculation. Yurai also stores nothing; it returns the evidence and stops there |

That leaves a specific gap, and it is the only claim this project makes for itself:

> No zero-dependency NuGet library exists that, using ordinary C# arithmetic syntax as-is in
> a production application, provides eagerly evaluated domain values together with queryable
> derivation evidence (a DAG) as one thing.

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

The package is not published yet. From v1.0 onward:

```shell
dotnet add package Yurai
```

Yurai targets `netstandard2.0` and has **zero runtime dependencies** — BCL only, including
the JSON export. It runs on .NET Framework 4.6.1+, .NET Core 2.0+, and every .NET release
since, without pulling anything in behind it.

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
