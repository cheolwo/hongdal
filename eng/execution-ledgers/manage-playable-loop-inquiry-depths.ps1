[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",
    [string] $CatalogPath =
        "eng/execution-ledgers/playable-loop-inquiry-depths.json",
    [string] $OutputPath =
        "docs/AI/generated/playable-loop-inquiry-depths.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PlayableLoopInquiryDepthInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedCatalog = Resolve-Path (Join-Path $repositoryRoot $CatalogPath)
$catalog = Get-Content -LiteralPath $resolvedCatalog -Raw -Encoding UTF8 |
    ConvertFrom-Json

Require ([string] $catalog.schemaVersion -eq
    "playable-loop-inquiry-depth-catalog.v1") "SchemaInvalid"
Require ([string] $catalog.evidenceModelRevision -eq
    "horizontal-dual-cycle-evidence.r3") "EvidenceModelInvalid"
foreach ($principle in @(
        "depthForecastsEvidenceReadiness",
        "depthNeverPromotesEvidenceAutomatically",
        "logicAndPresentationForecastsRemainDistinct",
        "actualEvidenceComesFromLedgersAndEvidencePackages",
        "globalQuestionNumberRemainsImmutable",
        "depthCanReopenEarlierQuestions",
        "questionIdentityRemainsStableAcrossRevisions",
        "questionBranchesPreserveParentAndSupersededLineage")) {
    Require ([bool] $catalog.principles.$principle) "PrincipleMissing:$principle"
}

$questionVersioning = $catalog.questionVersioning
Require ([string] $questionVersioning.schemaVersion -eq
    "playable-loop-question-branch.v1") "QuestionVersioningSchemaInvalid"
Require ([string] $questionVersioning.branchRevisionFormat -eq
    "{questionStableId}.r{positiveInteger}") "QuestionRevisionFormatInvalid"
$requiredQuestionFields = @(
    "questionStableId",
    "displayQuestionId",
    "branchRevision",
    "depthCode",
    "parentRevisionRef",
    "decisionStatusCode",
    "contextDeltaKo",
    "confirmedAnswerKo",
    "openQuestionKo",
    "graphImpactAssessment",
    "placementMapImpactAssessment",
    "supersedesRevisionRefs")
Require ((@($questionVersioning.requiredFields) -join ",") -eq
    ($requiredQuestionFields -join ",")) "QuestionRequiredFieldsInvalid"
$allowedDecisionStatuses = @(
    "Asked", "ConfirmedDirection", "Confirmed", "Deferred", "Superseded")
Require ((@($questionVersioning.allowedDecisionStatusCodes) -join ",") -eq
    ($allowedDecisionStatuses -join ",")) "QuestionDecisionStatusesInvalid"
foreach ($rule in @(
        "oneBranchRevisionContainsOneCoreQuestion",
        "answerCreatesNewRevisionInsteadOfOverwriting",
        "deeperQuestionReferencesParentRevision",
        "depthChangeRequiresNewRevision",
        "sameDepthRefinementMayCreateNewRevision",
        "revisionNeverPromotesEvidenceAutomatically")) {
    Require ([bool] $questionVersioning.rules.$rule) `
        "QuestionVersioningRuleMissing:$rule"
}
$graphImpact = $questionVersioning.graphImpactAssessment
Require ([string] $graphImpact.schemaVersion -eq
    "question-graph-impact-assessment.v1") "GraphImpactSchemaInvalid"
$allowedImpactCodes = @(
    "NoImpact", "UpdateExisting", "CreateSubgraph", "CreateGraphMap", "Blocked")
Require ((@($graphImpact.allowedImpactCodes) -join ",") -eq
    ($allowedImpactCodes -join ",")) "GraphImpactCodesInvalid"
$requiredGraphImpactFields = @(
    "impactCode",
    "primaryNodeRefs",
    "primaryEdgeRefs",
    "adjacentNodeRefs",
    "propagationEdgeRefs",
    "derivedHigherHRefs",
    "stoppingBoundaryRefs",
    "unknownImpactRefs",
    "noImpactReasonKo")
Require ((@($graphImpact.requiredFields) -join ",") -eq
    ($requiredGraphImpactFields -join ",")) "GraphImpactFieldsInvalid"
Require ([int] $graphImpact.defaultTraversalHopLimit -eq 2) `
    "GraphImpactHopLimitInvalid"
foreach ($rule in @(
        "directRingAlwaysAssessed",
        "adjacentRingUsesExplicitEdgesOnly",
        "propagationRingRequiresCausalJustification",
        "higherHChangesAreDerivedFromChildren",
        "unknownImpactNeverBecomesConfirmed",
        "beyondHopLimitCreatesFollowupRevision")) {
    Require ([bool] $graphImpact.rules.$rule) `
        "GraphImpactRuleMissing:$rule"
}
$placementMapImpact = $questionVersioning.placementMapImpactAssessment
Require ([string] $placementMapImpact.schemaVersion -eq
    "question-placement-map-impact-assessment.v1") `
    "PlacementMapImpactSchemaInvalid"
$allowedPlacementMapImpactCodes = @(
    "NoImpact", "UpdateExisting", "CreatePlacementMap", "Blocked")
Require ((@($placementMapImpact.allowedImpactCodes) -join ",") -eq
    ($allowedPlacementMapImpactCodes -join ",")) `
    "PlacementMapImpactCodesInvalid"
$requiredPlacementMapImpactFields = @(
    "impactCode",
    "targetGraphElementRefs",
    "hDefinitionRefs",
    "multiplicityIntentKo",
    "instanceRoleIntentKo",
    "placementConstraintRefs",
    "unknownImpactRefs",
    "noImpactReasonKo")
Require ((@($placementMapImpact.requiredFields) -join ",") -eq
    ($requiredPlacementMapImpactFields -join ",")) `
    "PlacementMapImpactFieldsInvalid"
foreach ($rule in @(
        "graphMeaningRemainsInGraphMap",
        "placementInstancesRemainInPlacementMap",
        "sameHDefinitionMayHaveMultipleInstances",
        "placementEdgesReferenceGraphEdges",
        "relativePlanDoesNotProveWorldPlacement",
        "unityEvidenceRemainsSeparate")) {
    Require ([bool] $placementMapImpact.rules.$rule) `
        "PlacementMapImpactRuleMissing:$rule"
}

$items = @($catalog.items)
Require ($items.Count -eq 5) "DepthCountInvalid"
Require ((@($items.depthCode) -join ",") -eq "D1,D2,D3,D4,D5") `
    "DepthOrderInvalid"
$verticalStages = @("E1", "E2", "E3", "E4", "E5", "E6", "E7")
$horizontalStages = @("E8", "E9")
foreach ($item in $items) {
    Require-Text $item.displayNameKo "DisplayNameMissing:$($item.depthCode)"
    Require-Text $item.questionScopeKo "QuestionScopeMissing:$($item.depthCode)"
    Require-Text $item.requiredEvidenceAfterInquiryKo `
        "RequiredEvidenceMissing:$($item.depthCode)"
    foreach ($stage in @($item.logicReadinessStageCodes) +
        @($item.presentationReadinessStageCodes)) {
        Require ($verticalStages -contains [string] $stage) `
            "VerticalStageInvalid:$($item.depthCode):$stage"
    }
    foreach ($stage in @($item.horizontalCampaignReadinessCodes)) {
        Require ($horizontalStages -contains [string] $stage) `
            "HorizontalStageInvalid:$($item.depthCode):$stage"
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# PlayableLoop Inquiry Depth and Evidence Readiness")
$lines.Add("")
$lines.Add("- Catalog revision: ``$($catalog.revision)``")
$lines.Add("- Evidence model: ``$($catalog.evidenceModelRevision)``")
$lines.Add("- Inquiry depth forecasts implementation readiness and never promotes actual Evidence.")
$lines.Add("")
$lines.Add("## Question branch revision")
$lines.Add("")
$lines.Add("- Schema: ``$($questionVersioning.schemaVersion)``")
$lines.Add("- Revision format: ``$($questionVersioning.branchRevisionFormat)``")
$lines.Add("- One branch revision contains one core question. An answer or a meaningful context/depth change creates a new revision instead of overwriting the previous one.")
$lines.Add("- A deeper question references its parent revision. Same-depth refinement may also create a child revision. Question revision never promotes Evidence automatically.")
$lines.Add("- Decision statuses: ``$(@($questionVersioning.allowedDecisionStatusCodes) -join ' / ')``")
$lines.Add("- Graph impact: assess the direct ring, explicit one-hop neighbors, and causally justified two-hop propagation. Effects beyond two hops become a follow-up question revision.")
$lines.Add("- Higher H changes are derived from child H and edge changes; unknown graph effects remain unknown and never become confirmed automatically.")
$lines.Add("- Placement Map impact: Graph Map keeps gameplay meaning; a separate versioned Placement Map keeps repeated H instances, relative arrangement, and placement constraints. Relative plans never prove Unity World placement.")
$lines.Add("")
$lines.Add("| Depth | Question scope | Logic readiness | Presentation readiness | Horizontal campaign readiness | Evidence still required |")
$lines.Add("| --- | --- | --- | --- | --- | --- |")
foreach ($item in $items) {
    $logic = @($item.logicReadinessStageCodes) -join "~"
    $presentation = @($item.presentationReadinessStageCodes) -join "~"
    $horizontal = if (@($item.horizontalCampaignReadinessCodes).Count -eq 0) {
        "-"
    } else { @($item.horizontalCampaignReadinessCodes) -join "~" }
    $lines.Add(("| ``{0}`` {1} | {2} | ``{3}`` | ``{4}`` | ``{5}`` | {6} |" -f
        $item.depthCode,
        (Escape-Cell $item.displayNameKo),
        (Escape-Cell $item.questionScopeKo),
        $logic,
        $presentation,
        $horizontal,
        (Escape-Cell $item.requiredEvidenceAfterInquiryKo)))
}
$content = ($lines -join "`n") + "`n"
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    Write-DeterministicTextIfChanged -Path $resolvedOutput -Content $content |
        Out-Null
} else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    $actual = Get-Content -LiteralPath $resolvedOutput -Raw -Encoding UTF8
    Require ($actual -eq $content) "GeneratedOutputMismatch"
}

Write-Output ("PlayableLoopInquiryDepthValid:Depths={0};Revision={1}" -f
    $items.Count, $catalog.revision)
