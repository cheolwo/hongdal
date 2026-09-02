[CmdletBinding()]
param(
    [ValidateSet('Check', 'Write')]
    [string] $Mode = 'Check',
    [string] $LedgerPath = 'eng/world-seedbeds/graph-map-development-handoffs.json',
    [string] $JsonOutputPath = 'eng/world-seedbeds/generated/graph-map-development-handoffs.v1.json',
    [string] $MarkdownOutputPath = 'docs/AI/generated/graph-map-development-handoffs.md'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$utf8 = [Text.UTF8Encoding]::new($false)
. (Join-Path $PSScriptRoot 'GraphMapTooling.ps1') `
    -RepositoryRoot $repositoryRoot `
    -ErrorPrefix 'GraphMapDevelopmentHandoffInvalid'

$ledger = Read-Json $LedgerPath 'Ledger'
Require ([string] $ledger.schemaVersion -eq 'mirror-graph-map-development-handoffs.v1') 'SchemaVersion'
Require-Text $ledger.revision 'LedgerRevision'
Require ([string] $ledger.generatedAtRuleCode -eq 'DeterministicNoWallClock') 'GeneratedAtRule'

$boundary = $ledger.ownershipBoundary
Require ([string] $boundary.graphMapOwnerCode -eq 'GraphMapWorkstream') 'GraphMapOwner'
Require ([string] $boundary.developmentOwnerCode -eq 'Development') 'DevelopmentOwner'
Require ([string] $boundary.specialistIntegrationOwnerCode -eq 'Development') 'SpecialistIntegrationOwner'
Require ([string] $boundary.planningOwnerCode -eq 'Planning') 'PlanningOwner'
Require (-not [bool] $boundary.graphMapIsImplementationAuthority) 'GraphMapImplementationAuthority'
Require (-not [bool] $boundary.automaticGoalActivation) 'AutomaticGoalActivation'
Require (-not [bool] $boundary.automaticWorkItemCreation) 'AutomaticWorkItemCreation'
Require (-not [bool] $boundary.automaticUnityExecution) 'AutomaticUnityExecution'
Require (-not [bool] $boundary.automaticEvidencePromotion) 'AutomaticEvidencePromotion'

$wiCatalog = Read-Json 'eng/execution-ledgers/world-interactions.json' 'WorldInteractionCatalog'
$wiById = @{}
foreach ($wi in @($wiCatalog.items)) { $wiById[[string] $wi.id] = $wi }

$allowedStatuses = @('Draft', 'ReadyForDevelopment', 'AcceptedByDevelopment', 'InProgress', 'Integrated', 'Blocked', 'Deferred', 'NoImplementationRequired', 'Superseded')
$allowedTrackCodes = @('Logic', 'Presentation', 'Integration')
$allowedEvidenceStages = @('E1', 'E2', 'E3', 'E4', 'E5', 'E6', 'E7')
$items = @($ledger.items)
Require ($items.Count -gt 0) 'ItemsEmpty'
Require-Unique $items { param($x) $x.handoffId } 'HandoffDuplicate'
Require-Unique $items { param($x) $x.slice.sliceStableId } 'SliceDuplicate'

$snapshots = [Collections.Generic.List[object]]::new()
foreach ($item in $items) {
    $id = [string] $item.handoffId
    $status = [string] $item.statusCode
    Require-Text $id 'HandoffId'
    Require ($allowedStatuses -contains $status) "Status:$id"

    $source = $item.source
    $planningHandoffs = Read-Json ([string] $source.planningHandoffRef) "PlanningHandoff:$id"
    $planningHandoffId = [string] $source.planningHandoffId
    $planningMatches = @($planningHandoffs.items | Where-Object { [string] $_.handoffId -eq $planningHandoffId })
    Require ($planningMatches.Count -eq 1) "PlanningHandoffIdentity:$id"
    $planningHandoff = $planningMatches[0]
    Require ([string] $planningHandoff.statusCode -eq 'Integrated') "PlanningHandoffNotIntegrated:$id"

    $planHash = Require-Hash ([string] $source.graphMapPlanRef) $source.graphMapPlanExpectedSha256 "GraphMapPlan:$id"
    $plan = Read-Json ([string] $source.graphMapPlanRef) "GraphMapPlan:$id"
    Require ([string] $plan.graphMapStableId -eq [string] $source.graphMapStableId) "GraphMapStableId:$id"
    Require ([string] $plan.revision -eq [string] $source.graphMapRevision) "GraphMapRevision:$id"
    Require ([string] $planningHandoff.request.targetGraphMapStableId -eq [string] $source.graphMapStableId) "PlanningGraphStableId:$id"
    Require ([string] $planningHandoff.request.targetGraphMapRevision -eq [string] $source.graphMapRevision) "PlanningGraphRevision:$id"
    Require ([string] $planningHandoff.result.graphMapPlanRef -eq [string] $source.graphMapPlanRef) "PlanningGraphRef:$id"
    Require ([string] $planningHandoff.result.graphMapPlanExpectedSha256 -eq $planHash) "PlanningGraphHash:$id"

    $outputHash = Require-Hash ([string] $source.graphMapOutputRef) $source.graphMapOutputExpectedSha256 "GraphMapOutput:$id"
    $graphOutput = Read-Json ([string] $source.graphMapOutputRef) "GraphMapOutput:$id"
    Require ([string] $graphOutput.sourcePlanRef -eq [string] $source.graphMapPlanRef) "GraphMapOutputSourceRef:$id"
    Require ([string] $graphOutput.sourcePlanHashSha256 -eq $planHash) "GraphMapOutputSourceHash:$id"

    $slice = $item.slice
    Require-Text $slice.sliceStableId "SliceStableId:$id"
    Require-Text $slice.title "SliceTitle:$id"
    Require-Text $slice.scopeSummary "SliceSummary:$id"
    Require (@($slice.excludedScope).Count -gt 0) "ExcludedScopeEmpty:$id"
    foreach ($meaningRef in @($slice.planningMeaningRefs)) {
        Require-Text $meaningRef "PlanningMeaningRefEmpty:$id"
        $documentRef = ([string] $meaningRef -split '#')[0]
        Require (Test-Path -LiteralPath (Resolve-RepoChild $documentRef "PlanningMeaningRef:$id") -PathType Leaf) "PlanningMeaningRefMissing:$id`:$documentRef"
    }

    $nodeById = @{}
    foreach ($node in @($plan.level1.nodes)) { $nodeById[[string] $node.nodeId] = $node }
    $edgeById = @{}
    foreach ($edge in @($plan.level1.edges)) { $edgeById[[string] $edge.edgeId] = $edge }
    $constraintById = @{}
    foreach ($constraint in @($plan.level2.constraints)) { $constraintById[[string] $constraint.constraintId] = $constraint }
    $bindingIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($binding in @($plan.level3.bindingAssignments)) { $null = $bindingIds.Add([string] $binding.bindingId) }
    $placementRuleById = @{}
    foreach ($ruleBinding in @($graphOutput.resolvedPlacementRuleBindings)) { $placementRuleById[[string] $ruleBinding.ruleRef] = $ruleBinding }

    $selectedNodes = @($slice.nodeRefs)
    $selectedEdges = @($slice.edgeRefs)
    $selectedConstraints = @($slice.constraintRefs)
    Require ($selectedNodes.Count -gt 0) "SliceNodesEmpty:$id"
    Require-Unique $selectedNodes { param($x) [string] $x } "SliceNodeDuplicate:$id"
    Require-Unique $selectedEdges { param($x) [string] $x } "SliceEdgeDuplicate:$id"
    Require-Unique $selectedConstraints { param($x) [string] $x } "SliceConstraintDuplicate:$id"
    Require-Unique @($slice.placementRuleRefs) { param($x) [string] $x } "SlicePlacementRuleDuplicate:$id"
    Require-Unique @($slice.codeBindingRefs) { param($x) [string] $x } "SliceCodeBindingDuplicate:$id"

    $selectedNodeSet = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($nodeRef in $selectedNodes) {
        Require ($nodeById.ContainsKey([string] $nodeRef)) "SliceNodeUnknown:$id`:$nodeRef"
        Require ([string] $nodeById[[string] $nodeRef].stateCode -ne 'Unresolved') "SliceNodeUnresolved:$id`:$nodeRef"
        $null = $selectedNodeSet.Add([string] $nodeRef)
    }
    foreach ($edgeRef in $selectedEdges) {
        Require ($edgeById.ContainsKey([string] $edgeRef)) "SliceEdgeUnknown:$id`:$edgeRef"
        $edge = $edgeById[[string] $edgeRef]
        Require ([string] $edge.stateCode -ne 'Unresolved') "SliceEdgeUnresolved:$id`:$edgeRef"
        Require ($selectedNodeSet.Contains([string] $edge.fromNodeId)) "SliceEdgeFromOutside:$id`:$edgeRef"
        Require ($selectedNodeSet.Contains([string] $edge.toNodeId)) "SliceEdgeToOutside:$id`:$edgeRef"
    }
    foreach ($constraintRef in $selectedConstraints) {
        Require ($constraintById.ContainsKey([string] $constraintRef)) "SliceConstraintUnknown:$id`:$constraintRef"
    }
    foreach ($placementRuleRef in @($slice.placementRuleRefs)) {
        $placementRuleText = [string] $placementRuleRef
        Require ($placementRuleById.ContainsKey($placementRuleText)) "SlicePlacementRuleUnknown:$id`:$placementRuleText"
        $ruleBinding = $placementRuleById[$placementRuleText]
        $sharesConstraint = @($ruleBinding.constraintRefs | Where-Object { $selectedConstraints -contains [string] $_ }).Count -gt 0
        Require $sharesConstraint "SlicePlacementRuleOutsideConstraints:$id`:$placementRuleText"
    }
    foreach ($bindingRef in @($slice.codeBindingRefs)) {
        Require ($bindingIds.Contains([string] $bindingRef)) "SliceCodeBindingUnknown:$id`:$bindingRef"
    }

    $target = $item.developmentTarget
    Require ($allowedTrackCodes -contains [string] $target.trackCode) "TrackCode:$id"
    Require ($allowedEvidenceStages -contains [string] $target.targetEvidenceStageCode) "TargetEvidenceStage:$id"
    Require ($wiById.ContainsKey([string] $target.worldInteractionId)) "WorldInteractionUnknown:$id"
    $sliceWiFound = $false
    foreach ($nodeRef in $selectedNodes) {
        if (@($nodeById[[string] $nodeRef].worldInteractionIds) -contains [string] $target.worldInteractionId) { $sliceWiFound = $true }
    }
    Require $sliceWiFound "WorldInteractionNotInSlice:$id"

    $goals = Read-Json ([string] $target.goalCatalogRef) "GoalCatalog:$id"
    Require ([string] $goals.revision -eq [string] $target.expectedGoalCatalogRevision) "GoalCatalogRevision:$id"
    $targetLoopStableId = [string] $target.loopStableId
    $goalMatches = @($goals.items | Where-Object { [string] $_.loopStableId -eq $targetLoopStableId })
    Require ($goalMatches.Count -eq 1) "GoalIdentity:$id"
    Require ([string] $goalMatches[0].goalStateCode -ne 'Completed') "GoalCompleted:$id"

    $candidateWorkItemId = [string] $target.candidateWorkItemId
    $candidateMatches = @($goals.workItems | Where-Object { [string] $_.workItemId -eq $candidateWorkItemId })
    Require ($candidateMatches.Count -eq 1) "CandidateWorkItemIdentity:$id"
    $candidate = $candidateMatches[0]
    Require ([string] $candidate.loopStableId -eq [string] $target.loopStableId) "CandidateLoopMismatch:$id"
    Require ([string] $candidate.worldInteractionId -eq [string] $target.worldInteractionId) "CandidateWorldInteractionMismatch:$id"
    Require ([string] $candidate.trackCode -eq [string] $target.trackCode) "CandidateTrackMismatch:$id"
    Require ([string] $candidate.statusCode -eq [string] $target.candidateExpectedStatusCode) "CandidateStatusMismatch:$id"
    Require ([string] $candidate.workOrderRef -eq [string] $target.workOrderRef) "CandidateWorkOrderRefMismatch:$id"
    Require ([string] $candidate.workOrderSha256 -eq ([string] $target.workOrderExpectedSha256).ToUpperInvariant()) "CandidateWorkOrderHashMismatch:$id"
    Require (@($candidate.writePaths).Count -gt 0) "CandidateWritePathsEmpty:$id"
    Require (@($candidate.sharedContractKeys).Count -gt 0) "CandidateSharedContractsEmpty:$id"

    $workOrderHash = Require-Hash ([string] $target.workOrderRef) $target.workOrderExpectedSha256 "WorkOrder:$id"
    $workOrder = Read-Json ([string] $target.workOrderRef) "WorkOrder:$id"
    Require ([string] $workOrder.playableUnitStableId -eq [string] $target.loopStableId) "WorkOrderLoopMismatch:$id"
    Require ([string] $workOrder.activeWorldInteractionId -eq [string] $target.worldInteractionId) "WorkOrderWorldInteractionMismatch:$id"
    Require ([string] $workOrder.planningGate.statusCode -eq 'Approved') "WorkOrderPlanningGate:$id"
    Require ([string] $workOrder.planningGate.designDocumentRef -eq [string] $target.planningDesignRef) "WorkOrderDesignRef:$id"
    Require ([string] $workOrder.planningGate.designRevision -eq [string] $target.planningDesignRevision) "WorkOrderDesignRevision:$id"
    Require ([string] $workOrder.planningGate.designHashSha256 -eq ([string] $target.planningDesignExpectedSha256).ToUpperInvariant()) "WorkOrderDesignHash:$id"
    $designHash = Require-Hash ([string] $target.planningDesignRef) $target.planningDesignExpectedSha256 "PlanningDesign:$id"

    $acceptance = $item.acceptanceContract
    Require (@($acceptance.requiredBeforeAcceptCodes).Count -gt 0) "BeforeAcceptCodesEmpty:$id"
    Require (@($acceptance.requiredResultCodes).Count -gt 0) "RequiredResultCodesEmpty:$id"
    Require-Text $acceptance.completionEvidenceUpperBoundCode "EvidenceUpperBound:$id"
    Require (-not [bool] $acceptance.sceneSaveAllowed) "SceneSaveAllowed:$id"
    Require (-not [bool] $acceptance.commitAllowed) "CommitAllowed:$id"
    Require (-not [bool] $acceptance.pushAllowed) "PushAllowed:$id"

    $readiness = $item.readiness
    $result = $item.result
    $evidence = $item.evidenceBoundary
    Require-Text $readiness.nextAction "NextAction:$id"
    Require-Text $result.returnSummary "ResultSummary:$id"

    if ($status -eq 'ReadyForDevelopment') {
        Require ([bool] $readiness.readyForDevelopment) "ReadyFlag:$id"
        Require (-not [bool] $readiness.developmentAccepted) "ReadyAlreadyAccepted:$id"
        Require (-not [bool] $readiness.autoActivated) "ReadyAutoActivated:$id"
        Require ([int] $readiness.unresolvedSelectedElementCount -eq 0) "ReadyUnresolvedElements:$id"
        Require (@($readiness.blockerItems).Count -eq 0) "ReadyHasBlockers:$id"
        Require ([string] $result.resultCode -eq 'PendingDevelopmentAcceptance') "ReadyResultCode:$id"
        foreach ($property in $evidence.PSObject.Properties) {
            Require (-not [bool] $property.Value) "ReadyEvidenceRaised:$id`:$($property.Name)"
        }
    }
    elseif ($status -eq 'Blocked') {
        Require (@($readiness.blockerItems).Count -gt 0) "BlockedWithoutBlocker:$id"
        Require ([string] $result.resultCode -eq 'Blocked') "BlockedResultCode:$id"
    }
    elseif ($status -eq 'Integrated') {
        Require ([bool] $readiness.developmentAccepted) "IntegratedNotAccepted:$id"
        Require ([string] $result.resultCode -eq 'Integrated') "IntegratedResultCode:$id"
        Require (@($result.consumedGraphElementRefs).Count -gt 0) "IntegratedConsumedRefsEmpty:$id"
        Require (@($result.workItemRefs).Count -gt 0) "IntegratedWorkItemsEmpty:$id"
        Require (@($result.verificationRefs).Count -gt 0) "IntegratedVerificationEmpty:$id"
    }

    Require (-not [bool] $evidence.evidencePromoted) "EvidencePromotedByHandoff:$id"

    $snapshots.Add([ordered]@{
        handoffId = $id
        statusCode = $status
        source = [ordered]@{
            planningHandoffId = [string] $source.planningHandoffId
            graphMapStableId = [string] $source.graphMapStableId
            graphMapRevision = [string] $source.graphMapRevision
            graphMapPlanRef = [string] $source.graphMapPlanRef
            graphMapPlanSha256 = $planHash
            graphMapOutputRef = [string] $source.graphMapOutputRef
            graphMapOutputSha256 = $outputHash
        }
        slice = $slice
        developmentTarget = [ordered]@{
            loopStableId = [string] $target.loopStableId
            worldInteractionId = [string] $target.worldInteractionId
            trackCode = [string] $target.trackCode
            targetEvidenceStageCode = [string] $target.targetEvidenceStageCode
            goalStateCode = [string] $goalMatches[0].goalStateCode
            candidateWorkItemId = [string] $candidate.workItemId
            candidateStatusCode = [string] $candidate.statusCode
            candidateOwnerThreadId = [string] $candidate.ownerThreadId
            candidateWritePaths = @($candidate.writePaths)
            candidateSharedContractKeys = @($candidate.sharedContractKeys)
            workOrderRef = [string] $target.workOrderRef
            workOrderSha256 = $workOrderHash
            planningDesignRef = [string] $target.planningDesignRef
            planningDesignSha256 = $designHash
        }
        acceptanceContract = $acceptance
        readiness = $readiness
        result = $result
        evidenceBoundary = $evidence
    })
}

$counts = [ordered]@{
    total = $items.Count
    readyForDevelopment = @($items | Where-Object statusCode -eq 'ReadyForDevelopment').Count
    acceptedByDevelopment = @($items | Where-Object statusCode -eq 'AcceptedByDevelopment').Count
    inProgress = @($items | Where-Object statusCode -eq 'InProgress').Count
    integrated = @($items | Where-Object statusCode -eq 'Integrated').Count
    blocked = @($items | Where-Object statusCode -eq 'Blocked').Count
    deferred = @($items | Where-Object statusCode -eq 'Deferred').Count
}
$output = [ordered]@{
    schemaVersion = 'mirror-graph-map-development-handoff-output.v1'
    revision = 'mirror-graph-map-development-handoff-output.r1'
    generatedAtRuleCode = 'DeterministicNoWallClock'
    sourceLedgerRef = $LedgerPath
    sourceLedgerSha256 = File-Hash $LedgerPath 'Ledger'
    counts = $counts
    ownershipBoundary = $boundary
    items = @($snapshots)
}
$json = Stable-Json $output

$lines = [Collections.Generic.List[string]]::new()
$lines.Add('# Graph Map 개발 인계 상태')
$lines.Add('')
$lines.Add('> 이 문서는 `eng/world-seedbeds/graph-map-development-handoffs.json`에서 생성한다. 직접 수정하지 않는다.')
$lines.Add('')
$lines.Add("- 원장 판본: $($ledger.revision)")
$lines.Add("- 전체: $($counts.total) / 개발 준비: $($counts.readyForDevelopment) / 개발 수용: $($counts.acceptedByDevelopment) / 진행: $($counts.inProgress) / 통합: $($counts.integrated) / 차단: $($counts.blocked)")
$lines.Add('- ReadyForDevelopment는 개발 수용·자동 활성화·코드 변경을 뜻하지 않는다.')
$lines.Add('- 실제 구현 상태는 Goal·work item·작업 명세·코드·시험·EvidencePackage가 소유한다.')
$lines.Add('')
$lines.Add('| 인계 | 상태 | Graph Map slice | Loop / WI | 후보 work item | 목표·상한 |')
$lines.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($snapshot in $snapshots) {
    $target = "$($snapshot.developmentTarget.loopStableId)<br>$($snapshot.developmentTarget.worldInteractionId)"
    $limit = "$($snapshot.developmentTarget.targetEvidenceStageCode)<br>$($snapshot.acceptanceContract.completionEvidenceUpperBoundCode)"
    $lines.Add("| $(Escape-Cell $snapshot.handoffId) | $($snapshot.statusCode) | $(Escape-Cell $snapshot.slice.sliceStableId) | $target | $(Escape-Cell $snapshot.developmentTarget.candidateWorkItemId) | $limit |")
}
foreach ($snapshot in $snapshots) {
    $lines.Add('')
    $lines.Add("## $($snapshot.handoffId)")
    $lines.Add('')
    $lines.Add("- Graph Map: [$($snapshot.source.graphMapPlanRef)](../../../$($snapshot.source.graphMapPlanRef)) / $($snapshot.source.graphMapRevision) / SHA-256 $($snapshot.source.graphMapPlanSha256)")
    $lines.Add("- 선택 노드: $(Escape-Cell (@($snapshot.slice.nodeRefs) -join ', '))")
    $edgeLabel = if (@($snapshot.slice.edgeRefs).Count -eq 0) { '없음' } else { Escape-Cell (@($snapshot.slice.edgeRefs) -join ', ') }
    $lines.Add("- 선택 엣지: $edgeLabel")
    $lines.Add("- 선택 제약: $(Escape-Cell (@($snapshot.slice.constraintRefs) -join ', '))")
    $placementLabel = if (@($snapshot.slice.placementRuleRefs).Count -eq 0) { '없음' } else { Escape-Cell (@($snapshot.slice.placementRuleRefs) -join ', ') }
    $bindingLabel = if (@($snapshot.slice.codeBindingRefs).Count -eq 0) { '없음' } else { Escape-Cell (@($snapshot.slice.codeBindingRefs) -join ', ') }
    $lines.Add("- 배치 규칙: $placementLabel")
    $lines.Add("- 코드 결속: $bindingLabel")
    $lines.Add("- 개발 후보: $($snapshot.developmentTarget.candidateWorkItemId) / 현재 $($snapshot.developmentTarget.candidateStatusCode) / 자동 활성화 아님")
    $lines.Add("- 작업 명세: [$($snapshot.developmentTarget.workOrderRef)](../../../$($snapshot.developmentTarget.workOrderRef)) / SHA-256 $($snapshot.developmentTarget.workOrderSha256)")
    $lines.Add("- 현재 결과: $($snapshot.result.returnSummary)")
    $lines.Add("- 다음 행동: $($snapshot.readiness.nextAction)")
}
$markdown = Normalize-Text ($lines -join "`n")

$jsonPath = Resolve-RepoChild $JsonOutputPath 'JsonOutput'
$markdownPath = Resolve-RepoChild $MarkdownOutputPath 'MarkdownOutput'
if ($Mode -eq 'Write') {
    $null = New-Item -ItemType Directory -Force -Path (Split-Path -Parent $jsonPath)
    $null = New-Item -ItemType Directory -Force -Path (Split-Path -Parent $markdownPath)
    [IO.File]::WriteAllText($jsonPath, $json, $utf8)
    [IO.File]::WriteAllText($markdownPath, $markdown, $utf8)
    Write-Output "GraphMapDevelopmentHandoff Write passed: items=$($counts.total), ready=$($counts.readyForDevelopment), integrated=$($counts.integrated), blocked=$($counts.blocked)"
    exit 0
}

Require (Test-Path -LiteralPath $jsonPath -PathType Leaf) 'GeneratedJsonMissing'
Require (Test-Path -LiteralPath $markdownPath -PathType Leaf) 'GeneratedMarkdownMissing'
Require ((Normalize-Text (Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8)) -eq $json) 'GeneratedJsonStale'
Require ((Normalize-Text (Get-Content -LiteralPath $markdownPath -Raw -Encoding UTF8)) -eq $markdown) 'GeneratedMarkdownStale'
Write-Output "GraphMapDevelopmentHandoff Check passed: items=$($counts.total), ready=$($counts.readyForDevelopment), integrated=$($counts.integrated), blocked=$($counts.blocked)"
