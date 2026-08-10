using BenchmarkDotNet.Attributes;

namespace Yurai.Benchmarks;

[MemoryDiagnoser]
public class CalculationBenchmarks
{
    [Params(32, 10_000)]
    public int OperationCount { get; set; }

    [Benchmark(Baseline = true)]
    public decimal NativeDecimal()
    {
        decimal result = 0m;
        for (int index = 0; index < OperationCount; index++)
        {
            result += 1m;
        }

        return result;
    }

    [Benchmark]
    public Traced TracedDecimal()
    {
        Traced result = global::Yurai.Yurai.Of(0m, "Start");
        Traced increment = global::Yurai.Yurai.Of(1m, "Increment");
        for (int index = 0; index < OperationCount; index++)
        {
            result += increment;
        }

        return result;
    }
}
