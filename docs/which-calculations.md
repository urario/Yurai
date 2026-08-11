# Which calculations should use Yurai?

Yurai is for a bounded domain calculation whose result may need to be explained later.
It keeps an eagerly evaluated `decimal` value together with immutable derivation evidence,
so the code can calculate normally and still answer questions about the recorded result.

The useful boundary is usually a domain calculation, not an entire application.
Choose the calculation that has a domain name, a rule a person may question, or a result
that needs a dependency or derivation query after it has been computed.

## Good candidates

Use Yurai when most of these statements are true:

- The calculation represents a domain rule such as a price, deduction, tax, benefit amount,
  premium, score, or other policy-driven result.
- Someone may need to understand which named inputs, intermediate results, rounding steps,
  or selected branches produced the result.
- The calculation is a bounded region with a manageable number of domain steps. Yurai's
  intended everyday scale is tens of calculation steps, as described in the
  [performance baseline](performance.md).
- The result benefits from one of the existing exits: `Explain()`, `ToJson()`,
  `DependsOn`, `Inputs`, or `Trace`.

Pricing and payroll are representative examples. They contain named business inputs,
rounding decisions, and branches that are meaningful when a result is questioned. The
existing [Pricing sample](../samples/Pricing/README.md) and
[Payroll sample](../samples/Payroll/README.md) show the complete expressions and their
actual output.

## Calculations to leave plain

Keep a calculation as ordinary `decimal` code when its derivation is not useful to a
caller or reviewer. Examples include:

- incidental arithmetic used only to arrange data or format a response;
- large, unbounded, or high-throughput loops where retaining evidence for every operation
  would cost more than the explanation is worth;
- calculations whose real requirement is symbolic manipulation or another form of
  analysis rather than a concrete recorded derivation;
- application-wide data lineage, security taint tracking, or an audit platform with
  storage, signing, timestamps, and retention policies.

Yurai records the concrete derivation of the region you choose. It does not turn every
value in the process into a tracked value, and it does not claim that the resulting JSON
or text is a complete compliance record by itself.

## Isolate the explainable domain calculation

Keep input mapping and application orchestration in their existing types. Introduce
`Traced` values at the calculation boundary, name the domain inputs and result, and take
the plain `decimal` back out when the calculation is complete:

```csharp
using System;
using Yurai;

public static class PricingCalculator
{
    public static decimal CalculateTotal(
        decimal basePriceValue,
        decimal discountValue,
        Action<string> saveEvidence)
    {
        var basePrice = Traced.Of(basePriceValue, "BasePrice");
        var discount = Traced.Of(discountValue, "MemberDiscount");
        var total = (basePrice * (1 - discount))
            .Round(0, "Round to whole currency unit")
            .As("Total");

        saveEvidence(total.ToJson());
        return total.Value;
    }
}
```

The traced region is now local to the domain calculation. Callers receive the same plain
`decimal` type they used before, while code inside the boundary can call `Explain()`,
`ToJson()`, or dependency queries before the boundary is crossed. This example saves the
JSON evidence before returning `total.Value`; if the evidence is not needed, omit that
step. After `Value` has been returned as a plain `decimal`, the caller cannot recover the
`Traced` evidence from that decimal. The callback and storage policy belong to the caller,
not to Yurai.

This boundary is intentional. Propagating `Traced` through every application layer would
make unrelated APIs depend on evidence details and can encourage unrelated traced
operations, which may enlarge graphs without a clear question to answer. It also makes it
harder to see which calculation is being explained. Use a small, named region instead of
treating tracing as a property that must spread through the whole object graph.

## Branch conditions are an explicit boundary

The current API accepts a plain `bool` in `Traced.If`. To form that condition, read the
value at the point where the domain rule makes its decision. This minimal example uses
`CustomerTier` only for the condition, not in either selected value calculation:

```csharp
var customerTier = Traced.Of(2m, "CustomerTier");

var discount = Traced.If(
    customerTier.Value >= 2m,
    () => Traced.Of(0.10m),
    () => Traced.Of(0m),
    "MemberDiscountRule");
var total = (Traced.Of(100m, "BasePrice") * (1 - discount)).As("Total");

bool hasCustomerTierDependency = total.DependsOn("CustomerTier"); // false
```

The selected branch is recorded, but `CustomerTier` is not represented as a dependency
edge because it was used only to produce the plain Boolean condition. This is why the
boundary should be explicit in the calculation design, and why a dependency query should
be read as a query over recorded value derivation rather than over all control flow.

## A practical rule

Start with the smallest calculation that a domain expert might ask you to explain. Name
its inputs and important intermediate results, preserve the existing plain-value boundary,
and expand the traced region only when a concrete explanation or dependency question
requires it. If the question is about the whole application rather than one domain result,
Yurai is the wrong boundary for that question.
