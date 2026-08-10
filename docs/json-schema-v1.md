# Yurai JSON schema version 1

`Traced.ToJson()` exports the complete derivation evidence as a normalized JSON node
table. The document is designed for programs outside the process: callers can store it,
attach it to a case, or transform it into their own audit records.

The export is **material for an audit trail maintained by the caller**. It is not an
audit trail by itself: Yurai does not store documents, authenticate them, timestamp
them, control access, or define retention policy.

## Compatibility contract

The top-level `schemaVersion` is the integer `1`. Version 1 is a stable compatibility
contract. Producers emit exactly the fields and vocabulary documented here; version 1
does not promise that consumers will accept additional fields, node kinds, or enum
values. A change to structure, required fields, field meaning, node kinds, enum values,
decimal encoding, or identity semantics therefore requires a new schema version.

Corrections that make the implementation conform to this document do not change the
version. JSON object member order, insignificant whitespace, and the exact escape form
used for a character are not semantic. Consumers must parse JSON rather than compare
the raw text.

## Document shape

```json
{
  "schemaVersion": 1,
  "root": 1,
  "nodes": []
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `schemaVersion` | integer | Always `1` for this contract. |
| `root` | positive integer | The `id` of the result's evidence root. |
| `nodes` | array of node objects | Every distinct evidence node reachable from `root`, exactly once. |

Nodes are listed in deterministic root-first, left-to-right encounter order. IDs start
at `1` and increase in the same order, so `root` is `1` in version 1. Consumers should
still read the `root` field rather than infer it. An edge may refer to a node later in
the array, and multiple edges may refer to the same ID.

IDs are local to one document. They identify sharing inside that document only; they
are not stable object IDs and must not be compared across calls, graph revisions,
processes, or Yurai releases.

## Fields common to every node

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | positive integer | Unique document-local node ID. |
| `kind` | string enum | One of `input`, `binaryOperation`, `round`, `branch`, or `named`. |
| `value` | string | The node's exact .NET `decimal` value in invariant form. |

`value` is deliberately a JSON string, not a JSON number. Parsing it as an invariant
.NET `decimal` preserves the value, scale, and sign, including trailing zeros and
negative zero. A consumer must not parse it through binary floating point first.

## Node kinds

### `input`

| Additional field | Type | Meaning |
| --- | --- | --- |
| `name` | string or `null` | Developer-supplied input name, or `null` for an anonymous plain decimal operand. |

### `binaryOperation`

| Additional field | Type | Meaning |
| --- | --- | --- |
| `operation` | string enum | `add`, `subtract`, `multiply`, `divide`, `min`, or `max`. |
| `left` | positive integer | ID of the left operand. |
| `right` | positive integer | ID of the right operand. |
| `selectedOperand` | string enum or `null` | `left` or `right` for `min` and `max`; `null` for arithmetic operations. |

For numerically equal `min` or `max` operands, `selectedOperand` is `left`. Both operand
edges remain in the document regardless of which operand was selected.

### `round`

| Additional field | Type | Meaning |
| --- | --- | --- |
| `digits` | integer | Fractional digits supplied to native decimal rounding. |
| `midpointRounding` | string enum | `toEven` in version 1. |
| `reason` | string | Developer-supplied reason for the rounding decision. |
| `child` | positive integer | ID of the value before rounding. |

### `branch`

| Additional field | Type | Meaning |
| --- | --- | --- |
| `branchName` | string | Developer-supplied name of the decision. |
| `condition` | Boolean | Plain Boolean condition used to select the branch. |
| `selectedBranch` | string enum | `then` when `condition` is true; otherwise `else`. |
| `child` | positive integer | ID of the selected alternative. |

Only the selected alternative is evaluated and recorded. A plain Boolean carries no
edge back to traced values that may have been read to compute it, so version 1 does not
represent condition-only control dependencies.

### `named`

| Additional field | Type | Meaning |
| --- | --- | --- |
| `name` | string | Developer-supplied name attached to a derived value. |
| `child` | positive integer | ID of the evidence before naming. |

## Strings

Names and reasons are ordinary JSON strings. Quotes, backslashes, control characters,
and valid UTF-16 surrogate pairs are escaped as needed to produce valid JSON. Parsing
the document returns the original .NET string; Yurai does not trim, normalize, or
replace accepted metadata.

The public metadata entry points reject an isolated high or low UTF-16 surrogate with
`ArgumentException`. This validation happens when the name or reason enters the evidence,
so every accepted metadata string can be exported losslessly rather than failing later
at `ToJson()`.

## Complete example

For a shared input:

```csharp
var amount = Traced.Of(10.00m, "Amount");
var total = (amount + amount).As("Total");
string json = total.ToJson();
```

The formatted document below is semantically equivalent to the compact string returned
by `ToJson()`:

```json
{
  "schemaVersion": 1,
  "root": 1,
  "nodes": [
    {
      "id": 1,
      "kind": "named",
      "value": "20.00",
      "name": "Total",
      "child": 2
    },
    {
      "id": 2,
      "kind": "binaryOperation",
      "value": "20.00",
      "operation": "add",
      "left": 3,
      "right": 3,
      "selectedOperand": null
    },
    {
      "id": 3,
      "kind": "input",
      "value": "10.00",
      "name": "Amount"
    }
  ]
}
```

Both `left` and `right` refer to node `3`; the shared input is not duplicated. A typical
consumer first indexes `nodes` by `id`, reads `root`, and then follows the kind-specific
edge fields.
