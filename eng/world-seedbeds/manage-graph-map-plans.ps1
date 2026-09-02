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
. (Join-Path $PSScriptRoot 'GraphMapTooling.ps1') `
    -RepositoryRoot $repositoryRoot `
    -ErrorPrefix 'GraphMapInvalid'

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
$planningCatalogRef = @($catalogs | Where-Object kindCode -eq 'PlanningCatalog')
Require ($actualCatalogRef.Count -eq 1) 'ActualCatalogReference'
Require ($wiCatalogRef.Count -eq 1) 'WiCatalogReference'
Require ($decisionCatalogRef.Count -eq 1) 'DecisionCatalogReference'
Require ($planningCatalogRef.Count -eq 1) 'PlanningCatalogReference'

$actual = Read-Json ([string] $actualCatalogRef[0].path)
$wiCatalog = Read-Json ([string] $wiCatalogRef[0].path)
$decisionPath = Resolve-RepoPath ([string] $decisionCatalogRef[0].path)
$decisionText = Get-Content -LiteralPath $decisionPath -Raw -Encoding UTF8
$planningPath = Resolve-RepoPath ([string] $planningCatalogRef[0].path)
$planningText = Get-Content -LiteralPath $planningPath -Raw -Encoding UTF8
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
$planIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($match in [regex]::Matches($planningText, '`(PLAN-[A-Z0-9-]+)`')) {
    $null = $planIds.Add($match.Groups[1].Value)
}
$wiIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($wi in @($wiCatalog.items)) { $null = $wiIds.Add([string] $wi.id) }

$planningAssessments = @($plan.planningImpactAssessments)
Require ($planningAssessments.Count -eq $planIds.Count) 'PlanningAssessmentCoverageCount'
Require-Unique $planningAssessments { param($x) $x.planId } 'PlanningAssessmentDuplicate'
$assessmentPlanIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$allowedPlanningClassification = @('UpdateExisting', 'CreateSubgraph', 'CreateGraphMap', 'NoImpact', 'Blocked')
foreach ($assessment in $planningAssessments) {
    $planId = [string] $assessment.planId
    Require ($planIds.Contains($planId)) "PlanningAssessmentUnknown:$planId"
    $null = $assessmentPlanIds.Add($planId)
    Require ($allowedPlanningClassification -contains [string] $assessment.classificationCode) "PlanningAssessmentClassification:$planId"
    Require-Text $assessment.integrationStateCode "PlanningAssessmentState:$planId"
    Require-Text $assessment.sourceRef "PlanningAssessmentSource:$planId"
    Require-Text $assessment.sourceExpectedSha256 "PlanningAssessmentHash:$planId"
    Require-Text $assessment.reason "PlanningAssessmentReason:$planId"
    $assessmentSourcePath = Resolve-RepoPath ([string] $assessment.sourceRef)
    Require (Test-Path -LiteralPath $assessmentSourcePath -PathType Leaf) "PlanningAssessmentSourceMissing:$planId"
    Require ((Absolute-Hash $assessmentSourcePath) -eq ([string] $assessment.sourceExpectedSha256).ToUpperInvariant()) "PlanningAssessmentSourceHash:$planId"
}
foreach ($planId in $planIds) { Require ($assessmentPlanIds.Contains($planId)) "PlanningAssessmentMissing:$planId" }

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
    if ($null -ne $node.PSObject.Properties['sourcePlanIds']) {
        foreach ($planId in @($node.sourcePlanIds)) {
            Require ($planIds.Contains([string] $planId)) "NodeUnknownPlan:$nodeId`:$planId"
        }
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
        $validSource = $decisionIds.Contains($sourceText) -or $wiIds.Contains($sourceText) -or $planIds.Contains($sourceText)
        if (-not $validSource -and ($sourceText -match '^(docs|eng|Ssalddel\.)/')) {
            $validSource = Test-Path -LiteralPath (Resolve-RepoPath $sourceText)
        }
        Require $validSource "ConstraintSourceUnknown:$constraintId`:$sourceText"
    }
}

$placementRuleRef = $plan.level2.placementRuleCatalogRef
Require ($null -ne $placementRuleRef) 'PlacementRuleCatalogRefMissing'
$placementRuleCatalog = Read-Json ([string] $placementRuleRef.path)
Require ([string] $placementRuleCatalog.schemaVersion -eq 'mirror-graph-map-placement-rule-catalog.v1') 'PlacementRuleCatalogSchema'
Require ([string] $placementRuleCatalog.revision -eq [string] $placementRuleRef.expectedRevision) 'PlacementRuleCatalogRevision'
Require ([string] $placementRuleCatalog.graphMapStableId -eq [string] $plan.graphMapStableId) 'PlacementRuleGraphIdentity'

$placementBoundary = $placementRuleCatalog.authorityBoundary
Require ([bool] $placementBoundary.declarativePreflightOnly) 'PlacementRulePreflightBoundary'
Require (-not [bool] $placementBoundary.formsHInstances) 'PlacementRuleFormsHInstances'
Require (-not [bool] $placementBoundary.calculatesPlacementPlan) 'PlacementRuleCalculatesPlan'
Require (-not [bool] $placementBoundary.appliesUnityObjects) 'PlacementRuleAppliesUnity'
Require (-not [bool] $placementBoundary.changesSimulationAuthority) 'PlacementRuleChangesAuthority'
Require (-not [bool] $placementBoundary.runtimeVerified) 'PlacementRuleRuntimeBoundary'
Require (-not [bool] $placementBoundary.gameViewVerified) 'PlacementRuleGameViewBoundary'
Require-Text $placementBoundary.meaning 'PlacementRuleBoundaryMeaning'

$placementSourceRefs = @($placementRuleCatalog.sourceRefs)
Require ($placementSourceRefs.Count -eq 3) 'PlacementRuleSourceCount'
Require-Unique $placementSourceRefs { param($x) $x.kindCode } 'PlacementRuleSourceDuplicate'
foreach ($sourceRef in $placementSourceRefs) {
    Require-Text $sourceRef.path "PlacementRuleSourcePath:$($sourceRef.kindCode)"
    Require-Text $sourceRef.expectedRevision "PlacementRuleSourceRevision:$($sourceRef.kindCode)"
    Require (Test-Path -LiteralPath (Resolve-RepoPath ([string] $sourceRef.path)) -PathType Leaf) "PlacementRuleSourceMissing:$($sourceRef.kindCode)"
}

$h5PolicyRef = @($placementSourceRefs | Where-Object kindCode -eq 'H5WorldLayoutPolicy')
$hierarchyRef = @($placementSourceRefs | Where-Object kindCode -eq 'SpatialHierarchy')
$formationRef = @($placementSourceRefs | Where-Object kindCode -eq 'SpatialFormationModes')
Require ($h5PolicyRef.Count -eq 1) 'PlacementRuleH5Source'
Require ($hierarchyRef.Count -eq 1) 'PlacementRuleHierarchySource'
Require ($formationRef.Count -eq 1) 'PlacementRuleFormationSource'
$h5Policy = Read-Json ([string] $h5PolicyRef[0].path)
$hierarchyCatalog = Read-Json ([string] $hierarchyRef[0].path)
$formationCatalog = Read-Json ([string] $formationRef[0].path)
Require ([string] $h5Policy.schemaVersion -eq 'simulation-world-h5-layout-policy.v1') 'PlacementRuleH5Schema'
Require ([string] $h5Policy.revision -eq [string] $h5PolicyRef[0].expectedRevision) 'PlacementRuleH5Revision'
Require ([string] $hierarchyCatalog.schemaVersion -eq 'simulation-world-spatial-hierarchy.v1') 'PlacementRuleHierarchySchema'
Require ([string] $hierarchyCatalog.revision -eq [string] $hierarchyRef[0].expectedRevision) 'PlacementRuleHierarchyRevision'
Require ([string] $formationCatalog.schemaVersion -eq 'simulation-spatial-formation-modes.v1') 'PlacementRuleFormationSchema'
Require ([string] $formationCatalog.revision -eq [string] $formationRef[0].expectedRevision) 'PlacementRuleFormationRevision'

$hierarchyConsumption = $placementRuleCatalog.hierarchyConsumption
$expectedHLevels = @($hierarchyConsumption.expectedHLevelCodes)
$expectedFormationModes = @($hierarchyConsumption.expectedFormationModeCodes)
Require (($expectedHLevels -join '|') -ceq 'H1|H2|H3|H4') 'PlacementRuleExpectedHLevels'
Require ((@($hierarchyCatalog.levels | ForEach-Object code) -join '|') -ceq ($expectedHLevels -join '|')) 'PlacementRuleHierarchyLevelDrift'
Require ((@($formationCatalog.formationModes | ForEach-Object code) -join '|') -ceq ($expectedFormationModes -join '|')) 'PlacementRuleFormationModeDrift'
Require-Text $hierarchyConsumption.constraintAxisCode 'PlacementRuleConstraintAxis'
Require-Text $hierarchyConsumption.formationAxisCode 'PlacementRuleFormationAxis'
Require-Text $hierarchyConsumption.meaning 'PlacementRuleHierarchyMeaning'

$constraintById = @{}
foreach ($constraint in $constraints) { $constraintById[[string] $constraint.constraintId] = $constraint }
$nodeById = @{}
foreach ($node in $nodes) { $nodeById[[string] $node.nodeId] = $node }
$edgeById = @{}
foreach ($edge in $edges) { $edgeById[[string] $edge.edgeId] = $edge }
$presentGraphAreas = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($node in $nodes) {
    if ($null -ne $node.actualRef) { $null = $presentGraphAreas.Add([string] $node.actualRef.areaSetStableId) }
}

$constraintTargetAreas = @{}
foreach ($constraint in $constraints) {
    $targetAreas = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($target in @($constraint.targetRefs)) {
        $targetText = [string] $target
        $targetNodes = @()
        if ($targetText -eq 'gm-node:*') { $targetNodes = @($nodes) }
        elseif ($targetText -eq 'gm-edge:*') {
            foreach ($edge in $edges) {
                $targetNodes += @($nodeById[[string] $edge.fromNodeId], $nodeById[[string] $edge.toNodeId])
            }
        }
        elseif ($nodeById.ContainsKey($targetText)) { $targetNodes = @($nodeById[$targetText]) }
        elseif ($edgeById.ContainsKey($targetText)) {
            $edge = $edgeById[$targetText]
            $targetNodes = @($nodeById[[string] $edge.fromNodeId], $nodeById[[string] $edge.toNodeId])
        }
        foreach ($targetNode in $targetNodes) {
            if ($null -ne $targetNode.actualRef) { $null = $targetAreas.Add([string] $targetNode.actualRef.areaSetStableId) }
        }
    }
    $constraintTargetAreas[[string] $constraint.constraintId] = $targetAreas
}

$h5AnchorByArea = @{}
foreach ($anchor in @($h5Policy.areaAnchors)) { $h5AnchorByArea[[string] $anchor.areaSetStableId] = $anchor }
$areaRuleProfiles = @($placementRuleCatalog.areaRuleProfiles)
Require ($areaRuleProfiles.Count -eq @($h5Policy.areaAnchors).Count) 'PlacementRuleAreaProfileCount'
Require-Unique $areaRuleProfiles { param($x) $x.areaSetStableId } 'PlacementRuleAreaProfileDuplicate'
$allowedRuleUsage = @('BoundToGraphConstraint', 'AvailableNotSelected', 'OutsideCurrentGraph')
$allPlacementRuleRefs = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$placementBoundConstraintIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$resolvedPlacementRuleBindings = [Collections.Generic.List[object]]::new()
$placementRuleCount = 0

foreach ($profile in $areaRuleProfiles) {
    $areaSetId = [string] $profile.areaSetStableId
    Require ($h5AnchorByArea.ContainsKey($areaSetId)) "PlacementRuleAreaUnknown:$areaSetId"
    $anchor = $h5AnchorByArea[$areaSetId]
    Require ([string] $profile.canonicalAreaRoleCode -eq [string] $anchor.canonicalAreaRoleCode) "PlacementRuleAreaRoleMismatch:$areaSetId"
    $expectedPresence = if ($presentGraphAreas.Contains($areaSetId)) { 'Present' } else { 'OutsideCurrentGraph' }
    Require ([string] $profile.graphPresenceCode -eq $expectedPresence) "PlacementRuleAreaPresenceMismatch:$areaSetId"
    $rules = @($profile.rules)
    Require ($rules.Count -gt 0) "PlacementRuleAreaRulesEmpty:$areaSetId"
    Require-Unique $rules { param($x) $x.sourceRuleCode } "PlacementRuleSourceCodeDuplicate:$areaSetId"
    $sourceCodes = @($rules | ForEach-Object sourceRuleCode | Sort-Object)
    $anchorCodes = @($anchor.placementRuleCodes | Sort-Object)
    Require (($sourceCodes -join '|') -ceq ($anchorCodes -join '|')) "PlacementRuleSourceCodeDrift:$areaSetId"

    foreach ($rule in $rules) {
        $ruleRef = [string] $rule.ruleRef
        $usageCode = [string] $rule.usageCode
        Require-Text $ruleRef "PlacementRuleRef:$areaSetId"
        Require ($allPlacementRuleRefs.Add($ruleRef)) "PlacementRuleRefDuplicate:$ruleRef"
        Require ($allowedRuleUsage -contains $usageCode) "PlacementRuleUsage:$ruleRef"
        Require-Text $rule.reason "PlacementRuleReason:$ruleRef"
        $placementRuleCount++
        $ruleConstraintRefs = @($rule.constraintRefs)
        if ($usageCode -eq 'BoundToGraphConstraint') {
            Require ([string] $profile.graphPresenceCode -eq 'Present') "PlacementRuleBoundOutsideGraph:$ruleRef"
            Require ($ruleConstraintRefs.Count -gt 0) "PlacementRuleConstraintRefsEmpty:$ruleRef"
            Require-Unique $ruleConstraintRefs { param($x) [string] $x } "PlacementRuleConstraintRefDuplicate:$ruleRef"
            foreach ($constraintRef in $ruleConstraintRefs) {
                $constraintText = [string] $constraintRef
                Require ($constraintById.ContainsKey($constraintText)) "PlacementRuleConstraintUnknown:$ruleRef`:$constraintText"
                Require ($constraintTargetAreas[$constraintText].Contains($areaSetId)) "PlacementRuleConstraintAreaMismatch:$ruleRef`:$constraintText"
                $null = $placementBoundConstraintIds.Add($constraintText)
            }
            $resolvedPlacementRuleBindings.Add([pscustomobject][ordered]@{
                ruleRef = $ruleRef
                sourceRuleCode = [string] $rule.sourceRuleCode
                areaSetStableId = $areaSetId
                canonicalAreaRoleCode = [string] $profile.canonicalAreaRoleCode
                constraintRefs = @($ruleConstraintRefs)
                reason = [string] $rule.reason
            })
        }
        else {
            Require ($ruleConstraintRefs.Count -eq 0) "PlacementRuleUnexpectedConstraintRef:$ruleRef"
            if ($usageCode -eq 'OutsideCurrentGraph') {
                Require ([string] $profile.graphPresenceCode -eq 'OutsideCurrentGraph') "PlacementRuleOutsidePresentGraph:$ruleRef"
            }
            else {
                Require ([string] $profile.graphPresenceCode -eq 'Present') "PlacementRuleAvailableOutsideGraph:$ruleRef"
            }
        }
    }
}
foreach ($anchor in @($h5Policy.areaAnchors)) {
    Require (@($areaRuleProfiles | Where-Object areaSetStableId -eq $anchor.areaSetStableId).Count -eq 1) "PlacementRuleAreaCoverageMissing:$($anchor.areaSetStableId)"
}

$governanceConstraints = @($placementRuleCatalog.governanceOnlyConstraints)
Require-Unique $governanceConstraints { param($x) $x.constraintRef } 'PlacementRuleGovernanceConstraintDuplicate'
$governanceConstraintIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($governance in $governanceConstraints) {
    $constraintRef = [string] $governance.constraintRef
    Require ($constraintById.ContainsKey($constraintRef)) "PlacementRuleGovernanceConstraintUnknown:$constraintRef"
    Require (-not $placementBoundConstraintIds.Contains($constraintRef)) "PlacementRuleConstraintDoubleClassified:$constraintRef"
    Require-Text $governance.reason "PlacementRuleGovernanceReason:$constraintRef"
    Require (@($governance.sourceRefs).Count -gt 0) "PlacementRuleGovernanceSourcesEmpty:$constraintRef"
    foreach ($sourceRef in @($governance.sourceRefs)) {
        Require (Test-Path -LiteralPath (Resolve-RepoPath ([string] $sourceRef)) -PathType Leaf) "PlacementRuleGovernanceSourceMissing:$constraintRef`:$sourceRef"
    }
    $null = $governanceConstraintIds.Add($constraintRef)
}
foreach ($constraint in $constraints) {
    $constraintId = [string] $constraint.constraintId
    $isPlacementBound = $placementBoundConstraintIds.Contains($constraintId)
    $isGovernanceOnly = $governanceConstraintIds.Contains($constraintId)
    Require ($isPlacementBound -xor $isGovernanceOnly) "PlacementRuleConstraintCoverage:$constraintId"
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
$layers = @($overlayCatalog.layers)
Require ($layers.Count -eq 6) 'LayerCount'
Require-Unique $layers { param($x) $x.layerId } 'LayerDuplicate'
$layerIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$layerOrder = [Collections.Generic.HashSet[int]]::new()
$allowedLayerKinds = @('BaseSpace', 'WeatherTime', 'Transport', 'ThreatSecurity', 'LogisticsSupply', 'PlayerChoice')
foreach ($layer in $layers) {
    $layerId = [string] $layer.layerId
    Require ($layerIds.Add($layerId)) "LayerDuplicate:$layerId"
    Require-Text $layer.label "LayerLabel:$layerId"
    Require ($allowedLayerKinds -contains [string] $layer.layerKindCode) "LayerKind:$layerId"
    Require ($layerOrder.Add([int] $layer.compositionOrder)) "LayerOrderDuplicate:$layerId"
    Require-Text $layer.authorityBoundary "LayerAuthorityBoundary:$layerId"
    Require (@($layer.sourceRefs).Count -gt 0) "LayerSourcesEmpty:$layerId"
}
Require (@($overlayCatalog.compositionOrder).Count -eq $layers.Count) 'LayerCompositionCount'
for ($layerIndex = 0; $layerIndex -lt $layers.Count; $layerIndex++) {
    Require ([string] $overlayCatalog.compositionOrder[$layerIndex] -eq [string] (@($layers | Sort-Object compositionOrder)[$layerIndex].layerId)) "LayerCompositionOrder:$layerIndex"
}
$overlays = @($overlayCatalog.overlays)
Require ($overlays.Count -gt 0) 'OverlaysEmpty'
Require-Unique $overlays { param($x) $x.overlayId } 'OverlayDuplicate'
$routeContributionKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$allowedRouteStates = @('Open', 'Degraded', 'Blocked', 'Unknown')
$allowedCostDimensions = @('Distance', 'Time', 'Money', 'Resource', 'Risk')
$allowedValueStates = @('Observed', 'Derived', 'Unknown')
$allowedCombinationPolicies = @('Annotation', 'Constraint', 'Delta', 'Override')
$allowedFreshnessStates = @('Current', 'Stale', 'Unknown', 'PlanningCurrent')
$edgeEffectCount = 0
foreach ($overlay in $overlays) {
    $overlayId = [string] $overlay.overlayId
    Require ($layerIds.Contains([string] $overlay.layerRef)) "OverlayLayerUnknown:$overlayId"
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
    foreach ($edgeRef in @($overlay.targetEdgeRefs)) {
        Require ($planEdgeIds.Contains([string] $edgeRef)) "OverlayEdgeUnknown:$($overlayId):$edgeRef"
    }
    foreach ($sourceDecisionId in @($overlay.sourceDecisionIds)) {
        Require ($decisionIds.Contains([string] $sourceDecisionId)) "OverlayDecisionUnknown:$($overlayId):$sourceDecisionId"
    }
    if ($null -ne $overlay.PSObject.Properties['sourcePlanIds']) {
        foreach ($sourcePlanId in @($overlay.sourcePlanIds)) {
            Require ($planIds.Contains([string] $sourcePlanId)) "OverlayPlanUnknown:$($overlayId):$sourcePlanId"
        }
    }
    if ($null -ne $overlay.PSObject.Properties['edgeEffects']) {
        foreach ($effect in @($overlay.edgeEffects)) {
            $edgeEffectCount++
            $edgeRef = [string] $effect.edgeRef
            Require ($planEdgeIds.Contains($edgeRef)) "OverlayEffectEdgeUnknown:$($overlayId):$edgeRef"
            Require (@($overlay.targetEdgeRefs) -contains $edgeRef) "OverlayEffectEdgeNotTargeted:$($overlayId):$edgeRef"
            Require ($allowedRouteStates -contains [string] $effect.routeStateCode) "OverlayRouteState:$($overlayId):$edgeRef"
            Require-Text $effect.scopeCode "OverlayEffectScope:$($overlayId):$edgeRef"
            foreach ($component in @($effect.costComponents)) {
                $dimension = [string] $component.dimensionCode
                $valueState = [string] $component.valueStateCode
                $key = [string] $component.contributionKey
                Require ($allowedCostDimensions -contains $dimension) "OverlayCostDimension:$($overlayId):$edgeRef"
                Require ($allowedValueStates -contains $valueState) "OverlayCostValueState:$($overlayId):$($edgeRef):$dimension"
                Require-Text $component.unitCode "OverlayCostUnit:$($overlayId):$($edgeRef):$dimension"
                Require-Text $key "OverlayCostContributionKey:$($overlayId):$($edgeRef):$dimension"
                Require ($allowedCombinationPolicies -contains [string] $component.combinationPolicyCode) "OverlayCostCombination:$($overlayId):$($edgeRef):$dimension"
                Require ($routeContributionKeys.Add("$edgeRef|$dimension|$key")) "OverlayCostContributionDuplicate:$($edgeRef):$($dimension):$key"
                if ($valueState -eq 'Unknown') { Require ($null -eq $component.value) "OverlayUnknownCostHasValue:$($overlayId):$($edgeRef):$dimension" }
                else { Require ($null -ne $component.value) "OverlayKnownCostMissingValue:$($overlayId):$($edgeRef):$dimension" }
            }
            $capacity = $effect.capacity
            Require ($allowedValueStates -contains [string] $capacity.valueStateCode) "OverlayCapacityValueState:$($overlayId):$edgeRef"
            Require-Text $capacity.unitCode "OverlayCapacityUnit:$($overlayId):$edgeRef"
            if ([string] $capacity.valueStateCode -eq 'Unknown') { Require ($null -eq $capacity.value) "OverlayUnknownCapacityHasValue:$($overlayId):$edgeRef" }
            else { Require ($null -ne $capacity.value) "OverlayKnownCapacityMissingValue:$($overlayId):$edgeRef" }
            $evidence = $effect.evidence
            Require-Text $evidence.sourceRef "OverlayEvidenceSource:$($overlayId):$edgeRef"
            Require-Text $evidence.sourceRevision "OverlayEvidenceRevision:$($overlayId):$edgeRef"
            Require-Text $evidence.sourceSha256 "OverlayEvidenceHash:$($overlayId):$edgeRef"
            Require-Text $evidence.observedAt "OverlayEvidenceObservedAt:$($overlayId):$edgeRef"
            Require-Text $evidence.validUntil "OverlayEvidenceValidUntil:$($overlayId):$edgeRef"
            Require ($allowedFreshnessStates -contains [string] $evidence.freshnessStateCode) "OverlayEvidenceFreshness:$($overlayId):$edgeRef"
            $evidencePath = Resolve-RepoPath ([string] $evidence.sourceRef)
            Require (Test-Path -LiteralPath $evidencePath -PathType Leaf) "OverlayEvidenceSourceMissing:$($overlayId):$edgeRef"
            Require ((Absolute-Hash $evidencePath) -eq ([string] $evidence.sourceSha256).ToUpperInvariant()) "OverlayEvidenceHashMismatch:$($overlayId):$edgeRef"
        }
    }
}

$allImpactRefs = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($value in $planNodeIds) { $null = $allImpactRefs.Add($value) }
foreach ($value in $planEdgeIds) { $null = $allImpactRefs.Add($value) }
foreach ($value in $subgraphIds) { $null = $allImpactRefs.Add($value) }
foreach ($value in $layerIds) { $null = $allImpactRefs.Add($value) }
foreach ($value in @($overlays | ForEach-Object overlayId)) { $null = $allImpactRefs.Add([string] $value) }
$null = $allImpactRefs.Add([string] $plan.graphMapStableId)
$null = $allImpactRefs.Add([string] $overlayRef.path)
foreach ($assessment in $planningAssessments) {
    foreach ($impactRef in @($assessment.impactRefs)) {
        Require ($allImpactRefs.Contains([string] $impactRef)) "PlanningAssessmentImpactUnknown:$($assessment.planId):$impactRef"
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
            $targetRefs = @($nodes | Where-Object stateCode -eq 'ReferenceAvailable' | ForEach-Object nodeId) + @($edges | Where-Object stateCode -eq 'ReferenceAvailable' | ForEach-Object edgeId)
        }
        'AllResolvedNodes' {
            Require (@($assignment[0].explicitTargetRefs).Count -eq 0) "Level3SelectorExplicitTargets:$bindingId"
            $targetRefs = @($nodes | Where-Object stateCode -eq 'ReferenceAvailable' | ForEach-Object nodeId)
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

$normalizationCatalog = $null
$normalizationElements = @()
$normalizationRelations = @()
$normalizationBlockedCount = 0
if ($null -ne $plan.level1.PSObject.Properties['normalizationCatalogRef']) {
    $normalizationRef = $plan.level1.normalizationCatalogRef
    Require-Text $normalizationRef.path 'NormalizationCatalogPath'
    Require-Text $normalizationRef.expectedRevision 'NormalizationCatalogExpectedRevision'
    $normalizationCatalog = Read-Json ([string] $normalizationRef.path)
    Require ([string] $normalizationCatalog.schemaVersion -eq 'mirror-graph-map-normalization.v1') 'NormalizationCatalogSchema'
    Require ([string] $normalizationCatalog.revision -eq [string] $normalizationRef.expectedRevision) 'NormalizationCatalogRevision'
    Require ([string] $normalizationCatalog.graphMapStableId -eq [string] $plan.graphMapStableId) 'NormalizationGraphIdentity'
    Require ([string] $normalizationCatalog.sourcePlanRevision -eq [string] $plan.revision) 'NormalizationSourcePlanRevision'
    Require-Text $normalizationCatalog.sampleStableId 'NormalizationSampleStableId'
    Require (-not [bool] $normalizationCatalog.evidenceBoundary.unitySceneChanged) 'NormalizationSceneChanged'
    Require (-not [bool] $normalizationCatalog.evidenceBoundary.runtimeBindingVerified) 'NormalizationRuntimeBindingClaimed'
    Require (-not [bool] $normalizationCatalog.evidenceBoundary.eStagePromoted) 'NormalizationEStagePromoted'
    Require ([bool] $normalizationCatalog.evidenceBoundary.graphMapMeaningOnly) 'NormalizationMeaningBoundary'
    Require (@($normalizationCatalog.contextFieldOrder).Count -eq $requiredContext.Count) 'NormalizationContextFieldCount'
    for ($i = 0; $i -lt $requiredContext.Count; $i++) {
        Require ([string] $normalizationCatalog.contextFieldOrder[$i] -eq $requiredContext[$i]) "NormalizationContextFieldOrder:$i"
    }

    $normalizationElements = @($normalizationCatalog.elements)
    $normalizationRelations = @($normalizationCatalog.relations)
    Require ($normalizationElements.Count -gt 0) 'NormalizationElementsEmpty'
    Require ($normalizationRelations.Count -gt 0) 'NormalizationRelationsEmpty'
    Require-Unique $normalizationElements { param($x) $x.elementRef } 'NormalizationElementDuplicate'
    Require-Unique $normalizationRelations { param($x) $x.relationRef } 'NormalizationRelationDuplicate'

    $allowedMigrationState = @('Retained', 'Reclassified', 'RelationExtracted', 'CompatibilityAlias', 'Blocked')
    $allowedNormalizedNodeKind = @('Place', 'Actor', 'WorldObject', 'Resource', 'Gateway', 'CompatibilityAlias')
    $allowedNormalizedRelationKind = @('Movement', 'Work', 'Observation', 'Witness', 'StateTransition', 'PermissionGrant', 'ResourcePlacement')
    $allowedIdentityMode = @('ReuseLevel1Node', 'NewCanonicalNode')
    $normalizedElementIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $normalizedElementById = @{}
    foreach ($element in $normalizationElements) {
        $elementRef = [string] $element.elementRef
        $null = $normalizedElementIds.Add($elementRef)
        $normalizedElementById[$elementRef] = $element
        Require ($allowedIdentityMode -contains [string] $element.identityModeCode) "NormalizationIdentityMode:$elementRef"
        Require ($allowedNormalizedNodeKind -contains [string] $element.normalizedKindCode) "NormalizationNodeKind:$elementRef"
        Require ($allowedMigrationState -contains [string] $element.migrationStateCode) "NormalizationMigrationState:$elementRef"
        Require (@($element.sourceElementRefs).Count -gt 0) "NormalizationElementSourcesEmpty:$elementRef"
        foreach ($sourceRef in @($element.sourceElementRefs)) {
            $sourceText = [string] $sourceRef
            Require ($planNodeIds.Contains($sourceText) -or $planEdgeIds.Contains($sourceText)) "NormalizationElementSourceUnknown:$elementRef`:$sourceText"
        }
        if ([string] $element.identityModeCode -eq 'ReuseLevel1Node') {
            Require ($planNodeIds.Contains($elementRef)) "NormalizationReusedNodeUnknown:$elementRef"
        }
        else {
            Require (-not $planNodeIds.Contains($elementRef) -and -not $planEdgeIds.Contains($elementRef)) "NormalizationCanonicalIdCollision:$elementRef"
        }
        if ([string] $element.normalizedKindCode -eq 'Actor') {
            Require ([string] $element.actorIdentityCode -eq 'PersistentNamedActor') "NormalizationAnonymousActorMaterialized:$elementRef"
        }
        if ([string] $element.migrationStateCode -eq 'CompatibilityAlias') {
            Require ([string] $element.normalizedKindCode -eq 'CompatibilityAlias') "NormalizationAliasKind:$elementRef"
            Require (@($element.compatibilityTargetRefs).Count -gt 0) "NormalizationAliasTargetsEmpty:$elementRef"
        }
        if ([string] $element.migrationStateCode -eq 'Blocked') {
            Require-Text $element.blockedReason "NormalizationBlockedReason:$elementRef"
            $normalizationBlockedCount++
        }
    }

    $normalizedRelationIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($relation in $normalizationRelations) {
        $relationRef = [string] $relation.relationRef
        Require (-not $planEdgeIds.Contains($relationRef) -and -not $planNodeIds.Contains($relationRef) -and -not $normalizedElementIds.Contains($relationRef)) "NormalizationRelationIdCollision:$relationRef"
        $null = $normalizedRelationIds.Add($relationRef)
    }
    $normalizedSemanticIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($nodeId in $planNodeIds) { $null = $normalizedSemanticIds.Add($nodeId) }
    foreach ($edgeId in $planEdgeIds) { $null = $normalizedSemanticIds.Add($edgeId) }
    foreach ($elementId in $normalizedElementIds) { $null = $normalizedSemanticIds.Add($elementId) }
    foreach ($relationId in $normalizedRelationIds) { $null = $normalizedSemanticIds.Add($relationId) }

    $persistentActorCount = 0
    foreach ($element in $normalizationElements) {
        $elementRef = [string] $element.elementRef
        if ([string] $element.normalizedKindCode -eq 'Actor') { $persistentActorCount++ }
        if ([string] $element.migrationStateCode -eq 'CompatibilityAlias') {
            $hasExtractedRelation = $false
            foreach ($targetRef in @($element.compatibilityTargetRefs)) {
                $targetText = [string] $targetRef
                Require ($normalizedSemanticIds.Contains($targetText)) "NormalizationAliasTargetUnknown:$elementRef`:$targetText"
                if ($normalizedRelationIds.Contains($targetText)) { $hasExtractedRelation = $true }
            }
            Require $hasExtractedRelation "NormalizationAliasRelationMissing:$elementRef"
        }
    }
    Require ($persistentActorCount -gt 0) 'NormalizationPersistentActorMissing'

    foreach ($relation in $normalizationRelations) {
        $relationRef = [string] $relation.relationRef
        Require ($allowedNormalizedRelationKind -contains [string] $relation.normalizedKindCode) "NormalizationRelationKind:$relationRef"
        Require ([string] $relation.migrationStateCode -eq 'RelationExtracted') "NormalizationRelationState:$relationRef"
        Require ($allowedIntention -contains [string] $relation.intentionCode) "NormalizationRelationIntention:$relationRef"
        $actorNodeRef = [string] $relation.actorNodeRef
        $actorRoleRef = [string] $relation.actorRoleRef
        Require (([string]::IsNullOrWhiteSpace($actorNodeRef)) -xor ([string]::IsNullOrWhiteSpace($actorRoleRef))) "NormalizationRelationActorExclusive:$relationRef"
        if (-not [string]::IsNullOrWhiteSpace($actorNodeRef)) {
            Require ($normalizedElementIds.Contains($actorNodeRef)) "NormalizationActorNodeUnknown:$relationRef`:$actorNodeRef"
            Require ([string] $normalizedElementById[$actorNodeRef].normalizedKindCode -eq 'Actor') "NormalizationActorNodeKind:$relationRef`:$actorNodeRef"
        }
        else { Require-Text $actorRoleRef "NormalizationActorRoleEmpty:$relationRef" }
        foreach ($field in @('targetRefs', 'guardRefs', 'inputRefs', 'effectRefs', 'observationRefs', 'sourceElementRefs', 'constraintRefs', 'level3TargetRefs')) {
            Require (@($relation.$field).Count -gt 0) "NormalizationRelationFieldEmpty:$relationRef`:$field"
        }
        foreach ($targetRef in @($relation.targetRefs)) { Require ($normalizedSemanticIds.Contains([string] $targetRef)) "NormalizationTargetUnknown:$relationRef`:$targetRef" }
        foreach ($semanticField in @('inputRefs', 'effectRefs', 'observationRefs')) {
            foreach ($semanticRef in @($relation.$semanticField)) { Require ($normalizedSemanticIds.Contains([string] $semanticRef)) "NormalizationSemanticRefUnknown:$relationRef`:$semanticField`:$semanticRef" }
        }
        foreach ($returnField in @('failureReturnRef', 'interruptReturnRef')) {
            Require-Text $relation.$returnField "NormalizationReturnRefEmpty:$relationRef`:$returnField"
            Require ($normalizedSemanticIds.Contains([string] $relation.$returnField)) "NormalizationReturnRefUnknown:$relationRef`:$returnField"
        }
        foreach ($wiId in @($relation.worldInteractionIds)) { Require ($wiIds.Contains([string] $wiId)) "NormalizationUnknownWi:$relationRef`:$wiId" }
        foreach ($decisionId in @($relation.sourceDecisionIds)) { Require ($decisionIds.Contains([string] $decisionId)) "NormalizationUnknownDecision:$relationRef`:$decisionId" }
        foreach ($planId in @($relation.sourcePlanIds)) { Require ($planIds.Contains([string] $planId)) "NormalizationUnknownPlan:$relationRef`:$planId" }
        foreach ($constraintRef in @($relation.constraintRefs) + @($relation.guardRefs)) { Require (@($constraints | Where-Object { [string] $_.constraintId -eq [string] $constraintRef }).Count -eq 1) "NormalizationConstraintUnknown:$relationRef`:$constraintRef" }
        foreach ($targetRef in @($relation.level3TargetRefs)) { Require ($allCodeTargetIds.Contains([string] $targetRef)) "NormalizationLevel3TargetUnknown:$relationRef`:$targetRef" }
        foreach ($sourceRef in @($relation.sourceElementRefs)) { Require ($planNodeIds.Contains([string] $sourceRef) -or $planEdgeIds.Contains([string] $sourceRef)) "NormalizationRelationSourceUnknown:$relationRef`:$sourceRef" }
        $contextSourceRef = [string] $relation.contextSourceNodeRef
        Require ($planNodeIds.Contains($contextSourceRef)) "NormalizationContextSourceUnknown:$relationRef`:$contextSourceRef"
        $contextNode = @($nodes | Where-Object { [string] $_.nodeId -eq $contextSourceRef })[0]
        foreach ($field in $requiredContext) { Require-Text $contextNode.planningContext.$field "NormalizationContextRestoreMissing:$relationRef`:$field" }

        $relationWi = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($value in @($relation.worldInteractionIds)) { $null = $relationWi.Add([string] $value) }
        $relationDecision = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($value in @($relation.sourceDecisionIds)) { $null = $relationDecision.Add([string] $value) }
        $relationPlan = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($value in @($relation.sourcePlanIds)) { $null = $relationPlan.Add([string] $value) }
        foreach ($sourceRef in @($relation.sourceElementRefs)) {
            $sourceNode = @($nodes | Where-Object { [string] $_.nodeId -eq [string] $sourceRef })
            if ($sourceNode.Count -eq 1) {
                foreach ($value in @($sourceNode[0].worldInteractionIds)) { Require ($relationWi.Contains([string] $value)) "NormalizationWiLineageLost:$relationRef`:$value" }
                foreach ($value in @($sourceNode[0].sourceDecisionIds)) { Require ($relationDecision.Contains([string] $value)) "NormalizationDecisionLineageLost:$relationRef`:$value" }
                if ($null -ne $sourceNode[0].PSObject.Properties['sourcePlanIds']) {
                    foreach ($value in @($sourceNode[0].sourcePlanIds)) { Require ($relationPlan.Contains([string] $value)) "NormalizationPlanLineageLost:$relationRef`:$value" }
                }
            }
        }
    }
}
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
        placementRuleCatalogRef = [string] $placementRuleRef.path
        placementRuleCatalogRevision = [string] $placementRuleCatalog.revision
        placementRuleCatalogHashSha256 = File-Hash ([string] $placementRuleRef.path)
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
        placementRuleProfiles = $areaRuleProfiles.Count
        placementRules = $placementRuleCount
        placementRuleBindings = $resolvedPlacementRuleBindings.Count
        placementRuleBoundConstraints = $placementBoundConstraintIds.Count
        governanceOnlyConstraints = $governanceConstraintIds.Count
        traversalProfiles = $traversalProfiles.Count
        subgraphs = $subgraphs.Count
        ports = $portsById.Count
        connectors = $connectors.Count
        layers = $layers.Count
        overlays = $overlays.Count
        overlayEdgeEffects = $edgeEffectCount
        planningAssessments = $planningAssessments.Count
        codeBindings = $codeBindings.Count
        sourceCodeFiles = $uniqueSourceFiles.Count
        codeBoundLevel1Targets = $boundLevel1.Count
        unboundLevel1Targets = $unboundTargets.Count
        unresolvedNodes = $unresolvedNodeCount
        unresolvedEdges = $unresolvedEdgeCount
        normalizedElements = $normalizationElements.Count
        normalizedRelations = $normalizationRelations.Count
        normalizationBlocked = $normalizationBlockedCount
    }
    plan = $plan
    partitionCatalog = $partitionCatalog
    overlayCatalog = $overlayCatalog
    placementRuleCatalog = $placementRuleCatalog
    resolvedPlacementRuleBindings = @($resolvedPlacementRuleBindings)
    resolvedCodeBindings = @($codeBindings)
    normalizationCatalog = $normalizationCatalog
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
$markdownLines.Add("- 레이어: $($layers.Count) / 엣지 효과 $edgeEffectCount / 현행 기획 판정 $($planningAssessments.Count)")
$markdownLines.Add("- 배치 규칙: Area 프로필 $($areaRuleProfiles.Count) / 기존 규칙 $placementRuleCount / 직접 결속 $($resolvedPlacementRuleBindings.Count) / 규칙 결속 제약 $($placementBoundConstraintIds.Count)")
$markdownLines.Add("- 레벨 3 코드 결속: $($codeBindings.Count) / 소스 파일 $($uniqueSourceFiles.Count) / 실제 결속 미검증 대상 $($unboundTargets.Count)")
$markdownLines.Add("- 정규화 표본: 대상 $($normalizationElements.Count) / 관계 $($normalizationRelations.Count) / 차단 $normalizationBlockedCount")
$markdownLines.Add("- 이번 실제 Runtime 검증: false")
$markdownLines.Add('')
if ($null -ne $normalizationCatalog) {
    $markdownLines.Add('## 한스 표본 — 기존 요소와 정규화 노드·엣지 비교')
    $markdownLines.Add('')
    $markdownLines.Add('> 정규화 조회는 현행 레벨 1 요소를 삭제하지 않는다. CompatibilityAlias는 기존 안정 ID를 보존한 채 추출 관계를 가리키며, Blocked는 근거 부족을 숨기지 않는다.')
    $markdownLines.Add('')
    $markdownLines.Add('| 기존 요소 | 정규화 대상 | 종류 | 이관 상태 | 호환 대상 |')
    $markdownLines.Add('| --- | --- | --- | --- | --- |')
    foreach ($element in $normalizationElements) {
        $markdownLines.Add("| $(Escape-Cell (@($element.sourceElementRefs) -join ', ')) | $(Escape-Cell $element.elementRef) | $(Escape-Cell $element.normalizedKindCode) | $(Escape-Cell $element.migrationStateCode) | $(Escape-Cell (@($element.compatibilityTargetRefs) -join ', ')) |")
    }
    $markdownLines.Add('')
    $markdownLines.Add('| 정규화 엣지 | Actor | 대상 | WI | 상태 |')
    $markdownLines.Add('| --- | --- | --- | --- | --- |')
    foreach ($relation in $normalizationRelations) {
        $actor = if ([string]::IsNullOrWhiteSpace([string] $relation.actorNodeRef)) { "role:$($relation.actorRoleRef)" } else { [string] $relation.actorNodeRef }
        $markdownLines.Add("| $(Escape-Cell $relation.relationRef)<br>$(Escape-Cell $relation.normalizedKindCode) | $(Escape-Cell $actor) | $(Escape-Cell (@($relation.targetRefs) -join ', ')) | $(Escape-Cell (@($relation.worldInteractionIds) -join ', ')) | $(Escape-Cell $relation.migrationStateCode) |")
    }
    $markdownLines.Add('')
    $markdownLines.Add('### 일곱 칸 복원')
    $markdownLines.Add('')
    $markdownLines.Add('| 관계 | 지금 | 여기 | 나 | 너 | 이렇게 | 결과 | 다음 선택 |')
    $markdownLines.Add('| --- | --- | --- | --- | --- | --- | --- | --- |')
    foreach ($relation in $normalizationRelations) {
        $contextNode = @($nodes | Where-Object { [string] $_.nodeId -eq [string] $relation.contextSourceNodeRef })[0]
        $context = $contextNode.planningContext
        $markdownLines.Add("| $(Escape-Cell $relation.relationRef) | $(Escape-Cell $context.time) | $(Escape-Cell $context.place) | $(Escape-Cell $context.player) | $(Escape-Cell $context.target) | $(Escape-Cell $context.method) | $(Escape-Cell $context.result) | $(Escape-Cell $context.nextChoices) |")
    }
    $markdownLines.Add('')
    $markdownLines.Add('### 레벨 2 제약과 레벨 3 결속·미결속')
    $markdownLines.Add('')
    $markdownLines.Add('| 관계 | 레벨 2 제약 | 레벨 3 대상 | 현재 결속 상태 |')
    $markdownLines.Add('| --- | --- | --- | --- |')
    foreach ($relation in $normalizationRelations) {
        $bindingStates = @()
        foreach ($targetRef in @($relation.level3TargetRefs)) {
            $bindingStates += if ($unboundSet.Contains([string] $targetRef)) { "$targetRef=NoApprovedUnityBinding" } else { "$targetRef=SourceAndSymbolVerified" }
        }
        $markdownLines.Add("| $(Escape-Cell $relation.relationRef) | $(Escape-Cell (@($relation.constraintRefs) -join ', ')) | $(Escape-Cell (@($relation.level3TargetRefs) -join ', ')) | $(Escape-Cell ($bindingStates -join ', ')) |")
    }
    $markdownLines.Add('')
}
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
$markdownLines.Add('## 레벨 2 — 기존 배치 구조·제약 규칙 결속')
$markdownLines.Add('')
$markdownLines.Add('> 아래 규칙은 기존 H5 배치 정책을 판본째 참조한다. AvailableNotSelected는 규칙이 존재하지만 현재 Graph Map의 노드·엣지 제약에 아직 선택되지 않았다는 뜻이며, 통과나 실제 배치를 의미하지 않는다.')
$markdownLines.Add('')
$markdownLines.Add('| Area | Graph 포함 | 규칙 | 사용 상태 | Graph 제약 |')
$markdownLines.Add('| --- | --- | --- | --- | --- |')
foreach ($profile in $areaRuleProfiles) {
    foreach ($rule in @($profile.rules)) {
        $markdownLines.Add("| $(Escape-Cell $profile.canonicalAreaRoleCode)<br>$(Escape-Cell $profile.areaSetStableId) | $(Escape-Cell $profile.graphPresenceCode) | $(Escape-Cell $rule.ruleRef)<br>$(Escape-Cell $rule.sourceRuleCode) | $(Escape-Cell $rule.usageCode) | $(Escape-Cell (@($rule.constraintRefs) -join ', ')) |")
    }
}
$markdownLines.Add('')
$markdownLines.Add('| Graph 제약 | 분류 | 기존 배치 규칙 | 경계 |')
$markdownLines.Add('| --- | --- | --- | --- |')
foreach ($constraint in $constraints) {
    $constraintId = [string] $constraint.constraintId
    $ruleRefs = @($resolvedPlacementRuleBindings | Where-Object { @($_.constraintRefs) -contains $constraintId } | ForEach-Object ruleRef)
    if ($ruleRefs.Count -gt 0) {
        $markdownLines.Add("| $(Escape-Cell $constraintId) | PlacementRuleBound | $(Escape-Cell ($ruleRefs -join ', ')) | 기존 H5 규칙의 식별·적용 범위만 결속 |")
    }
    else {
        $governance = @($governanceConstraints | Where-Object constraintRef -eq $constraintId)[0]
        $markdownLines.Add("| $(Escape-Cell $constraintId) | GovernanceOnly | 없음 | $(Escape-Cell $governance.reason) |")
    }
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
$markdownLines.Add('### Graph Map 레이어')
$markdownLines.Add('')
$markdownLines.Add('| 순서 | 레이어 | 종류 | 권위 경계 |')
$markdownLines.Add('| --- | --- | --- | --- |')
foreach ($layer in @($layers | Sort-Object compositionOrder)) {
    $markdownLines.Add("| $($layer.compositionOrder) | $(Escape-Cell $layer.layerId)<br>$(Escape-Cell $layer.label) | $(Escape-Cell $layer.layerKindCode) | $(Escape-Cell $layer.authorityBoundary) |")
}
$markdownLines.Add('')
$markdownLines.Add('### 레이어 오버레이')
$markdownLines.Add('')
$markdownLines.Add('| 오버레이 | 레이어·계기 | 대상 하위 맵·엣지 | 효과 범주 | 토폴로지·권위 변경 |')
$markdownLines.Add('| --- | --- | --- | --- | --- |')
foreach ($overlay in $overlays) {
    $targets = @(@($overlay.targetSubgraphRefs) + @($overlay.targetEdgeRefs)) -join ', '
    $markdownLines.Add("| $(Escape-Cell $overlay.overlayId)<br>$(Escape-Cell $overlay.label) | $(Escape-Cell $overlay.layerRef)<br>$(Escape-Cell $overlay.triggerKindCode) | $(Escape-Cell $targets) | $(Escape-Cell (@($overlay.effectCategories) -join ', ')) | false / false |")
}
$markdownLines.Add('')
$markdownLines.Add('### 경로 레이어 엣지 효과')
$markdownLines.Add('')
$markdownLines.Add('| 오버레이·엣지 | 경로 상태 | 비용 차원·상태·기여 키 | 용량 | 근거·신선도 |')
$markdownLines.Add('| --- | --- | --- | --- | --- |')
foreach ($overlay in $overlays) {
    if ($null -ne $overlay.PSObject.Properties['edgeEffects']) {
        foreach ($effect in @($overlay.edgeEffects)) {
            $costText = @($effect.costComponents | ForEach-Object { "$($_.dimensionCode):$($_.valueStateCode):$($_.contributionKey):$($_.combinationPolicyCode)" }) -join ', '
            $markdownLines.Add("| $(Escape-Cell $overlay.overlayId)<br>$(Escape-Cell $effect.edgeRef) | $(Escape-Cell $effect.routeStateCode) | $(Escape-Cell $costText) | $(Escape-Cell $effect.capacity.valueStateCode) / $(Escape-Cell $effect.capacity.unitCode) | $(Escape-Cell $effect.evidence.sourceRevision)<br>$(Escape-Cell $effect.evidence.freshnessStateCode) |")
        }
    }
}
$markdownLines.Add('')
$markdownLines.Add('## 현행 기획 Graph Map 영향 판정')
$markdownLines.Add('')
$markdownLines.Add('> NoImpact는 누락이 아니라 공통 방법론·메타데이터·자료·대체 이력을 공간 Graph Map에 중복 투입하지 않는 명시적 판정이다. Blocked는 현재 E나 구현 상태를 올리지 않는다.')
$markdownLines.Add('')
$markdownLines.Add('| 기획 ID | 판정 | 통합 상태 | 영향 대상 | 근거 |')
$markdownLines.Add('| --- | --- | --- | --- | --- |')
foreach ($assessment in $planningAssessments) {
    $markdownLines.Add("| $(Escape-Cell $assessment.planId) | $(Escape-Cell $assessment.classificationCode) | $(Escape-Cell $assessment.integrationStateCode) | $(Escape-Cell (@($assessment.impactRefs) -join ', ')) | $(Escape-Cell $assessment.reason) |")
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

Write-Output "Graph Map plan $Mode passed: nodes=$($nodes.Count), edges=$($edges.Count), constraints=$($constraints.Count), placementRules=$placementRuleCount, ruleBoundConstraints=$($placementBoundConstraintIds.Count), subgraphs=$($subgraphs.Count), ports=$($portsById.Count), connectors=$($connectors.Count), overlays=$($overlays.Count), codeBindings=$($codeBindings.Count), sourceFiles=$($uniqueSourceFiles.Count), unresolved=$unresolvedNodeCount/$unresolvedEdgeCount, normalization=$($normalizationElements.Count)/$($normalizationRelations.Count)/$normalizationBlockedCount"
