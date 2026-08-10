using BenchmarkDotNet.Attributes;

namespace Yurai.Benchmarks;

[MemoryDiagnoser]
public class SharedTraceBenchmarks
{
    private Traced graph;

    [Params(8, 12, 16)]
    public int Depth { get; set; }

    [GlobalSetup]
    public void Setup() => graph = GraphFactory.BuildSharedDiamond(Depth);

    [Benchmark]
    public IReadOnlyList<IReadOnlyList<string>> Trace() => graph.Trace("Root");
}
