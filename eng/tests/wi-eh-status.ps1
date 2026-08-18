$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/world-seedbeds/manage-wi-eh-status.ps1"
$jsonPath = Join-Path $repositoryRoot "eng/world-seedbeds/generated/wi-eh-status.v1.json"
$markdownPath = Join-Path $repositoryRoot "docs/AI/generated/wi-eh-spatial-status.md"

$first = & $script -Mode Write
$firstJsonHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $jsonPath).Hash
$firstMarkdownHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $markdownPath).Hash
$firstJsonTicks = (Get-Item -LiteralPath $jsonPath).LastWriteTimeUtc.Ticks
$firstMarkdownTicks = (Get-Item -LiteralPath $markdownPath).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$check = & $script -Mode Check

if (($firstJsonHash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $jsonPath).Hash) -or
    ($firstMarkdownHash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $markdownPath).Hash)) {
    throw "WorldInteractionEhStatusGenerationIsNotDeterministic"
}
if (($firstJsonTicks -ne (Get-Item -LiteralPath $jsonPath).LastWriteTimeUtc.Ticks) -or
    ($firstMarkdownTicks -ne (Get-Item -LiteralPath $markdownPath).LastWriteTimeUtc.Ticks)) {
    throw "WorldInteractionEhStatusUnchangedOutputWasRewritten"
}

$status = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string] $status.schemaVersion -ne "simulation-world-interaction-eh-status.v1") {
    throw "WorldInteractionEhStatusSchemaInvalid"
}
$summaryInvalid = $status.summary.totalWorldInteractions -ne 37 -or
    $status.summary.implementationE3Count -ne 37 -or
    $status.summary.establishedH1Count -ne 13 -or
    $status.summary.candidateLineageCount -ne 17 -or
    $status.summary.missingRequiredCount -ne 1 -or
    $status.summary.notApplicableCount -ne 6
if ($summaryInvalid) {
    throw "WorldInteractionEhStatusSummaryInvalid"
}
$hierarchyCountsInvalid = $status.summary.officialH1DefinitionCount -ne 5 -or
    $status.summary.officialH2DefinitionCount -ne 0 -or
    $status.summary.definedH3Count -ne 5 -or
    $status.summary.definedH4Count -ne 1
if ($hierarchyCountsInvalid) {
    throw "WorldInteractionEhStatusHierarchyCountsInvalid"
}
$orderPacking = @($status.items | Where-Object worldInteractionId -eq "WI-ORDER-04")
$orderPackingInvalid = $orderPacking.Count -ne 1 -or
    $orderPacking[0].spatialDesignStateCode -ne "MissingRequired" -or
    @($orderPacking[0].warningCodes) -notcontains "RequiredSpatialDesignMissing"
if ($orderPackingInvalid) {
    throw "WorldInteractionOrderPackingGapMissing"
}
$repair = @($status.items | Where-Object worldInteractionId -eq "WI-WORLD-04")
$repairInvalid = $repair.Count -ne 1 -or
    @($repair[0].warningCodes) -notcontains "GraphBindingWithoutApprovedH1"
if ($repairInvalid) {
    throw "WorldInteractionFacilityRepairGapMissing"
}
if (@($status.items | Where-Object {
    (@($_.e5PlacementCandidateRefs).Count -gt 0) -and
    (@($_.warningCodes) -notcontains "E5PlacementReferenceWithoutH2Definition")
}).Count -ne 0) {
    throw "WorldInteractionE5CandidateBoundaryMissing"
}
if ($check -notmatch "WorldInteractionEhStatusValid:Items=37") {
    throw "WorldInteractionEhStatusCheckDidNotComplete"
}

Write-Output "WorldInteractionEhStatusTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
