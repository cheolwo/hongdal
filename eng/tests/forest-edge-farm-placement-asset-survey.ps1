$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$profilePath = Join-Path $repositoryRoot "eng/world-seedbeds/placement-map-profiles/forest-edge-farm-hans-living-farm.v1.json"
$planningPath = Join-Path $repositoryRoot "docs/AI/숲경계농장-H1-H2-배치맵-기획-2026-09-02.md"
$graphPath = Join-Path $repositoryRoot "eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json"
$functionalCatalogPath = Join-Path $repositoryRoot "eng/execution-ledgers/synty-asset-functional-modules.json"
$loopCatalogPath = Join-Path $repositoryRoot "eng/execution-ledgers/playable-loop-synty-expression-modules.json"
$spatialCatalogPath = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json"

function Require([bool] $condition, [string] $code) {
    if (-not $condition) { throw "ForestEdgeFarmAssetSurveyInvalid:$code" }
}

function Require-Unique([object[]] $items, [scriptblock] $selector, [string] $code) {
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($item in $items) {
        $value = [string] (& $selector $item)
        Require (-not [string]::IsNullOrWhiteSpace($value)) "$code`:Empty"
        Require ($seen.Add($value)) "$code`:Duplicate:$value"
    }
}

$profile = Get-Content -LiteralPath $profilePath -Raw -Encoding UTF8 | ConvertFrom-Json
$graph = Get-Content -LiteralPath $graphPath -Raw -Encoding UTF8 | ConvertFrom-Json
$functionalCatalog = Get-Content -LiteralPath $functionalCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$loopCatalog = Get-Content -LiteralPath $loopCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$spatialCatalog = Get-Content -LiteralPath $spatialCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $profile.schemaVersion -eq "simulation-world-placement-map-preparation-profile.v1") "Schema"
Require ([int] $profile.revision -eq 3) "Revision"
Require ([string] $profile.planningRevision -eq "forest-edge-farm-placement-map-planning.r24") "PlanningRevision"
Require ([string] $profile.planningSha256 -eq "71DC5B47F34F23E5FC5119650C18EA8C699D92729EBF61569F13C18A89C27113") "PlanningHash"
Require ((Get-FileHash -Algorithm SHA256 -LiteralPath $planningPath).Hash -eq [string] $profile.planningSha256) "PlanningFreshness"
Require ([string] $profile.primarySyntyPackCode -eq "Farm") "PrimaryPack"
Require ([string] $profile.firstImpressionCode -eq "AgedButOperatingLivingFarm") "FirstImpression"
Require ([string] $profile.validationResultCode -eq "Blocked") "ValidationBoundary"
Require (-not [bool] $profile.assetSurveyBoundary.inventoryMatchIsAssignment) "InventoryAssignmentBoundary"
Require (-not [bool] $profile.assetSurveyBoundary.visualReviewCompleted) "VisualReviewBoundary"
Require (-not [bool] $profile.assetSurveyBoundary.staticCompatibilityVerified) "StaticCompatibilityBoundary"
Require (-not [bool] $profile.assetSurveyBoundary.unityVerified) "UnityBoundary"
Require (-not [bool] $profile.assetSurveyBoundary.automaticPrefabAssignment) "AutomaticAssignmentBoundary"
Require (-not [bool] $profile.assetSurveyBoundary.syntySourceModificationAllowed) "SyntySourceBoundary"
Require (-not [bool] $profile.assetSurveyBoundary.blenderExecutionApproved) "BlenderExecutionBoundary"

$supportPacks = @($profile.supportPackPolicies)
Require-Unique $supportPacks { param($x) $x.packCode } "SupportPack"
Require (($supportPacks.packCode -join ",") -eq "Construction,Nature,AlpineMountain") "SupportPackOrder"
Require (@($supportPacks | Where-Object packCode -eq "Construction" | Where-Object supportRoleCode -eq "DamageRepairRecoveryState").Count -eq 1) "ConstructionRole"
Require (@($supportPacks | Where-Object packCode -eq "Nature" | Where-Object supportRoleCode -eq "ForestBoundaryBuffer").Count -eq 1) "NatureRole"
Require (@($supportPacks | Where-Object packCode -eq "AlpineMountain" | Where-Object supportRoleCode -eq "OptionalNorthernDistantTerrain").Count -eq 1) "AlpineRole"

$instances = @($profile.requiredPlacementInstances)
Require ($instances.Count -eq 5) "InstanceCount"
Require-Unique $instances { param($x) $x.placementInstanceId } "PlacementInstance"
$expectedInstanceIds = @(
    "placement-instance:forest-edge-farm:hans-residential-home",
    "placement-instance:forest-edge-farm:mixed-crop-field",
    "placement-instance:forest-edge-farm:damaged-fence-entry",
    "placement-instance:forest-edge-farm:barn-work-yard",
    "placement-instance:forest-edge-farm:forest-buffer"
)
Require (($instances.placementInstanceId -join ",") -eq ($expectedInstanceIds -join ",")) "InstanceOrder"

$knownModules = @($functionalCatalog.functionalModules.moduleCode)
$knownPacks = @($functionalCatalog.sourcePacks.packCode)
$knownFamilies = @()
$knownLoopModules = @($loopCatalog.loopModules.moduleStableId)
foreach ($module in @($loopCatalog.loopModules)) {
    foreach ($slot in @($module.slots)) { $knownFamilies += @($slot.assetFamilyIds) }
}
$knownFamilies = @($knownFamilies | Sort-Object -Unique)
$knownSpatialRefs = @(
    $spatialCatalog.h1InteractionDefinitionRefs.stableId +
    $spatialCatalog.h1ExpressionDefinitionRefs.stableId +
    $spatialCatalog.h2DefinitionRefs.stableId
)
$knownGraphNodes = @($graph.level1.nodes.nodeId)

foreach ($instance in $instances) {
    Require ([string] $instance.requirementCode -eq "RequiredForThisProfile") "Requirement:$($instance.placementInstanceId)"
    Require (@($instance.relativePlacementIntent).Count -gt 0) "PlacementIntent:$($instance.placementInstanceId)"
    Require ($knownSpatialRefs -contains [string] $instance.hDefinitionRef) "HRef:$($instance.placementInstanceId)"
    if ([string] $instance.graphBindingStateCode -eq "ExistingNode") {
        Require ($knownGraphNodes -contains [string] $instance.graphNodeRef) "GraphNode:$($instance.placementInstanceId)"
        Require ([string] $instance.graphNodeRef -eq [string] $instance.intendedGraphNodeRef) "GraphNodeIntent:$($instance.placementInstanceId)"
    }
    elseif ([string] $instance.graphBindingStateCode -eq "PendingStableNode") {
        Require ($null -eq $instance.graphNodeRef) "PendingGraphNodeMustBeNull:$($instance.placementInstanceId)"
        Require ($knownGraphNodes -notcontains [string] $instance.intendedGraphNodeRef) "PendingGraphNodeUnexpectedlyExists:$($instance.placementInstanceId)"
    }
    else { throw "ForestEdgeFarmAssetSurveyInvalid:GraphBindingState:$($instance.placementInstanceId)" }

    $survey = $instance.assetSurvey
    Require (@($survey.visualRequirementTags).Count -gt 0) "VisualRequirementTags:$($instance.placementInstanceId)"
    Require (($survey.sourcePreferenceCodes -join ",") -eq "SyntyOwnedFirst") "SourcePreference:$($instance.placementInstanceId)"
    Require ([string] $survey.surveyStateCode -eq "InventoryMatched") "SurveyState:$($instance.placementInstanceId)"
    Require (@($survey.candidateRefs).Count -eq 0) "ExactCandidateMustRemainEmpty:$($instance.placementInstanceId)"
    Require (@($survey.openGapCodes).Count -gt 0) "OpenGaps:$($instance.placementInstanceId)"
    foreach ($pack in @($survey.syntyPackCodes)) {
        $normalized = ([string] $pack).ToLowerInvariant().Replace("alpinemountain", "alpine-mountain")
        Require ($knownPacks -contains $normalized) "UnknownPack:$($instance.placementInstanceId):$pack"
    }
    foreach ($moduleCode in @($survey.functionalModuleCodes)) {
        Require ($knownModules -contains [string] $moduleCode) "UnknownModule:$($instance.placementInstanceId):$moduleCode"
    }
    foreach ($familyId in @($survey.assetFamilyIds)) {
        Require ($knownFamilies -contains [string] $familyId) "UnknownFamily:$($instance.placementInstanceId):$familyId"
    }
    foreach ($evidenceRef in @($survey.inventoryEvidenceRefs)) {
        $knownEvidence = ($knownSpatialRefs -contains [string] $evidenceRef) -or ($knownLoopModules -contains [string] $evidenceRef)
        Require $knownEvidence "UnknownEvidence:$($instance.placementInstanceId):$evidenceRef"
    }
}

$homeInstance = @($instances | Where-Object placementInstanceId -eq "placement-instance:forest-edge-farm:hans-residential-home")[0]
Require ([string] $homeInstance.assetSurvey.modificationNeedCode -eq "BlenderPlanRequiredCandidate") "HomeModificationState"
Require ([string] $homeInstance.assetSurvey.blenderGapSurvey.stateCode -eq "PlanningOnly") "BlenderPlanningState"
Require (($homeInstance.assetSurvey.blenderGapSurvey.evaluationOrderCodes -join ",") -eq "ExistingPrefab,CompositionOnly,ProjectOwnedVariant,BlenderPlanRequired,BlenderValidatedCopy") "BlenderEvaluationOrder"
Require ([string] $homeInstance.assetSurvey.blenderGapSurvey.singleDamageCueCode -eq "SmallRoofHoleVisibleFromForestTrail") "SingleRoofDamageCue"
Require (-not [bool] $homeInstance.assetSurvey.blenderGapSurvey.executionApproved) "BlenderExecutionApproval"

Require (@($profile.blockingReasons) -contains "CurrentGraphMapRevisionHasUnintegratedFederationReferences") "GraphPartialIntegrationBlocker"
Require (@($profile.blockingReasons) -contains "GraphVisualRequirementTagsPending") "GraphVisualTagsBlocker"
Require (@($profile.blockingReasons) -contains "AssetCandidateFingerprintPending") "CandidateFingerprintBlocker"

Write-Output "ForestEdgeFarmAssetSurveyTestsPassed:Instances=5;InventoryMatched=5;Candidates=0;GraphIntegrated=0;Unity=0;Blender=0"
