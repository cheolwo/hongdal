[CmdletBinding()]
param(
    [ValidateSet('Write', 'Check', 'Validate')]
    [string] $Mode = 'Check',
    [string] $InputPath = 'eng/execution-ledgers/hexagram-story-seeds.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../common/deterministic-text-output.ps1')

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramStorySeedsInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Resolve-RepositoryPath([string] $Path, [string] $Code, [bool] $MustExist) {
    Require-Text $Path "$Code`:PathMissing"
    Require (-not [IO.Path]::IsPathRooted($Path)) "$Code`:Rooted"
    $candidate = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path))
    $prefix = $repositoryRoot.TrimEnd('\') + '\'
    Require ($candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) "$Code`:Traversal"
    if ($MustExist) { Require (Test-Path -LiteralPath $candidate -PathType Leaf) "$Code`:Missing" }
    return $candidate
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

$inputFile = if ([IO.Path]::IsPathRooted($InputPath)) {
    [IO.Path]::GetFullPath($InputPath)
} else {
    Resolve-RepositoryPath $InputPath 'Input' $true
}
Require (Test-Path -LiteralPath $inputFile -PathType Leaf) 'InputMissing'
$source = Get-Content -LiteralPath $inputFile -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $source.schemaVersion -eq 'mirror-hexagram-story-seeds.v1') 'SchemaVersion'
Require ([string] $source.revision -eq 'hexagram-story-seeds.r1') 'Revision'
$productionPath = Resolve-RepositoryPath ([string] $source.parentProductionLedgerPath) 'ParentProductionLedger' $true
$production = Get-Content -LiteralPath $productionPath -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $production.schemaVersion -eq 'mirror-hexagram-story-production.v3') 'ParentSchemaVersion'
$hexagrams = @($production.hexagrams)
$hexagramIds = @($hexagrams | ForEach-Object { [string] $_.stableId })
$lineIds = @($hexagrams | ForEach-Object { @($_.lineStories) } | ForEach-Object { [string] $_.stableId })

$policy = $source.policy
Require ([string] $policy.intakeCode -eq 'FreePlanningBeforeHexagramAndLineClassification') 'IntakePolicy'
Require ([string] $policy.assignmentCode -eq 'OnePrimaryWithRankedSecondaryCandidates') 'AssignmentPolicy'
Require ([string] $policy.classificationAuthorityCode -eq 'CodexSuggestsUserConfirms') 'AuthorityPolicy'
Require ([string] $policy.lineGateCode -eq 'SubjectSituationTransformationResultKnown') 'LineGatePolicy'
foreach ($flag in @('preserveReclassificationHistory', 'doesNotChangeActiveProductionHexagram', 'doesNotCreateLinePlanDocument', 'doesNotCreateWorldInteraction', 'doesNotPromoteEvidence')) {
    Require ([bool] $policy.$flag) "PolicyFlag:$flag"
}

$allowedStates = @($source.allowedCodes.classificationStateCodes | ForEach-Object { [string] $_ })
$allowedLineStates = @($source.allowedCodes.lineClassificationStateCodes | ForEach-Object { [string] $_ })
$allowedHandoffStates = @($source.allowedCodes.developmentHandoffStateCodes | ForEach-Object { [string] $_ })
Require (($allowedStates -join ',') -eq 'StorySeed,HexagramCandidate,HexagramConfirmed,LineCandidate,LineConfirmed') 'AllowedClassificationStates'
Require (($allowedLineStates -join ',') -eq 'NotStarted,Deferred,Candidate,Confirmed') 'AllowedLineStates'

$seeds = @($source.seeds)
Require ($seeds.Count -gt 0) 'SeedCount'
Require (@($seeds.storySeedStableId | Sort-Object -Unique).Count -eq $seeds.Count) 'SeedIdDuplicate'
foreach ($seed in $seeds) {
    $seedId = [string] $seed.storySeedStableId
    Require ($seedId -match '^STORY-SEED-[A-Z0-9-]+-\d{3}$') "SeedId:$seedId"
    foreach ($field in @('title', 'synopsis')) {
        Require-Text $seed.$field "$field`:$seedId"
    }
    Require (@($seed.sourcePlanningRefs).Count -gt 0) "SourcePlanningRefs:$seedId"
    $state = [string] $seed.classificationStateCode
    $lineState = [string] $seed.lineClassificationStateCode
    Require ($state -in $allowedStates) "ClassificationState:$seedId"
    Require ($lineState -in $allowedLineStates) "LineState:$seedId"
    Require ([string] $seed.developmentHandoffStateCode -in $allowedHandoffStates) "HandoffState:$seedId"

    $primary = [string] $seed.primaryHexagramStableId
    $classified = $state -ne 'StorySeed'
    Require (($classified -and $primary -in $hexagramIds) -or (-not $classified -and [string]::IsNullOrWhiteSpace($primary))) "PrimaryHexagram:$seedId"
    if ($classified) { Require-Text $seed.primaryReason "PrimaryReason:$seedId" }
    $candidates = @($seed.secondaryCandidates)
    $candidateHexagramIds = @($candidates | ForEach-Object { [string] $_.hexagramStableId })
    Require (@($candidateHexagramIds | Sort-Object -Unique).Count -eq $candidates.Count) "SecondaryDuplicate:$seedId"
    for ($index = 0; $index -lt $candidates.Count; $index++) {
        $candidate = $candidates[$index]
        Require ([int] $candidate.rank -eq ($index + 1)) "SecondaryRank:$seedId"
        Require ([string] $candidate.hexagramStableId -in $hexagramIds) "SecondaryHexagram:$seedId"
        Require ([string] $candidate.hexagramStableId -ne $primary) "SecondaryMatchesPrimary:$seedId"
        Require-Text $candidate.reason "SecondaryReason:$seedId"
    }

    if ($state -in @('HexagramConfirmed', 'LineCandidate', 'LineConfirmed')) {
        Require-Text $seed.userConfirmationRef "UserConfirmation:$seedId"
    }
    $lineCandidates = @($seed.lineCandidates)
    $lineCandidateIds = @($lineCandidates | ForEach-Object { [string] $_.lineStableId })
    Require (@($lineCandidateIds | Sort-Object -Unique).Count -eq $lineCandidates.Count) "LineCandidateDuplicate:$seedId"
    if ($lineState -in @('Candidate', 'Confirmed')) {
        foreach ($field in @('subjectSummary', 'situationSummary', 'transformationSummary', 'resultOrNextStateSummary')) {
            Require-Text $seed.$field "LineGate$field`:$seedId"
        }
    }
    foreach ($lineCandidate in $lineCandidates) {
        Require ([string] $lineCandidate.lineStableId -in $lineIds) "LineCandidateUnknown:$seedId"
        Require ([string] $lineCandidate.lineStableId -like "$primary-L*") "LineCandidateOutsidePrimary:$seedId"
        Require-Text $lineCandidate.reason "LineCandidateReason:$seedId"
    }
    if ($lineState -eq 'Deferred') {
        Require ($lineCandidates.Count -eq 0) "DeferredHasCandidates:$seedId"
        Require-Text $seed.lineDeferralReason "LineDeferralReason:$seedId"
    }
    if ($lineState -eq 'Candidate') {
        Require ($state -eq 'LineCandidate' -and $lineCandidates.Count -gt 0) "LineCandidateState:$seedId"
    }
    if ($lineState -eq 'Confirmed') {
        Require ($state -eq 'LineConfirmed') "LineConfirmedState:$seedId"
        Require ([string] $seed.confirmedLineStableId -in $lineCandidateIds) "ConfirmedLine:$seedId"
    } else {
        Require ([string]::IsNullOrWhiteSpace([string] $seed.confirmedLineStableId)) "UnexpectedConfirmedLine:$seedId"
    }
    Require ($state -ne 'LineConfirmed' -or [string] $seed.developmentHandoffStateCode -ne 'NotEligible') "ConfirmedLineHandoff:$seedId"

    $history = @($seed.classificationHistory)
    Require ($history.Count -gt 0) "HistoryMissing:$seedId"
    for ($index = 0; $index -lt $history.Count; $index++) {
        Require ([int] $history[$index].revision -eq ($index + 1)) "HistoryRevision:$seedId"
        Require ([string] $history[$index].stateCode -in $allowedStates) "HistoryState:$seedId"
        Require-Text $history[$index].reason "HistoryReason:$seedId"
    }
    Require ([string] $history[-1].stateCode -eq $state) "HistoryCurrentState:$seedId"
    Require ([string] $history[-1].primaryHexagramStableId -eq $primary) "HistoryCurrentPrimary:$seedId"
}

$countsByHexagram = foreach ($hexagram in $hexagrams | Sort-Object ordinal) {
    $id = [string] $hexagram.stableId
    [ordered]@{
        ordinal = [int] $hexagram.ordinal
        hexagramStableId = $id
        symbol = [string] $hexagram.symbol
        nameKorean = [string] $hexagram.nameKorean
        primarySeedCount = @($seeds | Where-Object primaryHexagramStableId -eq $id).Count
        secondaryCandidateCount = @($seeds | Where-Object {
            @($_.secondaryCandidates | ForEach-Object { [string] $_.hexagramStableId }) -contains $id
        }).Count
    }
}
$result = [ordered]@{
    schemaVersion = 'mirror-hexagram-story-seeds-index.v1'
    revision = [string] $source.revision
    parentProductionRevision = [string] $production.revision
    formalAuthoringHexagramStableId = [string] $production.policy.formalAuthoringHexagramStableId
    prototypeDialogueLineStableId = [string] $production.policy.prototypeDialogueLineStableId
    policy = $policy
    counts = [ordered]@{
        seeds = $seeds.Count
        confirmedHexagrams = @($seeds | Where-Object classificationStateCode -eq 'HexagramConfirmed').Count
        lineCandidates = @($seeds | Where-Object classificationStateCode -eq 'LineCandidate').Count
        lineConfirmed = @($seeds | Where-Object classificationStateCode -eq 'LineConfirmed').Count
    }
    hexagramSummary = @($countsByHexagram)
    seeds = $seeds
}

$jsonText = ConvertTo-DeterministicText (($result | ConvertTo-Json -Depth 20) + "`n")
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine('# 역경 자유 기획 씨앗 분류 색인')
[void] $builder.AppendLine()
[void] $builder.AppendLine('> 자유 기획을 먼저 보존하고, 주괘와 후보괘를 거쳐 필요한 경우에만 효사로 내려간다. 이 문서는 자동 생성된다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 정식 제작 현재 괘: ``$($result.formalAuthoringHexagramStableId)``")
[void] $builder.AppendLine("- 선행 표본 문답 위치: ``$($result.prototypeDialogueLineStableId)``")
[void] $builder.AppendLine("- 자유 기획 씨앗: ``$($result.counts.seeds)`` / 괘 확정: ``$($result.counts.confirmedHexagrams)`` / 효 후보: ``$($result.counts.lineCandidates)`` / 효 확정: ``$($result.counts.lineConfirmed)``")
[void] $builder.AppendLine('- Codex가 주괘와 보조 후보를 제안하고 사용자가 확인한다. 이 분류는 제작 순서·Runtime·WI·Evidence를 자동 변경하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 기획 씨앗')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 씨앗 | 상태 | 주괘 | 보조 후보 | 효 분류 | 개발 인계 |')
[void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- |')
foreach ($seed in $seeds) {
    $primaryHexagram = $hexagrams | Where-Object stableId -eq $seed.primaryHexagramStableId | Select-Object -First 1
    $primaryLabel = if ($null -eq $primaryHexagram) { '미분류' } else { "$($primaryHexagram.symbol) $($primaryHexagram.nameKorean)" }
    $secondaryLabels = @($seed.secondaryCandidates | ForEach-Object {
        $candidateId = [string] $_.hexagramStableId
        $candidateHexagram = $hexagrams | Where-Object stableId -eq $candidateId | Select-Object -First 1
        "$($_.rank). $($candidateHexagram.symbol) $($candidateHexagram.nameKorean)"
    }) -join ', '
    if ([string]::IsNullOrWhiteSpace($secondaryLabels)) { $secondaryLabels = '없음' }
    [void] $builder.AppendLine("| ``$($seed.storySeedStableId)``<br>$(Escape-Cell $seed.title) | ``$($seed.classificationStateCode)`` | $(Escape-Cell $primaryLabel) | $(Escape-Cell $secondaryLabels) | ``$($seed.lineClassificationStateCode)`` | ``$($seed.developmentHandoffStateCode)`` |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 괘별 유입 현황')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 순번 | 괘 | 주괘 씨앗 | 보조 후보 |')
[void] $builder.AppendLine('| ---: | --- | ---: | ---: |')
foreach ($summary in $countsByHexagram | Where-Object { $_.primarySeedCount -gt 0 -or $_.secondaryCandidateCount -gt 0 }) {
    [void] $builder.AppendLine("| $($summary.ordinal) | $($summary.symbol) $($summary.nameKorean) | $($summary.primarySeedCount) | $($summary.secondaryCandidateCount) |")
}
$markdownText = ConvertTo-DeterministicText $builder.ToString()

$jsonOutput = Resolve-RepositoryPath ([string] $source.outputJsonPath) 'JsonOutput' $false
$markdownOutput = Resolve-RepositoryPath ([string] $source.outputMarkdownPath) 'MarkdownOutput' $false
if ($Mode -eq 'Write') {
    [void] (Write-DeterministicTextIfChanged -Path $jsonOutput -Content $jsonText)
    [void] (Write-DeterministicTextIfChanged -Path $markdownOutput -Content $markdownText)
} elseif ($Mode -eq 'Check') {
    Require (Test-Path -LiteralPath $jsonOutput -PathType Leaf) 'JsonOutputMissing'
    Require (Test-Path -LiteralPath $markdownOutput -PathType Leaf) 'MarkdownOutputMissing'
    Require ((Get-Content -LiteralPath $jsonOutput -Raw -Encoding UTF8) -ceq $jsonText) 'JsonOutputStale'
    Require ((Get-Content -LiteralPath $markdownOutput -Raw -Encoding UTF8) -ceq $markdownText) 'MarkdownOutputStale'
}

Write-Output "HexagramStorySeeds:$Mode`:OK:Seeds=$($result.counts.seeds):Confirmed=$($result.counts.confirmedHexagrams):LineConfirmed=$($result.counts.lineConfirmed)"
