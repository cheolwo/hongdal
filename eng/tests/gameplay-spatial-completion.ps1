if ($PSVersionTable.PSEdition -ne "Core") {
    & pwsh -NoProfile -File $PSCommandPath
    exit $LASTEXITCODE
}

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-gameplay-spatial-completion.ps1"
$sourcePath = Join-Path $repositoryRoot "eng/world-seedbeds/gameplay-spatial-completion.v1.json"
$jsonOutput = Join-Path $repositoryRoot "eng/world-seedbeds/generated/gameplay-spatial-completion.v1.json"
$markdownOutput = Join-Path $repositoryRoot "docs/AI/generated/gameplay-spatial-completion.md"

$first = & $manager -Mode Write
$firstJsonHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $jsonOutput).Hash
$firstMarkdownHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $markdownOutput).Hash
$firstJsonTicks = (Get-Item -LiteralPath $jsonOutput).LastWriteTimeUtc.Ticks
$firstMarkdownTicks = (Get-Item -LiteralPath $markdownOutput).LastWriteTimeUtc.Ticks
$second = & $manager -Mode Write
$check = & $manager -Mode Check

if ($first -notmatch "GameplaySpatialCompletionGenerated:Slices=1;Strict=2;Warnings=2;H=17/12/8/2;Blockers=4") { throw "GameplaySpatialCompletionFirstSummaryInvalid:$first" }
if ($second -notmatch "GameplaySpatialCompletionGenerated:Slices=1;Strict=2;Warnings=2;H=17/12/8/2;Blockers=4") { throw "GameplaySpatialCompletionSecondSummaryInvalid:$second" }
if ($check -notmatch "GameplaySpatialCompletionValid:Slices=1;Strict=2;Warnings=2;H=17/12/8/2;Blockers=4") { throw "GameplaySpatialCompletionCheckSummaryInvalid:$check" }
if ($firstJsonHash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $jsonOutput).Hash) { throw "GameplaySpatialCompletionJsonNotDeterministic" }
if ($firstMarkdownHash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $markdownOutput).Hash) { throw "GameplaySpatialCompletionMarkdownNotDeterministic" }
if ($firstJsonTicks -ne (Get-Item -LiteralPath $jsonOutput).LastWriteTimeUtc.Ticks) { throw "GameplaySpatialCompletionJsonRewritten" }
if ($firstMarkdownTicks -ne (Get-Item -LiteralPath $markdownOutput).LastWriteTimeUtc.Ticks) { throw "GameplaySpatialCompletionMarkdownRewritten" }

$report = Get-Content -LiteralPath $jsonOutput -Raw -Encoding UTF8 | ConvertFrom-Json
if ((@($report.axisDefinitions.PSObject.Properties.Name) -join ",") -ne "h,gameplayTrace,evidence,playableSlice") { throw "GameplaySpatialCompletionAxesInvalid" }
if ((@($report.coveredGamePlanCodes) -join ",") -ne "FarmProductionSurvival,NatureHomeThreatRecovery") { throw "GameplaySpatialCompletionStrictPlansInvalid" }
if ((@($report.warningOnlyMissingGamePlanCodes) -join ",") -ne "CityHubLogisticsResilience,TownLivingMarketSafety") { throw "GameplaySpatialCompletionWarningPlansInvalid" }

$slice = @($report.playableSlices | Where-Object playableSliceId -eq "reference-play:nature-farm-day.v1")
if ($slice.Count -ne 1) { throw "NatureFarmPlayableSliceMissing" }
if ([string] $slice[0].currentPlayableSliceStateCode -ne "SpatiallyComposed") { throw "NatureFarmActualSpatialStateInvalid" }
if ([string] $slice[0].theorySpatialBindingStateCode -ne "E5TheoryQualified") { throw "NatureFarmTheoryE5QualificationMissing" }
if ((@($slice[0].theoryAreaSetStableIds) -join ",") -ne "area-set:theory:farm-production-processing-region,area-set:theory:nature-home-exploration-region") { throw "NatureFarmTheoryAreaSetBindingInvalid" }
if ([string] $slice[0].actualSpatialBindingStateCode -ne "ActualE5Bound") { throw "NatureFarmActualE5BindingMissing" }
if ((@($slice[0].actualAreaSetStableIds) -join ",") -ne "area-set:sim:pyeongchang:farm-production.v1,area-set:sim:pyeongchang:nature-home.v1") { throw "NatureFarmActualAreaSetBindingInvalid" }
if (@($slice[0].completionBlockReasonCodes).Count -ne 4) { throw "NatureFarmCompletionBlockerCountInvalid" }
if (@($slice[0].completionBlockReasonCodes) -contains "ActualE5BindingMissing") { throw "NatureFarmActualE5BlockerWasNotClosed" }
if (@($slice[0].wiEvidence | Where-Object { @($_.e7EvidenceRefs).Count -gt 0 }).Count -ne 0) { throw "NatureFarmE7EvidenceWasInvented" }
if ([string] $report.theorySpatialFactoryRevision -ne "simulation-world-theory-spatial-factory-output.r3") { throw "TheorySpatialFactoryRevisionMissing" }
if ([string] $report.actualE5SpatialRevision -ne "simulation-world-actual-e5-spatial-output.r1") { throw "ActualE5SpatialRevisionMissing" }
$directE5Wi = @($slice[0].wiEvidence | Where-Object { @($_.e5PlacementRefs).Count -gt 0 })
$contextualE5Wi = @($slice[0].wiEvidence | Where-Object { @($_.e5ContextRefs).Count -gt 0 })
if ($directE5Wi.Count -ne 7 -or $contextualE5Wi.Count -ne 1) { throw "NatureFarmWiActualE5PartitionInvalid" }
if (@($slice[0].wiEvidence | Where-Object integrationStageCode -ne "E5").Count -ne 0) { throw "NatureFarmWiEffectiveE5StageInvalid" }
$natureToFarmHandoff = @($slice[0].regionalHandoffs | Where-Object handoffCode -eq "NatureToFarmTraversal")
if ($natureToFarmHandoff.Count -ne 1) { throw "NatureFarmRegionalHandoffMissing" }
if ([string] $natureToFarmHandoff[0].theoryBindingStateCode -ne "E5TheoryQualified") { throw "NatureFarmHandoffTheoryBindingInvalid" }
if ([string] $natureToFarmHandoff[0].actualBindingStateCode -ne "ActualE5Bound") { throw "NatureFarmHandoffActualBindingInvalid" }

$trailhead = @($report.hTrace | Where-Object knowledgeRef -eq "h1-stock:nature-trailhead")
if ($trailhead.Count -ne 1 -or [string] $trailhead[0].gameplayTraceStateCode -ne "Supporting") { throw "SupportingLandscapeTraceInvalid" }
if (@($trailhead[0].contributionCodes) -notcontains "TraversalGuidance") { throw "SupportingLandscapeContributionMissing" }
$farmProduction = @($report.hTrace | Where-Object knowledgeRef -eq "h1-stock:farm-production")
if ($farmProduction.Count -ne 1 -or [string] $farmProduction[0].gameplayTraceStateCode -ne "DirectAction") { throw "FarmDirectActionTraceInvalid" }
$natureThreatH2 = @($report.hTrace | Where-Object knowledgeRef -eq "h2-candidate:nature-threat-response")
if ($natureThreatH2.Count -ne 1 -or [string] $natureThreatH2[0].gameplayTraceStateCode -ne "SequenceMapped") { throw "NatureH2SequenceTraceInvalid" }
$natureH4 = @($report.hTrace | Where-Object knowledgeRef -eq "h4-blueprint:nature-home-exploration-region")
if ($natureH4.Count -ne 1 -or [string] $natureH4[0].gameplayTraceStateCode -ne "RegionalCausalityMapped") { throw "NatureH4RegionalTraceInvalid" }

$conditionSlot = @($slice[0].conditionSlots | Where-Object conditionSlotCode -eq "NextDayManagementCondition")
if ($conditionSlot.Count -ne 1 -or (@($conditionSlot[0].allowedStateCodes) -join ",") -ne "Normal,Opportunity,Threat,Recovery") { throw "NextDayConditionSlotInvalid" }
if (@($conditionSlot[0].spatialExpressionH1Refs).Count -ne 4) { throw "NextDayConditionSpatialRefsInvalid" }

$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts/local/validation"))
$tempRoot = [IO.Path]::GetFullPath((Join-Path $artifactsRoot ("gameplay-spatial-completion-tests-" + [guid]::NewGuid().ToString("N"))))
if (-not $tempRoot.StartsWith($artifactsRoot, [StringComparison]::OrdinalIgnoreCase)) { throw "GameplaySpatialCompletionTempPathEscaped" }
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

function Write-Fixture([object] $Fixture, [string] $Name) {
    $path = Join-Path $tempRoot "$Name.json"
    ($Fixture | ConvertTo-Json -Depth 40) + "`n" | Set-Content -LiteralPath $path -Encoding UTF8 -NoNewline
    $fullPath = [IO.Path]::GetFullPath($path)
    $repositoryPrefix = $repositoryRoot.TrimEnd([char[]] @("\", "/")) + [IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($repositoryPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "GameplaySpatialCompletionFixturePathEscaped"
    }
    return $fullPath.Substring($repositoryPrefix.Length).Replace("\", "/")
}

function Assert-FixtureFailure([object] $Fixture, [string] $Name, [string] $ExpectedCode) {
    $relativeSource = Write-Fixture $Fixture $Name
    $relativeJson = "artifacts/local/validation/$([IO.Path]::GetFileName($tempRoot))/$Name.output.json"
    $relativeMarkdown = "artifacts/local/validation/$([IO.Path]::GetFileName($tempRoot))/$Name.output.md"
    $errorText = ""
    try {
        & $manager -Mode Write -SourcePath $relativeSource -JsonOutputPath $relativeJson -MarkdownOutputPath $relativeMarkdown 2>&1 | Out-Null
    }
    catch {
        $errorText = $_.Exception.Message
    }
    if ($errorText -notmatch [regex]::Escape($ExpectedCode)) { throw "ExpectedFixtureFailureMissing:${Name}:${ExpectedCode}:$errorText" }
}

try {
    $fixture = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $fixture.playableSlices[0].steps[1].supportingContributionCodes = @("NotAContribution")
    Assert-FixtureFailure $fixture "unknown-supporting-contribution" "UnknownSupportingContribution"

    $fixture = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $fixture.playableSlices[0].steps[2].h3Refs = @("h3-candidate:nature-trail-network")
    Assert-FixtureFailure $fixture "disconnected-h3" "H3DoesNotContainStepH2"

    $fixture = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $fixture.playableSlices[0].gamePlanCodes = @("FarmProductionSurvival")
    Assert-FixtureFailure $fixture "strict-plan-missing" "StrictGamePlanNotCovered"

    $fixture = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $fixture.playableSlices[0].declaredPlayableSliceStateCode = "FunctionallyClosed"
    Assert-FixtureFailure $fixture "invented-completion-state" "DeclaredPlayableStateMismatch"

    $fixture = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $fixture.playableSlices[0].conditionSlots[0].spatialExpressionH1Refs += "h1-stock:farm-seed-preparation"
    Assert-FixtureFailure $fixture "untraced-condition-space" "UntracedConditionH1"

    $fixture = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $fixture.playableSlices[0].regionalHandoffs[0].relationCode = "InventedTheoryRelation"
    Assert-FixtureFailure $fixture "invented-theory-handoff" "HandoffTheoryRelationMissing"
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        $verifiedTempRoot = [IO.Path]::GetFullPath($tempRoot)
        if (-not $verifiedTempRoot.StartsWith($artifactsRoot, [StringComparison]::OrdinalIgnoreCase)) { throw "GameplaySpatialCompletionCleanupPathEscaped" }
        Remove-Item -LiteralPath $verifiedTempRoot -Recurse -Force
    }
}

Write-Output "GameplaySpatialCompletionTestsPassed:Slices=1;Strict=2;Warnings=2;H=17/12/8/2;Negative=6"
