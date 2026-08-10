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
            SmallDecimal,
            SmallDecimal,
            SmallDecimal,
            SmallDecimal,
            SmallDecimal,
            NonZeroDivisor,
            Gen.Int[0, 28],
            Gen.Bool).Sample(values => AssertParity(
                values.Item1,
                values.Item2,
                values.Item3,
                values.Item4,
                values.Item5,
                values.Item6,
                values.Item7,
                values.Item8));
    }

    [Fact]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-001")]
    public void CompositeCalculationExceptionsMatchEquivalentDecimalCalculation()
    {
        Gen.Select(
            Gen.Decimal,
            Gen.Decimal,
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
                values.Item6,
                values.Item7,
                values.Item8));
    }

    private static void AssertParity(
        decimal left,
        decimal right,
        decimal candidate,
        decimal addend,
        decimal multiplier,
        decimal divisor,
        int digits,
        bool condition)
    {
        decimal nativeResult = default;
        Exception? nativeException = Record.Exception(
            () =>
            {
                decimal selected = condition ? left : right;
                decimal selectedOperand = digits % 2 == 0
                    ? Math.Max(selected, candidate)
                    : Math.Min(selected, candidate);
                nativeResult = decimal.Round(
                    ((selectedOperand - addend) * multiplier) / divisor,
                    digits);
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
                TracedValue selectedOperand = digits % 2 == 0
                    ? YuraiApi.Max(selected, YuraiApi.Of(candidate))
                    : YuraiApi.Min(selected, YuraiApi.Of(candidate));
                tracedResult = ((selectedOperand - addend) * multiplier / divisor)
                    .Round(digits, "generated rounding");
            });

        if (nativeException is null)
        {
            Assert.Null(tracedException);
            TracedArithmeticTests.AssertDecimalBitsEqual(nativeResult, tracedResult.Value);
            AssertEvidence(tracedResult, condition, digits);
            return;
        }

        Assert.NotNull(tracedException);
        Assert.Equal(nativeException.GetType(), tracedException.GetType());
    }

    private static void AssertEvidence(TracedValue result, bool condition, int digits)
    {
        RoundEvidenceNode round = Assert.IsType<RoundEvidenceNode>(result.Root);
        Assert.Equal(digits, round.Digits);
        Assert.Equal("generated rounding", round.Reason);
        Assert.Contains("GeneratedDecision", result.Explain(), StringComparison.Ordinal);

        BranchEvidenceNode? branch = null;
        foreach (EvidenceNode node in EvidenceTraversal.PreOrder(result.Root))
        {
            if (node is BranchEvidenceNode branchNode)
            {
                branch = branchNode;
                break;
            }
        }

        Assert.NotNull(branch);
        Assert.Equal(condition ? BranchSelection.Then : BranchSelection.Else, branch!.SelectedBranch);
    }

    private static Gen<decimal> SmallDecimal => Gen.Decimal.Select(value => value % 1000m);

    private static Gen<decimal> NonZeroDivisor => Gen.Decimal
        .Where(value => value != 0m)
        .Select(value =>
        {
            decimal divisor = value % 10m;
            return divisor == 0m ? 1m : divisor;
        });
}
