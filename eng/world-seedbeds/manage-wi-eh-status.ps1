[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $OutputJsonPath = "eng/world-seedbeds/generated/wi-eh-status.v1.json",
    [string] $OutputMarkdownPath = "docs/AI/generated/wi-eh-spatial-status.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "WorldInteractionEhStatusInvalid:$Code" }
}

function Read-Json([string] $Path) {
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Get-Values([object] $Value, [string] $PropertyName) {
    if ($null -eq $Value -or -not ($Value.PSObject.Properties.Name -contains $PropertyName)) {
        return @()
    }
    return @($Value.$PropertyName | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) })
}

function Add-MapValue([hashtable] $Map, [string] $Key, [object] $Value) {
    if (-not $Map.ContainsKey($Key)) { $Map[$Key] = [Collections.Generic.List[object]]::new() }
    $Map[$Key].Add($Value)
}

function Assert-RealityGroundingBoundary([object] $Plan, [string] $Raw, [string] $CodePrefix) {
    Require (-not ($Raw -match '"requiredEvidencePurposeCodes"')) "$($CodePrefix)LegacyRequiredEvidenceForbidden"
    Require ($Plan.PSObject.Properties.Name -contains "realityGrounding") "$($CodePrefix)RealityGroundingMissing"
    $grounding = $Plan.realityGrounding
    Require ([string] $grounding.stageCode -eq "E6") "$($CodePrefix)RealityGroundingStageInvalid"
    Require (@("NotRequired", "Optional", "Required") -contains [string] $grounding.policyCode) "$($CodePrefix)RealityGroundingPolicyInvalid"
    Require (@("NotApplied", "Applied", "Stale") -contains [string] $grounding.applicationStateCode) "$($CodePrefix)RealityGroundingStateInvalid"
    Require (-not [bool] $grounding.blocksScenarioExecution) "$($CodePrefix)RealityGroundingMustNotBlockScenario"
    if ([string] $grounding.policyCode -eq "Optional") {
        Require (-not [bool] $grounding.requiredForTargetCompletion) "$($CodePrefix)OptionalGroundingMustNotBlockTarget"
    }
    $purposeCodes = @(Get-Values $grounding "candidateEvidencePurposeCodes")
    Require ($purposeCodes.Count -eq @($purposeCodes | Sort-Object -Unique).Count) "$($CodePrefix)RealityGroundingPurposeDuplicate"
}

function Get-Hash([string] $Content) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes((ConvertTo-DeterministicText $Content))
        return ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
    }
    finally { $sha.Dispose() }
}

function Escape-Markdown([string] $Value) {
    if ($null -eq $Value) { return "" }
    return $Value.Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$worldCatalogPath = Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json"
$priorityPath = Join-Path $repositoryRoot "eng/world-seedbeds/wi-spatial-priorities.v1.json"
$seedbedRoot = Join-Path $repositoryRoot "eng/world-seedbeds/wi-spatial-seedbeds"
$designRoot = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory"
$designCatalogPath = Join-Path $designRoot "catalog.v3.json"
$bindingPath = Join-Path $repositoryRoot "eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/spatial-capabilities.v1.json"
$resourceInventoryPath = Join-Path $repositoryRoot "eng/world-seedbeds/spatial-resource-inventory/catalog.v1.json"
$compositionPlanPath = Join-Path $repositoryRoot "eng/world-seedbeds/wi-spatial-composition-plans/reference-play-01-harvest-shipping.v1.json"
$p2CompositionPlanPath = Join-Path $repositoryRoot "eng/world-seedbeds/wi-spatial-composition-plans/p2-hub-inbound-storage.v1.json"
$actualE5Path = Join-Path $repositoryRoot "eng/world-seedbeds/generated/actual-e5-spatial.v1.json"

$worldCatalog = Read-Json $worldCatalogPath
$priority = Read-Json $priorityPath
$seedbedCatalog = Read-Json (Join-Path $seedbedRoot "catalog.json")
$designCatalog = Read-Json $designCatalogPath
$bindings = Read-Json $bindingPath
$resourceInventory = Read-Json $resourceInventoryPath
$compositionPlan = Read-Json $compositionPlanPath
$p2CompositionPlan = Read-Json $p2CompositionPlanPath
$actualE5 = Read-Json $actualE5Path
$actualE5BindingsById = @{}
foreach ($binding in @($actualE5.interactionSpatialCatalog.bindings)) {
    $actualE5BindingsById[[string] $binding.bindingStableId] = $binding
}

Require ([string] $worldCatalog.revision -eq [string] $priority.worldInteractionCatalogRevision) "PriorityWorldCatalogRevisionMismatch"
Require (@($worldCatalog.items).Count -eq 64) "WorldInteractionCountMustBe64"
Require ([string] $priority.schemaVersion -eq "simulation-world-interaction-spatial-priorities.v1") "PrioritySchemaInvalid"
Require ([string] $compositionPlan.schemaVersion -eq "simulation-world-interaction-spatial-composition-plan.v1") "CompositionPlanSchemaInvalid"
Require ([string] $p2CompositionPlan.schemaVersion -eq "simulation-world-interaction-spatial-composition-plan.v1") "P2CompositionPlanSchemaInvalid"
Require ([string] $resourceInventory.revision -eq "simulation-world-spatial-resource-inventory.r10") "ResourceInventoryRevisionInvalid"

$itemsById = @{}
foreach ($item in @($worldCatalog.items)) { $itemsById[[string] $item.id] = $item }

$notRequired = @{}
foreach ($id in @($priority.notRequiredWiIds)) {
    Require ($itemsById.ContainsKey([string] $id)) "NotRequiredWiUnknown:$id"
    $notRequired[[string] $id] = $true
}
$contextual = @{}
foreach ($id in @($priority.contextualWiIds)) {
    Require ($itemsById.ContainsKey([string] $id)) "ContextualWiUnknown:$id"
    Require (-not $notRequired.ContainsKey([string] $id)) "SpatialParticipationOverlap:$id"
    $contextual[[string] $id] = $true
}

$priorityByWi = @{}
foreach ($group in @($priority.priorityGroups)) {
    foreach ($id in @($group.wiIds)) {
        Require ($itemsById.ContainsKey([string] $id)) "PriorityWiUnknown:$id"
        Require (-not $priorityByWi.ContainsKey([string] $id)) "PriorityWiDuplicate:$id"
        $priorityByWi[[string] $id] = [string] $group.priorityCode
    }
}

$officialDefinitions = @()
$officialByWi = @{}
foreach ($reference in @($seedbedCatalog.definitionRefs)) {
    $definition = Read-Json (Join-Path $seedbedRoot ([string] $reference))
    Require ([string] $definition.reviewStatusCode -eq "ApprovedForSimulation") "OfficialH1NotApproved:$($definition.stableId)"
    $officialDefinitions += $definition
    foreach ($id in @($definition.includedWiIds)) {
        Require ($itemsById.ContainsKey([string] $id)) "OfficialH1WiUnknown:$id"
        Add-MapValue $officialByWi ([string] $id) $definition
    }
}

function Load-DesignDefinitions([object[]] $References) {
    return @($References | ForEach-Object {
        Read-Json (Join-Path $designRoot ([string] $_.definitionPath))
    })
}

$interactionH1 = Load-DesignDefinitions @($designCatalog.h1InteractionDefinitionRefs)
$expressionH1 = Load-DesignDefinitions @($designCatalog.h1ExpressionDefinitionRefs)
$h2Candidates = Load-DesignDefinitions @($designCatalog.h2DefinitionRefs)
$h3Candidates = Load-DesignDefinitions @($designCatalog.h3DefinitionRefs)
$h4Candidates = Load-DesignDefinitions @($designCatalog.h4DefinitionRefs)

$interactionH1ByWi = @{}
foreach ($definition in $interactionH1) {
    foreach ($id in @($definition.wiIds)) {
        Require ($itemsById.ContainsKey([string] $id)) "InteractionH1WiUnknown:$id"
        Add-MapValue $interactionH1ByWi ([string] $id) $definition
    }
}

$graphBindingsByWi = @{}
foreach ($binding in @($bindings.bindings)) {
    Require ($itemsById.ContainsKey([string] $binding.worldInteractionId)) "GraphBindingWiUnknown:$($binding.worldInteractionId)"
    Add-MapValue $graphBindingsByWi ([string] $binding.worldInteractionId) $binding
}

$officialH2Root = Join-Path $repositoryRoot "eng/world-seedbeds/landscape-blocks"
$officialH2Count = if (Test-Path -LiteralPath $officialH2Root) {
    @(Get-ChildItem -LiteralPath $officialH2Root -Filter "*.json" -File).Count
} else { 0 }

$planWiIds = @($compositionPlan.worldInteractionSequence)
Require (($planWiIds -join ",") -eq "WI-FARM-04,WI-FARM-05,WI-FARM-06,WI-LOG-01") "CompositionPlanSequenceInvalid"
$planSpaceCodes = @($compositionPlan.spaces.spaceCode)
Require ($planSpaceCodes.Count -eq @($planSpaceCodes | Sort-Object -Unique).Count) "CompositionPlanSpaceDuplicate"
foreach ($space in @($compositionPlan.spaces)) {
    Require (@($officialDefinitions.stableId) -contains [string] $space.h1DefinitionRef) "CompositionPlanOfficialH1Unknown:$($space.h1DefinitionRef)"
    foreach ($id in @($space.worldInteractionIds)) { Require ($planWiIds -contains [string] $id) "CompositionPlanWiUnknown:$id" }
    $allowedCompositionKeys = @($space.expressionGrammarSetRefs | ForEach-Object { "$($_):A" })
    Require ($allowedCompositionKeys -contains [string] $space.preferredCompositionKey) "CompositionPlanPreferredCompositionInvalid:$($space.spaceCode)"
}
foreach ($relation in @($compositionPlan.relations)) {
    Require ($planSpaceCodes -contains [string] $relation.fromSpaceCode) "CompositionPlanRelationFromUnknown:$($relation.fromSpaceCode)"
    Require ($planSpaceCodes -contains [string] $relation.toSpaceCode) "CompositionPlanRelationToUnknown:$($relation.toSpaceCode)"
}
$compositionPlanRaw = Get-Content -LiteralPath $compositionPlanPath -Raw -Encoding UTF8
Require (-not ($compositionPlanRaw -match '"(absoluteWorldPosition|worldEastingMeters|worldNorthingMeters|latitude|longitude|prefabPath|assetGuid|scenePath)"')) "CompositionPlanAuthorityFieldForbidden"
Assert-RealityGroundingBoundary $compositionPlan $compositionPlanRaw "CompositionPlan"

$p2PlanWiIds = @($p2CompositionPlan.worldInteractionSequence)
Require (($p2PlanWiIds -join ",") -eq "WI-LOG-04,WI-LOG-05,WI-001,WI-002") "P2CompositionPlanSequenceInvalid"
$p2PlanSpaceCodes = @($p2CompositionPlan.spaces.spaceCode)
Require ($p2PlanSpaceCodes.Count -eq @($p2PlanSpaceCodes | Sort-Object -Unique).Count) "P2CompositionPlanSpaceDuplicate"
foreach ($space in @($p2CompositionPlan.spaces)) {
    Require (@($officialDefinitions.stableId) -contains [string] $space.h1DefinitionRef) "P2CompositionPlanOfficialH1Unknown:$($space.h1DefinitionRef)"
    foreach ($id in @($space.worldInteractionIds)) { Require ($p2PlanWiIds -contains [string] $id) "P2CompositionPlanWiUnknown:$id" }
    $allowedCompositionKeys = @($space.expressionGrammarSetRefs | ForEach-Object { "$($_):A" })
    Require ($allowedCompositionKeys -contains [string] $space.preferredCompositionKey) "P2CompositionPlanPreferredCompositionInvalid:$($space.spaceCode)"
}
foreach ($relation in @($p2CompositionPlan.relations)) {
    Require ($p2PlanSpaceCodes -contains [string] $relation.fromSpaceCode) "P2CompositionPlanRelationFromUnknown:$($relation.fromSpaceCode)"
    Require ($p2PlanSpaceCodes -contains [string] $relation.toSpaceCode) "P2CompositionPlanRelationToUnknown:$($relation.toSpaceCode)"
}
$p2CompositionPlanRaw = Get-Content -LiteralPath $p2CompositionPlanPath -Raw -Encoding UTF8
Require (-not ($p2CompositionPlanRaw -match '"(absoluteWorldPosition|worldEastingMeters|worldNorthingMeters|latitude|longitude|prefabPath|assetGuid|scenePath)"')) "P2CompositionPlanAuthorityFieldForbidden"
Assert-RealityGroundingBoundary $p2CompositionPlan $p2CompositionPlanRaw "P2CompositionPlan"

$rows = @()
foreach ($item in @($worldCatalog.items | Sort-Object groupCode, sequence, id)) {
    $id = [string] $item.id
    $participation = if ($notRequired.ContainsKey($id)) { "NotRequired" }
        elseif ($contextual.ContainsKey($id)) { "Contextual" }
        else { "Required" }
    $priorityCode = if ($priorityByWi.ContainsKey($id)) { [string] $priorityByWi[$id] }
        else { [string] $priority.defaultPriorityCode }
    $official = @(if ($officialByWi.ContainsKey($id)) { $officialByWi[$id].ToArray() } else { @() })
    $interaction = @(if ($interactionH1ByWi.ContainsKey($id)) { $interactionH1ByWi[$id].ToArray() } else { @() })
    $interactionIds = @($interaction | ForEach-Object stableId | Sort-Object -Unique)
    $expression = @($expressionH1 | Where-Object {
        $supported = @($_.supportsInteractionH1Refs)
        @($supported | Where-Object { $interactionIds -contains [string] $_ }).Count -gt 0
    })
    $h2 = @($h2Candidates | Where-Object {
        $children = @(Get-Values $_ "requiredH1Refs") + @(Get-Values $_ "optionalH1Refs")
        @($children | Where-Object { $interactionIds -contains [string] $_ }).Count -gt 0
    })
    $h2Ids = @($h2 | ForEach-Object stableId | Sort-Object -Unique)
    $h3 = @($h3Candidates | Where-Object {
        $children = @(Get-Values $_ "requiredH2Refs") + @(Get-Values $_ "optionalH2Refs")
        @($children | Where-Object { $h2Ids -contains [string] $_ }).Count -gt 0
    })
    $h3Ids = @($h3 | ForEach-Object stableId | Sort-Object -Unique)
    $h4 = @($h4Candidates | Where-Object {
        $children = @(Get-Values $_ "requiredH3Refs") + @(Get-Values $_ "optionalH3Refs")
        @($children | Where-Object { $h3Ids -contains [string] $_ }).Count -gt 0
    })
    $graphBindings = @(if ($graphBindingsByWi.ContainsKey($id)) { $graphBindingsByWi[$id].ToArray() } else { @() })
    $e5Refs = @(Get-Values $item.integration "e5PlacementRefs")
    $warnings = [Collections.Generic.List[string]]::new()
    if ($participation -eq "Required" -and $interaction.Count -eq 0 -and $official.Count -eq 0) {
        $warnings.Add("RequiredSpatialDesignMissing")
    }
    if ($graphBindings.Count -gt 0 -and $official.Count -eq 0) { $warnings.Add("GraphBindingWithoutApprovedH1") }
    $resolvedE5Bindings = @($e5Refs | Where-Object { $actualE5BindingsById.ContainsKey([string] $_) })
    if ($e5Refs.Count -gt 0 -and $resolvedE5Bindings.Count -ne $e5Refs.Count) { $warnings.Add("E5PlacementReferenceMissing") }
    if ([string] $item.integration.currentStage -eq "E4" -and $official.Count -eq 0) { $warnings.Add("E4WithoutApprovedH1") }
    if ([string] $item.integration.currentStage -in @("E5", "E6", "E7") -and
        $participation -eq "Required" -and $e5Refs.Count -eq 0) {
        $warnings.Add("E5WithoutActualPlacement")
    }

    $designState = if ($participation -eq "NotRequired") { "NotApplicable" }
        elseif ([string] $item.integration.currentStage -in @("E5", "E6", "E7") -and $e5Refs.Count -gt 0 -and $resolvedE5Bindings.Count -eq $e5Refs.Count) { "EstablishedH3" }
        elseif ($official.Count -gt 0 -and [string] $item.integration.currentStage -in @("E4", "E5", "E6", "E7")) { "EstablishedH1" }
        elseif ($interaction.Count -gt 0) { "CandidateLineage" }
        elseif ($participation -eq "Required") { "MissingRequired" }
        else { "NeedsDecision" }
    $engineState = switch ($designState) {
        "EstablishedH3" { "ReadyForActualE5Input" }
        "EstablishedH1" { "ReadyForApprovedH1Input" }
        "CandidateLineage" { "DesignCandidateOnly" }
        "NotApplicable" { "NotApplicable" }
        default { "BlockedMissingDesign" }
    }

    $rows += [pscustomobject][ordered]@{
        worldInteractionId = $id
        groupCode = [string] $item.groupCode
        groupDisplayName = [string] $worldCatalog.groupDisplayNames.PSObject.Properties[
            [string] $item.groupCode].Value
        sequence = [int] $item.sequence
        title = [string] $item.title
        interactionKindCode = [string] $item.kind
        implementationEvidenceStage = [string] $item.implementation.currentStage
        integrationEvidenceStage = [string] $item.integration.currentStage
        spatialParticipationCode = $participation
        priorityCode = $priorityCode
        spatialDesignStateCode = $designState
        highestEstablishedHLevelCode = if ($designState -eq "EstablishedH3") { "H3" } elseif ($designState -eq "EstablishedH1") { "H1" } else { "" }
        approvedH1DefinitionRefs = @($official | ForEach-Object stableId | Sort-Object -Unique)
        interactionH1CandidateRefs = $interactionIds
        expressionH1CandidateRefs = @($expression | ForEach-Object stableId | Sort-Object -Unique)
        h2CandidateRefs = $h2Ids
        h3CandidateRefs = $h3Ids
        h4CandidateRefs = @($h4 | ForEach-Object stableId | Sort-Object -Unique)
        graphBindingRefs = @($graphBindings | ForEach-Object bindingStableId | Sort-Object -Unique)
        e5PlacementCandidateRefs = $e5Refs
        requiredSpatialCapabilityCodes = @($item.spatialRequirements | Sort-Object -Unique)
        lhEngineHandoffStateCode = $engineState
        warningCodes = @($warnings | Sort-Object -Unique)
    }
}

$summary = [ordered]@{
    totalWorldInteractions = $rows.Count
    implementationE3Count = @($rows | Where-Object implementationEvidenceStage -eq "E3").Count
    establishedH1Count = @($rows | Where-Object spatialDesignStateCode -eq "EstablishedH1").Count
    establishedH3Count = @($rows | Where-Object spatialDesignStateCode -eq "EstablishedH3").Count
    candidateLineageCount = @($rows | Where-Object spatialDesignStateCode -eq "CandidateLineage").Count
    missingRequiredCount = @($rows | Where-Object spatialDesignStateCode -eq "MissingRequired").Count
    notApplicableCount = @($rows | Where-Object spatialDesignStateCode -eq "NotApplicable").Count
    officialH1DefinitionCount = $officialDefinitions.Count
    officialH2DefinitionCount = $officialH2Count
    definedH3Count = 5
    definedH4Count = 1
}
Require ($summary.implementationE3Count -eq 60) "ImplementationE3CountMustBe60"
Require ($summary.establishedH1Count -eq 14) "EstablishedH1CountMustBe14"
Require ($summary.establishedH3Count -eq 15) "EstablishedH3CountMustBe15"
Require ($summary.candidateLineageCount -eq 21) "CandidateLineageCountMustBe21"
Require ($summary.missingRequiredCount -eq 5) "MissingRequiredCountMustBe5"
Require ($summary.notApplicableCount -eq 9) "NotApplicableCountMustBe9"
Require ($summary.officialH2DefinitionCount -eq 0) "OfficialH2MustRemainZero"
$missingSpatialIds = @($rows | Where-Object {
    $_.warningCodes -contains "RequiredSpatialDesignMissing"
} | Sort-Object worldInteractionId | ForEach-Object worldInteractionId)
Require (($missingSpatialIds -join ",") -eq "WI-CITY-01,WI-CITY-02,WI-CITY-03,WI-CITY-04,WI-REFLECT-01") `
    "OnlyKnownSpatialGapsAllowed"
Require (@($rows | Where-Object { $_.warningCodes -contains "GraphBindingWithoutApprovedH1" }).worldInteractionId -contains "WI-WORLD-04") "FacilityRepairBindingGapMustBeVisible"

$payload = [ordered]@{
    schemaVersion = "simulation-world-interaction-eh-status.v1"
    revision = "simulation-world-interaction-eh-status.r1"
    generatedFrom = [ordered]@{
        worldInteractionCatalogRevision = [string] $worldCatalog.revision
        spatialPriorityRevision = [string] $priority.revision
        designKnowledgeRevision = [string] $designCatalog.revision
        resourceInventoryRevision = [string] $resourceInventory.revision
        compositionPlanStableId = [string] $compositionPlan.planStableId
        compositionPlanRevision = [int] $compositionPlan.revision
        p2CompositionPlanStableId = [string] $p2CompositionPlan.planStableId
        p2CompositionPlanRevision = [int] $p2CompositionPlan.revision
    }
    summary = $summary
    items = $rows
    authorityBoundary = [ordered]@{
        candidateLineageDoesNotRaiseEvidence = $true
        authoredGraphBindingAloneDoesNotProveE5 = $true
        actualE5BindingWithRevisionAndHashProvesH3Placement = $true
        scenarioFallbackForbiddenForGraphRequest = $true
        lhEngineConsumesApprovedH1Only = $true
        optionalRealityGroundingDoesNotBlockScenarioExecution = $true
        demAndRoadAreNotGlobalRequirements = $true
    }
    presentationOnly = $true
    isOperationalState = $false
}
$payloadJson = ConvertTo-DeterministicText ($payload | ConvertTo-Json -Depth 12)
$output = [ordered]@{}
foreach ($property in $payload.Keys) { $output[$property] = $payload[$property] }
$output["contentHashSha256"] = Get-Hash $payloadJson
$expectedJson = ConvertTo-DeterministicText ($output | ConvertTo-Json -Depth 12)

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# WI별 E/H 공간 성립 현황")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 E/H 원장·공간 재고·공식 H 정의를 대조해 자동 생성한다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- WI: ``$($summary.totalWorldInteractions)개`` · E3: ``$($summary.implementationE3Count)개``")
[void] $builder.AppendLine("- E4/H1 실행 성립: ``$($summary.establishedH1Count)개``")
[void] $builder.AppendLine("- E5/H3 실제 공간 결속: ``$($summary.establishedH3Count)개``")
[void] $builder.AppendLine("- H1~H4 설계 후보 계보만 존재: ``$($summary.candidateLineageCount)개``")
[void] $builder.AppendLine("- 필수 공간 설계 누락: ``$($summary.missingRequiredCount)개``")
[void] $builder.AppendLine("- 공간 비적용: ``$($summary.notApplicableCount)개``")
[void] $builder.AppendLine("- 공식 H 정의: ``H1 $($summary.officialH1DefinitionCount) / H2 $($summary.officialH2DefinitionCount) / H3 $($summary.definedH3Count) / H4 $($summary.definedH4Count)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("후보 H2·H3·H4 계보와 Graph binding은 설계 입력이며 E 단계나 실제 배치를 자동 승격하지 않는다.")

foreach ($group in @($rows | Group-Object groupCode)) {
    $groupDisplayName = [string] $group.Group[0].groupDisplayName
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("## $groupDisplayName (``$($group.Name)``)")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |")
    [void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |")
    foreach ($row in @($group.Group | Sort-Object sequence, worldInteractionId)) {
        $established = if ([string]::IsNullOrWhiteSpace([string] $row.highestEstablishedHLevelCode)) { "-" } else { [string] $row.highestEstablishedHLevelCode }
        [void] $builder.AppendLine("| $(Escape-Markdown $row.title) · ``$($row.worldInteractionId)`` | ``$($row.implementationEvidenceStage)/$($row.integrationEvidenceStage)`` | ``$($row.spatialParticipationCode)`` | ``$established`` | ``$($row.spatialDesignStateCode)`` | ``$($row.priorityCode)`` | ``$($row.lhEngineHandoffStateCode)`` | $(Escape-Markdown (@($row.warningCodes) -join ', ')) |")
    }
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## P1 기준 플레이 공간 구성")
[void] $builder.AppendLine()
[void] $builder.AppendLine("``WI-FARM-04 → WI-FARM-05 → WI-FARM-06 → WI-LOG-01``을 생산구획 → 집하 → 포장 → 상차 공간으로 연결한다.")
[void] $builder.AppendLine("실행 입력은 ``eng/world-seedbeds/wi-spatial-composition-plans/reference-play-01-harvest-shipping.v1.json``에 있다. H 설계와 Scenario 실행은 공공데이터와 독립이다. DEM·토지피복·도로·Block 경계는 현실 정합을 선택할 때 사용하는 E6 후보 목적이며, 미적용 상태는 H 공간이나 Scenario E7을 차단하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## P2 진부 Hub 입고·보관 공간 구성")
[void] $builder.AppendLine()
[void] $builder.AppendLine("``WI-LOG-04 → WI-LOG-05 → WI-001 → WI-002``를 하차 공간 → 인수·검수 공간 → 창고 적재 공간으로 연결한다.")
[void] $builder.AppendLine("실행 입력은 ``eng/world-seedbeds/wi-spatial-composition-plans/p2-hub-inbound-storage.v1.json``에 있다. 진부 Hub의 권위 업무 Node와 E5 배치 Block이 없어 지역 인스턴스 후보로 유지한다. 도로·건물·Block 경계는 현실 정합을 선택할 때만 E6 후보 목적이 되며 Scenario 공간 실행을 막지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 확인이 필요한 공백")
[void] $builder.AppendLine()
foreach ($row in @($rows | Where-Object { @($_.warningCodes).Count -gt 0 })) {
    [void] $builder.AppendLine("- ``$($row.worldInteractionId)`` $($row.title): ``$(@($row.warningCodes) -join ', ')``")
}

$expectedMarkdown = ConvertTo-DeterministicText $builder.ToString()
$resolvedJsonOutput = Join-Path $repositoryRoot $OutputJsonPath
$resolvedMarkdownOutput = Join-Path $repositoryRoot $OutputMarkdownPath
if ($Mode -eq "Write") {
    Write-DeterministicTextIfChanged $resolvedJsonOutput $expectedJson | Out-Null
    Write-DeterministicTextIfChanged $resolvedMarkdownOutput $expectedMarkdown | Out-Null
    Write-Output "WorldInteractionEhStatusGenerated:Items=$($rows.Count);EstablishedH1=$($summary.establishedH1Count);EstablishedH3=$($summary.establishedH3Count);Candidate=$($summary.candidateLineageCount);Missing=$($summary.missingRequiredCount)"
}
else {
    Require (Test-Path -LiteralPath $resolvedJsonOutput) "GeneratedJsonMissing"
    Require (Test-Path -LiteralPath $resolvedMarkdownOutput) "GeneratedMarkdownMissing"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedJsonOutput))) -ceq $expectedJson) "GeneratedJsonOutOfDate"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedMarkdownOutput))) -ceq $expectedMarkdown) "GeneratedMarkdownOutOfDate"
    Write-Output "WorldInteractionEhStatusValid:Items=$($rows.Count);EstablishedH1=$($summary.establishedH1Count);EstablishedH3=$($summary.establishedH3Count);Candidate=$($summary.candidateLineageCount);Missing=$($summary.missingRequiredCount)"
}
