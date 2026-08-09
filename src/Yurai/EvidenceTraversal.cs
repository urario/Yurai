using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Yurai;

internal static class EvidenceTraversal
{
    internal static IEnumerable<EvidenceNode> PreOrder(EvidenceNode root)
    {
        foreach (EvidenceVisit visit in DepthFirst(root))
        {
            if (!visit.IsReference)
            {
                yield return visit.Node;
            }
        }
    }

    internal static IEnumerable<EvidenceVisit> DepthFirst(EvidenceNode root)
    {
        if (root is null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        var identifiers = new Dictionary<EvidenceNode, int>(EvidenceNodeReferenceComparer.Instance);
        var expanded = new HashSet<EvidenceNode>(EvidenceNodeReferenceComparer.Instance);
        var pending = new Stack<TraversalFrame>();
        pending.Push(new TraversalFrame(root, 0));
        int nextIdentifier = 1;

        while (pending.Count > 0)
        {
            TraversalFrame frame = pending.Pop();
            EvidenceNode current = frame.Node;
            if (!identifiers.TryGetValue(current, out int identifier))
            {
                identifier = nextIdentifier++;
                identifiers.Add(current, identifier);
            }

            bool isReference = !expanded.Add(current);
            yield return new EvidenceVisit(current, frame.Depth, identifier, isReference);
            if (isReference)
            {
                continue;
            }

            for (int index = current.ChildCount - 1; index >= 0; index--)
            {
                pending.Push(new TraversalFrame(current.GetChild(index), frame.Depth + 1));
            }
        }
    }

    private readonly struct TraversalFrame
    {
        internal TraversalFrame(EvidenceNode node, int depth)
        {
            Node = node;
            Depth = depth;
        }

        internal EvidenceNode Node { get; }

        internal int Depth { get; }
    }

    private sealed class EvidenceNodeReferenceComparer : IEqualityComparer<EvidenceNode>
    {
        internal static readonly EvidenceNodeReferenceComparer Instance = new();

        private EvidenceNodeReferenceComparer()
        {
        }

        public bool Equals(EvidenceNode? x, EvidenceNode? y) => ReferenceEquals(x, y);

        public int GetHashCode(EvidenceNode obj) => RuntimeHelpers.GetHashCode(obj);
    }
}

internal readonly struct EvidenceVisit
{
    internal EvidenceVisit(EvidenceNode node, int depth, int identifier, bool isReference)
    {
        Node = node;
        Depth = depth;
        Identifier = identifier;
        IsReference = isReference;
    }

    internal EvidenceNode Node { get; }

    internal int Depth { get; }

    internal int Identifier { get; }

    internal bool IsReference { get; }
}
