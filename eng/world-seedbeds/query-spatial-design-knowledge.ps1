[CmdletBinding()]
param(
    [string[]] $WiIds = @(),
    [string[]] $GameplayCodes = @(),
    [string[]] $CapabilityCodes = @(),
    [string[]] $PackCodes = @(),
    [string[]] $TopologyCodes = @(),
    [string[]] $GrammarRefs = @(),
    [string[]] $CardKinds = @(),
    [ValidateSet("Text", "Json")]
    [string] $Format = "Text",
    [ValidateRange(1, 50)]
    [int] $Limit = 10,
    [string] $KnowledgeRootPath = "eng/world-seedbeds/synty-bottom-up-inventory"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-RepositoryPath([string] $RepositoryRoot, [string] $RelativePath) {
    return Join-Path $RepositoryRoot ($RelativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}

function Read-Json([string] $Path) {
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Expand-RequestedValues([string[]] $Values) {
    return @(
        $Values |
            ForEach-Object { @(([string] $_) -split ",") } |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )
}

function Get-Matches([object[]] $Actual, [string[]] $Requested) {
    if ($Requested.Count -eq 0) { return @() }
    return @($Requested | Where-Object { @($Actual) -contains $_ } | Sort-Object -Unique)
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$knowledgeRoot = Resolve-RepositoryPath $repositoryRoot $KnowledgeRootPath
$WiIds = @(Expand-RequestedValues $WiIds)
$GameplayCodes = @(Expand-RequestedValues $GameplayCodes)
$CapabilityCodes = @(Expand-RequestedValues $CapabilityCodes)
$PackCodes = @(Expand-RequestedValues $PackCodes)
$TopologyCodes = @(Expand-RequestedValues $TopologyCodes)
$GrammarRefs = @(Expand-RequestedValues $GrammarRefs)
$CardKinds = @(Expand-RequestedValues $CardKinds)
$v3CatalogPath = Join-Path $knowledgeRoot "catalog.v3.json"
$isV3 = Test-Path -LiteralPath $v3CatalogPath
$catalogPath = if ($isV3) { $v3CatalogPath } else { Join-Path $knowledgeRoot "catalog.v2.json" }
$catalog = Read-Json $catalogPath
foreach ($cardKind in $CardKinds) {
    if (@("InteractionSpace", "PackExpression") -notcontains $cardKind) { throw "SpatialDesignKnowledgeQueryInvalid:CardKind:$cardKind" }
}
$byId = @{}
$byLevel = @{ H1 = @(); H2 = @(); H3 = @(); H4 = @() }
$expressionItems = @()
foreach ($level in @("H1", "H2", "H3", "H4")) {
    if (-not $isV3 -and $level -eq "H4") { continue }
    $property = if ($isV3 -and $level -eq "H1") { "h1InteractionDefinitionRefs" } else { "$($level.ToLowerInvariant())DefinitionRefs" }
    foreach ($reference in @($catalog.$property)) {
        $item = Read-Json (Join-Path $knowledgeRoot ([string] $reference.definitionPath))
        $byId[[string] $item.stableId] = $item
        $byLevel[$level] += $item
    }
}
if ($isV3) {
    foreach ($reference in @($catalog.h1ExpressionDefinitionRefs)) {
        $item = Read-Json (Join-Path $knowledgeRoot ([string] $reference.definitionPath))
        $byId[[string] $item.stableId] = $item
        $expressionItems += $item
    }
}

$h1Rows = @()
foreach ($item in @($byLevel.H1)) {
    if ($CardKinds.Count -gt 0 -and $CardKinds -notcontains "InteractionSpace") { continue }
    $wiMatches = @(Get-Matches @($item.wiIds) $WiIds)
    $gameplayMatches = @(Get-Matches @($item.anticipatedGameplayCodes) $GameplayCodes)
    $capabilityMatches = @(Get-Matches @($item.capabilityCodes) $CapabilityCodes)
    $packMatches = @(Get-Matches @($item.sourcePackCodes) $PackCodes)
    if ($CapabilityCodes.Count -gt 0 -and $capabilityMatches.Count -ne $CapabilityCodes.Count) { continue }
    if ($PackCodes.Count -gt 0 -and $packMatches.Count -eq 0) { continue }
    $score = ($wiMatches.Count * 50) + ($gameplayMatches.Count * 35) + ($capabilityMatches.Count * 20) + ($packMatches.Count * 10)
    if (($WiIds.Count + $GameplayCodes.Count + $CapabilityCodes.Count + $PackCodes.Count) -eq 0) { $score = 1 }
    if ($score -eq 0) { continue }
    $reasons = @()
    if ($wiMatches.Count -gt 0) { $reasons += "WI=" + ($wiMatches -join ",") }
    if ($gameplayMatches.Count -gt 0) { $reasons += "예상행동=" + ($gameplayMatches -join ",") }
    if ($capabilityMatches.Count -gt 0) { $reasons += "능력=" + ($capabilityMatches -join ",") }
    if ($packMatches.Count -gt 0) { $reasons += "팩=" + ($packMatches -join ",") }
    $h1Rows += [pscustomobject][ordered]@{
        stableId = [string] $item.stableId
        title = [string] $item.title
        stateCode = [string] $item.knowledgeStateCode
        score = $score
        reasons = $reasons
    }
}
$h1Rows = @($h1Rows | Sort-Object @{ Expression = "score"; Descending = $true }, stableId | Select-Object -First $Limit)
$selectedH1Ids = @($h1Rows | ForEach-Object { [string] $_.stableId })

$expressionRows = @()
foreach ($item in $expressionItems) {
    if ($CardKinds.Count -gt 0 -and $CardKinds -notcontains "PackExpression") { continue }
    $packMatches = @(Get-Matches @($item.sourcePackCode) $PackCodes)
    $grammarMatches = @(Get-Matches @(@($item.sourceGrammarSetRef) + @($item.grammarVariantRefs)) $GrammarRefs)
    $interactionMatches = @($item.supportsInteractionH1Refs | Where-Object { $selectedH1Ids -contains [string] $_ })
    if ($PackCodes.Count -gt 0 -and $packMatches.Count -eq 0) { continue }
    if ($GrammarRefs.Count -gt 0 -and $grammarMatches.Count -eq 0) { continue }
    $score = ($interactionMatches.Count * 30) + ($grammarMatches.Count * 40) + ($packMatches.Count * 20)
    if (($WiIds.Count + $GameplayCodes.Count + $CapabilityCodes.Count + $PackCodes.Count + $GrammarRefs.Count) -eq 0) { $score = 1 }
    if ($score -eq 0) { continue }
    $reasons = @()
    if ($interactionMatches.Count -gt 0) { $reasons += "행동H1=" + ($interactionMatches -join ",") }
    if ($grammarMatches.Count -gt 0) { $reasons += "문법=" + ($grammarMatches -join ",") }
    if ($packMatches.Count -gt 0) { $reasons += "팩=" + ($packMatches -join ",") }
    $expressionRows += [pscustomobject][ordered]@{
        stableId = [string] $item.stableId
        title = [string] $item.title
        stateCode = [string] $item.knowledgeStateCode
        sourcePackCode = [string] $item.sourcePackCode
        score = $score
        grammarVariantRefs = @($item.grammarVariantRefs)
        supportsInteractionH1Refs = @($item.supportsInteractionH1Refs)
        reasons = $reasons
    }
}
$expressionRows = @($expressionRows | Sort-Object @{ Expression = "score"; Descending = $true }, stableId | Select-Object -First $Limit)

$h2Rows = @()
foreach ($item in @($byLevel.H2)) {
    if ($TopologyCodes.Count -gt 0 -and $TopologyCodes -notcontains [string] $item.topologyCode) { continue }
    $requiredMatches = @($item.requiredH1Refs | Where-Object { $selectedH1Ids -contains [string] $_ })
    $optionalMatches = @($item.optionalH1Refs | Where-Object { $selectedH1Ids -contains [string] $_ })
    $score = ($requiredMatches.Count * 30) + ($optionalMatches.Count * 10)
    if ($TopologyCodes -contains [string] $item.topologyCode) { $score += 15 }
    if ($score -eq 0) { continue }
    $h2Rows += [pscustomobject][ordered]@{
        stableId = [string] $item.stableId
        title = [string] $item.title
        stateCode = [string] $item.knowledgeStateCode
        topologyCode = [string] $item.topologyCode
        score = $score
        matchedRequiredH1Refs = $requiredMatches
        missingRequiredH1Refs = @($item.requiredH1Refs | Where-Object { $selectedH1Ids -notcontains [string] $_ })
        matchedOptionalH1Refs = $optionalMatches
    }
}
$h2Rows = @($h2Rows | Sort-Object @{ Expression = "score"; Descending = $true }, stableId | Select-Object -First $Limit)
$selectedH2Ids = @($h2Rows | ForEach-Object { [string] $_.stableId })

$h3Rows = @()
foreach ($item in @($byLevel.H3)) {
    if ($TopologyCodes.Count -gt 0 -and $TopologyCodes -notcontains [string] $item.topologyCode) { continue }
    $requiredMatches = @($item.requiredH2Refs | Where-Object { $selectedH2Ids -contains [string] $_ })
    $optionalMatches = @($item.optionalH2Refs | Where-Object { $selectedH2Ids -contains [string] $_ })
    $score = ($requiredMatches.Count * 35) + ($optionalMatches.Count * 10)
    if ($TopologyCodes -contains [string] $item.topologyCode) { $score += 15 }
    if ($score -eq 0) { continue }
    $h3Rows += [pscustomobject][ordered]@{
        stableId = [string] $item.stableId
        title = [string] $item.title
        stateCode = [string] $item.knowledgeStateCode
        topologyCode = [string] $item.topologyCode
        score = $score
        matchedRequiredH2Refs = $requiredMatches
        missingRequiredH2Refs = @($item.requiredH2Refs | Where-Object { $selectedH2Ids -notcontains [string] $_ })
        connectorRoleCodes = @($item.connectorRoleCodes)
    }
}
$h3Rows = @($h3Rows | Sort-Object @{ Expression = "score"; Descending = $true }, stableId | Select-Object -First $Limit)
$selectedH3Ids = @($h3Rows | ForEach-Object { [string] $_.stableId })

$h4Rows = @()
foreach ($item in @($byLevel.H4)) {
    $requiredMatches = @($item.requiredH3Refs | Where-Object { $selectedH3Ids -contains [string] $_ })
    $optionalMatches = @($item.optionalH3Refs | Where-Object { $selectedH3Ids -contains [string] $_ })
    $score = ($requiredMatches.Count * 40) + ($optionalMatches.Count * 10)
    if ($score -eq 0) { continue }
    $h4Rows += [pscustomobject][ordered]@{
        stableId = [string] $item.stableId
        title = [string] $item.title
        stateCode = [string] $item.knowledgeStateCode
        score = $score
        matchedRequiredH3Refs = $requiredMatches
        missingRequiredH3Refs = @($item.requiredH3Refs | Where-Object { $selectedH3Ids -notcontains [string] $_ })
        matchedOptionalH3Refs = $optionalMatches
        requiredEvidencePurposeCodes = @($item.requiredEvidencePurposeCodes)
    }
}
$h4Rows = @($h4Rows | Sort-Object @{ Expression = "score"; Descending = $true }, stableId | Select-Object -First $Limit)

$matchedWiIds = @($h1Rows | ForEach-Object { $byId[[string] $_.stableId].wiIds } | Sort-Object -Unique)
$matchedCapabilities = @($h1Rows | ForEach-Object { $byId[[string] $_.stableId].capabilityCodes } | Sort-Object -Unique)
$result = [pscustomobject][ordered]@{
    schemaVersion = "simulation-world-spatial-design-knowledge-query.v2"
    query = [pscustomobject][ordered]@{
        wiIds = $WiIds
        gameplayCodes = $GameplayCodes
        capabilityCodes = $CapabilityCodes
        packCodes = $PackCodes
        topologyCodes = $TopologyCodes
        grammarRefs = $GrammarRefs
        cardKinds = $CardKinds
    }
    h1Recommendations = $h1Rows
    h1ExpressionRecommendations = $expressionRows
    h2Recommendations = $h2Rows
    h3Recommendations = $h3Rows
    h4Recommendations = $h4Rows
    gaps = [pscustomobject][ordered]@{
        unmatchedWiIds = @($WiIds | Where-Object { $matchedWiIds -notcontains $_ })
        unmatchedCapabilityCodes = @($CapabilityCodes | Where-Object { $matchedCapabilities -notcontains $_ })
    }
    authorityBoundary = "추천 결과는 설계 후보이며 H 정의·E 증거·AreaSet·LandscapeGraph를 자동 승인하지 않는다."
}

if ($Format -eq "Json") {
    $result | ConvertTo-Json -Depth 20
    exit 0
}

Write-Output "공간설계지식검색:행동H1=$($h1Rows.Count);표현H1=$($expressionRows.Count);H2=$($h2Rows.Count);H3=$($h3Rows.Count);H4=$($h4Rows.Count)"
foreach ($row in $h1Rows) { Write-Output "H1-행동|$($row.score)|$($row.stableId)|$($row.title)|$(@($row.reasons) -join ';')" }
foreach ($row in $expressionRows) { Write-Output "H1-표현|$($row.score)|$($row.stableId)|$($row.title)|$(@($row.reasons) -join ';')" }
foreach ($row in $h2Rows) { Write-Output "H2|$($row.score)|$($row.stableId)|$($row.title)|미충족=$(@($row.missingRequiredH1Refs) -join ',')" }
foreach ($row in $h3Rows) { Write-Output "H3|$($row.score)|$($row.stableId)|$($row.title)|미충족=$(@($row.missingRequiredH2Refs) -join ',')" }
foreach ($row in $h4Rows) { Write-Output "H4|$($row.score)|$($row.stableId)|$($row.title)|미충족=$(@($row.missingRequiredH3Refs) -join ',')" }
if (@($result.gaps.unmatchedWiIds).Count -gt 0) { Write-Output "부족WI:$(@($result.gaps.unmatchedWiIds) -join ',')" }
if (@($result.gaps.unmatchedCapabilityCodes).Count -gt 0) { Write-Output "부족능력:$(@($result.gaps.unmatchedCapabilityCodes) -join ',')" }
Write-Output $result.authorityBoundary
