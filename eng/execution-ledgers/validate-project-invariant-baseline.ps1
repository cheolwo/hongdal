[CmdletBinding()]
param(
    [string] $InputPath = "eng/execution-ledgers/project-invariant-baseline.json"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "ProjectInvariantBaselineInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = (Resolve-Path (Join-Path $repositoryRoot $InputPath)).Path
$catalog = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 |
    ConvertFrom-Json

Require ([string] $catalog.schemaVersion -eq
    "ssalddel-project-invariant-baseline.v1") "SchemaInvalid"
Require-Text $catalog.revision "RevisionMissing"
Require-Text $catalog.authorityDocumentPath "AuthorityDocumentMissing"
$authorityDocument = Join-Path $repositoryRoot ([string] $catalog.authorityDocumentPath)
Require (Test-Path -LiteralPath $authorityDocument) "AuthorityDocumentNotFound"

foreach ($principle in @(
    "refactoringDoesNotPromoteEvidence",
    "compatibilityRequiresExplicitMigration",
    "integrationOwnsNoIndependentAuthority",
    "sameSimulationCoreAcrossAuthorityLocations",
    "unityPresentationIsNotSimulationAuthority")) {
    $property = $catalog.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) "PrincipleMissing:$principle"
}

$expectedIds = @(
    "INV-AUTHORITY-01",
    "INV-CORE-01",
    "INV-COMMAND-01",
    "INV-TIME-01",
    "INV-EGH-01",
    "INV-SPACE-01",
    "INV-WORLD-01",
    "INV-COMPAT-01"
)
$invariants = @($catalog.invariants)
Require (($invariants.id -join ",") -eq ($expectedIds -join ",")) "InvariantOrderInvalid"

$seen = @{}
foreach ($invariant in $invariants) {
    $id = [string] $invariant.id
    Require-Text $id "InvariantIdMissing"
    Require (-not $seen.ContainsKey($id)) "InvariantDuplicate:$id"
    $seen[$id] = $true
    Require (@($catalog.allowedCategories) -contains
        [string] $invariant.category) "CategoryInvalid:$id"
    Require-Text $invariant.koreanName "KoreanNameMissing:$id"
    Require-Text $invariant.rule "RuleMissing:$id"
    Require ([string] $invariant.changePolicy -eq
        [string] $catalog.requiredChangePolicy) "ChangePolicyInvalid:$id"
    Require (@($invariant.owners).Count -gt 0) "OwnerMissing:$id"
    foreach ($owner in @($invariant.owners)) {
        Require (@($catalog.allowedOwners) -contains [string] $owner) "OwnerInvalid:${id}:$owner"
    }
    Require (@($invariant.guardReferences).Count -gt 0) "GuardReferenceMissing:$id"
    foreach ($reference in @($invariant.guardReferences)) {
        $guardReference = Join-Path $repositoryRoot ([string] $reference)
        Require (Test-Path -LiteralPath $guardReference) "GuardReferenceNotFound:${id}:$reference"
    }
}

Write-Output ("ProjectInvariantBaselineValid:{0};Revision={1}" -f
    $invariants.Count, [string] $catalog.revision)
