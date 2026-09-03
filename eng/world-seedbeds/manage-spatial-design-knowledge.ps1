[CmdletBinding()]
param(
    [ValidateSet("Bootstrap", "Write", "Check")]
    [string] $Mode = "Check",
    [string] $KnowledgeRootPath = "eng/world-seedbeds/synty-bottom-up-inventory",
    [string] $LegacyCatalogPath = "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v1.json",
    [string] $ExpansionPath = "eng/world-seedbeds/synty-bottom-up-inventory/expansion.v2.json"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "SpatialDesignKnowledgeInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Resolve-RepositoryPath([string] $RepositoryRoot, [string] $RelativePath) {
    return Join-Path $RepositoryRoot ($RelativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}

function ConvertTo-StableText([string] $Content) {
    if ($null -eq $Content) { return "" }
    return $Content.Replace("`r`n", "`n").Replace("`r", "`n")
}

function ConvertTo-StableJson([object] $Value) {
    return ConvertTo-StableText (($Value | ConvertTo-Json -Depth 30) + "`n")
}

function Get-TextSha256([string] $Content) {
    $bytes = [Text.Encoding]::UTF8.GetBytes((ConvertTo-StableText $Content))
    return ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))).ToLowerInvariant()
}

function Write-TextIfChanged([string] $Path, [string] $Content) {
    $normalized = ConvertTo-StableText $Content
    if (Test-Path -LiteralPath $Path) {
        $current = ConvertTo-StableText ([IO.File]::ReadAllText($Path))
        if ($current -ceq $normalized) { return $false }
    }
    $directory = Split-Path -Parent $Path
    [IO.Directory]::CreateDirectory($directory) | Out-Null
    $temporaryPath = Join-Path $directory (".{0}.{1}.tmp" -f [IO.Path]::GetFileName($Path), [Guid]::NewGuid().ToString("N"))
    try {
        [IO.File]::WriteAllText($temporaryPath, $normalized, [Text.UTF8Encoding]::new($false))
        [IO.File]::Move($temporaryPath, $Path, $true)
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) { Remove-Item -LiteralPath $temporaryPath -Force }
    }
    return $true
}

function Read-Json([string] $Path) {
    Require (Test-Path -LiteralPath $Path) "SourceMissing:$Path"
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Get-Slug([string] $StableId) {
    return ($StableId.Split(":")[-1] -replace "\.v[0-9]+$", "")
}

function Get-PackCodes([object[]] $GrammarSetRefs) {
    $map = @{ nature = "Nature"; farm = "Farm"; town = "Town"; city = "City"; network = "Network"; transition = "Transition" }
    return @($GrammarSetRefs | ForEach-Object {
        $family = ([string] $_).Split(":")[0]
        if ($map.ContainsKey($family)) { $map[$family] } else { $family }
    } | Sort-Object -Unique)
}

function New-SignalObject([bool] $ExistingWi, [bool] $Future, [bool] $Reusable, [bool] $Synty) {
    return [pscustomobject][ordered]@{
        requiredByExistingWi = $ExistingWi
        likelyFutureGameplay = $Future
        reusableAcrossWorlds = $Reusable
        representableByOwnedSynty = $Synty
    }
}

function New-H1FromLegacy([object] $Item, [hashtable] $ApprovedDefinitions) {
    $approvedId = [string] $Item.approvedSeedbedStableId
    $isApproved = [string] $Item.stateCode -eq "ApprovedReference"
    $capabilities = @()
    $connectors = @()
    if ($isApproved -and $ApprovedDefinitions.ContainsKey($approvedId)) {
        $official = $ApprovedDefinitions[$approvedId]
        $capabilities = @($official.internalSpaces.capabilityCodes | Sort-Object -Unique)
        $connectors = @($official.externalConnectorStubs.connectorTypeCode | Sort-Object -Unique)
    }
    $stableId = [string] $Item.inventoryId
    $slug = Get-Slug $stableId
    return [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h1.v2"
        stableId = $stableId
        revision = 2
        hierarchyLevelCode = "H1"
        title = [string] $Item.title
        summary = "기존 상향식 재고 v1에서 이관한 $($Item.title) 설계 지식이다."
        knowledgeStateCode = if ($isApproved) { "ApprovedReference" } else { "ExploratoryInventory" }
        originModeCode = "Mixed"
        approvedDefinitionStableId = if ($isApproved) { $approvedId } else { $null }
        wiIds = @($Item.wiIds)
        anticipatedGameplayCodes = @()
        spatialRoleCodes = @($Item.spatialRoleCodes)
        capabilityCodes = $capabilities
        capacityConceptCodes = if ($Item.PSObject.Properties.Name -contains "capacityConceptCodes") { @($Item.capacityConceptCodes) } else { @() }
        predecessorH1Refs = @()
        successorH1Refs = @()
        connectorRoleCodes = $connectors
        sourcePackCodes = @(Get-PackCodes @($Item.grammarSetRefs))
        grammarSetRefs = @($Item.grammarSetRefs)
        qualificationSignals = New-SignalObject $true $true $true $true
        authoredDocument = "authored/h1/$slug.v2.md"
        sourceReferences = @("$LegacyCatalogPath#$stableId")
        unresolvedItems = if ($isApproved) { @() } else { @("공간 능력·용량·연결구를 검토한 뒤 CandidateForReview로 승격한다.") }
        presentationOnly = $true
        isOperationalState = $false
    }
}

function New-H1FromExpansion([object] $Item) {
    $slug = Get-Slug ([string] $Item.inventoryId)
    return [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h1.v2"
        stableId = [string] $Item.inventoryId
        revision = 1
        hierarchyLevelCode = "H1"
        title = [string] $Item.title
        summary = [string] $Item.summary
        knowledgeStateCode = [string] $Item.stateCode
        originModeCode = [string] $Item.originModeCode
        approvedDefinitionStableId = $null
        wiIds = @($Item.wiIds)
        anticipatedGameplayCodes = @($Item.anticipatedGameplayCodes)
        spatialRoleCodes = @($Item.spatialRoleCodes)
        capabilityCodes = @($Item.capabilityCodes)
        capacityConceptCodes = if ($Item.PSObject.Properties.Name -contains "capacityConceptCodes") { @($Item.capacityConceptCodes) } else { @() }
        predecessorH1Refs = @($Item.predecessorH1Refs)
        successorH1Refs = @($Item.successorH1Refs)
        connectorRoleCodes = @($Item.connectorRoleCodes)
        sourcePackCodes = @(Get-PackCodes @($Item.grammarSetRefs))
        grammarSetRefs = @($Item.grammarSetRefs)
        qualificationSignals = $Item.qualificationSignals
        authoredDocument = "authored/h1/$slug.v2.md"
        sourceReferences = @("$ExpansionPath#$($Item.inventoryId)")
        unresolvedItems = @("실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.")
        presentationOnly = $true
        isOperationalState = $false
    }
}

function New-H2FromLegacy([object] $Item) {
    $slug = Get-Slug ([string] $Item.candidateId)
    return [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h2.v2"
        stableId = [string] $Item.candidateId
        revision = 2
        hierarchyLevelCode = "H2"
        title = [string] $Item.title
        summary = "기존 상향식 재고 v1에서 이관한 $($Item.title) 조립 레시피다."
        knowledgeStateCode = "ExploratoryInventory"
        topologyCode = [string] $Item.topologyCode
        requiredH1Refs = @($Item.h1InventoryRefs)
        optionalH1Refs = @()
        connectorRoleCodes = @()
        sizeVariantCodes = @($Item.sizeVariantCodes)
        authoredDocument = "authored/h2/$slug.v2.md"
        sourceReferences = @("$LegacyCatalogPath#$($Item.candidateId)")
        unresolvedItems = @("필수 H1 사이 연결구와 내부 도달 가능성을 검토한다.")
        presentationOnly = $true
        isOperationalState = $false
    }
}

function New-H2FromExpansion([object] $Item) {
    $slug = Get-Slug ([string] $Item.candidateId)
    return [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h2.v2"
        stableId = [string] $Item.candidateId
        revision = 1
        hierarchyLevelCode = "H2"
        title = [string] $Item.title
        summary = [string] $Item.summary
        knowledgeStateCode = [string] $Item.stateCode
        topologyCode = [string] $Item.topologyCode
        requiredH1Refs = @($Item.requiredH1Refs)
        optionalH1Refs = @($Item.optionalH1Refs)
        connectorRoleCodes = @($Item.connectorRoleCodes)
        sizeVariantCodes = @($Item.sizeVariantCodes)
        authoredDocument = "authored/h2/$slug.v2.md"
        sourceReferences = @("$ExpansionPath#$($Item.candidateId)")
        unresolvedItems = @("기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.")
        presentationOnly = $true
        isOperationalState = $false
    }
}

function New-H3FromLegacy([object] $Item) {
    $slug = Get-Slug ([string] $Item.candidateId)
    $isSemanticCorridorRevision = [string] $Item.candidateId -in @("h3-candidate:farm-hub-logistics", "h3-candidate:hub-town-logistics")
    $summary = if ([string] $Item.candidateId -eq "h3-candidate:farm-hub-logistics") {
        "농장 출하 블록, Farm–Hub 회랑 블록, Hub 입고 블록을 연결점 의미와 방향에 맞춰 잇는 경계 통과 경관 청사진이다."
    }
    elseif ([string] $Item.candidateId -eq "h3-candidate:hub-town-logistics") {
        "Hub 출고 블록, Hub–Town 회랑 블록, Town 입고·시장 블록을 연결점 의미와 방향에 맞춰 잇는 경계 통과 경관 청사진이다."
    }
    else { "기존 상향식 재고 v1에서 이관한 $($Item.title) 지역 유형 청사진이다." }
    return [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h3.v2"
        stableId = [string] $Item.candidateId
        revision = if ($isSemanticCorridorRevision) { 3 } else { 2 }
        hierarchyLevelCode = "H3"
        title = [string] $Item.title
        summary = $summary
        knowledgeStateCode = "ExploratoryInventory"
        topologyCode = [string] $Item.topologyCode
        requiredH2Refs = @($Item.h2CandidateRefs)
        optionalH2Refs = @()
        connectorRoleCodes = @($Item.connectorRoleCodes)
        targetAreaRoleCodes = @()
        authoredDocument = "authored/h3/$slug.v2.md"
        sourceReferences = @("$LegacyCatalogPath#$($Item.candidateId)")
        unresolvedItems = @("AreaSet 적용 전에는 실제 Graph Node·Edge·좌표를 부여하지 않는다.")
        presentationOnly = $true
        isOperationalState = $false
    }
}

function New-H3FromExpansion([object] $Item) {
    $slug = Get-Slug ([string] $Item.candidateId)
    return [pscustomobject][ordered]@{
        schemaVersion = "simulation-world-spatial-design-knowledge-h3.v2"
        stableId = [string] $Item.candidateId
        revision = 1
        hierarchyLevelCode = "H3"
        title = [string] $Item.title
        summary = [string] $Item.summary
        knowledgeStateCode = [string] $Item.stateCode
        topologyCode = [string] $Item.topologyCode
        requiredH2Refs = @($Item.requiredH2Refs)
        optionalH2Refs = @($Item.optionalH2Refs)
        connectorRoleCodes = @($Item.connectorRoleCodes)
        targetAreaRoleCodes = @()
        authoredDocument = "authored/h3/$slug.v2.md"
        sourceReferences = @("$ExpansionPath#$($Item.candidateId)")
        unresolvedItems = @("실제 AreaSet과 공공데이터 근거를 적용하기 전까지 조립 후보로 유지한다.")
        presentationOnly = $true
        isOperationalState = $false
    }
}

function Add-DirectiveLines([Text.StringBuilder] $Builder, [string] $Name, [object[]] $Values) {
    foreach ($value in @($Values)) { [void] $Builder.AppendLine("@$Name $value") }
}

function New-KnowledgeMarkdown([object] $Definition) {
    $builder = [Text.StringBuilder]::new()
    [void] $builder.AppendLine("# $($Definition.title)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("@spatial-knowledge $($Definition.stableId)")
    [void] $builder.AppendLine("@hierarchy $($Definition.hierarchyLevelCode)")
    [void] $builder.AppendLine("@state $($Definition.knowledgeStateCode)")
    if ($Definition.hierarchyLevelCode -eq "H1") {
        Add-DirectiveLines $builder "wi" @($Definition.wiIds)
        Add-DirectiveLines $builder "gameplay" @($Definition.anticipatedGameplayCodes)
        Add-DirectiveLines $builder "role" @($Definition.spatialRoleCodes)
        Add-DirectiveLines $builder "capability" @($Definition.capabilityCodes)
        Add-DirectiveLines $builder "capacity" @($Definition.capacityConceptCodes)
        Add-DirectiveLines $builder "predecessor" @($Definition.predecessorH1Refs)
        Add-DirectiveLines $builder "successor" @($Definition.successorH1Refs)
        Add-DirectiveLines $builder "connector" @($Definition.connectorRoleCodes)
        Add-DirectiveLines $builder "grammar" @($Definition.grammarSetRefs)
    }
    elseif ($Definition.hierarchyLevelCode -eq "H2") {
        Add-DirectiveLines $builder "required-h1" @($Definition.requiredH1Refs)
        Add-DirectiveLines $builder "optional-h1" @($Definition.optionalH1Refs)
        Add-DirectiveLines $builder "connector" @($Definition.connectorRoleCodes)
    }
    else {
        Add-DirectiveLines $builder "required-h2" @($Definition.requiredH2Refs)
        Add-DirectiveLines $builder "optional-h2" @($Definition.optionalH2Refs)
        Add-DirectiveLines $builder "connector" @($Definition.connectorRoleCodes)
    }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## 존재 이유")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine($Definition.summary)
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## 설계 상태")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("- 재고 상태: ``$($Definition.knowledgeStateCode)``")
    [void] $builder.AppendLine("- 공간 계층: ``$($Definition.hierarchyLevelCode)``")
    [void] $builder.AppendLine("- 실제 지역 권위: 없음")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## 미해결")
    [void] $builder.AppendLine()
    foreach ($item in @($Definition.unresolvedItems)) { [void] $builder.AppendLine("- $item") }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.")
    return ConvertTo-StableText $builder.ToString()
}

function Test-DirectiveSet([string] $Markdown, [string] $Name, [object[]] $Expected, [string] $Id) {
    $matches = [regex]::Matches($Markdown, "(?m)^@$([regex]::Escape($Name)) (.+)$")
    $actual = @($matches | ForEach-Object { $_.Groups[1].Value.TrimEnd("`r") } | Sort-Object)
    $wanted = @($Expected | ForEach-Object { [string] $_ } | Sort-Object)
    Require (($actual -join "|") -eq ($wanted -join "|")) "MarkdownDirectiveMismatch:${Id}:$Name"
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$knowledgeRoot = Resolve-RepositoryPath $repositoryRoot $KnowledgeRootPath
$legacy = Read-Json (Resolve-RepositoryPath $repositoryRoot $LegacyCatalogPath)
$expansion = Read-Json (Resolve-RepositoryPath $repositoryRoot $ExpansionPath)
$wiCatalog = Read-Json (Resolve-RepositoryPath $repositoryRoot "eng/execution-ledgers/world-interactions.json")
$grammar = Read-Json (Resolve-RepositoryPath $repositoryRoot "eng/world-seedbeds/manifests/pyeongchang-landscape-grammar.v1.json")
$approvedCatalogPath = Resolve-RepositoryPath $repositoryRoot "eng/world-seedbeds/wi-spatial-seedbeds/catalog.json"
$approvedCatalog = Read-Json $approvedCatalogPath
$approvedDefinitions = @{}
foreach ($definitionRef in @($approvedCatalog.definitionRefs)) {
    $definition = Read-Json (Join-Path (Split-Path $approvedCatalogPath) ([string] $definitionRef))
    $approvedDefinitions[[string] $definition.stableId] = $definition
}

if ($Mode -eq "Bootstrap") {
    $h1 = @($legacy.h1Inventory | ForEach-Object { New-H1FromLegacy $_ $approvedDefinitions }) + @($expansion.h1Additions | ForEach-Object { New-H1FromExpansion $_ })
    $h2 = @($legacy.h2Candidates | ForEach-Object { New-H2FromLegacy $_ }) + @($expansion.h2Additions | ForEach-Object { New-H2FromExpansion $_ })
    $h3 = @($legacy.h3AssemblyCandidates | ForEach-Object { New-H3FromLegacy $_ }) + @($expansion.h3Additions | ForEach-Object { New-H3FromExpansion $_ })
    foreach ($definition in @($h1 + $h2 + $h3)) {
        $level = ([string] $definition.hierarchyLevelCode).ToLowerInvariant()
        $slug = Get-Slug ([string] $definition.stableId)
        $jsonPath = Join-Path $knowledgeRoot "definitions/$level/$slug.v2.json"
        $markdownPath = Join-Path $knowledgeRoot ([string] $definition.authoredDocument)
        if (-not (Test-Path -LiteralPath $jsonPath)) { [void] (Write-TextIfChanged $jsonPath (ConvertTo-StableJson $definition)) }
        if (-not (Test-Path -LiteralPath $markdownPath)) { [void] (Write-TextIfChanged $markdownPath (New-KnowledgeMarkdown $definition)) }
    }
}

$stateCodes = @("IdeaInventory", "ExploratoryInventory", "CandidateForReview", "ApprovedReference")
$originCodes = @("ExpressionExploration", "WorldIntent", "Mixed")
$wiIds = @{}
foreach ($item in @($wiCatalog.items)) { $wiIds[[string] $item.id] = $item }
$grammarKeys = @{}
foreach ($entry in @($grammar.entries)) { $grammarKeys[[string] $entry.compositionKey] = $true }

$definitionsByLevel = @{ H1 = @(); H2 = @(); H3 = @() }
$definitionsById = @{}
foreach ($level in @("h1", "h2", "h3")) {
    $directory = Join-Path $knowledgeRoot "definitions/$level"
    Require (Test-Path -LiteralPath $directory) "DefinitionDirectoryMissing:$level"
    foreach ($file in @(Get-ChildItem -LiteralPath $directory -Filter "*.json" -File | Sort-Object Name)) {
        $definition = Read-Json $file.FullName
        $id = [string] $definition.stableId
        Require-Text $id "DefinitionIdMissing:$($file.Name)"
        Require (-not $definitionsById.ContainsKey($id)) "DefinitionIdDuplicate:$id"
        Require ([string] $definition.hierarchyLevelCode -eq $level.ToUpperInvariant()) "HierarchyLevelMismatch:$id"
        Require ($stateCodes -contains [string] $definition.knowledgeStateCode) "KnowledgeStateInvalid:$id"
        Require ([bool] $definition.presentationOnly -and -not [bool] $definition.isOperationalState) "AuthorityBoundaryInvalid:$id"
        $raw = [IO.File]::ReadAllText($file.FullName)
        Require (-not ($raw -match '"(absoluteWorldPosition|worldEastingMeters|worldNorthingMeters|latitude|longitude|prefabPath|assetGuid|scenePath|landscapeGraphStableId|areaSetStableId|requiredEvidencePurposeCodes)"')) "AuthorityFieldForbidden:$id"
        $documentPath = Join-Path $knowledgeRoot ([string] $definition.authoredDocument)
        Require (Test-Path -LiteralPath $documentPath) "MarkdownMissing:$id"
        $markdown = [IO.File]::ReadAllText($documentPath)
        Test-DirectiveSet $markdown "spatial-knowledge" @($id) $id
        Test-DirectiveSet $markdown "hierarchy" @($definition.hierarchyLevelCode) $id
        Test-DirectiveSet $markdown "state" @($definition.knowledgeStateCode) $id
        if ($level -eq "h1") {
            Require ($originCodes -contains [string] $definition.originModeCode) "OriginModeInvalid:$id"
            Require (@($definition.spatialRoleCodes).Count -gt 0) "H1RoleMissing:$id"
            $trueSignals = @($definition.qualificationSignals.PSObject.Properties | Where-Object Value -eq $true).Count
            Require ($trueSignals -ge 2) "H1QualificationSignalsInsufficient:$id"
            foreach ($wiId in @($definition.wiIds)) { Require ($wiIds.ContainsKey([string] $wiId)) "H1WiUnknown:${id}:$wiId" }
            Require (@($definition.wiIds).Count -gt 0 -or @($definition.anticipatedGameplayCodes).Count -gt 0) "H1GameplayMissing:$id"
            foreach ($setRef in @($definition.grammarSetRefs)) {
                foreach ($variant in @("A", "B", "C")) { Require ($grammarKeys.ContainsKey("$setRef`:$variant")) "H1GrammarMissing:${id}:$setRef`:$variant" }
            }
            if ([string] $definition.knowledgeStateCode -eq "ApprovedReference") {
                Require ($approvedDefinitions.ContainsKey([string] $definition.approvedDefinitionStableId)) "H1ApprovedDefinitionMissing:$id"
            }
            if ([string] $definition.knowledgeStateCode -eq "CandidateForReview") {
                Require (@($definition.wiIds).Count -gt 0) "H1CandidateWiMissing:$id"
                Require (@($definition.capabilityCodes).Count -gt 0) "H1CandidateCapabilitiesMissing:$id"
                Require (@($definition.connectorRoleCodes).Count -gt 0) "H1CandidateConnectorsMissing:$id"
            }
            foreach ($pair in @(
                @{ Name = "wi"; Values = @($definition.wiIds) },
                @{ Name = "gameplay"; Values = @($definition.anticipatedGameplayCodes) },
                @{ Name = "role"; Values = @($definition.spatialRoleCodes) },
                @{ Name = "capability"; Values = @($definition.capabilityCodes) },
                @{ Name = "capacity"; Values = @($definition.capacityConceptCodes) },
                @{ Name = "predecessor"; Values = @($definition.predecessorH1Refs) },
                @{ Name = "successor"; Values = @($definition.successorH1Refs) },
                @{ Name = "connector"; Values = @($definition.connectorRoleCodes) },
                @{ Name = "grammar"; Values = @($definition.grammarSetRefs) })) {
                Test-DirectiveSet $markdown $pair.Name $pair.Values $id
            }
        }
        elseif ($level -eq "h2") {
            Require (@($definition.requiredH1Refs).Count -gt 0) "H2RequiredH1Missing:$id"
            Require ((@($definition.sizeVariantCodes) -join ",") -eq "Compact,Standard,Expanded") "H2SizeVariantsInvalid:$id"
            Test-DirectiveSet $markdown "required-h1" @($definition.requiredH1Refs) $id
            Test-DirectiveSet $markdown "optional-h1" @($definition.optionalH1Refs) $id
            Test-DirectiveSet $markdown "connector" @($definition.connectorRoleCodes) $id
        }
        else {
            Require (@($definition.requiredH2Refs).Count -gt 0) "H3RequiredH2Missing:$id"
            Require (@($definition.connectorRoleCodes).Count -gt 0) "H3ConnectorsMissing:$id"
            Test-DirectiveSet $markdown "required-h2" @($definition.requiredH2Refs) $id
            Test-DirectiveSet $markdown "optional-h2" @($definition.optionalH2Refs) $id
            Test-DirectiveSet $markdown "connector" @($definition.connectorRoleCodes) $id
        }
        $definitionsById[$id] = $definition
        $definitionsByLevel[$definition.hierarchyLevelCode] += $definition
    }
}

Require (@($definitionsByLevel.H1).Count -eq 53) "H1CountMustBe53"
Require (@($definitionsByLevel.H2).Count -eq 38) "H2CountMustBe38"
Require (@($definitionsByLevel.H3).Count -eq 20) "H3CountMustBe20"
foreach ($h1 in @($definitionsByLevel.H1)) {
    foreach ($reference in @($h1.predecessorH1Refs + $h1.successorH1Refs)) { Require ($definitionsById.ContainsKey([string] $reference)) "H1RelationUnknown:$($h1.stableId):$reference" }
}
foreach ($h2 in @($definitionsByLevel.H2)) {
    foreach ($reference in @($h2.requiredH1Refs + $h2.optionalH1Refs)) { Require ($definitionsById.ContainsKey([string] $reference) -and [string] $definitionsById[[string] $reference].hierarchyLevelCode -eq "H1") "H2ChildUnknown:$($h2.stableId):$reference" }
}
foreach ($h3 in @($definitionsByLevel.H3)) {
    foreach ($reference in @($h3.requiredH2Refs + $h3.optionalH2Refs)) { Require ($definitionsById.ContainsKey([string] $reference) -and [string] $definitionsById[[string] $reference].hierarchyLevelCode -eq "H2") "H3ChildUnknown:$($h3.stableId):$reference" }
}

$referenceLists = @{}
foreach ($level in @("H1", "H2", "H3")) {
    $referenceLists[$level] = @($definitionsByLevel[$level] | Sort-Object stableId | ForEach-Object {
        $slug = Get-Slug ([string] $_.stableId)
        $lower = $level.ToLowerInvariant()
        $definitionRelative = "definitions/$lower/$slug.v2.json"
        $documentRelative = [string] $_.authoredDocument
        [pscustomobject][ordered]@{
            stableId = [string] $_.stableId
            revision = [int] $_.revision
            definitionPath = $definitionRelative
            definitionSha256 = Get-TextSha256 ([IO.File]::ReadAllText((Join-Path $knowledgeRoot $definitionRelative)))
            documentPath = $documentRelative
            documentSha256 = Get-TextSha256 ([IO.File]::ReadAllText((Join-Path $knowledgeRoot $documentRelative)))
        }
    })
}

$catalog = [pscustomobject][ordered]@{
    schemaVersion = "simulation-world-spatial-design-knowledge-catalog.v2"
    revision = "simulation-world-spatial-design-knowledge.r2"
    title = "Codex 공간 설계 지식 저장소"
    summary = "게임 플레이와 Synty 표현 가능성에서 출발한 H1·H2·H3 항목별 Markdown+JSON 공간 설계 지식이다."
    legacyCatalogPath = $LegacyCatalogPath
    expansionSourcePath = $ExpansionPath
    h1DefinitionRefs = $referenceLists.H1
    h2DefinitionRefs = $referenceLists.H2
    h3DefinitionRefs = $referenceLists.H3
    stateCodes = $stateCodes
    promotionPolicy = [pscustomobject][ordered]@{
        requiresHumanReview = $true
        requiresExactChildRevision = $true
        requiresDeterministicHash = $true
        forbidsAutomaticAuthorityPromotion = $true
        forbidsAbsoluteCoordinates = $true
        forbidsUnityAssetAuthority = $true
    }
    presentationOnly = $true
    isOperationalState = $false
}

function New-IndexDocument([hashtable] $ByLevel) {
    $builder = [Text.StringBuilder]::new()
    [void] $builder.AppendLine("# Codex 공간 설계 지식 색인")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("> 항목별 JSON·Markdown에서 결정적으로 생성된다. 직접 수정하지 않는다.")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("- H1 작업공간 지식: ``53개``")
    [void] $builder.AppendLine("- H2 블록 조립법: ``24개``")
    [void] $builder.AppendLine("- H3 지역 유형 청사진: ``13개``")
    foreach ($level in @("H1", "H2", "H3")) {
        [void] $builder.AppendLine()
        [void] $builder.AppendLine("## $level")
        [void] $builder.AppendLine()
        [void] $builder.AppendLine("| 상태 | 고유 식별자 | 이름 | 검색 단서 |")
        [void] $builder.AppendLine("| --- | --- | --- | --- |")
        foreach ($item in @($ByLevel[$level] | Sort-Object knowledgeStateCode, stableId)) {
            $clues = if ($level -eq "H1") { @($item.wiIds + $item.anticipatedGameplayCodes + $item.capabilityCodes + $item.sourcePackCodes) -join ", " } elseif ($level -eq "H2") { @($item.requiredH1Refs + $item.connectorRoleCodes) -join ", " } else { @($item.requiredH2Refs + $item.connectorRoleCodes) -join ", " }
            [void] $builder.AppendLine("| ``$($item.knowledgeStateCode)`` | ``$($item.stableId)`` | $($item.title) | $($clues.Replace('|','\|')) |")
        }
    }
    return ConvertTo-StableText $builder.ToString()
}

function New-CompositionDocument([hashtable] $ByLevel) {
    $builder = [Text.StringBuilder]::new()
    [void] $builder.AppendLine("# 공간 설계 지식 조합표")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## H2 조립법")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| H2 | 위상 | 필수 H1 | 선택 H1 |")
    [void] $builder.AppendLine("| --- | --- | --- | --- |")
    foreach ($item in @($ByLevel.H2 | Sort-Object stableId)) { [void] $builder.AppendLine("| ``$($item.stableId)`` $($item.title) | ``$($item.topologyCode)`` | $(@($item.requiredH1Refs) -join ', ') | $(@($item.optionalH1Refs) -join ', ') |") }
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## H3 청사진")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| H3 | 위상 | 필수 H2 | 선택 H2 | 외부 연결 역할 |")
    [void] $builder.AppendLine("| --- | --- | --- | --- | --- |")
    foreach ($item in @($ByLevel.H3 | Sort-Object stableId)) { [void] $builder.AppendLine("| ``$($item.stableId)`` $($item.title) | ``$($item.topologyCode)`` | $(@($item.requiredH2Refs) -join ', ') | $(@($item.optionalH2Refs) -join ', ') | $(@($item.connectorRoleCodes) -join ', ') |") }
    return ConvertTo-StableText $builder.ToString()
}

function New-GapDocument([hashtable] $ByLevel) {
    $builder = [Text.StringBuilder]::new()
    [void] $builder.AppendLine("# 공간 설계 지식 공백 보고서")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| 계층 | 지식 | 현재 상태 | 다음 보완 |")
    [void] $builder.AppendLine("| --- | --- | --- | --- |")
    foreach ($level in @("H1", "H2", "H3")) {
        foreach ($item in @($ByLevel[$level] | Where-Object knowledgeStateCode -ne "ApprovedReference" | Sort-Object knowledgeStateCode, stableId)) {
            [void] $builder.AppendLine("| ``$level`` | ``$($item.stableId)`` $($item.title) | ``$($item.knowledgeStateCode)`` | $(@($item.unresolvedItems) -join '; ') |")
        }
    }
    return ConvertTo-StableText $builder.ToString()
}

function New-PromotionDocument([hashtable] $ByLevel, [hashtable] $ById) {
    $builder = [Text.StringBuilder]::new()
    [void] $builder.AppendLine("# 공간 설계 지식 승격 대기열")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("> 이 문서는 후보를 자동 승인하지 않고 다음 검토 순서와 차단 이유만 계산한다.")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| 우선 | 계층 | 지식 | 판정 |")
    [void] $builder.AppendLine("| ---: | --- | --- | --- |")
    $rows = @()
    foreach ($item in @($ByLevel.H1 | Where-Object knowledgeStateCode -ne "ApprovedReference")) {
        $score = 0
        if (@($item.wiIds).Count -gt 0) { $score += 40 }
        if (@($item.capabilityCodes).Count -gt 0) { $score += 25 }
        if (@($item.connectorRoleCodes).Count -gt 0) { $score += 20 }
        if (@($item.grammarSetRefs).Count -gt 0) { $score += 15 }
        $rows += [pscustomobject]@{ Score = $score; Level = "H1"; Id = $item.stableId; Title = $item.title; Result = if ($score -ge 80) { "사람 검토 우선 후보" } else { "설계 보완 필요" } }
    }
    foreach ($item in @($ByLevel.H2)) {
        $approvedChildren = @($item.requiredH1Refs | Where-Object { $ById.ContainsKey([string] $_) -and [string] $ById[[string] $_].knowledgeStateCode -eq "ApprovedReference" }).Count
        $score = if (@($item.requiredH1Refs).Count -eq 0) { 0 } else { [int] (100 * $approvedChildren / @($item.requiredH1Refs).Count) }
        $rows += [pscustomobject]@{ Score = $score; Level = "H2"; Id = $item.stableId; Title = $item.title; Result = "$approvedChildren/$(@($item.requiredH1Refs).Count) 필수 H1 승인 참조" }
    }
    foreach ($item in @($ByLevel.H3)) {
        $rows += [pscustomobject]@{ Score = 0; Level = "H3"; Id = $item.stableId; Title = $item.title; Result = "H2 설계 승인 전 조립 후보" }
    }
    $rank = 1
    foreach ($row in @($rows | Sort-Object @{ Expression = "Score"; Descending = $true }, Level, Id)) {
        [void] $builder.AppendLine("| $rank | ``$($row.Level)`` | ``$($row.Id)`` $($row.Title) | $($row.Result) |")
        $rank++
    }
    return ConvertTo-StableText $builder.ToString()
}

$catalogPath = Join-Path $knowledgeRoot "catalog.v2.json"
$generated = [ordered]@{
    "docs/AI/generated/spatial-knowledge-index.md" = New-IndexDocument $definitionsByLevel
    "docs/AI/generated/spatial-knowledge-composition-matrix.md" = New-CompositionDocument $definitionsByLevel
    "docs/AI/generated/spatial-knowledge-gap-report.md" = New-GapDocument $definitionsByLevel
    "docs/AI/generated/spatial-knowledge-promotion-queue.md" = New-PromotionDocument $definitionsByLevel $definitionsById
}

if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $catalogPath) "CatalogV2Missing"
    Require ((ConvertTo-StableText ([IO.File]::ReadAllText($catalogPath))) -ceq (ConvertTo-StableJson $catalog)) "CatalogV2OutOfDate"
    foreach ($pair in $generated.GetEnumerator()) {
        $path = Resolve-RepositoryPath $repositoryRoot $pair.Key
        Require (Test-Path -LiteralPath $path) "GeneratedDocumentMissing:$($pair.Key)"
        Require ((ConvertTo-StableText ([IO.File]::ReadAllText($path))) -ceq $pair.Value) "GeneratedDocumentOutOfDate:$($pair.Key)"
    }
    Write-Output "SpatialDesignKnowledgeValid:H1=53;H2=38;H3=20"
}
else {
    [void] (Write-TextIfChanged $catalogPath (ConvertTo-StableJson $catalog))
    foreach ($pair in $generated.GetEnumerator()) { [void] (Write-TextIfChanged (Resolve-RepositoryPath $repositoryRoot $pair.Key) $pair.Value) }
    Write-Output "SpatialDesignKnowledgeGenerated:H1=53;H2=38;H3=20"
}
