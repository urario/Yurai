<!--
This template is for pull requests written in English. Enter N/A where a section does not apply.
Implementation pull requests must complete both Self-review and Test and quality-gate evidence.
-->

## Summary

<!-- Briefly describe the purpose, main changes, and user or developer impact. -->

## Type

<!-- Select all that apply. -->

- [ ] Feature (feat)
- [ ] Bug fix (fix)
- [ ] Refactoring (refactor)
- [ ] Tests (test)
- [ ] Documentation (docs)
- [ ] Tooling or process (chore)

## Related requirements (RQ) and issues

- RQ: `RQ-xxx` or N/A
- Related artifacts: `ADR-xxxx` / other, or N/A
- Issue: Closes #xxx / Refs #xxx

## Self-review

<!-- For implementation PRs, add findings or rationale below as well as checking the boxes. -->

- [ ] I read the complete diff
- [ ] I checked the diff against the related requirements, design, and issue
- [ ] I considered compatibility, security, and performance impacts
- [ ] I added or updated the necessary tests, or explained why tests are not needed

Findings:

## Test and quality-gate evidence

<!--
Required for implementation PRs. Include commands, measured results, and links to CI runs or artifacts.
Do not remove rows when a gate was not run or does not apply; use NOT RUN / N/A and explain why.
The gate is defined in knowledge/process/testing-and-quality.md — this table is where you show it was met.
-->

| Gate | Applies to | Command or evidence | Result |
| --- | --- | --- | --- |
| Build, test, format (CI) | Every PR | `N/A` | N/A |
| Knowledge base (OKF) (CI) | Every PR | `N/A` | N/A |
| Test-first evidence | A PR that changes behavior | See below | N/A |
| Property test | A PR implementing a P0 requirement | `N/A` | N/A |
| Counterexample landed as an example test | A PR fixing a property failure | `N/A` | N/A |
| Mutation score (advisory) | Changes under `src/Yurai/` | `N/A` | N/A |
| Benchmark (advisory) | Changes to a hot path | `N/A` | N/A |

Line coverage is not a gate. Report it only where it is part of a finding.

Test-first evidence (required when behavior changes) — one of the three accepted forms:

<!--
1. Commit order: a failing `test:` commit precedes the `feat:` / `fix:` commit.
2. A named test that fails without the change. Say so if it does not compile against main — that counts.
3. A counterexample test, citing where the values came from (a seed, an issue).
-->

## Residual risks

<!-- List remaining manual checks, unresolved questions, or known limitations. Write "None known" if there are none. -->
