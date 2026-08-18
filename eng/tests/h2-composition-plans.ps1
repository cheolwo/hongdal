$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../.." )).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-h2-composition-plans.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/generated/h2-composition-plans.v1.json"

$write = & pwsh -NoProfile -File $manager -Mode Write
if ($write -notmatch "H2CompositionPlansGenerated:P1=2;P2=2;Nodes=11;Edges=7;Connectors=12") { throw "H2CompositionWriteFailed" }
$beforeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
$secondWrite = & pwsh -NoProfile -File $manager -Mode Write
$afterHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks

if ($check -notmatch "H2CompositionPlansValid:P1=2;P2=2;Nodes=11;Edges=7;Connectors=12") { throw "H2CompositionCheckFailed" }
if ($secondWrite -notmatch "H2CompositionPlansGenerated:P1=2;P2=2;Nodes=11;Edges=7;Connectors=12") { throw "H2CompositionSecondWriteFailed" }
if ($beforeHash -ne $afterHash) { throw "H2CompositionOutputHashChangedWithoutInputChange" }
if ($beforeTicks -ne $afterTicks) { throw "H2CompositionOutputWasRewrittenWithoutInputChange" }

$plans = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
if (@($plans.plans).Count -ne 4) { throw "H2CompositionPlanCountInvalid" }
if (@($plans.plans | Where-Object priorityCode -eq "P1").Count -ne 2) { throw "H2CompositionP1PlanCountInvalid" }
if (@($plans.plans | Where-Object priorityCode -eq "P2").Count -ne 2) { throw "H2CompositionP2PlanCountInvalid" }
if (@($plans.plans.authorityStateCode | Sort-Object -Unique) -ne "DesignCandidateOnly") { throw "H2CompositionAuthorityStateInvalid" }
if (@($plans.plans.evidenceStateCode | Sort-Object -Unique) -ne "WaitingForRoadBoundaryEvidence") { throw "H2CompositionEvidenceStateInvalid" }
if (@($plans.plans.derivationInputHashSha256 | Where-Object { $_ -notmatch '^[0-9a-f]{64}$' }).Count -ne 0) { throw "H2CompositionHashInvalid" }

Write-Output "H2CompositionPlanTestsPassed:P1=2;P2=2;Deterministic=True"
