using System.Text.Json;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class MetadataValidationTests
{
    public static TheoryData<int> MalformedUtf16Cases => new()
    {
        0,
        1,
        2,
        3,
        4,
        5,
        6,
    };

    [Theory]
    [MemberData(nameof(MalformedUtf16Cases))]
    [Trait("RQ", "RQ-013")]
    public void MetadataEntryPointsRejectMalformedUtf16(int caseId)
    {
        string metadata = MalformedUtf16(caseId);
        TracedValue traced = YuraiApi.Of(1m, "Input");
        int alternativeInvocations = 0;
        Func<TracedValue> alternative = () =>
        {
            alternativeInvocations++;
            return traced;
        };

        AssertMalformed(() => YuraiApi.Of(1m, metadata), "name");
        AssertMalformed(() => traced.As(metadata), "name");
        AssertMalformed(() => traced.Round(0, metadata), "reason");
        AssertMalformed(
            () => YuraiApi.If(true, alternative, alternative, metadata),
            "branchName");

        Assert.Equal(0, alternativeInvocations);
        Assert.Equal(1m, traced.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-013")]
    public void ValidSurrogatePairsRoundTripThroughEveryMetadataField()
    {
        const string metadata = "Domain \U0001F600 value";
        TracedValue input = YuraiApi.Of(1m, metadata);
        TracedValue named = input.As(metadata);
        TracedValue rounded = named.Round(0, metadata);
        TracedValue result = YuraiApi.If(
            true,
            () => rounded,
            () => YuraiApi.Of(0m),
            metadata);

        using JsonDocument document = JsonDocument.Parse(result.ToJson());
        JsonElement[] nodes = document.RootElement.GetProperty("nodes").EnumerateArray().ToArray();

        Assert.Equal(metadata, nodes[0].GetProperty("branchName").GetString());
        Assert.Equal(metadata, nodes[1].GetProperty("reason").GetString());
        Assert.Equal(metadata, nodes[2].GetProperty("name").GetString());
        Assert.Equal(metadata, nodes[3].GetProperty("name").GetString());
    }

    private static void AssertMalformed(Action action, string parameterName)
    {
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal(parameterName, exception.ParamName);
    }

    private static string MalformedUtf16(int caseId) => caseId switch
    {
        0 => "\ud800",
        1 => "\udc00",
        2 => "prefix\ud800",
        3 => "\udc00suffix",
        4 => "\ud800\ud800",
        5 => "\udc00\udc00",
        6 => "\ud800middle\udc00",
        _ => throw new ArgumentOutOfRangeException(nameof(caseId)),
    };
}
