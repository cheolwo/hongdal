param(
    [ValidateSet('Validate', 'Write')][string] $Mode = 'Validate',
    [string] $PolicyPath = 'eng/execution-ledgers/playable-loop-planning-e1-policy.json',
    [string] $OutputJsonPath = 'docs/AI/generated/playable-loop-planning-e1-index.json',
    [string] $OutputMarkdownPath = 'docs/AI/generated/playable-loop-planning-e1-index.md'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../common/deterministic-text-output.ps1')

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PlayableLoopPlanningE1IndexInvalid:$Code" }
}
function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}
function Normalize-RepoRef([string] $BaseDirectory, [string] $Path, [string] $RepositoryRoot) {
    $withoutAnchor = $Path.Split('#')[0]
    $absolute = [IO.Path]::GetFullPath((Join-Path $BaseDirectory $withoutAnchor))
    return [IO.Path]::GetRelativePath($RepositoryRoot, $absolute).Replace('\', '/')
}
function Stage-AtLeastE1([string] $Stage) {
    return $Stage -match '^E([1-9]|10)$'
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$resolvedPolicy = Join-Path $repositoryRoot $PolicyPath
Require (Test-Path -LiteralPath $resolvedPolicy) 'PolicyNotFound'
$policy = Get-Content -LiteralPath $resolvedPolicy -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $policy.schemaVersion -eq 'ssalddel-playable-loop-planning-e1-policy.v1') 'PolicySchemaInvalid'

$planningPath = Join-Path $repositoryRoot ([string] $policy.planningIndexRef)
$loopPath = Join-Path $repositoryRoot ([string] $policy.playableLoopCatalogRef)
$atomicLedgerPath = Join-Path $repositoryRoot ([string] $policy.atomicModuleLedgerRef)
$atomicOutlinePath = Join-Path $repositoryRoot ([string] $policy.atomicOutlineLedgerRef)
Require (Test-Path -LiteralPath $planningPath) 'PlanningIndexNotFound'
Require (Test-Path -LiteralPath $loopPath) 'PlayableLoopCatalogNotFound'
Require (Test-Path -LiteralPath $atomicLedgerPath) 'AtomicModuleLedgerNotFound'
Require (Test-Path -LiteralPath $atomicOutlinePath) 'AtomicOutlineLedgerNotFound'
$planningDirectory = Split-Path -Parent $planningPath
$loops = Get-Content -LiteralPath $loopPath -Raw -Encoding UTF8 | ConvertFrom-Json
$units = @($loops.items | Where-Object loopLevelCode -eq 'PlayableUnit')
$atomicLedger = Get-Content -LiteralPath $atomicLedgerPath -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $atomicLedger.schemaVersion -eq 'ssalddel-playable-loop-planning-atomic-e1-modules.v1') 'AtomicModuleSchemaInvalid'
Require ([string] $atomicLedger.atomicClosurePolicy.defaultModeCode -eq 'OnePrimaryAtomicClosureTarget') 'AtomicClosureModeInvalid'
Require ([bool] $atomicLedger.atomicClosurePolicy.moduleDoesNotAutomaticallyCreatePlayableUnit) 'AtomicModulePlayableUnitBoundaryMissing'
Require (@($atomicLedger.atomicClosurePolicy.requiredBeforeE1Codes).Count -gt 0) 'AtomicClosureE1RequirementsMissing'
Require (@($atomicLedger.atomicClosurePolicy.closureOrderCodes).Count -eq 7) 'AtomicClosureOrderInvalid'
Require ([bool] $atomicLedger.atomicClosurePolicy.doesNotImposeGlobalWorkInProgressLimit) 'AtomicClosureGlobalWipBoundaryMissing'
$atomicOutlines = Get-Content -LiteralPath $atomicOutlinePath -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $atomicOutlines.schemaVersion -eq 'ssalddel-playable-loop-planning-atomic-e1-outlines.v1') 'AtomicOutlineSchemaInvalid'
$worldInteractionPath = Join-Path $repositoryRoot ([string] $atomicLedger.worldInteractionCatalogRef)
Require (Test-Path -LiteralPath $worldInteractionPath) 'WorldInteractionCatalogNotFound'
$worldInteractions = Get-Content -LiteralPath $worldInteractionPath -Raw -Encoding UTF8 | ConvertFrom-Json
$knownWorldInteractionIds = @($worldInteractions.items | ForEach-Object { [string] $_.id })

$planRows = [Collections.Generic.List[object]]::new()
foreach ($line in Get-Content -LiteralPath $planningPath -Encoding UTF8) {
    if ($line -notmatch '^\|\s*`(?<id>PLAN-[^`]+)`\s*\|') { continue }
    $cells = @($line.Split('|') | ForEach-Object { $_.Trim() })
    Require ($cells.Count -ge 5) "PlanningRowMalformed:$($Matches.id)"
    $documentMatch = [regex]::Match($cells[2], '\[[^\]]+\]\((?<path>[^)]+)\)')
    Require $documentMatch.Success "PlanningDocumentMissing:$($Matches.id)"
    $documentRef = Normalize-RepoRef $planningDirectory $documentMatch.Groups['path'].Value $repositoryRoot
    Require (Test-Path -LiteralPath (Join-Path $repositoryRoot $documentRef)) "PlanningDocumentNotFound:$($Matches.id)"
    $planRows.Add([pscustomobject]@{
        planId = [string] $Matches.id
        documentRef = $documentRef
        planningStatus = ($cells[3] -replace '`', '')
    })
}
Require ($planRows.Count -eq [int] $policy.expectedPlanCount) "PlanCountMismatch:$($planRows.Count)"
Require ((@($planRows.planId | Sort-Object -Unique).Count) -eq $planRows.Count) 'PlanIdDuplicate'

$crossCutting = @($policy.crossCuttingPlanIds | ForEach-Object { [string] $_ })
$noDirect = @($policy.noDirectE1ImpactPlanIds | ForEach-Object { [string] $_ })
foreach ($configuredId in @($crossCutting + $noDirect)) {
    Require ($planRows.planId -contains $configuredId) "ConfiguredPlanUnknown:$configuredId"
}

$outlines = @($atomicOutlines.outlines)
Require ((@($outlines.sourcePlanId | Sort-Object -Unique).Count) -eq $outlines.Count) 'AtomicOutlinePlanDuplicate'
$outlineCandidateIds = @($outlines | ForEach-Object { @($_.candidates) } | ForEach-Object { [string] $_.candidateId })
Require ((@($outlineCandidateIds | Sort-Object -Unique).Count) -eq $outlineCandidateIds.Count) 'AtomicOutlineCandidateIdDuplicate'
foreach ($outline in $outlines) {
    $sourcePlanId = [string] $outline.sourcePlanId
    Require ($planRows.planId -contains $sourcePlanId) "AtomicOutlinePlanUnknown:$sourcePlanId"
    Require (@($outline.candidates).Count -ge 2) "AtomicOutlineTooSmall:$sourcePlanId"
    foreach ($candidate in @($outline.candidates)) {
        $candidateId = [string] $candidate.candidateId
        Require ($candidateId.StartsWith('atomic-outline:')) "AtomicOutlineCandidateIdInvalid:$candidateId"
        Require (-not [string]::IsNullOrWhiteSpace([string] $candidate.title)) "AtomicOutlineCandidateTitleMissing:$candidateId"
    }
}

$modules = @($atomicLedger.modules)
Require ((@($modules.moduleId | Sort-Object -Unique).Count) -eq $modules.Count) 'AtomicModuleIdDuplicate'
$cohorts = @($atomicLedger.atomicClosureCohorts)
Require ($cohorts.Count -gt 0) 'AtomicClosureCohortMissing'
Require ((@($cohorts.cohortId | Sort-Object -Unique).Count) -eq $cohorts.Count) 'AtomicClosureCohortIdDuplicate'
foreach ($cohort in $cohorts) {
    $cohortId = [string] $cohort.cohortId
    Require ($cohortId.StartsWith('atomic-closure-cohort:')) "AtomicClosureCohortIdInvalid:$cohortId"
    foreach ($planId in @($cohort.sourcePlanIds)) {
        Require ($planRows.planId -contains [string] $planId) "AtomicClosureCohortPlanUnknown:${cohortId}:$planId"
    }
    foreach ($loopId in @($cohort.targetPlayableLoopStableIds)) {
        Require ($units.loopStableId -contains [string] $loopId) "AtomicClosureCohortPlayableUnitUnknown:${cohortId}:$loopId"
    }
    foreach ($wiId in @($cohort.memberWorldInteractionIds)) {
        Require ($knownWorldInteractionIds -contains [string] $wiId) "AtomicClosureCohortWorldInteractionUnknown:${cohortId}:$wiId"
    }
    Require ([string] $cohort.sharedPreparationThroughStageCode -eq 'E4') "AtomicClosureCohortPreparationBoundaryInvalid:$cohortId"
    Require (@($cohort.e5EntryRequirementCodes) -contains 'FrozenApplicableFiveElementClassifications') "AtomicClosureCohortFiveElementGateMissing:$cohortId"
    Require (@($cohort.e5EntryRequirementCodes) -contains 'FrozenApplicableShengKeRelations') "AtomicClosureCohortElementRelationGateMissing:$cohortId"
    if ($null -ne $cohort.PSObject.Properties['completeGrowthActivationContract']) {
        $growth = $cohort.completeGrowthActivationContract
        Require (@($cohort.memberWorldInteractionIds) -contains [string] $growth.worldInteractionId) "AtomicClosureCohortGrowthWiUnknown:$cohortId"
        Require (@($growth.requiredConditionCodes).Count -gt 0) "AtomicClosureCohortGrowthConditionsMissing:$cohortId"
        Require ((@($growth.requiredRelationDisplayNames) -join ',') -eq '수생목,토극수') "AtomicClosureCohortGrowthRequiredRelationsInvalid:$cohortId"
        Require ([string] $growth.outcomeRelationDisplayName -eq '목생화') "AtomicClosureCohortGrowthOutcomeInvalid:$cohortId"
        Require ([bool] $growth.allRequiredConditionsMustPass) "AtomicClosureCohortGrowthGateMissing:$cohortId"
        Require ([bool] $growth.doesNotPromoteEvidence) "AtomicClosureCohortGrowthEvidenceBoundaryMissing:$cohortId"
    }
    Require ((@($cohort.coordinatedPromotionStageCodes) -join ',') -eq 'E5,E6,E7') "AtomicClosureCohortPromotionStagesInvalid:$cohortId"
    Require ([string] $cohort.memberEvidenceRuleCode -eq 'IndependentPerWorldInteraction') "AtomicClosureCohortMemberEvidenceRuleInvalid:$cohortId"
    Require ([string] $cohort.cohortEvidenceStageRuleCode -eq 'MinimumOfMembers') "AtomicClosureCohortStageRuleInvalid:$cohortId"
    Require ([bool] $cohort.doesNotForceAllFiveGwaeIntoEachWorldInteraction) "AtomicClosureCohortGwaeBoundaryMissing:$cohortId"
    Require ([bool] $cohort.doesNotPromoteEvidence) "AtomicClosureCohortEvidenceBoundaryMissing:$cohortId"
}
$activeClosureTarget = $atomicLedger.activePrimaryClosureTarget
Require ($null -ne $activeClosureTarget) 'ActiveAtomicClosureTargetMissing'
Require ($modules.moduleId -contains [string] $activeClosureTarget.moduleId) 'ActiveAtomicClosureModuleUnknown'
Require ($cohorts.cohortId -contains [string] $activeClosureTarget.parentCohortId) 'ActiveAtomicClosureCohortUnknown'
Require ($units.loopStableId -contains [string] $activeClosureTarget.targetPlayableLoopStableId) 'ActiveAtomicClosurePlayableUnitUnknown'
Require ([string] $activeClosureTarget.planningThreadBoundaryCode -eq 'E1AndPresentationE4Preparation') 'ActiveAtomicClosurePlanningBoundaryInvalid'
Require ([string] $activeClosureTarget.developmentHandoffStageCode -eq 'E5') 'ActiveAtomicClosureDevelopmentHandoffInvalid'
Require ([bool] $activeClosureTarget.doesNotPromoteEvidence) 'ActiveAtomicClosureEvidenceBoundaryMissing'
foreach ($reviewedPlanId in @($modules.sourcePlanId | Sort-Object -Unique)) {
    Require (-not ($outlines.sourcePlanId -contains $reviewedPlanId)) "AtomicOutlineAndReviewedModuleOverlap:$reviewedPlanId"
}
foreach ($module in $modules) {
    $moduleId = [string] $module.moduleId
    Require ($moduleId.StartsWith('play-transaction:')) "AtomicModuleIdInvalid:$moduleId"
    Require ($planRows.planId -contains [string] $module.sourcePlanId) "AtomicModulePlanUnknown:$moduleId"
    Require (-not [string]::IsNullOrWhiteSpace([string] $module.playerIntent)) "AtomicModuleIntentMissing:$moduleId"
    Require (@($module.entryConditions).Count -gt 0) "AtomicModuleEntryMissing:$moduleId"
    Require (-not [string]::IsNullOrWhiteSpace([string] $module.authorityTransitionMeaning)) "AtomicModuleTransitionMissing:$moduleId"
    Require (@($module.successConditions).Count -gt 0) "AtomicModuleSuccessMissing:$moduleId"
    Require (-not [string]::IsNullOrWhiteSpace([string] $module.failureRecovery)) "AtomicModuleRecoveryMissing:$moduleId"
    Require (-not [string]::IsNullOrWhiteSpace([string] $module.returnState)) "AtomicModuleReturnMissing:$moduleId"
    $primaryId = [string] $module.primaryWorldInteractionId
    if (-not [string]::IsNullOrWhiteSpace($primaryId)) {
        Require ($knownWorldInteractionIds -contains $primaryId) "AtomicModuleWorldInteractionUnknown:${moduleId}:$primaryId"
    }
    foreach ($reusedId in @($module.reusedWorldInteractionIds)) {
        Require ($knownWorldInteractionIds -contains [string] $reusedId) "AtomicModuleReusedWorldInteractionUnknown:${moduleId}:$reusedId"
    }
}
$assemblies = @($atomicLedger.e1Assemblies)
Require ((@($assemblies.assemblyId | Sort-Object -Unique).Count) -eq $assemblies.Count) 'E1AssemblyIdDuplicate'
foreach ($assembly in $assemblies) {
    $assemblyId = [string] $assembly.assemblyId
    Require ($assemblyId.StartsWith('e1-assembly:')) "E1AssemblyIdInvalid:$assemblyId"
    Require ($planRows.planId -contains [string] $assembly.sourcePlanId) "E1AssemblyPlanUnknown:$assemblyId"
    $targetLoopStableId = [string] $assembly.targetPlayableLoopStableId
    if (-not [string]::IsNullOrWhiteSpace($targetLoopStableId)) {
        Require ($units.loopStableId -contains $targetLoopStableId) "E1AssemblyTargetPlayableUnitUnknown:${assemblyId}:$targetLoopStableId"
    }
    $requiredIds = @($assembly.requiredModuleIds | ForEach-Object { [string] $_ })
    Require ($requiredIds.Count -gt 0) "E1AssemblyRequiredModulesMissing:$assemblyId"
    foreach ($moduleId in @($requiredIds + @($assembly.optionalModuleIds | ForEach-Object { [string] $_ }))) {
        Require ($modules.moduleId -contains $moduleId) "E1AssemblyModuleUnknown:${assemblyId}:$moduleId"
        $boundModule = @($modules | Where-Object moduleId -eq $moduleId)
        Require ([string] $boundModule[0].sourcePlanId -eq [string] $assembly.sourcePlanId) "E1AssemblyCrossPlanModuleInvalid:${assemblyId}:$moduleId"
    }
    Require ([bool] $assembly.doesNotEstablishE1) "CandidateAssemblyEvidenceBoundaryMissing:$assemblyId"
}

$unitRows = [Collections.Generic.List[object]]::new()
foreach ($unit in $units) {
    $gate = $unit.planningGate
    $contractChecks = [ordered]@{
        approvedPlanning = ([string] $gate.statusCode -in @('Approved', 'LegacyActiveMigration'))
        frozenDesignRevision = -not [string]::IsNullOrWhiteSpace([string] $gate.designRevision)
        frozenDesignHash = ([string] $gate.designHashSha256 -match '^[0-9A-Fa-f]{64}$')
        playerPromise = -not [string]::IsNullOrWhiteSpace([string] $unit.playerPromise)
        entryState = @($unit.entryStateCodes).Count -gt 0
        worldInteraction = @($unit.worldInteractionIds).Count -gt 0
        successState = @($unit.successStateCodes).Count -gt 0
        returnState = @($unit.returnStateCodes).Count -gt 0
        logicE1 = Stage-AtLeastE1 ([string] $unit.maturityTracks.logic.currentStage)
        presentationE1 = Stage-AtLeastE1 ([string] $unit.maturityTracks.presentation.currentStage)
    }
    $missing = @($contractChecks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
    $e1State = if ($missing.Count -eq 0) { 'Established' } elseif ([string] $gate.statusCode -eq 'NotStarted') { 'Blocked' } else { 'Conditional' }
    $unitRows.Add([pscustomobject]@{
        loopStableId = [string] $unit.loopStableId
        topicStableId = [string] $gate.topicStableId
        designDocumentRef = [string] $gate.designDocumentRef
        planningStatus = [string] $gate.statusCode
        worldInteractionIds = @($unit.worldInteractionIds | ForEach-Object { [string] $_ })
        e1State = $e1State
        missingContractParts = $missing
    })
}

$indexedPlans = [Collections.Generic.List[object]]::new()
foreach ($plan in $planRows) {
    $owners = @($unitRows | Where-Object designDocumentRef -eq $plan.documentRef)
    $supporters = @($units | Where-Object {
        $sourceProperty = $_.PSObject.Properties['sourcePlanningDocumentRefs']
        $null -ne $sourceProperty -and @($sourceProperty.Value) -contains $plan.documentRef
    })
    $planAssemblies = @($assemblies | Where-Object sourcePlanId -eq $plan.planId)
    $assemblyLoopRefs = @($planAssemblies | ForEach-Object { [string] $_.targetPlayableLoopStableId } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $loopRefs = @(@($owners | ForEach-Object { [string] $_.loopStableId }) + @($supporters | ForEach-Object { [string] $_.loopStableId }) + $assemblyLoopRefs | Sort-Object -Unique)
    $classification = if ($owners.Count -gt 0) { 'ContractOwner' }
        elseif ($supporters.Count -gt 0) { 'SupportingContext' }
        elseif ($crossCutting -contains $plan.planId) { 'CrossCuttingContext' }
        elseif ($noDirect -contains $plan.planId) { 'NoDirectE1Impact' }
        else { 'E1CandidateNeeded' }
    $planModules = @($modules | Where-Object sourcePlanId -eq $plan.planId | Sort-Object sequence)
    $planOutlines = @($outlines | Where-Object sourcePlanId -eq $plan.planId)
    $decompositionState = if ($planModules.Count -gt 0) { 'ReviewedAtomicModules' }
        elseif ($planOutlines.Count -gt 0) { 'OutlinedAtomicCandidates' }
        elseif ($classification -in @('CrossCuttingContext', 'NoDirectE1Impact')) { 'ContextOnly' }
        elseif ($classification -in @('ContractOwner', 'SupportingContext')) { 'NeedsAtomicReview' }
        else { 'NeedsDecomposition' }
    $indexedPlans.Add([pscustomobject]@{
        planId = $plan.planId
        documentRef = $plan.documentRef
        planningStatus = $plan.planningStatus
        classificationCode = $classification
        decompositionStateCode = $decompositionState
        atomicModuleIds = @($planModules | ForEach-Object { [string] $_.moduleId })
        atomicOutlineCandidateIds = @($planOutlines | ForEach-Object { @($_.candidates) } | ForEach-Object { [string] $_.candidateId })
        relatedPlayableLoopIds = $loopRefs
        nextAction = if ($planModules.Count -gt 0 -and $planAssemblies.Count -gt 0) {
            '검토된 원자 모듈과 E1 조립안의 열린 공백을 승인된 상세 기획·판본·WI 계약으로 닫는다.'
        } else {
            switch ($classification) {
                'ContractOwner' { '같은 설계 revision/hash와 E1 계약 완결성을 유지한다.' }
                'SupportingContext' { '소비 PlayableUnit의 상세 기획에서 어떤 계약 항목을 지지하는지 유지한다.' }
                'CrossCuttingContext' { '공통 원칙으로만 소비하며 단독 WI·E1을 자동 생성하지 않는다.' }
                'NoDirectE1Impact' { '관리·조사 결과를 관련 PlayableUnit이 명시적으로 선택할 때만 E1 근거로 연결한다.' }
                default { '기존 PlayableUnit 재사용 여부를 검토하고, 없을 때만 새 상세 기획·StableId·WI 후보를 승인한다.' }
            }
        }
    })
}

$result = [ordered]@{
    schemaVersion = 'ssalddel-playable-loop-planning-e1-index.v1'
    revision = 'ssalddel-playable-loop-planning-e1-index.r1'
    generatedAtRuleCode = 'DeterministicFromPlanningAndPlayableLoopCatalog'
    sourceSnapshots = @(
        [ordered]@{ path = [string] $policy.planningIndexRef; sha256 = (Get-FileHash $planningPath -Algorithm SHA256).Hash },
        [ordered]@{ path = [string] $policy.playableLoopCatalogRef; sha256 = (Get-FileHash $loopPath -Algorithm SHA256).Hash },
        [ordered]@{ path = $PolicyPath; sha256 = (Get-FileHash $resolvedPolicy -Algorithm SHA256).Hash },
        [ordered]@{ path = [string] $policy.atomicModuleLedgerRef; sha256 = (Get-FileHash $atomicLedgerPath -Algorithm SHA256).Hash },
        [ordered]@{ path = [string] $policy.atomicOutlineLedgerRef; sha256 = (Get-FileHash $atomicOutlinePath -Algorithm SHA256).Hash },
        [ordered]@{ path = [string] $atomicLedger.worldInteractionCatalogRef; sha256 = (Get-FileHash $worldInteractionPath -Algorithm SHA256).Hash }
    )
    counts = [ordered]@{
        plans = $indexedPlans.Count
        contractOwners = @($indexedPlans | Where-Object classificationCode -eq 'ContractOwner').Count
        supportingContexts = @($indexedPlans | Where-Object classificationCode -eq 'SupportingContext').Count
        crossCuttingContexts = @($indexedPlans | Where-Object classificationCode -eq 'CrossCuttingContext').Count
        noDirectE1Impact = @($indexedPlans | Where-Object classificationCode -eq 'NoDirectE1Impact').Count
        e1CandidatesNeeded = @($indexedPlans | Where-Object classificationCode -eq 'E1CandidateNeeded').Count
        playableUnits = $unitRows.Count
        establishedE1 = @($unitRows | Where-Object e1State -eq 'Established').Count
        conditionalE1 = @($unitRows | Where-Object e1State -eq 'Conditional').Count
        blockedE1 = @($unitRows | Where-Object e1State -eq 'Blocked').Count
        atomicModules = $modules.Count
        atomicClosureCohorts = $cohorts.Count
        e1Assemblies = $assemblies.Count
        reviewedAtomicPlans = @($indexedPlans | Where-Object decompositionStateCode -eq 'ReviewedAtomicModules').Count
        outlinedAtomicPlans = @($indexedPlans | Where-Object decompositionStateCode -eq 'OutlinedAtomicCandidates').Count
        atomicOutlineCandidates = $outlineCandidateIds.Count
        atomicReviewNeeded = @($indexedPlans | Where-Object decompositionStateCode -eq 'NeedsAtomicReview').Count
        decompositionNeeded = @($indexedPlans | Where-Object decompositionStateCode -eq 'NeedsDecomposition').Count
        contextOnlyPlans = @($indexedPlans | Where-Object decompositionStateCode -eq 'ContextOnly').Count
    }
    policy = [ordered]@{
        planningDocumentAloneIsNotE1 = $true
        doesNotCreateGameplayMeaning = $true
        doesNotPromoteEvidence = $true
    }
    atomicClosurePolicy = $atomicLedger.atomicClosurePolicy
    activePrimaryClosureTarget = $activeClosureTarget
    atomicClosureCohorts = @($cohorts)
    plans = @($indexedPlans)
    atomicOutlines = @($outlines)
    atomicModules = @($modules)
    e1Assemblies = @($assemblies)
    playableUnitE1Contracts = @($unitRows)
}

$jsonText = ConvertTo-DeterministicText (($result | ConvertTo-Json -Depth 20) + "`n")
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine('# 기획–E1 결속 인덱스')
[void] $builder.AppendLine()
[void] $builder.AppendLine('> 이 문서는 `PLANNING.md`, `playable-loops.json`, E1 결속 정책에서 자동 생성된다. 직접 수정하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 기획: ``$($result.counts.plans)`` / E1 계약 직접 소유: ``$($result.counts.contractOwners)`` / 명시 지원: ``$($result.counts.supportingContexts)``")
[void] $builder.AppendLine("- 공통 문맥: ``$($result.counts.crossCuttingContexts)`` / 직접 영향 없음: ``$($result.counts.noDirectE1Impact)`` / 결속 검토 필요: ``$($result.counts.e1CandidatesNeeded)``")
[void] $builder.AppendLine("- PlayableUnit E1: 성립 ``$($result.counts.establishedE1)`` / 조건부 ``$($result.counts.conditionalE1)`` / 차단 ``$($result.counts.blockedE1)``")
[void] $builder.AppendLine("- 원자 모듈: ``$($result.counts.atomicModules)`` / E1 조립안: ``$($result.counts.e1Assemblies)`` / 원자 검토 완료 기획: ``$($result.counts.reviewedAtomicPlans)``")
[void] $builder.AppendLine("- 공동 준비 묶음: ``$($result.counts.atomicClosureCohorts)`` / 묶음 단계 판정: 구성 WI 최솟값 / WI별 증거 판정: 독립")
[void] $builder.AppendLine("- 원자 분해 초안: 기획 ``$($result.counts.outlinedAtomicPlans)`` / 후보 ``$($result.counts.atomicOutlineCandidates)``")
[void] $builder.AppendLine("- 순서별 남은 검토: 기존 E1 근거의 원자 검토 ``$($result.counts.atomicReviewNeeded)`` / 원자 분해 ``$($result.counts.decompositionNeeded)`` / 문맥 전용 ``$($result.counts.contextOnlyPlans)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine('기획 문서 하나가 존재한다는 사실만으로 E1이 성립하지 않는다. 승인된 상세 기획, 한 PlayableUnit, WI, 권위·완료·실패/귀환 계약이 같은 판본으로 결속되어야 한다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('기본 운영은 원자 폐쇄 대상 하나를 주 목표로 골라 E1부터 E7까지 닫거나 명시적으로 차단한 뒤 다음 대상으로 이동한다. 이는 원자 모듈마다 새 PlayableUnit을 만들거나 독립 작업 전체에 전역 WIP 상한을 두는 규칙이 아니다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine("현재 주 원자 폐쇄 대상은 ``$($activeClosureTarget.moduleId)``이며 기획은 E1 계약과 Presentation E4 준비까지 담당하고 실제 E5부터 개발로 인계한다. 이 선택 자체는 Evidence를 승격하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("현재 대상은 ``$($activeClosureTarget.parentCohortId)``에 속한다. Farm 작업군은 오행·공간·도구·표현 문맥을 E4까지 함께 준비하고 E5~E7을 조율하지만, 각 WI의 증거는 독립 판정하며 묶음 단계는 가장 낮은 구성 WI를 따른다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 기획별 E1 관계')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 순서 | 기획 | 상태 | E1 관계 | 원자 분해 | 원자 모듈 | 관련 PlayableUnit | 다음 조치 |')
[void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- | --- | --- |')
$planOrder = 0
foreach ($plan in $indexedPlans) {
    $planOrder++
    $atomicRefs = @(@($plan.atomicModuleIds) + @($plan.atomicOutlineCandidateIds))
    [void] $builder.AppendLine("| $planOrder | ``$(Escape-Cell $plan.planId)`` | $(Escape-Cell $plan.planningStatus) | ``$($plan.classificationCode)`` | ``$($plan.decompositionStateCode)`` | $(Escape-Cell ($atomicRefs -join '<br>')) | $(Escape-Cell (@($plan.relatedPlayableLoopIds) -join '<br>')) | $(Escape-Cell $plan.nextAction) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 기획별 원자 분해 초안')
[void] $builder.AppendLine()
[void] $builder.AppendLine('이 목록은 기획 본문을 플레이 결과 경계로 나눈 검토 순서다. WI·권위·실패·귀환 계약 또는 E1 성립을 뜻하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 기획 | 초안 종류 | 원자 후보 순서 |')
[void] $builder.AppendLine('| --- | --- | --- |')
foreach ($outline in $outlines) {
    $candidateText = @($outline.candidates | ForEach-Object { "``$($_.candidateId)`` $($_.title)" }) -join '<br>'
    [void] $builder.AppendLine("| ``$(Escape-Cell $outline.sourcePlanId)`` | ``$(Escape-Cell $outline.outlineKindCode)`` | $(Escape-Cell $candidateText) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 검토된 플레이 원자 계약 모듈')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 기획 | 순서 | 원자 모듈 | 상태 | 주 WI | 성공 | 열린 결손 |')
[void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($module in $modules | Sort-Object sourcePlanId, sequence) {
    [void] $builder.AppendLine("| ``$(Escape-Cell $module.sourcePlanId)`` | $($module.sequence) | ``$(Escape-Cell $module.moduleId)``<br>$(Escape-Cell $module.title) | ``$($module.reviewStateCode)`` | ``$(Escape-Cell $module.primaryWorldInteractionId)`` | $(Escape-Cell (@($module.successConditions) -join ', ')) | $(Escape-Cell (@($module.openGaps) -join ', ')) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## E1 조립안')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 조립안 | 기획 | 상태 | 대상 PlayableUnit | 필수 모듈 | 열린 결손 |')
[void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- |')
foreach ($assembly in $assemblies) {
    [void] $builder.AppendLine("| ``$(Escape-Cell $assembly.assemblyId)`` | ``$(Escape-Cell $assembly.sourcePlanId)`` | ``$($assembly.stateCode)`` | ``$(Escape-Cell $assembly.targetPlayableLoopStableId)`` | $(Escape-Cell (@($assembly.requiredModuleIds) -join '<br>')) | $(Escape-Cell (@($assembly.openGaps) -join ', ')) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## PlayableUnit별 E1 계약')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| PlayableUnit | 기획 관문 | E1 | WI | 미완료 계약 |')
[void] $builder.AppendLine('| --- | --- | --- | --- | --- |')
foreach ($unit in $unitRows) {
    [void] $builder.AppendLine("| ``$(Escape-Cell $unit.loopStableId)`` | $($unit.planningStatus) | ``$($unit.e1State)`` | $(Escape-Cell (@($unit.worldInteractionIds) -join ', ')) | $(Escape-Cell (@($unit.missingContractParts) -join ', ')) |")
}
$markdownText = ConvertTo-DeterministicText $builder.ToString()

if ($Mode -eq 'Write') {
    [void] (Write-DeterministicTextIfChanged -Path (Join-Path $repositoryRoot $OutputJsonPath) -Content $jsonText)
    [void] (Write-DeterministicTextIfChanged -Path (Join-Path $repositoryRoot $OutputMarkdownPath) -Content $markdownText)
} else {
    foreach ($pair in @(@($OutputJsonPath, $jsonText), @($OutputMarkdownPath, $markdownText))) {
        $path = Join-Path $repositoryRoot $pair[0]
        Require (Test-Path -LiteralPath $path) "GeneratedOutputMissing:$($pair[0])"
        Require ((Get-Content -LiteralPath $path -Raw -Encoding UTF8) -eq $pair[1]) "GeneratedOutputStale:$($pair[0])"
    }
}
Write-Output "PlayableLoopPlanningE1IndexValid:Plans=$($indexedPlans.Count);Established=$($result.counts.establishedE1);Candidates=$($result.counts.e1CandidatesNeeded)"
