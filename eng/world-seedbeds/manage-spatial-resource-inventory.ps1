[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/world-seedbeds/spatial-resource-inventory/catalog.v1.json",
    [string] $OutputPath = "docs/AI/generated/simulation-world-spatial-resource-inventory.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "SpatialResourceInventoryInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Resolve-RepositoryPath([string] $RepositoryRoot, [string] $RelativePath) {
    return Join-Path $RepositoryRoot ($RelativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}

function Read-Json([string] $Path) {
    Require (Test-Path -LiteralPath $Path) "SourceMissing:$Path"
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Require-SameSet([string[]] $Expected, [string[]] $Actual, [string] $Code) {
    $left = @($Expected | Sort-Object)
    $right = @($Actual | Sort-Object)
    Require (($left -join "|") -eq ($right -join "|")) $Code
}

function Get-JsonArrayProperty([object] $Document, [string] $PropertyName, [string] $Code) {
    $property = $Document.PSObject.Properties[$PropertyName]
    Require ($null -ne $property) "$Code`PropertyMissing:$PropertyName"
    return @($property.Value)
}

function Get-SourceCount(
    [string] $RepositoryRoot,
    [object] $Inventory,
    [object] $Source,
    [string] $LevelCode,
    [string] $StockKindCode
) {
    $sourceType = [string] $Source.sourceTypeCode
    if ($sourceType -eq "InlineCollection") {
        return @(Get-JsonArrayProperty $Inventory ([string] $Source.collectionProperty) "$LevelCode$StockKindCode").Count
    }

    $sourcePath = Resolve-RepositoryPath $RepositoryRoot ([string] $Source.sourcePath)
    if ($sourceType -eq "JsonArrayProperty") {
        $document = Read-Json $sourcePath
        return @(Get-JsonArrayProperty $document ([string] $Source.collectionProperty) "$LevelCode$StockKindCode").Count
    }
    if ($sourceType -eq "DefinitionCatalogRefs") {
        $document = Read-Json $sourcePath
        return @($document.definitionRefs).Count
    }
    if ($sourceType -eq "JsonDirectory") {
        if (-not (Test-Path -LiteralPath $sourcePath)) { return 0 }
        return @(Get-ChildItem -LiteralPath $sourcePath -Filter "*.json" -File -Recurse).Count
    }
    if ($sourceType -eq "SingleJson") {
        [void] (Read-Json $sourcePath)
        return 1
    }

    throw "SpatialResourceInventoryInvalid:SourceTypeUnknown:$sourceType"
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = Resolve-RepositoryPath $repositoryRoot $InputPath
$raw = [IO.File]::ReadAllText($resolvedInput, [Text.Encoding]::UTF8)
$inventory = $raw | ConvertFrom-Json

Require ([string] $inventory.schemaVersion -eq "simulation-world-spatial-resource-inventory.v1") "SchemaVersionInvalid"
Require ([string] $inventory.revision -eq "simulation-world-spatial-resource-inventory.r9") "RevisionInvalid"
Require ([bool] $inventory.presentationOnly) "PresentationOnlyMustBeTrue"
Require (-not [bool] $inventory.isOperationalState) "OperationalStateMustBeFalse"
Require (-not ($raw -match '"(absoluteWorldPosition|worldEastingMeters|worldNorthingMeters|latitude|longitude|prefabPath|assetGuid|scenePath)"')) "AuthorityFieldForbidden"

$hierarchyPath = Resolve-RepositoryPath $repositoryRoot ([string] $inventory.hierarchyCatalogPath)
$hierarchy = Read-Json $hierarchyPath
$levels = @($inventory.levels)
$hierarchyLevels = @($hierarchy.levels)
Require (($levels.levelCode -join ",") -eq "H1,H2,H3,H4") "LevelOrderInvalid"
Require ($levels.Count -eq 4) "LevelCountInvalid"

$designCounts = @{}
$definitionCounts = @{}
for ($index = 0; $index -lt $levels.Count; $index++) {
    $level = $levels[$index]
    $hierarchyLevel = $hierarchyLevels[$index]
    $code = [string] $level.levelCode
    Require ([string] $hierarchyLevel.code -eq $code) "HierarchyLevelMismatch:$code"
    Require ([string] $hierarchyLevel.resourceKindCode -eq [string] $level.resourceKindCode) "ResourceKindMismatch:$code"
    Require-Text $level.familyLabel "FamilyLabelMissing:$code"

    $designCount = 0
    foreach ($source in @($level.designInventorySources)) {
        $count = Get-SourceCount $repositoryRoot $inventory $source $code "Design"
        $sourceIdentity = if ([string] $source.sourceTypeCode -eq "InlineCollection") {
            "inline:$($source.collectionProperty)"
        }
        else {
            [string] $source.sourcePath
        }
        Require ($count -eq [int] $source.expectedCount) "DesignSourceCountMismatch:${code}:$sourceIdentity"
        $designCount += $count
    }
    $definitionCount = 0
    foreach ($source in @($level.definitionSources)) {
        $count = Get-SourceCount $repositoryRoot $inventory $source $code "Definition"
        Require ($count -eq [int] $source.expectedCount) "DefinitionSourceCountMismatch:${code}:$($source.sourcePath)"
        $definitionCount += $count
    }
    Require ($definitionCount -eq [int] $hierarchyLevel.expectedCurrentDefinitionCount) "HierarchyDefinitionCountMismatch:$code"
    $designCounts[$code] = $designCount
    $definitionCounts[$code] = $definitionCount
}

$h1Catalog = Read-Json (Resolve-RepositoryPath $repositoryRoot "eng/world-seedbeds/wi-spatial-seedbeds/catalog.json")
$h1DefinitionIds = @()
$h1Root = Split-Path -Parent (Resolve-RepositoryPath $repositoryRoot "eng/world-seedbeds/wi-spatial-seedbeds/catalog.json")
foreach ($definitionRef in @($h1Catalog.definitionRefs)) {
    $definition = Read-Json (Join-Path $h1Root ([string] $definitionRef))
    Require ([string] $definition.reviewStatusCode -eq "ApprovedForSimulation") "H1DefinitionNotApproved:$($definition.stableId)"
    $h1DefinitionIds += [string] $definition.stableId
}
Require (@($h1DefinitionIds | Select-Object -Unique).Count -eq $h1DefinitionIds.Count) "H1DefinitionDuplicate"

$candidateRoot = Resolve-RepositoryPath $repositoryRoot "eng/world-seedbeds/landscape-block-candidates"
foreach ($candidateFile in @(Get-ChildItem -LiteralPath $candidateRoot -Filter "*.json" -File)) {
    $candidate = Read-Json $candidateFile.FullName
    Require ([string] $candidate.promotionGate.targetHierarchyLevelCode -eq "H2") "H2CandidateTargetInvalid:$($candidateFile.Name)"
    Require (-not [bool] $candidate.isOperationalState) "H2CandidateOperationalStateForbidden:$($candidateFile.Name)"
}

$graphRoot = Resolve-RepositoryPath $repositoryRoot "eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/landscape-graphs"
$graphIds = @()
foreach ($graphFile in @(Get-ChildItem -LiteralPath $graphRoot -Filter "*.json" -File | Sort-Object Name)) {
    $graph = Read-Json $graphFile.FullName
    $graphIds += [string] $graph.landscapeGraphStableId
}
$areaSet = Read-Json (Resolve-RepositoryPath $repositoryRoot "eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/area-set.json")
$h4Stock = @($inventory.h4Inventory)
Require ($h4Stock.Count -eq 1) "H4InventoryCountInvalid"
Require ([string] $h4Stock[0].stateCode -eq "DefinedPartialAssemblyReference") "H4InventoryStateInvalid"
Require ([string] $h4Stock[0].definitionStableId -eq [string] $areaSet.areaSetStableId) "H4DefinitionReferenceMismatch"
Require-SameSet $graphIds @($h4Stock[0].h3DefinitionRefs) "H4ChildDefinitionsMismatch"

Require-SameSet @("IdeaInventory", "ExploratoryInventory", "CandidateForReview", "ApprovedReference", "DefinedPartialAssemblyReference") @($inventory.stateModel.designStateCodes) "DesignStateModelInvalid"
Require-SameSet @("Unallocated", "Allocated", "Placed") @($inventory.stateModel.placementStateCodes) "PlacementStateModelInvalid"
Require ([string] $inventory.compositionPolicy.directionCode -eq "BottomUp") "CompositionDirectionInvalid"
foreach ($required in @("requiresExactChildRevision", "requiresDeterministicHash", "requiresHumanReviewForPromotion", "h1RecognitionIsSufficientForH2Composition", "h2IsFirstSpatialCompositionJudgmentSurface", "h2VisualReviewRequiresUnityRoot", "forbidsAutomaticAuthorityPromotion", "forbidsHierarchyCycles", "definitionInventorySeparatedFromPlacementInventory")) {
    Require ([bool] $inventory.compositionPolicy.$required) "CompositionPolicyRequired:$required"
}
foreach ($required in @("preserveExistingStableIds", "preserveExistingSchemaVersions", "storeHierarchyCodeOnlyInInventoryLedger", "legacySeedbedTermsRemainAdapters")) {
    Require ([bool] $inventory.compatibilityPolicy.$required) "CompatibilityPolicyRequired:$required"
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# Simulation World H 공간 구성 재고")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$InputPath``에서 결정적으로 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 재고 개정: ``$($inventory.revision)``")
[void] $builder.AppendLine("- 계열 의미: 모판은 H1 하나의 이름이 아니라 H1~H4를 상향 조립하는 공간 구성 자원 계열이다.")
[void] $builder.AppendLine("- 축 구분: H는 공간 자원 종류, 재고 상태는 후보·승인·배정·배치, E는 구현·통합 증거 깊이다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 공간 구성 재고")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 계층 | 사람 중심 명칭 | 기술 자원 | 설계 재고 | 현재 정의 |")
[void] $builder.AppendLine("| --- | --- | --- | ---: | ---: |")
foreach ($level in $levels) {
    $code = [string] $level.levelCode
    [void] $builder.AppendLine("| ``$code`` | $($level.familyLabel) | ``$($level.resourceKindCode)`` | $($designCounts[$code]) | $($definitionCounts[$code]) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('```text')
[void] $builder.AppendLine("H1 작업공간 모판 재고")
[void] $builder.AppendLine("  → H2 블록 모판 재고")
[void] $builder.AppendLine("    → H3 경관 모판 재고")
[void] $builder.AppendLine("      → H4 지역 모판 재고")
[void] $builder.AppendLine('```')
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 정의 재고와 배치 재고")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 설계 재고는 위치 독립적인 후보·승인 참조다. Unity Prefab이나 절대좌표가 공간 권위를 갖지 않는다.")
[void] $builder.AppendLine("- 현재 정의는 각 H 기술 자원의 실제 버전 관리 정의다. H3·H4 정의가 있어도 실제 H2가 없으면 E5가 아니다.")
[void] $builder.AppendLine("- 배치 상태 ``Unallocated / Allocated / Placed``는 정의 상태와 별도이며 아직 이 대장에서 실제 배치 수량을 꾸며내지 않는다.")
[void] $builder.AppendLine("- 상위 재고는 하위 재고의 정확한 revision과 결정적 hash를 참조하고 사람 검토 없이는 권위 정의로 자동 승격되지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 호환 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("기존 WI 공간 모판, LandscapeBlock, LandscapeGraph, AreaSet의 stable ID·schema·공개 계약은 유지한다. H 코드는 이 공통 재고 대장에서만 계산하며 기존 실행 JSON과 저장 상태에 중복 저장하지 않는다. Unity 배치 객체 모판과 규칙 실험 모판은 각각 표현·시험 adapter로 남고 H 공간 자원으로 자동 편입되지 않는다.")

$expected = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Resolve-RepositoryPath $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    Write-DeterministicTextIfChanged $resolvedOutput $expected | Out-Null
    Write-Output "SpatialResourceInventoryGenerated:H1=$($designCounts.H1)/$($definitionCounts.H1);H2=$($designCounts.H2)/$($definitionCounts.H2);H3=$($designCounts.H3)/$($definitionCounts.H3);H4=$($designCounts.H4)/$($definitionCounts.H4)"
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedDocumentMissing:$OutputPath"
    $actual = ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedOutput))
    Require ($actual -eq $expected) "GeneratedDocumentOutOfDate:$OutputPath"
    Write-Output "SpatialResourceInventoryValid:H1=$($designCounts.H1)/$($definitionCounts.H1);H2=$($designCounts.H2)/$($definitionCounts.H2);H3=$($designCounts.H3)/$($definitionCounts.H3);H4=$($designCounts.H4)/$($definitionCounts.H4)"
}
