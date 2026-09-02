param(
    [ValidateSet('Write', 'Validate')]
    [string]$Mode = 'Validate'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$decisionPath = Join-Path $repositoryRoot 'docs\AI\DECISIONS.md'
$jsonPath = Join-Path $repositoryRoot 'docs\AI\generated\decision-field-index.json'
$markdownPath = Join-Path $repositoryRoot 'docs\AI\generated\decision-field-index.md'

$areaLabels = [ordered]@{
    ARCHITECTURE = '아키텍처'
    DATA = '데이터·공공자료'
    SIMULATION = 'Simulation 규칙·권위'
    UNITY = 'Unity 구현 기반'
    PRESENTATION = '표현·시각'
    WORLD = '월드·공간'
    GAMEPLAY = '게임플레이'
    PLANNING = '기획·업무 방법'
    OPERATIONS = '운영·개발 운영'
    STORY = '스토리'
    INTERACTION = '상호작용·UI'
    EVIDENCE = '증거·성숙도'
}

function Test-InRange([int]$Value, [int]$From, [int]$To) {
    return $Value -ge $From -and $Value -le $To
}

function Resolve-DecisionField([int]$Id, [string]$Title) {
    if ($Id -eq 96 -and $Title -match '카메라') { return @('PRESENTATION', 'WORLD-CAMERA-STREAMING') }
    if ($Id -eq 96 -and $Title -match '타로') { return @('GAMEPLAY', 'TAROT') }

    if (Test-InRange $Id 1 17) { return @('ARCHITECTURE', 'UNITY-WORLD-FOUNDATION') }
    if (Test-InRange $Id 18 26) { return @('DATA', 'MARKET-SUPPLY') }
    if (Test-InRange $Id 27 32) { return @('ARCHITECTURE', 'OPERATIONS-SIMULATION') }
    if (Test-InRange $Id 33 35) { return @('PRESENTATION', 'WORLD-REGION-ASSET') }
    if (Test-InRange $Id 36 37) { return @('DATA', 'PRODUCT-ASSET-IDENTITY') }
    if (Test-InRange $Id 38 50) { return @('SIMULATION', 'SETTLEMENT-ECONOMY') }
    if ($Id -eq 51) { return @('PLANNING', 'CODE-NAMING') }
    if (Test-InRange $Id 52 71) { return @('SIMULATION', 'LOGISTICS-TRADE') }
    if (Test-InRange $Id 72 80) { return @('DATA', 'TIME-PUBLIC-DATA') }
    if (Test-InRange $Id 81 95) { return @('SIMULATION', 'WORLD-OBJECT-RULES') }
    if (Test-InRange $Id 97 99) { return @('GAMEPLAY', 'TAROT') }
    if (Test-InRange $Id 100 113) { return @('WORLD', 'SPATIAL-DATA-PRESENTATION') }
    if (Test-InRange $Id 114 125) { return @('PRESENTATION', 'WORLD-CAMERA-STREAMING') }
    if (Test-InRange $Id 126 138) { return @('GAMEPLAY', 'TEAM-COMBAT') }
    if (Test-InRange $Id 139 144) { return @('ARCHITECTURE', 'REFACTOR-DATA-ASSET') }
    if (Test-InRange $Id 145 160) { return @('EVIDENCE', 'SPATIAL-MATURITY') }
    if (Test-InRange $Id 161 176) { return @('GAMEPLAY', 'NATURE-THREAT-RECOVERY') }
    if (Test-InRange $Id 177 200) { return @('WORLD', 'H-LH-ASSET-COMPOSITION') }
    if (Test-InRange $Id 201 210) { return @('EVIDENCE', 'WORLD-PLAY-LOOPS') }
    if (Test-InRange $Id 211 213) { return @('GAMEPLAY', 'TAROT-REALITY-SPATIAL') }
    if (Test-InRange $Id 214 230) { return @('ARCHITECTURE', 'PLAYABLE-DEVELOPMENT') }
    if (Test-InRange $Id 231 249) { return @('GAMEPLAY', 'WI-LOOP-PROGRESSION') }
    if (Test-InRange $Id 250 268) { return @('EVIDENCE', 'PRESENTATION-INTEGRATION') }
    if (Test-InRange $Id 269 286) { return @('PLANNING', 'GOAL-INQUIRY-HANDOFF') }
    if (Test-InRange $Id 287 319) { return @('GAMEPLAY', 'PLAYER-RECOVERY-RESOURCES') }
    if (Test-InRange $Id 320 324) { return @('GAMEPLAY', 'CONSTRUCTION-CANCEL') }
    if ($Id -eq 325) { return @('PLANNING', 'INQUIRY-SEARCH') }
    if (Test-InRange $Id 326 333) { return @('GAMEPLAY', 'HERBAL-TEA') }
    if (Test-InRange $Id 334 339) { return @('GAMEPLAY', 'IDEA-NPC-INQUIRY') }
    if (Test-InRange $Id 340 345) { return @('GAMEPLAY', 'CREDIT-MULTIPLAYER') }
    if (Test-InRange $Id 346 356) { return @('GAMEPLAY', 'HERBAL-CONTENT') }
    if ($Id -eq 357) { return @('GAMEPLAY', 'FARM-DELEGATION') }
    if (Test-InRange $Id 358 361) { return @('PRESENTATION', 'ANIMATION-WORKFLOW') }
    if (Test-InRange $Id 362 364) { return @('WORLD', 'LANDSCAPE-PLACEMENT-LH') }
    if (Test-InRange $Id 365 366) { return @('GAMEPLAY', 'FOCUS-RESEARCH') }
    if (Test-InRange $Id 367 374) { return @('GAMEPLAY', 'TRADE-REALITY') }
    if (Test-InRange $Id 375 383) { return @('OPERATIONS', 'OVERNIGHT-VISUAL-DEV') }
    if ($Id -eq 384) { return @('PLANNING', 'PROJECT-IDENTITY') }
    if ($Id -eq 385) { return @('PRESENTATION', 'HERBAL-PROP') }
    if (Test-InRange $Id 386 390) { return @('EVIDENCE', 'PRESENTATION-E4-E5') }
    if (Test-InRange $Id 391 398) { return @('PLANNING', 'PLAYER-CENTERED-INQUIRY') }
    if (Test-InRange $Id 399 405) { return @('GAMEPLAY', 'SEASON-TECH-TREE') }
    if (Test-InRange $Id 406 407) { return @('WORLD', 'WORLDMAP-PROPOSAL') }
    if (Test-InRange $Id 408 415) { return @('STORY', 'FIRST-DISCOVERY') }
    if (Test-InRange $Id 416 421) { return @('GAMEPLAY', 'PERSPECTIVE-FOCUS') }
    if (Test-InRange $Id 426 441) { return @('DATA', 'GAME-OBJECT-ASSET-DB') }
    if (Test-InRange $Id 442 447) { return @('WORLD', 'GRAPH-MAP-E6') }
    if ($Id -eq 448) { return @('OPERATIONS', 'GIT-COMMIT') }
    if (Test-InRange $Id 449 454) { return @('GAMEPLAY', 'HUB-REALITY-LOGISTICS') }
    if (Test-InRange $Id 455 464) { return @('INTERACTION', 'QUEST') }
    if (Test-InRange $Id 465 468) { return @('PLANNING', 'DISCOVERY-PLAN') }
    if ($Id -eq 469) { return @('EVIDENCE', 'E5-CONTEXT') }
    if (Test-InRange $Id 470 471) { return @('STORY', 'MAIN-STORY') }
    if (Test-InRange $Id 472 473) { return @('PLANNING', 'EVIDENCE-GOVERNANCE') }
    if (Test-InRange $Id 474 475) { return @('STORY', 'YODONG') }
    if (Test-InRange $Id 476 479) { return @('GAMEPLAY', 'YODONG') }
    if (Test-InRange $Id 480 482) { return @('PRESENTATION', 'COMBAT-RISK') }
    if (Test-InRange $Id 483 484) { return @('INTERACTION', 'COMBAT-COMMAND') }
    if ($Id -eq 485) { return @('GAMEPLAY', 'BATTLE-PREPARATION') }
    if ($Id -eq 486) { return @('PLANNING', 'DECISION-NAMING') }
    if ($Id -eq 487) { return @('PLANNING', 'DECISION-WI-RELATION') }
    if ($Id -eq 488) { return @('PLANNING', 'GRAPH-MAP-HANDOFF') }
    if ($Id -eq 489) { return @('PLANNING', 'GRAPH-MAP-DEVELOPMENT-HANDOFF') }
    if ($Id -eq 490) { return @('STORY', 'PROTAGONIST') }
    if ($Id -eq 491) { return @('GAMEPLAY', 'ALCHEMY-ECONOMY') }
    if ($Id -eq 492) { return @('STORY', 'YODONG-SUCCESSION') }
    if ($Id -eq 493) { return @('GAMEPLAY', 'YODONG-CRISIS') }
    if ($Id -eq 494) { return @('GAMEPLAY', 'WINTER-LOGISTICS') }
    if ($Id -eq 495) { return @('STORY', 'DUAL-PROTAGONIST') }
    if ($Id -eq 496) { return @('STORY', 'DUAL-PROTAGONIST') }
    if ($Id -eq 497) { return @('GAMEPLAY', 'SETTLEMENT-SUPPLY') }
    if ($Id -eq 498) { return @('GAMEPLAY', 'SETTLEMENT-SUPPLY') }
    if ($Id -eq 499) { return @('GAMEPLAY', 'SETTLEMENT-SUPPLY') }
    if ($Id -eq 500) { return @('GAMEPLAY', 'SETTLEMENT-SUPPLY') }
    if ($Id -eq 501) { return @('GAMEPLAY', 'SETTLEMENT-SUPPLY') }
    if ($Id -eq 502) { return @('GAMEPLAY', 'SETTLEMENT-SUPPLY') }
    if ($Id -eq 503) { return @('INTERACTION', 'EMBODIED-STORY-CHOICE') }
    if ($Id -eq 504) { return @('INTERACTION', 'EMBODIED-STORY-CHOICE') }
    if ($Id -eq 505) { return @('PLANNING', 'AUDIO-REQUIREMENT') }
    if ($Id -eq 506) { return @('INTERACTION', 'EMBODIED-STORY-CHOICE') }
    if ($Id -eq 507) { return @('STORY', 'ADVENTURER-POWER-GROWTH') }
    if ($Id -eq 508) { return @('INTERACTION', 'EMBODIED-STORY-CHOICE') }
    if ($Id -eq 509) { return @('STORY', 'ADVENTURER-FIRST-MEETING') }
    if ($Id -eq 510) { return @('STORY', 'ADVENTURER-POWER-GROWTH') }
    if ($Id -eq 511) { return @('INTERACTION', 'NPC-FIELD-AUTONOMY') }
    if ($Id -eq 512) { return @('GAMEPLAY', 'COMPANION-PARTY-COMPOSITION') }
    if ($Id -eq 513) { return @('STORY', 'ADVENTURER-IDEA-SIGHT') }
    if ($Id -eq 514) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 515) { return @('INTERACTION', 'NPC-FIELD-AUTONOMY') }
    if ($Id -eq 516) { return @('GAMEPLAY', 'COMPANION-PARTY-COMPOSITION') }
    if ($Id -eq 517) { return @('STORY', 'ADVENTURER-POWER-GROWTH') }
    if ($Id -eq 518) { return @('STORY', 'ADVENTURER-IDEA-SIGHT') }
    if ($Id -eq 519) { return @('STORY', 'ADVENTURER-IDEA-SIGHT') }
    if ($Id -eq 520) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 521) { return @('STORY', 'ADVENTURER-IDEA-SIGHT') }
    if ($Id -eq 522) { return @('PLANNING', 'PROJECT-IDENTITY') }
    if ($Id -eq 523) { return @('GAMEPLAY', 'COMPANION-PARTY-COMPOSITION') }
    if ($Id -eq 524) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 525) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 526) { return @('INTERACTION', 'NPC-FIELD-AUTONOMY') }
    if ($Id -eq 527) { return @('INTERACTION', 'EMBODIED-STORY-CHOICE') }
    if ($Id -eq 528) { return @('STORY', 'ADVENTURER-IDEA-SIGHT') }
    if ($Id -eq 529) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 530) { return @('INTERACTION', 'EMBODIED-STORY-CHOICE') }
    if ($Id -eq 531) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 532) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 533) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 534) { return @('INTERACTION', 'EMBODIED-STORY-CHOICE') }
    if ($Id -eq 535) { return @('PRESENTATION', 'FARM-HANS-HOUSE') }
    if ($Id -eq 536) { return @('PRESENTATION', 'FARM-HANS-HOUSE') }
    if ($Id -eq 537) { return @('STORY', 'ADVENTURER-FIRST-MISSION') }
    if ($Id -eq 538) { return @('STORY', 'HANS-HIDDEN-MASTER') }
    if ($Id -eq 539) { return @('STORY', 'HANS-HIDDEN-MASTER') }
    if ($Id -eq 540) { return @('STORY', 'HANS-HIDDEN-MASTER') }
    if ($Id -eq 541) { return @('STORY', 'HANS-HIDDEN-MASTER') }
    if ($Id -eq 542) { return @('PRESENTATION', 'HANS-WEAPON') }
    if ($Id -eq 543) { return @('STORY', 'FARM-BOUNDARY-THREAT') }
    if ($Id -eq 544) { return @('PRESENTATION', 'HANS-WEAPON') }
    if ($Id -eq 545) { return @('STORY', 'FARM-BOUNDARY-THREAT') }
    if ($Id -eq 546) { return @('STORY', 'ADVENTURER-IDEA-SIGHT') }
    if ($Id -eq 547) { return @('GAMEPLAY', 'HUB-INFERENCE-QUEST') }
    if ($Id -eq 548) { return @('GAMEPLAY', 'HUB-RECOVERY-MEDITATION') }
    if ($Id -eq 549) { return @('GAMEPLAY', 'MEDITATION-INSPIRATION') }
    if ($Id -eq 550) { return @('GAMEPLAY', 'MEDITATION-RECOVERY-LOOP') }
    if ($Id -eq 551) { return @('GAMEPLAY', 'COMBAT-MIND-FOCUS') }
    if ($Id -eq 552) { return @('INTERACTION', 'COMBAT-FOCUS-TRIGGER') }
    if ($Id -eq 553) { return @('GAMEPLAY', 'COMBAT-DIVIDE-CONQUER') }
    if ($Id -eq 554) { return @('PRESENTATION', 'SYNTY-SURVEY-HANDOFF') }
    if ($Id -eq 555) { return @('PRESENTATION', 'FARM-BOUNDARY-BEAST') }
    if ($Id -eq 556) { return @('GAMEPLAY', 'FIVE-ELEMENT-RECOVERY-PURPOSE') }

    throw "분류되지 않은 결정: D-$('{0:D3}' -f $Id) $Title"
}

function Escape-MarkdownCell([string]$Value) {
    return ($Value -replace '\|', '\|' -replace "`r?`n", ' ')
}

$decisionText = Get-Content -LiteralPath $decisionPath -Raw -Encoding UTF8
$headingMatches = [regex]::Matches($decisionText, '(?m)^## D-(\d+)\s+(.+)$')
$lineStarts = New-Object System.Collections.Generic.List[int]
$lineStarts.Add(0)
for ($i = 0; $i -lt $decisionText.Length; $i++) {
    if ($decisionText[$i] -eq "`n") { $lineStarts.Add($i + 1) }
}

$rawDecisions = foreach ($match in $headingMatches) {
    $lineNumber = [Array]::BinarySearch($lineStarts.ToArray(), $match.Index)
    if ($lineNumber -lt 0) { $lineNumber = -$lineNumber - 2 }
    [pscustomobject]@{
        Id = [int]$match.Groups[1].Value
        GlobalId = 'D-{0:D3}' -f [int]$match.Groups[1].Value
        Title = $match.Groups[2].Value.Trim()
        SourceLine = $lineNumber + 1
        SourceOrder = $match.Index
    }
}

$orderedDecisions = @($rawDecisions | Sort-Object Id, SourceOrder)
$topicOrdinals = @{}
$globalOccurrences = @{}
$decisions = New-Object System.Collections.Generic.List[object]

foreach ($decision in $orderedDecisions) {
    $resolved = Resolve-DecisionField $decision.Id $decision.Title
    $area = $resolved[0]
    $topic = $resolved[1]
    $topicKey = "$area/$topic"
    if (-not $topicOrdinals.ContainsKey($topicKey)) { $topicOrdinals[$topicKey] = 0 }
    $topicOrdinals[$topicKey]++

    if (-not $globalOccurrences.ContainsKey($decision.GlobalId)) { $globalOccurrences[$decision.GlobalId] = 0 }
    $globalOccurrences[$decision.GlobalId]++
    $occurrence = $globalOccurrences[$decision.GlobalId]
    $globalKey = if (($orderedDecisions | Where-Object GlobalId -eq $decision.GlobalId).Count -gt 1) {
        "$($decision.GlobalId)#$occurrence"
    } else {
        $decision.GlobalId
    }

    $fieldId = 'D-{0}-{1}-{2:D3}' -f $area, $topic, $topicOrdinals[$topicKey]
    $reviewDepth = if ($decision.Id -ge 474) { 'BodyReviewed' } else { 'TitleReviewed' }
    $decisions.Add([pscustomobject][ordered]@{
        globalKey = $globalKey
        globalDecisionId = $decision.GlobalId
        globalOccurrence = $occurrence
        fieldDecisionId = $fieldId
        primaryAreaCode = $area
        primaryAreaName = $areaLabels[$area]
        topicCode = $topic
        topicOrdinal = $topicOrdinals[$topicKey]
        title = $decision.Title
        reviewDepth = $reviewDepth
        sourceLine = $decision.SourceLine
    })
}

$duplicateGlobalIds = @($decisions | Group-Object globalDecisionId | Where-Object Count -gt 1 | ForEach-Object Name)
$maximumId = ($decisions | ForEach-Object { [int]($_.globalDecisionId.Substring(2)) } | Measure-Object -Maximum).Maximum
$presentIds = @($decisions | ForEach-Object { [int]($_.globalDecisionId.Substring(2)) } | Sort-Object -Unique)
$missingGlobalIds = @(1..$maximumId | Where-Object { $_ -notin $presentIds } | ForEach-Object { 'D-{0:D3}' -f $_ })

$inlineIdMatches = [regex]::Matches($decisionText, '(?m)^- 분야별 ID: `([^`]+)`$')
$inlineIds = @($inlineIdMatches | ForEach-Object { $_.Groups[1].Value })
$generatedIds = @($decisions | ForEach-Object fieldDecisionId)
foreach ($inlineId in $inlineIds) {
    if ($inlineId -notin $generatedIds) { throw "본문 분야별 ID가 전수 색인과 다릅니다: $inlineId" }
}
if (@($generatedIds | Group-Object | Where-Object Count -gt 1).Count -gt 0) {
    throw '중복 분야별 ID가 생성되었습니다.'
}

$topicSummary = @($decisions | Group-Object primaryAreaCode, topicCode | ForEach-Object {
    $sample = $_.Group[0]
    [ordered]@{
        primaryAreaCode = $sample.primaryAreaCode
        primaryAreaName = $sample.primaryAreaName
        topicCode = $sample.topicCode
        decisionCount = $_.Count
        bodyReviewedCount = @($_.Group | Where-Object reviewDepth -eq 'BodyReviewed').Count
        titleReviewedCount = @($_.Group | Where-Object reviewDepth -eq 'TitleReviewed').Count
        firstGlobalDecisionId = $_.Group[0].globalDecisionId
        lastGlobalDecisionId = $_.Group[-1].globalDecisionId
    }
} | Sort-Object primaryAreaCode, topicCode)

$areaSummary = @($decisions | Group-Object primaryAreaCode | ForEach-Object {
    $sample = $_.Group[0]
    [ordered]@{
        primaryAreaCode = $sample.primaryAreaCode
        primaryAreaName = $sample.primaryAreaName
        decisionCount = $_.Count
        topicCount = @($_.Group | Select-Object -ExpandProperty topicCode -Unique).Count
        bodyReviewedCount = @($_.Group | Where-Object reviewDepth -eq 'BodyReviewed').Count
        titleReviewedCount = @($_.Group | Where-Object reviewDepth -eq 'TitleReviewed').Count
    }
} | Sort-Object primaryAreaCode)

$sourceHash = (Get-FileHash -LiteralPath $decisionPath -Algorithm SHA256).Hash
$jsonObject = [ordered]@{
    schemaVersion = 'decision-field-index.v1'
    sourceRef = 'docs/AI/DECISIONS.md'
    sourceSha256 = $sourceHash
    classificationMethod = 'ExhaustiveHeadingInventoryWithManualFieldRanges'
    reviewDepthMeaning = [ordered]@{
        TitleReviewed = '결정 제목과 주변 결정군을 전수 대조한 1차 분야 분류'
        BodyReviewed = '결정 본문까지 현재 기획에서 직접 재검토한 분류'
    }
    inventory = [ordered]@{
        headingCount = $decisions.Count
        uniqueGlobalIdCount = @($presentIds).Count
        maximumGlobalId = 'D-{0:D3}' -f [int]$maximumId
        missingGlobalIds = $missingGlobalIds
        duplicateGlobalIds = $duplicateGlobalIds
        primaryAreaCount = $areaSummary.Count
        topicCount = $topicSummary.Count
    }
    areaSummary = $areaSummary
    topicSummary = $topicSummary
    decisions = $decisions
}
$json = $jsonObject | ConvertTo-Json -Depth 8

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# 결정 분야별 전수 색인')
$markdown.Add('')
$markdown.Add('> 기준 원문: [DECISIONS.md](../DECISIONS.md). 이 문서는 생성 산출물이며 직접 수정하지 않는다.')
$markdown.Add('')
$markdown.Add('## 읽는 법')
$markdown.Add('')
$markdown.Add('- `D-###`는 전역 작성 이력 번호다.')
$markdown.Add('- `D-{주분야}-{세부주제}-{순번}`은 분야별 결정 ID다. 마지막 숫자는 같은 주분야·세부주제 안의 누적 결정 수다.')
$markdown.Add('- `TitleReviewed`는 제목과 주변 결정군을 전수 대조한 1차 분류이고, `BodyReviewed`는 본문까지 현재 기획에서 직접 재검토한 분류다.')
$markdown.Add('- 분야별 결정 수는 구현 완료나 E 성숙도가 아니라 기획 결정의 분류 개수다.')
$markdown.Add('')
$markdown.Add('## 전수성 점검')
$markdown.Add('')
$missingText = if ($missingGlobalIds.Count -eq 0) { '없음' } else { $missingGlobalIds -join ', ' }
$duplicateText = if ($duplicateGlobalIds.Count -eq 0) { '없음' } else { $duplicateGlobalIds -join ', ' }
$markdown.Add(('- 결정 제목: **{0}개**' -f $decisions.Count))
$markdown.Add(('- 고유 전역 번호: **{0}개** / 최댓값 `D-{1:D3}`' -f @($presentIds).Count, [int]$maximumId))
$markdown.Add(('- 비어 있는 전역 번호: `{0}`' -f $missingText))
$markdown.Add(('- 중복 전역 번호: `{0}`' -f $duplicateText))
$markdown.Add(('- 주분야: **{0}개**, 세부주제: **{1}개**' -f $areaSummary.Count, $topicSummary.Count))
$markdown.Add('')
$markdown.Add('## 주분야 요약')
$markdown.Add('')
$markdown.Add('| 주분야 | 세부주제 수 | 결정 수 | 본문 재검토 | 제목 1차 분류 |')
$markdown.Add('| --- | ---: | ---: | ---: | ---: |')
foreach ($area in $areaSummary) {
    $markdown.Add(('| {0} (`{1}`) | {2} | {3} | {4} | {5} |' -f $area.primaryAreaName, $area.primaryAreaCode, $area.topicCount, $area.decisionCount, $area.bodyReviewedCount, $area.titleReviewedCount))
}
$markdown.Add('')
$markdown.Add('## 세부주제 요약')
$markdown.Add('')
$markdown.Add('| 주분야 | 세부주제 | 결정 수 | 본문 재검토 | 전역 범위 |')
$markdown.Add('| --- | --- | ---: | ---: | --- |')
foreach ($topic in $topicSummary) {
    $markdown.Add(('| {0} | `{1}` | {2} | {3} | `{4}` ~ `{5}` |' -f $topic.primaryAreaName, $topic.topicCode, $topic.decisionCount, $topic.bodyReviewedCount, $topic.firstGlobalDecisionId, $topic.lastGlobalDecisionId))
}
$markdown.Add('')
$markdown.Add('## 전체 결정 대응표')
$markdown.Add('')
$markdown.Add('| 전역 번호 | 분야별 ID | 결정 제목 | 검토 깊이 |')
$markdown.Add('| --- | --- | --- | --- |')
foreach ($decision in $decisions) {
    $anchor = ($decision.globalDecisionId.ToLowerInvariant() + '-' + ($decision.title.ToLowerInvariant() -replace '[^0-9a-z가-힣\s-]', '' -replace '\s+', '-'))
    $title = Escape-MarkdownCell $decision.title
    $markdown.Add(('| [{0}](../DECISIONS.md#{1}) | `{2}` | {3} | `{4}` |' -f $decision.globalKey, $anchor, $decision.fieldDecisionId, $title, $decision.reviewDepth))
}
$markdownText = ($markdown -join "`n") + "`n"

if ($Mode -eq 'Write') {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($jsonPath, $json + "`n", $utf8NoBom)
    [System.IO.File]::WriteAllText($markdownPath, $markdownText, $utf8NoBom)
    Write-Host "[write] $jsonPath"
    Write-Host "[write] $markdownPath"
} else {
    if (-not (Test-Path -LiteralPath $jsonPath)) { throw "생성 JSON이 없습니다: $jsonPath" }
    if (-not (Test-Path -LiteralPath $markdownPath)) { throw "생성 Markdown이 없습니다: $markdownPath" }
    $actualJson = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8
    $actualMarkdown = Get-Content -LiteralPath $markdownPath -Raw -Encoding UTF8
    if ($actualJson -ne ($json + "`n")) { throw 'decision-field-index.json이 현재 DECISIONS.md와 다릅니다. Write가 필요합니다.' }
    if ($actualMarkdown -ne $markdownText) { throw 'decision-field-index.md가 현재 DECISIONS.md와 다릅니다. Write가 필요합니다.' }
    Write-Host '[pass] 결정 분야별 JSON/Markdown이 현재 원문과 일치합니다.'
}

Write-Host "headings=$($decisions.Count) uniqueGlobalIds=$(@($presentIds).Count) areas=$($areaSummary.Count) topics=$($topicSummary.Count)"
Write-Host "missing=$($missingGlobalIds -join ',') duplicates=$($duplicateGlobalIds -join ',')"
