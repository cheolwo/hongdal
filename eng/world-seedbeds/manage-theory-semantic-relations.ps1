param(
    [ValidateSet("Check", "Write")]
    [string] $Mode = "Check",
    [string] $OutputPath = "eng/world-seedbeds/synty-bottom-up-inventory/semantic-spatial-relations.v1.json"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path

function Read-Json([string] $path) {
    return Get-Content -LiteralPath (Join-Path $repositoryRoot $path) -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Normalize([string] $value) { return (($value -replace "`r`n", "`n").TrimEnd()) + "`n" }
function Slug([string] $value) { return (($value -replace '^[^:]+:', '') -replace '[^a-zA-Z0-9-]', '-') }

function Movement-Kinds([string] $stableId) {
    if ($stableId -match 'nature') { return @("PlayerTraversal", "WorkTraversal", "IncidentHandoff", "RecoveryHandoff") }
    if ($stableId -match 'town') { return @("PlayerTraversal", "WorkTraversal", "CargoLogistics", "ServiceTraversal") }
    return @("PlayerTraversal", "WorkTraversal", "CargoLogistics")
}

function New-Relation([string] $code, [string] $fromRef, [string] $toRef, [string] $kind = "WorkTraversal", [string] $direction = "Directed") {
    return [ordered]@{
        relationCode = $code
        fromRef = $fromRef
        fromConnectorRoleCode = "Output"
        toRef = $toRef
        toConnectorRoleCode = "Input"
        relationKindCode = $kind
        relationDirectionCode = $direction
        compatibilityRuleCode = if ($direction -eq "Bidirectional") { "BidirectionalSameFlow" } else { "OutputToInputSameFlow" }
    }
}

$catalog = Read-Json "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json"
$priorities = Read-Json "eng/world-seedbeds/synty-bottom-up-inventory/area-set-composition-priorities.v1.json"
$h2Definitions = @($catalog.h2DefinitionRefs | ForEach-Object { Read-Json ("eng/world-seedbeds/synty-bottom-up-inventory/" + [string] $_.definitionPath) })
$h3Definitions = @($catalog.h3DefinitionRefs | ForEach-Object { Read-Json ("eng/world-seedbeds/synty-bottom-up-inventory/" + [string] $_.definitionPath) })

$h3Overrides = @{
    "h3-candidate:farm-hub-logistics" = @(
        "h2-candidate:farm-processing-shipping",
        "h2-candidate:farm-hub-corridor",
        "h2-candidate:hub-inbound-storage")
    "h3-candidate:hub-town-logistics" = @(
        "h2-candidate:hub-outbound-vehicle",
        "h2-candidate:hub-town-corridor",
        "h2-candidate:market-life-commerce")
}

$h2Recipes = @()
foreach ($definition in @($h2Definitions | Sort-Object stableId)) {
    $targetRef = [string] $definition.stableId
    $children = @($definition.requiredH1Refs)
    $movementKinds = @(Movement-Kinds $targetRef)
    $childProfiles = @()
    foreach ($child in @($children | Sort-Object)) {
        $childProfiles += [ordered]@{
            childRef = [string] $child
            connectors = @(
                [ordered]@{ connectorStableId = "connector:h1:" + (Slug ([string] $child)) + ":input"; roleCode = "Input"; directionCode = "Input"; movementKindCodes = $movementKinds },
                [ordered]@{ connectorStableId = "connector:h1:" + (Slug ([string] $child)) + ":output"; roleCode = "Output"; directionCode = "Output"; movementKindCodes = $movementKinds })
        }
    }
    $relations = @()
    for ($index = 1; $index -lt $children.Count; $index++) {
        $kind = if ($targetRef -match 'corridor|shipping|storage|market|hub|fulfillment|receiving') { "CargoLogistics" } elseif ($targetRef -match 'nature') { "PlayerTraversal" } else { "WorkTraversal" }
        $relations += New-Relation ("h1-flow-$index") ([string] $children[$index - 1]) ([string] $children[$index]) $kind
    }
    $h2Recipes += [ordered]@{
        targetRef = $targetRef
        requiredChildRefs = $children
        childConnectorProfiles = $childProfiles
        relations = $relations
        exposedConnectors = @(
            [ordered]@{ connectorStableId = "connector:h2:" + (Slug $targetRef) + ":ingress"; roleCode = "Ingress"; directionCode = "Input"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[0]; sourceChildConnectorRoleCode = "Input" },
            [ordered]@{ connectorStableId = "connector:h2:" + (Slug $targetRef) + ":egress"; roleCode = "Egress"; directionCode = "Output"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[-1]; sourceChildConnectorRoleCode = "Output" })
        flowRequirements = @([ordered]@{ flowRequirementStableId = "flow:h2:" + (Slug $targetRef) + ":ingress-egress"; fromConnectorRoleCode = "Ingress"; toConnectorRoleCode = "Egress"; movementKindCode = [string] $relations[0].relationKindCode })
    }
}

$h3Recipes = @()
foreach ($definition in @($h3Definitions | Sort-Object stableId)) {
    $targetRef = [string] $definition.stableId
    $children = if ($h3Overrides.ContainsKey($targetRef)) { @($h3Overrides[$targetRef]) } else { @($definition.requiredH2Refs) }
    $movementKinds = @(Movement-Kinds $targetRef)
    $relationKind = if ($targetRef -match 'hub|logistics|market|farm-processing|seasonal') { "CargoLogistics" } elseif ($targetRef -match 'nature|town') { "PlayerTraversal" } else { "WorkTraversal" }
    $relations = @()
    for ($index = 1; $index -lt $children.Count; $index++) {
        $relation = New-Relation ("h2-flow-$index") ([string] $children[$index - 1]) ([string] $children[$index]) $relationKind
        $relation.fromConnectorRoleCode = "Egress"
        $relation.toConnectorRoleCode = "Ingress"
        $relations += $relation
    }
    if ($targetRef -eq "h3-candidate:nature-home-encounter-defense") {
        $returnRelation = New-Relation "h2-flow-3" ([string] $children[-1]) ([string] $children[0]) "RecoveryHandoff"
        $returnRelation.fromConnectorRoleCode = "Egress"
        $returnRelation.toConnectorRoleCode = "Ingress"
        $relations += $returnRelation
    }
    $exposed = @(
        [ordered]@{ connectorStableId = "connector:h3:" + (Slug $targetRef) + ":ingress"; roleCode = "Ingress"; directionCode = "Input"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[0]; sourceChildConnectorRoleCode = "Ingress" },
        [ordered]@{ connectorStableId = "connector:h3:" + (Slug $targetRef) + ":egress"; roleCode = "Egress"; directionCode = "Output"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[-1]; sourceChildConnectorRoleCode = "Egress" })
    if ($targetRef -eq "h3-candidate:nature-home-encounter-defense") {
        $exposed += @(
            [ordered]@{ connectorStableId = "connector:h3:nature-home-encounter-defense:safecoregate"; roleCode = "SafeCoreGate"; directionCode = "Input"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[0]; sourceChildConnectorRoleCode = "Ingress" },
            [ordered]@{ connectorStableId = "connector:h3:nature-home-encounter-defense:explorationoutput"; roleCode = "ExplorationOutput"; directionCode = "Output"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[0]; sourceChildConnectorRoleCode = "Egress" },
            [ordered]@{ connectorStableId = "connector:h3:nature-home-encounter-defense:threatinput"; roleCode = "ThreatInput"; directionCode = "Input"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[-1]; sourceChildConnectorRoleCode = "Ingress" },
            [ordered]@{ connectorStableId = "connector:h3:nature-home-encounter-defense:recoveryreturn"; roleCode = "RecoveryReturn"; directionCode = "Input"; movementKindCodes = $movementKinds; sourceChildRef = [string] $children[0]; sourceChildConnectorRoleCode = "Ingress" })
    }
    else {
        $roleCodes = @($definition.connectorRoleCodes)
        for ($index = 0; $index -lt $roleCodes.Count; $index++) {
            $isInput = $index -lt [Math]::Ceiling($roleCodes.Count / 2.0)
            $exposed += [ordered]@{
                connectorStableId = "connector:h3:" + (Slug $targetRef) + ":" + (Slug ([string] $roleCodes[$index])).ToLowerInvariant()
                roleCode = [string] $roleCodes[$index]
                directionCode = if ($isInput) { "Input" } else { "Output" }
                movementKindCodes = $movementKinds
                sourceChildRef = if ($isInput) { [string] $children[0] } else { [string] $children[-1] }
                sourceChildConnectorRoleCode = if ($isInput) { "Ingress" } else { "Egress" }
            }
        }
    }
    $flowRequirements = @([ordered]@{ flowRequirementStableId = "flow:h3:" + (Slug $targetRef) + ":ingress-egress"; fromConnectorRoleCode = "Ingress"; toConnectorRoleCode = "Egress"; movementKindCode = $relationKind })
    if ($targetRef -eq "h3-candidate:nature-home-encounter-defense") {
        $flowRequirements += [ordered]@{ flowRequirementStableId = "flow:h3:nature-home-encounter-defense:recovery-return"; fromConnectorRoleCode = "Egress"; toConnectorRoleCode = "RecoveryReturn"; movementKindCode = "RecoveryHandoff" }
    }
    $h3Recipes += [ordered]@{
        targetRef = $targetRef
        requiredChildRefs = $children
        relations = $relations
        exposedConnectors = $exposed
        flowRequirements = $flowRequirements
    }
}

$areaRecipes = @()
foreach ($candidate in @($priorities.areaSetCandidates | Sort-Object priorityCode)) {
    $targetRef = [string] $candidate.areaSetCandidateRef
    $children = @($candidate.requiredH3Refs)
    $relationKind = if ([string] $candidate.gamePlanCode -eq "NatureHomeThreatRecovery") { "PlayerTraversal" } else { "CargoLogistics" }
    $relations = @()
    for ($index = 1; $index -lt $children.Count; $index++) {
        $relation = New-Relation ("graph-flow-$index") ([string] $children[$index - 1]) ([string] $children[$index]) $relationKind
        $relation.fromConnectorRoleCode = "Egress"
        $relation.toConnectorRoleCode = "Ingress"
        $relations += $relation
    }
    $exposed = @(
        [ordered]@{ connectorStableId = "connector:area:" + (Slug $targetRef) + ":ingress"; roleCode = "Ingress"; directionCode = "Bidirectional"; movementKindCodes = @("PlayerTraversal", "WorkTraversal", "CargoLogistics"); sourceChildRef = [string] $children[0]; sourceChildConnectorRoleCode = "Ingress" },
        [ordered]@{ connectorStableId = "connector:area:" + (Slug $targetRef) + ":egress"; roleCode = "Egress"; directionCode = "Bidirectional"; movementKindCodes = @("PlayerTraversal", "WorkTraversal", "CargoLogistics"); sourceChildRef = [string] $children[-1]; sourceChildConnectorRoleCode = "Egress" })
    switch ([string] $candidate.gamePlanCode) {
        "FarmProductionSurvival" { $exposed += [ordered]@{ connectorStableId = "connector:area:farm:cargo-to-hub"; roleCode = "CargoToHubOutput"; directionCode = "Output"; movementKindCodes = @("CargoLogistics"); sourceChildRef = "h3-candidate:farm-seasonal-production-loop"; sourceChildConnectorRoleCode = "FarmShippingGate" } }
        "CityHubLogisticsResilience" {
            $exposed += [ordered]@{ connectorStableId = "connector:area:city-hub:cargo-from-farm"; roleCode = "CargoFromFarmInput"; directionCode = "Input"; movementKindCodes = @("CargoLogistics"); sourceChildRef = "h3-candidate:jinbu-hub"; sourceChildConnectorRoleCode = "HubInboundGate" }
            $exposed += [ordered]@{ connectorStableId = "connector:area:city-hub:cargo-to-town"; roleCode = "CargoToTownOutput"; directionCode = "Output"; movementKindCodes = @("CargoLogistics"); sourceChildRef = "h3-candidate:resilient-logistics-hub"; sourceChildConnectorRoleCode = "HubOutboundGate" }
        }
        "TownLivingMarketSafety" { $exposed += [ordered]@{ connectorStableId = "connector:area:town:cargo-from-hub"; roleCode = "CargoFromHubInput"; directionCode = "Input"; movementKindCodes = @("CargoLogistics"); sourceChildRef = "h3-candidate:lowrise-market-town"; sourceChildConnectorRoleCode = "TownReceivingGate" } }
    }
    $flowRequirements = @([ordered]@{ flowRequirementStableId = "flow:area:" + (Slug $targetRef) + ":ingress-egress"; fromConnectorRoleCode = "Ingress"; toConnectorRoleCode = "Egress"; movementKindCode = $relationKind })
    if ([string] $candidate.gamePlanCode -eq "CityHubLogisticsResilience") {
        $flowRequirements += [ordered]@{ flowRequirementStableId = "flow:area:city-hub:farm-to-town-cargo"; fromConnectorRoleCode = "CargoFromFarmInput"; toConnectorRoleCode = "CargoToTownOutput"; movementKindCode = "CargoLogistics" }
    }
    $areaRecipes += [ordered]@{
        targetRef = $targetRef
        requiredChildRefs = $children
        relations = $relations
        exposedConnectors = $exposed
        flowRequirements = $flowRequirements
    }
}

$result = [ordered]@{
    schemaVersion = "simulation-world-semantic-spatial-relations.v1"
    revision = "simulation-world-semantic-spatial-relations.r2"
    allowedDirectionCodes = @("Input", "Output", "Bidirectional")
    allowedRelationDirectionCodes = @("Directed", "Bidirectional")
    allowedMovementKindCodes = @("PlayerTraversal", "WorkTraversal", "CargoLogistics", "IncidentHandoff", "RecoveryHandoff", "ServiceTraversal")
    compatibilityRules = @(
        [ordered]@{ ruleCode = "OutputToInputSameFlow"; relationDirectionCode = "Directed"; allowedFromDirectionCodes = @("Output", "Bidirectional"); allowedToDirectionCodes = @("Input", "Bidirectional"); requireMovementKindOnBoth = $true },
        [ordered]@{ ruleCode = "BidirectionalSameFlow"; relationDirectionCode = "Bidirectional"; allowedFromDirectionCodes = @("Bidirectional"); allowedToDirectionCodes = @("Bidirectional"); requireMovementKindOnBoth = $true })
    h2RelationRecipes = $h2Recipes
    h3RelationRecipes = $h3Recipes
    areaSetRelationRecipes = $areaRecipes
    worldRelationRecipe = [ordered]@{
        targetRef = "theory-world:nature-farm-city-town"
        requiredChildRefs = @($priorities.areaSetCandidates.areaSetCandidateRef)
        relations = @(
            [ordered]@{ relationCode = "NatureFarmTraversal"; fromRef = "h4-blueprint:nature-home-exploration-region"; fromConnectorRoleCode = "Egress"; toRef = "h4-blueprint:farm-production-processing-region"; toConnectorRoleCode = "Ingress"; relationKindCode = "PlayerTraversal"; relationDirectionCode = "Bidirectional"; compatibilityRuleCode = "BidirectionalSameFlow" },
            [ordered]@{ relationCode = "NatureTownTraversal"; fromRef = "h4-blueprint:nature-home-exploration-region"; fromConnectorRoleCode = "Egress"; toRef = "h4-blueprint:lowrise-market-region"; toConnectorRoleCode = "Ingress"; relationKindCode = "PlayerTraversal"; relationDirectionCode = "Bidirectional"; compatibilityRuleCode = "BidirectionalSameFlow" },
            [ordered]@{ relationCode = "NatureCityHubTraversal"; fromRef = "h4-blueprint:nature-home-exploration-region"; fromConnectorRoleCode = "Egress"; toRef = "h4-blueprint:logistics-hub-region"; toConnectorRoleCode = "Ingress"; relationKindCode = "PlayerTraversal"; relationDirectionCode = "Bidirectional"; compatibilityRuleCode = "BidirectionalSameFlow" },
            [ordered]@{ relationCode = "FarmCargoToCityHub"; fromRef = "h4-blueprint:farm-production-processing-region"; fromConnectorRoleCode = "CargoToHubOutput"; toRef = "h4-blueprint:logistics-hub-region"; toConnectorRoleCode = "CargoFromFarmInput"; relationKindCode = "CargoLogistics"; relationDirectionCode = "Directed"; compatibilityRuleCode = "OutputToInputSameFlow" },
            [ordered]@{ relationCode = "CityHubCargoToTown"; fromRef = "h4-blueprint:logistics-hub-region"; fromConnectorRoleCode = "CargoToTownOutput"; toRef = "h4-blueprint:lowrise-market-region"; toConnectorRoleCode = "CargoFromHubInput"; relationKindCode = "CargoLogistics"; relationDirectionCode = "Directed"; compatibilityRuleCode = "OutputToInputSameFlow" })
        flowRequirements = @([ordered]@{ flowRequirementStableId = "flow:world:farm-hub-town-cargo"; fromChildRef = "h4-blueprint:farm-production-processing-region"; fromConnectorRoleCode = "CargoToHubOutput"; toChildRef = "h4-blueprint:lowrise-market-region"; toConnectorRoleCode = "CargoFromHubInput"; movementKindCode = "CargoLogistics" })
    }
    gapQueue = @(
        [ordered]@{ gapKindCode = "EvidenceGap"; targetRef = "h3-candidate:nature-threat-recovery"; gapCode = "NaturePowerCoreWiAndEvidencePending" },
        [ordered]@{ gapKindCode = "EvidenceGap"; targetRef = "h4-blueprint:logistics-hub-region"; gapCode = "CityHubPlayableSliceEvidencePending" },
        [ordered]@{ gapKindCode = "EvidenceGap"; targetRef = "h4-blueprint:lowrise-market-region"; gapCode = "TownPlayableSliceEvidencePending" })
    authorityBoundary = [ordered]@{ positionIndependent = $true; publicDataForbidden = $true; unityAssetReferencesForbidden = $true; operationalStateForbidden = $true }
}

$json = Normalize ($result | ConvertTo-Json -Depth 100)
$output = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Check") {
    if (-not (Test-Path -LiteralPath $output)) { throw "SemanticRelationLedgerMissing" }
    if ((Get-Content -LiteralPath $output -Raw -Encoding UTF8) -ne $json) { throw "SemanticRelationLedgerStale" }
    Write-Output "TheorySemanticRelationsValid:H2=$($h2Recipes.Count);H3=$($h3Recipes.Count);AreaSets=$($areaRecipes.Count);WorldRelations=5"
    exit 0
}
[IO.File]::WriteAllText($output, $json, [Text.UTF8Encoding]::new($false))
Write-Output "TheorySemanticRelationsGenerated:H2=$($h2Recipes.Count);H3=$($h3Recipes.Count);AreaSets=$($areaRecipes.Count);WorldRelations=5"
