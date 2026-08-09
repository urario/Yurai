[CmdletBinding()]
param(
    [string]$OutputPath = "artifacts/stryker"
)

$ErrorActionPreference = "Stop"
$originalLocation = Get-Location
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path

if (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot $OutputPath
}

try {
    Set-Location $repositoryRoot
    & dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet tool restore failed with exit code $LASTEXITCODE."
    }

    Set-Location (Join-Path $repositoryRoot "tests/Yurai.Tests")
    & dotnet stryker --config-file stryker-config.json --output $OutputPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet stryker failed with exit code $LASTEXITCODE."
    }
}
finally {
    Set-Location $originalLocation
}
