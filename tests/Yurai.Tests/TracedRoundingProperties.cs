using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedRoundingProperties
{
    [Fact]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-010")]
    public void RoundMatchesDecimalRoundForGeneratedValues()
    {
        Gen.Select(Gen.Decimal, Gen.Int[-2, 30]).Sample(values => AssertParity(values.Item1, values.Item2));
    }

    private static void AssertParity(decimal value, int digits)
    {
        decimal nativeResult = default;
        Exception? nativeException = Record.Exception(
            () => { nativeResult = decimal.Round(value, digits); });

        TracedValue tracedResult = default;
        Exception? tracedException = Record.Exception(
            () => { tracedResult = YuraiApi.Of(value).Round(digits, "generated rounding"); });

        if (nativeException is null)
        {
            Assert.Null(tracedException);
            TracedArithmeticTests.AssertDecimalBitsEqual(nativeResult, tracedResult.Value);
            return;
        }

        Assert.NotNull(tracedException);
        Assert.Equal(nativeException.GetType(), tracedException.GetType());
    }
}
