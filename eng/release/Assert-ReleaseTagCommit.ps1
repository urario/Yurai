[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [psobject]$TagObject,

    [Parameter(Mandatory)]
    [string]$ExpectedCommit,

    [Parameter(Mandatory)]
    [scriptblock]$ResolveAnnotatedTag,

    [ValidateRange(1, 100)]
    [int]$MaximumTagDepth = 16
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($ExpectedCommit -cnotmatch '^[0-9a-f]{40}$') {
    throw "Expected release commit '$ExpectedCommit' is not a full lowercase commit SHA."
}

$currentObject = $TagObject
$seenTagObjects = @{}
$tagDepth = 0

while ($true) {
    if ($null -eq $currentObject) {
        throw 'Release tag resolved to an empty Git object.'
    }

    $objectType = [string]$currentObject.type
    $objectSha = [string]$currentObject.sha
    if ($objectSha -cnotmatch '^[0-9a-f]{40}$') {
        throw "Release tag resolved to an invalid Git object SHA '$objectSha'."
    }

    if ($objectType -ceq 'commit') {
        if ($objectSha -cne $ExpectedCommit) {
            throw "The current release tag does not point to the selected release run commit. Resolved '$objectSha', expected '$ExpectedCommit'."
        }

        return $objectSha
    }

    if ($objectType -cne 'tag') {
        throw "Release tag resolved to unsupported object type '$objectType'."
    }

    if ($tagDepth -ge $MaximumTagDepth) {
        throw "Release tag exceeded the maximum annotated tag depth of $MaximumTagDepth."
    }
    if ($seenTagObjects.ContainsKey($objectSha)) {
        throw "Release tag contains a cycle at tag object '$objectSha'."
    }

    $seenTagObjects[$objectSha] = $true
    $annotatedTag = & $ResolveAnnotatedTag $objectSha
    if ($null -eq $annotatedTag -or $null -eq $annotatedTag.object) {
        throw "Annotated tag object '$objectSha' did not identify another Git object."
    }

    $currentObject = $annotatedTag.object
    $tagDepth++
}
