$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path | Split-Path -Parent
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-hexagram-story-tree.ps1'
$outputPath = Join-Path $repositoryRoot 'docs/AI/generated/hexagram-story-tree.json'
$hOutputPath = Join-Path $repositoryRoot 'docs/AI/generated/hexagram-h-reference-index.json'

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramStoryTreeTestFailed:$Code" }
}

& $manager -Mode Validate | Out-Null
& $manager -Mode Check | Out-Null
$tree = Get-Content -LiteralPath $outputPath -Raw -Encoding UTF8 | ConvertFrom-Json
$hIndex = Get-Content -LiteralPath $hOutputPath -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([int] $tree.counts.hexagrams -eq 64) 'HexagramCount'
Require ([int] $tree.counts.lines -eq 384) 'LineCount'
Require (@($tree.hexagrams.lines.anchor | Sort-Object -Unique).Count -eq 384) 'LineAnchorCount'
Require (@($tree.hexagrams.anchor | Sort-Object -Unique).Count -eq 64) 'HexagramAnchorCount'
Require (@($tree.hexagrams.lines | Where-Object documentStateCode -eq 'Opened').Count -eq 12) 'OpenedLineDocuments'
Require (@($tree.hexagrams.lines | Where-Object requirementStateCode -ne 'NotDeclared').Count -eq 6) 'DeclaredRequirements'
Require ([int] $tree.counts.classifiedExistingPlans -eq 27) 'ClassifiedExistingPlans'
Require ([int] $tree.counts.confirmedExistingPlans -eq 1) 'ConfirmedExistingPlans'
Require (@($tree.hexagrams.lines | Where-Object { $_.documentStateCode -eq 'GeneratedDetail' -and (-not [string]::IsNullOrWhiteSpace([string] $_.documentRef)) }).Count -eq 0) 'GeneratedDetailHasPhysicalPath'
Require (@($tree.worldInteractionReferences.id | Sort-Object -Unique).Count -eq @($tree.worldInteractionReferences).Count) 'WorldInteractionReferenceDuplicate'
Require (@($hIndex.items.anchor | Sort-Object -Unique).Count -eq @($hIndex.items).Count) 'HReferenceDuplicate'
Require ([bool] $tree.policy.doesNotPromoteEvidence) 'EvidenceBoundary'

Write-Output 'HexagramStoryTreeTests:OK:Cases=12'
