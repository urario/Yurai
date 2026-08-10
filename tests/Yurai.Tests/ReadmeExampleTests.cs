using Xunit;
using Yurai;

// Deliberately outside the Yurai namespace, and deliberately without the type alias the
// other test files use. A reader of the README writes `using Yurai;` from their own
// namespace, so that is the name resolution these tests have to exercise: if the examples
// below stop compiling, the README stops being copy-pasteable. Keeping this file out of
// Yurai.Tests is what makes it a regression test for ADR-0017.
namespace YuraiReadme.Consumer;

public sealed class ReadmeExampleTests
{
    [Fact]
    [Trait("RQ", "RQ-002")]
    [Trait("RQ", "RQ-012")]
    public void ReadmeOpeningExampleExplainsExactlyAsDocumented()
    {
        var basePrice = Traced.Of(1000m, "BasePrice");
        var discount = Traced.Of(0.10m, "MemberDiscount");
        var taxRate = Traced.Of(0.10m, "TaxRate");
        var total = (basePrice * (1 - discount) * (1 + taxRate))
            .Round(0, "Round to whole currency unit")
            .As("Total");

        string expected = string.Join(
            "\n",
            "Result",
            "  990",
            "Derivation",
            "  Total = 990",
            "    Round(digits: 0, reason: \"Round to whole currency unit\") = 990",
            "      Multiply = 990.0000",
            "        Multiply = 900.00",
            "          BasePrice = 1000",
            "          Subtract = 0.90",
            "            1",
            "            MemberDiscount = 0.10",
            "        Add = 1.10",
            "          1",
            "          TaxRate = 0.10");

        Assert.Equal(expected, total.Explain());
        Assert.Equal(990m, total.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-014")]
    public void ReadmeDependencyQueryExampleReturnsTheDocumentedNamesAndPath()
    {
        var subtotal = (Traced.Of(100m, "BasePrice") - Traced.Of(10m, "Discount"))
            .As("Subtotal");
        var total = (subtotal + 5m).As("Total");

        bool stillUsesBasePrice = total.DependsOn("BasePrice");
        IReadOnlyList<string> inputs = total.Inputs;
        IReadOnlyList<IReadOnlyList<string>> paths = total.Trace("BasePrice");

        Assert.True(stillUsesBasePrice);
        Assert.Equal(["BasePrice", "Discount"], inputs);
        Assert.Equal(["BasePrice", "Subtotal", "Total"], Assert.Single(paths));
    }
}
