# Payroll calculation

This conceptual sample explores a payroll calculation with a base salary,
overtime pay, social insurance, progressive income tax, and rounding. It is
deliberately not compiled because the public API is still being designed in
issues [#17](https://github.com/urario/Yurai/issues/17) and
[#18](https://github.com/urario/Yurai/issues/18).

The example uses a monthly base salary of 300,000, 20 overtime hours at 2,500
per hour, and a social-insurance rate of 0.152345. Income tax has three
progressive brackets: 10% up to 200,000, 20% from 200,000 to 400,000, and 30%
above 400,000.

```csharp
var baseSalary = Yurai.Of(300000m, "BaseSalary");
var overtimeHourlyRate = Yurai.Of(2500m, "OvertimeHourlyRate");
var overtimeHours = Yurai.Of(20m, "OvertimeHours");
var socialInsuranceRate = Yurai.Of(0.152345m, "SocialInsuranceRate");

var overtimePay = (overtimeHourlyRate * overtimeHours).As("OvertimePay");
var grossPay = (baseSalary + overtimePay).As("GrossPay");

var socialInsurance = (grossPay * socialInsuranceRate)
    .Round(0, "Round social insurance to whole currency units")
    .As("SocialInsurance");
var taxableIncome = (grossPay - socialInsurance).As("TaxableIncome");

// The condition/value access and the value-vs-lambda form are illustrative.
// Issue #18 Q3 decides the final Yurai.If contract and evaluation behavior.
// Reading .Value also turns the condition into a plain bool; Q13 decides whether
// condition-only inputs remain outside v1 dependency queries.
var incomeTax = Yurai.If(
    taxableIncome.Value <= 200000m,
    taxableIncome * 0.10m,
    Yurai.If(
        taxableIncome.Value <= 400000m,
        20000m + (taxableIncome - 200000m) * 0.20m,
        60000m + (taxableIncome - 400000m) * 0.30m,
        "TaxableIncomeAtMost400000"),
    "TaxableIncomeAtMost200000")
    .Round(0, "Round income tax to whole currency units")
    .As("IncomeTax");

var totalDeductions = (socialInsurance + incomeTax).As("TotalDeductions");
var netPay = (grossPay - totalDeductions).As("NetPay");

Console.WriteLine(netPay.Explain());

Console.WriteLine(socialInsurance.DependsOn("SocialInsuranceRate"));
Console.WriteLine(socialInsurance.Trace("SocialInsuranceRate"));
```

The arithmetic checkpoints intentionally preserve the native `decimal` scale
of the unrounded intermediate values. The trailing zeros are part of this
sample's expected numeric evidence; the final rendering policy remains design
work.

The arithmetic checkpoints are:

| Result | Value |
| --- | ---: |
| `OvertimePay` | `50,000` |
| `GrossPay` | `350,000` |
| Social insurance before rounding | `53,320.750000` |
| `SocialInsurance` | `53,321` |
| `TaxableIncome` | `296,679` |
| Income tax before rounding | `39,335.80` |
| `IncomeTax` | `39,336` |
| `NetPay` | `257,343` |

## Expected explanation

The exact punctuation and indentation remain design work. The explanation must
make the following facts visible:

```text
NetPay = 257343
  TotalDeductions = 92657
    SocialInsurance = 53321
      Round(digits: 0, reason: "Round social insurance to whole currency units") = 53321
        Multiply = 53320.750000
          GrossPay = 350000
          SocialInsuranceRate = 0.152345
    IncomeTax = 39336
      Round(digits: 0, reason: "Round income tax to whole currency units") = 39336
        If(name: "TaxableIncomeAtMost200000", branch: "else")
          If(name: "TaxableIncomeAtMost400000", branch: "then") = 39335.80
            TaxableIncome = 296679
  <reference to GrossPay = 350000>
```

The outer income-tax condition is false and the nested condition is true for
this input. The explanation therefore records both decisions and the selected
tax calculation. Whether an implementation evaluates or displays unselected
alternatives is intentionally left to the Q3 decision.

`GrossPay` is expanded under `SocialInsurance` and then encountered again as the final
subtraction's other operand. The second occurrence above is conceptually a reference,
not a second expansion. Its exact marker and its relationship to JSON node IDs are the
Q14 decision.

The conditions in this sketch read `taxableIncome.Value` before calling `Yurai.If`.
That produces a plain `bool`, so the current evidence model cannot retain a dependency
edge from an input used only by the condition. Q13 decides whether v1 documents and
tests that boundary or adds a traced-predicate capability.

The dependency query is expected to report `true` and a conceptual path such
as:

```text
SocialInsuranceRate -> Multiply -> Round -> SocialInsurance
```

The direction and return type of `Trace` are also provisional until the API
design is settled.

## API ergonomics observations

- Ordinary arithmetic keeps the payroll formula close to its domain wording.
- Naming each intermediate result makes the explanation easier to scan than a
  single expression, especially around deductions and taxable income.
- Nested `Yurai.If` expresses progressive brackets, but the final contract must
  clarify whether conditions use `.Value`, whether alternatives are lazy, and
  how the selected branch is named. It must also state whether condition-only inputs
  participate in dependency queries.
- `Round(digits, reason)` keeps the reason beside the policy decision, while the
  default midpoint treatment still needs to be fixed by the architecture work.
- `DependsOn` is easy to read for a direct deduction-rate relationship; the
  final API should document `Trace` direction and behavior when multiple paths
  exist.
