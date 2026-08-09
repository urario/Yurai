using Xunit;

namespace Yurai.Tests;

public sealed class EvidenceTraversalTests
{
    [Fact]
    [Trait("RQ", "RQ-011")]
    public void PreOrderVisitsADiamondOnceInRootFirstLeftToRightOrder()
    {
        var shared = new InputEvidenceNode(10m, "Shared");
        var left = new NamedEvidenceNode(shared, "Left");
        var right = new NamedEvidenceNode(shared, "Right");
        var root = new BinaryOperationEvidenceNode(20m, BinaryOperationKind.Add, left, right);

        EvidenceNode[] visited = EvidenceTraversal.PreOrder(root).ToArray();

        Assert.Equal([root, left, shared, right], visited);
    }

    [Fact]
    [Trait("RQ", "RQ-011")]
    public void PreOrderUsesReferenceIdentityRatherThanValueEquality()
    {
        var left = new InputEvidenceNode(1m, "Same");
        var right = new InputEvidenceNode(1m, "Same");
        var root = new BinaryOperationEvidenceNode(2m, BinaryOperationKind.Add, left, right);

        EvidenceNode[] visited = EvidenceTraversal.PreOrder(root).ToArray();

        Assert.Equal([root, left, right], visited);
    }

    [Fact]
    [Trait("RQ", "RQ-011")]
    public void PreOrderHandlesAChainBeyondTenThousandNodesWithoutRecursion()
    {
        EvidenceNode root = new InputEvidenceNode(1m, "Input");
        const int namedNodeCount = 10_001;
        for (int index = 0; index < namedNodeCount; index++)
        {
            root = new NamedEvidenceNode(root, $"Node{index}");
        }

        int visitedCount = EvidenceTraversal.PreOrder(root).Count();

        Assert.Equal(namedNodeCount + 1, visitedCount);
    }

    [Fact]
    [Trait("RQ", "RQ-011")]
    public async Task ImmutableGraphCanBeTraversedConcurrently()
    {
        var left = new InputEvidenceNode(2m, "Left");
        var right = new InputEvidenceNode(3m, "Right");
        var root = new BinaryOperationEvidenceNode(6m, BinaryOperationKind.Multiply, left, right);

        Task<EvidenceNode[]>[] reads = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(() => EvidenceTraversal.PreOrder(root).ToArray()))
            .ToArray();

        EvidenceNode[][] results = await Task.WhenAll(reads);

        Assert.All(results, result => Assert.Equal([root, left, right], result));
    }

    [Fact]
    public void PreOrderRejectsNullRoot()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => { EvidenceTraversal.PreOrder(null!).ToArray(); });

        Assert.Equal("root", exception.ParamName);
    }
}
