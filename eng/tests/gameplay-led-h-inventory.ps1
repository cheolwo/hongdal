$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-gameplay-led-h-inventory.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/generated/gameplay-led-h-inventory.v1.json"

$beforeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
$write = & pwsh -NoProfile -File $manager -Mode Write
$afterHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
if ($check -notmatch "GameplayLedHInventoryValid:Plans=4;H1=52\+32;H2=24;H3=13;H4=6;Violations=0") { throw "GameplayLedHInventoryCheckFailed" }
if ($write -notmatch "GameplayLedHInventoryGenerated:Plans=4;H1=52\+32;H2=24;H3=13;H4=6;Violations=0") { throw "GameplayLedHInventoryWriteFailed" }
if ($beforeHash -ne $afterHash -or $beforeTicks -ne $afterTicks) { throw "GameplayLedHInventoryNonDeterministic" }

$report = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
if ([int] $report.counts.violations -ne 0) { throw "GameplayLedHInventoryViolationFound" }
if ([int] $report.counts.quarantinedExpressionH1 -ne 9) { throw "GameplayLedHInventoryQuarantineCountInvalid" }
if ((@($report.planCoverage.gamePlanCode) -join ",") -ne "NatureHomeThreatRecovery,FarmProductionSurvival,TownLivingMarketSafety,CityHubLogisticsResilience") { throw "GameplayLedHInventoryPlanOrderInvalid" }
$town = @($report.planCoverage | Where-Object gamePlanCode -eq "TownLivingMarketSafety")
if (@($town.coverage.h1InteractionRefs) -notcontains "h1-stock:town-order-packing") { throw "GameplayLedHInventoryOrderPackingMissing" }
if ((@($report.hExpansionQueue.priorityCode) -join ",") -ne "H-P0,H-P1,H-P2,H-P3,H-P4") { throw "GameplayLedHInventoryHPriorityInvalid" }
if ((@($report.wiEvidenceQueue.priorityCode) -join ",") -ne "E-P1,E-P2,E-P3,E-P4,E-P5") { throw "GameplayLedHInventoryEPriorityInvalid" }
if ([string] $report.wiEvidenceQueue[-1].targetStageCode -ne "E6") { throw "GameplayLedHInventoryE6BoundaryInvalid" }

Write-Output "GameplayLedHInventoryTestsPassed:Plans=4;Violations=0;OrderPacking=Covered"
