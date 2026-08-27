$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot `
    "eng/execution-ledgers/manage-playable-loop-engine-interaction-validation.ps1"
& $manager -Mode Validate

Write-Output "PlayableLoopEngineInteractionValidationTestPassed"
