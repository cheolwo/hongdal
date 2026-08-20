[CmdletBinding()]
param(
    [ValidateSet("Check", "Write")]
    [string] $Mode = "Check",
    [string] $PolicyPath = "eng/world-seedbeds/h5-world-layout-policy.v1.json",
    [string] $TheoryPath = "eng/world-seedbeds/generated/theory-spatial-factory.v1.json",
    [string] $ActualE5Path = "eng/world-seedbeds/generated/actual-e5-spatial.v1.json",
    [string] $OutputPath = "eng/world-seedbeds/generated/h5-world-layout.v1.json",
    [string] $MarkdownOutputPath = "docs/AI/generated/h5-world-layout.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path

function Resolve-RepoPath([string] $relativePath) {
    Join-Path $repositoryRoot ($relativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}
function Read-Json([string] $relativePath) {
    $path = Resolve-RepoPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) { throw "H5WorldLayoutJsonMissing:$relativePath" }
    Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Require([bool] $condition, [string] $code) {
    if (-not $condition) { throw "H5WorldLayoutInvalid:$code" }
}
function Normalize([string] $value) { (($value -replace "`r`n", "`n").TrimEnd()) + "`n" }
function Stable-Json([object] $value) { (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n") }
function Text-Hash([string] $value) {
    $bytes = [Text.Encoding]::UTF8.GetBytes($value)
    $sha = [Security.Cryptography.SHA256]::Create()
    try { (($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join "") }
    finally { $sha.Dispose() }
}
function Slug([string] $stableId) { $stableId.Substring($stableId.LastIndexOf(':') + 1) -replace '[^a-zA-Z0-9-]', '-' }
function Graph-Id([string] $h3Ref) { "landscape-graph:sim:pyeongchang:" + (Slug $h3Ref) + ".v1" }
function Normalize-Rotation([double] $value) {
    $result = $value % 360.0
    if ($result -lt 0.0) { $result += 360.0 }
    [Math]::Round($result, 6)
}
function Rotate-Point([double] $x, [double] $z, [double] $rotationDegrees) {
    $radians = $rotationDegrees * [Math]::PI / 180.0
    [pscustomobject]@{
        X = [Math]::Cos($radians) * $x + [Math]::Sin($radians) * $z
        Z = -[Math]::Sin($radians) * $x + [Math]::Cos($radians) * $z
    }
}
function Placement([string] $coordinateSpaceCode, [double] $x, [double] $z, [double] $rotation) {
    [ordered]@{
        coordinateSpaceCode = $coordinateSpaceCode
        localXMeters = [Math]::Round($x, 6)
        localZMeters = [Math]::Round($z, 6)
        rotationDegrees = Normalize-Rotation $rotation
        sizeVariantCode = "Reference"
        mirrorCode = "None"
    }
}
function Apply-Pose([object] $transform, [object] $pose, [string] $coordinateSpaceCode) {
    $point = Rotate-Point ([double] $pose.localXMeters) ([double] $pose.localZMeters) ([double] $transform.rotationDegrees)
    $core = [ordered]@{
        connectorStableId = [string] $pose.connectorStableId
        coordinateSpaceCode = $coordinateSpaceCode
        localXMeters = [Math]::Round([double] $transform.localXMeters + $point.X, 6)
        localZMeters = [Math]::Round([double] $transform.localZMeters + $point.Z, 6)
        rotationDegrees = Normalize-Rotation ([double] $transform.rotationDegrees + [double] $pose.rotationDegrees)
        widthMeters = [double] $pose.widthMeters
        directionCode = [string] $pose.directionCode
        travelTypeCodes = @($pose.travelTypeCodes | Sort-Object -Unique)
    }
    $core.connectorPoseHashSha256 = Text-Hash (Stable-Json $core)
    $core
}
function Fit-ChildTransform([object] $parentOutputWorldPose, [object] $childInputLocalPose) {
    $rotation = Normalize-Rotation ([double] $parentOutputWorldPose.rotationDegrees + 180.0 - [double] $childInputLocalPose.rotationDegrees)
    $rotated = Rotate-Point ([double] $childInputLocalPose.localXMeters) ([double] $childInputLocalPose.localZMeters) $rotation
    Placement "ScenarioLocalMeters" ([double] $parentOutputWorldPose.localXMeters - $rotated.X) ([double] $parentOutputWorldPose.localZMeters - $rotated.Z) $rotation
}
function Graph-ConnectorPose([object] $graph, [string] $side, [string] $travelTypeCode) {
    $stub = $graph.externalConnectorStubs | Where-Object directionCode -eq $side | Select-Object -First 1
    Require ($null -ne $stub) "GraphConnectorMissing:$($graph.landscapeGraphStableId):$side"
    $centerX = ([double] $graph.bounds.minEastingMeters + [double] $graph.bounds.maxEastingMeters) / 2.0
    $centerZ = ([double] $graph.bounds.minNorthingMeters + [double] $graph.bounds.maxNorthingMeters) / 2.0
    $dx = [double] $stub.worldEastingMeters - $centerX
    $dz = [double] $stub.worldNorthingMeters - $centerZ
    $rotation = [Math]::Atan2($dx, $dz) * 180.0 / [Math]::PI
    $core = [ordered]@{
        connectorStableId = [string] $stub.stubStableId
        coordinateSpaceCode = "ParentLocalMeters"
        localXMeters = [double] $stub.worldEastingMeters
        localZMeters = [double] $stub.worldNorthingMeters
        rotationDegrees = Normalize-Rotation $rotation
        widthMeters = [double] $stub.widthMeters
        directionCode = $side
        travelTypeCodes = @($travelTypeCode)
    }
    $core.connectorPoseHashSha256 = Text-Hash (Stable-Json $core)
    $core
}
function Pose-Distance([object] $a, [object] $b) {
    $dx = [double] $a.localXMeters - [double] $b.localXMeters
    $dz = [double] $a.localZMeters - [double] $b.localZMeters
    [Math]::Sqrt($dx * $dx + $dz * $dz)
}
function Opposed-Rotation-Difference([object] $a, [object] $b) {
    $difference = [Math]::Abs((Normalize-Rotation ([double] $a.rotationDegrees - [double] $b.rotationDegrees)) - 180.0)
    [Math]::Min($difference, 360.0 - $difference)
}

$policy = Read-Json $PolicyPath
$theory = Read-Json $TheoryPath
$actual = Read-Json $ActualE5Path
Require ([string] $policy.schemaVersion -eq "simulation-world-h5-layout-policy.v1") "PolicySchema"
Require ([string] $actual.schemaVersion -eq "simulation-world-actual-e5-spatial-output.v1") "ActualE5Schema"
Require ([string] $policy.coordinateSpaceCode -eq "ScenarioLocalMeters") "RootCoordinateSpace"
Require (@($policy.physicalCorridors).Count -eq 3) "PhysicalCorridorCount"
Require ([bool] $policy.presentationOnly -and -not [bool] $policy.isOperationalState) "AuthorityBoundary"

$actualAreaById = @{}
foreach ($area in @($actual.areaSets)) { $actualAreaById[[string] $area.definition.areaSetStableId] = $area }
$graphById = @{}
foreach ($graph in @($actual.areaSets.graphs) + @($actual.routeGraphs)) { $graphById[[string] $graph.landscapeGraphStableId] = $graph }
$theoryAreaById = @{}
foreach ($area in @($theory.e5AreaSetInstances)) { $theoryAreaById[[string] $area.areaSetStableId] = $area }
$areaPolicyByActual = @{}
$actualPolicy = Read-Json "eng/world-seedbeds/actual-e5-spatial-policy.v1.json"
foreach ($area in @($actualPolicy.areaSets)) { $areaPolicyByActual[[string] $area.actualAreaSetStableId] = $area }

$areaLocal = @{}
foreach ($areaId in @($actualAreaById.Keys | Sort-Object)) {
    $areaDocument = $actualAreaById[$areaId]
    $mapping = $areaPolicyByActual[$areaId]
    Require ($null -ne $mapping) "AreaPolicyMissing:$areaId"
    $theoryArea = $theoryAreaById[[string] $mapping.theoryAreaSetStableId]
    Require ($null -ne $theoryArea) "TheoryAreaMissing:$areaId"
    $graphInstances = @()
    $areaConnectors = @()
    foreach ($source in @($theoryArea.graphInstances)) {
        $graphId = Graph-Id ([string] $source.h3Ref)
        if (-not $graphById.ContainsKey($graphId)) { continue }
        $graph = $graphById[$graphId]
        $transform = Placement "ParentLocalMeters" ([double] $source.x) ([double] $source.z) 0.0
        $sourceHash = [string] $source.h3TheoryHashSha256
        $placementHash = Text-Hash (Stable-Json $transform)
        $connectors = @(
            Graph-ConnectorPose $graph "Ingress" "PlayerTraversal"
            Graph-ConnectorPose $graph "Egress" "PlayerTraversal"
        )
        $graphInstances += [ordered]@{
            graphInstanceStableId = "graph-instance:h4:" + (Slug $areaId) + ":" + (Slug ([string] $source.h3Ref))
            landscapeGraphStableId = $graphId
            h3Ref = [string] $source.h3Ref
            placementTransform = $transform
            externalConnectors = $connectors
            sourcePatternHashSha256 = $sourceHash
            placementHashSha256 = $placementHash
            instanceHashSha256 = Text-Hash (Stable-Json ([ordered]@{ graph = $graphId; source = $sourceHash; placement = $placementHash }))
        }
        foreach ($connector in $connectors) {
            $areaConnectors += Apply-Pose $transform $connector "ParentLocalMeters"
        }
    }
    Require ($graphInstances.Count -gt 0) "AreaGraphInstancesMissing:$areaId"
    $areaLocal[$areaId] = [pscustomobject]@{
        AreaDocument = $areaDocument
        BlueprintStableId = [string] $theoryArea.worldIntentRef
        GraphInstances = $graphInstances
        Connectors = $areaConnectors
    }
}

function Area-Connector([string] $areaId, [string] $h3Ref, [string] $side, [string] $travelTypeCode) {
    $entry = $areaLocal[$areaId]
    $instance = $entry.GraphInstances | Where-Object h3Ref -eq $h3Ref | Select-Object -First 1
    Require ($null -ne $instance) "AreaGraphInstanceMissing:${areaId}:${h3Ref}"
    $pose = $instance.externalConnectors | Where-Object directionCode -eq $side | Select-Object -First 1
    Require ($null -ne $pose) "AreaGraphConnectorMissing:${areaId}:${h3Ref}:${side}"
    $pose.travelTypeCodes = @($travelTypeCode)
    Apply-Pose $instance.placementTransform $pose "ParentLocalMeters"
}

$areaTransforms = @{}
$areaTransforms[[string] $policy.rootAreaSetStableId] = Placement "ScenarioLocalMeters" 0.0 0.0 0.0
$corridorInstances = @()
$physicalRelationCodes = @()
foreach ($corridor in @($policy.physicalCorridors)) {
    $relationSuffix = ":" + ([string] $corridor.relationCode).ToLowerInvariant()
    $relation = $actual.network.relations | Where-Object { ([string] $_.relationStableId).EndsWith($relationSuffix, [StringComparison]::Ordinal) } | Select-Object -First 1
    Require ($null -ne $relation) "NetworkRelationMissing:$($corridor.relationCode)"
    $fromAreaId = [string] $corridor.fromAreaSetStableId
    $toAreaId = [string] $corridor.toAreaSetStableId
    Require ($areaTransforms.ContainsKey($fromAreaId)) "LayoutOrderInvalid:$fromAreaId"
    $travelType = [string] $relation.relationKindCode
    $fromLocal = Area-Connector $fromAreaId ([string] $corridor.fromH3Ref) "Egress" $travelType
    $fromWorld = Apply-Pose $areaTransforms[$fromAreaId] $fromLocal "ScenarioLocalMeters"
    $routeGraphId = Graph-Id ([string] $corridor.routeH3Ref)
    $routeGraph = $graphById[$routeGraphId]
    Require ($null -ne $routeGraph) "CorridorGraphMissing:$routeGraphId"
    $routeIngress = Graph-ConnectorPose $routeGraph "Ingress" $travelType
    $routeEgress = Graph-ConnectorPose $routeGraph "Egress" $travelType
    $corridorTransform = Fit-ChildTransform $fromWorld $routeIngress
    $routeIngressWorld = Apply-Pose $corridorTransform $routeIngress "ScenarioLocalMeters"
    Require ((Pose-Distance $fromWorld $routeIngressWorld) -le [double] $policy.connectorPositionToleranceMeters) "ConnectorFitStartPosition:$($corridor.relationCode)"
    Require ((Opposed-Rotation-Difference $fromWorld $routeIngressWorld) -le [double] $policy.connectorRotationToleranceDegrees) "ConnectorFitStartRotation:$($corridor.relationCode)"
    $routeEgressWorld = Apply-Pose $corridorTransform $routeEgress "ScenarioLocalMeters"
    $toLocal = Area-Connector $toAreaId ([string] $corridor.toH3Ref) "Ingress" $travelType
    $toTransform = Fit-ChildTransform $routeEgressWorld $toLocal
    $areaTransforms[$toAreaId] = $toTransform
    $toWorld = Apply-Pose $toTransform $toLocal "ScenarioLocalMeters"
    Require ((Pose-Distance $routeEgressWorld $toWorld) -le [double] $policy.connectorPositionToleranceMeters) "ConnectorFitPosition:$($corridor.relationCode)"
    Require ((Opposed-Rotation-Difference $routeEgressWorld $toWorld) -le [double] $policy.connectorRotationToleranceDegrees) "ConnectorFitRotation:$($corridor.relationCode)"
    $corridorId = "corridor-instance:h5:" + ([string] $corridor.relationCode).ToLowerInvariant()
    $placementHash = Text-Hash (Stable-Json $corridorTransform)
    $corridorInstances += [ordered]@{
        corridorInstanceStableId = $corridorId
        landscapeGraphStableId = $routeGraphId
        placementTransform = $corridorTransform
        fromAreaSetInstanceStableId = $fromAreaId
        fromConnectorStableId = [string] $fromLocal.connectorStableId
        toAreaSetInstanceStableId = $toAreaId
        toConnectorStableId = [string] $toLocal.connectorStableId
        relationStableId = [string] $relation.relationStableId
        externalConnectors = @($routeIngress, $routeEgress)
        placementHashSha256 = $placementHash
        instanceHashSha256 = Text-Hash (Stable-Json ([ordered]@{ graph = $routeGraphId; placement = $placementHash; relation = [string] $relation.relationStableId }))
    }
    $physicalRelationCodes += [string] $corridor.relationCode
}

Require ($areaTransforms.Count -eq 4) "AreaPlacementCount"
$areaInstances = @()
foreach ($areaId in @($areaTransforms.Keys | Sort-Object)) {
    $entry = $areaLocal[$areaId]
    $transform = $areaTransforms[$areaId]
    $placementHash = Text-Hash (Stable-Json $transform)
    $role = [string] $entry.AreaDocument.areaRoleCode
    $areaInstances += [ordered]@{
        areaSetInstanceStableId = $areaId
        blueprintStableId = [string] $entry.BlueprintStableId
        areaRoleCode = $role
        loadPolicyCode = [string] $entry.AreaDocument.loadPolicyCode
        placementTransform = $transform
        graphInstances = @($entry.GraphInstances)
        externalConnectors = @($entry.Connectors)
        placementHashSha256 = $placementHash
        instanceHashSha256 = Text-Hash (Stable-Json ([ordered]@{ area = $areaId; blueprint = [string] $entry.BlueprintStableId; placement = $placementHash; graphs = @($entry.GraphInstances.instanceHashSha256) }))
    }
}

$corridorByRelation = @{}; foreach ($item in $corridorInstances) { $corridorByRelation[[string] $item.relationStableId] = $item }
$relations = @()
foreach ($relation in @($actual.network.relations | Sort-Object relationStableId)) {
    $policyCode = ([string] $relation.relationStableId).Split(':')[-1]
    $physical = $corridorInstances | Where-Object relationStableId -eq ([string] $relation.relationStableId) | Select-Object -First 1
    $relations += [ordered]@{
        relationStableId = [string] $relation.relationStableId
        fromAreaSetInstanceStableId = [string] $relation.fromAreaSetStableId
        toAreaSetInstanceStableId = [string] $relation.toAreaSetStableId
        relationKindCode = [string] $relation.relationKindCode
        spatialRealizationCode = if ($null -ne $physical) { "PhysicalCorridor" } else { "AbstractTravel" }
        corridorInstanceStableId = if ($null -ne $physical) { [string] $physical.corridorInstanceStableId } else { "" }
    }
}

$overlapRules = @()
$areaIds = @($areaInstances.areaSetInstanceStableId | Sort-Object)
for ($i = 0; $i -lt $areaIds.Count; $i++) {
    for ($j = $i + 1; $j -lt $areaIds.Count; $j++) {
        $corridor = $corridorInstances | Where-Object {
            ($_.fromAreaSetInstanceStableId -eq $areaIds[$i] -and $_.toAreaSetInstanceStableId -eq $areaIds[$j]) -or
            ($_.fromAreaSetInstanceStableId -eq $areaIds[$j] -and $_.toAreaSetInstanceStableId -eq $areaIds[$i])
        } | Select-Object -First 1
        $overlapRules += [ordered]@{
            fromInstanceStableId = $areaIds[$i]
            toInstanceStableId = $areaIds[$j]
            overlapPolicyCode = if ($null -ne $corridor) { "TransitionOverlap" } else { "Disallow" }
            corridorInstanceStableId = if ($null -ne $corridor) { [string] $corridor.corridorInstanceStableId } else { "" }
        }
    }
}

$definition = [ordered]@{
    schemaVersion = "simulation-world-layout-definition.v1"
    worldLayoutStableId = [string] $policy.worldLayoutStableId
    worldLayoutRevision = 1
    worldIntentStableId = [string] $policy.worldIntentStableId
    areaSetNetworkStableId = [string] $policy.areaSetNetworkStableId
    coordinateSpaceCode = "ScenarioLocalMeters"
    worldGroundingPolicyCode = [string] $policy.worldGroundingPolicyCode
    areaSetInstances = @($areaInstances)
    corridorInstances = @($corridorInstances)
    relations = @($relations)
    overlapRules = @($overlapRules)
    worldLayoutHashSha256 = ""
    presentationOnly = $true
    isOperationalState = $false
}
$definition.worldLayoutHashSha256 = Text-Hash (Stable-Json $definition)

$binding = [ordered]@{
    schemaVersion = "simulation-world-grounding-binding.v1"
    groundingBindingStableId = "grounding-binding:sim:pyeongchang:nature-farm-hub-town.v1"
    groundingBindingRevision = 1
    worldLayoutStableId = [string] $definition.worldLayoutStableId
    worldLayoutRevision = [int] $definition.worldLayoutRevision
    worldLayoutHashSha256 = [string] $definition.worldLayoutHashSha256
    placementAuthorityCode = "ScenarioRelative"
    worldGroundingStateCode = "NotApplied"
    e6AnchorStableId = ""
    groundingEvidenceHashSha256 = ""
    bindingHashSha256 = ""
    presentationOnly = $true
    isOperationalState = $false
}
$binding.bindingHashSha256 = Text-Hash (Stable-Json $binding)

$readiness = [ordered]@{
    schemaVersion = "simulation-world-grounding-readiness.v1"
    worldLayoutStableId = [string] $definition.worldLayoutStableId
    groundingReadinessStateCode = "Partial"
    availableEvidenceKindCodes = @()
    missingEvidenceKindCodes = @("WorldAnchor", "DEM", "Landcover", "Road")
    blockReasonCodes = @()
    readinessHashSha256 = ""
    appliesAuthority = $false
    presentationOnly = $true
    isOperationalState = $false
}
$readiness.readinessHashSha256 = Text-Hash (Stable-Json $readiness)

$result = [ordered]@{
    schemaVersion = "simulation-world-h5-spatial-output.v1"
    revision = "simulation-world-h5-spatial-output.r1"
    policyRevision = [string] $policy.revision
    generatedAtRuleCode = "DeterministicNoWallClock"
    worldLayoutDefinition = $definition
    worldGroundingBinding = $binding
    groundingReadiness = $readiness
    authorityBoundary = [ordered]@{
        scenarioRelativeIsAuthoritative = $true
        e6IsOptional = $true
        e6CannotRewriteLayout = $true
        floatingOriginExcludedFromAuthority = $true
        presentationOnly = $true
        operationalState = $false
    }
}
$json = Normalize (Stable-Json $result)

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# H5 세계 배치")
[void] $builder.AppendLine()
[void] $builder.AppendLine("H4 AreaSet과 H3 회랑을 H5의 ``ScenarioLocalMeters``에 배치한 실제 E5 공간 조립 결과다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- H5: ``$($definition.worldLayoutStableId)``")
[void] $builder.AppendLine("- H4 AreaSet 인스턴스: ``$($areaInstances.Count)``")
[void] $builder.AppendLine("- 물리 회랑: ``$($corridorInstances.Count)``")
[void] $builder.AppendLine("- 현실 결속: ``$($definition.worldGroundingPolicyCode) / $($binding.worldGroundingStateCode)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("E6가 없어도 이 H5는 ScenarioRelative 권위 세계다. E6는 H5 이하 상대 X/Z 배치를 바꾸지 않는다.")
$markdown = Normalize $builder.ToString()

$jsonPath = Resolve-RepoPath $OutputPath
$markdownPath = Resolve-RepoPath $MarkdownOutputPath
if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $jsonPath) "JsonOutputMissing"
    Require (Test-Path -LiteralPath $markdownPath) "MarkdownOutputMissing"
    Require ((Normalize ([IO.File]::ReadAllText($jsonPath))) -ceq $json) "JsonOutputStale"
    Require ((Normalize ([IO.File]::ReadAllText($markdownPath))) -ceq $markdown) "MarkdownOutputStale"
    Write-Output "H5WorldLayoutValid:AreaSets=$($areaInstances.Count);Corridors=$($corridorInstances.Count);Grounding=Optional/NotApplied"
    exit 0
}

foreach ($pair in @(@($jsonPath, $json), @($markdownPath, $markdown))) {
    $directory = Split-Path -Parent $pair[0]
    if (-not (Test-Path -LiteralPath $directory)) { [void] (New-Item -ItemType Directory -Path $directory) }
    if (-not (Test-Path -LiteralPath $pair[0]) -or (Normalize ([IO.File]::ReadAllText($pair[0]))) -cne [string] $pair[1]) {
        [IO.File]::WriteAllText($pair[0], [string] $pair[1], [Text.UTF8Encoding]::new($false))
    }
}
Write-Output "H5WorldLayoutGenerated:AreaSets=$($areaInstances.Count);Corridors=$($corridorInstances.Count);Grounding=Optional/NotApplied"
