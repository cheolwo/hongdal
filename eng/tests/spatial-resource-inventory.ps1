$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/world-seedbeds/manage-spatial-resource-inventory.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/simulation-world-spatial-resource-inventory.md"

$first = & $script -Mode Write
$firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$firstWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$secondWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & $script -Mode Check

if ($firstHash -ne $secondHash) { throw "SpatialResourceInventoryGenerationIsNotDeterministic" }
if ($firstWriteTicks -ne $secondWriteTicks) { throw "SpatialResourceInventoryUnchangedOutputWasRewritten" }
if ($check -notmatch "SpatialResourceInventoryValid:H1=68/5;H2=19/0;H3=10/5;H4=5/1") {
    throw "SpatialResourceInventoryValidationDidNotComplete"
}

$document = Get-Content -LiteralPath $output -Raw -Encoding UTF8
if ($document -notmatch "모판은 H1 하나의 이름이 아니라 H1~H4") {
    throw "SpatialResourceInventoryFamilyMeaningMissing"
}
if ($document -notmatch "H1 작업공간 모판 재고[\s\S]*H2 블록 모판 재고[\s\S]*H3 경관 모판 재고[\s\S]*H4 지역 모판 재고") {
    throw "SpatialResourceInventoryBottomUpFlowMissing"
}
if ($document -notmatch "H3·H4 정의가 있어도 실제 H2가 없으면 E5가 아니다") {
    throw "SpatialResourceInventoryFalsePromotionBoundaryMissing"
}

Write-Output "SpatialResourceInventoryTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
