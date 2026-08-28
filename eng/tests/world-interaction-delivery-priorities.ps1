$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/execution-ledgers/manage-world-interaction-delivery-priorities.ps1"
$ledger = Join-Path $repositoryRoot "eng/execution-ledgers/world-interaction-delivery-priorities.json"
$markdown = Join-Path $repositoryRoot "docs/AI/generated/world-interaction-delivery-priorities.md"
$contract = Join-Path $repositoryRoot "Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationWorldInteractionDeliveryPriorities.generated.cs"
$definition = Get-Content -LiteralPath $ledger -Raw -Encoding UTF8 | ConvertFrom-Json
$expectedActiveWorldInteractionId = [string]$definition.activeWork.worldInteractionId
$expectedActiveEvidenceStage = [string]$definition.activeWork.currentEvidenceStage

$first = & $script -Mode Write
$markdownHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $markdown).Hash
$markdownTicks = (Get-Item -LiteralPath $markdown).LastWriteTimeUtc.Ticks
$contractHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $contract).Hash
$contractTicks = (Get-Item -LiteralPath $contract).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$check = & $script -Mode Check

if ((Get-FileHash -Algorithm SHA256 -LiteralPath $markdown).Hash -ne $markdownHash -or
    (Get-Item -LiteralPath $markdown).LastWriteTimeUtc.Ticks -ne $markdownTicks) {
    throw "WorldInteractionDeliveryPriorityMarkdownIsNotDeterministic"
}
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $contract).Hash -ne $contractHash -or
    (Get-Item -LiteralPath $contract).LastWriteTimeUtc.Ticks -ne $contractTicks) {
    throw "WorldInteractionDeliveryPriorityContractIsNotDeterministic"
}

$catalog = Get-Content -LiteralPath $contract -Raw -Encoding UTF8
if ($catalog -notmatch "public string 실행파동Code" -or
    $catalog -notmatch "public string 목표EvidenceStage" -or
    $catalog -notmatch "public string 개발작업상태Code" -or
    $catalog -notmatch "public string NpcE8정책Code" -or
    $catalog -notmatch ('ActiveWorldInteractionId = "' + [regex]::Escape($expectedActiveWorldInteractionId) + '"') -or
    $catalog -notmatch ('ActiveEvidenceStage = "' + [regex]::Escape($expectedActiveEvidenceStage) + '"') -or
    $catalog -notmatch "WorkInProgressLimit = 1" -or
    $catalog -notmatch "SimulationWI실행우선순위Catalog") {
    throw "WorldInteractionDeliveryPriorityContractShapeInvalid"
}
if ($check -notmatch "WorldInteractionDeliveryPrioritiesValid:65") {
    throw "WorldInteractionDeliveryPriorityValidationDidNotComplete"
}

Write-Output "WorldInteractionDeliveryPriorityTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
