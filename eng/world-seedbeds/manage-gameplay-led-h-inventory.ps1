param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $OutputJsonPath = "eng/world-seedbeds/generated/gameplay-led-h-inventory.v1.json",
    [string] $OutputMarkdownPath = "docs/AI/generated/gameplay-led-h-inventory.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "GameplayLedHInventoryInvalid:$Code" }
}

function Read-Json([string] $Path) {
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Load-Definitions([string] $Root, [object[]] $References) {
    return @($References | ForEach-Object {
        Read-Json (Join-Path $Root ([string] $_.definitionPath))
    })
}

function New-Map([object[]] $Items) {
    $map = @{}
    foreach ($item in $Items) { $map[[string] $item.stableId] = $item }
    return $map
}

function Add-Unique([Collections.Generic.List[string]] $List, [string] $Value) {
    if (-not [string]::IsNullOrWhiteSpace($Value) -and -not $List.Contains($Value)) { $List.Add($Value) }
}

function Expand-H4Coverage(
    [string[]] $H4Refs,
    [hashtable] $H4Map,
    [hashtable] $H3Map,
    [hashtable] $H2Map,
    [object[]] $ExpressionH1) {
    $h4RefsOut = [Collections.Generic.List[string]]::new()
    $h3RefsOut = [Collections.Generic.List[string]]::new()
    $h2RefsOut = [Collections.Generic.List[string]]::new()
    $h1RefsOut = [Collections.Generic.List[string]]::new()

    foreach ($h4Ref in $H4Refs) {
        Require ($H4Map.ContainsKey($h4Ref)) "UnknownH4:$h4Ref"
        Add-Unique $h4RefsOut $h4Ref
        $h4 = $H4Map[$h4Ref]
        foreach ($h3Ref in @($h4.requiredH3Refs + $h4.optionalH3Refs)) {
            Require ($H3Map.ContainsKey([string] $h3Ref)) "UnknownH3:${h4Ref}:$h3Ref"
            Add-Unique $h3RefsOut ([string] $h3Ref)
        }
    }
    foreach ($h3Ref in @($h3RefsOut)) {
        $h3 = $H3Map[$h3Ref]
        foreach ($h2Ref in @($h3.requiredH2Refs + $h3.optionalH2Refs)) {
            Require ($H2Map.ContainsKey([string] $h2Ref)) "UnknownH2:${h3Ref}:$h2Ref"
            Add-Unique $h2RefsOut ([string] $h2Ref)
        }
    }
    foreach ($h2Ref in @($h2RefsOut)) {
        $h2 = $H2Map[$h2Ref]
        foreach ($h1Ref in @($h2.requiredH1Refs + $h2.optionalH1Refs)) {
            Add-Unique $h1RefsOut ([string] $h1Ref)
        }
    }
    $expressionRefs = [Collections.Generic.List[string]]::new()
    foreach ($expression in $ExpressionH1) {
        if (@($expression.supportsInteractionH1Refs | Where-Object { $h1RefsOut.Contains([string] $_) }).Count -gt 0) {
            Add-Unique $expressionRefs ([string] $expression.stableId)
        }
    }
    return [ordered]@{
        h1InteractionRefs = @($h1RefsOut | Sort-Object)
        h1ExpressionRefs = @($expressionRefs | Sort-Object)
        h2Refs = @($h2RefsOut | Sort-Object)
        h3Refs = @($h3RefsOut | Sort-Object)
        h4Refs = @($h4RefsOut | Sort-Object)
    }
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$knowledgeRoot = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory"
$policyPath = Join-Path $knowledgeRoot "gameplay-led-h-policy.v1.json"
$priorityPath = Join-Path $knowledgeRoot "area-set-composition-priorities.v1.json"
$catalogPath = Join-Path $knowledgeRoot "catalog.v3.json"
$worldInteractionPath = Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json"

$policy = Read-Json $policyPath
$priority = Read-Json $priorityPath
$catalog = Read-Json $catalogPath
$worldInteractions = Read-Json $worldInteractionPath
Require ([string] $policy.schemaVersion -eq "simulation-world-gameplay-led-h-policy.v1") "PolicySchema"
Require ([bool] $policy.admissionRules.publicDataFieldsForbiddenInHDefinitions) "PublicDataBoundary"
Require ([string] $policy.admissionRules.orphanDispositionCode -eq "QuarantineAsIdeaInventory") "OrphanDisposition"
Require (@($priority.areaSetCandidates).Count -eq 4) "PrimaryGamePlanCount"

$interactionH1 = Load-Definitions $knowledgeRoot @($catalog.h1InteractionDefinitionRefs)
$expressionH1 = Load-Definitions $knowledgeRoot @($catalog.h1ExpressionDefinitionRefs)
$h2 = Load-Definitions $knowledgeRoot @($catalog.h2DefinitionRefs)
$h3 = Load-Definitions $knowledgeRoot @($catalog.h3DefinitionRefs)
$h4 = Load-Definitions $knowledgeRoot @($catalog.h4DefinitionRefs)
$h1Map = New-Map $interactionH1
$h2Map = New-Map $h2
$h3Map = New-Map $h3
$h4Map = New-Map $h4
$knownWiIds = @($worldInteractions.items.id)

$contextlessInteractionH1 = @($interactionH1 | Where-Object {
    @($_.wiIds).Count -eq 0 -and @($_.anticipatedGameplayCodes).Count -eq 0
} | ForEach-Object stableId)
$unlinkedExpressionH1 = @($expressionH1 | Where-Object {
    @($_.supportsInteractionH1Refs).Count -eq 0 -and [string] $_.knowledgeStateCode -ne "IdeaInventory"
} | ForEach-Object stableId)
$quarantinedExpressionH1 = @($expressionH1 | Where-Object {
    @($_.supportsInteractionH1Refs).Count -eq 0 -and [string] $_.knowledgeStateCode -eq "IdeaInventory"
} | ForEach-Object stableId)
foreach ($definition in $interactionH1) {
    foreach ($wiId in @($definition.wiIds)) { Require ($knownWiIds -contains [string] $wiId) "UnknownWi:$($definition.stableId):$wiId" }
}
foreach ($definition in $expressionH1) {
    foreach ($h1Ref in @($definition.supportsInteractionH1Refs)) { Require ($h1Map.ContainsKey([string] $h1Ref)) "UnknownExpressionTarget:$($definition.stableId):$h1Ref" }
}

$usedH3 = @($h4 | ForEach-Object { @($_.requiredH3Refs + $_.optionalH3Refs) } | Sort-Object -Unique)
$usedH2 = @($h3 | ForEach-Object { @($_.requiredH2Refs + $_.optionalH2Refs) } | Sort-Object -Unique)
$usedH1 = @($h2 | ForEach-Object { @($_.requiredH1Refs + $_.optionalH1Refs) } | Sort-Object -Unique)
$orphanH1 = @($interactionH1.stableId | Where-Object { $_ -notin $usedH1 } | Sort-Object)
$orphanH2 = @($h2.stableId | Where-Object { $_ -notin $usedH2 } | Sort-Object)
$orphanH3 = @($h3.stableId | Where-Object { $_ -notin $usedH3 } | Sort-Object)

$supportByPlan = @{}
foreach ($binding in @($policy.supportBlueprintBindings)) {
    if (-not $supportByPlan.ContainsKey([string] $binding.gamePlanCode)) { $supportByPlan[[string] $binding.gamePlanCode] = @() }
    $supportByPlan[[string] $binding.gamePlanCode] += [string] $binding.h4Ref
}
$planCoverage = @()
foreach ($candidate in @($priority.areaSetCandidates)) {
    $planCode = [string] $candidate.gamePlanCode
    Require (-not [string]::IsNullOrWhiteSpace($planCode)) "GamePlanCodeMissing:$($candidate.priorityCode)"
    Require (@($candidate.corePlayerVerbCodes).Count -gt 0) "PlayerVerbsMissing:$planCode"
    Require (@($candidate.coreWiIds).Count -gt 0) "CoreWiMissing:$planCode"
    foreach ($wiId in @($candidate.coreWiIds)) { Require ($knownWiIds -contains [string] $wiId) "CoreWiUnknown:${planCode}:$wiId" }
    $rootRefs = @([string] $candidate.areaSetCandidateRef)
    if ($supportByPlan.ContainsKey($planCode)) { $rootRefs += @($supportByPlan[$planCode]) }
    $coverage = Expand-H4Coverage $rootRefs $h4Map $h3Map $h2Map $expressionH1
    $planCoverage += [ordered]@{
        priorityCode = [string] $candidate.priorityCode
        gamePlanCode = $planCode
        title = [string] $candidate.title
        playerWorldRoleCode = [string] $candidate.playerWorldRoleCode
        corePlayerVerbCodes = @($candidate.corePlayerVerbCodes)
        coreWiIds = @($candidate.coreWiIds)
        coverage = $coverage
    }
}

$crossWorldCoverage = @()
foreach ($binding in @($policy.crossWorldBlueprintBindings)) {
    $coverage = Expand-H4Coverage @([string] $binding.h4Ref) $h4Map $h3Map $h2Map $expressionH1
    $crossWorldCoverage += [ordered]@{
        gamePlanCode = [string] $binding.gamePlanCode
        roleCode = [string] $binding.roleCode
        participatingGamePlanCodes = @($binding.participatingGamePlanCodes)
        coverage = $coverage
    }
}

$violations = [ordered]@{
    contextlessInteractionH1Refs = $contextlessInteractionH1
    unlinkedExpressionH1Refs = $unlinkedExpressionH1
    quarantinedExpressionH1Refs = $quarantinedExpressionH1
    orphanH1Refs = $orphanH1
    orphanH2Refs = $orphanH2
    orphanH3Refs = $orphanH3
}
$result = [ordered]@{
    schemaVersion = "simulation-world-gameplay-led-h-inventory-report.v1"
    policyRevision = [string] $policy.revision
    catalogRevision = [string] $catalog.revision
    areaSetPriorityRevision = [string] $priority.revision
    counts = [ordered]@{
        primaryGamePlans = @($planCoverage).Count
        crossWorldPlans = @($crossWorldCoverage).Count
        h1Interaction = @($interactionH1).Count
        h1Expression = @($expressionH1).Count
        h2 = @($h2).Count
        h3 = @($h3).Count
        h4 = @($h4).Count
        violations = @($contextlessInteractionH1 + $unlinkedExpressionH1 + $orphanH1 + $orphanH2 + $orphanH3).Count
        quarantinedExpressionH1 = @($quarantinedExpressionH1).Count
    }
    planCoverage = $planCoverage
    crossWorldCoverage = $crossWorldCoverage
    violations = $violations
    hExpansionQueue = @($policy.hExpansionQueue)
    wiEvidenceQueue = @($policy.wiEvidenceQueue)
    authorityBoundary = "H는 게임 기획에 속한 위치 독립 설계다. E5가 실제 배치, E6가 선정 WI의 공공데이터 계보다."
}

$json = ($result | ConvertTo-Json -Depth 30) + "`n"
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# 게임 기획 주도 H 공간 재고와 우선순위")
[void] $builder.AppendLine()
[void] $builder.AppendLine("H 재고는 게임 기획 묶음에 속해야 하며, WI 또는 예상 플레이와 연결되지 않은 카드는 공식 승격하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 감사 결과")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 상호작용 H1: $(@($interactionH1).Count)장")
[void] $builder.AppendLine("- 팩 표현 H1: $(@($expressionH1).Count)장")
[void] $builder.AppendLine("- H2/H3/H4: $(@($h2).Count)/$(@($h3).Count)/$(@($h4).Count)")
[void] $builder.AppendLine("- 맥락·계보 위반: $($result.counts.violations)건")
[void] $builder.AppendLine("- 게임 기획 연결 전 격리된 팩 표현 H1: $($result.counts.quarantinedExpressionH1)장")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 게임 기획 묶음")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 순위 | 게임 기획 | 핵심 동사 | H1/H2/H3/H4 범위 |")
[void] $builder.AppendLine("| --- | --- | --- | --- |")
foreach ($plan in $planCoverage) {
    [void] $builder.AppendLine("| $($plan.priorityCode) | $($plan.title) ($($plan.gamePlanCode)) | $(@($plan.corePlayerVerbCodes) -join ' → ') | $(@($plan.coverage.h1InteractionRefs).Count)/$(@($plan.coverage.h2Refs).Count)/$(@($plan.coverage.h3Refs).Count)/$(@($plan.coverage.h4Refs).Count) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## H 확장 순서")
[void] $builder.AppendLine()
foreach ($item in @($policy.hExpansionQueue)) { [void] $builder.AppendLine("1. **$($item.priorityCode) $($item.title)** — $($item.goal)") }
[void] $builder.AppendLine()
[void] $builder.AppendLine("## WI의 E 채움 순서")
[void] $builder.AppendLine()
foreach ($item in @($policy.wiEvidenceQueue)) { [void] $builder.AppendLine("1. **$($item.priorityCode) $($item.title)** — 목표 $($item.targetStageCode)") }
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 맥락 없는 H는 삭제하지 않고 IdeaInventory 격리 대상으로 보고한다.")
[void] $builder.AppendLine("- 팩 표현 카드는 상호작용 H1과 게임 기획에 연결되기 전에는 공식 작업공간이 아니다.")
[void] $builder.AppendLine("- 실제 AreaSet 배치는 E5, 필요한 공공데이터 계보는 E6에서만 수행한다.")
$markdown = $builder.ToString()

$outputJson = Join-Path $repositoryRoot $OutputJsonPath
$outputMarkdown = Join-Path $repositoryRoot $OutputMarkdownPath
if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $outputJson) "OutputJsonMissing"
    Require (Test-Path -LiteralPath $outputMarkdown) "OutputMarkdownMissing"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($outputJson))) -ceq (ConvertTo-DeterministicText $json)) "OutputJsonStale"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($outputMarkdown))) -ceq (ConvertTo-DeterministicText $markdown)) "OutputMarkdownStale"
    Write-Output "GameplayLedHInventoryValid:Plans=4;H1=$(@($interactionH1).Count)+$(@($expressionH1).Count);H2=$(@($h2).Count);H3=$(@($h3).Count);H4=$(@($h4).Count);Violations=$($result.counts.violations)"
    exit 0
}

[void] (Write-DeterministicTextIfChanged $outputJson $json)
[void] (Write-DeterministicTextIfChanged $outputMarkdown $markdown)
Write-Output "GameplayLedHInventoryGenerated:Plans=4;H1=$(@($interactionH1).Count)+$(@($expressionH1).Count);H2=$(@($h2).Count);H3=$(@($h3).Count);H4=$(@($h4).Count);Violations=$($result.counts.violations)"
