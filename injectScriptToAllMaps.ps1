# Generates the Hardcore script from Source and injects it into every Hardcore map.
# Requires SFDScriptInjector on PATH and the game SDK link from LibrarySetup.fsx.
param(
    [string]$Destination
)

$ErrorActionPreference = "Stop"
$root = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }

if (-not $Destination) {
    $candidates = @(
        (Join-Path $env:USERPROFILE "OneDrive\Documents\Superfighters Deluxe\Maps\Custom"),
        (Join-Path $env:USERPROFILE "Documents\Superfighters Deluxe\Maps\Custom")
    )
    $Destination = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $Destination) {
        throw "Could not find the Superfighters Deluxe Custom maps folder. Pass -Destination."
    }
}

$generator = Join-Path $root "tools\SFD.ScriptTools.ScriptGenerator.fsx"
$sourceRoot = Join-Path $root "SFDScripts\Internal\Hardcore\Source"
$source = Get-ChildItem -LiteralPath $sourceRoot -Recurse -Filter *.cs |
    Where-Object { $_.Name -ne "MapDependentData.cs" } |
    Sort-Object FullName

if (-not $source) {
    throw "No Hardcore source files found under $sourceRoot"
}

$outDir = Join-Path $root "SFDScripts\bin\Debug\net8.0"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$generated = Join-Path $outDir "Hardcore.txt"
$injectFile = Join-Path $outDir "Hardcore.inject.txt"

& dotnet fsi $generator @($source.FullName) -o $generated
if ($LASTEXITCODE -ne 0) {
    throw "Script generator failed with exit code $LASTEXITCODE."
}

$body = Get-Content -LiteralPath $generated -Raw
$wrapped = "#region Script To Copy`r`n" + $body.TrimEnd() + "`r`n#endregion`r`n"
Set-Content -LiteralPath $injectFile -Value $wrapped -Encoding utf8

$mapsDir = Join-Path $root "SFDScripts\Internal\Hardcore\Maps"
$maps = Get-ChildItem -LiteralPath $mapsDir -File | Where-Object { $_.Extension -eq ".sfdm" }

foreach ($map in $maps) {
    & SFDScriptInjector $injectFile $map.FullName
    if ($LASTEXITCODE -ne 0) {
        throw "SFDScriptInjector failed for $($map.Name) with exit code $LASTEXITCODE."
    }
    Copy-Item -LiteralPath $map.FullName -Destination (Join-Path $Destination $map.Name) -Force
    Write-Output "Deployed $($map.Name) to $Destination"
}
