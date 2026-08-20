# UTF-8 BOM is intentional: Windows PowerShell 5.1 must decode the Korean
# presentation text in this generator before execution.
[CmdletBinding()]
param(
    [ValidateSet("Check", "Write")]
    [string] $Mode = "Check",
    [string] $PolicyPath = "eng/world-seedbeds/actual-e5-spatial-policy.v1.json",
    [string] $TheoryPath = "eng/world-seedbeds/generated/theory-spatial-factory.v1.json",
    [string] $JsonOutputPath = "eng/world-seedbeds/generated/actual-e5-spatial.v1.json",
    [string] $MarkdownOutputPath = "docs/AI/generated/actual-e5-spatial.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path

function Resolve-RepoPath([string] $relativePath) {
    return Join-Path $repositoryRoot ($relativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}

function Read-Json([string] $relativePath) {
    $path = Resolve-RepoPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) { throw "JsonMissing:$relativePath" }
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Require([bool] $condition, [string] $code) {
    if (-not $condition) { throw "ActualE5SpatialInvalid:$code" }
}

function Normalize([string] $value) { return (($value -replace "`r`n", "`n").TrimEnd()) + "`n" }
function Stable-Json([object] $value) { return (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n") }

function Text-Hash([string] $value) {
    $bytes = [Text.Encoding]::UTF8.GetBytes($value)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha256.ComputeHash($bytes)
        return (($hashBytes | ForEach-Object { $_.ToString("x2") }) -join "")
    }
    finally {
        $sha256.Dispose()
    }
}

function Slug([string] $stableId) {
    $value = $stableId.Substring($stableId.LastIndexOf(':') + 1)
    return ($value -replace '[^a-zA-Z0-9-]', '-')
}

function Graph-Id([string] $h3Ref) {
    return "landscape-graph:sim:pyeongchang:" + (Slug $h3Ref) + ".v1"
}

function Stub-Id([string] $h3Ref, [string] $side) {
    return "stub:actual-e5:" + (Slug $h3Ref) + ":" + $side
}

function Load-H1Definitions([object] $catalog) {
    $result = @{}
    foreach ($ref in @($catalog.h1InteractionDefinitionRefs)) {
        $definition = Read-Json ("eng/world-seedbeds/synty-bottom-up-inventory/" + [string] $ref.definitionPath)
        $result[[string] $definition.stableId] = $definition
    }
    return $result
}

function Values([object] $value, [string] $propertyName) {
    if ($null -eq $value -or $null -eq $value.PSObject.Properties[$propertyName]) { return @() }
    return @($value.$propertyName)
}

$policy = Read-Json $PolicyPath
$theory = Read-Json $TheoryPath
$worldInteractions = Read-Json "eng/execution-ledgers/world-interactions.json"
$spatialPriorities = Read-Json "eng/world-seedbeds/wi-spatial-priorities.v1.json"
$designCatalog = Read-Json "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json"
$h1Definitions = Load-H1Definitions $designCatalog

Require ([string] $policy.schemaVersion -eq "simulation-world-actual-e5-spatial-policy.v1") "PolicySchema"
Require ([string] $theory.schemaVersion -eq "simulation-world-theory-spatial-factory-output.v1") "TheorySchema"
Require ([bool] $policy.presentationOnly -and -not [bool] $policy.isOperationalState) "AuthorityBoundary"
Require (@($policy.areaSets).Count -eq 4) "AreaSetCount"
Require (@($policy.networkRouteH3Refs).Count -eq 3) "NetworkRouteGraphCount"
Require (@($policy.networkRelations).Count -eq 8) "NetworkRelationCount"

$h2ById = @{}; foreach ($h2 in @($theory.h2Plans)) { $h2ById[[string] $h2.h2StableId] = $h2 }
$h3ById = @{}; foreach ($h3 in @($theory.h3Plans)) { $h3ById[[string] $h3.h3StableId] = $h3 }
$areaPolicyByTheory = @{}; foreach ($area in @($policy.areaSets)) { $areaPolicyByTheory[[string] $area.theoryAreaSetStableId] = $area }
$areaByH3 = @{}
foreach ($instance in @($theory.e5AreaSetInstances)) {
    Require ($areaPolicyByTheory.ContainsKey([string] $instance.areaSetStableId)) "AreaPolicyMissing:$($instance.areaSetStableId)"
    foreach ($graph in @($instance.graphInstances)) {
        $areaByH3[[string] $graph.h3Ref] = $areaPolicyByTheory[[string] $instance.areaSetStableId]
    }
}
foreach ($routeRef in @($policy.networkRouteH3Refs)) {
    Require ($h3ById.ContainsKey([string] $routeRef)) "RouteH3Unknown:$routeRef"
    Require (-not $areaByH3.ContainsKey([string] $routeRef)) "RouteH3AreaOwned:$routeRef"
}
$promotedH3Refs = @{}
foreach ($areaH3Ref in @($areaByH3.Keys)) { $promotedH3Refs[[string] $areaH3Ref] = $true }
foreach ($routeH3Ref in @($policy.networkRouteH3Refs)) {
    Require (-not $promotedH3Refs.ContainsKey([string] $routeH3Ref)) "PromotedH3Duplicate:$routeH3Ref"
    $promotedH3Refs[[string] $routeH3Ref] = $true
}
$deferredH3Refs = @($policy.deferredTheoryH3Refs | Sort-Object -Unique)
foreach ($deferredH3Ref in $deferredH3Refs) {
    Require ($h3ById.ContainsKey([string] $deferredH3Ref)) "DeferredH3Unknown:$deferredH3Ref"
    Require (-not $promotedH3Refs.ContainsKey([string] $deferredH3Ref)) "DeferredH3Promoted:$deferredH3Ref"
}
foreach ($theoryH3Ref in @($h3ById.Keys)) {
    Require ($promotedH3Refs.ContainsKey([string] $theoryH3Ref) -or
        $deferredH3Refs -contains [string] $theoryH3Ref) "TheoryH3Unclassified:$theoryH3Ref"
}

$graphMetadata = @{}
$graphs = @()
foreach ($h3 in @($theory.h3Plans | Where-Object {
        $promotedH3Refs.ContainsKey([string] $_.h3StableId)
    } | Sort-Object h3StableId)) {
    $h3Ref = [string] $h3.h3StableId
    $graphId = Graph-Id $h3Ref
    $graphSlug = Slug $h3Ref
    $networkOwned = @($policy.networkRouteH3Refs) -contains $h3Ref
    $areaPolicy = if ($networkOwned) { $null } else { $areaByH3[$h3Ref] }
    Require ($networkOwned -or $null -ne $areaPolicy) "GraphOwnerMissing:$h3Ref"
    $ownerKind = if ($networkOwned) { "AreaSetNetwork" } else { "AreaSet" }
    $ownerStableId = if ($networkOwned) { [string] $policy.networkStableId } else { [string] $areaPolicy.actualAreaSetStableId }
    $areaRef = if ($networkOwned) { "" } else { "area:sim:pyeongchang:" + ([string] $areaPolicy.areaRoleCode).ToLowerInvariant() }

    $nodes = @()
    $edges = @()
    $placements = @()
    $nodeMetadata = @()
    $blockNodes = @()
    foreach ($h3Node in @($h3.nodes)) {
        $h2 = $h2ById[[string] $h3Node.h2Ref]
        Require ($null -ne $h2) "H2Missing:$($h3Node.h2Ref)"
        $h2Slug = Slug ([string] $h2.h2StableId)
        $blockNodeId = "node:actual-e5:${graphSlug}:block:${h2Slug}"
        $blockNodes += $blockNodeId
        $nodes += [ordered]@{
            nodeStableId = $blockNodeId
            parentNodeStableId = ""
            nodeKindCode = "area"
            semanticCode = "landscape-block:${h2Slug}"
            evidenceKindCode = "Scenario"
            centerEastingMeters = [double] $h3Node.x
            centerNorthingMeters = [double] $h3Node.z
            widthMeters = 360.0
            depthMeters = 280.0
        }

        $h1Nodes = @()
        foreach ($h1Node in @($h2.nodes)) {
            $h1Ref = [string] $h1Node.h1Ref
            $h1Slug = Slug $h1Ref
            $nodeId = "node:actual-e5:${graphSlug}:space:${h2Slug}:${h1Slug}"
            $x = [Math]::Round([double] $h3Node.x + [double] $h1Node.x, 2)
            $z = [Math]::Round([double] $h3Node.z + [double] $h1Node.z, 2)
            $definition = $h1Definitions[$h1Ref]
            Require ($null -ne $definition) "H1DefinitionMissing:$h1Ref"
            $grammarRefs = @(Values $definition "grammarSetRefs")
            Require ($grammarRefs.Count -gt 0) "H1GrammarMissing:$h1Ref"
            $compositionKey = [string] $grammarRefs[0] + ":A"
            $nodes += [ordered]@{
                nodeStableId = $nodeId
                parentNodeStableId = $blockNodeId
                nodeKindCode = "area"
                semanticCode = $h1Slug
                evidenceKindCode = "Scenario"
                centerEastingMeters = $x
                centerNorthingMeters = $z
                widthMeters = 36.0
                depthMeters = 28.0
            }
            $placementId = "placement:actual-e5:${graphSlug}:${h2Slug}:${h1Slug}"
            $seedText = "$graphId|$nodeId|$compositionKey"
            $seedHex = Text-Hash $seedText
            $seed = [Convert]::ToInt32($seedHex.Substring(0, 7), 16)
            $placements += [ordered]@{
                placementStableId = $placementId
                nodeStableId = $nodeId
                ownerTileKey = "scenario-local:${graphSlug}"
                compositionKey = $compositionKey
                topologyCode = "area"
                evidenceKindCode = "Scenario"
                eastingMeters = $x
                northingMeters = $z
                physicalElevationMeters = 0.0
                rotationDegrees = 0.0
                mirrored = $false
                deterministicSeed = $seed
                footprintWidthMeters = 30.0
                footprintDepthMeters = 24.0
                presentationOnly = $true
            }
            $h1Nodes += $nodeId
            $edges += [ordered]@{
                edgeStableId = "edge:actual-e5:${graphSlug}:contains:${h2Slug}:${h1Slug}"
                fromNodeStableId = $blockNodeId
                relationCode = "contains"
                toNodeStableId = $nodeId
                connectorTypeCode = "internal"
                evidenceKindCode = "Scenario"
            }
            $nodeMetadata += [pscustomobject]@{
                NodeStableId = $nodeId
                PlacementStableId = $placementId
                H1Ref = $h1Ref
                H2Ref = [string] $h2.h2StableId
                H3Ref = $h3Ref
                WiIds = @($h1Node.wiIds)
                SpatialRoleCodes = @($h1Node.spatialRoleCodes)
            }
        }
        foreach ($h2Edge in @($h2.edges)) {
            $fromLocal = @($h2.nodes | Where-Object nodeId -eq ([string] $h2Edge.fromNodeId))[0]
            $toLocal = @($h2.nodes | Where-Object nodeId -eq ([string] $h2Edge.toNodeId))[0]
            if ($null -eq $fromLocal -or $null -eq $toLocal) { continue }
            $fromId = "node:actual-e5:${graphSlug}:space:${h2Slug}:" + (Slug ([string] $fromLocal.h1Ref))
            $toId = "node:actual-e5:${graphSlug}:space:${h2Slug}:" + (Slug ([string] $toLocal.h1Ref))
            $edges += [ordered]@{
                edgeStableId = "edge:actual-e5:${graphSlug}:h2:${h2Slug}:" + (Slug ([string] $h2Edge.edgeId))
                fromNodeStableId = $fromId
                relationCode = "connects"
                toNodeStableId = $toId
                connectorTypeCode = "work-route"
                evidenceKindCode = "Scenario"
            }
        }
    }

    for ($index = 0; $index -lt @($h3.edges).Count; $index++) {
        $h3Edge = @($h3.edges)[$index]
        $fromIndex = [Array]::IndexOf(@($h3.nodes.nodeId), [string] $h3Edge.fromNodeId)
        $toIndex = [Array]::IndexOf(@($h3.nodes.nodeId), [string] $h3Edge.toNodeId)
        Require ($fromIndex -ge 0 -and $toIndex -ge 0) "H3EdgeNodeMissing:$h3Ref"
        $edges += [ordered]@{
            edgeStableId = "edge:actual-e5:${graphSlug}:h3:$($index + 1)"
            fromNodeStableId = $blockNodes[$fromIndex]
            relationCode = "transitions-to"
            toNodeStableId = $blockNodes[$toIndex]
            connectorTypeCode = "area-route"
            evidenceKindCode = "Scenario"
        }
    }

    Require ($nodeMetadata.Count -gt 0) "GraphH1NodesMissing:$h3Ref"
    $first = @($nodeMetadata | Sort-Object NodeStableId)[0]
    $last = @($nodeMetadata | Sort-Object NodeStableId)[-1]
    $connectorType = if ($networkOwned) { "cargo" } else { "player-work" }
    $stubs = @(
        [ordered]@{
            stubStableId = Stub-Id $h3Ref "ingress"
            placementStableId = [string] $first.PlacementStableId
            neighborTileKey = "area-set-network"
            connectorTypeCode = $connectorType
            routeSignature = "actual-e5.$graphSlug"
            directionCode = "Ingress"
            evidenceKindCode = "Scenario"
            worldEastingMeters = [double] (@($nodes | Where-Object nodeStableId -eq $first.NodeStableId)[0].centerEastingMeters)
            worldNorthingMeters = [double] (@($nodes | Where-Object nodeStableId -eq $first.NodeStableId)[0].centerNorthingMeters)
            widthMeters = 6.0
        },
        [ordered]@{
            stubStableId = Stub-Id $h3Ref "egress"
            placementStableId = [string] $last.PlacementStableId
            neighborTileKey = "area-set-network"
            connectorTypeCode = $connectorType
            routeSignature = "actual-e5.$graphSlug"
            directionCode = "Egress"
            evidenceKindCode = "Scenario"
            worldEastingMeters = [double] (@($nodes | Where-Object nodeStableId -eq $last.NodeStableId)[0].centerEastingMeters)
            worldNorthingMeters = [double] (@($nodes | Where-Object nodeStableId -eq $last.NodeStableId)[0].centerNorthingMeters)
            widthMeters = 6.0
        }
    )
    $xs = @($nodes.centerEastingMeters); $zs = @($nodes.centerNorthingMeters)
    $core = [ordered]@{
        landscapeGraphStableId = $graphId
        h3Ref = $h3Ref
        h3TheoryHashSha256 = [string] $h3.theoryHashSha256
        spatialOwnerKindCode = $ownerKind
        spatialOwnerStableId = $ownerStableId
        nodes = $nodes
        edges = $edges
        placements = $placements
        externalConnectorStubs = $stubs
    }
    $graphHash = Text-Hash (Stable-Json $core)
    $graphAreaRefs = [object[]] @()
    if (-not [string]::IsNullOrWhiteSpace($areaRef)) { $graphAreaRefs = ,$areaRef }
    $graph = [ordered]@{
        schemaVersion = "simulation-world-landscape-graph.v2"
        areaSetStableId = if ($networkOwned) { "" } else { [string] $areaPolicy.actualAreaSetStableId }
        landscapeGraphStableId = $graphId
        graphBuildStableId = "graph-build:actual-e5:${graphSlug}:" + $graphHash.Substring(0, 12)
        graphRoleCode = if ($networkOwned) { "NetworkRoute" } else { [string] $areaPolicy.areaRoleCode }
        graphRevision = 1
        definitionHashSha256 = [string] $h3.theoryHashSha256
        graphHashSha256 = $graphHash
        spatialOwnerKindCode = $ownerKind
        spatialOwnerStableId = $ownerStableId
        coordinateSpaceCode = [string] $policy.coordinateSpaceCode
        grammarRevision = "actual-e5-authored-scenario.r1"
        grammarHashSha256 = [string] $h3.theoryHashSha256
        statusCode = "Available"
        bounds = [ordered]@{
            minEastingMeters = ([double] ($xs | Measure-Object -Minimum).Minimum) - 24.0
            minNorthingMeters = ([double] ($zs | Measure-Object -Minimum).Minimum) - 24.0
            maxEastingMeters = ([double] ($xs | Measure-Object -Maximum).Maximum) + 24.0
            maxNorthingMeters = ([double] ($zs | Measure-Object -Maximum).Maximum) + 24.0
        }
        areaRefs = $graphAreaRefs
        tileRefs = @("scenario-local:${graphSlug}")
        scenarioRouteRefs = @("scenario-route:actual-e5:${graphSlug}.v1")
        nodes = $nodes
        edges = $edges
        placements = $placements
        externalConnectorStubs = $stubs
        unresolved = @()
        presentationOnly = $true
        isOperationalState = $false
    }
    $graphs += $graph
    $graphMetadata[$h3Ref] = [pscustomobject]@{
        Graph = $graph
        Nodes = $nodeMetadata
        AreaPolicy = $areaPolicy
        NetworkOwned = $networkOwned
    }
}

function Descriptor([object] $graph) {
    return [ordered]@{
        landscapeGraphStableId = [string] $graph.landscapeGraphStableId
        graphRoleCode = [string] $graph.graphRoleCode
        graphRevision = [int] $graph.graphRevision
        definitionHashSha256 = [string] $graph.definitionHashSha256
        buildStatusCode = [string] $graph.statusCode
        graphHashSha256 = [string] $graph.graphHashSha256
        spatialOwnerKindCode = [string] $graph.spatialOwnerKindCode
        spatialOwnerStableId = [string] $graph.spatialOwnerStableId
        coordinateSpaceCode = [string] $graph.coordinateSpaceCode
        bounds = $graph.bounds
        areaRefs = @($graph.areaRefs)
        tileRefs = @($graph.tileRefs)
        scenarioRouteRefs = @($graph.scenarioRouteRefs)
    }
}

$areaSets = @()
foreach ($areaPolicy in @($policy.areaSets)) {
    $theoryArea = @($theory.e5AreaSetInstances | Where-Object areaSetStableId -eq ([string] $areaPolicy.theoryAreaSetStableId))[0]
    Require ($null -ne $theoryArea) "TheoryAreaMissing:$($areaPolicy.theoryAreaSetStableId)"
    $areaGraphs = @($theoryArea.graphInstances | ForEach-Object { $graphMetadata[[string] $_.h3Ref].Graph })
    $relations = @()
    for ($index = 1; $index -lt $areaGraphs.Count; $index++) {
        $fromH3 = [string] $theoryArea.graphInstances[$index - 1].h3Ref
        $toH3 = [string] $theoryArea.graphInstances[$index].h3Ref
        $relations += [ordered]@{
            relationStableId = "graph-relation:actual-e5:" + ([string] $areaPolicy.areaRoleCode).ToLowerInvariant() + ":$index"
            fromGraphStableId = Graph-Id $fromH3
            toGraphStableId = Graph-Id $toH3
            relationCode = "Transition"
            connectorPair = [ordered]@{
                fromConnectorStableId = Stub-Id $fromH3 "egress"
                toConnectorStableId = Stub-Id $toH3 "ingress"
                connectorTypeCode = "player-work"
                routeSignature = "actual-e5.area-transition"
            }
        }
    }
    $areaCore = [ordered]@{
        areaSetStableId = [string] $areaPolicy.actualAreaSetStableId
        theoryAreaSetStableId = [string] $areaPolicy.theoryAreaSetStableId
        theoryHashSha256 = [string] $theoryArea.theoryHashSha256
        graphIds = @($areaGraphs.landscapeGraphStableId)
        graphRelations = $relations
    }
    $areaHash = Text-Hash (Stable-Json $areaCore)
    $areaRef = "area:sim:pyeongchang:" + ([string] $areaPolicy.areaRoleCode).ToLowerInvariant()
    $definition = [ordered]@{
        schemaVersion = "simulation-world-area-set.v1"
        areaSetStableId = [string] $areaPolicy.actualAreaSetStableId
        revision = 1
        title = [string] $theoryArea.title
        summary = "승인된 H1~H4를 작성 Scenario 지역 Graph에 결속한 실제 E5 공간이다."
        definitionHashSha256 = $areaHash
        documentHashSha256 = [string] $theoryArea.theoryHashSha256
        canonicalNetworkStableId = [string] $policy.networkStableId
        coordinateSpaceCode = [string] $policy.coordinateSpaceCode
        areaRefs = @($areaRef)
        scenarioRouteRefs = @($areaGraphs.scenarioRouteRefs | Sort-Object -Unique)
        completionAreaRefs = @()
        landscapeGraphs = @($areaGraphs | ForEach-Object { Descriptor $_ })
        graphRelations = $relations
        definitionStatusCode = "Available"
        presentationOnly = $true
        isOperationalState = $false
    }
    $areaSets += [ordered]@{
        theoryAreaSetStableId = [string] $areaPolicy.theoryAreaSetStableId
        areaRoleCode = [string] $areaPolicy.areaRoleCode
        loadPolicyCode = [string] $areaPolicy.loadPolicyCode
        defaultEntryConnectorStableId = Stub-Id ([string] $theoryArea.graphInstances[0].h3Ref) "ingress"
        definition = $definition
        graphs = $areaGraphs
    }
}

$areaByGraphRef = @{}
foreach ($area in $areaSets) {
    foreach ($instance in @($theory.e5AreaSetInstances | Where-Object areaSetStableId -eq $area.theoryAreaSetStableId).graphInstances) {
        $areaByGraphRef[[string] $instance.h3Ref] = $area
    }
}

$networkRelations = @()
foreach ($relation in @($policy.networkRelations)) {
    $fromArea = $areaByGraphRef[[string] $relation.fromH3Ref]
    $toArea = $areaByGraphRef[[string] $relation.toH3Ref]
    Require ($null -ne $fromArea -and $null -ne $toArea) "NetworkRelationAreaMissing:$($relation.relationCode)"
    $routeGraphId = if ([string]::IsNullOrWhiteSpace([string] $relation.routeH3Ref)) { "" } else { Graph-Id ([string] $relation.routeH3Ref) }
    $networkRelations += [ordered]@{
        relationStableId = "area-set-relation:actual-e5:" + ([string] $relation.relationCode).ToLowerInvariant()
        fromAreaSetStableId = [string] $fromArea.definition.areaSetStableId
        fromConnectorStableId = Stub-Id ([string] $relation.fromH3Ref) "egress"
        toAreaSetStableId = [string] $toArea.definition.areaSetStableId
        toConnectorStableId = Stub-Id ([string] $relation.toH3Ref) "ingress"
        relationKindCode = [string] $relation.relationKindCode
        directionCode = "OneWay"
        routeGraphStableId = $routeGraphId
        routeSignature = "actual-e5." + ([string] $relation.relationCode).ToLowerInvariant()
        sourceStableIds = @([string] $policy.approvalSourceStableId, "theory-relation:" + [string] $relation.relationCode)
    }
}

$networkAreaDescriptors = @($areaSets | ForEach-Object {
    [ordered]@{
        areaSetStableId = [string] $_.definition.areaSetStableId
        areaRoleCode = [string] $_.areaRoleCode
        loadPolicyCode = [string] $_.loadPolicyCode
        defaultEntryConnectorStableId = [string] $_.defaultEntryConnectorStableId
        areaSetRevision = [int] $_.definition.revision
        definitionHashSha256 = [string] $_.definition.definitionHashSha256
    }
})
$routeGraphs = @($policy.networkRouteH3Refs | ForEach-Object { $graphMetadata[[string] $_].Graph })
$networkCore = [ordered]@{
    networkStableId = [string] $policy.networkStableId
    areaSets = $networkAreaDescriptors
    routeGraphIds = @($routeGraphs.landscapeGraphStableId)
    relations = $networkRelations
}
$networkHash = Text-Hash (Stable-Json $networkCore)
$network = [ordered]@{
    schemaVersion = "simulation-world-area-set-network.v1"
    networkStableId = [string] $policy.networkStableId
    revision = 1
    title = "평창 Nature–Farm–Hub–Town 게임 플레이 Network"
    summary = "Nature 생활 거점과 Farm·City/Hub·Town 업무 영역을 플레이어 이동과 화물 물류로 연결한 실제 E5 Network다."
    coordinateSpaceCode = [string] $policy.coordinateSpaceCode
    evidenceStageCode = "ActualE5"
    definitionHashSha256 = $networkHash
    documentHashSha256 = Text-Hash (Stable-Json $policy)
    definitionStatusCode = "Available"
    areaSets = $networkAreaDescriptors
    routeGraphs = @($routeGraphs | ForEach-Object { Descriptor $_ })
    relations = $networkRelations
    presentationOnly = $true
    isOperationalState = $false
}

$worldById = @{}; foreach ($wi in @($worldInteractions.items)) { $worldById[[string] $wi.id] = $wi }
$bindings = @()
foreach ($preference in @($policy.wiBindingPreferences.PSObject.Properties | Sort-Object Name)) {
    $wiId = [string] $preference.Name
    $h3Ref = [string] $preference.Value
    Require ($worldById.ContainsKey($wiId)) "BindingWiUnknown:$wiId"
    Require ($graphMetadata.ContainsKey($h3Ref)) "BindingGraphUnknown:$wiId"
    $metadata = $graphMetadata[$h3Ref]
    $candidates = @($metadata.Nodes | Where-Object { @($_.WiIds) -contains $wiId } | Sort-Object NodeStableId)
    Require ($candidates.Count -gt 0) "BindingH1NodeMissing:$wiId"
    $selected = $candidates[0]
    $graph = $metadata.Graph
    $bindingAreaSetId = if ([string]::IsNullOrWhiteSpace([string] $graph.areaSetStableId)) {
        if ($wiId -eq "WI-LOG-03") { "area-set:sim:pyeongchang:farm-production.v1" }
        elseif ($wiId -eq "WI-MARKET-01") { "area-set:sim:pyeongchang:logistics-hub.v1" }
        else { [string] $policy.networkStableId }
    } else { [string] $graph.areaSetStableId }
    $requirements = @($worldById[$wiId].spatialRequirements | ForEach-Object {
        if ([string] $_ -like "Spatial.*") { [string] $_ } else { "Spatial." + [string] $_ }
    } | Sort-Object -Unique)
    $capacities = [object[]] @()
    if (@($requirements | Where-Object { $_ -like "*WorkArea" -or $_ -like "*Area" }).Count -gt 0) {
        $capacities = ,([ordered]@{
            capacityCode = "WorkArea"
            quantity = 1
            unitCode = "slot"
            evidenceKindCode = "Scenario"
            evidenceReference = "actual-e5-spatial-policy.r1"
            capacityRuleRevision = "actual-e5-work-area-capacity.r1"
        })
    }
    $bindings += [ordered]@{
        bindingStableId = "binding:actual-e5:" + $wiId.ToLowerInvariant()
        worldInteractionId = $wiId
        participationCode = "Required"
        areaSetStableId = $bindingAreaSetId
        spatialOwnerKindCode = [string] $graph.spatialOwnerKindCode
        spatialOwnerStableId = [string] $graph.spatialOwnerStableId
        landscapeGraphStableId = [string] $graph.landscapeGraphStableId
        requiredGraphRevision = [int] $graph.graphRevision
        requiredGraphHashSha256 = [string] $graph.graphHashSha256
        requiredNodeSemanticCode = Slug ([string] $selected.H1Ref)
        spatialRoleCode = if (@($selected.SpatialRoleCodes).Count -gt 0) { [string] @($selected.SpatialRoleCodes)[0] } else { "DirectActionSpace" }
        spatialStableId = "spatial:actual-e5:" + $wiId.ToLowerInvariant()
        facilityStableId = "facility:actual-e5:" + ([string] $bindingAreaSetId).Split(':')[-1]
        areaStableId = "area:actual-e5:" + ([string] $bindingAreaSetId).Split(':')[-1]
        capabilityCodes = $requirements
        baseCapacities = $capacities
        reviewStatusCode = "ApprovedForSimulation"
        h1Ref = [string] $selected.H1Ref
        h2Ref = [string] $selected.H2Ref
        h3Ref = [string] $selected.H3Ref
        sourceStableIds = @([string] $policy.approvalSourceStableId, [string] $selected.H1Ref, [string] $selected.H2Ref, [string] $selected.H3Ref)
    }
}

$contextAreaMap = [ordered]@{
    "WI-HUB-03" = "area-set:sim:pyeongchang:logistics-hub.v1"
    "WI-ORDER-07" = "area-set:sim:pyeongchang:town-market.v1"
    "WI-WORLD-01" = "area-set:sim:pyeongchang:farm-production.v1"
    "WI-WORLD-05" = [string] $policy.networkStableId
    "WI-WORLD-07" = [string] $policy.networkStableId
}
$contextBindings = @($contextAreaMap.GetEnumerator() | Sort-Object Key | ForEach-Object {
    [ordered]@{
        worldInteractionId = [string] $_.Key
        participationCode = "Contextual"
        contextStableId = [string] $_.Value
        contextBindingStateCode = "AreaSetContextBound"
        sourceStableIds = @([string] $policy.approvalSourceStableId)
    }
})
$nonSpatialWiIds = @($spatialPriorities.notRequiredWiIds | Sort-Object)
Require ($bindings.Count -eq 30) "DirectBindingCount:$($bindings.Count)"
Require ($contextBindings.Count -eq 5) "ContextBindingCount:$($contextBindings.Count)"
Require ($nonSpatialWiIds.Count -eq 6) "NonSpatialCount:$($nonSpatialWiIds.Count)"

$transitions = @()
$transitionIds = @{}
foreach ($wi in @($worldInteractions.items)) {
    foreach ($successor in @($wi.successorWiIds)) {
        $stableId = "transition:actual-e5:" + ([string] $wi.id).ToLowerInvariant() + ":" + ([string] $successor).ToLowerInvariant()
        if ($transitionIds.ContainsKey($stableId)) { continue }
        $transitionIds[$stableId] = $true
        $transitions += [ordered]@{
            transitionStableId = $stableId
            fromWorldInteractionId = [string] $wi.id
            toWorldInteractionId = [string] $successor
        }
    }
}

$interactionSpatialCatalog = [ordered]@{
    schemaVersion = "simulation-world-interaction-graph-binding.v2"
    networkStableId = [string] $policy.networkStableId
    catalogRevision = "actual-e5-regional-gameplay.r1"
    catalogHashSha256 = ""
    bindings = $bindings
    contextualBindings = $contextBindings
    nonSpatialWiIds = $nonSpatialWiIds
    transitions = $transitions
}
$interactionSpatialCatalog.catalogHashSha256 = Text-Hash (Stable-Json $interactionSpatialCatalog)

$result = [ordered]@{
    schemaVersion = "simulation-world-actual-e5-spatial-output.v1"
    revision = "simulation-world-actual-e5-spatial-output.r1"
    policyRevision = [string] $policy.revision
    generatedAtRuleCode = "DeterministicNoWallClock"
    counts = [ordered]@{
        areaSets = $areaSets.Count
        internalGraphs = @($graphs | Where-Object spatialOwnerKindCode -eq "AreaSet").Count
        networkRouteGraphs = $routeGraphs.Count
        totalGraphs = $graphs.Count
        networkRelations = $networkRelations.Count
        deferredTheoryGraphs = $deferredH3Refs.Count
        directBindings = $bindings.Count
        contextualBindings = $contextBindings.Count
        nonSpatialWi = $nonSpatialWiIds.Count
    }
    network = $network
    areaSets = $areaSets
    routeGraphs = $routeGraphs
    deferredTheoryH3Refs = $deferredH3Refs
    interactionSpatialCatalog = $interactionSpatialCatalog
    authorityBoundary = [ordered]@{
        evidenceStageCode = "ActualE5"
        evidenceKindCode = "Scenario"
        publicDataBound = $false
        runtimeValidated = $false
        operationalState = $false
        legacyFacadePreserved = $true
    }
    presentationOnly = $true
    isOperationalState = $false
}
$json = Normalize (Stable-Json $result)

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# 실제 E5 4영역 공간·WI 결속")
[void] $builder.AppendLine()
[void] $builder.AppendLine("이 문서는 이론 H2·H3를 작성 Scenario 좌표의 실제 AreaSet·Graph·Network에 결정적으로 결속한 결과다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- AreaSet: ``$($result.counts.areaSets)``")
[void] $builder.AppendLine("- 내부 Graph: ``$($result.counts.internalGraphs)`` · Network 경로 Graph: ``$($result.counts.networkRouteGraphs)``")
[void] $builder.AppendLine("- Network 관계: ``$($result.counts.networkRelations)``")
[void] $builder.AppendLine("- 이론 보류 Graph: ``$($result.counts.deferredTheoryGraphs)`` (정책 승격 전 실제 E5에서 제외)")
[void] $builder.AppendLine("- WI: 직접 ``$($result.counts.directBindings)`` · 문맥 ``$($result.counts.contextualBindings)`` · 비공간 ``$($result.counts.nonSpatialWi)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 영역 | 실제 AreaSet | Graph | 적재 정책 |")
[void] $builder.AppendLine("| --- | --- | ---: | --- |")
foreach ($area in $areaSets) {
    [void] $builder.AppendLine("| $($area.areaRoleCode) | ``$($area.definition.areaSetStableId)`` | $(@($area.graphs).Count) | ``$($area.loadPolicyCode)`` |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("작성 Scenario 근거는 E5 공간 결속이며 공공데이터 E6나 실제 서버·Unity E7 증거가 아니다.")
$markdown = Normalize $builder.ToString()

$jsonPath = Resolve-RepoPath $JsonOutputPath
$markdownPath = Resolve-RepoPath $MarkdownOutputPath
if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $jsonPath) "JsonOutputMissing"
    Require (Test-Path -LiteralPath $markdownPath) "MarkdownOutputMissing"
    Require ((Normalize ([IO.File]::ReadAllText($jsonPath))) -ceq $json) "JsonOutputStale"
    Require ((Normalize ([IO.File]::ReadAllText($markdownPath))) -ceq $markdown) "MarkdownOutputStale"
    Write-Output "ActualE5SpatialValid:AreaSets=$($areaSets.Count);Graphs=$($graphs.Count);Relations=$($networkRelations.Count);WI=$($bindings.Count)/$($contextBindings.Count)/$($nonSpatialWiIds.Count)"
    exit 0
}

foreach ($pair in @(@($jsonPath, $json), @($markdownPath, $markdown))) {
    $directory = Split-Path -Parent $pair[0]
    if (-not (Test-Path -LiteralPath $directory)) { [void] (New-Item -ItemType Directory -Path $directory) }
    if (-not (Test-Path -LiteralPath $pair[0]) -or (Normalize ([IO.File]::ReadAllText($pair[0]))) -cne [string] $pair[1]) {
        [IO.File]::WriteAllText($pair[0], [string] $pair[1], [Text.UTF8Encoding]::new($false))
    }
}
Write-Output "ActualE5SpatialGenerated:AreaSets=$($areaSets.Count);Graphs=$($graphs.Count);Relations=$($networkRelations.Count);WI=$($bindings.Count)/$($contextBindings.Count)/$($nonSpatialWiIds.Count)"
