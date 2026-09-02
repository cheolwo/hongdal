$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager = Join-Path $root 'eng/world-seedbeds/manage-graph-map-plans.ps1'
$source = Join-Path $root 'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json'
$partitionSource = Join-Path $root 'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.partitions.v1.json'
$overlaySource = Join-Path $root 'eng/world-seedbeds/graph-maps/graph-map-overlays.v1.json'
$codeSource = Join-Path $root 'eng/world-seedbeds/graph-maps/unity-code-bindings.v1.json'
$placementRuleSource = Join-Path $root 'eng/world-seedbeds/graph-maps/placement-rule-bindings.v1.json'
$normalizationSource = Join-Path $root 'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.normalization.v1.json'
$unityRoot = Join-Path ([Environment]::GetFolderPath('UserProfile')) 'ssalddel'
$folderRef = 'artifacts/local/validation/graph-map-plans'
$folder = Join-Path $root $folderRef
$utf8 = [Text.UTF8Encoding]::new($false)
$null = New-Item -ItemType Directory -Force -Path $folder
$checks = 0

function Assert([bool] $condition, [string] $code) {
    if (-not $condition) { throw "GraphMapTestFailed:$code" }
    $script:checks++
}

function Read-JsonFile([string] $path) {
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Clone-Json([object] $value) {
    return ($value | ConvertTo-Json -Depth 100) | ConvertFrom-Json
}

$officialPlan = Read-JsonFile $source
$officialPartition = Read-JsonFile $partitionSource
$officialOverlay = Read-JsonFile $overlaySource
$officialCode = Read-JsonFile $codeSource
$officialPlacementRules = Read-JsonFile $placementRuleSource
$officialNormalization = Read-JsonFile $normalizationSource

function New-State {
    return [pscustomobject]@{
        plan = Clone-Json $officialPlan
        partition = Clone-Json $officialPartition
        overlay = Clone-Json $officialOverlay
        code = Clone-Json $officialCode
        placementRules = Clone-Json $officialPlacementRules
        normalization = Clone-Json $officialNormalization
    }
}

function Save-State([object] $state, [string] $name) {
    $planRef = "$folderRef/$name.plan.json"
    $partitionRef = "$folderRef/$name.partitions.json"
    $overlayRef = "$folderRef/$name.overlays.json"
    $codeRef = "$folderRef/$name.code-bindings.json"
    $placementRuleRef = "$folderRef/$name.placement-rules.json"
    $normalizationRef = "$folderRef/$name.normalization.json"

    $state.plan.federation.partitionCatalogRef.path = $partitionRef
    $state.plan.federation.overlayCatalogRef.path = $overlayRef
    $state.plan.level3.codeBindingCatalogRef.path = $codeRef
    $state.plan.level2.placementRuleCatalogRef.path = $placementRuleRef
    $state.plan.level1.normalizationCatalogRef.path = $normalizationRef
    $state.partition.sourceElementCatalogRef.path = $planRef
    $state.partition.sourceElementCatalogRef.expectedRevision = $state.plan.revision

    $items = @(
        @($planRef, $state.plan),
        @($partitionRef, $state.partition),
        @($overlayRef, $state.overlay),
        @($codeRef, $state.code),
        @($placementRuleRef, $state.placementRules),
        @($normalizationRef, $state.normalization)
    )
    foreach ($item in $items) {
        $path = Join-Path $root ([string] $item[0] -replace '/', [IO.Path]::DirectorySeparatorChar)
        [IO.File]::WriteAllText($path, ($item[1] | ConvertTo-Json -Depth 100), $utf8)
    }
    return $planRef
}

function Invoke-Fixture([string] $planRef, [string] $name) {
    $parameters = @{
        Mode = 'Write'
        PlanPath = $planRef
        JsonOutputPath = "$folderRef/$name.output.json"
        MarkdownOutputPath = "$folderRef/$name.output.md"
        UnityProjectRoot = $unityRoot
        VerifyUnitySources = $true
    }
    return & $manager @parameters
}

function Reject([scriptblock] $action, [string] $expectedCode) {
    $message = ''
    try { & $action | Out-Null }
    catch { $message = $_.Exception.Message }
    Assert (-not [string]::IsNullOrWhiteSpace($message)) "RejectDidNotFail:$expectedCode"
    Assert ($message -match [regex]::Escape($expectedCode)) "RejectWrongReason:$($expectedCode):$message"
}

$protected = @(
    'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json',
    'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.partitions.v1.json',
    'eng/world-seedbeds/graph-maps/graph-map-overlays.v1.json',
    'eng/world-seedbeds/graph-maps/unity-code-bindings.v1.json',
    'eng/world-seedbeds/graph-maps/placement-rule-bindings.v1.json',
    'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.normalization.v1.json',
    'eng/world-seedbeds/generated/actual-e5-spatial.v1.json',
    'eng/execution-ledgers/world-interactions.json',
    'docs/AI/DECISIONS.md',
    'eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/area-set.json',
    'Ssalddel.Simulation.Application/Simulation경관조합검토Service.cs',
    'Ssalddel.Simulation.Application/Simulation배치적합성검사.cs'
)
$before = @{}
foreach ($relative in $protected) {
    $before[$relative] = (Get-FileHash -LiteralPath (Join-Path $root ($relative -replace '/', [IO.Path]::DirectorySeparatorChar))).Hash
}

$protectedUnity = @(
    'Assets/Ssalddel/Scenes/SimulationWorldShell.unity',
    'Assets/Ssalddel/Runtime/World/실제E5AreaSetNetworkModels.cs',
    'Assets/Ssalddel/Runtime/World/실제E5AreaSetNetworkStreaming.cs',
    'Assets/Ssalddel/Presentation/World/실제E5AreaSetNetworkController.cs',
    'Assets/Ssalddel/Presentation/World/실제E5AreaSetNetworkHudPresenter.cs',
    'Assets/Ssalddel/Presentation/World/공간문법LandscapeRuntimeAssembler.cs',
    'Assets/Ssalddel/Presentation/World/공간TileStreamingController.cs',
    'Assets/Ssalddel/Presentation/World/WorldVisualCatalog.cs',
    'Assets/Ssalddel/Presentation/World/WorldVisualInstanceView.cs',
    'Assets/Ssalddel/Runtime/World/공간실외자산배치Planning.cs',
    'Assets/Ssalddel/Runtime/World/공간LHWorldModels.cs',
    'Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs',
    'Assets/Ssalddel/Editor/WI공간모판검토실Builder.cs',
    'Assets/Ssalddel/Editor/H공간배치규칙EditorEngine.cs'
)
$beforeUnity = @{}
foreach ($relative in $protectedUnity) {
    $path = Join-Path $unityRoot ($relative -replace '/', [IO.Path]::DirectorySeparatorChar)
    Assert (Test-Path -LiteralPath $path -PathType Leaf) "UnityProtectedMissing:$relative"
    $beforeUnity[$relative] = (Get-FileHash -LiteralPath $path).Hash
}

$official = & $manager -Mode Check -UnityProjectRoot $unityRoot -VerifyUnitySources
Assert ($official -match 'nodes=34, edges=34, constraints=28, placementRules=21, ruleBoundConstraints=3, subgraphs=6, ports=12, connectors=6, overlays=9, codeBindings=6, sourceFiles=13, unresolved=23/25') 'OfficialCheck'

$state = New-State
$validRef = Save-State $state 'valid'
$validResult = Invoke-Fixture $validRef 'valid'
Assert ($validResult -match 'Write passed') 'ValidFixture'

$output = Read-JsonFile (Join-Path $folder 'valid.output.json')
Assert ($output.schemaVersion -eq 'mirror-graph-map-plan-output.v3') 'OutputSchema'
Assert ($output.counts.nodes -eq 34 -and $output.counts.edges -eq 34 -and $output.counts.constraints -eq 28) 'OutputCounts'
Assert ($output.counts.placementRuleProfiles -eq 5 -and $output.counts.placementRules -eq 21) 'PlacementRuleOutputCounts'
Assert ($output.counts.placementRuleBindings -eq 5 -and $output.counts.placementRuleBoundConstraints -eq 3 -and $output.counts.governanceOnlyConstraints -eq 25) 'PlacementRuleBindingCounts'
Assert ($output.counts.subgraphs -eq 6 -and $output.counts.ports -eq 12 -and $output.counts.connectors -eq 6) 'FederationOutputCounts'
Assert ($output.counts.traversalProfiles -eq 6 -and $output.counts.layers -eq 6 -and $output.counts.overlays -eq 9 -and $output.counts.overlayEdgeEffects -eq 4) 'CapabilityOverlayCounts'
Assert ($output.counts.planningAssessments -eq 21) 'PlanningAssessmentCount'
Assert ($output.counts.codeBindings -eq 6 -and $output.counts.sourceCodeFiles -eq 13) 'Level3OutputCounts'
Assert ($output.counts.codeBoundLevel1Targets -eq 19 -and $output.counts.unboundLevel1Targets -eq 48) 'Level3CoverageCounts'
Assert (@($output.resolvedCodeBindings | ForEach-Object targetRefs | Where-Object { $_ -eq 'gm-node:first-logging-reflection-preparation' }).Count -eq 0) 'PlannedNodeNotAutoBound'
Assert ($output.counts.normalizedElements -eq 9 -and $output.counts.normalizedRelations -eq 7 -and $output.counts.normalizationBlocked -eq 1) 'NormalizationCounts'
Assert (-not $output.sourceCatalogSnapshot.actualE5RuntimeValidated) 'RuntimeBoundaryPreserved'
Assert (-not $output.plan.authorityBoundary.worldApplied -and -not $output.plan.authorityBoundary.actualTraversalVerified) 'WorldBoundaryPreserved'
Assert (-not $output.plan.level3.evidenceBoundary.sceneWiringVerified -and -not $output.plan.level3.evidenceBoundary.runtimeExecutionVerified) 'Level3RuntimeBoundaryPreserved'
Assert (@($output.partitionCatalog.subgraphs).Count -eq 6 -and @($output.resolvedCodeBindings).Count -eq 6) 'ExpandedCatalogsPresent'
Assert (@($output.overlayCatalog.layers).Count -eq 6 -and @($output.overlayCatalog.overlays).Count -eq 9) 'LayeredOverlayCatalogPresent'
Assert (@($output.plan.planningImpactAssessments | Where-Object classificationCode -eq 'UpdateExisting').Count -eq 8) 'PlanningUpdateExistingCount'
Assert (@($output.plan.planningImpactAssessments | Where-Object classificationCode -eq 'CreateSubgraph').Count -eq 2) 'PlanningCreateSubgraphCount'
Assert (@($output.plan.planningImpactAssessments | Where-Object classificationCode -eq 'CreateGraphMap').Count -eq 0) 'PlanningCreateGraphMapCount'
Assert (@($output.plan.planningImpactAssessments | Where-Object classificationCode -eq 'Blocked').Count -eq 2) 'PlanningBlockedCount'
Assert (@($output.plan.planningImpactAssessments | Where-Object classificationCode -eq 'NoImpact').Count -eq 9) 'PlanningNoImpactCount'
Assert (@($output.placementRuleCatalog.areaRuleProfiles).Count -eq 5 -and @($output.resolvedPlacementRuleBindings).Count -eq 5) 'PlacementRuleCatalogPresent'
Assert ($output.normalizationCatalog.sampleStableId -eq 'graph-map-normalization-sample:hans-farm.v1') 'NormalizationCatalogPresent'
$maximumLine = (Get-Content -LiteralPath (Join-Path $folder 'valid.output.md') -Encoding UTF8 | ForEach-Object Length | Measure-Object -Maximum).Maximum
Assert ($maximumLine -le 1200) 'GeneratedMarkdownLineLimit'

$secondResult = Invoke-Fixture $validRef 'valid-second'
Assert ($secondResult -match 'Write passed') 'SecondWrite'
Assert ((Get-FileHash (Join-Path $folder 'valid.output.json')).Hash -eq (Get-FileHash (Join-Path $folder 'valid-second.output.json')).Hash) 'DeterministicJson'
Assert ((Get-FileHash (Join-Path $folder 'valid.output.md')).Hash -eq (Get-FileHash (Join-Path $folder 'valid-second.output.md')).Hash) 'DeterministicMarkdown'

$state = New-State
$state.plan.level1.nodes += @($state.plan.level1.nodes[0])
Reject { Invoke-Fixture (Save-State $state 'duplicate-node') 'duplicate-node' } 'NodeDuplicate'

$state = New-State
$state.plan.level1.edges[0].toNodeId = 'gm-node:missing'
Reject { Invoke-Fixture (Save-State $state 'missing-endpoint') 'missing-endpoint' } 'EdgeToUnknown'

$state = New-State
$state.plan.level1.nodes[0].worldInteractionIds += @('WI-NOT-REGISTERED')
Reject { Invoke-Fixture (Save-State $state 'unknown-wi') 'unknown-wi' } 'NodeUnknownWi'

$state = New-State
@($state.plan.level1.nodes | Where-Object nodeId -eq 'gm-node:yodong-defense-gateway')[0].stateCode = 'ReferenceAvailable'
Reject { Invoke-Fixture (Save-State $state 'planning-gateway-promoted') 'planning-gateway-promoted' } 'NodePlanningState'

$state = New-State
$state.plan.level1.nodes[0].planningContext.PSObject.Properties.Remove('result')
Reject { Invoke-Fixture (Save-State $state 'missing-context') 'missing-context' } 'NodeContextMissing'

$state = New-State
$state.plan.level1.edges[0].bidirectional = $false
Reject { Invoke-Fixture (Save-State $state 'required-no-return') 'required-no-return' } 'RequiredTraversalWithoutReturn'

$state = New-State
$state.plan.authorityBoundary.worldApplied = $true
Reject { Invoke-Fixture (Save-State $state 'world-applied') 'world-applied' } 'AuthorityWorldApplied'

$state = New-State
@($state.plan.level1.edges | Where-Object edgeId -eq 'gm-edge:farm-edge-to-production')[0].sourceRelationRefs = @()
Reject { Invoke-Fixture (Save-State $state 'reference-without-source') 'reference-without-source' } 'EdgeReferenceMissing'

$state = New-State
$state.plan.level1.nodes[0].actualRef.graphStableId = 'landscape-graph:sim:pyeongchang:highland-farm.v1'
Reject { Invoke-Fixture (Save-State $state 'node-graph-mismatch') 'node-graph-mismatch' } 'NodeActualAreaGraphMismatch'

$state = New-State
$state.plan.level2.constraints[0].targetRefs = @('gm-node:not-found')
Reject { Invoke-Fixture (Save-State $state 'constraint-target') 'constraint-target' } 'ConstraintTargetUnknown'

$state = New-State
@($state.plan.level1.edges | Where-Object edgeId -eq 'gm-edge:hub-outbound-to-yodong-gateway')[0].stateCode = 'ReferenceAvailable'
Reject { Invoke-Fixture (Save-State $state 'unresolved-promoted') 'unresolved-promoted' } 'EdgeReferenceMissing'

$state = New-State
$state.plan.level1.edges[0].capabilityProfileRef = 'gm-capability:not-found'
Reject { Invoke-Fixture (Save-State $state 'edge-capability') 'edge-capability' } 'EdgeCapabilityProfileUnknown'

$state = New-State
$state.plan.level2.constraints[0].enforcementCode = 'Unknown'
Reject { Invoke-Fixture (Save-State $state 'constraint-enforcement') 'constraint-enforcement' } 'ConstraintEnforcement'

$state = New-State
$state.normalization.elements += @($state.normalization.elements[0])
Reject { Invoke-Fixture (Save-State $state 'normalization-element-duplicate') 'normalization-element-duplicate' } 'NormalizationElementDuplicate'

$state = New-State
@($state.normalization.elements | Where-Object elementRef -eq 'gm-node:hans-first-trust')[0].compatibilityTargetRefs = @('gm-node:hans')
Reject { Invoke-Fixture (Save-State $state 'normalization-alias-relation-missing') 'normalization-alias-relation-missing' } 'NormalizationAliasRelationMissing'

$state = New-State
@($state.normalization.elements | Where-Object elementRef -eq 'gm-node:hans')[0].actorIdentityCode = 'AnonymousRole'
Reject { Invoke-Fixture (Save-State $state 'normalization-anonymous-actor') 'normalization-anonymous-actor' } 'NormalizationAnonymousActorMaterialized'

$state = New-State
@($state.normalization.relations | Where-Object relationRef -eq 'gm-edge:hans-normalized-witnesses-fence-repair')[0].actorRoleRef = 'FarmWorker'
Reject { Invoke-Fixture (Save-State $state 'normalization-actor-exclusive') 'normalization-actor-exclusive' } 'NormalizationRelationActorExclusive'

$state = New-State
@($state.normalization.relations | Where-Object relationRef -eq 'gm-edge:hans-normalized-witnesses-fence-repair')[0].sourceDecisionIds = @('D-529')
Reject { Invoke-Fixture (Save-State $state 'normalization-decision-lineage') 'normalization-decision-lineage' } 'NormalizationDecisionLineageLost'

$state = New-State
@($state.normalization.relations | Where-Object relationRef -eq 'gm-edge:hans-normalized-repairs-house')[0].contextSourceNodeRef = 'gm-node:not-found'
Reject { Invoke-Fixture (Save-State $state 'normalization-context-source') 'normalization-context-source' } 'NormalizationContextSourceUnknown'

$state = New-State
@($state.normalization.relations | Where-Object relationRef -eq 'gm-edge:hans-normalized-harvests-permitted-tree')[0].level3TargetRefs = @('gm-node:not-found')
Reject { Invoke-Fixture (Save-State $state 'normalization-level3-target') 'normalization-level3-target' } 'NormalizationLevel3TargetUnknown'

$state = New-State
$state.placementRules.areaRuleProfiles[1].rules[0].sourceRuleCode = 'InventedFarmRule'
Reject { Invoke-Fixture (Save-State $state 'placement-source-drift') 'placement-source-drift' } 'PlacementRuleSourceCodeDrift'

$state = New-State
$state.placementRules.areaRuleProfiles[1].rules[4].constraintRefs[0] = 'gm-constraint:not-found'
Reject { Invoke-Fixture (Save-State $state 'placement-constraint-unknown') 'placement-constraint-unknown' } 'PlacementRuleConstraintUnknown'

$state = New-State
$state.placementRules.areaRuleProfiles[3].rules[3].constraintRefs[0] = 'gm-constraint:farm-flow-separation'
Reject { Invoke-Fixture (Save-State $state 'placement-area-mismatch') 'placement-area-mismatch' } 'PlacementRuleConstraintAreaMismatch'

$state = New-State
$state.placementRules.governanceOnlyConstraints = @($state.placementRules.governanceOnlyConstraints | Where-Object constraintRef -ne 'gm-constraint:season-does-not-rewrite-topology')
Reject { Invoke-Fixture (Save-State $state 'placement-coverage-missing') 'placement-coverage-missing' } 'PlacementRuleConstraintCoverage'

$state = New-State
$state.placementRules.areaRuleProfiles[4].graphPresenceCode = 'Present'
Reject { Invoke-Fixture (Save-State $state 'placement-presence') 'placement-presence' } 'PlacementRuleAreaPresenceMismatch'

$state = New-State
$state.partition.subgraphs[1].nodeRefs += @('gm-node:nature-trailhead')
Reject { Invoke-Fixture (Save-State $state 'partition-node-duplicate') 'partition-node-duplicate' } 'SubgraphNodeOwnershipDuplicate'

$state = New-State
$state.partition.subgraphs[0].nodeRefs = @('gm-node:nature-trailhead')
Reject { Invoke-Fixture (Save-State $state 'partition-node-missing') 'partition-node-missing' } 'SubgraphInternalEdgeEndpointOutside'

$state = New-State
$state.partition.subgraphs[0].internalEdgeRefs += @('gm-edge:farm-production-to-work-yard')
Reject { Invoke-Fixture (Save-State $state 'partition-internal-edge') 'partition-internal-edge' } 'SubgraphInternalEdgeEndpointOutside'

$state = New-State
$state.partition.connectors[0].fromPortRef = 'gm-port:hub-outbound:to-town'
Reject { Invoke-Fixture (Save-State $state 'connector-node-mismatch') 'connector-node-mismatch' } 'ConnectorFromNodeMismatch'

$state = New-State
$state.partition.connectors[1].requiredCapabilityCodes += @('NotSupported')
Reject { Invoke-Fixture (Save-State $state 'connector-capability') 'connector-capability' } 'ConnectorCapabilityMissingFrom'

$state = New-State
$state.partition.connectors[0].stateCode = 'Unresolved'
Reject { Invoke-Fixture (Save-State $state 'connector-state') 'connector-state' } 'ConnectorStateMismatch'

$state = New-State
$state.partition.federationConstraintRefs += @('gm-constraint:farm-flow-separation')
Reject { Invoke-Fixture (Save-State $state 'partition-constraint-duplicate') 'partition-constraint-duplicate' } 'FederationConstraintOwnershipDuplicate'

$state = New-State
$state.partition.federationConstraintRefs = @($state.partition.federationConstraintRefs | Where-Object { $_ -ne 'gm-constraint:actual-reference-identity' })
Reject { Invoke-Fixture (Save-State $state 'partition-constraint-missing') 'partition-constraint-missing' } 'SubgraphConstraintCoverageMissing'

$state = New-State
$state.partition.splitPolicy.hardNodeLimit = 1
Reject { Invoke-Fixture (Save-State $state 'partition-hard-limit') 'partition-hard-limit' } 'SubgraphNodeHardLimit'

$state = New-State
$state.overlay.overlays[0].topologyMutationAllowed = $true
Reject { Invoke-Fixture (Save-State $state 'overlay-topology') 'overlay-topology' } 'OverlayTopologyMutation'

$state = New-State
$state.overlay.overlays[0].targetSubgraphRefs[0] = 'gm-subgraph:not-found'
Reject { Invoke-Fixture (Save-State $state 'overlay-subgraph') 'overlay-subgraph' } 'OverlaySubgraphUnknown'

$state = New-State
$state.overlay.overlays[0].sourceDecisionIds[0] = 'D-000'
Reject { Invoke-Fixture (Save-State $state 'overlay-decision') 'overlay-decision' } 'OverlayDecisionUnknown'

$state = New-State
$state.overlay.overlays[0].layerRef = 'gm-layer:not-found'
Reject { Invoke-Fixture (Save-State $state 'overlay-layer') 'overlay-layer' } 'OverlayLayerUnknown'

$state = New-State
$state.overlay.overlays[0].targetEdgeRefs = @('gm-edge:not-found')
Reject { Invoke-Fixture (Save-State $state 'overlay-edge') 'overlay-edge' } 'OverlayEdgeUnknown'

$state = New-State
$effectOverlay = @($state.overlay.overlays | Where-Object { $null -ne $_.PSObject.Properties['edgeEffects'] })[0]
$effectOverlay.edgeEffects[0].routeStateCode = 'Imagined'
Reject { Invoke-Fixture (Save-State $state 'overlay-route-state') 'overlay-route-state' } 'OverlayRouteState'

$state = New-State
$effectOverlay = @($state.overlay.overlays | Where-Object { $null -ne $_.PSObject.Properties['edgeEffects'] })[0]
$effectOverlay.edgeEffects[0].costComponents[0].value = 1
Reject { Invoke-Fixture (Save-State $state 'overlay-unknown-cost-value') 'overlay-unknown-cost-value' } 'OverlayUnknownCostHasValue'

$state = New-State
$effectOverlays = @($state.overlay.overlays | Where-Object { $null -ne $_.PSObject.Properties['edgeEffects'] })
$effectOverlays[1].edgeEffects[0].costComponents[0].dimensionCode = $effectOverlays[0].edgeEffects[0].costComponents[0].dimensionCode
$effectOverlays[1].edgeEffects[0].costComponents[0].contributionKey = $effectOverlays[0].edgeEffects[0].costComponents[0].contributionKey
Reject { Invoke-Fixture (Save-State $state 'overlay-contribution-duplicate') 'overlay-contribution-duplicate' } 'OverlayCostContributionDuplicate'

$state = New-State
$effectOverlay = @($state.overlay.overlays | Where-Object { $null -ne $_.PSObject.Properties['edgeEffects'] })[0]
$effectOverlay.edgeEffects[0].evidence.sourceSha256 = ('0' * 64)
Reject { Invoke-Fixture (Save-State $state 'overlay-evidence-hash') 'overlay-evidence-hash' } 'OverlayEvidenceHashMismatch'

$state = New-State
$state.plan.planningImpactAssessments = @($state.plan.planningImpactAssessments | Select-Object -Skip 1)
Reject { Invoke-Fixture (Save-State $state 'planning-assessment-missing') 'planning-assessment-missing' } 'PlanningAssessmentCoverageCount'

$state = New-State
$state.plan.planningImpactAssessments[0].sourceExpectedSha256 = ('0' * 64)
Reject { Invoke-Fixture (Save-State $state 'planning-assessment-hash') 'planning-assessment-hash' } 'PlanningAssessmentSourceHash'

$state = New-State
$state.plan.level3.bindingAssignments = @($state.plan.level3.bindingAssignments | Select-Object -Skip 1)
Reject { Invoke-Fixture (Save-State $state 'level3-assignment-missing') 'level3-assignment-missing' } 'Level3AssignmentCoverageCount'

$state = New-State
$state.plan.level3.bindingAssignments[2].explicitTargetRefs[0] = 'gm-node:not-found'
Reject { Invoke-Fixture (Save-State $state 'level3-unknown-target') 'level3-unknown-target' } 'Level3TargetUnknown'

$state = New-State
$state.plan.level3.bindingAssignments[2].explicitTargetRefs[0] = 'gm-node:yodong-defense-gateway'
Reject { Invoke-Fixture (Save-State $state 'level3-unresolved-bound') 'level3-unresolved-bound' } 'Level3UnresolvedTargetBound'

$state = New-State
$state.plan.level3.bindingAssignments[0].explicitTargetRefs = @('gm-node:nature-trailhead')
Reject { Invoke-Fixture (Save-State $state 'level3-selector-explicit') 'level3-selector-explicit' } 'Level3SelectorExplicitTargets'

$state = New-State
$state.code.bindings[0].files[0].symbols[0] = '존재하지않는GraphMap검증심볼'
Reject { Invoke-Fixture (Save-State $state 'level3-missing-symbol') 'level3-missing-symbol' } 'Level3SourceSymbolMissing'

$state = New-State
$state.code.bindings[0].files[0].expectedSha256 = ('0' * 64)
Reject { Invoke-Fixture (Save-State $state 'level3-hash-drift') 'level3-hash-drift' } 'Level3SourceHashMismatch'

$state = New-State
$state.code.bindings[4].runtimeUseCode = 'Runtime'
Reject { Invoke-Fixture (Save-State $state 'level3-editor-as-runtime') 'level3-editor-as-runtime' } 'Level3RuntimeBindingUsesEditor'

$state = New-State
$state.plan.level3.unboundTargets = @($state.plan.level3.unboundTargets | Where-Object targetRef -ne 'gm-edge:hub-outbound-to-yodong-gateway')
Reject { Invoke-Fixture (Save-State $state 'level3-unbound-coverage') 'level3-unbound-coverage' } 'Level3UnboundCoverageMissing'

$state = New-State
$state.code.sourceRoots[0].canonicalSceneSha256 = ('F' * 64)
Reject { Invoke-Fixture (Save-State $state 'level3-scene-drift') 'level3-scene-drift' } 'Level3CanonicalSceneHashMismatch'

$state = New-State
$state.code.bindings[0].files[0] | Add-Member -NotePropertyName sourceText -NotePropertyValue '코드 본문을 Graph Map에 복제하지 않는다.'
Reject { Invoke-Fixture (Save-State $state 'level3-source-body') 'level3-source-body' } 'Level3FileSourceBodyForbidden'

$state = New-State
$state.code.bindings[0].files[0].assemblyName = ''
Reject { Invoke-Fixture (Save-State $state 'level3-assembly') 'level3-assembly' } 'Level3AssemblyName'

$state = New-State
$state.code.bindings[0].bindingStageCode = 'RuntimeInvented'
Reject { Invoke-Fixture (Save-State $state 'level3-binding-stage') 'level3-binding-stage' } 'Level3BindingStage'

$state = New-State
$state.plan.summary += ' changed'
$staleRef = Save-State $state 'stale'
Reject {
    & $manager -Mode Check -PlanPath $staleRef -JsonOutputPath "$folderRef/valid.output.json" -MarkdownOutputPath "$folderRef/valid.output.md"
} 'GeneratedJsonStale'

foreach ($relative in $protected) {
    $after = (Get-FileHash -LiteralPath (Join-Path $root ($relative -replace '/', [IO.Path]::DirectorySeparatorChar))).Hash
    Assert ($before[$relative] -eq $after) "ProtectedUnchanged:$relative"
}
foreach ($relative in $protectedUnity) {
    $path = Join-Path $unityRoot ($relative -replace '/', [IO.Path]::DirectorySeparatorChar)
    $after = (Get-FileHash -LiteralPath $path).Hash
    Assert ($beforeUnity[$relative] -eq $after) "UnityProtectedUnchanged:$relative"
}

$report = [ordered]@{
    status = 'Passed'
    checks = $checks
    scope = 'FileBasedPartitionedGraphMapPlanValidationOnly'
    editorUsed = $false
    sceneChanged = $false
    worldApplied = $false
    actualTraversalVerified = $false
    unitySourcesReadOnlyVerified = $true
    unityProjectRoot = $unityRoot
    managerSha256 = (Get-FileHash -LiteralPath $manager).Hash
    planSha256 = (Get-FileHash -LiteralPath $source).Hash
    partitionCatalogSha256 = (Get-FileHash -LiteralPath $partitionSource).Hash
    overlayCatalogSha256 = (Get-FileHash -LiteralPath $overlaySource).Hash
    codeBindingCatalogSha256 = (Get-FileHash -LiteralPath $codeSource).Hash
    placementRuleCatalogSha256 = (Get-FileHash -LiteralPath $placementRuleSource).Hash
    normalizationCatalogSha256 = (Get-FileHash -LiteralPath $normalizationSource).Hash
}
[IO.File]::WriteAllText((Join-Path $folder 'results.json'), ($report | ConvertTo-Json), $utf8)
Write-Output "Graph Map plan tests: $checks passed"
