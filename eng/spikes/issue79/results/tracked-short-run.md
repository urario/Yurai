# Issue #79 tracked evidence

This file records the reproducible result summary for the tracked spike source.
The raw BenchmarkDotNet reports remain local under `artifacts/issue79-spike/`
because they are generated output; the commands and inputs below are the source
of record.

## Verification commands

```powershell
dotnet build eng/spikes/issue79/Positive/Issue79Spike.Positive.csproj --configuration Release --no-restore
dotnet build eng/spikes/issue79/Runtime/Issue79Spike.Runtime.csproj --configuration Release --no-restore
dotnet run --project eng/spikes/issue79/Runtime/Issue79Spike.Runtime.csproj --configuration Release --no-restore
dotnet run --project eng/spikes/issue79/Benchmarks/Issue79Evidence.Benchmarks.csproj --configuration Release --no-build --no-restore -- --job short --filter '*BindingCostBenchmarks*' '*AllocationBreakdownBenchmarks*' --exporters json markdown --artifacts artifacts/issue79-spike/results-tracked-final2
```

BenchmarkDotNet 0.15.8 has no `OperationsPerInvoke` attribute. Each benchmark
therefore executes a fixed 10,000-operation inner loop, consumes the checksum,
and reports the raw method invocation total. Inputs are runtime parameters
(`LeftSeed=12`, `RightSeed=3`) converted to non-constant decimal values in
`GlobalSetup`. The short job is still a directional measurement, not a release
threshold.

## Runtime probe

The runtime probe completed successfully with:

```text
First-touch allocation: decimal=24 B, Int64=24 B
Warmed closed binding allocation: 0 B
Hot-path IL box check: no box opcode in operator or binding probe.
```

The IL check inspects the `Traced<decimal>` binary operator and its bound
`BindingProbe.DecimalAdd` method for the `box` opcode. The allocation check
separates first-touch static binding initialization from one million warmed
calls.

## Binding-cost short run

Mean values below are for 10,000 operations per benchmark invocation. All
methods reported no measured allocation. Values are rounded from the generated
BenchmarkDotNet report; `µs` is microseconds.

| Operation | Native decimal | Bound decimal | Native Int64 | Bound Int64 |
|---|---:|---:|---:|---:|
| Add | 95.651 µs | 97.255 µs | 5.523 µs | 3.150 µs |
| Subtract | 91.441 µs | 92.552 µs | 2.878 µs | 3.004 µs |
| Multiply | 76.502 µs | 87.243 µs | 2.924 µs | 3.165 µs |
| Divide | 870.748 µs | 867.709 µs | 2.940 µs | 2.848 µs |

The short-run error bars are material for some rows, so these numbers support
the structural conclusion (no measured binding allocation and no large fixed
dispatch cost), not a precise nanosecond claim.

## Allocation breakdown short run

| Operation | Current Yurai decimal | Generic decimal | Generic Int64 |
|---|---:|---:|---:|
| Add | 546.88 KB | 468.75 KB | 390.63 KB |
| Subtract | 546.88 KB | 468.75 KB | 390.63 KB |
| Multiply | 546.88 KB | 468.75 KB | 390.63 KB |
| Divide | 546.88 KB | 468.75 KB | 390.63 KB |

The generic full path and its evidence-only control both measured the same
allocation (decimal 468.75 KB; Int64 390.63 KB) for 10,000 operations. This
isolates the generic binding/operator layer from the homogeneous evidence-node
allocation. The current Yurai baseline uses a different node layout and is
reported as a separate comparison; the table does not claim that the two node
representations have identical object sizes.

## Expected compile failures

The three negative projects remain deliberate compile-time evidence:

| Project | Expected diagnostic |
|---|---|
| `NegativeRound` | `CS1929` — `Traced<long>` has no `Round` member/extension |
| `NegativeJsonV1` | `CS1929` — `Traced<long>` has no `ToJsonV1` member/extension |
| `NegativeUnsupported` | `CS1503` — `string` is not an accepted carrier |
