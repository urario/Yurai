# Pricing calculation

This conceptual sample explores the intended Yurai API before the library is
implemented. It is deliberately not compiled. The final public API and text format may
change during the architecture work in issues
[#17](https://github.com/urario/Yurai/issues/17) and
[#18](https://github.com/urario/Yurai/issues/18).

An online store applies a ten-percent member discount to a base price, adds ten-percent
tax, and rounds the total to a whole monetary unit. The names use the domain vocabulary
that should appear in the explanation.

```csharp
var basePrice = Yurai.Of(1000m, "BasePrice");
var discount = Yurai.Of(0.10m, "MemberDiscount");
var taxRate = Yurai.Of(0.10m, "TaxRate");

var discounted = (basePrice * (1 - discount)).As("DiscountedPrice");
var total = (discounted * (1 + taxRate))
    .Round(0, "Round to whole currency unit")
    .As("Total");

Console.WriteLine(total.Explain());
```

The discount produces `900.00`, applying tax produces `990.0000`, and rounding to zero
digits produces `990`. The decimal scales in this sketch make the individual arithmetic
steps visible.

## Expected explanation

```text
Total = 990
└─ Round(digits: 0, reason: "Round to whole currency unit") = 990
   └─ Multiply = 990.0000
      ├─ DiscountedPrice = 900.00
      │  └─ Multiply = 900.00
      │     ├─ BasePrice = 1000
      │     └─ Subtract = 0.90
      │        ├─ 1
      │        └─ MemberDiscount = 0.10
      └─ Add = 1.10
         ├─ 1
         └─ TaxRate = 0.10
```

This output fixes the information that the explanation must communicate: the final
value, named inputs and intermediate values, arithmetic operations, rounding parameters,
and the rounding reason. The implementation uses invariant formatting, two-space
indentation, and document-local references for shared nodes. The final MVP framing is:

```text
Result
  990
Derivation
  Total = 990
    Round(digits: 0, reason: "Round to whole currency unit") = 990
      Multiply = 990.0000
        DiscountedPrice = 900.00
          Multiply = 900.00
            BasePrice = 1000
            Subtract = 0.90
              1
              MemberDiscount = 0.10
        Add = 1.10
          1
          TaxRate = 0.10
```

The literals in `1 - discount` and `1 + taxRate` are shown by invariant-formatted value
without a generated name. The midpoint rounding mode is `ToEven`; it does not affect
this example because `990.0000` is already integral.
