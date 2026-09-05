$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path | Split-Path -Parent
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-hexagram-line-planning-requirements.ps1'
$sourcePath = Join-Path $repositoryRoot 'eng/execution-ledgers/hexagram-line-planning-requirements.json'

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramLinePlanningRequirementsTestFailed:$Code" }
}

function New-SourceCopy {
    return (Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json)
}

function Expect-ValidationFailure([string] $CaseName, [object] $Fixture, [string] $ExpectedCode) {
    $tempPath = Join-Path ([IO.Path]::GetTempPath()) ("mirror-hexagram-line-{0}-{1}.json" -f $CaseName, [guid]::NewGuid().ToString('N'))
    try {
        [IO.File]::WriteAllText($tempPath, (($Fixture | ConvertTo-Json -Depth 40) + "`n"), [Text.UTF8Encoding]::new($false))
        $failed = $false
        try {
            & $manager -Mode Validate -InputPath $tempPath | Out-Null
        } catch {
            $failed = $true
            Require ($_.Exception.Message.Contains($ExpectedCode)) "$CaseName`:Unexpected:$($_.Exception.Message)"
        }
        Require $failed "$CaseName`:Accepted"
    } finally {
        if (Test-Path -LiteralPath $tempPath) { Remove-Item -LiteralPath $tempPath -Force }
    }
}

& $manager -Mode Validate | Out-Null
& $manager -Mode Check | Out-Null

$source = New-SourceCopy
$line4 = @($source.items | Where-Object hexagramLineStableId -eq 'HEX-03-ZHUN-L4')[0]
$line5 = @($source.items | Where-Object hexagramLineStableId -eq 'HEX-03-ZHUN-L5')[0]
$line6 = @($source.items | Where-Object hexagramLineStableId -eq 'HEX-03-ZHUN-L6')[0]
Require (@($line4.subjectRequirements | Where-Object { $_.roleCode -match 'ForestFowl' -and $_.necessityCode -eq 'Required' }).Count -eq 0) 'Line4ForestFowlStillRequired'
Require (@($line5.worldInteractionRequirements | Where-Object { $_.roleCode -eq 'OptionalForestFowlDomestication' -and $_.necessityCode -eq 'Optional' }).Count -eq 1) 'Line5OptionalForestFowlMissing'
$line6Objectives = @($line6.hRequirements | Where-Object { $_.necessityCode -eq 'Required' } | ForEach-Object roleCode)
Require ($line6Objectives -contains 'FarmhouseDefenseObjective') 'Line6FarmhouseObjectiveMissing'
Require ($line6Objectives -contains 'FarmProductionObjective') 'Line6FarmProductionObjectiveMissing'
Require ($line6Objectives -contains 'FarmFenceDefenseChokepoint') 'Line6FenceObjectiveMissing'
Require (@($line6Objectives | Where-Object { $_ -match 'ForestFowl' }).Count -eq 0) 'Line6ForestFowlStillRequired'

$fixture = New-SourceCopy
$fixture.items[1].linePlanId = $fixture.items[0].linePlanId
Expect-ValidationFailure 'duplicate-plan' $fixture 'LinePlanIdDuplicate'

$fixture = New-SourceCopy
$fixture.items[0].hexagramLineStableId = 'HEX-03-ZHUN-L9'
Expect-ValidationFailure 'unknown-line' $fixture 'HexagramLineSequence'

$fixture = New-SourceCopy
$fixture.items[0].worldInteractionRequirements[1].targetRef = 'WI-UNKNOWN'
Expect-ValidationFailure 'unknown-wi' $fixture 'UnknownCandidate'

$fixture = New-SourceCopy
$fixture.items[0].hRequirements[2].compositionEvidenceRefs = @()
Expect-ValidationFailure 'h2-composition' $fixture 'CompositionEvidenceMissing'

$fixture = New-SourceCopy
$fixture.items[0].worldInteractionRequirements[0].resolutionCode = 'CandidateNeedsReview'
Expect-ValidationFailure 'premature-development' $fixture 'DevelopmentReadyRequiredUnresolved'

$fixture = New-SourceCopy
$fixture.items[0].documentExpectedSha256 = ('0' * 64)
Expect-ValidationFailure 'stale-document' $fixture 'LineDocumentHashMismatch'

Write-Output 'HexagramLinePlanningRequirementsTests:OK:Cases=13'
