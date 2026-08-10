using System.Collections.Generic;

namespace Yurai;

internal static class DependencyQuery
{
    internal static bool DependsOn(EvidenceNode root, string name)
    {
        foreach (EvidenceNode node in EvidenceTraversal.PreOrder(root))
        {
            if (string.Equals(GetName(node), name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    internal static IReadOnlyList<string> GetInputs(EvidenceNode root)
    {
        var inputs = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (EvidenceNode node in EvidenceTraversal.PreOrder(root))
        {
            if (node is InputEvidenceNode { Name: not null } input && seen.Add(input.Name))
            {
                inputs.Add(input.Name);
            }
        }

        return inputs.AsReadOnly();
    }

    internal static IReadOnlyList<IReadOnlyList<string>> Trace(EvidenceNode root, string name)
    {
        var paths = new List<IReadOnlyList<string>>();
        var rootFirstNames = new List<string>();
        var pending = new Stack<PathFrame>();
        pending.Push(PathFrame.Enter(root));

        while (pending.Count > 0)
        {
            PathFrame frame = pending.Pop();
            if (frame.IsExit)
            {
                if (frame.RemovesName)
                {
                    rootFirstNames.RemoveAt(rootFirstNames.Count - 1);
                }

                continue;
            }

            EvidenceNode node = frame.Node;
            string? nodeName = GetName(node);
            bool hasName = nodeName is not null;
            if (hasName)
            {
                rootFirstNames.Add(nodeName!);
            }

            if (string.Equals(nodeName, name, StringComparison.Ordinal))
            {
                string[] path = rootFirstNames.ToArray();
                Array.Reverse(path);
                paths.Add(Array.AsReadOnly(path));
            }

            pending.Push(PathFrame.Exit(node, hasName));
            for (int index = node.ChildCount - 1; index >= 0; index--)
            {
                pending.Push(PathFrame.Enter(node.GetChild(index)));
            }
        }

        return paths.AsReadOnly();
    }

    private static string? GetName(EvidenceNode node) => node switch
    {
        InputEvidenceNode input => input.Name,
        NamedEvidenceNode named => named.Name,
        _ => null,
    };

    private readonly struct PathFrame
    {
        private PathFrame(EvidenceNode node)
        {
            Node = node;
            IsExit = false;
            // Stryker disable once Boolean: RemovesName is ignored until an exit frame is processed.
            RemovesName = false;
        }

        private PathFrame(EvidenceNode node, bool removesName)
        {
            Node = node;
            IsExit = true;
            RemovesName = removesName;
        }

        internal EvidenceNode Node { get; }

        internal bool IsExit { get; }

        internal bool RemovesName { get; }

        internal static PathFrame Enter(EvidenceNode node) => new(node);

        internal static PathFrame Exit(EvidenceNode node, bool removesName) => new(node, removesName);
    }
}
