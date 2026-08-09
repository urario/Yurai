using System.Globalization;

namespace Yurai;

/// <summary>
/// Carries an eagerly evaluated decimal value together with its immutable derivation evidence.
/// </summary>
public readonly struct Traced
{
    private readonly EvidenceNode? root;

    internal Traced(EvidenceNode root)
    {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
    }

    /// <summary>
    /// Gets the evaluated decimal value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    public decimal Value => GetRoot().Value;

    internal EvidenceNode Root => GetRoot();

    /// <summary>
    /// Attaches a domain name to this result without changing its value or existing evidence.
    /// </summary>
    /// <param name="name">The domain name to attach.</param>
    /// <returns>A new traced value with a named evidence root.</returns>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty or consists only of white-space characters.</exception>
    public Traced As(string name)
    {
        EvidenceNode currentRoot = GetRoot();
        string validName = NameValidation.Validate(name);
        return new Traced(new NamedEvidenceNode(currentRoot, validName));
    }

    /// <summary>
    /// Adds two traced decimal values using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">Either operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator +(Traced left, Traced right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = right.GetRoot();
        decimal value = leftRoot.Value + rightRoot.Value;
        return new Traced(new BinaryOperationEvidenceNode(value, BinaryOperationKind.Add, leftRoot, rightRoot));
    }

    /// <summary>
    /// Subtracts one traced decimal value from another using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">Either operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator -(Traced left, Traced right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = right.GetRoot();
        decimal value = leftRoot.Value - rightRoot.Value;
        return new Traced(new BinaryOperationEvidenceNode(value, BinaryOperationKind.Subtract, leftRoot, rightRoot));
    }

    /// <summary>
    /// Multiplies two traced decimal values using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">Either operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator *(Traced left, Traced right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = right.GetRoot();
        decimal value = leftRoot.Value * rightRoot.Value;
        return new Traced(new BinaryOperationEvidenceNode(value, BinaryOperationKind.Multiply, leftRoot, rightRoot));
    }

    /// <summary>
    /// Divides one traced decimal value by another using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">Either operand is uninitialized.</exception>
    /// <exception cref="DivideByZeroException">The right operand is zero.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator /(Traced left, Traced right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = right.GetRoot();
        decimal value = leftRoot.Value / rightRoot.Value;
        return new Traced(new BinaryOperationEvidenceNode(value, BinaryOperationKind.Divide, leftRoot, rightRoot));
    }

    /// <summary>
    /// Returns the invariant representation of the evaluated value, or an uninitialized diagnostic.
    /// </summary>
    /// <returns>The invariant decimal value or <c>Uninitialized Traced</c>.</returns>
    public override string ToString() => root is null
        ? "Uninitialized Traced"
        : root.Value.ToString(CultureInfo.InvariantCulture);

    private EvidenceNode GetRoot() => root
        ?? throw new InvalidOperationException("The traced value is uninitialized.");
}
