# UTF-8 BOM is intentional: Windows PowerShell 5.1 must decode Korean text.
[CmdletBinding()]
param(
    [ValidateSet("Check", "Write")]
    [string] $Mode = "Check",
    [string] $LedgerPath = "eng/world-seedbeds/synty-bottom-up-inventory/area-set-composition-patterns.v1.json",
    [string] $TheoryPath = "eng/world-seedbeds/generated/theory-spatial-factory.v1.json",
    [string] $WorldInteractionPath = "eng/execution-ledgers/world-interactions.json",
    [string] $JsonOutputPath = "eng/world-seedbeds/generated/area-set-composition-plans.v1.json",
    [string] $MarkdownOutputPath = "docs/AI/generated/area-set-composition-plans.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path

function Resolve-RepoPath([string] $relativePath) {
    Join-Path $repositoryRoot ($relativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
}

function Read-Json([string] $relativePath) {
    $path = Resolve-RepoPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) { throw "AreaSetCompositionJsonMissing:$relativePath" }
    Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Require([bool] $condition, [string] $code) {
    if (-not $condition) { throw "AreaSetCompositionInvalid:$code" }
}

function Normalize([string] $value) { (($value -replace "`r`n", "`n").TrimEnd()) + "`n" }
function Stable-Json([object] $value) { (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n") }

function Text-Hash([string] $value) {
    $bytes = [Text.Encoding]::UTF8.GetBytes($value)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try { (($sha256.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join "") }
    finally { $sha256.Dispose() }
}

function File-Hash([string] $relativePath) {
    $path = Resolve-RepoPath $relativePath
    Require (Test-Path -LiteralPath $path) "DocumentMissing:$relativePath"
    Text-Hash ([IO.File]::ReadAllText($path, [Text.Encoding]::UTF8))
}

function Same-Set([object[]] $left, [object[]] $right) {
    (@($left | ForEach-Object { [string] $_ } | Sort-Object -Unique) -join "|") -eq
    (@($right | ForEach-Object { [string] $_ } | Sort-Object -Unique) -join "|")
}

function Layout-Position([string] $code, [double] $spacing) {
    switch ($code) {
        "Center" { return [ordered]@{ x = 0.0; z = 0.0 } }
        "North" { return [ordered]@{ x = 0.0; z = $spacing } }
        "South" { return [ordered]@{ x = 0.0; z = -$spacing } }
        "East" { return [ordered]@{ x = $spacing; z = 0.0 } }
        "West" { return [ordered]@{ x = -$spacing; z = 0.0 } }
        "NorthEast" { return [ordered]@{ x = $spacing; z = $spacing } }
        "NorthWest" { return [ordered]@{ x = -$spacing; z = $spacing } }
        "SouthEast" { return [ordered]@{ x = $spacing; z = -$spacing } }
        "SouthWest" { return [ordered]@{ x = -$spacing; z = -$spacing } }
        default { throw "AreaSetCompositionInvalid:LayoutPositionUnknown:$code" }
    }
}

function Connector([object] $h3, [string] $roleCode) {
    @($h3.exposedConnectors | Where-Object roleCode -eq $roleCode)[0]
}

function Has-Movement([object] $connector, [string] $movementKindCode) {
    $null -ne $connector -and @($connector.movementKindCodes) -contains $movementKindCode
}

function Is-Reachable(
    [string] $start,
    [string] $target,
    [object[]] $connections,
    [string] $movementKindCode) {
    if ($start -eq $target) { return $true }
    $adjacency = @{}
    foreach ($connection in @($connections | Where-Object movementKindCode -eq $movementKindCode)) {
        $from = [string] $connection.fromRoleSlotCode
        $to = [string] $connection.toRoleSlotCode
        if (-not $adjacency.ContainsKey($from)) { $adjacency[$from] = [Collections.Generic.List[string]]::new() }
        $adjacency[$from].Add($to)
        if ([string] $connection.relationDirectionCode -eq "Bidirectional") {
            if (-not $adjacency.ContainsKey($to)) { $adjacency[$to] = [Collections.Generic.List[string]]::new() }
            $adjacency[$to].Add($from)
        }
    }
    $queue = [Collections.Generic.Queue[string]]::new()
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $queue.Enqueue($start); [void] $seen.Add($start)
    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        if (-not $adjacency.ContainsKey($current)) { continue }
        foreach ($next in $adjacency[$current]) {
            if ($next -eq $target) { return $true }
            if ($seen.Add($next)) { $queue.Enqueue($next) }
        }
    }
    $false
}

function Is-Connected([string[]] $slots, [object[]] $connections) {
    if ($slots.Count -eq 0) { return $false }
    $adjacency = @{}; foreach ($slot in $slots) { $adjacency[$slot] = [Collections.Generic.List[string]]::new() }
    foreach ($connection in $connections) {
        $from = [string] $connection.fromRoleSlotCode; $to = [string] $connection.toRoleSlotCode
        $adjacency[$from].Add($to); $adjacency[$to].Add($from)
    }
    $queue = [Collections.Generic.Queue[string]]::new(); $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $queue.Enqueue($slots[0]); [void] $seen.Add($slots[0])
    while ($queue.Count -gt 0) { foreach ($next in $adjacency[$queue.Dequeue()]) { if ($seen.Add($next)) { $queue.Enqueue($next) } } }
    $seen.Count -eq $slots.Count
}

function Write-IfChanged([string] $path, [string] $content) {
    $directory = Split-Path -Parent $path
    if (-not (Test-Path -LiteralPath $directory)) { [void] (New-Item -ItemType Directory -Path $directory) }
    if (Test-Path -LiteralPath $path) {
        if ((Normalize ([IO.File]::ReadAllText($path))) -ceq (Normalize $content)) { return }
    }
    [IO.File]::WriteAllText($path, (Normalize $content), [Text.UTF8Encoding]::new($false))
}

$ledger = Read-Json $LedgerPath
$theory = Read-Json $TheoryPath
$worldInteractions = Read-Json $WorldInteractionPath

Require ([string] $ledger.schemaVersion -eq "simulation-world-area-set-composition-patterns.v1") "LedgerSchema"
Require ([string] $theory.schemaVersion -eq "simulation-world-theory-spatial-factory-output.v1") "TheorySchema"
Require ([bool] $ledger.presentationOnly -and -not [bool] $ledger.isOperationalState) "AuthorityBoundary"
Require (@($ledger.areaRoleDefinitions).Count -eq 4) "AreaRoleCount"
Require (@($ledger.defaultSelections).Count -eq 4) "DefaultSelectionCount"
Require (@($ledger.compositionPatterns).Count -eq 8) "PatternCount"

$h2ById = @{}; foreach ($h2 in @($theory.h2Plans)) { $h2ById[[string] $h2.h2StableId] = $h2 }
$h3ById = @{}; foreach ($h3 in @($theory.h3Plans)) { $h3ById[[string] $h3.h3StableId] = $h3 }
$wiById = @{}; foreach ($wi in @($worldInteractions.items)) { $wiById[[string] $wi.id] = $wi }
$roleByCode = @{}; foreach ($role in @($ledger.areaRoleDefinitions)) { $roleByCode[[string] $role.areaRoleCode] = $role }
$patternById = @{}; foreach ($pattern in @($ledger.compositionPatterns)) {
    $id = [string] $pattern.compositionPatternStableId
    Require (-not $patternById.ContainsKey($id)) "PatternDuplicate:$id"
    $patternById[$id] = $pattern
}

$baselineByRole = @{}
foreach ($selection in @($ledger.defaultSelections)) {
    $roleCode = [string] $selection.areaRoleCode; $patternId = [string] $selection.baselinePatternStableId
    Require ($roleByCode.ContainsKey($roleCode)) "SelectionRoleUnknown:$roleCode"
    Require ($patternById.ContainsKey($patternId)) "SelectionPatternUnknown:$patternId"
    $selected = $patternById[$patternId]
    Require ([string] $selected.patternKindCode -eq "Baseline") "SelectionNotBaseline:$patternId"
    Require ([string] $selected.areaRoleCode -eq $roleCode) "SelectionRoleMismatch:$patternId"
    Require (-not $baselineByRole.ContainsKey($roleCode)) "SelectionRoleDuplicate:$roleCode"
    $baselineByRole[$roleCode] = $patternId
}

$resolvedPatterns = @()
foreach ($pattern in @($ledger.compositionPatterns | Sort-Object compositionPatternStableId)) {
    $patternId = [string] $pattern.compositionPatternStableId
    $roleCode = [string] $pattern.areaRoleCode
    Require ($roleByCode.ContainsKey($roleCode)) "PatternRoleUnknown:$patternId"
    $role = $roleByCode[$roleCode]
    Require ([string] $pattern.worldIntentRef -eq [string] $role.worldIntentRef) "WorldIntentMismatch:$patternId"
    Require ([string] $pattern.patternKindCode -in @("Baseline", "Variant")) "PatternKindInvalid:$patternId"
    if ([string] $pattern.patternKindCode -eq "Baseline") {
        Require ([string]::IsNullOrWhiteSpace([string] $pattern.basePatternStableId)) "BaselineHasBase:$patternId"
    }
    else {
        $baseId = [string] $pattern.basePatternStableId
        Require ($patternById.ContainsKey($baseId)) "VariantBaseUnknown:$patternId"
        Require ([string] $patternById[$baseId].patternKindCode -eq "Baseline") "VariantBaseNotBaseline:$patternId"
        Require ([string] $patternById[$baseId].areaRoleCode -eq $roleCode) "VariantBaseRoleMismatch:$patternId"
    }
    Require ([bool] $pattern.presentationOnly -and -not [bool] $pattern.isOperationalState) "PatternAuthorityInvalid:$patternId"
    Require (@($pattern.unresolvedItems).Count -eq 0) "PatternUnresolved:$patternId"

    $placements = @($pattern.h3Placements)
    $slots = @($placements | ForEach-Object { [string] $_.roleSlotCode })
    Require ($slots.Count -eq @($slots | Sort-Object -Unique).Count) "RoleSlotDuplicate:$patternId"
    Require (Same-Set @($role.requiredRoleSlotCodes) $slots) "RequiredRoleSlotsMismatch:$patternId"
    Require (Is-Connected $slots @($pattern.connections)) "StructureDisconnected:$patternId"

    $resolvedPlacements = @()
    $placementBySlot = @{}
    foreach ($placement in @($placements | Sort-Object placementStableId)) {
        $slot = [string] $placement.roleSlotCode; $h3Ref = [string] $placement.selectedH3PatternRef
        Require ($h3ById.ContainsKey($h3Ref)) "H3Unknown:${patternId}:$h3Ref"
        Require ([string] $h3ById[$h3Ref].theoryStateCode -eq "TheoryQualified") "H3NotQualified:${patternId}:$h3Ref"
        Require (@($ledger.layoutRules.allowedRotationDegrees) -contains [int] $placement.rotationDegrees) "RotationInvalid:${patternId}:$slot"
        Require (@($ledger.layoutRules.allowedScaleVariantCodes) -contains [string] $placement.scaleVariantCode) "ScaleVariantInvalid:${patternId}:$slot"
        $position = Layout-Position ([string] $placement.layoutPositionCode) ([double] $ledger.layoutRules.spacingMeters)
        $h3 = $h3ById[$h3Ref]
        $resolved = [ordered]@{
            placementStableId = [string] $placement.placementStableId
            roleSlotCode = $slot
            selectedH3PatternRef = $h3Ref
            h3PatternCode = [string] $h3.patternCode
            h3DisplayNameKo = [string] $h3.displayNameKo
            childH2Refs = @($h3.nodes | ForEach-Object { [string] $_.h2Ref } | Sort-Object)
            x = [double] $position.x
            z = [double] $position.z
            rotationDegrees = [int] $placement.rotationDegrees
            scaleVariantCode = [string] $placement.scaleVariantCode
            h3TheoryHashSha256 = [string] $h3.theoryHashSha256
        }
        $resolvedPlacements += $resolved
        $placementBySlot[$slot] = [ordered]@{ authored = $placement; resolved = $resolved; h3 = $h3 }
    }

    $resolvedOverrides = @()
    foreach ($override in @($pattern.h2Overrides | Sort-Object targetH3PlacementStableId, h2RoleSlotCode)) {
        $targetPlacementId = [string] $override.targetH3PlacementStableId
        $target = @($resolvedPlacements | Where-Object placementStableId -eq $targetPlacementId)[0]
        Require ($null -ne $target) "H2OverridePlacementUnknown:${patternId}:$targetPlacementId"
        $targetH2Ref = [string] $override.targetH2Ref; $replacementH2Ref = [string] $override.replacementH2PatternRef
        Require (@($target.childH2Refs) -contains $targetH2Ref) "H2OverrideTargetUnknown:${patternId}:$targetH2Ref"
        Require ($h2ById.ContainsKey($replacementH2Ref)) "H2OverrideReplacementUnknown:${patternId}:$replacementH2Ref"
        $requiredRoles = @($override.requiredConnectionRoleCodes | ForEach-Object { [string] $_ })
        $replacementRoles = @($h2ById[$replacementH2Ref].placementContract.connectionRoleCodes | ForEach-Object { [string] $_ })
        Require (@($requiredRoles | Where-Object { $replacementRoles -notcontains $_ }).Count -eq 0) "H2OverrideConnectorMismatch:${patternId}:$replacementH2Ref"
        $resolvedOverrides += [ordered]@{
            targetH3PlacementStableId = $targetPlacementId
            h2RoleSlotCode = [string] $override.h2RoleSlotCode
            targetH2Ref = $targetH2Ref
            replacementH2PatternRef = $replacementH2Ref
            requiredConnectionRoleCodes = @($requiredRoles | Sort-Object)
            replacementH2TheoryHashSha256 = [string] $h2ById[$replacementH2Ref].theoryHashSha256
        }
    }

    $resolvedConnections = @()
    foreach ($connection in @($pattern.connections | Sort-Object connectionStableId)) {
        $fromSlot = [string] $connection.fromRoleSlotCode; $toSlot = [string] $connection.toRoleSlotCode
        Require ($placementBySlot.ContainsKey($fromSlot) -and $placementBySlot.ContainsKey($toSlot)) "ConnectionSlotUnknown:$($connection.connectionStableId)"
        $fromConnector = Connector $placementBySlot[$fromSlot].h3 ([string] $connection.fromConnectorRoleCode)
        $toConnector = Connector $placementBySlot[$toSlot].h3 ([string] $connection.toConnectorRoleCode)
        Require ($null -ne $fromConnector -and $null -ne $toConnector) "ConnectionConnectorUnknown:$($connection.connectionStableId)"
        Require ([string] $fromConnector.directionCode -in @("Output", "Bidirectional")) "ConnectionFromDirectionInvalid:$($connection.connectionStableId)"
        Require ([string] $toConnector.directionCode -in @("Input", "Bidirectional")) "ConnectionToDirectionInvalid:$($connection.connectionStableId)"
        $movement = [string] $connection.movementKindCode
        Require ((Has-Movement $fromConnector $movement) -and (Has-Movement $toConnector $movement)) "ConnectionMovementInvalid:$($connection.connectionStableId)"
        Require ([string] $connection.relationDirectionCode -in @("Directed", "Bidirectional")) "ConnectionDirectionInvalid:$($connection.connectionStableId)"
        $resolvedConnections += [ordered]@{
            connectionStableId = [string] $connection.connectionStableId
            fromRoleSlotCode = $fromSlot
            fromH3PatternRef = [string] $placementBySlot[$fromSlot].resolved.selectedH3PatternRef
            fromConnectorRoleCode = [string] $connection.fromConnectorRoleCode
            fromConnectorStableId = [string] $fromConnector.connectorStableId
            toRoleSlotCode = $toSlot
            toH3PatternRef = [string] $placementBySlot[$toSlot].resolved.selectedH3PatternRef
            toConnectorRoleCode = [string] $connection.toConnectorRoleCode
            toConnectorStableId = [string] $toConnector.connectorStableId
            movementKindCode = $movement
            relationDirectionCode = [string] $connection.relationDirectionCode
            compatibilityRuleCode = [string] $connection.compatibilityRuleCode
        }
    }

    $loop = $pattern.gameplayLoop
    foreach ($slot in @([string] $loop.startRoleSlotCode, [string] $loop.resultRoleSlotCode, [string] $loop.returnRoleSlotCode)) {
        Require ($placementBySlot.ContainsKey($slot)) "GameplayLoopSlotUnknown:${patternId}:$slot"
    }
    Require (Is-Reachable ([string] $loop.startRoleSlotCode) ([string] $loop.resultRoleSlotCode) @($pattern.connections) ([string] $loop.movementKindCode)) "GameplayResultUnreachable:$patternId"
    Require (Is-Reachable ([string] $loop.resultRoleSlotCode) ([string] $loop.returnRoleSlotCode) @($pattern.connections) ([string] $loop.movementKindCode)) "GameplayReturnUnreachable:$patternId"
    foreach ($wiId in @($pattern.relatedWiStableIds)) { Require ($wiById.ContainsKey([string] $wiId)) "WiUnknown:${patternId}:$wiId" }

    $documentHash = File-Hash ([string] $pattern.documentPath)
    $core = [ordered]@{
        compositionPatternStableId = $patternId
        patternCode = [string] $pattern.patternCode
        title = [string] $pattern.title
        areaRoleCode = $roleCode
        worldIntentRef = [string] $pattern.worldIntentRef
        gamePlanCode = [string] $pattern.gamePlanCode
        patternKindCode = [string] $pattern.patternKindCode
        basePatternStableId = [string] $pattern.basePatternStableId
        leadPackCode = [string] $pattern.leadPackCode
        supportPackCodes = @($pattern.supportPackCodes | ForEach-Object { [string] $_ } | Sort-Object)
        documentPath = [string] $pattern.documentPath
        documentHashSha256 = $documentHash
        resolvedH3Placements = @($resolvedPlacements)
        resolvedH2Overrides = @($resolvedOverrides)
        resolvedConnections = @($resolvedConnections)
        gameplayLoop = [ordered]@{
            startRoleSlotCode = [string] $loop.startRoleSlotCode
            resultRoleSlotCode = [string] $loop.resultRoleSlotCode
            returnRoleSlotCode = [string] $loop.returnRoleSlotCode
            movementKindCode = [string] $loop.movementKindCode
        }
        relatedWiStableIds = @($pattern.relatedWiStableIds | ForEach-Object { [string] $_ } | Sort-Object)
        structureQualificationCode = "AreaSetCompositionStructureQualified"
        closureStateCode = "Closed"
        runtimeState = $false
        presentationOnly = $true
    }
    $resolvedPatterns += [ordered]@{
        compositionPatternStableId = $core.compositionPatternStableId
        patternCode = $core.patternCode
        title = $core.title
        areaRoleCode = $core.areaRoleCode
        worldIntentRef = $core.worldIntentRef
        gamePlanCode = $core.gamePlanCode
        patternKindCode = $core.patternKindCode
        basePatternStableId = $core.basePatternStableId
        leadPackCode = $core.leadPackCode
        supportPackCodes = $core.supportPackCodes
        documentPath = $core.documentPath
        documentHashSha256 = $core.documentHashSha256
        resolvedH3Placements = $core.resolvedH3Placements
        resolvedH2Overrides = $core.resolvedH2Overrides
        resolvedConnections = $core.resolvedConnections
        gameplayLoop = $core.gameplayLoop
        relatedWiStableIds = $core.relatedWiStableIds
        structureQualificationCode = $core.structureQualificationCode
        closureStateCode = $core.closureStateCode
        compositionPatternHashSha256 = Text-Hash (Stable-Json $core)
        runtimeState = $false
        presentationOnly = $true
    }
}

$resolvedById = @{}; foreach ($pattern in $resolvedPatterns) { $resolvedById[[string] $pattern.compositionPatternStableId] = $pattern }
$baselineSelections = @($ledger.defaultSelections | Sort-Object areaRoleCode | ForEach-Object {
    $resolved = $resolvedById[[string] $_.baselinePatternStableId]
    [ordered]@{
        areaRoleCode = [string] $_.areaRoleCode
        baselinePatternStableId = [string] $_.baselinePatternStableId
        worldIntentRef = [string] $resolved.worldIntentRef
        compositionPatternHashSha256 = [string] $resolved.compositionPatternHashSha256
        documentHashSha256 = [string] $resolved.documentHashSha256
    }
})

$result = [ordered]@{
    schemaVersion = "simulation-world-area-set-composition-plans.v1"
    revision = "simulation-world-area-set-composition-plans.r1"
    sourceLedgerRevision = [string] $ledger.revision
    theoryFactoryRevision = [string] $theory.revision
    generatedAtRuleCode = "DeterministicNoWallClock"
    counts = [ordered]@{
        areaRoles = @($ledger.areaRoleDefinitions).Count
        baselinePatterns = @($resolvedPatterns | Where-Object patternKindCode -eq "Baseline").Count
        variantPatterns = @($resolvedPatterns | Where-Object patternKindCode -eq "Variant").Count
        totalPatterns = $resolvedPatterns.Count
        resolvedH3Placements = @($resolvedPatterns.resolvedH3Placements).Count
        resolvedConnections = @($resolvedPatterns.resolvedConnections).Count
    }
    baselineSelections = $baselineSelections
    resolvedPatterns = @($resolvedPatterns | Sort-Object compositionPatternStableId)
    authorityBoundary = [ordered]@{
        newHierarchyLevelNotCreated = $true
        patternIsNotRuntimeState = $true
        unityCannotSelectAuthoritatively = $true
        publicDataNotBound = $true
        e6RemainsSeparate = $true
    }
    presentationOnly = $true
    isOperationalState = $false
}
$json = Normalize (Stable-Json $result)

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# 역할 슬롯 기반 AreaSet 구성 패턴")
[void] $builder.AppendLine()
[void] $builder.AppendLine("H4 세계 의도를 기존 H3·H2로 번역하는 조립 설명서의 결정적 생성 결과다. 새 H 계층이나 Runtime 상태가 아니다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 기준안: ``$($result.counts.baselinePatterns)`` · 변형안: ``$($result.counts.variantPatterns)``")
[void] $builder.AppendLine("- 해결된 H3 배치: ``$($result.counts.resolvedH3Placements)`` · 연결: ``$($result.counts.resolvedConnections)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 영역 | 종류 | 구성 패턴 | H3 | 연결 | 구조 상태 |")
[void] $builder.AppendLine("| --- | --- | --- | ---: | ---: | --- |")
foreach ($pattern in @($result.resolvedPatterns | Sort-Object areaRoleCode, patternKindCode, patternCode)) {
    [void] $builder.AppendLine("| $($pattern.areaRoleCode) | $($pattern.patternKindCode) | ``$($pattern.patternCode)`` $($pattern.title) | $(@($pattern.resolvedH3Placements).Count) | $(@($pattern.resolvedConnections).Count) | ``$($pattern.closureStateCode)`` |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("Markdown은 의도와 선택 이유, JSON은 실행 가능한 구성 권위, Unity는 검토·표현을 담당한다. 변형안 전환은 이번 범위의 Simulation 규칙이 아니다.")
$markdown = Normalize $builder.ToString()

$jsonPath = Resolve-RepoPath $JsonOutputPath
$markdownPath = Resolve-RepoPath $MarkdownOutputPath
if ($Mode -eq "Check") {
    Require (Test-Path -LiteralPath $jsonPath) "JsonOutputMissing"
    Require (Test-Path -LiteralPath $markdownPath) "MarkdownOutputMissing"
    Require ((Normalize ([IO.File]::ReadAllText($jsonPath))) -ceq $json) "JsonOutputStale"
    Require ((Normalize ([IO.File]::ReadAllText($markdownPath))) -ceq $markdown) "MarkdownOutputStale"
    Write-Output "AreaSetCompositionPatternsValid:Baseline=$($result.counts.baselinePatterns);Variants=$($result.counts.variantPatterns);H3=$($result.counts.resolvedH3Placements);Closed=True"
    exit 0
}

Write-IfChanged $jsonPath $json
Write-IfChanged $markdownPath $markdown
Write-Output "AreaSetCompositionPatternsGenerated:Baseline=$($result.counts.baselinePatterns);Variants=$($result.counts.variantPatterns);H3=$($result.counts.resolvedH3Placements);Closed=True"
