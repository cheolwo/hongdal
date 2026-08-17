[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/execution-ledgers/world-interactions.json",
    [string] $OutputPath = "docs/AI/generated/world-interaction-catalog.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "WorldInteractionCatalogInvalid:$Code" }
}

function Get-StageIndex([object[]] $Stages, [string] $Code) {
    for ($index = 0; $index -lt $Stages.Count; $index++) {
        if ([string] $Stages[$index].code -eq $Code) { return $index }
    }
    return -1
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Escape-Markdown([string] $Value) {
    if ($null -eq $Value) { return "" }
    return $Value.Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = (Resolve-Path (Join-Path $repositoryRoot $InputPath)).Path
$catalog = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json
$resolvedStageCatalog = (Resolve-Path (Join-Path $repositoryRoot ([string] $catalog.evidenceStageCatalogPath))).Path
$stageCatalog = Get-Content -LiteralPath $resolvedStageCatalog -Raw -Encoding UTF8 | ConvertFrom-Json
$evidenceStages = @($stageCatalog.stages)

Require-Text $catalog.catalogKey "CatalogKeyMissing"
Require-Text $catalog.revision "RevisionMissing"
Require-Text $catalog.evidenceStageCatalogPath "EvidenceStageCatalogPathMissing"
Require ([string] $catalog.defaultImplementationTargetStage -eq "E3") "DefaultImplementationTargetMustBeE3"
Require ([string] $catalog.defaultIntegrationTargetStage -eq "E7") "DefaultIntegrationTargetMustBeE7"
Require ([string] $stageCatalog.schemaVersion -eq "simulation-evidence-stages.v3") "EvidenceStageCatalogSchemaInvalid"
Require ($evidenceStages.Count -eq 8) "EvidenceStagesMustHaveEightEntries"
Require ((@($evidenceStages.code) -join ",") -eq "E0,E1,E2,E3,E4,E5,E6,E7") "EvidenceStageOrderInvalid"
Require (@($catalog.items).Count -eq 37) "WorldInteractionCountMustBe37"
Require ([string] $catalog.schemaVersion -eq "3") "WorldInteractionCatalogSchemaMustBe3"

$seedbedRoot = Join-Path $repositoryRoot "eng/world-seedbeds/wi-spatial-seedbeds"
$seedbedCatalog = Get-Content -LiteralPath (Join-Path $seedbedRoot "catalog.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$approvedSeedbedIds = @{}
foreach ($definitionRef in @($seedbedCatalog.definitionRefs)) {
    $definition = Get-Content -LiteralPath (Join-Path $seedbedRoot ([string] $definitionRef)) -Raw -Encoding UTF8 |
        ConvertFrom-Json
    Require ([string] $definition.reviewStatusCode -eq "ApprovedForSimulation") "SeedbedNotApproved:$($definition.stableId)"
    $approvedSeedbedIds[[string] $definition.stableId] = $true
}

$allowedKinds = @("Command", "AutomaticTransition", "SharedPolicy")
$allowedImplementationStatuses = @("NotStarted", "InProgress", "Blocked", "Done")
$allowedIntegrationStatuses = @("NotSelected", "Selected", "InProgress", "Done", "Blocked")
$itemsById = @{}

foreach ($item in @($catalog.items)) {
    $id = [string] $item.id
    Require-Text $id "ItemIdMissing"
    Require (-not $itemsById.ContainsKey($id)) "DuplicateItemId:$id"
    Require ($allowedKinds -contains [string] $item.kind) "UnknownKind:$id"
    Require-Text $item.groupCode "GroupCodeMissing:$id"
    Require ([int] $item.sequence -gt 0) "SequenceInvalid:$id"
    Require-Text $item.title "TitleMissing:$id"
    Require-Text $item.worldAction "WorldActionMissing:$id"
    Require (@($item.startStateCodes).Count -gt 0) "StartStateMissing:$id"
    Require (@($item.completionStateCodes).Count -gt 0) "CompletionStateMissing:$id"
    Require-Text $item.previewRule "PreviewRuleMissing:$id"
    Require-Text $item.confirmRule "ConfirmRuleMissing:$id"
    Require-Text $item.taskRule "TaskRuleMissing:$id"
    Require (@($item.effectCodes).Count -gt 0) "EffectMissing:$id"
    Require-Text $item.cancellationPolicy "CancellationPolicyMissing:$id"
    Require-Text $item.actionCode "ActionCodeMissing:$id"
    Require-Text $item.ruleRevision "RuleRevisionMissing:$id"
    Require (@($item.saveReplayPayloadCodes).Count -gt 0) "SaveReplayPayloadMissing:$id"
    Require (@($item.httpContracts).Count -gt 0) "HttpContractMissing:$id"
    Require (@($item.sourceReferences).Count -gt 0) "SourceReferenceMissing:$id"

    $implementation = $item.implementation
    $integration = $item.integration
    Require ($allowedImplementationStatuses -contains [string] $implementation.status) "ImplementationStatusInvalid:$id"
    Require ($allowedIntegrationStatuses -contains [string] $integration.status) "IntegrationStatusInvalid:$id"
    $implementationCurrent = Get-StageIndex $evidenceStages ([string] $implementation.currentStage)
    $implementationTarget = Get-StageIndex $evidenceStages ([string] $implementation.targetStage)
    $integrationCurrent = Get-StageIndex $evidenceStages ([string] $integration.currentStage)
    $integrationTarget = Get-StageIndex $evidenceStages ([string] $integration.targetStage)
    Require ($implementationCurrent -ge 0 -and $implementationTarget -ge 0) "ImplementationStageInvalid:$id"
    Require ($integrationCurrent -ge 0 -and $integrationTarget -ge 0) "IntegrationStageInvalid:$id"
    Require ($implementationCurrent -le $implementationTarget) "ImplementationStageExceedsTarget:$id"
    Require ($integrationCurrent -le $integrationTarget) "IntegrationStageExceedsTarget:$id"
    Require ([string] $implementation.targetStage -eq "E3") "ImplementationTargetMustBeE3:$id"
    Require ([string] $integration.targetStage -eq "E7") "IntegrationTargetMustBeE7:$id"
    if ([string] $implementation.status -eq "Done") {
        Require ([string] $implementation.currentStage -eq "E3") "ImplementationDoneWithoutE3:$id"
        Require (@($implementation.evidence).Count -gt 0) "ImplementationEvidenceMissing:$id"
    }
    if ($integrationCurrent -ge (Get-StageIndex $evidenceStages "E4")) {
        Require ($integration.PSObject.Properties.Name -contains "e4SeedbedRefs") "E4SeedbedRefsMissing:$id"
        Require (@($integration.e4SeedbedRefs).Count -gt 0) "E4SeedbedRefsEmpty:$id"
        foreach ($seedbedRef in @($integration.e4SeedbedRefs)) {
            Require ($approvedSeedbedIds.ContainsKey([string] $seedbedRef)) "E4SeedbedRefUnknown:${id}:$seedbedRef"
        }
    }
    if ([string] $item.kind -eq "AutomaticTransition") {
        Require ($null -ne $item.automaticTransition) "AutomaticTransitionContractMissing:$id"
        Require-Text $item.automaticTransition.triggerWiId "AutomaticTriggerMissing:$id"
        Require-Text $item.automaticTransition.triggerState "AutomaticTriggerStateMissing:$id"
        Require-Text $item.automaticTransition.targetState "AutomaticTargetStateMissing:$id"
        Require-Text $item.automaticTransition.causeLineage "AutomaticCauseLineageMissing:$id"
    }
    if ([string] $item.kind -eq "SharedPolicy") {
        Require ($null -ne $item.sharedPolicy) "SharedPolicyContractMissing:$id"
        Require (@($item.sharedPolicy.consumers).Count -gt 0) "SharedPolicyConsumersMissing:$id"
        Require (@($item.sharedPolicy.resultCodes).Count -gt 0) "SharedPolicyResultsMissing:$id"
    }
    foreach ($reference in @($item.sourceReferences)) {
        Require (Test-Path -LiteralPath (Join-Path $repositoryRoot ([string] $reference))) "SourceReferenceNotFound:${id}:$reference"
    }
    $itemsById[$id] = $item
}

Require (@($catalog.items | Where-Object kind -eq "Command").Count -eq 26) "CommandCountMustBe26"
Require (@($catalog.items | Where-Object kind -eq "AutomaticTransition").Count -eq 10) "AutomaticTransitionCountMustBe10"
Require (@($catalog.items | Where-Object kind -eq "SharedPolicy").Count -eq 1) "SharedPolicyCountMustBe1"

foreach ($item in @($catalog.items)) {
    foreach ($predecessor in @($item.predecessorWiIds)) {
        Require ($itemsById.ContainsKey([string] $predecessor)) "PredecessorNotFound:$($item.id):$predecessor"
        Require (@($itemsById[[string] $predecessor].successorWiIds) -contains [string] $item.id) "PredecessorNotReciprocal:$($item.id):$predecessor"
    }
    foreach ($successor in @($item.successorWiIds)) {
        Require ($itemsById.ContainsKey([string] $successor)) "SuccessorNotFound:$($item.id):$successor"
        Require (@($itemsById[[string] $successor].predecessorWiIds) -contains [string] $item.id) "SuccessorNotReciprocal:$($item.id):$successor"
    }
    if ([string] $item.kind -eq "AutomaticTransition") {
        Require ($itemsById.ContainsKey([string] $item.automaticTransition.triggerWiId)) "AutomaticTriggerNotFound:$($item.id)"
    }
}

$stageLabels = @{}
foreach ($stage in $evidenceStages) { $stageLabels[[string] $stage.code] = [string] $stage.label }
$kindLabels = @{ Command = "명시적 명령"; AutomaticTransition = "자동 상태 전이"; SharedPolicy = "공유 정책" }
$statusLabels = @{ NotStarted = "미착수"; InProgress = "진행 중"; Blocked = "차단"; Done = "완료"; NotSelected = "미선정"; Selected = "선정" }

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# 세계 상호작용 단위 대장")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$InputPath``에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 대장 개정: ``$($catalog.revision)``")
[void] $builder.AppendLine("- 증거 단계 개정: ``$($stageCatalog.revision)``")
[void] $builder.AppendLine("- 마지막 확인일: ``$($catalog.lastVerifiedDate)``")
[void] $builder.AppendLine("- 기본 구현 완료선: ``E3 자동 시험 통과``")
[void] $builder.AppendLine("- 실제 공간·공공데이터·Unity 통합 목표선: ``E7 실제 플레이 폐루프``")
[void] $builder.AppendLine("- 전체 항목: ``$($catalog.items.Count)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 읽는 법")
[void] $builder.AppendLine()
[void] $builder.AppendLine("WI는 새 업무 엔티티가 아니라 행위자·공간·자원·미리보기·확정·예약·작업·효과·저장/재생을 관통하는 구현·검증 단위다. ``Command``만 독립 확정을 가지며, ``AutomaticTransition``은 부모 명령의 계보와 Tick으로 진행되고, ``SharedPolicy``는 여러 WI가 함께 쓰는 판정 규칙이다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 분류 요약")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 분류 | 수 |")
[void] $builder.AppendLine("| --- | ---: |")
foreach ($kind in $allowedKinds) {
    [void] $builder.AppendLine("| $($kindLabels[$kind]) | $(@($catalog.items | Where-Object kind -eq $kind).Count) |")
}

foreach ($group in @($catalog.items | Sort-Object groupCode, sequence | Group-Object groupCode)) {
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## $($group.Name) 작업군")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| WI | 종류 | 시작 → 완료 | 구현 | 통합 |")
    [void] $builder.AppendLine("| --- | --- | --- | --- | --- |")
    foreach ($item in @($group.Group | Sort-Object sequence, id)) {
        $states = "$(Escape-Markdown (@($item.startStateCodes) -join ', ')) → $(Escape-Markdown (@($item.completionStateCodes) -join ', '))"
        $implementation = "$($statusLabels[[string] $item.implementation.status]) · ``$($item.implementation.currentStage)→$($item.implementation.targetStage)``"
        $integration = "$($statusLabels[[string] $item.integration.status]) · ``$($item.integration.currentStage)→$($item.integration.targetStage)``"
        [void] $builder.AppendLine("| ``$($item.id)`` $($item.title) | $($kindLabels[[string] $item.kind]) | $states | $implementation | $integration |")
    }
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 첫 E4 공간 모판 공급선")
[void] $builder.AppendLine()
[void] $builder.AppendLine('```text')
[void] $builder.AppendLine("WI-FARM-04 수확 (100㎡ × 3kg/㎡ = 300kg)")
[void] $builder.AppendLine("→ WI-FARM-05 집하")
[void] $builder.AppendLine("→ WI-FARM-06 출하 준비·포장")
[void] $builder.AppendLine("→ WI-LOG-01 상차 확정")
[void] $builder.AppendLine("→ WI-LOG-02 출발 [자동]")
[void] $builder.AppendLine("→ WI-LOG-03 Farm→Hub 이동 [자동]")
[void] $builder.AppendLine("→ WI-LOG-04 Hub 하차 [자동]")
[void] $builder.AppendLine("→ WI-LOG-05 Hub 인수 [WI-001 안의 자동 전이]")
[void] $builder.AppendLine("→ WI-001 입고검수")
[void] $builder.AppendLine("→ WI-002 창고 적재")
[void] $builder.AppendLine('```')
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 증거 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- E3는 계약·코드·자동 시험의 구현 완료선이다.")
[void] $builder.AppendLine("- Scenario 공간으로 통과한 E3는 실제 LandscapeGraph 또는 공공 공간자료 증거가 아니다.")
[void] $builder.AppendLine("- E4는 하나 이상의 E3 WI를 품는 위치 독립 공간 모판 완료선이다. 실제 AreaSet·Graph·좌표를 요구하지 않는다.")
[void] $builder.AppendLine("- E5는 승인된 E4 모판을 실제 AreaSet·도로·Block·LandscapeGraph에 배치해 이동 경로를 닫는 단계다.")
[void] $builder.AppendLine("- 실제 서버와 저장 Scene에서 사람이 조작한 Play Mode·Game View·Console 증거가 있어야 E7이다.")
[void] $builder.AppendLine("- Unity 애니메이션이나 GameObject 상태가 Task 완료를 확정하지 않는다.")

$expected = $builder.ToString().Replace("`r`n", "`n")
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    [IO.Directory]::CreateDirectory((Split-Path -Parent $resolvedOutput)) | Out-Null
    [IO.File]::WriteAllText($resolvedOutput, $expected, [Text.UTF8Encoding]::new($false))
    Write-Output "WorldInteractionCatalogGenerated:$OutputPath"
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedDocumentMissing:$OutputPath"
    $actual = [IO.File]::ReadAllText($resolvedOutput).Replace("`r`n", "`n")
    Require ($actual -eq $expected) "GeneratedDocumentOutOfDate:$OutputPath"
    Write-Output "WorldInteractionCatalogValid:$($catalog.items.Count)"
}
