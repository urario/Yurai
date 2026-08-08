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

1. Branch from `main`. `main` is protected; all changes arrive by pull request.
2. Keep it to one concern. Several small pull requests review faster than one large one.
3. Link the issue: `Refs #123`, or `Closes #123` when the pull request completes it.
4. Write tests. Yurai is developed test-first — a change in behavior comes with a change
   in tests.
5. Required checks must pass before merge — not before review. Open a draft pull
   request and ask for feedback whenever it is useful, red CI and all. (CI itself
   arrives with #6.)
6. Expect review comments, including from AI reviewers. Push fixes to the same branch;
   reply to anything you disagree with rather than silently skipping it.
7. A maintainer merges. Please don't merge your own pull request.

Say what you left out. A pull request that solves 80% of an issue and says which 20% is
missing is welcome; one that quietly narrows the scope is not.

## Code style

- .NET / C# conventions, English identifiers and comments.
- Target `netstandard2.0`, zero runtime dependencies. A pull request that adds a
  dependency needs to argue for it in the issue first.
- Comments explain *why*. The code already says what.

Formatting is enforced in CI; run the formatter locally before pushing.

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
