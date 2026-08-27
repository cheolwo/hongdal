$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/execution-ledgers/manage-codex-playable-loop-goals.ps1"
$inputPath = Join-Path $repositoryRoot "eng/execution-ledgers/codex-playable-loop-goals.json"
$outputPath = Join-Path $repositoryRoot "docs/AI/generated/codex-playable-loop-goals.md"

$first = & $manager -Mode Write
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $outputPath).Hash
$ticks = (Get-Item -LiteralPath $outputPath).LastWriteTimeUtc.Ticks
$second = & $manager -Mode Write
$check = & $manager -Mode Check
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $outputPath).Hash -ne $hash -or
    (Get-Item -LiteralPath $outputPath).LastWriteTimeUtc.Ticks -ne $ticks) {
    throw "CodexPlayableLoopGoalOutputIsNotDeterministic"
}
if ($check -notmatch "CodexPlayableLoopGoalsValid:Goals=16") {
    throw "CodexPlayableLoopGoalValidationDidNotComplete"
}

$generated = Get-Content -LiteralPath $outputPath -Raw -Encoding UTF8
foreach ($expected in @(
    "playable-loop:nature-shelter-foundation.v1",
    "playable-loop:nature-base-reflection.v1",
    "playable-loop:nature-regional-threat-recovery.v1",
    "WI-NATURE-15",
    "현재 성숙도 궤적: Presentation",
    "E7 PlayClosed",
    "폐루프 E4 / WI E6 → E7",
    "파이프라인 관문: Logic Passed / Presentation Blocked / 통합 Blocked",
    "Nature → Farm → Hub → Town → City",
    "Goal WIP: ``1/1``",
    "WI WIP: ``1/1``")) {
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
    [IO.File]::WriteAllText($path, ($Ledger | ConvertTo-Json -Depth 40), $utf8)
    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", $manager,
        "-Mode", "Check",
        "-InputPath", (Relative $path)
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
$missingPlayableUnit.items = @($missingPlayableUnit.items | Select-Object -First 15)
Require-Rejected "missing-playable-unit" $missingPlayableUnit `
    "PlayableUnitGoalCoverageCountInvalid"

$extensionBeforeCore = Read-Ledger
$coreOrder = $extensionBeforeCore.items[10].queueOrder
$extensionBeforeCore.items[10].queueOrder = $extensionBeforeCore.items[11].queueOrder
$extensionBeforeCore.items[11].queueOrder = $coreOrder
Require-Rejected "extension-before-core" $extensionBeforeCore `
    "CoreGoalAfterExtensionPhase"

$unapprovedCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot `
    "eng/execution-ledgers/playable-loops.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$unapprovedUnit = $unapprovedCatalog.items | Where-Object `
    loopStableId -eq "playable-loop:nature-night-day2.v1"
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
$legacyTransferredCatalog.designDocumentationPolicy.legacyActiveMigrationLoopStableId = `
    "playable-loop:nature-workbench-foundation.v1"
$legacyTransferredCatalogPath = Join-Path $artifactDirectory `
    "legacy-transferred-catalog.json"
[IO.File]::WriteAllText($legacyTransferredCatalogPath, `
    ($legacyTransferredCatalog | ConvertTo-Json -Depth 50), $utf8)
$legacyTransferredGoal = Read-Ledger
$legacyTransferredGoal.playableLoopCatalogPath = Relative $legacyTransferredCatalogPath
Require-Rejected "legacy-transferred-goal" $legacyTransferredGoal `
    "LegacyPlanningGateTransferred"

Write-Output "CodexPlayableLoopGoalTestsPassed:Positive=3;Negative=8;Goals=16"
Write-Output $first
Write-Output $second
Write-Output $check
