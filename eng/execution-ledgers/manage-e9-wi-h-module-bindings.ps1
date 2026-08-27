$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$bindingPath = Join-Path $repositoryRoot "eng/execution-ledgers/e9-wi-h-module-bindings.json"
$binding = Get-Content -LiteralPath $bindingPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($binding.schemaVersion -ne "e9-wi-h-module-bindings.v1") { throw "E9WiHModuleBindingsInvalid:SchemaVersion" }
if (-not [bool] $binding.principles.wiOrWiLoopIsEvidenceSubject -or
    -not [bool] $binding.principles.spatialEvidenceIsConditionalInput) {
    throw "E9WiHModuleBindingsInvalid:WiCenteredPrinciplesMissing"
}

$wiCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.worldInteractionCatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$moduleCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.e9ModuleCatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$spatialRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.gameplaySpatialCompletionPath) -Raw -Encoding UTF8
$actualSpatialRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.actualE5SpatialPath) -Raw -Encoding UTF8
$hRoot = Join-Path $repositoryRoot $binding.sources.hDefinitionRootPath
$hDefinitions = @(Get-ChildItem -LiteralPath $hRoot -Recurse -File -Filter "*.json" | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 | ConvertFrom-Json })
$h1SeedbedRoot = Join-Path $repositoryRoot $binding.sources.h1SeedbedRootPath
$h1Seedbeds = @(Get-ChildItem -LiteralPath $h1SeedbedRoot -File -Filter "*.json" | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 | ConvertFrom-Json })
$hDefinitionRaw = ($hDefinitions | ConvertTo-Json -Depth 20) -join "`n"
$h1SeedbedRaw = ($h1Seedbeds | ConvertTo-Json -Depth 20) -join "`n"
$runtimeInterfaceRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.e2RuntimeBoundary.interfaceSourcePath) -Raw -Encoding UTF8
$localAdapterRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.e2RuntimeBoundary.localAdapterSourcePath) -Raw -Encoding UTF8
$downwardSkeletonRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.downwardCodeSkeleton.sourcePath) -Raw -Encoding UTF8
$natureSurvivalContractsRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.natureSurvivalContractsSourcePath) -Raw -Encoding UTF8
$areaBuildingContractsRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.areaBuildingContractsSourcePath) -Raw -Encoding UTF8
if (-not $runtimeInterfaceRaw.Contains("interface $($binding.e2RuntimeBoundary.portTechnicalName)")) { throw "E9WiHModuleBindingsInvalid:E2RuntimePortMissing" }

$expectedStages = @("E9", "E8", "E7", "E6", "E5", "E4", "E3", "E2", "E1")
$responsibilityStages = @($binding.stageResponsibilities | ForEach-Object { $_.evidenceStage })
if (($responsibilityStages -join ",") -ne ($expectedStages -join ",")) { throw "E9WiHModuleBindingsInvalid:StageOrder" }
foreach ($responsibility in $binding.stageResponsibilities) {
    $module = @($moduleCatalog.modules | Where-Object { $_.evidenceStage -eq $responsibility.evidenceStage })
    if ($module.Count -ne 1 -or $module[0].technicalName -ne $responsibility.moduleTechnicalName) { throw "E9WiHModuleBindingsInvalid:ModuleReference:$($responsibility.evidenceStage)" }
}

if ($binding.downwardCodeSkeleton.stateCode -ne "SkeletonNamed" -or
    $binding.downwardCodeSkeleton.currentPass -ne "DownwardImpactReview") {
    throw "E9WiHModuleBindingsInvalid:DownwardSkeletonState"
}
if ($binding.downwardCodeSkeleton.runtimeWired -ne $false) {
    throw "E9WiHModuleBindingsInvalid:DownwardSkeletonUnexpectedRuntimeWiring"
}
if (-not $downwardSkeletonRaw.Contains("interface $($binding.downwardCodeSkeleton.compositeInterfaceTechnicalName)") -or
    -not $downwardSkeletonRaw.Contains("class $($binding.downwardCodeSkeleton.executionHeadCatalogTechnicalName)")) {
    throw "E9WiHModuleBindingsInvalid:DownwardSkeletonTypeMissing"
}
if ($localAdapterRaw.Contains($binding.downwardCodeSkeleton.compositeInterfaceTechnicalName)) {
    throw "E9WiHModuleBindingsInvalid:DownwardSkeletonAlreadyRuntimeWired"
}
$skeletonStages = @($binding.downwardCodeSkeleton.stageModuleHeads | ForEach-Object { $_.evidenceStage })
if (($skeletonStages -join ",") -ne ($expectedStages -join ",")) {
    throw "E9WiHModuleBindingsInvalid:DownwardSkeletonStageOrder"
}
foreach ($head in $binding.downwardCodeSkeleton.stageModuleHeads) {
    $responsibility = @($binding.stageResponsibilities | Where-Object { $_.evidenceStage -eq $head.evidenceStage })
    if ($responsibility.Count -ne 1 -or $responsibility[0].moduleTechnicalName -ne $head.moduleTechnicalName) {
        throw "E9WiHModuleBindingsInvalid:DownwardSkeletonModule:$($head.evidenceStage)"
    }
    if (-not $downwardSkeletonRaw.Contains("interface $($head.interfaceTechnicalName)") -or
        -not $downwardSkeletonRaw.Contains($head.methodName)) {
        throw "E9WiHModuleBindingsInvalid:DownwardSkeletonHeadMissing:$($head.evidenceStage)"
    }
}

$allowedStates = @($binding.allowedBindingStateCodes)
$spatialStateCodes = @("Bound", "RequiredMissing", "NotApplicable")
$spatialEvidenceByWi = @{}
foreach ($stateCode in $spatialStateCodes) {
    foreach ($wiId in @($binding.spatialEvidenceByWi.$stateCode)) {
        if ($spatialEvidenceByWi.ContainsKey($wiId)) {
            throw "E9WiHModuleBindingsInvalid:SpatialEvidenceDuplicate:$wiId"
        }
        $spatialEvidenceByWi[$wiId] = $stateCode
    }
}
$seenBindingIds = @{}
$seenWiIds = @{}
foreach ($item in $binding.bindings) {
    if ($seenBindingIds.ContainsKey($item.bindingId)) { throw "E9WiHModuleBindingsInvalid:DuplicateBinding:$($item.bindingId)" }
    if ($seenWiIds.ContainsKey($item.wiId)) { throw "E9WiHModuleBindingsInvalid:DuplicateWi:$($item.wiId)" }
    $seenBindingIds[$item.bindingId] = $true
    $seenWiIds[$item.wiId] = $true
    if (-not $spatialEvidenceByWi.ContainsKey($item.wiId)) {
        throw "E9WiHModuleBindingsInvalid:SpatialEvidenceMissing:$($item.wiId)"
    }

    $operationsProperty = $binding.e2RuntimeBoundary.wiOperations.PSObject.Properties[$item.wiId]
    if ($null -eq $operationsProperty) { throw "E9WiHModuleBindingsInvalid:E2OperationsMissing:$($item.wiId)" }
    foreach ($operation in @($operationsProperty.Value)) {
        if (-not $runtimeInterfaceRaw.Contains($operation) -or -not $localAdapterRaw.Contains($operation)) { throw "E9WiHModuleBindingsInvalid:E2OperationUnresolved:$($item.wiId):$operation" }
        if (-not $downwardSkeletonRaw.Contains($operation)) { throw "E9WiHModuleBindingsInvalid:ExecutionHeadOperationMissing:$($item.wiId):$operation" }
    }
    if (-not $downwardSkeletonRaw.Contains($item.wiId) -and
        -not $natureSurvivalContractsRaw.Contains($item.wiId) -and
        -not $areaBuildingContractsRaw.Contains($item.wiId)) {
        throw "E9WiHModuleBindingsInvalid:ExecutionHeadWiMissing:$($item.wiId)"
    }

    $wi = @($wiCatalog.items | Where-Object { $_.id -eq $item.wiId })
    if ($wi.Count -ne 1 -or $wi[0].title -ne $item.wiTitle) { throw "E9WiHModuleBindingsInvalid:WiReference:$($item.wiId)" }
    $implementationSnapshot = "$($wi[0].implementation.currentStage)/$($wi[0].implementation.status)"
    $integrationSnapshot = "$($wi[0].integration.currentStage)/$($wi[0].integration.status)"
    if ($item.sourceEvidenceSnapshot.implementation -ne $implementationSnapshot -or $item.sourceEvidenceSnapshot.integration -ne $integrationSnapshot) { throw "E9WiHModuleBindingsInvalid:EvidenceSnapshotStale:$($item.wiId)" }
    $actualPlacementProperty = $item.e4Audit.PSObject.Properties["actualSpatialPlacementStateCode"]
    $actualSpatialPlacementStateCode = if ($null -eq $actualPlacementProperty) { "ActualE5Bound" } else { [string] $actualPlacementProperty.Value }

    foreach ($level in @("H1", "H2", "H3", "H4")) {
        $refs = @($item.hRefs.$level)
        if ($refs.Count -eq 0) { throw "E9WiHModuleBindingsInvalid:EmptySpatialRefs:$($item.wiId):$level" }
        $prefix = $level.ToLowerInvariant() + "-"
        foreach ($ref in $refs) {
            $isH1Seedbed = $level -eq "H1" -and
                $ref.StartsWith("wi-spatial-seedbed:", [System.StringComparison]::Ordinal)
            if (-not $isH1Seedbed -and
                -not $ref.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
                throw "E9WiHModuleBindingsInvalid:SpatialLevelMismatch:$($item.wiId):$ref"
            }
            if ($isH1Seedbed) {
                if (-not $h1SeedbedRaw.Contains($ref)) {
                    throw "E9WiHModuleBindingsInvalid:UnresolvedSpatialRef:$($item.wiId):$ref"
                }
            }
            elseif (-not $hDefinitionRaw.Contains($ref)) {
                throw "E9WiHModuleBindingsInvalid:UnresolvedSpatialRef:$($item.wiId):$ref"
            }
            elseif ($wi[0].integration.currentStage -in @("E5", "E6", "E7") -and
                    -not $spatialRaw.Contains($ref) -and
                    -not $actualSpatialRaw.Contains($ref) -and
                    $actualSpatialPlacementStateCode -ne "ApprovedH1InputOnly") {
                throw "E9WiHModuleBindingsInvalid:SpatialCompletionRefMissing:$($item.wiId):$ref"
            }
        }
    }

    $h1Definitions = @($hDefinitions | Where-Object { $item.hRefs.H1 -contains $_.stableId })
    $boundH1Seedbeds = @($h1Seedbeds | Where-Object { $item.hRefs.H1 -contains $_.stableId })
    $coveredCapabilities = @($h1Definitions | ForEach-Object { @($_.capabilityCodes) }) +
        @($boundH1Seedbeds | ForEach-Object { @($_.internalSpaces.capabilityCodes) })
    $missingCapabilities = @($wi[0].spatialRequirements | Where-Object { $coveredCapabilities -notcontains "Spatial.$_" })
    if ($missingCapabilities.Count -gt 0 -or $item.e4Audit.capabilityCoverageCode -ne "Complete") { throw "E9WiHModuleBindingsInvalid:E4CapabilityCoverage:$($item.wiId):$($missingCapabilities -join ',')" }
    $directH1Claims = @($h1Definitions | Where-Object { @($_.wiIds) -contains $item.wiId }).Count +
        @($boundH1Seedbeds | Where-Object { @($_.includedWiIds) -contains $item.wiId }).Count
    if ($directH1Claims -eq 0 -or $item.e4Audit.directH1WiClaimCode -ne "Present") { throw "E9WiHModuleBindingsInvalid:E4DirectH1WiClaim:$($item.wiId)" }
    if ($actualSpatialPlacementStateCode -eq "ApprovedH1InputOnly" -and
        [string] $item.stageStates.E7 -ne "EvidenceMissing") {
        throw "E9WiHModuleBindingsInvalid:ActualPlacementEvidenceOverstated:$($item.wiId)"
    }
    if ($item.e4Audit.spatialReservationCoverageCode -eq "MissingCapacityConcept" -and
        (@($item.e4Audit.unresolvedSpatialReservationKinds).Count -eq 0 -or
         @($item.e4Audit.proposedCapacityConceptCodes).Count -eq 0)) { throw "E9WiHModuleBindingsInvalid:E4CapacityGapNotNamed:$($item.wiId)" }

    $stateStages = @($item.stageStates.PSObject.Properties.Name)
    if (($stateStages -join ",") -ne ($expectedStages -join ",")) { throw "E9WiHModuleBindingsInvalid:BindingStageOrder:$($item.wiId)" }
    foreach ($stage in $expectedStages) {
        $state = [string] $item.stageStates.$stage
        if ($allowedStates -notcontains $state) { throw "E9WiHModuleBindingsInvalid:UnknownBindingState:$($item.wiId):${stage}:$state" }
    }
    if ([string] $item.stageStates.E4 -notin @("ContextPartiallyBound", "ContextBound") -or
        [string] $item.stageStates.E5 -notin @("ManifestationMissing", "ManifestationPartial", "Manifested")) {
        throw "E9WiHModuleBindingsInvalid:WiCenteredStageState:$($item.wiId)"
    }
}

if ($spatialEvidenceByWi.Count -ne $binding.bindings.Count) {
    throw "E9WiHModuleBindingsInvalid:SpatialEvidenceCoverage"
}

Write-Output "E9WiHModuleBindingsValid:Bindings=$($binding.bindings.Count);Stages=9;Levels=H1-H4;SkeletonHeads=$($binding.downwardCodeSkeleton.stageModuleHeads.Count);RuntimeWired=$($binding.downwardCodeSkeleton.runtimeWired.ToString().ToLowerInvariant())"
