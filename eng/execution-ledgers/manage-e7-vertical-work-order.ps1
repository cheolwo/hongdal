[CmdletBinding()]
param(
    [string] $InputPath = "eng/execution-ledgers/work-orders/e7-vertical-work-order.template.json",
    [string] $ProtocolPath = "eng/execution-ledgers/e7-vertical-implementation-protocol.json",
    [string] $PlayableLoopPath = "eng/execution-ledgers/playable-loops.json"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

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
