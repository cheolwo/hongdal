$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$managementPath = Join-Path $repositoryRoot "eng/execution-ledgers/evidence-management-systems.json"
$management = Get-Content -LiteralPath $managementPath -Raw -Encoding UTF8 | ConvertFrom-Json
$stagesPath = Join-Path $repositoryRoot ([string] $management.evidenceStageCatalogPath)
$stages = Get-Content -LiteralPath $stagesPath -Raw -Encoding UTF8 | ConvertFrom-Json
$verticalProtocolPath = Join-Path $repositoryRoot (
    [string] $management.targetFirstVerticalImplementationProtocolPath)
$workOrderTemplatePath = Join-Path $repositoryRoot (
    [string] $management.targetFirstWorkOrderTemplatePath)
$responsibilityMapPath = Join-Path $repositoryRoot `
    "docs/AI/generated/evidence-responsibility-code-map.json"

if ([string] $management.schemaVersion -ne "simulation-evidence-management-systems.v1") {
    throw "EvidenceManagementSystemSchemaInvalid"
}
if ((@($stages.stages.code) -join ",") -ne "E0,E1,E2,E3,E4,E5,E6,E7,E8,E9") {
    throw "EvidenceManagementStageOrderInvalid"
}
if ((@($management.systems.code) -join ",") -ne "G1,G2,G3,G4") {
    throw "EvidenceManagementSystemOrderInvalid"
}
if (-not [bool] $management.principles.managementSystemsAreNotEvidenceStages -or
    -not [bool] $management.principles.managementCompletionDoesNotPromoteEvidence -or
    -not [bool] $management.principles.promotionRequiresStageCompletionGateEvidence -or
    -not [bool] $management.principles.targetFirstPlanningDoesNotPromoteEvidence) {
    throw "EvidenceManagementAxisBoundaryInvalid"
}
if (-not (Test-Path -LiteralPath $verticalProtocolPath) -or
    -not (Test-Path -LiteralPath $workOrderTemplatePath)) {
    throw "EvidenceManagementVerticalProtocolMissing"
}
$verticalProtocol = Get-Content -LiteralPath $verticalProtocolPath -Raw -Encoding UTF8 |
    ConvertFrom-Json
if ((@($verticalProtocol.downwardReviewOrder) -join ",") -ne
    "E9,E8,E7,E6,E5,E4,E3,E2,E1" -or
    (@($verticalProtocol.upwardValidationOrder) -join ",") -ne
    "E1,E2,E3,E4,E5,E6,E7,E8,E9" -or
    [string] $verticalProtocol.iterationCycle.cycleCode -ne "E9-E1-E9-Repeat") {
    throw "EvidenceManagementVerticalProtocolOrderInvalid"
}

$expectedTransitions = @("E1>E6", "E6>E7", "E7>E8", "E8>E9")
$actualTransitions = @($management.systems | ForEach-Object {
    "$($_.primaryTransition.from)>$($_.primaryTransition.to)"
})
if (($actualTransitions -join ",") -ne ($expectedTransitions -join ",")) {
    throw "EvidenceManagementTransitionInvalid"
}

$stageCodes = @($stages.stages.code)
foreach ($system in @($management.systems)) {
    if ([string]::IsNullOrWhiteSpace([string] $system.label) -or
        [string]::IsNullOrWhiteSpace([string] $system.question) -or
        @($system.responsibilities).Count -eq 0) {
        throw "EvidenceManagementSystemDefinitionIncomplete:$($system.code)"
    }
    if ([string] $system.primaryTransition.from -notin $stageCodes -or
        [string] $system.primaryTransition.to -notin $stageCodes) {
        throw "EvidenceManagementTransitionStageMissing:$($system.code)"
    }
}

$responsibilityMap = Get-Content -LiteralPath $responsibilityMapPath -Raw `
    -Encoding UTF8 | ConvertFrom-Json
if ([string] $responsibilityMap.schemaVersion -ne
    "ssalddel-evidence-responsibility-map.v2") {
    throw "EvidenceManagementResponsibilityMapSchemaInvalid"
}
$submodules = @($responsibilityMap.submodules)
if ($submodules.Count -ne 15 -or
    @($submodules | Where-Object evidenceStage -eq "E1").Count -ne 5 -or
    @($submodules | Where-Object evidenceStage -eq "E2").Count -ne 5 -or
    @($submodules | Where-Object evidenceStage -eq "E3").Count -ne 5) {
    throw "EvidenceManagementSubmoduleCatalogInvalid"
}
if (@($submodules | Where-Object {
        ([int] $_.primaryCount + [int] $_.secondaryCount) -eq 0
    }).Count -gt 0) {
    throw "EvidenceManagementSubmoduleBindingMissing"
}
$e5Components = @($responsibilityMap.components | Where-Object {
    $_.PSObject.Properties.Name -contains "primaryEvidenceStage" -and
    [string] $_.primaryEvidenceStage -eq "E5"
})
if ($e5Components.Count -eq 0) {
    throw "EvidenceManagementE5ComponentMissing"
}
$invalidE5Components = @($e5Components | Where-Object {
    [string] $_.assemblyName -like "Ssalddel.Simulation.Persistence*" -or
    [string] $_.assemblyName -like "Ssalddel.Unity*"
})
if ($invalidE5Components.Count -gt 0) {
    throw "EvidenceManagementE5BoundaryInvalid:$($invalidE5Components[0].componentName)"
}
if (-not ($e5Components | Where-Object {
        [string] $_.assemblyName -eq "Ssalddel.Simulation.Application"
    }) -or -not ($e5Components | Where-Object {
        [string] $_.assemblyName -eq "Ssalddel.Simulation.Domain"
    })) {
    throw "EvidenceManagementE5ExecutionPipelineMissing"
}

Write-Output "EvidenceManagementSystemsTestsPassed:G1=E1-E6;G2=E6-E7;G3=E7-E8;G4=E8-E9;Cycle=E9-E1-E9-Repeat;E1E3Submodules=$($submodules.Count);E5Components=$($e5Components.Count)"
