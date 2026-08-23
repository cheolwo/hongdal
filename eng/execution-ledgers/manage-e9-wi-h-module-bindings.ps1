$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$bindingPath = Join-Path $repositoryRoot "eng/execution-ledgers/e9-wi-h-module-bindings.json"
$binding = Get-Content -LiteralPath $bindingPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($binding.schemaVersion -ne "e9-wi-h-module-bindings.v1") { throw "E9WiHModuleBindingsInvalid:SchemaVersion" }

$wiCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.worldInteractionCatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$moduleCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.e9ModuleCatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$spatialRaw = Get-Content -LiteralPath (Join-Path $repositoryRoot $binding.sources.gameplaySpatialCompletionPath) -Raw -Encoding UTF8
$hRoot = Join-Path $repositoryRoot $binding.sources.hDefinitionRootPath
$hDefinitionRaw = (Get-ChildItem -LiteralPath $hRoot -Recurse -File -Filter "*.json" | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 }) -join "`n"

$expectedStages = @("E9", "E8", "E7", "E6", "E5", "E4", "E3", "E2", "E1")
$responsibilityStages = @($binding.stageResponsibilities | ForEach-Object { $_.evidenceStage })
if (($responsibilityStages -join ",") -ne ($expectedStages -join ",")) { throw "E9WiHModuleBindingsInvalid:StageOrder" }
foreach ($responsibility in $binding.stageResponsibilities) {
    $module = @($moduleCatalog.modules | Where-Object { $_.evidenceStage -eq $responsibility.evidenceStage })
    if ($module.Count -ne 1 -or $module[0].technicalName -ne $responsibility.moduleTechnicalName) { throw "E9WiHModuleBindingsInvalid:ModuleReference:$($responsibility.evidenceStage)" }
}

$allowedStates = @($binding.allowedBindingStateCodes)
$seenBindingIds = @{}
$seenWiIds = @{}
foreach ($item in $binding.bindings) {
    if ($seenBindingIds.ContainsKey($item.bindingId)) { throw "E9WiHModuleBindingsInvalid:DuplicateBinding:$($item.bindingId)" }
    if ($seenWiIds.ContainsKey($item.wiId)) { throw "E9WiHModuleBindingsInvalid:DuplicateWi:$($item.wiId)" }
    $seenBindingIds[$item.bindingId] = $true
    $seenWiIds[$item.wiId] = $true

    $wi = @($wiCatalog.items | Where-Object { $_.id -eq $item.wiId })
    if ($wi.Count -ne 1 -or $wi[0].title -ne $item.wiTitle) { throw "E9WiHModuleBindingsInvalid:WiReference:$($item.wiId)" }
    $implementationSnapshot = "$($wi[0].implementation.currentStage)/$($wi[0].implementation.status)"
    $integrationSnapshot = "$($wi[0].integration.currentStage)/$($wi[0].integration.status)"
    if ($item.sourceEvidenceSnapshot.implementation -ne $implementationSnapshot -or $item.sourceEvidenceSnapshot.integration -ne $integrationSnapshot) { throw "E9WiHModuleBindingsInvalid:EvidenceSnapshotStale:$($item.wiId)" }

    foreach ($level in @("H1", "H2", "H3", "H4")) {
        $refs = @($item.hRefs.$level)
        if ($refs.Count -eq 0) { throw "E9WiHModuleBindingsInvalid:EmptySpatialRefs:$($item.wiId):$level" }
        $prefix = $level.ToLowerInvariant() + "-"
        foreach ($ref in $refs) {
            if (-not $ref.StartsWith($prefix, [System.StringComparison]::Ordinal)) { throw "E9WiHModuleBindingsInvalid:SpatialLevelMismatch:$($item.wiId):$ref" }
            if (-not $spatialRaw.Contains($ref) -or -not $hDefinitionRaw.Contains($ref)) { throw "E9WiHModuleBindingsInvalid:UnresolvedSpatialRef:$($item.wiId):$ref" }
        }
    }

    $stateStages = @($item.stageStates.PSObject.Properties.Name)
    if (($stateStages -join ",") -ne ($expectedStages -join ",")) { throw "E9WiHModuleBindingsInvalid:BindingStageOrder:$($item.wiId)" }
    foreach ($stage in $expectedStages) {
        $state = [string] $item.stageStates.$stage
        if ($allowedStates -notcontains $state) { throw "E9WiHModuleBindingsInvalid:UnknownBindingState:$($item.wiId):${stage}:$state" }
    }
}

Write-Output "E9WiHModuleBindingsValid:Bindings=$($binding.bindings.Count);Stages=9;Levels=H1-H4"
