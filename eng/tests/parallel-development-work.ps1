$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $root 'eng/common/parallel-development-work.ps1')
$fixtureRoot = Join-Path $root 'artifacts/local/validation/parallel-development-work'
New-Item -ItemType Directory -Force -Path $fixtureRoot | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
function Save-Fixture([string] $Name, [object] $Value) {
    $relative = "artifacts/local/validation/parallel-development-work/$Name"
    [IO.File]::WriteAllText((Join-Path $root $relative), ($Value | ConvertTo-Json -Depth 40), $utf8)
    return $relative
}
function Clone($Value) { return ($Value | ConvertTo-Json -Depth 40 | ConvertFrom-Json) }
$designRef = Save-Fixture 'approved-design.json' @{ meaning = 'test-only approved design' }
$designHash = (Get-FileHash (Join-Path $root $designRef) -Algorithm SHA256).Hash
$loops = [pscustomobject]@{items=@(
    [pscustomobject]@{loopStableId='loop:a';loopLevelCode='PlayableUnit';worldInteractionIds=@('WI-A','WI-B')},
    [pscustomobject]@{loopStableId='loop:b';loopLevelCode='PlayableUnit';worldInteractionIds=@('WI-C')}
)}
function New-Work([string] $Id, [string] $Loop, [string] $Wi) {
    $order = @{
        schemaVersion='simulation-e7-vertical-work-order.v2'; playableUnitStableId=$Loop;activeWorldInteractionId=$Wi
        deliveryCap=@{currentDispatchTargetStage='E3';promotionBeyondStageAllowed=$false}
        integratedGate=@{candidateRevision='test.r1'}
        pipelineValidation=@{profileKey="$Loop|$Wi"}
        trackPlans=@{
            Logic=@{currentEvidenceStage='E1';downwardPlan=@(@{code='E3';disposition='Affected'})}
            Presentation=@{currentEvidenceStage='E1';downwardPlan=@(@{code='E1';disposition='Affected'},@{code='E2';disposition='Blocked'})}
        }
    }
    $orderRef = Save-Fixture "$Id.order.json" $order
    return [pscustomobject]@{
        workItemId=$Id;loopStableId=$Loop;worldInteractionId=$Wi;trackCode='Logic';statusCode='Active'
        ownerThreadId='same-owner-is-allowed';worktreePath=$root;writePaths=@("test-fixture/$Id");sharedContractKeys=@()
        dependsOnWorkItemIds=@();baselineFiles=@(@{path=$designRef;sha256=$designHash})
        workOrderRef=$orderRef;workOrderSha256=(Get-FileHash (Join-Path $root $orderRef)).Hash
        targetEvidenceStageCode='E3';planningGate=@{
            statusCode='Approved';designDocumentRef=$designRef;designRevision='test.r1';designHashSha256=$designHash;approvalEvidenceRef='fixture:approval'
        }
    }
}
$a=New-Work 'a' 'loop:a' 'WI-A'; $b=New-Work 'b' 'loop:a' 'WI-B'; $c=New-Work 'c' 'loop:b' 'WI-C'
$baseline=[pscustomobject]@{
    schemaVersion='codex-playable-loop-goals.v4';workItems=@($a,$b,$c)
    policy=@{integrationOwnerThreadId='integrator'}
    items=@(
        [pscustomobject]@{loopStableId='loop:a';goalStateCode='Active';activationPrerequisiteLoopStableIds=@()},
        [pscustomobject]@{loopStableId='loop:b';goalStateCode='Active';activationPrerequisiteLoopStableIds=@()}
    )
}
$script:passed=0
function Check([string] $Name, [scriptblock] $Action) { & $Action; $script:passed++; Write-Output "Passed:$Name" }
function Results($Ledger) { return @(Test-ParallelDevelopmentWorkItems -Ledger $Ledger -Loops $loops -RepositoryRoot $root) }
function Reject([object] $Ledger, [string] $Code) {
    try { Results $Ledger | Out-Null } catch { if ($_.Exception.Message.Contains($Code)) { return }; throw }
    throw "ExpectedRejection:$Code"
}
function Blocked([object] $Ledger, [string] $Id, [string] $Code) {
    $result=@(Results $Ledger | Where-Object workItemId -eq $Id)[0]
    if ($result.canExecute -or -not (@($result.blockerCodes) -match [regex]::Escape($Code))) { throw "MissingBlocker:$Id/$Code" }
}
Check 'two-goals-three-wis-same-owner' { if (@(Results $baseline | Where-Object canExecute).Count -ne 3) { throw 'ParallelWorkRejected' } }
Check 'same-wi-separate-files' { $x=Clone $baseline;$x.workItems[1]=Clone $a;$x.workItems[1].workItemId='a-other';$x.workItems[1].writePaths=@('test-fixture/other');if (@(Results $x | Where-Object canExecute).Count -ne 3) {throw 'SameWiRejected'} }
Check 'same-wi-logic-and-presentation' { $x=Clone $baseline;$x.workItems[1]=Clone $a;$x.workItems[1].workItemId='a-view';$x.workItems[1].writePaths=@('test-fixture/view');$x.workItems[1].trackCode='Presentation';$x.workItems[1].targetEvidenceStageCode='E1';if (@(Results $x | Where-Object canExecute).Count -ne 3) {throw 'TracksRejected'} }
Check 'blocked-work-does-not-stop-others' { $x=Clone $baseline;$x.workItems[0].statusCode='Blocked';$x.workItems[0].planningGate.statusCode='Draft';$r=Results $x;if (@($r | Where-Object canExecute).Count -ne 2) {throw 'BlockerLeaked'} }
Check 'dependency-blocks-only-dependent' { $x=Clone $baseline;$x.workItems[1].statusCode='Blocked';$x.workItems[1].dependsOnWorkItemIds=@('a');Blocked $x 'b' 'DependencyNotIntegrated';if (@(Results $x | Where-Object canExecute).Count -ne 2) {throw 'DependencyLeaked'} }
Check 'integrated-dependency-needs-receipt' { $x=Clone $baseline;$x.workItems[0].statusCode='Integrated';$x.workItems[1].dependsOnWorkItemIds=@('a');Blocked $x 'b' 'DependencyNotIntegrated' }
Check 'integrated-dependency-unblocks-with-receipt' {
    $x=Clone $baseline;$x.workItems[0].statusCode='Integrated';$x.workItems[1].dependsOnWorkItemIds=@('a')
    $receiptRef=Save-Fixture 'integration-receipt.json' @{statusCode='Accepted';workItemId='a';loopStableId='loop:a';worldInteractionId='WI-A';workOrderSha256=$a.workOrderSha256;targetEvidenceStageCode='E3';acceptedByThreadId='integrator';acceptedAt='2026-08-30';artifactRefs=@(@{path=$designRef;sha256=$designHash})}
    $x.workItems[0] | Add-Member integrationReceiptRef $receiptRef
    $x.workItems[0] | Add-Member integrationReceiptSha256 (Get-FileHash (Join-Path $root $receiptRef)).Hash
    if (@(Results $x | Where-Object canExecute).Count -ne 2) {throw 'IntegratedDependencyBlocked'}
}
Check 'multiple-integration-ready' { $x=Clone $baseline;$x.workItems[0].statusCode='ReadyForIntegration';$x.workItems[1].statusCode='ReadyForIntegration';if (@(Results $x | Where-Object canExecute).Count -ne 3) {throw 'IntegrationCountRestricted'} }
Check 'duplicate-work' { $x=Clone $baseline;$x.workItems+=Clone $a;Reject $x 'DuplicateWorkItem' }
Check 'same-file-other-worktree' { $x=Clone $baseline;$x.workItems[1].writePaths=$x.workItems[0].writePaths;$x.workItems[1].worktreePath='C:/different';Reject $x 'WriteOwnershipConflict' }
Check 'parent-folder-conflict' { $x=Clone $baseline;$x.workItems[1].writePaths=@('test-fixture/a/child.cs');Reject $x 'WriteOwnershipConflict' }
Check 'case-and-separator-conflict' { $x=Clone $baseline;$x.workItems[1].writePaths=@('TEST-FIXTURE\A');Reject $x 'WriteOwnershipConflict' }
Check 'prefix-is-not-folder-overlap' { $x=Clone $baseline;$x.workItems[1].writePaths=@('test-fixture/ab');if (@(Results $x | Where-Object canExecute).Count -ne 3) {throw 'FalsePathConflict'} }
Check 'contract-conflict' { $x=Clone $baseline;$x.workItems[0].sharedContractKeys=@('inventory');$x.workItems[1].sharedContractKeys=@('inventory');Reject $x 'SharedContractConflict' }
Check 'dependency-cycle' { $x=Clone $baseline;$x.workItems[0].dependsOnWorkItemIds=@('b');$x.workItems[1].dependsOnWorkItemIds=@('a');Reject $x 'DependencyCycle' }
Check 'unknown-dependency' { $x=Clone $baseline;$x.workItems[0].dependsOnWorkItemIds=@('missing');Reject $x 'DependencyUnknown' }
Check 'outside-root' { $x=Clone $baseline;$x.workItems[0].writePaths=@('../escape');Reject $x 'PathOutsideRepository' }
Check 'unapproved' { $x=Clone $baseline;$x.workItems[0].planningGate.statusCode='Draft';Blocked $x 'a' 'PlanningNotApproved' }
Check 'design-hash' { $x=Clone $baseline;$x.workItems[0].planningGate.designHashSha256='0'*64;Blocked $x 'a' 'DesignHashMismatch' }
Check 'work-order-hash' { $x=Clone $baseline;$x.workItems[0].workOrderSha256='0'*64;Blocked $x 'a' 'BaselineHashMismatch' }
Check 'baseline-hash' { $x=Clone $baseline;$x.workItems[0].baselineFiles[0].sha256='0'*64;Blocked $x 'a' 'BaselineHashMismatch' }
Check 'missing-baseline' { $x=Clone $baseline;$x.workItems[0].baselineFiles=@();Reject $x 'BaselineMissing' }
Check 'delivery-cap' { $x=Clone $baseline;$x.workItems[0].targetEvidenceStageCode='E4';Blocked $x 'a' 'DeliveryCapExceeded' }
Check 'presentation-target-not-approved' { $x=Clone $baseline;$x.workItems[0].trackCode='Presentation';$x.workItems[0].targetEvidenceStageCode='E2';Blocked $x 'a' 'TrackTargetNotApproved' }
Check 'presentation-requires-logic-e5' { $x=Clone $baseline;$x.workItems[0].trackCode='Presentation';$x.workItems[0].targetEvidenceStageCode='E5';Blocked $x 'a' 'PresentationRequiresLogicE5' }
Check 'wrong-wi-binding' { $x=Clone $baseline;$x.workItems[0].worldInteractionId='WI-B';Blocked $x 'a' 'WorkOrderBindingMismatch' }
Check 'wrong-design-revision' { $x=Clone $baseline;$x.workItems[0].planningGate.designRevision='unknown';Blocked $x 'a' 'DesignRevisionMismatch' }
Check 'goal-prerequisite' { $x=Clone $baseline;$x.items[1].activationPrerequisiteLoopStableIds=@('loop:a');Blocked $x 'c' 'GoalDependencyNotCompleted' }
Check 'legacy-projection' { $x=[pscustomobject]@{schemaVersion='codex-playable-loop-goals.v3';activeGoal=@{goalStateCode='Active';loopStableId='loop:a';activeWorldInteractionId='WI-A';activeMaturityTrackCode='Logic';workOrderRef='legacy'}};if (@(Get-ParallelDevelopmentWorkItems $x).Count -ne 1) {throw 'LegacyLost'} }
Check 'malformed-blocked-order-is-local' { $x=Clone $baseline;$x.workItems[0].statusCode='Blocked';$x.workItems[0].workOrderRef='README.md';$x.workItems[0].workOrderSha256=(Get-FileHash (Join-Path $root 'README.md')).Hash;Blocked $x 'a' 'WorkOrderInvalid';if (@(Results $x | Where-Object canExecute).Count -ne 2) {throw 'MalformedBlockerLeaked'} }
Write-Output "ParallelDevelopmentWorkTestsPassed:$script:passed"
