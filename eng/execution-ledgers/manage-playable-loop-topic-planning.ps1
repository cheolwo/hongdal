[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",
    [string] $PlayableLoopPath = "eng/execution-ledgers/playable-loops.json",
    [string] $GoalLedgerPath = "eng/execution-ledgers/codex-playable-loop-goals.json",
    [string] $OutputPath = "docs/AI/generated/playable-loop-topic-planning.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PlayableLoopTopicPlanningInvalid:$Code" }
}
function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}
function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}
function Resolve-RepoPath([string] $Path) { Join-Path $repositoryRoot $Path }

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$loops = Get-Content -LiteralPath (Resolve-RepoPath $PlayableLoopPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$goals = Get-Content -LiteralPath (Resolve-RepoPath $GoalLedgerPath) -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $loops.schemaVersion -eq "ssalddel-playable-loop-catalog.v6") "SchemaInvalid"
$policy = $loops.designDocumentationPolicy
Require ([bool] $policy.topicPlanningRequiredForPlayableUnits) "PlanningGatePolicyMissing"
Require ([bool] $policy.topicMapsToExactlyOnePlayableUnit) "TopicOneToOnePolicyMissing"
Require-Text $policy.detailedDesignRoot "DetailedDesignRootMissing"
Require-Text $policy.topicDesignTemplateRef "TopicDesignTemplateMissing"
Require (Test-Path -LiteralPath (Resolve-RepoPath ([string] $policy.topicDesignTemplateRef))) "TopicDesignTemplateNotFound"
$allowed = @($policy.allowedPlanningStatusCodes | ForEach-Object { [string] $_ })
Require (($allowed -join ",") -eq "NotStarted,Draft,ReadyForReview,Approved,LegacyActiveMigration") "PlanningStatusesInvalid"

$goalByLoop = @{}
foreach ($goal in @($goals.items)) { $goalByLoop[[string] $goal.loopStableId] = $goal }
$activeLoopId = [string] $goals.activeGoal.loopStableId
$legacyLoopId = [string] $policy.legacyActiveMigrationLoopStableId
$seenTopics = @{}
$units = @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit")
$legacyCount = 0
$requiredHeadings = @(
    "## 식별과 근거", "## 플레이어 약속과 재미", "## 반복 폐루프",
    "## 선택·대가·성공·실패·회복", "## WI 단일 책임 후보",
    "## 논리·표현 요구", "## H 공간과 자산 요구",
    "## 저장·권위·외부 경계", "## 제외 범위와 승인")

foreach ($loop in @($loops.items)) {
    $id = [string] $loop.loopStableId
    $gateProperty = $loop.PSObject.Properties["planningGate"]
    if ([string] $loop.loopLevelCode -ne "PlayableUnit") {
        Require ($null -eq $gateProperty) "AggregateHasPlanningGate:$id"
        continue
    }
    Require ($null -ne $gateProperty) "PlanningGateMissing:$id"
    $gate = $loop.planningGate
    $topicId = [string] $gate.topicStableId
    Require-Text $topicId "TopicStableIdMissing:$id"
    Require (-not $seenTopics.ContainsKey($topicId)) "TopicStableIdDuplicate:$topicId"
    $expectedTopicId = ([string] $id).Replace("playable-loop:", "topic:")
    Require ($topicId -eq $expectedTopicId) "TopicStableIdDrift:$id"
    $seenTopics[$topicId] = $id
    $status = [string] $gate.statusCode
    Require ($allowed -contains $status) "PlanningStatusInvalid:$id"

    if ($status -eq "NotStarted") {
        foreach ($field in @("designDocumentRef", "designRevision", "designHashSha256", "approvalEvidenceRef")) {
            Require ([string]::IsNullOrWhiteSpace([string] $gate.$field)) "NotStartedFieldMustBeEmpty:${id}:$field"
        }
    }
    elseif ($status -eq "LegacyActiveMigration") {
        $legacyCount++
        Require ($id -eq $legacyLoopId) "LegacyMigrationTransferred:$id"
        Require ($id -eq $activeLoopId) "LegacyMigrationIsNotActive:$id"
        Require ([string] $goals.activeGoal.goalStateCode -eq "Active") "LegacyMigrationGoalMustRemainActive:$id"
    }
    else {
        foreach ($field in @("designDocumentRef", "designRevision", "designHashSha256")) {
            Require-Text $gate.$field "PlanningFieldMissing:${id}:$field"
        }
        $documentRef = [string] $gate.designDocumentRef
        Require ($documentRef.Replace("\", "/").StartsWith(([string] $policy.detailedDesignRoot).TrimEnd("/") + "/")) "DesignDocumentOutsideRoot:$id"
        $documentPath = Resolve-RepoPath $documentRef
        Require (Test-Path -LiteralPath $documentPath) "DesignDocumentNotFound:$id"
        $content = Get-Content -LiteralPath $documentPath -Raw -Encoding UTF8
        foreach ($heading in $requiredHeadings) {
            Require ($content.Contains($heading)) "DesignHeadingMissing:${id}:$heading"
        }
        Require (@($loop.sourcePlanningDocumentRefs).Count -gt 0) "SourcePlanningDocumentMissing:$id"
        foreach ($sourceRef in @($loop.sourcePlanningDocumentRefs)) {
            Require (Test-Path -LiteralPath (Resolve-RepoPath ([string] $sourceRef))) "SourcePlanningDocumentNotFound:${id}:$sourceRef"
        }
        $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $documentPath).Hash
        Require ($hash.Equals([string] $gate.designHashSha256, [StringComparison]::OrdinalIgnoreCase)) "DesignHashMismatch:$id"
        if ($status -eq "Approved") {
            Require-Text $gate.approvalEvidenceRef "ApprovalEvidenceMissing:$id"
        }
        else {
            Require ([string]::IsNullOrWhiteSpace([string] $gate.approvalEvidenceRef)) "UnapprovedHasApprovalEvidence:$id"
        }
    }
}
Require ($legacyCount -le 1) "LegacyMigrationCountInvalid"

Require ($goalByLoop.ContainsKey($activeLoopId)) "ActiveGoalLoopUnknown"
$activeUnit = @($units | Where-Object loopStableId -eq $activeLoopId)[0]
$activePlanning = [string] $activeUnit.planningGate.statusCode
Require ($activePlanning -eq "Approved" -or ($activePlanning -eq "LegacyActiveMigration" -and $activeLoopId -eq $legacyLoopId)) "ActiveGoalPlanningNotApproved:$activeLoopId"

$nextGoal = @($goals.items | Where-Object goalStateCode -eq "Queued" | Sort-Object queueOrder | Select-Object -First 1)
$nextText = if ($nextGoal.Count -eq 0) { "없음" } else {
    $nextLoopId = [string] $nextGoal[0].loopStableId
    $nextLoop = @($units | Where-Object { [string] $_.loopStableId -eq $nextLoopId })[0]
    "$($nextGoal[0].loopStableId) / $($nextLoop.planningGate.statusCode)"
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# PlayableLoop 주제 기획 상태")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$PlayableLoopPath``와 ``$GoalLedgerPath``에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- PlayableUnit: ``$($units.Count)``")
[void] $builder.AppendLine("- 승인됨: ``$(@($units | Where-Object { $_.planningGate.statusCode -eq 'Approved' }).Count)``")
[void] $builder.AppendLine("- 한시적 이전: ``$legacyCount``")
[void] $builder.AppendLine("- 현재 Goal: ``$activeLoopId`` / ``$activePlanning``")
[void] $builder.AppendLine("- 다음 대기 Goal: ``$nextText``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 주제 | PlayableLoop | 기획 상태 | Goal | 현재 WI | Logic | Presentation | 통합 E | 다음 기획 책임 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |")
foreach ($loop in $units) {
    $gate = $loop.planningGate
    $goal = $goalByLoop[[string] $loop.loopStableId]
    $responsibility = switch ([string] $gate.statusCode) {
        "NotStarted" { "기획서 작성" }
        "Draft" { "필수 절 보완·검토 요청" }
        "ReadyForReview" { "명시적 승인" }
        "Approved" { "Goal 활성화 가능" }
        "LegacyActiveMigration" { "현재 Goal 완료 전 승인 전환" }
    }
    [void] $builder.AppendLine("| ``$(Escape-Cell $gate.topicStableId)`` | ``$(Escape-Cell $loop.loopStableId)`` | $($gate.statusCode) | $($goal.goalStateCode) | ``$(Escape-Cell $goal.nextWorldInteractionId)`` | $($loop.maturityTracks.logic.currentStage) | $($loop.maturityTracks.presentation.currentStage) | $($loop.currentEvidenceStage) | $responsibility |")
}
$content = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Resolve-RepoPath $OutputPath
if ($Mode -eq "Write") {
    [void] (Write-DeterministicTextIfChanged -Path $resolvedOutput -Content $content)
}
Write-Output "PlayableLoopTopicPlanningValid:Units=$($units.Count);Approved=$(@($units | Where-Object { $_.planningGate.statusCode -eq 'Approved' }).Count);Legacy=$legacyCount;Active=$activeLoopId"
