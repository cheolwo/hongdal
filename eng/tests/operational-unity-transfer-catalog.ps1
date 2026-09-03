$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-operational-unity-transfer-catalog.ps1'
$policyPath = Join-Path $repositoryRoot 'eng/execution-ledgers/operational-unity-transfer-policy.json'
$fixtureRoot = Join-Path $repositoryRoot ('artifacts/local/validation/operational-unity-transfer/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixtureRoot) | Out-Null
$fixtureRelative = $fixtureRoot.Substring($repositoryRoot.Length + 1).Replace('\', '/')
$script:cases = 0

function Assert-Case([bool] $Condition, [string] $Name) {
    if (-not $Condition) { throw "OperationalUnityTransferTestFailed:$Name" }
    $script:cases++
}

$machineOutput = "$fixtureRelative/catalog.json"
$markdownOutput = "$fixtureRelative/catalog.md"
& $manager -Mode Write -MachineOutputPath $machineOutput -OutputPath $markdownOutput | Out-Null
& $manager -Mode Check -MachineOutputPath $machineOutput -OutputPath $markdownOutput | Out-Null
$catalogPath = Join-Path $repositoryRoot $machineOutput
$markdownPath = Join-Path $repositoryRoot $markdownOutput
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 | ConvertFrom-Json

Assert-Case ([string] $catalog.schemaVersion -eq 'operational-unity-transfer-catalog.v1') 'Schema'
Assert-Case (@($catalog.pageCapabilities).Count -ge 200) 'PageCatalogIsComprehensive'
Assert-Case (@($catalog.dbSets).Count -ge 250) 'DbSetInventoryIsComprehensive'
Assert-Case (@($catalog.mongoCollections).Count -ge 20) 'MongoInventoryIsComprehensive'
Assert-Case (@($catalog.unityRepresentativeRoutes).Count -eq 18) 'RepresentativeUnityRoutesPreserved'
Assert-Case (@($catalog.pageCapabilities | Where-Object transferClassification -eq 'PlayableAction').Count -gt 0) 'PlayableActionsExist'
Assert-Case (@($catalog.pageCapabilities | Where-Object transferClassification -eq 'ReadOnlyContext').Count -gt 0) 'ReadOnlyContextsExist'
Assert-Case (@($catalog.pageCapabilities | Where-Object transferClassification -eq 'AmbientSimulation').Count -gt 0) 'AmbientSimulationsExist'
Assert-Case (@($catalog.pageCapabilities | Where-Object transferClassification -eq 'ServerOnly').Count -gt 0) 'ServerOnlyBoundariesExist'
Assert-Case (@($catalog.pageCapabilities | Where-Object { $_.appCode -eq 'SsalddelAdmin' -and $_.transferClassification -ne 'ServerOnly' }).Count -eq 0) 'AdminAlwaysServerOnly'
Assert-Case (@($catalog.pageCapabilities | Where-Object { $_.mappingRuleIds -contains 'warehouse-fulfillment' -and $_.hMappingStatus -ne 'MappedCandidate' }).Count -eq 0) 'WarehouseHasHMapping'
Assert-Case (@($catalog.pageCapabilities | Where-Object canonicalFeatureId -eq 'warehouse-inbound-inspection').Count -ge 2) 'ValidatedAliasGroup'
Assert-Case ([string] $catalog.firstSlice.playableLoopRef -eq 'playable-loop:hub-inbound-putaway.v1') 'HubFirstSlice'
Assert-Case ([string] $catalog.firstSlice.e5Status -eq 'BlockedPendingActualWorldPlacement') 'NoAutomaticE5Promotion'
$markdown = Get-Content -LiteralPath $markdownPath -Raw -Encoding UTF8
Assert-Case ($markdown.Contains('MappedCandidate') -and $markdown.Contains('E5')) 'MarkdownBoundary'

$beforeJson = (Get-FileHash -LiteralPath $catalogPath -Algorithm SHA256).Hash
$beforeMarkdown = (Get-FileHash -LiteralPath $markdownPath -Algorithm SHA256).Hash
& $manager -Mode Write -MachineOutputPath $machineOutput -OutputPath $markdownOutput | Out-Null
Assert-Case ((Get-FileHash -LiteralPath $catalogPath -Algorithm SHA256).Hash -ceq $beforeJson) 'JsonDeterministic'
Assert-Case ((Get-FileHash -LiteralPath $markdownPath -Algorithm SHA256).Hash -ceq $beforeMarkdown) 'MarkdownDeterministic'

$hubQuery = @(& $manager -Mode Query -QueryKind H1 -QueryValue 'h1-stock:hub-receiving-storage') -join "`n"
$hubRows = $hubQuery | ConvertFrom-Json
Assert-Case (@($hubRows).Count -gt 0) 'H1Query'
Assert-Case (@($hubRows | Where-Object { $_.areaCodes -contains 'Hub' }).Count -eq @($hubRows).Count) 'H1QueryArea'

$badPolicy = Get-Content -LiteralPath $policyPath -Raw -Encoding UTF8 | ConvertFrom-Json
$badPolicy.planning.documentSha256 = '0' * 64
$badPolicyPath = Join-Path $fixtureRoot 'bad-policy.json'
[IO.File]::WriteAllText($badPolicyPath, (($badPolicy | ConvertTo-Json -Depth 40) + "`n"), [Text.UTF8Encoding]::new($false))
$failureObserved = $false
try {
    & $manager -Mode Check -PolicyPath $badPolicyPath -MachineOutputPath $machineOutput -OutputPath $markdownOutput | Out-Null
}
catch { $failureObserved = $true }
Assert-Case $failureObserved 'PlanningHashDriftRejected'

Write-Output "OperationalUnityTransferCatalogTestsPassed:Cases=$script:cases;Pages=$(@($catalog.pageCapabilities).Count);DbSets=$(@($catalog.dbSets).Count);Mongo=$(@($catalog.mongoCollections).Count)"
