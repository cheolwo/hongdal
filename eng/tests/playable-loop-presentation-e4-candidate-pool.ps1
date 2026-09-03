$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-playable-loop-presentation-e4-candidate-pool.ps1'
$sourceCatalog = Join-Path $repositoryRoot 'eng/execution-ledgers/playable-loop-presentation-e4-candidate-pool.json'
$fixtureRoot = Join-Path $repositoryRoot ('artifacts/local/validation/presentation-e4-pool/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixtureRoot) | Out-Null
$fixtureRelative = $fixtureRoot.Substring($repositoryRoot.Length + 1).Replace('\', '/')
$script:cases = 0

function Copy-Value([object] $Value) {
    return ($Value | ConvertTo-Json -Depth 60 | ConvertFrom-Json)
}

function Save-Catalog([string] $Name, [object] $Value) {
    $path = Join-Path $fixtureRoot ($Name + '.json')
    [IO.File]::WriteAllText($path, (($Value | ConvertTo-Json -Depth 60) + "`n"), [Text.UTF8Encoding]::new($false))
    return ($fixtureRelative + '/' + $Name + '.json')
}

function Assert-Rejected([object] $Catalog, [string] $Name, [string] $Expected) {
    $catalogPath = Save-Catalog $Name $Catalog
    $caught = ''
    try {
        & $manager -Mode Write -CatalogPath $catalogPath -OutputPath ($fixtureRelative + '/' + $Name + '.md') -MachineOutputPath ($fixtureRelative + '/' + $Name + '.generated.json') | Out-Null
    }
    catch { $caught = $_.Exception.Message }
    if (-not $caught.Contains($Expected)) { throw "PresentationE4PoolNegativeFailed:$Name;expected=$Expected;actual=$caught" }
    $script:cases++
}

function Assert-Case([bool] $Condition, [string] $Name) {
    if (-not $Condition) { throw "PresentationE4PoolPositiveFailed:$Name" }
    $script:cases++
}

$catalog = Get-Content $sourceCatalog -Raw -Encoding UTF8 | ConvertFrom-Json
$output = $fixtureRelative + '/generated.md'
$machine = $fixtureRelative + '/generated.json'
$result = @(& $manager -Mode Write -OutputPath $output -MachineOutputPath $machine -UnityProjectRoot 'C:/Users/user/ssalddel')
Assert-Case (($result -join "`n").Contains('Plans=47;Frozen=11;Provisional=23;NotApplicable=13;WI=22')) 'CurrentCatalogWrites'
& $manager -Mode Check -OutputPath $output -MachineOutputPath $machine -UnityProjectRoot 'C:/Users/user/ssalddel' | Out-Null
Assert-Case ((Get-Content (Join-Path $repositoryRoot $machine) -Raw -Encoding UTF8 | ConvertFrom-Json).items.Count -eq 22) 'MachineViewHasAllWis'
$beforeHash = (Get-FileHash (Join-Path $repositoryRoot $machine) -Algorithm SHA256).Hash
& $manager -Mode Write -OutputPath $output -MachineOutputPath $machine -UnityProjectRoot 'C:/Users/user/ssalddel' | Out-Null
Assert-Case ((Get-FileHash (Join-Path $repositoryRoot $machine) -Algorithm SHA256).Hash -ceq $beforeHash) 'WriteIsDeterministic'
$farm = & $manager -Mode Query -QueryKind Area -QueryValue Farm | ConvertFrom-Json
Assert-Case ($farm.count -eq 4 -and @($farm.items | Where-Object readinessCode -ne 'Blocked').Count -eq 0) 'FarmQueryIsBlocked'
$blocked = & $manager -Mode Query -QueryKind Readiness -QueryValue Blocked | ConvertFrom-Json
Assert-Case ($blocked.count -eq 22) 'AllFirstBatchWisBlocked'

$bad = Copy-Value $catalog
$bad.planningIndex.sha256 = '0' * 64
Assert-Rejected $bad 'stale-plan-index' 'PlanningIndexStale'

$bad = Copy-Value $catalog
$bad.areaPreparations[1].worldInteractions[0].id = 'WI-UNKNOWN-01'
Assert-Rejected $bad 'unknown-wi' 'AreaWiSetMismatch'

$bad = Copy-Value $catalog
$bad.areaPreparations[1].hDefinitionRefs += 'eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json'
Assert-Rejected $bad 'unknown-h-definition' 'HDefinitionNotInCatalog'

$bad = Copy-Value $catalog
$bad.areaPreparations[1].worldInteractions[1].visualKeys = @($bad.areaPreparations[1].worldInteractions[0].visualKeys)
Assert-Rejected $bad 'duplicate-visual-key' 'VisualKeyDuplicate'

$bad = Copy-Value $catalog
$bad.areaPreparations[1].assetCandidates[0].guid = 'NOT-A-GUID'
Assert-Rejected $bad 'bad-asset-guid' 'AssetGuidInvalid'

$bad = Copy-Value $catalog
$bad.areaPreparations[0].assetCandidates = @($catalog.areaPreparations[1].assetCandidates[0])
Assert-Rejected $bad 'provisional-asset-freeze' 'UnapprovedAreaAssetFrozen'

$bad = Copy-Value $catalog
$bad.areaPreparations[1].readinessCode = 'Ready'
$bad.areaPreparations[1].constraintCodes = @()
Assert-Rejected $bad 'ready-without-constraints' 'ReadyConstraintsMissing'

$placementPath = Join-Path $fixtureRoot 'ready-placement.json'
[IO.File]::WriteAllText($placementPath, "{`"placementMapRef`":`"fixture-map`",`"validationResultCode`":`"Passed`"}`n", [Text.UTF8Encoding]::new($false))
$bad = Copy-Value $catalog
$bad.areaPreparations[1].readinessCode = 'Ready'
$bad.areaPreparations[1].placementMapRef = $fixtureRelative + '/ready-placement.json'
$bad.areaPreparations[1].worldInteractions[0] | Add-Member -NotePropertyName subjectKindCode -NotePropertyValue 'Actor'
Assert-Rejected $bad 'actor-ready-without-contract' 'ReadyActorPreparationMissing'

Write-Output "PresentationE4CandidatePoolTestsPassed:Cases=$script:cases;Plans=47;FirstBatchWI=22"
