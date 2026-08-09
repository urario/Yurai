using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedArithmeticTests
{
    public static TheoryData<string, decimal, decimal, decimal> SuccessfulOperations => new()
    {
        { "Add", 1.20m, 3.400m, 4.600m },
        { "Subtract", -5.25m, 2.5m, -7.75m },
        { "Multiply", 2.50m, 4.0m, 10.000m },
        { "Divide", 10.00m, 4m, 2.50m },
    };

    [Theory]
    [MemberData(nameof(SuccessfulOperations))]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-008")]
    public void OperatorMatchesDecimalAndCreatesAParentWithSharedOperands(
        string operation,
        decimal leftValue,
        decimal rightValue,
        decimal expected)
    {
        TracedValue left = YuraiApi.Of(leftValue, "Left");
        TracedValue right = YuraiApi.Of(rightValue, "Right");

        TracedValue result = Apply(operation, left, right);

        AssertDecimalBitsEqual(expected, result.Value);
        var node = Assert.IsType<BinaryOperationEvidenceNode>(result.Root);
        Assert.Equal(Enum.Parse<BinaryOperationKind>(operation), node.Operation);
        Assert.Same(left.Root, node.Left);
        Assert.Same(right.Root, node.Right);
        Assert.Same(left.Root, left.Root);
        Assert.Same(right.Root, right.Root);
    }

    [Fact]
    [Trait("RQ", "RQ-001")]
    public void DivisionByZeroMatchesDecimalExceptionAndLeavesOperandsUsable()
    {
        TracedValue left = YuraiApi.Of(1m, "Left");
        TracedValue right = YuraiApi.Of(0m, "Right");
        decimal zero = 0m;

        Exception native = Assert.Throws<DivideByZeroException>(() => { _ = 1m / zero; });
        Exception traced = Assert.Throws<DivideByZeroException>(() => { _ = left / right; });

        Assert.Equal(native.GetType(), traced.GetType());
        Assert.Equal(1m, left.Value);
        Assert.Equal(0m, right.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-001")]
    public void OverflowMatchesDecimalExceptionAndLeavesOperandsUsable()
    {
        TracedValue left = YuraiApi.Of(decimal.MaxValue, "Left");
        TracedValue right = YuraiApi.Of(1m, "Right");
        decimal one = 1m;

        Exception native = Assert.Throws<OverflowException>(() => { _ = decimal.MaxValue + one; });
        Exception traced = Assert.Throws<OverflowException>(() => { _ = left + right; });

        Assert.Equal(native.GetType(), traced.GetType());
        Assert.Equal(decimal.MaxValue, left.Value);
        Assert.Equal(1m, right.Value);
    }

    [Fact]
    public void OperatorsRejectDefaultOperandsFromLeftToRight()
    {
        TracedValue initialized = YuraiApi.Of(1m);
        TracedValue uninitialized = default;

        Assert.Throws<InvalidOperationException>(() => { _ = uninitialized + initialized; });
        Assert.Throws<InvalidOperationException>(() => { _ = initialized + uninitialized; });
    }

    internal static TracedValue Apply(string operation, TracedValue left, TracedValue right) =>
        operation switch
        {
            "Add" => left + right,
            "Subtract" => left - right,
            "Multiply" => left * right,
            "Divide" => left / right,
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };

    internal static decimal Apply(string operation, decimal left, decimal right) => operation switch
    {
        "Add" => left + right,
        "Subtract" => left - right,
        "Multiply" => left * right,
        "Divide" => left / right,
        _ => throw new ArgumentOutOfRangeException(nameof(operation)),
    };

    internal static void AssertDecimalBitsEqual(decimal expected, decimal actual) =>
        Assert.Equal(decimal.GetBits(expected), decimal.GetBits(actual));
}
