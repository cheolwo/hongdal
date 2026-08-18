$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$path = Join-Path $repositoryRoot "eng/world-seedbeds/landscape-block-candidates/daegwallyeong-harvest-day.v1.json"
$candidate = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json

if ([string] $candidate.schemaVersion -ne "simulation-world-landscape-block-candidate.v1") {
    throw "LandscapeBlockCandidateSchemaInvalid"
}
if ([string] $candidate.stateCode -ne "WaitingForRoadBoundaryEvidence") {
    throw "LandscapeBlockCandidateMustWaitForRoadBoundaryEvidence"
}
if ([int] $candidate.targetWalkDurationSeconds -ne 90) {
    throw "LandscapeBlockCandidateWalkDurationInvalid"
}
if ((@($candidate.pathRoleSequence) -join ",") -ne
    "farmhouse-start,potato-production-plot,collection-packing-yard,farm-loading-gate") {
    throw "LandscapeBlockCandidatePathSequenceInvalid"
}
$seedbedIds = @($candidate.seedbedBindings.seedbedStableId)
foreach ($required in @(
    "wi-spatial-seedbed:farm-production.v1",
    "wi-spatial-seedbed:farm-work-yard.v1",
    "wi-spatial-seedbed:farm-loading-gate.v1")) {
    if ($seedbedIds -notcontains $required) {
        throw "LandscapeBlockCandidateSeedbedMissing:$required"
    }
}
$road = @($candidate.evidenceInputs | Where-Object evidenceCode -eq "RoadNetworkAndJunction")
$boundary = @($candidate.evidenceInputs | Where-Object evidenceCode -eq "BlockBoundary")
if ($road.Count -ne 1 -or [string] $road[0].stateCode -ne "WaitingForSource") {
    throw "LandscapeBlockCandidateRoadGateInvalid"
}
if ($boundary.Count -ne 1 -or [string] $boundary[0].stateCode -ne "WaitingForDerivation") {
    throw "LandscapeBlockCandidateBoundaryGateInvalid"
}
if (([string] $candidate.promotionGate.targetHierarchyLevelCode -ne "H2") -or
    (-not [bool] $candidate.promotionGate.requiresSceneApplyApproval)) {
    throw "LandscapeBlockCandidatePromotionGateInvalid"
}
if (-not [bool] $candidate.presentationOnly -or [bool] $candidate.isOperationalState) {
    throw "LandscapeBlockCandidateAuthorityBoundaryInvalid"
}

Write-Output "LandscapeBlockCandidateTestsPassed:WaitingForRoadBoundaryEvidence"
