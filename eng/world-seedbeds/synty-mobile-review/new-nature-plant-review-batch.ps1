[CmdletBinding()]
param(
    [string]$OutputPath = "artifacts/local/synty-mobile-review/nature-plant-review-batch.v2.json"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../../..")).Path
$architecturePath = Join-Path $repositoryRoot "docs/Architecture"
$planDocument = Get-ChildItem -Path $architecturePath -Filter "*Synty*.md" -File |
    Where-Object {
        [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8).Contains(
            "h1-action:nature-recovery-plant-core.r1")
    } |
    Select-Object -First 1
if ($null -eq $planDocument) {
    throw "Could not find the Synty five-pack composition plan."
}
$planPath = $planDocument.FullName
$resolvedOutputPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    [System.IO.Path]::GetFullPath($OutputPath)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputPath))
}

function Get-Sha256Hex {
    param([Parameter(Mandatory = $true)][string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha256.ComputeHash($bytes)
    }
    finally {
        $sha256.Dispose()
    }
    return -join ($hash | ForEach-Object { $_.ToString("x2") })
}

function ConvertFrom-UnicodeJsonString {
    param([Parameter(Mandatory = $true)][string]$Value)

    $json = '"' + $Value + '"'
    return $json | ConvertFrom-Json
}

$planText = [System.IO.File]::ReadAllText($planPath, [System.Text.Encoding]::UTF8)
$planHash = Get-Sha256Hex $planText
$renderingProfileId = "rendering-profile:mobile-review-draft-no-capture.r1"
$renderingProfileRevision = "r1"
$renderingProfileHash = Get-Sha256Hex "$renderingProfileId|$renderingProfileRevision|WaitingForCapture"
$recoveryTitle = ConvertFrom-UnicodeJsonString '\ud68c\ubcf5 \ubc1c\uc804\uc18c'
$threatTitle = ConvertFrom-UnicodeJsonString '\uc704\ud611 \ubc1c\uc804\uc18c'
$normalLabel = ConvertFrom-UnicodeJsonString '\uae30\ubcf8 \uc0c1\ud0dc'
$intensifiedLabel = ConvertFrom-UnicodeJsonString '\uac15\ud654 \uc0c1\ud0dc'
$variantSuffix = ConvertFrom-UnicodeJsonString '\ud615 \u00b7'
$batchTitle = ConvertFrom-UnicodeJsonString '\uc2ec\ub9ac \uc601\uc5ed \ud68c\ubcf5\u00b7\uc704\ud611 \ubc1c\uc804\uc18c \ubaa8\ubc14\uc77c \uac80\ud1a0'

$plants = @(
    [pscustomobject]@{
        Code = "recovery"
        Title = $recoveryTitle
        H1 = "h1-action:nature-recovery-plant-core.r1"
        H2 = "h2-composition:nature-restoration-recovery.r1"
        PackUsages = @(
            [ordered]@{ packCode = "Nature"; usagePercent = 40; roleCode = "Lead" }
            [ordered]@{ packCode = "Construction"; usagePercent = 30; roleCode = "FunctionalLayer" }
            [ordered]@{ packCode = "Farm"; usagePercent = 15; roleCode = "Support" }
            [ordered]@{ packCode = "Town"; usagePercent = 10; roleCode = "Support" }
            [ordered]@{ packCode = "City"; usagePercent = 5; roleCode = "Support" }
        )
    }
    [pscustomobject]@{
        Code = "threat"
        Title = $threatTitle
        H1 = "h1-action:nature-threat-plant-core.r1"
        H2 = "h2-composition:nature-threat-response.r1"
        PackUsages = @(
            [ordered]@{ packCode = "Construction"; usagePercent = 40; roleCode = "FunctionalLayer" }
            [ordered]@{ packCode = "Nature"; usagePercent = 25; roleCode = "Lead" }
            [ordered]@{ packCode = "City"; usagePercent = 20; roleCode = "Support" }
            [ordered]@{ packCode = "Farm"; usagePercent = 10; roleCode = "Support" }
            [ordered]@{ packCode = "Town"; usagePercent = 5; roleCode = "Support" }
        )
    }
)

$states = @(
    [pscustomobject]@{ Code = "Normal"; Label = $normalLabel }
    [pscustomobject]@{ Code = "Intensified"; Label = $intensifiedLabel }
)

$items = [System.Collections.Generic.List[object]]::new()
foreach ($plant in $plants) {
    foreach ($variant in @("A", "B", "C")) {
        foreach ($state in $states) {
            $compositionStableId = "composition:nature-$($plant.Code)-plant-$($variant.ToLowerInvariant()).$($state.Code.ToLowerInvariant()).r1"
            $reviewItemStableId = "review-item:nature-$($plant.Code)-plant-$($variant.ToLowerInvariant()).$($state.Code.ToLowerInvariant()).r1"
            $inputMaterial = "$compositionStableId|$($plant.H1)|$($plant.H2)|h3-candidate:nature-threat-recovery|$planHash"
            $items.Add([ordered]@{
                expectedRevision = 0
                reviewItemStableId = $reviewItemStableId
                compositionStableId = $compositionStableId
                displayName = "$($plant.Title) $variant $variantSuffix $($state.Label)"
                h1StableId = $plant.H1
                h2StableId = $plant.H2
                h3StableId = "h3-candidate:nature-threat-recovery"
                variantCode = $variant
                stateProfileCode = $state.Code
                compositionInputHash = Get-Sha256Hex $inputMaterial
                planHash = $planHash
                renderingProfileId = $renderingProfileId
                renderingProfileRevision = $renderingProfileRevision
                renderingProfileHash = $renderingProfileHash
                parentCaptureBundleHash = ""
                captureBundleHash = ""
                packUsages = $plant.PackUsages
                captures = @()
            })
        }
    }
}

$batch = [ordered]@{
    schemaVersion = "synty-composition-review-batch.v2"
    batchStableId = "review-batch:nature-recovery-threat-plants.r1"
    batchRevision = "design-$($planHash.Substring(0, 12))"
    title = $batchTitle
    generatedAtUtc = [DateTime]::UtcNow.ToString("O")
    items = $items
}

$outputDirectory = [System.IO.Path]::GetDirectoryName($resolvedOutputPath)
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$json = $batch | ConvertTo-Json -Depth 12
[System.IO.File]::WriteAllText(
    $resolvedOutputPath,
    $json + [Environment]::NewLine,
    [System.Text.UTF8Encoding]::new($false))

Write-Output "SyntyMobileReviewDraftBuilt:$($items.Count)"
Write-Output "DesignPlanHash:$planHash"
Write-Output "OutputPath:$resolvedOutputPath"
Write-Output "EvidenceBoundary:WaitingForCapture;NotUnityGameViewProof"
