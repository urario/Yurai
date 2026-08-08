# Design

Architecture and design documents: the structures that several decisions add up to.

Empty for now. The architecture of the computation graph, the `Traced<decimal>` value
type, and the thread-safety model are designed in
[#17](https://github.com/urario/Yurai/issues/17), with the individual choices inside
them recorded as ADRs from [#18](https://github.com/urario/Yurai/issues/18) onward.

## What belongs here rather than in an ADR

An [ADR](../decisions/index.md) records *one decision* and the alternatives it was
chosen over. A design document describes *how the pieces fit* — the types, their
relationships, the invariants that hold across them, and the seams left for testing. If
you find yourself writing a third "Decision" heading in one file, those are ADRs and
this document should link to them.

The two are complementary and neither replaces the other: the design document says what
the shape is, the ADRs say why it is not one of the other shapes.

## Conventions

File names are lowercase kebab-case — `computation-graph.md`. Each document opens with
the standard header, including the `Requirements:` line naming the `RQ-###` identifiers
it serves ([knowledge policy](../process/knowledge-policy.md)), and links the ADRs that
constrain it.

A design document is kept true or superseded. When the code stops matching it, the pull
request that moved the code updates it — a design document describing a structure that
no longer exists is the most expensive kind of stale documentation, because it is
plausible.
