$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-playable-loop-topic-planning.ps1'
$loopsPath = Join-Path $repositoryRoot 'eng/execution-ledgers/playable-loops.json'
$goalsPath = Join-Path $repositoryRoot 'eng/execution-ledgers/codex-playable-loop-goals.json'
$outputPath = 'artifacts/local/validation/playable-loop-topic-planning/current.md'
$artifacts = Join-Path $repositoryRoot 'artifacts/local/validation/playable-loop-topic-planning'
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
$first = & $manager -Mode Write -OutputPath $outputPath
$hash = (Get-FileHash (Join-Path $repositoryRoot $outputPath)).Hash
$ticks = (Get-Item (Join-Path $repositoryRoot $outputPath)).LastWriteTimeUtc.Ticks
$second = & $manager -Mode Write -OutputPath $outputPath
if ((Get-FileHash (Join-Path $repositoryRoot $outputPath)).Hash -ne $hash -or (Get-Item (Join-Path $repositoryRoot $outputPath)).LastWriteTimeUtc.Ticks -ne $ticks) { throw 'TopicOutputNotDeterministic' }
function Read-Loops { Get-Content $loopsPath -Raw -Encoding UTF8 | ConvertFrom-Json }
function Read-Goals { Get-Content $goalsPath -Raw -Encoding UTF8 | ConvertFrom-Json }
function Invoke-Case([string] $Name, [object] $Loops, [object] $Goals, [string] $ExpectedError = '') {
    $loopRef = "artifacts/local/validation/playable-loop-topic-planning/$Name-loops.json"
    $goalRef = "artifacts/local/validation/playable-loop-topic-planning/$Name-goals.json"
    [IO.File]::WriteAllText((Join-Path $repositoryRoot $loopRef), ($Loops | ConvertTo-Json -Depth 100), $utf8)
    [IO.File]::WriteAllText((Join-Path $repositoryRoot $goalRef), ($Goals | ConvertTo-Json -Depth 100), $utf8)
    $failure = ''
    try { $null = & $manager -Mode Validate -PlayableLoopPath $loopRef -GoalLedgerPath $goalRef }
    catch { $failure = $_.Exception.Message }
    if ($ExpectedError -eq '' -and $failure -ne '') { throw "TopicPositiveFailed:${Name}:$failure" }
    if ($ExpectedError -ne '' -and -not $failure.Contains($ExpectedError)) { throw "TopicNegativeFailed:${Name}:$failure" }
}
$goals = Read-Goals
$loops = Read-Loops
$approved = @($loops.items | Where-Object { $_.loopLevelCode -eq 'PlayableUnit' -and $_.planningGate.statusCode -eq 'Approved' })
if ($approved.Count -lt 2) { throw 'TwoApprovedFixturesRequired' }
# All actually active goals must be checked, not only the display focus.
foreach ($unit in $approved[0..1]) { ($goals.items | Where-Object loopStableId -eq $unit.loopStableId).goalStateCode = 'Active' }
Invoke-Case 'multiple-approved' $loops $goals
$unapproved = Read-Loops
$other = @($unapproved.items | Where-Object { $_.loopLevelCode -eq 'PlayableUnit' -and $_.planningGate.statusCode -eq 'NotStarted' })[0]
$goals2 = Read-Goals
($goals2.items | Where-Object loopStableId -eq $other.loopStableId).goalStateCode = 'Active'
Invoke-Case 'nonfocus-unapproved' $unapproved $goals2 'ActiveGoalPlanningNotApproved'
$missingGate = Read-Loops
($missingGate.items | Where-Object loopStableId -eq $other.loopStableId).PSObject.Properties.Remove('planningGate')
Invoke-Case 'missing-gate' $missingGate (Read-Goals) 'PlanningGateMissing'
$duplicate = Read-Loops
($duplicate.items | Where-Object loopStableId -eq $approved[1].loopStableId).planningGate.topicStableId = $approved[0].planningGate.topicStableId
Invoke-Case 'duplicate-topic' $duplicate (Read-Goals) 'TopicStableId'
$legacy = Read-Loops
($legacy.items | Where-Object loopStableId -eq $other.loopStableId).planningGate.statusCode = 'LegacyActiveMigration'
Invoke-Case 'legacy-transfer' $legacy (Read-Goals) 'LegacyMigrationTransferred'
$badHash = Read-Loops
($badHash.items | Where-Object loopStableId -eq $approved[0].loopStableId).planningGate.designHashSha256 = ('0' * 64)
Invoke-Case 'bad-hash' $badHash (Read-Goals) 'DesignHashMismatch'
$noApproval = Read-Loops
($noApproval.items | Where-Object loopStableId -eq $approved[0].loopStableId).planningGate.approvalEvidenceRef = ''
Invoke-Case 'no-approval' $noApproval (Read-Goals) 'ApprovalEvidenceMissing'
$aggregate = Read-Loops
($aggregate.items | Where-Object loopLevelCode -ne PlayableUnit | Select-Object -First 1) | Add-Member -NotePropertyName planningGate -NotePropertyValue @{}
Invoke-Case 'aggregate-gate' $aggregate (Read-Goals) 'AggregateHasPlanningGate'
Write-Output 'PlayableLoopTopicPlanningTestsPassed:Positive=3;Negative=7;MultiActivePlanning=Verified'
