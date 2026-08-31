[CmdletBinding()]
param(
    [string] $InputPath = "eng/execution-ledgers/work-orders/e7-vertical-work-order.template.json",
    [string] $ProtocolPath = "eng/execution-ledgers/e7-vertical-implementation-protocol.json",
    [string] $PlayableLoopPath = "eng/execution-ledgers/playable-loops.json",
    [string] $UnityProjectRoot = ''
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../common/presentation-module-bindings.ps1')

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "E7VerticalWorkOrderInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$workOrder = Get-Content -LiteralPath (Join-Path $repositoryRoot $InputPath) `
    -Raw -Encoding UTF8 | ConvertFrom-Json
$protocol = Get-Content -LiteralPath (Join-Path $repositoryRoot $ProtocolPath) `
    -Raw -Encoding UTF8 | ConvertFrom-Json
$loops = Get-Content -LiteralPath (Join-Path $repositoryRoot $PlayableLoopPath) `
    -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $protocol.schemaVersion -eq
    "simulation-e7-vertical-implementation-protocol.v2") "ProtocolSchemaInvalid"
Require ([string] $protocol.evidenceModelRevision -eq
    "horizontal-dual-cycle-evidence.r3") "EvidenceModelInvalid"
foreach ($principleName in @(
    "presentationE4PreparesApplicableAssetPlacementHandoff",
    "assetResearchAloneNeverPromotesE5",
    "nonSpatialOrNonVisualWorkCanDeclareNotApplicable")) {
    $principle = $protocol.principles.PSObject.Properties[$principleName]
    Require ($null -ne $principle -and [bool] $principle.Value) `
        "ProtocolPrincipleMissing:$principleName"
}
$presentationHandoff = $protocol.presentationE4ToE5Handoff
Require ($null -ne $presentationHandoff) "PresentationE4HandoffMissing"
foreach ($applicabilityCode in @("Required", "NotApplicable")) {
    Require (@($presentationHandoff.allowedApplicabilityCodes) -contains
        $applicabilityCode) "PresentationApplicabilityMissing:$applicabilityCode"
}
foreach ($readinessCode in @("Ready", "Conditional", "Blocked")) {
    Require (@($presentationHandoff.allowedReadinessCodes) -contains
        $readinessCode) "PresentationReadinessMissing:$readinessCode"
}
foreach ($fieldName in @(
    "playerReadableMoment", "requiredHCapabilities", "visualKeys",
    "primaryAssetCandidateRefs", "alternativeAssetCandidateRefs",
    "fallbackPresentationRefs", "placementIntent",
    "interactionAnchorIntent", "candidateRevisionOrFingerprint",
    "e5ReadinessCode", "openGapRefs")) {
    Require (@($presentationHandoff.requiredFieldsWhenApplicable) -contains
        $fieldName) "PresentationHandoffFieldMissing:$fieldName"
}
Require ([string] $workOrder.schemaVersion -eq
    [string] $protocol.workOrderSchemaVersion) "WorkOrderSchemaInvalid"
Require ([string] $workOrder.protocolRevision -eq
    [string] $protocol.revision) "ProtocolRevisionInvalid"
Require ([string] $workOrder.evidenceModelRevision -eq
    [string] $protocol.evidenceModelRevision) "WorkOrderEvidenceModelInvalid"
Require-Text $workOrder.workOrderId "WorkOrderIdMissing"
Require ([string] $workOrder.workOrderId -match '^E7-WO-') "WorkOrderIdInvalid"
Require-Text $workOrder.title "TitleMissing"
Require-Text $workOrder.playableUnitStableId "PlayableUnitMissing"
Require-Text $workOrder.activeWorldInteractionId "WorldInteractionMissing"
Require ([string] $workOrder.targetEvidenceStage -eq "E7") "TargetMustBeE7"
Require ([string] $workOrder.currentEvidenceStage -match '^E[0-7]$') `
    "CurrentStageInvalid"
Require (@($protocol.allowedCurrentPasses) -contains
    [string] $workOrder.iterationState.currentPass) "CurrentPassInvalid"
Require-Text $workOrder.iterationState.nextReopenCondition `
    "NextReopenConditionMissing"
Require ($null -ne $workOrder.trackPlans) "TrackPlansMissing"
Require ($null -ne $workOrder.integratedGate) "IntegratedGateMissing"

$trackPlans = @($workOrder.trackPlans.logic, $workOrder.trackPlans.presentation)
foreach ($track in $trackPlans) {
    $trackCode = [string] $track.trackCode
    Require (@($protocol.allowedTrackCodes) -contains $trackCode) `
        "TrackCodeInvalid:$trackCode"
    Require ([string] $track.currentEvidenceStage -match '^E[0-7]$') `
        "TrackCurrentStageInvalid:$trackCode"
    Require ((@($track.downwardPlan.code) -join ',') -eq
        (@($protocol.downwardReviewOrder) -join ',')) "DownwardOrderInvalid:$trackCode"
    Require ((@($track.upwardValidation.code) -join ',') -eq
        (@($protocol.upwardValidationOrder) -join ',')) "UpwardOrderInvalid:$trackCode"

    foreach ($review in @($track.downwardPlan)) {
        Require (@($protocol.allowedDispositions) -contains [string] $review.disposition) `
            "DispositionInvalid:${trackCode}:$($review.code)"
        Require-Text $review.summary "SummaryMissing:${trackCode}:$($review.code)"
    }
    foreach ($review in @($track.upwardValidation)) {
        Require (@($protocol.allowedStatuses) -contains [string] $review.status) `
            "StatusInvalid:${trackCode}:$($review.code)"
    }
}

Require ([string] $workOrder.trackPlans.logic.trackCode -eq "Logic") `
    "LogicTrackMissing"
Require ([string] $workOrder.trackPlans.presentation.trackCode -eq "Presentation") `
    "PresentationTrackMissing"

$logicStageNumber = [int] ([string] $workOrder.trackPlans.logic.currentEvidenceStage).Substring(1)
$presentationStageNumber = [int] ([string] $workOrder.trackPlans.presentation.currentEvidenceStage).Substring(1)
$integratedStageNumber = [Math]::Min($logicStageNumber, $presentationStageNumber)
$integratedStage = "E$integratedStageNumber"
Require ([string] $workOrder.currentEvidenceStage -eq $integratedStage) `
    "CurrentStageMustEqualLowerTrack"
Require ([string] $workOrder.integratedGate.currentEvidenceStage -eq $integratedStage) `
    "IntegratedGateMustEqualLowerTrack"
Require (@($protocol.allowedStatuses) -contains [string] $workOrder.integratedGate.status) `
    "IntegratedStatusInvalid"
Require ($presentationStageNumber -lt 5 -or $logicStageNumber -ge 5) `
    "PresentationE5RequiresLogicE5"

$isTemplate = [string] $workOrder.workOrderId -eq "E7-WO-TEMPLATE"
if ($null -ne $workOrder.PSObject.Properties['presentationModuleBindings']) {
    $presentationModules = Get-Content -LiteralPath (Join-Path $repositoryRoot 'eng/execution-ledgers/playable-loop-presentation-validation-modules.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    Test-PresentationModuleBindings $workOrder $presentationModules $repositoryRoot $UnityProjectRoot | Out-Null
}
$preparationProperty = $workOrder.PSObject.Properties["presentationE4Preparation"]
if ($isTemplate -or $null -ne $preparationProperty) {
    $preparation = $workOrder.presentationE4Preparation
    Require ($null -ne $preparation) "PresentationE4PreparationMissing"
    Require (@($presentationHandoff.allowedApplicabilityCodes) -contains
        [string] $preparation.applicabilityCode) `
        "PresentationApplicabilityInvalid"
    Require (@($presentationHandoff.allowedReadinessCodes) -contains
        [string] $preparation.e5ReadinessCode) `
        "PresentationReadinessInvalid"
    foreach ($fieldName in @($presentationHandoff.requiredFieldsWhenApplicable)) {
        Require ($null -ne $preparation.PSObject.Properties[$fieldName]) `
            "PresentationFieldMissing:$fieldName"
    }
    Require ($null -ne $preparation.PSObject.Properties["notApplicableReason"]) `
        "PresentationNotApplicableReasonMissing"
}
if (-not $isTemplate) {
    $loop = @($loops.items | Where-Object {
        [string] $_.loopStableId -eq [string] $workOrder.playableUnitStableId
    })
    Require ($loop.Count -eq 1) "PlayableUnitUnknown"
    Require ([string] $loop[0].loopLevelCode -eq "PlayableUnit") `
        "SubjectMustBePlayableUnit"
    Require ([string] $loop[0].finalEvidenceStage -eq "E7") `
        "PlayableUnitFinalStageMustBeE7"
    Require (@($loop[0].worldInteractionIds) -contains
        [string] $workOrder.activeWorldInteractionId) "WorldInteractionOutsideUnit"
    Require ($null -ne $loop[0].maturityTracks) "PlayableUnitTracksMissing"
    Require ([string] $loop[0].maturityTracks.logic.currentStage -eq
        [string] $workOrder.trackPlans.logic.currentEvidenceStage) `
        "LogicStageDiffersFromPlayableUnit"
    Require ([string] $loop[0].maturityTracks.presentation.currentStage -eq
        [string] $workOrder.trackPlans.presentation.currentEvidenceStage) `
        "PresentationStageDiffersFromPlayableUnit"
}

if ([bool] $workOrder.promotionEligible) {
    Require (-not $isTemplate) "TemplateCannotPromote"
    Require ([string] $workOrder.currentEvidenceStage -eq "E7") `
        "PromotionRequiresE7"
    Require ($logicStageNumber -eq 7 -and $presentationStageNumber -eq 7) `
        "PromotionRequiresBothTracksE7"
    foreach ($track in $trackPlans) {
        Require (@($track.upwardValidation | Where-Object status -ne "Passed").Count `
            -eq 0) "PromotionRequiresAllPassed:$($track.trackCode)"
    }
    Require ([string] $workOrder.integratedGate.status -eq "Passed") `
        "PromotionRequiresIntegratedPass"
    Require-Text $workOrder.integratedGate.candidateRevision `
        "PromotionRequiresCandidateRevision"
    Require (@($workOrder.integratedGate.openFeedbackRefs).Count -eq 0) `
        "PromotionRequiresNoOpenFeedback"
}

Write-Output "E7VerticalWorkOrderValid:$($workOrder.workOrderId);Logic=$($workOrder.trackPlans.logic.currentEvidenceStage);Presentation=$($workOrder.trackPlans.presentation.currentEvidenceStage);Integrated=$($workOrder.currentEvidenceStage);Pass=$($workOrder.iterationState.currentPass);PromotionEligible=$([bool] $workOrder.promotionEligible)"
