$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $root "eng/world-seedbeds/manage-area-set-composition-patterns.ps1"
$output = Join-Path $root "eng/world-seedbeds/generated/area-set-composition-plans.v1.json"

$write = & pwsh -NoProfile -File $manager -Mode Write
if ($write -notmatch "AreaSetCompositionPatternsGenerated:Baseline=4;Variants=4;H3=32;Closed=True") { throw "AreaSetCompositionWriteFailed:$write" }
$beforeHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
if ($check -notmatch "AreaSetCompositionPatternsValid:Baseline=4;Variants=4;H3=32;Closed=True") { throw "AreaSetCompositionCheckFailed:$check" }
if ($beforeHash -ne (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash -or $beforeTicks -ne (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks) { throw "AreaSetCompositionNonDeterministic" }

$result = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
if ($result.schemaVersion -ne "simulation-world-area-set-composition-plans.v1") { throw "AreaSetCompositionSchemaInvalid" }
if ($result.counts.areaRoles -ne 4 -or $result.counts.totalPatterns -ne 8 -or $result.counts.resolvedConnections -ne 32) { throw "AreaSetCompositionCountsInvalid" }
if (@($result.baselineSelections).Count -ne 4 -or @($result.baselineSelections.areaRoleCode | Sort-Object -Unique).Count -ne 4) { throw "AreaSetCompositionBaselineSelectionInvalid" }
if (@($result.resolvedPatterns | Where-Object { $_.closureStateCode -ne "Closed" -or $_.structureQualificationCode -ne "AreaSetCompositionStructureQualified" -or $_.runtimeState -or -not $_.presentationOnly }).Count -ne 0) { throw "AreaSetCompositionQualificationInvalid" }
if (@($result.resolvedPatterns | Where-Object { @($_.resolvedH3Placements).Count -lt 3 -or @($_.resolvedConnections).Count -lt @($_.resolvedH3Placements).Count }).Count -ne 0) { throw "AreaSetCompositionLoopInvalid" }
if (@($result.resolvedPatterns.resolvedH3Placements | Where-Object { [string]::IsNullOrWhiteSpace($_.roleSlotCode) -or [string]::IsNullOrWhiteSpace($_.h3PatternCode) -or $_.h3TheoryHashSha256.Length -ne 64 }).Count -ne 0) { throw "AreaSetCompositionPlacementLineageInvalid" }
if (-not $result.authorityBoundary.patternIsNotRuntimeState -or -not $result.authorityBoundary.unityCannotSelectAuthoritatively -or -not $result.authorityBoundary.e6RemainsSeparate) { throw "AreaSetCompositionAuthorityInvalid" }

Write-Output "AreaSetCompositionPatternTestsPassed:AreaSets=4;Baseline=4;Variants=4;H3=32;Connections=32;Deterministic=True"
