# Performance baseline

This document publishes Yurai's initial performance and allocation baseline. The
numbers are advisory measurements, not compatibility promises or merge-blocking
thresholds. Compare ratios from the same run before comparing absolute numbers across
machines.

## Intended scale

Yurai is intended for explicitly bounded regions containing tens of domain calculation
steps. It is not intended to trace every variable in an application or to act as taint
tracking or general-purpose provenance. The large cases below deliberately exceed
10,000 evidence nodes to expose scaling behavior; they are stress cases, not the
recommended everyday graph size.

Every result retains its reachable immutable evidence DAG. Construction therefore
trades time and memory for an explanation that can be rendered, exported, or queried
later. Keep the traced region at the boundary of the calculation that needs that
evidence, then return to plain values.

## Method

The benchmarks use BenchmarkDotNet 0.15.8 with `MemoryDiagnoser`:

- `CalculationBenchmarks` compares the same addition loop using plain `decimal` and
  `Traced`. The traced increment is shared, so 32 operations produce 34 unique nodes
  and 10,000 operations produce 10,002 unique nodes.
- `OutputBenchmarks` prepares balanced graphs outside the measured operation. 32 leaves
  produce 63 unique nodes; 8,192 leaves produce 16,383 unique nodes. It then measures
  complete `Explain()` and `ToJson()` output and a one-match `Trace("Input0")` query.
- `SharedTraceBenchmarks` repeatedly reuses one result as both operands. A depth of
  *D* has only *D + 1* unique nodes but produces 2^*D* matching dependency paths. This
  isolates the path-expansion risk of `Trace` on a heavily shared graph.

`Allocated` is managed memory allocated during one operation. It is not a CLR object
layout guarantee and does not directly measure retained heap after garbage collection.
The construction benchmark's bytes-per-node figure is an allocation estimate for this
specific topology.

## Measurement environment

Measured on 2026-08-10 with the BenchmarkDotNet default job:

| Component | Value |
| --- | --- |
| OS | Windows 11 22H2, build 22621.4317 |
| CPU | Intel Core i7-6700 3.40 GHz, 4 physical / 8 logical cores |
| BenchmarkDotNet | 0.15.8 |
| .NET SDK | 10.0.302, selected through the repository's roll-forward policy |
| Runtime | .NET 8.0.29, X64 RyuJIT x86-64-v3 |
| GC | Concurrent workstation |

Power management, background load, runtime, GC mode, and hardware all affect absolute
times. The ratios below come from paired methods in the same run.

## Results

### Calculation and DAG construction

| Method | Operations | Unique nodes | Mean | Error | Native ratio | Allocated | Approx. bytes / node |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Native decimal | 32 | N/A | 151.4 ns | 3.05 ns | 1.00 | 0 B | N/A |
| Traced decimal | 32 | 34 | 629.2 ns | 12.40 ns | 4.17 | 1,872 B | 55.1 B |
| Native decimal | 10,000 | N/A | 46.182 us | 0.921 us | 1.00 | 0 B | N/A |
| Traced decimal | 10,000 | 10,002 | 241.408 us | 4.825 us | 5.24 | 560,080 B | 56.0 B |

The time ratio is meaningful for capacity planning, but the absolute cost is also
important: the tens-of-steps case completes in well under one microsecond on this
machine. The large construction case remains sub-millisecond while allocating roughly
56 bytes per unique evidence node for this shared-increment topology.

### Complete output and a single dependency path

| Method | Leaves | Unique nodes | Mean | Error | Allocated |
| --- | ---: | ---: | ---: | ---: | ---: |
| Explain | 32 | 63 | 13.621 us | 0.270 us | 25,811 B |
| ToJson | 32 | 63 | 19.980 us | 0.392 us | 48,128 B |
| Trace | 32 | 63 | 1.401 us | 0.028 us | 808 B |
| Explain | 8,192 | 16,383 | 5.441 ms | 0.108 ms | 6,885,760 B |
| ToJson | 8,192 | 16,383 | 9.156 ms | 0.181 ms | 11,491,817 B |
| Trace | 8,192 | 16,383 | 0.279 ms | 0.006 ms | 1,344 B |

`Explain()` and `ToJson()` intentionally create complete output, so their allocation
includes the returned strings and grows with the material produced. `Trace` still walks
the graph, but this balanced-tree case has one matching path and returns very little
material.

### Shared-graph path expansion

| Depth | Unique nodes | Returned paths | Mean | Error | Allocated |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 8 | 9 | 256 | 19.69 us | 0.382 us | 19.34 KiB |
| 12 | 13 | 4,096 | 322.16 us | 6.434 us | 289.43 KiB |
| 16 | 17 | 65,536 | 9.227 ms | 0.182 ms | 4,610.76 KiB |

This is why unique-node count alone cannot bound `Trace` cost. Use `DependsOn` when the
question is only whether a dependency exists. Use `Trace` when every dependency path is
actually needed, and keep sharing and path cardinality in mind.

## Reproducing the baseline

Run from the repository root in a quiet environment:

```shell
dotnet restore Yurai.sln
dotnet run --project benchmarks/Yurai.Benchmarks/Yurai.Benchmarks.csproj \
  --configuration Release --no-restore -- --filter '*'
```

BenchmarkDotNet writes detailed logs and machine-readable results under
`BenchmarkDotNet.Artifacts/` by default. The repository's non-blocking Deep quality
workflow uses a bounded short job and uploads Markdown and JSON artifacts:

```shell
dotnet run --project benchmarks/Yurai.Benchmarks/Yurai.Benchmarks.csproj \
  --configuration Release --no-restore -- \
  --job short --filter '*' --exporters json markdown \
  --artifacts artifacts/benchmarks
```

Do not treat a difference between the Windows baseline and a hosted Linux runner as a
regression by itself. Establish a repeated baseline on the same class of machine before
setting a threshold. If a future result motivates caching, pooling, or another evidence
representation, that change requires its own issue and must preserve value fidelity,
structural sharing, portability, and the public contract.
