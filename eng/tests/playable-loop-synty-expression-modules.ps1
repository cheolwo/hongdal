$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot `
    "eng/execution-ledgers/manage-playable-loop-synty-expression-modules.ps1"
$result = @(& $manager -Mode Validate)
if (($result -join "`n") -notlike
    "*PlayableLoopSyntyModulesValid:Loops=4;Shared=4;Slots=23;Families=31*") {
    throw "PlayableLoopSyntyModuleManagerFailed:$($result -join ';')"
}

$catalogPath = Join-Path $repositoryRoot `
    "eng/execution-ledgers/playable-loop-synty-expression-modules.json"
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 |
    ConvertFrom-Json
if ([bool] $catalog.legacyCompositionPolicy.canonicalForNewWork -or
    [bool] $catalog.legacyCompositionPolicy.newGenerationAllowed -or
    [bool] $catalog.legacyCompositionPolicy.forcedVariantCompletenessRequired) {
    throw "LegacyCompositionPolicyStillActive"
}

$night = @($catalog.loopModules | Where-Object {
    [string] $_.loopStableId -eq "playable-loop:nature-night-day2.v1"
})
if ($night.Count -ne 1 -or
    @($night[0].worldInteractionIds).Count -ne 3 -or
    @($night[0].slots | Where-Object {
        [string] $_.placementRoleCode -eq "interior-fixture"
    }).Count -lt 2) {
    throw "NatureNightDay2SyntyModuleIncomplete"
}

Write-Output "PlayableLoopSyntyModuleTestsPassed:LegacyABC=ReadOnly;NatureModules=4"
