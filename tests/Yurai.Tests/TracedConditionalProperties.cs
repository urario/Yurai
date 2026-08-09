using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedConditionalProperties
{
    [Fact]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-001")]
    public void IfMatchesTheConditionalOperatorForGeneratedValues()
    {
        Gen.Select(Gen.Select(Gen.Decimal, Gen.Decimal), Gen.Bool).Sample(values =>
        {
            decimal whenTrue = values.Item1.Item1;
            decimal whenFalse = values.Item1.Item2;
            bool condition = values.Item2;
            decimal expected = condition ? whenTrue : whenFalse;

            TracedValue actual = YuraiApi.If(
                condition,
                () => YuraiApi.Of(whenTrue),
                () => YuraiApi.Of(whenFalse),
                "GeneratedDecision");

            TracedArithmeticTests.AssertDecimalBitsEqual(expected, actual.Value);
        });
    }
}
