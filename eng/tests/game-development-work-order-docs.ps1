$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$standardPath = Join-Path $repoRoot 'docs/Architecture/게임개발업무순서기준.md'
$templatePath = Join-Path $repoRoot 'docs/ProjectOverview/templates/게임개발작업단위템플릿.md'
$readmePath = Join-Path $repoRoot 'README.md'
$docsReadmePath = Join-Path $repoRoot 'docs/README.md'

foreach ($path in @($standardPath, $templatePath, $readmePath, $docsReadmePath)) {
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

foreach ($expected in @(
    '현재 목표 확인',
    '플레이 단위 선택',
    'WI·권위 계약 확인',
    'Simulation 세로 조각',
    'H 공간 결속',
    'Unity 조립',
    '필요한 실제 증거 확인',
    'E9 목표 봉투',
    'E9→E1',
    'E1→E9',
    '다시 E9→E1',
    '안정 또는 명시적 차단까지 왕복',
    'E9하향식수직구현체계.md',
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
    'E9 우선 수직 작업 명세'
)) {
    Require-Text $template $expected "GameDevelopmentWorkTemplateMissing:$expected"
}

Require-Text $readme '게임 개발 업무 순서' 'RootReadmeGameWorkOrderSectionMissing'
Require-Text $readme 'docs/Architecture/게임개발업무순서기준.md' 'RootReadmeGameWorkOrderLinkMissing'
Require-Text $readme 'docs/Architecture/E9하향식수직구현체계.md' 'RootReadmeE9VerticalLinkMissing'
Require-Text $docsReadme '게임 개발 업무 순서' 'DocsReadmeGameWorkOrderEntryMissing'
Require-Text $docsReadme 'Architecture/E9하향식수직구현체계.md' 'DocsReadmeE9VerticalLinkMissing'

Write-Output 'GameDevelopmentWorkOrderDocsPassed:Cycle=Goal-E9Down-E1Up-Repeat-UntilStable'
