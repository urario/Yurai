using System.Globalization;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedExplainTests
{
    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainMatchesThePricingSample()
    {
        TracedValue basePrice = YuraiApi.Of(1000m, "BasePrice");
        TracedValue discount = YuraiApi.Of(0.10m, "MemberDiscount");
        TracedValue taxRate = YuraiApi.Of(0.10m, "TaxRate");

        TracedValue discounted = (basePrice * (1 - discount)).As("DiscountedPrice");
        TracedValue total = (discounted * (1 + taxRate))
            .Round(0, "Round to whole currency unit")
            .As("Total");

        string expected = string.Join(
            Environment.NewLine,
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

        Assert.Equal(expected,
            total.Explain());
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainMatchesThePayrollSampleAndShowsTheSelectedBranches()
    {
        TracedValue grossPay = YuraiApi.Of(350000m, "GrossPay");
        TracedValue taxableIncome = YuraiApi.Of(296679m, "TaxableIncome");
        TracedValue incomeTax = YuraiApi.If(
            false,
            () => taxableIncome * 0.10m,
            () => YuraiApi.If(
                true,
                () => 20000m + (taxableIncome - 200000m) * 0.20m,
                () => 60000m + (taxableIncome - 400000m) * 0.30m,
                "TaxableIncomeAtMost400000"),
            "TaxableIncomeAtMost200000")
            .Round(0, "Round income tax to whole currency units")
            .As("IncomeTax");

        TracedValue result = (grossPay - incomeTax).As("NetPay");

        Assert.Contains(
            "If(name: \"TaxableIncomeAtMost200000\", branch: \"else\") = 39335.80",
            result.Explain(),
            StringComparison.Ordinal);
        Assert.Contains(
            "If(name: \"TaxableIncomeAtMost400000\", branch: \"then\") = 39335.80",
            result.Explain(),
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainExpandsSharedNodesOnceAndUsesDocumentLocalReferences()
    {
        TracedValue shared = YuraiApi.Of(10m, "Shared");
        TracedValue result = (shared + shared).As("Total");

        string explanation = result.Explain();

        Assert.Equal(1, CountOccurrences(explanation, "Shared = 10"));
        Assert.Contains("<ref #", explanation, StringComparison.Ordinal);
        Assert.Equal(explanation, result.Explain());
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public async Task ExplainIsSafeForConcurrentReads()
    {
        TracedValue shared = YuraiApi.Of(10m, "Shared");
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
        TracedValue input = YuraiApi.Of(2.5m, "Input\"Name");
        TracedValue rounded = input.Round(0, "Reason\\line\nnext");
        TracedValue branch = YuraiApi.If(true, () => rounded, () => YuraiApi.Of(0m), "Decision");
        TracedValue result = YuraiApi.Max(
            YuraiApi.Min(branch, YuraiApi.Of(3m, "Other")),
            YuraiApi.Of(1m, "Last"));

        string explanation = result.Explain();

        Assert.Contains("Max = 2", explanation, StringComparison.Ordinal);
        Assert.Contains("Min = 2", explanation, StringComparison.Ordinal);
        Assert.Contains("If(name: \"Decision\", branch: \"then\") = 2", explanation, StringComparison.Ordinal);
        Assert.Contains("Round(digits: 0, reason: \"Reason\\\\line\\nnext\") = 2", explanation, StringComparison.Ordinal);
        string escapedName = "Input" + "\\" + "\"Name = 2.5";
        Assert.Contains(escapedName, explanation, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ExplainUsesInvariantCultureAndHandlesDeepChains()
    {
        TracedValue result = YuraiApi.Of(1.25m, "Value");
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

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
