using Xunit;
using TracedValue = global::Yurai.Traced;

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
        TracedValue whenTrue = TracedValue.Of(10.00m, "TrueValue");
        TracedValue whenFalse = TracedValue.Of(20.000m, "FalseValue");
        int trueInvocations = 0;
        int falseInvocations = 0;

        TracedValue result = TracedValue.If(
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
        TracedValue selected = TracedValue.Of(1m, "Selected");
        TracedValue unselected = TracedValue.Of(2m, "Unselected");
        int unselectedInvocations = 0;
        Func<TracedValue> returnsSelected = () => selected;
        Func<TracedValue> returnsUnselected = () =>
        {
            unselectedInvocations++;
            return unselected;
        };

        TracedValue result = condition
            ? TracedValue.If(true, returnsSelected, returnsUnselected, "Decision")
            : TracedValue.If(false, returnsUnselected, returnsSelected, "Decision");

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
        Func<TracedValue> returnsSelected = () => TracedValue.Of(1m);
        Func<TracedValue> throws = () => throw new InvalidOperationException("The unselected alternative ran.");

        TracedValue result = condition
            ? TracedValue.If(true, returnsSelected, throws, "Decision")
            : TracedValue.If(false, throws, returnsSelected, "Decision");

        Assert.Equal(1m, result.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-005")]
    public void NestedIfRecordsBothSelectedDecisions()
    {
        TracedValue selected = TracedValue.Of(30m, "Selected");
        int outerUnselectedInvocations = 0;
        int innerUnselectedInvocations = 0;

        TracedValue result = TracedValue.If(
            false,
            () =>
            {
                outerUnselectedInvocations++;
                return TracedValue.Of(10m);
            },
            () => TracedValue.If(
                true,
                () => selected,
                () =>
                {
                    innerUnselectedInvocations++;
                    return TracedValue.Of(20m);
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
    [Trait("RQ", "RQ-005")]
    public void BranchResultComposesWithDownstreamOperations()
    {
        TracedValue input = TracedValue.Of(10.005m, "Input");

        TracedValue result = TracedValue
            .If(true, () => input, () => TracedValue.Of(0m), "Decision")
            .Round(2, "Round to currency units")
            .As("Total");

        TracedArithmeticTests.AssertDecimalBitsEqual(10.00m, result.Value);
        var named = Assert.IsType<NamedEvidenceNode>(result.Root);
        var rounded = Assert.IsType<RoundEvidenceNode>(named.Child);
        var branch = Assert.IsType<BranchEvidenceNode>(rounded.Child);
        Assert.Same(input.Root, branch.Child);
        TracedArithmeticTests.AssertDecimalBitsEqual(input.Value, branch.Value);
    }

    [Fact]
    public void PlainBooleanConditionDoesNotCreateAControlDependency()
    {
        TracedValue conditionOnly = TracedValue.Of(5m, "ConditionOnly");
        TracedValue selected = TracedValue.Of(1m, "Selected");

        TracedValue result = TracedValue.If(
            conditionOnly.Value > 0m,
            () => selected,
            () => TracedValue.Of(0m),
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
            return TracedValue.Of(1m);
        };

        var nullWhenTrue = Assert.Throws<ArgumentNullException>(
            () => TracedValue.If(false, null!, valid, "Decision"));
        var nullWhenFalse = Assert.Throws<ArgumentNullException>(
            () => TracedValue.If(true, valid, null!, "Decision"));

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
            return TracedValue.Of(1m);
        };

        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => TracedValue.If(true, alternative, alternative, branchName!));

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
            () => TracedValue.If(true, () => default, () => TracedValue.Of(1m), "Decision"));
    }

    [Fact]
    public void IfPropagatesTheSelectedExceptionWithoutInvokingTheOtherAlternative()
    {
        var expected = new ArithmeticException("Selected failure");
        int unselectedInvocations = 0;

        Exception actual = Assert.Throws<ArithmeticException>(
            () => TracedValue.If(
                true,
                () => throw expected,
                () =>
                {
                    unselectedInvocations++;
                    return TracedValue.Of(1m);
                },
                "Decision"));

        Assert.Same(expected, actual);
        Assert.Equal(0, unselectedInvocations);
    }
}
