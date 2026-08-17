$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/execution-ledgers/manage-world-interactions.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/world-interaction-catalog.md"

$first = & pwsh -NoProfile -File $script -Mode Write
$firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$second = & pwsh -NoProfile -File $script -Mode Write
$secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$check = & pwsh -NoProfile -File $script -Mode Check
$catalog = Get-Content -LiteralPath (
    Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$stages = Get-Content -LiteralPath (
    Join-Path $repositoryRoot ([string] $catalog.evidenceStageCatalogPath)) -Raw -Encoding UTF8 |
    ConvertFrom-Json

if ($firstHash -ne $secondHash) { throw "WorldInteractionCatalogGenerationIsNotDeterministic" }
if ((@($stages.stages.code) -join ",") -ne "E0,E1,E2,E3,E4,E5,E6,E7") {
    throw "WorldInteractionEvidenceStageOrderInvalid"
}
if (@($catalog.items | Where-Object { $_.integration.targetStage -ne "E7" }).Count -ne 0) {
    throw "WorldInteractionIntegrationTargetMustBeE7"
}
if ($check -notmatch "WorldInteractionCatalogValid:37") {
    throw "WorldInteractionCatalogValidationDidNotComplete"
}

Write-Output "WorldInteractionCatalogTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
