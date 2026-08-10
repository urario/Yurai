using CsCheck;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class TracedDependencyQueryProperties
{
    [Fact]
    [Trait("Category", "Property")]
    [Trait("RQ", "RQ-014")]
    public void PublicQueriesMatchASmallGraphOracle()
    {
        Gen.Int.Sample(seed =>
        {
            string rightName = (seed & 1) == 0 ? "Right" : "Input";
            TracedValue left = YuraiApi.Of(seed, "Input");
            TracedValue right = YuraiApi.Of(seed + 1m, rightName);
            TracedValue shared = (left + right).As("Shared");
            TracedValue other = (seed & 2) == 0 ? shared : right;
            TracedValue result = (shared * other).As("Result");

            foreach (string name in new[] { "Input", "Right", "Shared", "Result", "Missing" })
            {
                string[][] expectedPaths = FindPaths(result.Root, name).ToArray();

                Assert.Equal(expectedPaths.Length > 0, result.DependsOn(name));
                Assert.Equal(expectedPaths, result.Trace(name).Select(path => path.ToArray()).ToArray());
            }

            Assert.Equal(FindInputs(result.Root), result.Inputs);
        });
    }

    private static IEnumerable<string[]> FindPaths(EvidenceNode root, string name)
    {
        var pending = new Stack<(EvidenceNode Node, string[] RootFirstNames)>();
        pending.Push((root, []));

        while (pending.Count > 0)
        {
            (EvidenceNode node, string[] rootFirstNames) = pending.Pop();
            string? nodeName = GetName(node);
            string[] names = nodeName is null ? rootFirstNames : [.. rootFirstNames, nodeName];
            if (string.Equals(nodeName, name, StringComparison.Ordinal))
            {
                yield return names.Reverse().ToArray();
            }

            for (int index = node.ChildCount - 1; index >= 0; index--)
            {
                pending.Push((node.GetChild(index), names));
            }
        }
    }

    private static string[] FindInputs(EvidenceNode root)
    {
        var names = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (EvidenceNode node in EvidenceTraversal.PreOrder(root))
        {
            if (node is InputEvidenceNode { Name: not null } input && seen.Add(input.Name))
            {
                names.Add(input.Name);
            }
        }

        return names.ToArray();
    }

    private static string? GetName(EvidenceNode node) => node switch
    {
        InputEvidenceNode input => input.Name,
        NamedEvidenceNode named => named.Name,
        _ => null,
    };
}
