---
name: yurai-implementation
description: Implement or change Yurai source code, tests, project structure, or developer tooling with test-first evidence, requirement traceability, zero-runtime-dependency checks, and pull-request verification.
---

# Yurai implementation

Implement the smallest change that satisfies the issue while leaving evidence a reviewer can reproduce.

The binding rules live in [`AGENTS.md`](../../../AGENTS.md) and
[`knowledge/`](../../../knowledge/index.md). Read them instead of relying on this skill
as a policy copy. Use [`yurai-git-workflow`](../yurai-git-workflow/SKILL.md) for branch,
commit, pull request, and handoff operations.

## 1. Establish the contract

1. Read the issue and every linked requirement, ADR, and design record.
2. Read [`knowledge/requirements/registry.md`](../../../knowledge/requirements/registry.md)
   and [`knowledge/process/traceability.md`](../../../knowledge/process/traceability.md).
3. Identify the registered `RQ-NNN` identifiers the change serves. Do not invent an
   identifier when the registry or issue does not provide one.
4. Run [`yurai-detailed-design`](../yurai-detailed-design/SKILL.md) when the issue leaves
   a contract, failure mode, compatibility rule, or test seam unclear.
5. Stop on a reserved decision from `AGENTS.md` section 2. Present options, trade-offs,
   and a recommendation in the issue, then hand the next action to the human.

## 2. Work test-first

Read
[`knowledge/process/testing-and-quality.md`](../../../knowledge/process/testing-and-quality.md)
before changing behavior.

1. Write the smallest useful failing example or property test.
2. Confirm the failure is caused by the missing behavior, not by the test setup.
3. Implement the smallest production change that makes it pass.
4. Refactor only while the focused tests remain green.
5. Add `[Trait("RQ", "RQ-NNN")]` only to a test that establishes that registered
   requirement.

Preserve one accepted evidence form in the pull request: ordered `test:` then `feat:` or
`fix:` commits, a named test that fails against `main`, or a counterexample with its
seed or issue origin. When a property finds a counterexample, retain the literal case as
an example test.

Documentation, tooling, formatting, and a behavior-preserving refactor do not require
test-first evidence. State why the row is not applicable instead of deleting it.

## 3. Protect the shipped library

- Keep `src/Yurai/` on `netstandard2.0` and BCL-only. Do not add a runtime package or
  project reference.
- Keep source code, identifiers, code comments, README content, and published
  documentation in English.
- Keep the public surface small. Never add or change a public type, member, signature,
  or semantic contract without the recorded human decision required by `AGENTS.md`.
- Treat `Trace` as the dependency path of a value, not as a general diagnostic term.

## 4. Verify and report

Run focused tests while iterating, then reproduce the pull-request lane before handoff:

```shell
dotnet restore Yurai.sln
dotnet build Yurai.sln --configuration Release --no-restore
dotnet test Yurai.sln --configuration Release --no-build --no-restore
dotnet format Yurai.sln --verify-no-changes --no-restore
```

Run any issue-specific property, mutation, benchmark, or knowledge checks as required.
Read the final diff and record commands and measured results in the pull request quality
gate table. Mark every skipped or inapplicable row `NOT RUN` or `N/A` with a reason;
never describe an unmeasured gate as passing.
