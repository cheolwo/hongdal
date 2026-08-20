$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-h5-world-layout.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/generated/h5-world-layout.v1.json"

$write = & pwsh -NoProfile -File $manager -Mode Write
if ($write -notmatch "H5WorldLayoutGenerated:AreaSets=4;Corridors=3;Grounding=Optional/NotApplied") { throw "H5WorldLayoutWriteFailed" }
$beforeHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
if ($check -notmatch "H5WorldLayoutValid:AreaSets=4;Corridors=3;Grounding=Optional/NotApplied") { throw "H5WorldLayoutCheckFailed" }
$afterHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
if ($beforeHash -ne $afterHash -or $beforeTicks -ne $afterTicks) { throw "H5WorldLayoutNonDeterministic" }

$result = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
$definition = $result.worldLayoutDefinition
$binding = $result.worldGroundingBinding
$readiness = $result.groundingReadiness
if ($definition.coordinateSpaceCode -ne "ScenarioLocalMeters" -or $definition.areaSetInstances.Count -ne 4 -or $definition.corridorInstances.Count -ne 3) { throw "H5WorldLayoutDefinitionInvalid" }
if (@($definition.areaSetInstances | Where-Object { $_.placementTransform.coordinateSpaceCode -ne "ScenarioLocalMeters" }).Count -ne 0) { throw "H5AreaCoordinateSpaceInvalid" }
if (@($definition.areaSetInstances.graphInstances | Where-Object { $_.placementTransform.coordinateSpaceCode -ne "ParentLocalMeters" }).Count -ne 0) { throw "H4GraphCoordinateSpaceInvalid" }
if ($binding.placementAuthorityCode -ne "ScenarioRelative" -or $binding.worldGroundingStateCode -ne "NotApplied" -or -not [string]::IsNullOrEmpty($binding.e6AnchorStableId)) { throw "H5GroundingBindingInvalid" }
if ($readiness.groundingReadinessStateCode -ne "Partial" -or $readiness.appliesAuthority) { throw "H5GroundingReadinessInvalid" }
if (-not $result.authorityBoundary.e6CannotRewriteLayout -or -not $result.authorityBoundary.scenarioRelativeIsAuthoritative) { throw "H5AuthorityBoundaryInvalid" }

Write-Output "H5WorldLayoutTestsPassed:AreaSets=4;Corridors=3;Deterministic=True"
