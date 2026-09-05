$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path | Split-Path -Parent
$manager = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-hexagram-story-production.ps1'
$sourcePath = Join-Path $repositoryRoot 'eng/execution-ledgers/hexagram-story-production.json'

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "HexagramStoryProductionTestFailed:$Code" }
}

function New-SourceCopy {
    return (Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8 | ConvertFrom-Json)
}

function Expect-ValidationFailure([string] $CaseName, [object] $Fixture, [string] $ExpectedCode) {
    $tempPath = Join-Path ([IO.Path]::GetTempPath()) ("mirror-hexagram-{0}-{1}.json" -f $CaseName, [guid]::NewGuid().ToString('N'))
    try {
        $json = ($Fixture | ConvertTo-Json -Depth 40) + "`n"
        [IO.File]::WriteAllText($tempPath, $json, [Text.UTF8Encoding]::new($false))
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
$zhun = @($source.hexagrams | Where-Object stableId -eq 'HEX-03-ZHUN')
Require ($zhun.Count -eq 1) 'ZhunMissing'
Require ((@($zhun[0].lineStories | ForEach-Object primaryStoryBeatRef) -join ',') -match 'establish-guest-place-through-fence-repair') 'ZhunLine1Beat'
Require ((@($zhun[0].lineStories | ForEach-Object primaryStoryBeatRef) -join ',') -match 'receive-bounded-house-and-plot-authority') 'ZhunLine4Beat'
Require ((@($zhun[0].lineStories | ForEach-Object primaryStoryBeatRef) -join ',') -match 'restore-one-plot-and-farmhouse') 'ZhunLine5Beat'
Require (@($zhun[0].lineStories | Where-Object { @($_.planningRefs) -notcontains 'PLAN-STORY-HEX03-CAMPAIGN-001' }).Count -eq 0) 'ZhunCampaignPlanRef'

$fixture = New-SourceCopy
$fixture.hexagrams = @($fixture.hexagrams | Select-Object -First 63)
Expect-ValidationFailure 'count' $fixture 'HexagramCount:63'

$fixture = New-SourceCopy
$fixture.hexagrams[1].symbol = $fixture.hexagrams[0].symbol
Expect-ValidationFailure 'symbol' $fixture 'HexagramSymbolDuplicate'

$fixture = New-SourceCopy
$fixture.hexagrams[0].lineStories = @($fixture.hexagrams[0].lineStories | Select-Object -First 5)
Expect-ValidationFailure 'lines' $fixture 'LineStoryCount:HEX-01-QIAN:5'

$fixture = New-SourceCopy
$fixture.policy.formalAuthoringHexagramStableId = 'HEX-03-ZHUN'
Expect-ValidationFailure 'formal-cursor' $fixture 'FormalAuthoringCursor'

$fixture = New-SourceCopy
$fixture.policy.playerPlayOrderPolicyCode = 'StrictLinear'
Expect-ValidationFailure 'runtime-order' $fixture 'PlayerPlayOrderPolicy'

$fixture = New-SourceCopy
$fixture.hexagrams[2].specialCommentaries = @([pscustomobject]@{
    code = 'EXTRA'
    traditionalName = 'EXTRA'
    handlingCode = 'SupplementaryInterpretation'
    createsLineStory = $false
})
Expect-ValidationFailure 'special-line' $fixture 'UnexpectedSpecialCommentary:HEX-03-ZHUN'

$fixture = New-SourceCopy
$fixture.hexagrams[2].legacyMappings[0].mappingCode = 'Accepted'
Expect-ValidationFailure 'prototype-accepted' $fixture 'LockedAcceptedMapping:HEX-03-ZHUN'

$fixture = New-SourceCopy
$fixture.presentationE4Preparation.futureItemPolicyCode = 'ExposeAll'
Expect-ValidationFailure 'future-visible' $fixture 'PresentationFuturePolicy'

$fixture = New-SourceCopy
$fixture.policy.concreteStoryStartHexagramStableId = 'HEX-02-KUN'
Expect-ValidationFailure 'concrete-start' $fixture 'ConcreteStoryStart'

$fixture = New-SourceCopy
$fixture.policy.visualCompanionStartsAtHexagramStableId = 'HEX-04-MENG'
Expect-ValidationFailure 'visual-start' $fixture 'VisualCompanionStart'

$fixture = New-SourceCopy
$fixture.policy.planningIntakePolicyCode = 'StrictOnly'
Expect-ValidationFailure 'planning-intake' $fixture 'PlanningIntakePolicy'

$fixture = New-SourceCopy
$fixture.policy.broadStoryOutlineStatusCode = 'Approved'
Expect-ValidationFailure 'unapproved-outline-promotion' $fixture 'BroadStoryOutlineStatus'

$fixture = New-SourceCopy
$fixture.policy.lineAdaptationPolicyCode = 'RewriteOriginalToFitStory'
Expect-ValidationFailure 'original-rewrite' $fixture 'LineAdaptationPolicy'

$fixture = New-SourceCopy
$fixture.policy.runtimeCampaignStateCode = 'Active'
Expect-ValidationFailure 'runtime-boundary' $fixture 'RuntimeCampaignBoundary'

$fixture = New-SourceCopy
$fixture.hexagrams[0].lineStories[0].storySynopsis = ''
Expect-ValidationFailure 'prologue-synopsis' $fixture 'PrologueSynopsis:HEX-01-QIAN-L1'

$fixture = New-SourceCopy
$fixture.hexagrams[2].lineStories[0].linePlanId = 'PLAN-STORY-HEX03-LINE-999'
Expect-ValidationFailure 'line-plan-id' $fixture 'LinePlanId:HEX-03-ZHUN-L1'

Write-Output 'HexagramStoryProductionTests:OK:Cases=21'
