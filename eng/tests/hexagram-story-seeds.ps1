$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path | Split-Path -Parent
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-hexagram-story-seeds.ps1'
$sourcePath = Join-Path $repositoryRoot 'eng/execution-ledgers/hexagram-story-seeds.json'

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramStorySeedsTestFailed:$Code" }
}

function New-SourceCopy {
    return (Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json)
}

function Expect-Failure([string] $Name, [object] $Fixture, [string] $ExpectedCode) {
    $tempPath = Join-Path ([IO.Path]::GetTempPath()) ("hexagram-story-seeds-$Name-$([guid]::NewGuid().ToString('N')).json")
    try {
        [IO.File]::WriteAllText($tempPath, (($Fixture | ConvertTo-Json -Depth 30) + "`n"), [Text.UTF8Encoding]::new($false))
        $failed = $false
        try { & $manager -Mode Validate -InputPath $tempPath | Out-Null } catch {
            $failed = $true
            Require ($_.Exception.Message.Contains($ExpectedCode)) "$Name`:Unexpected:$($_.Exception.Message)"
        }
        Require $failed "$Name`:Accepted"
    } finally {
        if (Test-Path -LiteralPath $tempPath) { Remove-Item -LiteralPath $tempPath -Force }
    }
}

function Expect-Success([string] $Name, [object] $Fixture) {
    $tempPath = Join-Path ([IO.Path]::GetTempPath()) ("hexagram-story-seeds-$Name-$([guid]::NewGuid().ToString('N')).json")
    try {
        [IO.File]::WriteAllText($tempPath, (($Fixture | ConvertTo-Json -Depth 30) + "`n"), [Text.UTF8Encoding]::new($false))
        & $manager -Mode Validate -InputPath $tempPath | Out-Null
    } finally {
        if (Test-Path -LiteralPath $tempPath) { Remove-Item -LiteralPath $tempPath -Force }
    }
}

& $manager -Mode Validate | Out-Null
& $manager -Mode Check | Out-Null

$fixture = New-SourceCopy
$fixture.seeds = @($fixture.seeds | Select-Object -First 1)
$fixture.seeds[0].classificationStateCode = 'StorySeed'
$fixture.seeds[0].primaryHexagramStableId = ''
$fixture.seeds[0].primaryReason = ''
$fixture.seeds[0].secondaryCandidates = @()
$fixture.seeds[0].subjectSummary = ''
$fixture.seeds[0].situationSummary = ''
$fixture.seeds[0].transformationSummary = ''
$fixture.seeds[0].resultOrNextStateSummary = ''
$fixture.seeds[0].lineClassificationStateCode = 'NotStarted'
$fixture.seeds[0].lineDeferralReason = ''
$fixture.seeds[0].userConfirmationRef = ''
$fixture.seeds[0].classificationHistory[0].stateCode = 'StorySeed'
$fixture.seeds[0].classificationHistory[0].primaryHexagramStableId = ''
Expect-Success 'minimal-story-seed' $fixture

$fixture = New-SourceCopy
$fixture.seeds[0].primaryHexagramStableId = 'HEX-99-NONE'
Expect-Failure 'unknown-primary' $fixture 'PrimaryHexagram:STORY-SEED-HANS-FARM-FOOTHOLD-001'

$fixture = New-SourceCopy
$fixture.seeds[0].secondaryCandidates[0].hexagramStableId = 'HEX-03-ZHUN'
Expect-Failure 'primary-secondary-duplicate' $fixture 'SecondaryMatchesPrimary:STORY-SEED-HANS-FARM-FOOTHOLD-001'

$fixture = New-SourceCopy
$fixture.seeds[0].userConfirmationRef = ''
Expect-Failure 'confirmation' $fixture 'UserConfirmation:STORY-SEED-HANS-FARM-FOOTHOLD-001'

$fixture = New-SourceCopy
$fixture.seeds[0].lineClassificationStateCode = 'Confirmed'
$fixture.seeds[0].classificationStateCode = 'LineConfirmed'
Expect-Failure 'confirmed-line' $fixture 'ConfirmedLine:STORY-SEED-HANS-FARM-FOOTHOLD-001'

$fixture = New-SourceCopy
$fixture.seeds[0].classificationHistory[0].stateCode = 'StorySeed'
Expect-Failure 'history-current' $fixture 'HistoryCurrentState:STORY-SEED-HANS-FARM-FOOTHOLD-001'

$fixture = New-SourceCopy
$fixture.policy.doesNotPromoteEvidence = $false
Expect-Failure 'evidence-boundary' $fixture 'PolicyFlag:doesNotPromoteEvidence'

Write-Output 'HexagramStorySeedsTests:OK:Cases=9'
