using System.Globalization;

namespace Issue79Spike.Contracts;

public readonly struct Traced<T>
{
    private readonly EvidenceNode<T>? root;

    internal Traced(EvidenceNode<T> root)
    {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public T Value => GetRoot().Value;

    internal EvidenceNode<T> Root => GetRoot();

    public static Traced<T> operator +(Traced<T> left, Traced<T> right) => CreateBinary(left, right, BinaryOperation.Add);

    public static Traced<T> operator +(Traced<T> left, T right) => CreateBinary(left, CreateAnonymous(right), BinaryOperation.Add);

    public static Traced<T> operator +(T left, Traced<T> right) => CreateBinary(CreateAnonymous(left), right, BinaryOperation.Add);

    public static Traced<T> operator -(Traced<T> left, Traced<T> right) => CreateBinary(left, right, BinaryOperation.Subtract);

    public static Traced<T> operator -(Traced<T> left, T right) => CreateBinary(left, CreateAnonymous(right), BinaryOperation.Subtract);

    public static Traced<T> operator -(T left, Traced<T> right) => CreateBinary(CreateAnonymous(left), right, BinaryOperation.Subtract);

    public static Traced<T> operator *(Traced<T> left, Traced<T> right) => CreateBinary(left, right, BinaryOperation.Multiply);

    public static Traced<T> operator *(Traced<T> left, T right) => CreateBinary(left, CreateAnonymous(right), BinaryOperation.Multiply);

    public static Traced<T> operator *(T left, Traced<T> right) => CreateBinary(CreateAnonymous(left), right, BinaryOperation.Multiply);

    public static Traced<T> operator /(Traced<T> left, Traced<T> right) => CreateBinary(left, right, BinaryOperation.Divide);

    public static Traced<T> operator /(Traced<T> left, T right) => CreateBinary(left, CreateAnonymous(right), BinaryOperation.Divide);

    public static Traced<T> operator /(T left, Traced<T> right) => CreateBinary(CreateAnonymous(left), right, BinaryOperation.Divide);

    private static Traced<T> CreateAnonymous(T value) => new(new InputEvidenceNode<T>(value, null));

    private static Traced<T> CreateBinary(Traced<T> left, Traced<T> right, BinaryOperation operation)
    {
        EvidenceNode<T> leftRoot = left.GetRoot();
        EvidenceNode<T> rightRoot = right.GetRoot();
        ValueOperations<T> operations = ValueBinding<T>.Operations;
        T value = operation switch
        {
            BinaryOperation.Add => operations.Add(leftRoot.Value, rightRoot.Value),
            BinaryOperation.Subtract => operations.Subtract(leftRoot.Value, rightRoot.Value),
            BinaryOperation.Multiply => operations.Multiply(leftRoot.Value, rightRoot.Value),
            BinaryOperation.Divide => operations.Divide(leftRoot.Value, rightRoot.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        return new Traced<T>(new BinaryEvidenceNode<T>(value, leftRoot, rightRoot));
    }

    private EvidenceNode<T> GetRoot() => root ?? throw new InvalidOperationException("The traced value is uninitialized.");
}

public static class Traced
{
    public static Traced<decimal> Of(decimal value) => Of(value, null);

    public static Traced<decimal> Of(decimal value, string? name) => new(new InputEvidenceNode<decimal>(value, name));

    public static Traced<long> OfInt64(long value) => OfInt64(value, null);

    public static Traced<long> OfInt64(long value, string? name) => new(new InputEvidenceNode<long>(value, name));

    public static Traced<T> If<T>(bool condition, Func<Traced<T>> whenTrue, Func<Traced<T>> whenFalse, string branchName)
    {
        if (whenTrue is null) throw new ArgumentNullException(nameof(whenTrue));
        if (whenFalse is null) throw new ArgumentNullException(nameof(whenFalse));
        if (branchName is null) throw new ArgumentNullException(nameof(branchName));
        return condition ? whenTrue() : whenFalse();
    }

    public static Traced<decimal> Round(this Traced<decimal> value, int digits, string reason)
    {
        if (reason is null) throw new ArgumentNullException(nameof(reason));
        return new Traced<decimal>(new InputEvidenceNode<decimal>(decimal.Round(value.Value, digits, MidpointRounding.ToEven), null));
    }

    public static string ToJsonV1(this Traced<decimal> value) => value.Value.ToString(CultureInfo.InvariantCulture);
}

internal enum BinaryOperation
{
    Add,
    Subtract,
    Multiply,
    Divide,
}
