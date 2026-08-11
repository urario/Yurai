[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PackagePath,

    [Parameter(Mandatory)]
    [string]$SymbolsPackagePath,

    [Parameter(Mandatory)]
    [string]$ExpectedPackageId,

    [Parameter(Mandatory)]
    [string]$ExpectedVersion
)

$ErrorActionPreference = 'Stop'
[Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem') | Out-Null

function Assert-Condition {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

Assert-Condition (Test-Path -LiteralPath $PackagePath -PathType Leaf) "Package was not found: $PackagePath"
Assert-Condition (Test-Path -LiteralPath $SymbolsPackagePath -PathType Leaf) "Symbols package was not found: $SymbolsPackagePath"
Assert-Condition ([IO.Path]::GetExtension($PackagePath) -eq '.nupkg') "Package must be a .nupkg file."
Assert-Condition ([IO.Path]::GetExtension($SymbolsPackagePath) -eq '.snupkg') "Symbols package must be a .snupkg file."

$temporaryDirectory = Join-Path ([IO.Path]::GetTempPath()) ("yurai-package-" + [Guid]::NewGuid().ToString('N'))

try {
    [IO.Compression.ZipFile]::ExtractToDirectory($PackagePath, $temporaryDirectory)

    $nuspecPath = Get-ChildItem -LiteralPath $temporaryDirectory -Filter '*.nuspec' -File | Select-Object -First 1
    Assert-Condition ($null -ne $nuspecPath) 'The package does not contain a nuspec file.'

    $nuspec = [xml](Get-Content -LiteralPath $nuspecPath.FullName -Raw)
    $metadata = $nuspec.package.metadata
    Assert-Condition ($metadata.id -eq $ExpectedPackageId) "Package ID '$($metadata.id)' does not match '$ExpectedPackageId'."
    Assert-Condition ($metadata.version -eq $ExpectedVersion) "Package version '$($metadata.version)' does not match '$ExpectedVersion'."
    Assert-Condition ($metadata.license.type -eq 'expression' -and $metadata.license.'#text' -eq 'MIT') 'Package license must be the MIT expression.'
    Assert-Condition (($metadata.dependencies.SelectNodes('.//*[local-name()="dependency"]') | Measure-Object).Count -eq 0) 'The package contains runtime dependencies.'

    $archive = [IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        $entries = $archive.Entries
        $entryNames = @($entries | ForEach-Object { $_.FullName })
        Assert-Condition ($entryNames -contains 'README.md') 'README.md is not included in the package.'
        Assert-Condition ($entryNames -contains 'LICENSE') 'LICENSE is not included in the package.'

        Assert-Condition ($entryNames -contains 'lib/netstandard2.0/Yurai.dll') 'The package does not contain lib/netstandard2.0/Yurai.dll.'
        Assert-Condition ($entryNames -contains 'lib/netstandard2.0/Yurai.xml') 'The package does not contain XML documentation.'
        Assert-Condition (-not ($entryNames | Where-Object { $_ -like 'lib/*' -and $_ -notlike 'lib/netstandard2.0/*' })) 'The package contains an unexpected library target.'
        Assert-Condition (-not ($entryNames | Where-Object { $_ -like 'runtimes/*' })) 'The package contains runtime-specific assets.'
    }
    finally {
        $archive.Dispose()
    }

    $symbolsArchive = [IO.Compression.ZipFile]::OpenRead($SymbolsPackagePath)
    try {
        $symbolsNuspecEntry = $symbolsArchive.GetEntry($nuspecPath.Name)
        Assert-Condition ($null -ne $symbolsNuspecEntry) 'The symbols package does not contain the matching nuspec file.'
        $symbolsReader = New-Object IO.StreamReader($symbolsNuspecEntry.Open())
        try {
            $symbolsNuspec = [xml]$symbolsReader.ReadToEnd()
        }
        finally {
            $symbolsReader.Dispose()
        }

        $symbolsMetadata = $symbolsNuspec.package.metadata
        Assert-Condition ($symbolsMetadata.id -eq $ExpectedPackageId) 'The symbols package ID does not match the main package.'
        Assert-Condition ($symbolsMetadata.version -eq $ExpectedVersion) 'The symbols package version does not match the main package.'
        Assert-Condition ($null -ne ($symbolsMetadata.packageTypes.packageType | Where-Object { $_.name -eq 'SymbolsPackage' })) 'The symbols package type is missing.'
        Assert-Condition (($symbolsMetadata.dependencies.SelectNodes('.//*[local-name()="dependency"]') | Measure-Object).Count -eq 0) 'The symbols package contains dependencies.'

        $pdbEntry = $symbolsArchive.GetEntry('lib/netstandard2.0/Yurai.pdb')
        Assert-Condition ($null -ne $pdbEntry) 'The symbols package does not contain lib/netstandard2.0/Yurai.pdb.'
        $pdbStream = $pdbEntry.Open()
        try {
            $pdbBytes = New-Object IO.MemoryStream
            $pdbStream.CopyTo($pdbBytes)
            $pdbText = [Text.Encoding]::ASCII.GetString($pdbBytes.ToArray())
            Assert-Condition ($pdbText -match 'raw\.githubusercontent\.com/urario/Yurai/') 'The PDB does not contain SourceLink information for urario/Yurai.'
        }
        finally {
            $pdbStream.Dispose()
        }
    }
    finally {
        $symbolsArchive.Dispose()
    }

    Write-Output "Verified $ExpectedPackageId ${ExpectedVersion}: runtime dependencies, metadata, and package contents."
}
finally {
    if (Test-Path -LiteralPath $temporaryDirectory) {
        Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force
    }
}
