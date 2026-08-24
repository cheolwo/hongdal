$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$ledgerPath = Join-Path $repositoryRoot 'eng/execution-ledgers/player-centered-development-flow.json'
$standardPath = Join-Path $repositoryRoot 'docs/Architecture/플레이어중심게임개발업무구조.md'
$workOrderPath = Join-Path $repositoryRoot 'docs/Architecture/게임개발업무순서기준.md'
$templatePath = Join-Path $repositoryRoot 'docs/ProjectOverview/templates/게임개발작업단위템플릿.md'
$readmePath = Join-Path $repositoryRoot 'README.md'

foreach ($path in @($ledgerPath, $standardPath, $workOrderPath, $templatePath, $readmePath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "PlayerCenteredDevelopmentDocumentMissing:$path"
    }
}

$ledger = Get-Content -LiteralPath $ledgerPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string] $ledger.schemaVersion -ne 'player-centered-development-flow.v1') {
    throw 'PlayerCenteredDevelopmentSchemaInvalid'
}
if (-not [bool] $ledger.authority.playerCenteredDoesNotGrantUnityStateAuthority -or
    [string] $ledger.authority.authoritativeStateOwner -ne 'SharedSimulationCore') {
    throw 'PlayerCenteredDevelopmentAuthorityBoundaryInvalid'
}

$expectedLoop = @(
    'ObserveSituation',
    'UnderstandGoal',
    'CompareChoices',
    'GatherAndAllocate',
    'PreviewAndConfirm',
    'PerformAndWait',
    'ReadWorldResult',
    'RecoverReturnOrChooseAgain'
)
if ((@($ledger.playerLoop.code) -join ',') -ne ($expectedLoop -join ',')) {
    throw 'PlayerCenteredDevelopmentLoopOrderInvalid'
}
if ((@($ledger.evidenceStageReview.stage) -join ',') -ne 'E1,E2,E3,E4,E5,E6,E7,E8,E9') {
    throw 'PlayerCenteredDevelopmentEvidenceReviewInvalid'
}
if ((@($ledger.spatialComposition.directPlayerCompositionLevels) -join ',') -ne 'H1' -or
    (@($ledger.spatialComposition.playerGoalDirectedGrowthLevels) -join ',') -ne 'H2,H3' -or
    (@($ledger.spatialComposition.worldAuthoredContractLevels) -join ',') -ne 'H4,H5') {
    throw 'PlayerCenteredDevelopmentSpatialBoundaryInvalid'
}
if ((@($ledger.spatialComposition.formationModes) -join ',') -ne
    'LhComposed,PlayerComposed,HybridEvolving') {
    throw 'PlayerCenteredDevelopmentFormationModesInvalid'
}

$expectedResourceSemantics = @(
    'WorldResourceOrLot',
    'ConstructionRecipeAndCost',
    'PlacementPreview',
    'H1SpatialInstance',
    'H2H3GrowthAssessment',
    'UnityVisualAsset'
)
if ((@($ledger.resourceSemantics) -join ',') -ne ($expectedResourceSemantics -join ',')) {
    throw 'PlayerCenteredDevelopmentResourceSemanticsInvalid'
}

foreach ($relativePath in @($ledger.sourceDocuments) + @($ledger.sourceLedgers)) {
    if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot ([string] $relativePath)))) {
        throw "PlayerCenteredDevelopmentSourceMissing:$relativePath"
    }
}

function Require-Text {
    param([string] $Content, [string] $Expected, [string] $ErrorCode)
    if (-not $Content.Contains($Expected)) { throw $ErrorCode }
}

$standard = Get-Content -LiteralPath $standardPath -Raw -Encoding UTF8
$workOrder = Get-Content -LiteralPath $workOrderPath -Raw -Encoding UTF8
$template = Get-Content -LiteralPath $templatePath -Raw -Encoding UTF8
$readme = Get-Content -LiteralPath $readmePath -Raw -Encoding UTF8

foreach ($expected in @(
    '플레이어 선택 폐루프',
    'E1~E9를 플레이어 관점에서 검토한다',
    'H1~H5와 플레이어 조립',
    '재료·조립·표현을 분리한다',
    '플레이어 중심은 상태 권위를 플레이어 또는 Unity'
)) {
    Require-Text $standard $expected "PlayerCenteredDevelopmentStandardMissing:$expected"
}

foreach ($expected in @('플레이어 약속', '플레이어 선택 폐루프', '재료·조립 계획')) {
    Require-Text $template $expected "PlayerCenteredDevelopmentTemplateMissing:$expected"
}
Require-Text $workOrder '플레이어중심게임개발업무구조.md' 'PlayerCenteredDevelopmentWorkOrderLinkMissing'
Require-Text $readme 'docs/Architecture/플레이어중심게임개발업무구조.md' 'PlayerCenteredDevelopmentReadmeLinkMissing'

Write-Output 'PlayerCenteredDevelopmentFlowPassed:PlayerLoop-E1E9-H1H5-Resources-Authority'
