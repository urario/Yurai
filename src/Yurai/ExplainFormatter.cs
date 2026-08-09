using System.Collections.Generic;
using System.Globalization;
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
        builder.Append("Result\n  ").Append(FormatValue(root.Value)).Append("\nDerivation\n");

        var visits = new List<EvidenceVisit>(EvidenceTraversal.DepthFirst(root));
        var sharedIdentifiers = new HashSet<int>();
        foreach (EvidenceVisit visit in visits)
        {
            if (visit.IsReference)
            {
                sharedIdentifiers.Add(visit.Identifier);
            }
        }

        var descriptionWriter = new DescriptionWriter(builder);
        foreach (EvidenceVisit visit in visits)
        {
            AppendIndent(builder, visit.Depth + 1);
            if (visit.IsReference)
            {
                builder.Append("<ref #").Append(visit.Identifier).Append(">\n");
                continue;
            }

            if (sharedIdentifiers.Contains(visit.Identifier))
            {
                builder.Append("[#").Append(visit.Identifier).Append("] ");
            }

            visit.Node.Accept(descriptionWriter);
            builder.Append('\n');
        }

        builder.Length--;
        return builder.ToString();
    }

    private static string FormatValue(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Escape(string value, bool quoted)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            switch (character)
            {
                case '\\':
                    builder.Append(quoted ? "\\\\" : "\\");
                    break;
                case '"':
                    builder.Append(quoted ? "\\\"" : "\"");
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

    private sealed class DescriptionWriter : IEvidenceNodeVisitor
    {
        private readonly StringBuilder builder;

        internal DescriptionWriter(StringBuilder builder)
        {
            this.builder = builder;
        }

        public void Visit(InputEvidenceNode node)
        {
            if (node.Name is not null)
            {
                builder.Append(Escape(node.Name, quoted: false)).Append(" = ");
            }

            builder.Append(FormatValue(node.Value));
        }

        public void Visit(BinaryOperationEvidenceNode node)
        {
            builder.Append(node.Operation).Append(" = ").Append(FormatValue(node.Value));
        }

        public void Visit(RoundEvidenceNode node)
        {
            builder.Append("Round(digits: ")
                .Append(node.Digits)
                .Append(", reason: \"")
                .Append(Escape(node.Reason, quoted: true))
                .Append("\") = ")
                .Append(FormatValue(node.Value));
        }

        public void Visit(BranchEvidenceNode node)
        {
            builder.Append("If(name: \"")
                .Append(Escape(node.DecisionName, quoted: true))
                .Append("\", branch: \"")
                .Append(node.SelectedBranch == BranchSelection.Then ? "then" : "else")
                .Append("\") = ")
                .Append(FormatValue(node.Value));
        }

        public void Visit(NamedEvidenceNode node)
        {
            builder.Append(Escape(node.Name, quoted: false)).Append(" = ").Append(FormatValue(node.Value));
        }
    }
}
