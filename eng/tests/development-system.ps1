$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot `
    "eng/execution-ledgers/manage-development-system.ps1"

$result = @(& $manager -Mode Validate)
if (($result -join "`n") -notlike `
    "*DevelopmentSystemValid:Loops=23;Independent=21;World=1;Cross=1;Evidence=32*") {
    throw "DevelopmentSystemValidationFailed:$($result -join ';')"
}

$completionPath = Join-Path $repositoryRoot `
    "docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md"
$completion = Get-Content -LiteralPath $completionPath -Raw -Encoding UTF8
foreach ($expected in @(
    "playable-loop:nature-survival-homestead.v1",
    "playable-loop:nature-shelter-foundation.v1",
    "playable-loop:nature-twilight-return.v1",
    "playable-loop:nature-regional-threat-recovery.v1",
    "playable-loop:nature-night-day2.v1",
    "playable-loop:nature-workbench-foundation.v1",
    "playable-loop:nature-building-learning.v1",
    "playable-loop:nature-base-reflection.v1",
    "playable-loop:town-order-consume-return.v1",
    "playable-loop:town-arcana-context.v1",
    "playable-loop:farm-crop-cycle.v1",
    "playable-loop:farm-pack-store-return.v1",
    "playable-loop:hub-inbound-putaway.v1",
    "playable-loop:hub-outbound-ready-return.v1",
    "playable-loop:city-demand-service-return.v1",
    "playable-loop:solo-world-day.v1",
    "playable-loop:nature-farm-roundtrip.v1",
    "evidence:hub-npc-routine-core-20260825",
    "evidence:nature-building-core-20260825",
    "evidence:nature-base-reflection-e3-20260826",
    "evidence:nature-shelter-playmode-20260825",
    "evidence:nature-shelter-hosted-parity-20260825",
    "evidence:nature-shelter-explicit-equipment-e7-20260827",
    "evidence:nature-shelter-e8-logic-20260827",
    "evidence:nature-shelter-e8-presentation-20260827",
    "evidence:nature-twilight-e8-logic-20260827",
    "evidence:nature-twilight-e8-presentation-20260827",
    "evidence:nature-regional-threat-core-20260826",
    "evidence:nature-night-day2-wi13-playmode-20260826",
    "evidence:nature-night-day2-wi13-hosted-parity-20260826",
    "evidence:nature-night-day2-wi14-playmode-20260826",
    "evidence:nature-night-day2-wi14-hosted-parity-20260826",
    "evidence:nature-night-day2-wi15-playmode-20260826",
    "evidence:nature-night-day2-wi15-hosted-parity-20260826",
    "evidence:town-arcana-core-20260825")) {
    if (-not $completion.Contains($expected)) {
        throw "DevelopmentCompletionLedgerEntryMissing:$expected"
    }
}

$artifactDirectory = Join-Path $repositoryRoot `
    "artifacts/local/validation/development-system/negative"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
$loopsPath = Join-Path $repositoryRoot `
    "eng/execution-ledgers/playable-loops.json"
$evidencePath = Join-Path $repositoryRoot `
    "eng/execution-ledgers/evidence-packages.json"

function Relative([string] $Path) {
    $rootWithSeparator = $repositoryRoot.TrimEnd("\") + "\"
    $rootUri = [Uri]::new($rootWithSeparator)
    $pathUri = [Uri]::new([IO.Path]::GetFullPath($Path))
    return [Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString())
}

function Read-Loops() {
    return Get-Content -LiteralPath $loopsPath -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Write-Json([object] $Value, [string] $Path) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 40), $utf8)
}

function Require-Rejected(
    [string] $Name,
    [object] $Catalog,
    [string] $ExpectedCode,
    [string] $EvidenceCatalogPath = "") {
    $path = Join-Path $artifactDirectory "$Name.json"
    $outputPath = Join-Path $artifactDirectory "$Name.md"
    Write-Json $Catalog $path
    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", $manager,
        "-Mode", "Write",
        "-PlayableLoopPath", (Relative $path),
        "-OutputPath", (Relative $outputPath)
    )
    if (-not [string]::IsNullOrWhiteSpace($EvidenceCatalogPath)) {
        $arguments += @("-EvidencePackagePath", $EvidenceCatalogPath)
    }
    $failureText = ""
    try {
        $failureText = (& powershell @arguments 2>&1 | Out-String)
    }
    catch {
        $failureText = $_.Exception.Message + "`n" + ($_ | Out-String)
    }
    $exitCode = $LASTEXITCODE
    if ($exitCode -eq 0 -or -not $failureText.Contains($ExpectedCode)) {
        throw "DevelopmentSystemNegativeWasAccepted:${Name}:$failureText"
    }
}

$unknownWi = Read-Loops
$unknownWiUnit = $unknownWi.items | Where-Object `
    loopStableId -eq "playable-loop:nature-shelter-foundation.v1"
$unknownWiUnit.worldInteractionIds[0] = "WI-NATURE-UNKNOWN"
Require-Rejected "unknown-wi" $unknownWi "LoopWiUnknown"

$missingPlanningGate = Read-Loops
$missingPlanningGateUnit = $missingPlanningGate.items | Where-Object `
    loopStableId -eq "playable-loop:nature-base-reflection.v1"
$missingPlanningGateUnit.PSObject.Properties.Remove("planningGate")
Require-Rejected "missing-planning-gate" $missingPlanningGate `
    "PlanningGateMissing:playable-loop:nature-base-reflection.v1"

$falseClosure = Read-Loops
$falseClosureUnit = $falseClosure.items | Where-Object `
    loopStableId -eq "playable-loop:nature-night-day2.v1"
$falseClosureUnit.closureStateCode = "PlayClosed"
Require-Rejected "false-dual-play-closure" $falseClosure `
    "DualMaturityPlayClosureBelowGate"

$falseWorldClosure = Read-Loops
$falseWorldClosureUnit = $falseWorldClosure.items | Where-Object `
    loopStableId -eq "playable-loop:nature-survival-homestead.v1"
$falseWorldClosureUnit.currentEvidenceStage = "E8"
$falseWorldClosureUnit.nextClosureTargetStage = "E9"
$falseWorldClosureUnit.closureStateCode = "WorldClosed"
Require-Rejected "false-world-closure" $falseWorldClosure "LoopWorldClosureBelowE9"

$missingInvalidation = Get-Content -LiteralPath $evidencePath -Raw -Encoding UTF8 |
    ConvertFrom-Json
$missingInvalidation.packages[0].invalidationTriggers = @()
$missingInvalidationPath = Join-Path $artifactDirectory "missing-invalidation-evidence.json"
Write-Json $missingInvalidation $missingInvalidationPath
$matchingLoops = Read-Loops
$matchingLoops.evidencePackageCatalogPath = Relative $missingInvalidationPath
Require-Rejected "missing-invalidation-loops" $matchingLoops `
    "EvidenceInvalidationTriggerMissing" (Relative $missingInvalidationPath)

$cycle = Read-Loops
$cycleParent = $cycle.items | Where-Object `
    loopStableId -eq "playable-loop:nature-survival-homestead.v1"
$cycleParent.requiredCoreChildLoopStableIds += $cycleParent.loopStableId
Require-Rejected "parent-cycle" $cycle "LoopHierarchyCycle"

$duplicateOwner = Read-Loops
$townParent = $duplicateOwner.items | Where-Object `
    loopStableId -eq "playable-loop:town-resident-market.v1"
$townParent.requiredCoreChildLoopStableIds += `
    "playable-loop:nature-shelter-foundation.v1"
Require-Rejected "duplicate-unit-parent" $duplicateOwner "LoopUnitHasMultipleParents"

$falseAggregate = Read-Loops
$natureParent = $falseAggregate.items | Where-Object `
    loopStableId -eq "playable-loop:nature-survival-homestead.v1"
$natureParent.currentEvidenceStage = "E5"
$natureParent.nextClosureTargetStage = "E6"
$natureParent.closureStateCode = "CoreClosed"
foreach ($childId in @($natureParent.requiredCoreChildLoopStableIds)) {
    $child = $falseAggregate.items | Where-Object loopStableId -eq $childId
    $child.currentEvidenceStage = "E5"
    $child.nextClosureTargetStage = "E6"
    if ($null -ne $child.PSObject.Properties["maturityTracks"]) {
        $child.maturityTracks.logic.currentStage = "E5"
        $child.maturityTracks.presentation.currentStage = "E5"
    }
    if ($child.closureStateCode -eq "PlayClosed") {
        $child.closureStateCode = "CoreClosed"
        $child.statusCode = "InProgress"
        $child.blockers = @("fixture-incomplete")
    }
    if ($childId -eq "playable-loop:nature-shelter-foundation.v1") {
        $child.closureStateCode = "CoreClosed"
        $child.statusCode = "InProgress"
        $child.blockers = @("fixture-incomplete")
    }
}
Require-Rejected "false-aggregate-closure" $falseAggregate `
    "LoopAggregateCoreClosureInvalid"

$extensionDoesNotBlockCore = Read-Loops
$natureParent = $extensionDoesNotBlockCore.items | Where-Object `
    loopStableId -eq "playable-loop:nature-survival-homestead.v1"
$natureParent.currentEvidenceStage = "E5"
$natureParent.nextClosureTargetStage = "E6"
$natureParent.closureStateCode = "CoreClosed"
foreach ($childId in @($natureParent.requiredCoreChildLoopStableIds)) {
    $child = $extensionDoesNotBlockCore.items | Where-Object loopStableId -eq $childId
    $child.currentEvidenceStage = "E5"
    $child.nextClosureTargetStage = "E6"
    if ($null -ne $child.PSObject.Properties["maturityTracks"]) {
        $child.maturityTracks.logic.currentStage = "E5"
        $child.maturityTracks.presentation.currentStage = "E5"
    }
    $child.closureStateCode = "CoreClosed"
    $child.statusCode = "InProgress"
    $child.blockers = @("fixture-incomplete")
}
$natureExtension = $extensionDoesNotBlockCore.items | Where-Object `
    loopStableId -eq "playable-loop:nature-building-learning.v1"
$natureExtension.currentEvidenceStage = "E1"
$natureExtension.closureStateCode = "Open"
$positivePath = Join-Path $artifactDirectory "extension-does-not-block-core.json"
Write-Json $extensionDoesNotBlockCore $positivePath
$positiveLedgerPath = Join-Path $artifactDirectory "extension-does-not-block-core.md"
& $manager -Mode Write -PlayableLoopPath (Relative $positivePath) `
    -OutputPath (Relative $positiveLedgerPath) | Out-Null

Write-Output `
    "DevelopmentSystemTestsPassed:Loops=23;Evidence=32;GeneratedLedger=1;Negative=8;Positive=1"
