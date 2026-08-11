# Yurai 0.1.0 release runbook

This runbook describes the human-controlled steps after the release workflows have
been merged. It never contains or requests a NuGet API key value. The key is stored
only as the `NUGET_API_KEY` GitHub Actions secret and is exposed only to a publish
step.

## Normal release

1. Confirm that `v0.1.0` resolves to the intended reviewed release commit, then wait
   for the tag-push `Release package` run to pass. Both lightweight and annotated tags
   are supported; GitHub commit and tag object IDs are handled as full lowercase SHA-1
   values. The run verifies the package, symbol package, metadata, dependencies,
   repository commit, and SourceLink information. The release commit must be reachable
   from `main`.
2. Record the ID of that successful `Release package` run. It must have event `push`,
   head branch `v0.1.0`, and a head SHA equal to the commit currently resolved from the
   tag. Confirm that its artifact name contains both `v0.1.0` and that SHA.
3. Run the current `Publish package` workflow from `main` with `release_tag=v0.1.0`,
   that run ID, `dry_run=true`, and `symbols_only=false`. The workflow stops before
   artifact download if the tag no longer resolves to the selected run's exact head
   SHA, including when an annotated tag must be dereferenced.
4. After the dry run succeeds, repeat with the same tag and run ID, `dry_run=false`,
   and `symbols_only=false`. Approve the `release` Environment only after checking the
   selected run and artifact.
5. Confirm the package, README, license, and symbols on NuGet.org before closing
   Issue #30.

## Tag correction before publishing

Moving a release tag is a maintainer-only recovery action. It is allowed only before
NuGet.org has accepted version `0.1.0`. After publication, unlisting does not remove or
replace that package version; publish a correction as `v0.1.1` or later instead.

If the maintainer has deliberately corrected the remote tag before publication, refresh
the local tag explicitly rather than relying on a normal tag fetch:

```shell
git fetch --force origin refs/tags/v0.1.0:refs/tags/v0.1.0
git rev-parse v0.1.0^{}
```

Then use the new tag-push `Release package` run ID. Never reuse a run whose `headSha`
belongs to the previous tag target.

## Symbols-only recovery

If the primary package upload succeeds but the symbols upload fails, do not rerun the
normal publish. First confirm that `Yurai` version `0.1.0` is already present on
NuGet.org. Then run `Publish package` again with the same `release_tag` and successful
release run ID, `dry_run=false`, and `symbols_only=true`. This skips the immutable
primary package and retries only the verified `.snupkg` with the same Environment and
secret protections.

Never paste the API key into workflow inputs, commands, comments, logs, or this file.
