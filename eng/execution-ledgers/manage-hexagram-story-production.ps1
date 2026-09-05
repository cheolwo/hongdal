[CmdletBinding()]
param(
    [ValidateSet('Write', 'Check', 'Validate')]
    [string] $Mode = 'Check',
    [string] $InputPath = 'eng/execution-ledgers/hexagram-story-production.json',
    [string] $JsonOutputPath = 'docs/AI/generated/hexagram-story-production.json',
    [string] $MarkdownOutputPath = 'docs/AI/generated/hexagram-story-production.md'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../common/deterministic-text-output.ps1')

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramStoryProductionInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Resolve-InputFile([string] $Path, [string] $Code) {
    $candidate = if ([IO.Path]::IsPathRooted($Path)) {
        [IO.Path]::GetFullPath($Path)
    } else {
        [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path))
    }
    Require (Test-Path -LiteralPath $candidate -PathType Leaf) "$Code`:Missing:$Path"
    return $candidate
}

function Resolve-RepositoryFile([string] $Path, [string] $Code) {
    Require-Text $Path "$Code`:PathMissing"
    Require (-not [IO.Path]::IsPathRooted($Path)) "$Code`:Rooted:$Path"
    $candidate = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path))
    $prefix = $repositoryRoot.TrimEnd('\') + '\'
    Require ($candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) "$Code`:Traversal:$Path"
    Require (Test-Path -LiteralPath $candidate -PathType Leaf) "$Code`:Missing:$Path"
    return $candidate
}

function Resolve-RepositoryOutput([string] $Path, [string] $Code) {
    Require-Text $Path "$Code`:PathMissing"
    Require (-not [IO.Path]::IsPathRooted($Path)) "$Code`:Rooted:$Path"
    $candidate = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path))
    $prefix = $repositoryRoot.TrimEnd('\') + '\'
    Require ($candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) "$Code`:Traversal:$Path"
    return $candidate
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Get-TraditionalLineName([string] $PolarityCode, [int] $Ordinal) {
    $polarityName = if ($PolarityCode -eq 'Yang') { '九' } else { '六' }
    if ($Ordinal -eq 1) { return "初$polarityName" }
    if ($Ordinal -eq 6) { return "上$polarityName" }
    $positionNames = @('', '', '二', '三', '四', '五')
    return "$polarityName$($positionNames[$Ordinal])"
}

function Get-ExpectedHexagramId([object] $Hexagram) {
    $suffix = ([string] $Hexagram.stableId) -replace '^HEX-\d{2}-', ''
    return 'HEX-{0:D2}-{1}' -f [int] $Hexagram.ordinal, $suffix
}

function Get-FileSnapshot([string] $Path) {
    $resolved = Resolve-RepositoryFile $Path 'Snapshot'
    return [ordered]@{
        path = $Path
        sha256 = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash.ToUpperInvariant()
    }
}

$resolvedInput = Resolve-InputFile $InputPath 'Input'
$source = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $source.schemaVersion -eq 'mirror-hexagram-story-production.v3') 'SchemaVersion'
Require ([string] $source.revision -eq 'hexagram-story-production.r14') 'Revision'
Require ([string] $source.canonicalPlan.planId -eq 'PLAN-STORY-HEXAGRAM-SEQUENCE-001') 'CanonicalPlanId'
Require ([string] $source.canonicalPlan.displayTitle -eq '역경 64괘 기반 게임 스토리 기획') 'CanonicalDisplayTitle'
Require ([string] $source.canonicalPlan.responseHeaderTemplate -eq '[기획 · 역경 384효 스토리 · {planId} · 제{hexagramOrdinal}괘 {hexagramName}/{lineName} · {revision}]') 'ResponseHeaderTemplate'

$canonicalPlanPath = Resolve-RepositoryFile ([string] $source.canonicalPlan.documentRef) 'CanonicalPlan'
$broadStoryOutlinePath = Resolve-RepositoryFile ([string] $source.canonicalPlan.broadStoryOutlineRef) 'BroadStoryOutline'
$handoffPath = Resolve-RepositoryFile ([string] $source.canonicalPlan.handoffDocumentRef) 'HandoffDocument'
$mainStoryPath = Resolve-RepositoryFile ([string] $source.canonicalPlan.mainStoryDocumentRef) 'MainStoryDocument'
$planningIndexPath = Resolve-RepositoryFile 'docs/AI/PLANNING.md' 'PlanningIndex'
$planningText = Get-Content -LiteralPath $planningIndexPath -Raw -Encoding UTF8
Require ($planningText.Contains('`PLAN-STORY-HEXAGRAM-SEQUENCE-001`')) 'PlanningIndexRegistrationMissing'

$sourceIds = @($source.sources | ForEach-Object { [string] $_.sourceId })
Require ($sourceIds.Count -eq 2) 'SourceCount'
Require ($sourceIds.Count -eq @($sourceIds | Sort-Object -Unique).Count) 'SourceDuplicate'
foreach ($requiredSourceId in @('unicode-yijing-hexagram-symbols-v17', 'wikisource-zhouyi-traditional')) {
    Require ($sourceIds -contains $requiredSourceId) "SourceMissing:$requiredSourceId"
}
foreach ($sourceRef in @($source.sources)) {
    Require-Text $sourceRef.title "SourceTitle:$($sourceRef.sourceId)"
    Require ([uri]::IsWellFormedUriString([string] $sourceRef.url, [UriKind]::Absolute)) "SourceUrl:$($sourceRef.sourceId)"
    Require ([string] $sourceRef.accessedOn -match '^\d{4}-\d{2}-\d{2}$') "SourceAccessedOn:$($sourceRef.sourceId)"
}

$policy = $source.policy
Require ([string] $policy.productionOrderCode -eq 'MainCampaignKingWenHexagramAndBottomToTopLineOrder') 'ProductionOrderPolicy'
Require ([string] $policy.planningIntakePolicyCode -eq 'BroadHexagramStoryThenOneLineAdaptationAtATime') 'PlanningIntakePolicy'
Require ([string] $policy.broadStoryOutlineStatusCode -eq 'Proposed') 'BroadStoryOutlineStatus'
Require ([string] $policy.lineAdaptationPolicyCode -eq 'PreserveOriginalMeaningAndRecordCreativeDifferences') 'LineAdaptationPolicy'
Require ([string] $policy.existingPlanningReferencePolicyCode -eq 'TechnicalAppendixReferenceOnly') 'ExistingPlanningReferencePolicy'
Require (-not [bool] $policy.existingPlanningClassificationIsMainStoryAuthority) 'ExistingPlanningAuthorityBoundary'
Require ([string] $policy.playerPlayOrderPolicyCode -eq 'ContinuousHexagramChapterWithInterHexagramFreeStay') 'PlayerPlayOrderPolicy'
Require ([string] $policy.hierarchyCode -eq 'MainStoryHexagramArcLineStory') 'HierarchyPolicy'
Require ([string] $policy.formalAuthoringHexagramStableId -eq 'HEX-01-QIAN') 'FormalAuthoringCursor'
Require ([string] $policy.prototypeDialogueLineStableId -eq 'HEX-04-MENG-L3') 'PrototypeDialogueCursor'
Require ([string] $policy.nextPrototypeDialogueLineStableId -eq 'HEX-04-MENG-L4') 'NextPrototypeDialogueCursor'
Require ([string] $policy.runtimeCampaignStateCode -eq 'NotEstablished') 'RuntimeCampaignBoundary'
Require ((@($policy.foundationalFrameHexagramStableIds) -join ',') -eq 'HEX-01-QIAN,HEX-02-KUN') 'FoundationalFrameHexagrams'
Require ([string] $policy.foundationalLineContentCode -eq 'SixShortPlayablePrologueBeats') 'FoundationalLineContent'
Require ([string] $policy.concreteStoryStartHexagramStableId -eq 'HEX-03-ZHUN') 'ConcreteStoryStart'
Require ([string] $policy.concreteLineContentCode -eq 'PlayableStoryCandidate') 'ConcreteLineContent'
Require ([string] $policy.foundationRealizationPolicyCode -eq 'QianKunPlayableProloguesLeadIntoHexagram03') 'FoundationRealizationPolicy'
Require ([bool] $policy.prologueDevelopmentRequiresApprovedLineRequirements) 'PrologueDevelopmentBoundary'
Require ([string] $policy.visualCompanionStartsAtHexagramStableId -eq 'HEX-03-ZHUN') 'VisualCompanionStart'
Require ([bool] $policy.concreteHexagramVisualCompanionRequired) 'VisualCompanionRequired'
Require ([string] $policy.visualCompanionFormatCode -eq 'HexagramUpperLowerTrigramLineDirectionMeaningQuestion') 'VisualCompanionFormat'
Require ([bool] $policy.visualCompanionDoesNotPromoteEvidence) 'VisualCompanionEvidenceBoundary'
Require ([int] $policy.lineStoryCountPerHexagram -eq 6) 'LineStoryCountPolicy'
Require ([string] $policy.nextHexagramAuthoringGateCode -eq 'AllSixLinesAndArcCoherenceStoryApprovedThenNextHexagramLine1') 'AuthoringGatePolicy'
Require ([string] $policy.downstreamHandoffCode -eq 'ApprovedLineMayHandoffIndependently') 'DownstreamHandoffPolicy'
Require ([string] $policy.lineBeatPolicyCode -eq 'BottomToTopContinuousStoryBeatsWithoutManualPauseOrLineSelection') 'LineBeatPolicy'
Require ([string] $policy.freeStayOpenAfterCode -eq 'UpperLineCompleted') 'FreeStayGate'
Require ([string] $policy.nextCampaignStartPolicyCode -eq 'ExplicitPlayerStartOnly') 'NextCampaignStartPolicy'
Require ([string] $policy.campaignIdentityLedgerPath -eq 'eng/execution-ledgers/hexagram-campaign-identities.json') 'CampaignIdentityLedgerPath'
Require ([string] $policy.campaignIdentityAuthoringOrderCode -eq 'DraftAll64ThenAuthorLinesInKingWenOrder') 'CampaignIdentityAuthoringOrder'
Require ([string] $policy.campaignIdentityDistinctivenessCode -eq 'UniqueConflictSubjectPressureJudgementCombination') 'CampaignIdentityDistinctiveness'
Require ([string] $policy.campaignMechanicReuseCode -eq 'ReuseComponentsWithUniqueCombination') 'CampaignMechanicReuse'
Resolve-RepositoryFile ([string] $policy.campaignIdentityLedgerPath) 'CampaignIdentityLedger' | Out-Null
Require ([string] $policy.futureVisibilityCode -eq 'CurrentAndCompletedOnly') 'FutureVisibilityPolicy'
Require ([string] $policy.classicalTextPolicyCode -eq 'VerifiedOriginalPlusProjectAuthoredKoreanSummary') 'ClassicalTextPolicy'
Require ([string] $policy.specialQianKunLinePolicyCode -eq 'SupplementaryInterpretationNotSeventhStory') 'SpecialLinePolicy'
Require ([string] $policy.legacyActChapterPolicyCode -eq 'CompatibilityGroupingOnly') 'LegacyHierarchyPolicy'
Require ([string] $policy.fiveElementMetadataRelationCode -eq 'SeparateMetadataOptionalReferenceOnly') 'FiveElementBoundary'
Require ([bool] $policy.doesNotDefineRuntimeUnlocks) 'RuntimeUnlockBoundary'
Require ([bool] $policy.doesNotCreateWorldInteractions) 'WorldInteractionBoundary'
Require ([bool] $policy.doesNotPromoteEvidence) 'EvidenceBoundary'

$allowedProduction = @($source.allowedCodes.productionStatusCodes | ForEach-Object { [string] $_ })
$allowedLineStates = @($source.allowedCodes.lineStoryStatusCodes | ForEach-Object { [string] $_ })
$allowedStoryArcs = @($source.allowedCodes.storyArcStatusCodes | ForEach-Object { [string] $_ })
$allowedVariantStates = @($source.allowedCodes.protagonistVariantStatusCodes | ForEach-Object { [string] $_ })
$allowedMappings = @($source.allowedCodes.mappingCodes | ForEach-Object { [string] $_ })
Require ((@($allowedProduction) -join ',') -eq 'Locked,Active') 'AllowedProductionCodes'
Require ((@($allowedLineStates) -join ',') -eq 'Unmapped,MacroFoundation,StoryApproved,ActiveStoryDialogue,StorySeeded') 'AllowedLineCodes'
Require ((@($allowedStoryArcs) -join ',') -eq 'StoryUnopened,StorySeeded,MacroFoundation,StoryApproved,ActiveStoryDialogue') 'AllowedStoryArcCodes'

$trigramByCode = @{}
foreach ($definition in @($source.trigramDefinitions)) {
    $code = [string] $definition.code
    Require ($code -in @('QIAN', 'KUN', 'ZHEN', 'KAN', 'GEN', 'XUN', 'LI', 'DUI')) "TrigramCode:$code"
    Require (-not $trigramByCode.ContainsKey($code)) "TrigramDuplicate:$code"
    Require (@($definition.lines).Count -eq 3) "TrigramLineCount:$code"
    foreach ($line in @($definition.lines)) {
        Require ([string] $line -in @('Yang', 'Yin')) "TrigramLinePolarity:$code"
    }
    $trigramByCode[$code] = $definition
}
Require ($trigramByCode.Count -eq 8) 'TrigramDefinitionCount'

$hexagrams = @($source.hexagrams | Sort-Object ordinal)
Require ($hexagrams.Count -eq 64) "HexagramCount:$($hexagrams.Count)"
Require (@($hexagrams.stableId | Sort-Object -Unique).Count -eq 64) 'HexagramStableIdDuplicate'
Require (@($hexagrams.symbol | Sort-Object -Unique).Count -eq 64) 'HexagramSymbolDuplicate'
$linePlanIds = [Collections.Generic.List[string]]::new()

$active = @($hexagrams | Where-Object productionStatusCode -eq 'Active')
Require ($active.Count -eq 1) "ActiveHexagramCount:$($active.Count)"
Require ([string] $active[0].stableId -eq [string] $policy.formalAuthoringHexagramStableId) 'FormalAuthoringCursorMismatch'
$qian = @($hexagrams | Where-Object stableId -eq 'HEX-01-QIAN')
Require ($qian.Count -eq 1) 'QianMissing'
Require ([string] $qian[0].primaryProtagonistRoleCode -eq 'Adventurer') 'QianPrimaryProtagonist'
Require ([string] $qian[0].arcGranularityCode -eq 'ShortPlayablePrologue') 'QianArcGranularity'
Require ([string] $qian[0].supportingEventPolicyCode -eq 'ReferenceLowerLevelPlansWithoutAbsorbingAllEvents') 'QianSupportingEventPolicy'

for ($index = 0; $index -lt $hexagrams.Count; $index++) {
    $hexagram = $hexagrams[$index]
    $ordinal = [int] $hexagram.ordinal
    $stableId = [string] $hexagram.stableId
    Require ($ordinal -eq ($index + 1)) "OrdinalSequence:$stableId"
    Require ($stableId -match ('^HEX-{0:D2}-[A-Z0-9-]+$' -f $ordinal)) "StableId:$stableId"
    Require ($stableId -eq (Get-ExpectedHexagramId $hexagram)) "StableIdOrdinal:$stableId"
    Require ([string] $hexagram.symbol -eq [char]::ConvertFromUtf32(0x4DC0 + $ordinal - 1)) "UnicodeSymbol:$stableId"
    Require-Text $hexagram.nameHanja "NameHanja:$stableId"
    Require-Text $hexagram.nameKorean "NameKorean:$stableId"
    Require ($trigramByCode.ContainsKey([string] $hexagram.upperTrigramCode)) "UpperTrigram:$stableId"
    Require ($trigramByCode.ContainsKey([string] $hexagram.lowerTrigramCode)) "LowerTrigram:$stableId"
    Require ([string] $hexagram.productionStatusCode -in $allowedProduction) "ProductionStatus:$stableId"
    Require ([string] $hexagram.storyArcStatusCode -in $allowedStoryArcs) "StoryArcStatus:$stableId"

    $expectedPredecessor = if ($ordinal -eq 1) { '' } else { [string] $hexagrams[$index - 1].stableId }
    $expectedSuccessor = if ($ordinal -eq 64) { '' } else { [string] $hexagrams[$index + 1].stableId }
    Require ([string] $hexagram.predecessorStableId -eq $expectedPredecessor) "Predecessor:$stableId"
    Require ([string] $hexagram.successorStableId -eq $expectedSuccessor) "Successor:$stableId"

    if ($ordinal -gt 1) {
        Require ([string] $hexagram.productionStatusCode -eq 'Locked') "FutureNotLocked:$stableId"
    }

    $lowerLines = @($trigramByCode[[string] $hexagram.lowerTrigramCode].lines | ForEach-Object { [string] $_ })
    $upperLines = @($trigramByCode[[string] $hexagram.upperTrigramCode].lines | ForEach-Object { [string] $_ })
    $expectedPolarities = @($lowerLines + $upperLines)
    $lineStories = @($hexagram.lineStories)
    Require ($lineStories.Count -eq 6) "LineStoryCount:$stableId`:$($lineStories.Count)"
    Require (@($lineStories.stableId | Sort-Object -Unique).Count -eq 6) "LineStableIdDuplicate:$stableId"
    for ($lineIndex = 0; $lineIndex -lt 6; $lineIndex++) {
        $lineStory = $lineStories[$lineIndex]
        $lineOrdinal = $lineIndex + 1
        $lineId = "$stableId-L$lineOrdinal"
        Require ([int] $lineStory.ordinal -eq $lineOrdinal) "LineOrdinal:$lineId"
        Require ([string] $lineStory.stableId -eq $lineId) "LineStableId:$lineId"
        $expectedLinePlanId = 'PLAN-STORY-HEX{0:D2}-LINE-{1:D3}' -f $ordinal, $lineOrdinal
        Require ([string] $lineStory.linePlanId -eq $expectedLinePlanId) "LinePlanId:$lineId"
        [void] $linePlanIds.Add([string] $lineStory.linePlanId)
        Require ([string] $lineStory.polarityCode -eq $expectedPolarities[$lineIndex]) "LinePolarity:$lineId"
        Require ([string] $lineStory.traditionalName -eq (Get-TraditionalLineName $expectedPolarities[$lineIndex] $lineOrdinal)) "LineTraditionalName:$lineId"
        Require ([string] $lineStory.progressLabel -eq "$lineOrdinal/6") "LineProgress:$lineId"
        Require ([string] $lineStory.storyStatusCode -in $allowedLineStates) "LineStatus:$lineId"
        Require ([string] $lineStory.protagonistVariants.adventurerStatusCode -in $allowedVariantStates) "AdventurerVariant:$lineId"
        Require ([string] $lineStory.protagonistVariants.youngLordStatusCode -in $allowedVariantStates) "YoungLordVariant:$lineId"
    }

    foreach ($mapping in @($hexagram.legacyMappings)) {
        Require ([string] $mapping.mappingCode -in $allowedMappings) "LegacyMappingCode:$stableId"
        Require-Text $mapping.planId "LegacyMappingPlan:$stableId"
        [void] (Resolve-RepositoryFile ([string] $mapping.documentRef) "LegacyMappingDocument:$stableId")
        Require ($planningText.Contains(('`{0}`' -f [string] $mapping.planId))) "LegacyMappingPlanUnknown:$stableId"
        if ([string] $hexagram.productionStatusCode -eq 'Locked') {
            Require ([string] $mapping.mappingCode -ne 'Accepted') "LockedAcceptedMapping:$stableId"
        }
    }

    $specialCommentaries = @($hexagram.specialCommentaries)
    if ($ordinal -in @(1, 2)) {
        Require ($specialCommentaries.Count -eq 1) "SpecialCommentaryMissing:$stableId"
        Require ([string] $specialCommentaries[0].handlingCode -eq 'SupplementaryInterpretation') "SpecialCommentaryHandling:$stableId"
        Require (-not [bool] $specialCommentaries[0].createsLineStory) "SpecialCommentaryCreatesStory:$stableId"
    } else {
        Require ($specialCommentaries.Count -eq 0) "UnexpectedSpecialCommentary:$stableId"
    }
}
Require ($linePlanIds.Count -eq 384) "LinePlanIdCount:$($linePlanIds.Count)"
Require (@($linePlanIds | Sort-Object -Unique).Count -eq 384) 'LinePlanIdDuplicate'

$zhun = @($hexagrams | Where-Object ordinal -eq 3)[0]
Require ([string] $zhun.storyArcStatusCode -eq 'StoryApproved') 'ZhunStoryStatus'
Require (@($zhun.legacyMappings | Where-Object mappingCode -eq 'PrototypeReference').Count -eq 1) 'HansPrototypeMapping'
Require (@($zhun.lineStories | Where-Object storyStatusCode -eq 'StoryApproved').Count -eq 6) 'ZhunStoryApprovedLineCount'
$qianKun = @($hexagrams | Where-Object ordinal -in @(1, 2))
Require (@($qianKun | Where-Object storyArcStatusCode -eq 'StorySeeded').Count -eq 2) 'PrologueStorySeededHexagrams'
Require (@($qianKun.lineStories | Where-Object storyStatusCode -eq 'StorySeeded').Count -eq 12) 'PrologueStorySeededLines'
foreach ($prologue in $qianKun) {
    Require ([string] $prologue.primaryProtagonistRoleCode -eq 'Adventurer') "ProloguePrimaryProtagonist:$($prologue.stableId)"
    Require ([string] $prologue.arcGranularityCode -eq 'ShortPlayablePrologue') "PrologueArcGranularity:$($prologue.stableId)"
    foreach ($lineStory in @($prologue.lineStories)) {
        Require-Text $lineStory.classicalMeaningSummary "PrologueMeaning:$($lineStory.stableId)"
        Require-Text $lineStory.narrativeQuestion "PrologueQuestion:$($lineStory.stableId)"
        Require-Text $lineStory.storySynopsis "PrologueSynopsis:$($lineStory.stableId)"
    }
}
$meng = @($hexagrams | Where-Object ordinal -eq 4)[0]
Require ([string] $meng.storyArcStatusCode -eq 'ActiveStoryDialogue') 'MengStoryStatus'
Require (@($meng.lineStories | Select-Object -First 2 | Where-Object storyStatusCode -eq 'StoryApproved').Count -eq 2) 'MengApprovedLineCount'
Require ([string] $meng.lineStories[2].storyStatusCode -eq 'ActiveStoryDialogue') 'MengLine3Active'
Require (@($meng.lineStories | Select-Object -Skip 3 | Where-Object storyStatusCode -eq 'StorySeeded').Count -eq 3) 'MengSeededLineCount'
Require (@($hexagrams.lineStories | Where-Object storyStatusCode -eq 'ActiveStoryDialogue').Count -eq 1) 'ActiveStoryDialogueCount'

$presentation = $source.presentationE4Preparation
Require ([string] $presentation.scopeCode -eq 'ContractAndWireframeOnly') 'PresentationScope'
Require ([string] $presentation.hudPlacementCode -eq 'TopRight') 'HudPlacement'
Require ([string] $presentation.example -eq '䷀ 乾 · 중천건 · 초구 · 1/6') 'HudExample'
Require ((@($presentation.viewStateCodes) -join ',') -eq 'Hidden,Current,Completed') 'PresentationViewStates'
Require ([string] $presentation.futureItemPolicyCode -eq 'DoNotExpose') 'PresentationFuturePolicy'
Require ([string] $presentation.glyphFallbackCode -eq 'BundledSpriteForU4DC0ToU4DFFWhenFontUnsupported') 'PresentationGlyphFallback'
Require (-not [bool] $presentation.actualUnityImplementationIncluded) 'PresentationUnityBoundary'
Require (-not [bool] $presentation.evidencePromotionIncluded) 'PresentationEvidenceBoundary'

$result = [ordered]@{
    authorityCode = 'ReferenceOnlyNotStoryOrRuntimeOrder'
    currentPolicyRef = 'docs/Architecture/스토리영감과플레이진행분리.md'
    schemaVersion = 'mirror-hexagram-story-production-index.v3'
    revision = [string] $source.revision
    sourceSnapshots = @(
        [ordered]@{ path = $InputPath; sha256 = (Get-FileHash -LiteralPath $resolvedInput -Algorithm SHA256).Hash.ToUpperInvariant() },
        (Get-FileSnapshot ([string] $source.canonicalPlan.documentRef)),
        (Get-FileSnapshot ([string] $source.canonicalPlan.broadStoryOutlineRef)),
        (Get-FileSnapshot ([string] $source.canonicalPlan.handoffDocumentRef)),
        (Get-FileSnapshot ([string] $source.canonicalPlan.mainStoryDocumentRef)),
        (Get-FileSnapshot 'docs/AI/PLANNING.md')
    )
    counts = [ordered]@{
        hexagrams = $hexagrams.Count
        lineStories = @($hexagrams | ForEach-Object { @($_.lineStories) }).Count
        linePlans = $linePlanIds.Count
        active = $active.Count
        locked = @($hexagrams | Where-Object productionStatusCode -eq 'Locked').Count
        playablePrologue = $qianKun.Count
        storyApproved = @($hexagrams | Where-Object storyArcStatusCode -eq 'StoryApproved').Count
        activeStoryDialogue = @($hexagrams.lineStories | Where-Object storyStatusCode -eq 'ActiveStoryDialogue').Count
        storySeeded = @($hexagrams.lineStories | Where-Object storyStatusCode -eq 'StorySeeded').Count
    }
    canonicalPlan = $source.canonicalPlan
    sources = @($source.sources)
    policy = $source.policy
    presentationE4Preparation = $source.presentationE4Preparation
    trigramDefinitions = @($source.trigramDefinitions)
    hexagrams = @($hexagrams)
}

$jsonText = ConvertTo-DeterministicText (($result | ConvertTo-Json -Depth 30) + "`n")
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine('> 참고 색인: 괘·효 순서는 자료의 위치이며 이야기·제작·실행 순서가 아니다. 현행 기준은 [스토리 영감과 플레이 진행 분리](../../Architecture/스토리영감과플레이진행분리.md)다. 아래 상태·순차 관문은 이전 기획 이력으로 보존한다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('# 역경 64괘 기반 게임 스토리 기획 색인')
[void] $builder.AppendLine()
[void] $builder.AppendLine('> 이 문서는 `hexagram-story-production.json`에서 자동 생성된다. 직접 수정하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 공부·저작 순서: ``$($policy.productionOrderCode)``")
[void] $builder.AppendLine("- 문답 방식: ``$($policy.planningIntakePolicyCode)`` / 기존 기획 참조: ``$($policy.existingPlanningReferencePolicyCode)``")
[void] $builder.AppendLine('- 문답 순서: 괘의 의미와 큰 이야기 제안 → 사용자와 줄기 합의 → 효사 원문·의미 대조 → 각색 차이를 기록한 사건 문답 → 주체·WI·H 요구사항.')
[void] $builder.AppendLine('- [64괘 플레이 스토리 큰 줄기 제안](../Planning/스토리/PLAN-STORY-HEXAGRAM-SEQUENCE-001/괘의미별-플레이스토리-큰줄기.md)은 Proposed이며 기존 승인 효·제작 커서·Runtime·Evidence를 자동 변경하지 않는다.')
[void] $builder.AppendLine("- 정식 제작 커서: ``$($policy.formalAuthoringHexagramStableId)``")
[void] $builder.AppendLine("- 선행 표본 문답 커서: ``$($policy.prototypeDialogueLineStableId)`` / 다음 표본 효: ``$($policy.nextPrototypeDialogueLineStableId)``")
[void] $builder.AppendLine("- Runtime 캠페인 상태: ``$($policy.runtimeCampaignStateCode)``")
[void] $builder.AppendLine("- 실제 플레이 순서: ``$($policy.playerPlayOrderPolicyCode)``")
[void] $builder.AppendLine("- 기반층: ``$(@($policy.foundationalFrameHexagramStableIds) -join ', ')`` / 구체 서사 시작: ``$($policy.concreteStoryStartHexagramStableId)``")
[void] $builder.AppendLine("- 시각 동반 시작: ``$($policy.visualCompanionStartsAtHexagramStableId)`` / 형식: ``$($policy.visualCompanionFormatCode)``")
[void] $builder.AppendLine("- 괘: ``$($result.counts.hexagrams)`` / 효 이야기 슬롯: ``$($result.counts.lineStories)`` / 효사 기획 ID: ``$($result.counts.linePlans)`` / StoryApproved 괘: ``$($result.counts.storyApproved)`` / ActiveStoryDialogue 효: ``$($result.counts.activeStoryDialogue)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine('제1괘 건과 제2괘 곤은 여섯 개의 짧은 실제 플레이 비트로 구성하는 서막 캠페인이고, 제3괘 수뢰둔부터 본격 캠페인이 시작된다. 정식 제작 커서는 중천건이며 산수몽 육삼은 순서를 건너뛰지 않는 선행 표본 문답이다.')
[void] $builder.AppendLine('제3괘부터 기획 문답에는 육효 괘상, 상괘·하괘, 효를 아래에서 위로 읽는 방향, 의미 요약과 질문 하나를 함께 보여 준다. 이 시각 자료는 기획 보조이며 Evidence를 승격하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 전체 순서')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 순번 | 괘 | 안정 ID | 상괘 / 하괘 | 공부 상태 | 이야기 상태 | 효 상태 | 다음 괘 |')
[void] $builder.AppendLine('| ---: | --- | --- | --- | --- | --- | --- | --- |')
foreach ($hexagram in $hexagrams) {
    $upper = $trigramByCode[[string] $hexagram.upperTrigramCode]
    $lower = $trigramByCode[[string] $hexagram.lowerTrigramCode]
    $lineStateSummary = @($hexagram.lineStories | Group-Object storyStatusCode | Sort-Object Name | ForEach-Object { "$($_.Name) $($_.Count)" }) -join ', '
    $next = if ([string]::IsNullOrWhiteSpace([string] $hexagram.successorStableId)) { '없음' } else { "``$($hexagram.successorStableId)``" }
    [void] $builder.AppendLine("| $($hexagram.ordinal) | $($hexagram.symbol) $($hexagram.nameHanja) · $(Escape-Cell $hexagram.nameKorean) | ``$($hexagram.stableId)`` | $($upper.symbol) $($upper.nameKorean) / $($lower.symbol) $($lower.nameKorean) | ``$($hexagram.productionStatusCode)`` | ``$($hexagram.storyArcStatusCode)`` | $(Escape-Cell $lineStateSummary) | $next |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 정식 공부 커서')
[void] $builder.AppendLine()
[void] $builder.AppendLine("### $($active[0].symbol) $($active[0].nameHanja) · $($active[0].nameKorean) (`$($active[0].stableId)`)")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 중심 주체: ``$($active[0].primaryProtagonistRoleCode)``")
[void] $builder.AppendLine("- 이야기 규모: ``$($active[0].arcGranularityCode)``")
[void] $builder.AppendLine("- 하위 사건 정책: ``$($active[0].supportingEventPolicyCode)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 효 | 안정 ID | 효사 기획 ID | 상태 | 고전 의미 요약 | 이야기 질문 |')
[void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- |')
foreach ($lineStory in @($active[0].lineStories)) {
    $meaning = if ([string]::IsNullOrWhiteSpace([string] $lineStory.classicalMeaningSummary)) { '미정' } else { [string] $lineStory.classicalMeaningSummary }
    $question = if ([string]::IsNullOrWhiteSpace([string] $lineStory.narrativeQuestion)) { '미정' } else { [string] $lineStory.narrativeQuestion }
    [void] $builder.AppendLine("| $($lineStory.traditionalName) · $($lineStory.progressLabel) | ``$($lineStory.stableId)`` | ``$($lineStory.linePlanId)`` | ``$($lineStory.storyStatusCode)`` | $(Escape-Cell $meaning) | $(Escape-Cell $question) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 승인된 첫 구체 프로토타입')
[void] $builder.AppendLine()
[void] $builder.AppendLine('제3괘 수뢰둔의 여섯 이야기는 StoryApproved다. 각 효는 다른 효의 개발 완료를 기다리지 않고 기술 별책으로 인계할 수 있으며, 승인만으로 WI·H·E를 승격하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 효 | 효사 기획 ID | 프로토타입 이야기 | 연결 기획 |')
[void] $builder.AppendLine('| --- | --- | --- | --- |')
foreach ($lineStory in @($zhun.lineStories)) {
    [void] $builder.AppendLine("| $($lineStory.traditionalName) · $($lineStory.progressLabel) | ``$($lineStory.linePlanId)`` | $(Escape-Cell $lineStory.storySynopsis) | $(Escape-Cell (@($lineStory.planningRefs) -join ', ')) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## HUD Presentation E4 준비 계약')
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 기본 표시: ``$($presentation.example)``")
[void] $builder.AppendLine("- 위치: ``$($presentation.hudPlacementCode)`` / 상태: ``$(@($presentation.viewStateCodes) -join ', ')``")
[void] $builder.AppendLine("- 미래 항목: ``$($presentation.futureItemPolicyCode)`` / 괘상 fallback: ``$($presentation.glyphFallbackCode)``")
[void] $builder.AppendLine('- 실제 Unity HUD와 Evidence 승격은 포함하지 않는다.')
$markdownText = ConvertTo-DeterministicText ($builder.ToString())

if ($Mode -eq 'Write') {
    [void] (Write-DeterministicTextIfChanged -Path (Resolve-RepositoryOutput $JsonOutputPath 'JsonOutput') -Content $jsonText)
    [void] (Write-DeterministicTextIfChanged -Path (Resolve-RepositoryOutput $MarkdownOutputPath 'MarkdownOutput') -Content $markdownText)
} elseif ($Mode -eq 'Check') {
    $resolvedJsonOutput = Resolve-InputFile $JsonOutputPath 'JsonOutput'
    $resolvedMarkdownOutput = Resolve-InputFile $MarkdownOutputPath 'MarkdownOutput'
    Require ((Get-Content -LiteralPath $resolvedJsonOutput -Raw -Encoding UTF8) -ceq $jsonText) 'JsonOutputStale'
    Require ((Get-Content -LiteralPath $resolvedMarkdownOutput -Raw -Encoding UTF8) -ceq $markdownText) 'MarkdownOutputStale'
}

Write-Output "HexagramStoryProduction:$Mode`:OK:Hexagrams=$($result.counts.hexagrams):Lines=$($result.counts.lineStories):Formal=$($policy.formalAuthoringHexagramStableId):Prototype=$($policy.prototypeDialogueLineStableId)"
