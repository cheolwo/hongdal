[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$manager = Join-Path $root 'world-seedbeds/manage-graph-map-planning-handoffs.ps1'
$officialLedgerPath = Join-Path $root 'world-seedbeds/graph-map-planning-handoffs.json'
$utf8 = [Text.UTF8Encoding]::new($false)
$testId = [Guid]::NewGuid().ToString('N')
$folderRef = "artifacts/local/validation/graph-map-planning-handoffs/$testId"
$folder = Join-Path (Split-Path -Parent $root) ($folderRef -replace '/', [IO.Path]::DirectorySeparatorChar)
$null = New-Item -ItemType Directory -Force -Path $folder
$passed = 0

function Assert([bool] $condition, [string] $code) {
    if (-not $condition) { throw "GraphMapPlanningHandoffTestFailed:$code" }
    $script:passed++
}

function Clone-Ledger {
    return (Get-Content -LiteralPath $officialLedgerPath -Raw -Encoding UTF8 | ConvertFrom-Json | ConvertTo-Json -Depth 100 | ConvertFrom-Json)
}

function Save-Ledger([object] $ledger, [string] $name) {
    $relative = "$folderRef/$name.ledger.json"
    $path = Join-Path (Split-Path -Parent $root) ($relative -replace '/', [IO.Path]::DirectorySeparatorChar)
    [IO.File]::WriteAllText($path, (($ledger | ConvertTo-Json -Depth 100) + "`n"), $utf8)
    return $relative
}

function Invoke-Fixture([string] $ledgerRef, [string] $name) {
    return & $manager -Mode Write -LedgerPath $ledgerRef -JsonOutputPath "$folderRef/$name.output.json" -MarkdownOutputPath "$folderRef/$name.output.md"
}

function Reject([scriptblock] $action, [string] $expectedCode) {
    $message = ''
    try { & $action | Out-Null }
    catch { $message = $_.Exception.Message }
    Assert (-not [string]::IsNullOrWhiteSpace($message)) "RejectDidNotFail:$expectedCode"
    Assert ($message -match [regex]::Escape($expectedCode)) "RejectWrongReason:$expectedCode`:$message"
}

$protected = @(
    'eng/world-seedbeds/graph-map-planning-handoffs.json',
    'docs/AI/북부생활권-첫그래프맵-상세제안-2026-09-01.md',
    'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json',
    'eng/world-seedbeds/generated/graph-map-plans.v1.json',
    'docs/Reports/그래프맵-프로젝트재검토와첫구현-2026-09-01.md',
    'docs/AI/DECISIONS.md',
    'eng/execution-ledgers/world-interactions.json'
)
$before = @{}
foreach ($relative in $protected) {
    $before[$relative] = (Get-FileHash -LiteralPath (Join-Path (Split-Path -Parent $root) ($relative -replace '/', [IO.Path]::DirectorySeparatorChar))).Hash
}

$officialCheck = & $manager -Mode Check
Assert ($officialCheck -match 'Check passed: items=7, integrated=1, blocked=0, noImpact=0') 'OfficialCheck'

$state = Clone-Ledger
$validRef = Save-Ledger $state 'valid'
$valid = Invoke-Fixture $validRef 'valid'
Assert ($valid -match 'Write passed') 'ValidFixture'
$output = Get-Content -LiteralPath (Join-Path $folder 'valid.output.json') -Raw -Encoding UTF8 | ConvertFrom-Json
Assert ([string] $output.schemaVersion -eq 'mirror-graph-map-planning-handoff-output.v1') 'OutputSchema'
Assert ($output.counts.total -eq 7 -and $output.counts.integrated -eq 1 -and $output.counts.superseded -eq 6) 'OutputCounts'
Assert ([string] $output.items[6].planningSource.revisionCode -eq 'planning-index.current-2026-09-02') 'PlanningRevisionOutput'
Assert ([string] $output.items[6].graphMapResult.revision -eq 'mirror-graph-map-plan.northern-life-hub-discovery.r10') 'GraphMapRevisionOutput'
Assert (-not [bool] $output.items[6].evidenceBoundary.unitySceneChanged -and -not [bool] $output.items[6].evidenceBoundary.evidencePromoted) 'EvidenceBoundaryOutput'

$second = Invoke-Fixture $validRef 'valid-second'
Assert ($second -match 'Write passed') 'SecondWrite'
Assert ((Get-FileHash (Join-Path $folder 'valid.output.json')).Hash -eq (Get-FileHash (Join-Path $folder 'valid-second.output.json')).Hash) 'DeterministicJson'
Assert ((Get-FileHash (Join-Path $folder 'valid.output.md')).Hash -eq (Get-FileHash (Join-Path $folder 'valid-second.output.md')).Hash) 'DeterministicMarkdown'

$state = Clone-Ledger
$state.items += @($state.items[6])
Reject { Invoke-Fixture (Save-Ledger $state 'duplicate-id') 'duplicate-id' } 'HandoffDuplicate'

$state = Clone-Ledger
$state.items[6].planningSource.expectedSha256 = ('0' * 64)
Reject { Invoke-Fixture (Save-Ledger $state 'stale-source') 'stale-source' } 'PlanningSource:graph-map-handoff:northern-life-hub-discovery:r10:HashMismatch'

$state = Clone-Ledger
$state.items[1].planningSource.expectedSha256 = ('0' * 64)
$historical = Invoke-Fixture (Save-Ledger $state 'historical-source-hash') 'historical-source-hash'
Assert ($historical -match 'Write passed') 'SupersededHistoricalHashNotRevalidated'

$state = Clone-Ledger
$state.items[2].planningSource.sourceDecisionIds[0] = 'D-000'
Reject { Invoke-Fixture (Save-Ledger $state 'unknown-decision') 'unknown-decision' } 'DecisionUnknown'

$state = Clone-Ledger
$state.items[2].planningSource.sourceWorldInteractionIds[0] = 'WI-NOT-REGISTERED'
Reject { Invoke-Fixture (Save-Ledger $state 'unknown-wi') 'unknown-wi' } 'WorldInteractionUnknown'

$state = Clone-Ledger
$state.items[6].planningSource.contextRefs.PSObject.Properties.Remove('result')
Reject { Invoke-Fixture (Save-Ledger $state 'missing-context') 'missing-context' } 'ContextMissing'

$state = Clone-Ledger
$state.items[6].request.requestedLevelCodes[0] = 'Level9'
Reject { Invoke-Fixture (Save-Ledger $state 'unknown-level') 'unknown-level' } 'RequestedLevelUnknown'

$state = Clone-Ledger
$state.items[6].request.targetGraphMapRevision = 'mirror-graph-map-plan.northern-life-hub-discovery.r999'
Reject { Invoke-Fixture (Save-Ledger $state 'target-revision') 'target-revision' } 'TargetRevisionMismatch'

$state = Clone-Ledger
$state.items[6].result.graphMapPlanExpectedSha256 = ('F' * 64)
Reject { Invoke-Fixture (Save-Ledger $state 'plan-hash') 'plan-hash' } 'GraphMapPlan:graph-map-handoff:northern-life-hub-discovery:r10:HashMismatch'

$state = Clone-Ledger
$state.items[6].result.mappedRefs = @()
Reject { Invoke-Fixture (Save-Ledger $state 'integrated-empty') 'integrated-empty' } 'IntegratedMappedRefsEmpty'

$state = Clone-Ledger
$state.items[6].statusCode = 'Blocked'
$state.items[6].returnToPlanning.terminalResultCode = 'Blocked'
$state.items[6].result.blockerItems = @()
Reject { Invoke-Fixture (Save-Ledger $state 'blocked-empty') 'blocked-empty' } 'BlockedWithoutBlocker'

$state = Clone-Ledger
$state.items[6].statusCode = 'NoImpact'
$state.items[6].impactCode = 'NoImpact'
$state.items[6].returnToPlanning.terminalResultCode = 'NoImpact'
Reject { Invoke-Fixture (Save-Ledger $state 'no-impact-levels') 'no-impact-levels' } 'NoImpactHasLevels'

$state = Clone-Ledger
$state.items[6].returnToPlanning.terminalResultCode = 'Blocked'
Reject { Invoke-Fixture (Save-Ledger $state 'return-status') 'return-status' } 'ReturnStatusMismatch'

$state = Clone-Ledger
$state.items[6].evidenceBoundary.gameViewCaptured = $true
Reject { Invoke-Fixture (Save-Ledger $state 'evidence-raised') 'evidence-raised' } 'EvidenceBoundaryRaised'

foreach ($relative in $protected) {
    $after = (Get-FileHash -LiteralPath (Join-Path (Split-Path -Parent $root) ($relative -replace '/', [IO.Path]::DirectorySeparatorChar))).Hash
    Assert ($after -eq $before[$relative]) "ProtectedChanged:$relative"
}

Write-Output "Graph Map planning handoff tests passed: $passed"
