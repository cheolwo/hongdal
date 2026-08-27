[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/world-seedbeds/spatial-hierarchy-levels.json",
    [string] $OutputPath = "docs/AI/generated/simulation-world-spatial-hierarchy.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "SpatialHierarchyInvalid:$Code" }
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

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = Resolve-RepositoryPath $repositoryRoot $InputPath
$catalog = Read-Json $resolvedInput
$levels = @($catalog.levels)

Require ([string] $catalog.schemaVersion -eq "simulation-world-spatial-hierarchy.v1") "SchemaVersionInvalid"
Require ([string] $catalog.revision -eq "simulation-world-spatial-hierarchy.r1") "RevisionInvalid"
Require ([string] $catalog.axisCode -eq "H") "AxisCodeMustBeH"
Require (($levels.code -join ",") -eq "H1,H2,H3,H4") "LevelOrderInvalid"
Require ($levels.Count -eq 4) "LevelCountMustBeFour"
Require (@($levels.code | Select-Object -Unique).Count -eq 4) "LevelCodeDuplicate"
Require (($levels.label -join ",") -eq "작업공간 모판,블록 모판,경관 모판,지역 모판") "FamilyLabelOrderInvalid"

$resourceInventoryPath = Resolve-RepositoryPath $repositoryRoot ([string] $catalog.resourceInventoryCatalogPath)
$resourceInventory = Read-Json $resourceInventoryPath
Require ([string] $resourceInventory.schemaVersion -eq "simulation-world-spatial-resource-inventory.v1") "ResourceInventorySchemaInvalid"
Require (($resourceInventory.levels.levelCode -join ",") -eq "H1,H2,H3,H4") "ResourceInventoryLevelOrderInvalid"

$expectedKinds = @("WiSpatialSeedbed", "LandscapeBlock", "LandscapeGraph", "AreaSet")
Require (($levels.resourceKindCode -join ",") -eq ($expectedKinds -join ",")) "ResourceKindOrderInvalid"
Require (@($levels[0].containsLevelCodes).Count -eq 0) "H1MustNotContainHierarchyLevel"
Require-SameSet @("H1") @($levels[1].containsLevelCodes) "H2MustContainH1"
Require-SameSet @("H2") @($levels[2].containsLevelCodes) "H3MustContainH2"
Require-SameSet @("H3") @($levels[3].containsLevelCodes) "H4MustContainH3"

$counts = @{}

$h1 = $levels[0]
Require ([string] $h1.definitionSourceTypeCode -eq "SeedbedCatalog") "H1SourceTypeInvalid"
Require ([string] $h1.judgmentSurfaceCode -eq "RecognizedSpatialPart") "H1JudgmentSurfaceInvalid"
$seedbedCatalogPath = Resolve-RepositoryPath $repositoryRoot ([string] $h1.definitionSourcePath)
$seedbedCatalog = Read-Json $seedbedCatalogPath
$seedbedRoot = Split-Path -Parent $seedbedCatalogPath
$seedbedIds = @()
foreach ($definitionRef in @($seedbedCatalog.definitionRefs)) {
    $definitionPath = Join-Path $seedbedRoot ([string] $definitionRef -replace "/", [IO.Path]::DirectorySeparatorChar)
    $definition = Read-Json $definitionPath
    Require ([string] $definition.reviewStatusCode -eq "ApprovedForSimulation") "H1SeedbedNotApproved:$($definition.stableId)"
    Require ([string] $definition.stableId -like "wi-spatial-seedbed:*") "H1StableIdInvalid:$($definition.stableId)"
    $seedbedIds += [string] $definition.stableId
}
Require (@($seedbedIds | Select-Object -Unique).Count -eq $seedbedIds.Count) "H1StableIdDuplicate"
$counts.H1 = $seedbedIds.Count

$h2 = $levels[1]
Require ([string] $h2.currentInstancePolicyCode -eq "DesignInventorySeparatedFromE5Instances") "H2PolicyInvalid"
Require ([string] $h2.judgmentSurfaceCode -eq "FirstSpatialCompositionReview") "H2JudgmentSurfaceInvalid"
$blockRoot = Resolve-RepositoryPath $repositoryRoot ([string] $h2.definitionSourcePath)
$blockDefinitions = @(
    if (Test-Path -LiteralPath $blockRoot) {
        Get-ChildItem -LiteralPath $blockRoot -Filter "*.json" -File -Recurse
    }
)
$counts.H2 = $blockDefinitions.Count

$h4 = $levels[3]
$areaSetPath = Resolve-RepositoryPath $repositoryRoot ([string] $h4.definitionSourcePath)
$areaSet = Read-Json $areaSetPath
Require ([string] $areaSet.schemaVersion -eq "simulation-world-area-set.v1") "H4AreaSetSchemaInvalid"
Require ([string] $areaSet.areaSetStableId -like "area-set:sim:*") "H4StableIdInvalid"
$counts.H4 = 1

$h3 = $levels[2]
Require ([string] $h3.ownerDefinitionPath -eq [string] $h4.definitionSourcePath) "H3OwnerMustBeH4"
$graphRoot = Resolve-RepositoryPath $repositoryRoot ([string] $h3.definitionSourcePath)
Require (Test-Path -LiteralPath $graphRoot) "H3DefinitionRootMissing"
$graphFiles = @(Get-ChildItem -LiteralPath $graphRoot -Filter "*.json" -File | Sort-Object Name)
$graphIds = @()
foreach ($graphFile in $graphFiles) {
    $graph = Read-Json $graphFile.FullName
    Require ([string] $graph.landscapeGraphStableId -like "landscape-graph:sim:*") "H3StableIdInvalid:$($graphFile.Name)"
    $graphIds += [string] $graph.landscapeGraphStableId
}
Require (@($graphIds | Select-Object -Unique).Count -eq $graphIds.Count) "H3StableIdDuplicate"
Require-SameSet @($areaSet.landscapeGraphRefs) $graphIds "H4MustReferenceEveryH3ExactlyOnce"
$counts.H3 = $graphIds.Count

foreach ($level in $levels) {
    $code = [string] $level.code
    Require-Text $level.label "LabelMissing:$code"
    Require-Text $level.boundary "BoundaryMissing:$code"
    Require ([int] $level.expectedCurrentDefinitionCount -eq [int] $counts[$code]) "DefinitionCountMismatch:$code"
}

Require (@($areaSet.graphRelations).Count -eq 4) "H4GraphRelationCountMustBeFour"
foreach ($relation in @($areaSet.graphRelations)) {
    Require ($graphIds -contains [string] $relation.fromGraphStableId) "H4RelationFromGraphMissing:$($relation.relationStableId)"
    Require ($graphIds -contains [string] $relation.toGraphStableId) "H4RelationToGraphMissing:$($relation.relationStableId)"
}

$evidencePath = Resolve-RepositoryPath $repositoryRoot ([string] $catalog.evidenceStageCatalogPath)
$evidence = Read-Json $evidencePath
$evidenceByCode = @{}
foreach ($stage in @($evidence.stages)) { $evidenceByCode[[string] $stage.code] = $stage }
Require ($evidenceByCode.ContainsKey("E4")) "E4Missing"
Require ($evidenceByCode.ContainsKey("E5")) "E5Missing"
Require ([string] $evidenceByCode.E4.completionGate -match "발생원") "E4MustVerifyTriggerSource"
Require ([string] $evidenceByCode.E4.completionGate -match "공간 적용") "E4MustClassifySpatialApplicability"
Require ([string] $evidenceByCode.E5.completionGate -match "Task") "E5MustVerifyTaskOrEffect"
Require ([string] $evidenceByCode.E5.completionGate -match "후속") "E5MustVerifySuccessor"
Require ([string] $evidenceByCode.E6.completionGate -notmatch "공간 포함 계층") "E6MustNotBeHierarchyLevel"
Require ([string] $evidenceByCode.E7.completionGate -notmatch "공간 포함 계층") "E7MustNotBeHierarchyLevel"
Require ($evidenceByCode.ContainsKey("E8")) "E8Missing"
Require ($evidenceByCode.ContainsKey("E9")) "E9Missing"

Require (@($catalog.referenceAxes).Count -eq 5) "ReferenceAxisCountMustBeFive"
$referenceAxisCodes = @($catalog.referenceAxes | ForEach-Object { [string] $_.code })
Require-SameSet @("TileL0L2", "Area", "LandscapeCompletionArea", "ScenarioRoute", "SyntyBottomUpInventory") $referenceAxisCodes "ReferenceAxisCodesInvalid"
$bottomUpInventoryAxis = @($catalog.referenceAxes | Where-Object code -eq "SyntyBottomUpInventory")
Require ($bottomUpInventoryAxis.Count -eq 1) "SyntyBottomUpInventoryAxisMissing"
$bottomUpInventoryCatalogPath = Resolve-RepositoryPath $repositoryRoot ([string] $bottomUpInventoryAxis[0].catalogPath)
Require (Test-Path -LiteralPath $bottomUpInventoryCatalogPath) "SyntyBottomUpInventoryCatalogMissing"
Require ([int] $catalog.grammarVocabulary.canonicalItemCount -eq 156) "GrammarVocabularyCountMustBe156"
Require ([bool] $catalog.presentationOnly) "PresentationOnlyMustBeTrue"
Require (-not [bool] $catalog.isOperationalState) "OperationalStateMustBeFalse"

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# Simulation World 공간 포함 계층")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$InputPath``에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 계층 대장 개정: ``$($catalog.revision)``")
[void] $builder.AppendLine("- 증거 단계 개정: ``$($evidence.revision)``")
[void] $builder.AppendLine("- 축 구분: ``E``는 증거 성숙도, ``G``는 성숙도를 높이는 관리 체계, ``H``는 공간 포함 깊이다.")
[void] $builder.AppendLine("- 모판 계열: ``H1 작업공간 → H2 블록 → H3 경관 → H4 지역``으로 상향 조립하며 재고 상태는 별도 대장에서 관리한다.")
[void] $builder.AppendLine("- 현재 정의 수: ``H1 $($counts.H1) / H2 $($counts.H2) / H3 $($counts.H3) / H4 $($counts.H4)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 포함 계층")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 계층 | 의미 | 포함 | 현재 정의 | 현재 정책 |")
[void] $builder.AppendLine("| --- | --- | --- | ---: | --- |")
foreach ($level in $levels) {
    $contains = if (@($level.containsLevelCodes).Count -eq 0) { "-" } else { @($level.containsLevelCodes) -join ", " }
    [void] $builder.AppendLine("| ``$($level.code)`` | $($level.label) | $contains | $($counts[[string] $level.code]) | ``$($level.currentInstancePolicyCode)`` |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('```text')
[void] $builder.AppendLine("H4 지역 모판 (AreaSet)")
[void] $builder.AppendLine("└─ H3 경관 모판 (LandscapeGraph)")
[void] $builder.AppendLine("   └─ H2 블록 모판 (LandscapeBlock)")
[void] $builder.AppendLine("      └─ H1 작업공간 모판 (WI 공간 모판) 인스턴스")
[void] $builder.AppendLine('```')
[void] $builder.AppendLine()
[void] $builder.AppendLine("H 코드는 리소스 종류를 분류할 뿐 E 완료 상태를 올리지 않는다. 현재 H4 AreaSet과 H3 Graph가 존재해도 WI의 실행 문맥, 권위 전이·Task/Effect·결과·후속 선택이 닫히지 않으면 E4·E5가 아니다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## E 증거 단계와의 관계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 증거 | H 계층 사용 | 완료 의미 |")
[void] $builder.AppendLine("| --- | --- | --- |")
[void] $builder.AppendLine("| ``E3`` | 없음 | WI 행위 계약·코드·자동 시험이 성립한다. |")
[void] $builder.AppendLine("| ``E4`` | 공간 적용이 Required인 WI만 H1~H5 참조 | 허용 발생원·주체·대상·자료·자원·시간과 선택적 공간 문맥을 결속한다. |")
[void] $builder.AppendLine("| ``E5`` | 공간 WI의 조건부 증거로 H1→H5 조립 사용 | 권위 전이·Task/Effect·결과·후속 선택이 결정적 세계에서 발현된다. 공간 조립만으로 완료되지 않는다. |")
[void] $builder.AppendLine("| ``E6`` | E5 WI 폐루프와 필요한 H 결과 사용 | WI·상태 변화와 인과 폐루프를 설명하고 필요한 현실 문맥의 출처·판본·hash·한계를 결속한다. |")
[void] $builder.AppendLine("| ``E7`` | E6 결과 사용 | 플레이어가 실제 서버와 저장 Scene에서 폐루프를 수행한다. |")
[void] $builder.AppendLine("| ``E8`` | 한 E7 PlayableUnit의 H 경로와 상태 사본 사용 | 같은 폐루프의 반복 결정성·Save 재진입·Local/Remote·실제 입력 안정성을 확인한다. |")
[void] $builder.AppendLine("| ``E9`` | 같은 영역의 E8 Core 둘 이상과 H 인계 사용 | 공간·시간·자원·회복·조건부 NPC 연속성의 조화와 사람 승인을 확인한다. |")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 계층에서 제외하는 축")
[void] $builder.AppendLine()
foreach ($axis in @($catalog.referenceAxes)) {
    [void] $builder.AppendLine("- **$($axis.label)**: $($axis.reason)")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("기존 156개 기준 경관 문법 모판은 H 계층이 아니다. H1의 허용 후보와 H2·H3 조립에서 사용하는 공간 문법 어휘다.")

$expected = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Resolve-RepositoryPath $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    Write-DeterministicTextIfChanged $resolvedOutput $expected | Out-Null
    Write-Output "SpatialHierarchyGenerated:$OutputPath"
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedDocumentMissing:$OutputPath"
    $actual = ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedOutput))
    Require ($actual -eq $expected) "GeneratedDocumentOutOfDate:$OutputPath"
    Write-Output "SpatialHierarchyValid:H1=$($counts.H1);H2=$($counts.H2);H3=$($counts.H3);H4=$($counts.H4)"
}
