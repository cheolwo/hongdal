[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/execution-ledgers/codex-playable-loop-goals.json",
    [string] $OutputPath = "docs/AI/generated/codex-playable-loop-goals.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")
. (Join-Path $PSScriptRoot "../common/parallel-development-work.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "CodexPlayableLoopGoalInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = (Resolve-Path (Join-Path $repositoryRoot $InputPath)).Path
$ledger = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json
$loops = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.playableLoopCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$delivery = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.worldInteractionDeliveryPriorityPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$evidence = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.evidencePackageCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$pipeline = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.engineInteractionValidationPath)) -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $ledger.schemaVersion -in @("codex-playable-loop-goals.v3", "codex-playable-loop-goals.v4")) "SchemaInvalid"
$parallel = [string] $ledger.schemaVersion -eq "codex-playable-loop-goals.v4"
foreach ($principle in @(
    "goalOwnsExactlyOnePlayableUnit",
    "aggregateMilestonesUseSeparateHarmonyCampaign",
    "evidenceMaturityDoesNotEqualImplementationProgress",
    "sameGoalSurvivesDownwardReassessment",
    "playerPromiseChangeReplacesGoal",
    "independentPlayableLoopChangeReplacesGoal",
    "corePrecedesExtensionWithinArea",
    "allCorePlayableUnitsPrecedeExtensions",
    "everyPlayableUnitAppearsExactlyOnce",
    "simulationAuthorityRemainsOutsideUnityPresentation",
    "presentationEvidenceE5RequiresLogicE5",
    "presentationFeedbackCanReopenLogic",
    "playableUnitGoalEndsAtE7",
    "postE7CampaignIsNotPlayableUnitGoal",
    "pipelineValidationIsIntegratedGate",
    "pipelineFailureReopensSameGoal",
    "topicPlanningGateRequiredBeforeActivation",
    "legacyActivePlanningGateCannotTransfer")) {
    $property = $ledger.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) "PrincipleMissing:$principle"
}
Require ([string] $ledger.policy.goalSubjectLevelCode -eq "PlayableUnit") "GoalSubjectMustBePlayableUnit"
Require ([string] $ledger.policy.aggregateTreatmentCode -eq
    "SeparateHarmonyCampaign") "AggregateTreatmentInvalid"
Require ([string] $ledger.policy.priorityModeCode -eq "CoreFirstPlayerContinuity") "PriorityModeInvalid"
if ($parallel) {
    Require ([string] $ledger.policy.concurrencyModeCode -eq 'DependencyAndOwnership') 'ConcurrencyModeInvalid'
    Require ($null -eq $ledger.policy.goalWorkInProgressLimit -and $null -eq $ledger.policy.worldInteractionWorkInProgressLimit) 'HardConcurrencyCapNotAllowed'
    Require ([bool] $ledger.principles.independentWorkMayProceedInParallel) 'ParallelPrincipleMissing'
} else {
    Require ([int] $ledger.policy.goalWorkInProgressLimit -eq 1) "GoalWorkInProgressLimitMustBeOne"
    Require ([int] $ledger.policy.worldInteractionWorkInProgressLimit -eq 1) "WorldInteractionWorkInProgressLimitMustBeOne"
}
Require ((@($ledger.policy.allowedMaturityTrackCodes) -join ",") -eq
    "Logic,Presentation") "MaturityTrackCodesInvalid"
Require ([string] $ledger.policy.defaultPlayerTargetEvidenceStage -eq "E7") "DefaultPlayerTargetInvalid"
Require (-not [bool] $ledger.policy.e9AllowedAsGoalTarget) "E9CannotBeImmediateGoalTarget"
Require ((@($ledger.policy.areaContinuityOrderCodes) -join ",") -eq "Nature,Farm,Hub,Town,City") "AreaContinuityOrderInvalid"
Require ((@($ledger.policy.goalReplacementTriggerCodes) -join ",") -eq "PlayerPromiseChanged,IndependentPlayableLoopSelected") "GoalReplacementTriggersInvalid"
Require ((@($ledger.policy.allowedPipelineGateStatusCodes) -join ",") -eq
    "Pending,Passed,Blocked,Stale") "PipelineGateStatusCodesInvalid"
Require ((@($ledger.policy.allowedActivePlanningStatusCodes) -join ",") -eq
    "Approved,LegacyActiveMigration") "ActivePlanningStatusCodesInvalid"
Require ([string] $pipeline.schemaVersion -eq
    "playable-loop-engine-interaction-validation.v2") `
    "PipelineValidationSchemaInvalid"

$loopById = @{}
foreach ($loop in @($loops.items)) { $loopById[[string] $loop.loopStableId] = $loop }
$evidenceById = @{}
foreach ($package in @($evidence.packages)) { $evidenceById[[string] $package.evidenceId] = $package }
$deliveryById = @{}
foreach ($item in @($delivery.items)) { $deliveryById[[string] $item.worldInteractionId] = $item }

$goalCount = @($ledger.items).Count
Require ($goalCount -gt 0) "GoalQueueCountInvalid"
$playableUnitIds = @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit" |
    ForEach-Object { [string] $_.loopStableId })
Require ($goalCount -eq $playableUnitIds.Count) "PlayableUnitGoalCoverageCountInvalid"
$orders = @($ledger.items.queueOrder | ForEach-Object { [int] $_ } | Sort-Object)
Require (($orders -join ",") -eq ((1..$goalCount) -join ",")) "GoalQueueOrderInvalid"
$activeItems = @($ledger.items | Where-Object goalStateCode -eq "Active")
if (-not $parallel) { Require ($activeItems.Count -le 1) "ActiveGoalCountInvalid" }
$seen = @{}
$areaOrder = @($ledger.policy.areaContinuityOrderCodes)
$lastAreaIndexByRole = @{ Core = -1; Extension = -1 }
$extensionPhaseStarted = $false
foreach ($item in @($ledger.items | Sort-Object queueOrder)) {
    $id = [string] $item.loopStableId
    Require ($loopById.ContainsKey($id)) "PlayableLoopUnknown:$id"
    Require (-not $seen.ContainsKey($id)) "PlayableLoopDuplicate:$id"
    Require (@($ledger.allowedGoalStateCodes) -contains [string] $item.goalStateCode) "GoalStateInvalid:$id"
    Require (@($ledger.allowedCompletionRoleCodes) -contains [string] $item.completionRoleCode) "CompletionRoleInvalid:$id"
    Require ([string] $loopById[$id].loopLevelCode -eq "PlayableUnit") "GoalSubjectIsNotPlayableUnit:$id"
    Require ([string] $loopById[$id].completionTierCode -eq [string] $item.completionRoleCode) "CompletionRoleDrift:$id"
    Require ([string] $loopById[$id].finalEvidenceStage -eq [string] $item.targetEvidenceStage) "TargetEvidenceStageDrift:$id"
    Require ([string] $item.targetClosureStateCode -eq "PlayClosed") `
        "TargetClosureInvalid:$id"
    Require ([string] $item.targetEvidenceStage -eq "E7") `
        "TargetEvidenceStageMustBeE7:$id"
    Require-Text $item.nextWorldInteractionId "NextWorldInteractionMissing:$id"
    Require ($deliveryById.ContainsKey([string] $item.nextWorldInteractionId)) "NextWorldInteractionUnknown:$id"
    Require (@($loopById[$id].worldInteractionIds) -contains [string] $item.nextWorldInteractionId) "NextWorldInteractionOutsideLoop:$id"
    foreach ($prerequisite in @($item.activationPrerequisiteLoopStableIds)) {
        Require ($loopById.ContainsKey([string] $prerequisite)) "PrerequisiteUnknown:${id}:$prerequisite"
        Require ($seen.ContainsKey([string] $prerequisite)) "PrerequisiteMustPrecedeGoal:${id}:$prerequisite"
    }
    $roleCode = [string] $item.completionRoleCode
    if ($roleCode -eq "Extension") { $extensionPhaseStarted = $true }
    if ($roleCode -eq "Core") {
        Require (-not $extensionPhaseStarted) "CoreGoalAfterExtensionPhase:$id"
    }
    $areaIndex = [Array]::IndexOf($areaOrder, [string] $item.areaCode)
    Require ($areaIndex -ge 0) "AreaUnknown:$id"
    Require ($areaIndex -ge [int] $lastAreaIndexByRole[$roleCode]) `
        "AreaContinuityBacktrack:${roleCode}:$id"
    if ($areaIndex -gt [int] $lastAreaIndexByRole[$roleCode]) {
        $lastAreaIndexByRole[$roleCode] = $areaIndex
    }
    $seen[$id] = $item
}
Require ((@($seen.Keys | Sort-Object) -join "|") -eq
    (@($playableUnitIds | Sort-Object) -join "|")) "PlayableUnitGoalCoverageSetInvalid"

$workItems = @(Get-ParallelDevelopmentWorkItems -Ledger $ledger)
$workChecks = @(Test-ParallelDevelopmentWorkItems -Ledger $ledger -Loops $loops -RepositoryRoot $repositoryRoot)
if ($parallel) {
    foreach ($result in $workChecks) {
        if ($result.statusCode -in @('Active','ReadyForIntegration')) {
            Require $result.canExecute "WorkItemNotExecutable:$($result.workItemId):$($result.blockerCodes -join ',')"
        }
    }
    $executingLoopIds = @($workItems | Where-Object { $_.statusCode -in @('Active','ReadyForIntegration') } | ForEach-Object { $_.loopStableId } | Sort-Object -Unique)
    foreach ($id in $executingLoopIds) {
        Require (@($activeItems | Where-Object loopStableId -eq $id).Count -eq 1) 'ActiveGoalWorkItemsMismatch'
    }
    foreach ($item in $activeItems) {
        Require (@($workItems | Where-Object { $_.loopStableId -eq $item.loopStableId -and $_.statusCode -ne 'Integrated' }).Count -gt 0) 'ActiveGoalWorkItemsMismatch'
    }
}
$active = $ledger.activeGoal # 이전 소비자를 위한 대표 표시. 전체 실행 집합은 workItems다.
$focusIsExecuting = @($workItems | Where-Object { $_.loopStableId -eq $active.loopStableId -and $_.worldInteractionId -eq $active.activeWorldInteractionId -and $_.trackCode -eq $active.activeMaturityTrackCode -and $_.workOrderRef -eq $active.workOrderRef -and $_.statusCode -in @('Active','ReadyForIntegration') }).Count -gt 0
$validateFocus = (-not $parallel) -or $focusIsExecuting
$activeItem = @($ledger.items | Where-Object {
        [string] $_.loopStableId -eq [string] $active.loopStableId
    })[0]
$activeLoop = $loopById[[string] $active.loopStableId]
$planningGate = $activeLoop.planningGate
Require ($null -ne $activeLoop.PSObject.Properties["planningGate"]) `
    "ActivePlanningGateMissing"
$activePlanningStatus = [string] $planningGate.statusCode
if ($validateFocus) {
    Require (@($ledger.policy.allowedActivePlanningStatusCodes) -contains $activePlanningStatus) "ActiveGoalPlanningNotApproved"
}
if ($validateFocus -and $activePlanningStatus -eq "LegacyActiveMigration") {
    Require ([string] $loops.designDocumentationPolicy.legacyActiveMigrationLoopStableId `
        -eq [string] $active.loopStableId) "LegacyPlanningGateTransferred"
    Require ([string] $active.goalStateCode -eq "Active") `
        "LegacyPlanningGateCannotComplete"
}
$topicPlanningManager = Join-Path $PSScriptRoot `
    "manage-playable-loop-topic-planning.ps1"
& $topicPlanningManager -Mode Validate `
    -PlayableLoopPath ([string] $ledger.playableLoopCatalogPath) `
    -GoalLedgerPath $InputPath | Out-Null
$pipelineState = $active.pipelineValidation
if ($validateFocus) {
Require ([string] $active.goalStateCode -in @("Active", "Completed")) "ActiveGoalStateInvalid"
Require ([string] $active.goalStateCode -eq [string] $activeItem.goalStateCode) "ActiveGoalStateDrift"
if ([string] $active.goalStateCode -eq "Active") {
    if ($parallel) {
        Require (@($workItems | Where-Object { $_.loopStableId -eq $active.loopStableId -and $_.worldInteractionId -eq $active.activeWorldInteractionId -and $_.trackCode -eq $active.activeMaturityTrackCode -and $_.workOrderRef -eq $active.workOrderRef }).Count -gt 0) 'RepresentativeWorkItemMissing'
    } else { Require ($activeItems.Count -eq 1) "ActiveGoalCountInvalid" }
}
else {
    if (-not $parallel) { Require ($activeItems.Count -eq 0) "ActiveGoalCountInvalid" }
}
Require ([string] $active.loopStableId -eq [string] $activeItem.loopStableId) "ActiveGoalReferenceDrift"
Require ([string] $active.targetEvidenceStage -eq [string] $activeItem.targetEvidenceStage) "ActiveGoalTargetDrift"
Require ([string] $active.targetClosureStateCode -eq [string] $activeItem.targetClosureStateCode) "ActiveGoalClosureDrift"
Require ([string] $active.activeWorldInteractionId -eq [string] $activeItem.nextWorldInteractionId) "ActiveWorldInteractionDrift"
Require (@($ledger.policy.allowedMaturityTrackCodes) -contains
    [string] $active.activeMaturityTrackCode) "ActiveMaturityTrackInvalid"
Require ([string] $delivery.activeWork.worldInteractionId -eq [string] $active.activeWorldInteractionId) "DeliveryActiveWorldInteractionDrift"
Require ([string] $delivery.revision -eq [string] $active.baselineRevision) "DeliveryBaselineRevisionDrift"
Require (Test-Path -LiteralPath (Join-Path $repositoryRoot ([string] $active.workOrderRef))) "ActiveWorkOrderMissing"
$workOrder = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $active.workOrderRef)) -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $workOrder.schemaVersion -eq "simulation-e7-vertical-work-order.v2") `
    "ActiveWorkOrderSchemaInvalid"
Require ([string] $workOrder.playableUnitStableId -eq [string] $active.loopStableId) `
    "ActiveWorkOrderLoopMissing"
Require ([string] $workOrder.integratedGate.candidateRevision -eq `
    [string] $active.workOrderTargetRevision) "ActiveWorkOrderRevisionDrift"
$pipelineState = $active.pipelineValidation
$pipelineKey = "{0}|{1}" -f $active.loopStableId,
    $active.activeWorldInteractionId
Require ([string] $pipelineState.profileKey -eq $pipelineKey) `
    "ActivePipelineProfileKeyDrift"
Require-Text $pipelineState.profileRevision "ActivePipelineProfileRevisionMissing"
foreach ($statusName in @("logicStatusCode", "presentationStatusCode",
    "integratedStatusCode")) {
    Require (@($ledger.policy.allowedPipelineGateStatusCodes) -contains
        [string] $pipelineState.$statusName) `
        "ActivePipelineStatusInvalid:$statusName"
}
Require ([string] $pipelineState.earliestReopenEvidenceStageCode -match
    '^E[1-7]$') "ActivePipelineReopenStageInvalid"
Require (@($activeLoop.worldInteractionIds) -contains
    [string] $active.activeWorldInteractionId) `
    "ActivePipelineProfilePairMissing"
Require ([string] $workOrder.pipelineValidation.profileKey -eq
    [string] $pipelineState.profileKey) "WorkOrderPipelineProfileDrift"
Require ([string] $workOrder.pipelineValidation.profileRevision -eq
    [string] $pipelineState.profileRevision) "WorkOrderPipelineRevisionDrift"
Require ([string] $workOrder.pipelineValidation.integratedStatusCode -eq
    [string] $pipelineState.integratedStatusCode) "WorkOrderPipelineStatusDrift"
}

$goalCompleted = [string] $active.goalStateCode -eq "Completed"
if ($goalCompleted -and $validateFocus) {
    Require ([string] $activeLoop.currentEvidenceStage -eq [string] $active.targetEvidenceStage) "CompletedGoalEvidenceStageDrift"
    Require ([string] $activeLoop.closureStateCode -eq [string] $active.targetClosureStateCode) "CompletedGoalClosureDrift"
    Require ([string] $activeLoop.statusCode -eq "Validated") "CompletedGoalLoopStatusInvalid"
    Require (@($activeLoop.blockers).Count -eq 0) "CompletedGoalBlockersRemain"
    Require ([string] $delivery.activeWork.workStateCode -eq "E7Closed") "CompletedGoalWorldInteractionOpen"
}
$activeWiId = [string] $active.activeWorldInteractionId
$currentWi = (Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $delivery.worldInteractionCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json).items |
    Where-Object { [string] $_.id -eq $activeWiId }
Require ($null -ne $currentWi) "ActiveWorldInteractionCatalogMissing"
$activeEvidence = @($activeLoop.evidencePackageRefs | Where-Object { $evidenceById.ContainsKey([string] $_) })
if ($validateFocus) { Require ($activeEvidence.Count -gt 0) "ActiveGoalEvidenceMissing" }

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# Codex PlayableLoop Goal 상태")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$InputPath``에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- Goal 원장 개정: ``$($ledger.revision)``")
$limitLabel = if ($parallel) { '상한 없음' } else { '1' }
[void] $builder.AppendLine("- Goal WIP: ``$($activeItems.Count)/$limitLabel``")
$wiWip = @($workItems | Where-Object { $_.statusCode -in @('Active','ReadyForIntegration') } | ForEach-Object { $_.worldInteractionId } | Sort-Object -Unique).Count
[void] $builder.AppendLine("- WI WIP: ``$wiWip/$limitLabel``")
[void] $builder.AppendLine("- 담당별 강제 상한 없음. 승인·의존성·쓰기 소유권으로 실행 가능 여부를 판정한다.")
[void] $builder.AppendLine("- 주제 기획 관문: ``$activePlanningStatus`` / ``$($planningGate.topicStableId)``")
[void] $builder.AppendLine("- 우선순위: ``$($ledger.policy.priorityModeCode)`` / ``$(@($ledger.policy.areaContinuityOrderCodes) -join ' → ')``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 대표 작업 `/goal` 입력 (전체 실행 목록 아님)")
[void] $builder.AppendLine()
[void] $builder.AppendLine("``````text")
[void] $builder.AppendLine("목표:")
[void] $builder.AppendLine("$($active.loopStableId)의 플레이어 약속을")
[void] $builder.AppendLine("$($active.targetEvidenceStage) $($active.targetClosureStateCode)까지 닫는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("플레이어 약속:")
[void] $builder.AppendLine([string] $activeLoop.playerPromise)
[void] $builder.AppendLine()
[void] $builder.AppendLine("현재 기준:")
[void] $builder.AppendLine("- 현재 폐루프 증거 단계: $($activeLoop.currentEvidenceStage)")
[void] $builder.AppendLine("- 현재 WI 증거 단계: $($delivery.activeWork.currentEvidenceStage)")
[void] $builder.AppendLine("- 현재 성숙도 궤적: $($active.activeMaturityTrackCode)")
[void] $builder.AppendLine("- 현재 작업 WI: $($active.activeWorldInteractionId) $($currentWi.title)")
[void] $builder.AppendLine("- 파이프라인 관문: Logic $($pipelineState.logicStatusCode) / Presentation $($pipelineState.presentationStatusCode) / 통합 $($pipelineState.integratedStatusCode)")
[void] $builder.AppendLine("- 파이프라인 재개 E: $($pipelineState.earliestReopenEvidenceStageCode)")
[void] $builder.AppendLine("- 기준 revision: $($active.baselineRevision) / $($active.workOrderTargetRevision)")
[void] $builder.AppendLine()
[void] $builder.AppendLine("운영 규칙:")
[void] $builder.AppendLine("- 승인된 독립 작업은 병렬로 구현한다. 같은 파일·계약 변경은 소유자를 조율한다.")
[void] $builder.AppendLine("- E7→E1로 폐루프 영향을 검토하고 가장 낮은 미완료 의존성을 고른다.")
[void] $builder.AppendLine("- 구현 후 E1→E7 방향으로 수직 증거를 검증한다.")
[void] $builder.AppendLine("- H 전체가 아니라 현재 폐루프에 필요한 공간 능력만 사용한다.")
[void] $builder.AppendLine("- Scene·Synty 배치·문서·EditMode만으로 E7을 선언하지 않는다.")
[void] $builder.AppendLine("- Solo LocalProcess와 Hosted RemoteHost는 같은 Simulation Core 계약을 사용한다.")
[void] $builder.AppendLine("- 플레이어 의도, Simulation 권위, Unity 표현을 분리한다.")
[void] $builder.AppendLine("- 권위 변경은 행위 원장 기록을 남기고 분야 성장은 적용 또는 사유 있는 NotApplicable로 판정한다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("완료 조건:")
[void] $builder.AppendLine("- 필수 WI가 모두 필요한 증거 단계를 충족한다.")
[void] $builder.AppendLine("- 성공·실패·회복·귀환 경로가 닫힌다.")
[void] $builder.AppendLine("- Save/Restore/Replay 결과가 결정적이다.")
[void] $builder.AppendLine("- E7 실제 입력·Play Mode·Game View 증거가 유효하다.")
[void] $builder.AppendLine("- EvidencePackage가 유효하며 미해결 차단 항목이 없다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("중지 조건:")
[void] $builder.AppendLine("새 권위, 외부 Provider·운영 쓰기, 범위 밖 폐루프 또는 플레이어 약속 변경이 필요하면 사용자 결정을 요청한다.")
[void] $builder.AppendLine("``````")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 현재 상태 보고")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 현재 WI | 현재 E | 현재 증거 | 남은 차단 | 다음 최저 의존성 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- |")
$firstOpenReview = @($delivery.activeWork.stageReviews | Where-Object resultCode -ne "Passed" | Select-Object -First 1)
$nextDependency = if ($goalCompleted) {
    "완료"
}
elseif ($firstOpenReview.Count -gt 0) {
    "``$($activeItem.nextWorldInteractionId) $($firstOpenReview[0].stageCode)``"
}
else {
    "``$($activeItem.nextWorldInteractionId)``"
}
$wiBlockers = @($delivery.activeWork.stageReviews | Where-Object resultCode -eq "Blocked" |
    ForEach-Object { [string] $_.summary })
$allBlockers = @(@($activeLoop.blockers) + $wiBlockers | Select-Object -Unique)
[void] $builder.AppendLine("| ``$($active.activeWorldInteractionId)`` $($currentWi.title) | 폐루프 $($activeLoop.currentEvidenceStage) / WI $($delivery.activeWork.currentEvidenceStage) → $($active.targetEvidenceStage) | $(@($activeLoop.evidencePackageRefs) -join '<br>') | $($allBlockers -join '<br>') | $nextDependency |")
[void] $builder.AppendLine()
if ($parallel) {
    [void] $builder.AppendLine('## 병렬 작업과 통합 인계')
    [void] $builder.AppendLine()
    [void] $builder.AppendLine('| 작업 | Goal / WI | 궤적 / 목표 | 담당 | 상태 | 실제 차단 |')
    [void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- |')
    foreach ($item in @($workItems | Sort-Object workItemId)) {
        $result = @($workChecks | Where-Object workItemId -eq $item.workItemId)[0]
        [void] $builder.AppendLine("| $(Escape-Cell $item.workItemId) | $(Escape-Cell $item.loopStableId)<br>$(Escape-Cell $item.worldInteractionId) | $($item.trackCode) / $($item.targetEvidenceStageCode) | $(Escape-Cell $item.ownerThreadId) | $($item.statusCode) | $(Escape-Cell ($result.blockerCodes -join ', ')) |")
    }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine('개발 스레드가 최종 통합한다. 공간 후보·코드 시험을 공식 Scene 승인이나 Evidence 승격으로 해석하지 않는다.')
    [void] $builder.AppendLine()
}
[void] $builder.AppendLine("## Goal 대기열")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 순서 | 영역 | 역할 | PlayableLoop | 목표 | 상태 | 다음 WI |")
[void] $builder.AppendLine("| ---: | --- | --- | --- | --- | --- | --- |")
foreach ($item in @($ledger.items | Sort-Object queueOrder)) {
    $loop = $loopById[[string] $item.loopStableId]
    [void] $builder.AppendLine("| $($item.queueOrder) | $($item.areaCode) | $($item.completionRoleCode) | $(Escape-Cell $loop.title)<br>``$($item.loopStableId)`` | $($item.targetEvidenceStage) $($item.targetClosureStateCode) | $($item.goalStateCode) | ``$($item.nextWorldInteractionId)`` |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("PlayableUnit Goal은 E7에서 끝난다. 각 PlayableUnit의 E8 반복 안정성, 둘 이상의 안정 Core를 묶는 E9 영역 조화·사람 승인, E10 제한 운영은 별도 캠페인에서 파생한다.")

$content = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    [void] (Write-DeterministicTextIfChanged -Path $resolvedOutput -Content $content)
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedOutput))) -ceq $content) "GeneratedOutputMismatch"
}

Write-Output "CodexPlayableLoopGoalsValid:Goals=$goalCount;Current=$($active.loopStableId);State=$($active.goalStateCode);WI=$($active.activeWorldInteractionId);Revision=$($ledger.revision)"
