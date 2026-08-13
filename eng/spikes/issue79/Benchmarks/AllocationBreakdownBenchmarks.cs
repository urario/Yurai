using BenchmarkDotNet.Attributes;
using CurrentTraced = Yurai.Traced;
using SpikeDecimalTraced = Issue79Spike.Contracts.Traced<decimal>;
using SpikeInt64Traced = Issue79Spike.Contracts.Traced<long>;

namespace Issue79Spike.Benchmarks;

[MemoryDiagnoser]
public class AllocationBreakdownBenchmarks
{
    private const int OperationCount = 10_000;

    [Params(12)]
    public int LeftSeed { get; set; }

    [Params(3)]
    public int RightSeed { get; set; }

    private CurrentTraced currentLeft;
    private CurrentTraced currentRight;
    private SpikeDecimalTraced decimalLeft;
    private SpikeDecimalTraced decimalRight;
    private SpikeInt64Traced int64Left;
    private SpikeInt64Traced int64Right;

    [GlobalSetup]
    public void Setup()
    {
        decimal decimalLeftValue = LeftSeed + 0.0001m;
        decimal decimalRightValue = RightSeed + 0.0002m;
        currentLeft = CurrentTraced.Of(decimalLeftValue, "Left");
        currentRight = CurrentTraced.Of(decimalRightValue, "Right");
        decimalLeft = Issue79Spike.Contracts.Traced.Of(decimalLeftValue, "Left");
        decimalRight = Issue79Spike.Contracts.Traced.Of(decimalRightValue, "Right");
        int64Left = Issue79Spike.Contracts.Traced.OfInt64(LeftSeed, "Left");
        int64Right = Issue79Spike.Contracts.Traced.OfInt64(RightSeed, "Right");
    }

    [Benchmark(Baseline = true)]
    public CurrentTraced CurrentDecimalAdd()
    {
        CurrentTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = currentLeft + currentRight;
        return result;
    }

    [Benchmark]
    public CurrentTraced CurrentDecimalSubtract()
    {
        CurrentTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = currentLeft - currentRight;
        return result;
    }

    [Benchmark]
    public CurrentTraced CurrentDecimalMultiply()
    {
        CurrentTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = currentLeft * currentRight;
        return result;
    }

    [Benchmark]
    public CurrentTraced CurrentDecimalDivide()
    {
        CurrentTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = currentLeft / currentRight;
        return result;
    }

    [Benchmark]
    public SpikeDecimalTraced GenericDecimalAdd()
    {
        SpikeDecimalTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = decimalLeft + decimalRight;
        return result;
    }

    [Benchmark]
    public SpikeDecimalTraced GenericDecimalSubtract()
    {
        SpikeDecimalTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = decimalLeft - decimalRight;
        return result;
    }

    [Benchmark]
    public SpikeDecimalTraced GenericDecimalMultiply()
    {
        SpikeDecimalTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = decimalLeft * decimalRight;
        return result;
    }

    [Benchmark]
    public SpikeDecimalTraced GenericDecimalDivide()
    {
        SpikeDecimalTraced result = default;
        for (int index = 0; index < OperationCount; index++) result = decimalLeft / decimalRight;
        return result;
    }

    [Benchmark]
    public SpikeInt64Traced GenericInt64Add()
    {
        SpikeInt64Traced result = default;
        for (int index = 0; index < OperationCount; index++) result = int64Left + int64Right;
        return result;
    }

    [Benchmark]
    public SpikeInt64Traced GenericInt64Subtract()
    {
        SpikeInt64Traced result = default;
        for (int index = 0; index < OperationCount; index++) result = int64Left - int64Right;
        return result;
    }

    [Benchmark]
    public SpikeInt64Traced GenericInt64Multiply()
    {
        SpikeInt64Traced result = default;
        for (int index = 0; index < OperationCount; index++) result = int64Left * int64Right;
        return result;
    }

    [Benchmark]
    public SpikeInt64Traced GenericInt64Divide()
    {
        SpikeInt64Traced result = default;
        for (int index = 0; index < OperationCount; index++) result = int64Left / int64Right;
        return result;
    }

    [Benchmark]
    public SpikeDecimalTraced GenericDecimalEvidenceOnly()
    {
        SpikeDecimalTraced result = default;
        for (int index = 0; index < OperationCount; index++)
        {
            result = Issue79Spike.Contracts.EvidenceProbe.DecimalBinaryWithoutBinding(decimalLeft, decimalRight, 15.0003m);
        }

        return result;
    }

    [Benchmark]
    public SpikeInt64Traced GenericInt64EvidenceOnly()
    {
        SpikeInt64Traced result = default;
        for (int index = 0; index < OperationCount; index++)
        {
            result = Issue79Spike.Contracts.EvidenceProbe.Int64BinaryWithoutBinding(int64Left, int64Right, 15L);
        }

        return result;
    }

}
