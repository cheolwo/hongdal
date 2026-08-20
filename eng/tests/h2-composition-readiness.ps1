if ($PSVersionTable.PSEdition -ne "Core") {
    & pwsh -NoProfile -File $PSCommandPath
    exit $LASTEXITCODE
}

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/world-seedbeds/manage-h2-composition-readiness.ps1"
$jsonOutput = Join-Path $repositoryRoot "eng/world-seedbeds/generated/h2-composition-readiness.v1.json"
$markdownOutput = Join-Path $repositoryRoot "docs/AI/generated/h2-composition-readiness.md"

$first = & $script -Mode Write
$firstJsonHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $jsonOutput).Hash
$firstMarkdownHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $markdownOutput).Hash
$firstJsonTicks = (Get-Item -LiteralPath $jsonOutput).LastWriteTimeUtc.Ticks
$firstMarkdownTicks = (Get-Item -LiteralPath $markdownOutput).LastWriteTimeUtc.Ticks

$second = & $script -Mode Write
$check = & $script -Mode Check

if ($firstJsonHash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $jsonOutput).Hash) { throw "H2ReadinessJsonIsNotDeterministic" }
if ($firstMarkdownHash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $markdownOutput).Hash) { throw "H2ReadinessMarkdownIsNotDeterministic" }
if ($firstJsonTicks -ne (Get-Item -LiteralPath $jsonOutput).LastWriteTimeUtc.Ticks) { throw "H2ReadinessJsonWasRewritten" }
if ($firstMarkdownTicks -ne (Get-Item -LiteralPath $markdownOutput).LastWriteTimeUtc.Ticks) { throw "H2ReadinessMarkdownWasRewritten" }
if ($check -notmatch "H2CompositionReadinessValid:H1=52;Used=51;H2=34/34;Theory=34;Authored=6;Derived=28;Unity=6;Review=6;Gameplay=12;TheoryPriority=24;ReviewPriority=5;StrictMissing=8;Warnings=12") { throw "H2ReadinessSummaryUnexpected:$check" }

$readiness = Get-Content -LiteralPath $jsonOutput -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string] $readiness.revision -ne "simulation-world-h2-composition-readiness.r3") { throw "H2ReadinessRevisionInvalid" }
if ([string] $readiness.theoryFactoryRevision -ne "simulation-world-theory-spatial-factory-output.r2") { throw "H2TheoryFactoryRevisionInvalid" }
if (@($readiness.items | Where-Object inventoryStateCode -eq "IdeaInventory" | Where-Object { -not $_.composable }).Count -ne 0) {
    throw "IdeaInventoryH1OrH2WasIncorrectlyExcludedFromRecognizedComposition"
}
if ([int] $readiness.counts.h2TheoryQualifiedCount -ne 34) { throw "H2TheoryQualifiedCountInvalid" }
if ([int] $readiness.counts.h2AuthoredTheoryRecipeCount -ne 6) { throw "H2AuthoredTheoryRecipeCountInvalid" }
if ([int] $readiness.counts.h2DerivedTheoryRecipeCount -ne 28) { throw "H2DerivedTheoryRecipeCountInvalid" }
if ([int] $readiness.counts.h2TheoryProductionBlockedByHumanReviewCount -ne 0) { throw "H2TheoryProductionWasBlockedByHumanReview" }
if (@($readiness.items | Where-Object { -not $_.theoryQualified -or $_.compositionStageCode -ne "TheoryQualified" }).Count -ne 0) {
    throw "H2TheoryQualificationMissing"
}
if (@($readiness.items | Where-Object { @($_.theoryBlockers).Count -gt 0 }).Count -ne 0) { throw "H2TheoryBlockerFound" }
if (@($readiness.items | Where-Object { [string] $_.theoryHashSha256 -notmatch "^[0-9a-f]{64}$" }).Count -ne 0) { throw "H2TheoryHashInvalid" }
if (@($readiness.items | Where-Object theoryRecipeSourceCode -eq "AuthoredRecipe" | Where-Object { [string]::IsNullOrWhiteSpace([string] $_.deterministicRecipeId) }).Count -ne 0) {
    throw "H2AuthoredTheoryRecipeLineageMissing"
}
if (@($readiness.items | Where-Object theoryRecipeSourceCode -eq "DerivedTheoryRecipe" | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_.deterministicRecipeId) }).Count -ne 0) {
    throw "H2DerivedTheoryRecipeIncorrectlyClaimedAuthoredLineage"
}
if (@($readiness.items | Where-Object reviewReady).Count -ne 6) { throw "H2ReviewReadyCountInvalid" }
if (@($readiness.items | Where-Object reviewReady | Where-Object { $_.deterministicRecipeId -eq $null }).Count -ne 0) {
    throw "H2ReviewReadyWithoutDeterministicRecipe"
}
if ([int] $readiness.counts.h2GameplayTraceCount -ne 12) { throw "H2GameplayTraceCountInvalid" }
if ([int] $readiness.counts.h2GameplayTheoryPriorityReadyCount -ne 24) { throw "H2GameplayTheoryPriorityReadyCountInvalid" }
if ([int] $readiness.counts.h2GameplayReviewPriorityReadyCount -ne 5) { throw "H2GameplayReviewPriorityReadyCountInvalid" }
if ([int] $readiness.counts.h2StrictGameplayTraceMissingCount -ne 8) { throw "H2StrictGameplayTraceMissingCountInvalid" }
if ([int] $readiness.counts.h2WarningOnlyGameplayTraceMissingCount -ne 12) { throw "H2WarningOnlyGameplayTraceCountInvalid" }
$strictReviewReady = @($readiness.items | Where-Object { $_.reviewReady -and $_.gameplayGateModeCode -eq "Strict" })
if ($strictReviewReady.Count -ne 5) { throw "H2StrictReviewReadyCountInvalid" }
if (@($strictReviewReady | Where-Object gameplayTraceStateCode -eq "SequenceMapped").Count -ne 4) { throw "H2StrictReviewReadySequenceCountInvalid" }
if (@($strictReviewReady | Where-Object { $_.gameplayTraceStateCode -eq "Unlinked" -and -not $_.gameplayReviewPriorityReady }).Count -ne 1) { throw "H2StrictReviewReadyMissingTraceGateInvalid" }
$strictTheoryReady = @($readiness.items | Where-Object gameplayGateModeCode -eq "Strict")
if ($strictTheoryReady.Count -ne 20) { throw "H2StrictTheoryReadyCountInvalid" }
if (@($strictTheoryReady | Where-Object { -not $_.gameplayTheoryPriorityReady }).Count -ne 8) { throw "H2StrictTheoryMissingTraceCountInvalid" }
$warningReviewReady = @($readiness.items | Where-Object { $_.reviewReady -and $_.gameplayGateModeCode -eq "WarningOnly" })
if ($warningReviewReady.Count -ne 1) { throw "H2WarningReviewReadyCountInvalid" }
if (@($warningReviewReady | Where-Object { -not $_.gameplayReviewPriorityReady }).Count -ne 0) { throw "H2WarningOnlyGateIncorrectlyBlockedReview" }
if (@($warningReviewReady | Where-Object { @($_.gameplayWarnings) -notcontains "WarningOnlyGameplayTraceMissing" }).Count -ne 0) { throw "H2WarningOnlyTraceWarningMissing" }
$warningTheoryReady = @($readiness.items | Where-Object gameplayGateModeCode -eq "WarningOnly")
if ($warningTheoryReady.Count -ne 12) { throw "H2WarningTheoryReadyCountInvalid" }
if (@($warningTheoryReady | Where-Object { -not $_.gameplayTheoryPriorityReady }).Count -ne 0) { throw "H2WarningOnlyGateIncorrectlyBlockedTheory" }
$notSelected = @($readiness.items | Where-Object gameplayGateModeCode -eq "NotSelected")
if ($notSelected.Count -ne 2 -or @($notSelected | Where-Object gameplayTheoryPriorityReady).Count -ne 0) { throw "H2NotSelectedTheoryPriorityInvalid" }

Write-Output "H2CompositionReadinessTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
