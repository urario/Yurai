using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedConditionalTests
{
    public static TheoryData<string?> InvalidBranchNames => new()
    {
        null,
        string.Empty,
        "   ",
    };

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-005")]
    public void IfMatchesConditionalAndRecordsTheSelectedBranch(bool condition)
    {
        TracedValue whenTrue = YuraiApi.Of(10.00m, "TrueValue");
        TracedValue whenFalse = YuraiApi.Of(20.000m, "FalseValue");
        int trueInvocations = 0;
        int falseInvocations = 0;

        TracedValue result = YuraiApi.If(
            condition,
            () =>
            {
                trueInvocations++;
                return whenTrue;
            },
            () =>
            {
                falseInvocations++;
                return whenFalse;
            },
            "RateThreshold");

        TracedValue expected = condition ? whenTrue : whenFalse;
        TracedArithmeticTests.AssertDecimalBitsEqual(expected.Value, result.Value);
        Assert.Equal(condition ? 1 : 0, trueInvocations);
        Assert.Equal(condition ? 0 : 1, falseInvocations);

        var branch = Assert.IsType<BranchEvidenceNode>(result.Root);
        Assert.Equal("RateThreshold", branch.DecisionName);
        Assert.Equal(condition, branch.Condition);
        Assert.Equal(condition ? BranchSelection.Then : BranchSelection.Else, branch.SelectedBranch);
        Assert.Same(expected.Root, branch.Child);
        Assert.Equal(1, branch.ChildCount);
        Assert.Same(expected.Root, branch.GetChild(0));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("RQ", "RQ-005")]
    public void IfDoesNotInvokeOrRecordTheUnselectedAlternative(bool condition)
    {
        TracedValue selected = YuraiApi.Of(1m, "Selected");
        TracedValue unselected = YuraiApi.Of(2m, "Unselected");
        int unselectedInvocations = 0;
        Func<TracedValue> returnsSelected = () => selected;
        Func<TracedValue> returnsUnselected = () =>
        {
            unselectedInvocations++;
            return unselected;
        };

        TracedValue result = condition
            ? YuraiApi.If(true, returnsSelected, returnsUnselected, "Decision")
            : YuraiApi.If(false, returnsUnselected, returnsSelected, "Decision");

        Assert.Equal(0, unselectedInvocations);
        EvidenceNode[] visited = EvidenceTraversal.PreOrder(result.Root).ToArray();
        Assert.Contains(selected.Root, visited);
        Assert.DoesNotContain(unselected.Root, visited);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("RQ", "RQ-005")]
    public void IfDoesNotInvokeAThrowingUnselectedAlternative(bool condition)
    {
        Func<TracedValue> returnsSelected = () => YuraiApi.Of(1m);
        Func<TracedValue> throws = () => throw new InvalidOperationException("The unselected alternative ran.");

        TracedValue result = condition
            ? YuraiApi.If(true, returnsSelected, throws, "Decision")
            : YuraiApi.If(false, throws, returnsSelected, "Decision");

        Assert.Equal(1m, result.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-005")]
    public void NestedIfRecordsBothSelectedDecisions()
    {
        TracedValue selected = YuraiApi.Of(30m, "Selected");
        int outerUnselectedInvocations = 0;
        int innerUnselectedInvocations = 0;

        TracedValue result = YuraiApi.If(
            false,
            () =>
            {
                outerUnselectedInvocations++;
                return YuraiApi.Of(10m);
            },
            () => YuraiApi.If(
                true,
                () => selected,
                () =>
                {
                    innerUnselectedInvocations++;
                    return YuraiApi.Of(20m);
                },
                "InnerDecision"),
            "OuterDecision");

        var outer = Assert.IsType<BranchEvidenceNode>(result.Root);
        var inner = Assert.IsType<BranchEvidenceNode>(outer.Child);
        Assert.Equal("OuterDecision", outer.DecisionName);
        Assert.False(outer.Condition);
        Assert.Equal(BranchSelection.Else, outer.SelectedBranch);
        Assert.Equal("InnerDecision", inner.DecisionName);
        Assert.True(inner.Condition);
        Assert.Equal(BranchSelection.Then, inner.SelectedBranch);
        Assert.Same(selected.Root, inner.Child);
        Assert.Equal(0, outerUnselectedInvocations);
        Assert.Equal(0, innerUnselectedInvocations);
    }

    [Fact]
    public void PlainBooleanConditionDoesNotCreateAControlDependency()
    {
        TracedValue conditionOnly = YuraiApi.Of(5m, "ConditionOnly");
        TracedValue selected = YuraiApi.Of(1m, "Selected");

        TracedValue result = YuraiApi.If(
            conditionOnly.Value > 0m,
            () => selected,
            () => YuraiApi.Of(0m),
            "Positive");

        EvidenceNode[] visited = EvidenceTraversal.PreOrder(result.Root).ToArray();
        Assert.Contains(selected.Root, visited);
        Assert.DoesNotContain(conditionOnly.Root, visited);
    }

    [Fact]
    public void IfValidatesBothDelegatesBeforeInvokingEither()
    {
        int invocations = 0;
        Func<TracedValue> valid = () =>
        {
            invocations++;
            return YuraiApi.Of(1m);
        };

        var nullWhenTrue = Assert.Throws<ArgumentNullException>(
            () => YuraiApi.If(false, null!, valid, "Decision"));
        var nullWhenFalse = Assert.Throws<ArgumentNullException>(
            () => YuraiApi.If(true, valid, null!, "Decision"));

        Assert.Equal("whenTrue", nullWhenTrue.ParamName);
        Assert.Equal("whenFalse", nullWhenFalse.ParamName);
        Assert.Equal(0, invocations);
    }

    [Theory]
    [MemberData(nameof(InvalidBranchNames))]
    public void IfValidatesBranchNameBeforeInvokingAnAlternative(string? branchName)
    {
        int invocations = 0;
        Func<TracedValue> alternative = () =>
        {
            invocations++;
            return YuraiApi.Of(1m);
        };

        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => YuraiApi.If(true, alternative, alternative, branchName!));

        Assert.Equal("branchName", exception.ParamName);
        Assert.Equal(0, invocations);
        if (branchName is null)
        {
            Assert.IsType<ArgumentNullException>(exception);
        }
        else
        {
            Assert.IsType<ArgumentException>(exception);
        }
    }

    [Fact]
    public void IfRejectsAnUninitializedSelectedResult()
    {
        Assert.Throws<InvalidOperationException>(
            () => YuraiApi.If(true, () => default, () => YuraiApi.Of(1m), "Decision"));
    }

    [Fact]
    public void IfPropagatesTheSelectedExceptionWithoutInvokingTheOtherAlternative()
    {
        var expected = new ArithmeticException("Selected failure");
        int unselectedInvocations = 0;

        Exception actual = Assert.Throws<ArithmeticException>(
            () => YuraiApi.If(
                true,
                () => throw expected,
                () =>
                {
                    unselectedInvocations++;
                    return YuraiApi.Of(1m);
                },
                "Decision"));

        Assert.Same(expected, actual);
        Assert.Equal(0, unselectedInvocations);
    }
}
