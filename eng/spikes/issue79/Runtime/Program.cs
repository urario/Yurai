using System.Reflection;
using System.Reflection.Emit;
using Issue79Spike.Contracts;

VerifyBindingAllocationPhases();
VerifyNoBoxInHotPath();
VerifyDecimalOperators();
VerifyInt64Operators();
VerifyInt64FailuresDoNotCreateResultEvidence();
VerifyUnsupportedDefaultCarrier();
VerifyPublicBoundary();

Console.WriteLine("All Issue #79 runtime checks passed.");

static void VerifyBindingAllocationPhases()
{
    ForceGc();
    long decimalBefore = GC.GetAllocatedBytesForCurrentThread();
    _ = BindingProbe.DecimalAdd(12.5m, 3.25m);
    long decimalFirstTouch = GC.GetAllocatedBytesForCurrentThread() - decimalBefore;

    ForceGc();
    long int64Before = GC.GetAllocatedBytesForCurrentThread();
    _ = BindingProbe.Int64Add(12L, 3L);
    long int64FirstTouch = GC.GetAllocatedBytesForCurrentThread() - int64Before;

    decimal decimalChecksum = 0m;
    long int64Checksum = 0L;
    long warmedBefore = GC.GetAllocatedBytesForCurrentThread();
    for (int index = 0; index < 1_000_000; index++)
    {
        decimalChecksum += BindingProbe.DecimalAdd(12.5m, 3.25m);
        int64Checksum += BindingProbe.Int64Add(12L, 3L);
    }

    long warmed = GC.GetAllocatedBytesForCurrentThread() - warmedBefore;
    GC.KeepAlive(decimalChecksum);
    GC.KeepAlive(int64Checksum);
    Equal(0L, warmed, "warmed closed binding allocation");
    Console.WriteLine($"First-touch allocation: decimal={decimalFirstTouch} B, Int64={int64FirstTouch} B");
    Console.WriteLine($"Warmed closed binding allocation: {warmed} B");
}

static void VerifyNoBoxInHotPath()
{
    MethodInfo genericOperator = typeof(Traced<decimal>)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method => method.Name == "op_Addition" && method.GetParameters().All(parameter => parameter.ParameterType == typeof(Traced<decimal>)));
    MethodInfo binding = typeof(BindingProbe).GetMethod("DecimalAdd", BindingFlags.NonPublic | BindingFlags.Static)!;

    if (ContainsBoxOpcode(genericOperator) || ContainsBoxOpcode(binding))
    {
        throw new InvalidOperationException("The operator or binding hot path contains an IL box opcode.");
    }

    Console.WriteLine("Hot-path IL box check: no box opcode in operator or binding probe.");
}

static bool ContainsBoxOpcode(MethodInfo method)
{
    byte[] il = method.GetMethodBody()?.GetILAsByteArray() ?? Array.Empty<byte>();
    int offset = 0;
    while (offset < il.Length)
    {
        OpCode opcode = ReadOpCode(il, ref offset);
        if (opcode.Value == OpCodes.Box.Value) return true;
        offset += OperandSize(opcode.OperandType, il, offset);
    }

    return false;
}

static OpCode ReadOpCode(byte[] il, ref int offset)
{
    byte first = il[offset++];
    if (first == 0xFE)
    {
        return FindOpCode(il[offset++], true);
    }

    return FindOpCode(first, false);
}

static int OperandSize(OperandType operandType, byte[] il, int offset)
{
    return operandType switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineR or OperandType.ShortInlineVar =>
            operandType == OperandType.ShortInlineR ? 4 : 1,
        OperandType.InlineVar => 2,
        OperandType.InlineI or OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineMethod or
            OperandType.InlineSig or OperandType.InlineString or OperandType.InlineTok or OperandType.InlineType => 4,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        OperandType.InlineSwitch => 4 + (4 * BitConverter.ToInt32(il, offset)),
        _ => throw new InvalidOperationException($"Unsupported IL operand type: {operandType}."),
    };
}

static OpCode FindOpCode(byte value, bool multiByte)
{
    foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
    {
        if (field.FieldType != typeof(OpCode)) continue;
        OpCode opcode = (OpCode)field.GetValue(null)!;
        ushort opcodeValue = unchecked((ushort)opcode.Value);
        if (multiByte && (opcodeValue & 0xFF00) == 0xFE00 && (opcodeValue & 0xFF) == value)
        {
            return opcode;
        }
        else if (!multiByte && opcodeValue < 0x100 && opcodeValue == value)
        {
            return opcode;
        }
    }

    throw new InvalidOperationException($"Unknown IL opcode: 0x{value:X2}.");
}

static void VerifyDecimalOperators()
{
    Traced<decimal> left = Traced.Of(12m, "Left");
    Traced<decimal> right = Traced.Of(3m, "Right");
    Equal(15m, (left + right).Value, "decimal traced + traced");
    Equal(14m, (left + 2m).Value, "decimal traced + plain");
    Equal(14m, (2m + left).Value, "decimal plain + traced");
    Equal(9m, (left - right).Value, "decimal traced - traced");
    Equal(10m, (left - 2m).Value, "decimal traced - plain");
    Equal(-10m, (2m - left).Value, "decimal plain - traced");
    Equal(36m, (left * right).Value, "decimal traced * traced");
    Equal(24m, (left * 2m).Value, "decimal traced * plain");
    Equal(24m, (2m * left).Value, "decimal plain * traced");
    Equal(4m, (left / right).Value, "decimal traced / traced");
    Equal(6m, (left / 2m).Value, "decimal traced / plain");
    Equal(0.5m, (6m / left).Value, "decimal plain / traced");
}

static void VerifyInt64Operators()
{
    Traced<long> left = Traced.OfInt64(12L, "Left");
    Traced<long> right = Traced.OfInt64(5L, "Right");
    Equal(17L, (left + right).Value, "Int64 traced + traced");
    Equal(14L, (left + 2L).Value, "Int64 traced + plain");
    Equal(14L, (2L + left).Value, "Int64 plain + traced");
    Equal(7L, (left - right).Value, "Int64 traced - traced");
    Equal(10L, (left - 2L).Value, "Int64 traced - plain");
    Equal(-10L, (2L - left).Value, "Int64 plain - traced");
    Equal(60L, (left * right).Value, "Int64 traced * traced");
    Equal(24L, (left * 2L).Value, "Int64 traced * plain");
    Equal(24L, (2L * left).Value, "Int64 plain * traced");
    Equal(2L, (left / right).Value, "Int64 traced / traced");
    Equal(6L, (left / 2L).Value, "Int64 traced / plain");
    Equal(0L, (5L / left).Value, "Int64 plain / traced");
}

static void VerifyInt64FailuresDoNotCreateResultEvidence()
{
    Traced<long> maximum = Traced.OfInt64(long.MaxValue);
    Traced<long> minimum = Traced.OfInt64(long.MinValue);
    Traced<long> one = Traced.OfInt64(1L);
    Traced<long> minusOne = Traced.OfInt64(-1L);
    Traced<long> zero = Traced.OfInt64(0L);
    long before = EvidenceProbe.Int64CreatedCount;
    Throws<OverflowException>(() => _ = maximum + one, "Int64 addition overflow");
    Equal(before, EvidenceProbe.Int64CreatedCount, "overflow result evidence count");
    Throws<OverflowException>(() => _ = minimum / minusOne, "Int64 division overflow");
    Equal(before, EvidenceProbe.Int64CreatedCount, "division overflow result evidence count");
    Throws<DivideByZeroException>(() => _ = one / zero, "Int64 division by zero");
    Equal(before, EvidenceProbe.Int64CreatedCount, "division-by-zero result evidence count");
}

static void VerifyUnsupportedDefaultCarrier()
{
    Traced<string> unsupported = default;
    Throws<InvalidOperationException>(() => _ = unsupported.Value, "unsupported default carrier");
}

static void VerifyPublicBoundary()
{
    Equal(0, typeof(Traced<>).GetConstructors().Length, "public generic carrier constructors");
    string[] genericMethods = typeof(Traced).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(method => method.IsGenericMethodDefinition).Select(method => method.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
    if (genericMethods.Length != 1 || !string.Equals(genericMethods[0], "If", StringComparison.Ordinal))
        throw new InvalidOperationException($"generic companion methods: expected If, actual {string.Join(", ", genericMethods)}.");
}

static void ForceGc()
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}

static void Equal<T>(T expected, T actual, string name) where T : IEquatable<T>
{
    if (!expected.Equals(actual)) throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}.");
}

static void Throws<TException>(Action action, string name) where TException : Exception
{
    try { action(); }
    catch (TException) { return; }
    throw new InvalidOperationException($"{name}: expected {typeof(TException).Name}.");
}
