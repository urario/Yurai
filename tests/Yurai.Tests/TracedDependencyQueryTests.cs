using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedDependencyQueryTests
{
    [Fact]
    [Trait("RQ", "RQ-003")]
    [Trait("RQ", "RQ-014")]
    public void QueriesExposeNamedDependenciesAndAllPathsInDeterministicOrder()
    {
        TracedValue basePrice = YuraiApi.Of(100m, "BasePrice");
        TracedValue discount = YuraiApi.Of(10m, "Discount");
        TracedValue subtotal = (basePrice - discount).As("Subtotal");
        TracedValue total = (subtotal + basePrice).As("Total");

        Assert.True(total.DependsOn("BasePrice"));
        Assert.True(total.DependsOn("Subtotal"));
        Assert.False(total.DependsOn("Missing"));
        Assert.Equal(["BasePrice", "Discount"], total.Inputs);
        AssertPaths(
            total.Trace("BasePrice"),
            ["BasePrice", "Subtotal", "Total"],
            ["BasePrice", "Total"]);
        AssertPaths(total.Trace("Subtotal"), ["Subtotal", "Total"]);
        Assert.Empty(total.Trace("Missing"));
    }

    [Fact]
    [Trait("RQ", "RQ-014")]
    public void DuplicateNamesPreserveEveryMatchAndPathButInputsAreUnique()
    {
        TracedValue left = YuraiApi.Of(1m, "Rate");
        TracedValue right = YuraiApi.Of(2m, "Rate");
        TracedValue shared = (left + right).As("Rate");
        TracedValue result = (shared + shared).As("Total");

        Assert.Equal(["Rate"], result.Inputs);
        AssertPaths(
            result.Trace("Rate"),
            ["Rate", "Total"],
            ["Rate", "Rate", "Total"],
            ["Rate", "Rate", "Total"],
            ["Rate", "Total"],
            ["Rate", "Rate", "Total"],
            ["Rate", "Rate", "Total"]);
    }

    [Fact]
    public void RootMatchesAndAnonymousNodesAreTraversedButNotProjected()
    {
        TracedValue input = YuraiApi.Of(3m, "Input");
        TracedValue result = (input + 2m).As("Result");

        AssertPaths(result.Trace("Result"), ["Result"]);
        AssertPaths(result.Trace("Input"), ["Input", "Result"]);
        Assert.Equal(["Input"], result.Inputs);
    }

    [Fact]
    public void QueryResultsAreReadOnlySnapshots()
    {
        TracedValue result = YuraiApi.Of(1m, "Input").As("Result");

        IReadOnlyList<string> inputs = result.Inputs;
        IReadOnlyList<IReadOnlyList<string>> paths = result.Trace("Input");

        Assert.True(Assert.IsAssignableFrom<ICollection<string>>(inputs).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ICollection<IReadOnlyList<string>>>(paths).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ICollection<string>>(paths[0]).IsReadOnly);
        Assert.Throws<NotSupportedException>(() => Assert.IsAssignableFrom<IList<string>>(inputs).Add("Other"));
        Assert.NotSame(inputs, result.Inputs);
        Assert.NotSame(paths, result.Trace("Input"));
    }

    [Fact]
    public void ConditionOnlyInputIsNotARecordedDependency()
    {
        TracedValue conditionOnly = YuraiApi.Of(5m, "ConditionOnly");
        TracedValue selected = YuraiApi.Of(1m, "Selected");
        TracedValue result = YuraiApi.If(
            conditionOnly.Value > 0m,
            () => selected,
            () => YuraiApi.Of(0m, "Unselected"),
            "Positive");

        Assert.False(result.DependsOn("ConditionOnly"));
        Assert.Empty(result.Trace("ConditionOnly"));
        Assert.Equal(["Selected"], result.Inputs);
    }

    [Fact]
    public void QueriesRejectAnUninitializedCarrier()
    {
        TracedValue value = default;

        Assert.Throws<InvalidOperationException>(() => value.DependsOn("Input"));
        Assert.Throws<InvalidOperationException>(() => value.Trace("Input"));
        Assert.Throws<InvalidOperationException>(() => value.Inputs);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\uDC00")]
    public void NamedQueriesUseStrictNameValidation(string? name)
    {
        TracedValue value = YuraiApi.Of(1m, "Input");

        ArgumentException dependsOn = Assert.ThrowsAny<ArgumentException>(() => value.DependsOn(name!));
        ArgumentException trace = Assert.ThrowsAny<ArgumentException>(() => value.Trace(name!));

        Assert.Equal("name", dependsOn.ParamName);
        Assert.Equal("name", trace.ParamName);
        Assert.Equal(name is null, dependsOn is ArgumentNullException);
        Assert.Equal(name is null, trace is ArgumentNullException);
    }

    [Fact]
    [Trait("RQ", "RQ-014")]
    public void TraceHandlesAChainBeyondTenThousandNodesWithoutRecursion()
    {
        TracedValue result = YuraiApi.Of(1m, "Input");
        const int namedNodeCount = 10_001;
        for (int index = 0; index < namedNodeCount; index++)
        {
            result = result.As($"Node{index}");
        }

        IReadOnlyList<string> path = Assert.Single(result.Trace("Input"));

        Assert.Equal(namedNodeCount + 1, path.Count);
        Assert.Equal("Input", path[0]);
        Assert.Equal($"Node{namedNodeCount - 1}", path[path.Count - 1]);
    }

    [Fact]
    public async Task ImmutableGraphCanBeQueriedConcurrently()
    {
        TracedValue result = (YuraiApi.Of(2m, "Left") * YuraiApi.Of(3m, "Right")).As("Result");

        Task<(bool DependsOn, string[] Inputs, string[][] Paths)>[] reads = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(() => (
                result.DependsOn("Left"),
                result.Inputs.ToArray(),
                result.Trace("Left").Select(path => path.ToArray()).ToArray())))
            .ToArray();

        var snapshots = await Task.WhenAll(reads);

        Assert.All(snapshots, snapshot =>
        {
            Assert.True(snapshot.DependsOn);
            Assert.Equal(["Left", "Right"], snapshot.Inputs);
            Assert.Equal(["Left", "Result"], Assert.Single(snapshot.Paths));
        });
    }

    private static void AssertPaths(
        IReadOnlyList<IReadOnlyList<string>> actual,
        params string[][] expected)
    {
        Assert.Equal(expected.Length, actual.Count);
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index], actual[index]);
        }
    }
}
