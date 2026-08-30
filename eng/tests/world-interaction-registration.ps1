$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$저장소 = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $저장소 'eng/execution-ledgers/world-interaction-registration-functions.ps1')
. (Join-Path $저장소 'eng/common/deterministic-text-output.ps1')
$등록경로 = 'eng/execution-ledgers/world-interaction-registration-relations.json'
$대장경로 = 'eng/execution-ledgers/world-interactions.json'
$등록 = Read-WorldInteractionRegistration $저장소 $등록경로 $대장경로
$대장 = Get-Content (Join-Path $저장소 $대장경로) -Raw -Encoding UTF8 | ConvertFrom-Json
$신규 = @($등록.decisions | Where-Object dispositionCode -eq 'RegisterAction')
$캠페인 = Get-Content (Join-Path $저장소 'eng/execution-ledgers/remaining-32-wi-logic-e3-campaign.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$캠페인완료 = @($캠페인.items | Where-Object stateCode -eq 'LogicE3Verified')
if ($null -ne $캠페인.executionPolicy.wipLimit -or $캠페인.executionPolicy.concurrencyModeCode -ne 'DependencyAndOwnership') { throw 'CampaignParallelOwnershipPolicyInvalid' }
if (@($캠페인.items).Count -ne 32 -or @($캠페인.items.worldInteractionId | Sort-Object -Unique).Count -ne 32) { throw 'Campaign32ScopeInvalid' }
if (($캠페인.items.worldInteractionId | Sort-Object) -join ',' -ne (($신규.canonicalId | Where-Object { $_ -ne 'WI-HEAT-SOURCE-STATE-CHANGE' } | Sort-Object) -join ',')) { throw 'CampaignRegistrationScopeMismatch' }
if ($신규.Count -ne 33 -or @($등록.decisions | Where-Object dispositionCode -eq 'ReuseProfile').Count -ne 2 -or @($등록.decisions | Where-Object dispositionCode -eq 'MetadataFamily').Count -ne 5) { throw 'RegistrationDispositionCountsInvalid' }
foreach ($결정 in $신규) {
    $항목 = @($대장.items | Where-Object id -eq $결정.canonicalId)[0]
    if ($항목.id -eq 'WI-HEAT-SOURCE-STATE-CHANGE') {
        $명세 = Get-Content (Join-Path $저장소 'eng/execution-ledgers/work-orders/nature-heat-source.e7-work-order.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($항목.implementation.currentStage -ne 'E3' -or $항목.integration.currentStage -ne 'E1' -or $명세.trackPlans.logic.currentEvidenceStage -ne 'E3' -or $명세.deliveryCap.promotionBeyondStageAllowed -or @($항목.existingImplementationReferences).Count -ne 4) { throw 'HeatSourceApprovedE3BindingInvalid' }
        foreach ($파일 in $항목.existingImplementationReferences) { if (-not (Test-Path (Join-Path $저장소 $파일))) { throw 'HeatSourceImplementationMissing' } }
    } elseif (@($캠페인완료 | Where-Object worldInteractionId -eq $항목.id).Count -eq 1) {
        $완료 = @($캠페인완료 | Where-Object worldInteractionId -eq $항목.id)[0]
        $명세 = Get-Content (Join-Path $저장소 $완료.workOrderRef) -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($항목.implementation.currentStage -ne 'E3' -or $완료.logicStage -ne 'E3' -or $항목.integration.currentStage -ne 'E1' -or $명세.trackPlans.logic.currentEvidenceStage -ne 'E3' -or $명세.deliveryCap.promotionBeyondStageAllowed -or $명세.activeWorldInteractionId -ne $항목.id -or @($항목.existingImplementationReferences).Count -lt 4) { throw "CampaignE3EvidenceInvalid:$($항목.id)" }
        foreach ($파일 in $완료.implementationRefs) { if (-not (Test-Path (Join-Path $저장소 $파일)) -or $항목.existingImplementationReferences -notcontains $파일) { throw 'CampaignImplementationBindingMissing' } }
        if (@($완료.testEvidenceRefs).Count -eq 0 -or $명세.implementationEvidence.passed -le 0 -or $명세.implementationEvidence.failed -ne 0) { throw 'CampaignTestEvidenceMissing' }
        $증거대장 = Get-Content (Join-Path $저장소 'eng/execution-ledgers/evidence-packages.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        $증거 = @($증거대장.packages | Where-Object evidenceId -eq $완료.evidencePackageRef)
        if ($증거.Count -ne 1 -or $증거[0].resultCode -ne 'Passed' -or $증거[0].subjectRefs -notcontains $항목.id) { throw 'CampaignEvidencePackageMissing' }
        foreach ($파일 in $완료.testEvidenceRefs) {
            $시험근거 = @($증거[0].artifactReferences | Where-Object locator -eq $파일)
            if ($시험근거.Count -ne 1 -or $시험근거[0].sha256 -notmatch '^[0-9a-f]{64}$') { throw 'CampaignTestHashMissing' }
            if (-not (Test-Path (Join-Path $저장소 $파일))) { continue } # 다른 작업 트리에는 LocalOnly 결과가 없을 수 있다.
            if ((Get-FileHash (Join-Path $저장소 $파일)).Hash.ToLowerInvariant() -ne $시험근거[0].sha256) { throw 'CampaignTestHashMismatch' }
            [xml]$시험결과 = Get-Content (Join-Path $저장소 $파일) -Raw -Encoding UTF8
            if ([int]$시험결과.TestRun.ResultSummary.Counters.failed -ne 0 -or [int]$시험결과.TestRun.ResultSummary.Counters.passed -ne $명세.implementationEvidence.passed) { throw 'CampaignTestResultMismatch' }
        }
    } elseif ($항목.implementation.status -ne 'NotStarted' -or $항목.implementation.currentStage -ne 'E0' -or $항목.integration.currentStage -ne 'E0' -or @($항목.implementation.evidence).Count -ne 0 -or @($항목.integration.evidence).Count -ne 0 -or @($항목.existingImplementationReferences).Count -ne 0) { throw "RegistrationPromotedEvidence:$($항목.id)" }
    if (@($항목.httpContracts | Where-Object { $_ -notlike 'NotApproved:*' }).Count -gt 0 -or @($항목.saveReplayPayloadCodes | Where-Object { $_ -notlike 'NotApproved:*' }).Count -gt 0) { throw 'RegistrationCreatedRuntimeContract' }
}
$원문후보 = @($등록.decisions.candidateId | Sort-Object)
$범위 = Get-Content (Join-Path $저장소 'eng/execution-ledgers/playable-loop-inquiry-implementation-scope.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$범위후보 = @($범위.smallImplementationBatches | ForEach-Object { $_.worldInteractionIds; if ($null -ne $_.PSObject.Properties['plannedWorldInteractionIds']) { $_.plannedWorldInteractionIds } } | Where-Object { $_ -in $원문후보 } | Sort-Object -Unique)
if (($범위후보 -join ',') -ne ($원문후보 -join ',')) { throw 'OriginalCandidateCoverageLost' }
$목표 = Get-Content (Join-Path $저장소 'eng/execution-ledgers/codex-playable-loop-goals.json') -Raw -Encoding UTF8 | ConvertFrom-Json
# 문답의 대표 조회 작업과 Goal의 대표 조회 작업은 서로 다를 수 있다.
# 실행 후보는 단일 대표 값의 일치가 아니라 전체 작업 목록의 실제 결속으로 검증한다.
foreach ($작업 in @($목표.workItems)) {
    if (@($대장.items | Where-Object id -eq $작업.worldInteractionId).Count -ne 1) { throw 'ImplementationWorkItemWorldInteractionMissing' }
    $작업명세 = Get-Content (Join-Path $저장소 $작업.workOrderRef) -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($작업명세.activeWorldInteractionId -ne $작업.worldInteractionId) { throw 'ImplementationWorkItemWorldInteractionMismatch' }
}
$확인 = @(
    @{ code='DuplicateCandidate'; change={ param($r) $r.decisions[-1] = $r.decisions[0] } },
    @{ code='UnknownMember'; change={ param($r) $r.families[0].memberWorldInteractionIds += 'WI-UNKNOWN' } },
    @{ code='ExecutableFamily'; change={ param($r) $r.families[0].executionCode='Command' } },
    @{ code='FamilyCycle'; change={ param($r) $r.families[0].parentFamilyId=$r.families[0].id } },
    @{ code='SpecializationCycle'; change={ param($r) $r.specializations += [pscustomobject]@{parentWorldInteractionId='WI-NATURE-05';childWorldInteractionId='WI-ACTOR-01'} } },
    @{ code='DuplicateProfileAction'; change={ param($r) ($r.decisions | Where-Object dispositionCode -eq 'ReuseProfile' | Select-Object -First 1).canonicalId='WI-UNKNOWN' } },
    @{ code='OutcomeMismatch'; change={ param($r) $r.decisions[0].primaryOutcomeCode='UnrelatedEffect' } },
    @{ code='InvalidProjectionOwner'; change={ param($r) $r.modules[0].executionCode='Command' } },
    @{ code='UnapprovedQuestionRange'; change={ param($r) $r.decisions[0].questions=@('Q-340') } }
)
$원본 = $등록 | ConvertTo-Json -Depth 30
foreach ($시험 in $확인) {
    $사본 = $원본 | ConvertFrom-Json
    & $시험.change $사본
    $파일 = "artifacts/local/validation/wi-registration/negative-$($시험.code).json"
    Write-DeterministicTextIfChanged (Join-Path $저장소 $파일) ($사본 | ConvertTo-Json -Depth 30) | Out-Null
    $오류 = ''
    try { $null = Read-WorldInteractionRegistration $저장소 $파일 $대장경로 } catch { $오류 = $_.Exception.Message }
    if ($오류 -notlike "WiRegistrationInvalid:$($시험.code)*") { throw "NegativeTestFailed:$($시험.code):$오류" }
}
$생성기 = Join-Path $저장소 'eng/execution-ledgers/manage-world-interaction-registration.ps1'
& $생성기 -Mode Write
$문서경로 = Join-Path $저장소 'docs/AI/generated/world-interaction-registration.md'
$해시 = (Get-FileHash $문서경로).Hash
$수정시각 = (Get-Item $문서경로).LastWriteTimeUtc.Ticks
& $생성기 -Mode Write
& $생성기 -Mode Check
if ($해시 -ne (Get-FileHash $문서경로).Hash -or $수정시각 -ne (Get-Item $문서경로).LastWriteTimeUtc.Ticks) { throw 'RegistrationGenerationNotIdempotent' }
Write-Output "WorldInteractionRegistrationTestsPassed:Candidates=41;New=33;Negative=9;CampaignLogicE3=$($캠페인완료.Count);RemainingE0=$(32-$캠페인완료.Count);WorkItemsConsistent"
