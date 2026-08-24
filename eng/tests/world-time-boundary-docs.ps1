$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$standardPath = Join-Path $repoRoot 'docs/Architecture/WorldTick과실시간실행경계.md'
$simulationMapPath = Join-Path $repoRoot 'docs/AI/authority-maps/05_SIMULATION_DOMAIN_MAP.md'
$unityMapPath = Join-Path $repoRoot 'docs/AI/authority-maps/06_UNITY_CURRENT_STRUCTURE.md'
$readmePath = Join-Path $repoRoot 'README.md'
$docsReadmePath = Join-Path $repoRoot 'docs/README.md'

foreach ($path in @($standardPath, $simulationMapPath, $unityMapPath,
        $readmePath, $docsReadmePath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "WorldTimeBoundaryDocumentMissing:$path"
    }
}

function Require-Text {
    param([string] $Content, [string] $Expected, [string] $ErrorCode)
    if (-not $Content.Contains($Expected)) { throw $ErrorCode }
}

$standard = Get-Content -LiteralPath $standardPath -Raw -Encoding utf8
$simulationMap = Get-Content -LiteralPath $simulationMapPath -Raw -Encoding utf8
$unityMap = Get-Content -LiteralPath $unityMapPath -Raw -Encoding utf8
$readme = Get-Content -LiteralPath $readmePath -Raw -Encoding utf8
$docsReadme = Get-Content -LiteralPath $docsReadmePath -Raw -Encoding utf8

foreach ($expected in @(
    'WorldTick과 WorldRevision은 다르다',
    'Unity 표현 시간',
    '권위 실시간 시계',
    'OneTickOneDay',
    'ElapsedRealtimeSeconds',
    '1,200초',
    'BattleTick',
    'CombatStepMilliseconds = 100',
    'simulation-save.v13',
    '실시간 통신'
)) {
    Require-Text $standard $expected "WorldTimeBoundaryMissing:$expected"
}

Require-Text $simulationMap 'WorldTick과 실시간 실행 경계' `
    'SimulationDomainMapWorldTimeLinkMissing'
Require-Text $simulationMap 'Nature 기능 한정 `SoloLocal`' `
    'SimulationDomainMapSoloLocalBoundaryMissing'
Require-Text $unityMap '표현 실시간' 'UnityMapPresentationRealtimeMissing'
Require-Text $unityMap '권위 실시간' 'UnityMapAuthoritativeRealtimeMissing'
Require-Text $readme 'docs/Architecture/WorldTick과실시간실행경계.md' `
    'RootReadmeWorldTimeLinkMissing'
Require-Text $docsReadme 'Architecture/WorldTick과실시간실행경계.md' `
    'DocsReadmeWorldTimeLinkMissing'

Write-Output 'WorldTimeBoundaryDocsPassed:Frame-Realtime-WorldTick-BattleTick-Revision'
