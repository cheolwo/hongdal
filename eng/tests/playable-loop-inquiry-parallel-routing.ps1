$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager = Join-Path $root 'eng/execution-ledgers/manage-playable-loop-inquiry-implementation-scope.ps1'
$artifactRef = 'artifacts/local/validation/inquiry-parallel-routing'
$artifacts = Join-Path $root $artifactRef
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
function Save-Fixture([string] $Ref, [object] $Value) { [IO.File]::WriteAllText((Join-Path $root $Ref), ($Value | ConvertTo-Json -Depth 100), $utf8) }
$catalog = Get-Content (Join-Path $root 'eng/execution-ledgers/playable-loop-inquiry-implementation-scope.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$goals = Get-Content (Join-Path $root 'eng/execution-ledgers/codex-playable-loop-goals.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$goals.schemaVersion = 'codex-playable-loop-goals.v4'
$items = @()
foreach ($spec in @(
    @{ id='plan'; wi='WI-ACTOR-PLAN-SET'; order='nature-personal-plan'; doc='Nature개인계획E3'; question='Q-040' },
    @{ id='heat'; wi='WI-HEAT-SOURCE-STATE-CHANGE'; order='nature-heat-source'; doc='Nature열원관리E3'; question='Q-032' }
)) {
    $order = Get-Content (Join-Path $root "eng/execution-ledgers/work-orders/$($spec.order).e7-work-order.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    # Explicit synthetic approval at E2, never production approval.
    $order.trackPlans.logic.currentEvidenceStage = 'E1'
    foreach ($stage in $order.trackPlans.logic.upwardValidation) { if ($stage.code -ne 'E1') { $stage.status = 'Blocked' } }
    ($order.trackPlans.logic.downwardPlan | Where-Object code -eq E2).disposition = 'Affected'
    $order.approvedQuestionScope = @($spec.question)
    $orderRef = "$artifactRef/$($spec.id)-order.json"
    Save-Fixture $orderRef $order
    $docRef = "docs/Architecture/PlayableLoops/$($spec.doc).md"
    $orderHash = (Get-FileHash (Join-Path $root $orderRef)).Hash
    $items += [pscustomobject]@{
        workItemId="fixture:$($spec.id)"; loopStableId=$order.playableUnitStableId; worldInteractionId=$spec.wi
        trackCode='Logic'; statusCode='Active'; ownerThreadId="fixture:$($spec.id)"; worktreePath=$root
        writePaths=@("fixture/$($spec.id)"); sharedContractKeys=@(); dependsOnWorkItemIds=@()
        baselineFiles=@(@{path=$orderRef;sha256=$orderHash}); workOrderRef=$orderRef;workOrderSha256=$orderHash;targetEvidenceStageCode='E2'
        planningGate=@{statusCode='Approved';designDocumentRef=$docRef;designRevision=$order.integratedGate.candidateRevision;designHashSha256=(Get-FileHash (Join-Path $root $docRef)).Hash;approvalEvidenceRef='fixture:explicit-approval'}
    }
}
$goals | Add-Member -NotePropertyName workItems -NotePropertyValue $items -Force
$goalsRef = "$artifactRef/goals.json"
$catalog.codexGoalCatalogPath = $goalsRef
foreach ($questionId in @('Q-032','Q-033','Q-040')) {
    if ($null -eq $catalog.questionOverrides.PSObject.Properties[$questionId]) {
        $catalog.questionOverrides | Add-Member -NotePropertyName $questionId -NotePropertyValue ([pscustomobject]@{checks=[pscustomobject]@{designBinding='Incorporated';implementation='Partial'}})
    }
    $override = $catalog.questionOverrides.$questionId
    $override | Add-Member -NotePropertyName decisionStatusCode -NotePropertyValue 'Confirmed' -Force
    $override | Add-Member -NotePropertyName blockerCodes -NotePropertyValue @() -Force
    $override.checks.designBinding = 'Incorporated'
    $override.checks.implementation = 'Partial'
}
$catalogRef = "$artifactRef/catalog.json"
Save-Fixture $catalogRef $catalog
function Run-Projection([string] $Name) {
    Save-Fixture $goalsRef $goals
    $outputRef = "$artifactRef/$Name.md"
    $null = & $manager -Mode Write -CatalogPath $catalogRef -OutputPath $outputRef
    Get-Content (Join-Path $root $outputRef) -Raw -Encoding UTF8
}
$both = Run-Projection 'both-approved'
foreach ($wi in @('WI-ACTOR-PLAN-SET','WI-HEAT-SOURCE-STATE-CHANGE')) {
    if ($both -notmatch ('(?m)^\| `' + [regex]::Escape($wi) + '` .*ApprovedWorkItemExecutable')) { throw "IndependentApprovedWorkNotExecutable:$wi" }
}
if ($both -notmatch 'ReadyToDispatch: `2`') { throw 'OwnQuestionScopeNotHonored' }
$items[1].statusCode = 'Blocked'
$items[1].planningGate.statusCode = 'Draft'
$blocked = Run-Projection 'nonfocus-unapproved'
if ($blocked -notmatch '(?m)^\| `WI-HEAT-SOURCE-STATE-CHANGE` .*WorkItemBlocked') { throw 'UnapprovedNonfocusMustNotBecomeReady' }
if ($blocked -notmatch '(?m)^\| `WI-ACTOR-PLAN-SET` .*ApprovedWorkItemExecutable') { throw 'BlockedNonfocusMustNotBlockIndependentWork' }
if ($blocked -notmatch 'ReadyToDispatch: `1`') { throw 'UnapprovedQuestionMustNotDispatch' }
if ($blocked.Contains('ActiveWorldInteractionWipOwnedBy')) { throw 'GlobalWipOwnershipBlockerReturned' }
Write-Output 'InquiryParallelRoutingTestsPassed:ApprovedParallel=2;UnapprovedIsolated=1;QuestionScope=Verified'
