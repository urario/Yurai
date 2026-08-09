using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Yurai;

internal static class ExplainFormatter
{
    internal static string Render(EvidenceNode root)
    {
        if (root is null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        var builder = new StringBuilder();
        builder.AppendLine("Result");
        builder.Append("  ").AppendLine(FormatValue(root.Value));
        builder.AppendLine("Derivation");

        var identifiers = new Dictionary<EvidenceNode, int>(EvidenceNodeReferenceComparer.Instance);
        var expanded = new HashSet<EvidenceNode>(EvidenceNodeReferenceComparer.Instance);
        var pending = new Stack<Frame>();
        pending.Push(new Frame(root, 1));
        int nextIdentifier = 1;

        while (pending.Count > 0)
        {
            Frame frame = pending.Pop();
            EvidenceNode current = frame.Node;
            if (!identifiers.TryGetValue(current, out int identifier))
            {
                identifier = nextIdentifier++;
                identifiers.Add(current, identifier);
            }

            AppendIndent(builder, frame.Depth);
            if (!expanded.Add(current))
            {
                builder.Append("<ref #").Append(identifier).AppendLine(">");
                continue;
            }

            AppendDescription(builder, current);
            builder.AppendLine();

            for (int childIndex = current.ChildCount - 1; childIndex >= 0; childIndex--)
            {
                pending.Push(new Frame(current.GetChild(childIndex), frame.Depth + 1));
            }
        }

        return builder.ToString().TrimEnd('\r', '\n');
    }

    private static void AppendDescription(StringBuilder builder, EvidenceNode node)
    {
        switch (node)
        {
            case InputEvidenceNode input:
                if (input.Name is not null)
                {
                    builder.Append(Escape(input.Name)).Append(" = ");
                }

                builder.Append(FormatValue(node.Value));
                break;
            case NamedEvidenceNode named:
                builder.Append(Escape(named.Name)).Append(" = ").Append(FormatValue(node.Value));
                break;
            case BinaryOperationEvidenceNode binary:
                builder.Append(binary.Operation).Append(" = ").Append(FormatValue(node.Value));
                break;
            case RoundEvidenceNode round:
                builder.Append("Round(digits: ")
                    .Append(round.Digits)
                    .Append(", reason: \"")
                    .Append(Escape(round.Reason))
                    .Append("\") = ")
                    .Append(FormatValue(node.Value));
                break;
            case BranchEvidenceNode branch:
                builder.Append("If(name: \"")
                    .Append(Escape(branch.DecisionName))
                    .Append("\", branch: \"")
                    .Append(branch.SelectedBranch == BranchSelection.Then ? "then" : "else")
                    .Append("\") = ")
                    .Append(FormatValue(node.Value));
                break;
            default:
                throw new InvalidOperationException($"Unsupported evidence node type: {node.GetType().Name}.");
        }
    }

    private static string FormatValue(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            switch (character)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (char.IsControl(character))
                    {
                        builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    private static void AppendIndent(StringBuilder builder, int depth)
    {
        builder.Append(' ', depth * 2);
    }

    private readonly struct Frame
    {
        internal Frame(EvidenceNode node, int depth)
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
