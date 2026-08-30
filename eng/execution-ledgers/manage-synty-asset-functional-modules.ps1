[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",
    [string] $CatalogPath = "eng/execution-ledgers/synty-asset-functional-modules.json",
    [string] $OutputPath = "docs/AI/generated/synty-asset-functional-modules.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "SyntyAssetFunctionalModulesInvalid:$Code" }
}

$root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$catalog = Get-Content -LiteralPath (Join-Path $root $CatalogPath) -Raw -Encoding UTF8 |
    ConvertFrom-Json
Require ([string] $catalog.schemaVersion -eq "synty-asset-functional-modules.v1") "Schema"

foreach ($name in @(
    "sourcePackIsProvenanceNotGameplayModule",
    "everyPrefabHasModuleOrHoldReason",
    "constructionIsSharedStateLayerNotArea",
    "genericIsSharedBase",
    "starterIsPrototypeFallbackOnly",
    "playableLoopSelectsModuleBeforeAssetFamily",
    "humanDesignUsesKoreanTaxonomyAndStableCodes",
    "presentationCannotMutateAuthorityRevision",
    "animationClipsUseSeparateSourceProfiles",
    "purchasedPackDoesNotCreateGameplayArea",
    "newPackStartsAsNeedsReview")) {
    Require ([bool] $catalog.principles.$name) "Principle:$name"
}

$packs = @($catalog.sourcePacks)
Require ($packs.Count -eq 13) "PackCount"
Require ((@($packs.expectedPrefabCount | Measure-Object -Sum).Sum) -eq
    [int] $catalog.expectedPrefabCount) "PrefabCountSum"
Require ([int] $catalog.expectedPrefabCount -eq 4211) "ExpectedPrefabCount"
Require ([string] ($packs | Where-Object packCode -eq "construction").policyCode -eq
    "SharedConstructionStateLayer") "ConstructionPolicy"
Require ([string] ($packs | Where-Object packCode -eq "generic").policyCode -eq
    "SharedBase") "GenericPolicy"
Require ([string] ($packs | Where-Object packCode -eq "starter").policyCode -eq
    "PrototypeFallbackOnly") "StarterPolicy"

$purchasedProfiles = @($catalog.purchasedAssetUsageProfiles)
Require ($purchasedProfiles.Count -eq 6) "PurchasedProfileCount"
foreach ($profile in $purchasedProfiles) {
    $packCode = [string] $profile.packCode
    Require (@($packs | Where-Object packCode -eq $packCode).Count -eq 1) "PurchasedProfilePack:$packCode"
    Require (@("Prefab", "AnimationClip") -contains [string] $profile.sourceKindCode) `
        "PurchasedProfileKind:$packCode"
    Require (@($profile.requiredHCapabilities).Count -gt 0) "PurchasedProfileH:$packCode"
    Require (@($profile.playableLoopCandidateCodes).Count -gt 0) "PurchasedProfileLoop:$packCode"
}

$modules = @($catalog.functionalModules)
Require ($modules.Count -eq 12) "ModuleCount"
$moduleCodes = @($modules | ForEach-Object { [string] $_.moduleCode })
Require (@($moduleCodes | Sort-Object -Unique).Count -eq 12) "ModuleDuplicate"

$taxonomyPath = [string] $catalog.humanTaxonomyCatalogPath
Require (-not [string]::IsNullOrWhiteSpace($taxonomyPath)) "HumanTaxonomyPathMissing"
$taxonomy = Get-Content -LiteralPath (Join-Path $root $taxonomyPath) -Raw -Encoding UTF8 |
    ConvertFrom-Json
Require ([string] $taxonomy.schemaVersion -eq "synty-asset-human-taxonomy.v1") "HumanTaxonomySchema"
Require (@($taxonomy.계층순서).Count -eq 6) "HumanTaxonomyDepth"
foreach ($principleName in @(
    "사람이읽는이름은한국어",
    "저장StableCode는영문",
    "자산계열과Prefab은실제대장Entry에서연결",
    "Prefab경로는세계상태StableId가아님")) {
    Require ([bool] $taxonomy.설계원칙.$principleName) "HumanTaxonomyPrinciple:$principleName"
}
$scopes = @($taxonomy.표현범위)
Require ($scopes.Count -eq 3) "HumanTaxonomyScopeCount"
$expectedScopeNames = @{
    Outdoor = "실외 표현"
    Interior = "실내 표현"
    Shared = "공통 표현"
}
$taxonomyModules = @()
$subgroupCount = 0
$assetKindCount = 0
foreach ($scope in $scopes) {
    $scopeCode = [string] $scope.범위Code
    Require ($expectedScopeNames.ContainsKey($scopeCode)) "HumanTaxonomyScopeUnknown:$scopeCode"
    Require ([string] $scope.범위이름 -eq $expectedScopeNames[$scopeCode]) `
        "HumanTaxonomyScopeName:$scopeCode"
    foreach ($module in @($scope.기능군)) {
        $taxonomyModules += [pscustomobject]@{
            ModuleCode = [string] $module.기능군Code
            DisplayNameKo = [string] $module.기능군이름
            ScopeCode = $scopeCode
        }
        $subgroups = @($module.세부기능군)
        Require ($subgroups.Count -gt 0) "HumanTaxonomySubgroupMissing:$($module.기능군Code)"
        $subgroupCount += $subgroups.Count
        foreach ($subgroup in $subgroups) {
            Require (-not [string]::IsNullOrWhiteSpace([string] $subgroup.세부기능군Code)) `
                "HumanTaxonomySubgroupCodeMissing:$($module.기능군Code)"
            Require (-not [string]::IsNullOrWhiteSpace([string] $subgroup.세부기능군이름)) `
                "HumanTaxonomySubgroupNameMissing:$($module.기능군Code)"
            $assetKinds = @($subgroup.자산종류)
            Require ($assetKinds.Count -gt 0) "HumanTaxonomyAssetKindMissing:$($subgroup.세부기능군Code)"
            $assetKindCount += $assetKinds.Count
            foreach ($assetKind in $assetKinds) {
                Require (-not [string]::IsNullOrWhiteSpace([string] $assetKind.자산종류Code)) `
                    "HumanTaxonomyAssetKindCodeMissing:$($subgroup.세부기능군Code)"
                Require (-not [string]::IsNullOrWhiteSpace([string] $assetKind.자산종류이름)) `
                    "HumanTaxonomyAssetKindNameMissing:$($subgroup.세부기능군Code)"
            }
        }
    }
}
Require ($taxonomyModules.Count -eq 12) "HumanTaxonomyModuleCount"
Require (@($taxonomyModules.ModuleCode | Sort-Object -Unique).Count -eq 12) `
    "HumanTaxonomyModuleDuplicate"
foreach ($module in $modules) {
    $match = @($taxonomyModules | Where-Object ModuleCode -eq ([string] $module.moduleCode))
    Require ($match.Count -eq 1) "HumanTaxonomyModuleMissing:$($module.moduleCode)"
    Require ([string] $match[0].DisplayNameKo -eq [string] $module.displayNameKo) `
        "HumanTaxonomyModuleNameMismatch:$($module.moduleCode)"
    Require ([string] $match[0].ScopeCode -eq [string] $module.scopeCode) `
        "HumanTaxonomyModuleScopeMismatch:$($module.moduleCode)"
}
foreach ($roleProperty in @($catalog.placementRoleModuleCodes.PSObject.Properties)) {
    $refs = @($roleProperty.Value | ForEach-Object { [string] $_ })
    Require ($refs.Count -gt 0) "RoleModuleMissing:$($roleProperty.Name)"
    foreach ($ref in $refs) { Require ($moduleCodes -contains $ref) "RoleModuleUnknown:$ref" }
}

$loops = Get-Content -LiteralPath (Join-Path $root "eng/execution-ledgers/playable-loops.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$playableIds = @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit" |
    ForEach-Object { [string] $_.loopStableId })
foreach ($area in @($catalog.areaModuleSkeletons)) {
    Require (@($area.loopStableIds).Count -gt 0) "AreaLoopMissing:$($area.areaCode)"
    foreach ($loopId in @($area.loopStableIds)) {
        Require ($playableIds -contains [string] $loopId) "AreaLoopUnknown:$loopId"
    }
    foreach ($moduleCode in @($area.functionalModuleCodes)) {
        Require ($moduleCodes -contains [string] $moduleCode) "AreaModuleUnknown:$moduleCode"
    }
}

Require (-not [bool] $catalog.legacyCompositionPolicy.newAuthoringAllowed) "LegacyAuthoringEnabled"
Require (@($catalog.legacyCompositionPolicy.removalGateCodes).Count -eq 4) "LegacyRemovalGates"

$lines = @(
    "# Synty 자산 기능 체계",
    "",
    "> ``$CatalogPath``에서 자동 생성된다. Unity의 전수 Prefab 판정은 EditMode 시험이 검증한다.",
    "",
    "- revision: ``$($catalog.revision)``",
    "- 현재 원본 Prefab 기준 수량: ``$($catalog.expectedPrefabCount)``",
    "- 원본 팩: ``$($packs.Count)``",
    "- 신규 구매 활용 프로필: ``$($purchasedProfiles.Count)`` (Prefab과 AnimationClip 분리)",
    "- 기능군: ``$($modules.Count)``",
    "- 사람이 읽는 표현 범위: ``$($scopes.Count)`` (실외 표현 / 실내 표현 / 공통 표현)",
    "- 세부 기능군: ``$subgroupCount``",
    "- 자산 종류: ``$assetKindCount``",
    "- 업무 영역 골격: ``$(@($catalog.areaModuleSkeletons).Count)``",
    "- 기존 156개 A/B/C: ``LegacyGenerated / 신규 작성 금지 / 읽기 호환``",
    "",
    "| 원본 팩 | Prefab | 정책 |",
    "| --- | ---: | --- |"
)
foreach ($pack in $packs) {
    $lines += "| $($pack.packCode) | $($pack.expectedPrefabCount) | $($pack.policyCode) |"
}
$lines += @(
    "",
    "## 신규 구매 자산의 WI·H 활용 후보",
    "",
    "> 새 팩은 새 Area가 아니다. 아래 연결은 E4 후보 조사 입력이며 실제 채택과 E5 배치를 승인하지 않는다.",
    "",
    "| 원본 팩 | 원천 종류 | H Capability 후보 | PlayableLoop 후보 |",
    "| --- | --- | --- | --- |"
)
foreach ($profile in $purchasedProfiles) {
    $lines += "| $($profile.packCode) | $($profile.sourceKindCode) | " +
        "$(@($profile.requiredHCapabilities) -join ', ') | " +
        "$(@($profile.playableLoopCandidateCodes) -join ', ') |"
}
$lines += @(
    "",
    "## 사람이 읽는 한국어 분류",
    "",
    "> 설계·문서·대장은 한국어 이름을 먼저 사용한다. 괄호 안 영문은 저장과 호환을 위한 Stable Code다.",
    "",
    "분류 순서: ``$(@($taxonomy.계층순서) -join ' → ')``",
    ""
)
foreach ($scope in $scopes) {
    $lines += "### $($scope.범위이름) (``$($scope.범위Code)``)"
    $lines += ""
    foreach ($module in @($scope.기능군)) {
        $lines += "- $($module.기능군이름) (``$($module.기능군Code)``)"
        foreach ($subgroup in @($module.세부기능군)) {
            $assetKinds = @($subgroup.자산종류 | ForEach-Object {
                "$($_.자산종류이름) (``$($_.자산종류Code)``)"
            }) -join ", "
            $lines += "  - $($subgroup.세부기능군이름) (``$($subgroup.세부기능군Code)``): $assetKinds"
        }
    }
    $lines += ""
}
$generated = (($lines -join "`n").TrimEnd()) + "`n"
$output = Join-Path $root $OutputPath
if ($Mode -eq "Write") {
    $directory = Split-Path -Parent $output
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    [IO.File]::WriteAllText($output, $generated, [Text.UTF8Encoding]::new($false))
} else {
    Require (Test-Path -LiteralPath $output) "GeneratedOutputMissing"
    Require ((Get-Content -LiteralPath $output -Raw -Encoding UTF8) -eq $generated) "GeneratedOutputOutOfDate"
}

Write-Output "SyntyAssetFunctionalModulesValid:Packs=13;Prefabs=4211;PurchasedProfiles=6;Scopes=3;Modules=12;Subgroups=$subgroupCount;AssetKinds=$assetKindCount;Areas=4"
