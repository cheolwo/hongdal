param(
    [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $repositoryRoot 'Ssalddel.Tests\Ssalddel.Tests.csproj'
$resultsDirectory = Join-Path $repositoryRoot 'artifacts\local\validation'
New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$trxFileName = "world-bank-p6b-live-$timestamp.trx"
$trxPath = Join-Path $resultsDirectory $trxFileName

$previousOptIn = $env:SSALDDEL_RUN_WORLD_BANK_LIVE
try {
    $env:SSALDDEL_RUN_WORLD_BANK_LIVE = '1'
    & dotnet test $testProject `
        --filter 'Category=ExternalLive' `
        --logger "trx;LogFileName=$trxFileName" `
        --results-directory $resultsDirectory `
        --blame-hang-timeout "${TimeoutSeconds}s"
    if ($LASTEXITCODE -ne 0) {
        throw "World Bank P6-B live verification failed with exit code $LASTEXITCODE."
    }
    if (-not (Test-Path -LiteralPath $trxPath)) {
        throw "World Bank P6-B live verification did not produce a TRX result."
    }

    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
    $counters = $trx.SelectSingleNode("//*[local-name()='Counters']")
    if ($null -eq $counters -or [int]$counters.executed -ne 1 -or [int]$counters.passed -ne 1) {
        throw "World Bank P6-B live verification did not execute and pass exactly one test."
    }
    Write-Host "World Bank P6-B live verification passed. TRX: $trxPath"
}
finally {
    $env:SSALDDEL_RUN_WORLD_BANK_LIVE = $previousOptIn
}
