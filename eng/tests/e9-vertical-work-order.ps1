$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot (
    "eng/execution-ledgers/manage-e9-vertical-work-order.ps1")

function Get-RepositoryRelativePath([string] $Path) {
    $rootPrefix = $repositoryRoot.TrimEnd("\") + "\"
    if (-not $Path.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "PathOutsideRepository:$Path"
    }

    return $Path.Substring($rootPrefix.Length).Replace("\", "/")
}

$templateResult = & $manager -InputPath (
    "eng/execution-ledgers/work-orders/e9-vertical-work-order.template.json")
if ([string] $templateResult -notlike "E9VerticalWorkOrderValid:E9-WO-TEMPLATE;*") {
    throw "E9VerticalWorkOrderTemplateValidationFailed:$templateResult"
}

$natureResult = & $manager -InputPath (
    "eng/execution-ledgers/work-orders/nature-survival-solo-placement.e9-work-order.json")
if ([string] $natureResult -ne (
    "E9VerticalWorkOrderValid:E9-WO-NATURE-SURVIVAL-SOLO-PLACEMENT;" +
    "Current=E4;Pass=UpwardAssemblyAndValidation;PromotionEligible=false")) {
    throw "E9VerticalNatureWorkOrderValidationFailed:$natureResult"
}

$soloRuntimeResult = & $manager -InputPath (
    "eng/execution-ledgers/work-orders/solo-first-simulation-runtime.e9-work-order.json")
if ([string] $soloRuntimeResult -ne (
    "E9VerticalWorkOrderValid:E9-WO-SOLO-FIRST-SIMULATION-RUNTIME;" +
    "Current=E3;Pass=UpwardAssemblyAndValidation;PromotionEligible=false")) {
    throw "E9VerticalSoloRuntimeWorkOrderValidationFailed:$soloRuntimeResult"
}

$regionalDevelopmentResult = & $manager -InputPath (
    "eng/execution-ledgers/work-orders/nature-farm-regional-development.e9-work-order.json")
if ([string] $regionalDevelopmentResult -ne (
    "E9VerticalWorkOrderValid:E9-WO-NATURE-FARM-REGIONAL-DEVELOPMENT;" +
    "Current=E2;Pass=UpwardAssemblyAndValidation;PromotionEligible=false")) {
    throw "E9VerticalRegionalDevelopmentWorkOrderValidationFailed:$regionalDevelopmentResult"
}

$hubNpcRoutineResult = & $manager -InputPath (
    "eng/execution-ledgers/work-orders/hub-npc-routine-outbound.e9-work-order.json")
if ([string] $hubNpcRoutineResult -ne (
    "E9VerticalWorkOrderValid:E9-WO-HUB-NPC-ROUTINE-OUTBOUND;" +
    "Current=E4;Pass=StableOrExplicitlyBlocked;PromotionEligible=false")) {
    throw "E9VerticalHubNpcRoutineWorkOrderValidationFailed:$hubNpcRoutineResult"
}

$artifactDirectory = Join-Path $repositoryRoot (
    "artifacts/local/validation/e9-vertical-work-order/negative")
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
$templatePath = Join-Path $repositoryRoot (
    "eng/execution-ledgers/work-orders/e9-vertical-work-order.template.json")

$invalidOrder = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8 |
    ConvertFrom-Json
$invalidOrder.downwardPlan[0].code = "E8"
$invalidOrderPath = Join-Path $artifactDirectory "invalid-stage-order.json"
[IO.File]::WriteAllText($invalidOrderPath,
    ($invalidOrder | ConvertTo-Json -Depth 20), $utf8)
$invalidOrderRelative = Get-RepositoryRelativePath $invalidOrderPath
$orderRejected = $false
try {
    & $manager -InputPath $invalidOrderRelative | Out-Null
}
catch {
    $orderRejected = $_.Exception.Message.Contains("WorkOrderDownwardOrderInvalid")
}
if (-not $orderRejected) { throw "E9VerticalInvalidOrderWasAccepted" }

$invalidPromotion = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8 |
    ConvertFrom-Json
$invalidPromotion.promotionEligible = $true
$invalidPromotionPath = Join-Path $artifactDirectory "invalid-new-slice-promotion.json"
[IO.File]::WriteAllText($invalidPromotionPath,
    ($invalidPromotion | ConvertTo-Json -Depth 20), $utf8)
$invalidPromotionRelative = Get-RepositoryRelativePath $invalidPromotionPath
$promotionRejected = $false
try {
    & $manager -InputPath $invalidPromotionRelative | Out-Null
}
catch {
    $promotionRejected = $_.Exception.Message.Contains(
        "NewSliceCannotBeImmediatelyPromotionEligible")
}
if (-not $promotionRejected) { throw "E9VerticalInvalidPromotionWasAccepted" }

$invalidCurrentPass = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8 |
    ConvertFrom-Json
$invalidCurrentPass.iterationState.currentPass = "OnePassComplete"
$invalidCurrentPassPath = Join-Path $artifactDirectory "invalid-current-pass.json"
[IO.File]::WriteAllText($invalidCurrentPassPath,
    ($invalidCurrentPass | ConvertTo-Json -Depth 20), $utf8)
$invalidCurrentPassRelative = Get-RepositoryRelativePath $invalidCurrentPassPath
$currentPassRejected = $false
try {
    & $manager -InputPath $invalidCurrentPassRelative | Out-Null
}
catch {
    $currentPassRejected = $_.Exception.Message.Contains("WorkOrderCurrentPassInvalid")
}
if (-not $currentPassRejected) { throw "E9VerticalInvalidCurrentPassWasAccepted" }

$missingReopenCondition = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8 |
    ConvertFrom-Json
$missingReopenCondition.iterationState.nextReopenCondition = ""
$missingReopenConditionPath = Join-Path $artifactDirectory "missing-reopen-condition.json"
[IO.File]::WriteAllText($missingReopenConditionPath,
    ($missingReopenCondition | ConvertTo-Json -Depth 20), $utf8)
$missingReopenConditionRelative = Get-RepositoryRelativePath $missingReopenConditionPath
$reopenConditionRejected = $false
try {
    & $manager -InputPath $missingReopenConditionRelative | Out-Null
}
catch {
    $reopenConditionRejected = $_.Exception.Message.Contains("NextReopenConditionMissing")
}
if (-not $reopenConditionRejected) { throw "E9VerticalMissingReopenConditionWasAccepted" }

$missingOwner = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8 |
    ConvertFrom-Json
$missingOwner.collaborationProfile.ownerRef = ""
$missingOwnerPath = Join-Path $artifactDirectory "missing-collaboration-owner.json"
[IO.File]::WriteAllText($missingOwnerPath,
    ($missingOwner | ConvertTo-Json -Depth 20), $utf8)
$missingOwnerRelative = Get-RepositoryRelativePath $missingOwnerPath
$missingOwnerRejected = $false
try {
    & $manager -InputPath $missingOwnerRelative | Out-Null
}
catch {
    $missingOwnerRejected = $_.Exception.Message.Contains(
        "CollaborationOwnerMissing")
}
if (-not $missingOwnerRejected) { throw "E9VerticalMissingOwnerWasAccepted" }

$unknownLoop = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8 |
    ConvertFrom-Json
$unknownLoop.playableLoopRefs = @("playable-loop:unknown.v1")
$unknownLoopPath = Join-Path $artifactDirectory "unknown-playable-loop.json"
[IO.File]::WriteAllText($unknownLoopPath,
    ($unknownLoop | ConvertTo-Json -Depth 20), $utf8)
$unknownLoopRejected = $false
try {
    & $manager -InputPath (Get-RepositoryRelativePath $unknownLoopPath) | Out-Null
}
catch {
    $unknownLoopRejected = $_.Exception.Message.Contains("PlayableLoopRefUnknown")
}
if (-not $unknownLoopRejected) { throw "E9VerticalUnknownPlayableLoopWasAccepted" }

$unknownEvidence = Get-Content -LiteralPath $templatePath -Raw -Encoding utf8 |
    ConvertFrom-Json
$unknownEvidence.evidencePackageRefs = @("evidence:unknown")
$unknownEvidencePath = Join-Path $artifactDirectory "unknown-evidence-package.json"
[IO.File]::WriteAllText($unknownEvidencePath,
    ($unknownEvidence | ConvertTo-Json -Depth 20), $utf8)
$unknownEvidenceRejected = $false
try {
    & $manager -InputPath (Get-RepositoryRelativePath $unknownEvidencePath) | Out-Null
}
catch {
    $unknownEvidenceRejected = $_.Exception.Message.Contains(
        "EvidencePackageRefUnknown")
}
if (-not $unknownEvidenceRejected) {
    throw "E9VerticalUnknownEvidencePackageWasAccepted"
}

$workOrders = @(
    Get-ChildItem -LiteralPath (Join-Path $repositoryRoot `
        "eng/execution-ledgers/work-orders") -Filter "*.json" |
        ForEach-Object {
            Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 |
                ConvertFrom-Json
        } |
        Where-Object {
            [string]$_.schemaVersion -like "simulation-e9-vertical-work-order.*"
        }
)
$knownWorkOrderIds = @($workOrders.workOrderId)
foreach ($order in $workOrders) {
    foreach ($dependency in @($order.collaborationProfile.dependencyWorkOrderIds)) {
        if ($knownWorkOrderIds -notcontains [string] $dependency) {
            throw "E9VerticalDependencyWorkOrderUnknown:$($order.workOrderId):$dependency"
        }
    }
}

Write-Output (
    "E9VerticalWorkOrderTestsPassed:Template=1;Nature=1;SoloRuntime=1;Regional=1;HubNpcRoutine=1;DependencyGraph=$($workOrders.Count);Negative=7;Cycle=E9-E1-E9-Repeat")
