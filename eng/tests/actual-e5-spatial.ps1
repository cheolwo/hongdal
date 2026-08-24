$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$generatorPath = Join-Path $root "eng/world-seedbeds/manage-actual-e5-spatial.ps1"
$generatorBytes = [IO.File]::ReadAllBytes($generatorPath)
if ($generatorBytes.Length -lt 3 -or $generatorBytes[0] -ne 0xEF -or $generatorBytes[1] -ne 0xBB -or $generatorBytes[2] -ne 0xBF) {
    throw "ActualE5GeneratorUtf8BomMissing"
}
& $generatorPath -Mode Write
& $generatorPath -Mode Check
$jsonPath = Join-Path $root "eng/world-seedbeds/generated/actual-e5-spatial.v1.json"
$markdownPath = Join-Path $root "docs/AI/generated/actual-e5-spatial.md"
$value = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($value.counts.areaSets -ne 4) { throw "ActualE5AreaSetCountInvalid" }
if ($value.counts.internalGraphs -ne 16 -or $value.counts.networkRouteGraphs -ne 3) { throw "ActualE5GraphCountInvalid" }
if ($value.counts.deferredTheoryGraphs -ne 1 -or @($value.deferredTheoryH3Refs).Count -ne 1) { throw "ActualE5DeferredTheoryGraphCountInvalid" }
if ($value.counts.networkRelations -ne 8) { throw "ActualE5RelationCountInvalid" }
if ($value.counts.directBindings -ne 37 -or $value.counts.contextualBindings -ne 6 -or $value.counts.nonSpatialWi -ne 6 -or $value.counts.pendingE5Wi -ne 0) { throw "ActualE5WiPartitionInvalid" }
$unresolvedCount = @($value.areaSets | ForEach-Object { @($_.graphs) } | ForEach-Object { @($_.unresolved) } | Where-Object { $null -ne $_ }).Count
$unresolvedCount += @($value.routeGraphs | ForEach-Object { @($_.unresolved) } | Where-Object { $null -ne $_ }).Count
if ($unresolvedCount -ne 0) { throw "ActualE5GraphUnresolved" }
if (@($value.areaSets.graphs.statusCode | Where-Object { $_ -ne "Available" }).Count -ne 0) { throw "ActualE5InternalGraphUnavailable" }
if (@($value.areaSets | Where-Object { [string]::IsNullOrWhiteSpace($_.compositionPatternStableId) -or $_.compositionPatternHashSha256.Length -ne 64 -or $_.compositionDocumentHashSha256.Length -ne 64 }).Count -ne 0) { throw "ActualE5CompositionLineageMissing" }
if ((@($value.areaSets.compositionPatternCode | Sort-Object) -join ",") -ne "CITYHUB-ASET-COMP-01,FARM-ASET-COMP-01,NATURE-ASET-COMP-01,TOWN-ASET-COMP-01") { throw "ActualE5CompositionSelectionInvalid" }
if ($value.compositionPlanRevision -ne "simulation-world-area-set-composition-plans.r1" -or $value.compositionLedgerRevision -ne "simulation-world-area-set-composition-patterns.r1") { throw "ActualE5CompositionRevisionInvalid" }
if (@($value.routeGraphs.statusCode | Where-Object { $_ -ne "Available" }).Count -ne 0) { throw "ActualE5RouteGraphUnavailable" }
$graphIds = @($value.areaSets.graphs.landscapeGraphStableId) + @($value.routeGraphs.landscapeGraphStableId)
if ($graphIds.Count -ne @($graphIds | Sort-Object -Unique).Count) { throw "ActualE5GraphOwnershipDuplicate" }
if ([string] $value.network.title -notmatch '[\uac00-\ud7a3]') { throw "ActualE5NetworkTitleEncodingInvalid" }
$markdown = Get-Content -LiteralPath $markdownPath -Raw -Encoding UTF8
if ($markdown -notmatch '[\uac00-\ud7a3]' -or $markdown.Contains([char] 0xFFFD)) { throw "ActualE5MarkdownEncodingInvalid" }
Write-Output "ActualE5SpatialTestsPassed:AreaSets=4;Graphs=19;Deferred=1;Relations=8;WI=37/6/6/0;CompositionPatterns=4"
