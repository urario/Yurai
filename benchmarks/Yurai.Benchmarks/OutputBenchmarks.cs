using BenchmarkDotNet.Attributes;

namespace Yurai.Benchmarks;

[MemoryDiagnoser]
public class OutputBenchmarks
{
    private Traced graph;

    [Params(32, 8_192)]
    public int LeafCount { get; set; }

    [GlobalSetup]
    public void Setup() => graph = GraphFactory.BuildBalanced(LeafCount);

    [Benchmark]
    public string Explain() => graph.Explain();

    [Benchmark]
    public string ToJson() => graph.ToJson();

    [Benchmark]
    public IReadOnlyList<IReadOnlyList<string>> Trace() => graph.Trace("Input0");
}
