[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",

    [string] $CatalogPath =
        "eng/execution-ledgers/playable-loop-presentation-validation-modules.json",

    [string] $PlayableLoopPath =
        "eng/execution-ledgers/playable-loops.json",

    [string] $OutputPath =
        "docs/AI/generated/playable-loop-presentation-validation.md",
    [string] $UnityProjectRoot = ''
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../common/presentation-module-bindings.ps1')
. (Join-Path $PSScriptRoot '../common/deterministic-text-output.ps1')

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PresentationValidationInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedCatalog = Join-Path $repositoryRoot $CatalogPath
$resolvedLoops = Join-Path $repositoryRoot $PlayableLoopPath
$catalog = Get-Content -LiteralPath $resolvedCatalog -Raw -Encoding UTF8 |
    ConvertFrom-Json
$loops = Get-Content -LiteralPath $resolvedLoops -Raw -Encoding UTF8 |
    ConvertFrom-Json
$documentText = $catalog.generatedDocumentText

Require ([string] $catalog.schemaVersion -in @(
    "playable-loop-presentation-validation-modules.v1",
    "playable-loop-presentation-validation-modules.v2")) "SchemaInvalid"
$hasImplementationContract = $catalog.schemaVersion -eq 'playable-loop-presentation-validation-modules.v2'
if ($hasImplementationContract) {
    Require ((@($catalog.allowedEvidenceStageCodes) -join ',') -eq 'E1,E2,E3,E4,E5,E6,E7') "MinimumStagesInvalid"
}
Require ([string] $catalog.trackCode -eq "Presentation") "TrackInvalid"
foreach ($propertyName in @(
    "titleKo", "sourceNoticeFormatKo", "catalogLabelKo",
    "commonCountLabelKo", "conditionalCountLabelKo",
    "playableUnitCountLabelKo", "commonGateHeadingKo",
    "commonGateHeaderKo", "profileHeadingKo", "profileHeaderKo",
    "commonOnlyKo")) {
    Require-Text $documentText.$propertyName "GeneratedTextMissing:$propertyName"
}
foreach ($principle in @(
    "commonModulesApplyToEveryPlayableUnit",
    "featureModulesAreSelectedByDeclaredCapability",
    "blockingFailurePreventsTrackPromotion",
    "warningDoesNotPromoteEvidence",
    "runtimeEvidenceDoesNotReplaceAutomatedPrecheck",
    "failureReopensEarliestOwningStage",
    "presentationValidationDoesNotMutateSimulationAuthority",
    "e4FreezesApplicableAssetPlacementHandoff",
    "assetResearchAloneNeverPromotesE5",
    "notApplicableDoesNotForceSpatialAssets")) {
    $property = $catalog.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) `
        "PrincipleMissing:$principle"
}

$moduleByCode = @{}
foreach ($module in @($catalog.modules)) {
    $code = [string] $module.moduleCode
    Require-Text $code "ModuleCodeMissing"
    Require (-not $moduleByCode.ContainsKey($code)) "ModuleDuplicate:$code"
    Require-Text $module.displayNameKo "ModuleNameMissing:$code"
    Require (@($catalog.allowedEvidenceStageCodes) -contains
        [string] $module.evidenceStageCode) "StageInvalid:$code"
    Require (@($catalog.allowedApplicabilityCodes) -contains
        [string] $module.applicabilityCode) "ApplicabilityInvalid:$code"
    Require (@($catalog.allowedSeverityCodes) -contains
        [string] $module.severityCode) "SeverityInvalid:$code"
    Require (@($catalog.allowedAutomationLevelCodes) -contains
        [string] $module.automationLevelCode) "AutomationInvalid:$code"
    Require (@($module.reads).Count -gt 0) "ReadsMissing:$code"
    Require (@($module.failureReasonCodes).Count -gt 0) "FailureCodeMissing:$code"
    if ($hasImplementationContract) {
        foreach ($field in @('outputs','implementationRefs','testRefs')) {
            Require ($null -ne $module.PSObject.Properties[$field]) "ModuleContractMissing:${code}:$field"
        }
        Require (@($module.outputs).Count -gt 0) "OutputsMissing:$code"
        foreach ($reference in @($module.implementationRefs) + @($module.testRefs)) {
            Resolve-PresentationModuleReference ([string] $reference) $repositoryRoot $UnityProjectRoot | Out-Null
        }
    }
    if ([string] $module.applicabilityCode -eq "Common") {
        Require (@($module.requiredFeatureCodes).Count -eq 0) `
            "CommonHasFeature:$code"
    }
    else {
        Require (@($module.requiredFeatureCodes).Count -gt 0) `
            "FeatureMissing:$code"
        foreach ($featureCode in @($module.requiredFeatureCodes)) {
            Require (@($catalog.featureCodes) -contains [string] $featureCode) `
                "FeatureUnknown:${code}:$featureCode"
        }
    }
    $moduleByCode[$code] = $module
}

foreach ($commonCode in @($catalog.commonModuleCodes)) {
    Require ($moduleByCode.ContainsKey([string] $commonCode)) `
        "CommonModuleUnknown:$commonCode"
    Require ([string] $moduleByCode[[string] $commonCode].applicabilityCode -eq
        "Common") "CommonModuleNotCommon:$commonCode"
}
foreach ($stageCode in @($catalog.allowedEvidenceStageCodes)) {
    Require (@($catalog.commonModuleCodes | Where-Object {
        [string] $moduleByCode[[string] $_].evidenceStageCode -eq
            [string] $stageCode }).Count -gt 0) "CommonStageMissing:$stageCode"
}

$loopById = @{}
foreach ($loop in @($loops.items)) {
    $loopById[[string] $loop.loopStableId] = $loop
}
$profileByLoopId = @{}
foreach ($profile in @($catalog.loopProfiles)) {
    $loopId = [string] $profile.loopStableId
    Require-Text $loopId "ProfileLoopMissing"
    Require (-not $profileByLoopId.ContainsKey($loopId)) `
        "ProfileDuplicate:$loopId"
    Require ($loopById.ContainsKey($loopId)) "ProfileLoopUnknown:$loopId"
    Require ([string] $loopById[$loopId].loopLevelCode -eq "PlayableUnit") `
        "ProfileNotPlayableUnit:$loopId"
    $features = @($profile.featureCodes | ForEach-Object { [string] $_ })
    Require ($features.Count -eq @($features | Sort-Object -Unique).Count) `
        "ProfileFeatureDuplicate:$loopId"
    foreach ($featureCode in $features) {
        Require (@($catalog.featureCodes) -contains $featureCode) `
            "ProfileFeatureUnknown:${loopId}:$featureCode"
        Require (@($catalog.modules | Where-Object {
            [string] $_.applicabilityCode -eq "Feature" -and
            @($_.requiredFeatureCodes) -contains $featureCode }).Count -gt 0) `
            "ProfileFeatureUnmapped:${loopId}:$featureCode"
    }
    $profileByLoopId[$loopId] = $profile
}
$resolvedModules = @{}
$playableUnits = @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit")
foreach ($loop in $playableUnits) {
    $loopId = [string] $loop.loopStableId
    $features = if ($profileByLoopId.ContainsKey($loopId)) {
        @($profileByLoopId[$loopId].featureCodes | ForEach-Object { [string] $_ })
    }
    else { @() }
    $codes = [Collections.Generic.List[string]]::new()
    foreach ($commonCode in @($catalog.commonModuleCodes)) {
        $codes.Add([string] $commonCode)
    }
    foreach ($module in @($catalog.modules | Where-Object {
        [string] $_.applicabilityCode -eq "Feature"
    })) {
        $required = @($module.requiredFeatureCodes |
            ForEach-Object { [string] $_ })
        if (@($required | Where-Object { $features -notcontains $_ }).Count -eq 0) {
            $codes.Add([string] $module.moduleCode)
        }
    }
    $resolvedModules[$loopId] = @($codes | Sort-Object -Unique)
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# $($documentText.titleKo)")
[void] $builder.AppendLine()
[void] $builder.AppendLine(([string] $documentText.sourceNoticeFormatKo -f
    $CatalogPath, $PlayableLoopPath))
[void] $builder.AppendLine()
[void] $builder.AppendLine('대장·연결 검증은 Unity 실행이나 E 승격이 아니다. 파일 참조는 구현 위치이며 실제 동작·시험 통과를 뜻하지 않는다. `unity:` 참조는 -UnityProjectRoot를 제공해야 파일 존재를 대조한다. 모듈별 Passed는 기존 EvidencePackage의 대상·단계·판본·파일 hash를 확인하며 실제 품질 평가는 별도다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.catalogLabelKo, $catalog.revision))
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.commonCountLabelKo, @($catalog.commonModuleCodes).Count))
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.conditionalCountLabelKo,
    @($catalog.modules | Where-Object applicabilityCode -eq "Feature").Count))
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.playableUnitCountLabelKo, $playableUnits.Count))
[void] $builder.AppendLine()
[void] $builder.AppendLine("## $($documentText.commonGateHeadingKo)")
[void] $builder.AppendLine()
[void] $builder.AppendLine([string] $documentText.commonGateHeaderKo)
[void] $builder.AppendLine("| --- | --- | --- | --- |")
foreach ($commonCode in @($catalog.commonModuleCodes)) {
    $module = $moduleByCode[[string] $commonCode]
    [void] $builder.AppendLine(('| {0} | `{1}` {2} | {3} | {4} |' -f
        (Escape-Cell $module.evidenceStageCode),
        (Escape-Cell $module.moduleCode),
        (Escape-Cell $module.displayNameKo),
        (Escape-Cell $module.automationLevelCode),
        (Escape-Cell $module.severityCode)))
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## $($documentText.profileHeadingKo)")
[void] $builder.AppendLine()
[void] $builder.AppendLine([string] $documentText.profileHeaderKo)
[void] $builder.AppendLine("| --- | --- | --- |")
foreach ($loop in @($playableUnits | Sort-Object priority, loopStableId)) {
    $loopId = [string] $loop.loopStableId
    $features = if ($profileByLoopId.ContainsKey($loopId)) {
        @($profileByLoopId[$loopId].featureCodes) -join ", "
    }
    else { [string] $documentText.commonOnlyKo }
    [void] $builder.AppendLine(('| `{0}` | {1} | {2} |' -f
        (Escape-Cell $loopId), (Escape-Cell $features),
        (Escape-Cell (@($resolvedModules[$loopId]) -join ", "))))
}

if ($hasImplementationContract) {
    [void] $builder.AppendLine()
    [void] $builder.AppendLine('## 단계별 최소 구현 책임')
    [void] $builder.AppendLine()
    [void] $builder.AppendLine('| E / 모듈 | 입력 → 출력 | 재사용 구현 참조 | 시험 참조 |')
    [void] $builder.AppendLine('| --- | --- | --- | --- |')
    foreach ($module in @($catalog.modules | Sort-Object evidenceStageCode,moduleCode)) {
        [void] $builder.AppendLine(('| {0} / `{1}` | {2} → {3} | {4} | {5} |' -f
            $module.evidenceStageCode, $module.moduleCode,
            (Escape-Cell (@($module.reads) -join ', ')), (Escape-Cell (@($module.outputs) -join ', ')),
            (Escape-Cell (@($module.implementationRefs) -join '<br>')),
            (Escape-Cell (@($module.testRefs) -join '<br>'))))
    }
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 작업 명세의 구현·증거 연결')
[void] $builder.AppendLine()
[void] $builder.AppendLine('미연결 과거 명세는 Unverified이며, 기존 E를 변경하지 않는다. 이 표는 명세에 기록된 범위만 다루며 한 WI의 준비를 전체 폐루프 완료로 올리지 않는다.')
foreach ($loop in @($playableUnits | Sort-Object priority,loopStableId)) {
    $loopId = [string] $loop.loopStableId
    $profile = if ($profileByLoopId.ContainsKey($loopId)) { $profileByLoopId[$loopId] } else { $null }
    $referenceProperty = if ($null -ne $profile) { $profile.PSObject.Properties['workOrderRef'] } else { $null }
    if ($null -eq $referenceProperty) {
        [void] $builder.AppendLine()
        [void] $builder.AppendLine(('`{0}`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.' -f $loopId))
        continue
    }
    $workOrderPath = Resolve-PresentationModuleReference ('repo:' + $referenceProperty.Value) $repositoryRoot '' $true
    $workOrder = Get-Content -LiteralPath $workOrderPath -Raw -Encoding UTF8 | ConvertFrom-Json
    Require ($workOrder.playableUnitStableId -eq $loopId) "WorkOrderLoopMismatch:$loopId"
    $bindingRows = @(Test-PresentationModuleBindings $workOrder $catalog $repositoryRoot $UnityProjectRoot)
    [void] $builder.AppendLine()
    [void] $builder.AppendLine(('### `{0}`' -f $loopId))
    [void] $builder.AppendLine()
    [void] $builder.AppendLine(('명세: `{0}`; 현재 Logic {1} / Presentation {2} / 통합 {3}. 자동 승격 없음.' -f
        $referenceProperty.Value, $workOrder.trackPlans.logic.currentEvidenceStage,
        $workOrder.trackPlans.presentation.currentEvidenceStage, $workOrder.currentEvidenceStage))
    [void] $builder.AppendLine()
    [void] $builder.AppendLine('| E / 필요 모듈 | 검증 상태 | 구현 / 시험 참조 | 증거 ID | 남은 문제·범위 |')
    [void] $builder.AppendLine('| --- | --- | --- | --- | --- |')
    foreach ($row in $bindingRows) {
        [void] $builder.AppendLine(('| {0} / `{1}` | {2} | {3} | {4} | {5} |' -f
            $row.evidenceStageCode, $row.moduleCode, $row.statusCode,
            (Escape-Cell ((@($row.implementationRefs) + @($row.testRefs)) -join '<br>')),
            (Escape-Cell (@($row.evidenceRefs) -join ', ')), (Escape-Cell $row.reason)))
    }
}

$generated = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    $directory = Split-Path -Parent $resolvedOutput
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    Write-DeterministicTextIfChanged $resolvedOutput $generated | Out-Null
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    $existing = Get-Content -LiteralPath $resolvedOutput -Raw -Encoding UTF8
    Require ((ConvertTo-DeterministicText $existing) -ceq $generated) "GeneratedOutputOutOfDate"
}

Write-Output ("PlayableLoopPresentationValidationValid:Modules={0};Profiles={1};PlayableUnits={2}" -f
    @($catalog.modules).Count, @($catalog.loopProfiles).Count,
    $playableUnits.Count)
