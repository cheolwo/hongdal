param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$profilePath = Join-Path $PSScriptRoot "area-set-immersion/farm-production.v1.json"
$actualE5Path = Join-Path $PSScriptRoot "generated/actual-e5-spatial.v1.json"
$theoryPath = Join-Path $PSScriptRoot "generated/theory-spatial-factory.v1.json"
$h5Path = Join-Path $PSScriptRoot "generated/h5-world-layout.v1.json"
$wiPath = Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json"
$outputPath = Join-Path $PSScriptRoot "generated/area-set-immersion-readiness.v1.json"
$markdownPath = Join-Path $repositoryRoot "docs/AI/generated/farm-area-set-immersion-readiness.md"

function Read-Json([string] $Path) {
    Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Get-Sha256([byte[]] $Bytes) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash($Bytes))).Replace("-", "").ToLowerInvariant() }
    finally { $sha.Dispose() }
}

function Get-TextHash([string] $Text) {
    Get-Sha256 ([Text.Encoding]::UTF8.GetBytes($Text))
}

function Get-StableJson($Value) {
    $Value | ConvertTo-Json -Depth 100
}

function Require([bool] $Condition, [string] $ErrorCode) {
    if (-not $Condition) { throw $ErrorCode }
}

function Get-GraphReachable(
    [string] $FromGraph,
    [string] $ToGraph,
    [object[]] $Relations) {
    $visited = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $queue = [Collections.Generic.Queue[string]]::new()
    $queue.Enqueue($FromGraph)
    [void] $visited.Add($FromGraph)
    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        if ($current -eq $ToGraph) { return $true }
        foreach ($relation in $Relations) {
            $next = $null
            if ([string] $relation.fromGraphStableId -eq $current) { $next = [string] $relation.toGraphStableId }
            elseif ([string] $relation.toGraphStableId -eq $current) { $next = [string] $relation.fromGraphStableId }
            if ($null -ne $next -and $visited.Add($next)) { $queue.Enqueue($next) }
        }
    }
    return $false
}

$profile = Read-Json $profilePath
$actualE5 = Read-Json $actualE5Path
$theory = Read-Json $theoryPath
$h5 = Read-Json $h5Path
$wiCatalog = Read-Json $wiPath

Require ($profile.schemaVersion -eq "simulation-area-set-immersion-profile.v1") "AreaSetImmersionProfileSchemaInvalid"
$farmArea = @($actualE5.areaSets | Where-Object { $_.definition.areaSetStableId -eq $profile.areaSetStableId })
Require ($farmArea.Count -eq 1) "AreaSetImmersionFarmAreaSetMissing"
$farmArea = $farmArea[0]
Require ($farmArea.definition.definitionStatusCode -eq "Available") "AreaSetImmersionFarmE5Unavailable"
Require (@($farmArea.graphs).Count -eq 4) "AreaSetImmersionFarmH3CountInvalid"

$wiById = @{}
foreach ($wi in $wiCatalog.items) { $wiById[[string] $wi.id] = $wi }
$evidenceById = @{}
$evidenceSnapshots = @()
foreach ($evidence in @($profile.evidenceSnapshots | Sort-Object evidenceSnapshotStableId)) {
    $sourcePath = Join-Path $repositoryRoot ([string] $evidence.sourceReferencePath)
    Require (Test-Path -LiteralPath $sourcePath) "AreaSetImmersionEvidenceSourceMissing:$($evidence.evidenceSnapshotStableId)"
    $snapshot = [ordered]@{
        evidenceSnapshotStableId = [string] $evidence.evidenceSnapshotStableId
        evidenceKindCode = [string] $evidence.evidenceKindCode
        sourceStableId = [string] $evidence.sourceStableId
        sourceRevision = [string] $evidence.sourceRevision
        sourceHashSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $sourcePath).Hash.ToLowerInvariant()
        linkStatusCode = [string] $evidence.linkStatusCode
        limitationCodes = @($evidence.limitationCodes | Sort-Object)
    }
    $evidenceById[$snapshot.evidenceSnapshotStableId] = $snapshot
    $evidenceSnapshots += $snapshot
}

$h3Audits = @()
foreach ($audit in @($profile.h3Audits | Sort-Object priority, h3StableId)) {
    $h3Plan = @($theory.h3Plans | Where-Object { $_.h3StableId -eq $audit.h3StableId })
    $graph = @($farmArea.graphs | Where-Object { $_.landscapeGraphStableId -eq $audit.landscapeGraphStableId })
    $definitionGraph = @($farmArea.definition.landscapeGraphs | Where-Object { $_.landscapeGraphStableId -eq $audit.landscapeGraphStableId })
    Require ($h3Plan.Count -eq 1) "AreaSetImmersionH3PlanMissing:$($audit.h3StableId)"
    Require ($graph.Count -eq 1 -and $definitionGraph.Count -eq 1) "AreaSetImmersionGraphMissing:$($audit.h3StableId)"
    $h3Plan = $h3Plan[0]
    $graph = $graph[0]
    $definitionGraph = $definitionGraph[0]
    Require ($h3Plan.theoryStateCode -eq "TheoryQualified" -and $h3Plan.closureStateCode -eq "Closed") "AreaSetImmersionH3TheoryUnqualified:$($audit.h3StableId)"
    Require ($graph.statusCode -eq "Available" -and @($graph.unresolved).Count -eq 0) "AreaSetImmersionH3GraphUnresolved:$($audit.h3StableId)"

    $h2Refs = @($h3Plan.nodes | ForEach-Object { [string] $_.h2Ref } | Sort-Object -Unique)
    $h1Refs = @()
    foreach ($h2Ref in $h2Refs) {
        $h2 = @($theory.h2Plans | Where-Object { $_.h2StableId -eq $h2Ref })
        Require ($h2.Count -eq 1) "AreaSetImmersionH2PlanMissing:$h2Ref"
        $h1Refs += @($h2[0].nodes | ForEach-Object { [string] $_.h1Ref })
    }
    $h1Refs = @($h1Refs | Sort-Object -Unique)
    foreach ($wiId in @($audit.worldInteractionIds)) {
        Require $wiById.ContainsKey([string] $wiId) "AreaSetImmersionWiMissing:$wiId"
    }

    $questions = @()
    $questionBlocked = $false
    foreach ($question in @($audit.questions | Sort-Object questionStableId)) {
        $evidenceIds = @($question.evidenceSnapshotIds | Sort-Object -Unique)
        $missingEvidence = @($evidenceIds | Where-Object { -not $evidenceById.ContainsKey([string] $_) })
        $passed = $missingEvidence.Count -eq 0 -and -not [string]::IsNullOrWhiteSpace([string] $question.answerSummary)
        if ([bool] $question.requiredForQualification -and -not $passed) { $questionBlocked = $true }
        $limitations = @()
        foreach ($evidenceId in $evidenceIds) { $limitations += @($evidenceById[[string] $evidenceId].limitationCodes) }
        $questions += [ordered]@{
            questionStableId = [string] $question.questionStableId
            questionTypeCode = [string] $question.questionTypeCode
            questionText = [string] $question.questionText
            requiredForQualification = [bool] $question.requiredForQualification
            qualificationResultCode = $(if ($passed) { "Pass" } else { "Unresolved" })
            answerSummary = [string] $question.answerSummary
            evidenceSnapshotIds = $evidenceIds
            limitationCodes = @($limitations | Sort-Object -Unique)
        }
    }

    $blocking = @()
    if ($questionBlocked) { $blocking += "RequiredImmersionQuestionUnresolved" }
    $maturity = if ($blocking.Count -eq 0) { "ImmersionQualified" } else { "ContextEvidenceBound" }
    $h3Audits += [ordered]@{
        h3StableId = [string] $audit.h3StableId
        landscapeGraphStableId = [string] $audit.landscapeGraphStableId
        graphRevision = [int] $graph.graphRevision
        graphHashSha256 = [string] $graph.graphHashSha256
        immersionMaturityCode = $maturity
        freshnessStateCode = "Current"
        h2StableIds = $h2Refs
        h1StableIds = $h1Refs
        worldInteractionIds = @($audit.worldInteractionIds | Sort-Object -Unique)
        questions = $questions
        blockingReasonCodes = $blocking
    }
}

$closures = @()
foreach ($closure in @($profile.crossH3Closures | Sort-Object closureStableId)) {
    $fromAudit = @($profile.h3Audits | Where-Object h3StableId -eq $closure.fromH3StableId)
    $toAudit = @($profile.h3Audits | Where-Object h3StableId -eq $closure.toH3StableId)
    Require ($fromAudit.Count -eq 1 -and $toAudit.Count -eq 1) "AreaSetImmersionClosureH3Missing:$($closure.closureStableId)"
    $missingWi = @($closure.worldInteractionIds | Where-Object { -not $wiById.ContainsKey([string] $_) })
    $pathClosed = Get-GraphReachable ([string] $fromAudit[0].landscapeGraphStableId) ([string] $toAudit[0].landscapeGraphStableId) @($farmArea.definition.graphRelations)
    $blocking = @()
    if (-not $pathClosed) { $blocking += "CrossH3SpatialPathUnresolved" }
    if ($missingWi.Count -gt 0) { $blocking += "CrossH3WorldInteractionMissing" }
    $closures += [ordered]@{
        closureStableId = [string] $closure.closureStableId
        fromH3StableId = [string] $closure.fromH3StableId
        toH3StableId = [string] $closure.toH3StableId
        worldInteractionIds = @($closure.worldInteractionIds | Sort-Object -Unique)
        inputSemanticCode = [string] $closure.inputSemanticCode
        outputSemanticCode = [string] $closure.outputSemanticCode
        qualificationResultCode = $(if ($blocking.Count -eq 0) { "Pass" } else { "Unresolved" })
        blockingReasonCodes = $blocking
    }
}

$inputFiles = @($profilePath, $actualE5Path, $theoryPath, $h5Path, $wiPath)
$inputSignature = ($inputFiles | ForEach-Object {
    "$(Resolve-Path $_)|$((Get-FileHash -Algorithm SHA256 -LiteralPath $_).Hash.ToLowerInvariant())"
}) -join "`n"
$inputHash = Get-TextHash $inputSignature
$blockingReasons = @()
if (@($h3Audits | Where-Object immersionMaturityCode -ne "ImmersionQualified").Count -gt 0) { $blockingReasons += "H3ImmersionUnqualified" }
if (@($closures | Where-Object qualificationResultCode -ne "Pass").Count -gt 0) { $blockingReasons += "CrossH3ClosureUnqualified" }
$overallMaturity = if ($blockingReasons.Count -eq 0) { "ImmersionQualified" } else { "ContextEvidenceBound" }
$freshness = "Current"
$e7Gate = if ($overallMaturity -eq "ImmersionQualified" -and $freshness -eq "Current") { "Open" } else { "Closed" }

$outputWithoutHash = [ordered]@{
    schemaVersion = "simulation-area-set-immersion-readiness.v1"
    areaSetStableId = [string] $profile.areaSetStableId
    areaSetRevision = [int] $farmArea.definition.revision
    areaSetHashSha256 = [string] $farmArea.definition.definitionHashSha256
    spatialMaturityCode = [string] $profile.spatialMaturityCode
    immersionMaturityCode = $overallMaturity
    freshnessStateCode = $freshness
    groundingStatusCode = [string] $h5.worldGroundingBinding.worldGroundingStateCode
    e7GatePolicyCode = [string] $profile.e7GatePolicyCode
    e7GateStateCode = $e7Gate
    immersionPolicyRevision = [string] $profile.immersionPolicyRevision
    questionMatrixRevision = [string] $profile.questionMatrixRevision
    generatorVersion = [string] $profile.generatorVersion
    inputHashSha256 = $inputHash
    h3Audits = $h3Audits
    crossH3Closures = $closures
    evidenceSnapshots = $evidenceSnapshots
    blockingReasonCodes = $blockingReasons
    publicDataChangesSimulationRules = [bool] $profile.authorityBoundary.publicDataChangesSimulationRules
    publicDataMovesSpatialDefinitions = [bool] $profile.authorityBoundary.publicDataMovesSpatialDefinitions
    runtimeValidated = [bool] $profile.authorityBoundary.runtimeValidated
}
$qualificationHash = Get-TextHash (Get-StableJson $outputWithoutHash)
$output = [ordered]@{}
foreach ($property in $outputWithoutHash.GetEnumerator()) { $output[$property.Key] = $property.Value }
$output["qualificationHashSha256"] = $qualificationHash
$json = (Get-StableJson $output) + "`n"

$lines = @(
    "# Farm AreaSet E6 정밀 몰입 판정",
    "",
    "- AreaSet: $($profile.areaSetStableId)",
    "- 공간 성숙도: $($profile.spatialMaturityCode)",
    "- 몰입 성숙도: $overallMaturity",
    "- 최신성: $freshness",
    "- GIS 결속: $($h5.worldGroundingBinding.worldGroundingStateCode)",
    "- E7 시작 관문: $e7Gate",
    "- 판정 해시: $qualificationHash",
    "",
    "## H3 정밀 조사",
    ""
)
foreach ($audit in $h3Audits) {
    $questionCount = @($audit.questions).Count
    $h2Count = @($audit.h2StableIds).Count
    $h1Count = @($audit.h1StableIds).Count
    $lines += "- $($audit.h3StableId) - $($audit.immersionMaturityCode), questions $questionCount, H2 $h2Count, H1 $h1Count"
}
$lines += @("", "## AreaSet 교차 H3 폐루프", "")
foreach ($closure in $closures) {
    $lines += "- $($closure.closureStableId) - $($closure.qualificationResultCode), $($closure.inputSemanticCode) to $($closure.outputSemanticCode)"
}
$lines += @(
    "",
    "## 권위 경계",
    "",
    "공공자료는 장소와 작업의 현실 문맥을 설명하는 근거다. 이 판정은 H5 좌표를 이동하거나 생산량·수익성·Simulation 규칙을 자동 변경하지 않는다. 라이브 Provider 호출, Unity Play Mode, Game View와 실제 E7 완료는 수행하지 않았다.",
    ""
)
$markdown = $lines -join "`n"

if ($Mode -eq "Write") {
    $outputDirectory = Split-Path -Parent $outputPath
    if (-not (Test-Path $outputDirectory)) { New-Item -ItemType Directory -Path $outputDirectory | Out-Null }
    $markdownDirectory = Split-Path -Parent $markdownPath
    if (-not (Test-Path $markdownDirectory)) { New-Item -ItemType Directory -Path $markdownDirectory | Out-Null }
    [IO.File]::WriteAllText($outputPath, $json, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($markdownPath, $markdown + "`n", [Text.UTF8Encoding]::new($false))
}
else {
    Require (Test-Path $outputPath) "AreaSetImmersionOutputMissing"
    Require (Test-Path $markdownPath) "AreaSetImmersionMarkdownMissing"
    Require ((Get-Content $outputPath -Raw -Encoding UTF8) -eq $json) "AreaSetImmersionOutputStale"
    Require ((Get-Content $markdownPath -Raw -Encoding UTF8) -eq ($markdown + "`n")) "AreaSetImmersionMarkdownStale"
}

Write-Output "AreaSetImmersionValid:H3=$($h3Audits.Count);Closures=$($closures.Count);E7Gate=$e7Gate;Hash=$qualificationHash"
