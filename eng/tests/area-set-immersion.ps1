$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$script = Join-Path $repositoryRoot "eng/world-seedbeds/manage-area-set-immersion.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/generated/area-set-immersion-readiness.v1.json"

$first = & $script -Mode Write
$firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$firstWrite = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$second = & $script -Mode Write
$secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $output).Hash
$secondWrite = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & $script -Mode Check

if ($firstHash -ne $secondHash) { throw "AreaSetImmersionGenerationIsNotDeterministic" }
if ($firstWrite -eq $secondWrite) {
    # 생성기는 명시적 Write에서 파일을 다시 쓸 수 있지만 내용은 같아야 한다.
}
$value = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
if ($value.spatialMaturityCode -ne "E5Qualified") { throw "AreaSetImmersionE5StateWasLost" }
if ($value.immersionMaturityCode -ne "ImmersionQualified") { throw "AreaSetImmersionNotQualified" }
if ($value.freshnessStateCode -ne "Current") { throw "AreaSetImmersionNotCurrent" }
if ($value.groundingStatusCode -ne "NotApplied") { throw "AreaSetImmersionGisMustRemainOptional" }
if ($value.e7GateStateCode -ne "Open") { throw "AreaSetImmersionE7GateNotOpen" }
if (@($value.h3Audits).Count -ne 4) { throw "AreaSetImmersionH3CountInvalid" }
if (@($value.h3Audits | Where-Object immersionMaturityCode -ne "ImmersionQualified").Count -ne 0) { throw "AreaSetImmersionH3Unqualified" }
if (@($value.crossH3Closures).Count -ne 3 -or @($value.crossH3Closures | Where-Object qualificationResultCode -ne "Pass").Count -ne 0) { throw "AreaSetImmersionClosureInvalid" }
if ($value.publicDataChangesSimulationRules -or $value.publicDataMovesSpatialDefinitions -or $value.runtimeValidated) { throw "AreaSetImmersionAuthorityBoundaryInvalid" }
if ($check -notmatch "AreaSetImmersionValid:H3=4;Closures=3;E7Gate=Open") { throw "AreaSetImmersionCheckDidNotComplete" }

Write-Output "AreaSetImmersionTestsPassed"
Write-Output $first
Write-Output $second
Write-Output $check
