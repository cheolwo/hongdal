[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/execution-ledgers/world-interaction-delivery-priorities.json",
    [string] $OutputPath = "docs/AI/generated/world-interaction-delivery-priorities.md",
    [string] $ContractOutputPath = "Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationWorldInteractionDeliveryPriorities.generated.cs"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "WorldInteractionDeliveryPriorityInvalid:$Code" }
}

function Escape-CSharp([string] $Value) {
    return $Value.Replace('"', '\"')
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = (Resolve-Path (Join-Path $repositoryRoot $InputPath)).Path
$ledger = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json
$catalog = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.worldInteractionCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$loops = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.playableLoopCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$wiEh = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.wiEhStatusPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$synty = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.syntyCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $ledger.schemaVersion -eq "world-interaction-delivery-priorities.v1") "SchemaInvalid"
Require ([bool] $ledger.principles.everyWorldInteractionAssignedExactlyOnce) "CoveragePrincipleMissing"
Require ([bool] $ledger.principles.deliveryWaveDoesNotChangeSimulationEffects) "NoEffectPrincipleMissing"
Require ([bool] $ledger.principles.playableLoopOwnsPlayClosedPromotion) "PlayableLoopAuthorityMissing"
Require ([bool] $ledger.principles.e7RequiresCurrentRuntimeEvidence) "E7EvidencePrincipleMissing"
Require ([bool] $ledger.principles.syntyExpressionDoesNotPromoteEvidence) "SyntyBoundaryMissing"
Require ([bool] $ledger.principles.independentAreasPrecedeIntegrationRoutes) "IndependentAreaPrincipleMissing"
Require ([string] $ledger.deliveryModeCode -eq "SingleWorldInteractionVertical") "DeliveryModeInvalid"
Require ([int] $ledger.workInProgressLimit -eq 1) "WorkInProgressLimitMustBeOne"
Require ([string] $ledger.defaultIndividualTargetEvidenceStage -eq "E7") "DefaultTargetMustBeE7"
$worldInteractionCount = @($catalog.items).Count
Require ($worldInteractionCount -gt 0) "WorldInteractionCountInvalid"
Require (@($ledger.items).Count -eq $worldInteractionCount) "DeliveryItemCountMismatch"
Require (@($ledger.waves).Count -eq 6) "DeliveryWaveCountMustBe6"

$catalogById = @{}
foreach ($item in @($catalog.items)) { $catalogById[[string] $item.id] = $item }
$loopById = @{}
foreach ($loop in @($loops.items)) { $loopById[[string] $loop.loopStableId] = $loop }
$statusById = @{}
foreach ($item in @($wiEh.items)) { $statusById[[string] $item.worldInteractionId] = $item }
$waveByCode = @{}
foreach ($wave in @($ledger.waves)) {
    $code = [string] $wave.code
    Require (-not $waveByCode.ContainsKey($code)) "DeliveryWaveDuplicate:$code"
    $waveByCode[$code] = $wave
}

$allowedRoles = @($ledger.allowedCompletionRoleCodes)
$assigned = @{}
foreach ($item in @($ledger.items)) {
    $id = [string] $item.worldInteractionId
    Require ($catalogById.ContainsKey($id)) "WorldInteractionUnknown:$id"
    Require (-not $assigned.ContainsKey($id)) "WorldInteractionDuplicate:$id"
    Require ($waveByCode.ContainsKey([string] $item.deliveryWaveCode)) "DeliveryWaveUnknown:$id"
    Require ($allowedRoles -contains [string] $item.completionRoleCode) "CompletionRoleUnknown:$id"
    Require ([int] $item.orderInWave -gt 0) "OrderInvalid:$id"
    Require (-not [string]::IsNullOrWhiteSpace([string] $item.syntyTrackCode)) "SyntyTrackMissing:$id"
    foreach ($loopRef in @($item.playableLoopRefs)) {
        Require ($loopById.ContainsKey([string] $loopRef)) "PlayableLoopUnknown:${id}:$loopRef"
        $loop = $loopById[[string] $loopRef]
        Require (@($loop.worldInteractionIds) -contains $id) "PlayableLoopDoesNotContainWorldInteraction:${id}:$loopRef"
    }
    if ([string] $item.completionRoleCode -ne "DeferredIntegration") {
        Require (@($item.playableLoopRefs).Count -gt 0) "ActiveItemPlayableLoopMissing:$id"
    }
    $assigned[$id] = $item
}

$missing = @($catalog.items | Where-Object { -not $assigned.ContainsKey([string] $_.id) })
$missingIds = @($missing | ForEach-Object { [string] $_.id })
Require ($missing.Count -eq 0) "WorldInteractionCoverageMissing:$($missingIds -join ',')"
$activeId = [string] $ledger.activeWork.worldInteractionId
Require ($assigned.ContainsKey($activeId)) "ActiveWorldInteractionUnknown:$activeId"
$activeWorkState = [string] $ledger.activeWork.workStateCode
Require ($activeWorkState -in @("Active", "E7Closed")) "ActiveWorkStateInvalid"
Require ([string] $ledger.activeWork.currentEvidenceStage -eq
    [string] $catalogById[$activeId].integration.currentStage) "ActiveEvidenceStageDrift:$activeId"
Require ([string] $ledger.activeWork.targetEvidenceStage -eq "E7") "ActiveTargetMustBeE7"
Require ([string] $assigned[$activeId].deliveryWaveCode -eq "D1" -and
    [string] $assigned[$activeId].completionRoleCode -ne "DeferredIntegration") `
    "ActiveWorldInteractionMustBeNatureDeliveryWork"
$stageReviews = @($ledger.activeWork.stageReviews)
Require ($stageReviews.Count -eq 4) "ActiveStageReviewCountInvalid"
Require ((@($stageReviews.stageCode) -join ",") -eq "E4,E5,E6,E7") `
    "ActiveStageReviewOrderInvalid"
foreach ($review in $stageReviews) {
    Require (@($ledger.allowedStageReviewResultCodes) -contains
        [string] $review.resultCode) "ActiveStageReviewResultInvalid:$($review.stageCode)"
    Require (-not [string]::IsNullOrWhiteSpace([string] $review.summary)) `
        "ActiveStageReviewSummaryMissing:$($review.stageCode)"
    Require (@($review.evidenceRefs).Count -gt 0) `
        "ActiveStageReviewEvidenceMissing:$($review.stageCode)"
}
$passedStageCodes = @($stageReviews | Where-Object resultCode -eq "Passed" |
    ForEach-Object stageCode)
if ($activeWorkState -eq "Active") {
    $currentStageIndex = [Array]::IndexOf(@("E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7"),
        [string] $ledger.activeWork.currentEvidenceStage)
    Require ($currentStageIndex -ge 1 -and $currentStageIndex -lt 7) `
        "ActiveEvidenceStageInvalid"
    $expectedPassedStageCodes = if ($currentStageIndex -lt 4) {
        @()
    }
    else {
        @($stageReviews | Where-Object {
                [Array]::IndexOf(@("E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7"),
                    [string] $_.stageCode) -lt $currentStageIndex
            } | ForEach-Object stageCode)
    }
    Require (($passedStageCodes -join ",") -eq ($expectedPassedStageCodes -join ",")) `
        "ActivePassedStageSequenceInvalid"
    $firstOpenReviewIndex = $passedStageCodes.Count
    for ($index = $firstOpenReviewIndex + 1; $index -lt $stageReviews.Count; $index++) {
        Require ([string] $stageReviews[$index].resultCode -eq "Pending") `
            "ActiveLaterStageMustRemainPending:$($stageReviews[$index].stageCode)"
    }
    Require ([string] ($stageReviews | Where-Object stageCode -eq "E7").resultCode `
        -ne "Passed") "ActiveE7MustRemainOpen"
}
else {
    Require ([string] $ledger.activeWork.currentEvidenceStage -eq "E7") `
        "ClosedWorkMustBeE7"
    Require (($passedStageCodes -join ",") -eq "E4,E5,E6,E7") `
        "ClosedPassedStageSequenceInvalid"
}
Require ([string] $stageReviews[2].refinement.realityGroundingCode -eq "NotApplied") `
    "ActiveE6RealityBoundaryMissing"
Require ([string] $stageReviews[2].refinement.authorityCode -eq "SimulationCore") `
    "ActiveE6AuthorityInvalid"

$requiredNpcE8 = @($ledger.npcE8Policy.requiredCodes)
$conditionalNpcE8 = @($ledger.npcE8Policy.conditionalCodes)
Require (@($requiredNpcE8 | Select-Object -Unique).Count -eq $requiredNpcE8.Count) "NpcE8RequiredDuplicate"
Require (@($conditionalNpcE8 | Select-Object -Unique).Count -eq $conditionalNpcE8.Count) "NpcE8ConditionalDuplicate"
Require (@($requiredNpcE8 | Where-Object { $conditionalNpcE8 -contains $_ }).Count -eq 0) "NpcE8PolicyOverlap"
foreach ($id in @($requiredNpcE8 + $conditionalNpcE8)) {
    Require ($assigned.ContainsKey([string] $id)) "NpcE8WorldInteractionUnknown:$id"
}
$actorMigrationIds = @("WI-HUB-04", "WI-HUB-05", "WI-ORDER-03", "WI-ORDER-04")
$expectedRequiredNpcE8 = @($catalog.items | Where-Object {
        [string] $_.controlPolicyCode -eq "NpcRoutine"
    } | ForEach-Object { [string] $_.id }) + $actorMigrationIds
$expectedRequiredNpcE8 = @($expectedRequiredNpcE8 | Sort-Object -Unique)
Require ((@($requiredNpcE8 | Sort-Object) -join ",") -eq
    ($expectedRequiredNpcE8 -join ",")) "NpcE8RequiredSetInvalid"
$expectedConditionalNpcE8 = @($catalog.items | Where-Object {
        [string] $_.controlPolicyCode -eq "PlayerOrNpc"
    } | ForEach-Object { [string] $_.id } | Sort-Object)
Require ((@($conditionalNpcE8 | Sort-Object) -join ",") -eq
    ($expectedConditionalNpcE8 -join ",")) "NpcE8ConditionalSetInvalid"
foreach ($wave in @($ledger.waves)) {
    $waveCode = [string] $wave.code
    $items = @($ledger.items | Where-Object {
            [string] $_.deliveryWaveCode -eq $waveCode
        })
    Require ($items.Count -eq [int] $wave.expectedWorldInteractionCount) "WaveCountInvalid:$($wave.code)"
    $orders = @($items.orderInWave | ForEach-Object { [int] $_ } | Sort-Object)
    Require (($orders -join ",") -eq ((1..$items.Count) -join ",")) "WaveOrderInvalid:$($wave.code)"
}
Require ([int] $synty.counts.h1Total -ge 84) "SyntyH1InventoryMissing"

$stageOrder = @("E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9")
function Next-Gate([string] $CurrentStage) {
    $index = [Array]::IndexOf($stageOrder, $CurrentStage)
    if ($index -lt 0) { return "Unknown" }
    if ($index -ge 7) { return "Complete" }
    return $stageOrder[$index + 1]
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# WI 행동 단위 E7 실행 우선순위")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$InputPath``와 WI·폐루프·H 상태 대장에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 실행 우선순위 개정: ``$($ledger.revision)``")
[void] $builder.AppendLine("- 전체 WI: ``$(@($ledger.items).Count)``")
[void] $builder.AppendLine("- 진행 방식: ``$($ledger.deliveryModeCode)`` / 작업 중 한도 ``$($ledger.workInProgressLimit)``")
[void] $builder.AppendLine("- 현재 활성 WI: ``$activeId`` / ``$($ledger.activeWork.currentEvidenceStage)`` → ``$($ledger.activeWork.targetEvidenceStage)``")
[void] $builder.AppendLine("- Synty H1 설계 재고: ``$($synty.counts.h1Total)``")
[void] $builder.AppendLine("- E7은 최신 PlayMode·Game View·Hosted 동등성 증거가 있을 때만 승격한다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 현재 단일 WI 증거 관문")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| E 단계 | 판정 | 정제·검증 요약 |")
[void] $builder.AppendLine("| --- | --- | --- |")
foreach ($review in $stageReviews) {
    [void] $builder.AppendLine("| $($review.stageCode) | $($review.resultCode) | $($review.summary) |")
}
[void] $builder.AppendLine()
foreach ($wave in @($ledger.waves | Sort-Object order)) {
    [void] $builder.AppendLine("## $($wave.code) $($wave.title)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("- 진입: $($wave.entryRule)")
    [void] $builder.AppendLine("- 완료: $($wave.exitRule)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| 순서 | 한국어 행동명 · 고유 식별자 | 작업 | 역할 | 현재 구현 | 현재 통합 | 다음 관문 | NPC E8 | H 상태 | Synty | 폐루프 |")
    [void] $builder.AppendLine("| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
    $waveCode = [string] $wave.code
    foreach ($priority in @($ledger.items | Where-Object {
                [string] $_.deliveryWaveCode -eq $waveCode
            } | Sort-Object orderInWave)) {
        $wi = $catalogById[[string] $priority.worldInteractionId]
        $status = $statusById[[string] $priority.worldInteractionId]
        $loopsText = if (@($priority.playableLoopRefs).Count -eq 0) { "후속 정의" } else { @($priority.playableLoopRefs) -join "<br>" }
        $workState = if ([string] $priority.worldInteractionId -eq $activeId) { $activeWorkState }
            elseif ([string] $priority.completionRoleCode -eq "DeferredIntegration") { "Deferred" }
            else { "Queued" }
        $npcE8 = if ($requiredNpcE8 -contains [string] $priority.worldInteractionId) { "Required" }
            elseif ($conditionalNpcE8 -contains [string] $priority.worldInteractionId) { "Conditional" }
            else { "NotApplicable" }
        [void] $builder.AppendLine("| $($priority.orderInWave) | $($wi.title)<br>``$($wi.id)`` | $workState | $($priority.completionRoleCode) | $($wi.implementation.currentStage) | $($wi.integration.currentStage) | $(Next-Gate ([string] $wi.integration.currentStage)) | $npcE8 | $($status.spatialDesignStateCode) | $($priority.syntyTrackCode) | $loopsText |")
    }
    [void] $builder.AppendLine()
}

$contract = [Text.StringBuilder]::new()
[void] $contract.AppendLine("// <auto-generated />")
[void] $contract.AppendLine("#nullable enable")
[void] $contract.AppendLine("using System;")
[void] $contract.AppendLine("using System.Collections.Generic;")
[void] $contract.AppendLine()
[void] $contract.AppendLine("namespace Ssalddel.Simulation.Contracts")
[void] $contract.AppendLine("{")
[void] $contract.AppendLine("    public sealed class SimulationWI실행우선순위Definition")
[void] $contract.AppendLine("    {")
[void] $contract.AppendLine("        public SimulationWI실행우선순위Definition(string worldInteractionId,")
[void] $contract.AppendLine("            string 실행파동Code, int 파동내순서, string 완결역할Code,")
[void] $contract.AppendLine("            string 개발작업상태Code, string npcE8정책Code,")
[void] $contract.AppendLine("            string synty활용TrackCode, string[] 폐루프StableIds)")
[void] $contract.AppendLine("        {")
[void] $contract.AppendLine("            WorldInteractionId = worldInteractionId;")
[void] $contract.AppendLine("            this.실행파동Code = 실행파동Code;")
[void] $contract.AppendLine("            this.파동내순서 = 파동내순서;")
[void] $contract.AppendLine("            this.완결역할Code = 완결역할Code;")
[void] $contract.AppendLine("            this.개발작업상태Code = 개발작업상태Code;")
[void] $contract.AppendLine("            this.NpcE8정책Code = npcE8정책Code;")
[void] $contract.AppendLine("            this.Synty활용TrackCode = synty활용TrackCode;")
[void] $contract.AppendLine("            this.폐루프StableIds = 폐루프StableIds;")
[void] $contract.AppendLine("        }")
[void] $contract.AppendLine("        public string WorldInteractionId { get; }")
[void] $contract.AppendLine("        public string 실행파동Code { get; }")
[void] $contract.AppendLine("        public int 파동내순서 { get; }")
[void] $contract.AppendLine("        public string 완결역할Code { get; }")
[void] $contract.AppendLine("        public string 개발작업상태Code { get; }")
[void] $contract.AppendLine("        public string NpcE8정책Code { get; }")
[void] $contract.AppendLine("        public string Synty활용TrackCode { get; }")
[void] $contract.AppendLine("        public string[] 폐루프StableIds { get; }")
[void] $contract.AppendLine('        public string 목표EvidenceStage => "E7";')
[void] $contract.AppendLine("    }")
[void] $contract.AppendLine()
[void] $contract.AppendLine("    public static class SimulationWI실행우선순위Catalog")
[void] $contract.AppendLine("    {")
[void] $contract.AppendLine(('        public const string Revision = "{0}";' -f
        (Escape-CSharp ([string] $ledger.revision))))
[void] $contract.AppendLine(('        public const string ActiveWorldInteractionId = "{0}";' -f
        (Escape-CSharp $activeId)))
[void] $contract.AppendLine(('        public const string ActiveEvidenceStage = "{0}";' -f
        (Escape-CSharp ([string] $ledger.activeWork.currentEvidenceStage))))
[void] $contract.AppendLine("        public const int WorkInProgressLimit = 1;")
[void] $contract.AppendLine("        private static readonly SimulationWI실행우선순위Definition[] 항목들 =")
[void] $contract.AppendLine("        {")
foreach ($priority in @($ledger.items | Sort-Object { [int] $waveByCode[[string] $_.deliveryWaveCode].order }, orderInWave)) {
    $loopArgs = @($priority.playableLoopRefs | ForEach-Object {
            '"{0}"' -f (Escape-CSharp ([string] $_))
        }) -join ", "
    $loopExpression = if (@($priority.playableLoopRefs).Count -eq 0) {
        "Array.Empty<string>()"
    }
    else {
        "new[] { $loopArgs }"
    }
    $workState = if ([string] $priority.worldInteractionId -eq $activeId) { $activeWorkState }
        elseif ([string] $priority.completionRoleCode -eq "DeferredIntegration") { "Deferred" }
        else { "Queued" }
    $npcE8 = if ($requiredNpcE8 -contains [string] $priority.worldInteractionId) { "Required" }
        elseif ($conditionalNpcE8 -contains [string] $priority.worldInteractionId) { "Conditional" }
        else { "NotApplicable" }
    [void] $contract.AppendLine((
            '            new SimulationWI실행우선순위Definition("{0}", "{1}", {2}, "{3}", "{4}", "{5}", "{6}", {7}),' -f
            (Escape-CSharp ([string] $priority.worldInteractionId)),
            [string] $priority.deliveryWaveCode,
            [int] $priority.orderInWave,
            [string] $priority.completionRoleCode,
            $workState,
            $npcE8,
            [string] $priority.syntyTrackCode,
            $loopExpression))
}
[void] $contract.AppendLine("        };")
[void] $contract.AppendLine("        public static IReadOnlyList<SimulationWI실행우선순위Definition> All => 항목들;")
[void] $contract.AppendLine("        public static SimulationWI실행우선순위Definition? Find(string worldInteractionId)")
[void] $contract.AppendLine("        {")
[void] $contract.AppendLine("            if (string.IsNullOrWhiteSpace(worldInteractionId)) return null;")
[void] $contract.AppendLine("            foreach (var 항목 in 항목들)")
[void] $contract.AppendLine("                if (string.Equals(항목.WorldInteractionId, worldInteractionId, StringComparison.Ordinal)) return 항목;")
[void] $contract.AppendLine("            return null;")
[void] $contract.AppendLine("        }")
[void] $contract.AppendLine("    }")
[void] $contract.AppendLine("}")

$markdownContent = ConvertTo-DeterministicText ($builder.ToString().TrimEnd() + "`n")
$contractContent = ConvertTo-DeterministicText $contract.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
$resolvedContractOutput = Join-Path $repositoryRoot $ContractOutputPath
if ($Mode -eq "Write") {
    [void] (Write-DeterministicTextIfChanged -Path $resolvedOutput -Content $markdownContent)
    [void] (Write-DeterministicTextIfChanged -Path $resolvedContractOutput -Content $contractContent)
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    Require (Test-Path -LiteralPath $resolvedContractOutput) "GeneratedContractMissing"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedOutput))) -ceq $markdownContent) "GeneratedOutputMismatch"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedContractOutput))) -ceq $contractContent) "GeneratedContractMismatch"
}

Write-Output "WorldInteractionDeliveryPrioritiesValid:$worldInteractionCount;Waves=6;Revision=$($ledger.revision)"
