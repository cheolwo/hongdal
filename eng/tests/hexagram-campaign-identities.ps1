$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path | Split-Path -Parent
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-hexagram-campaign-identities.ps1'
$sourcePath = Join-Path $repositoryRoot 'eng/execution-ledgers/hexagram-campaign-identities.json'

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramCampaignIdentityTestFailed:$Code" }
}

function New-SourceCopy {
    return (Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json)
}

function Expect-ValidationFailure([string] $CaseName, [object] $Fixture, [string] $ExpectedCode) {
    $tempPath = Join-Path ([IO.Path]::GetTempPath()) ("mirror-hexagram-campaign-{0}-{1}.json" -f $CaseName, [guid]::NewGuid().ToString('N'))
    try {
        [IO.File]::WriteAllText($tempPath, (($Fixture | ConvertTo-Json -Depth 30) + "`n"), [Text.UTF8Encoding]::new($false))
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
$zhun = @($source.campaigns | Where-Object hexagramStableId -eq 'HEX-03-ZHUN')
Require ($zhun.Count -eq 1) 'ZhunCampaignMissing'
Require ([string] $zhun[0].subjectRelation -match '생활주택.+감자밭.+울타리') 'ZhunSubjectRelation'
Require ([string] $zhun[0].pressureModel -match '필연적 부분 손실') 'ZhunPressureModel'
Require ([string] $zhun[0].completionTransformation -match '공동 생활 거점') 'ZhunCompletionTransformation'

$fixture = New-SourceCopy
$fixture.campaigns = @($fixture.campaigns | Select-Object -First 63)
Expect-ValidationFailure 'count' $fixture 'CampaignCount:63'

$fixture = New-SourceCopy
$fixture.campaigns[1].hexagramStableId = $fixture.campaigns[0].hexagramStableId
Expect-ValidationFailure 'id' $fixture 'CampaignStableIdDuplicate'

$fixture = New-SourceCopy
$fixture.campaigns[2].campaignScaleCode = 'ShortPlayablePrologue'
Expect-ValidationFailure 'scale' $fixture 'ExpectedScale:HEX-03-ZHUN'

$fixture = New-SourceCopy
$fixture.campaigns[3].coreConflict = ''
Expect-ValidationFailure 'missing-field' $fixture 'coreConflict:HEX-04-MENG'

$fixture = New-SourceCopy
$fixture.campaigns[4].combinationFingerprint = $fixture.campaigns[5].combinationFingerprint
Expect-ValidationFailure 'fingerprint' $fixture 'FingerprintDuplicate'

$fixture = New-SourceCopy
$fixture.campaigns[6].coreConflict = $fixture.campaigns[7].coreConflict
$fixture.campaigns[6].subjectRelation = $fixture.campaigns[7].subjectRelation
$fixture.campaigns[6].pressureModel = $fixture.campaigns[7].pressureModel
$fixture.campaigns[6].signatureRuleCombination = $fixture.campaigns[7].signatureRuleCombination
Expect-ValidationFailure 'combination' $fixture 'IdentityCombinationDuplicate'

$fixture = New-SourceCopy
$fixture.policy.doesNotPromoteEvidence = $false
Expect-ValidationFailure 'evidence-boundary' $fixture 'EvidenceBoundary'

Write-Output 'HexagramCampaignIdentityTests:OK:Cases=12'
