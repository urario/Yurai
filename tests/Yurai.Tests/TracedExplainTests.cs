using System.Globalization;
using Xunit;
using TracedValue = global::Yurai.Traced;

namespace Yurai.Tests;

public sealed class TracedExplainTests
{
    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainMatchesThePricingSample()
    {
        TracedValue basePrice = TracedValue.Of(1000m, "BasePrice");
        TracedValue discount = TracedValue.Of(0.10m, "MemberDiscount");
        TracedValue taxRate = TracedValue.Of(0.10m, "TaxRate");

        TracedValue discounted = (basePrice * (1 - discount)).As("DiscountedPrice");
        TracedValue total = (discounted * (1 + taxRate))
            .Round(0, "Round to whole currency unit")
            .As("Total");

        string expected = Lines(
            "Result",
            "  990",
            "Derivation",
            "  Total = 990",
            "    Round(digits: 0, reason: \"Round to whole currency unit\") = 990",
            "      Multiply = 990.0000",
            "        DiscountedPrice = 900.00",
            "          Multiply = 900.00",
            "            BasePrice = 1000",
            "            Subtract = 0.90",
            "              1",
            "              MemberDiscount = 0.10",
            "        Add = 1.10",
            "          1",
            "          TaxRate = 0.10");

        Assert.Equal(expected, total.Explain());
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainMatchesThePayrollSampleAndShowsTheSelectedBranches()
    {
        TracedValue baseSalary = TracedValue.Of(300000m, "BaseSalary");
        TracedValue overtimeHourlyRate = TracedValue.Of(2500m, "OvertimeHourlyRate");
        TracedValue overtimeHours = TracedValue.Of(20m, "OvertimeHours");
        TracedValue socialInsuranceRate = TracedValue.Of(0.152345m, "SocialInsuranceRate");

        TracedValue overtimePay = (overtimeHourlyRate * overtimeHours).As("OvertimePay");
        TracedValue grossPay = (baseSalary + overtimePay).As("GrossPay");
        TracedValue socialInsurance = (grossPay * socialInsuranceRate)
            .Round(0, "Round social insurance to whole currency units")
            .As("SocialInsurance");
        TracedValue taxableIncome = (grossPay - socialInsurance).As("TaxableIncome");
        TracedValue incomeTax = TracedValue.If(
            taxableIncome.Value <= 200000m,
            () => taxableIncome * 0.10m,
            () => TracedValue.If(
                taxableIncome.Value <= 400000m,
                () => 20000m + (taxableIncome - 200000m) * 0.20m,
                () => 60000m + (taxableIncome - 400000m) * 0.30m,
                "TaxableIncomeAtMost400000"),
            "TaxableIncomeAtMost200000")
            .Round(0, "Round income tax to whole currency units")
            .As("IncomeTax");
        TracedValue totalDeductions = (socialInsurance + incomeTax).As("TotalDeductions");
        TracedValue netPay = (grossPay - totalDeductions).As("NetPay");

        string expected = Lines(
            "Result",
            "  257343",
            "Derivation",
            "  NetPay = 257343",
            "    Subtract = 257343",
            "      [#3] GrossPay = 350000",
            "        Add = 350000",
            "          BaseSalary = 300000",
            "          OvertimePay = 50000",
            "            Multiply = 50000",
            "              OvertimeHourlyRate = 2500",
            "              OvertimeHours = 20",
            "      TotalDeductions = 92657",
            "        Add = 92657",
            "          [#12] SocialInsurance = 53321",
            "            Round(digits: 0, reason: \"Round social insurance to whole currency units\") = 53321",
            "              Multiply = 53320.750000",
            "                <ref #3>",
            "                SocialInsuranceRate = 0.152345",
            "          IncomeTax = 39336",
            "            Round(digits: 0, reason: \"Round income tax to whole currency units\") = 39336",
            "              If(name: \"TaxableIncomeAtMost200000\", branch: \"else\") = 39335.80",
            "                If(name: \"TaxableIncomeAtMost400000\", branch: \"then\") = 39335.80",
            "                  Add = 39335.80",
            "                    20000",
            "                    Multiply = 19335.80",
            "                      Subtract = 96679",
            "                        TaxableIncome = 296679",
            "                          Subtract = 296679",
            "                            <ref #3>",
            "                            <ref #12>",
            "                        200000",
            "                      0.20");

        Assert.Equal(expected, netPay.Explain());
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainExpandsSharedNodesOnceAndUsesDocumentLocalReferences()
    {
        TracedValue shared = TracedValue.Of(10m, "Shared");
        TracedValue result = (shared + shared).As("Total");

        string expected = Lines(
            "Result",
            "  20",
            "Derivation",
            "  Total = 20",
            "    Add = 20",
            "      [#3] Shared = 10",
            "      <ref #3>");
        string explanation = result.Explain();

        Assert.Equal(expected, explanation);
        Assert.Equal(explanation, result.Explain());
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public async Task ExplainIsSafeForConcurrentReads()
    {
        TracedValue shared = TracedValue.Of(10m, "Shared");
        TracedValue result = (shared * 2m).As("Total");

        Task<string>[] reads = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(result.Explain))
            .ToArray();

        string[] explanations = await Task.WhenAll(reads);

        Assert.All(explanations, explanation => Assert.Equal(explanations[0], explanation));
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainRendersEveryEvidenceNodeKindAndEscapesMetadata()
    {
        TracedValue input = TracedValue.Of(2.5m, "Input\"Name");
        TracedValue rounded = input.Round(0, "Reason\\line\nnext");
        TracedValue branch = TracedValue.If(true, () => rounded, () => TracedValue.Of(0m), "Decision");
        TracedValue result = TracedValue.Max(
            TracedValue.Min(branch, TracedValue.Of(3m, "Other")),
            TracedValue.Of(1m, "Last"));

        string explanation = result.Explain();

        Assert.Contains("Max = 2", explanation, StringComparison.Ordinal);
        Assert.Contains("Min = 2", explanation, StringComparison.Ordinal);
        Assert.Contains("If(name: \"Decision\", branch: \"then\") = 2", explanation, StringComparison.Ordinal);
        Assert.Contains("Round(digits: 0, reason: \"Reason\\\\line\\nnext\") = 2", explanation, StringComparison.Ordinal);
        Assert.Contains("Input\"Name = 2.5", explanation, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainEscapesControlCharactersInQuotedAndUnquotedMetadata()
    {
        TracedValue input = TracedValue.Of(2.5m, "Input\\\"Name\r\n\t\u0001");
        TracedValue rounded = input.Round(0, "Reason\\\"line\r\n\t\u0002");
        TracedValue branch = TracedValue.If(
            true,
            () => rounded,
            () => TracedValue.Of(0m),
            "Decision\\\"Name\r\n\t\u0003");
        TracedValue result = branch.As("Result\\\"Name\r\n\t\u0004");

        string expected = Lines(
            "Result",
            "  2",
            "Derivation",
            "  Result\\\"Name\\r\\n\\t\\u0004 = 2",
            "    If(name: \"Decision\\\\\\\"Name\\r\\n\\t\\u0003\", branch: \"then\") = 2",
            "      Round(digits: 0, reason: \"Reason\\\\\\\"line\\r\\n\\t\\u0002\") = 2",
            "        Input\\\"Name\\r\\n\\t\\u0001 = 2.5");

        Assert.Equal(expected, result.Explain());
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainUsesInvariantCultureAndHandlesDeepChains()
    {
        TracedValue result = TracedValue.Of(1.25m, "Value");
        for (int index = 0; index < 10_001; index++)
        {
            result = result.As($"Node{index}");
        }

        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            string explanation = result.Explain();

            Assert.Contains("1.25", explanation, StringComparison.Ordinal);
            Assert.DoesNotContain("1,25", explanation, StringComparison.Ordinal);
            Assert.DoesNotContain('\r', explanation);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ExplainReturnsADiagnosticForAnUninitializedCarrier()
    {
        Assert.Equal("Uninitialized Traced", default(TracedValue).Explain());
    }

    private static string Lines(params string[] lines) => string.Join("\n", lines);
}
