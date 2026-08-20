param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$knowledgeRoot = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory"
$catalogPath = Join-Path $knowledgeRoot "catalog.v2.json"
$priorityPath = Join-Path $knowledgeRoot "h2-composition-priorities.v1.json"
$recipePath = Join-Path $knowledgeRoot "h2-composition-recipes.v1.json"
$worldInteractionPath = Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json"
$outputPath = Join-Path $knowledgeRoot "generated/h2-composition-plans.v1.json"
$documentPath = Join-Path $repositoryRoot "docs/AI/generated/h2-composition-plans.md"

function Read-Json([string] $Path) {
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Require([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function ConvertTo-StableJson([object] $Value) {
    return ($Value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n"
}

function Get-TextSha256([string] $Text) {
    $bytes = [Text.Encoding]::UTF8.GetBytes($Text)
    $hash = [Security.Cryptography.SHA256]::HashData($bytes)
    return ([Convert]::ToHexString($hash)).ToLowerInvariant()
}

function Write-TextIfChanged([string] $Path, [string] $Text) {
    $normalized = $Text.TrimEnd() + "`n"
    if ((Test-Path -LiteralPath $Path) -and ((Get-Content -LiteralPath $Path -Raw -Encoding UTF8) -ceq $normalized)) { return }
    $directory = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $directory)) { [void] (New-Item -ItemType Directory -Path $directory) }
    [IO.File]::WriteAllText($Path, $normalized, [Text.UTF8Encoding]::new($false))
}

$catalog = Read-Json $catalogPath
$priorities = Read-Json $priorityPath
$recipes = Read-Json $recipePath
$worldInteractions = Read-Json $worldInteractionPath

Require ([string] $recipes.coordinateSpaceCode -eq "LocalMeters") "H2CompositionCoordinateSpaceMustBeLocalMeters"
Require (-not ([string] $recipes.authorityBoundary -match "실제.*권위.*갖는다")) "H2CompositionAuthorityBoundaryInvalid"
Require (@($recipes.recipes).Count -eq 6) "H2CompositionP1P2P3RecipeCountMustBe6"

$h1ById = @{}
foreach ($reference in @($catalog.h1DefinitionRefs)) {
    $definitionPath = Join-Path $knowledgeRoot ([string] $reference.definitionPath)
    $h1ById[[string] $reference.stableId] = Read-Json $definitionPath
}
$h2ById = @{}
foreach ($reference in @($catalog.h2DefinitionRefs)) {
    $definitionPath = Join-Path $knowledgeRoot ([string] $reference.definitionPath)
    $h2ById[[string] $reference.stableId] = Read-Json $definitionPath
}
$h3ById = @{}
foreach ($reference in @($catalog.h3DefinitionRefs)) {
    $definitionPath = Join-Path $knowledgeRoot ([string] $reference.definitionPath)
    $h3ById[[string] $reference.stableId] = Read-Json $definitionPath
}
$wiById = @{}
foreach ($item in @($worldInteractions.items)) { $wiById[[string] $item.id] = $item }
$priorityByCandidate = @{}
foreach ($candidate in @($priorities.candidates)) { $priorityByCandidate[[string] $candidate.candidateRef] = $candidate }

$plans = @()
$recipesByTarget = @{}
foreach ($recipe in @($recipes.recipes | Sort-Object targetKnowledgeRef)) {
    $targetId = [string] $recipe.targetKnowledgeRef
    Require ($h2ById.ContainsKey($targetId)) "H2CompositionTargetUnknown:$targetId"
    Require ($priorityByCandidate.ContainsKey($targetId)) "H2CompositionPriorityUnknown:$targetId"
    Require (@("P1", "P2", "P3") -contains [string] $recipe.priorityCode) "H2CompositionPriorityNotEnabled:$targetId"
    Require ([string] $priorityByCandidate[$targetId].priorityCode -eq [string] $recipe.priorityCode) "H2CompositionPriorityMismatch:$targetId"

    $target = $h2ById[$targetId]
    $recipesByTarget[$targetId] = $recipe
    $requiredH1 = @($target.requiredH1Refs | Sort-Object -Unique)
    $recipeH1 = @($recipe.nodes | Where-Object required | ForEach-Object h1Ref | Sort-Object -Unique)
    Require (($requiredH1 -join "`n") -ceq ($recipeH1 -join "`n")) "H2CompositionRequiredH1Mismatch:$targetId"

    $nodeIds = @($recipe.nodes.localNodeId)
    Require (@($nodeIds | Sort-Object -Unique).Count -eq $nodeIds.Count) "H2CompositionNodeDuplicate:$targetId"
    foreach ($node in @($recipe.nodes)) {
        Require ($h1ById.ContainsKey([string] $node.h1Ref)) "H2CompositionH1Unknown:${targetId}:$($node.h1Ref)"
        Require ([Math]::Abs([double] $node.localX) -le ([double] $recipe.referenceSizeMeters.width / 2)) "H2CompositionNodeOutsideWidth:${targetId}:$($node.localNodeId)"
        Require ([Math]::Abs([double] $node.localZ) -le ([double] $recipe.referenceSizeMeters.depth / 2)) "H2CompositionNodeOutsideDepth:${targetId}:$($node.localNodeId)"
        if ([string] $recipe.priorityCode -eq "P1") {
            Require (-not [string]::IsNullOrWhiteSpace([string] $node.playRoleCode)) "H2CompositionPlayRoleMissing:${targetId}:$($node.localNodeId)"
            Require (@($node.wiIds).Count -gt 0) "H2CompositionWiMissing:${targetId}:$($node.localNodeId)"
            Require (@($node.planningCapacities).Count -gt 0) "H2CompositionCapacityMissing:${targetId}:$($node.localNodeId)"
            $h1 = $h1ById[[string] $node.h1Ref]
            $h1HasGameContext = @($h1.wiIds).Count -gt 0 -or @($h1.anticipatedGameplayCodes).Count -gt 0
            $h1HasSpatialMeaning = @($h1.spatialRoleCodes).Count -gt 0
            $h1HasExpressionSource = @($h1.sourcePackCodes).Count -gt 0 -or @($h1.grammarSetRefs).Count -gt 0
            Require ($h1HasGameContext -and $h1HasSpatialMeaning -and $h1HasExpressionSource) "H2CompositionH1NotRecognized:${targetId}:$($node.h1Ref)"
            foreach ($wiId in @($node.wiIds)) {
                Require ($wiById.ContainsKey([string] $wiId)) "H2CompositionWiUnknown:${targetId}:$wiId"
                Require (@($h1.wiIds) -contains [string] $wiId) "H2CompositionH1WiMismatch:${targetId}:$($node.h1Ref):$wiId"
            }
            foreach ($capacity in @($node.planningCapacities)) {
                Require ([double] $capacity.quantity -gt 0) "H2CompositionCapacityQuantityInvalid:${targetId}:$($node.localNodeId)"
                Require (-not [string]::IsNullOrWhiteSpace([string] $capacity.unitCode)) "H2CompositionCapacityUnitMissing:${targetId}:$($node.localNodeId)"
                Require (@($h1.capacityConceptCodes) -contains [string] $capacity.capacityCode) "H2CompositionCapacityNotDeclared:${targetId}:$($node.h1Ref):$($capacity.capacityCode)"
            }
        }
    }
    foreach ($edge in @($recipe.edges)) {
        Require ($nodeIds -contains [string] $edge.fromNodeId) "H2CompositionEdgeFromUnknown:${targetId}:$($edge.localEdgeId)"
        Require ($nodeIds -contains [string] $edge.toNodeId) "H2CompositionEdgeToUnknown:${targetId}:$($edge.localEdgeId)"
    }

    $targetConnectors = @($target.connectorRoleCodes | Sort-Object -Unique)
    $recipeConnectors = @($recipe.externalConnectors.roleCode | Sort-Object -Unique)
    Require (($targetConnectors -join "`n") -ceq ($recipeConnectors -join "`n")) "H2CompositionConnectorMismatch:$targetId"
    foreach ($connector in @($recipe.externalConnectors)) {
        Require ($nodeIds -contains [string] $connector.attachedNodeId) "H2CompositionConnectorNodeUnknown:${targetId}:$($connector.connectorId)"
    }

    $input = [pscustomobject][ordered]@{
        recipeRevision = [string] $recipes.revision
        targetKnowledgeRef = $targetId
        targetRevision = [int] $target.revision
        targetTopologyCode = [string] $target.topologyCode
        recipe = $recipe
    }
    $plans += [pscustomobject][ordered]@{
        planId = "h2-plan:$($targetId.Substring('h2-candidate:'.Length))"
        targetKnowledgeRef = $targetId
        title = [string] $target.title
        priorityCode = [string] $recipe.priorityCode
        topologyCode = [string] $target.topologyCode
        coordinateSpaceCode = [string] $recipes.coordinateSpaceCode
        referenceSizeMeters = $recipe.referenceSizeMeters
        nodes = @($recipe.nodes)
        edges = @($recipe.edges)
        externalConnectors = @($recipe.externalConnectors)
        designStateCode = "ReadyForPlanningReview"
        authorityStateCode = "DesignCandidateOnly"
        derivationInputHashSha256 = Get-TextSha256 (ConvertTo-StableJson $input)
    }
}

Require (@($recipes.referencePlayLoops).Count -eq 1) "NatureReferencePlayLoopCountMustBe1"
$naturePlayLoop = @($recipes.referencePlayLoops)[0]
Require ([string] $naturePlayLoop.playLoopId -eq "reference-play:nature-threat-recovery.v1") "NatureReferencePlayLoopIdInvalid"
Require ($h3ById.ContainsKey([string] $naturePlayLoop.h3Ref)) "NatureReferencePlayH3Unknown"
Require ([string] $h3ById[[string] $naturePlayLoop.h3Ref].knowledgeStateCode -eq "CandidateForReview") "NatureReferencePlayH3NotReviewReady"
Require (@($naturePlayLoop.branches).Count -eq 2) "NatureReferencePlayBranchCountInvalid"
foreach ($branch in @($naturePlayLoop.branches)) {
    foreach ($wiId in @($branch.wiSequence)) { Require ($wiById.ContainsKey([string] $wiId)) "NatureReferencePlayWiUnknown:$wiId" }
    foreach ($h1Ref in @($branch.h1Sequence)) { Require ($h1ById.ContainsKey([string] $h1Ref)) "NatureReferencePlayH1Unknown:$h1Ref" }
}
Require ((@($naturePlayLoop.branches | Where-Object branchCode -eq "RetreatAndRecover").wiSequence -join ",") -eq "WI-NATURE-01,WI-NATURE-02,WI-NATURE-04") "NatureReferencePlayRetreatBranchInvalid"
Require ((@($naturePlayLoop.branches | Where-Object branchCode -eq "RestoreAndRecover").wiSequence -join ",") -eq "WI-NATURE-01,WI-NATURE-03,WI-NATURE-04") "NatureReferencePlayRestoreBranchInvalid"
foreach ($handoff in @($naturePlayLoop.h2Handoffs)) {
    Require ($recipesByTarget.ContainsKey([string] $handoff.fromH2Ref)) "NatureReferencePlayHandoffFromUnknown:$($handoff.relationCode)"
    Require ($recipesByTarget.ContainsKey([string] $handoff.toH2Ref)) "NatureReferencePlayHandoffToUnknown:$($handoff.relationCode)"
    Require (@($recipesByTarget[[string] $handoff.fromH2Ref].externalConnectors.roleCode) -contains [string] $handoff.fromConnectorRoleCode) "NatureReferencePlayHandoffFromConnectorUnknown:$($handoff.relationCode)"
    Require (@($recipesByTarget[[string] $handoff.toH2Ref].externalConnectors.roleCode) -contains [string] $handoff.toConnectorRoleCode) "NatureReferencePlayHandoffToConnectorUnknown:$($handoff.relationCode)"
}
Require (@($naturePlayLoop.h2Handoffs.relationCode) -contains "SafeCoreReentry") "NatureReferencePlaySafeCoreReentryMissing"
Require ([string] $naturePlayLoop.completionContext.exitConnectorRoleCode -eq "RestoredRouteOutput") "NatureReferencePlayExplorationExitInvalid"
Require ([string] $naturePlayLoop.authorityStateCode -eq "DesignCandidateOnly") "NatureReferencePlayAuthorityInvalid"
Require ([string] $naturePlayLoop.evidenceStageCode -eq "E1") "NatureReferencePlayEvidenceStageInvalid"

$output = [pscustomobject][ordered]@{
    schemaVersion = "simulation-world-h2-composition-plans.v1"
    revision = "simulation-world-h2-composition-plans.r3"
    sourceRecipeRevision = [string] $recipes.revision
    generatedPlanCount = $plans.Count
    plans = $plans
    referencePlayLoops = @($recipes.referencePlayLoops)
    authorityBoundary = [string] $recipes.authorityBoundary
    presentationOnly = $true
    isOperationalState = $false
}
$outputJson = ConvertTo-StableJson $output

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# P1~P3 H2 조립안")
[void] $builder.AppendLine()
[void] $builder.AppendLine("이 문서는 H1을 상대 위치·관계·연결구로 조립한 위치 독립 H2 설계안이다. 실제 도로·경계·AreaSet·경관 그래프 권위가 아니다.")
foreach ($plan in $plans) {
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## $($plan.title)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("- 후보: ``$($plan.targetKnowledgeRef)``")
    [void] $builder.AppendLine("- 위상: ``$($plan.topologyCode)``")
    [void] $builder.AppendLine("- 기준 크기: ``$($plan.referenceSizeMeters.width)m × $($plan.referenceSizeMeters.depth)m``")
    [void] $builder.AppendLine("- 설계 상태: ``$($plan.designStateCode)``")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| H1 노드 | 플레이 역할 | WI | 계획 용량 | 로컬 X/Z | 회전 |")
    [void] $builder.AppendLine("| --- | --- | --- | --- | ---: | ---: |")
    foreach ($node in $plan.nodes) {
        $playRole = if ($node.PSObject.Properties.Name -contains "playRoleCode") { [string] $node.playRoleCode } else { "-" }
        $wiText = if ($node.PSObject.Properties.Name -contains "wiIds") { @($node.wiIds) -join ", " } else { "-" }
        $capacityText = if ($node.PSObject.Properties.Name -contains "planningCapacities") {
            @($node.planningCapacities | ForEach-Object { "$($_.capacityCode) $($_.quantity)$($_.unitCode)" }) -join ", "
        } else { "-" }
        [void] $builder.AppendLine("| ``$($node.h1Ref)`` | ``$playRole`` | $wiText | $capacityText | $($node.localX) / $($node.localZ) | $($node.rotationDegrees)° |")
    }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("연결구: " + (@($plan.externalConnectors.roleCode | ForEach-Object { "``$_``" }) -join ", "))
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## Nature 기준 플레이 폐루프")
[void] $builder.AppendLine()
$retreatBranchText = @($naturePlayLoop.branches | Where-Object branchCode -eq "RetreatAndRecover").wiSequence -join " → "
$restoreBranchText = @($naturePlayLoop.branches | Where-Object branchCode -eq "RestoreAndRecover").wiSequence -join " → "
$handoffText = @($naturePlayLoop.h2Handoffs.relationCode | ForEach-Object { "``$_``" }) -join ", "
[void] $builder.AppendLine("- 기준 플레이: ``$($naturePlayLoop.playLoopId)``")
[void] $builder.AppendLine("- H3 후보: ``$($naturePlayLoop.h3Ref)``")
[void] $builder.AppendLine("- 후퇴 분기: ``$retreatBranchText``")
[void] $builder.AppendLine("- 복원 분기: ``$restoreBranchText``")
[void] $builder.AppendLine("- H2 인계: $handoffText")
[void] $builder.AppendLine("- 다음 플레이: ``$($naturePlayLoop.completionContext.nextPlayerVerbCode)``")
[void] $builder.AppendLine("- 증거 단계: ``$($naturePlayLoop.evidenceStageCode)`` · 위치 독립 설계 후보")
$document = $builder.ToString().TrimEnd() + "`n"

if ($Mode -eq "Write") {
    Write-TextIfChanged $outputPath $outputJson
    Write-TextIfChanged $documentPath $document
    Write-Output "H2CompositionPlansGenerated:P1=2;P2=2;P3=2;Nodes=$(@($plans.nodes).Count);Edges=$(@($plans.edges).Count);Connectors=$(@($plans.externalConnectors).Count)"
}
else {
    Require (Test-Path -LiteralPath $outputPath) "H2CompositionOutputMissing"
    Require (Test-Path -LiteralPath $documentPath) "H2CompositionDocumentMissing"
    Require ((Get-Content -LiteralPath $outputPath -Raw -Encoding UTF8) -ceq ($outputJson.TrimEnd() + "`n")) "H2CompositionOutputOutOfDate"
    Require ((Get-Content -LiteralPath $documentPath -Raw -Encoding UTF8) -ceq $document) "H2CompositionDocumentOutOfDate"
    Write-Output "H2CompositionPlansValid:P1=2;P2=2;P3=2;Nodes=$(@($plans.nodes).Count);Edges=$(@($plans.edges).Count);Connectors=$(@($plans.externalConnectors).Count)"
}
