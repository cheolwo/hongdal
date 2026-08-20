param(
    [ValidateSet("Check", "Write")]
    [string] $Mode = "Check",
    [string] $QueuePath = "eng/world-seedbeds/synty-bottom-up-inventory/h2-human-review-queue.v1.json",
    [string] $OutputMarkdownPath = "docs/AI/generated/h2-human-review-queue.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path

function Read-Json([string] $relativePath) {
    $path = Join-Path $repositoryRoot ($relativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path)) { throw "JsonMissing:$relativePath" }
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Require([bool] $condition, [string] $message) {
    if (-not $condition) { throw $message }
}

function Normalize([string] $value) { return (($value -replace "`r`n", "`n").TrimEnd()) + "`n" }

$queue = Read-Json $QueuePath
Require ([string] $queue.schemaVersion -eq "simulation-world-h2-human-review-queue.v1") "QueueSchemaInvalid"
Require ([bool] $queue.reviewPolicy.oneH2PerDecision) "OneH2PerDecisionRequired"
Require ([int] $queue.reviewPolicy.requiredViewCount -eq 5) "FiveViewReviewRequired"
Require ([bool] $queue.reviewPolicy.automaticApprovalForbidden) "AutomaticApprovalMustBeForbidden"

$priorityCatalog = Read-Json ([string] $queue.sourcePriorityPath)
$evidenceCatalog = Read-Json ([string] $queue.sourceEvidencePath)
$gameplayCatalog = Read-Json ([string] $queue.sourceGameplayPath)

$candidateMap = @{}
foreach ($candidate in @($priorityCatalog.candidates)) { $candidateMap[[string] $candidate.candidateRef] = $candidate }
$evidenceMap = @{}
foreach ($evidence in @($evidenceCatalog.items)) { $evidenceMap[[string] $evidence.evidenceRef] = $evidence }
$declaredPlayableSliceIds = @($gameplayCatalog.playableSlices.playableSliceId) + @($gameplayCatalog.nextPlayableSliceQueue.playableSliceId)

$expectedPriority = 1
$seenH2 = @{}
$rows = @()
foreach ($item in @($queue.items | Sort-Object priority)) {
    Require ([int] $item.priority -eq $expectedPriority) "ReviewPriorityGap:$($item.priority)"
    $expectedPriority++
    Require (-not $seenH2.ContainsKey([string] $item.h2Ref)) "DuplicateH2Review:$($item.h2Ref)"
    $seenH2[[string] $item.h2Ref] = $true
    Require ($candidateMap.ContainsKey([string] $item.h2Ref)) "UnknownH2Candidate:$($item.h2Ref)"
    Require ($evidenceMap.ContainsKey([string] $item.evidenceRef)) "EvidenceMissing:$($item.h2Ref)"
    $evidence = $evidenceMap[[string] $item.evidenceRef]
    Require ([string] $evidence.targetKnowledgeRef -eq [string] $item.h2Ref) "EvidenceTargetMismatch:$($item.h2Ref)"
    Require ([int] $evidence.captureCount -eq 5) "CaptureCountInvalid:$($item.h2Ref)"
    Require ([string] $evidence.reviewStateCode -eq "AwaitingHumanReview") "EvidenceAlreadyDecided:$($item.h2Ref)"
    Require ([string] $item.reviewStateCode -eq "AwaitingHumanReview") "QueueStateInvalid:$($item.h2Ref)"
    Require ([string] $item.playableSliceId -in $declaredPlayableSliceIds) "PlayableSliceUnknown:$($item.playableSliceId)"
    Require (-not [string]::IsNullOrWhiteSpace([string] $item.reviewQuestion)) "ReviewQuestionMissing:$($item.h2Ref)"
    $rows += [ordered]@{
        priority = [int] $item.priority
        h2Ref = [string] $item.h2Ref
        priorityCode = [string] $candidateMap[[string] $item.h2Ref].priorityCode
        playableSliceId = [string] $item.playableSliceId
        gameplayMomentCode = [string] $item.gameplayMomentCode
        evidenceRef = [string] $item.evidenceRef
        captureFolderRelativePath = [string] $evidence.captureFolderRelativePath
        reviewQuestion = [string] $item.reviewQuestion
        reviewStateCode = [string] $item.reviewStateCode
    }
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# H2 사람 검토 대기열")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$QueuePath``에서 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 대기 항목: $($rows.Count)개")
[void] $builder.AppendLine("- 검토 단위: H2 하나")
[void] $builder.AppendLine("- 필수 화면: H2당 5시점")
[void] $builder.AppendLine("- 판단: ``ApproveCandidate`` / ``NeedsRevision`` / ``Hold``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 순위 | H2 후보 | 기준 플레이 | 검토 질문 | 상태 |")
[void] $builder.AppendLine("| ---: | --- | --- | --- | --- |")
foreach ($row in $rows) {
    [void] $builder.AppendLine("| $($row.priority) | ``$($row.h2Ref)`` | ``$($row.playableSliceId)`` | $($row.reviewQuestion) | ``$($row.reviewStateCode)`` |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 판단 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 입구·목표·위험·출구와 게임 플레이 흐름을 H2 조합 전체로 판단한다.")
[void] $builder.AppendLine("- H1 이미지는 부품 존재 확인용이며 H2 승인 화면을 대신하지 않는다.")
[void] $builder.AppendLine("- 후보 승인은 AreaSet 배치, WI E단계, 공공데이터 또는 Runtime 완료가 아니다.")
$markdown = Normalize $builder.ToString()
$outputPath = Join-Path $repositoryRoot ($OutputMarkdownPath -replace "/", [IO.Path]::DirectorySeparatorChar)

if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $outputPath) "OutputMissing"
    Require ((Normalize ([IO.File]::ReadAllText($outputPath))) -ceq $markdown) "OutputStale"
    Write-Output "H2HumanReviewQueueValid:Items=$($rows.Count);Nature=2;Farm=2;Town=2"
    exit 0
}

$directory = Split-Path -Parent $outputPath
if (-not (Test-Path -LiteralPath $directory)) { [void] (New-Item -ItemType Directory -Path $directory) }
if (-not (Test-Path -LiteralPath $outputPath) -or (Normalize ([IO.File]::ReadAllText($outputPath))) -cne $markdown) {
    [IO.File]::WriteAllText($outputPath, $markdown, [Text.UTF8Encoding]::new($false))
}
Write-Output "H2HumanReviewQueueGenerated:Items=$($rows.Count);Nature=2;Farm=2;Town=2"
