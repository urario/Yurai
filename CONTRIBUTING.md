# Contributing to Yurai

Thanks for your interest. Yurai is a small, dependency-free .NET library, and the
contribution process is kept small to match.

## Before you write code

Open an issue first, or comment on an existing one. This is not bureaucracy — the
project is developed by a mix of humans and AI agents that coordinate entirely through
GitHub, so an issue is how work becomes visible to everyone. A pull request that
arrives without one may duplicate work already in flight.

Small, obvious fixes (typos, broken links, a failing test) can go straight to a pull
request.

If you are about to propose something that changes the design, check
[`knowledge/`](knowledge/index.md) first — the requirements, the architecture decision
records, and the conventions the project works by are there. Knowing that a question was
already decided, and why, saves you writing a pull request that gets turned down for a
reason nobody had told you.

## Language

| Artifact | Language |
|---|---|
| Issues | Japanese by default — **English is welcome** from external contributors |
| Pull requests | Japanese by default; English is fine |
| Source code, identifiers, code comments | English |
| README and published documentation | English |

Everything a user of the library reads is English. Project coordination happens in
Japanese, but you are never required to write Japanese to contribute — write in
English and you will get an English answer.

## Pull requests

1. Branch from `main`. Every change arrives by pull request — no direct pushes, and
   branch protection will enforce that once it is configured (#4).
2. Keep it to one concern. Several small pull requests review faster than one large one.
3. Link the issue: `Refs #123`, or `Closes #123` when the pull request completes it.
4. Write tests. Yurai is developed test-first — a change in behavior comes with a change
   in tests, and the pull request shows that the test came first. What counts as showing
   it is in the
   [testing and quality strategy](knowledge/process/testing-and-quality.md#what-counts-as-evidence).
5. Required checks must pass before merge — not before review. Open a draft pull
   request and ask for feedback whenever it is useful, red CI and all.
6. Expect review comments, including from AI reviewers. Push fixes to the same branch;
   reply to anything you disagree with rather than silently skipping it.
7. A maintainer merges. Please don't merge your own pull request.

Say what you left out. A pull request that solves 80% of an issue and says which 20% is
missing is welcome; one that quietly narrows the scope is not.

## Build and test

Install .NET SDK 8.0.100 or any later stable SDK. `global.json` keeps C# at version 12
and accepts a later major SDK when it is the only installed compatible SDK.

```shell
dotnet restore Yurai.sln
dotnet build Yurai.sln --configuration Release --no-restore
dotnet test Yurai.sln --configuration Release --no-build --no-restore
dotnet format Yurai.sln --verify-no-changes --no-restore
```

## Code style

- .NET / C# conventions, English identifiers and comments.
- Target `netstandard2.0`, zero runtime dependencies. A pull request that adds a
  dependency needs to argue for it in the issue first.
- Comments explain *why*. The code already says what.

CI enforces formatting, so run the formatter locally before pushing and match the style
of the surrounding code.

## Developed with AI agents

This repository is worked on by Claude Code and Codex alongside human maintainers. If
you want to understand who does what and why review comments look the way they do, read
[`AGENTS.md`](AGENTS.md). Contributors are not required to use AI tooling — but if you
do, the same bar applies: you are responsible for what you submit, and "the model wrote
it" is not a review answer.

## Reporting bugs

Include the Yurai version, the target framework, a minimal reproduction, and what you
expected instead. For calculation-correctness issues, the inputs and the exact expected
and actual values matter more than a description of them.

## Security

Do not open a public issue for a security problem. Report it privately to the
maintainer via GitHub's security advisory feature on this repository.

## License

By contributing, you agree that your contributions are licensed under the
[MIT License](LICENSE), the same license as the project.
