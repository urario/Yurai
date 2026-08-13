using BenchmarkDotNet.Attributes;
using Issue79Spike.Contracts;

namespace Issue79Spike.Benchmarks;

[MemoryDiagnoser]
public class BindingCostBenchmarks
{
    private const int OperationCount = 10_000;

    [Params(12)]
    public int LeftSeed { get; set; }

    [Params(3)]
    public int RightSeed { get; set; }

    private decimal decimalLeft;
    private decimal decimalRight;
    private long int64Left;
    private long int64Right;

    [GlobalSetup]
    public void Setup()
    {
        decimalLeft = LeftSeed + 0.0001m;
        decimalRight = RightSeed + 0.0002m;
        int64Left = LeftSeed;
        int64Right = RightSeed;
    }

    [Benchmark(Baseline = true)]
    public decimal NativeDecimalAdd()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += decimalLeft + decimalRight;
        return checksum;
    }

    [Benchmark]
    public decimal BoundDecimalAdd()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.DecimalAdd(decimalLeft, decimalRight);
        return checksum;
    }

    [Benchmark]
    public decimal NativeDecimalSubtract()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += decimalLeft - decimalRight;
        return checksum;
    }

    [Benchmark]
    public decimal BoundDecimalSubtract()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.DecimalSubtract(decimalLeft, decimalRight);
        return checksum;
    }

    [Benchmark]
    public decimal NativeDecimalMultiply()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += decimalLeft * decimalRight;
        return checksum;
    }

    [Benchmark]
    public decimal BoundDecimalMultiply()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.DecimalMultiply(decimalLeft, decimalRight);
        return checksum;
    }

    [Benchmark]
    public decimal NativeDecimalDivide()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += decimalLeft / decimalRight;
        return checksum;
    }

    [Benchmark]
    public decimal BoundDecimalDivide()
    {
        decimal checksum = 0m;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.DecimalDivide(decimalLeft, decimalRight);
        return checksum;
    }

    [Benchmark]
    public long NativeInt64Add()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += int64Left + int64Right;
        return checksum;
    }

    [Benchmark]
    public long BoundInt64Add()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.Int64Add(int64Left, int64Right);
        return checksum;
    }

    [Benchmark]
    public long NativeInt64Subtract()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += int64Left - int64Right;
        return checksum;
    }

    [Benchmark]
    public long BoundInt64Subtract()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.Int64Subtract(int64Left, int64Right);
        return checksum;
    }

    [Benchmark]
    public long NativeInt64Multiply()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += int64Left * int64Right;
        return checksum;
    }

    [Benchmark]
    public long BoundInt64Multiply()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.Int64Multiply(int64Left, int64Right);
        return checksum;
    }

    [Benchmark]
    public long NativeInt64Divide()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += int64Left / int64Right;
        return checksum;
    }

    [Benchmark]
    public long BoundInt64Divide()
    {
        long checksum = 0L;
        for (int index = 0; index < OperationCount; index++) checksum += BindingProbe.Int64Divide(int64Left, int64Right);
        return checksum;
    }
}
