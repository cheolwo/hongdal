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
    $catalog -notmatch "MaximumConcurrentWorkItems => null" -or
    $catalog -notmatch "IReadOnlyList<string> ActiveWorldInteractionIds" -or
    $catalog -notmatch "IsActiveWorldInteraction" -or
    $catalog -notmatch "SimulationWI실행우선순위Catalog") {
    throw "WorldInteractionDeliveryPriorityContractShapeInvalid"
}
if ($check -notmatch "WorldInteractionDeliveryPrioritiesValid:105") {
    throw "WorldInteractionDeliveryPriorityValidationDidNotComplete"
}

# Generation consumes the Goal work list, not the representative activeWork field.
# The standalone generator must validate approvals as well as project active IDs.
# Full cloned work orders below are synthetic test approvals, never production approval.
$fixtureDirectory = "artifacts/local/validation/delivery-parallel-$([Guid]::NewGuid().ToString('N'))"
[void] (New-Item -ItemType Directory -Path (Join-Path $repositoryRoot $fixtureDirectory) -Force)
$fixtureGoalPath = "$fixtureDirectory/goals.json"
$fixtureMarkdownPath = "$fixtureDirectory/delivery.md"
$fixtureContractPath = "$fixtureDirectory/delivery.cs"
$legacyDeliveryPath = "$fixtureDirectory/legacy-delivery.json"
$legacyDelivery = Get-Content -LiteralPath $ledger -Raw -Encoding UTF8 | ConvertFrom-Json
$legacyDelivery.deliveryModeCode = "SingleWorldInteractionVertical"
$legacyDelivery.workInProgressLimit = 1
$legacyDelivery.PSObject.Properties.Remove('concurrencyModeCode')
$legacyDelivery | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $repositoryRoot $legacyDeliveryPath) -Encoding UTF8
$null = & $script -Mode Write -InputPath $legacyDeliveryPath -OutputPath $fixtureMarkdownPath -ContractOutputPath $fixtureContractPath
$legacyContract = Get-Content -LiteralPath (Join-Path $repositoryRoot $fixtureContractPath) -Raw -Encoding UTF8
if ($legacyContract -notmatch 'MaximumConcurrentWorkItems => null') { throw "LegacyDisplayLimitBecameExecutionLimit" }
$goalFixture = Get-Content -LiteralPath (Join-Path $repositoryRoot "eng/execution-ledgers/codex-playable-loop-goals.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$fixtureItems = @()
foreach ($spec in @(
    @{id='first';wi='WI-ACTOR-PLAN-SET';order='nature-personal-plan';doc='Nature개인계획E3';status='Active'},
    @{id='second';wi='WI-HEAT-SOURCE-STATE-CHANGE';order='nature-heat-source';doc='Nature열원관리E3';status='ReadyForIntegration'}
)) {
    $order = Get-Content -LiteralPath (Join-Path $repositoryRoot "eng/execution-ledgers/work-orders/$($spec.order).e7-work-order.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $orderRef = "$fixtureDirectory/$($spec.id)-order.json"
    $order | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $repositoryRoot $orderRef) -Encoding UTF8
    $orderHash = (Get-FileHash -LiteralPath (Join-Path $repositoryRoot $orderRef)).Hash
    $docRef = "docs/Architecture/PlayableLoops/$($spec.doc).md"
    $fixtureItems += [pscustomobject]@{
        workItemId="fixture:$($spec.id)";loopStableId=$order.playableUnitStableId;worldInteractionId=$spec.wi
        trackCode='Logic';statusCode=$spec.status;ownerThreadId="fixture:$($spec.id)";worktreePath=$repositoryRoot
        writePaths=@("fixture/$($spec.id)");sharedContractKeys=@();dependsOnWorkItemIds=@()
        baselineFiles=@(@{path=$orderRef;sha256=$orderHash});workOrderRef=$orderRef;workOrderSha256=$orderHash;targetEvidenceStageCode='E2'
        planningGate=@{statusCode='Approved';designDocumentRef=$docRef;designRevision=$order.integratedGate.candidateRevision;designHashSha256=(Get-FileHash -LiteralPath (Join-Path $repositoryRoot $docRef)).Hash;approvalEvidenceRef='fixture:explicit-approval'}
    }
}
$additionalIds = @($fixtureItems | ForEach-Object { [string] $_.worldInteractionId })
$duplicate = $fixtureItems[0] | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$duplicate.workItemId='fixture:duplicate-wi-track'
$duplicate.writePaths=@('fixture/duplicate-wi-track')
$blockedFocus = $goalFixture.workItems[0] | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$blockedFocus.workItemId='fixture:blocked-focus'
$blockedFocus.statusCode='Blocked'
$blockedFocus.planningGate.statusCode='Draft'
$fixtureItems += @($duplicate, $blockedFocus)
$goalFixture | Add-Member -MemberType NoteProperty -Name workItems -Value $fixtureItems -Force
$goalFixture | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $repositoryRoot $fixtureGoalPath) -Encoding UTF8
$null = & $script -Mode Write -GoalLedgerPath $fixtureGoalPath -OutputPath $fixtureMarkdownPath -ContractOutputPath $fixtureContractPath
$fixtureContract = Get-Content -LiteralPath (Join-Path $repositoryRoot $fixtureContractPath) -Raw -Encoding UTF8
$activeLine = @($fixtureContract -split "`n" | Where-Object { $_ -match "IReadOnlyList<string> ActiveWorldInteractionIds" })[0]
foreach ($id in $additionalIds) {
    if (($activeLine | Select-String -Pattern ([regex]::Escape('"' + $id + '"')) -AllMatches).Matches.Count -ne 1) {
        throw "ParallelActiveIdProjectionInvalid:$id"
    }
}
if ($activeLine.Contains('"' + $expectedActiveWorldInteractionId + '"')) { throw "BlockedFocusMustNotExecute" }
if (@($fixtureContract -split "`n" | Where-Object { $_ -match 'new SimulationWI.*"Active"' }).Count -ne 2) {
    throw "ParallelActiveStateProjectionInvalid"
}
$null = & $script -Mode Check -GoalLedgerPath $fixtureGoalPath -OutputPath $fixtureMarkdownPath -ContractOutputPath $fixtureContractPath

# Standalone Write cannot project an unapproved Active task as executable.
$goalFixture.workItems[0].planningGate.statusCode = 'Draft'
$goalFixture | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $repositoryRoot $fixtureGoalPath) -Encoding UTF8
$rejected = $false
try { $null = & $script -Mode Write -GoalLedgerPath $fixtureGoalPath -OutputPath $fixtureMarkdownPath -ContractOutputPath $fixtureContractPath }
catch {
    if ($_.Exception.Message -notlike '*ExecutingWorkItemNotApproved:fixture:first:PlanningNotApproved*') { throw }
    $rejected = $true
}
if (-not $rejected) { throw 'UnapprovedActiveWorkWasAccepted' }
$goalFixture.workItems[0].planningGate.statusCode = 'Approved'

# An unregistered WI still fails even with the global count limit removed.
$goalFixture.workItems[0].worldInteractionId = "WI-UNREGISTERED-PARALLEL-FIXTURE"
$goalFixture | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $repositoryRoot $fixtureGoalPath) -Encoding UTF8
$rejected = $false
try { $null = & $script -Mode Write -GoalLedgerPath $fixtureGoalPath -OutputPath $fixtureMarkdownPath -ContractOutputPath $fixtureContractPath }
catch {
    if ($_.Exception.Message -notlike "*WorkOutsideLoop:fixture:first*") { throw }
    $rejected = $true
}
if (-not $rejected) { throw "UnregisteredParallelWorkWasAccepted" }

Write-Output "WorldInteractionDeliveryPriorityTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
