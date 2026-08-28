$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/world-seedbeds/manage-h5-world-layout.ps1"
$output = Join-Path $repositoryRoot "eng/world-seedbeds/generated/h5-world-layout.v1.json"
$policyPath = Join-Path $repositoryRoot "eng/world-seedbeds/h5-world-layout-policy.v1.json"

$policy = Get-Content -LiteralPath $policyPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($policy.worldGroundingPolicyCode -ne "Optional" -or
    $policy.realityGroundingDefaults.applicationStateCode -ne "NotApplied" -or
    [bool] $policy.realityGroundingDefaults.requiredForScenarioExecution -or
    @($policy.realityGroundingDefaults.globalRequiredEvidencePurposeCodes).Count -ne 0) {
    throw "H5OptionalRealityGroundingDefaultsInvalid"
}

$write = & pwsh -NoProfile -File $manager -Mode Write
if ($write -notmatch "H5WorldLayoutGenerated:AreaSets=4;Anchors=5;Corridors=3;Reserved=1;Grounding=Optional/NotApplied") { throw "H5WorldLayoutWriteFailed" }
$beforeHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$beforeTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
$check = & pwsh -NoProfile -File $manager -Mode Check
if ($check -notmatch "H5WorldLayoutValid:AreaSets=4;Anchors=5;Corridors=3;Reserved=1;Grounding=Optional/NotApplied") { throw "H5WorldLayoutCheckFailed" }
$afterHash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
$afterTicks = (Get-Item -LiteralPath $output).LastWriteTimeUtc.Ticks
if ($beforeHash -ne $afterHash -or $beforeTicks -ne $afterTicks) { throw "H5WorldLayoutNonDeterministic" }

$result = Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
$definition = $result.worldLayoutDefinition
$binding = $result.worldGroundingBinding
$readiness = $result.groundingReadiness
if ($definition.coordinateSpaceCode -ne "ScenarioLocalMeters" -or $definition.areaSetInstances.Count -ne 4 -or $definition.areaAnchors.Count -ne 5 -or $definition.corridorInstances.Count -ne 3 -or $definition.reservedConnections.Count -ne 1) { throw "H5WorldLayoutDefinitionInvalid" }
if (@($definition.areaSetInstances | Where-Object { $_.placementTransform.coordinateSpaceCode -ne "ScenarioLocalMeters" }).Count -ne 0) { throw "H5AreaCoordinateSpaceInvalid" }
if (@($definition.areaSetInstances.graphInstances | Where-Object { $_.placementTransform.coordinateSpaceCode -ne "ParentLocalMeters" }).Count -ne 0) { throw "H4GraphCoordinateSpaceInvalid" }
$hub = @($definition.areaSetInstances | Where-Object areaSetInstanceStableId -eq "area-set:sim:pyeongchang:logistics-hub.v1")
if ($hub.Count -ne 1 -or $hub[0].areaRoleCode -ne "Hub" -or @($hub[0].legacyAreaRoleCodes) -notcontains "CityHub") { throw "H5HubCompatibilityInvalid" }
$city = @($definition.areaAnchors | Where-Object canonicalAreaRoleCode -eq "City")
if ($city.Count -ne 1 -or $city[0].placementStateCode -ne "Reserved" -or $city[0].canTraverse -or $city[0].canActivate -or -not $city[0].canPrefetchMetadata) { throw "H5ReservedCityInvalid" }
if ([Math]::Abs([double]$city[0].fixedPlacementTransform.localXMeters - -980.157889) -gt 0.000001 -or [Math]::Abs([double]$city[0].fixedPlacementTransform.localZMeters - -2971.799236) -gt 0.000001) { throw "H5ReservedCityPositionInvalid" }
$roles = @($definition.areaAnchors.canonicalAreaRoleCode | Sort-Object -Unique)
if ($roles.Count -ne 5 -or @($roles) -notcontains "NatureHome" -or @($roles) -notcontains "Farm" -or @($roles) -notcontains "Hub" -or @($roles) -notcontains "Town" -or @($roles) -notcontains "City") { throw "H5AreaAnchorRolesInvalid" }
if ($binding.placementAuthorityCode -ne "ScenarioRelative" -or $binding.worldGroundingStateCode -ne "NotApplied" -or -not [string]::IsNullOrEmpty($binding.e6AnchorStableId)) { throw "H5GroundingBindingInvalid" }
if ($readiness.groundingReadinessStateCode -ne "Partial" -or $readiness.appliesAuthority) { throw "H5GroundingReadinessInvalid" }
if (-not $result.authorityBoundary.e6CannotRewriteLayout -or -not $result.authorityBoundary.scenarioRelativeIsAuthoritative) { throw "H5AuthorityBoundaryInvalid" }

Write-Output "H5WorldLayoutTestsPassed:AreaSets=4;Anchors=5;Corridors=3;Reserved=1;Deterministic=True"
