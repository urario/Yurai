# Issue #79 ADR-0018 implementation spike

This directory contains the auditable, non-shipping source for the ADR-0018
compile, binding, allocation, and compatibility spike. It is not referenced by
the solution and has no effect on `src/Yurai/`.

## Reproduce

From the repository root, restore and build the two compile projects:

```powershell
dotnet restore eng/spikes/issue79/Positive/Issue79Spike.Positive.csproj
dotnet build eng/spikes/issue79/Positive/Issue79Spike.Positive.csproj --configuration Release --no-restore
dotnet build eng/spikes/issue79/Runtime/Issue79Spike.Runtime.csproj --configuration Release --no-restore
```

The three projects under `Negative*` are expected to fail compilation. The
expected diagnostics and the tracked result summary are recorded in
[`results/tracked-short-run.md`](results/tracked-short-run.md).

Run the runtime checks:

```powershell
dotnet run --project eng/spikes/issue79/Runtime/Issue79Spike.Runtime.csproj --configuration Release --no-restore
```

Run the bounded BenchmarkDotNet evidence job:

```powershell
dotnet restore eng/spikes/issue79/Benchmarks/Issue79Evidence.Benchmarks.csproj
dotnet run --project eng/spikes/issue79/Benchmarks/Issue79Evidence.Benchmarks.csproj `
  --configuration Release --no-restore -- `
  --job short --filter '*BindingCostBenchmarks*' '*AllocationBreakdownBenchmarks*' `
  --exporters json markdown --artifacts artifacts/issue79-spike/results-tracked
```

The runtime probe reports first-touch and warmed binding allocation separately,
checks hot-path IL for `box`, and consumes every benchmark result as a checksum.
BenchmarkDotNet 0.15.8 does not expose an `OperationsPerInvoke` attribute, so the
tracked benchmark uses an explicit 10,000-iteration inner loop and reports the
raw invocation totals. Inputs are runtime parameters rather than constants. The
original 32-operation result remains in the Issue comment as a short-workload
reference.

## Boundary

The spike deliberately uses a closed `decimal` / `long` set, immutable internal
binding, and homogeneous evidence nodes. It does not add production API,
runtime dependencies, a public policy or registry, or a second target framework.
