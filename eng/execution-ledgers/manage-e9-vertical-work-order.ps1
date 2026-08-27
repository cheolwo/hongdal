[CmdletBinding()]
param(
    [string] $InputPath = "eng/execution-ledgers/work-orders/e9-vertical-work-order.template.json",
    [string] $ProtocolPath = "eng/execution-ledgers/e9-vertical-implementation-protocol.json"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "E9VerticalWorkOrderInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedProtocol = (Resolve-Path (Join-Path $repositoryRoot $ProtocolPath)).Path
$resolvedInput = (Resolve-Path (Join-Path $repositoryRoot $InputPath)).Path
$protocol = Get-Content -LiteralPath $resolvedProtocol -Raw -Encoding UTF8 | ConvertFrom-Json
$workOrder = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json
$resolvedStages = (Resolve-Path (Join-Path $repositoryRoot (
    [string] $protocol.evidenceStageCatalogPath))).Path
$stageCatalog = Get-Content -LiteralPath $resolvedStages -Raw -Encoding UTF8 | ConvertFrom-Json
$resolvedLoops = (Resolve-Path (Join-Path $repositoryRoot (
    [string] $protocol.playableLoopCatalogPath))).Path
$loopCatalog = Get-Content -LiteralPath $resolvedLoops -Raw -Encoding UTF8 | ConvertFrom-Json
$resolvedEvidence = (Resolve-Path (Join-Path $repositoryRoot (
    [string] $protocol.evidencePackageCatalogPath))).Path
$evidenceCatalog = Get-Content -LiteralPath $resolvedEvidence -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $protocol.schemaVersion -eq
    "simulation-e9-vertical-implementation-protocol.v1") "ProtocolSchemaInvalid"
Require ([string] $workOrder.schemaVersion -eq
    [string] $protocol.workOrderSchemaVersion) "WorkOrderSchemaInvalid"
Require ([string] $workOrder.protocolRevision -eq
    [string] $protocol.revision) "ProtocolRevisionMismatch"
Require ((@($stageCatalog.stages.code) -join ",") -eq
    "E0,E1,E2,E3,E4,E5,E6,E7,E8,E9") "EvidenceStageCatalogOrderInvalid"
Require ((@($protocol.downwardReviewOrder) -join ",") -eq
    "E9,E8,E7,E6,E5,E4,E3,E2,E1") "DownwardReviewOrderInvalid"
Require ((@($protocol.upwardImplementationOrder) -join ",") -eq
    "E1,E2,E3,E4,E5,E6,E7,E8") "ImplementationOrderInvalid"
Require ((@($protocol.upwardValidationOrder) -join ",") -eq
    "E1,E2,E3,E4,E5,E6,E7,E8,E9") "UpwardOrderInvalid"
Require ([string] $protocol.iterationCycle.cycleCode -eq
    "E9-E1-E9-Repeat") "IterationCycleCodeInvalid"
Require ((@($protocol.iterationCycle.stepOrder) -join ",") -eq
    ("DownwardImpactReview,LowestUnclosedDependencyImplementation," +
    "UpwardAssemblyAndValidation,ImpactReassessment")) "IterationStepOrderInvalid"
foreach ($condition in @(
    "repeatWhenUpwardFindingsChangeImpact",
    "repeatWhenLowerContractOrEvidenceReopens",
    "closeWhenSelectedScopeStableOrExplicitlyBlocked",
    "doesNotRequireAllStagesToBeImplementedInOneCycle")) {
    $property = $protocol.iterationCycle.PSObject.Properties[$condition]
    Require ($null -ne $property -and [bool] $property.Value) "IterationConditionMissing:$condition"
}

foreach ($condition in @(
    "primaryWorkstreamMustParticipate",
    "ownerAndReviewerRequired",
    "allowedChangeRootsRequired",
    "handoffInAndOutRequired",
    "commitScopeRequired")) {
    $property = $protocol.collaborationPolicy.PSObject.Properties[$condition]
    Require ($null -ne $property -and [bool] $property.Value) `
        "CollaborationPolicyMissing:$condition"
}

foreach ($principle in @(
    "targetFirstPlanningDoesNotClaimE9Evidence",
    "everyStageMustBeAssessed",
    "firstDownwardReviewPrecedesFirstImplementation",
    "implementationStartsFromLowestUnclosedDependency",
    "moduleSkeletonDoesNotRequireImmediateImplementation",
    "upwardValidationIsRequiredForPromotion",
    "downwardAndUpwardPassesRepeatUntilStable",
    "upwardFindingsReopenDownwardImpactReview",
    "workOrderRepresentsCurrentCycleSnapshot",
    "evidenceStagesRemainDistinctFromManagementAndSpatialAxes",
    "wiOrWiLoopIsEvidenceSubject",
    "spatialEvidenceIsConditionalInput",
    "soloAndHostedUseTheSameSimulationCore",
    "authorityStorageAdapterRefactorsRequireWorkOrder",
    "placementControlIsNotAnHLevel",
    "canonicalUnitySceneRemainsSingle")) {
    $property = $protocol.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) "PrincipleMissing:$principle"
}

Require-Text $workOrder.workOrderId "WorkOrderIdMissing"
Require-Text $workOrder.title "TitleMissing"
foreach ($loopRef in @($workOrder.playableLoopRefs)) {
    Require (@($loopCatalog.items.loopStableId) -contains [string] $loopRef) `
        "PlayableLoopRefUnknown:$loopRef"
}
foreach ($evidenceRef in @($workOrder.evidencePackageRefs)) {
    Require (@($evidenceCatalog.packages.evidenceId) -contains [string] $evidenceRef) `
        "EvidencePackageRefUnknown:$evidenceRef"
}
Require (@($protocol.allowedWorkTypes) -contains [string] $workOrder.workType) "WorkTypeInvalid"
Require ([string] $workOrder.targetEvidenceStage -eq "E9") "TargetMustBeE9"
Require (@($stageCatalog.stages.code) -contains [string] $workOrder.currentEvidenceStage) "CurrentEvidenceStageInvalid"
Require-Text $workOrder.targetStableRevision "TargetStableRevisionMissing"
Require ([string] $workOrder.iterationState.cycleCode -eq
    [string] $protocol.iterationCycle.cycleCode) "WorkOrderCycleCodeInvalid"
Require (@($protocol.allowedCurrentPasses) -contains
    [string] $workOrder.iterationState.currentPass) "WorkOrderCurrentPassInvalid"
Require-Text $workOrder.iterationState.lastReassessment "LastReassessmentMissing"
Require-Text $workOrder.iterationState.nextReopenCondition "NextReopenConditionMissing"

$collaboration = $workOrder.collaborationProfile
Require (@($protocol.collaborationPolicy.allowedWorkstreams) -contains
    [string] $collaboration.primaryWorkstream) "PrimaryWorkstreamInvalid"
$participating = @($collaboration.participatingWorkstreams)
Require ($participating.Count -gt 0) "ParticipatingWorkstreamsMissing"
Require (($participating | Sort-Object -Unique).Count -eq $participating.Count) `
    "ParticipatingWorkstreamDuplicate"
foreach ($workstream in $participating) {
    Require (@($protocol.collaborationPolicy.allowedWorkstreams) -contains
        [string] $workstream) "ParticipatingWorkstreamInvalid:$workstream"
}
Require ($participating -contains [string] $collaboration.primaryWorkstream) `
    "PrimaryWorkstreamMustParticipate"
Require-Text $collaboration.ownerRef "CollaborationOwnerMissing"
Require (@($collaboration.reviewerRefs).Count -gt 0) "CollaborationReviewerMissing"
foreach ($reviewer in @($collaboration.reviewerRefs)) {
    Require-Text $reviewer "CollaborationReviewerEmpty"
}
Require (@($collaboration.allowedChangeRoots).Count -gt 0) `
    "AllowedChangeRootsMissing"
foreach ($root in @($collaboration.allowedChangeRoots)) {
    Require-Text $root "AllowedChangeRootEmpty"
}
foreach ($dependency in @($collaboration.dependencyWorkOrderIds)) {
    Require-Text $dependency "DependencyWorkOrderIdEmpty"
    Require ([string] $dependency -ne [string] $workOrder.workOrderId) `
        "DependencyWorkOrderSelfReference"
}
Require (@($collaboration.handoffIn).Count -gt 0) "HandoffInMissing"
Require (@($collaboration.handoffOut).Count -gt 0) "HandoffOutMissing"
foreach ($handoff in @($collaboration.handoffIn) + @($collaboration.handoffOut)) {
    Require-Text $handoff "HandoffEmpty"
}
Require (@($protocol.collaborationPolicy.allowedBranchPolicies) -contains
    [string] $collaboration.branchPolicy) "BranchPolicyInvalid"
Require-Text $collaboration.commitScope "CommitScopeMissing"

Require ([string] $workOrder.authorityProfile.simulationCore -eq "SharedSimulationCore") "SharedSimulationCoreRequired"
Require ([string] $workOrder.authorityProfile.soloAuthorityLocation -eq "LocalProcess") "SoloAuthorityMustBeLocalProcess"
Require ([string] $workOrder.authorityProfile.hostedAuthorityLocation -eq "RemoteHost") "HostedAuthorityMustBeRemoteHost"
Require ($null -ne $workOrder.authorityProfile.PSObject.Properties["operationalEffectsIncluded"]) "OperationalEffectsBoundaryMissing"
Require ([string] $workOrder.spatialProfile.canonicalUnityScene -eq "SimulationWorldShell") "CanonicalSceneInvalid"
Require-Text $workOrder.spatialProfile.placementControlPolicy "PlacementControlPolicyMissing"
Require ([bool] $workOrder.spatialProfile.hHierarchyIsSemanticNotPlacementControl) "PlacementControlMustRemainSeparateFromH"
Require (-not [bool] $workOrder.spatialProfile.newOfficialSceneAllowed) "NewOfficialSceneMustBeDisabled"

$stagePlansByCode = @{}
foreach ($stagePlan in @($protocol.stagePlans)) {
    Require-Text $stagePlan.code "ProtocolStageCodeMissing"
    Require (-not $stagePlansByCode.ContainsKey([string] $stagePlan.code)) "ProtocolStageDuplicate:$($stagePlan.code)"
    Require (@($stagePlan.requiredDimensions).Count -gt 0) "ProtocolDimensionsMissing:$($stagePlan.code)"
    $stagePlansByCode[[string] $stagePlan.code] = $stagePlan
}
Require ((@($protocol.stagePlans.code) -join ",") -eq
    (@($protocol.downwardReviewOrder) -join ",")) "ProtocolStagePlanOrderInvalid"

$downward = @($workOrder.downwardPlan)
Require (($downward.code -join ",") -eq
    (@($protocol.downwardReviewOrder) -join ",")) "WorkOrderDownwardOrderInvalid"
foreach ($stage in $downward) {
    $code = [string] $stage.code
    Require ($stagePlansByCode.ContainsKey($code)) "DownwardStageUnknown:$code"
    Require (@($protocol.allowedDownwardDispositions) -contains
        [string] $stage.disposition) "DownwardDispositionInvalid:$code"
    Require-Text $stage.summary "DownwardSummaryMissing:$code"
    Require-Text $stage.nextAction "DownwardNextActionMissing:$code"
    Require (@($stage.sourceReferences).Count -gt 0) "SourceReferenceMissing:$code"
    foreach ($reference in @($stage.sourceReferences)) {
        Require (Test-Path -LiteralPath (Join-Path $repositoryRoot ([string] $reference))) "SourceReferenceNotFound:${code}:$reference"
    }
    foreach ($dimension in @($stagePlansByCode[$code].requiredDimensions)) {
        $property = $stage.dimensions.PSObject.Properties[[string] $dimension]
        Require ($null -ne $property) "DimensionMissing:${code}:$dimension"
        Require-Text $property.Value "DimensionEmpty:${code}:$dimension"
    }
}

$upward = @($workOrder.upwardValidation)
Require (($upward.code -join ",") -eq
    (@($protocol.upwardValidationOrder) -join ",")) "WorkOrderUpwardOrderInvalid"
foreach ($stage in $upward) {
    $code = [string] $stage.code
    Require (@($protocol.allowedUpwardStatuses) -contains [string] $stage.status) "UpwardStatusInvalid:$code"
    Require-Text $stage.nextAction "UpwardNextActionMissing:$code"
    if ([string] $stage.status -eq "Passed") {
        Require (@($stage.evidence).Count -gt 0) "PassedWithoutEvidence:$code"
    }
}

if ([string] $workOrder.workType -eq "ExistingE8Change") {
    Require-Text $workOrder.baselineE8Revision "ExistingChangeBaselineMissing"
}
if ([string] $workOrder.workType -eq "NewVerticalSlice") {
    Require (-not [bool] $workOrder.promotionEligible) "NewSliceCannotBeImmediatelyPromotionEligible"
}

if ([bool] $workOrder.promotionEligible) {
    Require ([string] $workOrder.workType -eq "ExistingE8Change") "PromotionRequiresExistingE8Change"
    Require (@($downward | Where-Object {
        [string] $_.disposition -in @("Gap", "Blocked") }).Count -eq 0) "PromotionHasDownwardGap"
    Require (@($upward | Where-Object { [string] $_.status -ne "Passed" }).Count -eq 0) "PromotionHasUnpassedStage"
    Require (@($workOrder.promotionBlockers).Count -eq 0) "PromotionHasBlockers"
    Require (@($workOrder.finalEvidenceReferences).Count -gt 0) "PromotionEvidenceMissing"
}
else {
    Require (@($workOrder.promotionBlockers).Count -gt 0) "NonPromotableWorkOrderNeedsBlocker"
}

Write-Output ("E9VerticalWorkOrderValid:{0};Current={1};Pass={2};PromotionEligible={3}" -f
    [string] $workOrder.workOrderId,
    [string] $workOrder.currentEvidenceStage,
    [string] $workOrder.iterationState.currentPass,
    ([bool] $workOrder.promotionEligible).ToString().ToLowerInvariant())
