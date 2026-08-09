using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedCreationTests
{
    [Fact]
    [Trait("RQ", "RQ-007")]
    public void NamedInputPreservesValueAndName()
    {
        TracedValue traced = YuraiApi.Of(123.4500m, "BasePrice");

        AssertDecimalBitsEqual(123.4500m, traced.Value);
        var input = Assert.IsType<InputEvidenceNode>(traced.Root);
        Assert.Equal("BasePrice", input.Name);
    }

    [Fact]
    [Trait("RQ", "RQ-007")]
    public void AnonymousInputHasNoInventedName()
    {
        TracedValue traced = YuraiApi.Of(10m);

        var input = Assert.IsType<InputEvidenceNode>(traced.Root);
        Assert.Null(input.Name);
    }

    [Fact]
    [Trait("RQ", "RQ-007")]
    public void AsCreatesANamedParentWithoutChangingItsChild()
    {
        TracedValue input = YuraiApi.Of(10m, "Input");

        TracedValue named = input.As("Result");

        var node = Assert.IsType<NamedEvidenceNode>(named.Root);
        Assert.Equal("Result", node.Name);
        Assert.Same(input.Root, node.Child);
        Assert.Same(input.Root, input.Root);
        AssertDecimalBitsEqual(input.Value, named.Value);
    }

    [Fact]
    public void NamedInputRejectsInvalidNames()
    {
        var nullName = Assert.Throws<ArgumentNullException>(() => { YuraiApi.Of(1m, null!); });
        var emptyName = Assert.Throws<ArgumentException>(() => { YuraiApi.Of(1m, string.Empty); });
        var whitespaceName = Assert.Throws<ArgumentException>(() => { YuraiApi.Of(1m, " \t\r\n"); });

        Assert.Equal("name", nullName.ParamName);
        Assert.Equal("name", emptyName.ParamName);
        Assert.Equal("name", whitespaceName.ParamName);
    }

    [Fact]
    public void AsRejectsInvalidNames()
    {
        TracedValue traced = YuraiApi.Of(1m);

        var nullName = Assert.Throws<ArgumentNullException>(() => { traced.As(null!); });
        var emptyName = Assert.Throws<ArgumentException>(() => { traced.As(string.Empty); });
        var whitespaceName = Assert.Throws<ArgumentException>(() => { traced.As(" \t\r\n"); });

        Assert.Equal("name", nullName.ParamName);
        Assert.Equal("name", emptyName.ParamName);
        Assert.Equal("name", whitespaceName.ParamName);
    }

    [Fact]
    public void DefaultValueIsInvalidButToStringIsDiagnostic()
    {
        TracedValue traced = default;

        Assert.Throws<InvalidOperationException>(() => { _ = traced.Value; });
        Assert.Throws<InvalidOperationException>(() => { traced.As("Name"); });
        Assert.Equal("Uninitialized Traced", traced.ToString());
    }

    [Fact]
    public void ToStringUsesInvariantDecimalFormatting()
    {
        TracedValue traced = YuraiApi.Of(1234.5600m);

        Assert.Equal("1234.5600", traced.ToString());
    }

    private static void AssertDecimalBitsEqual(decimal expected, decimal actual) =>
        Assert.Equal(decimal.GetBits(expected), decimal.GetBits(actual));
}
