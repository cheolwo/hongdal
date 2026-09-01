param(
    [ValidateSet('Write', 'Validate')]
    [string]$Mode = 'Validate'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$decisionPath = Join-Path $repositoryRoot 'docs\AI\DECISIONS.md'
$decisionIndexPath = Join-Path $repositoryRoot 'docs\AI\generated\decision-field-index.json'
$wiCatalogPath = Join-Path $repositoryRoot 'eng\execution-ledgers\world-interactions.json'
$jsonPath = Join-Path $repositoryRoot 'docs\AI\generated\decision-wi-relationship-index.json'
$markdownPath = Join-Path $repositoryRoot 'docs\AI\generated\decision-wi-relationship-index.md'

function Escape-MarkdownCell([string]$Value) {
    return ($Value -replace '\|', '\|' -replace "`r?`n", ' ')
}

$decisionText = Get-Content -LiteralPath $decisionPath -Raw -Encoding UTF8
$decisionIndex = Get-Content -LiteralPath $decisionIndexPath -Raw -Encoding UTF8 | ConvertFrom-Json
$wiCatalog = Get-Content -LiteralPath $wiCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json

$decisionHash = (Get-FileHash -LiteralPath $decisionPath -Algorithm SHA256).Hash
if ($decisionIndex.sourceSha256 -cne $decisionHash) {
    throw 'decision-field-index.json이 현재 DECISIONS.md와 다릅니다. 분야별 색인을 먼저 갱신하세요.'
}

$wiById = @{}
foreach ($wi in $wiCatalog.items) {
    if ($wiById.ContainsKey($wi.id)) { throw "공식 WI ID 중복: $($wi.id)" }
    $wiById[$wi.id] = $wi
}

$headingMatches = [regex]::Matches($decisionText, '(?m)^## D-(\d+)\s+(.+)$')
if ($headingMatches.Count -ne $decisionIndex.decisions.Count) {
    throw '결정 제목 수와 분야별 색인의 결정 수가 다릅니다.'
}
$lineStarts = New-Object System.Collections.Generic.List[int]
$lineStarts.Add(0)
for ($lineIndex = 0; $lineIndex -lt $decisionText.Length; $lineIndex++) {
    if ($decisionText[$lineIndex] -eq "`n") { $lineStarts.Add($lineIndex + 1) }
}

$decisionRows = New-Object System.Collections.Generic.List[object]
$relationshipRows = New-Object System.Collections.Generic.List[object]
$nonCanonicalRows = New-Object System.Collections.Generic.List[object]

for ($index = 0; $index -lt $headingMatches.Count; $index++) {
    $match = $headingMatches[$index]
    $end = if ($index + 1 -lt $headingMatches.Count) { $headingMatches[$index + 1].Index } else { $decisionText.Length }
    $block = $decisionText.Substring($match.Index, $end - $match.Index)
    $sourceLineIndex = [Array]::BinarySearch($lineStarts.ToArray(), $match.Index)
    if ($sourceLineIndex -lt 0) { $sourceLineIndex = -$sourceLineIndex - 2 }
    $sourceLine = $sourceLineIndex + 1
    $globalDecisionId = 'D-{0:D3}' -f [int]$match.Groups[1].Value
    $matchesAtSource = @($decisionIndex.decisions | Where-Object {
        $_.globalDecisionId -ceq $globalDecisionId -and [int]$_.sourceLine -eq $sourceLine
    })
    if ($matchesAtSource.Count -ne 1) {
        throw "결정 원문 위치를 분야별 색인에서 단일하게 찾지 못했습니다: $globalDecisionId line $sourceLine"
    }
    $decision = $matchesAtSource[0]

    $canonicalIds = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
    foreach ($tokenMatch in [regex]::Matches($block, '(?<![A-Z0-9-])(WI-[A-Z0-9]+(?:-[A-Z0-9]+)+)')) {
        $token = $tokenMatch.Groups[1].Value
        if ($wiById.ContainsKey($token)) { [void]$canonicalIds.Add($token) }
    }

    foreach ($rangeMatch in [regex]::Matches($block, '(?<![A-Z0-9-])WI-([A-Z0-9]+)-(\d+)~(?:WI-\1-)?(\d+)')) {
        $prefix = $rangeMatch.Groups[1].Value
        $fromText = $rangeMatch.Groups[2].Value
        $toText = $rangeMatch.Groups[3].Value
        $from = [int]$fromText
        $to = [int]$toText
        if ($to -lt $from -or ($to - $from) -gt 100) { continue }
        $width = [Math]::Max($fromText.Length, $toText.Length)
        for ($number = $from; $number -le $to; $number++) {
            $candidate = 'WI-{0}-{1}' -f $prefix, $number.ToString(('D{0}' -f $width))
            if ($wiById.ContainsKey($candidate)) { [void]$canonicalIds.Add($candidate) }
        }
    }

    $allTokens = @([regex]::Matches($block, '(?<![A-Z0-9-])(WI-[A-Z0-9]+(?:-[A-Z0-9]+)+)') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
    $nonCanonicalTokens = @($allTokens | Where-Object { -not $wiById.ContainsKey($_) } | Sort-Object -Unique)
    foreach ($token in $nonCanonicalTokens) {
        $nonCanonicalRows.Add([pscustomobject][ordered]@{
            globalKey = $decision.globalKey
            globalDecisionId = $decision.globalDecisionId
            fieldDecisionId = $decision.fieldDecisionId
            token = $token
            reasonCode = if ($token -match '^WI-(SINGLE-RESPONSIBILITY|YIN-YANG-ACTOR-QUADRANTS)$') { 'LikelyEmbeddedWorkOrderToken' } else { 'MissingFromCurrentCatalog' }
        })
    }

    $linkedIds = @($canonicalIds | Sort-Object)
    foreach ($wiId in $linkedIds) {
        $relationshipRows.Add([pscustomobject][ordered]@{
            relationKindCode = 'ExplicitMention'
            globalKey = $decision.globalKey
            globalDecisionId = $decision.globalDecisionId
            fieldDecisionId = $decision.fieldDecisionId
            primaryAreaCode = $decision.primaryAreaCode
            topicCode = $decision.topicCode
            wiId = $wiId
        })
    }

    $decisionRows.Add([pscustomobject][ordered]@{
        globalKey = $decision.globalKey
        globalDecisionId = $decision.globalDecisionId
        fieldDecisionId = $decision.fieldDecisionId
        primaryAreaCode = $decision.primaryAreaCode
        primaryAreaName = $decision.primaryAreaName
        topicCode = $decision.topicCode
        title = $decision.title
        sourceLine = $decision.sourceLine
        relationStateCode = if ($linkedIds.Count -gt 0) { 'ExplicitCanonicalLink' } elseif ($nonCanonicalTokens.Count -gt 0) { 'NonCanonicalTokenOnly' } else { 'NoExplicitCanonicalLink' }
        explicitCanonicalWiIds = $linkedIds
        nonCanonicalWiTokens = $nonCanonicalTokens
    })
}

$wiRows = New-Object System.Collections.Generic.List[object]
foreach ($wi in @($wiCatalog.items | Sort-Object groupCode, sequence, id)) {
    $links = @($relationshipRows | Where-Object wiId -eq $wi.id | Sort-Object globalDecisionId, globalKey)
    $wiRows.Add([pscustomobject][ordered]@{
        wiId = $wi.id
        groupCode = $wi.groupCode
        title = $wi.title
        kind = $wi.kind
        implementationStatusCode = $wi.implementation.status
        implementationStageCode = $wi.implementation.currentStage
        integrationStatusCode = $wi.integration.status
        integrationStageCode = $wi.integration.currentStage
        relationStateCode = if ($links.Count -gt 0) { 'ExplicitDecisionLink' } else { 'NoExplicitDecisionLink' }
        linkedDecisions = @($links | ForEach-Object {
            [ordered]@{
                globalKey = $_.globalKey
                globalDecisionId = $_.globalDecisionId
                fieldDecisionId = $_.fieldDecisionId
                primaryAreaCode = $_.primaryAreaCode
                topicCode = $_.topicCode
                relationKindCode = $_.relationKindCode
            }
        })
    })
}

$nonCanonicalTokenValues = @()
foreach ($nonCanonicalRow in $nonCanonicalRows) { $nonCanonicalTokenValues += $nonCanonicalRow.token }
$nonCanonicalTokenTypeCount = @($nonCanonicalTokenValues | Sort-Object -Unique).Count
$linkedDecisionCount = 0
foreach ($decisionRow in $decisionRows) {
    if ($decisionRow.relationStateCode -eq 'ExplicitCanonicalLink') { $linkedDecisionCount++ }
}
$linkedWiCount = 0
foreach ($wiRow in $wiRows) {
    if ($wiRow.relationStateCode -eq 'ExplicitDecisionLink') { $linkedWiCount++ }
}
$relationshipArray = @()
foreach ($relationshipRow in $relationshipRows) { $relationshipArray += $relationshipRow }
$nonCanonicalArray = @()
foreach ($nonCanonicalRow in $nonCanonicalRows) { $nonCanonicalArray += $nonCanonicalRow }
$decisionArray = @()
foreach ($decisionRow in $decisionRows) { $decisionArray += $decisionRow }
$wiArray = @()
foreach ($wiRow in $wiRows) { $wiArray += $wiRow }

$jsonObject = [ordered]@{
    schemaVersion = 'decision-wi-relationship-index.v1'
    authority = 'DerivedPlanningLookupOnly'
    decisionSource = [ordered]@{
        path = 'docs/AI/DECISIONS.md'
        sha256 = $decisionHash
        fieldIndexPath = 'docs/AI/generated/decision-field-index.json'
        fieldIndexSha256 = (Get-FileHash -LiteralPath $decisionIndexPath -Algorithm SHA256).Hash
    }
    wiSource = [ordered]@{
        path = 'eng/execution-ledgers/world-interactions.json'
        sha256 = (Get-FileHash -LiteralPath $wiCatalogPath -Algorithm SHA256).Hash
        revision = $wiCatalog.revision
    }
    relationPolicy = [ordered]@{
        automaticRelationKind = 'ExplicitMention'
        inferenceByTitleOrField = $false
        nonCanonicalTokensAreMapped = $false
        relationDoesNotProve = @('Approval', 'Implementation', 'EvidenceStage', 'UnityBinding', 'RuntimeExecution')
    }
    inventory = [ordered]@{
        decisionCount = $decisionRows.Count
        wiCount = $wiRows.Count
        explicitRelationshipCount = $relationshipRows.Count
        linkedDecisionCount = $linkedDecisionCount
        linkedWiCount = $linkedWiCount
        nonCanonicalOccurrenceCount = $nonCanonicalRows.Count
        nonCanonicalTokenCount = $nonCanonicalTokenTypeCount
    }
    relationships = $relationshipArray
    nonCanonicalTokens = $nonCanonicalArray
    decisions = $decisionArray
    worldInteractions = $wiArray
}
$json = $jsonObject | ConvertTo-Json -Depth 12

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# 결정과 WI 양방향 관계 색인')
$markdown.Add('')
$markdown.Add('> [DECISIONS.md](../DECISIONS.md), [결정 분야별 전수 색인](decision-field-index.md), [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json)을 읽어 생성한다. 직접 수정하지 않는다.')
$markdown.Add('')
$markdown.Add('## 판정 경계')
$markdown.Add('')
$markdown.Add('- `ExplicitMention`: 결정 본문에 현재 공식 WI ID가 직접 적혔거나 같은 접두사의 숫자 범위로 명시된 관계다.')
$markdown.Add('- 분야·제목·순번이 비슷하다는 이유로 연결하지 않는다. 명시 관계가 없으면 `미명시`로 남긴다.')
$markdown.Add('- 관계는 기획 탐색용이다. 승인·구현·E 단계·Unity 배치·실행 증거를 뜻하지 않는다.')
$markdown.Add('- 공식 WI 대장에 없는 표기는 아래 비정규 표기 표에서 따로 확인한다.')
$markdown.Add('')
$markdown.Add('## 전수 요약')
$markdown.Add('')
$markdown.Add(('- 결정 **{0}개**, 공식 WI **{1}개**, 명시 관계 **{2}쌍**' -f $decisionRows.Count, $wiRows.Count, $relationshipRows.Count))
$markdown.Add(('- WI가 명시된 결정 **{0}개**, 결정을 명시적으로 연결한 WI **{1}개**' -f $jsonObject.inventory.linkedDecisionCount, $jsonObject.inventory.linkedWiCount))
$markdown.Add(('- 비정규 WI 표기 **{0}종 / {1}건**' -f $jsonObject.inventory.nonCanonicalTokenCount, $jsonObject.inventory.nonCanonicalOccurrenceCount))
$markdown.Add(('- WI 대장 판본: `{0}`' -f $wiCatalog.revision))
$markdown.Add('')
$markdown.Add('## 결정에서 WI 보기')
$markdown.Add('')
$markdown.Add('| 전역 결정 | 분야별 결정 | 분야 / 주제 | 결정 | 공식 WI 명시 | 상태 |')
$markdown.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($decision in $decisionRows) {
    $wiText = if ($decision.explicitCanonicalWiIds.Count -gt 0) { ($decision.explicitCanonicalWiIds | ForEach-Object { "``$_``" }) -join ', ' } else { '미명시' }
    $markdown.Add(('| [{0}](../DECISIONS.md#L{1}) | `{2}` | `{3}` / `{4}` | {5} | {6} | `{7}` |' -f $decision.globalKey, $decision.sourceLine, $decision.fieldDecisionId, $decision.primaryAreaCode, $decision.topicCode, (Escape-MarkdownCell $decision.title), $wiText, $decision.relationStateCode))
}
$markdown.Add('')
$markdown.Add('## WI에서 결정 보기')
$markdown.Add('')
$markdown.Add('| WI | 그룹 | 기능 | 구현 / 통합 | 연결된 결정 | 상태 |')
$markdown.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($wi in $wiRows) {
    $decisionTextCell = if ($wi.linkedDecisions.Count -gt 0) { ($wi.linkedDecisions | ForEach-Object { "``$($_.globalKey)`` / ``$($_.fieldDecisionId)``" }) -join '<br>' } else { '미명시' }
    $stageText = '{0}:{1} / {2}:{3}' -f $wi.implementationStatusCode, $wi.implementationStageCode, $wi.integrationStatusCode, $wi.integrationStageCode
    $markdown.Add(('| `{0}` | `{1}` | {2} | `{3}` | {4} | `{5}` |' -f $wi.wiId, $wi.groupCode, (Escape-MarkdownCell $wi.title), $stageText, $decisionTextCell, $wi.relationStateCode))
}
$markdown.Add('')
$markdown.Add('## 비정규·현 대장 외 WI 표기')
$markdown.Add('')
if ($nonCanonicalRows.Count -eq 0) {
    $markdown.Add('- 없음')
} else {
    $markdown.Add('| 결정 | 표기 | 판정 |')
    $markdown.Add('| --- | --- | --- |')
    foreach ($row in $nonCanonicalRows) {
        $markdown.Add(('| `{0}` / `{1}` | `{2}` | `{3}` |' -f $row.globalKey, $row.fieldDecisionId, $row.token, $row.reasonCode))
    }
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
    if ((Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8) -ne ($json + "`n")) { throw 'decision-wi-relationship-index.json이 현재 원문과 다릅니다.' }
    if ((Get-Content -LiteralPath $markdownPath -Raw -Encoding UTF8) -ne $markdownText) { throw 'decision-wi-relationship-index.md가 현재 원문과 다릅니다.' }
    Write-Host '[pass] 결정-WI JSON/Markdown이 현재 원문과 일치합니다.'
}

Write-Host "decisions=$($decisionRows.Count) wis=$($wiRows.Count) relationships=$($relationshipRows.Count) linkedDecisions=$($jsonObject.inventory.linkedDecisionCount) linkedWis=$($jsonObject.inventory.linkedWiCount)"
Write-Host "nonCanonicalTypes=$($jsonObject.inventory.nonCanonicalTokenCount) nonCanonicalOccurrences=$($jsonObject.inventory.nonCanonicalOccurrenceCount)"
