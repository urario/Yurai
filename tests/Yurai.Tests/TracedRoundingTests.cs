using System.Globalization;
using Xunit;
using TracedValue = global::Yurai.Traced;

namespace Yurai.Tests;

public sealed class TracedRoundingTests
{
    [Theory]
    [InlineData("2.5", 0, "2")]
    [InlineData("3.5", 0, "4")]
    [InlineData("-2.5", 0, "-2")]
    [InlineData("-3.5", 0, "-4")]
    [InlineData("12.345", 2, "12.34")]
    [InlineData("12.355", 2, "12.36")]
    [InlineData("1.2000", 2, "1.20")]
    [InlineData("1.5", 5, "1.5")]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-010")]
    public void RoundMatchesDecimalRoundAndPreservesScale(
        string valueText,
        int digits,
        string expectedText)
    {
        decimal value = decimal.Parse(valueText, CultureInfo.InvariantCulture);
        decimal expected = decimal.Parse(expectedText, CultureInfo.InvariantCulture);
        TracedValue traced = TracedValue.Of(value, "Value");

        TracedValue rounded = traced.Round(digits, "business rounding");

        AssertDecimalBitsEqual(decimal.Round(value, digits), rounded.Value);
        AssertDecimalBitsEqual(expected, rounded.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-010")]
    [Trait("RQ", "RQ-011")]
    public void RoundRecordsPolicyReasonAndOriginalRoot()
    {
        TracedValue traced = TracedValue.Of(12.345m, "Value");

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
    [Trait("RQ", "RQ-010")]
    public void RoundPreservesNativeDigitsException(int digits)
    {
        TracedValue traced = TracedValue.Of(1.25m, "Value");
        Exception? native = Record.Exception(() => decimal.Round(1.25m, digits));

        Exception? actual = Record.Exception(() => traced.Round(digits, "reason"));

        Assert.NotNull(native);
        Assert.NotNull(actual);
        Assert.Equal(native.GetType(), actual.GetType());
        Assert.Equal("decimals", actual is ArgumentOutOfRangeException rangeException ? rangeException.ParamName : null);
        Assert.Equal(1.25m, traced.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-010")]
    public void RoundValidatesReasonWithoutCreatingEvidence()
    {
        TracedValue traced = TracedValue.Of(1.25m, "Value");
        EvidenceNode originalRoot = traced.Root;

        var nullReason = Assert.Throws<ArgumentNullException>(() => traced.Round(2, null!));
        var emptyReason = Assert.Throws<ArgumentException>(() => traced.Round(2, string.Empty));
        var whitespaceReason = Assert.Throws<ArgumentException>(() => traced.Round(2, "   "));

        Assert.Equal("reason", nullReason.ParamName);
        Assert.Equal("reason", emptyReason.ParamName);
        Assert.Equal("reason", whitespaceReason.ParamName);
        Assert.Same(originalRoot, traced.Root);
    }

    [Fact]
    [Trait("RQ", "RQ-010")]
    public void NativeDigitsFailureTakesPrecedenceOverInvalidReason()
    {
        TracedValue traced = TracedValue.Of(1.25m, "Value");

        Assert.Throws<ArgumentOutOfRangeException>(() => traced.Round(-1, null!));
    }

    [Fact]
    [Trait("RQ", "RQ-010")]
    public void RoundRejectsAnUninitializedCarrier()
    {
        TracedValue traced = default;

        Assert.Throws<InvalidOperationException>(() => traced.Round(2, "reason"));
    }

    private static void AssertDecimalBitsEqual(decimal expected, decimal actual) =>
        Assert.Equal(decimal.GetBits(expected), decimal.GetBits(actual));
}
