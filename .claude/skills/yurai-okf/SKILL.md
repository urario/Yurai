---
name: yurai-okf
description: Add, change, or retire a document in the knowledge/ bundle — ADRs, the requirements registry, design and process documents — keeping it conformant to Open Knowledge Format v0.2. Use whenever a durable decision, requirement, or convention needs to be recorded or updated.
---

# Knowledge bundle maintenance

How to put something in [`knowledge/`](../../../knowledge/index.md) so that the next
contributor finds it, and so that CI accepts it.

The conventions are
[`knowledge/process/knowledge-policy.md`](../../../knowledge/process/knowledge-policy.md)
and the decision behind the mechanical checks is
[ADR-0004](../../../knowledge/decisions/ADR-0004-conformance-check-in-ci.md). Read them
there. This skill is the procedure and the local reproduction of the checks.

## 0. Does it belong here at all?

The test is one sentence: **if the answer would still matter after every open issue is
closed, it belongs in `knowledge/`.**

| It is | It goes |
|---|---|
| A decision, with the context that forced it | An ADR in `decisions/` |
| A requirement, with acceptance criteria | A row in `requirements/registry.md` |
| Structure spanning several decisions | A document in `design/` |
| A convention the project works by | A document in `process/` |
| Progress, blockers, what is left | The issue |
| What changed and how it was verified | The pull request |
| A rule that already exists in `AGENTS.md`, `CONTRIBUTING.md`, or the execution plan | Nowhere — link to it |

That last row is the one most often got wrong. A second copy of a rule drifts, and then
nobody knows which one binds.

## 1. Frontmatter

Every non-reserved document opens with one YAML mapping. `type` is the only field OKF
requires, and it must be present exactly once and non-empty.

```yaml
---
type: ADR
title: "Rounding mode for Round(digits, reason)"
description: One sentence a reader can use to decide whether to open the file.
tags: [rounding, decimal]
status: draft
requirements: [RQ-NNN]
generated: { by: claude-code/2.1.226, at: 2026-08-09T04:12:00Z }
sources:
  - id: issue-18
    resource: https://github.com/urario/Yurai/issues/18
    title: "Issue #18: design decisions for the open questions"
---
```

- **`type`** is one of `ADR`, `Process`, `Requirements Registry`, `Design`, `Template`.
- **`generated.by`** names the tool that actually wrote the document, with its version —
  read it, do not guess:

  ```shell
  claude --version
  ```

  Where no version can be read, `<producer>/<YYYY-MM>` is the honest fallback. The model
  behind the tool is not recorded.
- **`generated.at`** is the last *meaningful* change, not the birthday. A pull request
  that changes what the document says moves it — including one that adds a supersession
  note. A typo or link fix does not.
- **`requirements`** is omitted rather than written empty.
- **`verified` is never written in advance.** The maintainer's merge is what confirms a
  document, and that happens after the file is written. Absence means unverified, which
  is accurate.

**Examples never use a real identifier format.** Write `RQ-NNN`, `RQ-MMM`, `ADR-NNNN`.
A plausible-looking identifier in an example is a record that does not exist, and neither
a search nor CI can tell it from a genuine reference.

## 2. Body, files, and links

- **An H1 title, then a paragraph saying what the document is for**, before any structure.
  A reader who opened the wrong file should find that out in one sentence.
- **English**, including in a document summarising a Japanese issue. Quote in translation
  and link the issue.
- **File names** are lowercase kebab-case. ADRs carry their identifier so it survives
  grep: `ADR-NNNN-short-slug.md` with the next unused number.
- **Links between documents are relative** (`../decisions/index.md`), never
  bundle-absolute — this bundle sits inside a larger repository, so a leading `/`
  resolves against the repository root in GitHub's renderer and breaks for every human
  reader.

## 3. Status

`draft` → `stable` → `deprecated`, and the timing is asymmetric on purpose.

- **A document in an open pull request is `draft`.** Not yet reviewed is simply what is
  true. When the maintainer approves, push one more commit raising the approved documents
  to `stable`; that is the commit that gets merged.
- **Retirement moves early.** A record superseded by something in the same pull request
  goes to `deprecated` with `superseded_by` immediately, while its replacement is still
  `draft`. `deprecated` under-claims and an early `stable` over-claims — metadata that
  machines read before they read the prose should fail toward claiming too little.

## 4. Adding an ADR

1. **Take the next unused number.** Never reuse one, and never fill a gap.

   ```shell
   ls knowledge/decisions/
   ```

2. **Copy the template** — [`knowledge/decisions/adr-template.md`](../../../knowledge/decisions/adr-template.md).
3. **One decision per record.** The test is supersession: if part of it could plausibly
   be reversed while the rest stands, that part is its own ADR.
4. **Never edit a decision into a different decision.** Correct a typo freely; when the
   decision changes, write a new ADR, set the old one to `deprecated` with
   `superseded_by`, and add a line at the top of its body pointing forward. Everything
   else in the superseded record stays as written — the reasoning that turned out to be
   wrong is the most useful part of it.
5. **Update both indexes** — `knowledge/decisions/index.md` and the bundle root
   `knowledge/index.md`. A record no index lists is a record nobody finds.

## 5. Indexes

An `index.md` is a listing, not an essay: headings, each followed by link entries with a
one-line description. A sentence of framing is fine; a document with no entries is not an
index. Only the bundle root carries frontmatter, and only `okf_version: "0.2"`.

There is no `log.md`. Git already records what changed, when, and by whom.

## 6. Check it before pushing

CI asserts four things ([ADR-0004](../../../knowledge/decisions/ADR-0004-conformance-check-in-ci.md)):
frontmatter parses, `type` is present and non-empty, reserved files keep their shape, and
no phantom requirement identifiers exist. The authoritative script is the
`Knowledge base (OKF)` job in [`.github/workflows/ci.yml`](../../../.github/workflows/ci.yml).
Reproduce the two that catch most mistakes locally:

```shell
# Every string in the RQ-### format names a registered requirement.
# Every hit must match a row of the table in knowledge/requirements/registry.md.
git grep -nE 'RQ-[0-9]{3}'
```

```shell
# Frontmatter parses and carries exactly one non-empty type.
ruby -rpsych -e '
Dir.glob("knowledge/**/*.md").sort.each do |path|
  next if %w[index.md log.md].include?(File.basename(path))
  m = File.read(path).match(/\A---\r?\n(.*?)\r?\n---(?:\r?\n|\z)/m)
  abort "#{path}: missing or unterminated frontmatter" unless m
  data = Psych.safe_load(m[1], permitted_classes: [Date, Time], aliases: true)
  abort "#{path}: frontmatter must be one mapping" unless data.is_a?(Hash)
  type = data["type"]
  abort "#{path}: type must be a non-empty string" unless type.is_a?(String) && !type.strip.empty?
end
puts "ok"
'
```

What the checks do **not** assert, and what therefore stays with review: that a
`description` is useful, that an index lists everything in its directory, that `type`
values are used consistently, that `generated.at` was moved when the content changed.
Those are judgements — check them by reading.

## 7. Same gate as code

A branch, a pull request, an issue link, and a review from someone other than the author.
A typo or a broken link does not need the ceremony. Nothing here is edited on `main`
directly — see [`yurai-git-workflow`](../yurai-git-workflow/SKILL.md).
