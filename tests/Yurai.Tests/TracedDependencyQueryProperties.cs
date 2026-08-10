using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;

namespace Yurai.Tests;

public sealed class TracedDependencyQueryProperties
{
    private static readonly string[] GeneratedNames = ["A", "B", "C"];
    private static readonly string[] QueryNames = [.. GeneratedNames, "Missing"];

    [Fact]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-014")]
    public void PublicQueriesMatchAnIndependentlyGeneratedGraphModel()
    {
        Gen.Int.Sample(seed =>
        {
            GeneratedGraph graph = GenerateGraph(seed);

            foreach (string name in QueryNames)
            {
                string[][] expectedPaths = FindPaths(graph.ModelRoot, name).ToArray();

                Assert.Equal(expectedPaths.Length > 0, graph.Result.DependsOn(name));
                Assert.Equal(expectedPaths, graph.Result.Trace(name).Select(path => path.ToArray()).ToArray());
            }

            Assert.Equal(graph.ExpectedInputs, graph.Result.Inputs);
        });
    }

    private static GeneratedGraph GenerateGraph(int seed)
    {
        var random = new DeterministicRandom(seed);
        var available = new List<GeneratedNode>();
        GeneratedNode current = CreateInput(random);
        available.Add(current);

        int operationCount = 1 + random.Next(8);
        for (int index = 0; index < operationCount; index++)
        {
            GeneratedNode right;
            if (random.Next(3) == 0)
            {
                right = CreateInput(random);
                available.Add(right);
            }
            else
            {
                right = available[random.Next(available.Count)];
            }

            TracedValue result = current.Value + right.Value;
            ModelNode model = ModelNode.Operation(current.Model, right.Model);
            if (random.Next(2) == 0)
            {
                string name = GeneratedNames[random.Next(GeneratedNames.Length)];
                result = result.As(name);
                model = ModelNode.Named(name, model);
            }

            current = new GeneratedNode(result, model);
            available.Add(current);
        }

        return new GeneratedGraph(current.Value, current.Model, FindExpectedInputs(current.Model));
    }

    private static GeneratedNode CreateInput(DeterministicRandom random)
    {
        string? name = random.Next(4) == 0
            ? null
            : GeneratedNames[random.Next(GeneratedNames.Length)];
        decimal value = random.Next(10);
        TracedValue traced = name is null ? TracedValue.Of(value) : TracedValue.Of(value, name);
        return new GeneratedNode(traced, ModelNode.Input(name));
    }

    private static IEnumerable<string[]> FindPaths(ModelNode root, string name)
    {
        var pending = new Stack<(ModelNode Node, string[] RootFirstNames)>();
        pending.Push((root, []));

        while (pending.Count > 0)
        {
            (ModelNode node, string[] rootFirstNames) = pending.Pop();
            string[] names = node.Name is null ? rootFirstNames : [.. rootFirstNames, node.Name];
            if (string.Equals(node.Name, name, StringComparison.Ordinal))
            {
                yield return names.Reverse().ToArray();
            }

            for (int index = node.Children.Length - 1; index >= 0; index--)
            {
                pending.Push((node.Children[index], names));
            }
        }
    }

    private static string[] FindExpectedInputs(ModelNode root)
    {
        var inputs = new List<string>();
        var seenNodes = new HashSet<ModelNode>();
        var seenNames = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<ModelNode>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            ModelNode node = pending.Pop();
            if (!seenNodes.Add(node))
            {
                continue;
            }

            if (node.IsInput && node.Name is not null && seenNames.Add(node.Name))
            {
                inputs.Add(node.Name);
            }

            for (int index = node.Children.Length - 1; index >= 0; index--)
            {
                pending.Push(node.Children[index]);
            }
        }

        return inputs.ToArray();
    }

    private sealed class GeneratedGraph
    {
        internal GeneratedGraph(TracedValue result, ModelNode modelRoot, string[] expectedInputs)
        {
            Result = result;
            ModelRoot = modelRoot;
            ExpectedInputs = expectedInputs;
        }

        internal TracedValue Result { get; }

        internal ModelNode ModelRoot { get; }

        internal string[] ExpectedInputs { get; }
    }

    private sealed class GeneratedNode
    {
        internal GeneratedNode(TracedValue value, ModelNode model)
        {
            Value = value;
            Model = model;
        }

        internal TracedValue Value { get; }

        internal ModelNode Model { get; }
    }

    private sealed class ModelNode
    {
        private ModelNode(string? name, bool isInput, ModelNode[] children)
        {
            Name = name;
            IsInput = isInput;
            Children = children;
        }

        internal string? Name { get; }

        internal bool IsInput { get; }

        internal ModelNode[] Children { get; }

        internal static ModelNode Input(string? name) => new(name, true, []);

        internal static ModelNode Operation(ModelNode left, ModelNode right) => new(null, false, [left, right]);

        internal static ModelNode Named(string name, ModelNode child) => new(name, false, [child]);
    }

    private sealed class DeterministicRandom
    {
        private uint state;

        internal DeterministicRandom(int seed)
        {
            state = unchecked((uint)seed) ^ 0x9E3779B9u;
            if (state == 0)
            {
                state = 0xA341316Cu;
            }
        }

        internal int Next(int exclusiveMaximum) => (int)(NextUInt32() % (uint)exclusiveMaximum);

        private uint NextUInt32()
        {
            uint value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value;
            return value;
        }
    }
}
