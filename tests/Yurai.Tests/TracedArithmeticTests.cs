using Xunit;
using TracedValue = global::Yurai.Traced;

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
        TracedValue left = TracedValue.Of(leftValue, "Left");
        TracedValue right = TracedValue.Of(rightValue, "Right");
        EvidenceNode originalLeftRoot = left.Root;
        EvidenceNode originalRightRoot = right.Root;

        TracedValue result = Apply(operation, left, right);

        AssertDecimalBitsEqual(expected, result.Value);
        var node = Assert.IsType<BinaryOperationEvidenceNode>(result.Root);
        Assert.Equal(Enum.Parse<BinaryOperationKind>(operation), node.Operation);
        Assert.Equal(SelectedOperand.None, node.SelectedOperand);
        Assert.Same(originalLeftRoot, node.Left);
        Assert.Same(originalRightRoot, node.Right);
        Assert.Same(originalLeftRoot, left.Root);
        Assert.Same(originalRightRoot, right.Root);
    }

    [Fact]
    [Trait("RQ", "RQ-001")]
    public void DivisionByZeroMatchesDecimalExceptionAndLeavesOperandsUsable()
    {
        TracedValue left = TracedValue.Of(1m, "Left");
        TracedValue right = TracedValue.Of(0m, "Right");
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
        TracedValue left = TracedValue.Of(leftValue, "Left");
        TracedValue right = TracedValue.Of(rightValue, "Right");
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
        TracedValue initialized = TracedValue.Of(1m);
        TracedValue uninitialized = default;

        Assert.Throws<InvalidOperationException>(() => { _ = Apply(operation, uninitialized, initialized); });
        Assert.Throws<InvalidOperationException>(() => { _ = Apply(operation, initialized, uninitialized); });
    }

    [Theory]
    [InlineData("Add", 1.25, 2.5)]
    [InlineData("Subtract", 1.25, 2.5)]
    [InlineData("Multiply", 1.25, 2.5)]
    [InlineData("Divide", 5.0, 2.5)]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-009")]
    public void MixedOperatorsMatchDecimalAndRecordThePlainOperand(
        string operation,
        decimal tracedValue,
        decimal plainValue)
    {
        TracedValue traced = TracedValue.Of(tracedValue, "Traced");

        TracedValue leftResult = ApplyMixed(operation, traced, plainValue);
        TracedValue rightResult = ApplyMixed(operation, plainValue, traced);

        AssertDecimalBitsEqual(Apply(operation, tracedValue, plainValue), leftResult.Value);
        AssertDecimalBitsEqual(Apply(operation, plainValue, tracedValue), rightResult.Value);
        AssertAnonymousInput(leftResult.Root, plainValue, expectedSide: SelectedOperand.Right);
        AssertAnonymousInput(rightResult.Root, plainValue, expectedSide: SelectedOperand.Left);
        Assert.Same(traced.Root, Assert.IsType<BinaryOperationEvidenceNode>(leftResult.Root).Left);
        Assert.Same(traced.Root, Assert.IsType<BinaryOperationEvidenceNode>(rightResult.Root).Right);
    }

    [Fact]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-008")]
    public void MinAndMaxMatchDecimalAndRecordTheSelectedOperand()
    {
        TracedValue left = TracedValue.Of(2m, "Left");
        TracedValue right = TracedValue.Of(3m, "Right");

        TracedValue min = TracedValue.Min(left, right);
        TracedValue max = TracedValue.Max(left, right);

        Assert.Equal(Math.Min(2m, 3m), min.Value);
        Assert.Equal(Math.Max(2m, 3m), max.Value);
        Assert.Equal(SelectedOperand.Left, Assert.IsType<BinaryOperationEvidenceNode>(min.Root).SelectedOperand);
        Assert.Equal(SelectedOperand.Right, Assert.IsType<BinaryOperationEvidenceNode>(max.Root).SelectedOperand);
        Assert.Same(left.Root, Assert.IsType<BinaryOperationEvidenceNode>(min.Root).Left);
        Assert.Same(right.Root, Assert.IsType<BinaryOperationEvidenceNode>(min.Root).Right);
        Assert.Same(left.Root, Assert.IsType<BinaryOperationEvidenceNode>(max.Root).Left);
        Assert.Same(right.Root, Assert.IsType<BinaryOperationEvidenceNode>(max.Root).Right);
    }

    [Fact]
    [Trait("RQ", "RQ-008")]
    public void MinAndMaxSelectTheLeftOperandWhenValuesAreEqual()
    {
        TracedValue left = TracedValue.Of(2.00m, "Left");
        TracedValue right = TracedValue.Of(2.00m, "Right");

        Assert.Equal(SelectedOperand.Left, Assert.IsType<BinaryOperationEvidenceNode>(TracedValue.Min(left, right).Root).SelectedOperand);
        Assert.Equal(SelectedOperand.Left, Assert.IsType<BinaryOperationEvidenceNode>(TracedValue.Max(left, right).Root).SelectedOperand);
    }

    [Fact]
    public void MinAndMaxRejectDefaultOperands()
    {
        TracedValue initialized = TracedValue.Of(1m);
        TracedValue uninitialized = default;

        Assert.Throws<InvalidOperationException>(() => TracedValue.Min(uninitialized, initialized));
        Assert.Throws<InvalidOperationException>(() => TracedValue.Min(initialized, uninitialized));
        Assert.Throws<InvalidOperationException>(() => TracedValue.Max(uninitialized, initialized));
        Assert.Throws<InvalidOperationException>(() => TracedValue.Max(initialized, uninitialized));
    }

    private static void AssertAnonymousInput(EvidenceNode root, decimal expectedValue, SelectedOperand expectedSide)
    {
        BinaryOperationEvidenceNode operation = Assert.IsType<BinaryOperationEvidenceNode>(root);
        InputEvidenceNode input = Assert.IsType<InputEvidenceNode>(expectedSide == SelectedOperand.Left ? operation.Left : operation.Right);
        Assert.Null(input.Name);
        AssertDecimalBitsEqual(expectedValue, input.Value);
    }

    internal static TracedValue ApplyMixed(string operation, TracedValue traced, decimal plain) => operation switch
    {
        "Add" => traced + plain,
        "Subtract" => traced - plain,
        "Multiply" => traced * plain,
        "Divide" => traced / plain,
        _ => throw new ArgumentOutOfRangeException(nameof(operation)),
    };

    internal static TracedValue ApplyMixed(string operation, decimal plain, TracedValue traced) => operation switch
    {
        "Add" => plain + traced,
        "Subtract" => plain - traced,
        "Multiply" => plain * traced,
        "Divide" => plain / traced,
        _ => throw new ArgumentOutOfRangeException(nameof(operation)),
    };

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
