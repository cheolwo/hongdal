$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/execution-ledgers/manage-world-interactions.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/world-interaction-catalog.md"
$contractOutput = Join-Path $repositoryRoot "Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationWorldInteractionNames.generated.cs"

$first = & $script -Mode Write
$firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$firstWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$firstContractHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $contractOutput).Hash
$firstContractWriteTicks = (Get-Item -LiteralPath $contractOutput).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$secondWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$secondContractHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $contractOutput).Hash
$secondContractWriteTicks = (Get-Item -LiteralPath $contractOutput).LastWriteTimeUtc.Ticks
$check = & $script -Mode Check
$catalog = Get-Content -LiteralPath (
    Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$stages = Get-Content -LiteralPath (
    Join-Path $repositoryRoot ([string] $catalog.evidenceStageCatalogPath)) -Raw -Encoding UTF8 |
    ConvertFrom-Json

if ($firstHash -ne $secondHash) { throw "WorldInteractionCatalogGenerationIsNotDeterministic" }
if ($firstWriteTicks -ne $secondWriteTicks) { throw "WorldInteractionCatalogUnchangedOutputWasRewritten" }
if ($firstContractHash -ne $secondContractHash) { throw "WorldInteractionNameContractGenerationIsNotDeterministic" }
if ($firstContractWriteTicks -ne $secondContractWriteTicks) { throw "WorldInteractionNameContractUnchangedOutputWasRewritten" }
if ((@($stages.stages.code) -join ",") -ne
    "E0,E1,E2,E3,E4,E5,E6,E7,E8,E9,E10") {
    throw "WorldInteractionEvidenceStageOrderInvalid"
}
if (@($catalog.items | Where-Object { $_.integration.targetStage -ne "E7" }).Count -ne 0) {
    throw "WorldInteractionIntegrationTargetMustBeE7"
}
if ([string] $catalog.schemaVersion -ne "5" -or [string] $catalog.revision -ne "simulation-world-interactions.r27") {
    throw "WorldInteractionCatalogRevisionMustBeR27"
}
$responsibilities = Get-Content -LiteralPath (
    Join-Path $repositoryRoot ([string] $catalog.responsibilityPolicyPath)) -Raw -Encoding UTF8 |
    ConvertFrom-Json
$flows = Get-Content -LiteralPath (
    Join-Path $repositoryRoot ([string] $catalog.workflowCatalogPath)) -Raw -Encoding UTF8 |
    ConvertFrom-Json
$polarities = Get-Content -LiteralPath (
    Join-Path $repositoryRoot ([string] $catalog.polarityQuadrantCatalogPath)) -Raw -Encoding UTF8 |
    ConvertFrom-Json
if (@($responsibilities.primaryOutcomeCodes.PSObject.Properties).Count -ne 64) {
    throw "WorldInteractionPrimaryOutcomeCountInvalid"
}
if (@($responsibilities.legacyCompositeMigrations).Count -ne 6) {
    throw "WorldInteractionLegacyCompositeAuditCountInvalid"
}
if (@($responsibilities.proceduralStepMigrations).Count -ne 6) {
    throw "WorldInteractionProceduralStepAuditCountInvalid"
}
if (@($responsibilities.actorResponsibilityMigrations).Count -ne 4) {
    throw "WorldInteractionActorResponsibilityAuditCountInvalid"
}
if ((@($responsibilities.proceduralStepMigrations.worldInteractionId | Sort-Object) -join ",") -ne
    ((@("WI-LOG-02", "WI-LOG-03", "WI-LOG-04", "WI-LOG-05",
        "WI-ORDER-02", "WI-ORDER-05") | Sort-Object) -join ",")) {
    throw "WorldInteractionProceduralStepAuditSetInvalid"
}
if ((@($responsibilities.actorResponsibilityMigrations.worldInteractionId | Sort-Object) -join ",") -ne
    ((@("WI-HUB-04", "WI-HUB-05", "WI-ORDER-03", "WI-ORDER-04") |
        Sort-Object) -join ",")) {
    throw "WorldInteractionActorResponsibilityAuditSetInvalid"
}
if ((@($responsibilities.legacyCompositeMigrations.worldInteractionId | Sort-Object) -join ",") -ne
    ((@("WI-001", "WI-NATURE-11", "WI-NATURE-14", "WI-NATURE-16",
        "WI-NATURE-17", "WI-WORLD-07") | Sort-Object) -join ",")) {
    throw "WorldInteractionLegacyCompositeAuditSetInvalid"
}
if (@($flows.flows.edges).Count -ne 60) {
    throw "WorldInteractionFlowEdgeCountInvalid"
}
if (@($polarities.fixedYangWorldInteractionIds).Count -ne 27 -or
    @($polarities.fixedYinWorldInteractionIds).Count -ne 23 -or
    @($polarities.contextualWorldInteractionIds).Count -ne 6 -or
    @($polarities.notApplicableWorldInteractionIds).Count -ne 8) {
    throw "WorldInteractionPolarityCoverageInvalid"
}
if ((@($polarities.actorMigrationGatedWorldInteractionIds | Sort-Object) -join ",") -ne
    ((@("WI-HUB-04", "WI-HUB-05", "WI-ORDER-03", "WI-ORDER-04") |
        Sort-Object) -join ",")) {
    throw "WorldInteractionPolarityActorMigrationGateInvalid"
}
if ((@($polarities.quadrants.symbol) -join ",") -ne "++,+-,-+,--" -or
    -not [bool] $polarities.principles.triggerSourceDoesNotDetermineActorSign -or
    -not [bool] $polarities.principles.classificationDoesNotChangeSimulationEffects) {
    throw "WorldInteractionPolarityPrinciplesInvalid"
}
if (-not [bool] $responsibilities.principles.actorBindingDoesNotChangeResponsibility -or
    -not [bool] $responsibilities.principles.workflowOrderingIsOwnedOutsideWorldInteraction -or
    -not [bool] $flows.principles.flowOrderDoesNotBecomeWorldInteractionIdentity) {
    throw "WorldInteractionSingleResponsibilityPrinciplesMissing"
}
$actualE5Generator = Get-Content -LiteralPath (
    Join-Path $repositoryRoot "eng/world-seedbeds/manage-actual-e5-spatial.ps1") -Raw -Encoding UTF8
if ($actualE5Generator -notmatch 'worldInteractionFlows' -or
    $actualE5Generator -match '\$wi\.successorWiIds') {
    throw "ActualE5SpatialStillUsesInlineWorldInteractionProcedure"
}
$generatedContract = Get-Content -LiteralPath $contractOutput -Raw -Encoding UTF8
if ($generatedContract -notmatch "public int 대장순번" -or
    $generatedContract -notmatch "public string 주요결과코드" -or
    $generatedContract -notmatch "public string 음양분류Code" -or
    $generatedContract -notmatch "public static string 문맥음양Code" -or
    $generatedContract -match '단계 ·') {
    throw "WorldInteractionGeneratedContractStillProcedureCentric"
}
$groupCodes = @($catalog.items.groupCode | Select-Object -Unique)
if (@($catalog.groupDisplayNames.PSObject.Properties).Count -ne $groupCodes.Count) {
    throw "WorldInteractionGroupDisplayNamesInvalid"
}
foreach ($item in @($catalog.items)) {
    if ([string] $item.title -notmatch "[가-힣]") {
        throw "WorldInteractionKoreanTitleMissing:$($item.id)"
    }
}
foreach ($group in @($catalog.items | Group-Object groupCode)) {
    $sequence = @($group.Group | Sort-Object sequence | ForEach-Object { [int] $_.sequence })
    if (($sequence -join ",") -ne ((1..$group.Count) -join ",")) {
        throw "WorldInteractionGroupSequenceInvalid:$($group.Name)"
    }
}
$natureIds = @(
    "WI-NATURE-01", "WI-NATURE-02", "WI-NATURE-03", "WI-NATURE-04",
    "WI-NATURE-05", "WI-NATURE-06", "WI-NATURE-07", "WI-NATURE-08",
    "WI-NATURE-09", "WI-NATURE-10", "WI-NATURE-11", "WI-NATURE-12",
    "WI-NATURE-13", "WI-NATURE-14", "WI-NATURE-15", "WI-NATURE-16",
    "WI-NATURE-17", "WI-NATURE-18"
)
$natureItems = @($catalog.items | Where-Object id -in $natureIds | Sort-Object sequence)
if ($natureItems.Count -ne 18 -or (@($natureItems.id) -join ",") -ne ($natureIds -join ",")) {
    throw "WorldInteractionNatureContractsMissing"
}
if (@($natureItems | Where-Object {
        $_.implementation.currentStage -ne "E3" -or $_.implementation.status -ne "Done"
    }).Count -ne 0) {
    throw "WorldInteractionNatureEvidenceStagesInvalid"
}
if ((@($natureItems.actionCode) -join ",") -ne "RegionalThreatObservation,EmergencyRetreat,NatureRestoration,PartyRecovery,AcquireAxe,BeginHarvest,PlaceCabinBlueprint,BeginCabinBuild,EnterCabin,LeaveCabin,ResolveEncounter,CancelActiveWork,StoreAtCabin,SleepInCabin,SelectExpansionPlan,PrepareFieldSupply,PrepareFieldSupplyDelegated,CollectDroppedTimber") {
    throw "WorldInteractionNatureActionCodesInvalid"
}
$e4Items = @($catalog.items | Where-Object { $_.integration.currentStage -eq "E4" })
$e4SpatialItems = @($e4Items | Where-Object { @($_.spatialRequirements).Count -gt 0 })
$e5Items = @($catalog.items | Where-Object { $_.integration.currentStage -eq "E5" })
$e6Items = @($catalog.items | Where-Object { $_.integration.currentStage -eq "E6" })
$e7Items = @($catalog.items | Where-Object { $_.integration.currentStage -eq "E7" })
if ($e4Items.Count -ne 14) { throw "WorldInteractionE4SeedbedItemCountMustBe14" }
if ((@($e5Items.id | Sort-Object) -join ",") -ne
    "WI-ACTOR-01,WI-ACTOR-02,WI-NATURE-14") {
    throw "WorldInteractionE5ItemsInvalid"
}
if ($e6Items.Count -ne 9) { throw "WorldInteractionE6ItemCountMustBe9" }
if ($e7Items.Count -ne 6) { throw "WorldInteractionE7ItemCountMustBe6" }
if (@($e4SpatialItems | Where-Object { @($_.integration.e4SeedbedRefs).Count -eq 0 }).Count -ne 0) {
    throw "WorldInteractionE4SeedbedRefMissing"
}
if (@(($e4SpatialItems + $e6Items + $e7Items).integration.e4SeedbedRefs | Select-Object -Unique).Count -ne 7) {
    throw "WorldInteractionE4SeedbedCountMustBe7"
}
if (@($e6Items | Where-Object id -ne "WI-NATURE-05" | Where-Object {
        @($_.integration.e5PlacementRefs).Count -ne 1 -or
        $_.integration.e5PlacementRefs[0] -notlike "binding:actual-e5:*" -or
        @($_.integration.e6EvidenceRefs).Count -eq 0
    }).Count -ne 0) {
    throw "WorldInteractionFarmE5BindingInvalid"
}
$natureThreatObservation = @($e7Items | Where-Object id -eq "WI-NATURE-01")
if ($natureThreatObservation.Count -ne 1 -or
    @($natureThreatObservation[0].integration.e4SeedbedRefs) -notcontains
        "wi-spatial-seedbed:nature-survival-encounter.v1" -or
    @($natureThreatObservation[0].integration.e5PlacementRefs) -notcontains
        "binding:actual-e5:wi-nature-01" -or
    @($natureThreatObservation[0].integration.e6EvidenceRefs).Count -lt 2) {
    throw "WorldInteractionNatureThreatObservationE5BindingInvalid"
}
$natureAxe = @($e6Items | Where-Object id -eq "WI-NATURE-05")
if ($natureAxe.Count -ne 1 -or
    $natureAxe[0].integration.status -ne "InProgress" -or
    @($natureAxe[0].integration.e5PlacementRefs).Count -ne 1 -or
    $natureAxe[0].integration.e5PlacementRefs[0] -ne
        "spatial:actual-e5:wi-nature-05" -or
    @($natureAxe[0].integration.e6EvidenceRefs).Count -eq 0 -or
    @($natureAxe[0].integration.e7EvidenceRefs).Count -lt 3) {
    throw "WorldInteractionNatureAxeEvidenceInvalid"
}
if ($check -notmatch "WorldInteractionCatalogValid:64") {
    throw "WorldInteractionCatalogValidationDidNotComplete"
}

Write-Output "WorldInteractionCatalogTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
