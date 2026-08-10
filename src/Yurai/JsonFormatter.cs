using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Yurai;

internal static class JsonFormatter
{
    internal static string Render(EvidenceNode root)
    {
        if (root is null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        var nodes = new List<EvidenceVisit>();
        var identifiers = new Dictionary<EvidenceNode, int>(EvidenceNodeReferenceComparer.Instance);
        foreach (EvidenceVisit visit in EvidenceTraversal.DepthFirst(root))
        {
            if (visit.IsReference)
            {
                continue;
            }

            nodes.Add(visit);
            identifiers.Add(visit.Node, visit.Identifier);
        }

        var builder = new StringBuilder();
        builder.Append("{\"schemaVersion\":1,\"root\":");
        AppendInteger(builder, identifiers[root]);
        builder.Append(",\"nodes\":[");

        var writer = new NodeWriter(builder, identifiers);
        for (int index = 0; index < nodes.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            EvidenceVisit visit = nodes[index];
            writer.Identifier = visit.Identifier;
            visit.Node.Accept(writer);
        }

        return builder.Append("]}").ToString();
    }

    private static void AppendInteger(StringBuilder builder, int value) =>
        builder.Append(value.ToString(CultureInfo.InvariantCulture));

    private static string FormatDecimal(decimal value)
    {
        string text = value.ToString(CultureInfo.InvariantCulture);
        if (value != decimal.Zero)
        {
            return text;
        }

        int flags = decimal.GetBits(value)[3];
        bool isNegativeZero = (flags & int.MinValue) != 0;
        return isNegativeZero && !text.StartsWith("-", StringComparison.Ordinal) ? "-" + text : text;
    }

    private static void AppendString(StringBuilder builder, string value)
    {
        builder.Append('"');
        foreach (char character in value)
        {
            switch (character)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (character < ' ' || char.IsSurrogate(character))
                    {
                        builder.Append("\\u")
                            .Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        builder.Append('"');
    }

    private sealed class NodeWriter : IEvidenceNodeVisitor
    {
        private readonly StringBuilder builder;
        private readonly IReadOnlyDictionary<EvidenceNode, int> identifiers;

        internal NodeWriter(
            StringBuilder builder,
            IReadOnlyDictionary<EvidenceNode, int> identifiers)
        {
            this.builder = builder;
            this.identifiers = identifiers;
        }

        internal int Identifier { get; set; }

        public void Visit(InputEvidenceNode node)
        {
            AppendCommon("input", node.Value);
            builder.Append(",\"name\":");
            if (node.Name is null)
            {
                builder.Append("null");
            }
            else
            {
                AppendString(builder, node.Name);
            }

            builder.Append('}');
        }

        public void Visit(BinaryOperationEvidenceNode node)
        {
            AppendCommon("binaryOperation", node.Value);
            builder.Append(",\"operation\":");
            AppendString(builder, OperationName(node.Operation));
            builder.Append(",\"left\":");
            AppendInteger(builder, identifiers[node.Left]);
            builder.Append(",\"right\":");
            AppendInteger(builder, identifiers[node.Right]);
            builder.Append(",\"selectedOperand\":");
            if (node.SelectedOperand == SelectedOperand.None)
            {
                builder.Append("null");
            }
            else
            {
                AppendString(builder, SelectedOperandName(node.SelectedOperand));
            }

            builder.Append('}');
        }

        public void Visit(RoundEvidenceNode node)
        {
            AppendCommon("round", node.Value);
            builder.Append(",\"digits\":");
            AppendInteger(builder, node.Digits);
            builder.Append(",\"midpointRounding\":");
            AppendString(builder, RoundingName(node.Rounding));
            builder.Append(",\"reason\":");
            AppendString(builder, node.Reason);
            builder.Append(",\"child\":");
            AppendInteger(builder, identifiers[node.Child]);
            builder.Append('}');
        }

        public void Visit(BranchEvidenceNode node)
        {
            AppendCommon("branch", node.Value);
            builder.Append(",\"branchName\":");
            AppendString(builder, node.DecisionName);
            builder.Append(",\"condition\":")
                .Append(node.Condition ? "true" : "false")
                .Append(",\"selectedBranch\":");
            AppendString(builder, node.SelectedBranch == BranchSelection.Then ? "then" : "else");
            builder.Append(",\"child\":");
            AppendInteger(builder, identifiers[node.Child]);
            builder.Append('}');
        }

        public void Visit(NamedEvidenceNode node)
        {
            AppendCommon("named", node.Value);
            builder.Append(",\"name\":");
            AppendString(builder, node.Name);
            builder.Append(",\"child\":");
            AppendInteger(builder, identifiers[node.Child]);
            builder.Append('}');
        }

        private void AppendCommon(string kind, decimal value)
        {
            builder.Append("{\"id\":");
            AppendInteger(builder, Identifier);
            builder.Append(",\"kind\":");
            AppendString(builder, kind);
            builder.Append(",\"value\":");
            AppendString(builder, FormatDecimal(value));
        }

        private static string OperationName(BinaryOperationKind operation) => operation switch
        {
            BinaryOperationKind.Add => "add",
            BinaryOperationKind.Subtract => "subtract",
            BinaryOperationKind.Multiply => "multiply",
            BinaryOperationKind.Divide => "divide",
            BinaryOperationKind.Min => "min",
            BinaryOperationKind.Max => "max",
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };

        private static string SelectedOperandName(SelectedOperand selectedOperand) => selectedOperand switch
        {
            SelectedOperand.Left => "left",
            SelectedOperand.Right => "right",
            _ => throw new ArgumentOutOfRangeException(nameof(selectedOperand)),
        };

        private static string RoundingName(MidpointRounding rounding) => rounding switch
        {
            MidpointRounding.ToEven => "toEven",
            _ => throw new ArgumentOutOfRangeException(nameof(rounding)),
        };
    }
}
