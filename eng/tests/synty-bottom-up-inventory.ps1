$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/world-seedbeds/manage-synty-bottom-up-inventory.ps1"

& pwsh -NoProfile -File $script -Mode Check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Output "SyntyBottomUpInventoryTestsPassed"
