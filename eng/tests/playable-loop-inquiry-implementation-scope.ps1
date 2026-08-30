[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$managerPath = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-playable-loop-inquiry-implementation-scope.ps1'
$catalogPath = Join-Path $repositoryRoot 'eng/execution-ledgers/playable-loop-inquiry-implementation-scope.json'
$generatedPath = Join-Path $repositoryRoot 'docs/AI/generated/playable-loop-inquiry-implementation-scope.md'

$result = & powershell -NoProfile -ExecutionPolicy Bypass -File $managerPath -Mode Write
if ($LASTEXITCODE -ne 0) { throw 'PlayableLoopInquiryImplementationScopeManagerFailed' }
if (($result -join "`n") -notmatch 'Questions=339') { throw 'QuestionCount339Missing' }

$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([int] $catalog.questionRange.last -ne 339) { throw 'QuestionRangeLastInvalid' }
if ([string] $catalog.revision -notmatch '^playable-loop-inquiry-implementation-scope\.r[0-9]+$') { throw 'RevisionInvalid' }
$goal = Get-Content (Join-Path $repositoryRoot 'eng/execution-ledgers/codex-playable-loop-goals.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$workOrder = Get-Content (Join-Path $repositoryRoot $catalog.executionRouting.activeWorkOrderRef) -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string] $catalog.executionRouting.questionNumberRoleCode -ne 'TraceabilityAndExtractionTraversal') { throw 'QuestionNumberRoleInvalid' }
if ([string] $catalog.executionRouting.questionTraversalPurposeCode -ne 'WorldInteractionExtraction') { throw 'QuestionTraversalPurposeInvalid' }
if ([string] $catalog.executionRouting.questionTraversalDirectionCode -ne 'Q001ToQ339') { throw 'QuestionTraversalDirectionInvalid' }
if ([string] $catalog.executionRouting.selectionModeCode -ne 'ActivePlayableUnitDependency') { throw 'ExecutionSelectionModeInvalid' }
if ([string] $catalog.executionRouting.impactReviewDirectionCode -ne 'E7ToE1') { throw 'ImpactReviewDirectionInvalid' }
if ([string] $catalog.executionRouting.initialAssemblyDirectionCode -ne 'E1ToE7') { throw 'InitialAssemblyDirectionInvalid' }
if ([string] $catalog.executionRouting.implementationCycleCode -ne 'EarliestAffectedStageReopen') { throw 'ImplementationCycleInvalid' }
if ([string] $catalog.executionRouting.logicPresentationCycleCode -ne 'Bidirectional') { throw 'LogicPresentationCycleInvalid' }
if ([string] $catalog.executionRouting.dispatchStateCode -ne 'WaitingForApprovedRevision') { throw 'DispatchStateInvalid' }
if (@($catalog.executionRouting.parkedCandidateQuestionIds) -notcontains 'Q-025') { throw 'Q025ParkedCandidateMissing' }
if (@($catalog.executionRouting.invariants) -notcontains 'TraverseQuestionsByNumericOrderForWorldInteractionExtraction') { throw 'SequentialExtractionInvariantMissing' }
if (@($catalog.executionRouting.invariants) -notcontains 'DoNotSelectImplementationByNumericOrder') { throw 'NumericImplementationOrderGuardMissing' }
if (@($catalog.executionRouting.invariants) -notcontains 'DoNotSelectImplementationByReverseNumericOrder') { throw 'ReverseNumericImplementationOrderGuardMissing' }
if ((@($catalog.worldInteractionExtraction.evidenceCycleQueueSelectionBasisCodes) -join ',') -ne 'ApprovedWorkItemDependencies,ApprovedPlanningGate,RegisteredWorldInteraction,EarliestReopenEvidenceStage') { throw 'WorldInteractionEvidenceCycleQueueSelectionBasisInvalid' }
if ([string] $catalog.worldInteractionExtraction.evidenceCycleQueueTieBreakCode -ne 'WorldInteractionStableIdForDeterministicOutputOnly') { throw 'WorldInteractionEvidenceCycleQueueTieBreakInvalid' }

$batches = @($catalog.smallImplementationBatches)
if ($batches.Count -ne 110) { throw 'SmallImplementationBatchCountInvalid' }
$currentBatch = @($batches | Where-Object { $_.batchStableId -eq [string] $catalog.currentWorkBatchStableId })
if ($currentBatch.Count -ne 1) { throw 'CurrentSmallImplementationBatchMissingOrDuplicated' }
if ([string] $currentBatch[0].playableUnitStableId -ne $catalog.executionRouting.activePlayableUnitStableId) { throw 'CurrentBatchPlayableUnitInvalid' }
if (@($currentBatch[0].worldInteractionIds) -notcontains $catalog.executionRouting.activeWorldInteractionId) { throw 'CurrentBatchWorldInteractionInvalid' }
if ([string] $currentBatch[0].executionStateCode -ne 'WaitingForApprovedRevision') { throw 'CurrentBatchMustWaitForApprovedRevision' }
if ([string] $currentBatch[0].nextStageCode -ne 'E2') { throw 'CurrentBatchEarliestResumeStageInvalid' }
if (@($currentBatch[0].questionIds | Where-Object { $_ -notin $workOrder.approvedQuestionScope }).Count -gt 0) { throw 'CurrentBatchQuestionBindingInvalid' }
$sleepSpatialBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:nature-shelter-sleep:risky-sleep-weather-spatial.r1' })
if ($sleepSpatialBatch.Count -ne 1) { throw 'SleepSpatialBatchMissing' }
if ([string] $sleepSpatialBatch[0].executionStateCode -ne 'ParkedCandidate') { throw 'SleepSpatialBatchMustBeParked' }
if (@($sleepSpatialBatch[0].worldInteractionIds) -notcontains 'WI-NATURE-14') { throw 'SleepSpatialBatchWorldInteractionInvalid' }
$lhBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:farm-landscape:placement-plan-lh-lifecycle.r1' })
if ($lhBatch.Count -ne 1 -or [string] $lhBatch[0].executionStateCode -ne 'ImplementedParked') { throw 'LhImplementedParkedBatchInvalid' }
if ([string] $lhBatch[0].commonModuleStableId -ne 'module:lh-space-lifecycle') { throw 'LhBatchCommonModuleInvalid' }
if ([string] $lhBatch[0].currentStageCode -ne 'N/A' -or [string] $lhBatch[0].nextStageCode -ne 'N/A') { throw 'LhCommonModuleMustNotOwnEvidenceStage' }
if (@($lhBatch[0].worldInteractionIds).Count -ne 0) { throw 'LhLifecycleMustNotBindWorldWorkCancellationInteraction' }
$plannedBrewBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains 'WI-CRAFT-BREW' })
if ($plannedBrewBatches.Count -ne 4) { throw 'PlannedBrewBatchCountInvalid' }
$sourceRecoveryBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:nature-basic-herbal-recovery:source-recovery-decisions.r1' })
if ($sourceRecoveryBatch.Count -ne 1 -or [string] $sourceRecoveryBatch[0].executionStateCode -ne 'PlanningBlocked') { throw 'SourceRecoveryPlanningBlockedBatchInvalid' }
$herbalQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'herbal-recipe-crafting' })) { $herbalQuestionIds += @($batch.questionIds) }
if (@($herbalQuestionIds | Sort-Object -Unique).Count -ne 47) { throw 'HerbalTopicBatchCoverageInvalid' }
$shelterQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'nature-shelter-sleep' })) { $shelterQuestionIds += @($batch.questionIds) }
if (@($shelterQuestionIds | Sort-Object -Unique).Count -ne 23) { throw 'NatureShelterTopicBatchCoverageInvalid' }
if (@($catalog.fullyPartitionedTopicCodes).Count -ne 15) { throw 'FullyPartitionedTopicCountInvalid' }
$playerMindQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'player-mind-meditation' })) { $playerMindQuestionIds += @($batch.questionIds) }
if (@($playerMindQuestionIds | Sort-Object -Unique).Count -ne 32) { throw 'PlayerMindTopicBatchCoverageInvalid' }
$plannedPlayerPlanBatches = @($batches | Where-Object { @($_.worldInteractionIds) -contains 'WI-ACTOR-PLAN-SET' -or ($null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains 'WI-ACTOR-PLAN-SET') })
if ($plannedPlayerPlanBatches.Count -ne 1) { throw 'PlayerPlanBatchBindingInvalid' }
$natureConstructionQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'nature-resource-construction' })) { $natureConstructionQuestionIds += @($batch.questionIds) }
if (@($natureConstructionQuestionIds | Sort-Object -Unique).Count -ne 15) { throw 'NatureResourceConstructionTopicBatchCoverageInvalid' }
$plannedResourceRegenerationBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains 'WI-WORLD-RESOURCE-REGENERATE' })
if ($plannedResourceRegenerationBatches.Count -ne 1) { throw 'PlannedResourceRegenerationBatchInvalid' }
$plannedConstructionContributionBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains 'WI-CON-WORK-CONTRIBUTE' })
if ($plannedConstructionContributionBatches.Count -ne 2) { throw 'PlannedConstructionContributionBatchCountInvalid' }
$saveLoadQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'save-load-runtime' })) { $saveLoadQuestionIds += @($batch.questionIds) }
if (@($saveLoadQuestionIds | Sort-Object -Unique).Count -ne 7) { throw 'SaveLoadRuntimeTopicBatchCoverageInvalid' }
$saveLoadBatches = @($batches | Where-Object { $_.topicCode -eq 'save-load-runtime' })
if (@($saveLoadBatches | Where-Object { $_.currentStageCode -ne 'N/A' -or $_.nextStageCode -ne 'N/A' }).Count -ne 0) { throw 'CommonModuleBatchMustNotOwnEvidenceStage' }
if (@($saveLoadBatches | Where-Object { $null -eq $_.PSObject.Properties['commonModuleStableId'] }).Count -ne 0) { throw 'SaveLoadCommonModuleSubjectMissing' }
$plannedHeatSourceBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains 'WI-HEAT-SOURCE-STATE-CHANGE' })
if ($plannedHeatSourceBatches.Count -ne 4) { throw 'PlannedHeatSourceBatchCountInvalid' }
$plannedDefenseRepairBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains 'WI-DEFENSE-SEGMENT-REPAIR' })
if ($plannedDefenseRepairBatches.Count -ne 1 -or [string] $plannedDefenseRepairBatches[0].executionStateCode -ne 'ParkedCandidate') { throw 'PlannedDefenseRepairBatchInvalid' }
$farmSpatialQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'farm-building-spatial-placement' })) { $farmSpatialQuestionIds += @($batch.questionIds) }
if (@($farmSpatialQuestionIds | Sort-Object -Unique).Count -ne 56) { throw 'FarmBuildingSpatialTopicBatchCoverageInvalid' }
foreach ($plannedFarmWi in @('WI-FARM-LAND-IMPROVE', 'WI-FARM-SOIL-AMEND', 'WI-FARM-FIELD-BOUNDARY-CONFIRM', 'WI-FARM-WATER-TRANSFER')) {
    $plannedFarmBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains $plannedFarmWi })
    if ($plannedFarmBatches.Count -ne 1 -or [string] $plannedFarmBatches[0].executionStateCode -ne 'ParkedCandidate') { throw "PlannedFarmWorldInteractionBatchInvalid:$plannedFarmWi" }
}
$townOrderQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'town-order-pickup' })) { $townOrderQuestionIds += @($batch.questionIds) }
if (@($townOrderQuestionIds | Sort-Object -Unique).Count -ne 11) { throw 'TownOrderPickupTopicBatchCoverageInvalid' }
foreach ($plannedTownWi in @('WI-TOWN-STOCK-REPLENISH', 'WI-TOWN-DELIVERY-RECEIVE', 'WI-TOWN-DELIVERY-INSPECT', 'WI-TOWN-STOCK-PUTAWAY', 'WI-TOWN-SUPPLY-DISPATCH')) {
    $plannedTownBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains $plannedTownWi })
    if ($plannedTownBatches.Count -ne 1 -or [string] $plannedTownBatches[0].executionStateCode -ne 'ParkedCandidate') { throw "PlannedTownWorldInteractionBatchInvalid:$plannedTownWi" }
}
$regionThreatQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'region-five-elements-monster' })) { $regionThreatQuestionIds += @($batch.questionIds) }
if (@($regionThreatQuestionIds | Sort-Object -Unique).Count -ne 35) { throw 'RegionFiveElementsMonsterTopicBatchCoverageInvalid' }
foreach ($plannedThreatWi in @('WI-NATURE-TRACE-INVESTIGATE', 'WI-NATURE-THREAT-CORE-CLEAR', 'WI-COMMUNITY-COOPERATION-PROPOSE', 'WI-COMBAT-TACTICAL-COMMAND', 'WI-COMBAT-DIRECT-CONTROL-SET', 'WI-COMBAT-CASUALTY-RESPONSE', 'WI-THREAT-NONCOMBAT-RESOLVE')) {
    $plannedThreatBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains $plannedThreatWi })
    if ($plannedThreatBatches.Count -lt 1) { throw "PlannedRegionalThreatWorldInteractionBatchInvalid:$plannedThreatWi" }
}
$noncombatBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:region-threat:noncombat-resolution-and-world-return.r1' })
if ($noncombatBatch.Count -ne 1 -or [string] $noncombatBatch[0].executionStateCode -ne 'PlanningBlocked') { throw 'NoncombatResolutionPlanningBlockInvalid' }
$communityVisitorQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'community-membership-visitor' })) { $communityVisitorQuestionIds += @($batch.questionIds) }
if (@($communityVisitorQuestionIds | Sort-Object -Unique).Count -ne 21) { throw 'CommunityMembershipVisitorTopicBatchCoverageInvalid' }
foreach ($plannedCommunityWi in @('WI-COMMUNITY-MEMBERSHIP-CONFIRM', 'WI-COMMUNITY-REMOTE-RESPONSE', 'WI-COMMUNITY-ENTRANCE-POLICY-SET')) {
    $plannedCommunityBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains $plannedCommunityWi })
    if ($plannedCommunityBatches.Count -ne 1) { throw "PlannedCommunityWorldInteractionBatchInvalid:$plannedCommunityWi" }
}
$visitorStayBatches = @($batches | Where-Object { @($_.worldInteractionIds) -contains 'WI-COMMUNITY-VISITOR-STAY' })
if ($visitorStayBatches.Count -ne 2) { throw 'RegisteredVisitorStayBatchCountInvalid' }
if (@($visitorStayBatches | Where-Object { $_.playableUnitStableId -ne 'playable-loop:nature-camp-visitor-stay.v1' }).Count -ne 0) { throw 'VisitorStayPlayableUnitBindingInvalid' }
$buildingPlacementQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'building-placement-assistance' })) { $buildingPlacementQuestionIds += @($batch.questionIds) }
if (@($buildingPlacementQuestionIds | Sort-Object -Unique).Count -ne 3) { throw 'BuildingPlacementAssistanceTopicBatchCoverageInvalid' }
$buildingPlacementBatches = @($batches | Where-Object { $_.topicCode -eq 'building-placement-assistance' })
if (@($buildingPlacementBatches | Where-Object { $_.currentStageCode -ne 'N/A' -or $_.nextStageCode -ne 'N/A' }).Count -ne 0) { throw 'BuildingPlacementCommonModuleMustNotOwnEvidenceStage' }
if (@($buildingPlacementBatches | Where-Object { $_.commonModuleStableId -notin @('module:placement-candidate-generation', 'module:placement-validation') }).Count -ne 0) { throw 'BuildingPlacementCommonModuleSubjectInvalid' }
$farmBarracksQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'farm-barracks-defense' })) { $farmBarracksQuestionIds += @($batch.questionIds) }
if (@($farmBarracksQuestionIds | Sort-Object -Unique).Count -ne 17) { throw 'FarmBarracksDefenseTopicBatchCoverageInvalid' }
foreach ($plannedBarracksWi in @('WI-COMMUNITY-SUPPORT-MISSION-JOIN')) {
    $plannedBarracksBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains $plannedBarracksWi })
    if ($plannedBarracksBatches.Count -ne 1 -or [string] $plannedBarracksBatches[0].executionStateCode -ne 'ParkedCandidate') { throw "PlannedFarmBarracksWorldInteractionBatchInvalid:$plannedBarracksWi" }
}
$registeredBarracksMobilize = @($batches | Where-Object { @($_.worldInteractionIds) -contains 'WI-FARM-DEFENSE-MOBILIZE' })
if ($registeredBarracksMobilize.Count -ne 1 -or [string] $registeredBarracksMobilize[0].executionStateCode -ne 'WaitingForApprovedRevision') { throw 'RegisteredFarmBarracksMobilizeBatchInvalid' }
$registeredSquadAssignment = @($batches | Where-Object { @($_.worldInteractionIds) -contains 'WI-SQUAD-ASSIGN' })
if ($registeredSquadAssignment.Count -ne 1 -or [string] $registeredSquadAssignment[0].executionStateCode -ne 'WaitingForApprovedRevision') { throw 'RegisteredFarmBarracksSquadAssignmentBatchInvalid' }
$registeredSquadSupply = @($batches | Where-Object { @($_.worldInteractionIds) -contains 'WI-SQUAD-SUPPLY' })
if ($registeredSquadSupply.Count -ne 1 -or [string] $registeredSquadSupply[0].executionStateCode -ne 'WaitingForApprovedRevision') { throw 'RegisteredFarmBarracksSquadSupplyBatchInvalid' }
$registeredDefenseResolve = @($batches | Where-Object { @($_.worldInteractionIds) -contains 'WI-FARM-DEFENSE-RESOLVE' })
if ($registeredDefenseResolve.Count -ne 1 -or [string] $registeredDefenseResolve[0].executionStateCode -ne 'WaitingForApprovedRevision') { throw 'RegisteredFarmBarracksDefenseResolveBatchInvalid' }
$registeredDefenseReturn = @($batches | Where-Object { @($_.worldInteractionIds) -contains 'WI-FARM-DEFENSE-RETURN' })
if ($registeredDefenseReturn.Count -ne 1 -or [string] $registeredDefenseReturn[0].executionStateCode -ne 'WaitingForApprovedRevision') { throw 'RegisteredFarmBarracksDefenseReturnBatchInvalid' }
if (@($catalog.executionRouting.parkedCandidateQuestionIds) -contains 'Q-223') { throw 'FullyPartitionedTopicMustNotRequireDuplicatedQuestionRouting' }
$hubDemandQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'hub-demand-allocation' })) { $hubDemandQuestionIds += @($batch.questionIds) }
if (@($hubDemandQuestionIds | Sort-Object -Unique).Count -ne 11) { throw 'HubDemandAllocationTopicBatchCoverageInvalid' }
foreach ($plannedHubDemandWi in @('WI-HUB-DEMAND-ALLOCATE', 'WI-HUB-DEMAND-REMAINDER-RETURN', 'WI-HUB-SUPPLY-TASK-ACCEPT')) {
    $plannedHubDemandBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains $plannedHubDemandWi })
    if ($plannedHubDemandBatches.Count -ne 1 -or [string] $plannedHubDemandBatches[0].executionStateCode -ne 'ParkedCandidate') { throw "PlannedHubDemandWorldInteractionBatchInvalid:$plannedHubDemandWi" }
}
$hubDeferredBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:hub-demand:mandatory-task-failure-response-decision.r1' })
if ($hubDeferredBatch.Count -ne 1 -or [string] $hubDeferredBatch[0].executionStateCode -ne 'PlanningBlocked') { throw 'HubMandatoryTaskFailureDecisionBlockInvalid' }
if ([string] $hubDeferredBatch[0].commonModuleStableId -ne 'module:survival-economy-projection') { throw 'HubDeferredDecisionCommonModuleInvalid' }
$survivalEconomyQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'survival-economy' })) { $survivalEconomyQuestionIds += @($batch.questionIds) }
if (@($survivalEconomyQuestionIds | Sort-Object -Unique).Count -ne 16) { throw 'SurvivalEconomyTopicBatchCoverageInvalid' }
foreach ($plannedEconomyWi in @('WI-SURVIVAL-RATION-POLICY-SET', 'WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM')) {
    $plannedEconomyBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains $plannedEconomyWi })
    if ($plannedEconomyBatches.Count -ne 1 -or [string] $plannedEconomyBatches[0].executionStateCode -ne 'ParkedCandidate') { throw "PlannedSurvivalEconomyWorldInteractionBatchInvalid:$plannedEconomyWi" }
}
$hostedEconomyBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:survival-economy:hosted-economic-authority-decision.r1' })
if ($hostedEconomyBatch.Count -ne 1 -or [string] $hostedEconomyBatch[0].executionStateCode -ne 'PlanningBlocked') { throw 'HostedEconomyAuthorityDecisionBlockInvalid' }
$soloDelegationQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'solo-work-delegation' })) { $soloDelegationQuestionIds += @($batch.questionIds) }
if (@($soloDelegationQuestionIds | Sort-Object -Unique).Count -ne 2) { throw 'SoloWorkDelegationTopicBatchCoverageInvalid' }
$soloExistingBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:solo-work-delegation:existing-direct-experience-and-assignment-rules.r1' })
if ($soloExistingBatch.Count -ne 1 -or [string] $soloExistingBatch[0].executionStateCode -ne 'ImplementedParked') { throw 'SoloExistingDelegationReuseBatchInvalid' }
if (@($soloExistingBatch[0].worldInteractionIds) -notcontains 'WI-WORLD-01') { throw 'SoloExistingDelegationWorldInteractionInvalid' }
$soloExceptionBatch = @($batches | Where-Object { $_.batchStableId -eq 'batch:solo-work-delegation:exception-auto-resolution-boundary.r1' })
if ($soloExceptionBatch.Count -ne 1 -or [string] $soloExceptionBatch[0].executionStateCode -ne 'PlanningBlocked') { throw 'SoloDelegationExceptionDecisionBlockInvalid' }
$farmLandscapeQuestionIds = @()
foreach ($batch in @($batches | Where-Object { $_.topicCode -eq 'farm-landscape-pattern-inventory' })) { $farmLandscapeQuestionIds += @($batch.questionIds) }
if (@($farmLandscapeQuestionIds | Sort-Object -Unique).Count -ne 43) { throw 'FarmLandscapePatternTopicBatchCoverageInvalid' }
$plannedPatternPlacementBatches = @($batches | Where-Object { $null -ne $_.PSObject.Properties['plannedWorldInteractionIds'] -and @($_.plannedWorldInteractionIds) -contains 'WI-WORLD-PATTERN-PLACEMENT-CONFIRM' })
if ($plannedPatternPlacementBatches.Count -ne 1 -or [string] $plannedPatternPlacementBatches[0].executionStateCode -ne 'ParkedCandidate') { throw 'PlannedPatternPlacementWorldInteractionBatchInvalid' }
$farmLandscapeTopic = @($catalog.topics | Where-Object { $_.topicCode -eq 'farm-landscape-pattern-inventory' })
if ($farmLandscapeTopic.Count -ne 1 -or @($farmLandscapeTopic[0].worldInteractionRefs).Count -ne 0) { throw 'FarmLandscapeTopicMustNotBindWorldCancellationInteraction' }

$q001 = $catalog.questionOverrides.'Q-001'
if ([string] $q001.implementationStatusCode -ne 'Partial') { throw 'Q001ImplementationStatusInvalid' }
if ([string] $q001.checks.automatedVerification -ne 'Passed') { throw 'Q001AutomatedVerificationInvalid' }
if ([string] $q001.checks.runtimeVerification -ne 'NotRun') { throw 'Q001RuntimeMustRemainNotRun' }
if (@($q001.blockerCodes) -notcontains 'TemperatureFatigueDiseaseRulesNotImplemented') { throw 'Q001BodyStateGapMissing' }

$q002 = $catalog.questionOverrides.'Q-002'
if ([string] $q002.implementationStatusCode -ne 'Partial') { throw 'Q002ImplementationStatusInvalid' }
if ([string] $q002.checks.automatedVerification -ne 'Passed') { throw 'Q002AutomatedVerificationInvalid' }
if ([string] $q002.checks.runtimeVerification -ne 'NotRun') { throw 'Q002RuntimeMustRemainNotRun' }
if (@($q002.blockerCodes) -notcontains 'SleepPermissionAndInterruptionPolicyUnresolved') { throw 'Q002SleepPolicyGapMissing' }
if (@($q002.blockerCodes) -notcontains 'FireFuelCostUnresolved') { throw 'Q002FuelCostGapMissing' }

$q003 = $catalog.questionOverrides.'Q-003'
if ([string] $q003.implementationStatusCode -ne 'Partial') { throw 'Q003ImplementationStatusInvalid' }
if ([string] $q003.checks.automatedVerification -ne 'Passed') { throw 'Q003AutomatedVerificationInvalid' }
if ([string] $q003.checks.runtimeVerification -ne 'NotRun') { throw 'Q003RuntimeMustRemainNotRun' }
if (@($q003.blockerCodes) -notcontains 'PreviewWarningUiNotWired') { throw 'Q003PreviewUiGapMissing' }
if (@($q003.blockerCodes) -notcontains 'PlayerWarningPreferencePersistenceNotImplemented') { throw 'Q003PreferencePersistenceGapMissing' }

$q004 = $catalog.questionOverrides.'Q-004'
if ([string] $q004.implementationStatusCode -ne 'Partial') { throw 'Q004ImplementationStatusInvalid' }
if ([string] $q004.checks.automatedVerification -ne 'Passed') { throw 'Q004AutomatedVerificationInvalid' }
if ([string] $q004.checks.runtimeVerification -ne 'NotRun') { throw 'Q004RuntimeMustRemainNotRun' }
if (@($q004.blockerCodes) -notcontains 'ThreatSpawnFrequencyValuesNotDefined') { throw 'Q004SpawnFrequencyGapMissing' }
if (@($q004.blockerCodes) -notcontains 'DifficultyProfileSaveReplayBindingNotImplemented') { throw 'Q004SaveReplayGapMissing' }

$q005 = $catalog.questionOverrides.'Q-005'
if ([string] $q005.implementationStatusCode -ne 'Partial') { throw 'Q005ImplementationStatusInvalid' }
if ([string] $q005.checks.automatedVerification -ne 'Passed') { throw 'Q005AutomatedVerificationInvalid' }
if ([string] $q005.checks.runtimeVerification -ne 'NotRun') { throw 'Q005RuntimeMustRemainNotRun' }
if (@($q005.blockerCodes) -notcontains 'ThreatIntensityValuesNotDefined') { throw 'Q005ThreatValuesGapMissing' }
if (@($q005.blockerCodes) -notcontains 'FocusRequirementThresholdNotDefined') { throw 'Q005FocusThresholdGapMissing' }

$q006 = $catalog.questionOverrides.'Q-006'
if ([string] $q006.implementationStatusCode -ne 'Partial') { throw 'Q006ImplementationStatusInvalid' }
if ([string] $q006.checks.automatedVerification -ne 'Passed') { throw 'Q006AutomatedVerificationInvalid' }
if ([string] $q006.checks.runtimeVerification -ne 'NotRun') { throw 'Q006RuntimeMustRemainNotRun' }
if (@($q006.blockerCodes) -notcontains 'MeditationProficiencyAccessCurveNotDefined') { throw 'Q006AccessCurveGapMissing' }
if (@($q006.blockerCodes) -notcontains 'BasicAttackCombatEffectNotApproved') { throw 'Q006CombatEffectGateMissing' }

$q007 = $catalog.questionOverrides.'Q-007'
if ([string] $q007.implementationStatusCode -ne 'Partial') { throw 'Q007ImplementationStatusInvalid' }
if ([string] $q007.checks.automatedVerification -ne 'Passed') { throw 'Q007AutomatedVerificationInvalid' }
if ([string] $q007.checks.runtimeVerification -ne 'NotRun') { throw 'Q007RuntimeMustRemainNotRun' }
if (@($q007.blockerCodes) -notcontains 'CriticalChanceCurveNotDefined') { throw 'Q007CriticalCurveGapMissing' }
if (@($q007.blockerCodes) -notcontains 'CombatEffectRevisionNotApproved') { throw 'Q007CombatEffectGateMissing' }

$q008 = $catalog.questionOverrides.'Q-008'
if ([string] $q008.implementationStatusCode -ne 'Partial') { throw 'Q008ImplementationStatusInvalid' }
if ([string] $q008.checks.automatedVerification -ne 'Passed') { throw 'Q008AutomatedVerificationInvalid' }
if ([string] $q008.checks.runtimeVerification -ne 'NotRun') { throw 'Q008RuntimeMustRemainNotRun' }
if (@($q008.blockerCodes) -notcontains 'ObservationProficiencyThresholdsNotDefined') { throw 'Q008ThresholdGapMissing' }
if (@($q008.blockerCodes) -notcontains 'SocialGrowthHintResolutionOwnedByQ009') { throw 'Q008Q009HandoverMissing' }

$q009 = $catalog.questionOverrides.'Q-009'
if ([string] $q009.implementationStatusCode -ne 'Partial') { throw 'Q009ImplementationStatusInvalid' }
if ([string] $q009.checks.automatedVerification -ne 'Passed') { throw 'Q009AutomatedVerificationInvalid' }
if ([string] $q009.checks.runtimeVerification -ne 'NotRun') { throw 'Q009RuntimeMustRemainNotRun' }
if (@($q009.blockerCodes) -notcontains 'GrowthHintAuthorizationPolicyNotBoundToOnlineIdentity') { throw 'Q009AuthorizationGapMissing' }
if (@($q009.blockerCodes) -notcontains 'PartyBenefitOwnedByQ010') { throw 'Q009Q010HandoverMissing' }

$q010 = $catalog.questionOverrides.'Q-010'
if ([string] $q010.implementationStatusCode -ne 'Partial') { throw 'Q010ImplementationStatusInvalid' }
if ([string] $q010.checks.automatedVerification -ne 'Passed') { throw 'Q010AutomatedVerificationInvalid' }
if ([string] $q010.checks.runtimeVerification -ne 'NotRun') { throw 'Q010RuntimeMustRemainNotRun' }
if (@($q010.blockerCodes) -notcontains 'RuntimePartyDistanceAuthorityNotBound') { throw 'Q010DistanceAuthorityGapMissing' }
if (@($q010.blockerCodes) -notcontains 'RecoveryEffectOutcomeOwnedByQ011') { throw 'Q010Q011HandoverMissing' }

$q011 = $catalog.questionOverrides.'Q-011'
if ([string] $q011.implementationStatusCode -ne 'Partial') { throw 'Q011ImplementationStatusInvalid' }
if ([string] $q011.checks.automatedVerification -ne 'Passed') { throw 'Q011AutomatedVerificationInvalid' }
if ([string] $q011.checks.runtimeVerification -ne 'NotRun') { throw 'Q011RuntimeMustRemainNotRun' }
if (@($q011.blockerCodes) -notcontains 'NatureMindImpactApplicationNotImplemented') { throw 'Q011NatureMindApplicationGapMissing' }
if (@($q011.blockerCodes) -notcontains 'ResonancePersistenceOwnedByQ012') { throw 'Q011Q012HandoverMissing' }

$q012 = $catalog.questionOverrides.'Q-012'
if ([string] $q012.implementationStatusCode -ne 'Partial') { throw 'Q012ImplementationStatusInvalid' }
if ([string] $q012.checks.automatedVerification -ne 'Passed') { throw 'Q012AutomatedVerificationInvalid' }
if ([string] $q012.checks.runtimeVerification -ne 'NotRun') { throw 'Q012RuntimeMustRemainNotRun' }
if (@($q012.blockerCodes) -notcontains 'AfterglowSaveReplayBindingNotImplemented') { throw 'Q012SaveReplayGapMissing' }
if (@($q012.blockerCodes) -notcontains 'ResonanceStackingOwnedByQ013') { throw 'Q012Q013HandoverMissing' }

$q013 = $catalog.questionOverrides.'Q-013'
if ([string] $q013.implementationStatusCode -ne 'Partial') { throw 'Q013ImplementationStatusInvalid' }
if ([string] $q013.checks.automatedVerification -ne 'Passed') { throw 'Q013AutomatedVerificationInvalid' }
if ([string] $q013.checks.runtimeVerification -ne 'NotRun') { throw 'Q013RuntimeMustRemainNotRun' }
if (@($q013.blockerCodes) -notcontains 'MaximumContributorCountNotDefined') { throw 'Q013ContributorCapGapMissing' }
if (@($q013.blockerCodes) -notcontains 'GwangbokEntryCapOwnedByQ014') { throw 'Q013Q014HandoverMissing' }

$q014 = $catalog.questionOverrides.'Q-014'
if ([string] $q014.implementationStatusCode -ne 'Partial') { throw 'Q014ImplementationStatusInvalid' }
if ([string] $q014.checks.automatedVerification -ne 'Passed') { throw 'Q014AutomatedVerificationInvalid' }
if ([string] $q014.checks.runtimeVerification -ne 'NotRun') { throw 'Q014RuntimeMustRemainNotRun' }
if (@($q014.blockerCodes) -notcontains 'PeriodTransitionApplicationNotImplemented') { throw 'Q014PeriodTransitionGapMissing' }
if (@($q014.blockerCodes) -notcontains 'EligibleSelfRecoveryActionOwnedByQ015') { throw 'Q014Q015HandoverMissing' }

$q015 = $catalog.questionOverrides.'Q-015'
if ([string] $q015.implementationStatusCode -ne 'Partial') { throw 'Q015ImplementationStatusInvalid' }
if ([string] $q015.checks.automatedVerification -ne 'Passed') { throw 'Q015AutomatedVerificationInvalid' }
if ([string] $q015.checks.runtimeVerification -ne 'NotRun') { throw 'Q015RuntimeMustRemainNotRun' }
if (@($q015.blockerCodes) -notcontains 'ReflectRecoveryActionRecordNotImplemented') { throw 'Q015ReflectActionRecordGapMissing' }
if (@($q015.blockerCodes) -notcontains 'ResonanceMaintenanceOwnedByQ016') { throw 'Q015Q016HandoverMissing' }

$q016 = $catalog.questionOverrides.'Q-016'
if ([string] $q016.implementationStatusCode -ne 'Partial') { throw 'Q016ImplementationStatusInvalid' }
if ([string] $q016.checks.automatedVerification -ne 'Passed') { throw 'Q016AutomatedVerificationInvalid' }
if ([string] $q016.checks.runtimeVerification -ne 'NotRun') { throw 'Q016RuntimeMustRemainNotRun' }
if (@($q016.blockerCodes) -notcontains 'SelfRecoveryRefreshIntervalNotDefined') { throw 'Q016RefreshIntervalGapMissing' }
if (@($q016.blockerCodes) -notcontains 'RecoveryDecayProfileOwnedByQ017') { throw 'Q016Q017HandoverMissing' }

$q017 = $catalog.questionOverrides.'Q-017'
if ([string] $q017.implementationStatusCode -ne 'Partial') { throw 'Q017ImplementationStatusInvalid' }
if ([string] $q017.checks.automatedVerification -ne 'Passed') { throw 'Q017AutomatedVerificationInvalid' }
if ([string] $q017.checks.runtimeVerification -ne 'NotRun') { throw 'Q017RuntimeMustRemainNotRun' }
if (@($q017.blockerCodes) -notcontains 'RecoveryDecayNumericCoefficientsNotDefined') { throw 'Q017DecayCoefficientGapMissing' }
if (@($q017.blockerCodes) -notcontains 'OfflineRecoveryDecayOwnedByQ018') { throw 'Q017Q018HandoverMissing' }

$q018 = $catalog.questionOverrides.'Q-018'
if ([string] $q018.implementationStatusCode -ne 'Partial') { throw 'Q018ImplementationStatusInvalid' }
if ([string] $q018.checks.automatedVerification -ne 'Passed') { throw 'Q018AutomatedVerificationInvalid' }
if ([string] $q018.checks.runtimeVerification -ne 'NotRun') { throw 'Q018RuntimeMustRemainNotRun' }
if (@($q018.blockerCodes) -notcontains 'RecoverySaveReferenceTickBindingNotImplemented') { throw 'Q018SaveReferenceTickGapMissing' }
if (@($q018.blockerCodes) -notcontains 'RecoveryThreatOffsetOwnedByQ019') { throw 'Q018Q019HandoverMissing' }

$q019 = $catalog.questionOverrides.'Q-019'
if ([string] $q019.implementationStatusCode -ne 'Partial') { throw 'Q019ImplementationStatusInvalid' }
if ([string] $q019.checks.automatedVerification -ne 'Passed') { throw 'Q019AutomatedVerificationInvalid' }
if ([string] $q019.checks.runtimeVerification -ne 'NotRun') { throw 'Q019RuntimeMustRemainNotRun' }
if (@($q019.blockerCodes) -notcontains 'RecoveryThreatOffsetRatioNotDefined') { throw 'Q019OffsetRatioGapMissing' }
if (@($q019.blockerCodes) -notcontains 'GwangbokDarkAgeConflictOwnedByQ020') { throw 'Q019Q020HandoverMissing' }

$q020 = $catalog.questionOverrides.'Q-020'
if ([string] $q020.implementationStatusCode -ne 'Partial') { throw 'Q020ImplementationStatusInvalid' }
if ([string] $q020.checks.automatedVerification -ne 'Passed') { throw 'Q020AutomatedVerificationInvalid' }
if ([string] $q020.checks.runtimeVerification -ne 'NotRun') { throw 'Q020RuntimeMustRemainNotRun' }
if (@($q020.blockerCodes) -notcontains 'ExtremeMeditationProficiencyThresholdNotDefined') { throw 'Q020ProficiencyThresholdGapMissing' }
if (@($q020.blockerCodes) -notcontains 'AllowedMindfulnessEffectScopeOwnedByQ021') { throw 'Q020Q021HandoverMissing' }

$q021 = $catalog.questionOverrides.'Q-021'
if ([string] $q021.implementationStatusCode -ne 'Partial') { throw 'Q021ImplementationStatusInvalid' }
if ([string] $q021.checks.automatedVerification -ne 'Passed') { throw 'Q021AutomatedVerificationInvalid' }
if ([string] $q021.checks.runtimeVerification -ne 'NotRun') { throw 'Q021RuntimeMustRemainNotRun' }
if (@($q021.blockerCodes) -notcontains 'PersonalEffectConsumersNotBoundToRuntime') { throw 'Q021RuntimeConsumerGapMissing' }
if (@($q021.blockerCodes) -notcontains 'PersonalMindfulnessEffectStrengthOwnedByQ022') { throw 'Q021Q022HandoverMissing' }

$q022 = $catalog.questionOverrides.'Q-022'
if ([string] $q022.implementationStatusCode -ne 'Partial') { throw 'Q022ImplementationStatusInvalid' }
if ([string] $q022.checks.automatedVerification -ne 'Passed') { throw 'Q022AutomatedVerificationInvalid' }
if ([string] $q022.checks.runtimeVerification -ne 'NotRun') { throw 'Q022RuntimeMustRemainNotRun' }
if (@($q022.blockerCodes) -notcontains 'LongTermMeditationStrengthCurveNotDefined') { throw 'Q022StrengthCurveGapMissing' }
if (@($q022.blockerCodes) -notcontains 'MindfulnessEffectDurationAndCostNotDefined') { throw 'Q022DurationCostGapMissing' }

$q023 = $catalog.questionOverrides.'Q-023'
if ([string] $q023.implementationStatusCode -ne 'Partial') { throw 'Q023ImplementationStatusInvalid' }
if ([string] $q023.checks.automatedVerification -ne 'Passed') { throw 'Q023AutomatedVerificationInvalid' }
if ([string] $q023.checks.runtimeVerification -ne 'NotRun') { throw 'Q023RuntimeMustRemainNotRun' }
if (@($q023.blockerCodes) -notcontains 'RiskySleepWakeOutcomeNumericRulesNotDefined') { throw 'Q023WakeOutcomeRulesGapMissing' }
if (@($q023.blockerCodes) -notcontains 'WeatherProfileBindingOwnedByQ024') { throw 'Q023Q024HandoverMissing' }

$q024 = $catalog.questionOverrides.'Q-024'
if ([string] $q024.implementationStatusCode -ne 'Partial') { throw 'Q024ImplementationStatusInvalid' }
if ([string] $q024.checks.automatedVerification -ne 'Passed') { throw 'Q024AutomatedVerificationInvalid' }
if ([string] $q024.checks.runtimeVerification -ne 'NotRun') { throw 'Q024RuntimeMustRemainNotRun' }
if (@($q024.blockerCodes) -notcontains 'LiveKmaObservationApprovalNotVerified') { throw 'Q024LiveKmaEvidenceGapMissing' }
if (@($q024.blockerCodes) -notcontains 'WeatherProfileSaveReplayBindingNotImplemented') { throw 'Q024SaveReplayGapMissing' }

$q025 = $catalog.questionOverrides.'Q-025'
if ([string] $q025.implementationStatusCode -ne 'Partial') { throw 'Q025ImplementationStatusInvalid' }
if ([string] $q025.checks.automatedVerification -ne 'Passed') { throw 'Q025AutomatedVerificationInvalid' }
if ([string] $q025.checks.runtimeVerification -ne 'NotRun') { throw 'Q025RuntimeMustRemainNotRun' }
if (@($q025.blockerCodes) -notcontains 'NotActivePlayableUnitDependency') { throw 'Q025ActiveDependencyGuardMissing' }
if (@($q025.blockerCodes) -notcontains 'ActualHPlacementGraphEvidenceNotBound') { throw 'Q025HPlacementGraphGapMissing' }

foreach ($questionId in @('Q-272', 'Q-273', 'Q-274')) {
    $item = $catalog.questionOverrides.$questionId
    if ([string] $item.decisionStatusCode -ne 'NeedsSourceRecovery') { throw "SourceRecoveryDecisionMissing:$questionId" }
    if ([string] $item.checks.planningRecord -ne 'NeedsSourceRecovery') { throw "SourceRecoveryCheckMissing:$questionId" }
}

$farmTopic = @($catalog.topics | Where-Object { $_.topicCode -eq 'farm-landscape-pattern-inventory' })
if ($farmTopic.Count -ne 1) { throw 'FarmLandscapePatternTopicMissingOrDuplicated' }
if ([string] $farmTopic[0].questionSelector -ne '297-339') { throw 'FarmLandscapePatternSelectorInvalid' }
if ([string] $farmTopic[0].checkDefaults.runtimeVerification -ne 'NotRun') { throw 'FarmRuntimeStatusMustRemainNotRun' }

$q338 = $catalog.questionOverrides.'Q-338'
if ([string] $q338.implementationStatusCode -ne 'Implemented') { throw 'Q338ImplementationStatusInvalid' }
if ([string] $q338.checks.implementation -ne 'Implemented') { throw 'Q338ImplementationCheckInvalid' }
if ([string] $q338.checks.automatedVerification -ne 'Passed') { throw 'Q338AutomatedVerificationInvalid' }
if ([string] $q338.checks.runtimeVerification -ne 'NotRun') { throw 'Q338RuntimeMustRemainNotRun' }

$generated = Get-Content -LiteralPath $generatedPath -Raw -Encoding UTF8
foreach ($expected in @('Q-001~Q-339', 'ReadyToDispatch: `0`', 'NotReady: `339`', 'question number role: `TraceabilityAndExtractionTraversal`', 'question traversal: `Q001ToQ339` for `WorldInteractionExtraction`', 'question coverage: `339 / 339`', 'extraction source: `SmallBatchRefinement=268; TopicSeed=71`', 'normalized WI candidates: `64`', '| `WI-ACTOR-03` | `Registered` |', '| `WI-ACTOR-PLAN-SET` | `Registered` |', '| `WI-CON-BLUEPRINT-PLACE` | `Registered` |', '| `WI-WORLD-RESOURCE-REGENERATE` | `Registered` |', '| `WI-HEAT-SOURCE-STATE-CHANGE` | `Registered` |', '| `WI-FARM-LAND-IMPROVE` | `MetadataFamily` |', '| `WI-FARM-SOIL-AMEND` | `Registered` |', '| `WI-FARM-FIELD-BOUNDARY-CONFIRM` | `Registered` |', '| `WI-FARM-WATER-TRANSFER` | `Registered` |', '| `WI-TOWN-STOCK-REPLENISH` | `Registered` |', '| `WI-TOWN-DELIVERY-RECEIVE` | `Registered` |', '| `WI-TOWN-DELIVERY-INSPECT` | `Registered` |', '| `WI-TOWN-STOCK-PUTAWAY` | `Registered` |', '| `WI-TOWN-SUPPLY-DISPATCH` | `Registered` |', '| `WI-NATURE-TRACE-INVESTIGATE` | `Registered` |', '| `WI-NATURE-THREAT-CORE-CLEAR` | `MetadataFamily` |', '| `WI-COMBAT-TACTICAL-COMMAND` | `Registered` |', '| `WI-COMBAT-DIRECT-CONTROL-SET` | `Registered` |', '| `WI-COMBAT-CASUALTY-RESPONSE` | `MetadataFamily` |', '| `WI-THREAT-NONCOMBAT-RESOLVE` | `MetadataFamily` |', '| `WI-COMMUNITY-MEMBERSHIP-CONFIRM` | `Registered` |', '| `WI-COMMUNITY-REMOTE-RESPONSE` | `Registered` |', '| `WI-COMMUNITY-ENTRANCE-POLICY-SET` | `Registered` |', '| `WI-FARM-DEFENSE-MOBILIZE` | `Registered` |', '| `WI-SQUAD-ASSIGN` | `PlannedCandidate` |', '| `WI-SQUAD-SUPPLY` | `PlannedCandidate` |', '| `WI-FARM-DEFENSE-RESOLVE` | `PlannedCandidate` |', '| `WI-FARM-DEFENSE-RETURN` | `PlannedCandidate` |', 'selection: `ActivePlayableUnitDependency`', 'impact review: `E7ToE1`', 'initial assembly: `E1ToE7`', 'implementation cycle: `EarliestAffectedStageReopen`', 'logic / presentation cycle: `Bidirectional`', 'dispatch state: `WaitingForApprovedRevision`', 'current work batch: `batch:farm-barracks:defense-mobilization-and-production-cost.r1`', 'active implementation batch WIP: `0 / 1`', 'active topic batch coverage: `17 / 17`', 'fully partitioned topic coverage: `herbal-recipe-crafting=47/47; nature-shelter-sleep=23/23; player-mind-meditation=32/32; nature-resource-construction=15/15; save-load-runtime=7/7; farm-building-spatial-placement=56/56; town-order-pickup=11/11; region-five-elements-monster=35/35; community-membership-visitor=21/21; building-placement-assistance=3/3; farm-barracks-defense=17/17`', 'Q-131, Q-133', 'planned:WI-CRAFT-BREW', 'planned:WI-HEAT-SOURCE-STATE-CHANGE', 'batch:player-mind:completion-recovery-wi-family.r1', 'batch:nature-resource-construction:construction-sub-wi-contracts.r1', 'batch:save-load-runtime:safe-reentry-lh-gate.r1', 'batch:farm-spatial:terrain-improvement-checkpoints.r1', 'batch:farm-spatial:irrigation-weather-and-route.r1', 'batch:town-order:store-stock-replenishment.r1', 'batch:town-order:corridor-safety-maintenance.r1', 'batch:region-threat:tactical-view-command-and-direct-control.r1', 'batch:region-threat:noncombat-resolution-and-world-return.r1', 'batch:community-visitor:membership-confirmation.r1', 'batch:community-visitor:remote-response-and-event-review.r1', 'batch:building-placement-assistance:complete-candidate-generation.r1', 'batch:farm-barracks:defense-mobilization-and-production-cost.r1', 'batch:farm-barracks:return-loot-treatment-and-production.r1', '`module:placement-validation`', '`module:runtime-reentry`', 'batch:nature-basic-herbal-recovery:source-recovery-decisions.r1', 'PlanningBlocked', 'batch:nature-shelter-sleep:risky-sleep-weather-spatial.r1', 'ImplementedParked', '| `Q-272` |', 'NeedsSourceRecovery', '| `Q-339` |', 'Runtime:NotRun', 'Evidence:Unbound')) {
    if ($expected -eq 'active implementation batch WIP: `0 / 1`') { $expected = 'active implementation batches: `' + @($batches | Where-Object executionStateCode -eq Active).Count + '` (고정 WIP 상한 없음)' }
    if ($expected -eq 'extraction source: `SmallBatchRefinement=268; TopicSeed=71`') { $expected = 'extraction source: `SmallBatchRefinement=339`' }
    if ($expected -eq 'normalized WI candidates: `64`') { $expected = 'normalized WI candidates: `70`' }
    if ($expected -eq 'active topic batch coverage: `17 / 17`') { $topicCount = @($batches | Where-Object topicCode -eq $catalog.executionRouting.activeTopicCode | ForEach-Object questionIds | Sort-Object -Unique).Count; $expected = 'active topic batch coverage: `' + $topicCount + ' / ' + $topicCount + '`' }
    if ($expected -eq 'fully partitioned topic coverage: `herbal-recipe-crafting=47/47; nature-shelter-sleep=23/23; player-mind-meditation=32/32; nature-resource-construction=15/15; save-load-runtime=7/7; farm-building-spatial-placement=56/56; town-order-pickup=11/11; region-five-elements-monster=35/35; community-membership-visitor=21/21; building-placement-assistance=3/3; farm-barracks-defense=17/17`') { $expected = 'fully partitioned topic coverage: `herbal-recipe-crafting=47/47; nature-shelter-sleep=23/23; player-mind-meditation=32/32; nature-resource-construction=15/15; save-load-runtime=7/7; farm-building-spatial-placement=56/56; town-order-pickup=11/11; region-five-elements-monster=35/35; community-membership-visitor=21/21; building-placement-assistance=3/3; farm-barracks-defense=17/17; hub-demand-allocation=11/11; survival-economy=16/16; solo-work-delegation=2/2; farm-landscape-pattern-inventory=43/43`' }
    if ($expected -eq '| `WI-SQUAD-ASSIGN` | `PlannedCandidate` |') { $expected = '| `WI-SQUAD-ASSIGN` | `Registered` |' }
    if ($expected -eq '| `WI-SQUAD-SUPPLY` | `PlannedCandidate` |') { $expected = '| `WI-SQUAD-SUPPLY` | `Registered` |' }
    if ($expected -eq '| `WI-FARM-DEFENSE-RESOLVE` | `PlannedCandidate` |') { $expected = '| `WI-FARM-DEFENSE-RESOLVE` | `Registered` |' }
    if ($expected -eq '| `WI-FARM-DEFENSE-RETURN` | `PlannedCandidate` |') { $expected = '| `WI-FARM-DEFENSE-RETURN` | `Registered` |' }
    if ($expected -eq 'current work batch: `batch:farm-barracks:defense-mobilization-and-production-cost.r1`') { $expected = 'current work batch: `' + $catalog.currentWorkBatchStableId + '`' }
    if (-not $generated.Contains($expected)) { throw "GeneratedChecklistValueMissing:$expected" }
}
if (-not $generated.Contains('| `WI-COMMUNITY-VISITOR-STAY` | `Registered` |')) { throw 'GeneratedRegisteredVisitorStayWorldInteractionMissing' }
if (-not $generated.Contains('| `WI-COMMUNITY-SUPPORT-MISSION-JOIN` | `Registered` |')) { throw 'GeneratedHostedSupportMissionWorldInteractionMissing' }
foreach ($plannedHubDemandWi in @('WI-HUB-DEMAND-ALLOCATE', 'WI-HUB-DEMAND-REMAINDER-RETURN', 'WI-HUB-SUPPLY-TASK-ACCEPT')) {
    if (-not $generated.Contains("| ``$plannedHubDemandWi`` | ``$(if ($plannedHubDemandWi -eq 'WI-HUB-DEMAND-REMAINDER-RETURN') { 'ResultProjection' } else { 'Registered' })`` |")) { throw "GeneratedHubDemandWorldInteractionMissing:$plannedHubDemandWi" }
}
foreach ($plannedEconomyWi in @('WI-SURVIVAL-RATION-POLICY-SET', 'WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM')) {
    if (-not $generated.Contains("| ``$plannedEconomyWi`` | ``Registered`` |")) { throw "GeneratedSurvivalEconomyWorldInteractionMissing:$plannedEconomyWi" }
}
if (-not $generated.Contains('| `WI-WORLD-PATTERN-PLACEMENT-CONFIRM` | `ReuseProfile` |')) { throw 'GeneratedPatternPlacementWorldInteractionMissing' }
if (-not $generated.Contains('| `WI-WORLD-03` | `Registered` | 2 | Q-093, Q-094 |')) { throw 'GeneratedWorldCancellationInteractionQuestionsInvalid' }
foreach ($selectionExpected in @(
    'decision: `CurrentBatchWaitingForApprovedRevision`',
    'current / earliest resume E: `E1` / `E2`',
    'wait or next action: `ApprovePresentationRevisionBeforeE2OrSelectNextLogicE3Batch`'
)) {
    if (-not $generated.Contains($selectionExpected)) { throw "GeneratedExecutionSelectionMissing:$selectionExpected" }
}

$queueSectionStart = $generated.IndexOf("## $($catalog.output.worldInteractionEvidenceCycleQueueTitleKo)")
$queueSectionEnd = $generated.IndexOf("## $($catalog.output.executionRoutingTitleKo)")
if ($queueSectionStart -lt 0 -or $queueSectionEnd -le $queueSectionStart) { throw 'GeneratedWorldInteractionEvidenceCycleQueueSectionMissing' }
$queueSection = $generated.Substring($queueSectionStart, $queueSectionEnd - $queueSectionStart)
$queueRows = @($queueSection -split "`n" | Where-Object { $_ -match '^\| `WI-[^`]+` \|' })
if ($queueRows.Count -ne 62) { throw "GeneratedWorldInteractionEvidenceCycleQueueCountInvalid:$($queueRows.Count)" }
if ($queueRows[0] -notmatch ('^\| `' + [regex]::Escape($catalog.executionRouting.activeWorldInteractionId) + '` ')) { throw 'GeneratedDisplayFocusWorldInteractionEvidenceCycleQueueEntryInvalid' }
if ($queueRows[0] -notmatch '`Bound`<br>`E1,E2,E3` / 4 files') { throw 'GeneratedActiveWorldInteractionCSharpEvidenceBindingInvalid' }
if (-not $queueSection.Contains('selection basis: `ApprovedWorkItemDependencies -> ApprovedPlanningGate -> RegisteredWorldInteraction -> EarliestReopenEvidenceStage`')) { throw 'GeneratedWorldInteractionEvidenceCycleQueueSelectionBasisMissing' }
if (-not $queueSection.Contains('deterministic tie-break: `WorldInteractionStableIdForDeterministicOutputOnly`')) { throw 'GeneratedWorldInteractionEvidenceCycleQueueTieBreakMissing' }
$boundCount = @($queueRows | Where-Object { $_ -match '`Bound`' }).Count
$unboundCount = @($queueRows | Where-Object { $_ -match '`RegisteredWithoutCSharpEvidenceBinding`' }).Count
if ($boundCount + $unboundCount -ne $queueRows.Count -or -not $queueSection.Contains("C# E responsibility bindings: ``Bound=$boundCount; RegisteredWithoutCSharpEvidenceBinding=$unboundCount``")) { throw 'GeneratedWorldInteractionCSharpEvidenceBindingCountsInvalid' }
$resourceRow = @($queueRows | Where-Object { $_ -match '^\| `WI-WORLD-RESOURCE-REGENERATE` ' })
if ($resourceRow.Count -ne 1 -or $resourceRow[0] -notmatch '`Bound`<br>`E1,E2,E3`') { throw 'ResourceRegenerationEvidenceMapBindingMissing' }
if ($queueSection -match '(?i)question(priority|order)|Q-001.*priority') { throw 'GeneratedWorldInteractionEvidenceCycleQueueMustNotUseQuestionPriority' }
if ($generated.Contains('ActiveWorldInteractionWipOwnedBy') -or $generated.Contains('ParkedByActiveWip')) { throw 'GlobalWipBlockerMustNotRemain' }
if ($queueSection.Contains('WorldInteractionRegistrationRequired')) { throw 'ResolvedRegistrationBlockerMustNotRemain' }
$farmInteractionRow = @($queueRows | Where-Object { $_ -match '^\| `WI-FARM-01` ' })
# Farm E2 결속은 필수지만 자연회복 등의 E1/E3 근거가 추가될 수 있다.
# 코드 근거 목록을 현재 WI 성숙도 또는 E2 하나로 고정하지 않는다.
if ($farmInteractionRow.Count -ne 1 -or $farmInteractionRow[0] -notmatch '`Bound`<br>`(?:E[1-7],)*E2(?:,E[1-7])*` / [1-9][0-9]* files' -or $farmInteractionRow[0] -match 'CSharpEvidenceResponsibilityBindingRequired') { throw 'GeneratedFarmWorldInteractionCSharpBindingInvalid' }
# 결속된 Farm을 미결속으로 가정하지 않고, 미결속 행 전체의 차단을 검증한다.
if ($unboundCount -eq 0) { throw 'UnboundWorldInteractionGuardFixtureRequired' }
foreach ($unboundRow in @($queueRows | Where-Object { $_ -match '`RegisteredWithoutCSharpEvidenceBinding`' })) {
    if ($unboundRow -notmatch 'CSharpEvidenceResponsibilityBindingRequired' -or $unboundRow -match '`ApprovedWorkItemExecutable`') { throw 'GeneratedRegisteredWorldInteractionMissingCSharpBindingGuardInvalid' }
}

Write-Output 'PlayableLoopInquiryImplementationScopeTestsPassed:Questions=339;Q001=Partial;Q002=Partial;Q003=Partial;Q004=Partial;Q005=Partial;Q006=Partial;Q007=Partial;Q008=Partial;Q009=Partial;Q010=Partial;Q011=Partial;Q012=Partial;Q013=Partial;Q014=Partial;Q015=Partial;Q016=Partial;Q017=Partial;Q018=Partial;Q019=Partial;Q020=Partial;Q021=Partial;Q022=Partial;Q023=Partial;Q024=Partial;Q025=PartialParked;SourceRecovery=3;FarmPattern=43;Q338=Implemented'
