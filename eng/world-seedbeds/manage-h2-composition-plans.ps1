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

Require ([string] $recipes.coordinateSpaceCode -eq "LocalMeters") "H2CompositionCoordinateSpaceMustBeLocalMeters"
Require (-not ([string] $recipes.authorityBoundary -match "실제.*권위.*갖는다")) "H2CompositionAuthorityBoundaryInvalid"
Require (@($recipes.recipes).Count -eq 6) "H2CompositionP1P2P3RecipeCountMustBe6"

$h1ById = @{}
foreach ($reference in @($catalog.h1DefinitionRefs)) { $h1ById[[string] $reference.stableId] = $reference }
$h2ById = @{}
foreach ($reference in @($catalog.h2DefinitionRefs)) {
    $definitionPath = Join-Path $knowledgeRoot ([string] $reference.definitionPath)
    $h2ById[[string] $reference.stableId] = Read-Json $definitionPath
}
$priorityByCandidate = @{}
foreach ($candidate in @($priorities.candidates)) { $priorityByCandidate[[string] $candidate.candidateRef] = $candidate }

$plans = @()
foreach ($recipe in @($recipes.recipes | Sort-Object targetKnowledgeRef)) {
    $targetId = [string] $recipe.targetKnowledgeRef
    Require ($h2ById.ContainsKey($targetId)) "H2CompositionTargetUnknown:$targetId"
    Require ($priorityByCandidate.ContainsKey($targetId)) "H2CompositionPriorityUnknown:$targetId"
    Require (@("P1", "P2", "P3") -contains [string] $recipe.priorityCode) "H2CompositionPriorityNotEnabled:$targetId"
    Require ([string] $priorityByCandidate[$targetId].priorityCode -eq [string] $recipe.priorityCode) "H2CompositionPriorityMismatch:$targetId"

    $target = $h2ById[$targetId]
    $requiredH1 = @($target.requiredH1Refs | Sort-Object -Unique)
    $recipeH1 = @($recipe.nodes | Where-Object required | ForEach-Object h1Ref | Sort-Object -Unique)
    Require (($requiredH1 -join "`n") -ceq ($recipeH1 -join "`n")) "H2CompositionRequiredH1Mismatch:$targetId"

    $nodeIds = @($recipe.nodes.localNodeId)
    Require (@($nodeIds | Sort-Object -Unique).Count -eq $nodeIds.Count) "H2CompositionNodeDuplicate:$targetId"
    foreach ($node in @($recipe.nodes)) {
        Require ($h1ById.ContainsKey([string] $node.h1Ref)) "H2CompositionH1Unknown:${targetId}:$($node.h1Ref)"
        Require ([Math]::Abs([double] $node.localX) -le ([double] $recipe.referenceSizeMeters.width / 2)) "H2CompositionNodeOutsideWidth:${targetId}:$($node.localNodeId)"
        Require ([Math]::Abs([double] $node.localZ) -le ([double] $recipe.referenceSizeMeters.depth / 2)) "H2CompositionNodeOutsideDepth:${targetId}:$($node.localNodeId)"
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

$output = [pscustomobject][ordered]@{
    schemaVersion = "simulation-world-h2-composition-plans.v1"
    revision = "simulation-world-h2-composition-plans.r2"
    sourceRecipeRevision = [string] $recipes.revision
    generatedPlanCount = $plans.Count
    plans = $plans
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
    [void] $builder.AppendLine("| H1 노드 | 로컬 X/Z | 회전 |")
    [void] $builder.AppendLine("| --- | ---: | ---: |")
    foreach ($node in $plan.nodes) { [void] $builder.AppendLine("| ``$($node.h1Ref)`` | $($node.localX) / $($node.localZ) | $($node.rotationDegrees)° |") }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("연결구: " + (@($plan.externalConnectors.roleCode | ForEach-Object { "``$_``" }) -join ", "))
}
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
