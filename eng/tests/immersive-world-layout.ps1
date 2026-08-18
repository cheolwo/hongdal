$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($PSVersionTable.PSEdition -ne "Core") {
    & pwsh -NoProfile -File $PSCommandPath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    exit 0
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-immersive-world-layout.ps1"
$output = Join-Path $repositoryRoot "docs/AI/generated/immersive-world-layout.md"

& $manager -Mode Write | Out-Host
$beforeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & $manager -Mode Check
& $manager -Mode Write | Out-Host
$afterHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks

if ($check -notmatch "ImmersiveWorldLayoutValid:Instances=4;RiskBands=3;Apocalypse=Waiting") { throw "ImmersiveWorldLayoutCheckFailed" }
if ($beforeHash -ne $afterHash) { throw "ImmersiveWorldLayoutHashChanged" }
if ($beforeTicks -ne $afterTicks) { throw "ImmersiveWorldLayoutRewrittenWithoutChange" }

Write-Output "ImmersiveWorldLayoutTestsPassed"
