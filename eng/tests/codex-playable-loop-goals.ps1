$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/execution-ledgers/manage-codex-playable-loop-goals.ps1"
$inputPath = Join-Path $repositoryRoot "eng/execution-ledgers/codex-playable-loop-goals.json"
$outputPath = Join-Path $repositoryRoot "docs/AI/generated/codex-playable-loop-goals.md"
$ledger = Get-Content -LiteralPath $inputPath -Raw -Encoding UTF8 | ConvertFrom-Json
$expectedGoalCount = @($ledger.items).Count

$first = & $manager -Mode Write
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $outputPath).Hash
$ticks = (Get-Item -LiteralPath $outputPath).LastWriteTimeUtc.Ticks
$second = & $manager -Mode Write
$check = & $manager -Mode Check
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $outputPath).Hash -ne $hash -or
    (Get-Item -LiteralPath $outputPath).LastWriteTimeUtc.Ticks -ne $ticks) {
    throw "CodexPlayableLoopGoalOutputIsNotDeterministic"
}
if ($check -notmatch "CodexPlayableLoopGoalsValid:Goals=$expectedGoalCount") {
    throw "CodexPlayableLoopGoalValidationDidNotComplete"
}

$generated = Get-Content -LiteralPath $outputPath -Raw -Encoding UTF8
$expectedGoalWip = if ([string] $ledger.activeGoal.goalStateCode -eq "Completed") { 0 } else { 1 }
$expectedWiWip = $expectedGoalWip
foreach ($expected in @(
    [string] $ledger.activeGoal.loopStableId,
    [string] $ledger.activeGoal.activeWorldInteractionId,
    "현재 성숙도 궤적: $($ledger.activeGoal.activeMaturityTrackCode)",
    "$($ledger.activeGoal.targetEvidenceStage) $($ledger.activeGoal.targetClosureStateCode)",
    "파이프라인 관문: Logic $($ledger.activeGoal.pipelineValidation.logicStatusCode) / Presentation $($ledger.activeGoal.pipelineValidation.presentationStatusCode) / 통합 $($ledger.activeGoal.pipelineValidation.integratedStatusCode)",
    "Nature → Farm → Hub → Town → City",
    "Goal WIP: ``$expectedGoalWip/1``",
    "WI WIP: ``$expectedWiWip/1``")) {
    if (-not $generated.Contains($expected)) {
        throw "CodexPlayableLoopGoalGeneratedEntryMissing:$expected"
    }
}

$artifactDirectory = Join-Path $repositoryRoot "artifacts/local/validation/codex-playable-loop-goals/negative"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
function Relative([string] $Path) {
    $rootWithSeparator = $repositoryRoot.TrimEnd("\") + "\"
    $rootUri = [Uri]::new($rootWithSeparator)
    $pathUri = [Uri]::new([IO.Path]::GetFullPath($Path))
    return [Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString())
}
function Read-Ledger() {
    return Get-Content -LiteralPath $inputPath -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Require-Rejected([string] $Name, [object] $Ledger, [string] $ExpectedCode) {
    $path = Join-Path $artifactDirectory "$Name.json"
    $generatedPath = Join-Path $artifactDirectory "$Name.md"
    [IO.File]::WriteAllText($path, ($Ledger | ConvertTo-Json -Depth 40), $utf8)
    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", $manager,
        "-Mode", "Write",
        "-InputPath", (Relative $path),
        "-OutputPath", (Relative $generatedPath)
    )
    $failureText = ""
    try {
        $failureText = (& powershell @arguments 2>&1 | Out-String)
    }
    catch {
        $failureText = $_.Exception.Message + "`n" + ($_ | Out-String)
    }
    if ($LASTEXITCODE -eq 0 -or -not $failureText.Contains($ExpectedCode)) {
        throw "CodexPlayableLoopGoalNegativeWasAccepted:${Name}:$failureText"
    }
}

$aggregate = Read-Ledger
$aggregate.items[0].loopStableId = "playable-loop:nature-survival-homestead.v1"
$aggregate.activeGoal.loopStableId = "playable-loop:nature-survival-homestead.v1"
Require-Rejected "aggregate-as-goal" $aggregate "GoalSubjectIsNotPlayableUnit"

$secondActive = Read-Ledger
$secondActive.items[1].goalStateCode = "Active"
Require-Rejected "two-active-goals" $secondActive "ActiveGoalCountInvalid"

$e9Target = Read-Ledger
$e9Target.items[2].targetEvidenceStage = "E9"
Require-Rejected "e9-immediate-target" $e9Target "TargetEvidenceStageDrift"

$wrongWi = Read-Ledger
$wrongWi.items[0].nextWorldInteractionId = "WI-FARM-01"
$wrongWi.activeGoal.activeWorldInteractionId = "WI-FARM-01"
Require-Rejected "wi-outside-loop" $wrongWi "NextWorldInteractionOutsideLoop"

$missingPlayableUnit = Read-Ledger
$missingPlayableUnit.items = @($missingPlayableUnit.items |
    Select-Object -First ($expectedGoalCount - 1))
Require-Rejected "missing-playable-unit" $missingPlayableUnit `
    "PlayableUnitGoalCoverageCountInvalid"

$extensionBeforeCore = Read-Ledger
$lastCore = $extensionBeforeCore.items | Where-Object completionRoleCode -eq "Core" |
    Sort-Object queueOrder | Select-Object -Last 1
$firstExtension = $extensionBeforeCore.items |
    Where-Object completionRoleCode -eq "Extension" |
    Sort-Object queueOrder | Select-Object -First 1
$coreOrder = $lastCore.queueOrder
$lastCore.queueOrder = $firstExtension.queueOrder
$firstExtension.queueOrder = $coreOrder
Require-Rejected "extension-before-core" $extensionBeforeCore `
    "CoreGoalAfterExtensionPhase"

$unapprovedCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot `
    "eng/execution-ledgers/playable-loops.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$unapprovedUnit = $unapprovedCatalog.items | Where-Object `
    loopStableId -eq ([string] $ledger.activeGoal.loopStableId)
$unapprovedUnit.planningGate.statusCode = "NotStarted"
$unapprovedCatalogPath = Join-Path $artifactDirectory "unapproved-active-catalog.json"
[IO.File]::WriteAllText($unapprovedCatalogPath, `
    ($unapprovedCatalog | ConvertTo-Json -Depth 50), $utf8)
$unapprovedGoal = Read-Ledger
$unapprovedGoal.playableLoopCatalogPath = Relative $unapprovedCatalogPath
Require-Rejected "unapproved-active-goal" $unapprovedGoal `
    "ActiveGoalPlanningNotApproved"

$legacyTransferredCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot `
    "eng/execution-ledgers/playable-loops.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$legacyActiveUnit = $legacyTransferredCatalog.items | Where-Object `
    loopStableId -eq ([string] $ledger.activeGoal.loopStableId)
$legacyActiveUnit.planningGate.statusCode = "LegacyActiveMigration"
$differentPlayableUnit = $legacyTransferredCatalog.items | Where-Object {
    $_.loopLevelCode -eq "PlayableUnit" -and
    $_.loopStableId -ne [string] $ledger.activeGoal.loopStableId
} | Select-Object -First 1
$legacyTransferredCatalog.designDocumentationPolicy.legacyActiveMigrationLoopStableId = `
    [string] $differentPlayableUnit.loopStableId
$legacyTransferredCatalogPath = Join-Path $artifactDirectory `
    "legacy-transferred-catalog.json"
[IO.File]::WriteAllText($legacyTransferredCatalogPath, `
    ($legacyTransferredCatalog | ConvertTo-Json -Depth 50), $utf8)
$legacyTransferredGoal = Read-Ledger
$legacyTransferredGoal.playableLoopCatalogPath = Relative $legacyTransferredCatalogPath
Require-Rejected "legacy-transferred-goal" $legacyTransferredGoal `
    "LegacyPlanningGateTransferred"

Write-Output "CodexPlayableLoopGoalTestsPassed:Positive=3;Negative=8;Goals=$expectedGoalCount"
Write-Output $first
Write-Output $second
Write-Output $check
