namespace Yurai;

/// <summary>
/// Provides entry points for introducing decimal values into a traced calculation.
/// </summary>
public static class Yurai
{
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
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty or consists only of white-space characters.</exception>
    public static Traced Of(decimal value, string name)
    {
        string validName = NameValidation.Validate(name);
        return new Traced(new InputEvidenceNode(value, validName));
    }

    /// <summary>
    /// Returns the smaller traced value and records the selected operand.
    /// </summary>
    public static Traced Min(Traced left, Traced right)
    {
        EvidenceNode leftRoot = left.Root;
        EvidenceNode rightRoot = right.Root;
        decimal value = Math.Min(leftRoot.Value, rightRoot.Value);
        SelectedOperand selected = leftRoot.Value <= rightRoot.Value
            ? SelectedOperand.Left
            : SelectedOperand.Right;
        return new Traced(new BinaryOperationEvidenceNode(value, BinaryOperationKind.Min, leftRoot, rightRoot, selected));
    }

    /// <summary>
    /// Returns the larger traced value and records the selected operand.
    /// </summary>
    public static Traced Max(Traced left, Traced right)
    {
        EvidenceNode leftRoot = left.Root;
        EvidenceNode rightRoot = right.Root;
        decimal value = Math.Max(leftRoot.Value, rightRoot.Value);
        SelectedOperand selected = leftRoot.Value >= rightRoot.Value
            ? SelectedOperand.Left
            : SelectedOperand.Right;
        return new Traced(new BinaryOperationEvidenceNode(value, BinaryOperationKind.Max, leftRoot, rightRoot, selected));
    }
}
