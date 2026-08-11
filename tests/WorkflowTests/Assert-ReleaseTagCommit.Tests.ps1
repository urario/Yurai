$ErrorActionPreference = 'Stop'

$scriptPath = Join-Path $PSScriptRoot '../../eng/release/Assert-ReleaseTagCommit.ps1'
$fixturePath = Join-Path $PSScriptRoot 'Fixtures/v0.1.0-annotated-tag.json'
$resolvedCommit = '1111111111111111111111111111111111111111'
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
        [string]$ExpectedMessage
    )

    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -cne $ExpectedMessage) {
            throw "Expected error '$ExpectedMessage', got '$($_.Exception.Message)'."
        }

        return
    }

    throw "Expected error '$ExpectedMessage', but no error was thrown."
}

$tests = @(
    @{
        Name = 'Lightweight tag resolves to the expected commit'
        Action = {
            $actual = & $scriptPath `
                -TagObject (New-GitObject -Type commit -Sha $resolvedCommit) `
                -ExpectedCommit $resolvedCommit `
                -ResolveAnnotatedTag { throw 'Resolver must not be called for a lightweight tag.' }

            Assert-Equal $actual $resolvedCommit
        }
    },
    @{
        Name = 'Captured GitHub annotated tag response resolves to the expected commit'
        Action = {
            $fixture = Get-Content -Raw $fixturePath | ConvertFrom-Json
            $actual = & $scriptPath `
                -TagObject $fixture.tagRef.object `
                -ExpectedCommit $fixture.expectedCommit `
                -ResolveAnnotatedTag {
                    param($Sha)
                    Assert-Equal $Sha $fixture.annotatedTag.sha
                    $fixture.annotatedTag
                }

            Assert-Equal $actual $fixture.expectedCommit
        }
    },
    @{
        Name = 'Annotated tag resolves to the expected commit'
        Action = {
            $actual = & $scriptPath `
                -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                -ExpectedCommit $resolvedCommit `
                -ResolveAnnotatedTag {
                    param($Sha)
                    Assert-Equal $Sha $outerTagSha
                    [pscustomobject]@{ object = New-GitObject -Type commit -Sha $resolvedCommit }
                }

            Assert-Equal $actual $resolvedCommit
        }
    },
    @{
        Name = 'Annotated tag fails when the expected run commit differs'
        Action = {
            Assert-Throws -ExpectedMessage "The current release tag does not point to the selected release run commit. Resolved '$resolvedCommit', expected '$otherCommit'." -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                    -ExpectedCommit $otherCommit `
                    -ResolveAnnotatedTag {
                        [pscustomobject]@{ object = New-GitObject -Type commit -Sha $resolvedCommit }
                    }
            }
        }
    },
    @{
        Name = 'Tag object that cannot resolve to a commit fails closed'
        Action = {
            Assert-Throws -ExpectedMessage "Release tag resolved to unsupported object type 'blob'." -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type blob -Sha $outerTagSha) `
                    -ExpectedCommit $resolvedCommit `
                    -ResolveAnnotatedTag { throw 'Resolver must not be called for a blob.' }
            }
        }
    },
    @{
        Name = 'Nested annotated tags resolve to the expected commit'
        Action = {
            $actual = & $scriptPath `
                -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                -ExpectedCommit $resolvedCommit `
                -ResolveAnnotatedTag {
                    param($Sha)
                    if ($Sha -ceq $outerTagSha) {
                        return [pscustomobject]@{ object = New-GitObject -Type tag -Sha $innerTagSha }
                    }
                    if ($Sha -ceq $innerTagSha) {
                        return [pscustomobject]@{ object = New-GitObject -Type commit -Sha $resolvedCommit }
                    }

                    throw "Unexpected tag object '$Sha'."
                }

            Assert-Equal $actual $resolvedCommit
        }
    },
    @{
        Name = 'Annotated tag cycle fails closed'
        Action = {
            Assert-Throws -ExpectedMessage "Release tag contains a cycle at tag object '$outerTagSha'." -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                    -ExpectedCommit $resolvedCommit `
                    -ResolveAnnotatedTag {
                        [pscustomobject]@{ object = New-GitObject -Type tag -Sha $outerTagSha }
                    }
            }
        }
    },
    @{
        Name = 'Annotated tag without an object fails closed'
        Action = {
            Assert-Throws -ExpectedMessage "Annotated tag object '$outerTagSha' did not identify another Git object." -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type tag -Sha $outerTagSha) `
                    -ExpectedCommit $resolvedCommit `
                    -ResolveAnnotatedTag { [pscustomobject]@{ tag = 'v0.1.0' } }
            }
        }
    },
    @{
        Name = 'Invalid tag object SHA fails closed'
        Action = {
            Assert-Throws -ExpectedMessage "Release tag resolved to an invalid Git object SHA 'not-a-sha'." -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type commit -Sha 'not-a-sha') `
                    -ExpectedCommit $resolvedCommit `
                    -ResolveAnnotatedTag { throw 'Resolver must not be called for an invalid SHA.' }
            }
        }
    },
    @{
        Name = 'Invalid expected commit SHA fails closed'
        Action = {
            Assert-Throws -ExpectedMessage "Expected release commit 'NOT-A-COMMIT' is not a full lowercase commit SHA." -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type commit -Sha $resolvedCommit) `
                    -ExpectedCommit 'NOT-A-COMMIT' `
                    -ResolveAnnotatedTag { throw 'Resolver must not be called for an invalid expected commit.' }
            }
        }
    },
    @{
        Name = 'Annotated tag depth limit fails closed'
        Action = {
            $tagShas = @(1..17 | ForEach-Object { $_.ToString('x40') })
            Assert-Throws -ExpectedMessage 'Release tag exceeded the maximum annotated tag depth of 16.' -Action {
                & $scriptPath `
                    -TagObject (New-GitObject -Type tag -Sha $tagShas[0]) `
                    -ExpectedCommit $resolvedCommit `
                    -ResolveAnnotatedTag {
                        param($Sha)
                        $index = [array]::IndexOf($tagShas, $Sha)
                        if ($index -lt 0 -or $index -ge ($tagShas.Count - 1)) {
                            throw "Unexpected tag object '$Sha'."
                        }

                        [pscustomobject]@{ object = New-GitObject -Type tag -Sha $tagShas[$index + 1] }
                    }
            }
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
    $failures | ForEach-Object { [Console]::Error.WriteLine($_) }
    exit 1
}
