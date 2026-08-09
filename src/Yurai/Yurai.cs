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
    /// <paramref name="branchName"/> is empty or consists only of white-space characters.
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
