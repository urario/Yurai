# Payroll calculation

This sample calculates a monthly payroll with a base salary, overtime pay, social
insurance, progressive income tax, and explicit rounding. It uses a base salary of
300,000, 20 overtime hours at 2,500 per hour, and a social-insurance rate of 0.152345.

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

// Reading .Value turns each condition into a plain bool. Condition-only inputs
// intentionally remain outside v1 dependency queries.
var incomeTax = Yurai.If(
    taxableIncome.Value <= 200000m,
    () => taxableIncome * 0.10m,
    () => Yurai.If(
        taxableIncome.Value <= 400000m,
        () => 20000m + (taxableIncome - 200000m) * 0.20m,
        () => 60000m + (taxableIncome - 400000m) * 0.30m,
        "TaxableIncomeAtMost400000"),
    "TaxableIncomeAtMost200000")
    .Round(0, "Round income tax to whole currency units")
    .As("IncomeTax");

var totalDeductions = (socialInsurance + incomeTax).As("TotalDeductions");
var netPay = (grossPay - totalDeductions).As("NetPay");

Console.WriteLine(netPay.Explain());
Console.WriteLine($"Inputs: {string.Join(", ", netPay.Inputs)}");
foreach (IReadOnlyList<string> path in netPay.Trace("GrossPay"))
{
    Console.WriteLine(string.Join(" -> ", path));
}
```

## Expected explanation

```text
Result
  257343
Derivation
  NetPay = 257343
    Subtract = 257343
      [#3] GrossPay = 350000
        Add = 350000
          BaseSalary = 300000
          OvertimePay = 50000
            Multiply = 50000
              OvertimeHourlyRate = 2500
              OvertimeHours = 20
      TotalDeductions = 92657
        Add = 92657
          [#12] SocialInsurance = 53321
            Round(digits: 0, reason: "Round social insurance to whole currency units") = 53321
              Multiply = 53320.750000
                <ref #3>
                SocialInsuranceRate = 0.152345
          IncomeTax = 39336
            Round(digits: 0, reason: "Round income tax to whole currency units") = 39336
              If(name: "TaxableIncomeAtMost200000", branch: "else") = 39335.80
                If(name: "TaxableIncomeAtMost400000", branch: "then") = 39335.80
                  Add = 39335.80
                    20000
                    Multiply = 19335.80
                      Subtract = 96679
                        TaxableIncome = 296679
                          Subtract = 296679
                            <ref #3>
                            <ref #12>
                        200000
                      0.20
```

The outer income-tax condition is false and the nested condition is true. Unselected
alternatives are neither evaluated nor displayed. `GrossPay` and `SocialInsurance` are
shared nodes: their first expanded lines define `[#3]` and `[#12]`, and later occurrences
use the matching `<ref #N>` marker.

The arithmetic output deliberately preserves native `decimal` scale, including
`53320.750000` and `39335.80`. The conditions read `taxableIncome.Value`, so v1 does not
retain a dependency edge from a value used only by the plain-Boolean condition.

## Expected dependency queries

```text
Inputs: BaseSalary, OvertimeHourlyRate, OvertimeHours, SocialInsuranceRate
GrossPay -> NetPay
GrossPay -> SocialInsurance -> TotalDeductions -> NetPay
GrossPay -> TaxableIncome -> IncomeTax -> TotalDeductions -> NetPay
GrossPay -> SocialInsurance -> TaxableIncome -> IncomeTax -> TotalDeductions -> NetPay
```

`GrossPay` is shared by the final subtraction, social-insurance calculation, and taxable
income calculation. `Trace` preserves all four routes rather than collapsing the shared
node to its first occurrence.

## API ergonomics observations

- Ordinary arithmetic keeps the payroll formula close to its domain wording.
- Naming intermediate results makes the explanation easier to scan around deductions
  and taxable income.
- Nested `Yurai.If` records only the selected branches.
- `Round(digits, reason)` keeps the policy explanation beside the operation.
- Dependency queries expose shared calculation routes without parsing the explanation.
