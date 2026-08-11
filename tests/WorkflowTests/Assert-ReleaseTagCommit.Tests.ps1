$ErrorActionPreference = 'Stop'

$scriptPath = Join-Path $PSScriptRoot '../../eng/release/Assert-ReleaseTagCommit.ps1'
$expectedCommit = '1111111111111111111111111111111111111111'
$otherCommit = '2222222222222222222222222222222222222222'
$outerTagSha = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
$innerTagSha = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'

function New-GitObject {
    param(
        [string]$Type,
        [string]$Sha
    )

    [pscustomobject]@{
        type = $Type
        sha = $Sha
    }
}

function Assert-Equal {
    param(
        [object]$Actual,
        [object]$Expected
    )

    if ($Actual -cne $Expected) {
        throw "Expected '$Expected', got '$Actual'."
    }
}

function Assert-Throws {
    param(
        [scriptblock]$Action,
        [string]$MessagePattern
    )

    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -notlike $MessagePattern) {
            throw "Expected error '$MessagePattern', got '$($_.Exception.Message)'."
        }

        return
    }

    throw "Expected error '$MessagePattern', but no error was thrown."
}

$tests = @(
    @{
        Name = 'Lightweight tag resolves to the expected commit'
        Action = {
            $actual = & $scriptPath `
                -TagObject (New-GitObject -Type commit -Sha $expectedCommit) `
                -ExpectedCommit $expectedCommit `
                -ResolveAnnotatedTag { throw 'Resolver must not be called for a lightweight tag.' }

            Assert-Equal $actual $expectedCommit
        }
    },
    @{
        Name = 'Annotated tag resolves to the expected commit'
        Action = {
            $actual = & $scriptPath `
                -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                -ExpectedCommit $expectedCommit `
                -ResolveAnnotatedTag {
                    param($Sha)
                    Assert-Equal $Sha $outerTagSha
                    [pscustomobject]@{ object = New-GitObject -Type commit -Sha $expectedCommit }
                }

            Assert-Equal $actual $expectedCommit
        }
    },
    @{
        Name = 'Annotated tag fails when the expected run commit differs'
        Action = {
            Assert-Throws -MessagePattern 'The current release tag does not point to the selected release run commit.*' -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                    -ExpectedCommit $otherCommit `
                    -ResolveAnnotatedTag {
                        [pscustomobject]@{ object = New-GitObject -Type commit -Sha $expectedCommit }
                    }
            }
        }
    },
    @{
        Name = 'Tag object that cannot resolve to a commit fails closed'
        Action = {
            Assert-Throws -MessagePattern "Release tag resolved to unsupported object type 'blob'." -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type blob -Sha $outerTagSha) `
                    -ExpectedCommit $expectedCommit `
                    -ResolveAnnotatedTag { throw 'Resolver must not be called for a blob.' }
            }
        }
    },
    @{
        Name = 'Nested annotated tags resolve to the expected commit'
        Action = {
            $actual = & $scriptPath `
                -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                -ExpectedCommit $expectedCommit `
                -ResolveAnnotatedTag {
                    param($Sha)
                    if ($Sha -ceq $outerTagSha) {
                        return [pscustomobject]@{ object = New-GitObject -Type tag -Sha $innerTagSha }
                    }
                    if ($Sha -ceq $innerTagSha) {
                        return [pscustomobject]@{ object = New-GitObject -Type commit -Sha $expectedCommit }
                    }

                    throw "Unexpected tag object '$Sha'."
                }

            Assert-Equal $actual $expectedCommit
        }
    }
)

$failures = @()
foreach ($test in $tests) {
    try {
        & $test.Action
        Write-Output "PASS: $($test.Name)"
    }
    catch {
        $failures += "FAIL: $($test.Name): $($_.Exception.Message)"
    }
}

if ($failures.Count -gt 0) {
    throw ($failures -join [Environment]::NewLine)
}
