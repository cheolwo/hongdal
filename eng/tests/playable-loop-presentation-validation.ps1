$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot `
    "eng/execution-ledgers/manage-playable-loop-presentation-validation.ps1"
$result = @(& $manager -Mode Validate)
if (($result -join "`n") -notlike
    "*PlayableLoopPresentationValidationValid:Modules=*;Profiles=*;PlayableUnits=*") {
    throw "PresentationValidationManagerFailed:$($result -join ';')"
}

$generatedPath = Join-Path $repositoryRoot `
    "docs/AI/generated/playable-loop-presentation-validation.md"
$generated = Get-Content -LiteralPath $generatedPath -Raw -Encoding UTF8
$catalogPath = Join-Path $repositoryRoot `
    "eng/execution-ledgers/playable-loop-presentation-validation-modules.json"
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 |
    ConvertFrom-Json
foreach ($expected in @(
    [string] $catalog.generatedDocumentText.titleKo,
    [string] $catalog.generatedDocumentText.commonGateHeadingKo,
    [string] $catalog.generatedDocumentText.profileHeadingKo,
    "presentation-binding",
    "surface-clearance",
    "building-foundation-entry",
    "actual-camera-input-result-return",
    "playable-loop:nature-shelter-foundation.v1",
    "playable-loop:nature-workbench-foundation.v1")) {
    if (-not $generated.Contains($expected)) {
        throw "PresentationValidationGeneratedEntryMissing:$expected"
    }
}

$commonCount = @($catalog.commonModuleCodes).Count
$conditionalCount = @($catalog.modules |
    Where-Object applicabilityCode -eq "Feature").Count
$profileCount = @($catalog.loopProfiles).Count
Write-Output `
    "PlayableLoopPresentationValidationTestsPassed:Common=$commonCount;Conditional=$conditionalCount;Profiles=$profileCount"
