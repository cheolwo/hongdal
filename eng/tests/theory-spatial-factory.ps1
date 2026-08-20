$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../.." )).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-theory-spatial-factory.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/generated/theory-spatial-factory.v1.json"

$write = & pwsh -NoProfile -File $manager -Mode Write
if ($write -notmatch "TheorySpatialFactoryGenerated:H2=37;H3=20;E5=4;World=TheoryWorldQualified") { throw "TheoryFactoryWriteFailed" }
$beforeHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
if ($check -notmatch "TheorySpatialFactoryValid:H2=37;H3=20;E5=4;World=TheoryWorldQualified") { throw "TheoryFactoryCheckFailed" }
$afterHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
if ($beforeHash -ne $afterHash -or $beforeTicks -ne $afterTicks) { throw "TheoryFactoryNonDeterministic" }

$result = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string] $result.policyRevision -ne "simulation-world-theory-spatial-factory-policy.r4") { throw "TheoryFactoryPolicyRevisionInvalid" }
if ([int] $result.counts.h2TheoryQualified -ne 37 -or [int] $result.counts.h3TheoryQualified -ne 20 -or [int] $result.counts.e5TheoryQualifiedAreaSets -ne 4) { throw "TheoryFactoryCountInvalid" }
if (@($result.h2Plans | Where-Object theoryStateCode -ne "TheoryQualified").Count -ne 0) { throw "TheoryFactoryH2StateInvalid" }
if (@($result.h3Plans | Where-Object theoryStateCode -ne "TheoryQualified").Count -ne 0) { throw "TheoryFactoryH3StateInvalid" }
if (@($result.e5AreaSetInstances | Where-Object { $_.evidenceStageCode -ne "E5" -or $_.e5QualificationCode -ne "E5TheoryQualified" -or $_.humanReviewed -or $_.publicDataBound -or $_.runtimeValidated }).Count -ne 0) { throw "TheoryFactoryE5BoundaryInvalid" }
if ((@($result.e5AreaSetInstances.gamePlanCode | Sort-Object) -join ",") -ne "CityHubLogisticsResilience,FarmProductionSurvival,NatureHomeThreatRecovery,TownLivingMarketSafety") { throw "TheoryFactoryAreaSetCoverageInvalid" }
if (@($result.h2Plans | Where-Object { @($_.connectors).Count -lt 2 -or @($_.edges).Count -lt (@($_.nodes).Count - 1) }).Count -ne 0) { throw "TheoryFactoryH2ClosureInvalid" }
if (@($result.h3Plans | Where-Object { @($_.nodes).Count -lt 2 -or @($_.connectors).Count -lt 2 -or @($_.edges).Count -lt (@($_.nodes).Count - 1) }).Count -ne 0) { throw "TheoryFactoryH3ClosureInvalid" }
if ([string] $result.patternNamingRevision -ne "simulation-world-h-pattern-naming.r8") { throw "TheoryFactoryPatternNamingRevisionInvalid" }
if ([string] $result.semanticRelationRevision -ne "simulation-world-semantic-spatial-relations.r1") { throw "TheoryFactorySemanticRevisionInvalid" }
if (@($result.h2Plans + $result.h3Plans | Where-Object { $_.structureQualificationCode -ne "StructureQualified" -or $_.closureStateCode -ne "Closed" -or @($_.unresolvedSemanticRelations).Count -ne 0 }).Count -ne 0) { throw "TheoryFactorySemanticClosureInvalid" }
if (@($result.e5AreaSetInstances | Where-Object { $_.structureQualificationCode -ne "E5StructureQualified" -or $_.closureStateCode -ne "Closed" -or @($_.unresolvedSemanticRelations).Count -ne 0 }).Count -ne 0) { throw "TheoryFactoryAreaSetSemanticClosureInvalid" }
if ($result.theoryWorld.qualificationCode -ne "TheoryWorldQualified" -or $result.theoryWorld.closureStateCode -ne "Closed" -or @($result.theoryWorld.unresolvedSemanticRelations).Count -ne 0) { throw "TheoryFactoryWorldSemanticClosureInvalid" }
if (@($result.theoryWorld.flowRequirements | Where-Object movementKindCode -eq "CargoLogistics").Count -ne 1) { throw "TheoryFactoryCargoFlowMissing" }
if (@($result.h2Plans | Where-Object { [string]::IsNullOrWhiteSpace($_.patternCode) -or [string]::IsNullOrWhiteSpace($_.displayNameKo) -or [string]::IsNullOrWhiteSpace($_.leadPackCode) }).Count -ne 0) { throw "TheoryFactoryH2PatternMetadataMissing" }
if (@($result.h3Plans | Where-Object { [string]::IsNullOrWhiteSpace($_.patternCode) -or [string]::IsNullOrWhiteSpace($_.displayNameKo) -or [string]::IsNullOrWhiteSpace($_.leadPackCode) }).Count -ne 0) { throw "TheoryFactoryH3PatternMetadataMissing" }
if (@($result.h3Plans.nodes | Where-Object { [string]::IsNullOrWhiteSpace($_.h2PatternCode) }).Count -ne 0) { throw "TheoryFactoryH3ChildPatternMissing" }
if (@($result.h2Plans | Where-Object { $_.resourceKindCode -ne "BlockPattern" -or [string]::IsNullOrWhiteSpace($_.spatialDisplayNameKo) -or [string]::IsNullOrWhiteSpace($_.gameplayProfileNameKo) -or -not $_.placementContract.placeableAsUnit -or $_.placementContract.placementUnitCode -ne "H2Block" -or $_.placementContract.localCoordinateSystemCode -ne "LocalMeters" -or [double] $_.placementContract.referenceBoundsMeters.width -le 0 -or [double] $_.placementContract.referenceBoundsMeters.depth -le 0 -or @($_.placementContract.connectionRoleCodes).Count -lt 2 }).Count -ne 0) { throw "TheoryFactoryH2PlacementContractInvalid" }
if (@($result.h3Plans | Where-Object { $_.resourceKindCode -ne "LandscapeAssemblyPattern" -or [string]::IsNullOrWhiteSpace($_.spatialDisplayNameKo) -or [string]::IsNullOrWhiteSpace($_.gameplayProfileNameKo) -or -not $_.placementContract.placeableAsUnit -or $_.placementContract.placementUnitCode -ne "H3District" -or $_.placementContract.localCoordinateSystemCode -ne "LocalMeters" -or [double] $_.placementContract.referenceBoundsMeters.width -le 0 -or [double] $_.placementContract.referenceBoundsMeters.depth -le 0 -or @($_.placementContract.connectionRoleCodes).Count -lt 2 }).Count -ne 0) { throw "TheoryFactoryH3PlacementContractInvalid" }
if (@($result.h2Plans + $result.h3Plans | Where-Object { [string] $_.displayNameKo -ne [string] $_.spatialDisplayNameKo -or [string] $_.spatialDisplayNameKo -eq [string] $_.gameplayProfileNameKo }).Count -ne 0) { throw "TheoryFactoryPrimarySpatialNameProjectionInvalid" }
if (@($result.h3Plans.nodes | Where-Object { [string]::IsNullOrWhiteSpace($_.h2DisplayNameKo) }).Count -ne 0) { throw "TheoryFactoryH3ChildSpatialNameMissing" }
if (@($result.e5AreaSetInstances.graphInstances | Where-Object { [string]::IsNullOrWhiteSpace($_.h3PatternCode) }).Count -ne 0) { throw "TheoryFactoryAreaSetPatternMissing" }
if (@($result.h2Plans.patternCode + $result.h3Plans.patternCode | Sort-Object -Unique).Count -ne 57) { throw "TheoryFactoryPatternCodeUniquenessInvalid" }
if ([int] $result.counts.queuedExpansionOrRepairItems -ne 3 -or @($result.priorityExpansionQueue).Count -ne 3) { throw "TheoryFactoryExpansionQueueInvalid" }
if ([int] $result.counts.semanticGapItems -ne 3 -or @($result.semanticGapQueue).Count -ne 3) { throw "TheoryFactorySemanticGapQueueInvalid" }
if (@($result.productionPhases).Count -ne 5) { throw "TheoryFactoryProductionPhaseInvalid" }

Write-Output "TheorySpatialFactoryTestsPassed:H2=37;H3=20;Patterns=57;E5=4;World=Qualified;Deterministic=True"
