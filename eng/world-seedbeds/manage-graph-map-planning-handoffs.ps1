[CmdletBinding()]
param(
    [ValidateSet('Check', 'Write')]
    [string] $Mode = 'Check',
    [string] $LedgerPath = 'eng/world-seedbeds/graph-map-planning-handoffs.json',
    [string] $JsonOutputPath = 'eng/world-seedbeds/generated/graph-map-planning-handoffs.v1.json',
    [string] $MarkdownOutputPath = 'docs/AI/generated/graph-map-planning-handoffs.md'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$utf8 = [Text.UTF8Encoding]::new($false)

function Require([bool] $condition, [string] $code) {
    if (-not $condition) { throw "GraphMapHandoffInvalid:$code" }
}

function Require-Text([object] $value, [string] $code) {
    Require ($null -ne $value -and -not [string]::IsNullOrWhiteSpace([string] $value)) $code
}

function Resolve-RepoChild([string] $relativePath, [string] $code) {
    Require-Text $relativePath "$code`:Empty"
    Require (-not [IO.Path]::IsPathRooted($relativePath)) "$code`:Rooted"
    Require (-not (@($relativePath -split '[/\\]') -contains '..')) "$code`:Traversal"
    $normalizedRoot = [IO.Path]::GetFullPath($repositoryRoot).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    $candidate = [IO.Path]::GetFullPath((Join-Path $normalizedRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)))
    $prefix = $normalizedRoot + [IO.Path]::DirectorySeparatorChar
    Require ($candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) "$code`:OutsideRoot"
    return $candidate
}

function Read-Json([string] $relativePath, [string] $code) {
    $path = Resolve-RepoChild $relativePath $code
    Require (Test-Path -LiteralPath $path -PathType Leaf) "$code`:Missing:$relativePath"
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function File-Hash([string] $relativePath, [string] $code) {
    $path = Resolve-RepoChild $relativePath $code
    Require (Test-Path -LiteralPath $path -PathType Leaf) "$code`:Missing:$relativePath"
    return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Require-Hash([string] $relativePath, [object] $expected, [string] $code) {
    Require-Text $expected "$code`:ExpectedEmpty"
    Require ([string] $expected -match '^[0-9a-fA-F]{64}$') "$code`:ExpectedFormat"
    $actual = File-Hash $relativePath $code
    Require ($actual -eq ([string] $expected).ToLowerInvariant()) "$code`:HashMismatch"
    return $actual
}

function Require-Unique([object[]] $values, [scriptblock] $selector, [string] $code) {
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($value in @($values)) {
        $key = [string] (& $selector $value)
        Require-Text $key "$code`:Empty"
        Require ($seen.Add($key)) "$code`:$key"
    }
}

function Stable-Json([object] $value) {
    return (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n") + "`n"
}

function Normalize-Text([string] $value) {
    return (($value -replace "`r`n", "`n").TrimEnd()) + "`n"
}

function Escape-Cell([object] $value) {
    if ($null -eq $value) { return '' }
    return ([string] $value).Replace('|', '\|').Replace("`r", '').Replace("`n", '<br>')
}

$ledger = Read-Json $LedgerPath 'Ledger'
Require ([string] $ledger.schemaVersion -eq 'mirror-graph-map-planning-handoffs.v1') 'SchemaVersion'
Require-Text $ledger.revision 'LedgerRevision'
Require ([string] $ledger.generatedAtRuleCode -eq 'DeterministicNoWallClock') 'GeneratedAtRule'

$boundary = $ledger.ownershipBoundary
Require ([string] $boundary.planningOwnerCode -eq 'Planning') 'PlanningOwner'
Require ([string] $boundary.graphMapOwnerCode -eq 'GraphMapWorkstream') 'GraphMapOwner'
Require ([string] $boundary.developmentIntegrationOwnerCode -eq 'Development') 'DevelopmentOwner'
Require (-not [bool] $boundary.planningEditsGraphMap) 'PlanningEditsGraphMap'
Require (-not [bool] $boundary.graphMapInventsPlanningMeaning) 'GraphMapInventsPlanningMeaning'
Require (-not [bool] $boundary.intermediateWaitRequired) 'IntermediateWaitRequired'
Require (-not [bool] $boundary.automaticUnityExecution) 'AutomaticUnityExecution'
Require (-not [bool] $boundary.automaticEvidencePromotion) 'AutomaticEvidencePromotion'

$decisionText = Get-Content -LiteralPath (Resolve-RepoChild 'docs/AI/DECISIONS.md' 'DecisionCatalog') -Raw -Encoding UTF8
$decisionIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($match in [regex]::Matches($decisionText, '(?m)^## (D-\d{3})\b')) { $null = $decisionIds.Add($match.Groups[1].Value) }
$wiCatalog = Read-Json 'eng/execution-ledgers/world-interactions.json' 'WorldInteractionCatalog'
$wiIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($wi in @($wiCatalog.items)) { $null = $wiIds.Add([string] $wi.id) }

$allowedStatuses = @('Draft', 'ApprovedForHandoff', 'AcceptedByGraphMap', 'Integrated', 'Blocked', 'NoImpact', 'Superseded')
$terminalStatuses = @('Integrated', 'Blocked', 'NoImpact')
$allowedImpacts = @('NoImpact', 'UpdateExisting', 'CreateSubgraph', 'CreateGraphMap')
$allowedLevels = @('Federation', 'Level1', 'Level2', 'Level3')
$requiredContext = @('time', 'place', 'player', 'target', 'method', 'result', 'nextChoices')
$items = @($ledger.items)
Require ($items.Count -gt 0) 'ItemsEmpty'
Require-Unique $items { param($x) $x.handoffId } 'HandoffDuplicate'

$snapshots = [Collections.Generic.List[object]]::new()
foreach ($item in $items) {
    $id = [string] $item.handoffId
    Require-Text $id 'HandoffId'
    $status = [string] $item.statusCode
    $impact = [string] $item.impactCode
    Require ($allowedStatuses -contains $status) "Status:$id"
    Require ($allowedImpacts -contains $impact) "Impact:$id"

    $source = $item.planningSource
    Require-Text $source.revisionCode "PlanningRevision:$id"
    Require ([string] $source.approvalStateCode -eq 'ApprovedForHandoff') "PlanningApproval:$id"
    $sourceHash = Require-Hash ([string] $source.path) $source.expectedSha256 "PlanningSource:$id"
    foreach ($decisionId in @($source.sourceDecisionIds)) {
        Require ($decisionIds.Contains([string] $decisionId)) "DecisionUnknown:$id`:$decisionId"
    }
    foreach ($wiId in @($source.sourceWorldInteractionIds)) {
        Require ($wiIds.Contains([string] $wiId)) "WorldInteractionUnknown:$id`:$wiId"
    }
    foreach ($field in $requiredContext) {
        Require ($null -ne $source.contextRefs.PSObject.Properties[$field]) "ContextMissing:$id`:$field"
        Require-Text $source.contextRefs.$field "ContextEmpty:$id`:$field"
    }
    Require (@($source.exclusionRefs).Count -gt 0) "ExclusionsEmpty:$id"

    $request = $item.request
    $requestedLevels = @($request.requestedLevelCodes)
    Require-Unique $requestedLevels { param($x) [string] $x } "RequestedLevelDuplicate:$id"
    if ($impact -eq 'NoImpact') {
        Require ($requestedLevels.Count -eq 0) "NoImpactHasLevels:$id"
    }
    else {
        Require ($requestedLevels.Count -gt 0) "RequestedLevelsEmpty:$id"
        foreach ($level in $requestedLevels) { Require ($allowedLevels -contains [string] $level) "RequestedLevelUnknown:$id`:$level" }
        Require-Text $request.targetGraphMapStableId "TargetStableId:$id"
        Require-Text $request.targetGraphMapRevision "TargetRevision:$id"
        if ($requestedLevels -contains 'Level1') { Require-Text $request.level1Intent "Level1Intent:$id" }
        if ($requestedLevels -contains 'Level2') { Require-Text $request.level2Intent "Level2Intent:$id" }
        if ($requestedLevels -contains 'Level3') { Require-Text $request.level3Intent "Level3Intent:$id" }
    }

    $result = $item.result
    $return = $item.returnToPlanning
    Require-Text $return.summary "ReturnSummary:$id"
    Require-Text $return.nextPlanningFocus "ReturnNextPlanning:$id"
    Require ([string] $return.terminalResultCode -eq $status) "ReturnStatusMismatch:$id"
    if ($terminalStatuses -contains $status) {
        if ($status -eq 'Blocked') {
            Require (@($result.blockerItems).Count -gt 0) "BlockedWithoutBlocker:$id"
        }
        if ($status -eq 'NoImpact') {
            Require ($impact -eq 'NoImpact') "NoImpactMismatch:$id"
            Require (-not [bool] $return.planningQuestionRequired -or @($result.blockerItems).Count -gt 0) "NoImpactQuestionWithoutBlocker:$id"
        }
    }
    else {
        Require (-not ($terminalStatuses -contains [string] $return.terminalResultCode)) "NonTerminalHasTerminalReturn:$id"
    }

    $planSnapshot = $null
    if ($status -eq 'Integrated') {
        Require ($impact -ne 'NoImpact') "IntegratedNoImpact:$id"
        Require (@($result.mappedRefs).Count -gt 0) "IntegratedMappedRefsEmpty:$id"
        Require (@($result.verificationCodes).Count -gt 0) "IntegratedVerificationEmpty:$id"
        $planHash = Require-Hash ([string] $result.graphMapPlanRef) $result.graphMapPlanExpectedSha256 "GraphMapPlan:$id"
        $plan = Read-Json ([string] $result.graphMapPlanRef) "GraphMapPlan:$id"
        Require ([string] $plan.graphMapStableId -eq [string] $request.targetGraphMapStableId) "TargetStableIdMismatch:$id"
        Require ([string] $plan.revision -eq [string] $request.targetGraphMapRevision) "TargetRevisionMismatch:$id"

        $generatedHash = Require-Hash ([string] $result.generatedGraphMapRef) $result.generatedGraphMapExpectedSha256 "GeneratedGraphMap:$id"
        $generated = Read-Json ([string] $result.generatedGraphMapRef) "GeneratedGraphMap:$id"
        Require ([string] $generated.sourcePlanRef -eq [string] $result.graphMapPlanRef) "GeneratedSourceRefMismatch:$id"
        Require ([string] $generated.sourcePlanHashSha256 -eq $planHash) "GeneratedSourceHashMismatch:$id"
        Require ([string] $generated.plan.graphMapStableId -eq [string] $request.targetGraphMapStableId) "GeneratedStableIdMismatch:$id"
        Require ([string] $generated.plan.revision -eq [string] $request.targetGraphMapRevision) "GeneratedRevisionMismatch:$id"
        $reportHash = Require-Hash ([string] $result.reportRef) $result.reportExpectedSha256 "Report:$id"
        $planSnapshot = [ordered]@{
            stableId = [string] $plan.graphMapStableId
            revision = [string] $plan.revision
            planRef = [string] $result.graphMapPlanRef
            planSha256 = $planHash
            generatedRef = [string] $result.generatedGraphMapRef
            generatedSha256 = $generatedHash
            reportRef = [string] $result.reportRef
            reportSha256 = $reportHash
        }
    }

    $evidence = $item.evidenceBoundary
    foreach ($property in @('unitySceneChanged', 'prefabApplied', 'runtimeExecuted', 'gameViewCaptured', 'actualTraversalVerified', 'evidencePromoted', 'operationalStateChanged')) {
        Require ($null -ne $evidence.PSObject.Properties[$property]) "EvidenceFieldMissing:$id`:$property"
        Require (-not [bool] $evidence.$property) "EvidenceBoundaryRaised:$id`:$property"
    }

    $snapshots.Add([ordered]@{
        handoffId = $id
        statusCode = $status
        impactCode = $impact
        planningSource = [ordered]@{
            path = [string] $source.path
            revisionCode = [string] $source.revisionCode
            sha256 = $sourceHash
            sourceDecisionIds = @($source.sourceDecisionIds)
            sourceWorldInteractionIds = @($source.sourceWorldInteractionIds)
            contextRefs = $source.contextRefs
            exclusionRefs = @($source.exclusionRefs)
        }
        request = $request
        graphMapResult = $planSnapshot
        mappedRefs = @($result.mappedRefs)
        unmappedItems = @($result.unmappedItems)
        blockerItems = @($result.blockerItems)
        verificationCodes = @($result.verificationCodes)
        returnToPlanning = $return
        evidenceBoundary = $evidence
    })
}

$counts = [ordered]@{
    total = $items.Count
    draft = @($items | Where-Object statusCode -eq 'Draft').Count
    approvedForHandoff = @($items | Where-Object statusCode -eq 'ApprovedForHandoff').Count
    acceptedByGraphMap = @($items | Where-Object statusCode -eq 'AcceptedByGraphMap').Count
    integrated = @($items | Where-Object statusCode -eq 'Integrated').Count
    blocked = @($items | Where-Object statusCode -eq 'Blocked').Count
    noImpact = @($items | Where-Object statusCode -eq 'NoImpact').Count
    superseded = @($items | Where-Object statusCode -eq 'Superseded').Count
}

$output = [ordered]@{
    schemaVersion = 'mirror-graph-map-planning-handoff-output.v1'
    revision = 'mirror-graph-map-planning-handoff-output.r1'
    generatedAtRuleCode = 'DeterministicNoWallClock'
    sourceLedgerRef = $LedgerPath
    sourceLedgerSha256 = File-Hash $LedgerPath 'Ledger'
    counts = $counts
    ownershipBoundary = $boundary
    items = @($snapshots)
}
$json = Stable-Json $output

$lines = [Collections.Generic.List[string]]::new()
$lines.Add('# Graph Map 기획 인계 상태')
$lines.Add('')
$lines.Add('> 이 문서는 `eng/world-seedbeds/graph-map-planning-handoffs.json`에서 생성한다. 직접 수정하지 않는다.')
$lines.Add('')
$lines.Add("- 원장 판본: $($ledger.revision)")
$lines.Add("- 전체: $($counts.total) / 반영: $($counts.integrated) / 차단: $($counts.blocked) / 영향 없음: $($counts.noImpact)")
$lines.Add('- 기획은 승인 판본과 인계만 소유하고, Graph Map 작업은 레벨 1·2·3 반영과 최종 반환을 소유한다.')
$lines.Add('- 이 상태는 Unity Scene·Prefab·실제 입력·Game View·E 승격을 뜻하지 않는다.')
$lines.Add('')
$lines.Add('| 인계 | 상태 | 영향 | 기획 판본 | 대상 Graph Map | 요청 레벨 | 기획 질문 |')
$lines.Add('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($snapshot in $snapshots) {
    $target = if ($null -eq $snapshot.graphMapResult) { Escape-Cell $snapshot.request.targetGraphMapStableId } else { "$(Escape-Cell $snapshot.graphMapResult.stableId)<br>$(Escape-Cell $snapshot.graphMapResult.revision)" }
    $levels = (@($snapshot.request.requestedLevelCodes) -join ', ')
    $question = if ([bool] $snapshot.returnToPlanning.planningQuestionRequired) { '필요' } else { '없음' }
    $lines.Add("| $(Escape-Cell $snapshot.handoffId) | $($snapshot.statusCode) | $($snapshot.impactCode) | $(Escape-Cell $snapshot.planningSource.revisionCode) | $target | $(Escape-Cell $levels) | $question |")
}

foreach ($snapshot in $snapshots) {
    $lines.Add('')
    $lines.Add("## $($snapshot.handoffId)")
    $lines.Add('')
    $lines.Add("- 기획: [$($snapshot.planningSource.path)](../../../$($snapshot.planningSource.path)) / $($snapshot.planningSource.revisionCode) / SHA-256 $($snapshot.planningSource.sha256)")
    $lines.Add("- 상태·영향: $($snapshot.statusCode) / $($snapshot.impactCode)")
    $lines.Add("- 반영: $(Escape-Cell (@($snapshot.mappedRefs) -join ', '))")
    $lines.Add("- 미반영: $(if (@($snapshot.unmappedItems).Count -eq 0) { '없음' } else { Escape-Cell (@($snapshot.unmappedItems) -join '; ') })")
    $lines.Add("- 차단: $(if (@($snapshot.blockerItems).Count -eq 0) { '없음' } else { Escape-Cell (@($snapshot.blockerItems) -join '; ') })")
    $lines.Add("- 검증: $(Escape-Cell (@($snapshot.verificationCodes) -join ', '))")
    $lines.Add("- 기획 반환: $($snapshot.returnToPlanning.summary)")
    $lines.Add("- 다음 기획 초점: $($snapshot.returnToPlanning.nextPlanningFocus)")
    if ($null -ne $snapshot.graphMapResult) {
        $lines.Add("- 결과 Graph Map: [$($snapshot.graphMapResult.planRef)](../../../$($snapshot.graphMapResult.planRef)) / $($snapshot.graphMapResult.revision) / SHA-256 $($snapshot.graphMapResult.planSha256)")
        $lines.Add("- 결과 보고: [$($snapshot.graphMapResult.reportRef)](../../../$($snapshot.graphMapResult.reportRef))")
    }
}
$markdown = Normalize-Text ($lines -join "`n")

$jsonPath = Resolve-RepoChild $JsonOutputPath 'JsonOutput'
$markdownPath = Resolve-RepoChild $MarkdownOutputPath 'MarkdownOutput'
if ($Mode -eq 'Write') {
    $null = New-Item -ItemType Directory -Force -Path (Split-Path -Parent $jsonPath)
    $null = New-Item -ItemType Directory -Force -Path (Split-Path -Parent $markdownPath)
    [IO.File]::WriteAllText($jsonPath, $json, $utf8)
    [IO.File]::WriteAllText($markdownPath, $markdown, $utf8)
    Write-Output "GraphMapPlanningHandoff Write passed: items=$($counts.total), integrated=$($counts.integrated), blocked=$($counts.blocked), noImpact=$($counts.noImpact)"
    exit 0
}

Require (Test-Path -LiteralPath $jsonPath -PathType Leaf) 'GeneratedJsonMissing'
Require (Test-Path -LiteralPath $markdownPath -PathType Leaf) 'GeneratedMarkdownMissing'
$existingJson = Normalize-Text (Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8)
$existingMarkdown = Normalize-Text (Get-Content -LiteralPath $markdownPath -Raw -Encoding UTF8)
Require ($existingJson -eq $json) 'GeneratedJsonStale'
Require ($existingMarkdown -eq $markdown) 'GeneratedMarkdownStale'
Write-Output "GraphMapPlanningHandoff Check passed: items=$($counts.total), integrated=$($counts.integrated), blocked=$($counts.blocked), noImpact=$($counts.noImpact)"
