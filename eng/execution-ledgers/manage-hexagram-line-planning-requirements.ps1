param(
    [ValidateSet('Validate', 'Write', 'Check')]
    [string] $Mode = 'Validate',
    [string] $InputPath = 'eng/execution-ledgers/hexagram-line-planning-requirements.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path | Split-Path -Parent

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramLinePlanningRequirementsInvalid:$Code" }
}

function Require-Text($Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Resolve-RepositoryFile([string] $Path, [string] $Code) {
    Require-Text $Path $Code
    $candidate = if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $repositoryRoot $Path }
    Require (Test-Path -LiteralPath $candidate -PathType Leaf) "$Code`:Missing:$Path"
    return (Resolve-Path -LiteralPath $candidate).Path
}

function Resolve-RepositoryDirectory([string] $Path, [string] $Code) {
    Require-Text $Path $Code
    $candidate = Join-Path $repositoryRoot $Path
    Require (Test-Path -LiteralPath $candidate -PathType Container) "$Code`:Missing:$Path"
    return (Resolve-Path -LiteralPath $candidate).Path
}

function Normalize-Text([string] $Text) {
    return (($Text -replace "`r`n", "`n").TrimEnd() + "`n")
}

function Get-Sha256([string] $Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Get-FileSnapshot([string] $RepositoryPath) {
    $resolved = Resolve-RepositoryFile $RepositoryPath "Snapshot:$RepositoryPath"
    return [ordered]@{ path = $RepositoryPath; sha256 = Get-Sha256 $resolved }
}

function Escape-Cell($Value) {
    return ([string] $Value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", '<br>')
}

$resolvedInput = Resolve-RepositoryFile $InputPath 'Input'
$source = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $source.schemaVersion -eq 'mirror-hexagram-line-planning-requirements.v1') 'SchemaVersion'
Require ([string] $source.revision -eq 'hexagram-line-planning-requirements.r6') 'Revision'
Require ([bool] $source.principles.allLinesHaveStablePlanIds) 'AllLinesHaveStablePlanIds'
Require ([bool] $source.principles.onlyStudiedLinesRequireDocuments) 'OnlyStudiedLinesRequireDocuments'
Require ([bool] $source.principles.atMostOneActiveStudy) 'AtMostOneActiveStudy'
Require ([bool] $source.principles.storyApprovalAndDevelopmentReadinessAreSeparate) 'StoryAndDevelopmentBoundary'
Require ([bool] $source.principles.linePlanDeclaresButDoesNotCreateWorldInteractionsOrH) 'DeclarationBoundary'
Require ([bool] $source.principles.developmentGoalOwnsExactlyOneWorldInteraction) 'GoalWorldInteractionBoundary'
Require ([bool] $source.principles.higherHRequiresCompositionEvidence) 'HigherHCompositionBoundary'
Require (-not [bool] $source.principles.automaticGraphMapUnityOrEvidencePromotion) 'AutomaticPromotionBoundary'

$parentPath = Resolve-RepositoryFile ([string] $source.parentProductionLedgerPath) 'ParentProductionLedger'
$subjectPath = Resolve-RepositoryFile ([string] $source.subjectCatalogPath) 'SubjectCatalog'
$worldInteractionPath = Resolve-RepositoryFile ([string] $source.worldInteractionCatalogPath) 'WorldInteractionCatalog'
$hRoot = Resolve-RepositoryDirectory ([string] $source.hDefinitionRootPath) 'HDefinitionRoot'
$parent = Get-Content -LiteralPath $parentPath -Raw -Encoding UTF8 | ConvertFrom-Json
$subjectCatalog = Get-Content -LiteralPath $subjectPath -Raw -Encoding UTF8 | ConvertFrom-Json
$worldInteractionCatalog = Get-Content -LiteralPath $worldInteractionPath -Raw -Encoding UTF8 | ConvertFrom-Json

$allParentLines = @($parent.hexagrams | ForEach-Object { @($_.lineStories) })
$allLinePlanIds = @($allParentLines | ForEach-Object { [string] $_.linePlanId })
Require ($allLinePlanIds.Count -eq 384) "ParentLinePlanCount:$($allLinePlanIds.Count)"
Require (@($allLinePlanIds | Sort-Object -Unique).Count -eq 384) 'ParentLinePlanDuplicate'

$subjectIds = @($subjectCatalog.items | ForEach-Object { [string] $_.subjectStableId })
$worldInteractionIds = @($worldInteractionCatalog.items | ForEach-Object { [string] $_.id })
$hIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($file in Get-ChildItem -LiteralPath $hRoot -Filter '*.json' -File -Recurse) {
    $definition = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
    if (-not [string]::IsNullOrWhiteSpace([string] $definition.stableId)) {
        [void] $hIds.Add([string] $definition.stableId)
    }
}

$allowed = $source.allowedCodes
$planningCodes = @($allowed.planningStatusCodes | ForEach-Object { [string] $_ })
$requirementStateCodes = @($allowed.requirementStateCodes | ForEach-Object { [string] $_ })
$handoffCodes = @($allowed.handoffStateCodes | ForEach-Object { [string] $_ })
$necessityCodes = @($allowed.necessityCodes | ForEach-Object { [string] $_ })
$resolutionCodes = @($allowed.resolutionCodes | ForEach-Object { [string] $_ })
$fitCodes = @($allowed.originalFitCodes | ForEach-Object { [string] $_ })
$hLevelCodes = @($allowed.hLevelCodes | ForEach-Object { [string] $_ })
$graphImpactCodes = @($allowed.graphMapImpactCodes | ForEach-Object { [string] $_ })
$placementCodes = @($allowed.placementRequirementCodes | ForEach-Object { [string] $_ })

$items = @($source.items)
Require ($items.Count -eq 6) "RequirementItemCount:$($items.Count)"
Require (@($items.linePlanId | Sort-Object -Unique).Count -eq 6) 'LinePlanIdDuplicate'
Require (@($items.hexagramLineStableId | Sort-Object -Unique).Count -eq 6) 'HexagramLineDuplicate'
Require (@($items | Where-Object planningStatusCode -eq 'ActiveStudy').Count -le 1) 'ActiveStudyCount'

for ($index = 0; $index -lt $items.Count; $index++) {
    $item = $items[$index]
    $ordinal = $index + 1
    $expectedLinePlanId = 'PLAN-STORY-HEX03-LINE-{0:D3}' -f $ordinal
    $expectedLineId = 'HEX-03-ZHUN-L{0}' -f $ordinal
    Require ([string] $item.linePlanId -eq $expectedLinePlanId) "LinePlanSequence:$expectedLinePlanId"
    Require ([string] $item.hexagramLineStableId -eq $expectedLineId) "HexagramLineSequence:$expectedLineId"
    Require ($allLinePlanIds -contains [string] $item.linePlanId) "LinePlanUnknown:$($item.linePlanId)"
    $parentLine = @($allParentLines | Where-Object { [string] $_.stableId -eq [string] $item.hexagramLineStableId })
    Require ($parentLine.Count -eq 1) "ParentLineUnknown:$($item.hexagramLineStableId)"
    Require ([string] $parentLine[0].linePlanId -eq [string] $item.linePlanId) "ParentLinePlanMismatch:$($item.linePlanId)"
    Require ([string] $item.planningStatusCode -in $planningCodes) "PlanningStatus:$($item.linePlanId)"
    Require ([string] $item.requirementStateCode -in $requirementStateCodes) "RequirementState:$($item.linePlanId)"
    Require ([string] $item.handoffStateCode -in $handoffCodes) "HandoffState:$($item.linePlanId)"
    Require ([string] $item.originalFitCode -in $fitCodes) "OriginalFit:$($item.linePlanId)"
    Require-Text $item.originalText "OriginalText:$($item.linePlanId)"
    Require ([string] $item.originalSourceId -eq 'wikisource-zhouyi-traditional') "OriginalSource:$($item.linePlanId)"
    Require-Text $item.originalSourceLocator "OriginalSourceLocator:$($item.linePlanId)"
    Require-Text $item.primaryStoryBeatStableId "PrimaryStoryBeat:$($item.linePlanId)"
    Require ([string] $item.graphMapImpactCode -in $graphImpactCodes) "GraphImpact:$($item.linePlanId)"
    Require ([string] $item.placementMapRequirementCode -in $placementCodes) "PlacementRequirement:$($item.linePlanId)"

    if ([string] $item.planningStatusCode -eq 'Seeded') {
        Require ([string]::IsNullOrWhiteSpace([string] $item.documentRef)) "SeededDocumentRef:$($item.linePlanId)"
        Require ([string]::IsNullOrWhiteSpace([string] $item.documentRevisionCode)) "SeededDocumentRevision:$($item.linePlanId)"
        Require ([string]::IsNullOrWhiteSpace([string] $item.documentExpectedSha256)) "SeededDocumentHash:$($item.linePlanId)"
    } else {
        $documentPath = Resolve-RepositoryFile ([string] $item.documentRef) "LineDocument:$($item.linePlanId)"
        Require-Text $item.documentRevisionCode "LineDocumentRevision:$($item.linePlanId)"
        Require ([string] $item.documentExpectedSha256 -match '^[0-9A-F]{64}$') "LineDocumentHashFormat:$($item.linePlanId)"
        Require ((Get-Sha256 $documentPath) -eq [string] $item.documentExpectedSha256) "LineDocumentHashMismatch:$($item.linePlanId)"
    }

    foreach ($requirement in @($item.subjectRequirements)) {
        $code = "Subject:$($item.linePlanId):$($requirement.roleCode)"
        Require-Text $requirement.roleCode "$code`:Role"
        Require ([string] $requirement.necessityCode -in $necessityCodes) "$code`:Necessity"
        Require ([string] $requirement.resolutionCode -in $resolutionCodes) "$code`:Resolution"
        if ([string] $requirement.resolutionCode -eq 'ExistingReused') {
            Require ($subjectIds -contains [string] $requirement.targetRef) "$code`:UnknownExisting:$($requirement.targetRef)"
        }
        if ([string] $requirement.resolutionCode -eq 'NewDefinitionRequired') {
            Require ([string]::IsNullOrWhiteSpace([string] $requirement.targetRef)) "$code`:NewDefinitionHasTarget"
        }
    }

    foreach ($requirement in @($item.worldInteractionRequirements)) {
        $code = "WorldInteraction:$($item.linePlanId):$($requirement.roleCode)"
        Require-Text $requirement.roleCode "$code`:Role"
        Require ([string] $requirement.necessityCode -in $necessityCodes) "$code`:Necessity"
        Require ([string] $requirement.resolutionCode -in $resolutionCodes) "$code`:Resolution"
        if ([string] $requirement.resolutionCode -in @('ExistingReused', 'CandidateNeedsReview')) {
            Require ($worldInteractionIds -contains [string] $requirement.targetRef) "$code`:UnknownCandidate:$($requirement.targetRef)"
        }
        if ([string] $requirement.resolutionCode -eq 'NewDefinitionRequired') {
            Require ([string]::IsNullOrWhiteSpace([string] $requirement.targetRef)) "$code`:NewDefinitionHasTarget"
        }
    }

    $levels = @($item.hRequirements | ForEach-Object { [string] $_.levelCode } | Sort-Object -Unique)
    foreach ($requiredLevel in $hLevelCodes) {
        Require ($levels -contains $requiredLevel) "HLevelMissing:$($item.linePlanId):$requiredLevel"
    }
    foreach ($requirement in @($item.hRequirements)) {
        $code = "H:$($item.linePlanId):$($requirement.levelCode):$($requirement.roleCode)"
        Require ([string] $requirement.levelCode -in $hLevelCodes) "$code`:Level"
        Require-Text $requirement.roleCode "$code`:Role"
        Require ([string] $requirement.necessityCode -in $necessityCodes) "$code`:Necessity"
        Require ([string] $requirement.resolutionCode -in $resolutionCodes) "$code`:Resolution"
        if ([string] $requirement.resolutionCode -eq 'CandidateNeedsReview') {
            Require ($hIds.Contains([string] $requirement.targetRef)) "$code`:UnknownCandidate:$($requirement.targetRef)"
        }
        if ([string] $requirement.resolutionCode -eq 'NewDefinitionRequired') {
            Require ([string]::IsNullOrWhiteSpace([string] $requirement.targetRef)) "$code`:NewDefinitionHasTarget"
        }
        if ([string] $requirement.necessityCode -eq 'NotApplicable') {
            Require ([string] $requirement.resolutionCode -eq 'NotApplicable') "$code`:NotApplicableResolution"
        }
        if ([string] $requirement.levelCode -in @('H2', 'H3', 'H4') -and [string] $requirement.necessityCode -ne 'NotApplicable') {
            Require (@($requirement.compositionEvidenceRefs).Count -gt 0) "$code`:CompositionEvidenceMissing"
        }
    }

    if ([string] $item.handoffStateCode -eq 'ReadyForDevelopment') {
        Require ([string] $item.planningStatusCode -eq 'StoryApproved') "DevelopmentReadyWithoutStoryApproval:$($item.linePlanId)"
        Require ([string] $item.requirementStateCode -eq 'Resolved') "DevelopmentReadyWithUnresolvedRequirements:$($item.linePlanId)"
        $requiredUnresolved = @(
            @($item.subjectRequirements) + @($item.worldInteractionRequirements) + @($item.hRequirements) |
                Where-Object { [string] $_.necessityCode -eq 'Required' -and [string] $_.resolutionCode -ne 'ExistingReused' }
        )
        Require ($requiredUnresolved.Count -eq 0) "DevelopmentReadyRequiredUnresolved:$($item.linePlanId)"
    }
}

$documentSnapshots = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_.documentRef) } | ForEach-Object { Get-FileSnapshot ([string] $_.documentRef) })
$result = [ordered]@{
    schemaVersion = 'mirror-hexagram-line-planning-requirements-index.v1'
    revision = [string] $source.revision
    sourceSnapshots = @(
        (Get-FileSnapshot $InputPath),
        (Get-FileSnapshot ([string] $source.parentProductionLedgerPath)),
        (Get-FileSnapshot ([string] $source.subjectCatalogPath)),
        (Get-FileSnapshot ([string] $source.worldInteractionCatalogPath))
    ) + $documentSnapshots
    counts = [ordered]@{
        registeredLinePlanIds = $allLinePlanIds.Count
        openedRequirementPlans = $items.Count
        activeStudy = @($items | Where-Object planningStatusCode -eq 'ActiveStudy').Count
        seeded = @($items | Where-Object planningStatusCode -eq 'Seeded').Count
        readyForDevelopment = @($items | Where-Object handoffStateCode -eq 'ReadyForDevelopment').Count
    }
    principles = $source.principles
    items = $items
}

$jsonText = Normalize-Text (($result | ConvertTo-Json -Depth 30) + "`n")
$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine('# 효사별 기획·WI·H 요구사항 색인')
[void] $builder.AppendLine()
[void] $builder.AppendLine('> 이 문서는 `hexagram-line-planning-requirements.json`에서 자동 생성된다. 직접 수정하지 않는다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 등록 효사 기획 ID: ``$($result.counts.registeredLinePlanIds)``")
[void] $builder.AppendLine("- 요구사항을 연 효사: ``$($result.counts.openedRequirementPlans)`` / 현재 문답: ``$($result.counts.activeStudy)`` / Seeded: ``$($result.counts.seeded)``")
[void] $builder.AppendLine("- 개발 인계 가능: ``$($result.counts.readyForDevelopment)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine('효사 기획은 필요한 주체·WI·H를 선언하지만 자동 생성하지 않는다. 이야기 승인과 개발 준비를 분리하며, 개발 Goal 하나는 WI 하나만 소유한다.')
[void] $builder.AppendLine()
[void] $builder.AppendLine('| 효사 기획 | 효 | 기획 | 요구사항 | 인계 | 원문 적합 | Graph Map | 배치 맵 | 공백 |')
[void] $builder.AppendLine('| --- | --- | --- | --- | --- | --- | --- | --- | --- |')
foreach ($item in $items) {
    [void] $builder.AppendLine("| ``$($item.linePlanId)`` | ``$($item.hexagramLineStableId)`` | ``$($item.planningStatusCode)`` | ``$($item.requirementStateCode)`` | ``$($item.handoffStateCode)`` | ``$($item.originalFitCode)`` | ``$($item.graphMapImpactCode)`` | ``$($item.placementMapRequirementCode)`` | $(Escape-Cell (@($item.openGaps) -join ', ')) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine('## 요구사항 요약')
foreach ($item in $items) {
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("### ``$($item.linePlanId)``")
    [void] $builder.AppendLine()
    [void] $builder.AppendLine("- Story Beat: ``$($item.primaryStoryBeatStableId)``")
    [void] $builder.AppendLine("- 주체: $(@($item.subjectRequirements | ForEach-Object { "``$($_.roleCode)``=$($_.necessityCode)/$($_.resolutionCode)" }) -join ', ')")
    [void] $builder.AppendLine("- WI: $(@($item.worldInteractionRequirements | ForEach-Object { "``$($_.roleCode)``=$($_.necessityCode)/$($_.resolutionCode)" }) -join ', ')")
    [void] $builder.AppendLine("- H: $(@($item.hRequirements | ForEach-Object { "``$($_.levelCode):$($_.roleCode)``=$($_.necessityCode)/$($_.resolutionCode)" }) -join ', ')")
}
$markdownText = Normalize-Text $builder.ToString()

$jsonOutput = Join-Path $repositoryRoot ([string] $source.outputJsonPath)
$markdownOutput = Join-Path $repositoryRoot ([string] $source.outputMarkdownPath)

if ($Mode -eq 'Write') {
    [IO.Directory]::CreateDirectory((Split-Path -Parent $jsonOutput)) | Out-Null
    [IO.File]::WriteAllText($jsonOutput, $jsonText, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($markdownOutput, $markdownText, [Text.UTF8Encoding]::new($false))
}

if ($Mode -eq 'Check') {
    Require (Test-Path -LiteralPath $jsonOutput -PathType Leaf) 'JsonOutputMissing'
    Require (Test-Path -LiteralPath $markdownOutput -PathType Leaf) 'MarkdownOutputMissing'
    Require ((Normalize-Text (Get-Content -LiteralPath $jsonOutput -Raw -Encoding UTF8)) -eq $jsonText) 'JsonOutputStale'
    Require ((Normalize-Text (Get-Content -LiteralPath $markdownOutput -Raw -Encoding UTF8)) -eq $markdownText) 'MarkdownOutputStale'
}

Write-Output "HexagramLinePlanningRequirements:$Mode`:OK:Registered=$($result.counts.registeredLinePlanIds):Opened=$($result.counts.openedRequirementPlans):Active=$($result.counts.activeStudy)"
