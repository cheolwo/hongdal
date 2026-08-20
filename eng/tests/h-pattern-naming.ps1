$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../.." )).Path
$namingPath = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/h-pattern-names.v1.json"
$catalogPath = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json"

$naming = Get-Content -LiteralPath $namingPath -Raw -Encoding UTF8 | ConvertFrom-Json
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string] $naming.schemaVersion -ne "simulation-world-h-pattern-naming.v1") { throw "PatternNamingSchemaInvalid" }

$h2Patterns = @($naming.h2Patterns)
$h3Patterns = @($naming.h3Patterns)
$h2StableIds = @($catalog.h2DefinitionRefs.stableId)
$h3StableIds = @($catalog.h3DefinitionRefs.stableId)
if ($h2Patterns.Count -ne 34 -or $h3Patterns.Count -ne 18) { throw "PatternNamingCountInvalid" }
if ((@($h2Patterns.stableId | Sort-Object) -join "|") -ne (@($h2StableIds | Sort-Object) -join "|")) { throw "H2PatternStableIdCoverageInvalid" }
if ((@($h3Patterns.stableId | Sort-Object) -join "|") -ne (@($h3StableIds | Sort-Object) -join "|")) { throw "H3PatternStableIdCoverageInvalid" }

$allPatterns = @($h2Patterns) + @($h3Patterns)
if (@($allPatterns.patternCode | Sort-Object -Unique).Count -ne 52) { throw "PatternCodeDuplicate" }
foreach ($pattern in $allPatterns) {
    if ([string] $pattern.patternCode -notmatch '^(NATURE|FARM|CITY|TOWN|MIX)-H[23]-[A-Z0-9-]+-\d{2}$') { throw "PatternCodeFormatInvalid:$($pattern.patternCode)" }
    if ([string]::IsNullOrWhiteSpace([string] $pattern.displayNameKo)) { throw "PatternDisplayNameMissing:$($pattern.stableId)" }
    if ([int] $pattern.patternSequence -lt 1) { throw "PatternSequenceInvalid:$($pattern.stableId)" }
    $supports = @($pattern.supportPackCodes)
    if ([string] $pattern.compositionModeCode -eq "SinglePack" -and $supports.Count -ne 0) { throw "SinglePackSupportInvalid:$($pattern.stableId)" }
    if ([string] $pattern.compositionModeCode -eq "CrossPackTransition" -and ([string] $pattern.leadPackCode -ne "Mixed" -or $supports.Count -lt 2)) { throw "CrossPackPatternInvalid:$($pattern.stableId)" }
}

$townVillage = @($allPatterns | Where-Object patternCode -like "TOWN-H*-VILLAGE-*")
if ($townVillage.Count -ne 7) { throw "TownVillagePatternCountInvalid" }
$reserved = @($naming.priorityExpansionQueue | ForEach-Object { [string] $_.reservedPatternCode })
if ($reserved.Count -ne 0) { throw "PatternExpansionQueueInvalid" }
if (@($reserved | Where-Object { $_ -in @($allPatterns.patternCode) }).Count -ne 0) { throw "PatternExpansionCodeCollision" }
if (@($naming.inventoryTargets).Count -ne 5) { throw "PatternInventoryTargetInvalid" }
if ([string] $naming.revision -ne "simulation-world-h-pattern-naming.r6") { throw "PatternNamingRevisionInvalid" }
if ([string] $naming.displayPolicy.primarySpatialNameSourceCode -ne "PatternSpatialDisplayName" -or -not [bool] $naming.displayPolicy.spatialNameMustBeShownBeforeGameplayProfile) { throw "PatternSpatialDisplayPolicyInvalid" }
if (@($naming.h2Patterns + $naming.h3Patterns | Where-Object { [string]::IsNullOrWhiteSpace([string] $_.spatialDisplayNameKo) }).Count -ne 0) { throw "PatternSpatialDisplayNameMissing" }
if (@($naming.h2Patterns | Where-Object { -not ([string] $_.spatialDisplayNameKo).EndsWith("블록") }).Count -ne 0) { throw "H2SpatialDisplayNameMustDescribeBlock" }
if (@($naming.h3Patterns | Where-Object { -not (([string] $_.spatialDisplayNameKo).EndsWith("구역") -or ([string] $_.spatialDisplayNameKo).EndsWith("회랑")) }).Count -ne 0) { throw "H3SpatialDisplayNameMustDescribeDistrictOrCorridor" }
if (@($naming.h2Patterns + $naming.h3Patterns | Where-Object { [string] $_.spatialDisplayNameKo -eq [string] $_.displayNameKo }).Count -ne 0) { throw "SpatialAndGameplayNamesMustRemainDistinct" }
if (@($naming.productionPhases).Count -ne 5) { throw "PatternProductionPhaseCountInvalid" }
if (@($naming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P1" -and ($_.hierarchyLevelCode -ne "H2" -or $_.plannedCompositionModeCode -ne "SinglePack") }).Count -ne 0) { throw "P1MustBeSinglePackH2" }
if (@($naming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P2" -and ($_.hierarchyLevelCode -ne "H3" -or $_.plannedCompositionModeCode -ne "SinglePack") }).Count -ne 0) { throw "P2MustBeSinglePackH3" }
if (@($naming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P4" -and $_.plannedCompositionModeCode -ne "CrossPackTransition" }).Count -ne 0) { throw "P4MustBeCrossPack" }
if (@($naming.priorityExpansionQueue | Where-Object { $_.priorityCode -eq "P5" -and $_.plannedCompositionModeCode -ne "CrossPackTransition" }).Count -ne 0) { throw "P5MustBeCrossPack" }
Write-Output "HPatternNamingTestsPassed:H2=34;H3=18;Total=52;Reserved=0;TownVillage=7;StableIdsPreserved=True"
