# Yurai 0.1.0 release runbook

This runbook describes the human-controlled steps after the release workflows have
been merged. It never contains or requests a NuGet API key value. The key is stored
only as the `NUGET_API_KEY` GitHub Actions secret and is exposed only to a publish
step.

## Normal release

1. Run `Release package` on the `v0.1.0` tag and wait for the package, symbol package,
   metadata, dependency, repository-commit, and SourceLink checks to pass.
2. Record the successful run ID from that workflow. The artifact name contains both
   `v0.1.0` and the verified commit SHA.
3. Run `Publish package` with `release_tag=v0.1.0`, that run ID, `dry_run=false`, and
   `symbols_only=false`. Approve the `release` Environment only after checking the
   selected run and artifact.
4. Confirm the package, README, license, and symbols on NuGet.org before closing
   Issue #30.

## Symbols-only recovery

If the primary package upload succeeds but the symbols upload fails, do not rerun the
normal publish. First confirm that `Yurai` version `0.1.0` is already present on
NuGet.org. Then run `Publish package` again with the same `release_tag` and successful
release run ID, `dry_run=false`, and `symbols_only=true`. This skips the immutable
primary package and retries only the verified `.snupkg` with the same Environment and
secret protections.

Never paste the API key into workflow inputs, commands, comments, logs, or this file.
