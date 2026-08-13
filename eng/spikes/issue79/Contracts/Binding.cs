namespace Issue79Spike.Contracts;

internal abstract class ValueOperations<T>
{
    internal abstract T Add(T left, T right);

    internal abstract T Subtract(T left, T right);

    internal abstract T Multiply(T left, T right);

    internal abstract T Divide(T left, T right);
}

internal sealed class DecimalOperations : ValueOperations<decimal>
{
    internal static readonly DecimalOperations Instance = new();

    private DecimalOperations()
    {
    }

    internal override decimal Add(decimal left, decimal right) => left + right;

    internal override decimal Subtract(decimal left, decimal right) => left - right;

    internal override decimal Multiply(decimal left, decimal right) => left * right;

    internal override decimal Divide(decimal left, decimal right) => left / right;
}

internal sealed class Int64Operations : ValueOperations<long>
{
    internal static readonly Int64Operations Instance = new();

    private Int64Operations()
    {
    }

    internal override long Add(long left, long right) => checked(left + right);

    internal override long Subtract(long left, long right) => checked(left - right);

    internal override long Multiply(long left, long right) => checked(left * right);

    internal override long Divide(long left, long right) => checked(left / right);
}

internal static class ValueBinding<T>
{
    internal static readonly ValueOperations<T> Operations = Create();

    private static ValueOperations<T> Create()
    {
        if (typeof(T) == typeof(decimal))
        {
            return (ValueOperations<T>)(object)DecimalOperations.Instance;
        }

        if (typeof(T) == typeof(long))
        {
            return (ValueOperations<T>)(object)Int64Operations.Instance;
        }

        throw new NotSupportedException($"The value type {typeof(T).FullName} is not supported.");
    }
}

internal static class BindingProbe
{
    internal static decimal DecimalAdd(decimal left, decimal right) => ValueBinding<decimal>.Operations.Add(left, right);

    internal static decimal DecimalSubtract(decimal left, decimal right) => ValueBinding<decimal>.Operations.Subtract(left, right);

    internal static decimal DecimalMultiply(decimal left, decimal right) => ValueBinding<decimal>.Operations.Multiply(left, right);

    internal static decimal DecimalDivide(decimal left, decimal right) => ValueBinding<decimal>.Operations.Divide(left, right);

    internal static long Int64Add(long left, long right) => ValueBinding<long>.Operations.Add(left, right);

    internal static long Int64Subtract(long left, long right) => ValueBinding<long>.Operations.Subtract(left, right);

    internal static long Int64Multiply(long left, long right) => ValueBinding<long>.Operations.Multiply(left, right);

    internal static long Int64Divide(long left, long right) => ValueBinding<long>.Operations.Divide(left, right);
}
