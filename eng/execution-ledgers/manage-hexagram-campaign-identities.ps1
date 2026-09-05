[CmdletBinding()]
param(
    [ValidateSet('Write', 'Check', 'Validate')]
    [string] $Mode = 'Check',
    [string] $InputPath = 'eng/execution-ledgers/hexagram-campaign-identities.json',
    [string] $JsonOutputPath = 'docs/AI/generated/hexagram-campaign-identities.json',
    [string] $MarkdownOutputPath = 'docs/AI/generated/hexagram-campaign-identities.md'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot '../common/deterministic-text-output.ps1')

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramCampaignIdentityInvalid:$Code" }
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

$resolvedInput = Resolve-InputFile $InputPath 'Input'
$source = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $source.schemaVersion -eq 'mirror-hexagram-campaign-identities.v1') 'SchemaVersion'
Require ([string] $source.revision -eq 'hexagram-campaign-identities.r1') 'Revision'

$productionPath = Resolve-InputFile ([string] $source.parentProductionLedgerPath) 'ProductionLedger'
$production = Get-Content -LiteralPath $productionPath -Raw -Encoding UTF8 | ConvertFrom-Json
Require ([string] $production.schemaVersion -eq 'mirror-hexagram-story-production.v3') 'ProductionSchemaVersion'
Require ([string] $production.revision -eq 'hexagram-story-production.r14') 'ProductionRevision'
Require ([string] $production.policy.campaignIdentityLedgerPath -eq [string] $source.parentProductionLedgerPath.Replace('hexagram-story-production.json', 'hexagram-campaign-identities.json')) 'ProductionIdentityLink'

$policy = $source.policy
Require ([string] $policy.authoringOrderCode -eq 'DraftAll64ThenAuthorLinesInKingWenOrder') 'AuthoringOrder'
Require ([string] $policy.identityContractCode -eq 'CoreSituationConflictSubjectPressureJudgementCompletion') 'IdentityContract'
Require ([string] $policy.distinctivenessCode -eq 'UniqueConflictSubjectPressureJudgementCombination') 'Distinctiveness'
Require ([string] $policy.mechanicReuseCode -eq 'ReuseWorldInteractionsAndSystemsWithUniqueCombination') 'MechanicReuse'
Require ((@($policy.prologueHexagramStableIds) -join ',') -eq 'HEX-01-QIAN,HEX-02-KUN') 'PrologueHexagrams'
Require ([string] $policy.fullCampaignStartsAtHexagramStableId -eq 'HEX-03-ZHUN') 'FullCampaignStart'
Require ([bool] $policy.doesNotApproveLineStories) 'LineApprovalBoundary'
Require ([bool] $policy.doesNotCreateWorldInteractions) 'WorldInteractionBoundary'
Require ([bool] $policy.doesNotPromoteEvidence) 'EvidenceBoundary'

$allowedScaleCodes = @($source.allowedCodes.campaignScaleCodes | ForEach-Object { [string] $_ })
$allowedStatusCodes = @($source.allowedCodes.identityStatusCodes | ForEach-Object { [string] $_ })
$campaigns = @($source.campaigns)
$hexagrams = @($production.hexagrams | Sort-Object ordinal)
Require ($campaigns.Count -eq 64) "CampaignCount:$($campaigns.Count)"
Require (@($campaigns.hexagramStableId | Sort-Object -Unique).Count -eq 64) 'CampaignStableIdDuplicate'

$fingerprints = [Collections.Generic.List[string]]::new()
$combinations = [Collections.Generic.List[string]]::new()
for ($index = 0; $index -lt 64; $index++) {
    $campaign = $campaigns[$index]
    $hexagram = $hexagrams[$index]
    $id = [string] $campaign.hexagramStableId
    Require ($id -eq [string] $hexagram.stableId) "CampaignOrder:$id"
    Require ([string] $campaign.campaignScaleCode -in $allowedScaleCodes) "CampaignScale:$id"
    Require ([string] $campaign.identityStatusCode -in $allowedStatusCodes) "IdentityStatus:$id"
    $expectedScale = if ($index -lt 2) { 'ShortPlayablePrologue' } else { 'FullCampaign' }
    Require ([string] $campaign.campaignScaleCode -eq $expectedScale) "ExpectedScale:$id"
    foreach ($field in @('coreSituation', 'coreConflict', 'subjectRelation', 'pressureModel', 'signatureRuleCombination', 'completionTransformation', 'combinationFingerprint')) {
        Require-Text $campaign.$field "$field`:$id"
    }
    $fingerprint = [string] $campaign.combinationFingerprint
    Require ($fingerprint -match '^[A-Z0-9_]+$') "FingerprintFormat:$id"
    [void] $fingerprints.Add($fingerprint)
    [void] $combinations.Add((@(
        [string] $campaign.coreConflict,
        [string] $campaign.subjectRelation,
        [string] $campaign.pressureModel,
        [string] $campaign.signatureRuleCombination
    ) -join '|'))
}
Require (@($fingerprints | Sort-Object -Unique).Count -eq 64) 'FingerprintDuplicate'
Require (@($combinations | Sort-Object -Unique).Count -eq 64) 'IdentityCombinationDuplicate'

$rows = for ($index = 0; $index -lt 64; $index++) {
    $campaign = $campaigns[$index]
    $hexagram = $hexagrams[$index]
    [ordered]@{
        ordinal = [int] $hexagram.ordinal
        symbol = [string] $hexagram.symbol
        nameHanja = [string] $hexagram.nameHanja
        nameKorean = [string] $hexagram.nameKorean
        hexagramStableId = [string] $campaign.hexagramStableId
        campaignScaleCode = [string] $campaign.campaignScaleCode
        identityStatusCode = [string] $campaign.identityStatusCode
        coreSituation = [string] $campaign.coreSituation
        coreConflict = [string] $campaign.coreConflict
        subjectRelation = [string] $campaign.subjectRelation
        pressureModel = [string] $campaign.pressureModel
        signatureRuleCombination = [string] $campaign.signatureRuleCombination
        completionTransformation = [string] $campaign.completionTransformation
        combinationFingerprint = [string] $campaign.combinationFingerprint
    }
}

$result = [ordered]@{
    schemaVersion = 'mirror-hexagram-campaign-identities-index.v1'
    revision = [string] $source.revision
    parentProductionRevision = [string] $production.revision
    sourceSnapshots = @(
        [ordered]@{ path = $InputPath; sha256 = (Get-FileHash -LiteralPath $resolvedInput -Algorithm SHA256).Hash.ToUpperInvariant() },
        [ordered]@{ path = [string] $source.parentProductionLedgerPath; sha256 = (Get-FileHash -LiteralPath $productionPath -Algorithm SHA256).Hash.ToUpperInvariant() }
    )
    policy = $policy
    counts = [ordered]@{
        campaigns = $rows.Count
        prologues = @($rows | Where-Object campaignScaleCode -eq 'ShortPlayablePrologue').Count
        fullCampaigns = @($rows | Where-Object campaignScaleCode -eq 'FullCampaign').Count
        reviewed = @($rows | Where-Object identityStatusCode -eq 'Reviewed').Count
        seeded = @($rows | Where-Object identityStatusCode -eq 'Seeded').Count
    }
    campaigns = @($rows)
}

$jsonText = ConvertTo-DeterministicText (($result | ConvertTo-Json -Depth 20) + "`n")
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine('# 역경 64괘 캠페인 정체성 대장')
[void] $builder.AppendLine()
[void] $builder.AppendLine('> 이 문서는 `hexagram-campaign-identities.json`에서 자동 생성된다. 각 항목은 육효 상세 기획 전의 한 줄 기준이며 효 이야기나 개발을 자동 승인하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 제작 순서: ``$($policy.authoringOrderCode)``")
[void] $builder.AppendLine("- 차별화: ``$($policy.distinctivenessCode)``")
[void] $builder.AppendLine("- 재사용: ``$($policy.mechanicReuseCode)``")
[void] $builder.AppendLine("- 전체: ``$($result.counts.campaigns)`` / 짧은 실제 플레이 서막: ``$($result.counts.prologues)`` / 본격 캠페인: ``$($result.counts.fullCampaigns)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 순번 | 괘 | 규모·상태 | 핵심 상황·갈등 | 주체·압박 | 고유 규칙 조합 | 완주 변화 |')
[void] $builder.AppendLine('| ---: | --- | --- | --- | --- | --- | --- |')
foreach ($row in $rows) {
    [void] $builder.AppendLine("| $($row.ordinal) | $($row.symbol) $($row.nameHanja) · $(Escape-Cell $row.nameKorean)<br>``$($row.hexagramStableId)`` | ``$($row.campaignScaleCode)``<br>``$($row.identityStatusCode)`` | $(Escape-Cell $row.coreSituation)<br>$(Escape-Cell $row.coreConflict) | $(Escape-Cell $row.subjectRelation)<br>압박: $(Escape-Cell $row.pressureModel) | $(Escape-Cell $row.signatureRuleCombination)<br>``$($row.combinationFingerprint)`` | $(Escape-Cell $row.completionTransformation) |")
}
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

Write-Output "HexagramCampaignIdentities:$Mode`:OK:Campaigns=$($result.counts.campaigns):Prologues=$($result.counts.prologues):Full=$($result.counts.fullCampaigns)"
