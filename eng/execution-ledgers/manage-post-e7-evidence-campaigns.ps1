[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/execution-ledgers/post-e7-evidence-campaigns.json",
    [string] $OutputPath = "docs/AI/generated/post-e7-evidence-campaigns.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PostE7EvidenceCampaignInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Same-Set([object[]] $Left, [object[]] $Right) {
    return ((@($Left | ForEach-Object { [string] $_ } | Sort-Object) -join ',') -eq
        (@($Right | ForEach-Object { [string] $_ } | Sort-Object) -join ','))
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$catalog = Get-Content -LiteralPath (Join-Path $repositoryRoot $InputPath) `
    -Raw -Encoding UTF8 | ConvertFrom-Json
$loops = Get-Content -LiteralPath (Join-Path $repositoryRoot `
    ([string] $catalog.playableLoopCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $catalog.schemaVersion -eq "post-e7-evidence-campaigns.v3") "SchemaInvalid"
Require ([string] $catalog.evidenceModelRevision -eq
    "horizontal-dual-cycle-evidence.r3") "EvidenceModelInvalid"
foreach ($principle in @(
    "playableUnitVerticalMaturityEndsAtE7",
    "e8UsesPlayableUnitStabilitySubject",
    "everyPlayableUnitHasExactlyOneE8Campaign",
    "e8LogicAndPresentationShareFrozenRevision",
    "e9UsesAreaHarmonySetSubject",
    "e9RequiresAtLeastTwoStableCoreMembers",
    "e9IncludesHumanPlayAndApproval",
    "integratedStageUsesLowerTrack",
    "e10RequiresImmutableBuildAndObservationWindow",
    "automaticValidationCannotApproveE9OrE10",
    "findingReopensE9OrEarliestAffectedE1ToE8",
    "revisionChangeInvalidatesActiveObservationWindow",
    "operationalEffectsRequireSeparateAuthority")) {
    $property = $catalog.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) "PrincipleMissing:$principle"
}

$policy = $catalog.stabilityPolicy
Require ([int] $policy.requiredLogicDeterministicRunCount -eq 3) "LogicRunThresholdInvalid"
Require ([bool] $policy.requiresSaveRestoreReplay) "SaveRestoreReplayMustBeRequired"
Require ([bool] $policy.requiresLocalRemoteParity) "LocalRemoteParityMustBeRequired"
Require ([int] $policy.requiredPresentationActualInputRunCount -eq 2) "PresentationRunThresholdInvalid"
Require ([bool] $policy.requiresSaveReentry) "SaveReentryMustBeRequired"
Require ([int] $policy.requiredBlockingConsoleErrorCount -eq 0) "ConsoleErrorThresholdInvalid"

$loopById = @{}
foreach ($loop in @($loops.items)) { $loopById[[string] $loop.loopStableId] = $loop }
$playableUnits = @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit")
$playableUnitIds = @($playableUnits | ForEach-Object { [string] $_.loopStableId })

$stabilityById = @{}
$stabilityByLoop = @{}
foreach ($campaign in @($catalog.playableUnitStabilityCampaigns)) {
    $id = [string] $campaign.stabilityCampaignStableId
    $loopId = [string] $campaign.loopStableId
    Require-Text $id "StabilityIdMissing"
    Require (-not $stabilityById.ContainsKey($id)) "StabilityIdDuplicate:$id"
    Require (-not $stabilityByLoop.ContainsKey($loopId)) "StabilityLoopDuplicate:$loopId"
    Require ($loopById.ContainsKey($loopId)) "StabilityLoopUnknown:$id"
    $loop = $loopById[$loopId]
    Require ([string] $loop.loopLevelCode -eq "PlayableUnit") "StabilitySubjectNotPlayableUnit:$id"
    Require ([string] $loop.finalEvidenceStage -eq "E7") "StabilityLoopFinalStageInvalid:$id"
    Require ([string] $campaign.evidenceStageCode -eq "E8") "StabilityStageInvalid:$id"
    Require (@($catalog.allowedCampaignStatusCodes) -contains [string] $campaign.statusCode) `
        "StabilityStatusInvalid:$id"
    Require ([string] $campaign.currentEvidenceStage -eq [string] $loop.currentEvidenceStage) `
        "StabilityObservedStageStale:$id"
    Require ([string] $campaign.observedClosureStateCode -eq [string] $loop.closureStateCode) `
        "StabilityObservedClosureStale:$id"
    Require ([string] $campaign.observedLoopStatusCode -eq [string] $loop.statusCode) `
        "StabilityObservedStatusStale:$id"
    $loopIsE7 = [string] $loop.currentEvidenceStage -eq "E7" -and
        [string] $loop.closureStateCode -eq "PlayClosed" -and
        [string] $loop.statusCode -eq "Validated"
    if ($loopIsE7) {
        Require ([string] $campaign.statusCode -ne "WaitingForE7") "StabilityReadyStateStale:$id"
    }
    else {
        Require ([string] $campaign.statusCode -in @("WaitingForE7", "Blocked")) `
            "StabilityMustWaitForE7:$id"
        Require (-not [bool] $campaign.promotionEligible) "StabilityCannotPromoteBeforeE7:$id"
    }
    if ([string] $campaign.statusCode -in @("InProgress", "Passed")) {
        Require-Text $campaign.candidateRevision "StabilityCandidateRevisionMissing:$id"
        Require-Text $campaign.candidateBuildHash "StabilityCandidateBuildMissing:$id"
    }
    if ([bool] $campaign.promotionEligible) {
        Require ($loopIsE7) "StabilityNeedsE7:$id"
        Require ([string] $campaign.statusCode -eq "Passed") "StabilityPromotionStatusInvalid:$id"
        Require ([int] $campaign.results.logicDeterministicRunCount -ge
            [int] $policy.requiredLogicDeterministicRunCount) "StabilityLogicRunsBelowGate:$id"
        Require ([bool] $campaign.results.saveRestoreReplayPassed) "StabilitySaveReplayMissing:$id"
        Require ([bool] $campaign.results.localRemoteParityPassed) "StabilityParityMissing:$id"
        Require ([int] $campaign.results.presentationActualInputRunCount -ge
            [int] $policy.requiredPresentationActualInputRunCount) "StabilityPresentationRunsBelowGate:$id"
        Require ([bool] $campaign.results.saveReentryPassed) "StabilitySaveReentryMissing:$id"
        Require ([int] $campaign.results.blockingConsoleErrorCount -eq
            [int] $policy.requiredBlockingConsoleErrorCount) "StabilityConsoleErrorsRemain:$id"
        Require (@($campaign.results.evidenceRefs).Count -gt 0) "StabilityEvidenceMissing:$id"
        Require (@($campaign.blockers).Count -eq 0) "StabilityBlockersRemain:$id"
    }
    $stabilityById[$id] = $campaign
    $stabilityByLoop[$loopId] = $campaign
}
Require (@($catalog.playableUnitStabilityCampaigns).Count -eq $playableUnits.Count) `
    "StabilityCoverageCountInvalid"
Require (Same-Set $playableUnitIds @($stabilityByLoop.Keys)) "StabilityCoverageSetInvalid"

$harmonyById = @{}
foreach ($campaign in @($catalog.areaHarmonyCampaigns)) {
    $id = [string] $campaign.harmonySetStableId
    Require-Text $id "HarmonySetIdMissing"
    Require (-not $harmonyById.ContainsKey($id)) "HarmonySetDuplicate:$id"
    Require ([string] $campaign.evidenceStageCode -eq "E9") "HarmonyStageInvalid:$id"
    Require (@($catalog.allowedCampaignStatusCodes) -contains [string] $campaign.statusCode) `
        "HarmonyStatusInvalid:$id"
    Require ($loopById.ContainsKey([string] $campaign.aggregateLoopStableId)) `
        "AggregateLoopUnknown:$id"
    $aggregate = $loopById[[string] $campaign.aggregateLoopStableId]
    Require ([string] $aggregate.loopLevelCode -eq "AreaAggregate") "AggregateLoopLevelInvalid:$id"
    Require ([string] $aggregate.finalEvidenceStage -eq "E9") "AggregateFinalStageInvalid:$id"
    $memberCampaignIds = @($campaign.requiredMemberStabilityCampaignStableIds)
    Require ($memberCampaignIds.Count -ge 2) "HarmonyNeedsTwoStableMembers:$id"
    Require ($memberCampaignIds.Count -eq @($memberCampaignIds | Sort-Object -Unique).Count) `
        "HarmonyMemberDuplicate:$id"
    foreach ($memberCampaignId in $memberCampaignIds) {
        Require ($stabilityById.ContainsKey([string] $memberCampaignId)) `
            "HarmonyStabilityUnknown:${id}:$memberCampaignId"
        $stability = $stabilityById[[string] $memberCampaignId]
        $memberLoop = $loopById[[string] $stability.loopStableId]
        Require ([string] $memberLoop.completionTierCode -eq "Core") `
            "HarmonyMemberMustBeCore:${id}:$($memberLoop.loopStableId)"
        Require ([string] $memberLoop.parentLoopStableId -eq [string] $aggregate.loopStableId) `
            "HarmonyMemberMustBelongToAggregateCore:${id}:$($memberLoop.loopStableId)"
    }
    Require (@($campaign.executionOrderProfiles).Count -ge 2) "HarmonyOrderProfilesMissing:$id"
    $tracks = @($campaign.maturityTracks.logic, $campaign.maturityTracks.presentation)
    foreach ($track in $tracks) {
        $trackCode = [string] $track.trackCode
        Require (@($catalog.allowedTrackCodes) -contains $trackCode) `
            "HarmonyTrackCodeInvalid:${id}:$trackCode"
        Require ([string] $track.currentEvidenceStage -eq "E8" -and
            [string] $track.targetEvidenceStage -eq "E9") `
            "HarmonyTrackStageInvalid:${id}:$trackCode"
        Require ([string] $track.statusCode -eq [string] $campaign.statusCode) `
            "HarmonyTrackStatusDrift:${id}:$trackCode"
        Require (Same-Set @($catalog.requiredHarmonyModuleCodesByTrack.$trackCode) `
            @($track.moduleReviews.moduleCode)) "HarmonyModuleSetInvalid:${id}:$trackCode"
        foreach ($module in @($track.moduleReviews)) {
            Require (@($catalog.allowedModuleStatusCodes) -contains [string] $module.statusCode) `
                "HarmonyModuleStatusInvalid:${id}:${trackCode}:$($module.moduleCode)"
            Require ([string] $module.applicabilityCode -in @("Required", "NotApplicable")) `
                "HarmonyModuleApplicabilityInvalid:${id}:${trackCode}:$($module.moduleCode)"
            if ([string] $module.applicabilityCode -eq "NotApplicable") {
                Require ([string] $module.statusCode -eq "NotApplicable") `
                    "HarmonyNotApplicableStatusInvalid:${id}:${trackCode}:$($module.moduleCode)"
                Require (@($module.evidenceRefs).Count -gt 0) `
                    "HarmonyNotApplicableNeedsReason:${id}:${trackCode}:$($module.moduleCode)"
            }
        }
    }
    Require ([string] $campaign.integratedGate.currentEvidenceStage -eq "E8" -and
        [string] $campaign.integratedGate.targetEvidenceStage -eq "E9") `
        "HarmonyIntegratedStageInvalid:$id"
    Require ([string] $campaign.integratedGate.statusCode -eq [string] $campaign.statusCode) `
        "HarmonyIntegratedStatusDrift:$id"
    Require ((@($campaign.humanAcceptance.requiredRatingCodes) -join ',') -eq
        "Fun,Immersion,Completeness") "HumanRatingCodesInvalid:$id"
    Require ([int] $campaign.humanAcceptance.minimumRating -eq 4) "HumanMinimumRatingInvalid:$id"
    foreach ($trackCode in @($catalog.allowedTrackCodes)) {
        Require (Same-Set @($catalog.requiredHumanMetricCodesByTrack.$trackCode) `
            @($campaign.humanAcceptance.requiredMetricCodesByTrack.$trackCode)) `
            "HumanTrackMetricSetInvalid:${id}:$trackCode"
    }
    foreach ($finding in @($campaign.humanAcceptance.findings)) {
        Require (@($catalog.allowedFindingSeverityCodes) -contains [string] $finding.severityCode) `
            "FindingSeverityInvalid:$id"
        Require (@($catalog.allowedFindingStatusCodes) -contains [string] $finding.statusCode) `
            "FindingStatusInvalid:$id"
        Require ([string] $finding.reopenStageCode -match '^E[1-9]$') "FindingReopenStageInvalid:$id"
        Require (@($catalog.allowedTrackCodes) -contains [string] $finding.targetTrackCode) `
            "FindingTrackInvalid:$id"
        Require-Text $finding.reopenSubjectRef "FindingReopenSubjectMissing:$id"
    }
    Require-Text $campaign.nextAction "HarmonyNextActionMissing:$id"
    if ([bool] $campaign.promotionEligible) {
        foreach ($memberCampaignId in $memberCampaignIds) {
            Require ([bool] $stabilityById[[string] $memberCampaignId].promotionEligible) `
                "HarmonyMemberNotE8Stable:${id}:$memberCampaignId"
        }
        Require ([string] $campaign.statusCode -eq "Passed") "HarmonyPromotionStatusInvalid:$id"
        Require-Text $campaign.frozenWorldRevision "HarmonyFrozenRevisionMissing:$id"
        Require-Text $campaign.candidateBuildHash "HarmonyBuildHashMissing:$id"
        foreach ($track in $tracks) {
            Require (@($track.moduleReviews | Where-Object {
                [string] $_.statusCode -notin @("Passed", "NotApplicable")
            }).Count -eq 0) "HarmonyModuleOpen:${id}:$($track.trackCode)"
        }
        Require (@($campaign.integratedGate.openFeedbackRefs).Count -eq 0) `
            "HarmonyIntegratedFeedbackOpen:$id"
        $human = $campaign.humanAcceptance
        Require (@($human.sessions).Count -gt 0) "HumanSessionMissing:$id"
        Require (@($human.findings | Where-Object {
            [string] $_.statusCode -eq "Open" -and
            [string] $_.severityCode -in @("Blocking", "Major")
        }).Count -eq 0) "HumanBlockingFindingOpen:$id"
        $latest = @($human.sessions)[-1]
        foreach ($trackCode in @($catalog.allowedTrackCodes)) {
            $trackMetrics = $latest.trackMetrics.PSObject.Properties[$trackCode]
            Require ($null -ne $trackMetrics) "HumanSessionTrackMetricsMissing:${id}:$trackCode"
            foreach ($metricCode in @($human.requiredMetricCodesByTrack.$trackCode)) {
                $metric = $trackMetrics.Value.PSObject.Properties[[string] $metricCode]
                Require ($null -ne $metric -and [int] $metric.Value -ge [int] $human.minimumRating) `
                    "HumanTrackMetricBelowGate:${id}:${trackCode}:$metricCode"
            }
        }
        foreach ($ratingCode in @($human.requiredRatingCodes)) {
            $rating = $latest.ratings.PSObject.Properties[[string] $ratingCode]
            Require ($null -ne $rating -and [int] $rating.Value -ge [int] $human.minimumRating) `
                "HumanRatingBelowGate:${id}:$ratingCode"
        }
        Require ([bool] $human.humanCandidateApproval) "HumanApprovalMissing:$id"
        Require ([string] $human.approvedCandidateRevision -eq [string] $campaign.frozenWorldRevision) `
            "HumanCandidateRevisionDiffersFromHarmony:$id"
        Require ([string] $human.approvedCandidateBuildHash -eq [string] $campaign.candidateBuildHash) `
            "HumanCandidateBuildDiffersFromHarmony:$id"
        Require (@($campaign.blockers).Count -eq 0) "HarmonyBlockersRemain:$id"
    }
    $harmonyById[$id] = $campaign
}

foreach ($deferred in @($catalog.deferredAreaHarmonySubjects)) {
    $aggregateId = [string] $deferred.aggregateLoopStableId
    Require ($loopById.ContainsKey($aggregateId)) "DeferredAggregateUnknown:$aggregateId"
    $aggregate = $loopById[$aggregateId]
    Require ([string] $aggregate.loopLevelCode -eq "AreaAggregate") `
        "DeferredAggregateLevelInvalid:$aggregateId"
    Require ([int] $deferred.currentCorePlayableUnitCount -eq
        @($aggregate.requiredCoreChildLoopStableIds).Count) "DeferredCoreCountStale:$aggregateId"
    Require ([int] $deferred.currentCorePlayableUnitCount -lt
        [int] $deferred.minimumCorePlayableUnitCount) "DeferredAreaAlreadyEligible:$aggregateId"
    Require (@($catalog.areaHarmonyCampaigns | Where-Object {
        [string] $_.areaCode -eq [string] $deferred.areaCode
    }).Count -eq 0) "DeferredAreaHasHarmonyCampaign:$aggregateId"
    Require-Text $deferred.reason "DeferredReasonMissing:$aggregateId"
}

$profileByCode = @{}
foreach ($profile in @($catalog.limitedOperationProfiles)) {
    $code = [string] $profile.profileCode
    Require-Text $code "OperationProfileCodeMissing"
    Require (-not $profileByCode.ContainsKey($code)) "OperationProfileDuplicate:$code"
    Require ([string] $profile.platformCode -eq "WindowsX64") "OperationPlatformInvalid:$code"
    Require ([string] $profile.authorityLocationCode -eq "LocalProcess") "OperationAuthorityInvalid:$code"
    Require ([int] $profile.minimumDistinctDays -gt 0 -and
        [int] $profile.minimumCompletedSessions -gt 0 -and
        [int] $profile.minimumTesterCount -gt 0) "OperationThresholdInvalid:$code"
    Require ([bool] $profile.requiresRollbackExercise) "OperationRollbackRequired:$code"
    $profileByCode[$code] = $profile
}

foreach ($window in @($catalog.limitedOperationWindows)) {
    $id = [string] $window.windowStableId
    Require-Text $id "OperationWindowIdMissing"
    Require ([string] $window.evidenceStageCode -eq "E10") "OperationWindowStageInvalid:$id"
    Require ($harmonyById.ContainsKey([string] $window.areaHarmonySetStableId)) `
        "OperationHarmonyUnknown:$id"
    Require ($profileByCode.ContainsKey([string] $window.profileCode)) "OperationProfileUnknown:$id"
    Require (-not [bool] $window.externalOperationalEffectsAuthorized) `
        "OperationEffectsMustRemainUnauthorized:$id"
    if ([bool] $window.promotionEligible) {
        $harmony = $harmonyById[[string] $window.areaHarmonySetStableId]
        $profile = $profileByCode[[string] $window.profileCode]
        Require ([bool] $harmony.promotionEligible) "OperationNeedsE9:$id"
        Require ([string] $window.statusCode -eq "Passed") "OperationStatusInvalid:$id"
        Require ([string] $window.candidateRevision -eq [string] $harmony.frozenWorldRevision) `
            "OperationCandidateRevisionDrift:$id"
        Require ([string] $window.candidateBuildHash -eq [string] $harmony.candidateBuildHash) `
            "OperationBuildHashDrift:$id"
        Require ([int] $window.observedDistinctDays -ge [int] $profile.minimumDistinctDays) `
            "OperationDaysBelowGate:$id"
        Require ([int] $window.completedSessionCount -ge [int] $profile.minimumCompletedSessions) `
            "OperationSessionsBelowGate:$id"
        Require ([int] $window.testerCount -ge [int] $profile.minimumTesterCount) `
            "OperationTestersBelowGate:$id"
        Require ([int] $window.freshTargetPlayerCount -ge [int] $profile.minimumFreshTargetPlayerCount) `
            "OperationFreshPlayersBelowGate:$id"
        foreach ($metric in @("crashCount", "unhandledExceptionCount", "saveCorruptionCount", "replayHashMismatchCount")) {
            Require ([int] $window.$metric -eq 0) "OperationFailureMetric:${id}:$metric"
        }
        Require ([bool] $window.rollbackExercisePassed) "OperationRollbackMissing:$id"
        Require ([bool] $window.humanContinueOperationApproval) "OperationHumanApprovalMissing:$id"
        Require (@($window.blockers).Count -eq 0) "OperationBlockersRemain:$id"
    }
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# E7 이후 개별 안정·영역 조화·제한 운영 상태")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> ``$InputPath``에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 증거 모델: ``$($catalog.evidenceModelRevision)``")
[void] $builder.AppendLine("- E8 개별 안정 후보: ``$(@($catalog.playableUnitStabilityCampaigns).Count)``")
[void] $builder.AppendLine("- E9 영역 조화·사람 승인 후보: ``$(@($catalog.areaHarmonyCampaigns).Count)``")
[void] $builder.AppendLine("- E9 보류 영역: ``$(@($catalog.deferredAreaHarmonySubjects).Count)``")
[void] $builder.AppendLine("- E10 제한 운영 창: ``$(@($catalog.limitedOperationWindows).Count)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## E8 PlayableUnit 안정성")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| PlayableUnit | 현재 E | 상태 | 승격 가능 | 차단 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- |")
foreach ($campaign in @($catalog.playableUnitStabilityCampaigns)) {
    [void] $builder.AppendLine("| ``$($campaign.loopStableId)`` | $($campaign.currentEvidenceStage) | $($campaign.statusCode) | $([bool] $campaign.promotionEligible) | $(@($campaign.blockers) -join '<br>') |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## E9 영역 조화와 사람 승인")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 영역 | 후보 | 구성원 | 논리 | 표현 | 통합 | 사람 승인 |")
[void] $builder.AppendLine("| --- | --- | ---: | --- | --- | --- | --- |")
foreach ($campaign in @($catalog.areaHarmonyCampaigns)) {
    [void] $builder.AppendLine("| $($campaign.areaCode) | ``$($campaign.harmonySetStableId)`` | $(@($campaign.requiredMemberStabilityCampaignStableIds).Count) | $($campaign.maturityTracks.logic.statusCode) | $($campaign.maturityTracks.presentation.statusCode) | $($campaign.integratedGate.statusCode) | $([bool] $campaign.humanAcceptance.humanCandidateApproval) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("Town과 City는 기존 단일 Core를 억지로 분할하지 않는다. 새 독립 Core 플레이 약속이 생길 때 E9 후보를 연다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("PlayableUnit Goal은 E7에서 끝난다. E8 결함은 같은 PlayableUnit의 가장 이른 E1~E7을 다시 열고, E9 결함은 조화 관문 또는 관련 PlayableUnit의 가장 이른 E1~E8을 다시 연다.")

$content = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    [void] (Write-DeterministicTextIfChanged -Path $resolvedOutput -Content $content)
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedOutput))) -ceq $content) `
        "GeneratedOutputMismatch"
}

Write-Output "PostE7EvidenceCampaignsValid:E8=$(@($catalog.playableUnitStabilityCampaigns).Count);E9=$(@($catalog.areaHarmonyCampaigns).Count);Deferred=$(@($catalog.deferredAreaHarmonySubjects).Count);E10=$(@($catalog.limitedOperationWindows).Count);Revision=$($catalog.revision)"
