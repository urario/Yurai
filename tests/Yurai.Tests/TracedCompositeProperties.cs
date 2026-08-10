using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedCompositeProperties
{
    [Fact]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-005")]
    [Trait("RQ", "RQ-008")]
    [Trait("RQ", "RQ-009")]
    [Trait("RQ", "RQ-010")]
    public void CompositeCalculationMatchesEquivalentDecimalCalculation()
    {
        Gen.Select(
            Gen.Decimal,
            Gen.Decimal,
            Gen.Decimal,
            Gen.Decimal,
            Gen.Int[0, 28],
            Gen.Bool).Sample(values => AssertParity(
                values.Item1,
                values.Item2,
                values.Item3,
                values.Item4,
                values.Item5,
                values.Item6));
    }

    private static void AssertParity(
        decimal left,
        decimal right,
        decimal candidate,
        decimal divisor,
        int digits,
        bool condition)
    {
        decimal nativeResult = default;
        Exception? nativeException = Record.Exception(
            () =>
            {
                decimal selected = condition ? left : right;
                decimal minimum = Math.Min(selected, candidate);
                nativeResult = decimal.Round((minimum + divisor) / divisor, digits);
            });

        TracedValue tracedResult = default;
        Exception? tracedException = Record.Exception(
            () =>
            {
                TracedValue selected = YuraiApi.If(
                    condition,
                    () => YuraiApi.Of(left),
                    () => YuraiApi.Of(right),
                    "GeneratedDecision");
                TracedValue minimum = YuraiApi.Min(selected, YuraiApi.Of(candidate));
                tracedResult = ((minimum + divisor) / divisor).Round(digits, "generated rounding");
            });

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
