$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot `
    "eng/execution-ledgers/manage-post-e7-evidence-campaigns.ps1"
$result = & $manager -Mode Write
if ([string] $result -ne
    "PostE7EvidenceCampaignsValid:E8=16;E9=4;Deferred=2;E10=1;Revision=post-e7-evidence-campaigns.r5") {
    throw "PostE7EvidenceCampaignValidationFailed:$result"
}
$check = & $manager -Mode Check
if ([string] $check -ne [string] $result) {
    throw "PostE7EvidenceCampaignCheckDrift:$check"
}

$sourcePath = Join-Path $repositoryRoot `
    "eng/execution-ledgers/post-e7-evidence-campaigns.json"
$artifactDirectory = Join-Path $repositoryRoot `
    "artifacts/local/validation/post-e7-evidence-campaigns"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)

function Write-Case([object] $Value, [string] $Name) {
    $path = Join-Path $artifactDirectory "$Name.json"
    [IO.File]::WriteAllText($path, ($Value | ConvertTo-Json -Depth 60), $utf8)
    return $path.Substring($repositoryRoot.Length + 1).Replace('\', '/')
}

function Require-Rejected([object] $Value, [string] $Name, [string] $Code) {
    $relative = Write-Case $Value $Name
    $rejected = $false
    try {
        & $manager -Mode Write -InputPath $relative `
            -OutputPath "artifacts/local/validation/post-e7-evidence-campaigns/$Name.md" | Out-Null
    }
    catch { $rejected = $_.Exception.Message.Contains($Code) }
    if (-not $rejected) { throw "PostE7InvalidCaseAccepted:$Name" }
}

$stale = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$stale.playableUnitStabilityCampaigns[0].currentEvidenceStage = "E5"
Require-Rejected $stale "stale-stability" "StabilityObservedStageStale"

$missing = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$missing.playableUnitStabilityCampaigns = @($missing.playableUnitStabilityCampaigns | Select-Object -First 15)
Require-Rejected $missing "missing-stability" "StabilityCoverageCountInvalid"

$premature = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$premature.playableUnitStabilityCampaigns[2].promotionEligible = $true
Require-Rejected $premature "premature-stability" "StabilityCannotPromoteBeforeE7"

$single = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$single.areaHarmonyCampaigns[0].requiredMemberStabilityCampaignStableIds = @(
    "stability:nature-shelter-foundation.v1")
Require-Rejected $single "single-member" "HarmonyNeedsTwoStableMembers"

$foreign = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$foreign.areaHarmonyCampaigns[1].requiredMemberStabilityCampaignStableIds[1] =
    "stability:farm-crop-cycle.v1"
Require-Rejected $foreign "foreign-member" "HarmonyMemberMustBelongToAggregateCore"

$automatic = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$automatic.areaHarmonyCampaigns[0].promotionEligible = $true
$automatic.areaHarmonyCampaigns[0].statusCode = "Passed"
$automatic.areaHarmonyCampaigns[0].maturityTracks.logic.statusCode = "Passed"
$automatic.areaHarmonyCampaigns[0].maturityTracks.presentation.statusCode = "Passed"
$automatic.areaHarmonyCampaigns[0].integratedGate.statusCode = "Passed"
Require-Rejected $automatic "automatic-harmony-promotion" "HarmonyMemberNotE8Stable"

$effects = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$effects.limitedOperationWindows[0].externalOperationalEffectsAuthorized = $true
Require-Rejected $effects "external-effects" "OperationEffectsMustRemainUnauthorized"

$invalidReopen = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json
$invalidReopen.areaHarmonyCampaigns[0].humanAcceptance.findings = @(
    [pscustomobject]@{
        findingStableId = "finding:test:invalid-reopen"
        severityCode = "Minor"
        statusCode = "Open"
        reopenStageCode = "E10"
        targetTrackCode = "Logic"
        reopenSubjectRef = "playable-loop:nature-shelter-foundation.v1"
    })
Require-Rejected $invalidReopen "invalid-reopen-stage" "FindingReopenStageInvalid"

Write-Output "PostE7EvidenceCampaignTestsPassed:Stability=16;Harmony=4;Deferred=2;Operation=1;Negative=8"
