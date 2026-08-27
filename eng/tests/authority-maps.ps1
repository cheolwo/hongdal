$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$mapRoot = Join-Path $repoRoot 'docs/AI/authority-maps'
$expectedFiles = @(
    '00_GAME_DESIGN_TREE.md',
    '01_E1_E9_DEFINITION.md',
    '02_G1_G4_MANAGEMENT.md',
    '03_H1_H5_CURRENT_TREE.md',
    '04_WI_GAMEPLAY_GRAPH.md',
    '05_SIMULATION_DOMAIN_MAP.md',
    '06_UNITY_CURRENT_STRUCTURE.md',
    '07_CURRENT_COMPLETION_LEDGER.md',
    '08_CURRENT_WORK.md',
    '09_DECISIONS.md'
)

foreach ($fileName in $expectedFiles) {
    $path = Join-Path $mapRoot $fileName
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Authority map missing: $fileName"
    }
}

function Assert-Contains {
    param(
        [string] $Path,
        [string[]] $Needles
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding utf8
    foreach ($needle in $Needles) {
        if (-not $content.Contains($needle)) {
            throw "Missing '$needle' in $Path"
        }
    }
}

Assert-Contains (Join-Path $mapRoot '00_GAME_DESIGN_TREE.md') @(
    'Nature↔Farm',
    '플레이 목적 → PlayableLoop → WI → H → E → G → EvidencePackage',
    '독립 내부 폐루프'
)

$eDefinitionPath = Join-Path $mapRoot '01_E1_E9_DEFINITION.md'
foreach ($stage in 1..9) {
    Assert-Contains $eDefinitionPath @("E$stage")
}
Assert-Contains $eDefinitionPath @('WI 실행 문맥 결속', 'WI 세계 발현')

Assert-Contains (Join-Path $mapRoot '02_G1_G4_MANAGEMENT.md') @('G1', 'G2', 'G3', 'G4')
Assert-Contains (Join-Path $mapRoot '03_H1_H5_CURRENT_TREE.md') @(
    '설계 지식 재고',
    '실행용 공간 자원 지도',
    'Nature·Farm 기준 플레이 추적 부분집합',
    '공간 조립 호환'
)

$wiLedgerPath = Join-Path $repoRoot 'eng/execution-ledgers/world-interactions.json'
$wiLedger = Get-Content -LiteralPath $wiLedgerPath -Raw -Encoding utf8 | ConvertFrom-Json
$wiMap = Get-Content -LiteralPath (Join-Path $mapRoot '04_WI_GAMEPLAY_GRAPH.md') -Raw -Encoding utf8
$wiIds = @($wiLedger.items | ForEach-Object { $_.id })
if ($wiIds.Count -ne 65) {
    throw "Expected 65 WI ledger entries, found $($wiIds.Count). Update the authority map and this gate together."
}
foreach ($wiId in $wiIds) {
    if (-not $wiMap.Contains($wiId)) {
        throw "WI missing from gameplay graph: $wiId"
    }
}

Assert-Contains (Join-Path $mapRoot '05_SIMULATION_DOMAIN_MAP.md') @(
    '경영SimulationSessionAggregate',
    'Decision',
    'Task',
    'Effect',
    'WorldTick',
    'Save / Replay'
)
Assert-Contains (Join-Path $mapRoot '06_UNITY_CURRENT_STRUCTURE.md') @(
    'SimulationWorldShell',
    'ReviewFixture',
    'SessionMode',
    'Play Mode·Game View',
    '실시간 재확인하지 않았다'
)
Assert-Contains (Join-Path $mapRoot '07_CURRENT_COMPLETION_LEDGER.md') @(
    '판정 기준',
    '전체 상태',
    '영역 집계와 자식 상태',
    '현재 닫힌 E5 단위',
    '열린 경계',
    '보류 또는 독립 준비 후 통합',
    '현재 증거 묶음',
    'playable-loop:nature-survival-homestead.v1'
)
Assert-Contains (Join-Path $mapRoot '08_CURRENT_WORK.md') @('../CURRENT_WORK.md')
Assert-Contains (Join-Path $mapRoot '09_DECISIONS.md') @('../DECISIONS.md')

Write-Host "Authority maps validated: $($expectedFiles.Count) files, $($wiIds.Count) WI entries."
