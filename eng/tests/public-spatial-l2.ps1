$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$buildScript = Join-Path $repositoryRoot "eng/public-spatial/build-daegwallyeong-l2-artifacts.ps1"
$manifestPath = Join-Path $repositoryRoot "eng/public-spatial/manifests/kr5186-l2-700-1145.json"

& powershell -NoProfile -ExecutionPolicy Bypass -File $buildScript
$firstManifestHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $manifestPath).Hash
$first = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
& powershell -NoProfile -ExecutionPolicy Bypass -File $buildScript
$secondManifestHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $manifestPath).Hash
$second = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

if ($firstManifestHash -ne $secondManifestHash) { throw "SpatialArtifactManifestIsNotDeterministic" }
if ($first.tileKey -ne "kr5186:l2:700:1145") { throw "SpatialArtifactTileKeyInvalid" }
if ($first.artifacts.elevation.formatCode -ne "height-f32-v1") { throw "ElevationFormatInvalid" }
if ($first.artifacts.elevation.sha256 -ne $second.artifacts.elevation.sha256) { throw "ElevationHashChanged" }
if ($first.artifacts.landCover.sha256 -ne $second.artifacts.landCover.sha256) { throw "LandCoverHashChanged" }
if ($first.artifacts.placementMask.sha256 -ne $second.artifacts.placementMask.sha256) { throw "PlacementMaskHashChanged" }
if ($first.statistics.validElevationSampleCount -le 0) { throw "ElevationSamplesMissing" }
if ($first.statistics.noDataCellCount -ne 0) { throw "CenterTileContainsUnexpectedNoData" }
if (-not $first.physicalElevationIsAuthoritativeForPlacement) { throw "PhysicalElevationAuthorityMissing" }
if (-not $first.visualHeightExaggerationStoredSeparately) { throw "VisualElevationBoundaryMissing" }

Write-Output "PublicSpatialL2TestsPassed"
Write-Output "ManifestHash:$firstManifestHash"
Write-Output "Fingerprint:$($first.fingerprintSha256)"
