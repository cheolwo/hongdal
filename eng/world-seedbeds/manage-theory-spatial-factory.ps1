param(
    [ValidateSet("Check", "Write")]
    [string] $Mode = "Check",
    [string] $PolicyPath = "eng/world-seedbeds/theory-spatial-factory-policy.v1.json",
    [string] $JsonOutputPath = "eng/world-seedbeds/generated/theory-spatial-factory.v1.json",
    [string] $MarkdownOutputPath = "docs/AI/generated/theory-spatial-factory.md"
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

function Require([bool] $condition, [string] $message) { if (-not $condition) { throw $message } }
function Normalize([string] $value) { return (($value -replace "`r`n", "`n").TrimEnd()) + "`n" }

function Stable-Json([object] $value) {
    return (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n")
}

function Text-Hash([string] $value) {
    $bytes = [Text.Encoding]::UTF8.GetBytes($value)
    return ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))).ToLowerInvariant()
}

function Array-Property([object] $value, [string] $name) {
    if ($null -eq $value.PSObject.Properties[$name]) { return @() }
    return @($value.$name)
}

function Slug([string] $stableId) {
    $suffix = $stableId.Substring($stableId.IndexOf(':') + 1)
    return ($suffix -replace '[^a-zA-Z0-9-]', '-')
}

function Position([string] $topology, [int] $index, [int] $count, [double] $spacing) {
    switch ($topology) {
        "Linear" { return [ordered]@{ x = [Math]::Round(($index - (($count - 1) / 2.0)) * $spacing, 2); z = 0.0 } }
        { $_ -in @("Grid", "ModifiedGrid") } {
            $columns = [Math]::Ceiling([Math]::Sqrt($count))
            $row = [Math]::Floor($index / $columns)
            $column = $index % $columns
            return [ordered]@{ x = [Math]::Round(($column - (($columns - 1) / 2.0)) * $spacing, 2); z = [Math]::Round(($row - .5) * $spacing, 2) }
        }
        { $_ -in @("Radial", "Cluster") } {
            $angle = (2.0 * [Math]::PI * $index) / [Math]::Max(1, $count)
            return [ordered]@{ x = [Math]::Round([Math]::Cos($angle) * $spacing, 2); z = [Math]::Round([Math]::Sin($angle) * $spacing, 2) }
        }
        default {
            return [ordered]@{ x = [Math]::Round(($index - (($count - 1) / 2.0)) * $spacing, 2); z = if (($index % 2) -eq 0) { -$spacing * .35 } else { $spacing * .35 } }
        }
    }
}

function Spatial-Form([string] $hierarchyLevelCode, [string] $topologyCode) {
    if ($hierarchyLevelCode -eq "H2") {
        switch ($topologyCode) {
            "Linear" { return "LinearBlock" }
            { $_ -in @("Grid", "ModifiedGrid") } { return "StreetBlock" }
            "Radial" { return "RingBlock" }
            "Cluster" { return "CompoundBlock" }
            default { return "TerrainAdaptiveBlock" }
        }
    }
    switch ($topologyCode) {
        "Linear" { return "CorridorAssembly" }
        { $_ -in @("Grid", "ModifiedGrid") } { return "DistrictAssembly" }
        { $_ -in @("Radial", "Cluster") } { return "CampusAssembly" }
        default { return "LandscapeDistrictAssembly" }
    }
}

function Reference-Bounds([object[]] $nodes, [double] $paddingMeters) {
    $xValues = @($nodes | ForEach-Object { [double] $_.x })
    $zValues = @($nodes | ForEach-Object { [double] $_.z })
    $minimumX = [Math]::Round(($xValues | Measure-Object -Minimum).Minimum - $paddingMeters, 2)
    $maximumX = [Math]::Round(($xValues | Measure-Object -Maximum).Maximum + $paddingMeters, 2)
    $minimumZ = [Math]::Round(($zValues | Measure-Object -Minimum).Minimum - $paddingMeters, 2)
    $maximumZ = [Math]::Round(($zValues | Measure-Object -Maximum).Maximum + $paddingMeters, 2)
    return [ordered]@{
        minimumX = $minimumX
        minimumZ = $minimumZ
        maximumX = $maximumX
        maximumZ = $maximumZ
        width = [Math]::Round($maximumX - $minimumX, 2)
        depth = [Math]::Round($maximumZ - $minimumZ, 2)
    }
}

function Load-Definitions([object[]] $refs) {
    $result = @{}
    foreach ($ref in $refs) {
        $definition = Read-Json ("eng/world-seedbeds/synty-bottom-up-inventory/" + [string] $ref.definitionPath)
        $result[[string] $ref.stableId] = $definition
    }
    return $result
}

$policy = Read-Json $PolicyPath
Require ([string] $policy.schemaVersion -eq "simulation-world-theory-spatial-factory-policy.v1") "PolicySchemaInvalid"
Require (-not [bool] $policy.humanReviewPolicy.blocking) "HumanReviewMustNotBlockFactory"
Require ([bool] $policy.authorityBoundary.e6AndE7RemainSeparate) "E6E7BoundaryRequired"
$catalog = Read-Json ([string] $policy.sourceCatalogPath)
$authoredRecipes = Read-Json ([string] $policy.sourceAuthoredRecipePath)
$areaSetPriorities = Read-Json ([string] $policy.sourceAreaSetPriorityPath)
$worldInteractions = Read-Json ([string] $policy.sourceWorldInteractionPath)
$patternNaming = Read-Json ([string] $policy.sourcePatternNamingPath)
Require ([string] $patternNaming.schemaVersion -eq "simulation-world-h-pattern-naming.v1") "PatternNamingSchemaInvalid"
Require ([string] $patternNaming.displayPolicy.primarySpatialNameSourceCode -eq "PatternSpatialDisplayName") "PrimarySpatialNameSourceInvalid"
Require ([bool] $patternNaming.displayPolicy.spatialNameMustBeShownBeforeGameplayProfile) "SpatialNameDisplayOrderInvalid"
Require (@($patternNaming.h2Patterns + $patternNaming.h3Patterns | Where-Object { [string]::IsNullOrWhiteSpace([string] $_.spatialDisplayNameKo) }).Count -eq 0) "PatternSpatialDisplayNameMissing"
$h1 = Load-Definitions @($catalog.h1InteractionDefinitionRefs)
$h2 = Load-Definitions @($catalog.h2DefinitionRefs)
$h3 = Load-Definitions @($catalog.h3DefinitionRefs)
$h4 = Load-Definitions @($catalog.h4DefinitionRefs)
$wiIds = @($worldInteractions.items.id)
$authoredByTarget = @{}
foreach ($recipe in @($authoredRecipes.recipes)) { $authoredByTarget[[string] $recipe.targetKnowledgeRef] = $recipe }
$h2PatternByStableId = @{}
$h3PatternByStableId = @{}
$allPatternCodes = @()
foreach ($pattern in @($patternNaming.h2Patterns)) {
    $stableId = [string] $pattern.stableId
    Require (-not $h2PatternByStableId.ContainsKey($stableId)) "H2PatternStableIdDuplicate:$stableId"
    $h2PatternByStableId[$stableId] = $pattern
    $allPatternCodes += [string] $pattern.patternCode
}
foreach ($pattern in @($patternNaming.h3Patterns)) {
    $stableId = [string] $pattern.stableId
    Require (-not $h3PatternByStableId.ContainsKey($stableId)) "H3PatternStableIdDuplicate:$stableId"
    $h3PatternByStableId[$stableId] = $pattern
    $allPatternCodes += [string] $pattern.patternCode
}
Require (@($allPatternCodes | Sort-Object -Unique).Count -eq $allPatternCodes.Count) "PatternCodeDuplicate"
$reservedPatternCodes = @($patternNaming.priorityExpansionQueue | ForEach-Object { [string] $_.reservedPatternCode })
Require (@($reservedPatternCodes | Sort-Object -Unique).Count -eq $reservedPatternCodes.Count) "ReservedPatternCodeDuplicate"
Require (@($reservedPatternCodes | Where-Object { $_ -in $allPatternCodes }).Count -eq 0) "ReservedPatternCodeAlreadyActive"
Require (@($patternNaming.productionPhases).Count -eq 5) "PatternProductionPhaseCountInvalid"
$expectedProductionOrder = "PackNativeH2,PackNativeH3,LeadPackWithSupport,CrossPackH2,CrossPackH3"
Require ((@($policy.patternProductionOrderCodes) -join ",") -eq $expectedProductionOrder) "PatternProductionOrderPolicyInvalid"
Require ((@($patternNaming.productionPhases.phaseCode) -join ",") -eq "P1,P2,P3,P4,P5") "PatternProductionPhaseOrderInvalid"
Require (@($patternNaming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P1" -and ($_.hierarchyLevelCode -ne "H2" -or $_.plannedCompositionModeCode -ne "SinglePack") }).Count -eq 0) "P1MustBeSinglePackH2"
Require (@($patternNaming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P2" -and ($_.hierarchyLevelCode -ne "H3" -or $_.plannedCompositionModeCode -ne "SinglePack") }).Count -eq 0) "P2MustBeSinglePackH3"
Require (@($patternNaming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P4" -and ($_.hierarchyLevelCode -ne "H2" -or $_.plannedCompositionModeCode -ne "CrossPackTransition") }).Count -eq 0) "P4MustBeCrossPackH2"
Require (@($patternNaming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P5" -and ($_.hierarchyLevelCode -ne "H3" -or $_.plannedCompositionModeCode -ne "CrossPackTransition") }).Count -eq 0) "P5MustBeCrossPackH3"
Require ($h2PatternByStableId.Count -eq $h2.Count) "H2PatternCoverageCountInvalid"
Require ($h3PatternByStableId.Count -eq $h3.Count) "H3PatternCoverageCountInvalid"
foreach ($pattern in @($patternNaming.h2Patterns) + @($patternNaming.h3Patterns)) {
    $stableId = [string] $pattern.stableId
    $patternCode = [string] $pattern.patternCode
    Require ($patternCode -match '^(NATURE|FARM|CITY|TOWN|MIX)-H[23]-[A-Z0-9-]+-\d{2}$') "PatternCodeFormatInvalid:$patternCode"
    Require (-not [string]::IsNullOrWhiteSpace([string] $pattern.displayNameKo)) "PatternDisplayNameMissing:$stableId"
    $supportPacks = @(Array-Property $pattern "supportPackCodes")
    if ([string] $pattern.compositionModeCode -eq "SinglePack") { Require ($supportPacks.Count -eq 0) "SinglePackSupportMustBeEmpty:$stableId" }
    if ([string] $pattern.compositionModeCode -eq "CrossPackTransition") { Require ([string] $pattern.leadPackCode -eq "Mixed" -and $supportPacks.Count -ge 2) "CrossPackPatternInvalid:$stableId" }
}
foreach ($stableId in $h2.Keys) { Require ($h2PatternByStableId.ContainsKey([string] $stableId)) "H2PatternMissing:$stableId" }
foreach ($stableId in $h3.Keys) { Require ($h3PatternByStableId.ContainsKey([string] $stableId)) "H3PatternMissing:$stableId" }

$h2Plans = @()
foreach ($definition in @($h2.Values | Sort-Object stableId)) {
    $targetId = [string] $definition.stableId
    $pattern = $h2PatternByStableId[$targetId]
    $requiredH1Refs = @(Array-Property $definition "requiredH1Refs")
    Require ($requiredH1Refs.Count -ge 2) "H2RequiredH1Insufficient:$targetId"
    foreach ($h1Ref in $requiredH1Refs) { Require ($h1.ContainsKey([string] $h1Ref)) "H2H1Unknown:${targetId}:$h1Ref" }
    $recipeSourceCode = if ($authoredByTarget.ContainsKey($targetId)) { "AuthoredRecipe" } else { "DerivedTheoryRecipe" }
    $nodes = @()
    $edges = @()
    $connectors = @()
    if ($recipeSourceCode -eq "AuthoredRecipe") {
        $recipe = $authoredByTarget[$targetId]
        $nodes = @($recipe.nodes | ForEach-Object { [ordered]@{ nodeId = [string] $_.localNodeId; h1Ref = [string] $_.h1Ref; x = [double] $_.localX; z = [double] $_.localZ; wiIds = @(Array-Property $_ "wiIds"); spatialRoleCodes = @(Array-Property $h1[[string] $_.h1Ref] "spatialRoleCodes"); capacityConceptCodes = @(Array-Property $h1[[string] $_.h1Ref] "capacityConceptCodes") } })
        $edges = @($recipe.edges | ForEach-Object { [ordered]@{ edgeId = [string] $_.localEdgeId; fromNodeId = [string] $_.fromNodeId; toNodeId = [string] $_.toNodeId; relationCode = [string] $_.relationCode } })
        $connectors = @($recipe.externalConnectors | ForEach-Object { [ordered]@{ connectorId = [string] $_.connectorId; roleCode = [string] $_.roleCode; attachedNodeId = [string] $_.attachedNodeId } })
    }
    else {
        for ($index = 0; $index -lt $requiredH1Refs.Count; $index++) {
            $h1Ref = [string] $requiredH1Refs[$index]
            $h1Definition = $h1[$h1Ref]
            $position = Position ([string] $definition.topologyCode) $index $requiredH1Refs.Count ([double] $policy.layoutRules.nodeSpacingMeters)
            $nodeId = "h1-" + (Slug $h1Ref)
            $nodes += [ordered]@{ nodeId = $nodeId; h1Ref = $h1Ref; x = $position.x; z = $position.z; wiIds = @(Array-Property $h1Definition "wiIds" | Where-Object { $_ -in $wiIds }); spatialRoleCodes = @(Array-Property $h1Definition "spatialRoleCodes"); capacityConceptCodes = @(Array-Property $h1Definition "capacityConceptCodes") }
            if ($index -gt 0) { $edges += [ordered]@{ edgeId = "edge-$index"; fromNodeId = [string] $nodes[$index - 1].nodeId; toNodeId = $nodeId; relationCode = "TheoryInternalRoute" } }
        }
        $connectorRoles = @(Array-Property $definition "connectorRoleCodes")
        if ($connectorRoles.Count -eq 0) { $connectorRoles = @("TheoryIngress", "TheoryEgress") }
        elseif ($connectorRoles.Count -eq 1) { $connectorRoles += "TheoryEgress" }
        for ($index = 0; $index -lt $connectorRoles.Count; $index++) {
            $node = if ($index -lt [Math]::Ceiling($connectorRoles.Count / 2.0)) { $nodes[0] } else { $nodes[-1] }
            $connectors += [ordered]@{ connectorId = "connector-$index"; roleCode = [string] $connectorRoles[$index]; attachedNodeId = [string] $node.nodeId }
        }
    }
    Require ($edges.Count -ge ($nodes.Count - 1)) "H2GraphNotConnected:$targetId"
    Require ($connectors.Count -ge 2) "H2IngressEgressMissing:$targetId"
    for ($left = 0; $left -lt $nodes.Count; $left++) {
        for ($right = $left + 1; $right -lt $nodes.Count; $right++) {
            $distance = [Math]::Sqrt([Math]::Pow([double] $nodes[$left].x - [double] $nodes[$right].x, 2) + [Math]::Pow([double] $nodes[$left].z - [double] $nodes[$right].z, 2))
            Require ($distance -ge [double] $policy.layoutRules.minimumNodeSeparationMeters) "H2NodeOverlap:${targetId}:${left}:${right}"
        }
    }
    $gameplayRefs = @($nodes | ForEach-Object { @($_["wiIds"]) } | Sort-Object -Unique)
    $anticipated = @($requiredH1Refs | ForEach-Object { Array-Property $h1[[string] $_] "anticipatedGameplayCodes" } | Sort-Object -Unique)
    Require ($gameplayRefs.Count -gt 0 -or $anticipated.Count -gt 0) "H2GameplayContextMissing:$targetId"
    $placementContract = [ordered]@{ resourceKindCode = "BlockPattern"; placementUnitCode = "H2Block"; placeableAsUnit = $true; localCoordinateSystemCode = "LocalMeters"; spatialFormCode = (Spatial-Form "H2" ([string] $definition.topologyCode)); referenceBoundsMeters = (Reference-Bounds $nodes ([double] $policy.layoutRules.minimumNodeSeparationMeters)); allowedRotationStepDegrees = 90; sizeVariantCodes = @(Array-Property $definition "sizeVariantCodes"); connectionRoleCodes = @($connectors.roleCode) }
    $planCore = [ordered]@{ h2StableId = $targetId; patternNamingRevision = [string] $patternNaming.revision; patternCode = [string] $pattern.patternCode; spatialDisplayNameKo = [string] $pattern.spatialDisplayNameKo; gameplayProfileNameKo = [string] $pattern.displayNameKo; definitionRevision = [int] $definition.revision; topologyCode = [string] $definition.topologyCode; recipeSourceCode = $recipeSourceCode; placementContract = $placementContract; nodes = $nodes; edges = $edges; connectors = $connectors }
    $h2Plans += [ordered]@{ h2StableId = $targetId; patternCode = [string] $pattern.patternCode; displayNameKo = [string] $pattern.spatialDisplayNameKo; spatialDisplayNameKo = [string] $pattern.spatialDisplayNameKo; gameplayProfileNameKo = [string] $pattern.displayNameKo; leadPackCode = [string] $pattern.leadPackCode; supportPackCodes = @(Array-Property $pattern "supportPackCodes"); compositionModeCode = [string] $pattern.compositionModeCode; patternFamilyCode = [string] $pattern.patternFamilyCode; patternSequence = [int] $pattern.patternSequence; resourceKindCode = "BlockPattern"; title = [string] $definition.title; theoryStateCode = [string] $policy.outputStateCodes.h2; topologyCode = [string] $definition.topologyCode; recipeSourceCode = $recipeSourceCode; placementContract = $placementContract; nodes = $nodes; edges = $edges; connectors = $connectors; gameplayWiIds = $gameplayRefs; anticipatedGameplayCodes = $anticipated; theoryHashSha256 = Text-Hash (Stable-Json $planCore) }
}
$h2PlanMap = @{}; foreach ($plan in $h2Plans) { $h2PlanMap[[string] $plan.h2StableId] = $plan }

$h3Plans = @()
foreach ($definition in @($h3.Values | Sort-Object stableId)) {
    $targetId = [string] $definition.stableId
    $pattern = $h3PatternByStableId[$targetId]
    $requiredH2Refs = @(Array-Property $definition "requiredH2Refs")
    Require ($requiredH2Refs.Count -ge 1) "H3RequiredH2Missing:$targetId"
    $nodes = @()
    for ($index = 0; $index -lt $requiredH2Refs.Count; $index++) {
        $h2Ref = [string] $requiredH2Refs[$index]
        Require ($h2PlanMap.ContainsKey($h2Ref)) "H3H2NotTheoryQualified:${targetId}:$h2Ref"
        $position = Position ([string] $definition.topologyCode) $index $requiredH2Refs.Count ([double] $policy.layoutRules.h2SpacingMeters)
        $nodes += [ordered]@{ nodeId = "h2-" + (Slug $h2Ref); h2Ref = $h2Ref; h2PatternCode = [string] $h2PlanMap[$h2Ref].patternCode; h2DisplayNameKo = [string] $h2PlanMap[$h2Ref].displayNameKo; x = $position.x; z = $position.z; h2TheoryHashSha256 = [string] $h2PlanMap[$h2Ref].theoryHashSha256 }
    }
    $edges = @(); for ($index = 1; $index -lt $nodes.Count; $index++) { $edges += [ordered]@{ edgeId = "h3-edge-$index"; fromNodeId = [string] $nodes[$index - 1].nodeId; toNodeId = [string] $nodes[$index].nodeId; relationCode = "TheoryBlockTraversal" } }
    $connectorRoles = @(Array-Property $definition "connectorRoleCodes"); if ($connectorRoles.Count -eq 0) { $connectorRoles = @("TheoryIngress", "TheoryEgress") } elseif ($connectorRoles.Count -eq 1) { $connectorRoles += "TheoryEgress" }
    $connectors = @(); for ($index = 0; $index -lt $connectorRoles.Count; $index++) { $node = if ($index -lt [Math]::Ceiling($connectorRoles.Count / 2.0)) { $nodes[0] } else { $nodes[-1] }; $connectors += [ordered]@{ connectorId = "h3-connector-$index"; roleCode = [string] $connectorRoles[$index]; attachedNodeId = [string] $node.nodeId } }
    Require ($nodes.Count -eq 1 -or $edges.Count -ge ($nodes.Count - 1)) "H3GraphNotConnected:$targetId"
    Require ($connectors.Count -ge 2) "H3ConnectorClosureMissing:$targetId"
    $placementContract = [ordered]@{ resourceKindCode = "LandscapeAssemblyPattern"; placementUnitCode = "H3District"; placeableAsUnit = $true; localCoordinateSystemCode = "LocalMeters"; spatialFormCode = (Spatial-Form "H3" ([string] $definition.topologyCode)); referenceBoundsMeters = (Reference-Bounds $nodes ([double] $policy.layoutRules.h2SpacingMeters / 2.0)); allowedRotationStepDegrees = 90; sizeVariantCodes = @("Reference"); connectionRoleCodes = @($connectors.roleCode) }
    $planCore = [ordered]@{ h3StableId = $targetId; patternNamingRevision = [string] $patternNaming.revision; patternCode = [string] $pattern.patternCode; spatialDisplayNameKo = [string] $pattern.spatialDisplayNameKo; gameplayProfileNameKo = [string] $pattern.displayNameKo; definitionRevision = [int] $definition.revision; topologyCode = [string] $definition.topologyCode; placementContract = $placementContract; nodes = $nodes; edges = $edges; connectors = $connectors }
    $h3Plans += [ordered]@{ h3StableId = $targetId; patternCode = [string] $pattern.patternCode; displayNameKo = [string] $pattern.spatialDisplayNameKo; spatialDisplayNameKo = [string] $pattern.spatialDisplayNameKo; gameplayProfileNameKo = [string] $pattern.displayNameKo; leadPackCode = [string] $pattern.leadPackCode; supportPackCodes = @(Array-Property $pattern "supportPackCodes"); compositionModeCode = [string] $pattern.compositionModeCode; patternFamilyCode = [string] $pattern.patternFamilyCode; patternSequence = [int] $pattern.patternSequence; resourceKindCode = "LandscapeAssemblyPattern"; title = [string] $definition.title; theoryStateCode = [string] $policy.outputStateCodes.h3; topologyCode = [string] $definition.topologyCode; placementContract = $placementContract; nodes = $nodes; edges = $edges; connectors = $connectors; theoryHashSha256 = Text-Hash (Stable-Json $planCore) }
}
$h3PlanMap = @{}; foreach ($plan in $h3Plans) { $h3PlanMap[[string] $plan.h3StableId] = $plan }

$priorityRank = @{}; for ($index = 0; $index -lt @($policy.productionPriority).Count; $index++) { $priorityRank[[string] $policy.productionPriority[$index]] = $index }
$areaSetInstances = @()
foreach ($candidate in @($areaSetPriorities.areaSetCandidates | Sort-Object { $priorityRank[[string] $_.gamePlanCode] })) {
    $h4Ref = [string] $candidate.areaSetCandidateRef
    Require ($h4.ContainsKey($h4Ref)) "AreaSetWorldIntentUnknown:$h4Ref"
    $requiredH3Refs = @($candidate.requiredH3Refs)
    $nodes = @()
    for ($index = 0; $index -lt $requiredH3Refs.Count; $index++) {
        $h3Ref = [string] $requiredH3Refs[$index]
        Require ($h3PlanMap.ContainsKey($h3Ref)) "AreaSetH3NotTheoryQualified:${h4Ref}:$h3Ref"
        $position = Position "ModifiedGrid" $index $requiredH3Refs.Count ([double] $policy.layoutRules.h3SpacingMeters)
        $nodes += [ordered]@{ graphInstanceStableId = "graph:theory:" + (Slug $h3Ref); h3Ref = $h3Ref; h3PatternCode = [string] $h3PlanMap[$h3Ref].patternCode; h3DisplayNameKo = [string] $h3PlanMap[$h3Ref].displayNameKo; x = $position.x; z = $position.z; h3TheoryHashSha256 = [string] $h3PlanMap[$h3Ref].theoryHashSha256 }
    }
    $relations = @(); for ($index = 1; $index -lt $nodes.Count; $index++) { $relations += [ordered]@{ relationStableId = "graph-relation:theory:${index}:" + (Slug $h4Ref); fromGraphInstanceStableId = [string] $nodes[$index - 1].graphInstanceStableId; toGraphInstanceStableId = [string] $nodes[$index].graphInstanceStableId; relationKindCode = "PlayerAndWorkTraversal" } }
    Require ($nodes.Count -eq 1 -or $relations.Count -ge ($nodes.Count - 1)) "AreaSetGraphRelationNotClosed:$h4Ref"
    $areaSetStableId = "area-set:theory:" + (Slug $h4Ref)
    $instanceCore = [ordered]@{ areaSetStableId = $areaSetStableId; worldIntentRef = $h4Ref; gamePlanCode = [string] $candidate.gamePlanCode; graphInstances = $nodes; graphRelations = $relations }
    $areaSetInstances += [ordered]@{ areaSetStableId = $areaSetStableId; title = [string] $candidate.title; worldIntentRef = $h4Ref; gamePlanCode = [string] $candidate.gamePlanCode; evidenceStageCode = "E5"; e5QualificationCode = [string] $policy.outputStateCodes.e5; evidenceKindCode = "TheoryGenerated"; humanReviewed = $false; publicDataBound = $false; runtimeValidated = $false; graphInstances = $nodes; graphRelations = $relations; theoryHashSha256 = Text-Hash (Stable-Json $instanceCore) }
}

$result = [ordered]@{
    schemaVersion = "simulation-world-theory-spatial-factory-output.v1"
    revision = "simulation-world-theory-spatial-factory-output.r2"
    policyRevision = [string] $policy.revision
    patternNamingRevision = [string] $patternNaming.revision
    generatedAtRuleCode = "DeterministicNoWallClock"
    counts = [ordered]@{ h1Inputs = $h1.Count; h2TheoryQualified = $h2Plans.Count; h3TheoryQualified = $h3Plans.Count; e5TheoryQualifiedAreaSets = $areaSetInstances.Count; authoredH2RecipesReused = @($h2Plans | Where-Object recipeSourceCode -eq "AuthoredRecipe").Count; derivedH2Recipes = @($h2Plans | Where-Object recipeSourceCode -eq "DerivedTheoryRecipe").Count; reservedExpansionPatterns = $reservedPatternCodes.Count }
    h2Plans = $h2Plans
    h3Plans = $h3Plans
    e5AreaSetInstances = $areaSetInstances
    interAreaSetRelations = @($areaSetPriorities.interAreaSetRelations)
    inventoryTargets = @($patternNaming.inventoryTargets)
    productionPhases = @($patternNaming.productionPhases)
    priorityExpansionQueue = @($patternNaming.priorityExpansionQueue)
    humanReviewModeCode = [string] $policy.humanReviewPolicy.modeCode
    authorityBoundary = [ordered]@{ humanApprovalNotClaimed = $true; publicDataNotBound = $true; runtimeNotValidated = $true; e6AndE7RemainSeparate = $true }
}
$json = Normalize (Stable-Json $result)

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# 이론 기반 H2·H3·E5 공간 생산 결과")
[void] $builder.AppendLine()
[void] $builder.AppendLine("사람 검토를 생산 관문으로 사용하지 않고 결정적 공간 이론 규칙으로 반복 생성한 결과다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- H1 입력: $($result.counts.h1Inputs)")
[void] $builder.AppendLine("- H2 이론 적격: $($result.counts.h2TheoryQualified) (수기 조립법 $($result.counts.authoredH2RecipesReused), 자동 유도 $($result.counts.derivedH2Recipes))")
[void] $builder.AppendLine("- H3 이론 적격: $($result.counts.h3TheoryQualified)")
[void] $builder.AppendLine("- 이론 E5 AreaSet 인스턴스: $($result.counts.e5TheoryQualifiedAreaSets)")
[void] $builder.AppendLine("- 패턴 이름 대장: ``$($result.patternNamingRevision)``")
[void] $builder.AppendLine("- 사람 검토: ``$($result.humanReviewModeCode)`` · 생산 비차단")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 우선 | 이론 AreaSet | 게임 기획 | H3 패턴 | E5 상태 |")
[void] $builder.AppendLine("| ---: | --- | --- | --- | --- |")
$rank = 1
foreach ($instance in $areaSetInstances) { $patterns = @($instance.graphInstances.h3PatternCode) -join ", "; [void] $builder.AppendLine("| $rank | ``$($instance.areaSetStableId)`` | ``$($instance.gamePlanCode)`` | $patterns | ``$($instance.e5QualificationCode)`` |"); $rank++ }
[void] $builder.AppendLine()
[void] $builder.AppendLine("## H2 팩 주도 패턴")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 패턴 코드 | 배치 공간 이름 | 게임플레이 활용 유형 | 공간 형태 | 기준 크기 | 기존 StableId |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- |")
foreach ($plan in @($h2Plans | Sort-Object { [string] $_.patternCode })) { $bounds = $plan.placementContract.referenceBoundsMeters; [void] $builder.AppendLine("| ``$($plan.patternCode)`` | $($plan.spatialDisplayNameKo) | $($plan.gameplayProfileNameKo) | ``$($plan.placementContract.spatialFormCode)`` | $($bounds.width)m × $($bounds.depth)m | ``$($plan.h2StableId)`` |") }
[void] $builder.AppendLine()
[void] $builder.AppendLine("## H3 팩 주도 패턴")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 패턴 코드 | 배치 구역 이름 | 게임플레이 활용 유형 | 구역 형태 | 포함 H2 패턴 | 기존 StableId |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- |")
foreach ($plan in @($h3Plans | Sort-Object { [string] $_.patternCode })) { $children = @($plan.nodes.h2PatternCode) -join ", "; [void] $builder.AppendLine("| ``$($plan.patternCode)`` | $($plan.spatialDisplayNameKo) | $($plan.gameplayProfileNameKo) | ``$($plan.placementContract.spatialFormCode)`` | $children | ``$($plan.h3StableId)`` |") }
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 증거 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- ``E5TheoryQualified``는 명시적 세계 의도와 H3를 가진 특정 Theory AreaSet 공간 인스턴스다.")
[void] $builder.AppendLine("- 사람 승인, 공공데이터 결속, Unity Runtime 또는 실제 플레이를 주장하지 않는다.")
[void] $builder.AppendLine("- 공공데이터는 E6, 실제 서버·저장 Scene 플레이는 E7에서 별도로 검증한다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 다음 패턴 확장 대기열")
[void] $builder.AppendLine()
[void] $builder.AppendLine("생산 순서는 팩 단독 H2 → 팩 내부 H3 → 주도·보조 팩 조합 → 혼합 H2 → 혼합 H3다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 우선 | 예약 패턴 코드 | 한국어 이름 | 단계 | 조합 | 게임 플레이 목적 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- |")
foreach ($candidate in @($patternNaming.priorityExpansionQueue | Sort-Object priorityCode, reservedPatternCode)) { [void] $builder.AppendLine("| ``$($candidate.priorityCode)`` | ``$($candidate.reservedPatternCode)`` | $($candidate.displayNameKo) | ``$($candidate.hierarchyLevelCode)`` | ``$($candidate.plannedCompositionModeCode)`` | ``$($candidate.gameplayPurposeCode)`` |") }
$markdown = Normalize $builder.ToString()

$jsonPath = Resolve-RepoPath $JsonOutputPath
$markdownPath = Resolve-RepoPath $MarkdownOutputPath
if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $jsonPath) "JsonOutputMissing"
    Require (Test-Path -LiteralPath $markdownPath) "MarkdownOutputMissing"
    Require ((Normalize ([IO.File]::ReadAllText($jsonPath))) -ceq $json) "JsonOutputStale"
    Require ((Normalize ([IO.File]::ReadAllText($markdownPath))) -ceq $markdown) "MarkdownOutputStale"
    Write-Output "TheorySpatialFactoryValid:H2=$($h2Plans.Count);H3=$($h3Plans.Count);E5=$($areaSetInstances.Count);HumanReview=Deferred"
    exit 0
}

foreach ($pair in @(@($jsonPath, $json), @($markdownPath, $markdown))) {
    $directory = Split-Path -Parent $pair[0]
    if (-not (Test-Path -LiteralPath $directory)) { [void] (New-Item -ItemType Directory -Path $directory) }
    if (-not (Test-Path -LiteralPath $pair[0]) -or (Normalize ([IO.File]::ReadAllText($pair[0]))) -cne [string] $pair[1]) { [IO.File]::WriteAllText($pair[0], [string] $pair[1], [Text.UTF8Encoding]::new($false)) }
}
Write-Output "TheorySpatialFactoryGenerated:H2=$($h2Plans.Count);H3=$($h3Plans.Count);E5=$($areaSetInstances.Count);HumanReview=Deferred"
