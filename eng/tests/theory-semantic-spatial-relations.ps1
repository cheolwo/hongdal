$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$relationManager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-theory-semantic-relations.ps1"
$factoryManager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-theory-spatial-factory.ps1"
$ledgerRelative = "eng/world-seedbeds/synty-bottom-up-inventory/semantic-spatial-relations.v1.json"
$ledgerPath = Join-Path $repositoryRoot $ledgerRelative
$baselinePath = Join-Path $repositoryRoot "eng/world-seedbeds/generated/theory-spatial-factory.v1.json"
$testRoot = Join-Path $repositoryRoot "artifacts/local/validation/theory-semantic-spatial-relations"
if (Test-Path -LiteralPath $testRoot) { Remove-Item -LiteralPath $testRoot -Recurse -Force }
[void] (New-Item -ItemType Directory -Path $testRoot)

$ledgerCheck = & pwsh -NoProfile -File $relationManager -Mode Check
if ($ledgerCheck -notmatch "TheorySemanticRelationsValid:H2=37;H3=20;AreaSets=4;WorldRelations=5") { throw "SemanticRelationLedgerCheckFailed" }
$ledger = Get-Content -LiteralPath $ledgerPath -Raw -Encoding UTF8 | ConvertFrom-Json
if (@($ledger.h2RelationRecipes).Count -ne 37 -or @($ledger.h3RelationRecipes).Count -ne 20 -or @($ledger.areaSetRelationRecipes).Count -ne 4) { throw "SemanticRelationRecipeCoverageInvalid" }
if (@($ledger.h3RelationRecipes | Where-Object { @($_.requiredChildRefs).Count -lt 2 }).Count -ne 0) { throw "SemanticRelationH3MinimumChildInvalid" }
if (@($ledger.h2RelationRecipes + $ledger.h3RelationRecipes + $ledger.areaSetRelationRecipes | ForEach-Object { @($_.exposedConnectors) } | Where-Object { [string]::IsNullOrWhiteSpace([string] $_.sourceChildRef) -or [string]::IsNullOrWhiteSpace([string] $_.sourceChildConnectorRoleCode) }).Count -ne 0) { throw "SemanticRelationConnectorLineageMissing" }

$baselineHash = (Get-FileHash -LiteralPath $baselinePath -Algorithm SHA256).Hash
$shuffled = Get-Content -LiteralPath $ledgerPath -Raw -Encoding UTF8 | ConvertFrom-Json
$shuffled.h2RelationRecipes = @($shuffled.h2RelationRecipes | Sort-Object targetRef -Descending)
$shuffled.h3RelationRecipes = @($shuffled.h3RelationRecipes | Sort-Object targetRef -Descending)
$shuffled.areaSetRelationRecipes = @($shuffled.areaSetRelationRecipes | Sort-Object targetRef -Descending)
foreach ($recipe in @($shuffled.h2RelationRecipes) + @($shuffled.h3RelationRecipes) + @($shuffled.areaSetRelationRecipes)) { $recipe.relations = @($recipe.relations | Sort-Object relationCode -Descending) }
$shuffled.worldRelationRecipe.relations = @($shuffled.worldRelationRecipe.relations | Sort-Object relationCode -Descending)
$shufflePath = Join-Path $testRoot "shuffled-relations.json"
[IO.File]::WriteAllText($shufflePath, (($shuffled | ConvertTo-Json -Depth 100) + "`n"), [Text.UTF8Encoding]::new($false))
$shuffleRelative = [IO.Path]::GetRelativePath($repositoryRoot, $shufflePath).Replace("\", "/")
$shuffleOutputRelative = "artifacts/local/validation/theory-semantic-spatial-relations/shuffled-output.json"
$shuffleMarkdownRelative = "artifacts/local/validation/theory-semantic-spatial-relations/shuffled-output.md"
$shuffleResult = & pwsh -NoProfile -File $factoryManager -Mode Write -SemanticRelationsPath $shuffleRelative -JsonOutputPath $shuffleOutputRelative -MarkdownOutputPath $shuffleMarkdownRelative
if ($shuffleResult -notmatch "World=TheoryWorldQualified") { throw "SemanticRelationShuffleGenerationFailed" }
if ((Get-FileHash -LiteralPath (Join-Path $repositoryRoot $shuffleOutputRelative) -Algorithm SHA256).Hash -ne $baselineHash) { throw "SemanticRelationArrayOrderChangedOutput" }

$malformed = Get-Content -LiteralPath $ledgerPath -Raw -Encoding UTF8 | ConvertFrom-Json
$malformed.worldRelationRecipe.relations[0].compatibilityRuleCode = "UnknownRule"
$malformedPath = Join-Path $testRoot "malformed-relations.json"
[IO.File]::WriteAllText($malformedPath, (($malformed | ConvertTo-Json -Depth 100) + "`n"), [Text.UTF8Encoding]::new($false))
$malformedRelative = [IO.Path]::GetRelativePath($repositoryRoot, $malformedPath).Replace("\", "/")
$malformedMessage = & pwsh -NoProfile -File $factoryManager -Mode Write -SemanticRelationsPath $malformedRelative -JsonOutputPath "artifacts/local/validation/theory-semantic-spatial-relations/malformed-output.json" -MarkdownOutputPath "artifacts/local/validation/theory-semantic-spatial-relations/malformed-output.md" 2>&1 | Out-String
if ($LASTEXITCODE -eq 0) { throw "MalformedSemanticContractUnexpectedSuccess" }
if ($malformedMessage -notmatch "MalformedContract:CompatibilityRuleUnknown") { throw "MalformedSemanticContractWasNotRejected:$malformedMessage" }

$unresolved = Get-Content -LiteralPath $ledgerPath -Raw -Encoding UTF8 | ConvertFrom-Json
$unresolved.h2RelationRecipes[0].childConnectorProfiles[0].connectors = @($unresolved.h2RelationRecipes[0].childConnectorProfiles[0].connectors | Where-Object roleCode -ne "Output")
$unresolvedPath = Join-Path $testRoot "unresolved-relations.json"
[IO.File]::WriteAllText($unresolvedPath, (($unresolved | ConvertTo-Json -Depth 100) + "`n"), [Text.UTF8Encoding]::new($false))
$unresolvedRelative = [IO.Path]::GetRelativePath($repositoryRoot, $unresolvedPath).Replace("\", "/")
$unresolvedMessage = & pwsh -NoProfile -File $factoryManager -Mode Write -SemanticRelationsPath $unresolvedRelative -JsonOutputPath "artifacts/local/validation/theory-semantic-spatial-relations/unresolved-output.json" -MarkdownOutputPath "artifacts/local/validation/theory-semantic-spatial-relations/unresolved-output.md" 2>&1 | Out-String
if ($LASTEXITCODE -eq 0) { throw "SemanticRelationUnresolvedUnexpectedSuccess" }
if ($unresolvedMessage -notmatch "H2SemanticRelationUnresolved") { throw "SemanticRelationUnresolvedWasNotSeparated:$unresolvedMessage" }

Write-Output "TheorySemanticSpatialRelationTestsPassed:H2=37;H3=20;AreaSets=4;World=Qualified;OrderIndependent=True;NegativeFixtures=2"
