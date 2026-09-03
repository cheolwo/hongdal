$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-playable-loop-planning-e1-index.ps1'
$artifactRoot = Join-Path $repositoryRoot 'artifacts/local/validation/playable-loop-planning-e1-index'
New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null

$jsonRef = 'artifacts/local/validation/playable-loop-planning-e1-index/index.json'
$markdownRef = 'artifacts/local/validation/playable-loop-planning-e1-index/index.md'
$first = & $manager -Mode Write -OutputJsonPath $jsonRef -OutputMarkdownPath $markdownRef
$jsonPath = Join-Path $repositoryRoot $jsonRef
$markdownPath = Join-Path $repositoryRoot $markdownRef
$firstJsonHash = (Get-FileHash $jsonPath -Algorithm SHA256).Hash
$firstMarkdownHash = (Get-FileHash $markdownPath -Algorithm SHA256).Hash
$firstJsonTicks = (Get-Item $jsonPath).LastWriteTimeUtc.Ticks
$firstMarkdownTicks = (Get-Item $markdownPath).LastWriteTimeUtc.Ticks
$second = & $manager -Mode Write -OutputJsonPath $jsonRef -OutputMarkdownPath $markdownRef
if ((Get-FileHash $jsonPath -Algorithm SHA256).Hash -ne $firstJsonHash -or (Get-Item $jsonPath).LastWriteTimeUtc.Ticks -ne $firstJsonTicks) { throw 'PlanningE1JsonNotDeterministic' }
if ((Get-FileHash $markdownPath -Algorithm SHA256).Hash -ne $firstMarkdownHash -or (Get-Item $markdownPath).LastWriteTimeUtc.Ticks -ne $firstMarkdownTicks) { throw 'PlanningE1MarkdownNotDeterministic' }

$result = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([int] $result.counts.plans -ne 47) { throw 'PlanningE1PlanCountInvalid' }
if ([int] $result.counts.playableUnits -ne 20) { throw 'PlanningE1PlayableUnitCountInvalid' }
if (@($result.plans | Where-Object classificationCode -eq 'E1CandidateNeeded').Count -eq 0) { throw 'PlanningE1GapFixtureMissing' }
if (@($result.playableUnitE1Contracts | Where-Object e1State -eq 'Established').Count -eq 0) { throw 'PlanningE1EstablishedFixtureMissing' }
if (-not [bool] $result.policy.planningDocumentAloneIsNotE1) { throw 'PlanningDocumentBoundaryMissing' }
if ([string] $result.atomicClosurePolicy.defaultModeCode -ne 'OnePrimaryAtomicClosureTarget') { throw 'AtomicClosureModeMissing' }
if (-not [bool] $result.atomicClosurePolicy.moduleDoesNotAutomaticallyCreatePlayableUnit) { throw 'AtomicClosurePlayableUnitBoundaryMissing' }
if ((@($result.atomicClosurePolicy.closureOrderCodes) -join ',') -ne 'E1,E2,E3,E4,E5,E6,E7') { throw 'AtomicClosureOrderMissing' }
if (-not [bool] $result.atomicClosurePolicy.doesNotImposeGlobalWorkInProgressLimit) { throw 'AtomicClosureGlobalWipBoundaryMissing' }
if ([int] $result.counts.atomicModules -ne 11) { throw 'PlanningAtomicModuleCountInvalid' }
if ([int] $result.counts.atomicClosureCohorts -ne 1) { throw 'PlanningAtomicClosureCohortCountInvalid' }
if ([int] $result.counts.e1Assemblies -ne 2) { throw 'PlanningE1AssemblyCountInvalid' }
if ([int] $result.counts.reviewedAtomicPlans -ne 2) { throw 'PlanningAtomicReviewedPlanCountInvalid' }
if ([int] $result.counts.outlinedAtomicPlans -ne 34) { throw 'PlanningAtomicOutlinePlanCountInvalid' }
if ([int] $result.counts.atomicOutlineCandidates -ne 182) { throw 'PlanningAtomicOutlineCandidateCountInvalid' }
if ([int] $result.counts.decompositionNeeded -ne 0) { throw 'PlanningAtomicDecompositionQueueNotCovered' }
if ([int] $result.counts.atomicReviewNeeded -ne 0) { throw 'PlanningAtomicReviewQueueNotOutlined' }
$hans = @($result.plans | Where-Object planId -eq 'PLAN-STORY-FIRST-FARM-DISCOVERY-001')
if ($hans.Count -ne 1 -or [string] $hans[0].decompositionStateCode -ne 'ReviewedAtomicModules') { throw 'HansAtomicReviewMissing' }
$woodcutting = @($result.atomicModules | Where-Object moduleId -eq 'play-transaction:hans-farm.voluntary-woodcutting.v1')
if ($woodcutting.Count -ne 1 -or [string] $woodcutting[0].primaryWorldInteractionId -ne 'WI-NATURE-06') { throw 'HansWoodcuttingReuseMissing' }
$tilling = @($result.atomicModules | Where-Object moduleId -eq 'play-transaction:hans-farm.till-one-plot.v1')
if ($tilling.Count -ne 1 -or [string] $tilling[0].primaryWorldInteractionId -ne 'WI-FARM-01') { throw 'HansFarmTillingReuseMissing' }
if ([string] $result.activePrimaryClosureTarget.moduleId -ne 'play-transaction:hans-farm.till-one-plot.v1' -or [string] $result.activePrimaryClosureTarget.developmentHandoffStageCode -ne 'E5') { throw 'ActiveAtomicClosureTargetInvalid' }
$farmCohort = @($result.atomicClosureCohorts | Where-Object cohortId -eq 'atomic-closure-cohort:farm-life-five-elements.v1')
if ($farmCohort.Count -ne 1) { throw 'FarmFiveElementClosureCohortMissing' }
if ((@($farmCohort[0].memberWorldInteractionIds) -join ',') -ne 'WI-FARM-01,WI-FARM-02,WI-FARM-03,WI-FARM-04,WI-FARM-05,WI-FARM-06') { throw 'FarmFiveElementClosureMembersInvalid' }
if ([string] $farmCohort[0].sharedPreparationThroughStageCode -ne 'E4' -or (@($farmCohort[0].coordinatedPromotionStageCodes) -join ',') -ne 'E5,E6,E7') { throw 'FarmFiveElementClosureStagesInvalid' }
if ([string] $farmCohort[0].memberEvidenceRuleCode -ne 'IndependentPerWorldInteraction' -or [string] $farmCohort[0].cohortEvidenceStageRuleCode -ne 'MinimumOfMembers') { throw 'FarmFiveElementClosureEvidenceRuleInvalid' }
if (-not (@($farmCohort[0].e5EntryRequirementCodes) -contains 'FrozenApplicableFiveElementClassifications') -or -not (@($farmCohort[0].e5EntryRequirementCodes) -contains 'FrozenApplicableShengKeRelations')) { throw 'FarmFiveElementE5EntryGateInvalid' }
if ((@($farmCohort[0].completeGrowthActivationContract.requiredConditionCodes) -join ',') -ne 'SeedSoilCompatibility,WaterWithinSoilCapacity,RequiredCropCareCompleted') { throw 'FarmCompleteGrowthConditionsInvalid' }
if ([string] $farmCohort[0].completeGrowthActivationContract.outcomeRelationDisplayName -ne '목생화' -or -not [bool] $farmCohort[0].completeGrowthActivationContract.allRequiredConditionsMustPass) { throw 'FarmCompleteGrowthOutcomeInvalid' }
if ([string] $farmCohort[0].farmFieldManagementH2Contract.h2StableIdRef -ne 'h2-candidate:farm-irrigation-service') { throw 'FarmFieldManagementH2ReferenceInvalid' }
if ((@($farmCohort[0].farmFieldManagementH2Contract.managedWorldInteractionIds) -join ',') -ne 'WI-FARM-01,WI-FARM-02,WI-FARM-03,WI-FARM-04') { throw 'FarmFieldManagementH2WorldInteractionsInvalid' }
if ((@($farmCohort[0].farmFieldManagementH2Contract.relationDisplayNames) -join ',') -ne '수생목,토극수,금극목,목생화') { throw 'FarmFieldManagementH2RelationsInvalid' }
if ((@($farmCohort[0].farmFieldManagementH2Contract.waterSourceBoundary.allowedLocationCodes) -join ',') -ne 'InternalH1,ExternalConnectedSource') { throw 'FarmFieldManagementH2WaterLocationsInvalid' }
if ([string] $farmCohort[0].farmFieldManagementH2Contract.waterSourceBoundary.requiredConnectorRoleCode -ne 'WaterServiceRoute' -or -not [bool] $farmCohort[0].farmFieldManagementH2Contract.waterSourceBoundary.currentRevisionAvailabilityRequired) { throw 'FarmFieldManagementH2WaterConnectionInvalid' }
if ([string] $farmCohort[0].farmFieldManagementH2Contract.waterSourceBoundary.missingConnectionEffectCode -ne 'CultivationCycleBlocked' -or -not [bool] $farmCohort[0].farmFieldManagementH2Contract.waterSourceBoundary.missingConnectionDoesNotDeleteH2) { throw 'FarmFieldManagementH2WaterFailureBoundaryInvalid' }
if (-not [bool] $farmCohort[0].farmFieldManagementH2Contract.doesNotEstablishWorldPlacement -or -not [bool] $farmCohort[0].farmFieldManagementH2Contract.doesNotPromoteEvidence) { throw 'FarmFieldManagementH2BoundaryInvalid' }
if ([string] $farmCohort[0].farmHarvestLogisticsContinuityContract.handoffStateCode -ne 'FieldEdgeHarvestLotCreated' -or -not [bool] $farmCohort[0].farmHarvestLogisticsContinuityContract.singleAuthorityHandoffEventRequired) { throw 'FarmHarvestLogisticsHandoffInvalid' }
if ((@($farmCohort[0].farmHarvestLogisticsContinuityContract.dualContextRoles | ForEach-Object { "$($_.contextCode):$($_.elementCode)" }) -join ',') -ne 'CropCycle:METAL,LogisticsCycle:WOOD') { throw 'FarmHarvestLogisticsDualContextInvalid' }
if ((@($farmCohort[0].farmHarvestLogisticsContinuityContract.logisticsStageBindings | ForEach-Object { "$($_.stageCode):$($_.elementCode)" }) -join ',') -ne 'ReceivingWorkIngress:WOOD,InboundInspectionAndOrdering:FIRE,StowageCompletion:EARTH,OrderInventoryAllocation:METAL,PackingDispatchAndTransport:WATER') { throw 'FarmHarvestLogisticsStagesInvalid' }
if ((@($farmCohort[0].farmHarvestLogisticsContinuityContract.confirmedShengRelations | ForEach-Object displayName) -join ',') -ne '목생화,화생토,토생금,금생수') { throw 'FarmHarvestLogisticsShengRelationsInvalid' }
if ([string] $farmCohort[0].farmHarvestLogisticsContinuityContract.arrivalReceivingGate.displayName -ne '수생목' -or [string] $farmCohort[0].farmHarvestLogisticsContinuityContract.arrivalReceivingGate.reviewStateCode -ne 'Confirmed') { throw 'FarmHarvestLogisticsArrivalRelationInvalid' }
if (-not [bool] $farmCohort[0].farmHarvestLogisticsContinuityContract.arrivalReceivingGate.arrivalDoesNotOpenReceivingWork -or -not [bool] $farmCohort[0].farmHarvestLogisticsContinuityContract.arrivalReceivingGate.acceptedArrivalOpensReceivingWork -or -not [bool] $farmCohort[0].farmHarvestLogisticsContinuityContract.arrivalReceivingGate.rejectedOrUnresolvedArrivalDoesNotOpenReceivingWork) { throw 'FarmHarvestLogisticsArrivalGateInvalid' }
if ((@($farmCohort[0].farmHarvestLogisticsContinuityContract.arrivalReceivingGate.requiredCheckCodes) -join ',') -ne 'DestinationMatch,OrderMatch,QuantityMatch,SealOrDamageStateAccepted') { throw 'FarmHarvestLogisticsArrivalChecksInvalid' }
if ([string] $farmCohort[0].farmHarvestLogisticsContinuityContract.arrivalReceivingGate.productSpecificCriteriaProfileStateCode -ne 'Deferred') { throw 'FarmHarvestLogisticsProductCriteriaBoundaryInvalid' }
if ([string] $farmCohort[0].farmHarvestLogisticsContinuityContract.placementTermMeaningCode -ne 'OrderInventoryAllocationNotSpatialPlacement' -or -not [bool] $farmCohort[0].farmHarvestLogisticsContinuityContract.stowageCompletionDoesNotCompleteWholeLogistics) { throw 'FarmHarvestLogisticsTermBoundaryInvalid' }
if (-not [bool] $farmCohort[0].farmHarvestLogisticsContinuityContract.operationalSequenceIsNotAutomaticallyShengSequence -or -not [bool] $farmCohort[0].farmHarvestLogisticsContinuityContract.doesNotDuplicateLotOrAuthorityEvent) { throw 'FarmHarvestLogisticsContinuityBoundaryInvalid' }
$placement = @($result.plans | Where-Object planId -eq 'PLAN-PLACEMENT-CROSS-AREA-BUILDING-001')
if ($placement.Count -ne 1 -or [string] $placement[0].decompositionStateCode -ne 'ReviewedAtomicModules') { throw 'FarmPlacementAtomicReviewMissing' }
if (-not (@($placement[0].relatedPlayableLoopIds) -contains 'playable-loop:farm-player-placement.v1')) { throw 'FarmPlacementPlayableUnitRelationMissing' }
$placementPreview = @($result.atomicModules | Where-Object moduleId -eq 'play-transaction:farm-building.inspect-blueprint-placement.v1')
if ($placementPreview.Count -ne 1 -or [string] $placementPreview[0].primaryWorldInteractionId -ne 'WI-CON-01') { throw 'FarmPlacementPreviewReuseMissing' }
$placementCancel = @($result.atomicModules | Where-Object moduleId -eq 'play-transaction:farm-building.cancel-active-construction.v1')
if ($placementCancel.Count -ne 1 -or [string] $placementCancel[0].primaryWorldInteractionId -ne 'WI-WORLD-03') { throw 'FarmPlacementCancellationReuseMissing' }
$placementAssembly = @($result.e1Assemblies | Where-Object assemblyId -eq 'e1-assembly:farm-building-placement.v1')
if ($placementAssembly.Count -ne 1 -or [string] $placementAssembly[0].stateCode -ne 'Blocked' -or [string] $placementAssembly[0].targetPlayableLoopStableId -ne 'playable-loop:farm-player-placement.v1') { throw 'FarmPlacementAssemblyBoundaryInvalid' }

Write-Output "PlayableLoopPlanningE1IndexTestsPassed:Plans=$($result.counts.plans);PlayableUnits=$($result.counts.playableUnits);AtomicModules=$($result.counts.atomicModules);Candidates=$($result.counts.e1CandidatesNeeded)"
