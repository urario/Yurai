using System.Collections.Generic;
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
    /// Introduces an anonymous decimal input into a traced calculation.
    /// </summary>
    /// <param name="value">The evaluated decimal value.</param>
    /// <returns>A traced value rooted at an anonymous input.</returns>
    public static Traced Of(decimal value) => new(new InputEvidenceNode(value, null));

    /// <summary>
    /// Introduces a named decimal input into a traced calculation.
    /// </summary>
    /// <param name="value">The evaluated decimal value.</param>
    /// <param name="name">The domain name of the input.</param>
    /// <returns>A traced value rooted at the named input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty, consists only of white-space characters, or contains malformed UTF-16 text.
    /// </exception>
    public static Traced Of(decimal value, string name)
    {
        string validName = ArgumentValidation.Validate(name, nameof(name));
        return new Traced(new InputEvidenceNode(value, validName));
    }

    /// <summary>
    /// Returns the smaller traced value and records the selected operand.
    /// </summary>
    public static Traced Min(Traced left, Traced right)
        => CreateSelection(left, right, BinaryOperationKind.Min);

    /// <summary>
    /// Returns the larger traced value and records the selected operand.
    /// </summary>
    public static Traced Max(Traced left, Traced right)
        => CreateSelection(left, right, BinaryOperationKind.Max);

    /// <summary>
    /// Evaluates one conditional alternative and records the branch that produced the result.
    /// The unselected alternative is never invoked, so exception and side-effect behavior matches a native conditional.
    /// </summary>
    /// <param name="condition">The plain Boolean condition used to select an alternative.</param>
    /// <param name="whenTrue">The alternative evaluated when <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="whenFalse">The alternative evaluated when <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <param name="branchName">The developer-supplied name of the recorded decision.</param>
    /// <returns>A traced value containing the selected result and branch evidence.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="whenTrue"/>, <paramref name="whenFalse"/>, or <paramref name="branchName"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="branchName"/> is empty, consists only of white-space characters, or contains malformed UTF-16 text.
    /// </exception>
    /// <exception cref="InvalidOperationException">The selected alternative returns an uninitialized traced value.</exception>
    public static Traced If(
        bool condition,
        Func<Traced> whenTrue,
        Func<Traced> whenFalse,
        string branchName)
    {
        if (whenTrue is null)
        {
            throw new ArgumentNullException(nameof(whenTrue));
        }

        if (whenFalse is null)
        {
            throw new ArgumentNullException(nameof(whenFalse));
        }

        string validBranchName = ArgumentValidation.Validate(branchName, nameof(branchName));
        Traced selected = condition ? whenTrue() : whenFalse();
        EvidenceNode selectedRoot = selected.Root;

        return new Traced(new BranchEvidenceNode(selectedRoot, validBranchName, condition));
    }

    /// <summary>
    /// Attaches a domain name to this result without changing its value or existing evidence.
    /// </summary>
    /// <param name="name">The domain name to attach.</param>
    /// <returns>A new traced value with a named evidence root.</returns>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty, consists only of white-space characters, or contains malformed UTF-16 text.
    /// </exception>
    public Traced As(string name)
    {
        EvidenceNode currentRoot = GetRoot();
        string validName = ArgumentValidation.Validate(name, nameof(name));
        return new Traced(new NamedEvidenceNode(currentRoot, validName));
    }

    /// <summary>
    /// Rounds the value using native decimal rounding and records the rounding policy.
    /// </summary>
    /// <param name="digits">The number of fractional digits to retain.</param>
    /// <param name="reason">The reason for applying the rounding policy.</param>
    /// <returns>A new traced value with the rounding evidence attached.</returns>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="digits"/> is outside the range supported by decimal rounding.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="reason"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="reason"/> is empty, consists only of white-space characters, or contains malformed UTF-16 text.
    /// </exception>
    public Traced Round(int digits, string reason)
    {
        EvidenceNode currentRoot = GetRoot();
        const MidpointRounding rounding = MidpointRounding.ToEven;
        decimal value = decimal.Round(currentRoot.Value, digits, rounding);
        string validReason = ArgumentValidation.Validate(reason, nameof(reason));
        return new Traced(new RoundEvidenceNode(value, digits, rounding, validReason, currentRoot));
    }

    /// <summary>
    /// Determines whether this result depends on a named input or named intermediate result.
    /// </summary>
    /// <param name="name">The exact developer-supplied name to find.</param>
    /// <returns><see langword="true"/> when any recorded dependency has the name; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty, consists only of white-space characters, or contains malformed UTF-16 text.
    /// </exception>
    public bool DependsOn(string name)
    {
        EvidenceNode currentRoot = GetRoot();
        string validName = ArgumentValidation.Validate(name, nameof(name));
        return DependencyQuery.DependsOn(currentRoot, validName);
    }

    /// <summary>
    /// Gets the distinct names of recorded inputs on which this result depends.
    /// </summary>
    /// <value>
    /// A read-only snapshot computed on every access in deterministic root-first, left-to-right discovery order.
    /// Duplicate names are returned once, and named intermediate results are not included.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    public IReadOnlyList<string> Inputs => DependencyQuery.GetInputs(GetRoot());

    /// <summary>
    /// Returns every recorded dependency path from a named input or named intermediate result to this result.
    /// </summary>
    /// <param name="name">The exact developer-supplied name at which each path starts.</param>
    /// <returns>
    /// A read-only snapshot of all matching paths in deterministic root-first, left-to-right discovery order.
    /// Each path is projected as developer-supplied names from the match to this result; anonymous nodes are omitted.
    /// A trace is a dependency path only. It does not express sensitivity or attribution.
    /// Because every path is retained, the result size can grow exponentially relative to the number of unique
    /// evidence nodes in a heavily shared graph. <see cref="DependsOn(string)"/> and <see cref="Inputs"/> remain
    /// linear in graph size.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty, consists only of white-space characters, or contains malformed UTF-16 text.
    /// </exception>
    public IReadOnlyList<IReadOnlyList<string>> Trace(string name)
    {
        EvidenceNode currentRoot = GetRoot();
        string validName = ArgumentValidation.Validate(name, nameof(name));
        return DependencyQuery.Trace(currentRoot, validName);
    }

    /// <summary>
    /// Returns a deterministic, human-readable explanation of the evaluated derivation.
    /// </summary>
    /// <returns>
    /// The evaluated result followed by its derivation. The output uses invariant culture;
    /// a shared evidence node is expanded once and later occurrences are rendered as references.
    /// An uninitialized value returns <c>Uninitialized Traced</c>.
    /// </returns>
    public string Explain() => root is null
        ? "Uninitialized Traced"
        : ExplainFormatter.Render(root);

    /// <summary>
    /// Returns the complete derivation as version 1 of Yurai's stable JSON schema.
    /// </summary>
    /// <returns>
    /// A dependency-free JSON document containing the normalized evidence node table.
    /// Decimal values are invariant strings, and node identifiers are local to this document.
    /// The document is material for an audit trail maintained by the caller, not an audit trail itself.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The value is an uninitialized <see cref="Traced"/> instance.
    /// </exception>
    public string ToJson() => JsonFormatter.Render(GetRoot());

    /// <summary>
    /// Adds two traced decimal values using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">Either operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator +(Traced left, Traced right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = right.GetRoot();
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Add, (leftValue, rightValue) => leftValue + rightValue);
    }

    /// <summary>
    /// Adds a decimal value to a traced value using native decimal arithmetic.
    /// </summary>
    public static Traced operator +(Traced left, decimal right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = new InputEvidenceNode(right, null);
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Add, (leftValue, rightValue) => leftValue + rightValue);
    }

    /// <summary>
    /// Adds a traced value to a decimal value using native decimal arithmetic.
    /// </summary>
    public static Traced operator +(decimal left, Traced right)
    {
        EvidenceNode leftRoot = new InputEvidenceNode(left, null);
        EvidenceNode rightRoot = right.GetRoot();
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Add, (leftValue, rightValue) => leftValue + rightValue);
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
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Subtract, (leftValue, rightValue) => leftValue - rightValue);
    }

    /// <summary>
    /// Subtracts a decimal value from a traced value using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">The traced operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator -(Traced left, decimal right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = new InputEvidenceNode(right, null);
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Subtract, (leftValue, rightValue) => leftValue - rightValue);
    }

    /// <summary>
    /// Subtracts a traced value from a decimal value using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">The traced operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator -(decimal left, Traced right)
    {
        EvidenceNode leftRoot = new InputEvidenceNode(left, null);
        EvidenceNode rightRoot = right.GetRoot();
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Subtract, (leftValue, rightValue) => leftValue - rightValue);
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
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Multiply, (leftValue, rightValue) => leftValue * rightValue);
    }

    /// <summary>
    /// Multiplies a traced value by a decimal value using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">The traced operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator *(Traced left, decimal right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = new InputEvidenceNode(right, null);
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Multiply, (leftValue, rightValue) => leftValue * rightValue);
    }

    /// <summary>
    /// Multiplies a decimal value by a traced value using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">The traced operand is uninitialized.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator *(decimal left, Traced right)
    {
        EvidenceNode leftRoot = new InputEvidenceNode(left, null);
        EvidenceNode rightRoot = right.GetRoot();
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Multiply, (leftValue, rightValue) => leftValue * rightValue);
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
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Divide, (leftValue, rightValue) => leftValue / rightValue);
    }

    /// <summary>
    /// Divides a traced value by a decimal value using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">The traced operand is uninitialized.</exception>
    /// <exception cref="DivideByZeroException">The decimal operand is zero.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator /(Traced left, decimal right)
    {
        EvidenceNode leftRoot = left.GetRoot();
        EvidenceNode rightRoot = new InputEvidenceNode(right, null);
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Divide, (leftValue, rightValue) => leftValue / rightValue);
    }

    /// <summary>
    /// Divides a decimal value by a traced value using native decimal arithmetic.
    /// </summary>
    /// <exception cref="InvalidOperationException">The traced operand is uninitialized.</exception>
    /// <exception cref="DivideByZeroException">The traced operand is zero.</exception>
    /// <exception cref="OverflowException">The native decimal operation overflows.</exception>
    public static Traced operator /(decimal left, Traced right)
    {
        EvidenceNode leftRoot = new InputEvidenceNode(left, null);
        EvidenceNode rightRoot = right.GetRoot();
        return CreateBinary(leftRoot, rightRoot, BinaryOperationKind.Divide, (leftValue, rightValue) => leftValue / rightValue);
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

    private static Traced CreateBinary(
        EvidenceNode left,
        EvidenceNode right,
        BinaryOperationKind operation,
        Func<decimal, decimal, decimal> evaluate)
    {
        decimal value = evaluate(left.Value, right.Value);
        return new Traced(new BinaryOperationEvidenceNode(value, operation, left, right));
    }

    private static Traced CreateSelection(Traced left, Traced right, BinaryOperationKind operation)
    {
        EvidenceNode leftRoot = left.Root;
        EvidenceNode rightRoot = right.Root;
        bool leftSelected = operation == BinaryOperationKind.Min
            ? leftRoot.Value <= rightRoot.Value
            : leftRoot.Value >= rightRoot.Value;
        decimal value = operation == BinaryOperationKind.Min
            ? Math.Min(leftRoot.Value, rightRoot.Value)
            : Math.Max(leftRoot.Value, rightRoot.Value);

        // Keep native Min/Max as the value oracle; leftSelected records the deterministic
        // evidence choice for numerically equal operands, including signed-zero cases.
        SelectedOperand selected = leftSelected
            ? SelectedOperand.Left
            : SelectedOperand.Right;
        return new Traced(new BinaryOperationEvidenceNode(value, operation, leftRoot, rightRoot, selected));
    }
}
