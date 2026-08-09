using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedArithmeticProperties
{
    [Theory]
    [InlineData("Add")]
    [InlineData("Subtract")]
    [InlineData("Multiply")]
    [InlineData("Divide")]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-001")]
    public void OperatorMatchesDecimalForGeneratedOperands(string operation)
    {
        Gen.Select(Gen.Decimal, Gen.Decimal).Sample(values => AssertParity(operation, values.Item1, values.Item2));
    }

    private static void AssertParity(string operation, decimal left, decimal right)
    {
        decimal nativeResult = default;
        Exception? nativeException = Record.Exception(
            () => { nativeResult = TracedArithmeticTests.Apply(operation, left, right); });

        TracedValue tracedResult = default;
        Exception? tracedException = Record.Exception(
            () => { tracedResult = TracedArithmeticTests.Apply(operation, YuraiApi.Of(left), YuraiApi.Of(right)); });

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
