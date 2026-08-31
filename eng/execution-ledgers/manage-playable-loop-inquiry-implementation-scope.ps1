[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",
    [string] $CatalogPath = "eng/execution-ledgers/playable-loop-inquiry-implementation-scope.json",
    [string] $OutputPath = "docs/AI/generated/playable-loop-inquiry-implementation-scope.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")
. (Join-Path $PSScriptRoot "../common/parallel-development-work.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PlayableLoopInquiryImplementationScopeInvalid:$Code" }
}

function Expand-Selector([string] $Selector) {
    $numbers = [System.Collections.Generic.List[int]]::new()
    foreach ($part in $Selector.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $value = $part.Trim()
        if ($value -match '^(\d+)-(\d+)$') {
            $start = [int] $Matches[1]
            $end = [int] $Matches[2]
            Require ($start -le $end) "QuestionRangeReversed:$value"
            for ($number = $start; $number -le $end; $number++) { $numbers.Add($number) }
        } elseif ($value -match '^\d+$') {
            $numbers.Add([int] $value)
        } else {
            throw "PlayableLoopInquiryImplementationScopeInvalid:QuestionSelectorInvalid:$value"
        }
    }
    return @($numbers)
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Read-DepthMap([string] $RepositoryRoot, [object] $Catalog) {
    $result = @{}
    $planningRoot = Join-Path $RepositoryRoot 'docs/Architecture/PlayableLoops/PlanningSessions'
    $legacyCandidates = @(Get-ChildItem -LiteralPath $planningRoot -Filter 'Q001-Q198*.md')
    Require ($legacyCandidates.Count -eq 1) 'LegacyDepthIndexMissingOrAmbiguous'
    $legacyIndex = $legacyCandidates[0].FullName
    $legacyText = Get-Content -LiteralPath $legacyIndex -Raw -Encoding UTF8
    foreach ($match in [regex]::Matches($legacyText, 'D([1-5])-\d+/Q-(\d{3})')) {
        $questionId = 'Q-{0:D3}' -f [int] $match.Groups[2].Value
        Require (-not $result.ContainsKey($questionId)) "DepthDuplicated:$questionId"
        $result[$questionId] = 'D' + $match.Groups[1].Value
    }

    foreach ($topic in @($Catalog.topics | Where-Object { $_.topicCode -in @('farm-barracks-defense', 'hub-demand-allocation', 'survival-economy') })) {
        $source = Join-Path $RepositoryRoot ([string] $topic.sourceRef)
        $text = Get-Content -LiteralPath $source -Raw -Encoding UTF8
        foreach ($match in [regex]::Matches($text, 'Q-(\d{3})`?\s*\|\s*`?(D[1-5])')) {
            $questionId = 'Q-{0:D3}' -f [int] $match.Groups[1].Value
            if (-not $result.ContainsKey($questionId)) { $result[$questionId] = $match.Groups[2].Value }
        }
    }

    foreach ($property in $Catalog.depthOverrides.PSObject.Properties) {
        $result[$property.Name] = [string] $property.Value
    }
    return $result
}

function Get-LoopMaturity([object[]] $LoopRefs, [hashtable] $LoopsById) {
    if ($LoopRefs.Count -eq 0) { return @{ logic = 'N/A'; presentation = 'N/A'; integrated = 'N/A' } }
    $logic = [System.Collections.Generic.List[int]]::new()
    $presentation = [System.Collections.Generic.List[int]]::new()
    foreach ($loopRef in $LoopRefs) {
        if (-not $LoopsById.ContainsKey([string] $loopRef)) { continue }
        $loop = $LoopsById[[string] $loopRef]
        if ($null -ne $loop.maturityTracks) {
            $logic.Add([int] ([string] $loop.maturityTracks.logic.currentStage).Substring(1))
            $presentation.Add([int] ([string] $loop.maturityTracks.presentation.currentStage).Substring(1))
        } else {
            $stage = [int] ([string] $loop.currentEvidenceStage).Substring(1)
            $logic.Add($stage)
            $presentation.Add($stage)
        }
    }
    if ($logic.Count -eq 0) { return @{ logic = 'N/A'; presentation = 'N/A'; integrated = 'N/A' } }
    $logicStage = ($logic | Measure-Object -Minimum).Minimum
    $presentationStage = ($presentation | Measure-Object -Minimum).Minimum
    return @{ logic = "E$logicStage"; presentation = "E$presentationStage"; integrated = "E$([Math]::Min($logicStage, $presentationStage))" }
}

# 기존 339개 실행 선택과 분리된 추가 문답 조회다. 이 함수는 원장/검색색인/승인 상태를 쓰지 않는다.
function Add-AdditionalInquiryProjection([object] $Catalog, [string] $Root, [object] $Lines,
    [hashtable] $WorldInteractions, [hashtable] $Loops, [object[]] $Readiness) {
    if ($null -eq $Catalog.PSObject.Properties['additionalInquiryTracking']) { return }
    $extension = $Catalog.additionalInquiryTracking
    Require ([string] $extension.schemaVersion -eq 'inquiry-implementation-extension.v1') 'AdditionalSchemaInvalid'
    Require ([int] $extension.coverage.numberedRange.first -eq 340 -and [int] $extension.coverage.numberedRange.last -eq 403) 'AdditionalNumberedRangeInvalid'
    $semanticIds = @($extension.coverage.semanticQuestionIds)
    Require ($semanticIds.Count -gt 0 -and @($semanticIds | Sort-Object -Unique).Count -eq $semanticIds.Count) 'AdditionalSemanticCoverageInvalid'
    $config = Get-Content -LiteralPath (Join-Path $Root 'eng/planning-inquiries/sources.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $expectedSources = @{}
    foreach ($supplement in @($config.supplements)) {
        foreach ($number in @(Expand-Selector ([string] $supplement.selector))) {
            if ($number -ge 340 -and $number -le 403) { $expectedSources[('Q-{0:D3}' -f $number)] = [string] $supplement.sourceRef }
        }
    }
    $sourceRefs = @(@($Catalog.topics | ForEach-Object sourceRef) + @($config.extraSources | ForEach-Object path) | Sort-Object -Unique)
    $sourceTexts = @{}
    foreach ($sourceRef in $sourceRefs) {
        $text = Get-Content -LiteralPath (Join-Path $Root $sourceRef) -Raw -Encoding UTF8
        $sourceTexts[$sourceRef] = $text
        foreach ($match in [regex]::Matches($text, '질문 식별:\s*`([a-z][a-z0-9-]+)`')) {
            $id = $match.Groups[1].Value
            Require (-not $expectedSources.ContainsKey($id)) "AdditionalSourceQuestionDuplicated:$id"
            $expectedSources[$id] = $sourceRef
        }
    }
    foreach ($id in $semanticIds) {
        Require ([string] $id -cmatch '^[a-z][a-z0-9-]+$' -and $expectedSources.ContainsKey([string] $id)) "AdditionalSemanticIdInvalid:$id"
    }
    # 명시 coverage의 정확 집합만 평가한다. 후속 원문 증가를 실행 평가행 자동등록으로 바꾸지 않는다.
    $declaredIds = @(@(340..403 | ForEach-Object { 'Q-{0:D3}' -f $_ }) + $semanticIds)
    $outsideDeclared = @($expectedSources.Keys | Where-Object { $_ -cnotin $declaredIds } | Sort-Object)
    foreach ($id in $declaredIds) { Require ($expectedSources.ContainsKey($id)) "AdditionalSourceQuestionMissing:$id" }
    foreach ($id in $outsideDeclared) { $expectedSources.Remove($id) }
    $snapshots = @{}
    foreach ($snapshot in @($extension.sourceSnapshots)) {
        $ref = [string] $snapshot.sourceRef
        Require ($sourceTexts.ContainsKey($ref)) "AdditionalSourceUnknown:$ref"
        Require (-not $snapshots.ContainsKey($ref)) "AdditionalSourceSnapshotDuplicated:$ref"
        Require (-not [string]::IsNullOrWhiteSpace([string] $snapshot.sourceRevision)) "AdditionalSourceRevisionMissing:$ref"
        Require ([string] $snapshot.sha256 -match '^[a-fA-F0-9]{64}$') "AdditionalSourceHashInvalid:$ref"
        Require ((Get-FileHash -LiteralPath (Join-Path $Root $ref) -Algorithm SHA256).Hash -eq [string] $snapshot.sha256) "AdditionalSourceHashMismatch:$ref"
        $snapshots[$ref] = $snapshot
    }
    $items = @($extension.items)
    Require ($items.Count -eq $declaredIds.Count) 'AdditionalItemCountInvalid'
    $seen = @{}
    $classes = @('ExecutableTechnicalFollowup', 'TechnicalDependency', 'PlanningStudyBindingRequired', 'Deferred', 'CommonOrSuperseded')
    foreach ($item in $items) {
        $id = [string] $item.questionId
        Require (-not $seen.ContainsKey($id)) "AdditionalQuestionDuplicated:$id"
        Require ($expectedSources.ContainsKey($id)) "AdditionalQuestionUnknown:$id"
        $seen[$id] = $true
        $expectedKind = if ($id -cmatch '^Q-\d{3}$') { 'SupplementNumbered' } else { 'SemanticFollowup' }
        Require ([string] $item.kind -ceq $expectedKind) "AdditionalQuestionKindInvalid:$id"
        Require ([string] $item.sourceRef -ceq $expectedSources[$id]) "AdditionalQuestionSourceMismatch:$id"
        $ref = [string] $item.sourceRef
        Require ($snapshots.ContainsKey($ref)) "AdditionalSourceSnapshotMissing:$id"
        Require ([string] $item.sourceRevision -ceq [string] $snapshots[$ref].sourceRevision) "AdditionalItemRevisionMismatch:$id"
        Require ([string] $item.sourceSha256 -eq [string] $snapshots[$ref].sha256) "AdditionalItemHashMismatch:$id"
        Require (-not [string]::IsNullOrWhiteSpace([string] $item.topicCode)) "AdditionalTopicMissing:$id"
        Require (-not [string]::IsNullOrWhiteSpace([string] $item.sourceRecordStatus)) "AdditionalDecisionMissing:$id"
        Require ($classes -ccontains [string] $item.assessmentClass) "AdditionalAssessmentInvalid:$id"
        Require (-not [string]::IsNullOrWhiteSpace([string] $item.nextActionKo)) "AdditionalNextActionMissing:$id"
        Require ($null -ne $item.PSObject.Properties['e5GapCodes']) "AdditionalE5GapMissing:$id"
        $anchor = [string] $item.sourceAnchor
        Require (-not [string]::IsNullOrWhiteSpace($anchor)) "AdditionalAnchorMissing:$id"
        $text = [string] $sourceTexts[$ref]
        # 원문에 실제 존재하는 질문 식별/제목 또는 해당 Markdown 제목의 slug만 허용한다.
        $anchorFound = $text.Contains($anchor)
        if (-not $anchorFound -and $anchor.StartsWith('#')) {
            foreach ($heading in [regex]::Matches($text, '(?m)^#{1,6}\s+(.+?)\r?$')) {
                $slug = $heading.Groups[1].Value.Trim().ToLowerInvariant() -replace '[^\p{L}\p{Nd}\s_-]', '' -replace '\s', '-'
                if ($anchor.Substring(1) -ceq $slug) { $anchorFound = $true; break }
            }
        }
        Require ($anchorFound -and [regex]::IsMatch($text, '(?<![a-zA-Z0-9-])' + [regex]::Escape($id) + '(?![a-zA-Z0-9-])')) "AdditionalAnchorInvalid:$id"
        $linkedOrders = @()
        foreach ($link in @($item.links)) {
            foreach ($array in @('worldInteractionRefs','playableLoopRefs','workOrderRefs')) {
                Require ($null -ne $link.PSObject.Properties[$array]) "AdditionalLinkFieldMissing:${id}:$array"
            }
            foreach ($wi in @($link.worldInteractionRefs)) { Require ($WorldInteractions.ContainsKey([string] $wi)) "AdditionalWorldInteractionUnknown:${id}:$wi" }
            foreach ($loop in @($link.playableLoopRefs)) { Require ($Loops.ContainsKey([string] $loop)) "AdditionalPlayableLoopUnknown:${id}:$loop" }
            foreach ($orderRef in @($link.workOrderRefs)) {
                $path = [IO.Path]::GetFullPath((Join-Path $Root ([string] $orderRef)))
                Require ($path.StartsWith($Root.TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) "AdditionalWorkOrderOutsideRepository:$id"
                Require (Test-Path -LiteralPath $path -PathType Leaf) "AdditionalWorkOrderMissing:${id}:$orderRef"
                $order = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
                Require ([string] $order.schemaVersion -eq 'simulation-e7-vertical-work-order.v2') "AdditionalWorkOrderSchemaInvalid:$id"
                $linkedOrders += [string] $orderRef
            }
        }
        if ([string] $item.assessmentClass -eq 'ExecutableTechnicalFollowup') {
            Require ([string] $item.sourceRecordStatus -in @('Confirmed','Incorporated')) "AdditionalDecisionNotExecutable:$id"
            $matching = @($Readiness | Where-Object {
                $candidate = $_
                $candidate.canExecute -and $null -ne $candidate.workOrder -and
                $null -ne $candidate.workOrder.PSObject.Properties['approvedQuestionScope'] -and
                @($candidate.workOrder.approvedQuestionScope) -ccontains $id -and
                @($item.links | Where-Object { @($_.worldInteractionRefs) -contains [string] $candidate.worldInteractionId -and @($_.playableLoopRefs) -contains [string] $candidate.loopStableId }).Count -gt 0 -and
                @($linkedOrders | Where-Object {
                    $order = Get-Content -LiteralPath (Join-Path $Root $_) -Raw -Encoding UTF8 | ConvertFrom-Json
                    $order.workOrderId -ceq $candidate.workOrder.workOrderId
                }).Count -gt 0
            })
            Require ($matching.Count -gt 0) "AdditionalExecutableApprovalMissing:$id"
        }
    }
    foreach ($id in $expectedSources.Keys) { Require ($seen.ContainsKey($id)) "AdditionalQuestionMissing:$id" }
    $Lines.Add('')
    $Lines.Add('## 추가 문답 E5 연결 조사 — 실행 권위 없음')
    $Lines.Add('')
    $Lines.Add('- coverage: `LegacyNumbered=339; SupplementNumbered=64; SemanticFollowup=' + $semanticIds.Count + '; Total=' + (339 + $declaredIds.Count) + '`')
    $Lines.Add('- 위 수는 선언된 추가추적 범위이며 최신 검색 전체 수가 아니다. 원천의 질문 식별 선언 중 범위 밖 ' + $outsideDeclared.Count + '개는 평가행·실행 권한에 자동 추가하지 않았다: ' + ($outsideDeclared -join ', '))
    $Lines.Add('- 아래 분류는 원문 판본에 대한 조회다. 기존 339개 실행 선택·승인·WIP·E를 변경하지 않는다. 연결 폐루프의 E는 이 질문의 달성 E가 아니다.')
    $Lines.Add('- source hash 변경은 재조사가 필요하다. 구현 원장 변경 뒤 개발 담당이 기존 검색 색인을 재생성·검증해야 한다.')
    $Lines.Add('')
    $Lines.Add('| 질문 / 종류 | 원문 판본 / 결정 | 조회 분류 | 연결 WI / 폐루프 / 작업 명세 | E5 누락 / 다음 최소 작업 |')
    $Lines.Add('| --- | --- | --- | --- | --- |')
    foreach ($item in @($items | Sort-Object questionId)) {
        $refs = @($item.links | ForEach-Object { @($_.worldInteractionRefs) + @($_.playableLoopRefs) + @($_.workOrderRefs) }) -join '<br>'
        $source = [string] $item.sourceRef + ' / ' + [string] $item.sourceAnchor + '<br>' + [string] $item.sourceRevision + ' / ' + [string] $item.sourceSha256 + '<br>' + [string] $item.sourceRecordStatus
        $gaps = (@($item.e5GapCodes) -join ', ') + '<br>' + [string] $item.nextActionKo
        $Lines.Add("| ``$($item.questionId)`` / ``$($item.kind)`` | $(Escape-Cell $source) | ``$($item.assessmentClass)`` | $(Escape-Cell $refs) | $(Escape-Cell $gaps) |")
    }
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $PSScriptRoot 'world-interaction-registration-functions.ps1')
$registration = Read-WorldInteractionRegistration $repositoryRoot 'eng/execution-ledgers/world-interaction-registration-relations.json' 'eng/execution-ledgers/world-interactions.json'
$registrationByCandidate = @{}
foreach ($decision in $registration.decisions) { $registrationByCandidate[[string] $decision.candidateId] = $decision }
$catalog = Get-Content -LiteralPath (Join-Path $repositoryRoot $CatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $catalog.schemaVersion -eq 'playable-loop-inquiry-implementation-scope.v1') 'SchemaInvalid'
foreach ($principle in @('everyQuestionAppearsExactlyOnce', 'questionDoesNotEqualFeature', 'actualEvidenceComesFromLinkedEvidenceSubject', 'commonModuleDoesNotOwnEvidenceMaturity', 'deferredBranchDoesNotBlockUnrelatedWork', 'independentApprovedWorkMayRunConcurrently', 'questionNumberDoesNotSetImplementationPriority', 'implementationOrderComesFromActivePlayableUnitDependency', 'questionSequenceIsUsedForWorldInteractionExtraction', 'extractedWorldInteractionDrivesEvidenceCycle', 'impactReviewRunsE7ToE1', 'initialAssemblyStartsE1ToE7', 'implementationReopensEarliestAffectedStage', 'logicPresentationCycleBidirectionally', 'unrelatedQuestionCandidateCannotAdvanceActiveGoal')) {
    Require ([bool] $catalog.principles.$principle) "PrincipleMissing:$principle"
}

$routing = $catalog.executionRouting
Require ([string] $routing.selectionModeCode -eq 'ActivePlayableUnitDependency') 'ExecutionSelectionModeInvalid'
Require ([string] $routing.questionNumberRoleCode -eq 'TraceabilityAndExtractionTraversal') 'QuestionNumberRoleInvalid'
Require ([string] $routing.questionTraversalPurposeCode -eq 'WorldInteractionExtraction') 'QuestionTraversalPurposeInvalid'
Require ([string] $routing.questionTraversalDirectionCode -eq 'Q001ToQ339') 'QuestionTraversalDirectionInvalid'
Require ([string] $routing.impactReviewDirectionCode -eq 'E7ToE1') 'ImpactReviewDirectionInvalid'
Require ([string] $routing.initialAssemblyDirectionCode -eq 'E1ToE7') 'InitialAssemblyDirectionInvalid'
Require ([string] $routing.implementationCycleCode -eq 'EarliestAffectedStageReopen') 'ImplementationCycleInvalid'
Require ([string] $routing.logicPresentationCycleCode -eq 'Bidirectional') 'LogicPresentationCycleInvalid'
Require ((@($catalog.worldInteractionExtraction.phaseOrderCodes) -join ',') -eq 'QuestionSequentialTraversal,WorldInteractionCandidateNormalization,WorldInteractionEvidenceCycle') 'WorldInteractionExtractionPhaseOrderInvalid'
Require ([string] $catalog.worldInteractionExtraction.implementationEntryCode -eq 'RegisteredWorldInteractionAndApprovedDesignOnly') 'WorldInteractionExtractionEntryInvalid'
Require ((@($catalog.worldInteractionExtraction.evidenceCycleQueueSelectionBasisCodes) -join ',') -eq 'ApprovedWorkItemDependencies,ApprovedPlanningGate,RegisteredWorldInteraction,EarliestReopenEvidenceStage') 'WorldInteractionEvidenceCycleQueueSelectionBasisInvalid'
Require ([string] $catalog.worldInteractionExtraction.evidenceCycleQueueTieBreakCode -eq 'WorldInteractionStableIdForDeterministicOutputOnly') 'WorldInteractionEvidenceCycleQueueTieBreakInvalid'
Require (@($routing.invariants) -contains 'TraverseQuestionsByNumericOrderForWorldInteractionExtraction') 'SequentialExtractionInvariantMissing'
Require (@($routing.invariants) -contains 'DoNotSelectImplementationByNumericOrder') 'NumericImplementationOrderGuardMissing'
Require (@($routing.invariants) -contains 'DoNotSelectImplementationByReverseNumericOrder') 'ReverseNumericImplementationOrderGuardMissing'

$goalCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $catalog.codexGoalCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
# Legacy routing is a display focus, not an execution lock or approval source.
$activeWorkOrder = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $routing.activeWorkOrderRef)) -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $activeWorkOrder.playableUnitStableId -eq [string] $routing.activePlayableUnitStableId) 'WorkOrderPlayableUnitRoutingMismatch'
Require ([string] $activeWorkOrder.activeWorldInteractionId -eq [string] $routing.activeWorldInteractionId) 'WorkOrderWorldInteractionRoutingMismatch'

$allowedStatuses = @($catalog.allowedImplementationStatusCodes)
$allowedKinds = @($catalog.allowedImplementationKindCodes)
$allowedDecisions = @($catalog.allowedDecisionStatusCodes)
$allowedCheckStatuses = $catalog.allowedCheckStatusCodes
$depthMap = Read-DepthMap $repositoryRoot $catalog
$loopCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $catalog.playableLoopCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$loopsById = @{}
foreach ($loop in @($loopCatalog.items)) { $loopsById[[string] $loop.loopStableId] = $loop }
$parallelWorkItems = @(Get-ParallelDevelopmentWorkItems -Ledger $goalCatalog)
$parallelReadiness = @(Test-ParallelDevelopmentWorkItems -Ledger $goalCatalog -Loops $loopCatalog -RepositoryRoot $repositoryRoot)
$worldInteractionCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $catalog.worldInteractionCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$worldInteractionsById = @{}
foreach ($worldInteraction in @($worldInteractionCatalog.items)) { $worldInteractionsById[[string] $worldInteraction.id] = $worldInteraction }
$evidenceResponsibilityCodeMap = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $catalog.evidenceResponsibilityCodeMapPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $evidenceResponsibilityCodeMap.schemaVersion -eq 'ssalddel-evidence-responsibility-map.v2') 'EvidenceResponsibilityCodeMapSchemaInvalid'
$evidenceComponentsByWorldInteractionId = @{}
foreach ($component in @($evidenceResponsibilityCodeMap.components)) {
    foreach ($worldInteractionId in @($component.worldInteractionIds)) {
        if (-not $evidenceComponentsByWorldInteractionId.ContainsKey([string] $worldInteractionId)) {
            $evidenceComponentsByWorldInteractionId[[string] $worldInteractionId] = [System.Collections.Generic.List[object]]::new()
        }
        $evidenceComponentsByWorldInteractionId[[string] $worldInteractionId].Add($component)
    }
}

$questions = [System.Collections.Generic.List[object]]::new()
$seen = @{}
foreach ($topic in @($catalog.topics)) {
    Require (Test-Path -LiteralPath (Join-Path $repositoryRoot ([string] $topic.sourceRef))) "SourceMissing:$($topic.topicCode)"
    Require ($allowedStatuses -contains [string] $topic.implementationStatusCode) "StatusInvalid:$($topic.topicCode)"
    Require ($allowedKinds -contains [string] $topic.implementationKindCode) "KindInvalid:$($topic.topicCode)"
    $maturity = if ([string] $topic.implementationKindCode -eq 'PlayableLoop') {
        Get-LoopMaturity @($topic.playableLoopRefs) $loopsById
    } else { @{ logic = 'N/A'; presentation = 'N/A'; integrated = 'N/A' } }
    foreach ($number in @(Expand-Selector ([string] $topic.questionSelector))) {
        $questionId = 'Q-{0:D3}' -f $number
        Require (-not $seen.ContainsKey($questionId)) "QuestionDuplicated:$questionId"
        Require ($depthMap.ContainsKey($questionId)) "DepthMissing:$questionId"
        $seen[$questionId] = $true
        $decision = 'Confirmed'
        $status = [string] $topic.implementationStatusCode
        $kind = [string] $topic.implementationKindCode
        $next = [string] $topic.nextTargetStageCode
        $blockers = @($topic.blockerCodes)
        $override = $null
        if ($null -ne $catalog.questionOverrides.PSObject.Properties[$questionId]) {
            $override = $catalog.questionOverrides.$questionId
            if ($null -ne $override.PSObject.Properties['decisionStatusCode']) { $decision = [string] $override.decisionStatusCode }
            if ($null -ne $override.PSObject.Properties['implementationStatusCode']) { $status = [string] $override.implementationStatusCode }
            if ($null -ne $override.PSObject.Properties['implementationKindCode']) { $kind = [string] $override.implementationKindCode }
            if ($null -ne $override.PSObject.Properties['nextTargetStageCode']) { $next = [string] $override.nextTargetStageCode }
            if ($null -ne $override.PSObject.Properties['blockerCodes']) { $blockers = @($override.blockerCodes) }
        }
        Require ($allowedDecisions -contains $decision) "DecisionInvalid:$questionId"
        Require ($allowedStatuses -contains $status) "StatusInvalid:$questionId"
        Require ($allowedKinds -contains $kind) "KindInvalid:$questionId"
        $questionMaturity = if ($kind -eq 'PlayableLoop') { $maturity } else { @{ logic = 'N/A'; presentation = 'N/A'; integrated = 'N/A' } }
        $actualLoopRefs = @($topic.playableLoopRefs | Where-Object { $loopsById.ContainsKey([string] $_) })
        $plannedLoopRefs = @($topic.playableLoopRefs | Where-Object { -not $loopsById.ContainsKey([string] $_) })
        $actualWorldInteractionRefs = @($topic.worldInteractionRefs | Where-Object { $worldInteractionsById.ContainsKey([string] $_) })
        $plannedWorldInteractionRefs = @($topic.worldInteractionRefs | Where-Object { -not $worldInteractionsById.ContainsKey([string] $_) })
        $checks = [ordered]@{}
        foreach ($checkName in @('planningRecord', 'designBinding', 'implementation', 'automatedVerification', 'runtimeVerification', 'evidenceBinding')) {
            $value = [string] $catalog.checkDefaults.$checkName
            if ($null -ne $topic.PSObject.Properties['checkDefaults'] -and $null -ne $topic.checkDefaults.PSObject.Properties[$checkName]) {
                $value = [string] $topic.checkDefaults.$checkName
            }
            if ($null -ne $override -and $null -ne $override.PSObject.Properties['checks'] -and $null -ne $override.checks.PSObject.Properties[$checkName]) {
                $value = [string] $override.checks.$checkName
            }
            Require (@($allowedCheckStatuses.$checkName) -contains $value) "CheckStatusInvalid:${questionId}:${checkName}:$value"
            $checks[$checkName] = $value
        }
        $checkRefs = @()
        if ($null -ne $topic.PSObject.Properties['checkRefs']) { $checkRefs += @($topic.checkRefs) }
        if ($null -ne $override -and $null -ne $override.PSObject.Properties['checkRefs']) { $checkRefs += @($override.checkRefs) }
        if ($checks.implementation -eq 'Implemented' -or $checks.automatedVerification -eq 'Passed' -or $checks.runtimeVerification -eq 'Passed') {
            Require ($checkRefs.Count -gt 0) "VerifiedCheckRequiresReference:$questionId"
        }
        $questions.Add([pscustomobject]@{
            questionId = $questionId
            topicCode = [string] $topic.topicCode
            topicTitleKo = [string] $topic.titleKo
            depthCode = [string] $depthMap[$questionId]
            decisionStatusCode = $decision
            implementationKindCode = $kind
            implementationStatusCode = $status
            playableLoopRefs = $actualLoopRefs
            plannedPlayableLoopRefs = $plannedLoopRefs
            worldInteractionRefs = $actualWorldInteractionRefs
            plannedWorldInteractionRefs = $plannedWorldInteractionRefs
            hCapabilityRefs = @($topic.hCapabilityRefs)
            commonModuleRefs = @($topic.commonModuleRefs)
            logicStageCode = [string] $questionMaturity.logic
            presentationStageCode = [string] $questionMaturity.presentation
            integratedStageCode = [string] $questionMaturity.integrated
            nextTargetStageCode = $next
            blockerCodes = $blockers
            checks = [pscustomobject] $checks
            checkRefs = $checkRefs
            evidenceRefs = @([string] $topic.sourceRef, [string] $catalog.playableLoopCatalogPath)
        })
    }
}

$first = [int] $catalog.questionRange.first
$last = [int] $catalog.questionRange.last
Require ($questions.Count -eq ($last - $first + 1)) "QuestionCountInvalid:$($questions.Count)"
for ($number = $first; $number -le $last; $number++) {
    $questionId = 'Q-{0:D3}' -f $number
    Require ($seen.ContainsKey($questionId)) "QuestionMissing:$questionId"
}

$ordered = @($questions | Sort-Object questionId)
$questionsById = @{}
foreach ($question in $ordered) { $questionsById[$question.questionId] = $question }
$allowedBatchStates = @($catalog.allowedImplementationBatchStateCodes)
$batchIds = @{}
$batchedQuestionIds = @{}
$implementationBatches = @($catalog.smallImplementationBatches)
foreach ($batch in $implementationBatches) {
    $batchId = [string] $batch.batchStableId
    Require (-not [string]::IsNullOrWhiteSpace($batchId)) 'ImplementationBatchStableIdRequired'
    Require (-not $batchIds.ContainsKey($batchId)) "ImplementationBatchDuplicated:$batchId"
    $batchIds[$batchId] = $true
    Require ($allowedBatchStates -contains [string] $batch.executionStateCode) "ImplementationBatchStateInvalid:$batchId"
    Require (@($catalog.topics.topicCode) -contains [string] $batch.topicCode) "ImplementationBatchTopicMissing:$batchId"
    $batchPlayableUnitStableId = if ($null -ne $batch.PSObject.Properties['playableUnitStableId']) { [string] $batch.playableUnitStableId } else { '' }
    $batchCommonModuleStableId = if ($null -ne $batch.PSObject.Properties['commonModuleStableId']) { [string] $batch.commonModuleStableId } else { '' }
    Require (-not ([string]::IsNullOrWhiteSpace($batchPlayableUnitStableId) -and [string]::IsNullOrWhiteSpace($batchCommonModuleStableId))) "ImplementationBatchEvidenceSubjectMissing:$batchId"
    Require ([string]::IsNullOrWhiteSpace($batchPlayableUnitStableId) -or [string]::IsNullOrWhiteSpace($batchCommonModuleStableId)) "ImplementationBatchEvidenceSubjectAmbiguous:$batchId"
    if (-not [string]::IsNullOrWhiteSpace($batchPlayableUnitStableId)) {
        Require ($loopsById.ContainsKey($batchPlayableUnitStableId)) "ImplementationBatchPlayableUnitMissing:$batchId"
    }
    if (-not [string]::IsNullOrWhiteSpace($batchCommonModuleStableId)) {
        $batchTopic = @($catalog.topics | Where-Object { $_.topicCode -eq [string] $batch.topicCode })[0]
        Require (@($batchTopic.commonModuleRefs) -contains $batchCommonModuleStableId) "ImplementationBatchCommonModuleMissing:${batchId}:$batchCommonModuleStableId"
        Require ([string] $batch.executionStateCode -notin @('Active', 'WaitingForApprovedRevision')) "CommonModuleImplementationBatchCannotBeCurrent:$batchId"
    }
    if (-not [string]::IsNullOrWhiteSpace($batchCommonModuleStableId)) {
        Require ([string] $batch.currentStageCode -eq 'N/A') "CommonModuleImplementationBatchCurrentStageInvalid:$batchId"
        Require ([string] $batch.nextStageCode -eq 'N/A') "CommonModuleImplementationBatchNextStageInvalid:$batchId"
    } else {
        Require ([string] $batch.currentStageCode -match '^E[1-7]$') "ImplementationBatchCurrentStageInvalid:$batchId"
        Require ([string] $batch.nextStageCode -match '^E[1-7]$') "ImplementationBatchNextStageInvalid:$batchId"
    }
    Require (@($batch.questionIds).Count -gt 0) "ImplementationBatchQuestionMissing:$batchId"
    Require (@($batch.checkRefs).Count -gt 0) "ImplementationBatchCheckRefMissing:$batchId"
    foreach ($questionId in @($batch.questionIds)) {
        Require ($questionsById.ContainsKey([string] $questionId)) "ImplementationBatchQuestionUnknown:${batchId}:$questionId"
        Require ([string] $questionsById[[string] $questionId].topicCode -eq [string] $batch.topicCode) "ImplementationBatchQuestionTopicMismatch:${batchId}:$questionId"
        Require (-not $batchedQuestionIds.ContainsKey([string] $questionId)) "ImplementationBatchQuestionDuplicated:$questionId"
        $batchedQuestionIds[[string] $questionId] = $batchId
        if ([string] $batch.executionStateCode -in @('PlanningBlocked', 'ParkedCandidate', 'ImplementedParked')) {
            $isExplicitlyParked = @($routing.parkedCandidateQuestionIds) -contains [string] $questionId
            $isRoutedByFullyPartitionedTopic = @($catalog.fullyPartitionedTopicCodes) -contains [string] $batch.topicCode
            Require ($isExplicitlyParked -or $isRoutedByFullyPartitionedTopic) "ParkedImplementationBatchQuestionNotRouted:${batchId}:$questionId"
        }
    }
    $plannedWorldInteractionIds = @()
    if ($null -ne $batch.PSObject.Properties['plannedWorldInteractionIds']) {
        $plannedWorldInteractionIds = @($batch.plannedWorldInteractionIds)
    }
    foreach ($worldInteractionId in @($batch.worldInteractionIds)) {
        Require ($worldInteractionsById.ContainsKey([string] $worldInteractionId)) "ImplementationBatchWorldInteractionMissing:${batchId}:$worldInteractionId"
    }
    foreach ($worldInteractionId in $plannedWorldInteractionIds) {
        Require ([string] $worldInteractionId -match '^WI-[A-Z0-9-]+$') "PlannedWorldInteractionIdInvalid:${batchId}:$worldInteractionId"
        Require (-not $worldInteractionsById.ContainsKey([string] $worldInteractionId) -or $registrationByCandidate.ContainsKey([string] $worldInteractionId)) "PlannedWorldInteractionAlreadyRegistered:${batchId}:$worldInteractionId"
    }
    if ([string] $batch.executionStateCode -in @('Active', 'WaitingForApprovedRevision')) {
        Require ($plannedWorldInteractionIds.Count -eq 0) "CurrentImplementationBatchCannotUsePlannedWorldInteraction:$batchId"
    }
}
$activeBatches = @($implementationBatches | Where-Object { $_.executionStateCode -eq 'Active' })
Require ($batchIds.ContainsKey([string] $catalog.currentWorkBatchStableId)) 'CurrentWorkBatchMissing'
$currentWorkBatch = @($implementationBatches | Where-Object { $_.batchStableId -eq [string] $catalog.currentWorkBatchStableId })[0]
Require ([string] $currentWorkBatch.playableUnitStableId -eq [string] $routing.activePlayableUnitStableId) 'CurrentWorkBatchPlayableUnitMismatch'
Require (@($currentWorkBatch.worldInteractionIds) -contains [string] $routing.activeWorldInteractionId) 'CurrentWorkBatchWorldInteractionMismatch'
$executionSelectionStateCode = if ([string] $currentWorkBatch.executionStateCode -eq 'Active') {
    'CurrentBatchReady'
} elseif ([string] $currentWorkBatch.executionStateCode -eq 'WaitingForApprovedRevision') {
    'CurrentBatchWaitingForApprovedRevision'
} else {
    'DisplayBatchNotActive'
}
$executableImplementationBatches = @($implementationBatches | Where-Object {
    $batch = $_
    @($parallelReadiness | Where-Object {
        $readiness = $_
        $readiness.canExecute -and $null -ne $batch.PSObject.Properties['playableUnitStableId'] -and $readiness.loopStableId -eq $batch.playableUnitStableId -and
        @($batch.worldInteractionIds) -contains $readiness.worldInteractionId -and $null -ne $readiness.workOrder.PSObject.Properties['approvedQuestionScope'] -and
        @($batch.questionIds | Where-Object { @($readiness.workOrder.approvedQuestionScope) -contains $_ }).Count -gt 0
    }).Count -gt 0
})
$earliestResumeStageCode = [string] $currentWorkBatch.nextStageCode
$currentBatchQuestionIds = @($currentWorkBatch.questionIds)
$activeTopicCode = [string] $routing.activeTopicCode
$activeTopicQuestionIds = @($ordered | Where-Object { $_.topicCode -eq $activeTopicCode } | ForEach-Object { $_.questionId })
$unbatchedActiveTopicQuestionIds = @($activeTopicQuestionIds | Where-Object { -not $batchedQuestionIds.ContainsKey([string] $_) })
Require ($unbatchedActiveTopicQuestionIds.Count -eq 0) "ActiveTopicImplementationBatchCoverageMissing:$($unbatchedActiveTopicQuestionIds -join ',')"
$fullyPartitionedTopicCoverage = [System.Collections.Generic.List[string]]::new()
foreach ($topicCode in @($catalog.fullyPartitionedTopicCodes)) {
    Require (@($catalog.topics.topicCode) -contains [string] $topicCode) "FullyPartitionedTopicMissing:$topicCode"
    $topicQuestionIds = @($ordered | Where-Object { $_.topicCode -eq [string] $topicCode } | ForEach-Object { $_.questionId })
    $unbatchedTopicQuestionIds = @($topicQuestionIds | Where-Object { -not $batchedQuestionIds.ContainsKey([string] $_) })
    Require ($unbatchedTopicQuestionIds.Count -eq 0) "FullyPartitionedTopicBatchCoverageMissing:${topicCode}:$($unbatchedTopicQuestionIds -join ',')"
    $fullyPartitionedTopicCoverage.Add("$topicCode=$($topicQuestionIds.Count)/$($topicQuestionIds.Count)")
}
$questionWiExtractions = [System.Collections.Generic.List[object]]::new()
function Resolve-RegistrationWi([string] $Id) {
    if ($registrationByCandidate.ContainsKey($Id)) {
        $decision = $registrationByCandidate[$Id]
        if ($decision.dispositionCode -in @('MetadataFamily','ResultProjection')) { return '' }
        return [string] $decision.canonicalId
    }
    return $Id
}
function Sort-OrdinalRegistrationIds([string[]] $Ids) {
    $sorted = [string[]] @($Ids | Select-Object -Unique)
    [Array]::Sort($sorted, [StringComparer]::Ordinal)
    return $sorted
}
$worldInteractionExtractionById = @{}
foreach ($question in $ordered) {
    $actualIds = @($question.worldInteractionRefs)
    $plannedIds = @($question.plannedWorldInteractionRefs)
    $sourceCode = 'TopicSeed'
    if ($batchedQuestionIds.ContainsKey([string] $question.questionId)) {
        $batchId = [string] $batchedQuestionIds[[string] $question.questionId]
        $batch = @($implementationBatches | Where-Object { $_.batchStableId -eq $batchId })[0]
        $actualIds = @($batch.worldInteractionIds)
        $plannedIds = @()
        if ($null -ne $batch.PSObject.Properties['plannedWorldInteractionIds']) {
            $plannedIds = @($batch.plannedWorldInteractionIds)
        }
        $sourceCode = 'SmallBatchRefinement'
    }
    $actualIds = @($actualIds | Where-Object { $_ } | Sort-Object -Unique)
    $plannedIds = @($plannedIds | Where-Object { $_ } | Sort-Object -Unique)
    $classificationCode = if ($actualIds.Count -gt 0 -and $plannedIds.Count -gt 0) {
        'MixedExistingAndPlanned'
    } elseif ($actualIds.Count -gt 0) {
        'ExistingCandidateExtracted'
    } elseif ($plannedIds.Count -gt 0) {
        'PlannedCandidateExtracted'
    } else {
        'NoDirectCandidate'
    }
    $questionWiExtractions.Add([pscustomobject]@{
        questionId = [string] $question.questionId
        sourceCode = $sourceCode
        classificationCode = $classificationCode
        actualWorldInteractionIds = $actualIds
        plannedWorldInteractionIds = $plannedIds
    })
    foreach ($worldInteractionId in @($actualIds + $plannedIds)) {
        if (-not $worldInteractionExtractionById.ContainsKey([string] $worldInteractionId)) {
            $worldInteractionExtractionById[[string] $worldInteractionId] = [ordered]@{
                registrationCode = if ($worldInteractionsById.ContainsKey([string] $worldInteractionId)) { 'Registered' }
                    elseif ($registrationByCandidate.ContainsKey([string] $worldInteractionId)) { [string] $registrationByCandidate[[string] $worldInteractionId].dispositionCode }
                    else { 'PlannedCandidate' }
                questionIds = [System.Collections.Generic.List[string]]::new()
            }
        }
        $questionIdsForWorldInteraction = $worldInteractionExtractionById[[string] $worldInteractionId].questionIds
        if (-not $questionIdsForWorldInteraction.Contains([string] $question.questionId)) {
            $questionIdsForWorldInteraction.Add([string] $question.questionId)
        }
    }
}
Require ($questionWiExtractions.Count -eq $ordered.Count) 'WorldInteractionExtractionQuestionCoverageInvalid'
$worldInteractionExtractionCounts = @($questionWiExtractions | Group-Object classificationCode | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Count)" })
$worldInteractionExtractionSourceCounts = @($questionWiExtractions | Group-Object sourceCode | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Count)" })
$normalizedWorldInteractionExtractions = @($worldInteractionExtractionById.Keys | Sort-Object | ForEach-Object {
    [pscustomobject]@{
        worldInteractionId = [string] $_
        registrationCode = [string] $worldInteractionExtractionById[$_].registrationCode
        questionIds = @($worldInteractionExtractionById[$_].questionIds | Sort-Object)
    }
})
$workReopenStages = @{}
$worldInteractionEvidenceCycleQueue = @($normalizedWorldInteractionExtractions | Where-Object { $_.registrationCode -in @('Registered','PlannedCandidate') } | ForEach-Object {
    $extraction = $_
    $worldInteractionId = [string] $extraction.worldInteractionId
    $relatedBatches = @($implementationBatches | Where-Object {
        $plannedIds = if ($null -ne $_.PSObject.Properties['plannedWorldInteractionIds']) { @($_.plannedWorldInteractionIds) } else { @() }
        $resolvedIds = @(@($_.worldInteractionIds) + $plannedIds | ForEach-Object { Resolve-RegistrationWi ([string] $_) })
        $resolvedIds -contains $worldInteractionId
    })
    Require ($relatedBatches.Count -gt 0) "WorldInteractionEvidenceCycleBatchMissing:$worldInteractionId"

    $relatedPlayableUnitIds = @($relatedBatches | ForEach-Object {
        if ($null -ne $_.PSObject.Properties['playableUnitStableId'] -and -not [string]::IsNullOrWhiteSpace([string] $_.playableUnitStableId)) {
            [string] $_.playableUnitStableId
        }
    } | Sort-Object -Unique)
    $planningStatuses = @($relatedPlayableUnitIds | ForEach-Object {
        $loop = $loopsById[[string] $_]
        if ($null -ne $loop.PSObject.Properties['planningGate']) { [string] $loop.planningGate.statusCode } else { 'NotApproved' }
    })
    $isActiveWorldInteraction = $worldInteractionId -eq [string] $routing.activeWorldInteractionId
    $wiWorkReadiness = @($parallelReadiness | Where-Object { $_.worldInteractionId -eq $worldInteractionId })
    $executableWiWork = @($wiWorkReadiness | Where-Object canExecute)
    $governingBatches = @($relatedBatches)
    $planningApprovalCode = if ($executableWiWork.Count -gt 0) {
        'ApprovedForWorkItem'
    } elseif ($wiWorkReadiness.Count -gt 0) {
        'WorkItemBlocked'
    } elseif ($planningStatuses.Count -gt 0 -and @($planningStatuses | Where-Object { $_ -ne 'Approved' }).Count -eq 0) {
        'Approved'
    } else {
        'NotApproved'
    }
    $stageNumbers = @($wiWorkReadiness | ForEach-Object {
        $readiness = $_
        $workItem = @($parallelWorkItems | Where-Object { $_.workItemId -eq $readiness.workItemId })[0]
        $trackName = ([string] $workItem.trackCode).ToLowerInvariant()
        if ($null -ne $readiness.workOrder -and $trackName -in @('logic','presentation') -and
            $null -ne $readiness.workOrder.PSObject.Properties['trackPlans'] -and $null -ne $readiness.workOrder.trackPlans.PSObject.Properties[$trackName]) {
            $track = $readiness.workOrder.trackPlans.$trackName
            $openStages = @()
            if ($null -ne $track.PSObject.Properties['upwardValidation']) { $openStages = @($track.upwardValidation | Where-Object { $_.status -notin @('Passed','NotApplicable') -and $_.code -match '^E[1-7]$' } | Sort-Object { [int] ([string] $_.code).Substring(1) }) }
            if ($openStages.Count -gt 0) {
                $workReopenStages[[string] $workItem.workItemId] = [string] $openStages[0].code
                [int] ([string] $openStages[0].code).Substring(1)
            }
        }
    })
    if ($stageNumbers.Count -eq 0) {
        $stageNumbers = @($governingBatches | ForEach-Object { if ([string] $_.nextStageCode -match '^E([1-7])$') { [int] $Matches[1] } })
    }
    $earliestReopenStageCode = if ($stageNumbers.Count -gt 0) { "E$(($stageNumbers | Measure-Object -Minimum).Minimum)" } else { 'N/A' }
    $queueStateCode = if ([string] $extraction.registrationCode -ne 'Registered') {
        'PlannedRegistrationRequired'
    } elseif ($executableWiWork.Count -gt 0) {
        'ApprovedWorkItemExecutable'
    } elseif ($wiWorkReadiness.Count -gt 0) {
        'WorkItemBlocked'
    } elseif ($planningApprovalCode -ne 'Approved') {
        'PlanningApprovalRequired'
    } else {
        'ApprovedWorkItemRegistrationRequired'
    }
    $csharpEvidenceComponents = @()
    if ($evidenceComponentsByWorldInteractionId.ContainsKey($worldInteractionId)) {
        $componentList = $evidenceComponentsByWorldInteractionId[$worldInteractionId]
        $csharpEvidenceComponents = @($componentList.ToArray())
    }
    $csharpEvidenceBindingCode = if ([string] $extraction.registrationCode -ne 'Registered') {
        'NotApplicableUntilRegistered'
    } elseif ($csharpEvidenceComponents.Count -gt 0) {
        'Bound'
    } else {
        'RegisteredWithoutCSharpEvidenceBinding'
    }
    $csharpEvidenceStageCodes = @($csharpEvidenceComponents | ForEach-Object { $_.primaryEvidenceStage } | Where-Object { [string] $_ -match '^E([1-9]|10)$' } | Sort-Object { [int] ([string] $_).Substring(1) } -Unique)
    $csharpEvidenceSourcePaths = @($csharpEvidenceComponents | ForEach-Object { $_.sourcePath } | Where-Object { $_ } | Sort-Object -Unique)
    $blockerCodes = if ($wiWorkReadiness.Count -gt 0) {
        @($wiWorkReadiness | Where-Object { -not $_.canExecute } | ForEach-Object { @($_.blockerCodes) })
    } else { @($governingBatches | ForEach-Object { @($_.blockerCodes) }) }
    if ($extraction.registrationCode -eq 'Registered') {
        $blockerCodes = @($blockerCodes | Where-Object { $_ -notmatch 'WorldInteractionRegistrationRequired$' })
    }
    if ($wiWorkReadiness.Count -eq 0) { $blockerCodes += 'ApprovedWorkItemRegistrationRequired' }
    if ([string] $extraction.registrationCode -ne 'Registered') { $blockerCodes += 'WorldInteractionRegistrationRequired' }
    if ($planningApprovalCode -eq 'NotApproved') { $blockerCodes += 'ApprovedPlanningGateRequired' }
    if ($csharpEvidenceBindingCode -eq 'RegisteredWithoutCSharpEvidenceBinding') { $blockerCodes += 'CSharpEvidenceResponsibilityBindingRequired' }
    $blockerCodes = @($blockerCodes | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) } | Sort-Object -Unique)
    $planningRank = switch ($planningApprovalCode) {
        'ApprovedForWorkItem' { 0 }
        'WorkItemBlocked' { 1 }
        'Approved' { 2 }
        default { 3 }
    }
    [pscustomobject]@{
        worldInteractionId = $worldInteractionId
        registrationCode = [string] $extraction.registrationCode
        planningApprovalCode = $planningApprovalCode
        csharpEvidenceBindingCode = $csharpEvidenceBindingCode
        csharpEvidenceStageCodes = $csharpEvidenceStageCodes
        csharpEvidenceSourcePaths = $csharpEvidenceSourcePaths
        relatedBatchStableIds = @(Sort-OrdinalRegistrationIds @($relatedBatches.batchStableId))
        earliestReopenStageCode = $earliestReopenStageCode
        queueStateCode = $queueStateCode
        blockerCodes = $blockerCodes
        activeRank = if ($isActiveWorldInteraction) { 0 } else { 1 }
        planningRank = $planningRank
        registrationRank = if ([string] $extraction.registrationCode -eq 'Registered') { 0 } else { 1 }
        stageRank = if ($earliestReopenStageCode -match '^E([1-7])$') { [int] $Matches[1] } else { 99 }
    }
} | Sort-Object activeRank, planningRank, registrationRank, stageRank, worldInteractionId)
Require ($worldInteractionEvidenceCycleQueue.Count -eq @($normalizedWorldInteractionExtractions | Where-Object { $_.registrationCode -in @('Registered','PlannedCandidate') }).Count) 'WorldInteractionEvidenceCycleQueueCoverageInvalid'
Require (@($worldInteractionEvidenceCycleQueue | Where-Object { $_.worldInteractionId -eq [string] $routing.activeWorldInteractionId }).Count -eq 1) 'ActiveWorldInteractionEvidenceCycleQueueEntryInvalid'
$worldInteractionEvidenceCycleQueueStateCounts = @($worldInteractionEvidenceCycleQueue | Group-Object queueStateCode | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Count)" })
$worldInteractionCSharpEvidenceBindingCounts = @($worldInteractionEvidenceCycleQueue | Group-Object csharpEvidenceBindingCode | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Count)" })
$readyToDispatch = @($ordered | Where-Object {
    $question = $_
    $matchingWork = @($parallelReadiness | Where-Object {
        $readiness = $_
        if (-not $readiness.canExecute -or $null -eq $readiness.workOrder.PSObject.Properties['approvedQuestionScope']) { return $false }
        $relatedBatch = @($implementationBatches | Where-Object {
            $planned = if ($null -ne $_.PSObject.Properties['plannedWorldInteractionIds']) { @($_.plannedWorldInteractionIds) } else { @() }
            @($_.questionIds) -contains $question.questionId -and $null -ne $_.PSObject.Properties['playableUnitStableId'] -and $_.playableUnitStableId -eq $readiness.loopStableId -and
            @(@($_.worldInteractionIds) + $planned | ForEach-Object { Resolve-RegistrationWi ([string] $_) }) -contains $readiness.worldInteractionId
        })
        $relatedBatch.Count -gt 0 -and @($readiness.workOrder.approvedQuestionScope) -contains $question.questionId -and $worldInteractionsById.ContainsKey([string] $readiness.worldInteractionId)
    })
    $matchingWork.Count -gt 0 -and $question.decisionStatusCode -eq 'Confirmed' -and
    $question.checks.designBinding -eq 'Incorporated' -and $question.checks.implementation -in @('NotStarted', 'Partial') -and @($question.blockerCodes).Count -eq 0
})
$readyQuestionIds = if ($readyToDispatch.Count -eq 0) { @() } else { @($readyToDispatch | ForEach-Object { $_.questionId }) }
$notReady = @($ordered | Where-Object { $_.questionId -notin $readyQuestionIds })
$implementationStatusCounts = @($ordered | Group-Object implementationStatusCode | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Count)" })
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# $($catalog.output.titleKo)")
$lines.Add('')
$lines.Add("- ledger revision: ``$($catalog.revision)``")
$lines.Add("- question count: ``$($ordered.Count)`` - $($catalog.output.questionCountNoteKo)")
$lines.Add("- note: $($catalog.output.commonModuleMaturityNoteKo)")
$lines.Add("- evidence: $($catalog.output.evidenceNoteKo)")
$lines.Add('')
$lines.Add("## $($catalog.output.summaryTitleKo)")
$lines.Add('')
$lines.Add("- $($catalog.output.implementationStatusCountsLabelKo): ``$($implementationStatusCounts -join '; ')``")
$lines.Add('')
$lines.Add("## $($catalog.output.dispatchTitleKo)")
$lines.Add('')
$lines.Add("- $($catalog.output.readyToDispatchLabelKo): ``$($readyToDispatch.Count)``")
$lines.Add("- $($catalog.output.notReadyLabelKo): ``$($notReady.Count)``")
$lines.Add("- rule: $($catalog.output.dispatchRuleNoteKo)")
$lines.Add('')
$lines.Add("## $($catalog.output.worldInteractionExtractionTitleKo)")
$lines.Add('')
$lines.Add("- phase order: ``$(@($catalog.worldInteractionExtraction.phaseOrderCodes) -join ' -> ')``")
$lines.Add("- traversal: ``$($routing.questionTraversalDirectionCode)`` / purpose ``$($routing.questionTraversalPurposeCode)``")
$lines.Add("- question coverage: ``$($questionWiExtractions.Count) / $($ordered.Count)``")
$lines.Add("- extraction classification: ``$($worldInteractionExtractionCounts -join '; ')``")
$lines.Add("- extraction source: ``$($worldInteractionExtractionSourceCounts -join '; ')``")
$lines.Add("- normalized WI candidates: ``$($normalizedWorldInteractionExtractions.Count)``")
$lines.Add('- 원문 후보 ID는 추적용으로 보존한다. 상위 분류·결과 투영·특화 프로필은 실행 WI 대기열에서 제외하고 실제 등록 대상만 한 번 포함한다.')
$lines.Add('- 분류와 중복 판정: [WI 등록 결과](world-interaction-registration.md). 등록은 기획 승인·C# 구현·Evidence 승격과 다르다.')
$lines.Add("- boundary: $($catalog.output.worldInteractionExtractionBoundaryNoteKo) ``questionLevelRefinementRequired=$([bool] $catalog.worldInteractionExtraction.questionLevelRefinementRequired)``")
$lines.Add('')
$lines.Add('| ' + (@($catalog.output.worldInteractionExtractionHeadersKo) -join ' | ') + ' |')
$lines.Add('| --- | --- | ---: | --- |')
foreach ($extraction in $normalizedWorldInteractionExtractions) {
    $questionText = @($extraction.questionIds) -join ', '
    $lines.Add("| ``$($extraction.worldInteractionId)`` | ``$($extraction.registrationCode)`` | $(@($extraction.questionIds).Count) | $questionText |")
}
$lines.Add('')
$lines.Add("## $($catalog.output.worldInteractionEvidenceCycleQueueTitleKo)")
$lines.Add('')
$lines.Add("- candidates: ``$($worldInteractionEvidenceCycleQueue.Count)``")
$lines.Add("- selection basis: ``$(@($catalog.worldInteractionExtraction.evidenceCycleQueueSelectionBasisCodes) -join ' -> ')``")
$lines.Add("- deterministic tie-break: ``$($catalog.worldInteractionExtraction.evidenceCycleQueueTieBreakCode)``")
$lines.Add("- queue states: ``$($worldInteractionEvidenceCycleQueueStateCounts -join '; ')``")
$lines.Add("- C# E responsibility bindings: ``$($worldInteractionCSharpEvidenceBindingCounts -join '; ')``")
$lines.Add("- boundary: $($catalog.output.worldInteractionEvidenceCycleQueueBoundaryNoteKo)")
$lines.Add('')
$lines.Add('| ' + (@($catalog.output.worldInteractionEvidenceCycleQueueHeadersKo) -join ' | ') + ' |')
$lines.Add('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($queueEntry in $worldInteractionEvidenceCycleQueue) {
    $registrationAndPlanning = "``$($queueEntry.registrationCode)``<br>``$($queueEntry.planningApprovalCode)``"
    $csharpEvidenceText = "``$($queueEntry.csharpEvidenceBindingCode)``"
    if (@($queueEntry.csharpEvidenceStageCodes).Count -gt 0) {
        $csharpEvidenceText += "<br>``$(@($queueEntry.csharpEvidenceStageCodes) -join ',')`` / $(@($queueEntry.csharpEvidenceSourcePaths).Count) files"
    }
    $relatedBatchText = (@($queueEntry.relatedBatchStableIds) | ForEach-Object { "``$_``" }) -join '<br>'
    $blockerText = @($queueEntry.blockerCodes) -join '<br>'
    $lines.Add("| ``$($queueEntry.worldInteractionId)`` | $registrationAndPlanning | $csharpEvidenceText | $relatedBatchText | ``$($queueEntry.earliestReopenStageCode)`` | ``$($queueEntry.queueStateCode)`` | $(Escape-Cell $blockerText) |")
}
$lines.Add('')
$lines.Add("## $($catalog.output.executionRoutingTitleKo)")
$lines.Add('')
$lines.Add('대표 표시 항목과 실제 실행 승인 목록은 별개다. 아래 작업별 판정만 실행 가능 여부를 결정한다.')
$lines.Add('')
$lines.Add('| 작업 | PlayableUnit / WI | 궤적 / 재개 E / 승인 목표 | 실행 가능 | 차단 |')
$lines.Add('| --- | --- | --- | --- | --- |')
foreach ($readiness in @($parallelReadiness | Sort-Object workItemId)) {
    $item = @($parallelWorkItems | Where-Object { $_.workItemId -eq $readiness.workItemId })[0]
    $reopen = if ($workReopenStages.ContainsKey([string] $item.workItemId)) { $workReopenStages[[string] $item.workItemId] } else { 'N/A' }
    $lines.Add("| ``$($readiness.workItemId)`` | ``$($readiness.loopStableId)`` / ``$($readiness.worldInteractionId)`` | $($item.trackCode) / $reopen / $($item.targetEvidenceStageCode) | $($readiness.canExecute) | $(Escape-Cell (@($readiness.blockerCodes) -join '<br>')) |")
}
$lines.Add('')
$lines.Add("- question number role: ``$($routing.questionNumberRoleCode)``")
$lines.Add("- question traversal: ``$($routing.questionTraversalDirectionCode)`` for ``$($routing.questionTraversalPurposeCode)``")
$lines.Add("- selection: ``$($routing.selectionModeCode)``")
$lines.Add("- active: ``$($routing.activePlayableUnitStableId)`` / ``$($routing.activeWorldInteractionId)``")
$lines.Add("- impact review: ``$($routing.impactReviewDirectionCode)``")
$lines.Add("- initial assembly: ``$($routing.initialAssemblyDirectionCode)``")
$lines.Add("- implementation cycle: ``$($routing.implementationCycleCode)``")
$lines.Add("- logic / presentation cycle: ``$($routing.logicPresentationCycleCode)``")
$lines.Add("- current / dispatch ceiling: ``$($routing.currentIntegratedStageCode)`` / ``$($routing.dispatchCeilingStageCode)``")
$lines.Add("- dispatch state: ``$($routing.dispatchStateCode)`` / next ``$($routing.nextActionCode)``")
$lines.Add("- parked candidates: ``$(@($routing.parkedCandidateQuestionIds).Count)`` - $($routing.parkedCandidateReasonCode)")
$lines.Add('')
$lines.Add("## $($catalog.output.executionSelectionTitleKo)")
$lines.Add('')
$lines.Add("- decision: ``$executionSelectionStateCode``")
$lines.Add("- executable implementation batches: ``$($executableImplementationBatches.Count)``")
$lines.Add("- current owner: ``$($catalog.currentWorkBatchStableId)`` / ``$($routing.activePlayableUnitStableId)`` / ``$($routing.activeWorldInteractionId)``")
$lines.Add("- current / earliest resume E: ``$($currentWorkBatch.currentStageCode)`` / ``$earliestResumeStageCode``")
$lines.Add("- wait or next action: ``$($routing.nextActionCode)``")
$lines.Add("- rule: $($catalog.output.executionSelectionRuleNoteKo)")
$lines.Add('')
$lines.Add("## $($catalog.output.implementationBatchTitleKo)")
$lines.Add('')
$lines.Add("- current work batch: ``$($catalog.currentWorkBatchStableId)``")
$lines.Add("- active implementation batches: ``$($activeBatches.Count)`` (고정 WIP 상한 없음)")
$lines.Add("- active topic batch coverage: ``$($activeTopicQuestionIds.Count - $unbatchedActiveTopicQuestionIds.Count) / $($activeTopicQuestionIds.Count)``")
$lines.Add("- fully partitioned topic coverage: ``$($fullyPartitionedTopicCoverage -join '; ')``")
$lines.Add('')
$lines.Add('| ' + (@($catalog.output.implementationBatchHeadersKo) -join ' | ') + ' |')
$lines.Add('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($batch in $implementationBatches) {
    $worldInteractionText = (@($batch.worldInteractionIds) | ForEach-Object { "``$_``" }) -join '<br>'
    $plannedWorldInteractionIds = @()
    if ($null -ne $batch.PSObject.Properties['plannedWorldInteractionIds']) {
        $plannedWorldInteractionIds = @($batch.plannedWorldInteractionIds)
    }
    $plannedWorldInteractionText = ($plannedWorldInteractionIds | ForEach-Object { "``planned:$_``" }) -join '<br>'
    if (-not [string]::IsNullOrWhiteSpace($plannedWorldInteractionText)) {
        if ([string]::IsNullOrWhiteSpace($worldInteractionText)) {
            $worldInteractionText = $plannedWorldInteractionText
        } else {
            $worldInteractionText += "<br>$plannedWorldInteractionText"
        }
    }
    $batchEvidenceSubject = if ($null -ne $batch.PSObject.Properties['playableUnitStableId'] -and -not [string]::IsNullOrWhiteSpace([string] $batch.playableUnitStableId)) { [string] $batch.playableUnitStableId } else { [string] $batch.commonModuleStableId }
    $subjects = "``$batchEvidenceSubject``"
    if (-not [string]::IsNullOrWhiteSpace($worldInteractionText)) {
        $subjects += "<br>$worldInteractionText"
    }
    $questionsText = @($batch.questionIds) -join ', '
    $blockersText = @($batch.blockerCodes) -join '<br>'
    $lines.Add("| ``$($batch.batchStableId)``<br>$(Escape-Cell $batch.titleKo) | ``$($batch.topicCode)`` | $(Escape-Cell $questionsText) | $subjects | ``$($batch.currentStageCode)`` / ``$($batch.nextStageCode)`` | ``$($batch.executionStateCode)`` | $(Escape-Cell $blockersText) |")
}
$lines.Add('')
$lines.Add('| ' + (@($catalog.output.tableHeadersKo) -join ' | ') + ' |')
$lines.Add('| --- | --- | --- | --- | --- | --- | --- | --- | --- |')
foreach ($item in $ordered) {
    $links = (@($item.playableLoopRefs) + @($item.plannedPlayableLoopRefs | ForEach-Object { "planned:$_" }) + @($item.worldInteractionRefs) + @($item.plannedWorldInteractionRefs | ForEach-Object { "planned:$_" }) + @($item.hCapabilityRefs) + @($item.commonModuleRefs)) -join '<br>'
    $blockers = @($item.blockerCodes) -join '<br>'
    $labels = $catalog.output.checkLabelsKo
    $checkText = "$($labels.planningRecord):$($item.checks.planningRecord)<br>$($labels.designBinding):$($item.checks.designBinding)<br>$($labels.implementation):$($item.checks.implementation)<br>$($labels.automatedVerification):$($item.checks.automatedVerification)<br>$($labels.runtimeVerification):$($item.checks.runtimeVerification)<br>$($labels.evidenceBinding):$($item.checks.evidenceBinding)"
    $lines.Add("| ``$($item.questionId)`` | $(Escape-Cell $item.topicTitleKo)<br>``$($item.depthCode)`` | ``$($item.decisionStatusCode)`` | ``$($item.implementationKindCode)``<br>``$($item.implementationStatusCode)`` | $checkText | $(Escape-Cell $links) | ``$($item.logicStageCode)`` / ``$($item.presentationStageCode)`` / ``$($item.integratedStageCode)`` | ``$($item.nextTargetStageCode)`` | $(Escape-Cell $blockers)<br>$(Escape-Cell $item.evidenceRefs[0]) |")
}
Add-AdditionalInquiryProjection $catalog $repositoryRoot $lines $worldInteractionsById $loopsById $parallelReadiness
$content = ($lines -join "`n") + "`n"
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq 'Write') {
    Write-DeterministicTextIfChanged -Path $resolvedOutput -Content $content | Out-Null
} else {
    Require (Test-Path -LiteralPath $resolvedOutput) 'GeneratedOutputMissing'
    $actual = Get-Content -LiteralPath $resolvedOutput -Raw -Encoding UTF8
    Require ($actual -eq $content) 'GeneratedOutputMismatch'
}

$statusCounts = $ordered | Group-Object implementationStatusCode | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Count)" }
Write-Output ("PlayableLoopInquiryImplementationScopeValid:Questions={0};{1};Revision={2}" -f $ordered.Count, ($statusCounts -join ';'), $catalog.revision)
