---
name: yurai-design-review
description: Review a change against Yurai's requirements, public API discipline, and vocabulary rules. Use before or during review of anything that adds or changes a public type or member, touches the dependency or target-framework setup, or writes prose describing what the library is.
---

# Design review

Check a change against what Yurai is required to be, before checking whether it is
correct. A correct implementation of the wrong thing is still wrong.

This skill is procedure only. The rules it checks live in
[`AGENTS.md`](../../../AGENTS.md), [`CLAUDE.md`](../../../CLAUDE.md), and
[`knowledge/`](../../../knowledge/index.md); read them there rather than trusting a
summary. Where a check has a command, run the command — the point is to look, not to
recall.

## 1. Requirements

Read [`knowledge/requirements/registry.md`](../../../knowledge/requirements/registry.md)
and answer two questions about the change:

- **Which requirement is it serving?** A change that serves none is either out of scope
  or a requirement nobody registered. Both are findings.
- **Does it collide with a non-goal?** Non-goals are registered in the same table,
  phrased in the negative. They exist to stop a debate from recurring; a change that
  re-opens one needs the maintainer, not a review comment.

The registry is empty until [#12](https://github.com/urario/Yurai/issues/12) fills it. Until
then, the reference points are the project proposal's requirement labels (R1–R6) as they
appear in the issue being worked, and this check is a judgement rather than a lookup —
say so in the review instead of implying a registry check happened.

**Identifier hygiene.** Requirement identifiers in this repository are `RQ-` plus three
digits, and every string in that format is a reference to a registered requirement
([traceability](../../../knowledge/process/traceability.md#the-identifier)). Examples in
prose, tests, and templates write `RQ-NNN` — never a real-format identifier. CI enforces
this repository-wide:

```shell
git grep -nE 'RQ-[0-9]{3}'
```

Every hit has to appear in the registry table. While the registry is empty, any hit is a
finding.

## 2. Public API

The surface is the product. Every addition is permanent in a way an internal
detail is not.

- **Is the new member necessary, or is it convenience the caller can write themselves?**
  Convenience earns its place only when the caller's version would be wrong more often
  than right.
- **Does it read from a call site?** Judge the name where it is used, not where it is
  declared.
- **Can it be added later instead?** Adding a member later is a minor release. Removing
  one is a breaking change. When in doubt, leave it out.

**Any change to the public surface — new type or member, changed signature, changed
semantics — is a reserved decision**
([AGENTS.md §2](../../../AGENTS.md#2-reserved-decisions-ai-proposes-the-human-decides)).
If the pull request makes one without a recorded decision behind it, that is blocking:
say what the surface change is and ask for the decision, rather than reviewing the
implementation of it.

## 3. Zero dependencies and the target framework

Yurai ships `netstandard2.0` with no runtime dependencies. Both are load-bearing and
both are reserved decisions.

```shell
git grep -n 'PackageReference' -- src/ Directory.Packages.props
git grep -n 'TargetFramework' -- src/ Directory.Build.props
```

The shipped library takes no `PackageReference`, and its target stays `netstandard2.0`.
Test-project dependencies are a different matter and are not covered by this rule.

A `netstandard2.0` consequence worth watching in review: APIs that only exist on newer
targets compile in the test project and not in the library. If a change reaches for one,
the finding is the reach, not the compile error.

## 4. Vocabulary

Two rules, both from the project proposal, both stated in
[`CLAUDE.md`](../../../CLAUDE.md#how-the-project-describes-itself) — read that section and
compare the diff against it. It holds the list; this skill deliberately does not repeat
it, because a banned phrase copied into a second file is the thing the rule is trying to
prevent.

- **Banned phrases** — as phrases, anywhere, in English and Japanese alike.
- **No unsupported novelty claims** — the claim, not the word.

One Yurai-specific term needs its own check: **`Trace` means the dependency path of a
value, and nothing else.** Not an execution log, not a diagnostic trail, not a
`System.Diagnostics` concept. If a new public name, XML doc comment, or README sentence
uses "trace" in the general sense, it collides with the term the library has already
spent, and readers will merge the two meanings. Say which meaning is intended and name a
replacement.

## 5. Records

A decision that outlives its issue belongs in
[`knowledge/`](../../../knowledge/index.md) — an ADR for a decision, a registry entry for
a requirement ([knowledge policy](../../../knowledge/process/knowledge-policy.md)). If
the pull request settles something that the next contributor would otherwise re-derive
and leaves it in a pull request comment, that is a finding: the comment is unreachable
from `main`.

Conversely, task state, progress notes, and change narration do not belong in
`knowledge/`. A knowledge document that reads like a changelog entry is the same finding
from the other side.

## Reporting

Separate the two kinds of finding, and never blur them:

- **Blocking** — a reserved decision taken without the maintainer, a requirement or
  non-goal violated, a phantom identifier, a dependency or target-framework change, a
  banned phrase. Each one names the file and line, says what breaks, and says what to do
  instead.
- **Suggestion** — everything else. Say plainly that it does not block.

Nothing found is a legitimate result and should be stated as one. A review that
manufactures a finding to look thorough costs the next reviewer their trust in this one.
