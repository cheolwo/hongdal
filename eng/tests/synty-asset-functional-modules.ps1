$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $root "eng/execution-ledgers/manage-synty-asset-functional-modules.ps1"
$result = @(& $manager -Mode Validate)
if (($result -join "`n") -notlike
    "*SyntyAssetFunctionalModulesValid:Packs=13;Prefabs=4211;PurchasedProfiles=6;Scopes=3;Modules=12;Subgroups=*") {
    throw "SyntyAssetFunctionalModuleManagerFailed:$($result -join ';')"
}
$taxonomy = Get-Content -LiteralPath (Join-Path $root `
    "eng/execution-ledgers/synty-asset-human-taxonomy.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
$scopeNames = @($taxonomy.표현범위 | ForEach-Object { [string] $_.범위이름 })
$expectedScopeNames = @("실외 표현", "실내 표현", "공통 표현")
if ($scopeNames.Count -ne $expectedScopeNames.Count -or
    @($expectedScopeNames | Where-Object { $scopeNames -notcontains $_ }).Count -gt 0) {
    throw "KoreanPresentationScopesInvalid:$($scopeNames -join ',')"
}
$moduleCodes = @($taxonomy.표현범위.기능군 | ForEach-Object { [string] $_.기능군Code })
if ($moduleCodes.Count -ne 12 -or @($moduleCodes | Sort-Object -Unique).Count -ne 12) {
    throw "KoreanFunctionalGroupsInvalid:$($moduleCodes.Count)"
}

$catalog = Get-Content -LiteralPath (Join-Path $root `
    "eng/execution-ledgers/synty-asset-functional-modules.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json
if (@($catalog.purchasedAssetUsageProfiles).Count -ne 6) {
    throw "PurchasedSyntyUsageProfilesMissing"
}
if ([bool] $catalog.legacyCompositionPolicy.newAuthoringAllowed) {
    throw "LegacySyntyCompositionAuthoringStillEnabled"
}
if ([string] ($catalog.sourcePacks | Where-Object packCode -eq "construction").policyCode -ne
    "SharedConstructionStateLayer") {
    throw "ConstructionWasTreatedAsArea"
}

Write-Output "SyntyAssetFunctionalModuleTestsPassed"
