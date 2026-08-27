[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",

    [string] $CatalogPath =
        "eng/execution-ledgers/playable-loop-engine-interaction-validation.json",

    [string] $PlayableLoopPath =
        "eng/execution-ledgers/playable-loops.json",

    [string] $OutputPath =
        "docs/AI/generated/playable-loop-engine-interaction-validation.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "EngineInteractionValidationInvalid:$Code" }
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

Require ([string] $catalog.schemaVersion -eq
    "playable-loop-engine-interaction-validation.v2") "SchemaInvalid"
foreach ($principle in @(
    "engineInteractionIsIntegratedGateNotThirdMaturityTrack",
    "simulationAuthorityPrecedesPresentation",
    "actionJournalIsRequiredForAuthorityChange",
    "playerProgressionIsConditionalWithReason",
    "presentationReadsActionJournalByCursor",
    "presentationMustPreserveAuthorityRevision",
    "traceIsExcludedFromSaveReplayCanonicalHash",
    "actualPlayModeEvidenceRemainsSeparate",
    "localProcessAndRemoteHostShareTheSameProfile")) {
    $property = $catalog.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) `
        "PrincipleMissing:$principle"
}

$componentByCode = @{}
foreach ($component in @($catalog.components)) {
    $code = [string] $component.componentCode
    Require-Text $code "ComponentCodeMissing"
    Require (-not $componentByCode.ContainsKey($code)) "ComponentDuplicate:$code"
    Require-Text $component.componentKindCode "ComponentKindMissing:$code"
    Require-Text $component.componentRevision "ComponentRevisionMissing:$code"
    if ([bool] $component.mayMutateAuthority) {
        Require ([string] $component.componentKindCode -eq "Authority") `
            "NonAuthorityMutation:$code"
    }
    if ([string] $component.componentKindCode -eq "Presentation") {
        Require (-not [bool] $component.mayMutateAuthority) `
            "PresentationMutation:$code"
    }
    $componentByCode[$code] = $component
}

$loopById = @{}
foreach ($loop in @($loops.items)) {
    $loopById[[string] $loop.loopStableId] = $loop
}
$overrideByKey = @{}
foreach ($override in @($catalog.profileOverrides)) {
    $key = "{0}|{1}" -f $override.loopStableId, $override.worldInteractionId
    Require (-not $overrideByKey.ContainsKey($key)) "ProfileOverrideDuplicate:$key"
    $overrideByKey[$key] = $override
}

$playableUnits = @($loops.items | Where-Object {
    [string] $_.loopLevelCode -eq
        [string] $catalog.profileDerivation.requiredLoopLevelCode
})
Require ($playableUnits.Count -eq
    [int] $catalog.profileDerivation.expectedPlayableUnitCount) `
    "PlayableUnitCountMismatch"
$allProfiles = [Collections.Generic.List[object]]::new()
foreach ($loop in $playableUnits) {
    $loopId = [string] $loop.loopStableId
    $areaCode = (($loopId -replace '^playable-loop:', '') -split '-')[0]
    $areaProperty = $catalog.profileDerivation.areaDefaults.PSObject.Properties[
        $areaCode]
    Require ($null -ne $areaProperty) "AreaDefaultMissing:$areaCode"
    foreach ($wiIdValue in @($loop.worldInteractionIds)) {
        $wiId = [string] $wiIdValue
        $key = "$loopId|$wiId"
        $override = if ($overrideByKey.ContainsKey($key)) {
            $overrideByKey[$key]
        } else { $null }
        $consumers = if ($null -ne $override) {
            @($override.presentationConsumerCodes)
        } else { @($areaProperty.Value.presentationConsumerCodes) }
        $changes = if ($null -ne $override) {
            @($override.changeSemanticCodes)
        } else { @($areaProperty.Value.changeSemanticCodes) }
        Require ($consumers.Count -gt 0) "PresentationConsumersMissing:$key"
        Require ($changes.Count -gt 0) "ChangeSemanticsMissing:$key"
        $requirements = [Collections.Generic.List[object]]::new()
        foreach ($base in @($catalog.profileDerivation.baseRequirements)) {
            $requirements.Add([pscustomobject]@{
                order = [int] $base.order
                componentCode = [string] $base.componentCode
                phaseCode = [string] $base.phaseCode
                trackCode = [string] $base.trackCode
                reopenStage = [string] $base.reopenStage
                allowsReused = [bool] $base.allowsReused
                allowsNotApplicable = [bool] $base.allowsNotApplicable
            })
        }
        $order = 70
        foreach ($consumer in $consumers) {
            $phaseMap = $catalog.profileDerivation.presentationPhaseByComponent
            $phaseProperty = $phaseMap.PSObject.Properties[[string] $consumer]
            Require ($null -ne $phaseProperty) `
                "PresentationPhaseMissing:${key}:$consumer"
            $requirements.Add([pscustomobject]@{
                order = $order
                componentCode = [string] $consumer
                phaseCode = "ActionRecordRead"
                trackCode = "Presentation"
                reopenStage = "E5"
                allowsReused = $true
                allowsNotApplicable = $false
            })
            $order += 10
            $requirements.Add([pscustomobject]@{
                order = $order
                componentCode = [string] $consumer
                phaseCode = [string] $phaseProperty.Value
                trackCode = "Presentation"
                reopenStage = "E5"
                allowsReused = $true
                allowsNotApplicable = $false
            })
            $order += 10
        }
        $allProfiles.Add([pscustomobject]@{
            loopStableId = $loopId
            worldInteractionId = $wiId
            displayNameKo = if ($null -ne $override) {
                [string] $override.displayNameKo
            } else { [string] $loop.title }
            profileRevision = if ($null -ne $override) {
                [string] $override.profileRevision
            } else { [string] $catalog.profileDerivation.profileRevision }
            changeSemanticCodes = $changes
            presentationConsumerCodes = $consumers
            requirements = $requirements.ToArray()
        })
    }
}
Require ($allProfiles.Count -eq
    [int] $catalog.profileDerivation.expectedLoopWiPairCount) `
    "LoopWiPairCountMismatch"

$profileKeys = @{}
foreach ($profile in $allProfiles) {
    $loopId = [string] $profile.loopStableId
    $wiId = [string] $profile.worldInteractionId
    $key = "$loopId|$wiId"
    Require-Text $profile.profileRevision "ProfileRevisionMissing:$key"
    Require-Text $profile.displayNameKo "ProfileNameMissing:$key"
    Require (-not $profileKeys.ContainsKey($key)) "ProfileDuplicate:$key"
    Require ($loopById.ContainsKey($loopId)) "LoopUnknown:$loopId"
    Require (@($loopById[$loopId].worldInteractionIds) -contains $wiId) `
        "WorldInteractionNotInLoop:$key"
    $orders = @($profile.requirements | ForEach-Object { [int] $_.order })
    Require ($orders.Count -gt 0) "RequirementsMissing:$key"
    Require ($orders.Count -eq @($orders | Sort-Object -Unique).Count) `
        "RequirementOrderDuplicate:$key"
    $sortedOrdersText = (@($orders | Sort-Object) -join ",")
    $declaredOrdersText = ($orders -join ",")
    Require ($sortedOrdersText -eq $declaredOrdersText) `
        "RequirementOrderInvalid:$key"
    foreach ($requirement in @($profile.requirements)) {
        $componentCode = [string] $requirement.componentCode
        Require ($componentByCode.ContainsKey($componentCode)) `
            "RequirementComponentUnknown:${key}:$componentCode"
        Require-Text $requirement.phaseCode `
            "RequirementPhaseMissing:${key}:$componentCode"
        Require (@("Logic", "Presentation") -contains
            [string] $requirement.trackCode) `
            "RequirementTrackInvalid:${key}:$componentCode"
        Require ([string] $requirement.reopenStage -match '^E[1-7]$') `
            "RequirementReopenStageInvalid:${key}:$componentCode"
    }
    $authorityOrder = [int] (@($profile.requirements | Where-Object {
        [string] $_.componentCode -eq "Simulation.AuthorityCore"
    })[0].order)
    $actionOrder = [int] (@($profile.requirements | Where-Object {
        [string] $_.componentCode -eq "Simulation.ActionJournal"
    })[0].order)
    $progressOrder = [int] (@($profile.requirements | Where-Object {
        [string] $_.componentCode -eq "Simulation.PlayerDomainProgression"
    })[0].order)
    Require ($actionOrder -gt $authorityOrder) "ActionBeforeAuthority:$key"
    Require ($progressOrder -gt $actionOrder) "ProgressBeforeAction:$key"
    foreach ($presentationStep in @($profile.requirements | Where-Object {
        [string] $componentByCode[[string] $_.componentCode].componentKindCode -eq
            "Presentation"
    })) {
        Require ([int] $presentationStep.order -gt $actionOrder) `
            "PresentationBeforeAuthority:${key}:$($presentationStep.componentCode)"
    }
    $profileKeys[$key] = $profile
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# PlayableLoop 엔진 상호작용 검증 상태")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> ``$CatalogPath``와 ``$PlayableLoopPath``에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 대장 revision: ``$($catalog.revision)``")
[void] $builder.AppendLine("- 성숙도 축: 기존 ``Logic``·``Presentation`` 유지")
[void] $builder.AppendLine("- 엔진 상호작용: 두 궤적을 같은 WI·Command·Revision으로 묶는 통합 관문")
[void] $builder.AppendLine("- Save/Replay canonical hash 포함: ``false``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 구성 요소")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 구성 요소 | 종류 | 권위 변경 | revision |")
[void] $builder.AppendLine("| --- | --- | --- | --- |")
foreach ($component in @($catalog.components)) {
    [void] $builder.AppendLine(("| ``{0}`` | {1} | {2} | ``{3}`` |" -f
        (Escape-Cell $component.componentCode),
        (Escape-Cell $component.componentKindCode),
        ([bool] $component.mayMutateAuthority).ToString().ToLowerInvariant(),
        (Escape-Cell $component.componentRevision)))
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 전체 PlayableUnit 프로필")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 폐루프 | WI | 순서 |")
[void] $builder.AppendLine("| --- | --- | --- |")
foreach ($profile in $allProfiles) {
    $sequence = @($profile.requirements | ForEach-Object {
        "{0}:{1}" -f $_.componentCode, $_.phaseCode }) -join " → "
    [void] $builder.AppendLine(("| ``{0}`` | ``{1}`` {2} | {3} |" -f
        (Escape-Cell $profile.loopStableId),
        (Escape-Cell $profile.worldInteractionId),
        (Escape-Cell $profile.displayNameKo), (Escape-Cell $sequence)))
}

$generated = $builder.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    $directory = Split-Path -Parent $resolvedOutput
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    [IO.File]::WriteAllText($resolvedOutput, $generated,
        [Text.UTF8Encoding]::new($false))
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    $existing = Get-Content -LiteralPath $resolvedOutput -Raw -Encoding UTF8
    Require ($existing -eq $generated) "GeneratedOutputOutOfDate"
}

Write-Output ("PlayableLoopEngineInteractionValidationValid:Components={0};Profiles={1}" -f
    @($catalog.components).Count, $allProfiles.Count)
