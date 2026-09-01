[CmdletBinding()]
param(
    [ValidateSet('Check', 'Write')]
    [string] $Mode = 'Check',
    [string] $PlanPath = 'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json',
    [string] $JsonOutputPath = 'eng/world-seedbeds/generated/graph-map-plans.v1.json',
    [string] $MarkdownOutputPath = 'docs/AI/generated/graph-map-plans.md',
    [string] $UnityProjectRoot = '',
    [switch] $VerifyUnitySources
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$utf8 = [Text.UTF8Encoding]::new($false)

function Resolve-RepoPath([string] $relativePath) {
    return Join-Path $repositoryRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
}

function Read-Json([string] $relativePath) {
    $path = Resolve-RepoPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) { throw "GraphMapInvalid:JsonMissing:$relativePath" }
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Require([bool] $condition, [string] $code) {
    if (-not $condition) { throw "GraphMapInvalid:$code" }
}

function Require-Text([object] $value, [string] $code) {
    Require ($null -ne $value -and -not [string]::IsNullOrWhiteSpace([string] $value)) $code
}

function Require-Unique([object[]] $values, [scriptblock] $selector, [string] $code) {
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($value in @($values)) {
        $key = [string] (& $selector $value)
        Require-Text $key "$code`:Empty"
        Require ($seen.Add($key)) "$code`:$key"
    }
}

function File-Hash([string] $relativePath) {
    return (Get-FileHash -LiteralPath (Resolve-RepoPath $relativePath) -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Absolute-Hash([string] $path) {
    return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Resolve-UnityRoot {
    $candidates = [Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($UnityProjectRoot)) { $candidates.Add($UnityProjectRoot) }
    if (-not [string]::IsNullOrWhiteSpace($env:SSALDDEL_UNITY_ROOT)) { $candidates.Add($env:SSALDDEL_UNITY_ROOT) }
    $candidates.Add((Join-Path ([Environment]::GetFolderPath('UserProfile')) 'ssalddel'))
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Container) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }
    throw 'GraphMapInvalid:UnityProjectRootMissing'
}

function Resolve-ExternalChild([string] $root, [string] $relativePath, [string] $code) {
    Require (-not [IO.Path]::IsPathRooted($relativePath)) "$code`:Rooted"
    Require (-not (@($relativePath -split '[/\\]') -contains '..')) "$code`:Traversal"
    $normalizedRoot = [IO.Path]::GetFullPath($root).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    $candidate = [IO.Path]::GetFullPath((Join-Path $normalizedRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)))
    $prefix = $normalizedRoot + [IO.Path]::DirectorySeparatorChar
    Require ($candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) "$code`:OutsideRoot"
    return $candidate
}

function Stable-Json([object] $value) {
    return (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n") + "`n"
}

function Normalize-Text([string] $value) {
    return (($value -replace "`r`n", "`n").TrimEnd()) + "`n"
}

function Escape-Cell([object] $value) {
    if ($null -eq $value) { return '' }
    return ([string] $value).Replace('|', '\|').Replace("`r", '').Replace("`n", '<br>')
}

$plan = Read-Json $PlanPath
Require ([string] $plan.schemaVersion -eq 'mirror-graph-map-plan.v3') 'PlanSchema'
Require-Text $plan.revision 'PlanRevision'
Require-Text $plan.graphMapStableId 'GraphMapStableId'
Require ([string] $plan.purposeCode -eq 'PreWorldE5E6Planning') 'Purpose'

$catalogs = @($plan.sourceCatalogs)
Require ($catalogs.Count -ge 3) 'SourceCatalogCount'
Require-Unique $catalogs { param($x) $x.kindCode } 'SourceCatalogDuplicate'
foreach ($catalog in $catalogs) {
    Require-Text $catalog.path "SourceCatalogPath:$($catalog.kindCode)"
    Require (Test-Path -LiteralPath (Resolve-RepoPath ([string] $catalog.path))) "SourceCatalogMissing:$($catalog.kindCode)"
}

$actualCatalogRef = @($catalogs | Where-Object kindCode -eq 'ActualE5SpatialSnapshot')
$wiCatalogRef = @($catalogs | Where-Object kindCode -eq 'WorldInteractionCatalog')
$decisionCatalogRef = @($catalogs | Where-Object kindCode -eq 'DecisionCatalog')
Require ($actualCatalogRef.Count -eq 1) 'ActualCatalogReference'
Require ($wiCatalogRef.Count -eq 1) 'WiCatalogReference'
Require ($decisionCatalogRef.Count -eq 1) 'DecisionCatalogReference'

$actual = Read-Json ([string] $actualCatalogRef[0].path)
$wiCatalog = Read-Json ([string] $wiCatalogRef[0].path)
$decisionPath = Resolve-RepoPath ([string] $decisionCatalogRef[0].path)
$decisionText = Get-Content -LiteralPath $decisionPath -Raw -Encoding UTF8
Require ([string] $actual.revision -eq [string] $actualCatalogRef[0].expectedRevision) 'ActualCatalogRevision'
Require ([string] $wiCatalog.revision -eq [string] $wiCatalogRef[0].expectedRevision) 'WiCatalogRevision'

foreach ($catalog in $catalogs | Where-Object kindCode -eq 'HistoricalPlanningSeedbed') {
    $historical = Read-Json ([string] $catalog.path)
    Require ([string] $historical.revision -eq [string] $catalog.expectedRevision) "HistoricalCatalogRevision:$($catalog.path)"
}

$decisionIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($match in [regex]::Matches($decisionText, '(?m)^## (D-\d{3})\b')) {
    $null = $decisionIds.Add($match.Groups[1].Value)
}
$wiIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($wi in @($wiCatalog.items)) { $null = $wiIds.Add([string] $wi.id) }

$areaIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$graphIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$nodeIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$actualSourceRefs = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$graphArea = @{}
$nodeGraph = @{}

foreach ($area in @($actual.areaSets)) {
    $areaId = [string] $area.definition.areaSetStableId
    Require ($areaIds.Add($areaId)) "ActualAreaDuplicate:$areaId"
    $null = $actualSourceRefs.Add($areaId)
    foreach ($graph in @($area.graphs)) {
        $graphId = [string] $graph.landscapeGraphStableId
        Require ($graphIds.Add($graphId)) "ActualGraphDuplicate:$graphId"
        $graphArea[$graphId] = $areaId
        $null = $actualSourceRefs.Add($graphId)
        foreach ($node in @($graph.nodes)) {
            $nodeId = [string] $node.nodeStableId
            Require ($nodeIds.Add($nodeId)) "ActualNodeDuplicate:$nodeId"
            $nodeGraph[$nodeId] = $graphId
            $null = $actualSourceRefs.Add($nodeId)
        }
        foreach ($edge in @($graph.edges)) { $null = $actualSourceRefs.Add([string] $edge.edgeStableId) }
    }
    foreach ($relation in @($area.definition.graphRelations)) {
        $null = $actualSourceRefs.Add([string] $relation.relationStableId)
    }
}
foreach ($graph in @($actual.routeGraphs)) { $null = $actualSourceRefs.Add([string] $graph.landscapeGraphStableId) }
foreach ($relation in @($actual.network.relations)) { $null = $actualSourceRefs.Add([string] $relation.relationStableId) }

$authority = $plan.authorityBoundary
Require ([bool] $authority.presentationOnly) 'AuthorityPresentationOnly'
Require (-not [bool] $authority.isOperationalState) 'AuthorityOperationalState'
Require (-not [bool] $authority.worldApplied) 'AuthorityWorldApplied'
Require (-not [bool] $authority.actualTraversalVerified) 'AuthorityTraversalVerified'
Require (-not [bool] $authority.unitySceneChanged) 'AuthoritySceneChanged'

$requiredContext = @('time', 'place', 'player', 'target', 'method', 'result', 'nextChoices')
Require (@($plan.level1.contextFieldOrder).Count -eq $requiredContext.Count) 'ContextFieldCount'
for ($i = 0; $i -lt $requiredContext.Count; $i++) {
    Require ([string] $plan.level1.contextFieldOrder[$i] -eq $requiredContext[$i]) "ContextFieldOrder:$i"
}

$nodes = @($plan.level1.nodes)
$edges = @($plan.level1.edges)
$constraints = @($plan.level2.constraints)
Require ($nodes.Count -gt 0) 'NodesEmpty'
Require ($edges.Count -gt 0) 'EdgesEmpty'
Require ($constraints.Count -gt 0) 'ConstraintsEmpty'
Require-Unique $nodes { param($x) $x.nodeId } 'NodeDuplicate'
Require-Unique $edges { param($x) $x.edgeId } 'EdgeDuplicate'
Require-Unique $constraints { param($x) $x.constraintId } 'ConstraintDuplicate'

$planNodeIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($node in $nodes) { $null = $planNodeIds.Add([string] $node.nodeId) }
$planEdgeIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($edge in $edges) { $null = $planEdgeIds.Add([string] $edge.edgeId) }

$traversalProfiles = @($plan.traversalProfiles)
Require ($traversalProfiles.Count -gt 0) 'TraversalProfilesEmpty'
Require-Unique $traversalProfiles { param($x) $x.profileId } 'TraversalProfileDuplicate'
$traversalProfileIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$allowedReturnPolicy = @('RequiredWhenTraversalIsRequired', 'NotApplicable', 'WorkResultCanReturnToSource', 'ReturnOrHoldRequired', 'IndependentAreasRemainRunnable', 'Unresolved')
foreach ($profile in $traversalProfiles) {
    $profileId = [string] $profile.profileId
    $null = $traversalProfileIds.Add($profileId)
    Require-Text $profile.label "TraversalProfileLabel:$profileId"
    Require-Text $profile.cargoModeCode "TraversalProfileCargo:$profileId"
    Require-Text $profile.vehicleModeCode "TraversalProfileVehicle:$profileId"
    Require ($allowedReturnPolicy -contains [string] $profile.returnPolicyCode) "TraversalProfileReturnPolicy:$profileId"
    Require (@($profile.conditionInputs).Count -gt 0) "TraversalProfileConditionsEmpty:$profileId"
    Require-Text $profile.evidenceBoundary "TraversalProfileEvidenceBoundary:$profileId"
}

$allowedRealization = @('ExistingActualGraphRef', 'ExistingPartialGraphRef', 'PlanningGateway', 'UnresolvedSpatial')
$allowedState = @('ReferenceAvailable', 'Planned', 'Unresolved')
foreach ($node in $nodes) {
    $nodeId = [string] $node.nodeId
    Require ($allowedRealization -contains [string] $node.realizationCode) "NodeRealization:$nodeId"
    Require ($allowedState -contains [string] $node.stateCode) "NodeState:$nodeId"
    foreach ($field in $requiredContext) {
        Require ($null -ne $node.planningContext.PSObject.Properties[$field]) "NodeContextMissing:$nodeId`:$field"
        Require-Text $node.planningContext.$field "NodeContextEmpty:$nodeId`:$field"
    }
    foreach ($wiId in @($node.worldInteractionIds)) {
        Require ($wiIds.Contains([string] $wiId)) "NodeUnknownWi:$nodeId`:$wiId"
    }
    foreach ($decisionId in @($node.sourceDecisionIds)) {
        Require ($decisionIds.Contains([string] $decisionId)) "NodeUnknownDecision:$nodeId`:$decisionId"
    }

    if ([string] $node.realizationCode -eq 'ExistingActualGraphRef') {
        Require ($null -ne $node.actualRef) "NodeActualRefMissing:$nodeId"
        $areaId = [string] $node.actualRef.areaSetStableId
        $graphId = [string] $node.actualRef.graphStableId
        $actualNodeId = [string] $node.actualRef.nodeStableId
        Require ($areaIds.Contains($areaId)) "NodeActualAreaUnknown:$nodeId"
        Require ($graphIds.Contains($graphId)) "NodeActualGraphUnknown:$nodeId"
        Require ($nodeIds.Contains($actualNodeId)) "NodeActualNodeUnknown:$nodeId"
        Require ([string] $graphArea[$graphId] -eq $areaId) "NodeActualAreaGraphMismatch:$nodeId"
        Require ([string] $nodeGraph[$actualNodeId] -eq $graphId) "NodeActualGraphNodeMismatch:$nodeId"
        Require ([string] $node.stateCode -eq 'ReferenceAvailable') "NodeActualState:$nodeId"
    }
    elseif ([string] $node.realizationCode -in @('PlanningGateway', 'UnresolvedSpatial')) {
        Require ($null -eq $node.actualRef) "NodePlanningActualRef:$nodeId"
        Require ([string] $node.stateCode -eq 'Unresolved') "NodePlanningState:$nodeId"
    }
}

$allowedEdgeKind = @('Traversal', 'DiscoverySightline', 'WorkHandoff', 'Logistics', 'ExternalGateway')
$allowedIntention = @('Required', 'Optional', 'Separated', 'Unknown')
foreach ($edge in $edges) {
    $edgeId = [string] $edge.edgeId
    Require ($planNodeIds.Contains([string] $edge.fromNodeId)) "EdgeFromUnknown:$edgeId"
    Require ($planNodeIds.Contains([string] $edge.toNodeId)) "EdgeToUnknown:$edgeId"
    Require ([string] $edge.fromNodeId -ne [string] $edge.toNodeId) "EdgeSelfLoop:$edgeId"
    Require ($allowedEdgeKind -contains [string] $edge.kindCode) "EdgeKind:$edgeId"
    Require ($allowedIntention -contains [string] $edge.intentionCode) "EdgeIntention:$edgeId"
    Require ($allowedState -contains [string] $edge.stateCode) "EdgeState:$edgeId"
    Require ($traversalProfileIds.Contains([string] $edge.capabilityProfileRef)) "EdgeCapabilityProfileUnknown:$edgeId"
    Require-Text $edge.reason "EdgeReason:$edgeId"
    if ([string] $edge.stateCode -eq 'ReferenceAvailable') {
        Require (@($edge.sourceRelationRefs).Count -gt 0) "EdgeReferenceMissing:$edgeId"
        foreach ($sourceRef in @($edge.sourceRelationRefs)) {
            Require ($actualSourceRefs.Contains([string] $sourceRef)) "EdgeReferenceUnknown:$edgeId`:$sourceRef"
        }
        $fromNode = @($nodes | Where-Object nodeId -eq $edge.fromNodeId)[0]
        $toNode = @($nodes | Where-Object nodeId -eq $edge.toNodeId)[0]
        Require ([string] $fromNode.realizationCode -eq 'ExistingActualGraphRef') "EdgeReferenceFromNotActual:$edgeId"
        Require ([string] $toNode.realizationCode -eq 'ExistingActualGraphRef') "EdgeReferenceToNotActual:$edgeId"
    }
    if ([string] $edge.stateCode -eq 'Unresolved') {
        Require (@($edge.sourceRelationRefs).Count -eq 0) "EdgeUnresolvedHasActualRef:$edgeId"
    }
    if ([string] $edge.kindCode -eq 'Traversal' -and [string] $edge.intentionCode -eq 'Required') {
        $hasReturn = [bool] $edge.bidirectional -or @($edges | Where-Object {
            [string] $_.fromNodeId -eq [string] $edge.toNodeId -and
            [string] $_.toNodeId -eq [string] $edge.fromNodeId -and
            [string] $_.stateCode -ne 'Unresolved'
        }).Count -gt 0
        Require $hasReturn "RequiredTraversalWithoutReturn:$edgeId"
    }
}

$allowedSeverity = @('Blocking', 'Advisory')
$allowedConstraintState = @('Required', 'Optional')
$allowedConstraintEnforcement = @('Static', 'StaticAndHumanReview', 'StaticAndPlayMode')
$allowedConstraintEvidence = @('E4', 'E5')
foreach ($constraint in $constraints) {
    $constraintId = [string] $constraint.constraintId
    Require ($allowedSeverity -contains [string] $constraint.severityCode) "ConstraintSeverity:$constraintId"
    Require ($allowedConstraintState -contains [string] $constraint.stateCode) "ConstraintState:$constraintId"
    Require-Text $constraint.rule "ConstraintRule:$constraintId"
    Require (@($constraint.targetRefs).Count -gt 0) "ConstraintTargetsEmpty:$constraintId"
    Require (@($constraint.sourceRefs).Count -gt 0) "ConstraintSourcesEmpty:$constraintId"
    Require ($allowedConstraintEnforcement -contains [string] $constraint.enforcementCode) "ConstraintEnforcement:$constraintId"
    Require ($allowedConstraintEvidence -contains [string] $constraint.requiredAtEvidence) "ConstraintEvidenceStage:$constraintId"
    Require (@($constraint.validatorRefs).Count -gt 0) "ConstraintValidatorsEmpty:$constraintId"
    Require-Text $constraint.failureCode "ConstraintFailureCode:$constraintId"
    Require (@($constraint.invalidationConditions).Count -gt 0) "ConstraintInvalidationEmpty:$constraintId"
    foreach ($target in @($constraint.targetRefs)) {
        $targetText = [string] $target
        $validTarget = $planNodeIds.Contains($targetText) -or $planEdgeIds.Contains($targetText) -or
            $targetText -eq 'gm-node:*' -or $targetText -eq 'gm-edge:*'
        Require $validTarget "ConstraintTargetUnknown:$constraintId`:$targetText"
    }
    foreach ($source in @($constraint.sourceRefs)) {
        $sourceText = [string] $source
        $validSource = $decisionIds.Contains($sourceText) -or $wiIds.Contains($sourceText)
        if (-not $validSource -and ($sourceText -match '^(docs|eng|Ssalddel\.)/')) {
            $validSource = Test-Path -LiteralPath (Resolve-RepoPath $sourceText)
        }
        Require $validSource "ConstraintSourceUnknown:$constraintId`:$sourceText"
    }
}

$federation = $plan.federation
Require-Text $federation.federationId 'FederationId'
Require ([string] $federation.scaleLayerCode -eq 'PartitionedGraphMap') 'FederationScaleLayer'
Require-Text $federation.splitPolicyMeaning 'FederationSplitMeaning'

$partitionRef = $federation.partitionCatalogRef
$partitionCatalog = Read-Json ([string] $partitionRef.path)
Require ([string] $partitionCatalog.schemaVersion -eq 'mirror-graph-map-partition-catalog.v1') 'PartitionCatalogSchema'
Require ([string] $partitionCatalog.revision -eq [string] $partitionRef.expectedRevision) 'PartitionCatalogRevision'
Require ([string] $partitionCatalog.federationStableId -eq [string] $plan.graphMapStableId) 'PartitionFederationIdentity'
Require ([string] $partitionCatalog.sourceElementCatalogRef.path -eq [string] $PlanPath) 'PartitionSourcePlanPath'
Require ([string] $partitionCatalog.sourceElementCatalogRef.expectedRevision -eq [string] $plan.revision) 'PartitionSourcePlanRevision'
Require ([int] $partitionCatalog.splitPolicy.hardNodeLimit -gt 0) 'PartitionHardNodeLimit'
Require ([int] $partitionCatalog.splitPolicy.hardEdgeLimit -gt 0) 'PartitionHardEdgeLimit'

$subgraphs = @($partitionCatalog.subgraphs)
$connectors = @($partitionCatalog.connectors)
Require ($subgraphs.Count -gt 0) 'SubgraphsEmpty'
Require-Unique $subgraphs { param($x) $x.subgraphId } 'SubgraphDuplicate'
Require-Unique $connectors { param($x) $x.connectorId } 'ConnectorDuplicate'
$subgraphIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$partitionNodeIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$partitionEdgeIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$partitionConstraintIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$portsById = @{}
$portSubgraphById = @{}

foreach ($subgraph in $subgraphs) {
    $subgraphId = [string] $subgraph.subgraphId
    $null = $subgraphIds.Add($subgraphId)
    Require-Text $subgraph.label "SubgraphLabel:$subgraphId"
    Require-Text $subgraph.scopeCode "SubgraphScope:$subgraphId"
    Require-Text $subgraph.ownerCode "SubgraphOwner:$subgraphId"
    Require (@($subgraph.nodeRefs).Count -le [int] $partitionCatalog.splitPolicy.hardNodeLimit) "SubgraphNodeHardLimit:$subgraphId"
    Require (@($subgraph.internalEdgeRefs).Count -le [int] $partitionCatalog.splitPolicy.hardEdgeLimit) "SubgraphEdgeHardLimit:$subgraphId"
    foreach ($areaRef in @($subgraph.areaSetRefs)) {
        Require ($areaIds.Contains([string] $areaRef)) "SubgraphAreaUnknown:$($subgraphId):$areaRef"
    }
    $localNodeIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($nodeRef in @($subgraph.nodeRefs)) {
        $nodeText = [string] $nodeRef
        Require ($planNodeIds.Contains($nodeText)) "SubgraphNodeUnknown:$($subgraphId):$nodeText"
        Require ($localNodeIds.Add($nodeText)) "SubgraphNodeLocalDuplicate:$($subgraphId):$nodeText"
        Require ($partitionNodeIds.Add($nodeText)) "SubgraphNodeOwnershipDuplicate:$nodeText"
    }
    foreach ($edgeRef in @($subgraph.internalEdgeRefs)) {
        $edgeText = [string] $edgeRef
        Require ($planEdgeIds.Contains($edgeText)) "SubgraphEdgeUnknown:$($subgraphId):$edgeText"
        Require ($partitionEdgeIds.Add($edgeText)) "SubgraphEdgeOwnershipDuplicate:$edgeText"
        $edge = @($edges | Where-Object edgeId -eq $edgeText)[0]
        Require ($localNodeIds.Contains([string] $edge.fromNodeId) -and $localNodeIds.Contains([string] $edge.toNodeId)) "SubgraphInternalEdgeEndpointOutside:$($subgraphId):$edgeText"
    }
    foreach ($constraintRef in @($subgraph.constraintRefs)) {
        $constraintText = [string] $constraintRef
        Require (@($constraints | Where-Object constraintId -eq $constraintText).Count -eq 1) "SubgraphConstraintUnknown:$($subgraphId):$constraintText"
        Require ($partitionConstraintIds.Add($constraintText)) "SubgraphConstraintOwnershipDuplicate:$constraintText"
    }
    foreach ($port in @($subgraph.ports)) {
        $portId = [string] $port.portId
        Require-Text $portId "PortId:$subgraphId"
        Require (-not $portsById.ContainsKey($portId)) "PortDuplicate:$portId"
        Require ($localNodeIds.Contains([string] $port.nodeRef)) "PortNodeOutsideSubgraph:$portId"
        Require ([string] $port.directionCode -in @('Inbound', 'Outbound')) "PortDirection:$portId"
        Require (@($port.capabilityCodes).Count -gt 0) "PortCapabilitiesEmpty:$portId"
        Require-Text $port.meaning "PortMeaning:$portId"
        $portsById[$portId] = $port
        $portSubgraphById[$portId] = $subgraphId
    }
}

foreach ($connector in $connectors) {
    $connectorId = [string] $connector.connectorId
    $edgeRef = [string] $connector.edgeRef
    $fromPortRef = [string] $connector.fromPortRef
    $toPortRef = [string] $connector.toPortRef
    Require ($planEdgeIds.Contains($edgeRef)) "ConnectorEdgeUnknown:$connectorId"
    Require ($partitionEdgeIds.Add($edgeRef)) "ConnectorEdgeOwnershipDuplicate:$edgeRef"
    Require ($portsById.ContainsKey($fromPortRef)) "ConnectorFromPortUnknown:$connectorId"
    Require ($portsById.ContainsKey($toPortRef)) "ConnectorToPortUnknown:$connectorId"
    Require ([string] $portSubgraphById[$fromPortRef] -ne [string] $portSubgraphById[$toPortRef]) "ConnectorSameSubgraph:$connectorId"
    $fromPort = $portsById[$fromPortRef]
    $toPort = $portsById[$toPortRef]
    Require ([string] $fromPort.directionCode -eq 'Outbound') "ConnectorFromDirection:$connectorId"
    Require ([string] $toPort.directionCode -eq 'Inbound') "ConnectorToDirection:$connectorId"
    $edge = @($edges | Where-Object edgeId -eq $edgeRef)[0]
    Require ([string] $edge.fromNodeId -eq [string] $fromPort.nodeRef) "ConnectorFromNodeMismatch:$connectorId"
    Require ([string] $edge.toNodeId -eq [string] $toPort.nodeRef) "ConnectorToNodeMismatch:$connectorId"
    Require ([string] $connector.stateCode -eq [string] $edge.stateCode) "ConnectorStateMismatch:$connectorId"
    foreach ($capability in @($connector.requiredCapabilityCodes)) {
        Require (@($fromPort.capabilityCodes) -contains [string] $capability) "ConnectorCapabilityMissingFrom:$($connectorId):$capability"
        Require (@($toPort.capabilityCodes) -contains [string] $capability) "ConnectorCapabilityMissingTo:$($connectorId):$capability"
    }
}

foreach ($constraintRef in @($partitionCatalog.federationConstraintRefs)) {
    $constraintText = [string] $constraintRef
    Require (@($constraints | Where-Object constraintId -eq $constraintText).Count -eq 1) "FederationConstraintUnknown:$constraintText"
    Require ($partitionConstraintIds.Add($constraintText)) "FederationConstraintOwnershipDuplicate:$constraintText"
}
foreach ($node in $nodes) { Require ($partitionNodeIds.Contains([string] $node.nodeId)) "SubgraphNodeCoverageMissing:$($node.nodeId)" }
foreach ($edge in $edges) { Require ($partitionEdgeIds.Contains([string] $edge.edgeId)) "SubgraphEdgeCoverageMissing:$($edge.edgeId)" }
foreach ($constraint in $constraints) { Require ($partitionConstraintIds.Contains([string] $constraint.constraintId)) "SubgraphConstraintCoverageMissing:$($constraint.constraintId)" }

$overlayRef = $federation.overlayCatalogRef
$overlayCatalog = Read-Json ([string] $overlayRef.path)
Require ([string] $overlayCatalog.schemaVersion -eq 'mirror-graph-map-overlay-catalog.v1') 'OverlayCatalogSchema'
Require ([string] $overlayCatalog.revision -eq [string] $overlayRef.expectedRevision) 'OverlayCatalogRevision'
$overlays = @($overlayCatalog.overlays)
Require ($overlays.Count -gt 0) 'OverlaysEmpty'
Require-Unique $overlays { param($x) $x.overlayId } 'OverlayDuplicate'
foreach ($overlay in $overlays) {
    $overlayId = [string] $overlay.overlayId
    Require-Text $overlay.label "OverlayLabel:$overlayId"
    Require-Text $overlay.triggerKindCode "OverlayTrigger:$overlayId"
    Require-Text $overlay.stateCode "OverlayState:$overlayId"
    Require (-not [bool] $overlay.topologyMutationAllowed) "OverlayTopologyMutation:$overlayId"
    Require (-not [bool] $overlay.authorityMutationAllowed) "OverlayAuthorityMutation:$overlayId"
    Require (@($overlay.effectCategories).Count -gt 0) "OverlayEffectsEmpty:$overlayId"
    Require-Text $overlay.valueBoundary "OverlayValueBoundary:$overlayId"
    foreach ($subgraphRef in @($overlay.targetSubgraphRefs)) {
        Require ($subgraphIds.Contains([string] $subgraphRef)) "OverlaySubgraphUnknown:$($overlayId):$subgraphRef"
    }
    foreach ($sourceDecisionId in @($overlay.sourceDecisionIds)) {
        Require ($decisionIds.Contains([string] $sourceDecisionId)) "OverlayDecisionUnknown:$($overlayId):$sourceDecisionId"
    }
}

$level3 = $plan.level3
Require-Text $level3.meaning 'Level3Meaning'
$codeCatalogRef = $level3.codeBindingCatalogRef
$codeCatalog = Read-Json ([string] $codeCatalogRef.path)
Require ([string] $codeCatalog.schemaVersion -eq 'mirror-graph-map-code-binding-catalog.v1') 'Level3CatalogSchema'
Require ([string] $codeCatalog.revision -eq [string] $codeCatalogRef.expectedRevision) 'Level3CatalogRevision'
Require ([bool] $codeCatalog.evidenceBoundary.sourceAndSymbolVerified) 'Level3SourceEvidence'
Require (-not [bool] $codeCatalog.evidenceBoundary.sceneWiringVerified) 'Level3SceneWiringBoundary'
Require (-not [bool] $codeCatalog.evidenceBoundary.runtimeExecutionVerified) 'Level3RuntimeBoundary'
Require (-not [bool] $codeCatalog.evidenceBoundary.gameViewVerified) 'Level3GameViewBoundary'
Require-Text $codeCatalog.evidenceBoundary.meaning 'Level3EvidenceMeaning'
Require ([bool] $level3.evidenceBoundary.sourceAndSymbolVerified -eq [bool] $codeCatalog.evidenceBoundary.sourceAndSymbolVerified) 'Level3EvidenceSourceMismatch'
Require ([bool] $level3.evidenceBoundary.sceneWiringVerified -eq [bool] $codeCatalog.evidenceBoundary.sceneWiringVerified) 'Level3EvidenceSceneMismatch'
Require ([bool] $level3.evidenceBoundary.runtimeExecutionVerified -eq [bool] $codeCatalog.evidenceBoundary.runtimeExecutionVerified) 'Level3EvidenceRuntimeMismatch'
Require ([bool] $level3.evidenceBoundary.gameViewVerified -eq [bool] $codeCatalog.evidenceBoundary.gameViewVerified) 'Level3EvidenceGameViewMismatch'

$sourceRoots = @($codeCatalog.sourceRoots)
$catalogBindings = @($codeCatalog.bindings)
$bindingAssignments = @($level3.bindingAssignments)
$unboundTargets = @($level3.unboundTargets)
Require ($sourceRoots.Count -gt 0) 'Level3SourceRootsEmpty'
Require ($catalogBindings.Count -gt 0) 'Level3BindingsEmpty'
Require-Unique $sourceRoots { param($x) $x.sourceRootCode } 'Level3SourceRootDuplicate'
Require-Unique $catalogBindings { param($x) $x.bindingId } 'Level3BindingDuplicate'
Require-Unique $bindingAssignments { param($x) $x.bindingId } 'Level3AssignmentDuplicate'
Require-Unique $unboundTargets { param($x) $x.targetRef } 'Level3UnboundDuplicate'
Require ($bindingAssignments.Count -eq $catalogBindings.Count) 'Level3AssignmentCoverageCount'

$allCodeTargetIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($node in $nodes) { $null = $allCodeTargetIds.Add([string] $node.nodeId) }
foreach ($edge in $edges) { $null = $allCodeTargetIds.Add([string] $edge.edgeId) }
foreach ($constraint in $constraints) { $null = $allCodeTargetIds.Add([string] $constraint.constraintId) }
$unresolvedLevel1 = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($node in $nodes | Where-Object stateCode -eq 'Unresolved') { $null = $unresolvedLevel1.Add([string] $node.nodeId) }
foreach ($edge in $edges | Where-Object stateCode -eq 'Unresolved') { $null = $unresolvedLevel1.Add([string] $edge.edgeId) }

$codeBindings = [Collections.Generic.List[object]]::new()
foreach ($catalogBinding in $catalogBindings) {
    $bindingId = [string] $catalogBinding.bindingId
    $assignment = @($bindingAssignments | Where-Object bindingId -eq $bindingId)
    Require ($assignment.Count -eq 1) "Level3AssignmentMissing:$bindingId"
    $selector = [string] $assignment[0].targetSelectorCode
    $targetRefs = @()
    switch ($selector) {
        'AllResolvedNodesAndEdges' {
            Require (@($assignment[0].explicitTargetRefs).Count -eq 0) "Level3SelectorExplicitTargets:$bindingId"
            $targetRefs = @($nodes | Where-Object stateCode -ne 'Unresolved' | ForEach-Object nodeId) + @($edges | Where-Object stateCode -ne 'Unresolved' | ForEach-Object edgeId)
        }
        'AllResolvedNodes' {
            Require (@($assignment[0].explicitTargetRefs).Count -eq 0) "Level3SelectorExplicitTargets:$bindingId"
            $targetRefs = @($nodes | Where-Object stateCode -ne 'Unresolved' | ForEach-Object nodeId)
        }
        'ExplicitRefs' {
            $targetRefs = @($assignment[0].explicitTargetRefs)
            Require ($targetRefs.Count -gt 0) "Level3ExplicitTargetsEmpty:$bindingId"
        }
        default { throw "GraphMapInvalid:Level3Selector:$($bindingId):$selector" }
    }
    $codeBindings.Add([pscustomobject][ordered]@{
        bindingId = $bindingId
        label = $catalogBinding.label
        relationshipCode = $catalogBinding.relationshipCode
        runtimeUseCode = $catalogBinding.runtimeUseCode
        evidenceStateCode = $catalogBinding.evidenceStateCode
        bindingStageCode = $catalogBinding.bindingStageCode
        targetSelectorCode = $selector
        targetRefs = @($targetRefs)
        responsibilities = @($catalogBinding.responsibilities)
        files = @($catalogBinding.files)
    })
}

$sourceRootIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$resolvedSourceRoots = @{}
foreach ($sourceRoot in $sourceRoots) {
    $sourceRootCode = [string] $sourceRoot.sourceRootCode
    $null = $sourceRootIds.Add($sourceRootCode)
    Require ([string] $sourceRoot.repositoryKindCode -eq 'UnityProject') "Level3SourceRootKind:$sourceRootCode"
    Require-Text $sourceRoot.projectFolderName "Level3ProjectFolder:$sourceRootCode"
    Require ([string] $sourceRoot.observedRepositoryHead -match '^[0-9a-fA-F]{40}$') "Level3ObservedHead:$sourceRootCode"
    Require-Text $sourceRoot.canonicalScenePath "Level3CanonicalScene:$sourceRootCode"
    Require ([string] $sourceRoot.canonicalSceneSha256 -match '^[0-9a-fA-F]{64}$') "Level3CanonicalSceneHash:$sourceRootCode"
    Require-Text $sourceRoot.snapshotBoundary "Level3SnapshotBoundary:$sourceRootCode"
    if ($VerifyUnitySources) {
        Require ($sourceRootCode -eq 'SsalddelUnity') "Level3UnsupportedExternalRoot:$sourceRootCode"
        $resolvedRoot = Resolve-UnityRoot
        $resolvedSourceRoots[$sourceRootCode] = $resolvedRoot
        $scenePath = Resolve-ExternalChild $resolvedRoot ([string] $sourceRoot.canonicalScenePath) "Level3CanonicalScenePath:$sourceRootCode"
        Require (Test-Path -LiteralPath $scenePath -PathType Leaf) "Level3CanonicalSceneMissing:$sourceRootCode"
        Require ((Absolute-Hash $scenePath) -ceq ([string] $sourceRoot.canonicalSceneSha256).ToUpperInvariant()) "Level3CanonicalSceneHashMismatch:$sourceRootCode"
    }
}

$boundLevel1 = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$sourceFileHashes = @{}
$uniqueSourceFiles = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$allowedRelationship = @('SharedNetworkProjectionPipeline', 'SharedLandscapeRealizationPipeline', 'ExactHStableIdConsumer', 'EditorPreviewOnly', 'EditorConstraintInspector')
$allowedRuntimeUse = @('Runtime', 'EditorOnly')
$allowedEvidenceState = @('SourceAndSymbolVerified')
$allowedBindingStage = @($codeCatalog.bindingStageOrder)
$allowedComponentKind = @('RuntimeContract', 'RuntimeRepository', 'RuntimeAssembler', 'RuntimePlanningProvider', 'RuntimeWorldModel', 'MonoBehaviourController', 'MonoBehaviourPresenter', 'MonoBehaviourView', 'ScriptableObjectCatalog', 'EditorBuilder', 'EditorRuleEngine')

foreach ($binding in $codeBindings) {
    $bindingId = [string] $binding.bindingId
    Require-Text $binding.label "Level3BindingLabel:$bindingId"
    Require ($allowedRelationship -contains [string] $binding.relationshipCode) "Level3Relationship:$bindingId"
    Require ($allowedRuntimeUse -contains [string] $binding.runtimeUseCode) "Level3RuntimeUse:$bindingId"
    Require ($allowedEvidenceState -contains [string] $binding.evidenceStateCode) "Level3EvidenceState:$bindingId"
    Require ($allowedBindingStage -contains [string] $binding.bindingStageCode) "Level3BindingStage:$bindingId"
    Require (@($binding.targetRefs).Count -gt 0) "Level3TargetsEmpty:$bindingId"
    Require (@($binding.responsibilities).Count -gt 0) "Level3ResponsibilitiesEmpty:$bindingId"
    Require (@($binding.files).Count -gt 0) "Level3FilesEmpty:$bindingId"
    foreach ($target in @($binding.targetRefs)) {
        $targetText = [string] $target
        Require ($allCodeTargetIds.Contains($targetText)) "Level3TargetUnknown:$($bindingId):$targetText"
        Require (-not $unresolvedLevel1.Contains($targetText)) "Level3UnresolvedTargetBound:$($bindingId):$targetText"
        if ($planNodeIds.Contains($targetText) -or $planEdgeIds.Contains($targetText)) { $null = $boundLevel1.Add($targetText) }
    }
    foreach ($file in @($binding.files)) {
        $sourceRootCode = [string] $file.sourceRootCode
        $pathText = [string] $file.path
        $hashText = ([string] $file.expectedSha256).ToUpperInvariant()
        Require ($sourceRootIds.Contains($sourceRootCode)) "Level3FileSourceRootUnknown:$($bindingId):$sourceRootCode"
        Require ($pathText -match '^Assets/.+\.cs$') "Level3FilePathInvalid:$($bindingId):$pathText"
        Require ($hashText -match '^[0-9A-F]{64}$') "Level3FileHashInvalid:$($bindingId):$pathText"
        Require ($allowedComponentKind -contains [string] $file.componentKindCode) "Level3ComponentKind:$($bindingId):$pathText"
        Require-Text $file.assemblyName "Level3AssemblyName:$($bindingId):$pathText"
        Require-Text $file.ownerCode "Level3OwnerCode:$($bindingId):$pathText"
        Require (@($file.symbols).Count -gt 0) "Level3SymbolsEmpty:$($bindingId):$pathText"
        Require ($null -eq $file.PSObject.Properties['sourceText']) "Level3FileSourceBodyForbidden:$($bindingId):$pathText"
        $isEditorPath = $pathText.IndexOf('/Editor/', [StringComparison]::OrdinalIgnoreCase) -ge 0
        if ([string] $binding.runtimeUseCode -eq 'EditorOnly') { Require $isEditorPath "Level3EditorBindingOutsideEditor:$($bindingId):$pathText" }
        else { Require (-not $isEditorPath) "Level3RuntimeBindingUsesEditor:$($bindingId):$pathText" }
        $fileKey = "$($sourceRootCode):$pathText"
        if ($sourceFileHashes.ContainsKey($fileKey)) { Require ([string] $sourceFileHashes[$fileKey] -ceq $hashText) "Level3FileHashConflict:$fileKey" }
        else { $sourceFileHashes[$fileKey] = $hashText }
        $null = $uniqueSourceFiles.Add($fileKey)
        foreach ($symbol in @($file.symbols)) { Require-Text $symbol "Level3SymbolEmpty:$($bindingId):$pathText" }
        if ($VerifyUnitySources) {
            $resolvedRoot = [string] $resolvedSourceRoots[$sourceRootCode]
            $sourcePath = Resolve-ExternalChild $resolvedRoot $pathText "Level3SourcePath:$bindingId"
            Require (Test-Path -LiteralPath $sourcePath -PathType Leaf) "Level3SourceMissing:$($bindingId):$pathText"
            Require ((Absolute-Hash $sourcePath) -ceq $hashText) "Level3SourceHashMismatch:$($bindingId):$pathText"
            $sourceText = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8
            foreach ($symbol in @($file.symbols)) {
                Require ($sourceText.IndexOf([string] $symbol, [StringComparison]::Ordinal) -ge 0) "Level3SourceSymbolMissing:$($bindingId):$($pathText):$symbol"
            }
        }
    }
}

$unboundSet = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($unbound in $unboundTargets) {
    $targetRef = [string] $unbound.targetRef
    Require ($allCodeTargetIds.Contains($targetRef)) "Level3UnboundTargetUnknown:$targetRef"
    Require ($unresolvedLevel1.Contains($targetRef)) "Level3UnboundTargetResolved:$targetRef"
    Require ([string] $unbound.reasonCode -eq 'NoApprovedUnityBinding') "Level3UnboundReasonCode:$targetRef"
    Require-Text $unbound.reason "Level3UnboundReason:$targetRef"
    $null = $unboundSet.Add($targetRef)
}
foreach ($unresolvedTarget in $unresolvedLevel1) { Require ($unboundSet.Contains($unresolvedTarget)) "Level3UnboundCoverageMissing:$unresolvedTarget" }
foreach ($node in $nodes | Where-Object stateCode -eq 'ReferenceAvailable') { Require ($boundLevel1.Contains([string] $node.nodeId)) "Level3NodeBindingMissing:$($node.nodeId)" }
foreach ($edge in $edges | Where-Object stateCode -eq 'ReferenceAvailable') { Require ($boundLevel1.Contains([string] $edge.edgeId)) "Level3EdgeBindingMissing:$($edge.edgeId)" }
$unresolvedNodeCount = @($nodes | Where-Object stateCode -eq 'Unresolved').Count
$unresolvedEdgeCount = @($edges | Where-Object stateCode -eq 'Unresolved').Count
$output = [ordered]@{
    schemaVersion = 'mirror-graph-map-plan-output.v3'
    revision = 'mirror-graph-map-plan-output.r3'
    generatedAtRuleCode = 'DeterministicNoWallClock'
    sourcePlanRef = $PlanPath
    sourcePlanHashSha256 = File-Hash $PlanPath
    federationSnapshot = [ordered]@{
        partitionCatalogRef = [string] $partitionRef.path
        partitionCatalogRevision = [string] $partitionCatalog.revision
        partitionCatalogHashSha256 = File-Hash ([string] $partitionRef.path)
        overlayCatalogRef = [string] $overlayRef.path
        overlayCatalogRevision = [string] $overlayCatalog.revision
        overlayCatalogHashSha256 = File-Hash ([string] $overlayRef.path)
        codeBindingCatalogRef = [string] $codeCatalogRef.path
        codeBindingCatalogRevision = [string] $codeCatalog.revision
        codeBindingCatalogHashSha256 = File-Hash ([string] $codeCatalogRef.path)
    }
    sourceCatalogSnapshot = [ordered]@{
        actualE5Revision = [string] $actual.revision
        actualE5PolicyRevision = [string] $actual.policyRevision
        areaSetCount = [int] $actual.counts.areaSets
        graphCount = [int] $actual.counts.totalGraphs
        directBindingCount = [int] $actual.counts.directBindings
        contextualBindingCount = [int] $actual.counts.contextualBindings
        actualE5RuntimeValidated = [bool] $actual.authorityBoundary.runtimeValidated
        worldInteractionRevision = [string] $wiCatalog.revision
        worldInteractionCount = @($wiCatalog.items).Count
    }
    counts = [ordered]@{
        plans = 1
        nodes = $nodes.Count
        edges = $edges.Count
        constraints = $constraints.Count
        traversalProfiles = $traversalProfiles.Count
        subgraphs = $subgraphs.Count
        ports = $portsById.Count
        connectors = $connectors.Count
        overlays = $overlays.Count
        codeBindings = $codeBindings.Count
        sourceCodeFiles = $uniqueSourceFiles.Count
        codeBoundLevel1Targets = $boundLevel1.Count
        unboundLevel1Targets = $unboundTargets.Count
        unresolvedNodes = $unresolvedNodeCount
        unresolvedEdges = $unresolvedEdgeCount
    }
    plan = $plan
    partitionCatalog = $partitionCatalog
    overlayCatalog = $overlayCatalog
    resolvedCodeBindings = @($codeBindings)
}

$json = Stable-Json $output
$markdownLines = [Collections.Generic.List[string]]::new()
$fence = ([string] [char]96) * 3
$markdownLines.Add('# Graph Map 계획 조회')
$markdownLines.Add('')
$markdownLines.Add('> 이 문서는 파일 기반 계획 그래프의 생성 조회다. ReferenceAvailable은 기존 실제 E5 공간 사본의 식별자를 확인했다는 뜻이며, 이번 작업의 Unity Scene 배치·Play Mode 이동·입력·결과 또는 E5/E6 승격 증거가 아니다.')
$markdownLines.Add('')
$markdownLines.Add("- 그래프 맵: $($plan.graphMapStableId)")
$markdownLines.Add("- 판본: $($plan.revision)")
$markdownLines.Add("- 원본: [$PlanPath](../../../$PlanPath)")
$markdownLines.Add("- 원본 SHA-256: $($output.sourcePlanHashSha256)")
$markdownLines.Add("- 기준 공간 사본: $($actual.revision) / AreaSet $($actual.counts.areaSets) / Graph $($actual.counts.totalGraphs) / 직접 결속 $($actual.counts.directBindings)")
$markdownLines.Add("- 기준 WI: $($wiCatalog.revision) / $(@($wiCatalog.items).Count)개")
$markdownLines.Add("- federation: 하위 맵 $($subgraphs.Count) / port $($portsById.Count) / connector $($connectors.Count)")
$markdownLines.Add("- 이동 능력 프로필: $($traversalProfiles.Count) / 오버레이 $($overlays.Count)")
$markdownLines.Add("- 레벨 3 코드 결속: $($codeBindings.Count) / 소스 파일 $($uniqueSourceFiles.Count) / 실제 결속 미검증 대상 $($unboundTargets.Count)")
$markdownLines.Add("- 이번 실제 Runtime 검증: false")
$markdownLines.Add('')
$markdownLines.Add('## 규모 계층 — 하위 맵과 연결 포트')
$markdownLines.Add('')
$markdownLines.Add('| 하위 맵 | 책임 | 노드 | 내부 엣지 | 제약 | 포트 |')
$markdownLines.Add('| --- | --- | ---: | ---: | ---: | ---: |')
foreach ($subgraph in $subgraphs) {
    $markdownLines.Add("| $(Escape-Cell $subgraph.subgraphId)<br>$(Escape-Cell $subgraph.label) | $(Escape-Cell $subgraph.ownerCode) / $(Escape-Cell $subgraph.scopeCode) | $(@($subgraph.nodeRefs).Count) | $(@($subgraph.internalEdgeRefs).Count) | $(@($subgraph.constraintRefs).Count) | $(@($subgraph.ports).Count) |")
}
$markdownLines.Add('')
$markdownLines.Add('| connector | from → to | Graph Map 엣지 | 필요 능력 | 상태 |')
$markdownLines.Add('| --- | --- | --- | --- | --- |')
foreach ($connector in $connectors) {
    $markdownLines.Add("| $(Escape-Cell $connector.connectorId) | $(Escape-Cell $connector.fromPortRef) → $(Escape-Cell $connector.toPortRef) | $(Escape-Cell $connector.edgeRef) | $(Escape-Cell (@($connector.requiredCapabilityCodes) -join ', ')) | $(Escape-Cell $connector.stateCode) |")
}
$markdownLines.Add('')
$markdownLines.Add('## 레벨 1 — 플레이 관계')
$markdownLines.Add('')
$markdownLines.Add($fence + 'mermaid')
$markdownLines.Add('flowchart LR')
for ($i = 0; $i -lt $nodes.Count; $i++) {
    $node = $nodes[$i]
    $shape = if ([string] $node.stateCode -eq 'Unresolved') { "N$i{{""$($node.label)<br/>미해결""}}" } else { "N$i[""$($node.label)""]" }
    $markdownLines.Add("    $shape")
}
for ($i = 0; $i -lt $edges.Count; $i++) {
    $edge = $edges[$i]
    $fromIndex = [array]::IndexOf([object[]] $nodes, @($nodes | Where-Object nodeId -eq $edge.fromNodeId)[0])
    $toIndex = [array]::IndexOf([object[]] $nodes, @($nodes | Where-Object nodeId -eq $edge.toNodeId)[0])
    $arrow = if ([string] $edge.stateCode -eq 'Unresolved') { '-.->' } elseif ([bool] $edge.bidirectional) { '<-->' } else { '-->' }
    $markdownLines.Add("    N$fromIndex $arrow|$($edge.kindCode)| N$toIndex")
}
$markdownLines.Add($fence)
$markdownLines.Add('')
$markdownLines.Add('| 노드 | 역할 | 실현 상태 | WI | 실제 공간 참조 |')
$markdownLines.Add('| --- | --- | --- | --- | --- |')
foreach ($node in $nodes) {
    $actualRef = if ($null -eq $node.actualRef) { '없음' } else { "$(Escape-Cell $node.actualRef.graphStableId)<br>$(Escape-Cell $node.actualRef.nodeStableId)" }
    $markdownLines.Add("| $(Escape-Cell $node.nodeId)<br>$(Escape-Cell $node.label) | $(Escape-Cell $node.roleCode) | $(Escape-Cell $node.realizationCode) / $(Escape-Cell $node.stateCode) | $(Escape-Cell (@($node.worldInteractionIds) -join ', ')) | $actualRef |")
}
$markdownLines.Add('')
$markdownLines.Add('| 엣지 | 종류·의도 | 이동 능력 | 상태 | 방향 | 이유 |')
$markdownLines.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($edge in $edges) {
    $direction = if ([bool] $edge.bidirectional) { '양방향' } else { '단방향' }
    $markdownLines.Add("| $(Escape-Cell $edge.edgeId)<br>$(Escape-Cell $edge.fromNodeId) → $(Escape-Cell $edge.toNodeId) | $(Escape-Cell $edge.kindCode) / $(Escape-Cell $edge.intentionCode) | $(Escape-Cell $edge.capabilityProfileRef) | $(Escape-Cell $edge.stateCode) | $direction | $(Escape-Cell $edge.reason) |")
}
$markdownLines.Add('')
$markdownLines.Add('### 이동 능력 프로필')
$markdownLines.Add('')
$markdownLines.Add('| 프로필 | Actor | 화물 | 차량 | 권위 근거 | 귀환 정책 |')
$markdownLines.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($profile in $traversalProfiles) {
    $markdownLines.Add("| $(Escape-Cell $profile.profileId)<br>$(Escape-Cell $profile.label) | $(Escape-Cell (@($profile.allowedActorModes) -join ', ')) | $(Escape-Cell $profile.cargoModeCode) | $(Escape-Cell $profile.vehicleModeCode) | $([bool] $profile.requiresAuthorityEvidence) | $(Escape-Cell $profile.returnPolicyCode) |")
}
$markdownLines.Add('')
$markdownLines.Add('## 레벨 2 — 배치 전 제약')
$markdownLines.Add('')
$markdownLines.Add('| 제약 | 분류 | 심각도 | 집행 | 필요 E | 실패 코드 | 규칙 |')
$markdownLines.Add('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($constraint in $constraints) {
    $markdownLines.Add("| $(Escape-Cell $constraint.constraintId) | $(Escape-Cell $constraint.categoryCode) | $(Escape-Cell $constraint.severityCode) | $(Escape-Cell $constraint.enforcementCode) | $(Escape-Cell $constraint.requiredAtEvidence) | $(Escape-Cell $constraint.failureCode) | $(Escape-Cell $constraint.rule) |")
}
$markdownLines.Add('')
$markdownLines.Add('### 시간·날씨 오버레이')
$markdownLines.Add('')
$markdownLines.Add('| 오버레이 | 계기 | 대상 하위 맵 | 효과 범주 | 토폴로지·권위 변경 |')
$markdownLines.Add('| --- | --- | --- | --- | --- |')
foreach ($overlay in $overlays) {
    $markdownLines.Add("| $(Escape-Cell $overlay.overlayId)<br>$(Escape-Cell $overlay.label) | $(Escape-Cell $overlay.triggerKindCode) | $(Escape-Cell (@($overlay.targetSubgraphRefs) -join ', ')) | $(Escape-Cell (@($overlay.effectCategories) -join ', ')) | false / false |")
}
$markdownLines.Add('')
$markdownLines.Add('## 레벨 3 — Unity 코드·Component 결속')
$markdownLines.Add('')
$markdownLines.Add('> 레벨 3은 코드 본문을 복제하지 않는다. 공용 코드 결속 대장에서 파일·assembly·SHA-256·심볼을 한 번만 관리하고, 이 맵은 대상 selector만 소유한다. SourceAndSymbolVerified는 Scene wiring, Play Mode 실행, Game View 또는 E5 성립을 뜻하지 않는다.')
$markdownLines.Add('')
$markdownLines.Add("- 코드 대장: $($codeCatalogRef.path) / $($codeCatalog.revision) / SHA-256 $(File-Hash ([string] $codeCatalogRef.path))")
foreach ($sourceRoot in $sourceRoots) {
    $markdownLines.Add("- 소스 루트 $($sourceRoot.sourceRootCode): 관측 HEAD $($sourceRoot.observedRepositoryHead) / canonical Scene $($sourceRoot.canonicalScenePath) / Scene SHA-256 $($sourceRoot.canonicalSceneSha256)")
}
foreach ($binding in $codeBindings) {
    $markdownLines.Add('')
    $markdownLines.Add("### $($binding.label)")
    $markdownLines.Add('')
    $markdownLines.Add("- 결속 ID: $($binding.bindingId)")
    $markdownLines.Add("- 단계·사용·관계: $($binding.bindingStageCode) / $($binding.runtimeUseCode) / $($binding.relationshipCode)")
    $markdownLines.Add("- 대상 선택: $($binding.targetSelectorCode) / $(@($binding.targetRefs).Count)개")
    $markdownLines.Add("- 대상: $(@($binding.targetRefs) -join ', ')")
    $markdownLines.Add('')
    $markdownLines.Add('| assembly | 소유 | 파일 | 심볼 |')
    $markdownLines.Add('| --- | --- | --- | --- |')
    foreach ($file in @($binding.files)) {
        $markdownLines.Add("| $(Escape-Cell $file.assemblyName) | $(Escape-Cell $file.ownerCode) | $(Escape-Cell $file.path) | $(Escape-Cell (@($file.symbols) -join ', ')) |")
    }
}
$markdownLines.Add('')
$markdownLines.Add('### 아직 Unity 코드와 결속하지 않은 대상')
$markdownLines.Add('')
$markdownLines.Add('| 대상 | 사유 |')
$markdownLines.Add('| --- | --- |')
foreach ($unbound in $unboundTargets) {
    $markdownLines.Add("| $(Escape-Cell $unbound.targetRef) | $(Escape-Cell $unbound.reasonCode) — $(Escape-Cell $unbound.reason) |")
}
$markdownLines.Add('')
$markdownLines.Add('## 현재 미해결')
$markdownLines.Add('')
$markdownLines.Add("- 미해결 노드: $unresolvedNodeCount")
$markdownLines.Add("- 미해결 엣지: $unresolvedEdgeCount")
$markdownLines.Add('- 요동성 방비 관문은 기획 방향만 있으며 실제 WI·AreaSet·Graph·경로가 없다.')
$markdownLines.Add('- 최신 공간 사본 자체가 runtimeValidated=false이므로 실제 이동·Collider·Game View 근거로 확대하지 않는다.')
$markdownLines.Add('- Synty 후보, 지면·통로 실측, InteractionAnchor, 입력·결과, 적용·해제는 후속 작은 실행 범위에서 별도 검증한다.')
$markdown = Normalize-Text ($markdownLines -join [char]10)
$jsonPath = Resolve-RepoPath $JsonOutputPath
$markdownPath = Resolve-RepoPath $MarkdownOutputPath
if ($Mode -eq 'Write') {
    $null = New-Item -ItemType Directory -Force -Path (Split-Path $jsonPath -Parent)
    $null = New-Item -ItemType Directory -Force -Path (Split-Path $markdownPath -Parent)
    [IO.File]::WriteAllText($jsonPath, $json, $utf8)
    [IO.File]::WriteAllText($markdownPath, $markdown, $utf8)
}
else {
    Require (Test-Path -LiteralPath $jsonPath) 'GeneratedJsonMissing'
    Require (Test-Path -LiteralPath $markdownPath) 'GeneratedMarkdownMissing'
    $existingJson = Normalize-Text (Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8)
    $existingMarkdown = Normalize-Text (Get-Content -LiteralPath $markdownPath -Raw -Encoding UTF8)
    Require ($existingJson -ceq $json) 'GeneratedJsonStale'
    Require ($existingMarkdown -ceq $markdown) 'GeneratedMarkdownStale'
}

Write-Output "Graph Map plan $Mode passed: nodes=$($nodes.Count), edges=$($edges.Count), constraints=$($constraints.Count), subgraphs=$($subgraphs.Count), ports=$($portsById.Count), connectors=$($connectors.Count), overlays=$($overlays.Count), codeBindings=$($codeBindings.Count), sourceFiles=$($uniqueSourceFiles.Count), unresolved=$unresolvedNodeCount/$unresolvedEdgeCount"
