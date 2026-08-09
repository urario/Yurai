namespace Yurai;

internal abstract class EvidenceNode
{
    protected EvidenceNode(decimal value)
    {
        Value = value;
    }

    internal decimal Value { get; }

    internal abstract int ChildCount { get; }

    internal abstract EvidenceNode GetChild(int index);
}

internal sealed class InputEvidenceNode : EvidenceNode
{
    internal InputEvidenceNode(decimal value, string? name)
        : base(value)
    {
        Name = name;
    }

    internal string? Name { get; }

    internal override int ChildCount => 0;

    internal override EvidenceNode GetChild(int index) =>
        throw new ArgumentOutOfRangeException(nameof(index));
}

internal enum BinaryOperationKind
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Min,
    Max,
}

internal enum SelectedOperand
{
    None,
    Left,
    Right,
}

internal sealed class BinaryOperationEvidenceNode : EvidenceNode
{
    internal BinaryOperationEvidenceNode(
        decimal value,
        BinaryOperationKind operation,
        EvidenceNode left,
        EvidenceNode right,
        SelectedOperand selectedOperand = SelectedOperand.None)
        : base(value)
    {
        Operation = operation;
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
        SelectedOperand = selectedOperand;
    }

    internal BinaryOperationKind Operation { get; }

    internal EvidenceNode Left { get; }

    internal EvidenceNode Right { get; }

    internal SelectedOperand SelectedOperand { get; }

    internal override int ChildCount => 2;

    internal override EvidenceNode GetChild(int index) => index switch
    {
        0 => Left,
        1 => Right,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}

internal sealed class RoundEvidenceNode : EvidenceNode
{
    internal RoundEvidenceNode(
        decimal value,
        int digits,
        MidpointRounding rounding,
        string reason,
        EvidenceNode child)
        : base(value)
    {
        Digits = digits;
        Rounding = rounding;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        Child = child ?? throw new ArgumentNullException(nameof(child));
    }

    internal int Digits { get; }

    internal MidpointRounding Rounding { get; }

    internal string Reason { get; }

    internal EvidenceNode Child { get; }

    internal override int ChildCount => 1;

    internal override EvidenceNode GetChild(int index) => index == 0
        ? Child
        : throw new ArgumentOutOfRangeException(nameof(index));
}

internal enum BranchSelection
{
    Then,
    Else,
}

internal sealed class BranchEvidenceNode : EvidenceNode
{
    internal BranchEvidenceNode(
        EvidenceNode child,
        string decisionName,
        bool condition,
        BranchSelection selectedBranch)
        : base((child ?? throw new ArgumentNullException(nameof(child))).Value)
    {
        Child = child;
        DecisionName = decisionName ?? throw new ArgumentNullException(nameof(decisionName));
        Condition = condition;
        SelectedBranch = selectedBranch;
    }

    internal string DecisionName { get; }

    internal bool Condition { get; }

    internal BranchSelection SelectedBranch { get; }

    internal EvidenceNode Child { get; }

    internal override int ChildCount => 1;

    internal override EvidenceNode GetChild(int index) => index == 0
        ? Child
        : throw new ArgumentOutOfRangeException(nameof(index));
}

internal sealed class NamedEvidenceNode : EvidenceNode
{
    internal NamedEvidenceNode(EvidenceNode child, string name)
        : base((child ?? throw new ArgumentNullException(nameof(child))).Value)
    {
        Child = child;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    internal string Name { get; }

    internal EvidenceNode Child { get; }

    internal override int ChildCount => 1;

    internal override EvidenceNode GetChild(int index) => index == 0
        ? Child
        : throw new ArgumentOutOfRangeException(nameof(index));
}
