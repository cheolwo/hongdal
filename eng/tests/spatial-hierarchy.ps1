$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/world-seedbeds/manage-spatial-hierarchy.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/simulation-world-spatial-hierarchy.md"

$first = & $script -Mode Write
$firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$firstWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$secondWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & $script -Mode Check

if ($firstHash -ne $secondHash) { throw "SpatialHierarchyGenerationIsNotDeterministic" }
if ($firstWriteTicks -ne $secondWriteTicks) { throw "SpatialHierarchyUnchangedOutputWasRewritten" }
if ($check -notmatch "SpatialHierarchyValid:H1=8;H2=0;H3=5;H4=1") {
    throw "SpatialHierarchyValidationDidNotComplete"
}

$document = Get-Content -LiteralPath $output -Raw -Encoding UTF8
if ($document -notmatch "E.*증거 성숙도.*G.*관리 체계.*H.*공간 포함 깊이") {
    throw "SpatialHierarchyAxisDistinctionMissing"
}
if ($document -notmatch "H4 지역 모판 \(AreaSet\)[\s\S]*H3 경관 모판 \(LandscapeGraph\)[\s\S]*H2 블록 모판 \(LandscapeBlock\)[\s\S]*H1 작업공간 모판 \(WI 공간 모판\)") {
    throw "SpatialHierarchyContainmentDiagramMissing"
}
if ($document -notmatch "H4 AreaSet과 H3 Graph가 존재해도[\s\S]*E4·E5가 아니다") {
    throw "SpatialHierarchyDoesNotPreventFalseE5Promotion"
}

Write-Output "SpatialHierarchyTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
