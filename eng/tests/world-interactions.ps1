$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/execution-ledgers/manage-world-interactions.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/world-interaction-catalog.md"

$first = & $script -Mode Write
$firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$firstWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$secondWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & $script -Mode Check
$catalog = Get-Content -LiteralPath (
    Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$stages = Get-Content -LiteralPath (
    Join-Path $repositoryRoot ([string] $catalog.evidenceStageCatalogPath)) -Raw -Encoding UTF8 |
    ConvertFrom-Json

if ($firstHash -ne $secondHash) { throw "WorldInteractionCatalogGenerationIsNotDeterministic" }
if ($firstWriteTicks -ne $secondWriteTicks) { throw "WorldInteractionCatalogUnchangedOutputWasRewritten" }
if ((@($stages.stages.code) -join ",") -ne "E0,E1,E2,E3,E4,E5,E6,E7") {
    throw "WorldInteractionEvidenceStageOrderInvalid"
}
if (@($catalog.items | Where-Object { $_.integration.targetStage -ne "E7" }).Count -ne 0) {
    throw "WorldInteractionIntegrationTargetMustBeE7"
}
if ([string] $catalog.schemaVersion -ne "3" -or [string] $catalog.revision -ne "simulation-world-interactions.r5") {
    throw "WorldInteractionCatalogRevisionMustBeR5"
}
$natureIds = @("WI-NATURE-01", "WI-NATURE-02", "WI-NATURE-03", "WI-NATURE-04")
$natureItems = @($catalog.items | Where-Object id -in $natureIds | Sort-Object sequence)
if ($natureItems.Count -ne 4 -or (@($natureItems.id) -join ",") -ne ($natureIds -join ",")) {
    throw "WorldInteractionNatureContractsMissing"
}
if (@($natureItems | Where-Object {
        $_.implementation.currentStage -ne "E3" -or $_.implementation.status -ne "Done"
    }).Count -ne 0) {
    throw "WorldInteractionNatureEvidenceStagesInvalid"
}
if ((@($natureItems.actionCode) -join ",") -ne "RegionalThreatObservation,EmergencyRetreat,NatureRestoration,PartyRecovery") {
    throw "WorldInteractionNatureActionCodesInvalid"
}
$e4Items = @($catalog.items | Where-Object { $_.integration.currentStage -eq "E4" })
$e5Items = @($catalog.items | Where-Object { $_.integration.currentStage -eq "E5" })
$e6Items = @($catalog.items | Where-Object { $_.integration.currentStage -eq "E6" })
if ($e4Items.Count -ne 5) { throw "WorldInteractionE4SeedbedItemCountMustBe5" }
if ($e5Items.Count -ne 0) { throw "WorldInteractionFarmE5MustBePromoted" }
if ($e6Items.Count -ne 8) { throw "WorldInteractionFarmE6ItemCountMustBe8" }
if (@($e4Items | Where-Object { @($_.integration.e4SeedbedRefs).Count -eq 0 }).Count -ne 0) {
    throw "WorldInteractionE4SeedbedRefMissing"
}
if (@(($e4Items + $e6Items).integration.e4SeedbedRefs | Select-Object -Unique).Count -ne 5) {
    throw "WorldInteractionE4SeedbedCountMustBe5"
}
if (@($e6Items | Where-Object { @($_.integration.e5PlacementRefs).Count -ne 1 -or $_.integration.e5PlacementRefs[0] -notlike "binding:actual-e5:*" -or @($_.integration.e6EvidenceRefs).Count -eq 0 }).Count -ne 0) {
    throw "WorldInteractionFarmE5BindingInvalid"
}
if ($check -notmatch "WorldInteractionCatalogValid:41") {
    throw "WorldInteractionCatalogValidationDidNotComplete"
}

Write-Output "WorldInteractionCatalogTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
