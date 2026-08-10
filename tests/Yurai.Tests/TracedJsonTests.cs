using System.Globalization;
using System.Text.Json;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedJsonTests
{
    [Fact]
    [Trait("RQ", "RQ-013")]
    [Trait("RQ", "RQ-027")]
    public void ToJsonExportsTheNormalizedSchemaForEveryEvidenceKind()
    {
        TracedValue input = YuraiApi.Of(2.500m, "Input\"Name\n");
        TracedValue rounded = input.Round(1, "Reason\\line\nnext");
        TracedValue branch = YuraiApi.If(
            false,
            () => YuraiApi.Of(-1m),
            () => rounded,
            "Decision\"");
        TracedValue anonymous = YuraiApi.Of(3m);
        TracedValue selected = YuraiApi.Min(branch, anonymous);
        TracedValue result = (selected + input).As("Total");

        using JsonDocument document = JsonDocument.Parse(result.ToJson());
        JsonElement root = document.RootElement;
        AssertPropertyNames(root, "schemaVersion", "root", "nodes");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(1, root.GetProperty("root").GetInt32());

        JsonElement[] nodes = root.GetProperty("nodes").EnumerateArray().ToArray();
        Assert.Equal(Enumerable.Range(1, 7), nodes.Select(NodeId));

        JsonElement named = nodes[0];
        AssertPropertyNames(named, "id", "kind", "value", "name", "child");
        AssertNode(named, 1, "named", "5.000");
        Assert.Equal("Total", named.GetProperty("name").GetString());
        Assert.Equal(2, named.GetProperty("child").GetInt32());

        JsonElement addition = nodes[1];
        AssertPropertyNames(addition, "id", "kind", "value", "operation", "left", "right", "selectedOperand");
        AssertNode(addition, 2, "binaryOperation", "5.000");
        Assert.Equal("add", addition.GetProperty("operation").GetString());
        Assert.Equal(3, addition.GetProperty("left").GetInt32());
        Assert.Equal(6, addition.GetProperty("right").GetInt32());
        Assert.Equal(JsonValueKind.Null, addition.GetProperty("selectedOperand").ValueKind);

        JsonElement minimum = nodes[2];
        AssertPropertyNames(minimum, "id", "kind", "value", "operation", "left", "right", "selectedOperand");
        AssertNode(minimum, 3, "binaryOperation", "2.5");
        Assert.Equal("min", minimum.GetProperty("operation").GetString());
        Assert.Equal(4, minimum.GetProperty("left").GetInt32());
        Assert.Equal(7, minimum.GetProperty("right").GetInt32());
        Assert.Equal("left", minimum.GetProperty("selectedOperand").GetString());

        JsonElement branchNode = nodes[3];
        AssertPropertyNames(branchNode, "id", "kind", "value", "branchName", "condition", "selectedBranch", "child");
        AssertNode(branchNode, 4, "branch", "2.5");
        Assert.Equal("Decision\"", branchNode.GetProperty("branchName").GetString());
        Assert.False(branchNode.GetProperty("condition").GetBoolean());
        Assert.Equal("else", branchNode.GetProperty("selectedBranch").GetString());
        Assert.Equal(5, branchNode.GetProperty("child").GetInt32());

        JsonElement round = nodes[4];
        AssertPropertyNames(round, "id", "kind", "value", "digits", "midpointRounding", "reason", "child");
        AssertNode(round, 5, "round", "2.5");
        Assert.Equal(1, round.GetProperty("digits").GetInt32());
        Assert.Equal("toEven", round.GetProperty("midpointRounding").GetString());
        Assert.Equal("Reason\\line\nnext", round.GetProperty("reason").GetString());
        Assert.Equal(6, round.GetProperty("child").GetInt32());

        JsonElement namedInput = nodes[5];
        AssertPropertyNames(namedInput, "id", "kind", "value", "name");
        AssertNode(namedInput, 6, "input", "2.500");
        Assert.Equal("Input\"Name\n", namedInput.GetProperty("name").GetString());

        JsonElement anonymousInput = nodes[6];
        AssertPropertyNames(anonymousInput, "id", "kind", "value", "name");
        AssertNode(anonymousInput, 7, "input", "3");
        Assert.Equal(JsonValueKind.Null, anonymousInput.GetProperty("name").ValueKind);
    }

    [Theory]
    [InlineData("add")]
    [InlineData("subtract")]
    [InlineData("multiply")]
    [InlineData("divide")]
    [InlineData("min")]
    [InlineData("max")]
    [Trait("RQ", "RQ-013")]
    public void ToJsonUsesTheStableBinaryOperationVocabulary(string operation)
    {
        TracedValue left = YuraiApi.Of(6m, "Left");
        TracedValue right = YuraiApi.Of(2m, "Right");
        TracedValue result = operation switch
        {
            "add" => left + right,
            "subtract" => left - right,
            "multiply" => left * right,
            "divide" => left / right,
            "min" => YuraiApi.Min(left, right),
            "max" => YuraiApi.Max(left, right),
            _ => throw new InvalidOperationException(),
        };

        using JsonDocument document = JsonDocument.Parse(result.ToJson());
        JsonElement node = document.RootElement.GetProperty("nodes")[0];

        Assert.Equal(operation, node.GetProperty("operation").GetString());
        if (operation == "min" || operation == "max")
        {
            Assert.Equal(operation == "min" ? "right" : "left", node.GetProperty("selectedOperand").GetString());
        }
        else
        {
            Assert.Equal(JsonValueKind.Null, node.GetProperty("selectedOperand").ValueKind);
        }
    }

    [Theory]
    [InlineData(true, "then", "TrueValue")]
    [InlineData(false, "else", "FalseValue")]
    [Trait("RQ", "RQ-005")]
    [Trait("RQ", "RQ-013")]
    public void ToJsonRecordsBothBranchOutcomes(
        bool condition,
        string expectedBranch,
        string expectedChildName)
    {
        TracedValue result = YuraiApi.If(
            condition,
            () => YuraiApi.Of(1m, "TrueValue"),
            () => YuraiApi.Of(2m, "FalseValue"),
            "Decision");

        using JsonDocument document = JsonDocument.Parse(result.ToJson());
        JsonElement[] nodes = document.RootElement.GetProperty("nodes").EnumerateArray().ToArray();

        Assert.Equal(condition, nodes[0].GetProperty("condition").GetBoolean());
        Assert.Equal(expectedBranch, nodes[0].GetProperty("selectedBranch").GetString());
        Assert.Equal(2, nodes[0].GetProperty("child").GetInt32());
        Assert.Equal(expectedChildName, nodes[1].GetProperty("name").GetString());
    }

    [Theory]
    [InlineData("0.00")]
    [InlineData("-0.000")]
    [InlineData("1.2300")]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("79228162514264337593543950335")]
    [Trait("RQ", "RQ-001")]
    [Trait("RQ", "RQ-013")]
    public void ToJsonPreservesDecimalValueAndScaleAsInvariantText(string valueText)
    {
        decimal value = decimal.Parse(valueText, NumberStyles.Number, CultureInfo.InvariantCulture);
        TracedValue traced = YuraiApi.Of(value);

        using JsonDocument document = JsonDocument.Parse(traced.ToJson());
        string exported = document.RootElement.GetProperty("nodes")[0].GetProperty("value").GetString()!;
        decimal parsed = decimal.Parse(exported, NumberStyles.Number, CultureInfo.InvariantCulture);

        Assert.Equal(valueText, exported);
        Assert.Equal(decimal.GetBits(value), decimal.GetBits(parsed));
    }

    [Fact]
    [Trait("RQ", "RQ-013")]
    public void ToJsonEscapesEveryJsonStringCharacterClass()
    {
        const string metadata = "quote\" slash\\ backspace\b formfeed\f newline\n carriage\r tab\t control\u0001 emoji \U0001F600";
        TracedValue traced = YuraiApi.Of(1m, metadata).Round(0, metadata).As(metadata);

        string json = traced.ToJson();
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement[] nodes = document.RootElement.GetProperty("nodes").EnumerateArray().ToArray();

        Assert.Equal(metadata, nodes[0].GetProperty("name").GetString());
        Assert.Equal(metadata, nodes[1].GetProperty("reason").GetString());
        Assert.Equal(metadata, nodes[2].GetProperty("name").GetString());
        Assert.Contains("\\\"", json, StringComparison.Ordinal);
        Assert.Contains("\\\\", json, StringComparison.Ordinal);
        Assert.Contains("\\b", json, StringComparison.Ordinal);
        Assert.Contains("\\f", json, StringComparison.Ordinal);
        Assert.Contains("\\n", json, StringComparison.Ordinal);
        Assert.Contains("\\r", json, StringComparison.Ordinal);
        Assert.Contains("\\t", json, StringComparison.Ordinal);
        Assert.Contains("\\u0001", json, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("RQ", "RQ-011")]
    [Trait("RQ", "RQ-013")]
    public void ToJsonEmitsSharedNodesOnceAndUsesDeterministicReferences()
    {
        TracedValue shared = YuraiApi.Of(10m, "Shared");
        TracedValue result = shared + shared;

        string first = result.ToJson();
        string second = result.ToJson();
        using JsonDocument document = JsonDocument.Parse(first);
        JsonElement[] nodes = document.RootElement.GetProperty("nodes").EnumerateArray().ToArray();

        Assert.Equal(first, second);
        Assert.Equal(2, nodes.Length);
        Assert.Equal(2, nodes[0].GetProperty("left").GetInt32());
        Assert.Equal(2, nodes[0].GetProperty("right").GetInt32());
    }

    [Fact]
    [Trait("RQ", "RQ-011")]
    [Trait("RQ", "RQ-013")]
    public void ToJsonHandlesAChainBeyondTenThousandNodesWithoutRecursion()
    {
        TracedValue result = YuraiApi.Of(1m, "Input");
        const int namedNodeCount = 10_001;
        for (int index = 0; index < namedNodeCount; index++)
        {
            result = result.As($"Node{index}");
        }

        using JsonDocument document = JsonDocument.Parse(result.ToJson());

        Assert.Equal(namedNodeCount + 1, document.RootElement.GetProperty("nodes").GetArrayLength());
    }

    [Fact]
    [Trait("RQ", "RQ-013")]
    public async Task ToJsonIsSafeForConcurrentReads()
    {
        TracedValue result = (YuraiApi.Of(10m, "Input") * 2m).As("Total");

        Task<string>[] reads = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(result.ToJson))
            .ToArray();

        string[] documents = await Task.WhenAll(reads);

        Assert.All(documents, json => Assert.Equal(documents[0], json));
    }

    [Fact]
    public void ToJsonRejectsAnUninitializedCarrier()
    {
        Assert.Throws<InvalidOperationException>(() => default(TracedValue).ToJson());
    }

    [Fact]
    public void JsonFormatterRejectsNullRoot()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => JsonFormatter.Render(null!));

        Assert.Equal("root", exception.ParamName);
    }

    private static int NodeId(JsonElement node) => node.GetProperty("id").GetInt32();

    private static void AssertNode(JsonElement node, int id, string kind, string value)
    {
        Assert.Equal(id, NodeId(node));
        Assert.Equal(kind, node.GetProperty("kind").GetString());
        Assert.Equal(value, node.GetProperty("value").GetString());
    }

    private static void AssertPropertyNames(JsonElement element, params string[] expected)
    {
        Assert.Equal(
            expected.OrderBy(name => name, StringComparer.Ordinal),
            element.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
    }
}
