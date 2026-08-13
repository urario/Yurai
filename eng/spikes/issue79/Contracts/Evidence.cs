namespace Issue79Spike.Contracts;

internal abstract class EvidenceNode<T>
{
    private static long createdCount;

    protected EvidenceNode(T value)
    {
        Value = value;
        createdCount++;
    }

    internal T Value { get; }

    internal static long CreatedCount => createdCount;
}

internal sealed class InputEvidenceNode<T> : EvidenceNode<T>
{
    internal InputEvidenceNode(T value, string? name)
        : base(value)
    {
        Name = name;
    }

    internal string? Name { get; }
}

internal sealed class BinaryEvidenceNode<T> : EvidenceNode<T>
{
    internal BinaryEvidenceNode(T value, EvidenceNode<T> left, EvidenceNode<T> right)
        : base(value)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    internal EvidenceNode<T> Left { get; }

    internal EvidenceNode<T> Right { get; }
}

internal static class EvidenceProbe
{
    internal static long DecimalCreatedCount => EvidenceNode<decimal>.CreatedCount;

    internal static long Int64CreatedCount => EvidenceNode<long>.CreatedCount;

    internal static Traced<decimal> DecimalBinaryWithoutBinding(Traced<decimal> left, Traced<decimal> right, decimal value) =>
        new(new BinaryEvidenceNode<decimal>(value, left.Root, right.Root));

    internal static Traced<long> Int64BinaryWithoutBinding(Traced<long> left, Traced<long> right, long value) =>
        new(new BinaryEvidenceNode<long>(value, left.Root, right.Root));
}
