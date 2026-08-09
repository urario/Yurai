using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedRoundingTests
{
    [Theory]
    [InlineData(2.5, 0, 2.0)]
    [InlineData(3.5, 0, 4.0)]
    [InlineData(-2.5, 0, -2.0)]
    [InlineData(-3.5, 0, -4.0)]
    [InlineData(12.345, 2, 12.34)]
    [InlineData(12.355, 2, 12.36)]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-010")]
    public void RoundMatchesDecimalRoundAndPreservesScale(
        decimal value,
        int digits,
        decimal expected)
    {
        TracedValue traced = YuraiApi.Of(value, "Value");

        TracedValue rounded = traced.Round(digits, "business rounding");

        AssertDecimalBitsEqual(decimal.Round(value, digits), rounded.Value);
        AssertDecimalBitsEqual(expected, rounded.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-010")]
    [Trait("RQ", "RQ-011")]
    public void RoundRecordsPolicyReasonAndOriginalRoot()
    {
        TracedValue traced = YuraiApi.Of(12.345m, "Value");

        TracedValue rounded = traced.Round(2, "regulatory rounding to cents");
        RoundEvidenceNode node = Assert.IsType<RoundEvidenceNode>(rounded.Root);

        Assert.Equal(2, node.Digits);
        Assert.Equal(MidpointRounding.ToEven, node.Rounding);
        Assert.Equal("regulatory rounding to cents", node.Reason);
        Assert.Same(traced.Root, node.Child);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(29)]
    [Trait("RQ", "RQ-001")]
    public void RoundPreservesNativeDigitsException(int digits)
    {
        TracedValue traced = YuraiApi.Of(1.25m, "Value");
        Exception native = Assert.Throws<ArgumentOutOfRangeException>(() => decimal.Round(1.25m, digits));

        Exception actual = Assert.Throws<ArgumentOutOfRangeException>(() => traced.Round(digits, "reason"));

        Assert.Equal(native.GetType(), actual.GetType());
        Assert.Equal(1.25m, traced.Value);
    }

    [Fact]
    public void RoundValidatesReasonWithoutCreatingEvidence()
    {
        TracedValue traced = YuraiApi.Of(1.25m, "Value");
        EvidenceNode originalRoot = traced.Root;

        Assert.Throws<ArgumentNullException>(() => traced.Round(2, null!));
        Assert.Throws<ArgumentException>(() => traced.Round(2, string.Empty));
        Assert.Throws<ArgumentException>(() => traced.Round(2, "   "));

        Assert.Same(originalRoot, traced.Root);
    }

    [Fact]
    public void NativeDigitsFailureTakesPrecedenceOverInvalidReason()
    {
        TracedValue traced = YuraiApi.Of(1.25m, "Value");

        Assert.Throws<ArgumentOutOfRangeException>(() => traced.Round(-1, null!));
    }

    private static void AssertDecimalBitsEqual(decimal expected, decimal actual) =>
        Assert.Equal(decimal.GetBits(expected), decimal.GetBits(actual));
}
