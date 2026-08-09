using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Yurai;

internal static class EvidenceTraversal
{
    internal static IEnumerable<EvidenceNode> PreOrder(EvidenceNode root)
    {
        if (root is null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        var visited = new HashSet<EvidenceNode>(EvidenceNodeReferenceComparer.Instance);
        var pending = new Stack<EvidenceNode>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            EvidenceNode current = pending.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            yield return current;

            for (int index = current.ChildCount - 1; index >= 0; index--)
            {
                pending.Push(current.GetChild(index));
            }
        }
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
