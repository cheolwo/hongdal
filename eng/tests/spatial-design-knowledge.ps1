$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-spatial-design-knowledge.ps1"
$managerV3 = Join-Path $repositoryRoot "eng/world-seedbeds/manage-spatial-design-knowledge-v3.ps1"
$query = Join-Path $repositoryRoot "eng/world-seedbeds/query-spatial-design-knowledge.ps1"
$catalogPath = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v2.json"
$catalogV3Path = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json"

$beforeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $catalogPath).Hash
$beforeTicks = (Get-Item -LiteralPath $catalogPath).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
$write = & pwsh -NoProfile -File $manager -Mode Write
$afterHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $catalogPath).Hash
$afterTicks = (Get-Item -LiteralPath $catalogPath).LastWriteTimeUtc.Ticks

if ($check -notmatch "SpatialDesignKnowledgeValid:H1=41;H2=18;H3=10") { throw "SpatialDesignKnowledgeCheckFailed" }
if ($write -notmatch "SpatialDesignKnowledgeGenerated:H1=41;H2=18;H3=10") { throw "SpatialDesignKnowledgeWriteFailed" }
if ($beforeHash -ne $afterHash) { throw "SpatialDesignKnowledgeCatalogHashChangedWithoutInputChange" }
if ($beforeTicks -ne $afterTicks) { throw "SpatialDesignKnowledgeCatalogWasRewrittenWithoutInputChange" }

$beforeV3Hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $catalogV3Path).Hash
$beforeV3Ticks = (Get-Item -LiteralPath $catalogV3Path).LastWriteTimeUtc.Ticks
$checkV3 = & pwsh -NoProfile -File $managerV3 -Mode Check
$writeV3 = & pwsh -NoProfile -File $managerV3 -Mode Write
$afterV3Hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $catalogV3Path).Hash
$afterV3Ticks = (Get-Item -LiteralPath $catalogV3Path).LastWriteTimeUtc.Ticks
if ($checkV3 -notmatch "SpatialDesignKnowledgeV3Valid:Grammar=52/156;H1=73\(41\+32\);H2=18;H3=10;H4=5") { throw "SpatialDesignKnowledgeV3CheckFailed" }
if ($writeV3 -notmatch "SpatialDesignKnowledgeV3Generated:Grammar=52/156;H1=73\(41\+32\);H2=18;H3=10;H4=5") { throw "SpatialDesignKnowledgeV3WriteFailed" }
if ($beforeV3Hash -ne $afterV3Hash) { throw "SpatialDesignKnowledgeV3CatalogHashChangedWithoutInputChange" }
if ($beforeV3Ticks -ne $afterV3Ticks) { throw "SpatialDesignKnowledgeV3CatalogWasRewrittenWithoutInputChange" }

$json = (& $query -WiIds @("WI-FARM-04", "WI-FARM-05", "WI-FARM-06") -PackCodes @("Farm", "Nature") -Format Json) | ConvertFrom-Json
if (@($json.h1Recommendations).Count -eq 0) { throw "SpatialDesignKnowledgeFarmH1RecommendationMissing" }
if (@($json.h1Recommendations.stableId) -notcontains "h1-stock:farm-production") { throw "SpatialDesignKnowledgeFarmProductionMissing" }
if (@($json.h2Recommendations).Count -eq 0) { throw "SpatialDesignKnowledgeFarmH2RecommendationMissing" }
if (@($json.h3Recommendations).Count -eq 0) { throw "SpatialDesignKnowledgeFarmH3RecommendationMissing" }
if ([string] $json.authorityBoundary -notmatch "자동 승인하지 않는다") { throw "SpatialDesignKnowledgeAuthorityBoundaryMissing" }
if (@($json.h1ExpressionRecommendations).Count -eq 0) { throw "SpatialDesignKnowledgeExpressionRecommendationMissing" }
if (@($json.h4Recommendations.stableId) -notcontains "h4-blueprint:farm-production-processing-region") { throw "SpatialDesignKnowledgeH4RecommendationMissing" }

$commaSeparatedJson = (& $query -WiIds "WI-FARM-04,WI-FARM-05,WI-FARM-06" -PackCodes "Farm,Nature" -Format Json) | ConvertFrom-Json
if (@($commaSeparatedJson.h1Recommendations.stableId) -notcontains "h1-stock:farm-production") { throw "SpatialDesignKnowledgeCommaSeparatedQueryFailed" }

$nature = (& $query -PackCodes "Nature" -CardKinds "PackExpression" -Limit 50 -Format Json) | ConvertFrom-Json
if (@($nature.h1Recommendations).Count -ne 0) { throw "SpatialDesignKnowledgeNatureInteractionLeak" }
if (@($nature.h1ExpressionRecommendations).Count -ne 12) { throw "SpatialDesignKnowledgeNatureExpressionCountInvalid" }
$farm = (& $query -PackCodes "Farm" -CardKinds "PackExpression" -Limit 50 -Format Json) | ConvertFrom-Json
$town = (& $query -PackCodes "Town" -CardKinds "PackExpression" -Limit 50 -Format Json) | ConvertFrom-Json
$city = (& $query -PackCodes "City" -CardKinds "PackExpression" -Limit 50 -Format Json) | ConvertFrom-Json
if (@($farm.h1ExpressionRecommendations).Count -ne 8) { throw "SpatialDesignKnowledgeFarmExpressionCountInvalid" }
if (@($town.h1ExpressionRecommendations).Count -ne 6) { throw "SpatialDesignKnowledgeTownExpressionCountInvalid" }
if (@($city.h1ExpressionRecommendations).Count -ne 6) { throw "SpatialDesignKnowledgeCityExpressionCountInvalid" }
$grammarQuery = (& $query -GrammarRefs "farm:감자밭 두렁:A" -CardKinds "PackExpression" -Limit 50 -Format Json) | ConvertFrom-Json
if (@($grammarQuery.h1ExpressionRecommendations).Count -ne 1) { throw "SpatialDesignKnowledgeGrammarExpressionCountInvalid" }
if ([string] $grammarQuery.h1ExpressionRecommendations[0].stableId -ne "h1-expression:farm:감자밭-두렁") { throw "SpatialDesignKnowledgeGrammarExpressionMismatch" }

Write-Output "SpatialDesignKnowledgeTestsPassed:Grammar=52/156;H1=73(41+32);H2=18;H3=10;H4=5"
