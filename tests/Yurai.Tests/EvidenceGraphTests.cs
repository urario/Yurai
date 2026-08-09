using System.Reflection;
using Xunit;

namespace Yurai.Tests;

public sealed class EvidenceGraphTests
{
    [Fact]
    [Trait("RQ", "RQ-011")]
    public void NodeKindsAreSealedAndImmutable()
    {
        Type[] nodeTypes =
        [
            typeof(InputEvidenceNode),
            typeof(BinaryOperationEvidenceNode),
            typeof(NamedEvidenceNode),
        ];

        foreach (Type nodeType in nodeTypes)
        {
            Assert.True(nodeType.IsSealed);
            Assert.All(
                nodeType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
                field => Assert.True(field.IsInitOnly, $"{nodeType.Name}.{field.Name} must be readonly."));
            Assert.All(
                nodeType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
                property => Assert.Null(property.SetMethod));
        }
    }

    [Fact]
    [Trait("RQ", "RQ-011")]
    public void FixedChildrenPreserveSourceOrderAndSharedReferences()
    {
        var shared = new InputEvidenceNode(10m, "Shared");
        var left = new NamedEvidenceNode(shared, "Left");
        var right = new NamedEvidenceNode(shared, "Right");
        var root = new BinaryOperationEvidenceNode(20m, BinaryOperationKind.Add, left, right);

        Assert.Equal(2, root.ChildCount);
        Assert.Same(left, root.GetChild(0));
        Assert.Same(right, root.GetChild(1));
        Assert.Same(shared, left.GetChild(0));
        Assert.Same(shared, right.GetChild(0));
        Assert.Equal(shared.Value, left.Value);
        Assert.Equal(shared.Value, right.Value);
    }

    [Fact]
    [Trait("RQ", "RQ-011")]
    public void ConstructorsRejectNullChildren()
    {
        var input = new InputEvidenceNode(1m, null);

        var named = Assert.Throws<ArgumentNullException>(() => new NamedEvidenceNode(null!, "Name"));
        var left = Assert.Throws<ArgumentNullException>(
            () => new BinaryOperationEvidenceNode(1m, BinaryOperationKind.Add, null!, input));
        var right = Assert.Throws<ArgumentNullException>(
            () => new BinaryOperationEvidenceNode(1m, BinaryOperationKind.Add, input, null!));

        Assert.Equal("child", named.ParamName);
        Assert.Equal("left", left.ParamName);
        Assert.Equal("right", right.ParamName);
    }

    [Fact]
    public void ChildAccessRejectsAnInvalidIndex()
    {
        var input = new InputEvidenceNode(1m, null);
        var named = new NamedEvidenceNode(input, "Name");

        Assert.Throws<ArgumentOutOfRangeException>(() => { input.GetChild(0); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { named.GetChild(-1); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { named.GetChild(1); });
    }
}
