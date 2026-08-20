[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $CatalogPath = "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json",
    [string] $PriorityPath = "eng/world-seedbeds/synty-bottom-up-inventory/h2-composition-priorities.v1.json",
    [string] $RecipePath = "eng/world-seedbeds/synty-bottom-up-inventory/h2-composition-recipes.v1.json",
    [string] $GameplayCompletionPath = "eng/world-seedbeds/gameplay-spatial-completion.v1.json",
    [string] $TheoryFactoryPath = "eng/world-seedbeds/generated/theory-spatial-factory.v1.json",
    [string] $JsonOutputPath = "eng/world-seedbeds/generated/h2-composition-readiness.v1.json",
    [string] $MarkdownOutputPath = "docs/AI/generated/h2-composition-readiness.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "H2CompositionReadinessInvalid:$Code" }
}

function Resolve-RepositoryPath([string] $RepositoryRoot, [string] $RelativePath) {
    return Join-Path $RepositoryRoot ($RelativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}

function Read-Json([string] $Path) {
    Require (Test-Path -LiteralPath $Path) "SourceMissing:$Path"
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Get-Sha256Lower([string] $Path) {
    return (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
}

function Test-H1RecognizedPart([object] $Definition) {
    if ($null -eq $Definition) { return $false }
    $hasGameContext = @($Definition.wiIds).Count -gt 0 -or @($Definition.anticipatedGameplayCodes).Count -gt 0
    $hasSpatialMeaning = @($Definition.spatialRoleCodes).Count -gt 0
    $hasExpressionSource = @($Definition.sourcePackCodes).Count -gt 0 -or @($Definition.grammarSetRefs).Count -gt 0
    return $hasGameContext -and $hasSpatialMeaning -and $hasExpressionSource
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedCatalogPath = Resolve-RepositoryPath $repositoryRoot $CatalogPath
$resolvedPriorityPath = Resolve-RepositoryPath $repositoryRoot $PriorityPath
$resolvedRecipePath = Resolve-RepositoryPath $repositoryRoot $RecipePath
$resolvedGameplayCompletionPath = Resolve-RepositoryPath $repositoryRoot $GameplayCompletionPath
$resolvedTheoryFactoryPath = Resolve-RepositoryPath $repositoryRoot $TheoryFactoryPath
$resolvedJsonOutputPath = Resolve-RepositoryPath $repositoryRoot $JsonOutputPath
$resolvedMarkdownOutputPath = Resolve-RepositoryPath $repositoryRoot $MarkdownOutputPath

$catalog = Read-Json $resolvedCatalogPath
$priority = Read-Json $resolvedPriorityPath
$recipes = Read-Json $resolvedRecipePath
$gameplayCompletion = Read-Json $resolvedGameplayCompletionPath
$theoryFactory = Read-Json $resolvedTheoryFactoryPath
$resolvedAreaSetPriorityPath = Resolve-RepositoryPath $repositoryRoot ([string] $gameplayCompletion.areaSetPriorityPath)
$areaSetPriority = Read-Json $resolvedAreaSetPriorityPath
$resolvedUnityEvidencePath = Resolve-RepositoryPath $repositoryRoot ([string] $priority.unityReviewEvidenceCatalogPath)
$unityEvidenceCatalog = Read-Json $resolvedUnityEvidencePath

Require ([string] $catalog.schemaVersion -eq "simulation-world-spatial-design-knowledge-catalog.v3") "CatalogSchemaInvalid"
Require ([string] $priority.schemaVersion -eq "simulation-world-h2-composition-priorities.v1") "PrioritySchemaInvalid"
Require ([string] $recipes.schemaVersion -eq "simulation-world-h2-composition-recipes.v1") "RecipeSchemaInvalid"
Require ([string] $gameplayCompletion.schemaVersion -eq "simulation-world-gameplay-spatial-completion.v1") "GameplayCompletionSchemaInvalid"
Require ([string] $theoryFactory.schemaVersion -eq "simulation-world-theory-spatial-factory-output.v1") "TheoryFactorySchemaInvalid"
Require ([string] $areaSetPriority.schemaVersion -eq "simulation-world-area-set-composition-priorities.v1") "AreaSetPrioritySchemaInvalid"
Require ([string] $unityEvidenceCatalog.schemaVersion -eq "simulation-world-h2-unity-review-evidence.v1") "UnityEvidenceSchemaInvalid"
Require ([string] $unityEvidenceCatalog.sourceRecipeRevision -eq [string] $recipes.revision) "UnityEvidenceRecipeRevisionMismatch"
Require ([string] $unityEvidenceCatalog.sourceRecipeSha256 -eq (Get-Sha256Lower $resolvedRecipePath)) "UnityEvidenceRecipeHashMismatch"
Require ([string] $unityEvidenceCatalog.captureProfileCode -eq "H2BlockFiveViews") "UnityEvidenceCaptureProfileInvalid"
Require ([bool] $priority.promotionGate.h1OfficialSimulationApprovalRequired -eq $false) "H1ApprovalMustNotGateComposition"
Require ([bool] $priority.promotionGate.areaSetPlacementRequired -eq $false) "AreaSetMustNotGateH2"
Require ([bool] $priority.promotionGate.publicDataRequired -eq $false) "PublicDataMustNotGateH2"
Require ([bool] $gameplayCompletion.gatePolicy.theorySpatialProductionIndependentFromGameplayTrace) "GameplayTraceMustNotGateTheoryProduction"
Require ([string] $theoryFactory.humanReviewModeCode -eq "DeferredBatchReview") "TheoryFactoryHumanReviewModeInvalid"
Require ([bool] $theoryFactory.authorityBoundary.humanApprovalNotClaimed) "TheoryFactoryHumanApprovalBoundaryInvalid"
Require ([bool] $theoryFactory.authorityBoundary.publicDataNotBound) "TheoryFactoryPublicDataBoundaryInvalid"
Require ([bool] $theoryFactory.authorityBoundary.runtimeNotValidated) "TheoryFactoryRuntimeBoundaryInvalid"
Require ([bool] $theoryFactory.authorityBoundary.e6AndE7RemainSeparate) "TheoryFactoryEvidenceBoundaryInvalid"

$catalogRoot = Split-Path -Parent $resolvedCatalogPath
$h1ById = @{}
foreach ($definitionRef in @($catalog.h1InteractionDefinitionRefs | Sort-Object stableId)) {
    $definitionPath = Join-Path $catalogRoot ([string] $definitionRef.definitionPath -replace "/", [IO.Path]::DirectorySeparatorChar)
    $definition = Read-Json $definitionPath
    Require ([string] $definition.hierarchyLevelCode -eq "H1") "H1LevelInvalid:$($definition.stableId)"
    Require ((Get-Sha256Lower $definitionPath) -eq [string] $definitionRef.definitionSha256) "H1HashMismatch:$($definition.stableId)"
    Require (-not $h1ById.ContainsKey([string] $definition.stableId)) "H1Duplicate:$($definition.stableId)"
    $h1ById[[string] $definition.stableId] = $definition
}

$h3ById = @{}
foreach ($definitionRef in @($catalog.h3DefinitionRefs | Sort-Object stableId)) {
    $definitionPath = Join-Path $catalogRoot ([string] $definitionRef.definitionPath -replace "/", [IO.Path]::DirectorySeparatorChar)
    $definition = Read-Json $definitionPath
    Require ([string] $definition.hierarchyLevelCode -eq "H3") "H3LevelInvalid:$($definition.stableId)"
    Require ((Get-Sha256Lower $definitionPath) -eq [string] $definitionRef.definitionSha256) "H3HashMismatch:$($definition.stableId)"
    $h3ById[[string] $definition.stableId] = $definition
}

$h4ById = @{}
foreach ($definitionRef in @($catalog.h4DefinitionRefs | Sort-Object stableId)) {
    $definitionPath = Join-Path $catalogRoot ([string] $definitionRef.definitionPath -replace "/", [IO.Path]::DirectorySeparatorChar)
    $definition = Read-Json $definitionPath
    Require ([string] $definition.hierarchyLevelCode -eq "H4") "H4LevelInvalid:$($definition.stableId)"
    Require ((Get-Sha256Lower $definitionPath) -eq [string] $definitionRef.definitionSha256) "H4HashMismatch:$($definition.stableId)"
    $h4ById[[string] $definition.stableId] = $definition
}

$h2GamePlanCodes = @{}
foreach ($candidate in @($areaSetPriority.areaSetCandidates)) {
    $gamePlanCode = [string] $candidate.gamePlanCode
    $h4Ref = [string] $candidate.areaSetCandidateRef
    Require ($h4ById.ContainsKey($h4Ref)) "AreaSetCandidateH4Unknown:$h4Ref"
    foreach ($h3Ref in @($h4ById[$h4Ref].requiredH3Refs + $h4ById[$h4Ref].optionalH3Refs)) {
        Require ($h3ById.ContainsKey([string] $h3Ref)) "AreaSetCandidateH3Unknown:${h4Ref}:$h3Ref"
        foreach ($h2Ref in @($h3ById[[string] $h3Ref].requiredH2Refs + $h3ById[[string] $h3Ref].optionalH2Refs)) {
            if (-not $h2GamePlanCodes.ContainsKey([string] $h2Ref)) { $h2GamePlanCodes[[string] $h2Ref] = @() }
            $h2GamePlanCodes[[string] $h2Ref] = @($h2GamePlanCodes[[string] $h2Ref] + $gamePlanCode | Sort-Object -Unique)
        }
    }
    $stagedProperty = $candidate.PSObject.Properties["stagedPackNativeH2Refs"]
    $stagedRefs = if ($null -eq $stagedProperty) { @() } else { @($stagedProperty.Value) }
    foreach ($h2Ref in $stagedRefs) {
        if (-not $h2GamePlanCodes.ContainsKey([string] $h2Ref)) { $h2GamePlanCodes[[string] $h2Ref] = @() }
        $h2GamePlanCodes[[string] $h2Ref] = @($h2GamePlanCodes[[string] $h2Ref] + $gamePlanCode | Sort-Object -Unique)
    }
}

$h2GameplayTrace = @{}
foreach ($slice in @($gameplayCompletion.playableSlices)) {
    foreach ($step in @($slice.steps)) {
        foreach ($h2Ref in @($step.h2Refs)) {
            if (-not $h2GameplayTrace.ContainsKey([string] $h2Ref)) {
                $h2GameplayTrace[[string] $h2Ref] = [ordered]@{ sliceIds = @(); stepIds = @() }
            }
            $trace = $h2GameplayTrace[[string] $h2Ref]
            $trace.sliceIds = @($trace.sliceIds + [string] $slice.playableSliceId | Sort-Object -Unique)
            $trace.stepIds = @($trace.stepIds + [string] $step.stepId | Sort-Object -Unique)
        }
    }
}
$strictGamePlanCodes = @($gameplayCompletion.gatePolicy.strictGamePlanCodes)
$warningOnlyGamePlanCodes = @($gameplayCompletion.gatePolicy.warningOnlyGamePlanCodes)

$recipeTargetIds = @{}
foreach ($recipe in @($recipes.recipes)) {
    Require (-not $recipeTargetIds.ContainsKey([string] $recipe.targetKnowledgeRef)) "RecipeTargetDuplicate:$($recipe.targetKnowledgeRef)"
    $recipeTargetIds[[string] $recipe.targetKnowledgeRef] = [string] $recipe.recipeId
}

$theoryPlanByTarget = @{}
foreach ($theoryPlan in @($theoryFactory.h2Plans)) {
    $h2StableId = [string] $theoryPlan.h2StableId
    Require (-not [string]::IsNullOrWhiteSpace($h2StableId)) "TheoryPlanTargetMissing"
    Require (-not $theoryPlanByTarget.ContainsKey($h2StableId)) "TheoryPlanTargetDuplicate:$h2StableId"
    Require ([string] $theoryPlan.theoryStateCode -eq "TheoryQualified") "TheoryPlanStateInvalid:$h2StableId"
    Require ([string] $theoryPlan.recipeSourceCode -in @("AuthoredRecipe", "DerivedTheoryRecipe")) "TheoryPlanRecipeSourceInvalid:$h2StableId"
    Require ([string] $theoryPlan.theoryHashSha256 -match "^[0-9a-f]{64}$") "TheoryPlanHashInvalid:$h2StableId"
    $theoryPlanByTarget[$h2StableId] = $theoryPlan
}
Require ($theoryPlanByTarget.Count -eq @($catalog.h2DefinitionRefs).Count) "TheoryPlanCoverageCountInvalid"
Require ([int] $theoryFactory.counts.h2TheoryQualified -eq $theoryPlanByTarget.Count) "TheoryFactoryQualifiedCountInvalid"
Require ([int] $theoryFactory.counts.authoredH2RecipesReused -eq @($theoryFactory.h2Plans | Where-Object recipeSourceCode -eq "AuthoredRecipe").Count) "TheoryFactoryAuthoredCountInvalid"
Require ([int] $theoryFactory.counts.derivedH2Recipes -eq @($theoryFactory.h2Plans | Where-Object recipeSourceCode -eq "DerivedTheoryRecipe").Count) "TheoryFactoryDerivedCountInvalid"

$unityEvidenceByTarget = @{}
foreach ($evidence in @($unityEvidenceCatalog.items)) {
    Require (-not $unityEvidenceByTarget.ContainsKey([string] $evidence.targetKnowledgeRef)) "UnityEvidenceDuplicate:$($evidence.targetKnowledgeRef)"
    Require ([int] $evidence.captureCount -eq 5) "UnityEvidenceCaptureCountInvalid:$($evidence.targetKnowledgeRef)"
    Require ([string] $evidence.reviewStateCode -eq "AwaitingHumanReview") "UnityEvidenceStateInvalid:$($evidence.targetKnowledgeRef)"
    foreach ($hash in @($evidence.prefabAssetSha256, $evidence.sourceCompositionSha256, $evidence.captureBundleSha256)) {
        Require ([string] $hash -match "^[0-9a-f]{64}$") "UnityEvidenceHashInvalid:$($evidence.targetKnowledgeRef)"
    }
    $unityEvidenceByTarget[[string] $evidence.targetKnowledgeRef] = $evidence
}

$items = @()
foreach ($definitionRef in @($catalog.h2DefinitionRefs | Sort-Object stableId)) {
    $definitionPath = Join-Path $catalogRoot ([string] $definitionRef.definitionPath -replace "/", [IO.Path]::DirectorySeparatorChar)
    $definition = Read-Json $definitionPath
    Require ([string] $definition.hierarchyLevelCode -eq "H2") "H2LevelInvalid:$($definition.stableId)"
    Require ((Get-Sha256Lower $definitionPath) -eq [string] $definitionRef.definitionSha256) "H2HashMismatch:$($definition.stableId)"

    $requiredH1Refs = @($definition.requiredH1Refs | ForEach-Object { [string] $_ })
    $recognizedH1Refs = @()
    $unrecognizedH1Refs = @()
    foreach ($h1Ref in $requiredH1Refs) {
        if ($h1ById.ContainsKey($h1Ref) -and (Test-H1RecognizedPart $h1ById[$h1Ref])) {
            $recognizedH1Refs += $h1Ref
        }
        else {
            $unrecognizedH1Refs += $h1Ref
        }
    }

    $h2StableId = [string] $definition.stableId
    $isComposable = $requiredH1Refs.Count -ge 2 -and $unrecognizedH1Refs.Count -eq 0
    $hasDetailedRecipe = $recipeTargetIds.ContainsKey($h2StableId)
    $hasTheoryPlan = $theoryPlanByTarget.ContainsKey($h2StableId)
    Require $hasTheoryPlan "TheoryPlanMissing:$h2StableId"
    $theoryPlan = $theoryPlanByTarget[$h2StableId]
    $theoryH1Refs = @($theoryPlan.nodes.h1Ref | ForEach-Object { [string] $_ } | Sort-Object -Unique)
    Require (($theoryH1Refs -join "|") -eq (@($requiredH1Refs | Sort-Object -Unique) -join "|")) "TheoryPlanH1LineageMismatch:$h2StableId"
    if ([string] $theoryPlan.recipeSourceCode -eq "AuthoredRecipe") {
        Require $hasDetailedRecipe "AuthoredTheoryRecipeMissing:$h2StableId"
    }
    else {
        Require (-not $hasDetailedRecipe) "DerivedTheoryRecipeConflictsWithAuthoredRecipe:$h2StableId"
    }
    $isTheoryQualified = $isComposable -and [string] $theoryPlan.theoryStateCode -eq "TheoryQualified"
    $hasUnityReviewEvidence = $unityEvidenceByTarget.ContainsKey($h2StableId)
    if ($hasUnityReviewEvidence) {
        $evidenceH1Refs = @($unityEvidenceByTarget[$h2StableId].childH1StableIds | Sort-Object)
        Require (($evidenceH1Refs -join "|") -eq (@($requiredH1Refs | Sort-Object) -join "|")) "UnityEvidenceH1LineageMismatch:$($definition.stableId)"
    }
    $isReviewReady = $isTheoryQualified -and $hasUnityReviewEvidence
    $theoryBlockers = @()
    if ($requiredH1Refs.Count -lt 2) { $theoryBlockers += "RequiredH1CountLessThanTwo" }
    foreach ($unrecognized in $unrecognizedH1Refs) { $theoryBlockers += "H1NotRecognized:$unrecognized" }
    if (-not $hasTheoryPlan) { $theoryBlockers += "TheoryFactoryPlanMissing" }
    $reviewBlockers = @()
    if (-not $isTheoryQualified) { $reviewBlockers += "TheoryQualificationMissing" }
    if (-not $hasUnityReviewEvidence) { $reviewBlockers += "UnityH2RootAndFiveViewCaptureMissing" }

    $gamePlanCodes = if ($h2GamePlanCodes.ContainsKey($h2StableId)) { @($h2GamePlanCodes[$h2StableId]) } else { @() }
    $isStrictGameplayGate = @($gamePlanCodes | Where-Object { $_ -in $strictGamePlanCodes }).Count -gt 0
    $isWarningOnlyGameplayGate = -not $isStrictGameplayGate -and @($gamePlanCodes | Where-Object { $_ -in $warningOnlyGamePlanCodes }).Count -gt 0
    $hasGameplayTrace = $h2GameplayTrace.ContainsKey($h2StableId)
    $gameplayGateModeCode = if ($isStrictGameplayGate) { "Strict" } elseif ($isWarningOnlyGameplayGate) { "WarningOnly" } else { "NotSelected" }
    $gameplayTraceStateCode = if ($hasGameplayTrace) { "SequenceMapped" } else { "Unlinked" }
    $gameplayGateSatisfied = -not $isStrictGameplayGate -or $hasGameplayTrace
    $isGameplaySelected = @($gamePlanCodes).Count -gt 0
    $gameplayTheoryPriorityReady = $isTheoryQualified -and $isGameplaySelected -and $gameplayGateSatisfied
    $gameplayReviewPriorityReady = $isReviewReady -and $isGameplaySelected -and $gameplayGateSatisfied
    $gameplayBlockers = @()
    $gameplayWarnings = @()
    if ($isStrictGameplayGate -and -not $hasGameplayTrace) { $gameplayBlockers += "StrictGameplayTraceMissing" }
    if ($isWarningOnlyGameplayGate -and -not $hasGameplayTrace) { $gameplayWarnings += "WarningOnlyGameplayTraceMissing" }

    $stageCode = if ($isTheoryQualified) {
        "TheoryQualified"
    }
    elseif ($isComposable) {
        "Composable"
    }
    else {
        "RecognizedPartsIncomplete"
    }

    $items += [ordered]@{
        h2StableId = [string] $definition.stableId
        title = [string] $definition.title
        inventoryStateCode = [string] $definition.knowledgeStateCode
        compositionStageCode = $stageCode
        reviewStateCode = if ($isReviewReady) { "ReviewReady" } else { "AwaitingUnityReviewEvidence" }
        requiredH1Refs = $requiredH1Refs
        recognizedH1Refs = @($recognizedH1Refs)
        unrecognizedH1Refs = @($unrecognizedH1Refs)
        deterministicRecipeId = if ($hasDetailedRecipe) { $recipeTargetIds[$h2StableId] } else { $null }
        theoryRecipeSourceCode = [string] $theoryPlan.recipeSourceCode
        theoryHashSha256 = [string] $theoryPlan.theoryHashSha256
        unityReviewEvidenceRef = if ($hasUnityReviewEvidence) { [string] $unityEvidenceByTarget[$h2StableId].evidenceRef } else { $null }
        composable = $isComposable
        theoryQualified = $isTheoryQualified
        reviewReady = $isReviewReady
        theoryProductionBlockedByHumanReview = $false
        theoryBlockers = @($theoryBlockers)
        reviewBlockers = @($reviewBlockers)
        blockers = @($theoryBlockers)
        gamePlanCodes = @($gamePlanCodes)
        gameplayGateModeCode = $gameplayGateModeCode
        gameplayTraceStateCode = $gameplayTraceStateCode
        gameplayTraceSliceIds = if ($hasGameplayTrace) { @($h2GameplayTrace[$h2StableId].sliceIds) } else { @() }
        gameplayTraceStepIds = if ($hasGameplayTrace) { @($h2GameplayTrace[$h2StableId].stepIds) } else { @() }
        gameplayGateSatisfied = $gameplayGateSatisfied
        gameplayTheoryPriorityReady = $gameplayTheoryPriorityReady
        gameplayReviewPriorityReady = $gameplayReviewPriorityReady
        gameplayPriorityReady = $gameplayTheoryPriorityReady
        gameplayBlockers = @($gameplayBlockers)
        gameplayWarnings = @($gameplayWarnings)
    }
}

$counts = [ordered]@{
    h1RecognizedPartCount = @($h1ById.Values | Where-Object { Test-H1RecognizedPart $_ }).Count
    h1UsedByH2Count = @($items.requiredH1Refs | Sort-Object -Unique).Count
    h2CandidateCount = $items.Count
    h2ComposableCount = @($items | Where-Object composable).Count
    h2TheoryQualifiedCount = @($items | Where-Object theoryQualified).Count
    h2AuthoredTheoryRecipeCount = @($items | Where-Object theoryRecipeSourceCode -eq "AuthoredRecipe").Count
    h2DerivedTheoryRecipeCount = @($items | Where-Object theoryRecipeSourceCode -eq "DerivedTheoryRecipe").Count
    h2DetailedRecipeReadyCount = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_.deterministicRecipeId) }).Count
    h2UnityReviewEvidenceCount = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_.unityReviewEvidenceRef) }).Count
    h2ReviewReadyCount = @($items | Where-Object reviewReady).Count
    h2TheoryProductionBlockedByHumanReviewCount = @($items | Where-Object theoryProductionBlockedByHumanReview).Count
    h2GameplayTraceCount = @($items | Where-Object gameplayTraceStateCode -eq "SequenceMapped").Count
    h2StrictGameplayGateCount = @($items | Where-Object gameplayGateModeCode -eq "Strict").Count
    h2StrictGameplayTraceMissingCount = @($items | Where-Object { $_.gameplayGateModeCode -eq "Strict" -and -not $_.gameplayGateSatisfied }).Count
    h2WarningOnlyGameplayTraceMissingCount = @($items | Where-Object { $_.gameplayGateModeCode -eq "WarningOnly" -and @($_.gameplayWarnings).Count -gt 0 }).Count
    h2GameplayTheoryPriorityReadyCount = @($items | Where-Object gameplayTheoryPriorityReady).Count
    h2GameplayReviewPriorityReadyCount = @($items | Where-Object gameplayReviewPriorityReady).Count
    h2GameplayPriorityReadyCount = @($items | Where-Object gameplayTheoryPriorityReady).Count
}

$result = [ordered]@{
    schemaVersion = "simulation-world-h2-composition-readiness.v1"
    revision = "simulation-world-h2-composition-readiness.r3"
    catalogRevision = [string] $catalog.revision
    catalogSha256 = Get-Sha256Lower $resolvedCatalogPath
    priorityRevision = [string] $priority.revision
    prioritySha256 = Get-Sha256Lower $resolvedPriorityPath
    recipeRevision = [string] $recipes.revision
    recipeSha256 = Get-Sha256Lower $resolvedRecipePath
    unityEvidenceRevision = [string] $unityEvidenceCatalog.revision
    unityEvidenceSha256 = Get-Sha256Lower $resolvedUnityEvidencePath
    gameplayCompletionRevision = [string] $gameplayCompletion.revision
    gameplayCompletionSha256 = Get-Sha256Lower $resolvedGameplayCompletionPath
    theoryFactoryRevision = [string] $theoryFactory.revision
    theoryFactoryPolicyRevision = [string] $theoryFactory.policyRevision
    theoryFactorySha256 = Get-Sha256Lower $resolvedTheoryFactoryPath
    recognitionRule = "H1 정의가 존재하고 WI 또는 예상 게임 플레이, 공간 역할, Synty 팩 또는 기준 문법 표현 근거가 각각 하나 이상이면 인지 부품이다. 공식 Simulation 승인 상태는 H2 조합 조건이 아니다."
    theoryProductionRule = "H2는 인지된 필수 H1, 게임 맥락, 연결된 결정적 상대 배치와 출입 연결구를 갖추면 TheoryQualified가 된다. 사람 검토와 게임플레이 추적은 이론 생산을 막지 않는다."
    reviewRule = "TheoryQualified H2에 Unity H2 Root와 표준 5시점 촬영 근거가 등록되면 사람 검토 준비가 된다. 검토 부재는 별도 상태로 보이되 이론 생산을 되돌리지 않는다."
    gameplayRule = "Nature·Farm의 엄격 추적은 게임플레이 우선순위만 막는다. Town·Hub 추적 누락은 경고이며, 어느 추적 상태도 H2 이론 생산의 자격 조건이 아니다."
    counts = $counts
    items = $items
    compatibilityNote = "gameplayPriorityReady는 gameplayTheoryPriorityReady의 호환 별칭이다. 사람 검토 우선순위는 gameplayReviewPriorityReady를 사용한다."
    authorityBoundary = "이 결과의 TheoryQualified는 위치 독립 이론 공간 생산 상태다. 사람의 공식 H2 승인, 실제 지역 E5 배치, E6 공공데이터, E7 Runtime 또는 Play Mode 증거가 아니다."
    presentationOnly = $true
    isOperationalState = $false
}

$json = ($result | ConvertTo-Json -Depth 20) + "`n"
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# H2 공간 조합 준비도")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$JsonOutputPath``와 함께 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- H1 인지 부품: ``$($counts.h1RecognizedPartCount)``")
[void] $builder.AppendLine("- H2에서 사용하는 H1: ``$($counts.h1UsedByH2Count)``")
[void] $builder.AppendLine("- H2 조합 가능: ``$($counts.h2ComposableCount) / $($counts.h2CandidateCount)``")
[void] $builder.AppendLine("- H2 이론 공간 생산 완료: ``$($counts.h2TheoryQualifiedCount) / $($counts.h2CandidateCount)``")
[void] $builder.AppendLine("- 작성 조립법 재사용 / 이론 파생 조립법: ``$($counts.h2AuthoredTheoryRecipeCount) / $($counts.h2DerivedTheoryRecipeCount)``")
[void] $builder.AppendLine("- 사람 검토 때문에 이론 생산이 막힌 H2: ``$($counts.h2TheoryProductionBlockedByHumanReviewCount)``")
[void] $builder.AppendLine("- Unity H2 Root·5시점 근거 등록: ``$($counts.h2UnityReviewEvidenceCount)``")
[void] $builder.AppendLine("- H2 사람 검토 준비: ``$($counts.h2ReviewReadyCount)``")
[void] $builder.AppendLine("- 기준 플레이 H2 추적: ``$($counts.h2GameplayTraceCount)``")
[void] $builder.AppendLine("- 엄격 관문 H2 / 추적 누락: ``$($counts.h2StrictGameplayGateCount) / $($counts.h2StrictGameplayTraceMissingCount)``")
[void] $builder.AppendLine("- 경고 전용 추적 누락: ``$($counts.h2WarningOnlyGameplayTraceMissingCount)``")
[void] $builder.AppendLine("- 게임플레이 우선 이론 생산 / 사람 검토 준비: ``$($counts.h2GameplayTheoryPriorityReadyCount) / $($counts.h2GameplayReviewPriorityReadyCount)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("H1의 재고 상태와 사람 검토 대기 여부는 H2 이론 공간 생산을 직접 막지 않는다. 존재·게임 맥락·공간 역할·표현 근거를 인지한 뒤, 이론 공간 공장의 결정성·연결성 관문을 통과하면 ``TheoryQualified``로 생산한다. Unity 검토 자료와 게임플레이 추적은 별도 축으로 남긴다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| H2 후보 | 이론 생산 | 조립법 출처 | 사람 검토 | 게임플레이 관문 | 추적 | H1 | 이론 차단 | 검토 차단 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- | ---: | --- | --- |")
foreach ($item in $items) {
    $theoryBlockerText = if (@($item.theoryBlockers).Count -eq 0) { "없음" } else { @($item.theoryBlockers) -join ", " }
    $reviewBlockerText = if (@($item.reviewBlockers).Count -eq 0) { "없음" } else { @($item.reviewBlockers) -join ", " }
    [void] $builder.AppendLine("| $($item.title) (``$($item.h2StableId)``) | ``$($item.compositionStageCode)`` | ``$($item.theoryRecipeSourceCode)`` | ``$($item.reviewStateCode)`` | ``$($item.gameplayGateModeCode)`` | ``$($item.gameplayTraceStateCode)`` | $(@($item.requiredH1Refs).Count) | $theoryBlockerText | $reviewBlockerText |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 판정 경계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- ``TheoryQualified``: 상대 좌표·위상·관계·연결구를 가진 결정적 위치 독립 H2 이론 공간이다.")
[void] $builder.AppendLine("- ``AuthoredRecipe``와 ``DerivedTheoryRecipe``는 출처를 구분한다. 둘 다 같은 이론 품질 관문을 통과해야 한다.")
[void] $builder.AppendLine("- ``ReviewReady``: Unity H2 Root와 표준 5시점 촬영 근거가 등록돼 사람이 사후 검토를 시작할 수 있다.")
[void] $builder.AppendLine("- 게임플레이 추적은 작업 우선순위를 정하지만 이론 생산을 차단하거나 되돌리지 않는다.")
[void] $builder.AppendLine("- 어느 상태도 사람의 공식 H2 승인, 실제 지역 E5 배치, E6 공공데이터, E7 Runtime·Play Mode 검증을 뜻하지 않는다.")
$markdown = $builder.ToString()

if ($Mode -eq "Write") {
    Write-DeterministicTextIfChanged $resolvedJsonOutputPath $json | Out-Null
    Write-DeterministicTextIfChanged $resolvedMarkdownOutputPath $markdown | Out-Null
}
else {
    Require (Test-Path -LiteralPath $resolvedJsonOutputPath) "JsonOutputMissing"
    Require (Test-Path -LiteralPath $resolvedMarkdownOutputPath) "MarkdownOutputMissing"
    Require ((ConvertTo-DeterministicText (Get-Content -LiteralPath $resolvedJsonOutputPath -Raw -Encoding UTF8)) -ceq (ConvertTo-DeterministicText $json)) "JsonOutputStale"
    Require ((ConvertTo-DeterministicText (Get-Content -LiteralPath $resolvedMarkdownOutputPath -Raw -Encoding UTF8)) -ceq (ConvertTo-DeterministicText $markdown)) "MarkdownOutputStale"
}

Write-Output "H2CompositionReadinessValid:H1=$($counts.h1RecognizedPartCount);Used=$($counts.h1UsedByH2Count);H2=$($counts.h2ComposableCount)/$($counts.h2CandidateCount);Theory=$($counts.h2TheoryQualifiedCount);Authored=$($counts.h2AuthoredTheoryRecipeCount);Derived=$($counts.h2DerivedTheoryRecipeCount);Unity=$($counts.h2UnityReviewEvidenceCount);Review=$($counts.h2ReviewReadyCount);Gameplay=$($counts.h2GameplayTraceCount);TheoryPriority=$($counts.h2GameplayTheoryPriorityReadyCount);ReviewPriority=$($counts.h2GameplayReviewPriorityReadyCount);StrictMissing=$($counts.h2StrictGameplayTraceMissingCount);Warnings=$($counts.h2WarningOnlyGameplayTraceMissingCount)"
