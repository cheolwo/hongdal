$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-spatial-formation-modes.ps1"
$result = & $manager
if ([string] $result -ne "SpatialFormationModesValid:Modes=3;Levels=H1-H4;PlayerDirect=H1") { throw "SpatialFormationModesValidationFailed:$result" }
Write-Output "SpatialFormationModesTestsPassed:Modes=3;Levels=4"
