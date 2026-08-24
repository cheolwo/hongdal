$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$documentPath = Join-Path $repoRoot 'docs/Architecture/Synty5팩자산-H1-H3연결지도.md'
if (-not (Test-Path -LiteralPath $documentPath)) {
    throw 'SyntyPackHDocumentationMissing'
}

$content = Get-Content -LiteralPath $documentPath -Raw -Encoding utf8

function Require-Text {
    param([string] $Value, [string] $ErrorCode)
    if (-not $content.Contains($Value)) {
        throw $ErrorCode
    }
}

$packCounts = [ordered]@{
    Nature = 227
    Farm = 498
    Town = 702
    City = 335
    Construction = 584
}

foreach ($entry in $packCounts.GetEnumerator()) {
    Require-Text "| $($entry.Key) | $($entry.Value) |" "SyntyPackCountMissing:$($entry.Key)"
}

if (($packCounts.Values | Measure-Object -Sum).Sum -ne 2346) {
    throw 'SyntyPackTotalInvalid'
}

foreach ($requiredText in @(
    '1,499개 의미 자산군',
    '131개 의미 자산군',
    '363개 의미 자산군',
    '435개 의미 자산군',
    '221개 의미 자산군',
    '349개 의미 자산군',
    'Prefab 하나가 H1인 것은 아니다.',
    'h1-stock:nature-trailhead',
    'h2-candidate:nature-home-core',
    'h3-candidate:nature-home-encounter-defense',
    'h1-stock:farm-production',
    'h3-candidate:highland-farm',
    'h1-stock:town-market-receiving',
    'h3-candidate:town-market-fulfillment',
    'h1-stock:hub-receiving-storage',
    'h3-candidate:hub-fulfillment-operations',
    'Construction은 `h1-expression:construction:*`, 독립 H2 또는 독립 H3를 만들지 않는다.',
    '모두 `PresentationOnly`'
)) {
    Require-Text $requiredText "SyntyPackHDocumentationTextMissing:$requiredText"
}

$definitionRoot = Join-Path $repoRoot 'eng/world-seedbeds/synty-bottom-up-inventory/definitions'
$definitionFiles = @(
    'h1/nature-trailhead.v2.json',
    'h2/nature-home-core.v2.json',
    'h3/nature-home-encounter-defense.v2.json',
    'h1/farm-production.v2.json',
    'h3/highland-farm.v2.json',
    'h1/town-market-receiving.v2.json',
    'h3/town-market-fulfillment.v2.json',
    'h1/hub-receiving-storage.v2.json',
    'h3/hub-fulfillment-operations.v2.json'
)
foreach ($relativePath in $definitionFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $definitionRoot $relativePath))) {
        throw "SyntyPackHDefinitionMissing:$relativePath"
    }
}

Write-Output 'SyntyPackHDocumentationTestsPassed:Packs=5;Prefabs=2346;Construction=SupportLayer'
