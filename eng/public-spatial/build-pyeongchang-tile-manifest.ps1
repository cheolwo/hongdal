[CmdletBinding()]
param(
    [string]$WorldCoverPath = "artifacts/local/public-spatial/pyeongchang/pyeongchang-esa-worldcover-2021-v200-epsg5186.tif",
    [string]$OutputPath = "artifacts/local/public-spatial/pyeongchang/generated/pyeongchang-spatial-tile-manifest.json",
    [int[]]$Levels = @(0, 1, 2),
    [int]$BaseSeed = 51760
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;

public static class SpatialRasterAggregator
{
    public static Dictionary<string, long[]> Aggregate(
        byte[] buffer, int stride, int width, int height,
        double originEasting, double originNorthing, double pixelMeters,
        int[] levels, int[] sizes, long[] totals)
    {
        var result = new Dictionary<string, long[]>(StringComparer.Ordinal);
        for (var row = 0; row < height; row++)
        {
            var northing = originNorthing - ((row + .5d) * pixelMeters);
            var offset = row * stride;
            for (var column = 0; column < width; column++)
            {
                var code = buffer[offset + column];
                if (code == 0) continue;
                totals[code]++;
                var easting = originEasting + ((column + .5d) * pixelMeters);
                for (var index = 0; index < levels.Length; index++)
                {
                    var x = (int)Math.Floor(easting / sizes[index]);
                    var y = (int)Math.Floor(northing / sizes[index]);
                    var key = levels[index] + ":" + x + ":" + y;
                    long[] counts;
                    if (!result.TryGetValue(key, out counts))
                    {
                        counts = new long[256];
                        result.Add(key, counts);
                    }
                    counts[code]++;
                }
            }
        }
        return result;
    }
}
'@

function Get-Sha256([string]$Value) {
    $algorithm = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes($Value)
        return ([BitConverter]::ToString($algorithm.ComputeHash($bytes))).Replace("-", "")
    }
    finally {
        $algorithm.Dispose()
    }
}

function Get-WorldSeed([double]$Easting, [double]$Northing, [string]$SemanticKey) {
    $payload = [string]::Join("|", @(
        $BaseSeed.ToString([Globalization.CultureInfo]::InvariantCulture),
        $Easting.ToString("0.###", [Globalization.CultureInfo]::InvariantCulture),
        $Northing.ToString("0.###", [Globalization.CultureInfo]::InvariantCulture),
        $SemanticKey))
    return [Convert]::ToInt32((Get-Sha256 $payload).Substring(0, 7), 16)
}

function Get-GeoDoubleArray([System.Drawing.Image]$Image, [int]$PropertyId) {
    $property = $Image.PropertyItems | Where-Object Id -eq $PropertyId | Select-Object -First 1
    if ($null -eq $property) { throw "GeoTiffPropertyMissing:$PropertyId" }
    $values = [Collections.Generic.List[double]]::new()
    for ($index = 0; $index -lt $property.Value.Length; $index += 8) {
        $values.Add([BitConverter]::ToDouble($property.Value, $index))
    }
    return $values.ToArray()
}

$resolvedInput = (Resolve-Path -LiteralPath $WorldCoverPath).Path
$image = [System.Drawing.Bitmap]::FromFile($resolvedInput)
try {
    if ($image.PixelFormat -ne [Drawing.Imaging.PixelFormat]::Format8bppIndexed) {
        throw "WorldCoverPixelFormatUnsupported:$($image.PixelFormat)"
    }
    $pixelScale = Get-GeoDoubleArray $image 33550
    $tiePoint = Get-GeoDoubleArray $image 33922
    $pixelMeters = $pixelScale[0]
    $originEasting = $tiePoint[3]
    $originNorthing = $tiePoint[4]
    $rasterWidth = $image.Width
    $rasterHeight = $image.Height
    $bounds = [Drawing.Rectangle]::new(0, 0, $image.Width, $image.Height)
    $data = $image.LockBits($bounds, [Drawing.Imaging.ImageLockMode]::ReadOnly, $image.PixelFormat)
    try {
        $stride = [Math]::Abs($data.Stride)
        $buffer = [byte[]]::new($stride * $image.Height)
        [Runtime.InteropServices.Marshal]::Copy($data.Scan0, $buffer, 0, $buffer.Length)
    }
    finally {
        $image.UnlockBits($data)
    }
}
finally {
    $image.Dispose()
}

$levelSizes = @{ 0 = 8000; 1 = 2000; 2 = 500 }
$levelHalos = @{ 0 = 300; 1 = 150; 2 = 60 }
$classTotals = [long[]]::new(256)
$levelArray = [int[]]$Levels
$sizeArray = [int[]]@($Levels | ForEach-Object { $levelSizes[$_] })
$aggregated = [SpatialRasterAggregator]::Aggregate(
    $buffer, $stride, $rasterWidth, $rasterHeight,
    $originEasting, $originNorthing, $pixelMeters,
    $levelArray, $sizeArray, $classTotals)

$worldCoverGroups = [ordered]@{
    agriculture = @(40)
    forest = @(10)
    water = @(80, 90)
    'bare-ground' = @(60)
    built = @(50)
    grass = @(30)
}
$sourceHash = (Get-FileHash -LiteralPath $resolvedInput -Algorithm SHA256).Hash
$recipeHash = Get-Sha256 "world-build:kr:51760:farm-hub-town.v1|spatial-tile-area-set.v1|$BaseSeed"
$profileHash = Get-Sha256 "land-cover-composition:kr:51760:2024.v1|statistical-landscape-allocation.v1"
$tiles = [Collections.Generic.List[object]]::new()
foreach ($level in $Levels | Sort-Object) {
    foreach ($tileEntry in $aggregated.GetEnumerator() | Where-Object { $_.Key.StartsWith("${level}:") } | Sort-Object Key) {
        $parts = $tileEntry.Key.Split(':')
        $x = [int]$parts[1]
        $y = [int]$parts[2]
        $tileId = "kr5186:l${level}:${x}:${y}"
        $size = $levelSizes[$level]
        $counts = [ordered]@{}
        for ($code = 1; $code -lt $tileEntry.Value.Length; $code++) {
            if ($tileEntry.Value[$code] -gt 0) { $counts[[string]$code] = $tileEntry.Value[$code] }
        }
        $groups = [ordered]@{}
        foreach ($group in $worldCoverGroups.GetEnumerator()) {
            $pixelCount = 0L
            foreach ($code in $group.Value) {
                $pixelCount += $tileEntry.Value[$code]
            }
            $groups[$group.Key] = [Math]::Round($pixelCount * $pixelMeters * $pixelMeters / 1000000.0, 6)
        }
        $fingerprint = Get-Sha256 "$tileId|land-cover|$sourceHash|spatial-tile-area-set.v1|$recipeHash|$profileHash"
        $tiles.Add([ordered]@{
            tileKey = $tileId
            level = $level
            sizeMeters = $size
            haloMeters = $levelHalos[$level]
            coreBounds = [ordered]@{
                minEasting = $x * $size
                minNorthing = $y * $size
                maxEasting = ($x + 1) * $size
                maxNorthing = ($y + 1) * $size
            }
            generationBounds = [ordered]@{
                minEasting = $x * $size - $levelHalos[$level]
                minNorthing = $y * $size - $levelHalos[$level]
                maxEasting = ($x + 1) * $size + $levelHalos[$level]
                maxNorthing = ($y + 1) * $size + $levelHalos[$level]
            }
            worldCoordinateSeed = Get-WorldSeed (($x + 0.5) * $size) (($y + 0.5) * $size) 'landscape-composition'
            classPixelCounts = $counts
            candidateAreaSquareKm = $groups
            fingerprint = $fingerprint
        })
    }
}

$totalValidPixels = ($classTotals | Measure-Object -Sum).Sum
$totalRasterArea = $totalValidPixels * $pixelMeters * $pixelMeters / 1000000.0
$groupCandidateAreas = [ordered]@{}
foreach ($group in $worldCoverGroups.GetEnumerator()) {
    $pixels = 0L
    foreach ($code in $group.Value) { $pixels += $classTotals[$code] }
    $groupCandidateAreas[$group.Key] = $pixels * $pixelMeters * $pixelMeters / 1000000.0
}
$targets = @(
    [pscustomobject][ordered]@{ targetCode = 'rice-paddy'; group = 'agriculture'; targetAreaSquareKm = 5.0185 },
    [pscustomobject][ordered]@{ targetCode = 'dry-field'; group = 'agriculture'; targetAreaSquareKm = 102.5913 },
    [pscustomobject][ordered]@{ targetCode = 'greenhouse'; group = 'agriculture'; targetAreaSquareKm = 3.8150 },
    [pscustomobject][ordered]@{ targetCode = 'orchard'; group = 'agriculture'; targetAreaSquareKm = 1.2454 },
    [pscustomobject][ordered]@{ targetCode = 'broadleaf-forest'; group = 'forest'; targetAreaSquareKm = 666.2624 },
    [pscustomobject][ordered]@{ targetCode = 'conifer-forest'; group = 'forest'; targetAreaSquareKm = 369.3999 },
    [pscustomobject][ordered]@{ targetCode = 'mixed-forest'; group = 'forest'; targetAreaSquareKm = 116.1934 },
    [pscustomobject][ordered]@{ targetCode = 'inland-water'; group = 'water'; targetAreaSquareKm = 9.2042 },
    [pscustomobject][ordered]@{ targetCode = 'bare-ground'; group = 'bare-ground'; targetAreaSquareKm = 23.6943 }
)
$allocation = [Collections.Generic.List[object]]::new()
foreach ($group in $targets | Group-Object group) {
    $targetTotal = ($group.Group.targetAreaSquareKm | Measure-Object -Sum).Sum
    $groupName = [string]$group.Name
    $candidate = [double]$groupCandidateAreas[$groupName]
    $scale = if ($targetTotal -le 0) { 0.0 } else { [Math]::Min(1.0, $candidate / $targetTotal) }
    foreach ($target in $group.Group | Sort-Object targetCode) {
        $allocated = [double]$target.targetAreaSquareKm * $scale
        $allocation.Add([ordered]@{
            targetCode = $target.targetCode
            candidateGroup = $groupName
            targetAreaSquareKm = $target.targetAreaSquareKm
            candidateGroupAreaSquareKm = [Math]::Round($candidate, 6)
            allocatedAreaSquareKm = [Math]::Round($allocated, 6)
            unresolvedTargetAreaSquareKm = [Math]::Round([Math]::Max(0.0, $target.targetAreaSquareKm - $allocated), 6)
            meaningConfidence = 'StatisticallyAllocated'
        })
    }
}
$result = [ordered]@{
    schemaVersion = "pyeongchang-spatial-tile-manifest.v1"
    generatedAt = [DateTimeOffset]::UtcNow.ToString("O")
    crs = "EPSG:5186"
    source = [ordered]@{
        path = $resolvedInput
        sourceName = "ESA WorldCover 2021 v200"
        sourceVintage = "2021"
        horizontalResolutionMeters = $pixelMeters
        noData = 0
        sha256 = $sourceHash
        width = $rasterWidth
        height = $rasterHeight
        originEasting = $originEasting
        originNorthing = $originNorthing
    }
    tileScheme = [ordered]@{
        levels = @(
            [ordered]@{ level = 0; sizeMeters = 8000; haloMeters = 300 },
            [ordered]@{ level = 1; sizeMeters = 2000; haloMeters = 150 },
            [ordered]@{ level = 2; sizeMeters = 500; haloMeters = 60 }
        )
        seedStrategy = "world-coordinate-hash"
    }
    totalValidPixels = $totalValidPixels
    totalValidAreaSquareKm = [Math]::Round($totalRasterArea, 6)
    classPixelCounts = [ordered]@{}
    candidateGroupAreaSquareKm = $groupCandidateAreas
    landAllocationResults = $allocation
    tiles = $tiles
}
for ($code = 1; $code -lt $classTotals.Length; $code++) {
    if ($classTotals[$code] -gt 0) { $result.classPixelCounts[[string]$code] = $classTotals[$code] }
}

$parent = Split-Path -Parent $OutputPath
if ($parent) { [IO.Directory]::CreateDirectory((Join-Path (Get-Location) $parent)) | Out-Null }
$json = $result | ConvertTo-Json -Depth 12
[IO.File]::WriteAllText((Join-Path (Get-Location) $OutputPath), $json, [Text.UTF8Encoding]::new($false))
Write-Output "SpatialTileManifestCreated:$OutputPath"
Write-Output "TileCounts:L0=$(($tiles | Where-Object level -eq 0).Count),L1=$(($tiles | Where-Object level -eq 1).Count),L2=$(($tiles | Where-Object level -eq 2).Count)"
Write-Output "ValidAreaSquareKm=$([Math]::Round($totalValidPixels * $pixelMeters * $pixelMeters / 1000000.0, 4))"
