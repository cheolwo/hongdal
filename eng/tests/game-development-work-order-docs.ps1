$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$standardPath = Join-Path $repoRoot 'docs/Architecture/게임개발업무순서기준.md'
$templatePath = Join-Path $repoRoot 'docs/ProjectOverview/templates/게임개발작업단위템플릿.md'
$readmePath = Join-Path $repoRoot 'README.md'
$docsReadmePath = Join-Path $repoRoot 'docs/README.md'
$agentsPath = Join-Path $repoRoot 'AGENTS.md'
$loopEvidenceStandardPath = Join-Path $repoRoot `
    'docs/Architecture/플레이폐루프와증거묶음개발체계.md'
$loopMethodPath = Join-Path $repoRoot `
    'docs/Architecture/플레이폐루프논리시각이중순환체계.md'
$currentEvidenceModelPath = Join-Path $repoRoot `
    'docs/Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md'
$legacyE9Path = Join-Path $repoRoot `
    'docs/Architecture/E9하향식수직구현체계.md'
$e7ProtocolPath = Join-Path $repoRoot `
    'eng/execution-ledgers/e7-vertical-implementation-protocol.json'
$postE7CampaignPath = Join-Path $repoRoot `
    'eng/execution-ledgers/post-e7-evidence-campaigns.json'

foreach ($path in @($standardPath, $templatePath, $readmePath, $docsReadmePath,
    $agentsPath, $loopEvidenceStandardPath, $loopMethodPath,
    $currentEvidenceModelPath, $legacyE9Path, $e7ProtocolPath,
    $postE7CampaignPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "GameDevelopmentWorkOrderDocumentMissing:$path"
    }
}

function Require-Text {
    param([string] $Content, [string] $Expected, [string] $ErrorCode)
    if (-not $Content.Contains($Expected)) { throw $ErrorCode }
}

$standard = Get-Content -LiteralPath $standardPath -Raw -Encoding utf8
$template = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8
$readme = Get-Content -LiteralPath $readmePath -Raw -Encoding utf8
$docsReadme = Get-Content -LiteralPath $docsReadmePath -Raw -Encoding utf8
$agents = Get-Content -LiteralPath $agentsPath -Raw -Encoding utf8
$loopEvidenceStandard = Get-Content -LiteralPath $loopEvidenceStandardPath -Raw `
    -Encoding utf8
$loopMethod = Get-Content -LiteralPath $loopMethodPath -Raw -Encoding utf8

foreach ($expected in @(
    '현재 목표 확인',
    '플레이 단위 선택',
    'WI·권위 계약 확인',
    'Simulation 세로 조각',
    'H 공간 결속',
    'Unity 조립',
    '필요한 실제 증거 확인',
    'E7 플레이 약속',
    'E7→E1',
    'E1→E7',
    '다시 E7→E1',
    '안정 또는 명시적 차단까지 왕복',
    'e7-vertical-implementation-protocol.json',
    '별도 E8~E10 캠페인',
    '다음 작업 선택 우선순위',
    '현재 목표의 직접 차단 항목',
    'Farm·Hub·Town·City는 각각 내부 플레이',
    'Farm→Hub→Town은 존재 가능한 통합 흐름이지만 자동 다음 순서는 아니다.',
    'SimulationWorldShell',
    '코드 존재',
    '≠ Play Mode·Game View'
)) {
    Require-Text $standard $expected "GameDevelopmentWorkOrderMissing:$expected"
}

foreach ($expected in @(
    '현재 순서에서 이 작업을 선택한 이유',
    '플레이 흐름',
    '관련 WI',
    '관련 공간',
    '상태 권위',
    '목표 증거',
    '포함',
    '제외',
    '완료 판정',
    '작업 후 다음 판단',
    'E7 우선 수직 작업 명세',
    'PlayableLoop 등록',
    'EvidencePackage',
    '협업과 인계',
    '논리·표현 현재 판정',
    '결과·귀환 관문',
    '실제 입력 검증 명령열',
    '피드백 재개 기록',
    'activeMaturityTrackCode',
    'openFeedbackItems',
    'E4→E5 표현 준비 인계',
    'E5 준비 상태'
)) {
    Require-Text $template $expected "GameDevelopmentWorkTemplateMissing:$expected"
}

Require-Text $readme '게임 개발 업무 순서' 'RootReadmeGameWorkOrderSectionMissing'
Require-Text $readme 'docs/Architecture/게임개발업무순서기준.md' 'RootReadmeGameWorkOrderLinkMissing'
Require-Text $readme 'docs/Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md' `
    'RootReadmeCurrentEvidenceModelLinkMissing'
Require-Text $docsReadme '게임 개발 업무 순서' 'DocsReadmeGameWorkOrderEntryMissing'
Require-Text $docsReadme 'Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md' `
    'DocsReadmeCurrentEvidenceModelLinkMissing'
Require-Text $readme 'docs/Architecture/플레이폐루프와증거묶음개발체계.md' `
    'RootReadmePlayableLoopEvidenceLinkMissing'
Require-Text $docsReadme 'Architecture/플레이폐루프와증거묶음개발체계.md' `
    'DocsReadmePlayableLoopEvidenceLinkMissing'
Require-Text $docsReadme 'Architecture/플레이폐루프논리시각이중순환체계.md' `
    'DocsReadmePlayableLoopMethodLinkMissing'
Require-Text $agents 'docs/Architecture/플레이폐루프논리시각이중순환체계.md' `
    'AgentsPlayableLoopMethodLinkMissing'
foreach ($expected in @(
    'PlayableLoop',
    'EvidencePackage',
    '자동 완료 원장',
    'primaryWorkstream',
    'invalidationTriggers')) {
    Require-Text $loopEvidenceStandard $expected `
        "PlayableLoopEvidenceStandardMissing:$expected"
}

foreach ($expected in @(
    '폐루프 정의',
    '논리 순환',
    '표현 순환',
    '결과·귀환 관문',
    '피드백 재개',
    '원인별 재개 판정표',
    'Codex 착수와 종료 인계',
    '표현 검증 모듈 관문',
    'surface-clearance',
    'manage-playable-loop-presentation-validation.ps1',
    '세 번째 성숙도 궤적이 아니다',
    '통합성숙도 = min(논리성숙도, 표현성숙도)',
    'activeMaturityTrackCode',
    'invalidationTriggers',
    '자산 후보 조사는 E4의 준비 책임',
    'E5 준비 상태')) {
    Require-Text $loopMethod $expected `
        "PlayableLoopMethodMissing:$expected"
}
foreach ($expected in @(
    '표현 검증 모듈 선택',
    'presentation-binding',
    'visual-source-bounds',
    'actual-camera-input-result-return',
    'openFeedbackItems')) {
    Require-Text $template $expected `
        "PresentationValidationTemplateMissing:$expected"
}
Require-Text $agents 'playable-loop-presentation-validation-modules.json' `
    'AgentsPresentationValidationCatalogMissing'
Require-Text $agents 'presentationE4Preparation' `
    'AgentsPresentationE4PreparationMissing'
Require-Text $docsReadme 'AI/generated/playable-loop-presentation-validation.md' `
    'DocsReadmePresentationValidationStatusMissing'

$currentEvidenceModel = Get-Content -LiteralPath $currentEvidenceModelPath -Raw `
    -Encoding utf8
foreach ($expected in @(
    'horizontal-dual-cycle-evidence.r3',
    'Logic:       E7 → ... → E1 영향 검토 / E1 → ... → E7 조립·검증',
    'Presentation:E7 → ... → E1 영향 검토 / E1 → ... → E7 조립·검증',
    'E8 — 개별 플레이 폐루프 안정',
    'E9 — 영역 폐루프 조화·사람 승인',
    'E10 — 제한 운영 검증',
    'WindowsX64 + LocalProcess',
    'legacy-change-adaptive.r10')) {
    Require-Text $currentEvidenceModel $expected `
        "CurrentEvidenceModelMissing:$expected"
}
$legacyE9 = Get-Content -LiteralPath $legacyE9Path -Raw -Encoding utf8
Require-Text $legacyE9 '호환 문서' 'LegacyE9CompatibilityNoticeMissing'
Require-Text $legacyE9 '현재 E9 영역 조화·사람 승인 증거가 아니다' `
    'LegacyE9CurrentMeaningBoundaryMissing'

Write-Output 'GameDevelopmentWorkOrderDocsPassed:E1-E7-PostE7-Compatibility-Loop-Handoff'
