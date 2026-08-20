$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../.." )).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-h2-human-review-queue.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/h2-human-review-queue.md"

$write = & pwsh -NoProfile -File $manager -Mode Write
if ($write -notmatch "H2HumanReviewQueueGenerated:Items=6;Nature=2;Farm=2;Town=2") { throw "H2HumanReviewQueueWriteFailed" }
$beforeHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
if ($check -notmatch "H2HumanReviewQueueValid:Items=6;Nature=2;Farm=2;Town=2") { throw "H2HumanReviewQueueCheckFailed" }
$afterHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
if ($beforeHash -ne $afterHash -or $beforeTicks -ne $afterTicks) { throw "H2HumanReviewQueueNonDeterministic" }

$queue = Get-Content -LiteralPath (Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/h2-human-review-queue.v1.json") -Raw -Encoding UTF8 | ConvertFrom-Json
if ((@($queue.items.h2Ref) -join ",") -ne "h2-candidate:nature-threat-response,h2-candidate:nature-restoration-recovery,h2-candidate:farm-incident-containment,h2-candidate:farm-loss-restoration-handoff,h2-candidate:town-contamination-control,h2-candidate:town-recall-relief") {
    throw "H2HumanReviewQueueOrderInvalid"
}
if (@($queue.items | Where-Object reviewStateCode -ne "AwaitingHumanReview").Count -ne 0) { throw "H2HumanReviewQueueStateInvalid" }

Write-Output "H2HumanReviewQueueTestsPassed:Items=6;PriorityOrder=Nature-Farm-Town"
