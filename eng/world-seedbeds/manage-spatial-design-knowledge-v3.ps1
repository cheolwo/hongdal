[CmdletBinding()]
param(
    [ValidateSet("Bootstrap", "Write", "Check")]
    [string] $Mode = "Check",
    [string] $KnowledgeRootPath = "eng/world-seedbeds/synty-bottom-up-inventory",
    [string] $GrammarPath = "eng/world-seedbeds/manifests/pyeongchang-landscape-grammar.v1.json",
    [string] $RecipePath = "eng/world-seedbeds/synty-bottom-up-inventory/grammar-derivation-recipes.v1.json"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
. (Join-Path $repositoryRoot "eng/common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "SpatialDesignKnowledgeV3Invalid:$Code" }
}

function Resolve-RepositoryPath([string] $RelativePath) {
    return Join-Path $repositoryRoot ($RelativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}

function Read-Json([string] $Path) {
    Require (Test-Path -LiteralPath $Path) "SourceMissing:$Path"
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function ConvertTo-StableJson([object] $Value) {
    return ConvertTo-DeterministicText (($Value | ConvertTo-Json -Depth 40) + "`n")
}

function Get-TextSha256([string] $Content) {
    $bytes = [Text.Encoding]::UTF8.GetBytes((ConvertTo-DeterministicText $Content))
    return ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))).ToLowerInvariant()
}

function Get-FileTextSha256([string] $Path) {
    return Get-TextSha256 ([IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8))
}

function Get-NormalizedSlug([string] $Value) {
    $normalized = $Value.Normalize([Text.NormalizationForm]::FormC).ToLowerInvariant()
    $normalized = [Text.RegularExpressions.Regex]::Replace($normalized, "[^\p{L}\p{Nd}]+", "-")
    return $normalized.Trim("-")
}

function Escape-Markdown([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

function Write-Or-Check([string] $Path, [string] $Content, [string] $Code) {
    $expected = ConvertTo-DeterministicText $Content
    if ($Mode -eq "Check") {
        Require (Test-Path -LiteralPath $Path) "${Code}Missing:$Path"
        $actual = ConvertTo-DeterministicText ([IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8))
        Require ($actual -ceq $expected) "${Code}OutOfDate:$Path"
    }
    else {
        Write-DeterministicTextIfChanged $Path $expected | Out-Null
    }
}

function Get-UniqueStrings([object[]] $Values) {
    return @($Values | ForEach-Object { [string] $_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
}

function Get-PropertyValues([object[]] $Objects, [string] $PropertyName, [string] $ChildPropertyName = "") {
    $values = @()
    foreach ($item in @($Objects)) {
        $property = $item.PSObject.Properties[$PropertyName]
        if ($null -eq $property -or $null -eq $property.Value) { continue }
        foreach ($value in @($property.Value)) {
            if ([string]::IsNullOrWhiteSpace($ChildPropertyName)) {
                $values += $value
                continue
            }
            $childProperty = $value.PSObject.Properties[$ChildPropertyName]
            if ($null -ne $childProperty -and $null -ne $childProperty.Value) { $values += $childProperty.Value }
        }
    }
    return $values
}

function Get-PackLabel([string] $FamilyCode) {
    $labels = @{ nature = "Nature"; farm = "Farm"; town = "Town"; city = "City" }
    Require ($labels.ContainsKey($FamilyCode)) "PackFamilyUnknown:$FamilyCode"
    return $labels[$FamilyCode]
}

function New-ExpressionMarkdown([object] $Definition) {
    $builder = [Text.StringBuilder]::new()
    [void] $builder.AppendLine("# H1 팩 단독 표현 탐색 카드 — $($Definition.title)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("@spatial-knowledge $($Definition.stableId)")
    [void] $builder.AppendLine("@hierarchy H1")
    [void] $builder.AppendLine("@card-kind PackExpression")
    [void] $builder.AppendLine("@state $($Definition.knowledgeStateCode)")
    [void] $builder.AppendLine("@pack $($Definition.sourcePackCode)")
    [void] $builder.AppendLine("@grammar-set $($Definition.sourceGrammarSetRef)")
    foreach ($variant in @($Definition.grammarVariantRefs)) { [void] $builder.AppendLine("@grammar-variant $variant") }
    foreach ($reference in @($Definition.supportsInteractionH1Refs)) { [void] $builder.AppendLine("@supports-interaction-h1 $reference") }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine($Definition.summary)
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## 공간 표현 조건")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("- 위상: ``$(@($Definition.topologyCodes) -join ', ')``")
    [void] $builder.AppendLine("- 기준 크기 범위: $($Definition.footprint.minimumWidthMeters)~$($Definition.footprint.maximumWidthMeters)m × $($Definition.footprint.minimumDepthMeters)~$($Definition.footprint.maximumDepthMeters)m")
    [void] $builder.AppendLine("- 경사 범위: $($Definition.slope.minimumDegrees)~$($Definition.slope.maximumDegrees)도")
    [void] $builder.AppendLine("- 수계 마스크 필요: $($Definition.requiresWaterMask)")
    [void] $builder.AppendLine("- 토지피복 후보: $(@($Definition.allowedLandCoverCodes) -join ', ')")
    [void] $builder.AppendLine("- 지역 역할 후보: $(@($Definition.allowedRegionRoleCodes) -join ', ')")
    $connectorTypes = @($Definition.connectorTypeCodes) -join ', '
    if ([string]::IsNullOrWhiteSpace($connectorTypes)) { $connectorTypes = "없음" }
    [void] $builder.AppendLine("- 연결구 종류: $connectorTypes")
    [void] $builder.AppendLine("- 회전: $(@($Definition.rotationCodes) -join ', '), 반전 허용: $($Definition.mirrorAllowed)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## 권위 경계")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("이 카드는 한 Synty 팩의 표현 가능성을 설명한다. WI·공간 능력·업무 용량·실제 지역 위치를 확정하지 않으며 행동 공간 H1과 사람 검토를 거쳐야 공식 공간 정의에 참여할 수 있다.")
    return ConvertTo-DeterministicText $builder.ToString()
}

function New-H4Markdown([object] $Definition) {
    $builder = [Text.StringBuilder]::new()
    [void] $builder.AppendLine("# H4 지역 청사진 후보 — $($Definition.title)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("@spatial-knowledge $($Definition.stableId)")
    [void] $builder.AppendLine("@hierarchy H4")
    [void] $builder.AppendLine("@card-kind RegionalBlueprint")
    [void] $builder.AppendLine("@state $($Definition.knowledgeStateCode)")
    [void] $builder.AppendLine("@derivation-recipe $($Definition.derivationRecipeCode)")
    foreach ($reference in @($Definition.requiredH3Refs)) { [void] $builder.AppendLine("@required-h3 $reference") }
    foreach ($reference in @($Definition.optionalH3Refs)) { [void] $builder.AppendLine("@optional-h3 $reference") }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine($Definition.summary)
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## 세계 주제")
    [void] $builder.AppendLine()
    foreach ($theme in @($Definition.worldThemeCodes)) { [void] $builder.AppendLine("- ``$theme``") }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## 권위 경계")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("이 후보는 위치 독립 지역 세계 설계다. 지역 코드·좌표·공공데이터 요구·실제 LandscapeGraph StableId를 갖지 않으며, 하위 H3 구성과 관계를 사람이 검토한 뒤 설계 재고로 승인한다. 실제 AreaSet 배치와 공공데이터 연결은 각각 E5·E6에서 별도로 수행한다.")
    return ConvertTo-DeterministicText $builder.ToString()
}

$knowledgeRoot = Resolve-RepositoryPath $KnowledgeRootPath
$v2CatalogPath = Join-Path $knowledgeRoot "catalog.v2.json"
$v2Catalog = Read-Json $v2CatalogPath
$grammar = Read-Json (Resolve-RepositoryPath $GrammarPath)
$recipes = Read-Json (Resolve-RepositoryPath $RecipePath)

Require ([string] $v2Catalog.schemaVersion -eq "simulation-world-spatial-design-knowledge-catalog.v2") "V2CatalogSchemaInvalid"
Require ([string] $grammar.catalogRevision -eq [string] $recipes.grammarRevision) "GrammarRevisionMismatch"
Require ([bool] $grammar.presentationOnly -and [bool] $recipes.presentationOnly) "PresentationOnlyRequired"
Require (-not [bool] $recipes.isOperationalState) "OperationalStateForbidden"
foreach ($requiredPolicy in @("requiresHumanReview", "forbidsAutomaticAuthorityPromotion", "forbidsScenarioFallback", "h4BlueprintIsNotAreaSet")) {
    Require ([bool] $recipes.promotionPolicy.$requiredPolicy) "PromotionPolicyRequired:$requiredPolicy"
}
Require (@($grammar.entries).Count -eq 156) "GrammarEntryCountMustBe156"

$grammarByKey = @{}
$grammarGroups = @{}
foreach ($entry in @($grammar.entries)) {
    $key = [string] $entry.compositionKey
    Require (-not $grammarByKey.ContainsKey($key)) "GrammarKeyDuplicate:$key"
    $grammarByKey[$key] = $entry
    $groupKey = "$($entry.familyCode):$($entry.setName)"
    if (-not $grammarGroups.ContainsKey($groupKey)) { $grammarGroups[$groupKey] = @() }
    $grammarGroups[$groupKey] += $entry
}
Require ($grammarGroups.Count -eq 52) "GrammarMeaningGroupCountMustBe52"
foreach ($group in $grammarGroups.GetEnumerator()) {
    Require (@($group.Value).Count -eq 3) "GrammarGroupVariantCount:$($group.Key)"
    Require ((@($group.Value.variantCode | Sort-Object) -join ",") -eq "A,B,C") "GrammarGroupVariantsInvalid:$($group.Key)"
}

$definitionsById = @{}
$definitionRefsByLevel = @{ H1 = @(); H2 = @(); H3 = @() }
foreach ($level in @("H1", "H2", "H3")) {
    $property = "$($level.ToLowerInvariant())DefinitionRefs"
    foreach ($reference in @($v2Catalog.$property)) {
        $path = Join-Path $knowledgeRoot ([string] $reference.definitionPath)
        $definition = Read-Json $path
        Require ((Get-FileTextSha256 $path) -eq [string] $reference.definitionSha256) "V2DefinitionHashMismatch:$($reference.stableId)"
        $documentPath = Join-Path $knowledgeRoot ([string] $reference.documentPath)
        Require (Test-Path -LiteralPath $documentPath) "V2DocumentMissing:$($reference.stableId)"
        Require ((Get-FileTextSha256 $documentPath) -eq [string] $reference.documentSha256) "V2DocumentHashMismatch:$($reference.stableId)"
        $definitionsById[[string] $definition.stableId] = $definition
        $definitionRefsByLevel[$level] += $reference
    }
}
Require (@($definitionRefsByLevel.H1).Count -eq 52) "InteractionH1CountMustBe52"
Require (@($definitionRefsByLevel.H2).Count -eq 38) "H2CountMustBe38"
Require (@($definitionRefsByLevel.H3).Count -eq 20) "H3CountMustBe20"

$packPolicy = $recipes.h1ExpressionPolicy
$packFamilies = @($packPolicy.sourceFamilyCodes)
$expressionGroups = @($grammarGroups.GetEnumerator() | Where-Object { $packFamilies -contains ([string] $_.Value[0].familyCode) } | Sort-Object Name)
Require ($expressionGroups.Count -eq 32) "PackExpressionMeaningCountMustBe32"
foreach ($family in $packFamilies) {
    $actual = @($expressionGroups | Where-Object { [string] $_.Value[0].familyCode -eq $family }).Count
    $expected = [int] $packPolicy.expectedMeaningGroupCounts.$family
    Require ($actual -eq $expected) "PackMeaningCountMismatch:${family}:$actual/$expected"
}

$expressionDefinitions = @()
$expressionReferenceRows = @()
$expressionStableIds = @{}
foreach ($group in $expressionGroups) {
    $entries = @($group.Value | Sort-Object variantCode)
    $family = [string] $entries[0].familyCode
    $setName = [string] $entries[0].setName
    $slug = Get-NormalizedSlug $setName
    $stableId = "h1-expression:${family}:$slug"
    Require (-not $expressionStableIds.ContainsKey($stableId)) "ExpressionStableIdCollision:$stableId"
    $expressionStableIds[$stableId] = $true
    $sourceGrammarSetRef = "${family}:$setName"
    $supports = @($definitionRefsByLevel.H1 | ForEach-Object {
        $definition = $definitionsById[[string] $_.stableId]
        if (@($definition.grammarSetRefs) -contains $sourceGrammarSetRef) { [string] $definition.stableId }
    } | Sort-Object -Unique)
    $definitionPath = "definitions/h1-expression/$family/$slug.v3.json"
    $documentPath = "authored/h1-expression/$family/$slug.v3.md"
    $input = [pscustomobject][ordered]@{
        recipeRevision = if ($packPolicy.PSObject.Properties.Name -contains "revision") { [string] $packPolicy.revision } else { [string] $recipes.revision }
        grammarCatalogHashSha256 = [string] $grammar.catalogHashSha256
        grammarEntries = $entries
    }
    $definition = [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h1-expression.v3"
        stableId = $stableId
        revision = 1
        hierarchyLevelCode = "H1"
        cardKindCode = "PackExpression"
        title = "$setName — $(Get-PackLabel $family) 단독 표현"
        summary = "$setName 의미군의 A/B/C 표현 변형을 한 장으로 묶은 위치 독립 팩 단독 탐색 카드다."
        knowledgeStateCode = if ($supports.Count -eq 0) { [string] $packPolicy.unlinkedKnowledgeStateCode } else { [string] $packPolicy.knowledgeStateCode }
        originModeCode = [string] $packPolicy.originModeCode
        sourcePackCode = Get-PackLabel $family
        sourceGrammarSetRef = $sourceGrammarSetRef
        sourceGrammarRefs = @($entries.compositionKey)
        grammarVariantRefs = @($entries.compositionKey)
        topologyCodes = Get-UniqueStrings @($entries.topologyCode)
        assemblyScaleCodes = Get-UniqueStrings @($entries.assemblyScaleCode)
        footprint = [pscustomobject][ordered]@{
            minimumWidthMeters = [double] (($entries.footprintX | Measure-Object -Minimum).Minimum)
            maximumWidthMeters = [double] (($entries.footprintX | Measure-Object -Maximum).Maximum)
            minimumDepthMeters = [double] (($entries.footprintY | Measure-Object -Minimum).Minimum)
            maximumDepthMeters = [double] (($entries.footprintY | Measure-Object -Maximum).Maximum)
        }
        slope = [pscustomobject][ordered]@{
            minimumDegrees = [double] (($entries.minimumSlopeDegrees | Measure-Object -Minimum).Minimum)
            maximumDegrees = [double] (($entries.maximumSlopeDegrees | Measure-Object -Maximum).Maximum)
        }
        requiresWaterMask = @($entries.requiresWaterMask) -contains $true
        allowedLandCoverCodes = Get-UniqueStrings @(Get-PropertyValues $entries "allowedLandCoverCodes")
        allowedRegionRoleCodes = Get-UniqueStrings @(Get-PropertyValues $entries "allowedRegionRoleCodes")
        edgeProfileCodes = Get-UniqueStrings @(Get-PropertyValues $entries "edgeProfiles" "profileCode")
        connectorTypeCodes = Get-UniqueStrings @(Get-PropertyValues $entries "connectors" "connectorTypeCode")
        rotationCodes = Get-UniqueStrings @(Get-PropertyValues $entries "rotationCodes")
        mirrorAllowed = @($entries.mirrorAllowed) -contains $true
        repeatAllowed = @($entries.allowRepeat) -contains $true
        renderCost = [pscustomobject][ordered]@{
            minimumTriangleCount = [int] (($entries.triangleCount | Measure-Object -Minimum).Minimum)
            maximumTriangleCount = [int] (($entries.triangleCount | Measure-Object -Maximum).Maximum)
            maximumMaterialSlotCount = [int] (($entries.materialSlotCount | Measure-Object -Maximum).Maximum)
            maximumRendererCount = [int] (($entries.rendererCount | Measure-Object -Maximum).Maximum)
            maximumColliderCount = [int] (($entries.colliderCount | Measure-Object -Maximum).Maximum)
        }
        supportsInteractionH1Refs = $supports
        derivationRecipeCode = "h1.pack-expression.$family.r1"
        derivationInputHashSha256 = Get-TextSha256 (ConvertTo-StableJson $input)
        authoredDocument = $documentPath
        unresolvedItems = if ($supports.Count -eq 0) { @("연결된 행동 공간 H1과 게임 기획 묶음이 없어 IdeaInventory로 격리한다.") } else { @("행동 공간 H1과 결합해도 WI·능력·용량은 별도 검토한다.") }
        presentationOnly = $true
        isOperationalState = $false
    }
    $definitionJson = ConvertTo-StableJson $definition
    Require (-not ($definitionJson -match '"(areaSetStableId|landscapeGraphStableId|dataRequirement|absoluteWorldPosition|worldEastingMeters|worldNorthingMeters|latitude|longitude|prefabPath|assetGuid|scenePath)"')) "ExpressionAuthorityFieldForbidden:$stableId"
    $markdown = New-ExpressionMarkdown $definition
    $definitionAbsolute = Join-Path $knowledgeRoot $definitionPath
    $documentAbsolute = Join-Path $knowledgeRoot $documentPath
    Write-Or-Check $definitionAbsolute $definitionJson "ExpressionDefinition"
    Write-Or-Check $documentAbsolute $markdown "ExpressionDocument"
    $expressionDefinitions += $definition
    $expressionReferenceRows += [pscustomobject][ordered]@{
        stableId = $stableId
        revision = 1
        cardKindCode = "PackExpression"
        definitionPath = $definitionPath
        definitionSha256 = Get-TextSha256 $definitionJson
        documentPath = $documentPath
        documentSha256 = Get-TextSha256 $markdown
    }
}

$allGrammarSetRefs = @($grammarGroups.Keys | Sort-Object)
function Require-GrammarSetRefs([object[]] $References, [string] $Owner) {
    foreach ($reference in @($References)) { Require ($allGrammarSetRefs -contains [string] $reference) "GrammarSetUnknown:${Owner}:$reference" }
}

$h2Bindings = @()
$h2BindingById = @{}
$h2Recipes = @($recipes.h2Derivations)
Require ($h2Recipes.Count -eq 38) "H2RecipeCountMustBe38"
Require (@($h2Recipes.targetKnowledgeRef | Sort-Object -Unique).Count -eq 38) "H2RecipeTargetDuplicate"
foreach ($recipe in $h2Recipes | Sort-Object targetKnowledgeRef) {
    $targetId = [string] $recipe.targetKnowledgeRef
    Require ($definitionsById.ContainsKey($targetId)) "H2RecipeTargetUnknown:$targetId"
    $target = $definitionsById[$targetId]
    Require ([string] $target.hierarchyLevelCode -eq "H2") "H2RecipeTargetLevel:$targetId"
    $children = Get-UniqueStrings @($target.requiredH1Refs + $target.optionalH1Refs)
    $childGrammar = @()
    $childHashes = @()
    foreach ($childId in $children) {
        Require ($definitionsById.ContainsKey($childId)) "H2ChildUnknown:${targetId}:$childId"
        $childGrammar += @($definitionsById[$childId].grammarSetRefs)
        $childRef = @($definitionRefsByLevel.H1 | Where-Object { [string] $_.stableId -eq $childId })[0]
        $childHashes += [string] $childRef.definitionSha256
    }
    $sourceGrammar = Get-UniqueStrings @($childGrammar + $recipe.supplementalGrammarSetRefs)
    Require-GrammarSetRefs $sourceGrammar $targetId
    Require (@($recipe.supplementalGrammarSetRefs | Where-Object { ([string] $_).StartsWith("network:") }).Count -gt 0) "H2NetworkGrammarMissing:$targetId"
    $input = [pscustomobject][ordered]@{ recipe = $recipe; targetRevision = $target.revision; childHashes = @($childHashes | Sort-Object); sourceGrammarRefs = $sourceGrammar }
    $binding = [pscustomobject][ordered]@{
        targetKnowledgeRef = $targetId
        targetHierarchyLevelCode = "H2"
        derivationRecipeCode = [string] $recipe.recipeCode
        childKnowledgeRefs = $children
        sourceGrammarRefs = $sourceGrammar
        derivationInputHashSha256 = Get-TextSha256 (ConvertTo-StableJson $input)
        presentationOnly = $true
    }
    $h2Bindings += $binding
    $h2BindingById[$targetId] = $binding
}

$h3Bindings = @()
$h3BindingById = @{}
$h3Recipes = @($recipes.h3Derivations)
Require ($h3Recipes.Count -eq 20) "H3RecipeCountMustBe20"
Require (@($h3Recipes.targetKnowledgeRef | Sort-Object -Unique).Count -eq 20) "H3RecipeTargetDuplicate"
foreach ($recipe in $h3Recipes | Sort-Object targetKnowledgeRef) {
    $targetId = [string] $recipe.targetKnowledgeRef
    Require ($definitionsById.ContainsKey($targetId)) "H3RecipeTargetUnknown:$targetId"
    $target = $definitionsById[$targetId]
    Require ([string] $target.hierarchyLevelCode -eq "H3") "H3RecipeTargetLevel:$targetId"
    $children = Get-UniqueStrings @($target.requiredH2Refs + $target.optionalH2Refs)
    $sourceGrammar = @()
    $childHashes = @()
    foreach ($childId in $children) {
        Require ($h2BindingById.ContainsKey($childId)) "H3ChildBindingMissing:${targetId}:$childId"
        $sourceGrammar += @($h2BindingById[$childId].sourceGrammarRefs)
        $childHashes += [string] $h2BindingById[$childId].derivationInputHashSha256
    }
    $sourceGrammar = Get-UniqueStrings @($sourceGrammar + $recipe.supplementalGrammarSetRefs)
    Require-GrammarSetRefs $sourceGrammar $targetId
    Require (@($recipe.supplementalGrammarSetRefs | Where-Object { ([string] $_).StartsWith("transition:") }).Count -gt 0) "H3TransitionGrammarMissing:$targetId"
    $input = [pscustomobject][ordered]@{ recipe = $recipe; targetRevision = $target.revision; childHashes = @($childHashes | Sort-Object); sourceGrammarRefs = $sourceGrammar }
    $binding = [pscustomobject][ordered]@{
        targetKnowledgeRef = $targetId
        targetHierarchyLevelCode = "H3"
        derivationRecipeCode = [string] $recipe.recipeCode
        childKnowledgeRefs = $children
        sourceGrammarRefs = $sourceGrammar
        derivationInputHashSha256 = Get-TextSha256 (ConvertTo-StableJson $input)
        presentationOnly = $true
    }
    $h3Bindings += $binding
    $h3BindingById[$targetId] = $binding
}

$h4Definitions = @()
$h4ReferenceRows = @()
Require (@($recipes.h4Blueprints).Count -eq 6) "H4BlueprintCountMustBe6"
foreach ($blueprint in @($recipes.h4Blueprints | Sort-Object stableId)) {
    $stableId = [string] $blueprint.stableId
    $children = Get-UniqueStrings @($blueprint.requiredH3Refs + $blueprint.optionalH3Refs)
    $sourceGrammar = @()
    $childHashes = @()
    foreach ($childId in $children) {
        Require ($h3BindingById.ContainsKey($childId)) "H4ChildBindingMissing:${stableId}:$childId"
        $sourceGrammar += @($h3BindingById[$childId].sourceGrammarRefs)
        $childHashes += [string] $h3BindingById[$childId].derivationInputHashSha256
    }
    $sourceGrammar = Get-UniqueStrings $sourceGrammar
    $input = [pscustomobject][ordered]@{ blueprint = $blueprint; childHashes = @($childHashes | Sort-Object); sourceGrammarRefs = $sourceGrammar }
    $slug = Get-NormalizedSlug ($stableId.Split(":")[-1])
    $definitionPath = "definitions/h4/$slug.v3.json"
    $documentPath = "authored/h4/$slug.v3.md"
    $definition = [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h4-blueprint.v3"
        stableId = $stableId
        revision = 1
        hierarchyLevelCode = "H4"
        cardKindCode = "RegionalBlueprint"
        title = [string] $blueprint.title
        summary = [string] $blueprint.summary
        knowledgeStateCode = "ExploratoryInventory"
        originModeCode = "ExpressionExploration"
        requiredH3Refs = @($blueprint.requiredH3Refs)
        optionalH3Refs = @($blueprint.optionalH3Refs)
        childKnowledgeRefs = $children
        worldThemeCodes = @($blueprint.worldThemeCodes)
        graphRelationRoleCodes = @($blueprint.graphRelationRoleCodes)
        sourceGrammarRefs = $sourceGrammar
        derivationRecipeCode = [string] $blueprint.recipeCode
        derivationInputHashSha256 = Get-TextSha256 (ConvertTo-StableJson $input)
        authoredDocument = $documentPath
        unresolvedItems = @("하위 H3 구성·관계와 외부 연결구에 대한 사람의 설계 검토가 필요하다.")
        presentationOnly = $true
        isOperationalState = $false
    }
    $definitionJson = ConvertTo-StableJson $definition
    Require (-not ($definitionJson -match '"(areaSetStableId|landscapeGraphStableId|dataRequirement|requiredEvidencePurposeCodes|absoluteWorldPosition|worldEastingMeters|worldNorthingMeters|latitude|longitude|prefabPath|assetGuid|scenePath)"')) "H4AuthorityFieldForbidden:$stableId"
    $markdown = New-H4Markdown $definition
    Write-Or-Check (Join-Path $knowledgeRoot $definitionPath) $definitionJson "H4Definition"
    Write-Or-Check (Join-Path $knowledgeRoot $documentPath) $markdown "H4Document"
    $h4Definitions += $definition
    $h4ReferenceRows += [pscustomobject][ordered]@{
        stableId = $stableId
        revision = 1
        cardKindCode = "RegionalBlueprint"
        definitionPath = $definitionPath
        definitionSha256 = Get-TextSha256 $definitionJson
        documentPath = $documentPath
        documentSha256 = Get-TextSha256 $markdown
    }
}

$coveredGrammarSets = Get-UniqueStrings @(
    @($expressionDefinitions.sourceGrammarSetRef) +
    @($h2Bindings.sourceGrammarRefs) +
    @($h3Bindings.sourceGrammarRefs)
)
$missingGrammarSets = @($allGrammarSetRefs | Where-Object { $coveredGrammarSets -notcontains $_ })
Require ($missingGrammarSets.Count -eq 0) "GrammarMeaningLineageMissing:$($missingGrammarSets -join ',')"
$packGrammarKeys = @($grammar.entries | Where-Object { $packFamilies -contains [string] $_.familyCode } | ForEach-Object compositionKey | Sort-Object)
$expressionGrammarKeys = @($expressionDefinitions.grammarVariantRefs | Sort-Object)
Require ($expressionGrammarKeys.Count -eq 96) "PackExpressionVariantCountMustBe96"
Require (@($expressionGrammarKeys | Sort-Object -Unique).Count -eq 96) "PackExpressionVariantDuplicate"
Require (($packGrammarKeys -join "`n") -ceq ($expressionGrammarKeys -join "`n")) "PackExpressionVariantCoverageMismatch"
$networkSetRefs = @($allGrammarSetRefs | Where-Object { $_.StartsWith("network:") } | Sort-Object)
$h2NetworkSetRefs = @($h2Bindings.sourceGrammarRefs | Where-Object { $_.StartsWith("network:") } | Sort-Object -Unique)
Require (($networkSetRefs -join "`n") -ceq ($h2NetworkSetRefs -join "`n")) "NetworkGrammarH2CoverageMismatch"
$transitionSetRefs = @($allGrammarSetRefs | Where-Object { $_.StartsWith("transition:") } | Sort-Object)
$h3TransitionSetRefs = @($h3Bindings.sourceGrammarRefs | Where-Object { $_.StartsWith("transition:") } | Sort-Object -Unique)
Require (($transitionSetRefs -join "`n") -ceq ($h3TransitionSetRefs -join "`n")) "TransitionGrammarH3CoverageMismatch"

$interactionReferenceRows = @($definitionRefsByLevel.H1 | ForEach-Object {
    [pscustomobject][ordered]@{
        stableId = [string] $_.stableId
        revision = [int] $_.revision
        cardKindCode = "InteractionSpace"
        definitionPath = [string] $_.definitionPath
        definitionSha256 = [string] $_.definitionSha256
        documentPath = [string] $_.documentPath
        documentSha256 = [string] $_.documentSha256
    }
})
$h1Combined = @($interactionReferenceRows + $expressionReferenceRows | Sort-Object stableId)
$catalogPayload = [pscustomobject][ordered]@{
    schemaVersion = "simulation-world-spatial-design-knowledge-catalog.v3"
    revision = "simulation-world-spatial-design-knowledge.r3"
    title = "기준 경관 문법 기반 H1~H4 상향 유도 공간 설계 지식"
    summary = "52개 의미군·156개 표현 변형에서 검토된 조립법으로 H1~H4 설계 후보와 계보를 제공한다."
    previousCatalogPath = "$KnowledgeRootPath/catalog.v2.json"
    previousCatalogSha256 = Get-FileTextSha256 $v2CatalogPath
    grammarCatalogPath = $GrammarPath
    grammarRevision = [string] $grammar.catalogRevision
    grammarCatalogHashSha256 = [string] $grammar.catalogHashSha256
    derivationRecipePath = $RecipePath
    derivationRecipeSha256 = Get-FileTextSha256 (Resolve-RepositoryPath $RecipePath)
    h1InteractionDefinitionRefs = @($interactionReferenceRows | Sort-Object stableId)
    h1ExpressionDefinitionRefs = @($expressionReferenceRows | Sort-Object stableId)
    h1DefinitionRefs = $h1Combined
    h2DefinitionRefs = @($definitionRefsByLevel.H2 | Sort-Object stableId)
    h3DefinitionRefs = @($definitionRefsByLevel.H3 | Sort-Object stableId)
    h4DefinitionRefs = @($h4ReferenceRows | Sort-Object stableId)
    h2DerivationBindings = @($h2Bindings | Sort-Object targetKnowledgeRef)
    h3DerivationBindings = @($h3Bindings | Sort-Object targetKnowledgeRef)
    counts = [pscustomobject][ordered]@{
        grammarMeaningGroups = 52
        grammarVariants = 156
        h1Interaction = 52
        h1Expression = 32
        h1Total = 84
        h2 = 18
        h3 = 10
        h4Blueprint = 5
    }
    authorityBoundary = "문법과 유도 후보는 세계 가능성을 제안할 뿐 H 정의·E 증거·AreaSet·LandscapeGraph·Simulation 상태를 자동 승인하지 않는다."
    presentationOnly = $true
    isOperationalState = $false
}
$catalog = [pscustomobject][ordered]@{}
foreach ($property in $catalogPayload.PSObject.Properties) { $catalog | Add-Member -NotePropertyName $property.Name -NotePropertyValue $property.Value }
$catalog | Add-Member -NotePropertyName contentHashSha256 -NotePropertyValue (Get-TextSha256 (ConvertTo-StableJson $catalogPayload))
$catalogJson = ConvertTo-StableJson $catalog
Write-Or-Check (Join-Path $knowledgeRoot "catalog.v3.json") $catalogJson "CatalogV3"

$guide = [Text.StringBuilder]::new()
[void] $guide.AppendLine("# 기준 경관 문법 156개 사용 설명서")
[void] $guide.AppendLine()
[void] $guide.AppendLine("> 이 문서는 ``$GrammarPath``와 ``$RecipePath``에서 결정적으로 생성된다. 직접 수정하지 않는다.")
[void] $guide.AppendLine()
[void] $guide.AppendLine("기준 경관 문법은 52개 공간 의미군의 A/B/C 표현 변형 156개다. 문법은 H 계층의 재료이며 공식 공간 정의나 Simulation 권위가 아니다.")
[void] $guide.AppendLine()
[void] $guide.AppendLine("| 계열 | 의미군 | 표현 변형 | H 유도 역할 |")
[void] $guide.AppendLine("| --- | ---: | ---: | --- |")
foreach ($family in @("nature", "farm", "town", "city", "network", "transition")) {
    $meaningCount = @($grammarGroups.GetEnumerator() | Where-Object { [string] $_.Value[0].familyCode -eq $family }).Count
    $role = if ($packFamilies -contains $family) { "팩 단독 H1 표현 카드" } elseif ($family -eq "network") { "H2 블록 골격" } else { "H3 경관 연결·전환" }
    [void] $guide.AppendLine("| ``$family`` | $meaningCount | $($meaningCount * 3) | $role |")
}
[void] $guide.AppendLine()
[void] $guide.AppendLine('```text')
[void] $guide.AppendLine("기준 문법 → H1 장소·표현 → H2 블록 → H3 경관 → H4 지역 청사진 → 사람 승인·현실 근거")
[void] $guide.AppendLine('```')

$packCards = [Text.StringBuilder]::new()
[void] $packCards.AppendLine("# 팩별 H1 표현 탐색 카드")
[void] $packCards.AppendLine()
[void] $packCards.AppendLine("| 팩 | 카드 | 기준 문법 A/B/C | 연결 행동 H1 |")
[void] $packCards.AppendLine("| --- | --- | --- | --- |")
foreach ($item in $expressionDefinitions | Sort-Object sourcePackCode, stableId) {
    [void] $packCards.AppendLine("| ``$($item.sourcePackCode)`` | ``$($item.stableId)`` $($item.title) | $(Escape-Markdown (@($item.grammarVariantRefs) -join ', ')) | $(Escape-Markdown (@($item.supportsInteractionH1Refs) -join ', ')) |")
}

$mapping = [Text.StringBuilder]::new()
[void] $mapping.AppendLine("# 행동 공간 H1과 팩 표현 H1 대응표")
[void] $mapping.AppendLine()
[void] $mapping.AppendLine("| 행동 공간 H1 | 표현 H1 후보 |")
[void] $mapping.AppendLine("| --- | --- |")
foreach ($reference in $interactionReferenceRows | Sort-Object stableId) {
    $matches = @($expressionDefinitions | Where-Object { @($_.supportsInteractionH1Refs) -contains [string] $reference.stableId } | ForEach-Object stableId)
    [void] $mapping.AppendLine("| ``$($reference.stableId)`` | $(Escape-Markdown ($matches -join ', ')) |")
}

$lineage = [Text.StringBuilder]::new()
[void] $lineage.AppendLine("# 기준 문법에서 H1~H4로 이어지는 유도 계보")
[void] $lineage.AppendLine()
[void] $lineage.AppendLine("| 계층 | 후보 | 조립법 | 하위 지식 | 기준 문법 |")
[void] $lineage.AppendLine("| --- | --- | --- | --- | --- |")
foreach ($binding in @($h2Bindings + $h3Bindings)) {
    [void] $lineage.AppendLine("| ``$($binding.targetHierarchyLevelCode)`` | ``$($binding.targetKnowledgeRef)`` | ``$($binding.derivationRecipeCode)`` | $(Escape-Markdown (@($binding.childKnowledgeRefs) -join ', ')) | $(Escape-Markdown (@($binding.sourceGrammarRefs) -join ', ')) |")
}
foreach ($item in $h4Definitions) {
    [void] $lineage.AppendLine("| ``H4`` | ``$($item.stableId)`` | ``$($item.derivationRecipeCode)`` | $(Escape-Markdown (@($item.childKnowledgeRefs) -join ', ')) | $(Escape-Markdown (@($item.sourceGrammarRefs) -join ', ')) |")
}

$h4Document = [Text.StringBuilder]::new()
[void] $h4Document.AppendLine("# H4 지역 청사진 후보집")
[void] $h4Document.AppendLine()
[void] $h4Document.AppendLine("> 이 후보들은 위치 독립 설계 재고이며 실제 지역 코드·좌표·공공데이터 요구·LandscapeGraph StableId를 갖지 않는다. 실제 배치와 공공데이터 연결은 E5·E6에서 분리한다.")
[void] $h4Document.AppendLine()
foreach ($item in $h4Definitions | Sort-Object stableId) {
    [void] $h4Document.AppendLine("## $($item.title)")
    [void] $h4Document.AppendLine()
    [void] $h4Document.AppendLine("- 후보: ``$($item.stableId)``")
    [void] $h4Document.AppendLine("- 필수 H3: $(@($item.requiredH3Refs) -join ', ')")
    $optionalH3 = @($item.optionalH3Refs) -join ', '
    if ([string]::IsNullOrWhiteSpace($optionalH3)) { $optionalH3 = "없음" }
    [void] $h4Document.AppendLine("- 선택 H3: $optionalH3")
    [void] $h4Document.AppendLine("- 설계 관계: $(@($item.graphRelationRoleCodes) -join ', ')")
    [void] $h4Document.AppendLine()
}

$gap = [Text.StringBuilder]::new()
[void] $gap.AppendLine("# 공간 설계 지식 v3 공백 보고서")
[void] $gap.AppendLine()
[void] $gap.AppendLine("| 계층 | 지식 | 공백 |")
[void] $gap.AppendLine("| --- | --- | --- |")
foreach ($item in $expressionDefinitions | Where-Object { @($_.supportsInteractionH1Refs).Count -eq 0 } | Sort-Object stableId) {
    [void] $gap.AppendLine("| ``H1`` | ``$($item.stableId)`` $($item.title) | 연결된 행동 공간 H1이 없다. |")
}
foreach ($item in $h4Definitions | Sort-Object stableId) {
    [void] $gap.AppendLine("| ``H4`` | ``$($item.stableId)`` $($item.title) | 실제 AreaSet 세계 의도와 지역 근거가 없다. |")
}

$promotion = [Text.StringBuilder]::new()
[void] $promotion.AppendLine("# 공간 설계 지식 v3 승격 대기열")
[void] $promotion.AppendLine()
[void] $promotion.AppendLine("> 유도 계보는 후보를 자동 승인하지 않는다.")
[void] $promotion.AppendLine()
[void] $promotion.AppendLine("| 우선 | 계층 | 후보 | 다음 관문 |")
[void] $promotion.AppendLine("| ---: | --- | --- | --- |")
$priority = 1
foreach ($item in $expressionDefinitions | Sort-Object @{ Expression = { @($_.supportsInteractionH1Refs).Count }; Descending = $true }, stableId) {
    $gate = if (@($item.supportsInteractionH1Refs).Count -gt 0) { "행동 H1과 능력·용량·연결구 검토" } else { "행동 공간 H1 연결" }
    [void] $promotion.AppendLine("| $priority | ``H1`` | ``$($item.stableId)`` | $gate |")
    $priority++
}
foreach ($item in $h4Definitions | Sort-Object stableId) {
    [void] $promotion.AppendLine("| $priority | ``H4`` | ``$($item.stableId)`` | 세계 의도·하위 H3·관계·외부 연결구 설계 검토 |")
    $priority++
}

$generated = [ordered]@{
    "docs/AI/generated/landscape-grammar-156-guide.md" = $guide.ToString()
    "docs/AI/generated/spatial-knowledge-pack-expression-cards.md" = $packCards.ToString()
    "docs/AI/generated/spatial-knowledge-interaction-expression-mapping.md" = $mapping.ToString()
    "docs/AI/generated/spatial-knowledge-h-lineage.md" = $lineage.ToString()
    "docs/AI/generated/spatial-knowledge-h4-blueprints.md" = $h4Document.ToString().TrimEnd() + "`n"
    "docs/AI/generated/spatial-knowledge-v3-gap-report.md" = $gap.ToString()
    "docs/AI/generated/spatial-knowledge-v3-promotion-queue.md" = $promotion.ToString()
}
foreach ($entry in $generated.GetEnumerator()) {
    Write-Or-Check (Resolve-RepositoryPath $entry.Key) $entry.Value "GeneratedDocument"
}

$expressionFileCount = @(Get-ChildItem -LiteralPath (Join-Path $knowledgeRoot "definitions/h1-expression") -Filter "*.json" -File -Recurse).Count
$h4FileCount = @(Get-ChildItem -LiteralPath (Join-Path $knowledgeRoot "definitions/h4") -Filter "*.json" -File -Recurse).Count
Require ($expressionFileCount -eq 32) "ExpressionDefinitionFileCount:$expressionFileCount"
Require ($h4FileCount -eq 6) "H4DefinitionFileCount:$h4FileCount"

if ($Mode -eq "Check") {
    Write-Output "SpatialDesignKnowledgeV3Valid:Grammar=52/156;H1=84(52+32);H2=38;H3=20;H4=6"
}
else {
    Write-Output "SpatialDesignKnowledgeV3Generated:Grammar=52/156;H1=84(52+32);H2=38;H3=20;H4=6"
}
