param(
    [ValidateSet("Check", "Write")]
    [string] $Mode = "Check",
    [string] $PolicyPath = "eng/world-seedbeds/theory-spatial-factory-policy.v1.json",
    [string] $SemanticRelationsPath = "",
    [string] $JsonOutputPath = "eng/world-seedbeds/generated/theory-spatial-factory.v1.json",
    [string] $MarkdownOutputPath = "docs/AI/generated/theory-spatial-factory.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path

function Resolve-RepoPath([string] $relativePath) { return Join-Path $repositoryRoot ($relativePath -replace "/", [IO.Path]::DirectorySeparatorChar) }
function Read-Json([string] $relativePath) {
    $path = Resolve-RepoPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) { throw "JsonMissing:$relativePath" }
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Require([bool] $condition, [string] $message) { if (-not $condition) { throw $message } }
function Normalize([string] $value) { return (($value -replace "`r`n", "`n").TrimEnd()) + "`n" }
function Write-TextIfChanged([string] $path, [string] $content) {
    if (Test-Path -LiteralPath $path) { if ((Normalize ([IO.File]::ReadAllText($path))) -ceq (Normalize $content)) { return } }
    for ($attempt = 1; $attempt -le 5; $attempt++) {
        try { [IO.File]::WriteAllText($path, $content, [Text.UTF8Encoding]::new($false)); return }
        catch [IO.IOException] { if ($attempt -eq 5) { throw }; Start-Sleep -Milliseconds (100 * $attempt) }
    }
}
function Stable-Json([object] $value) { return (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n") }
function Text-Hash([string] $value) { return ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($value)))).ToLowerInvariant() }
function Array-Property([object] $value, [string] $name) { if ($null -eq $value.PSObject.Properties[$name]) { return @() }; return @($value.$name) }
function Slug([string] $stableId) { return (($stableId -replace '^[^:]+:', '') -replace '[^a-zA-Z0-9-]', '-') }
function Same-Set([object[]] $left, [object[]] $right) { return ((@($left | ForEach-Object { [string] $_ } | Sort-Object) -join "|") -ceq (@($right | ForEach-Object { [string] $_ } | Sort-Object) -join "|")) }
function Relation-StableId([string] $scope, [object] $relation) {
    $key = @([string] $relation.fromRef, [string] $relation.fromConnectorRoleCode, [string] $relation.toRef, [string] $relation.toConnectorRoleCode, [string] $relation.relationKindCode, [string] $relation.relationDirectionCode, [string] $relation.compatibilityRuleCode) -join "|"
    return "relation:${scope}:" + (Text-Hash $key).Substring(0, 16)
}

function Position([string] $topology, [int] $index, [int] $count, [double] $spacing) {
    switch ($topology) {
        "Linear" { return [ordered]@{ x = [Math]::Round(($index - (($count - 1) / 2.0)) * $spacing, 2); z = 0.0 } }
        { $_ -in @("Grid", "ModifiedGrid") } {
            $columns = [Math]::Ceiling([Math]::Sqrt($count)); $row = [Math]::Floor($index / $columns); $column = $index % $columns
            return [ordered]@{ x = [Math]::Round(($column - (($columns - 1) / 2.0)) * $spacing, 2); z = [Math]::Round(($row - .5) * $spacing, 2) }
        }
        { $_ -in @("Radial", "Cluster") } {
            $angle = (2.0 * [Math]::PI * $index) / [Math]::Max(1, $count)
            return [ordered]@{ x = [Math]::Round([Math]::Cos($angle) * $spacing, 2); z = [Math]::Round([Math]::Sin($angle) * $spacing, 2) }
        }
        default { return [ordered]@{ x = [Math]::Round(($index - (($count - 1) / 2.0)) * $spacing, 2); z = if (($index % 2) -eq 0) { -$spacing * .35 } else { $spacing * .35 } } }
    }
}

function Spatial-Form([string] $level, [string] $topology) {
    if ($level -eq "H2") {
        switch ($topology) { "Linear" { return "LinearBlock" }; { $_ -in @("Grid", "ModifiedGrid") } { return "StreetBlock" }; "Radial" { return "RingBlock" }; "Cluster" { return "CompoundBlock" }; default { return "TerrainAdaptiveBlock" } }
    }
    switch ($topology) { "Linear" { return "CorridorAssembly" }; { $_ -in @("Grid", "ModifiedGrid") } { return "DistrictAssembly" }; { $_ -in @("Radial", "Cluster") } { return "CampusAssembly" }; default { return "LandscapeDistrictAssembly" } }
}

function Reference-Bounds([object[]] $nodes, [double] $padding) {
    $xs = @($nodes | ForEach-Object { [double] $_.x }); $zs = @($nodes | ForEach-Object { [double] $_.z })
    $minX = [Math]::Round(($xs | Measure-Object -Minimum).Minimum - $padding, 2); $maxX = [Math]::Round(($xs | Measure-Object -Maximum).Maximum + $padding, 2)
    $minZ = [Math]::Round(($zs | Measure-Object -Minimum).Minimum - $padding, 2); $maxZ = [Math]::Round(($zs | Measure-Object -Maximum).Maximum + $padding, 2)
    return [ordered]@{ minimumX = $minX; minimumZ = $minZ; maximumX = $maxX; maximumZ = $maxZ; width = [Math]::Round($maxX - $minX, 2); depth = [Math]::Round($maxZ - $minZ, 2) }
}

function Load-Definitions([object[]] $refs) {
    $map = @{}
    foreach ($ref in $refs) { $map[[string] $ref.stableId] = Read-Json ("eng/world-seedbeds/synty-bottom-up-inventory/" + [string] $ref.definitionPath) }
    return $map
}

function New-ProfileMap([object[]] $profiles) {
    $map = @{}
    foreach ($profile in $profiles) {
        $childRef = [string] $profile.childRef
        Require (-not $map.ContainsKey($childRef)) "MalformedContract:ChildConnectorProfileDuplicate:$childRef"
        $connectorMap = @{}
        foreach ($connector in @($profile.connectors)) {
            $role = [string] $connector.roleCode
            Require (-not $connectorMap.ContainsKey($role)) "MalformedContract:ChildConnectorRoleDuplicate:${childRef}:$role"
            $connectorMap[$role] = $connector
        }
        $map[$childRef] = $connectorMap
    }
    return $map
}

function Test-UndirectedConnected([string[]] $children, [object[]] $relations) {
    if ($children.Count -le 1) { return $true }
    $adjacency = @{}; foreach ($child in $children) { $adjacency[$child] = [Collections.Generic.List[string]]::new() }
    foreach ($relation in $relations) { $adjacency[[string] $relation.fromRef].Add([string] $relation.toRef); $adjacency[[string] $relation.toRef].Add([string] $relation.fromRef) }
    $seen = [Collections.Generic.HashSet[string]]::new(); $queue = [Collections.Generic.Queue[string]]::new(); $queue.Enqueue($children[0]); [void] $seen.Add($children[0])
    while ($queue.Count -gt 0) { $current = $queue.Dequeue(); foreach ($next in $adjacency[$current]) { if ($seen.Add($next)) { $queue.Enqueue($next) } } }
    return $seen.Count -eq $children.Count
}

function Test-DirectedReachable([string] $fromRef, [string] $toRef, [string] $movementKind, [object[]] $relations) {
    if ($fromRef -eq $toRef) { return $true }
    $adjacency = @{}
    foreach ($relation in @($relations | Where-Object { [string] $_.relationKindCode -eq $movementKind })) {
        $from = [string] $relation.fromRef; $to = [string] $relation.toRef
        if (-not $adjacency.ContainsKey($from)) { $adjacency[$from] = [Collections.Generic.List[string]]::new() }; $adjacency[$from].Add($to)
        if ([string] $relation.relationDirectionCode -eq "Bidirectional") { if (-not $adjacency.ContainsKey($to)) { $adjacency[$to] = [Collections.Generic.List[string]]::new() }; $adjacency[$to].Add($from) }
    }
    $seen = [Collections.Generic.HashSet[string]]::new(); $queue = [Collections.Generic.Queue[string]]::new(); $queue.Enqueue($fromRef); [void] $seen.Add($fromRef)
    while ($queue.Count -gt 0) { $current = $queue.Dequeue(); if (-not $adjacency.ContainsKey($current)) { continue }; foreach ($next in $adjacency[$current]) { if ($next -eq $toRef) { return $true }; if ($seen.Add($next)) { $queue.Enqueue($next) } } }
    return $false
}

function Evaluate-SemanticRecipe([string] $scope, [object] $recipe, [string[]] $children, [object[]] $childProfiles, [object] $ledger) {
    Require (Same-Set $children @($recipe.requiredChildRefs)) "MalformedContract:RequiredChildrenMismatch:$scope"
    $directionCodes = @($ledger.allowedDirectionCodes); $relationDirectionCodes = @($ledger.allowedRelationDirectionCodes); $movementKinds = @($ledger.allowedMovementKindCodes)
    $ruleMap = @{}; foreach ($rule in @($ledger.compatibilityRules)) { $ruleMap[[string] $rule.ruleCode] = $rule }
    $profiles = New-ProfileMap $childProfiles
    foreach ($child in $children) { Require ($profiles.ContainsKey($child)) "MalformedContract:ChildConnectorProfileMissing:${scope}:$child" }
    foreach ($child in $children) {
        foreach ($connector in $profiles[$child].Values) {
            Require (-not [string]::IsNullOrWhiteSpace([string] $connector.connectorStableId)) "MalformedContract:ConnectorStableIdMissing:${scope}:$child"
            Require ([string] $connector.directionCode -in $directionCodes) "MalformedContract:ConnectorDirectionUnknown:${scope}:$child"
            Require (@($connector.movementKindCodes | Where-Object { [string] $_ -notin $movementKinds }).Count -eq 0) "MalformedContract:ConnectorMovementKindUnknown:${scope}:$child"
        }
    }
    $unresolved = @(); $resolvedRelations = @(); $relationIds = [Collections.Generic.HashSet[string]]::new()
    foreach ($relation in @($recipe.relations)) {
        $fromRef = [string] $relation.fromRef; $toRef = [string] $relation.toRef
        Require ($fromRef -in $children -and $toRef -in $children) "MalformedContract:RelationChildUnknown:${scope}:${fromRef}:${toRef}"
        $kind = [string] $relation.relationKindCode; $relationDirection = [string] $relation.relationDirectionCode; $ruleCode = [string] $relation.compatibilityRuleCode
        Require ($kind -in $movementKinds) "MalformedContract:MovementKindUnknown:${scope}:$kind"
        Require ($relationDirection -in $relationDirectionCodes) "MalformedContract:RelationDirectionUnknown:${scope}:$relationDirection"
        Require ($ruleMap.ContainsKey($ruleCode)) "MalformedContract:CompatibilityRuleUnknown:${scope}:$ruleCode"
        $fromRole = [string] $relation.fromConnectorRoleCode; $toRole = [string] $relation.toConnectorRoleCode
        if (-not $profiles[$fromRef].ContainsKey($fromRole) -or -not $profiles[$toRef].ContainsKey($toRole)) {
            $unresolved += [ordered]@{ code = "SemanticRelationUnresolved"; reasonCode = "RequiredConnectorMissing"; relationCode = [string] $relation.relationCode; fromRef = $fromRef; toRef = $toRef }
            continue
        }
        $fromConnector = $profiles[$fromRef][$fromRole]; $toConnector = $profiles[$toRef][$toRole]; $rule = $ruleMap[$ruleCode]
        Require ([string] $fromConnector.directionCode -in $directionCodes -and [string] $toConnector.directionCode -in $directionCodes) "MalformedContract:ConnectorDirectionUnknown:$scope"
        Require ([string] $rule.relationDirectionCode -eq $relationDirection) "MalformedContract:CompatibilityDirectionMismatch:${scope}:$ruleCode"
        Require ([string] $fromConnector.directionCode -in @($rule.allowedFromDirectionCodes) -and [string] $toConnector.directionCode -in @($rule.allowedToDirectionCodes)) "MalformedContract:ConnectorDirectionIncompatible:${scope}:$($relation.relationCode)"
        if ($kind -notin @($fromConnector.movementKindCodes) -or $kind -notin @($toConnector.movementKindCodes)) {
            $unresolved += [ordered]@{ code = "SemanticRelationUnresolved"; reasonCode = "MovementKindUnavailable"; relationCode = [string] $relation.relationCode; movementKindCode = $kind }
            continue
        }
        $stableId = Relation-StableId $scope $relation
        Require ($relationIds.Add($stableId)) "MalformedContract:RelationStableIdDuplicate:${scope}:$stableId"
        $resolvedRelations += [ordered]@{ relationStableId = $stableId; relationCode = [string] $relation.relationCode; fromRef = $fromRef; fromConnectorRoleCode = $fromRole; toRef = $toRef; toConnectorRoleCode = $toRole; relationKindCode = $kind; relationDirectionCode = $relationDirection; compatibilityRuleCode = $ruleCode }
    }
    $structureQualified = Test-UndirectedConnected $children @($recipe.relations)
    $exposed = @(); $exposedRoles = [Collections.Generic.HashSet[string]]::new()
    foreach ($connector in @($recipe.exposedConnectors)) {
        $sourceRef = [string] $connector.sourceChildRef; $sourceRole = [string] $connector.sourceChildConnectorRoleCode; $role = [string] $connector.roleCode
        Require ($sourceRef -in $children) "MalformedContract:ExposedConnectorChildUnknown:${scope}:$sourceRef"
        Require ($exposedRoles.Add($role)) "MalformedContract:ExposedConnectorRoleDuplicate:${scope}:$role"
        Require ([string] $connector.directionCode -in $directionCodes) "MalformedContract:ExposedConnectorDirectionUnknown:${scope}:$role"
        Require (@($connector.movementKindCodes | Where-Object { [string] $_ -notin $movementKinds }).Count -eq 0) "MalformedContract:ExposedConnectorMovementKindUnknown:${scope}:$role"
        if (-not $profiles[$sourceRef].ContainsKey($sourceRole)) { $unresolved += [ordered]@{ code = "SemanticRelationUnresolved"; reasonCode = "ExposedSourceConnectorMissing"; roleCode = $role; sourceChildRef = $sourceRef }; continue }
        $source = $profiles[$sourceRef][$sourceRole]
        $exposed += [ordered]@{ connectorStableId = [string] $connector.connectorStableId; roleCode = $role; directionCode = [string] $connector.directionCode; movementKindCodes = @($connector.movementKindCodes | Sort-Object -Unique); sourceChildRef = $sourceRef; sourceChildConnectorRoleCode = $sourceRole; sourceChildConnectorStableId = [string] $source.connectorStableId }
    }
    foreach ($flow in @($recipe.flowRequirements)) {
        $from = @($exposed | Where-Object roleCode -eq ([string] $flow.fromConnectorRoleCode)); $to = @($exposed | Where-Object roleCode -eq ([string] $flow.toConnectorRoleCode))
        if ($from.Count -ne 1 -or $to.Count -ne 1 -or -not (Test-DirectedReachable ([string] $from[0].sourceChildRef) ([string] $to[0].sourceChildRef) ([string] $flow.movementKindCode) $resolvedRelations)) {
            $unresolved += [ordered]@{ code = "SemanticRelationUnresolved"; reasonCode = "RequiredFlowNotReachable"; flowRequirementStableId = [string] $flow.flowRequirementStableId }
        }
    }
    $semanticQualified = $structureQualified -and $unresolved.Count -eq 0
    return [ordered]@{ structureQualificationCode = if ($structureQualified) { "StructureQualified" } else { "StructureUnresolved" }; semanticQualificationCode = if ($semanticQualified) { "TheoryQualified" } else { "SemanticRelationUnresolved" }; closureStateCode = if ($semanticQualified) { "Closed" } else { "Unresolved" }; semanticRelations = @($resolvedRelations | Sort-Object { [string] $_.relationStableId }); exposedConnectors = @($exposed | Sort-Object { [string] $_.connectorStableId }); flowRequirements = @($recipe.flowRequirements | Sort-Object flowRequirementStableId); unresolvedItems = @($unresolved | Sort-Object { [string] $_["reasonCode"] }, { [string] $_["relationCode"] }) }
}

$policy = Read-Json $PolicyPath
Require ([string] $policy.schemaVersion -eq "simulation-world-theory-spatial-factory-policy.v1") "PolicySchemaInvalid"
Require (-not [bool] $policy.humanReviewPolicy.blocking) "HumanReviewMustNotBlockFactory"
Require ([bool] $policy.authorityBoundary.e6AndE7RemainSeparate) "E6E7BoundaryRequired"
$catalog = Read-Json ([string] $policy.sourceCatalogPath); $authoredRecipes = Read-Json ([string] $policy.sourceAuthoredRecipePath); $priorities = Read-Json ([string] $policy.sourceAreaSetPriorityPath)
$worldInteractions = Read-Json ([string] $policy.sourceWorldInteractionPath); $patternNaming = Read-Json ([string] $policy.sourcePatternNamingPath)
$semanticLedger = Read-Json $(if ([string]::IsNullOrWhiteSpace($SemanticRelationsPath)) { [string] $policy.sourceSemanticRelationsPath } else { $SemanticRelationsPath })
Require ([string] $semanticLedger.schemaVersion -eq "simulation-world-semantic-spatial-relations.v1") "SemanticRelationSchemaInvalid"
Require ([bool] $semanticLedger.authorityBoundary.positionIndependent -and [bool] $semanticLedger.authorityBoundary.publicDataForbidden -and [bool] $semanticLedger.authorityBoundary.unityAssetReferencesForbidden) "SemanticRelationAuthorityBoundaryInvalid"
$h1 = Load-Definitions @($catalog.h1InteractionDefinitionRefs); $h2 = Load-Definitions @($catalog.h2DefinitionRefs); $h3 = Load-Definitions @($catalog.h3DefinitionRefs); $h4 = Load-Definitions @($catalog.h4DefinitionRefs)
$wiIds = @($worldInteractions.items.id)
$authoredByTarget = @{}; foreach ($recipe in @($authoredRecipes.recipes)) { $authoredByTarget[[string] $recipe.targetKnowledgeRef] = $recipe }
$h2PatternById = @{}; foreach ($pattern in @($patternNaming.h2Patterns)) { $h2PatternById[[string] $pattern.stableId] = $pattern }
$h3PatternById = @{}; foreach ($pattern in @($patternNaming.h3Patterns)) { $h3PatternById[[string] $pattern.stableId] = $pattern }
$h2RecipeById = @{}; foreach ($recipe in @($semanticLedger.h2RelationRecipes)) { $h2RecipeById[[string] $recipe.targetRef] = $recipe }
$h3RecipeById = @{}; foreach ($recipe in @($semanticLedger.h3RelationRecipes)) { $h3RecipeById[[string] $recipe.targetRef] = $recipe }
$areaRecipeById = @{}; foreach ($recipe in @($semanticLedger.areaSetRelationRecipes)) { $areaRecipeById[[string] $recipe.targetRef] = $recipe }
Require ($h2PatternById.Count -eq $h2.Count -and $h2RecipeById.Count -eq $h2.Count) "H2SemanticCoverageInvalid"
Require ($h3PatternById.Count -eq $h3.Count -and $h3RecipeById.Count -eq $h3.Count) "H3SemanticCoverageInvalid"

$allPatternCodes = @($patternNaming.h2Patterns.patternCode) + @($patternNaming.h3Patterns.patternCode)
Require (@($allPatternCodes | Sort-Object -Unique).Count -eq $allPatternCodes.Count) "PatternCodeDuplicate"
$newQueue = @($patternNaming.priorityExpansionQueue | Where-Object { [string] $_.workKindCode -eq "NewCandidate" })
$revisionQueue = @($patternNaming.priorityExpansionQueue | Where-Object { [string] $_.workKindCode -in @("RevisionExpansion", "SemanticRepair") })
foreach ($item in $newQueue) { Require (-not [string]::IsNullOrWhiteSpace([string] $item.reservedPatternCode)) "QueueReservedPatternCodeMissing"; Require ([string] $item.reservedPatternCode -notin $allPatternCodes) "ReservedPatternCodeAlreadyActive" }
foreach ($item in $revisionQueue) { Require (-not [string]::IsNullOrWhiteSpace([string] $item.targetStableId)) "QueueTargetStableIdMissing"; Require ($h2.ContainsKey([string] $item.targetStableId) -or $h3.ContainsKey([string] $item.targetStableId)) "QueueTargetStableIdUnknown:$($item.targetStableId)" }

$h2Plans = @()
foreach ($definition in @($h2.Values | Sort-Object stableId)) {
    $target = [string] $definition.stableId; Require ($h2RecipeById.ContainsKey($target)) "H2SemanticRecipeMissing:$target"; $recipe = $h2RecipeById[$target]; $pattern = $h2PatternById[$target]
    $children = @($recipe.requiredChildRefs | ForEach-Object { [string] $_ }); Require ($children.Count -ge 2) "H2RequiredH1Insufficient:$target"; Require (Same-Set $children @(Array-Property $definition "requiredH1Refs")) "H2DefinitionSemanticChildrenMismatch:$target"
    foreach ($child in $children) { Require ($h1.ContainsKey($child)) "H2H1Unknown:${target}:$child" }
    $authored = if ($authoredByTarget.ContainsKey($target)) { $authoredByTarget[$target] } else { $null }; $nodes = @()
    for ($index = 0; $index -lt $children.Count; $index++) {
        $child = $children[$index]; $position = Position ([string] $definition.topologyCode) $index $children.Count ([double] $policy.layoutRules.nodeSpacingMeters)
        if ($null -ne $authored) { $match = @($authored.nodes | Where-Object h1Ref -eq $child); if ($match.Count -eq 1) { $position = [ordered]@{ x = [double] $match[0].localX; z = [double] $match[0].localZ } } }
        $childDefinition = $h1[$child]; $nodes += [ordered]@{ nodeId = "h1-" + (Slug $child); h1Ref = $child; x = $position.x; z = $position.z; wiIds = @(Array-Property $childDefinition "wiIds" | Where-Object { $_ -in $wiIds } | Sort-Object -Unique); spatialRoleCodes = @(Array-Property $childDefinition "spatialRoleCodes" | Sort-Object -Unique); capacityConceptCodes = @(Array-Property $childDefinition "capacityConceptCodes" | Sort-Object -Unique) }
    }
    for ($left = 0; $left -lt $nodes.Count; $left++) { for ($right = $left + 1; $right -lt $nodes.Count; $right++) { $distance = [Math]::Sqrt([Math]::Pow([double] $nodes[$left].x - [double] $nodes[$right].x, 2) + [Math]::Pow([double] $nodes[$left].z - [double] $nodes[$right].z, 2)); Require ($distance -ge [double] $policy.layoutRules.minimumNodeSeparationMeters) "H2NodeOverlap:${target}:${left}:${right}" } }
    $semantic = Evaluate-SemanticRecipe ("h2-" + (Slug $target)) $recipe $children @($recipe.childConnectorProfiles) $semanticLedger
    Require ([string] $semantic.semanticQualificationCode -eq "TheoryQualified") "H2SemanticRelationUnresolved:$target"
    $edges = @($semantic.semanticRelations | ForEach-Object { [ordered]@{ edgeId = [string] $_.relationStableId; fromNodeId = "h1-" + (Slug ([string] $_.fromRef)); toNodeId = "h1-" + (Slug ([string] $_.toRef)); relationCode = [string] $_.relationKindCode; relationDirectionCode = [string] $_.relationDirectionCode } } | Sort-Object { [string] $_.edgeId })
    $connectors = @($semantic.exposedConnectors | ForEach-Object { [ordered]@{ connectorId = [string] $_.connectorStableId; roleCode = [string] $_.roleCode; attachedNodeId = "h1-" + (Slug ([string] $_.sourceChildRef)); directionCode = [string] $_.directionCode; movementKindCodes = @($_.movementKindCodes) } } | Sort-Object { [string] $_.connectorId })
    $placement = [ordered]@{ resourceKindCode = "BlockPattern"; placementUnitCode = "H2Block"; placeableAsUnit = $true; localCoordinateSystemCode = "LocalMeters"; spatialFormCode = Spatial-Form "H2" ([string] $definition.topologyCode); referenceBoundsMeters = Reference-Bounds $nodes ([double] $policy.layoutRules.minimumNodeSeparationMeters); allowedRotationStepDegrees = 90; sizeVariantCodes = @(Array-Property $definition "sizeVariantCodes"); connectionRoleCodes = @($connectors.roleCode | Sort-Object -Unique) }
    $gameplay = @($nodes | ForEach-Object { @($_["wiIds"]) } | Sort-Object -Unique); $anticipated = @($children | ForEach-Object { Array-Property $h1[$_] "anticipatedGameplayCodes" } | Sort-Object -Unique); Require ($gameplay.Count -gt 0 -or $anticipated.Count -gt 0) "H2GameplayContextMissing:$target"
    $sortedNodes = @($nodes | Sort-Object { [string] $_.h1Ref })
    $core = [ordered]@{ h2StableId = $target; definitionRevision = [int] $definition.revision; semanticLedgerRevision = [string] $semanticLedger.revision; topologyCode = [string] $definition.topologyCode; placementContract = $placement; nodes = $sortedNodes; semanticRelations = $semantic.semanticRelations; exposedConnectors = $semantic.exposedConnectors; flowRequirements = $semantic.flowRequirements }
    $h2Plans += [ordered]@{ h2StableId = $target; patternCode = [string] $pattern.patternCode; displayNameKo = [string] $pattern.spatialDisplayNameKo; spatialDisplayNameKo = [string] $pattern.spatialDisplayNameKo; gameplayProfileNameKo = [string] $pattern.displayNameKo; leadPackCode = [string] $pattern.leadPackCode; supportPackCodes = @(Array-Property $pattern "supportPackCodes"); compositionModeCode = [string] $pattern.compositionModeCode; patternFamilyCode = [string] $pattern.patternFamilyCode; patternSequence = [int] $pattern.patternSequence; resourceKindCode = "BlockPattern"; title = [string] $definition.title; theoryStateCode = [string] $semantic.semanticQualificationCode; structureQualificationCode = [string] $semantic.structureQualificationCode; closureStateCode = [string] $semantic.closureStateCode; topologyCode = [string] $definition.topologyCode; recipeSourceCode = if ($null -ne $authored) { "AuthoredRecipe" } else { "DerivedTheoryRecipe" }; placementContract = $placement; nodes = $sortedNodes; edges = $edges; connectors = $connectors; semanticRelations = $semantic.semanticRelations; exposedConnectors = $semantic.exposedConnectors; flowRequirements = $semantic.flowRequirements; unresolvedSemanticRelations = $semantic.unresolvedItems; gameplayWiIds = $gameplay; anticipatedGameplayCodes = $anticipated; theoryHashSha256 = Text-Hash (Stable-Json $core) }
}
$h2Map = @{}; foreach ($plan in $h2Plans) { $h2Map[[string] $plan.h2StableId] = $plan }

$h3Plans = @()
foreach ($definition in @($h3.Values | Sort-Object stableId)) {
    $target = [string] $definition.stableId; $recipe = $h3RecipeById[$target]; $pattern = $h3PatternById[$target]; $children = @($recipe.requiredChildRefs | ForEach-Object { [string] $_ })
    Require ($children.Count -ge 2) "H3RequiredH2Insufficient:$target"; Require (Same-Set $children @(Array-Property $definition "requiredH2Refs")) "H3DefinitionSemanticChildrenMismatch:$target"
    $nodes = @(); $profiles = @()
    for ($index = 0; $index -lt $children.Count; $index++) {
        $child = $children[$index]; Require ($h2Map.ContainsKey($child)) "H3H2NotTheoryQualified:${target}:$child"; $childPlan = $h2Map[$child]; $position = Position ([string] $definition.topologyCode) $index $children.Count ([double] $policy.layoutRules.h2SpacingMeters)
        $nodes += [ordered]@{ nodeId = "h2-" + (Slug $child); h2Ref = $child; h2PatternCode = [string] $childPlan.patternCode; h2DisplayNameKo = [string] $childPlan.displayNameKo; x = $position.x; z = $position.z; h2TheoryHashSha256 = [string] $childPlan.theoryHashSha256 }
        $profiles += [ordered]@{ childRef = $child; connectors = @($childPlan.exposedConnectors) }
    }
    $semantic = Evaluate-SemanticRecipe ("h3-" + (Slug $target)) $recipe $children $profiles $semanticLedger; Require ([string] $semantic.semanticQualificationCode -eq "TheoryQualified") "H3SemanticRelationUnresolved:$target"
    $edges = @($semantic.semanticRelations | ForEach-Object { [ordered]@{ edgeId = [string] $_.relationStableId; fromNodeId = "h2-" + (Slug ([string] $_.fromRef)); toNodeId = "h2-" + (Slug ([string] $_.toRef)); relationCode = [string] $_.relationKindCode; relationDirectionCode = [string] $_.relationDirectionCode } } | Sort-Object { [string] $_.edgeId })
    $connectors = @($semantic.exposedConnectors | ForEach-Object { [ordered]@{ connectorId = [string] $_.connectorStableId; roleCode = [string] $_.roleCode; attachedNodeId = "h2-" + (Slug ([string] $_.sourceChildRef)); directionCode = [string] $_.directionCode; movementKindCodes = @($_.movementKindCodes) } } | Sort-Object { [string] $_.connectorId })
    $placement = [ordered]@{ resourceKindCode = "LandscapeAssemblyPattern"; placementUnitCode = "H3District"; placeableAsUnit = $true; localCoordinateSystemCode = "LocalMeters"; spatialFormCode = Spatial-Form "H3" ([string] $definition.topologyCode); referenceBoundsMeters = Reference-Bounds $nodes ([double] $policy.layoutRules.h2SpacingMeters / 2.0); allowedRotationStepDegrees = 90; sizeVariantCodes = @("Reference"); connectionRoleCodes = @($connectors.roleCode | Sort-Object -Unique) }
    $sortedNodes = @($nodes | Sort-Object { [string] $_.h2Ref })
    $core = [ordered]@{ h3StableId = $target; definitionRevision = [int] $definition.revision; semanticLedgerRevision = [string] $semanticLedger.revision; topologyCode = [string] $definition.topologyCode; placementContract = $placement; nodes = $sortedNodes; semanticRelations = $semantic.semanticRelations; exposedConnectors = $semantic.exposedConnectors; flowRequirements = $semantic.flowRequirements }
    $h3Plans += [ordered]@{ h3StableId = $target; patternCode = [string] $pattern.patternCode; displayNameKo = [string] $pattern.spatialDisplayNameKo; spatialDisplayNameKo = [string] $pattern.spatialDisplayNameKo; gameplayProfileNameKo = [string] $pattern.displayNameKo; leadPackCode = [string] $pattern.leadPackCode; supportPackCodes = @(Array-Property $pattern "supportPackCodes"); compositionModeCode = [string] $pattern.compositionModeCode; patternFamilyCode = [string] $pattern.patternFamilyCode; patternSequence = [int] $pattern.patternSequence; resourceKindCode = "LandscapeAssemblyPattern"; title = [string] $definition.title; theoryStateCode = [string] $semantic.semanticQualificationCode; structureQualificationCode = [string] $semantic.structureQualificationCode; closureStateCode = [string] $semantic.closureStateCode; topologyCode = [string] $definition.topologyCode; placementContract = $placement; nodes = $sortedNodes; edges = $edges; connectors = $connectors; semanticRelations = $semantic.semanticRelations; exposedConnectors = $semantic.exposedConnectors; flowRequirements = $semantic.flowRequirements; unresolvedSemanticRelations = $semantic.unresolvedItems; theoryHashSha256 = Text-Hash (Stable-Json $core) }
}
$h3Map = @{}; foreach ($plan in $h3Plans) { $h3Map[[string] $plan.h3StableId] = $plan }

$areaSetInstances = @()
foreach ($candidate in @($priorities.areaSetCandidates | Sort-Object areaSetCandidateRef)) {
    $h4Ref = [string] $candidate.areaSetCandidateRef; Require ($h4.ContainsKey($h4Ref) -and $areaRecipeById.ContainsKey($h4Ref)) "AreaSetWorldIntentOrRecipeMissing:$h4Ref"; $recipe = $areaRecipeById[$h4Ref]; $children = @($recipe.requiredChildRefs | ForEach-Object { [string] $_ })
    Require (Same-Set $children @($candidate.requiredH3Refs)) "AreaSetSemanticChildrenMismatch:$h4Ref"; $nodes = @(); $profiles = @()
    for ($index = 0; $index -lt $children.Count; $index++) { $child = $children[$index]; Require ($h3Map.ContainsKey($child)) "AreaSetH3NotTheoryQualified:${h4Ref}:$child"; $plan = $h3Map[$child]; $position = Position "ModifiedGrid" $index $children.Count ([double] $policy.layoutRules.h3SpacingMeters); $nodes += [ordered]@{ graphInstanceStableId = "graph:theory:" + (Slug $child); h3Ref = $child; h3PatternCode = [string] $plan.patternCode; h3DisplayNameKo = [string] $plan.displayNameKo; x = $position.x; z = $position.z; h3TheoryHashSha256 = [string] $plan.theoryHashSha256 }; $profiles += [ordered]@{ childRef = $child; connectors = @($plan.exposedConnectors) } }
    $semantic = Evaluate-SemanticRecipe ("area-" + (Slug $h4Ref)) $recipe $children $profiles $semanticLedger; Require ([string] $semantic.semanticQualificationCode -eq "TheoryQualified") "AreaSetSemanticRelationUnresolved:$h4Ref"
    $relations = @($semantic.semanticRelations | ForEach-Object { [ordered]@{ relationStableId = [string] $_.relationStableId; fromGraphInstanceStableId = "graph:theory:" + (Slug ([string] $_.fromRef)); fromConnectorRoleCode = [string] $_.fromConnectorRoleCode; toGraphInstanceStableId = "graph:theory:" + (Slug ([string] $_.toRef)); toConnectorRoleCode = [string] $_.toConnectorRoleCode; relationKindCode = [string] $_.relationKindCode; relationDirectionCode = [string] $_.relationDirectionCode; compatibilityRuleCode = [string] $_.compatibilityRuleCode } } | Sort-Object { [string] $_.relationStableId })
    $sortedNodes = @($nodes | Sort-Object { [string] $_.h3Ref })
    $stableId = "area-set:theory:" + (Slug $h4Ref); $core = [ordered]@{ areaSetStableId = $stableId; worldIntentRef = $h4Ref; semanticLedgerRevision = [string] $semanticLedger.revision; graphInstances = $sortedNodes; graphRelations = $relations; exposedConnectors = $semantic.exposedConnectors; flowRequirements = $semantic.flowRequirements }
    $areaSetInstances += [ordered]@{ areaSetStableId = $stableId; title = [string] $candidate.title; worldIntentRef = $h4Ref; gamePlanCode = [string] $candidate.gamePlanCode; evidenceStageCode = "E5"; structureQualificationCode = "E5StructureQualified"; e5QualificationCode = "E5TheoryQualified"; closureStateCode = [string] $semantic.closureStateCode; evidenceKindCode = "TheoryGenerated"; humanReviewed = $false; publicDataBound = $false; runtimeValidated = $false; graphInstances = $sortedNodes; graphRelations = $relations; exposedConnectors = $semantic.exposedConnectors; flowRequirements = $semantic.flowRequirements; unresolvedSemanticRelations = $semantic.unresolvedItems; theoryHashSha256 = Text-Hash (Stable-Json $core) }
}
$areaMap = @{}; foreach ($area in $areaSetInstances) { $areaMap[[string] $area.worldIntentRef] = $area }

$worldRecipe = $semanticLedger.worldRelationRecipe; $worldChildren = @($worldRecipe.requiredChildRefs | ForEach-Object { [string] $_ }); $worldProfiles = @(); foreach ($child in $worldChildren) { Require ($areaMap.ContainsKey($child)) "TheoryWorldAreaSetMissing:$child"; $worldProfiles += [ordered]@{ childRef = $child; connectors = @($areaMap[$child].exposedConnectors) } }
$worldRecipeForEvaluation = [ordered]@{ requiredChildRefs = $worldChildren; relations = @($worldRecipe.relations); exposedConnectors = @(); flowRequirements = @() }
$worldSemantic = Evaluate-SemanticRecipe "world-nature-farm-city-town" $worldRecipeForEvaluation $worldChildren $worldProfiles $semanticLedger
$worldUnresolved = @($worldSemantic.unresolvedItems)
foreach ($flow in @($worldRecipe.flowRequirements)) { if (-not (Test-DirectedReachable ([string] $flow.fromChildRef) ([string] $flow.toChildRef) ([string] $flow.movementKindCode) $worldSemantic.semanticRelations)) { $worldUnresolved += [ordered]@{ code = "SemanticRelationUnresolved"; reasonCode = "RequiredWorldFlowNotReachable"; flowRequirementStableId = [string] $flow.flowRequirementStableId } } }
Require ($worldUnresolved.Count -eq 0 -and [string] $worldSemantic.structureQualificationCode -eq "StructureQualified") "TheoryWorldSemanticRelationUnresolved"
$worldCore = [ordered]@{ worldStableId = [string] $worldRecipe.targetRef; semanticLedgerRevision = [string] $semanticLedger.revision; areaSetRefs = @($worldChildren | Sort-Object); semanticRelations = $worldSemantic.semanticRelations; flowRequirements = @($worldRecipe.flowRequirements | Sort-Object flowRequirementStableId) }
$theoryWorld = [ordered]@{ worldStableId = [string] $worldRecipe.targetRef; structureQualificationCode = "TheoryWorldStructureQualified"; qualificationCode = "TheoryWorldQualified"; closureStateCode = "Closed"; areaSetRefs = @($worldChildren | Sort-Object); semanticRelations = $worldSemantic.semanticRelations; flowRequirements = @($worldRecipe.flowRequirements | Sort-Object flowRequirementStableId); unresolvedSemanticRelations = @(); theoryHashSha256 = Text-Hash (Stable-Json $worldCore) }

$result = [ordered]@{
    schemaVersion = "simulation-world-theory-spatial-factory-output.v1"; revision = "simulation-world-theory-spatial-factory-output.r3"; policyRevision = [string] $policy.revision; patternNamingRevision = [string] $patternNaming.revision; semanticRelationRevision = [string] $semanticLedger.revision; generatedAtRuleCode = "DeterministicNoWallClock"
    counts = [ordered]@{ h1Inputs = $h1.Count; h2TheoryQualified = $h2Plans.Count; h3TheoryQualified = $h3Plans.Count; e5TheoryQualifiedAreaSets = $areaSetInstances.Count; theoryWorldQualified = 1; authoredH2RecipesReused = @($h2Plans | Where-Object recipeSourceCode -eq "AuthoredRecipe").Count; derivedH2Recipes = @($h2Plans | Where-Object recipeSourceCode -eq "DerivedTheoryRecipe").Count; authoredH2LayoutsReused = @($h2Plans | Where-Object recipeSourceCode -eq "AuthoredRecipe").Count; derivedH2Layouts = @($h2Plans | Where-Object recipeSourceCode -eq "DerivedTheoryRecipe").Count; queuedExpansionOrRepairItems = @($patternNaming.priorityExpansionQueue).Count; semanticGapItems = @($semanticLedger.gapQueue).Count }
    h2Plans = @($h2Plans | Sort-Object { [string] $_.h2StableId }); h3Plans = @($h3Plans | Sort-Object { [string] $_.h3StableId }); e5AreaSetInstances = @($areaSetInstances | Sort-Object { [string] $_.worldIntentRef }); theoryWorld = $theoryWorld; interAreaSetRelations = @($priorities.interAreaSetRelations); semanticGapQueue = @($semanticLedger.gapQueue | Sort-Object gapKindCode, targetRef); inventoryTargets = @($patternNaming.inventoryTargets); productionPhases = @($patternNaming.productionPhases); priorityExpansionQueue = @($patternNaming.priorityExpansionQueue); humanReviewModeCode = [string] $policy.humanReviewPolicy.modeCode
    authorityBoundary = [ordered]@{ humanApprovalNotClaimed = $true; publicDataNotBound = $true; runtimeNotValidated = $true; e6AndE7RemainSeparate = $true }
}
$json = Normalize (Stable-Json $result)
$builder = [Text.StringBuilder]::new(); [void] $builder.AppendLine("# 재귀형 의미 H 공간 생산 결과"); [void] $builder.AppendLine(); [void] $builder.AppendLine("H2·H3·AreaSet·World를 같은 연결점·관계·흐름 규칙으로 판정한 위치 독립 결과다."); [void] $builder.AppendLine()
[void] $builder.AppendLine("- H2 이론 적격: $($result.counts.h2TheoryQualified)"); [void] $builder.AppendLine("- H3 이론 적격: $($result.counts.h3TheoryQualified)"); [void] $builder.AppendLine("- 이론 E5 AreaSet: $($result.counts.e5TheoryQualifiedAreaSets)"); [void] $builder.AppendLine("- 이론 World: ``$($theoryWorld.qualificationCode)``"); [void] $builder.AppendLine("- 의미 관계 대장: ``$($result.semanticRelationRevision)``"); [void] $builder.AppendLine()
[void] $builder.AppendLine("## 이론 AreaSet 의미 폐쇄"); [void] $builder.AppendLine(); [void] $builder.AppendLine("| AreaSet | 게임 기획 | 구조 | 의미 | H3 수 |"); [void] $builder.AppendLine("| --- | --- | --- | --- | ---: |")
foreach ($area in @($areaSetInstances | Sort-Object { [string] $_.worldIntentRef })) { [void] $builder.AppendLine("| ``$($area.areaSetStableId)`` | ``$($area.gamePlanCode)`` | ``$($area.structureQualificationCode)`` | ``$($area.e5QualificationCode)`` | $(@($area.graphInstances).Count) |") }
[void] $builder.AppendLine(); [void] $builder.AppendLine("## 세계 흐름"); [void] $builder.AppendLine(); foreach ($relation in $theoryWorld.semanticRelations) { [void] $builder.AppendLine("- ``$($relation.fromRef)`` / ``$($relation.fromConnectorRoleCode)`` → ``$($relation.toRef)`` / ``$($relation.toConnectorRoleCode)``: ``$($relation.relationKindCode)`` · ``$($relation.relationDirectionCode)``") }
[void] $builder.AppendLine(); [void] $builder.AppendLine("## 미해결 근거 대기열"); [void] $builder.AppendLine(); foreach ($gap in $result.semanticGapQueue) { [void] $builder.AppendLine("- ``$($gap.gapKindCode)`` · ``$($gap.targetRef)`` · ``$($gap.gapCode)``") }
[void] $builder.AppendLine(); [void] $builder.AppendLine("## 권위 경계"); [void] $builder.AppendLine(); [void] $builder.AppendLine("- 이 결과는 사람 승인, 공공데이터 결속, Unity Runtime 또는 실제 플레이를 주장하지 않는다."); [void] $builder.AppendLine("- 공공데이터는 E6, 실제 서버·저장 Scene 플레이는 E7에서 별도로 검증한다.")
$markdown = Normalize $builder.ToString()

$jsonPath = Resolve-RepoPath $JsonOutputPath; $markdownPath = Resolve-RepoPath $MarkdownOutputPath
if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $jsonPath) "JsonOutputMissing"; Require (Test-Path -LiteralPath $markdownPath) "MarkdownOutputMissing"
    Require ((Normalize ([IO.File]::ReadAllText($jsonPath))) -ceq $json) "JsonOutputStale"; Require ((Normalize ([IO.File]::ReadAllText($markdownPath))) -ceq $markdown) "MarkdownOutputStale"
    Write-Output "TheorySpatialFactoryValid:H2=$($h2Plans.Count);H3=$($h3Plans.Count);E5=$($areaSetInstances.Count);World=TheoryWorldQualified"; exit 0
}
foreach ($pair in @(@($jsonPath, $json), @($markdownPath, $markdown))) { $directory = Split-Path -Parent $pair[0]; if (-not (Test-Path -LiteralPath $directory)) { [void] (New-Item -ItemType Directory -Path $directory) }; Write-TextIfChanged $pair[0] ([string] $pair[1]) }
Write-Output "TheorySpatialFactoryGenerated:H2=$($h2Plans.Count);H3=$($h3Plans.Count);E5=$($areaSetInstances.Count);World=TheoryWorldQualified"
