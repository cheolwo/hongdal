[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",

    [string] $CatalogPath =
        "eng/execution-ledgers/playable-loop-synty-expression-modules.json",

    [string] $OutputPath =
        "docs/AI/generated/playable-loop-synty-expression-modules.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PlayableLoopSyntyModuleInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedCatalog = Join-Path $repositoryRoot $CatalogPath
$catalog = Get-Content -LiteralPath $resolvedCatalog -Raw -Encoding UTF8 |
    ConvertFrom-Json
$resolvedLoops = Join-Path $repositoryRoot ([string] $catalog.playableLoopCatalogPath)
$resolvedWis = Join-Path $repositoryRoot ([string] $catalog.worldInteractionCatalogPath)
$loops = Get-Content -LiteralPath $resolvedLoops -Raw -Encoding UTF8 | ConvertFrom-Json
$wis = Get-Content -LiteralPath $resolvedWis -Raw -Encoding UTF8 | ConvertFrom-Json
$assetModules = Get-Content -LiteralPath (Join-Path $repositoryRoot `
    "eng/execution-ledgers/synty-asset-functional-modules.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$functionalModuleCodes = @($assetModules.functionalModules |
    ForEach-Object { [string] $_.moduleCode })

Require ([string] $catalog.schemaVersion -eq
    "playable-loop-synty-expression-modules.v1") "SchemaInvalid"
foreach ($principle in @(
    "playableUnitOwnsExpressionModule",
    "loopMomentAndPlacementRoleReplaceForcedVariantTriples",
    "assetFamilyPrecedesPrefabSelection",
    "selectionIsDeterministic",
    "presentationDoesNotMutateSimulationAuthority",
    "missingCandidateIsExplicitInsteadOfForced",
    "legacyCompositionKeysAreReadOnlyCompatibilityInputs")) {
    $property = $catalog.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) `
        "PrincipleMissing:$principle"
}

$legacy = $catalog.legacyCompositionPolicy
Require ([string] $legacy.statusCode -eq "LegacyGenerated") "LegacyStatusInvalid"
Require (-not [bool] $legacy.canonicalForNewWork) "LegacyStillCanonical"
Require (-not [bool] $legacy.newGenerationAllowed) "LegacyGenerationStillAllowed"
Require ([bool] $legacy.readCompatibilityRequired) "LegacyReadCompatibilityMissing"
Require (-not [bool] $legacy.forcedVariantCompletenessRequired) `
    "LegacyVariantCompletenessStillRequired"
Require (@($legacy.cleanupGateCodes).Count -ge 4) "LegacyCleanupGatesMissing"

$allowedMoments = @($catalog.allowedLoopMomentCodes | ForEach-Object { [string] $_ })
$allowedRoles = @($catalog.allowedPlacementRoleCodes | ForEach-Object { [string] $_ })
Require ($allowedMoments.Count -eq @($allowedMoments | Sort-Object -Unique).Count) `
    "MomentDuplicate"
Require ($allowedRoles.Count -eq @($allowedRoles | Sort-Object -Unique).Count) `
    "RoleDuplicate"

$packByCode = @{}
foreach ($pack in @($catalog.packPolicies)) {
    $code = [string] $pack.packCode
    Require-Text $code "PackCodeMissing"
    Require (-not $packByCode.ContainsKey($code)) "PackDuplicate:$code"
    Require-Text $pack.usagePolicyCode "PackPolicyMissing:$code"
    foreach ($role in @($pack.defaultRoles)) {
        Require ($allowedRoles -contains [string] $role) "PackRoleInvalid:${code}:$role"
    }
    $packByCode[$code] = $pack
}
foreach ($requiredPack in @(
    "nature", "farm", "town", "city", "construction", "generic", "starter")) {
    Require ($packByCode.ContainsKey($requiredPack)) "PackPolicyMissing:$requiredPack"
}
Require ([string] $packByCode["starter"].usagePolicyCode -eq
    "PrototypeFallbackOnly") "StarterMustRemainFallback"

$sharedById = @{}
foreach ($shared in @($catalog.sharedModules)) {
    $id = [string] $shared.moduleStableId
    Require-Text $id "SharedModuleIdMissing"
    Require (-not $sharedById.ContainsKey($id)) "SharedModuleDuplicate:$id"
    Require-Text $shared.displayNameKo "SharedModuleNameMissing:$id"
    Require (@($shared.placementRoleCodes).Count -gt 0) "SharedModuleRoleMissing:$id"
    foreach ($role in @($shared.placementRoleCodes)) {
        Require ($allowedRoles -contains [string] $role) "SharedModuleRoleInvalid:${id}:$role"
    }
    $sharedById[$id] = $shared
}

$loopById = @{}
foreach ($loop in @($loops.items)) {
    $loopById[[string] $loop.loopStableId] = $loop
}
$wiIds = @($wis.items | ForEach-Object { [string] $_.id })
$moduleByLoop = @{}
$slotIds = @{}
$familyIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)

foreach ($module in @($catalog.loopModules)) {
    $moduleId = [string] $module.moduleStableId
    $loopId = [string] $module.loopStableId
    Require-Text $moduleId "ModuleIdMissing"
    Require-Text $loopId "ModuleLoopMissing:$moduleId"
    Require-Text $module.moduleRevision "ModuleRevisionMissing:$moduleId"
    Require (-not $moduleByLoop.ContainsKey($loopId)) "ModuleLoopDuplicate:$loopId"
    Require ($loopById.ContainsKey($loopId)) "ModuleLoopUnknown:$loopId"
    $loop = $loopById[$loopId]
    Require ([string] $loop.loopLevelCode -eq "PlayableUnit") "ModuleLoopNotPlayableUnit:$loopId"

    foreach ($sharedRef in @($module.sharedModuleRefs)) {
        Require ($sharedById.ContainsKey([string] $sharedRef)) `
            "SharedModuleRefUnknown:${moduleId}:$sharedRef"
    }

    $declaredWis = @($module.worldInteractionIds | ForEach-Object { [string] $_ })
    $loopWis = @($loop.worldInteractionIds | ForEach-Object { [string] $_ })
    Require ($declaredWis.Count -eq @($declaredWis | Sort-Object -Unique).Count) `
        "ModuleWiDuplicate:$moduleId"
    Require ((@($declaredWis | Sort-Object) -join "|") -eq
        (@($loopWis | Sort-Object) -join "|")) "ModuleWiMismatch:$moduleId"
    foreach ($wiId in $declaredWis) {
        Require ($wiIds -contains $wiId) "ModuleWiUnknown:${moduleId}:$wiId"
    }

    $declaredH = @($module.requiredHCapabilities | ForEach-Object { [string] $_ })
    $loopH = @($loop.requiredHCapabilities | ForEach-Object { [string] $_ })
    Require ((@($declaredH | Sort-Object) -join "|") -eq
        (@($loopH | Sort-Object) -join "|")) "ModuleHCapabilityMismatch:$moduleId"

    $coveredWis = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($slot in @($module.slots)) {
        $slotId = [string] $slot.slotStableId
        $wiId = [string] $slot.worldInteractionId
        Require-Text $slotId "SlotIdMissing:$moduleId"
        Require (-not $slotIds.ContainsKey($slotId)) "SlotDuplicate:$slotId"
        Require ($declaredWis -contains $wiId) "SlotWiOutsideModule:${slotId}:$wiId"
        Require ($allowedMoments -contains [string] $slot.loopMomentCode) `
            "SlotMomentInvalid:$slotId"
        Require ($allowedRoles -contains [string] $slot.placementRoleCode) `
            "SlotRoleInvalid:$slotId"
        $roleModules = $assetModules.placementRoleModuleCodes.PSObject.Properties[
            [string] $slot.placementRoleCode]
        Require ($null -ne $roleModules) "SlotAssetModuleMappingMissing:$slotId"
        foreach ($moduleCode in @($roleModules.Value)) {
            Require ($functionalModuleCodes -contains [string] $moduleCode) `
                "SlotAssetModuleUnknown:${slotId}:$moduleCode"
        }
        Require (@($slot.authorityStateCodes).Count -gt 0) "SlotStateMissing:$slotId"
        $candidates = @($slot.assetFamilyIds | ForEach-Object { [string] $_ })
        Require ($candidates.Count -gt 0) "SlotCandidateMissing:$slotId"
        Require ($candidates.Count -eq @($candidates | Sort-Object -Unique).Count) `
            "SlotCandidateDuplicate:$slotId"
        foreach ($familyId in $candidates) {
            Require ($familyId -match '^synty-family:([^:]+):[^:]+:.+$') `
                "AssetFamilyIdInvalid:${slotId}:$familyId"
            $packCode = $Matches[1]
            Require ($packByCode.ContainsKey($packCode)) `
                "AssetFamilyPackUnknown:${slotId}:$packCode"
            Require ([string] $packByCode[$packCode].usagePolicyCode -ne
                "PrototypeFallbackOnly") "PrototypeAssetInProductionSlot:${slotId}:$familyId"
            [void] $familyIds.Add($familyId)
        }
        $slotIds[$slotId] = $slot
        [void] $coveredWis.Add($wiId)
    }
    Require (@($declaredWis | Where-Object { -not $coveredWis.Contains($_) }).Count -eq 0) `
        "ModuleWiSlotCoverageMissing:$moduleId"
    $moduleByLoop[$loopId] = $module
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# PlayableLoop Synty 표현 모듈 상태")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> ``$CatalogPath``와 ``$($catalog.playableLoopCatalogPath)``에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 대장 revision: ``$($catalog.revision)``")
[void] $builder.AppendLine("- 폐루프 모듈: ``$(@($catalog.loopModules).Count)``")
[void] $builder.AppendLine("- 공유 모듈: ``$(@($catalog.sharedModules).Count)``")
[void] $builder.AppendLine("- 표현 슬롯: ``$($slotIds.Count)``")
[void] $builder.AppendLine("- 사용 자산 계열: ``$($familyIds.Count)``")
[void] $builder.AppendLine("- 기존 A/B/C 기준 문법: ``LegacyGenerated / 신규 생성 금지``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 폐루프 | 모듈 | WI | 슬롯 | 공유 모듈 |")
[void] $builder.AppendLine("| --- | --- | ---: | ---: | --- |")
foreach ($module in @($catalog.loopModules)) {
    [void] $builder.AppendLine(('| `{0}` | `{1}` | {2} | {3} | {4} |' -f
        (Escape-Cell $module.loopStableId),
        (Escape-Cell $module.moduleStableId),
        @($module.worldInteractionIds).Count,
        @($module.slots).Count,
        (Escape-Cell (@($module.sharedModuleRefs) -join ", "))))
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 팩 사용 정책")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 팩 | 정책 | 기본 역할 |")
[void] $builder.AppendLine("| --- | --- | --- |")
foreach ($pack in @($catalog.packPolicies)) {
    [void] $builder.AppendLine(('| `{0}` | `{1}` | {2} |' -f
        (Escape-Cell $pack.packCode),
        (Escape-Cell $pack.usagePolicyCode),
        (Escape-Cell (@($pack.defaultRoles) -join ", "))))
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

Write-Output ("PlayableLoopSyntyModulesValid:Loops={0};Shared={1};Slots={2};Families={3}" -f
    @($catalog.loopModules).Count, @($catalog.sharedModules).Count,
    $slotIds.Count, $familyIds.Count)
