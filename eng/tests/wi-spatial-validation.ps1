$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($PSVersionTable.PSEdition -ne "Core") {
    & pwsh -NoProfile -File $PSCommandPath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    exit 0
}

$tests = @(
    "world-interactions.ps1",
    "spatial-hierarchy.ps1",
    "execution-ledger.ps1"
)
$started = [Diagnostics.Stopwatch]::StartNew()
foreach ($test in $tests) {
    & (Join-Path $PSScriptRoot $test) | Out-Host
}
$started.Stop()

Write-Output "WiSpatialValidationPassed:$($tests.Count)"
Write-Output "WiSpatialValidationElapsedMs:$($started.ElapsedMilliseconds)"
