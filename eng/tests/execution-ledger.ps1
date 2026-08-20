$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/execution-ledgers/manage-execution-ledger.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/simulation-unity-execution-tree.md"

$first = & $script -Mode Write
$firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$firstWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$secondWriteTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & $script -Mode Check

if ($firstHash -ne $secondHash) { throw "ExecutionLedgerGenerationIsNotDeterministic" }
if ($firstWriteTicks -ne $secondWriteTicks) { throw "ExecutionLedgerUnchangedOutputWasRewritten" }
$ledger = Get-Content -LiteralPath (
    Join-Path $repositoryRoot "eng/execution-ledgers/simulation-unity.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$stages = Get-Content -LiteralPath (
    Join-Path $repositoryRoot ([string] $ledger.evidenceStageCatalogPath)) -Raw -Encoding UTF8 |
    ConvertFrom-Json
$expectedCount = @($ledger.items).Count
if ((@($stages.stages.code) -join ",") -ne "E0,E1,E2,E3,E4,E5,E6,E7") {
    throw "ExecutionLedgerEvidenceStageOrderInvalid"
}
$e6 = @($stages.stages | Where-Object code -eq "E6")
if ($e6.Count -ne 1 -or
    [string] $e6[0].label -ne "AreaSet 정밀 몰입·현실 근거 결속" -or
    [string] $e6[0].completionGate -notmatch "DEM·도로를 공통 필수 자료로 간주하지 않는다") {
    throw "ExecutionLedgerE6AreaSetImmersionPolicyInvalid"
}
if (@($ledger.items | Where-Object { $_.targetEvidenceStage -ne "E7" }).Count -ne 0) {
    throw "ExecutionLedgerTargetMustBeE7"
}
if (@($ledger.items | Where-Object { $_.currentEvidenceStage -in @("E4", "E5", "E6", "E7") }).Count -ne 0) {
    throw "ExecutionLedgerLegacyE4E5MustBeReassessed"
}
if ($check -notmatch "ExecutionLedgerValid:$expectedCount") {
    throw "ExecutionLedgerValidationDidNotComplete"
}

Write-Output "ExecutionLedgerTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
