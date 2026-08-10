using System.Text;
using System.Text.Json;
using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;

namespace Yurai.Tests;

public sealed class TracedJsonProperties
{
    [Fact]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-013")]
    public void JsonMetadataRoundTripsForWellFormedUtf16()
    {
        Gen.Int.Sample(seed =>
        {
            string metadata = CreateMetadata(seed);
            TracedValue result = TracedValue.If(
                    true,
                    () => TracedValue.Of(1m, metadata).Round(0, metadata),
                    () => TracedValue.Of(2m),
                    metadata)
                .As(metadata);

            using JsonDocument document = JsonDocument.Parse(result.ToJson());
            JsonElement[] nodes = document.RootElement.GetProperty("nodes").EnumerateArray().ToArray();

            Assert.Equal(metadata, nodes[0].GetProperty("name").GetString());
            Assert.Equal(metadata, nodes[1].GetProperty("branchName").GetString());
            Assert.Equal(metadata, nodes[2].GetProperty("reason").GetString());
            Assert.Equal(metadata, nodes[3].GetProperty("name").GetString());
        });
    }

    private static string CreateMetadata(int seed)
    {
        string[] tokens =
        [
            "a",
            "Z",
            "0",
            " ",
            "\"",
            "\\",
            "\b",
            "\f",
            "\n",
            "\r",
            "\t",
            "\u0001",
            "\u001F",
            "é",
            "漢",
            "\U0001F600",
        ];

        var metadata = new StringBuilder("metadata:");
        uint bits = unchecked((uint)seed);
        for (int index = 0; index < 8; index++)
        {
            metadata.Append(tokens[bits & 0x0F]);
            bits >>= 4;
        }

        return metadata.ToString();
    }
}
