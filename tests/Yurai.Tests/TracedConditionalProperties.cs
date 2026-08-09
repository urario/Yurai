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
    [Trait("RQ", "RQ-005")]
    public void IfMatchesTheConditionalOperatorForGeneratedValues()
    {
        Gen.Select(Gen.Decimal, Gen.Decimal, Gen.Bool).Sample(values =>
        {
            decimal whenTrue = values.Item1;
            decimal whenFalse = values.Item2;
            bool condition = values.Item3;
            decimal expected = condition ? whenTrue : whenFalse;

            TracedValue actual = YuraiApi.If(
                condition,
                () => YuraiApi.Of(whenTrue),
                () => YuraiApi.Of(whenFalse),
                "GeneratedDecision");

            TracedArithmeticTests.AssertDecimalBitsEqual(expected, actual.Value);
            var branch = Assert.IsType<BranchEvidenceNode>(actual.Root);
            Assert.Equal("GeneratedDecision", branch.DecisionName);
            Assert.Equal(condition ? BranchSelection.Then : BranchSelection.Else, branch.SelectedBranch);
        });
    }
}
