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
$expectedGoalWip = @($ledger.items | Where-Object goalStateCode -eq 'Active').Count
$expectedWiWip = @($ledger.workItems | Where-Object { $_.statusCode -in @('Active','ReadyForIntegration') } | ForEach-Object { $_.worldInteractionId } | Sort-Object -Unique).Count
foreach ($expected in @(
    [string] $ledger.activeGoal.loopStableId,
    [string] $ledger.activeGoal.activeWorldInteractionId,
    "현재 성숙도 궤적: $($ledger.activeGoal.activeMaturityTrackCode)",
    "$($ledger.activeGoal.targetEvidenceStage) $($ledger.activeGoal.targetClosureStateCode)",
    "파이프라인 관문: Logic $($ledger.activeGoal.pipelineValidation.logicStatusCode) / Presentation $($ledger.activeGoal.pipelineValidation.presentationStatusCode) / 통합 $($ledger.activeGoal.pipelineValidation.integratedStatusCode)",
    "Nature → Farm → Hub → Town → City",
    "Goal WIP: ``$expectedGoalWip/상한 없음``",
    "WI WIP: ``$expectedWiWip/상한 없음``")) {
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
$unownedGoal = @($secondActive.items | Where-Object {
    $loopId = $_.loopStableId
    $_.goalStateCode -eq 'Queued' -and
        @($secondActive.workItems | Where-Object loopStableId -eq $loopId).Count -eq 0
}) | Select-Object -First 1
if ($null -eq $unownedGoal) { throw 'UnownedQueuedGoalFixtureRequired' }
$unownedGoal.goalStateCode = "Active"
Require-Rejected "active-goal-without-work-item" $secondActive "ActiveGoalWorkItemsMismatch"

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

function Require-Accepted([string] $Name, [object] $Ledger) {
    $path = Join-Path $artifactDirectory "$Name.json"
    [IO.File]::WriteAllText($path, ($Ledger | ConvertTo-Json -Depth 50), $utf8)
    & $manager -Mode Write -InputPath (Relative $path) -OutputPath (Relative (Join-Path $artifactDirectory "$Name.md")) | Out-Null
}
$parallelFixture = Read-Ledger
# 표시 순서가 아니라 대표 Goal의 실제 작업/소속 WI에서 합성 사례를 고른다.
$sourceWork = @($parallelFixture.workItems | Where-Object loopStableId -eq $parallelFixture.activeGoal.loopStableId)[0]
$fixtureLoops = Get-Content (Join-Path $repositoryRoot $parallelFixture.playableLoopCatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$sourceLoop = @($fixtureLoops.items | Where-Object loopStableId -eq $sourceWork.loopStableId)[0]
$alternativeWi = @($sourceLoop.worldInteractionIds | Where-Object { $_ -ne $sourceWork.worldInteractionId })[0]
if (-not $alternativeWi) { throw 'ParallelGoalFixtureRequiresSecondRegisteredWi' }
$nonfocus = $sourceWork | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$nonfocus.workItemId = 'test:nonfocus'
$nonfocus.worldInteractionId = $alternativeWi
$nonfocus.writePaths = @('synthetic-fixture/nonfocus')
$nonfocus.sharedContractKeys = @()
$nonfocusOrder = Get-Content (Join-Path $repositoryRoot $nonfocus.workOrderRef) -Raw -Encoding UTF8 | ConvertFrom-Json
$nonfocusOrder.activeWorldInteractionId = $nonfocus.worldInteractionId
$nonfocusOrder.pipelineValidation.profileKey = "$($nonfocus.loopStableId)|$($nonfocus.worldInteractionId)"
$nonfocusOrderPath = Join-Path $artifactDirectory 'nonfocus-order.json'
[IO.File]::WriteAllText($nonfocusOrderPath, ($nonfocusOrder | ConvertTo-Json -Depth 40), $utf8)
$nonfocus.workOrderRef = Relative $nonfocusOrderPath
$nonfocus.workOrderSha256 = (Get-FileHash $nonfocusOrderPath).Hash
$parallelFixture.workItems += $nonfocus
Require-Accepted 'same-goal-two-active-wis' $parallelFixture
$sourceWork.statusCode = 'Blocked'
$sourceWork.workOrderSha256 = '0' * 64
Require-Accepted 'blocked-stale-representative-with-independent-work' $parallelFixture
foreach ($workItem in $parallelFixture.workItems) { $workItem.statusCode = 'Blocked' }
Require-Accepted 'all-work-blocked-without-global-failure' $parallelFixture

# 복수 Goal도 같은 관리자 전체 경로로 검증한다. 합성 승인/명세는 artifacts에만 둔다.
$twoGoals = Read-Ledger
$twoLoops = Get-Content (Join-Path $repositoryRoot $twoGoals.playableLoopCatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$farmGoal = @($twoGoals.items | Where-Object loopStableId -eq 'playable-loop:farm-crop-cycle.v1')[0]
$farmGoal.goalStateCode='Active'
$farmLoop = @($twoLoops.items | Where-Object loopStableId -eq $farmGoal.loopStableId)[0]
$sourceLoop = @($twoLoops.items | Where-Object loopStableId -eq $twoGoals.activeGoal.loopStableId)[0]
$farmLoop.planningGate = $sourceLoop.planningGate | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$farmLoop.planningGate.topicStableId = 'topic:farm-crop-cycle.v1'
$farmLoop | Add-Member -NotePropertyName sourcePlanningDocumentRefs -NotePropertyValue $sourceLoop.sourcePlanningDocumentRefs -Force
$twoLoopsPath = Join-Path $artifactDirectory 'two-goals-loops.json'
[IO.File]::WriteAllText($twoLoopsPath, ($twoLoops | ConvertTo-Json -Depth 70), $utf8)
$twoGoals.playableLoopCatalogPath = Relative $twoLoopsPath
$farmWork = $nonfocus | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$farmWork.workItemId='test:farm';$farmWork.loopStableId=$farmGoal.loopStableId;$farmWork.worldInteractionId=$farmGoal.nextWorldInteractionId
$farmOrder=$nonfocusOrder | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$farmOrder.playableUnitStableId=$farmWork.loopStableId;$farmOrder.activeWorldInteractionId=$farmWork.worldInteractionId
$farmOrder.pipelineValidation.profileKey="$($farmWork.loopStableId)|$($farmWork.worldInteractionId)"
$farmOrderPath=Join-Path $artifactDirectory 'farm-order.json'
[IO.File]::WriteAllText($farmOrderPath, ($farmOrder | ConvertTo-Json -Depth 40), $utf8)
$farmWork.workOrderRef=Relative $farmOrderPath;$farmWork.workOrderSha256=(Get-FileHash $farmOrderPath).Hash
$twoGoals.workItems+=$farmWork
Require-Accepted 'two-independent-active-goals' $twoGoals

# 구형 단일 작업 원장은 새 작업 목록으로 자동 확장하지 않고 읽는다.
$legacy = Read-Ledger
$legacy.schemaVersion = 'codex-playable-loop-goals.v3'
$legacy.PSObject.Properties.Remove('workItems')
$legacy.policy.goalWorkInProgressLimit = 1
$legacy.policy.worldInteractionWorkInProgressLimit = 1
# v3 입력은 단일 대표 작업만 활성인 당시 형태로 만든다.
foreach ($item in $legacy.items) {
    if ($item.goalStateCode -eq 'Active' -and $item.loopStableId -ne $legacy.activeGoal.loopStableId) {
        $item.goalStateCode = 'Queued'
    }
}
Require-Accepted 'legacy-v3-read' $legacy

Write-Output "CodexPlayableLoopGoalTestsPassed:Positive=8;Negative=8;Goals=$expectedGoalCount"
Write-Output $first
Write-Output $second
Write-Output $check
