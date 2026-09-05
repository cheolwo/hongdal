[CmdletBinding()]
param(
    [ValidateSet('Write', 'Check', 'Validate')]
    [string] $Mode = 'Check',
    [string] $InputPath = 'eng/execution-ledgers/hexagram-story-tree.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../common/deterministic-text-output.ps1')

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramStoryTreeInvalid:$Code" }
}

function Resolve-RepositoryPath([string] $Path, [string] $Code, [bool] $MustExist) {
    Require (-not [string]::IsNullOrWhiteSpace($Path)) "$Code`:PathMissing"
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

function Get-GeneratedRelativeLink([string] $RepositoryPath) {
    if ($RepositoryPath.StartsWith('docs/AI/', [StringComparison]::OrdinalIgnoreCase)) {
        return '../' + $RepositoryPath.Substring('docs/AI/'.Length)
    }
    return '../../../' + $RepositoryPath
}

function Get-ReferenceAnchor([string] $Prefix, [string] $Value) {
    return ('ref-' + $Prefix + '-' + $Value.ToLowerInvariant().Replace(':', '-').Replace('.', '-').Replace('_', '-'))
}

$inputFile = if ([IO.Path]::IsPathRooted($InputPath)) { [IO.Path]::GetFullPath($InputPath) } else { Resolve-RepositoryPath $InputPath 'Input' $true }
$source = Get-Content -LiteralPath $inputFile -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $source.schemaVersion -eq 'mirror-hexagram-story-tree.v1') 'SchemaVersion'
Require ([string] $source.revision -eq 'hexagram-story-tree.r2') 'Revision'

$production = Get-Content -LiteralPath (Resolve-RepositoryPath ([string] $source.productionLedgerPath) 'Production' $true) -Raw -Encoding UTF8 | ConvertFrom-Json
$campaignIdentities = Get-Content -LiteralPath (Resolve-RepositoryPath ([string] $source.campaignIdentityLedgerPath) 'CampaignIdentities' $true) -Raw -Encoding UTF8 | ConvertFrom-Json
$seeds = Get-Content -LiteralPath (Resolve-RepositoryPath ([string] $source.storySeedLedgerPath) 'Seeds' $true) -Raw -Encoding UTF8 | ConvertFrom-Json
$existingPlanning = Get-Content -LiteralPath (Resolve-RepositoryPath ([string] $source.existingPlanningClassificationPath) 'ExistingPlanning' $true) -Raw -Encoding UTF8 | ConvertFrom-Json
$requirements = Get-Content -LiteralPath (Resolve-RepositoryPath ([string] $source.lineRequirementLedgerPath) 'Requirements' $true) -Raw -Encoding UTF8 | ConvertFrom-Json
$worldInteractions = Get-Content -LiteralPath (Resolve-RepositoryPath ([string] $source.worldInteractionCatalogPath) 'WorldInteractions' $true) -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $production.schemaVersion -eq 'mirror-hexagram-story-production.v3') 'ProductionSchema'
Require ([string] $campaignIdentities.schemaVersion -eq 'mirror-hexagram-campaign-identities.v1') 'CampaignIdentitySchema'
Require ([string] $seeds.schemaVersion -eq 'mirror-hexagram-story-seeds.v1') 'SeedSchema'
Require ([string] $existingPlanning.schemaVersion -eq 'mirror-hexagram-existing-planning-classifications.v1') 'ExistingPlanningSchema'
Require ([string] $requirements.schemaVersion -eq 'mirror-hexagram-line-planning-requirements.v1') 'RequirementSchema'

$policy = $source.policy
Require ([int] $policy.hexagramCount -eq 64) 'HexagramCountPolicy'
Require ([int] $policy.lineCountPerHexagram -eq 6) 'LineCountPolicy'
Require ([string] $policy.physicalDocumentPolicyCode -eq 'OpenedLinesOnly') 'PhysicalDocumentPolicy'
Require ([string] $policy.unopenedLinePolicyCode -eq 'GeneratedStableAnchor') 'UnopenedLinePolicy'
Require ([string] $policy.existingPlanningScopeCode -eq 'PlayerExperienceOnly') 'PlanningScopePolicy'
Require ([string] $policy.formalStudyOrderCode -eq 'StrictKingWen01To64') 'StudyOrderPolicy'
foreach ($flag in @('doesNotCreateLineDocuments', 'doesNotCreateWorldInteractionsOrH', 'doesNotActivateDevelopment', 'doesNotPromoteEvidence')) {
    Require ([bool] $policy.$flag) "PolicyFlag:$flag"
}

$hexagrams = @($production.hexagrams | Sort-Object ordinal)
Require ($hexagrams.Count -eq 64) 'HexagramCount'
Require (($hexagrams.ordinal -join ',') -eq ((1..64) -join ',')) 'HexagramOrder'
$allLines = @($hexagrams | ForEach-Object { @($_.lineStories) })
Require ($allLines.Count -eq 384) 'LineCount'
Require (@($allLines.stableId | Sort-Object -Unique).Count -eq 384) 'LineStableIdDuplicate'
Require (@($allLines.linePlanId | Sort-Object -Unique).Count -eq 384) 'LinePlanIdDuplicate'
$hexagramIds = @($hexagrams | ForEach-Object { [string] $_.stableId })
$campaignIdentityByHexagram = @{}
foreach ($identity in @($campaignIdentities.campaigns)) {
    $identityHexagramId = [string] $identity.hexagramStableId
    Require (-not $campaignIdentityByHexagram.ContainsKey($identityHexagramId)) "CampaignIdentityDuplicate:$identityHexagramId"
    Require ($identityHexagramId -in $hexagramIds) "CampaignIdentityUnknown:$identityHexagramId"
    $campaignIdentityByHexagram[$identityHexagramId] = $identity
}
Require ($campaignIdentityByHexagram.Count -eq 64) "CampaignIdentityCount:$($campaignIdentityByHexagram.Count)"

$planningItems = @($existingPlanning.items)
Require (@($planningItems.planId | Sort-Object -Unique).Count -eq $planningItems.Count) 'ExistingPlanningDuplicate'
foreach ($item in $planningItems) {
    $planId = [string] $item.planId
    Require ($planId -match '^PLAN-[A-Z0-9-]+-\d{3}$') "ExistingPlanningId:$planId"
    Require (-not [string]::IsNullOrWhiteSpace([string] $item.title)) "ExistingPlanningTitle:$planId"
    [void] (Resolve-RepositoryPath ([string] $item.documentRef) "ExistingPlanningDocument:$planId" $true)
    Require ([string] $item.classificationStateCode -in @('Candidate','Confirmed')) "ExistingPlanningState:$planId"
    Require ([string] $item.primaryHexagramStableId -in $hexagramIds) "ExistingPlanningPrimary:$planId"
    Require (-not [string]::IsNullOrWhiteSpace([string] $item.primaryReason)) "ExistingPlanningReason:$planId"
    $secondary = @($item.secondaryCandidates)
    Require ($secondary.Count -le 2) "ExistingPlanningSecondaryCount:$planId"
    Require (@($secondary.hexagramStableId | Sort-Object -Unique).Count -eq $secondary.Count) "ExistingPlanningSecondaryDuplicate:$planId"
    for ($index = 0; $index -lt $secondary.Count; $index++) {
        Require ([int] $secondary[$index].rank -eq ($index + 1)) "ExistingPlanningSecondaryRank:$planId"
        Require ([string] $secondary[$index].hexagramStableId -in $hexagramIds) "ExistingPlanningSecondaryUnknown:$planId"
        Require ([string] $secondary[$index].hexagramStableId -ne [string] $item.primaryHexagramStableId) "ExistingPlanningSecondaryMatchesPrimary:$planId"
        Require (-not [string]::IsNullOrWhiteSpace([string] $secondary[$index].reason)) "ExistingPlanningSecondaryReason:$planId"
    }
    if ([string] $item.classificationStateCode -eq 'Confirmed') {
        Require (-not [string]::IsNullOrWhiteSpace([string] $item.userConfirmationRef)) "ExistingPlanningConfirmation:$planId"
    } else {
        Require ([string]::IsNullOrWhiteSpace([string] $item.userConfirmationRef)) "ExistingPlanningUnexpectedConfirmation:$planId"
    }
}

$requirementByLine = @{}
foreach ($item in @($requirements.items)) {
    $lineId = [string] $item.hexagramLineStableId
    Require (-not $requirementByLine.ContainsKey($lineId)) "RequirementDuplicate:$lineId"
    Require ($lineId -in @($allLines.stableId)) "RequirementUnknownLine:$lineId"
    $requirementByLine[$lineId] = $item
}
$wiById = @{}
foreach ($item in @($worldInteractions.items)) { $wiById[[string] $item.id] = $item }

$treeHexagrams = @()
$allAnchors = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$hReferences = @{}
$wiReferences = @{}
foreach ($hexagram in $hexagrams) {
    $campaignIdentity = $campaignIdentityByHexagram[[string] $hexagram.stableId]
    $hexAnchor = ([string] $hexagram.stableId).ToLowerInvariant()
    Require ($allAnchors.Add($hexAnchor)) "AnchorDuplicate:$hexAnchor"
    $hexSeeds = @($seeds.seeds | Where-Object { [string] $_.primaryHexagramStableId -eq [string] $hexagram.stableId })
    $primaryPlanning = @($planningItems | Where-Object { [string] $_.primaryHexagramStableId -eq [string] $hexagram.stableId })
    $secondaryPlanning = @($planningItems | Where-Object { @($_.secondaryCandidates | ForEach-Object { [string] $_.hexagramStableId }) -contains [string] $hexagram.stableId })
    $treeLines = @()
    foreach ($line in @($hexagram.lineStories | Sort-Object ordinal)) {
        $lineAnchor = ([string] $line.stableId).ToLowerInvariant()
        Require ($allAnchors.Add($lineAnchor)) "AnchorDuplicate:$lineAnchor"
        $conventionalDocumentRef = "$($source.lineDocumentRootPath)/$($line.linePlanId)/README.md"
        $conventionalDocumentPath = Resolve-RepositoryPath $conventionalDocumentRef "LineDocument:$($line.stableId)" $false
        $documentExists = Test-Path -LiteralPath $conventionalDocumentPath -PathType Leaf
        $requirement = if ($requirementByLine.ContainsKey([string] $line.stableId)) { $requirementByLine[[string] $line.stableId] } else { $null }
        if ($null -ne $requirement -and -not [string]::IsNullOrWhiteSpace([string] $requirement.documentRef)) {
            Require ([string] $requirement.documentRef -eq $conventionalDocumentRef) "LineDocumentConvention:$($line.stableId)"
            Require $documentExists "RequiredLineDocumentMissing:$($line.stableId)"
        }

        $subjects = @()
        $wis = @()
        $hs = @()
        if ($null -ne $requirement) {
            $subjects = @($requirement.subjectRequirements | ForEach-Object {
                [ordered]@{ roleCode = [string] $_.roleCode; targetRef = [string] $_.targetRef; necessityCode = [string] $_.necessityCode; resolutionCode = [string] $_.resolutionCode }
            })
            $wis = @($requirement.worldInteractionRequirements | ForEach-Object {
                $targetRef = [string] $_.targetRef
                if (-not [string]::IsNullOrWhiteSpace($targetRef)) {
                    Require ($wiById.ContainsKey($targetRef)) "WorldInteractionUnknown:$targetRef"
                    $wiReferences[$targetRef] = $wiById[$targetRef]
                }
                [ordered]@{ roleCode = [string] $_.roleCode; targetRef = $targetRef; necessityCode = [string] $_.necessityCode; resolutionCode = [string] $_.resolutionCode }
            })
            $hs = @($requirement.hRequirements | ForEach-Object {
                $targetRef = [string] $_.targetRef
                $levelCode = [string] $_.levelCode
                Require ($levelCode -in @('H1','H2','H3','H4')) "HLevel:$($line.stableId)"
                if (-not [string]::IsNullOrWhiteSpace($targetRef)) {
                    $hKey = "$levelCode|$targetRef"
                    if (-not $hReferences.ContainsKey($hKey)) {
                        $hReferences[$hKey] = [ordered]@{ levelCode = $levelCode; targetRef = $targetRef; roles = @(); resolutionCodes = @(); compositionEvidenceRefs = @() }
                    }
                    $hReferences[$hKey].roles = @($hReferences[$hKey].roles + [string] $_.roleCode | Sort-Object -Unique)
                    $hReferences[$hKey].resolutionCodes = @($hReferences[$hKey].resolutionCodes + [string] $_.resolutionCode | Sort-Object -Unique)
                    $hReferences[$hKey].compositionEvidenceRefs = @($hReferences[$hKey].compositionEvidenceRefs + @($_.compositionEvidenceRefs) | Sort-Object -Unique)
                }
                [ordered]@{ levelCode = $levelCode; roleCode = [string] $_.roleCode; targetRef = $targetRef; necessityCode = [string] $_.necessityCode; resolutionCode = [string] $_.resolutionCode; compositionEvidenceRefs = @($_.compositionEvidenceRefs) }
            })
        }
        $lineSeeds = @($seeds.seeds | Where-Object { [string] $_.confirmedLineStableId -eq [string] $line.stableId })
        $treeLines += [ordered]@{
            ordinal = [int] $line.ordinal
            stableId = [string] $line.stableId
            anchor = $lineAnchor
            linePlanId = [string] $line.linePlanId
            traditionalName = [string] $line.traditionalName
            storyStatusCode = [string] $line.storyStatusCode
            documentStateCode = if ($documentExists) { 'Opened' } else { 'GeneratedDetail' }
            documentRef = if ($documentExists) { $conventionalDocumentRef } else { '' }
            planningStatusCode = if ($null -ne $requirement) { [string] $requirement.planningStatusCode } else { 'NotOpened' }
            requirementStateCode = if ($null -ne $requirement) { [string] $requirement.requirementStateCode } else { 'NotDeclared' }
            handoffStateCode = if ($null -ne $requirement) { [string] $requirement.handoffStateCode } else { 'NotEligible' }
            subjects = $subjects
            worldInteractions = $wis
            hRequirements = $hs
            graphMapImpactCode = if ($null -ne $requirement) { [string] $requirement.graphMapImpactCode } else { 'NotDeclared' }
            placementMapRequirementCode = if ($null -ne $requirement) { [string] $requirement.placementMapRequirementCode } else { 'NotDeclared' }
            storySeedStableIds = @($lineSeeds | ForEach-Object { [string] $_.storySeedStableId })
        }
    }
    $treeHexagrams += [ordered]@{
        ordinal = [int] $hexagram.ordinal
        stableId = [string] $hexagram.stableId
        anchor = $hexAnchor
        symbol = [string] $hexagram.symbol
        nameHanja = [string] $hexagram.nameHanja
        nameKorean = [string] $hexagram.nameKorean
        productionStatusCode = [string] $hexagram.productionStatusCode
        campaignIdentity = [ordered]@{
            campaignScaleCode = [string] $campaignIdentity.campaignScaleCode
            identityStatusCode = [string] $campaignIdentity.identityStatusCode
            coreSituation = [string] $campaignIdentity.coreSituation
            coreConflict = [string] $campaignIdentity.coreConflict
            subjectRelation = [string] $campaignIdentity.subjectRelation
            pressureModel = [string] $campaignIdentity.pressureModel
            signatureRuleCombination = [string] $campaignIdentity.signatureRuleCombination
            completionTransformation = [string] $campaignIdentity.completionTransformation
            combinationFingerprint = [string] $campaignIdentity.combinationFingerprint
        }
        storySeedStableIds = @($hexSeeds | ForEach-Object { [string] $_.storySeedStableId })
        primaryPlanningRefs = @($primaryPlanning | ForEach-Object { [ordered]@{ planId = [string] $_.planId; title = [string] $_.title; documentRef = [string] $_.documentRef; classificationStateCode = [string] $_.classificationStateCode } })
        secondaryPlanningRefs = @($secondaryPlanning | ForEach-Object { [ordered]@{ planId = [string] $_.planId; title = [string] $_.title; documentRef = [string] $_.documentRef; classificationStateCode = [string] $_.classificationStateCode } })
        lines = $treeLines
    }
}

$hIndexItems = @($hReferences.GetEnumerator() | Sort-Object Name | ForEach-Object { $_.Value } | ForEach-Object {
    [ordered]@{
        anchor = Get-ReferenceAnchor 'h' "$($_.levelCode)-$($_.targetRef)"
        levelCode = $_.levelCode
        targetRef = $_.targetRef
        roles = @($_.roles)
        resolutionCodes = @($_.resolutionCodes)
        compositionEvidenceRefs = @($_.compositionEvidenceRefs)
    }
})
$wiIndexItems = @($wiReferences.GetEnumerator() | Sort-Object Name | ForEach-Object {
    [ordered]@{ anchor = Get-ReferenceAnchor 'wi' ([string] $_.Key); id = [string] $_.Key; title = [string] $_.Value.title; kind = [string] $_.Value.kind; implementationStage = [string] $_.Value.implementation.currentStage; integrationStage = [string] $_.Value.integration.currentStage }
})

$result = [ordered]@{
    authorityCode = 'ReferenceOnlyNotStoryOrRuntimeOrder'
    currentPolicyRef = 'docs/Architecture/스토리영감과플레이진행분리.md'
    schemaVersion = 'mirror-hexagram-story-tree-index.v1'
    revision = [string] $source.revision
    productionRevision = [string] $production.revision
    campaignIdentityRevision = [string] $campaignIdentities.revision
    seedRevision = [string] $seeds.revision
    existingPlanningRevision = [string] $existingPlanning.revision
    lineRequirementRevision = [string] $requirements.revision
    policy = $policy
    counts = [ordered]@{ hexagrams = $treeHexagrams.Count; lines = @($treeHexagrams.lines).Count; openedLineDocuments = @($treeHexagrams.lines | Where-Object documentStateCode -eq 'Opened').Count; declaredLineRequirements = $requirementByLine.Count; classifiedExistingPlans = $planningItems.Count; confirmedExistingPlans = @($planningItems | Where-Object classificationStateCode -eq 'Confirmed').Count; hReferences = $hIndexItems.Count; worldInteractionReferences = $wiIndexItems.Count }
    hexagrams = $treeHexagrams
    worldInteractionReferences = $wiIndexItems
    hReferences = $hIndexItems
    existingPlanningClassifications = $planningItems
}

$jsonText = ConvertTo-DeterministicText (($result | ConvertTo-Json -Depth 30) + "`n")
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine('# 역경 64괘·384효 게임 기획 트리')
[void] $builder.AppendLine()
[void] $builder.AppendLine('> 괘·효는 영감과 원문 대조를 위한 참고 색인이다. 사건 수·제작 순서·플레이 진행은 이야기의 인과로 정한다. 아래 제작 상태는 이전 배정 이력이다. [현행 기준](../../Architecture/스토리영감과플레이진행분리.md). 이 문서는 자동 생성된다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 괘: **$($result.counts.hexagrams)** / 효: **$($result.counts.lines)** / 실제 열린 효사 문서: **$($result.counts.openedLineDocuments)** / 요구사항 선언 효: **$($result.counts.declaredLineRequirements)** / 기존 플레이 기획 분류: **$($result.counts.classifiedExistingPlans)**")
[void] $builder.AppendLine('- 물리 `README.md`는 실제로 연 효사에만 둔다. 미개방 효 링크는 이 문서 안의 안정 상세 앵커로 연결한다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 64괘 목차')
[void] $builder.AppendLine()
foreach ($hexagram in $treeHexagrams) {
    [void] $builder.AppendLine("- [$($hexagram.ordinal.ToString('00')). $($hexagram.symbol) $($hexagram.nameKorean) $($hexagram.nameHanja)](#$($hexagram.anchor))")
}
foreach ($hexagram in $treeHexagrams) {
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("<a id=`"$($hexagram.anchor)`"></a>")
    [void] $builder.AppendLine("## $($hexagram.ordinal.ToString('00')). $($hexagram.symbol) $($hexagram.nameKorean) $($hexagram.nameHanja)")
    [void] $builder.AppendLine()
    $seedText = if (@($hexagram.storySeedStableIds).Count -gt 0) { @($hexagram.storySeedStableIds | ForEach-Object { "``$_``" }) -join ', ' } else { '없음' }
    [void] $builder.AppendLine("- 제작 상태: ``$($hexagram.productionStatusCode)`` / 캠페인 규모: ``$($hexagram.campaignIdentity.campaignScaleCode)`` / 정체성: ``$($hexagram.campaignIdentity.identityStatusCode)`` / 괘 단위 기획 씨앗: $seedText")
    [void] $builder.AppendLine("- 핵심 상황·갈등: $(Escape-Cell $hexagram.campaignIdentity.coreSituation) / $(Escape-Cell $hexagram.campaignIdentity.coreConflict)")
    [void] $builder.AppendLine("- 주체·압박: $(Escape-Cell $hexagram.campaignIdentity.subjectRelation) / $(Escape-Cell $hexagram.campaignIdentity.pressureModel)")
    [void] $builder.AppendLine("- 고유 규칙 조합: $(Escape-Cell $hexagram.campaignIdentity.signatureRuleCombination) (``$($hexagram.campaignIdentity.combinationFingerprint)``)")
    [void] $builder.AppendLine("- 완주 변화: $(Escape-Cell $hexagram.campaignIdentity.completionTransformation)")
    $primaryPlanText = if (@($hexagram.primaryPlanningRefs).Count -gt 0) { @($hexagram.primaryPlanningRefs | ForEach-Object { "[$($_.title)]($(Get-GeneratedRelativeLink $_.documentRef)) (``$($_.classificationStateCode)``)" }) -join '; ' } else { '없음' }
    $secondaryPlanText = if (@($hexagram.secondaryPlanningRefs).Count -gt 0) { @($hexagram.secondaryPlanningRefs | ForEach-Object { "[$($_.title)]($(Get-GeneratedRelativeLink $_.documentRef))" }) -join '; ' } else { '없음' }
    [void] $builder.AppendLine("- 기존 기획 주괘 배치: $primaryPlanText")
    [void] $builder.AppendLine("- 기존 기획 보조 후보: $secondaryPlanText")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine('| 효 | 기획 ID | 문서 | 이야기 | 요구사항 | 인계 |')
    [void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- |')
    foreach ($line in $hexagram.lines) {
        $link = if ($line.documentStateCode -eq 'Opened') { Get-GeneratedRelativeLink $line.documentRef } else { "#$($line.anchor)" }
        [void] $builder.AppendLine("| [$($line.traditionalName)]($link) | ``$($line.linePlanId)`` | ``$($line.documentStateCode)`` | ``$($line.storyStatusCode)`` | ``$($line.requirementStateCode)`` | ``$($line.handoffStateCode)`` |")
    }
    [void] $builder.AppendLine()
    foreach ($line in $hexagram.lines) {
        [void] $builder.AppendLine()
        [void] $builder.AppendLine("<a id=`"$($line.anchor)`"></a>")
        [void] $builder.AppendLine("### $($line.traditionalName) · ``$($line.linePlanId)``")
        [void] $builder.AppendLine()
        if ($line.documentStateCode -eq 'Opened') { [void] $builder.AppendLine("- 기획 문서: [열기]($(Get-GeneratedRelativeLink $line.documentRef))") } else { [void] $builder.AppendLine('- 기획 문서: 미개방. 이 상세 앵커가 향후 문서 생성 전의 안정 링크다.') }
        $subjectText = if (@($line.subjects).Count -gt 0) { @($line.subjects | ForEach-Object { $target = if ([string]::IsNullOrWhiteSpace($_.targetRef)) { '미정' } else { $_.targetRef }; "``$($_.roleCode)`` → ``$target`` ($($_.resolutionCode))" }) -join '; ' } else { '미선언' }
        $wiText = if (@($line.worldInteractions).Count -gt 0) { @($line.worldInteractions | ForEach-Object { if ([string]::IsNullOrWhiteSpace($_.targetRef)) { "``$($_.roleCode)`` → 미정 ($($_.resolutionCode))" } else { "``$($_.roleCode)`` → [$($_.targetRef)](#$(Get-ReferenceAnchor 'wi' $_.targetRef)) ($($_.resolutionCode))" } }) -join '; ' } else { '미선언' }
        $hText = if (@($line.hRequirements).Count -gt 0) { @($line.hRequirements | ForEach-Object { if ([string]::IsNullOrWhiteSpace($_.targetRef)) { "``$($_.levelCode)`` ``$($_.roleCode)`` → 미정 ($($_.resolutionCode))" } else { "``$($_.roleCode)`` → [$($_.levelCode) $($_.targetRef)](hexagram-h-reference-index.md#$(Get-ReferenceAnchor 'h' "$($_.levelCode)-$($_.targetRef)"))" } }) -join '; ' } else { '미선언' }
        [void] $builder.AppendLine("- 주체: $subjectText")
        [void] $builder.AppendLine("- WI: $wiText")
        [void] $builder.AppendLine("- H: $hText")
        [void] $builder.AppendLine("- Graph Map: ``$($line.graphMapImpactCode)`` / 배치 맵: ``$($line.placementMapRequirementCode)``")
        [void] $builder.AppendLine("- 상태: 기획 ``$($line.planningStatusCode)`` / 요구사항 ``$($line.requirementStateCode)`` / 인계 ``$($line.handoffStateCode)``")
    }
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## WI 참조 색인')
[void] $builder.AppendLine()
[void] $builder.AppendLine('정본 전체 목록은 [세계 상호작용 구성 대장](world-interaction-catalog.md)에서 확인한다.')
foreach ($wi in $wiIndexItems) {
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("<a id=`"$($wi.anchor)`"></a>")
    [void] $builder.AppendLine("- ``$($wi.id)`` · $(Escape-Cell $wi.title) · ``$($wi.kind)`` · 구현 ``$($wi.implementationStage)`` / 통합 ``$($wi.integrationStage)``")
}
$markdownText = ConvertTo-DeterministicText $builder.ToString()

$hResult = [ordered]@{ schemaVersion = 'mirror-hexagram-h-reference-index.v1'; revision = [string] $source.revision; sourceLineRequirementRevision = [string] $requirements.revision; items = $hIndexItems }
$hJsonText = ConvertTo-DeterministicText (($hResult | ConvertTo-Json -Depth 20) + "`n")
$hBuilder = [Text.StringBuilder]::new()
[void] $hBuilder.AppendLine('# 역경 효사 기획 H 통합 참조 색인')
[void] $hBuilder.AppendLine()
[void] $hBuilder.AppendLine('> 효사 기획이 참조한 H1~H4를 한곳에서 찾기 위한 자동 생성 색인이다. 새 H를 만들거나 채택·배치·Evidence를 승격하지 않는다.')
[void] $hBuilder.AppendLine()
[void] $hBuilder.AppendLine('| 계층 | 대상 | 역할 | 해소 상태 | 조합 근거 |')
[void] $hBuilder.AppendLine('| --- | --- | --- | --- | --- |')
foreach ($h in $hIndexItems) {
    [void] $hBuilder.AppendLine("<a id=`"$($h.anchor)`"></a>")
    $roles = @($h.roles | ForEach-Object { "``$_``" }) -join ', '
    $states = @($h.resolutionCodes | ForEach-Object { "``$_``" }) -join ', '
    $evidence = if (@($h.compositionEvidenceRefs).Count -gt 0) { @($h.compositionEvidenceRefs | ForEach-Object { "``$_``" }) -join '<br>' } else { '없음' }
    [void] $hBuilder.AppendLine("| ``$($h.levelCode)`` | ``$($h.targetRef)`` | $roles | $states | $evidence |")
}
[void] $hBuilder.AppendLine()
[void] $hBuilder.AppendLine('- 후보·표현 배당 참고: [H1 Synty 표현 배당](h1-synty-representation-assignments.md)')
$hMarkdownText = ConvertTo-DeterministicText $hBuilder.ToString()

$outputs = @(
    @{ Path = Resolve-RepositoryPath ([string] $source.outputJsonPath) 'JsonOutput' $false; Content = $jsonText },
    @{ Path = Resolve-RepositoryPath ([string] $source.outputMarkdownPath) 'MarkdownOutput' $false; Content = $markdownText },
    @{ Path = Resolve-RepositoryPath ([string] $source.hReferenceJsonPath) 'HJsonOutput' $false; Content = $hJsonText },
    @{ Path = Resolve-RepositoryPath ([string] $source.hReferenceMarkdownPath) 'HMarkdownOutput' $false; Content = $hMarkdownText }
)
if ($Mode -eq 'Write') {
    foreach ($output in $outputs) { [void] (Write-DeterministicTextIfChanged -Path $output.Path -Content $output.Content) }
} elseif ($Mode -eq 'Check') {
    foreach ($output in $outputs) {
        Require (Test-Path -LiteralPath $output.Path -PathType Leaf) "OutputMissing:$($output.Path)"
        Require ((Get-Content -LiteralPath $output.Path -Raw -Encoding UTF8) -ceq $output.Content) "OutputStale:$($output.Path)"
    }
}

Write-Output "HexagramStoryTree:$Mode`:OK:Hexagrams=$($result.counts.hexagrams):Lines=$($result.counts.lines):Opened=$($result.counts.openedLineDocuments):Requirements=$($result.counts.declaredLineRequirements)"
