using Issue79Spike.Contracts;

namespace Issue79Spike.Positive;

public static class OperatorCalls
{
    public static void CompileAllForms()
    {
        Traced<decimal> decimalLeft = Traced.Of(12m, "Left");
        Traced<decimal> decimalRight = Traced.Of(3m, "Right");
        decimal decimalPlain = 2m;
        _ = decimalLeft + decimalRight; _ = decimalLeft + decimalPlain; _ = decimalPlain + decimalLeft;
        _ = decimalLeft - decimalRight; _ = decimalLeft - decimalPlain; _ = decimalPlain - decimalLeft;
        _ = decimalLeft * decimalRight; _ = decimalLeft * decimalPlain; _ = decimalPlain * decimalLeft;
        _ = decimalLeft / decimalRight; _ = decimalLeft / decimalPlain; _ = decimalPlain / decimalLeft;

        Traced<long> int64Left = Traced.OfInt64(12L, "Left");
        Traced<long> int64Right = Traced.OfInt64(3L, "Right");
        long int64Plain = 2L;
        _ = int64Left + int64Right; _ = int64Left + int64Plain; _ = int64Plain + int64Left;
        _ = int64Left - int64Right; _ = int64Left - int64Plain; _ = int64Plain - int64Left;
        _ = int64Left * int64Right; _ = int64Left * int64Plain; _ = int64Plain * int64Left;
        _ = int64Left / int64Right; _ = int64Left / int64Plain; _ = int64Plain / int64Left;
        _ = Traced.If(true, () => decimalLeft, () => decimalRight, "Decimal branch");
        _ = Traced.If(true, () => int64Left, () => int64Right, "Int64 branch");
        _ = int64Left + 1;
    }
}
