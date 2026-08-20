$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../.." )).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-theory-spatial-factory.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/generated/theory-spatial-factory.v1.json"

$write = & pwsh -NoProfile -File $manager -Mode Write
if ($write -notmatch "TheorySpatialFactoryGenerated:H2=34;H3=18;E5=4;HumanReview=Deferred") { throw "TheoryFactoryWriteFailed" }
$beforeHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
if ($check -notmatch "TheorySpatialFactoryValid:H2=34;H3=18;E5=4;HumanReview=Deferred") { throw "TheoryFactoryCheckFailed" }
$afterHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
if ($beforeHash -ne $afterHash -or $beforeTicks -ne $afterTicks) { throw "TheoryFactoryNonDeterministic" }

$result = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string] $result.policyRevision -ne "simulation-world-theory-spatial-factory-policy.r3") { throw "TheoryFactoryPolicyRevisionInvalid" }
if ([int] $result.counts.h2TheoryQualified -ne 34 -or [int] $result.counts.h3TheoryQualified -ne 18 -or [int] $result.counts.e5TheoryQualifiedAreaSets -ne 4) { throw "TheoryFactoryCountInvalid" }
if (@($result.h2Plans | Where-Object theoryStateCode -ne "TheoryQualified").Count -ne 0) { throw "TheoryFactoryH2StateInvalid" }
if (@($result.h3Plans | Where-Object theoryStateCode -ne "TheoryQualified").Count -ne 0) { throw "TheoryFactoryH3StateInvalid" }
if (@($result.e5AreaSetInstances | Where-Object { $_.evidenceStageCode -ne "E5" -or $_.e5QualificationCode -ne "E5TheoryQualified" -or $_.humanReviewed -or $_.publicDataBound -or $_.runtimeValidated }).Count -ne 0) { throw "TheoryFactoryE5BoundaryInvalid" }
if ((@($result.e5AreaSetInstances.gamePlanCode) -join ",") -ne "NatureHomeThreatRecovery,FarmProductionSurvival,CityHubLogisticsResilience,TownLivingMarketSafety") { throw "TheoryFactoryPriorityInvalid" }
if (@($result.h2Plans | Where-Object { @($_.connectors).Count -lt 2 -or @($_.edges).Count -lt (@($_.nodes).Count - 1) }).Count -ne 0) { throw "TheoryFactoryH2ClosureInvalid" }
if (@($result.h3Plans | Where-Object { @($_.connectors).Count -lt 2 -or (@($_.nodes).Count -gt 1 -and @($_.edges).Count -lt (@($_.nodes).Count - 1)) }).Count -ne 0) { throw "TheoryFactoryH3ClosureInvalid" }
if ([string] $result.patternNamingRevision -ne "simulation-world-h-pattern-naming.r6") { throw "TheoryFactoryPatternNamingRevisionInvalid" }
if (@($result.h2Plans | Where-Object { [string]::IsNullOrWhiteSpace($_.patternCode) -or [string]::IsNullOrWhiteSpace($_.displayNameKo) -or [string]::IsNullOrWhiteSpace($_.leadPackCode) }).Count -ne 0) { throw "TheoryFactoryH2PatternMetadataMissing" }
if (@($result.h3Plans | Where-Object { [string]::IsNullOrWhiteSpace($_.patternCode) -or [string]::IsNullOrWhiteSpace($_.displayNameKo) -or [string]::IsNullOrWhiteSpace($_.leadPackCode) }).Count -ne 0) { throw "TheoryFactoryH3PatternMetadataMissing" }
if (@($result.h3Plans.nodes | Where-Object { [string]::IsNullOrWhiteSpace($_.h2PatternCode) }).Count -ne 0) { throw "TheoryFactoryH3ChildPatternMissing" }
if (@($result.h2Plans | Where-Object { $_.resourceKindCode -ne "BlockPattern" -or [string]::IsNullOrWhiteSpace($_.spatialDisplayNameKo) -or [string]::IsNullOrWhiteSpace($_.gameplayProfileNameKo) -or -not $_.placementContract.placeableAsUnit -or $_.placementContract.placementUnitCode -ne "H2Block" -or $_.placementContract.localCoordinateSystemCode -ne "LocalMeters" -or [double] $_.placementContract.referenceBoundsMeters.width -le 0 -or [double] $_.placementContract.referenceBoundsMeters.depth -le 0 -or @($_.placementContract.connectionRoleCodes).Count -lt 2 }).Count -ne 0) { throw "TheoryFactoryH2PlacementContractInvalid" }
if (@($result.h3Plans | Where-Object { $_.resourceKindCode -ne "LandscapeAssemblyPattern" -or [string]::IsNullOrWhiteSpace($_.spatialDisplayNameKo) -or [string]::IsNullOrWhiteSpace($_.gameplayProfileNameKo) -or -not $_.placementContract.placeableAsUnit -or $_.placementContract.placementUnitCode -ne "H3District" -or $_.placementContract.localCoordinateSystemCode -ne "LocalMeters" -or [double] $_.placementContract.referenceBoundsMeters.width -le 0 -or [double] $_.placementContract.referenceBoundsMeters.depth -le 0 -or @($_.placementContract.connectionRoleCodes).Count -lt 2 }).Count -ne 0) { throw "TheoryFactoryH3PlacementContractInvalid" }
if (@($result.h2Plans + $result.h3Plans | Where-Object { [string] $_.displayNameKo -ne [string] $_.spatialDisplayNameKo -or [string] $_.spatialDisplayNameKo -eq [string] $_.gameplayProfileNameKo }).Count -ne 0) { throw "TheoryFactoryPrimarySpatialNameProjectionInvalid" }
if (@($result.h3Plans.nodes | Where-Object { [string]::IsNullOrWhiteSpace($_.h2DisplayNameKo) }).Count -ne 0) { throw "TheoryFactoryH3ChildSpatialNameMissing" }
if (@($result.e5AreaSetInstances.graphInstances | Where-Object { [string]::IsNullOrWhiteSpace($_.h3PatternCode) }).Count -ne 0) { throw "TheoryFactoryAreaSetPatternMissing" }
if (@($result.h2Plans.patternCode + $result.h3Plans.patternCode | Sort-Object -Unique).Count -ne 52) { throw "TheoryFactoryPatternCodeUniquenessInvalid" }
if ([int] $result.counts.reservedExpansionPatterns -ne 0 -or @($result.priorityExpansionQueue).Count -ne 0) { throw "TheoryFactoryExpansionQueueInvalid" }
if (@($result.productionPhases).Count -ne 5) { throw "TheoryFactoryProductionPhaseInvalid" }

Write-Output "TheorySpatialFactoryTestsPassed:H2=34;H3=18;Patterns=52;E5=4;Deterministic=True"
