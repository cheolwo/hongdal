[CmdletBinding()]
param(
    [string] $TileKey = "kr5186:l2:700:1145"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$pythonCommand = Get-Command python -ErrorAction SilentlyContinue
$pythonPath = if ($null -ne $pythonCommand) {
    $pythonCommand.Source
}
else {
    Join-Path $env:USERPROFILE ".cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe"
}
if (-not (Test-Path -LiteralPath $pythonPath)) {
    throw "PythonRuntimeMissing:$pythonPath"
}

$geospatialPackages = Join-Path $repositoryRoot "artifacts/local/python-packages/geospatial"
if (Test-Path -LiteralPath $geospatialPackages) {
    $env:PYTHONPATH = $geospatialPackages
}

Push-Location $repositoryRoot
try {
    & $pythonPath "eng/public-spatial/build-daegwallyeong-l2-artifacts.py" --tile-key $TileKey
    if ($LASTEXITCODE -ne 0) { throw "SpatialArtifactBuildFailed:$LASTEXITCODE" }
}
finally {
    Pop-Location
}
