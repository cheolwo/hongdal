$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-gameplay-led-h-inventory.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/generated/gameplay-led-h-inventory.v1.json"

$firstWrite = & pwsh -NoProfile -File $manager -Mode Write
$beforeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
$secondWrite = & pwsh -NoProfile -File $manager -Mode Write
$afterHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
if ($firstWrite -notmatch "GameplayLedHInventoryGenerated:Plans=4;H1=52\+32;H2=37;H3=20;H4=6;Violations=0") { throw "GameplayLedHInventoryFirstWriteFailed" }
if ($check -notmatch "GameplayLedHInventoryValid:Plans=4;H1=52\+32;H2=37;H3=20;H4=6;Violations=0") { throw "GameplayLedHInventoryCheckFailed" }
if ($secondWrite -notmatch "GameplayLedHInventoryGenerated:Plans=4;H1=52\+32;H2=37;H3=20;H4=6;Violations=0") { throw "GameplayLedHInventorySecondWriteFailed" }
if ($beforeHash -ne $afterHash -or $beforeTicks -ne $afterTicks) { throw "GameplayLedHInventoryNonDeterministic" }

$report = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
if ([int] $report.counts.violations -ne 0) { throw "GameplayLedHInventoryViolationFound" }
if ([int] $report.counts.quarantinedExpressionH1 -ne 9) { throw "GameplayLedHInventoryQuarantineCountInvalid" }
if ((@($report.planCoverage.gamePlanCode) -join ",") -ne "NatureHomeThreatRecovery,FarmProductionSurvival,TownLivingMarketSafety,CityHubLogisticsResilience") { throw "GameplayLedHInventoryPlanOrderInvalid" }
$town = @($report.planCoverage | Where-Object gamePlanCode -eq "TownLivingMarketSafety")
if (@($town.coverage.h1InteractionRefs) -notcontains "h1-stock:town-order-packing") { throw "GameplayLedHInventoryOrderPackingMissing" }
$nature = @($report.planCoverage | Where-Object gamePlanCode -eq "NatureHomeThreatRecovery")
$natureWiIds = @("WI-NATURE-01", "WI-NATURE-02", "WI-NATURE-03", "WI-NATURE-04")
foreach ($wiId in $natureWiIds) {
    if (@($nature.coreWiIds) -notcontains $wiId) { throw "GameplayLedHInventoryNatureWiMissing:$wiId" }
}
$natureEvidence = @($report.wiEvidenceQueue | Where-Object priorityCode -eq "E-P4")
if ($natureEvidence.Count -ne 1 -or (@($natureEvidence.wiIds) -join ",") -ne ($natureWiIds -join ",")) {
    throw "GameplayLedHInventoryNatureEvidenceQueueInvalid"
}
if ([string] $natureEvidence.evidenceTrackCode -ne "Integration") { throw "GameplayLedHInventoryNatureEvidenceTrackInvalid" }
if ((@($natureEvidence.currentStageCodes) -join ",") -ne "E1") { throw "GameplayLedHInventoryNatureCurrentStageInvalid" }
if ([string] $natureEvidence.targetStageCode -ne "E4") { throw "GameplayLedHInventoryNatureTargetStageInvalid" }
if ((@($report.hExpansionQueue.priorityCode) -join ",") -ne "H-P0,H-P1,H-P2,H-P3,H-P4") { throw "GameplayLedHInventoryHPriorityInvalid" }
if ((@($report.wiEvidenceQueue.priorityCode) -join ",") -ne "E-P1,E-P2,E-P3,E-P4,E-P5") { throw "GameplayLedHInventoryEPriorityInvalid" }
if ([string] $report.wiEvidenceQueue[-1].targetStageCode -ne "E6") { throw "GameplayLedHInventoryE6BoundaryInvalid" }
if ([string] $report.policyRevision -ne "simulation-world-gameplay-led-h-policy.r7") { throw "GameplayLedHInventoryPolicyRevisionInvalid" }
if (@($report.planCoverage.stagedPackNativeH2Refs | Sort-Object -Unique).Count -ne 0) { throw "GameplayLedHInventoryStagedPackNativeH2MustBeEmptyAfterH3Promotion" }
$playableSlice = @($report.playableSliceSummary | Where-Object playableSliceId -eq "reference-play:nature-farm-day.v1")
if ($playableSlice.Count -ne 1) { throw "GameplayLedHInventoryPlayableSliceMissing" }
if ([string] $playableSlice[0].declaredPlayableSliceStateCode -ne "SpatiallyComposed") { throw "GameplayLedHInventoryPlayableSliceStateInvalid" }
if ([string] $playableSlice[0].theorySpatialBindingStateCode -ne "E5TheoryQualified") { throw "GameplayLedHInventoryPlayableSliceTheorySpatialBoundaryInvalid" }
if ([string] $playableSlice[0].actualSpatialBindingStateCode -ne "ActualE5Bound") { throw "GameplayLedHInventoryPlayableSliceActualSpatialBoundaryInvalid" }
if ((@($report.warningOnlyGamePlanCodesMissingPlayableSlice) -join ",") -ne "CityHubLogisticsResilience,TownLivingMarketSafety") { throw "GameplayLedHInventoryWarningOnlyPlansInvalid" }
if ([string] $report.demandRevision -ne "simulation-world-gameplay-h-inventory-demands.r1") { throw "GameplayLedHInventoryDemandRevisionInvalid" }
if ([int] $report.counts.inventoryDemands -ne 5 -or [int] $report.counts.satisfiedInventoryDemands -ne 4) { throw "GameplayLedHInventoryDemandCountInvalid" }
if ((@($report.inventoryDemandSummary.newH2Refs | Sort-Object -Unique) -join ",") -ne "h2-candidate:hub-fulfillment,h2-candidate:town-market-receiving,h2-candidate:town-order-fulfillment") { throw "GameplayLedHInventoryNewH2DemandInvalid" }
if ((@($report.inventoryDemandSummary.newH3Refs | Sort-Object -Unique) -join ",") -ne "h3-candidate:hub-fulfillment-operations,h3-candidate:nature-threat-recovery,h3-candidate:town-market-fulfillment") { throw "GameplayLedHInventoryNewH3DemandInvalid" }

Write-Output "GameplayLedHInventoryTestsPassed:Plans=4;H2=37;H3=20;Demands=5;Violations=0;OrderPacking=Covered"
