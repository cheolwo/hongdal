[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v1.json",
    [string] $OutputPath = "docs/AI/generated/synty-bottom-up-spatial-inventory.md",
    [string] $UnityProjectPath = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "SyntyBottomUpInventoryInvalid:$Code" }
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
$raw = [IO.File]::ReadAllText($resolvedInput, [Text.Encoding]::UTF8)
$catalog = $raw | ConvertFrom-Json

Require ([string] $catalog.schemaVersion -eq "simulation-world-synty-bottom-up-inventory.v1") "SchemaVersionInvalid"
Require ([bool] $catalog.presentationOnly -and -not [bool] $catalog.isOperationalState) "AuthorityBoundaryInvalid"
Require (-not ($raw -match '"(absolute|world)(X|Y|Z|Coordinate|Position)"')) "AbsoluteCoordinateForbidden"
Require (-not ($raw -match '"(prefabPath|materialPath|scenePath|assetGuid|metaGuid|gameObjectName)"')) "UnityAssetAuthorityForbidden"

$packCounts = @{ Nature = 227; Farm = 498; Town = 702; City = 335 }
Require (@($catalog.packInventory).Count -eq 4) "PackCountInvalid"
foreach ($pack in @($catalog.packInventory)) {
    $code = [string] $pack.packCode
    Require ($packCounts.ContainsKey($code)) "PackUnknown:$code"
    Require ([int] $pack.prefabCount -eq [int] $packCounts[$code]) "PackPrefabCountInvalid:$code"
    Require-Text $pack.unityAssetRelativeRoot "PackRootMissing:$code"
    if (-not [string]::IsNullOrWhiteSpace($UnityProjectPath)) {
        $packRoot = Join-Path $UnityProjectPath ([string] $pack.unityAssetRelativeRoot)
        Require (Test-Path -LiteralPath $packRoot) "UnityPackRootMissing:$code"
        $actualCount = @(Get-ChildItem -LiteralPath $packRoot -Recurse -Filter "*.prefab" -File).Count
        Require ($actualCount -eq [int] $pack.prefabCount) "UnityPackPrefabCountMismatch:${code}:$actualCount"
    }
}
Require ((@($catalog.packInventory | Measure-Object prefabCount -Sum).Sum) -eq 1762) "PackPrefabTotalInvalid"

$grammarPath = Resolve-Path (Join-Path $repositoryRoot ([string] $catalog.landscapeGrammarManifestPath)
)
$grammar = Get-Content -LiteralPath $grammarPath -Raw -Encoding UTF8 | ConvertFrom-Json
$grammarKeys = @{}
foreach ($entry in @($grammar.entries)) { $grammarKeys[[string] $entry.compositionKey] = $true }

$approvedCatalogPath = Resolve-Path (Join-Path $repositoryRoot ([string] $catalog.approvedH1CatalogPath))
$approvedCatalog = Get-Content -LiteralPath $approvedCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$approvedSeedbeds = @{}
foreach ($definitionRef in @($approvedCatalog.definitionRefs)) {
    $definitionPath = Join-Path (Split-Path $approvedCatalogPath) ([string] $definitionRef)
    $definition = Get-Content -LiteralPath $definitionPath -Raw -Encoding UTF8 | ConvertFrom-Json
    Require ([string] $definition.reviewStatusCode -eq "ApprovedForSimulation") "ApprovedSeedbedStateInvalid:$($definition.stableId)"
    $approvedSeedbeds[[string] $definition.stableId] = $true
}

$wiCatalogPath = Resolve-Path (Join-Path $repositoryRoot ([string] $catalog.worldInteractionCatalogPath))
$wiCatalog = Get-Content -LiteralPath $wiCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$e3WiIds = @{}
foreach ($item in @($wiCatalog.items)) {
    if ([string] $item.implementation.status -eq "Done" -and [string] $item.implementation.currentStage -eq "E3") {
        $e3WiIds[[string] $item.id] = $true
    }
}

$allowedH1States = @("ApprovedReference", "CandidateForReview")
$h1Ids = @{}
$grammarCandidateLinks = 0
foreach ($item in @($catalog.h1Inventory)) {
    $id = [string] $item.inventoryId
    Require-Text $id "H1IdMissing"
    Require (-not $h1Ids.ContainsKey($id)) "H1IdDuplicate:$id"
    Require ($allowedH1States -contains [string] $item.stateCode) "H1StateInvalid:$id"
    Require (@($item.wiIds).Count -gt 0) "H1WiMissing:$id"
    Require (@($item.spatialRoleCodes).Count -gt 0) "H1RoleMissing:$id"
    Require (@($item.grammarSetRefs).Count -gt 0) "H1GrammarSetMissing:$id"
    foreach ($wiId in @($item.wiIds)) {
        Require ($e3WiIds.ContainsKey([string] $wiId)) "H1WiNotE3:${id}:$wiId"
    }
    if ([string] $item.stateCode -eq "ApprovedReference") {
        Require-Text $item.approvedSeedbedStableId "H1ApprovedReferenceMissing:$id"
        Require ($approvedSeedbeds.ContainsKey([string] $item.approvedSeedbedStableId)) "H1ApprovedSeedbedUnknown:$id"
    }
    else {
        Require ([string]::IsNullOrWhiteSpace([string] $item.approvedSeedbedStableId)) "H1CandidateMustNotReferenceApprovedSeedbed:$id"
    }
    foreach ($setRef in @($item.grammarSetRefs)) {
        foreach ($variant in @("A", "B", "C")) {
            $key = "$setRef`:$variant"
            Require ($grammarKeys.ContainsKey($key)) "H1GrammarKeyMissing:${id}:$key"
            $grammarCandidateLinks++
        }
    }
    $h1Ids[$id] = $true
}
Require (@($catalog.h1Inventory).Count -eq 19) "H1InventoryCountMustBe19"
Require (@($catalog.h1Inventory | Where-Object stateCode -eq "ApprovedReference").Count -eq 5) "H1ApprovedReferenceCountMustBe5"
Require (@($catalog.h1Inventory | Where-Object stateCode -eq "CandidateForReview").Count -eq 14) "H1CandidateCountMustBe14"

$h2Ids = @{}
foreach ($item in @($catalog.h2Candidates)) {
    $id = [string] $item.candidateId
    Require-Text $id "H2IdMissing"
    Require (-not $h2Ids.ContainsKey($id)) "H2IdDuplicate:$id"
    Require (@($item.h1InventoryRefs).Count -gt 0) "H2H1RefMissing:$id"
    Require ((@($item.sizeVariantCodes) -join ",") -eq "Compact,Standard,Expanded") "H2SizeVariantsInvalid:$id"
    foreach ($h1Ref in @($item.h1InventoryRefs)) {
        Require ($h1Ids.ContainsKey([string] $h1Ref)) "H2H1RefUnknown:${id}:$h1Ref"
    }
    $h2Ids[$id] = $true
}
Require (@($catalog.h2Candidates).Count -eq 10) "H2CandidateCountMustBe10"

$h3Ids = @{}
foreach ($item in @($catalog.h3AssemblyCandidates)) {
    $id = [string] $item.candidateId
    Require-Text $id "H3IdMissing"
    Require (-not $h3Ids.ContainsKey($id)) "H3IdDuplicate:$id"
    Require (@($item.h2CandidateRefs).Count -gt 0) "H3H2RefMissing:$id"
    Require (@($item.connectorRoleCodes).Count -gt 0) "H3ConnectorRoleMissing:$id"
    foreach ($h2Ref in @($item.h2CandidateRefs)) {
        Require ($h2Ids.ContainsKey([string] $h2Ref)) "H3H2RefUnknown:${id}:$h2Ref"
    }
    $h3Ids[$id] = $true
}
Require (@($catalog.h3AssemblyCandidates).Count -eq 6) "H3CandidateCountMustBe6"

$policy = $catalog.promotionPolicy
Require ([bool] $policy.requiresHumanReview) "HumanReviewRequired"
Require ([bool] $policy.requiresDeterministicHash) "DeterministicHashRequired"
Require ([bool] $policy.forbidsAutomaticAuthorityPromotion) "AutomaticPromotionMustBeForbidden"
Require ([bool] $policy.forbidsAbsoluteCoordinates) "AbsoluteCoordinatesMustBeForbidden"
Require ([bool] $policy.forbidsUnityAssetAuthority) "UnityAssetAuthorityMustBeForbidden"

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# Synty 상향식 공간 설계 재고")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$InputPath``에서 결정적으로 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 재고 개정: ``$($catalog.revision)``")
[void] $builder.AppendLine("- 원본 Prefab: ``1,762개``")
[void] $builder.AppendLine("- H1 의미 재고: ``19개`` — 승인 참조 5개, 검토 후보 14개")
[void] $builder.AppendLine("- H1 최소 A/B/C 표현 슬롯: ``54개``")
[void] $builder.AppendLine("- 기준 경관 문법 후보 연결: ``${grammarCandidateLinks}개``")
[void] $builder.AppendLine("- H2 블록 후보: ``10개 × 3 크기 = 30개 배치안``")
[void] $builder.AppendLine("- H3 조립 후보: ``6개``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 팩 재고")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 팩 | Prefab | Unity 상대 경로 |")
[void] $builder.AppendLine("| --- | ---: | --- |")
foreach ($pack in @($catalog.packInventory)) {
    [void] $builder.AppendLine("| $($pack.packCode) | $($pack.prefabCount) | ``$($pack.unityAssetRelativeRoot)`` |")
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## H1 의미 재고")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 상태 | 그룹 | 공간 재고 | E3 WI | 경관 문법 후보 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- |")
foreach ($item in @($catalog.h1Inventory)) {
    $state = if ([string] $item.stateCode -eq "ApprovedReference") { "승인 H1 참조" } else { "검토 후보" }
    [void] $builder.AppendLine("| $state | ``$($item.groupCode)`` | ``$($item.inventoryId)`` $($item.title) | $(Escape-Markdown (@($item.wiIds) -join ', ')) | $(Escape-Markdown (@($item.grammarSetRefs) -join ', ')) |")
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## H2 블록 후보")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 후보 | 위상 | 포함 H1 | 설계 상태 |")
[void] $builder.AppendLine("| --- | --- | --- | --- |")
foreach ($item in @($catalog.h2Candidates)) {
    [void] $builder.AppendLine("| ``$($item.candidateId)`` $($item.title) | ``$($item.topologyCode)`` | $(Escape-Markdown (@($item.h1InventoryRefs) -join ', ')) | 위치 독립 설계 |")
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## H3 조립 후보")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 후보 | 위상 | H2 후보 | 외부 연결 역할 |")
[void] $builder.AppendLine("| --- | --- | --- | --- |")
foreach ($item in @($catalog.h3AssemblyCandidates)) {
    [void] $builder.AppendLine("| ``$($item.candidateId)`` $($item.title) | ``$($item.topologyCode)`` | $(Escape-Markdown (@($item.h2CandidateRefs) -join ', ')) | $(Escape-Markdown (@($item.connectorRoleCodes) -join ', ')) |")
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 권위 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 검토 후보는 공식 H1 정의 수에 포함하지 않는다.")
[void] $builder.AppendLine("- H2 후보는 실제 도로·경계·지형 근거와 결정적 경계 hash가 생기기 전까지 H2가 아니다.")
[void] $builder.AppendLine("- H3 조립 후보는 공식 LandscapeGraph StableId, 실제 Node·Edge·좌표를 소유하지 않는다.")
[void] $builder.AppendLine("- Synty Prefab·GUID·Material·Scene 경로는 표현 대장에서만 연결하며 공간·Simulation 권위를 갖지 않는다.")

$expected = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    Write-DeterministicTextIfChanged $resolvedOutput $expected | Out-Null
    Write-Output "SyntyBottomUpInventoryGenerated:H1=19;H2Candidates=10;H3Candidates=6;GrammarLinks=$grammarCandidateLinks"
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedDocumentMissing:$OutputPath"
    $actual = ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedOutput))
    Require ($actual -eq $expected) "GeneratedDocumentOutOfDate:$OutputPath"
    Write-Output "SyntyBottomUpInventoryValid:H1=19;H2Candidates=10;H3Candidates=6;GrammarLinks=$grammarCandidateLinks"
}
