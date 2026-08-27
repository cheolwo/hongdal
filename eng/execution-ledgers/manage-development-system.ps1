[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",

    [string] $PlayableLoopPath = "eng/execution-ledgers/playable-loops.json",

    [string] $EvidencePackagePath = "eng/execution-ledgers/evidence-packages.json",

    [string] $PresentationValidationModulePath =
        "eng/execution-ledgers/playable-loop-presentation-validation-modules.json",

    [string] $EngineInteractionValidationPath =
        "eng/execution-ledgers/playable-loop-engine-interaction-validation.json",

    [string] $PostE7EvidenceCampaignPath =
        "eng/execution-ledgers/post-e7-evidence-campaigns.json",

    [string] $OutputPath = "docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "DevelopmentSystemInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Resolve-RepositoryPath([string] $RelativePath) {
    return Join-Path $repositoryRoot $RelativePath
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

function Stage-Index([string] $Code) {
    return [Array]::IndexOf($stageCodes, $Code)
}

function Closure-IsCoreReady([string] $Code) {
    return $Code -in @("CoreClosed", "ExtendedClosed", "PlayClosed", "WorldClosed")
}

function Closure-MeetsFinal([object] $Loop) {
    if ([string] $Loop.finalEvidenceStage -eq "E9") {
        return [string] $Loop.closureStateCode -eq "WorldClosed"
    }
    return [string] $Loop.closureStateCode -in @("PlayClosed", "WorldClosed")
}

function Visit-Loop([string] $LoopId) {
    if ($visitedLoops.ContainsKey($LoopId)) { return }
    Require (-not $visitingLoops.ContainsKey($LoopId)) "LoopHierarchyCycle:$LoopId"
    $visitingLoops[$LoopId] = $true
    $loop = $loopById[$LoopId]
    foreach ($childId in @($loop.requiredCoreChildLoopStableIds) +
        @($loop.optionalExtensionChildLoopStableIds)) {
        Visit-Loop ([string] $childId)
    }
    $visitingLoops.Remove($LoopId)
    $visitedLoops[$LoopId] = $true
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedLoops = (Resolve-Path (Resolve-RepositoryPath $PlayableLoopPath)).Path
$resolvedEvidence = (Resolve-Path (Resolve-RepositoryPath $EvidencePackagePath)).Path
$loops = Get-Content -LiteralPath $resolvedLoops -Raw -Encoding UTF8 | ConvertFrom-Json
$evidence = Get-Content -LiteralPath $resolvedEvidence -Raw -Encoding UTF8 | ConvertFrom-Json
$stagesPath = Resolve-RepositoryPath ([string] $loops.evidenceStageCatalogPath)
$wiPath = Resolve-RepositoryPath ([string] $loops.worldInteractionCatalogPath)
$presentationValidationPath = Resolve-RepositoryPath `
    $PresentationValidationModulePath
$stages = Get-Content -LiteralPath $stagesPath -Raw -Encoding UTF8 | ConvertFrom-Json
$wiCatalog = Get-Content -LiteralPath $wiPath -Raw -Encoding UTF8 | ConvertFrom-Json
$presentationValidation = Get-Content -LiteralPath $presentationValidationPath `
    -Raw -Encoding UTF8 | ConvertFrom-Json
$stageCodes = @($stages.stages.code)

$engineInteractionManager = Resolve-RepositoryPath `
    "eng/execution-ledgers/manage-playable-loop-engine-interaction-validation.ps1"
$engineInteractionOutputPath = if ($PlayableLoopPath -eq
    "eng/execution-ledgers/playable-loops.json") {
    "docs/AI/generated/playable-loop-engine-interaction-validation.md"
}
else {
    [IO.Path]::ChangeExtension($OutputPath, ".engine.md")
}
& $engineInteractionManager -Mode $Mode `
    -CatalogPath $EngineInteractionValidationPath `
    -PlayableLoopPath $PlayableLoopPath `
    -OutputPath $engineInteractionOutputPath | Out-Null
$postE7Manager = Resolve-RepositoryPath `
    "eng/execution-ledgers/manage-post-e7-evidence-campaigns.ps1"
$postE7Mode = if ($Mode -eq "Write") { "Write" } else { "Check" }
& $postE7Manager -Mode $postE7Mode `
    -InputPath $PostE7EvidenceCampaignPath | Out-Null

Require ([string] $loops.schemaVersion -eq "ssalddel-playable-loop-catalog.v6") `
    "PlayableLoopSchemaInvalid"
Require ([string] $loops.evidenceModelRevision -eq "horizontal-dual-cycle-evidence.r3") `
    "PlayableLoopEvidenceModelInvalid"
Require ([string] $evidence.schemaVersion -eq "ssalddel-evidence-package-catalog.v3") `
    "EvidencePackageSchemaInvalid"
Require ([string] $evidence.evidenceModelRevision -eq "horizontal-dual-cycle-evidence.r3") `
    "EvidencePackageModelInvalid"
Require ([string] $presentationValidation.schemaVersion -eq `
    "playable-loop-presentation-validation-modules.v1") `
    "PresentationValidationSchemaInvalid"
Require ([string] $loops.evidencePackageCatalogPath -eq $EvidencePackagePath) `
    "EvidencePackageCatalogPathMismatch"
Require ((@($stageCodes) -join ",") -eq
    "E0,E1,E2,E3,E4,E5,E6,E7,E8,E9,E10") `
    "EvidenceStageOrderInvalid"

foreach ($principle in @(
    "loopIsEvidenceSubjectDistinctFromIndividualWi",
    "independentAreaLoopPrecedesCrossAreaIntegration",
    "successRequiresReturnRecoveryOrNextChoice",
    "evidenceStageDoesNotFollowHighestSingleArtifact",
    "plannedLoopMayExposeMissingWorldInteractions",
    "areaAggregateDerivedFromRequiredCoreChildren",
    "extensionDoesNotBlockCoreClosure",
    "logicAndPresentationMaturityRemainDistinct",
    "integratedEvidenceStageUsesLowerTrack",
    "presentationFeedbackReopensEarliestAffectedStage",
    "worldInteractionPolarityDoesNotClassifyDevelopmentTrack",
    "playableUnitVerticalMaturityEndsAtE7",
    "playableUnitHorizontalStabilityUsesE8",
    "areaAggregateHarmonyStartsAtE9",
    "topicDesignPrecedesGoalActivation",
    "topicMapsToExactlyOnePlayableUnit")) {
    $property = $loops.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) `
        "PlayableLoopPrincipleMissing:$principle"
}
foreach ($principle in @(
    "artifactScopeDoesNotPromoteSubjectAutomatically",
    "codeTestRuntimeAndOperationsEvidenceRemainDistinct",
    "evidenceIsBoundToRevisionAndEnvironment",
    "invalidationTriggersMustBeExplicit",
    "localArtifactMayBeUnavailableOnAnotherWorktree",
    "logicPresentationAndIntegratedEvidenceRemainDistinct")) {
    $property = $evidence.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) `
        "EvidencePackagePrincipleMissing:$principle"
}

$dualPolicy = $loops.dualMaturityPolicy
Require ([string] $dualPolicy.rolloutCode -eq "AllPlayableUnits") `
    "DualMaturityRolloutInvalid"
Require ([string] $dualPolicy.requiredForLoopLevelCode -eq "PlayableUnit") `
    "DualMaturityRequiredLevelInvalid"
Require ((@($dualPolicy.allowedTrackCodes) -join ",") -eq "Logic,Presentation") `
    "DualMaturityTrackCodesInvalid"
Require ((@($evidence.allowedEvidenceTrackCodes) -join ",") -eq `
    "Logic,Presentation,Integrated") "EvidenceTrackCodesInvalid"
Require ([string] $dualPolicy.presentationPlanningMayAdvanceThroughStage -eq "E4") `
    "PresentationPlanningGateInvalid"
Require ([string] $dualPolicy.presentationEvidenceRequiresLogicStageFrom -eq "E5") `
    "PresentationLogicGateInvalid"
Require ([string] $dualPolicy.playClosedRequiresBothTracksAtLeast -eq "E7") `
    "DualMaturityPlayClosureGateInvalid"

foreach ($principle in @(
    "commonModulesApplyToEveryPlayableUnit",
    "featureModulesAreSelectedByDeclaredCapability",
    "blockingFailurePreventsTrackPromotion",
    "warningDoesNotPromoteEvidence",
    "runtimeEvidenceDoesNotReplaceAutomatedPrecheck",
    "failureReopensEarliestOwningStage",
    "presentationValidationDoesNotMutateSimulationAuthority")) {
    $property = $presentationValidation.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) `
        "PresentationValidationPrincipleMissing:$principle"
}
Require ([string] $presentationValidation.trackCode -eq "Presentation") `
    "PresentationValidationTrackInvalid"
$presentationModuleByCode = @{}
foreach ($module in @($presentationValidation.modules)) {
    $moduleCode = [string] $module.moduleCode
    Require-Text $moduleCode "PresentationValidationModuleCodeMissing"
    Require (-not $presentationModuleByCode.ContainsKey($moduleCode)) `
        "PresentationValidationModuleDuplicate:$moduleCode"
    Require-Text $module.displayNameKo `
        "PresentationValidationModuleNameMissing:$moduleCode"
    Require (@($presentationValidation.allowedEvidenceStageCodes) -contains `
        [string] $module.evidenceStageCode) `
        "PresentationValidationStageInvalid:$moduleCode"
    Require (@($presentationValidation.allowedApplicabilityCodes) -contains `
        [string] $module.applicabilityCode) `
        "PresentationValidationApplicabilityInvalid:$moduleCode"
    Require (@($presentationValidation.allowedSeverityCodes) -contains `
        [string] $module.severityCode) `
        "PresentationValidationSeverityInvalid:$moduleCode"
    Require (@($presentationValidation.allowedAutomationLevelCodes) -contains `
        [string] $module.automationLevelCode) `
        "PresentationValidationAutomationInvalid:$moduleCode"
    Require (@($module.reads).Count -gt 0) `
        "PresentationValidationReadsMissing:$moduleCode"
    Require (@($module.failureReasonCodes).Count -gt 0) `
        "PresentationValidationFailureCodeMissing:$moduleCode"
    if ([string] $module.applicabilityCode -eq "Common") {
        Require (@($module.requiredFeatureCodes).Count -eq 0) `
            "PresentationValidationCommonHasFeature:$moduleCode"
    }
    else {
        Require (@($module.requiredFeatureCodes).Count -gt 0) `
            "PresentationValidationFeatureMissing:$moduleCode"
        foreach ($featureCode in @($module.requiredFeatureCodes)) {
            Require (@($presentationValidation.featureCodes) -contains `
                [string] $featureCode) `
                "PresentationValidationFeatureUnknown:${moduleCode}:$featureCode"
        }
    }
    $presentationModuleByCode[$moduleCode] = $module
}
foreach ($moduleCode in @($presentationValidation.commonModuleCodes)) {
    Require ($presentationModuleByCode.ContainsKey([string] $moduleCode)) `
        "PresentationValidationCommonModuleUnknown:$moduleCode"
    Require ([string] $presentationModuleByCode[[string] $moduleCode].applicabilityCode `
        -eq "Common") "PresentationValidationCommonModuleNotCommon:$moduleCode"
}
foreach ($stageCode in @($presentationValidation.allowedEvidenceStageCodes)) {
    Require (@($presentationValidation.commonModuleCodes | Where-Object {
        [string] $presentationModuleByCode[[string] $_].evidenceStageCode `
            -eq [string] $stageCode }).Count -gt 0) `
        "PresentationValidationCommonStageMissing:$stageCode"
}

$packageById = @{}
foreach ($package in @($evidence.packages)) {
    $id = [string] $package.evidenceId
    Require-Text $id "EvidenceIdMissing"
    Require (-not $packageById.ContainsKey($id)) "EvidenceDuplicate:$id"
    Require-Text $package.title "EvidenceTitleMissing:$id"
    Require (@($evidence.allowedEvidenceKindCodes) -contains `
        [string] $package.evidenceKindCode) "EvidenceKindInvalid:$id"
    Require (@($evidence.allowedResultCodes) -contains `
        [string] $package.resultCode) "EvidenceResultInvalid:$id"
    Require (@($evidence.allowedStatusCodes) -contains `
        [string] $package.statusCode) "EvidenceStatusInvalid:$id"
    Require (@($package.subjectRefs).Count -gt 0) "EvidenceSubjectMissing:$id"
    Require (@($package.evidenceStageCodes).Count -gt 0) "EvidenceStageMissing:$id"
    foreach ($stage in @($package.evidenceStageCodes)) {
        Require (@($stageCodes) -contains [string] $stage) `
            "EvidenceStageInvalid:${id}:$stage"
    }
    foreach ($field in @("sourceRevision", "executedAt", "environment", "scope")) {
        Require-Text $package.$field "EvidenceFieldMissing:${id}:$field"
    }
    Require (@($package.artifactReferences).Count -gt 0) "EvidenceArtifactMissing:$id"
    Require (@($package.invalidationTriggers).Count -gt 0) `
        "EvidenceInvalidationTriggerMissing:$id"
    $trackProperty = $package.PSObject.Properties["evidenceTrackCode"]
    Require ($null -ne $trackProperty) "EvidenceTrackMissing:$id"
    Require (@($evidence.allowedEvidenceTrackCodes) -contains `
        [string] $trackProperty.Value) "EvidenceTrackInvalid:$id"
    foreach ($artifact in @($package.artifactReferences)) {
        Require (@($evidence.allowedLocationKinds) -contains `
            [string] $artifact.locationKind) "EvidenceLocationKindInvalid:$id"
        Require (@($evidence.allowedRetentionCodes) -contains `
            [string] $artifact.retentionCode) "EvidenceRetentionInvalid:$id"
        Require-Text $artifact.locator "EvidenceLocatorMissing:$id"
        $hash = [string] $artifact.sha256
        Require ([string]::IsNullOrWhiteSpace($hash) -or $hash -match "^[0-9a-fA-F]{64}$") `
            "EvidenceHashInvalid:$id"

        $artifactPath = if ([string] $artifact.locationKind -eq "ExternalCheckout") {
            [string] $artifact.locator
        }
        else {
            Resolve-RepositoryPath ([string] $artifact.locator)
        }
        if ([string] $artifact.retentionCode -eq "Tracked") {
            Require (Test-Path -LiteralPath $artifactPath) `
                "TrackedEvidenceArtifactMissing:${id}:$($artifact.locator)"
        }
        $mustVerifyHash = [string] $artifact.retentionCode -eq "Tracked" -or
            [string] $package.statusCode -eq "Current"
        if ($mustVerifyHash -and (Test-Path -LiteralPath $artifactPath) -and
            -not [string]::IsNullOrWhiteSpace($hash)) {
            $actualHash = (Get-FileHash -LiteralPath $artifactPath -Algorithm SHA256).Hash
            Require ($actualHash.Equals($hash, [StringComparison]::OrdinalIgnoreCase)) `
                "EvidenceArtifactHashMismatch:${id}:$($artifact.locator)"
        }
    }
    $packageById[$id] = $package
}

$wiIds = @($wiCatalog.items.id)
$loopById = @{}
foreach ($loop in @($loops.items)) {
    $id = [string] $loop.loopStableId
    Require-Text $id "LoopStableIdMissing"
    Require (-not $loopById.ContainsKey($id)) "LoopDuplicate:$id"
    Require ([int] $loop.priority -gt 0) "LoopPriorityInvalid:$id"
    Require-Text $loop.title "LoopTitleMissing:$id"
    Require-Text $loop.playerPromise "LoopPlayerPromiseMissing:$id"
    Require (@($loops.allowedLoopKinds) -contains [string] $loop.loopKind) `
        "LoopKindInvalid:$id"
    Require (@($loops.allowedLoopLevelCodes) -contains [string] $loop.loopLevelCode) `
        "LoopLevelInvalid:$id"
    Require (@($loops.allowedCompletionTierCodes) -contains `
        [string] $loop.completionTierCode) "LoopCompletionTierInvalid:$id"
    Require (@($loops.allowedClosureStateCodes) -contains `
        [string] $loop.closureStateCode) "LoopClosureStateInvalid:$id"
    Require (@($loops.allowedStatusCodes) -contains [string] $loop.statusCode) `
        "LoopStatusInvalid:$id"
    Require (@($loop.areaSetStableIds).Count -gt 0) "LoopAreaSetMissing:$id"
    Require (@($loop.entryStateCodes).Count -gt 0) "LoopEntryMissing:$id"
    Require (@($loop.successStateCodes).Count -gt 0) "LoopSuccessMissing:$id"
    Require (@($loop.returnStateCodes).Count -gt 0) "LoopReturnMissing:$id"
    Require (@($loop.requiredHCapabilities).Count -gt 0) "LoopHCapabilityMissing:$id"
    Require-Text $loop.nextAction "LoopNextActionMissing:$id"
    $currentIndex = Stage-Index ([string] $loop.currentEvidenceStage)
    $nextIndex = Stage-Index ([string] $loop.nextClosureTargetStage)
    $finalIndex = Stage-Index ([string] $loop.finalEvidenceStage)
    Require ($currentIndex -ge 0) "LoopCurrentEvidenceStageInvalid:$id"
    Require ($nextIndex -ge $currentIndex) "LoopNextEvidenceStageInvalid:$id"
    Require ($finalIndex -ge $nextIndex) "LoopFinalEvidenceStageInvalid:$id"

    $isUnit = [string] $loop.loopLevelCode -eq "PlayableUnit"
    if ($isUnit) {
        Require ($null -ne $loop.PSObject.Properties["planningGate"]) `
            "PlanningGateMissing:$id"
        $maturityProperty = $loop.PSObject.Properties["maturityTracks"]
        Require ($null -ne $maturityProperty) "DualMaturityMissing:$id"
        $logic = $loop.maturityTracks.logic
        $presentation = $loop.maturityTracks.presentation
        foreach ($track in @($logic, $presentation)) {
            Require (@($dualPolicy.allowedTrackCodes) -contains `
                [string] $track.trackCode) "DualMaturityTrackInvalid:$id"
            Require (@($dualPolicy.allowedTrackStatusCodes) -contains `
                [string] $track.statusCode) "DualMaturityStatusInvalid:$id"
            $trackCurrentIndex = Stage-Index ([string] $track.currentStage)
            $trackTargetIndex = Stage-Index ([string] $track.targetStage)
            Require ($trackCurrentIndex -ge 0) "DualMaturityCurrentStageInvalid:$id"
            Require ($trackTargetIndex -ge $trackCurrentIndex) `
                "DualMaturityTargetStageInvalid:$id"
            foreach ($evidenceRef in @($track.evidencePackageRefs)) {
                Require ($packageById.ContainsKey([string] $evidenceRef)) `
                    "DualMaturityEvidenceUnknown:${id}:$evidenceRef"
                Require (@($packageById[[string] $evidenceRef].subjectRefs) -contains $id) `
                    "DualMaturityEvidenceSubjectInvalid:${id}:$evidenceRef"
                Require ([string] $packageById[[string] $evidenceRef].evidenceTrackCode `
                    -in @([string] $track.trackCode, "Integrated")) `
                    "DualMaturityEvidenceTrackInvalid:${id}:$evidenceRef"
            }
            if ([string] $track.statusCode -eq "Blocked") {
                Require (@($track.blockers).Count -gt 0) `
                    "DualMaturityBlockedWithoutReason:$id"
            }
        }
        Require ([string] $logic.trackCode -eq "Logic") "LogicTrackCodeInvalid:$id"
        Require ([string] $presentation.trackCode -eq "Presentation") `
            "PresentationTrackCodeInvalid:$id"
        $logicIndex = Stage-Index ([string] $logic.currentStage)
        $presentationIndex = Stage-Index ([string] $presentation.currentStage)
        Require ($currentIndex -eq [Math]::Min($logicIndex, $presentationIndex)) `
            "IntegratedEvidenceStageMismatch:$id"
        if ($presentationIndex -ge (Stage-Index `
            ([string] $dualPolicy.presentationEvidenceRequiresLogicStageFrom))) {
            Require ($logicIndex -ge (Stage-Index `
                ([string] $dualPolicy.presentationEvidenceRequiresLogicStageFrom))) `
                "PresentationAdvancedBeforeLogicReady:$id"
        }
        foreach ($feedback in @($loop.openFeedbackItems)) {
            Require-Text $feedback.feedbackId "FeedbackIdMissing:$id"
            Require (@($dualPolicy.allowedFeedbackReasonCodes) -contains `
                [string] $feedback.reasonCode) "FeedbackReasonInvalid:$id"
            Require (@($dualPolicy.allowedTrackCodes) -contains `
                [string] $feedback.targetTrackCode) "FeedbackTargetTrackInvalid:$id"
            Require ((Stage-Index ([string] $feedback.detectedAtStage)) -ge 0) `
                "FeedbackDetectedStageInvalid:$id"
            Require ((Stage-Index ([string] $feedback.reopenStage)) -ge 0) `
                "FeedbackReopenStageInvalid:$id"
            Require ((Stage-Index ([string] $feedback.reopenStage)) -le `
                (Stage-Index ([string] $feedback.detectedAtStage))) `
                "FeedbackReopenAfterDetection:$id"
            Require (@($feedback.targetRefs).Count -gt 0) "FeedbackTargetMissing:$id"
            Require ([string] $feedback.statusCode -eq "Open") `
                "OpenFeedbackStatusInvalid:$id"
        }
        if ([string] $loop.closureStateCode -in @("PlayClosed", "WorldClosed")) {
            $playGate = Stage-Index ([string] $dualPolicy.playClosedRequiresBothTracksAtLeast)
            Require ($logicIndex -ge $playGate -and $presentationIndex -ge $playGate) `
                "DualMaturityPlayClosureBelowGate:$id"
            Require (@($loop.openFeedbackItems).Count -eq 0) `
                "DualMaturityPlayClosureHasFeedback:$id"
        }
    }
    else {
        Require ($null -eq $loop.PSObject.Properties["planningGate"]) `
            "AggregateHasPlanningGate:$id"
        Require ($null -eq $loop.PSObject.Properties["maturityTracks"]) `
            "DualMaturityOnAggregate:$id"
    }

    $isAggregate = -not $isUnit
    if ($isUnit) {
        Require ([string] $loop.finalEvidenceStage -eq "E7") `
            "PlayableUnitFinalStageMustBeE7:$id"
        Require-Text $loop.parentLoopStableId "LoopParentMissing:$id"
        Require (@($loop.requiredCoreChildLoopStableIds).Count -eq 0) `
            "LoopUnitHasCoreChild:$id"
        Require (@($loop.optionalExtensionChildLoopStableIds).Count -eq 0) `
            "LoopUnitHasExtensionChild:$id"
        Require (@($loop.worldInteractionIds).Count -gt 0) "LoopWiMissing:$id"
    }
    else {
        Require ([string] $loop.finalEvidenceStage -eq "E9") `
            "AggregateFinalStageMustBeE9:$id"
        Require ([string]::IsNullOrWhiteSpace([string] $loop.parentLoopStableId)) `
            "LoopAggregateHasParent:$id"
        Require ([string] $loop.completionTierCode -eq "Aggregate") `
            "LoopAggregateTierInvalid:$id"
        Require (@($loop.requiredCoreChildLoopStableIds).Count -gt 0) `
            "LoopAggregateCoreChildMissing:$id"
    }

    foreach ($wiId in @($loop.worldInteractionIds)) {
        Require (@($wiIds) -contains [string] $wiId) "LoopWiUnknown:${id}:$wiId"
    }
    foreach ($wiId in @($loop.failureRecoveryWorldInteractionIds)) {
        Require (@($loop.worldInteractionIds) -contains [string] $wiId) `
            "LoopRecoveryWiOutsideLoop:${id}:$wiId"
    }

    $isIndependent = [string] $loop.loopKind -eq "IndependentArea"
    Require ([bool] $loop.independentExecutionRequired -eq $isIndependent) `
        "LoopIndependentExecutionMismatch:$id"
    if ($currentIndex -ge (Stage-Index "E3")) {
        Require (@($loop.evidencePackageRefs).Count -gt 0) `
            "LoopE3NeedsEvidencePackage:$id"
    }
    foreach ($evidenceRef in @($loop.evidencePackageRefs)) {
        Require ($packageById.ContainsKey([string] $evidenceRef)) `
            "LoopEvidenceUnknown:${id}:$evidenceRef"
        Require (@($packageById[[string] $evidenceRef].subjectRefs) -contains $id) `
            "EvidenceSubjectDoesNotIncludeLoop:${id}:$evidenceRef"
    }
    $activeWorkOrderRefs = if ($null -ne `
        $loop.PSObject.Properties["workOrderRefs"]) {
        @($loop.workOrderRefs)
    }
    else { @() }
    foreach ($workOrderRef in $activeWorkOrderRefs) {
        Require ([string] $workOrderRef -match '\.e7-work-order\.json$') `
            "LoopActiveWorkOrderMustBeE7:${id}:$workOrderRef"
        $workOrderPath = Resolve-RepositoryPath ([string] $workOrderRef)
        Require (Test-Path -LiteralPath $workOrderPath) `
            "LoopWorkOrderMissing:${id}:$workOrderRef"
        $workOrder = Get-Content -LiteralPath $workOrderPath -Raw -Encoding UTF8 |
            ConvertFrom-Json
        Require-Text $workOrder.workOrderId "LoopWorkOrderIdMissing:${id}:$workOrderRef"
        Require ([string] $workOrder.schemaVersion -eq `
            "simulation-e7-vertical-work-order.v2") `
            "LoopWorkOrderSchemaInvalid:${id}:$workOrderRef"
        Require ([string] $workOrder.playableUnitStableId -eq $id) `
            "WorkOrderDoesNotIncludeLoop:${id}:$workOrderRef"
        & (Resolve-RepositoryPath `
            "eng/execution-ledgers/manage-e7-vertical-work-order.ps1") `
            -InputPath ([string] $workOrderRef) | Out-Null
    }
    $legacyWorkOrderRefs = if ($null -ne `
        $loop.PSObject.Properties["legacyWorkOrderRefs"]) {
        @($loop.legacyWorkOrderRefs)
    }
    else { @() }
    foreach ($legacyWorkOrderRef in $legacyWorkOrderRefs) {
        Require ([string] $legacyWorkOrderRef -match '\.e9-work-order\.json$') `
            "LoopLegacyWorkOrderMustBeE9:${id}:$legacyWorkOrderRef"
        $legacyWorkOrderPath = Resolve-RepositoryPath ([string] $legacyWorkOrderRef)
        Require (Test-Path -LiteralPath $legacyWorkOrderPath) `
            "LoopLegacyWorkOrderMissing:${id}:$legacyWorkOrderRef"
    }

    switch ([string] $loop.closureStateCode) {
        "CoreClosed" {
            Require ([string] $loop.completionTierCode -in @("Core", "Aggregate")) `
                "LoopCoreClosureTierInvalid:$id"
            Require ($currentIndex -ge (Stage-Index "E5")) "LoopCoreClosureBelowE5:$id"
        }
        "ExtendedClosed" {
            Require ([string] $loop.completionTierCode -in @("Extension", "Aggregate")) `
                "LoopExtensionClosureTierInvalid:$id"
            Require ($currentIndex -ge (Stage-Index "E5")) `
                "LoopExtensionClosureBelowE5:$id"
        }
        "PlayClosed" {
            Require ($currentIndex -ge (Stage-Index "E7")) "LoopPlayClosureBelowE7:$id"
        }
        "WorldClosed" {
            Require ($currentIndex -ge (Stage-Index "E9")) "LoopWorldClosureBelowE9:$id"
        }
    }

    if ([string] $loop.statusCode -eq "Validated") {
        Require ($currentIndex -eq $finalIndex) "ValidatedLoopStageMismatch:$id"
        Require (Closure-MeetsFinal $loop) "ValidatedLoopNotFinalClosed:$id"
        Require (@($loop.blockers).Count -eq 0) "ValidatedLoopHasBlocker:$id"
    }
    elseif ($currentIndex -lt $finalIndex -or -not (Closure-MeetsFinal $loop)) {
        Require (@($loop.blockers).Count -gt 0) "IncompleteLoopNeedsBlocker:$id"
    }
    $loopById[$id] = $loop
}

$unitOwnerById = @{}
foreach ($loop in @($loops.items)) {
    $id = [string] $loop.loopStableId
    $isUnit = [string] $loop.loopLevelCode -eq "PlayableUnit"
    foreach ($childId in @($loop.requiredCoreChildLoopStableIds)) {
        $childKey = [string] $childId
        Require ($loopById.ContainsKey($childKey)) "LoopCoreChildUnknown:${id}:$childKey"
        $child = $loopById[$childKey]
        Require ([string] $child.completionTierCode -in @("Core", "Aggregate")) `
            "LoopCoreChildTierInvalid:${id}:$childKey"
        if ([string] $child.loopLevelCode -eq "PlayableUnit") {
            Require (-not $unitOwnerById.ContainsKey($childKey)) `
                "LoopUnitHasMultipleParents:$childKey"
            $unitOwnerById[$childKey] = $id
        }
    }
    foreach ($childId in @($loop.optionalExtensionChildLoopStableIds)) {
        $childKey = [string] $childId
        Require ($loopById.ContainsKey($childKey)) `
            "LoopExtensionChildUnknown:${id}:$childKey"
        $child = $loopById[$childKey]
        Require ([string] $child.completionTierCode -eq "Extension") `
            "LoopExtensionChildTierInvalid:${id}:$childKey"
        Require ([string] $child.loopLevelCode -eq "PlayableUnit") `
            "LoopExtensionChildLevelInvalid:${id}:$childKey"
        Require (-not $unitOwnerById.ContainsKey($childKey)) `
            "LoopUnitHasMultipleParents:$childKey"
        $unitOwnerById[$childKey] = $id
    }

    if (-not $isUnit) {
        $childStages = @($loop.requiredCoreChildLoopStableIds | ForEach-Object {
            Stage-Index ([string] $loopById[[string] $_].currentEvidenceStage)
        })
        $minimumChildStage = ($childStages | Measure-Object -Minimum).Minimum
        $aggregateStage = Stage-Index ([string] $loop.currentEvidenceStage)
        if ([string] $loop.loopLevelCode -eq "AreaAggregate" -and
            $aggregateStage -eq (Stage-Index "E9")) {
            Require ($minimumChildStage -ge (Stage-Index "E7")) `
                "LoopAreaHarmonyRequiresE7ChildrenAndE8Campaigns:$id"
        }
        else {
            Require ($aggregateStage -le $minimumChildStage) `
                "LoopAggregateStageExceedsChild:$id"
        }

        if ([string] $loop.closureStateCode -in @("CoreClosed", "ExtendedClosed", "PlayClosed", "WorldClosed")) {
            foreach ($childId in @($loop.requiredCoreChildLoopStableIds)) {
                $childClosure = [string] $loopById[[string] $childId].closureStateCode
                Require (Closure-IsCoreReady $childClosure) `
                    "LoopAggregateCoreClosureInvalid:${id}:$childId"
            }
        }
        if ([string] $loop.closureStateCode -eq "ExtendedClosed") {
            foreach ($childId in @($loop.optionalExtensionChildLoopStableIds)) {
                Require ([string] $loopById[[string] $childId].closureStateCode -in `
                    @("ExtendedClosed", "PlayClosed", "WorldClosed")) `
                    "LoopAggregateExtensionClosureInvalid:${id}:$childId"
            }
        }
        if ([string] $loop.closureStateCode -eq "PlayClosed") {
            foreach ($childId in @($loop.requiredCoreChildLoopStableIds)) {
                Require ([string] $loopById[[string] $childId].closureStateCode -in `
                    @("PlayClosed", "WorldClosed")) "LoopAggregatePlayClosureInvalid:${id}:$childId"
            }
        }
        if ([string] $loop.closureStateCode -eq "WorldClosed") {
            foreach ($childId in @($loop.requiredCoreChildLoopStableIds)) {
                Require (Closure-MeetsFinal $loopById[[string] $childId]) `
                    "LoopAggregateWorldClosureInvalid:${id}:$childId"
            }
        }
    }
    if ($null -ne $loop.PSObject.Properties["predecessorLoopStableIds"]) {
        foreach ($predecessorId in @($loop.predecessorLoopStableIds)) {
            Require ($loopById.ContainsKey([string] $predecessorId)) `
                "LoopPredecessorUnknown:${id}:$predecessorId"
        }
    }
}

foreach ($loop in @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit")) {
    $id = [string] $loop.loopStableId
    Require ($unitOwnerById.ContainsKey($id)) "LoopUnitOwnerMissing:$id"
    Require ([string] $unitOwnerById[$id] -eq [string] $loop.parentLoopStableId) `
        "LoopParentBackReferenceMismatch:$id"
    Require ([string] $loopById[[string] $loop.parentLoopStableId].loopLevelCode -eq `
        "AreaAggregate") "LoopUnitParentLevelInvalid:$id"
}

$visitingLoops = @{}
$visitedLoops = @{}
foreach ($id in @($loopById.Keys)) { Visit-Loop ([string] $id) }

$presentationProfileByLoopId = @{}
foreach ($profile in @($presentationValidation.loopProfiles)) {
    $loopId = [string] $profile.loopStableId
    Require-Text $loopId "PresentationValidationProfileLoopMissing"
    Require (-not $presentationProfileByLoopId.ContainsKey($loopId)) `
        "PresentationValidationProfileDuplicate:$loopId"
    Require ($loopById.ContainsKey($loopId)) `
        "PresentationValidationProfileLoopUnknown:$loopId"
    Require ([string] $loopById[$loopId].loopLevelCode -eq "PlayableUnit") `
        "PresentationValidationProfileNotPlayableUnit:$loopId"
    $profileFeatures = @($profile.featureCodes | ForEach-Object { [string] $_ })
    Require ($profileFeatures.Count -eq `
        @($profileFeatures | Sort-Object -Unique).Count) `
        "PresentationValidationProfileFeatureDuplicate:$loopId"
    foreach ($featureCode in $profileFeatures) {
        Require (@($presentationValidation.featureCodes) -contains $featureCode) `
            "PresentationValidationProfileFeatureUnknown:${loopId}:$featureCode"
        Require (@($presentationValidation.modules | Where-Object {
            [string] $_.applicabilityCode -eq "Feature" -and
            @($_.requiredFeatureCodes) -contains $featureCode }).Count -gt 0) `
            "PresentationValidationProfileFeatureUnmapped:${loopId}:$featureCode"
    }
    $presentationProfileByLoopId[$loopId] = $profile
}
$resolvedPresentationModulesByLoopId = @{}
foreach ($loop in @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit")) {
    $loopId = [string] $loop.loopStableId
    $profileFeatures = if ($presentationProfileByLoopId.ContainsKey($loopId)) {
        @($presentationProfileByLoopId[$loopId].featureCodes |
            ForEach-Object { [string] $_ })
    }
    else { @() }
    $resolvedCodes = [Collections.Generic.List[string]]::new()
    foreach ($moduleCode in @($presentationValidation.commonModuleCodes)) {
        $resolvedCodes.Add([string] $moduleCode)
    }
    foreach ($module in @($presentationValidation.modules | Where-Object {
        [string] $_.applicabilityCode -eq "Feature"
    })) {
        $requiredFeatures = @($module.requiredFeatureCodes |
            ForEach-Object { [string] $_ })
        if (@($requiredFeatures | Where-Object {
            $profileFeatures -notcontains $_ }).Count -eq 0) {
            $resolvedCodes.Add([string] $module.moduleCode)
        }
    }
    Require ($resolvedCodes.Count -ge `
        @($presentationValidation.commonModuleCodes).Count) `
        "PresentationValidationResolutionMissingCommon:$loopId"
    $resolvedPresentationModulesByLoopId[$loopId] = @($resolvedCodes |
        Sort-Object -Unique)
}

foreach ($package in @($evidence.packages)) {
    foreach ($subjectRef in @($package.subjectRefs)) {
        Require ($loopById.ContainsKey([string] $subjectRef)) `
            "EvidenceLoopSubjectUnknown:$($package.evidenceId):$subjectRef"
    }
}

$orderedLoops = @($loops.items | Sort-Object priority, loopStableId)
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# 현재 플레이 폐루프 완결 원장")
[void] $builder.AppendLine()
[void] $builder.AppendLine(('> 이 문서는 `{0}`와 `{1}`에서 자동 생성된다. 직접 수정하지 않는다.' -f
    $PlayableLoopPath, $EvidencePackagePath))
[void] $builder.AppendLine("> `CoreClosed`는 E5 핵심 폐루프, `ExtendedClosed`는 선택형 확장 E5, `PlayClosed`는 E7 핵심 플레이 폐루프, `WorldClosed`는 E8 개별 안정성을 거친 둘 이상의 Core가 E9 영역 조화·사람 승인을 통과한 집계를 뜻한다.")
[void] $builder.AppendLine("> Nature 시범 루프는 논리·시각 성숙도를 별도로 기록하며 통합 E는 두 축 중 낮은 단계다. WI 음양 분류는 개발 성숙도 축과 무관하다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine(('- 플레이 폐루프 대장: `{0}`' -f $loops.revision))
[void] $builder.AppendLine(('- 증거 묶음 대장: `{0}`' -f $evidence.revision))
[void] $builder.AppendLine(('- 플레이 단위·집계: `{0}`' -f $orderedLoops.Count))
[void] $builder.AppendLine(('- 등록 증거 묶음: `{0}`' -f @($evidence.packages).Count))
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 판정 기준")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 기준 | 판정 |")
[void] $builder.AppendLine("| --- | --- |")
[void] $builder.AppendLine("| 핵심 완결 | 모든 필수 Core 자식이 E5 `CoreClosed` 이상이어야 영역 집계가 닫힘 |")
[void] $builder.AppendLine("| 확장 완결 | 건물·타로·배치 같은 선택형 자식은 `ExtendedClosed`로 별도 기록하며 Core를 막지 않음 |")
[void] $builder.AppendLine("| 플레이 완결 | 실제 입력·화면·귀환까지 E7이 확인된 자식만 `PlayClosed` |")
[void] $builder.AppendLine("| 이중 순환 | Nature 시범 루프는 논리 E와 시각 E가 모두 E7이고 열린 환류가 없어야 `PlayClosed` |")
[void] $builder.AppendLine("| 개별 안정 | E7 PlayableUnit이 반복 결정성·Save/Replay·Local/Remote·실제 입력 재진입을 통과하면 별도 E8 캠페인으로 기록 |")
[void] $builder.AppendLine("| 세계 완결 | E8을 통과한 둘 이상의 Core가 영역 조화와 사람 승인을 E9에서 통과한 집계만 `WorldClosed` |")
[void] $builder.AppendLine("| 독립 영역 우선 | 플레이어 연속성 기준 Nature→Farm→Hub→Town→City의 내부 폐루프를 먼저 닫고 영역 간 연결은 뒤로 미룸 |")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 전체 상태")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 우선 | 폐루프 | 수준·등급 | WI | 논리 E | 시각 E | 통합→다음/최종 E | 완결 | 상태 | 증거 | 다음 행동 |")
[void] $builder.AppendLine("| ---: | --- | --- | ---: | --- | --- | --- | --- | --- | ---: | --- |")
foreach ($loop in $orderedLoops) {
    $maturity = $loop.PSObject.Properties["maturityTracks"]
    $logicStage = if ($null -ne $maturity) {
        [string] $loop.maturityTracks.logic.currentStage
    } else { "-" }
    $presentationStage = if ($null -ne $maturity) {
        [string] $loop.maturityTracks.presentation.currentStage
    } else { "-" }
    [void] $builder.AppendLine(('| {0} | `{1}` {2} | {3}/{4} | {5} | {6} | {7} | {8}→{9}/{10} | {11} | {12} | {13} | {14} |' -f
        [int] $loop.priority,
        (Escape-Cell $loop.loopStableId),
        (Escape-Cell $loop.title),
        (Escape-Cell $loop.loopLevelCode),
        (Escape-Cell $loop.completionTierCode),
        @($loop.worldInteractionIds).Count,
        (Escape-Cell $logicStage),
        (Escape-Cell $presentationStage),
        (Escape-Cell $loop.currentEvidenceStage),
        (Escape-Cell $loop.nextClosureTargetStage),
        (Escape-Cell $loop.finalEvidenceStage),
        (Escape-Cell $loop.closureStateCode),
        (Escape-Cell $loop.statusCode),
        @($loop.evidencePackageRefs).Count,
        (Escape-Cell $loop.nextAction)))
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 영역 집계와 자식 상태")
[void] $builder.AppendLine()
foreach ($loop in @($orderedLoops | Where-Object loopLevelCode -eq "AreaAggregate")) {
    [void] $builder.AppendLine(('- **{0}** `{1}`' -f
        (Escape-Cell $loop.title), (Escape-Cell $loop.closureStateCode)))
    foreach ($childId in @($loop.requiredCoreChildLoopStableIds)) {
        $child = $loopById[[string] $childId]
        [void] $builder.AppendLine(('  - 필수 Core: `{0}` — {1}, {2}' -f
            $childId, $child.currentEvidenceStage, $child.closureStateCode))
    }
    foreach ($childId in @($loop.optionalExtensionChildLoopStableIds)) {
        $child = $loopById[[string] $childId]
        [void] $builder.AppendLine(('  - 선택 Extension: `{0}` — {1}, {2}' -f
            $childId, $child.currentEvidenceStage, $child.closureStateCode))
    }
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 현재 닫힌 E5 단위")
[void] $builder.AppendLine()
foreach ($loop in @($orderedLoops | Where-Object closureStateCode -in @("CoreClosed", "ExtendedClosed"))) {
    [void] $builder.AppendLine(('- `{0}` — {1} ({2})' -f
        $loop.loopStableId, $loop.closureStateCode, $loop.currentEvidenceStage))
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 열린 경계")
[void] $builder.AppendLine()
foreach ($loop in @($orderedLoops | Where-Object {
    [string] $_.closureStateCode -eq "Open" -and
    [string] $_.loopLevelCode -eq "PlayableUnit" })) {
    [void] $builder.AppendLine(('- `{0}`' -f $loop.loopStableId))
    foreach ($blocker in @($loop.blockers)) {
        [void] $builder.AppendLine("  - $(Escape-Cell $blocker)")
    }
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 보류 또는 독립 준비 후 통합")
[void] $builder.AppendLine()
foreach ($loop in @($orderedLoops | Where-Object statusCode -in @("Deferred", "Planned"))) {
    [void] $builder.AppendLine(('- `{0}`: {1}' -f $loop.loopStableId,
        (Escape-Cell $loop.nextAction)))
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 현재 증거 묶음")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 증거 | 축 | 종류 | 결과·상태 | 대상 E | 범위 | 제외 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |")
foreach ($package in @($evidence.packages | Sort-Object evidenceId)) {
    $trackProperty = $package.PSObject.Properties["evidenceTrackCode"]
    $trackCode = if ($null -ne $trackProperty) {
        [string] $trackProperty.Value
    } else { "미분류" }
    [void] $builder.AppendLine(('| `{0}` {1} | {2} | {3} | {4}/{5} | {6} | {7} | {8} |' -f
        (Escape-Cell $package.evidenceId),
        (Escape-Cell $package.title),
        (Escape-Cell $trackCode),
        (Escape-Cell $package.evidenceKindCode),
        (Escape-Cell $package.resultCode),
        (Escape-Cell $package.statusCode),
        (Escape-Cell (@($package.evidenceStageCodes) -join ", ")),
        (Escape-Cell $package.scope),
        (Escape-Cell (@($package.exclusions) -join " / "))))
}

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 증거 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 개별 WI 구현 E, PlayableUnit E와 영역 집계 E를 서로 대신하지 않는다.")
[void] $builder.AppendLine("- H 공간 조립은 필요한 공간 능력의 조건부 증거이며 E5·E7을 자동 승격하지 않는다.")
[void] $builder.AppendLine("- 자동 시험, Unity Play Mode, Game View, Hosted 동등성과 운영 효과를 별도 EvidencePackage로 기록한다.")
[void] $builder.AppendLine('- 증거가 `Stale`이면 과거 범위는 보존하되 현재 완결 판정의 단독 근거로 확대하지 않는다.')

[void] $builder.AppendLine()
[void] $builder.AppendLine("## 현재 최우선 실행 순서")
[void] $builder.AppendLine()
$nextIndex = 1
foreach ($loop in @($orderedLoops | Where-Object {
    [string] $_.loopLevelCode -eq "PlayableUnit" -and
    [string] $_.statusCode -notin @("Deferred", "Planned", "Validated") -and
    [string] $_.closureStateCode -eq "Open" })) {
    [void] $builder.AppendLine("$nextIndex. **$(Escape-Cell $loop.title)** — $(Escape-Cell $loop.nextAction)")
    $nextIndex++
}

$generated = $builder.ToString()
$resolvedOutput = Resolve-RepositoryPath $OutputPath
if ($Mode -eq "Write") {
    $outputDirectory = Split-Path -Parent $resolvedOutput
    if (-not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }
    [IO.File]::WriteAllText($resolvedOutput, $generated, [Text.UTF8Encoding]::new($false))
    Write-Output "DevelopmentCompletionLedgerGenerated:Loops=$($orderedLoops.Count);Evidence=$(@($evidence.packages).Count)"
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "CompletionLedgerMissing"
    $existing = Get-Content -LiteralPath $resolvedOutput -Raw -Encoding UTF8
    Require ($existing -eq $generated) "CompletionLedgerOutOfDate"
}

Write-Output ("DevelopmentSystemValid:Loops={0};Independent={1};World={2};Cross={3};Evidence={4};PresentationModules={5};PresentationProfiles={6}" -f
    $orderedLoops.Count,
    @($orderedLoops | Where-Object loopKind -eq "IndependentArea").Count,
    @($orderedLoops | Where-Object loopKind -eq "WorldIntegration").Count,
    @($orderedLoops | Where-Object loopKind -eq "CrossAreaIntegration").Count,
    @($evidence.packages).Count,
    @($presentationValidation.modules).Count,
    @($presentationValidation.loopProfiles).Count)
