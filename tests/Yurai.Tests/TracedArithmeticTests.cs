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
        EvidenceNode originalLeftRoot = left.Root;
        EvidenceNode originalRightRoot = right.Root;

        TracedValue result = Apply(operation, left, right);

        AssertDecimalBitsEqual(expected, result.Value);
        var node = Assert.IsType<BinaryOperationEvidenceNode>(result.Root);
        Assert.Equal(Enum.Parse<BinaryOperationKind>(operation), node.Operation);
        Assert.Same(originalLeftRoot, node.Left);
        Assert.Same(originalRightRoot, node.Right);
        Assert.Same(originalLeftRoot, left.Root);
        Assert.Same(originalRightRoot, right.Root);
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

    [Theory]
    [InlineData("Add")]
    [InlineData("Subtract")]
    [InlineData("Multiply")]
    [Trait("RQ", "RQ-001")]
    public void OverflowMatchesDecimalExceptionAndLeavesOperandsUsable(string operation)
    {
        decimal leftValue = operation == "Subtract" ? decimal.MinValue : decimal.MaxValue;
        decimal rightValue = operation == "Multiply" ? 2m : 1m;
        TracedValue left = YuraiApi.Of(leftValue, "Left");
        TracedValue right = YuraiApi.Of(rightValue, "Right");
        decimal one = 1m;
        decimal two = 2m;

        Exception native = Assert.Throws<OverflowException>(() =>
        {
            _ = operation switch
            {
                "Add" => decimal.MaxValue + one,
                "Subtract" => decimal.MinValue - one,
                "Multiply" => decimal.MaxValue * two,
                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };
        });
        Exception traced = Assert.Throws<OverflowException>(() => { _ = Apply(operation, left, right); });

        Assert.Equal(native.GetType(), traced.GetType());
        Assert.Equal(leftValue, left.Value);
        Assert.Equal(rightValue, right.Value);
    }

    [Theory]
    [InlineData("Add")]
    [InlineData("Subtract")]
    [InlineData("Multiply")]
    [InlineData("Divide")]
    public void OperatorsRejectDefaultOperandsFromLeftToRight(string operation)
    {
        TracedValue initialized = YuraiApi.Of(1m);
        TracedValue uninitialized = default;

        Assert.Throws<InvalidOperationException>(() => { _ = Apply(operation, uninitialized, initialized); });
        Assert.Throws<InvalidOperationException>(() => { _ = Apply(operation, initialized, uninitialized); });
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
