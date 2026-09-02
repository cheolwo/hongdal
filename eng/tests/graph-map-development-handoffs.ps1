[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$repositoryRoot = Split-Path -Parent $root
$manager = Join-Path $root 'world-seedbeds/manage-graph-map-development-handoffs.ps1'
$officialLedgerPath = Join-Path $root 'world-seedbeds/graph-map-development-handoffs.json'
$utf8 = [Text.UTF8Encoding]::new($false)
$testId = [Guid]::NewGuid().ToString('N')
$folderRef = "artifacts/local/validation/graph-map-development-handoffs/$testId"
$folder = Join-Path $repositoryRoot ($folderRef -replace '/', [IO.Path]::DirectorySeparatorChar)
$null = New-Item -ItemType Directory -Force -Path $folder
$passed = 0

function Assert([bool] $condition, [string] $code) {
    if (-not $condition) { throw "GraphMapDevelopmentHandoffTestFailed:$code" }
    $script:passed++
}

function Clone-Ledger {
    return (Get-Content -LiteralPath $officialLedgerPath -Raw -Encoding UTF8 | ConvertFrom-Json | ConvertTo-Json -Depth 100 | ConvertFrom-Json)
}

function Save-Ledger([object] $ledger, [string] $name) {
    $relative = "$folderRef/$name.ledger.json"
    $path = Join-Path $repositoryRoot ($relative -replace '/', [IO.Path]::DirectorySeparatorChar)
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
    'eng/world-seedbeds/graph-map-development-handoffs.json',
    'eng/world-seedbeds/graph-map-planning-handoffs.json',
    'eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json',
    'eng/world-seedbeds/generated/graph-map-plans.v1.json',
    'eng/execution-ledgers/codex-playable-loop-goals.json',
    'eng/execution-ledgers/world-interactions.json',
    'eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json',
    'docs/Architecture/PlayableLoops/Farm경작세계발현E5.md'
)
$before = @{}
foreach ($relative in $protected) {
    $before[$relative] = (Get-FileHash -LiteralPath (Join-Path $repositoryRoot ($relative -replace '/', [IO.Path]::DirectorySeparatorChar))).Hash
}

$official = & $manager -Mode Check
Assert ($official -match 'Check passed: items=2, ready=1, integrated=1, blocked=0') 'OfficialCheck'

$state = Clone-Ledger
$validRef = Save-Ledger $state 'valid'
$valid = Invoke-Fixture $validRef 'valid'
Assert ($valid -match 'Write passed') 'ValidFixture'
$output = Get-Content -LiteralPath (Join-Path $folder 'valid.output.json') -Raw -Encoding UTF8 | ConvertFrom-Json
Assert ([string] $output.schemaVersion -eq 'mirror-graph-map-development-handoff-output.v1') 'OutputSchema'
Assert ($output.counts.total -eq 2 -and $output.counts.readyForDevelopment -eq 1 -and $output.counts.integrated -eq 1) 'OutputCounts'
Assert ([string] $output.items[0].source.graphMapRevision -eq 'mirror-graph-map-plan.northern-life-hub-discovery.r10') 'GraphMapRevisionOutput'
Assert (@($output.items[0].slice.nodeRefs).Count -eq 2 -and @($output.items[0].slice.edgeRefs).Count -eq 1) 'SliceCounts'
Assert (@($output.items[0].slice.placementRuleRefs).Count -eq 1) 'PlacementRuleSliceCount'
Assert ([string] $output.items[0].developmentTarget.candidateStatusCode -eq 'Active') 'CandidateStatusOutput'
Assert (-not [bool] $output.items[0].evidenceBoundary.codeChanged -and -not [bool] $output.items[0].evidenceBoundary.runtimeExecuted) 'EvidenceBoundaryOutput'

$second = Invoke-Fixture $validRef 'valid-second'
Assert ($second -match 'Write passed') 'SecondWrite'
Assert ((Get-FileHash (Join-Path $folder 'valid.output.json')).Hash -eq (Get-FileHash (Join-Path $folder 'valid-second.output.json')).Hash) 'DeterministicJson'
Assert ((Get-FileHash (Join-Path $folder 'valid.output.md')).Hash -eq (Get-FileHash (Join-Path $folder 'valid-second.output.md')).Hash) 'DeterministicMarkdown'

$state = Clone-Ledger
$state.items += @($state.items[0])
Reject { Invoke-Fixture (Save-Ledger $state 'duplicate-id') 'duplicate-id' } 'HandoffDuplicate'

$state = Clone-Ledger
$state.items[0].source.planningHandoffId = 'graph-map-handoff:missing'
Reject { Invoke-Fixture (Save-Ledger $state 'planning-handoff') 'planning-handoff' } 'PlanningHandoffIdentity'

$state = Clone-Ledger
$state.items[0].source.graphMapPlanExpectedSha256 = ('0' * 64)
Reject { Invoke-Fixture (Save-Ledger $state 'plan-hash') 'plan-hash' } 'GraphMapPlan:graph-map-development-handoff:farm-production-work-yard:r1:HashMismatch'

$state = Clone-Ledger
$state.items[0].source.graphMapRevision = 'mirror-graph-map-plan.invalid'
Reject { Invoke-Fixture (Save-Ledger $state 'plan-revision') 'plan-revision' } 'GraphMapRevision'

$state = Clone-Ledger
$state.items[0].source.graphMapOutputExpectedSha256 = ('F' * 64)
Reject { Invoke-Fixture (Save-Ledger $state 'output-hash') 'output-hash' } 'GraphMapOutput:graph-map-development-handoff:farm-production-work-yard:r1:HashMismatch'

$state = Clone-Ledger
$state.items[0].slice.nodeRefs[0] = 'gm-node:missing'
Reject { Invoke-Fixture (Save-Ledger $state 'node-unknown') 'node-unknown' } 'SliceNodeUnknown'

$state = Clone-Ledger
$state.items[0].slice.nodeRefs[0] = 'gm-node:yodong-defense-gateway'
Reject { Invoke-Fixture (Save-Ledger $state 'node-unresolved') 'node-unresolved' } 'SliceNodeUnresolved'

$state = Clone-Ledger
$state.items[0].slice.edgeRefs[0] = 'gm-edge:farm-work-yard-to-loading-gate'
Reject { Invoke-Fixture (Save-Ledger $state 'edge-outside') 'edge-outside' } 'SliceEdgeToOutside'

$state = Clone-Ledger
$state.items[0].slice.constraintRefs[0] = 'gm-constraint:missing'
Reject { Invoke-Fixture (Save-Ledger $state 'constraint-unknown') 'constraint-unknown' } 'SliceConstraintUnknown'

$state = Clone-Ledger
$state.items[0].slice.codeBindingRefs[0] = 'gm-code:missing'
Reject { Invoke-Fixture (Save-Ledger $state 'binding-unknown') 'binding-unknown' } 'SliceCodeBindingUnknown'

$state = Clone-Ledger
$state.items[0].slice.placementRuleRefs[0] = 'gm-placement-rule:missing'
Reject { Invoke-Fixture (Save-Ledger $state 'placement-rule-unknown') 'placement-rule-unknown' } 'SlicePlacementRuleUnknown'

$state = Clone-Ledger
$state.items[0].slice.placementRuleRefs[0] = 'gm-placement-rule:hub:vehicle-turning-radius'
Reject { Invoke-Fixture (Save-Ledger $state 'placement-rule-outside') 'placement-rule-outside' } 'SlicePlacementRuleOutsideConstraints'

$state = Clone-Ledger
$state.items[0].developmentTarget.worldInteractionId = 'WI-NOT-REGISTERED'
Reject { Invoke-Fixture (Save-Ledger $state 'wi-unknown') 'wi-unknown' } 'WorldInteractionUnknown'

$state = Clone-Ledger
$state.items[0].developmentTarget.worldInteractionId = 'WI-HUB-03'
Reject { Invoke-Fixture (Save-Ledger $state 'wi-outside') 'wi-outside' } 'WorldInteractionNotInSlice'

$state = Clone-Ledger
$state.items[0].developmentTarget.expectedGoalCatalogRevision = 'codex-playable-loop-goals.invalid'
Reject { Invoke-Fixture (Save-Ledger $state 'goal-revision') 'goal-revision' } 'GoalCatalogRevision'

$state = Clone-Ledger
$state.items[0].developmentTarget.candidateWorkItemId = 'work:missing'
Reject { Invoke-Fixture (Save-Ledger $state 'workitem-unknown') 'workitem-unknown' } 'CandidateWorkItemIdentity'

$state = Clone-Ledger
$state.items[0].developmentTarget.candidateExpectedStatusCode = 'Integrated'
Reject { Invoke-Fixture (Save-Ledger $state 'workitem-status') 'workitem-status' } 'CandidateStatusMismatch'

$state = Clone-Ledger
$state.items[0].developmentTarget.workOrderExpectedSha256 = ('A' * 64)
Reject { Invoke-Fixture (Save-Ledger $state 'workorder-hash') 'workorder-hash' } 'CandidateWorkOrderHashMismatch'

$state = Clone-Ledger
$state.items[0].developmentTarget.planningDesignExpectedSha256 = ('B' * 64)
Reject { Invoke-Fixture (Save-Ledger $state 'design-hash') 'design-hash' } 'WorkOrderDesignHash'

$state = Clone-Ledger
$state.items[0].readiness.developmentAccepted = $true
Reject { Invoke-Fixture (Save-Ledger $state 'ready-accepted') 'ready-accepted' } 'ReadyAlreadyAccepted'

$state = Clone-Ledger
$state.items[0].readiness.blockerItems = @('Conflict')
Reject { Invoke-Fixture (Save-Ledger $state 'ready-blocker') 'ready-blocker' } 'ReadyHasBlockers'

$state = Clone-Ledger
$state.items[0].evidenceBoundary.testsExecuted = $true
Reject { Invoke-Fixture (Save-Ledger $state 'ready-evidence') 'ready-evidence' } 'ReadyEvidenceRaised'

$state = Clone-Ledger
$state.ownershipBoundary.automaticGoalActivation = $true
Reject { Invoke-Fixture (Save-Ledger $state 'auto-goal') 'auto-goal' } 'AutomaticGoalActivation'

$state = Clone-Ledger
$state.items[0].acceptanceContract.sceneSaveAllowed = $true
Reject { Invoke-Fixture (Save-Ledger $state 'scene-save') 'scene-save' } 'SceneSaveAllowed'

foreach ($relative in $protected) {
    $after = (Get-FileHash -LiteralPath (Join-Path $repositoryRoot ($relative -replace '/', [IO.Path]::DirectorySeparatorChar))).Hash
    Assert ($after -eq $before[$relative]) "ProtectedChanged:$relative"
}

Write-Output "Graph Map development handoff tests passed: $passed"
