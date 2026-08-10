using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;

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

    [Theory]
    [InlineData("Add")]
    [InlineData("Subtract")]
    [InlineData("Multiply")]
    [InlineData("Divide")]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-009")]
    public void MixedOperatorMatchesDecimalForGeneratedOperands(string operation)
    {
        Gen.Select(Gen.Decimal, Gen.Decimal).Sample(values =>
        {
            AssertMixedParity(operation, values.Item1, values.Item2, tracedOnLeft: true);
            AssertMixedParity(operation, values.Item1, values.Item2, tracedOnLeft: false);
        });
    }

    [Theory]
    [InlineData("Min")]
    [InlineData("Max")]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-008")]
    public void SelectionMatchesDecimalForGeneratedOperands(string operation)
    {
        Gen.Select(Gen.Decimal, Gen.Decimal).Sample(values =>
        {
            decimal left = values.Item1;
            decimal right = values.Item2;
            decimal expected = operation == "Min" ? Math.Min(left, right) : Math.Max(left, right);
            TracedValue actual = operation == "Min"
                ? TracedValue.Min(TracedValue.Of(left), TracedValue.Of(right))
                : TracedValue.Max(TracedValue.Of(left), TracedValue.Of(right));

            TracedArithmeticTests.AssertDecimalBitsEqual(expected, actual.Value);
        });
    }

    private static void AssertParity(string operation, decimal left, decimal right)
    {
        decimal nativeResult = default;
        Exception? nativeException = Record.Exception(
            () => { nativeResult = TracedArithmeticTests.Apply(operation, left, right); });

        TracedValue tracedResult = default;
        Exception? tracedException = Record.Exception(
            () => { tracedResult = TracedArithmeticTests.Apply(operation, TracedValue.Of(left), TracedValue.Of(right)); });

        if (nativeException is null)
        {
            Assert.Null(tracedException);
            TracedArithmeticTests.AssertDecimalBitsEqual(nativeResult, tracedResult.Value);
            return;
        }

        Assert.NotNull(tracedException);
        Assert.Equal(nativeException.GetType(), tracedException.GetType());
    }

    private static void AssertMixedParity(string operation, decimal plain, decimal traced, bool tracedOnLeft)
    {
        decimal nativeResult = default;
        Exception? nativeException = Record.Exception(
            () => { nativeResult = tracedOnLeft ? TracedArithmeticTests.Apply(operation, traced, plain) : TracedArithmeticTests.Apply(operation, plain, traced); });

        TracedValue tracedResult = default;
        Exception? tracedException = Record.Exception(
            () =>
            {
                TracedValue value = TracedValue.Of(traced);
                tracedResult = tracedOnLeft
                    ? TracedArithmeticTests.ApplyMixed(operation, value, plain)
                    : TracedArithmeticTests.ApplyMixed(operation, plain, value);
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
