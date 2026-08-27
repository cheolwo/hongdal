[CmdletBinding()]
param(
    [ValidateSet("Validate", "Write")]
    [string] $Mode = "Validate",

    [string] $CatalogPath =
        "eng/execution-ledgers/playable-loop-presentation-validation-modules.json",

    [string] $PlayableLoopPath =
        "eng/execution-ledgers/playable-loops.json",

    [string] $OutputPath =
        "docs/AI/generated/playable-loop-presentation-validation.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PresentationValidationInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

function Escape-Cell([object] $Value) {
    return ([string] $Value).Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedCatalog = Join-Path $repositoryRoot $CatalogPath
$resolvedLoops = Join-Path $repositoryRoot $PlayableLoopPath
$catalog = Get-Content -LiteralPath $resolvedCatalog -Raw -Encoding UTF8 |
    ConvertFrom-Json
$loops = Get-Content -LiteralPath $resolvedLoops -Raw -Encoding UTF8 |
    ConvertFrom-Json
$documentText = $catalog.generatedDocumentText

Require ([string] $catalog.schemaVersion -eq
    "playable-loop-presentation-validation-modules.v1") "SchemaInvalid"
Require ([string] $catalog.trackCode -eq "Presentation") "TrackInvalid"
foreach ($propertyName in @(
    "titleKo", "sourceNoticeFormatKo", "catalogLabelKo",
    "commonCountLabelKo", "conditionalCountLabelKo",
    "playableUnitCountLabelKo", "commonGateHeadingKo",
    "commonGateHeaderKo", "profileHeadingKo", "profileHeaderKo",
    "commonOnlyKo")) {
    Require-Text $documentText.$propertyName "GeneratedTextMissing:$propertyName"
}
foreach ($principle in @(
    "commonModulesApplyToEveryPlayableUnit",
    "featureModulesAreSelectedByDeclaredCapability",
    "blockingFailurePreventsTrackPromotion",
    "warningDoesNotPromoteEvidence",
    "runtimeEvidenceDoesNotReplaceAutomatedPrecheck",
    "failureReopensEarliestOwningStage",
    "presentationValidationDoesNotMutateSimulationAuthority")) {
    $property = $catalog.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) `
        "PrincipleMissing:$principle"
}

$moduleByCode = @{}
foreach ($module in @($catalog.modules)) {
    $code = [string] $module.moduleCode
    Require-Text $code "ModuleCodeMissing"
    Require (-not $moduleByCode.ContainsKey($code)) "ModuleDuplicate:$code"
    Require-Text $module.displayNameKo "ModuleNameMissing:$code"
    Require (@($catalog.allowedEvidenceStageCodes) -contains
        [string] $module.evidenceStageCode) "StageInvalid:$code"
    Require (@($catalog.allowedApplicabilityCodes) -contains
        [string] $module.applicabilityCode) "ApplicabilityInvalid:$code"
    Require (@($catalog.allowedSeverityCodes) -contains
        [string] $module.severityCode) "SeverityInvalid:$code"
    Require (@($catalog.allowedAutomationLevelCodes) -contains
        [string] $module.automationLevelCode) "AutomationInvalid:$code"
    Require (@($module.reads).Count -gt 0) "ReadsMissing:$code"
    Require (@($module.failureReasonCodes).Count -gt 0) "FailureCodeMissing:$code"
    if ([string] $module.applicabilityCode -eq "Common") {
        Require (@($module.requiredFeatureCodes).Count -eq 0) `
            "CommonHasFeature:$code"
    }
    else {
        Require (@($module.requiredFeatureCodes).Count -gt 0) `
            "FeatureMissing:$code"
        foreach ($featureCode in @($module.requiredFeatureCodes)) {
            Require (@($catalog.featureCodes) -contains [string] $featureCode) `
                "FeatureUnknown:${code}:$featureCode"
        }
    }
    $moduleByCode[$code] = $module
}

foreach ($commonCode in @($catalog.commonModuleCodes)) {
    Require ($moduleByCode.ContainsKey([string] $commonCode)) `
        "CommonModuleUnknown:$commonCode"
    Require ([string] $moduleByCode[[string] $commonCode].applicabilityCode -eq
        "Common") "CommonModuleNotCommon:$commonCode"
}
foreach ($stageCode in @($catalog.allowedEvidenceStageCodes)) {
    Require (@($catalog.commonModuleCodes | Where-Object {
        [string] $moduleByCode[[string] $_].evidenceStageCode -eq
            [string] $stageCode }).Count -gt 0) "CommonStageMissing:$stageCode"
}

$loopById = @{}
foreach ($loop in @($loops.items)) {
    $loopById[[string] $loop.loopStableId] = $loop
}
$profileByLoopId = @{}
foreach ($profile in @($catalog.loopProfiles)) {
    $loopId = [string] $profile.loopStableId
    Require-Text $loopId "ProfileLoopMissing"
    Require (-not $profileByLoopId.ContainsKey($loopId)) `
        "ProfileDuplicate:$loopId"
    Require ($loopById.ContainsKey($loopId)) "ProfileLoopUnknown:$loopId"
    Require ([string] $loopById[$loopId].loopLevelCode -eq "PlayableUnit") `
        "ProfileNotPlayableUnit:$loopId"
    $features = @($profile.featureCodes | ForEach-Object { [string] $_ })
    Require ($features.Count -eq @($features | Sort-Object -Unique).Count) `
        "ProfileFeatureDuplicate:$loopId"
    foreach ($featureCode in $features) {
        Require (@($catalog.featureCodes) -contains $featureCode) `
            "ProfileFeatureUnknown:${loopId}:$featureCode"
        Require (@($catalog.modules | Where-Object {
            [string] $_.applicabilityCode -eq "Feature" -and
            @($_.requiredFeatureCodes) -contains $featureCode }).Count -gt 0) `
            "ProfileFeatureUnmapped:${loopId}:$featureCode"
    }
    $profileByLoopId[$loopId] = $profile
}
$resolvedModules = @{}
$playableUnits = @($loops.items | Where-Object loopLevelCode -eq "PlayableUnit")
foreach ($loop in $playableUnits) {
    $loopId = [string] $loop.loopStableId
    $features = if ($profileByLoopId.ContainsKey($loopId)) {
        @($profileByLoopId[$loopId].featureCodes | ForEach-Object { [string] $_ })
    }
    else { @() }
    $codes = [Collections.Generic.List[string]]::new()
    foreach ($commonCode in @($catalog.commonModuleCodes)) {
        $codes.Add([string] $commonCode)
    }
    foreach ($module in @($catalog.modules | Where-Object {
        [string] $_.applicabilityCode -eq "Feature"
    })) {
        $required = @($module.requiredFeatureCodes |
            ForEach-Object { [string] $_ })
        if (@($required | Where-Object { $features -notcontains $_ }).Count -eq 0) {
            $codes.Add([string] $module.moduleCode)
        }
    }
    $resolvedModules[$loopId] = @($codes | Sort-Object -Unique)
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# $($documentText.titleKo)")
[void] $builder.AppendLine()
[void] $builder.AppendLine(([string] $documentText.sourceNoticeFormatKo -f
    $CatalogPath, $PlayableLoopPath))
[void] $builder.AppendLine()
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.catalogLabelKo, $catalog.revision))
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.commonCountLabelKo, @($catalog.commonModuleCodes).Count))
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.conditionalCountLabelKo,
    @($catalog.modules | Where-Object applicabilityCode -eq "Feature").Count))
[void] $builder.AppendLine(('- {0}: `{1}`' -f
    $documentText.playableUnitCountLabelKo, $playableUnits.Count))
[void] $builder.AppendLine()
[void] $builder.AppendLine("## $($documentText.commonGateHeadingKo)")
[void] $builder.AppendLine()
[void] $builder.AppendLine([string] $documentText.commonGateHeaderKo)
[void] $builder.AppendLine("| --- | --- | --- | --- |")
foreach ($commonCode in @($catalog.commonModuleCodes)) {
    $module = $moduleByCode[[string] $commonCode]
    [void] $builder.AppendLine(('| {0} | `{1}` {2} | {3} | {4} |' -f
        (Escape-Cell $module.evidenceStageCode),
        (Escape-Cell $module.moduleCode),
        (Escape-Cell $module.displayNameKo),
        (Escape-Cell $module.automationLevelCode),
        (Escape-Cell $module.severityCode)))
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## $($documentText.profileHeadingKo)")
[void] $builder.AppendLine()
[void] $builder.AppendLine([string] $documentText.profileHeaderKo)
[void] $builder.AppendLine("| --- | --- | --- |")
foreach ($loop in @($playableUnits | Sort-Object priority, loopStableId)) {
    $loopId = [string] $loop.loopStableId
    $features = if ($profileByLoopId.ContainsKey($loopId)) {
        @($profileByLoopId[$loopId].featureCodes) -join ", "
    }
    else { [string] $documentText.commonOnlyKo }
    [void] $builder.AppendLine(('| `{0}` | {1} | {2} |' -f
        (Escape-Cell $loopId), (Escape-Cell $features),
        (Escape-Cell (@($resolvedModules[$loopId]) -join ", "))))
}

$generated = $builder.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    $directory = Split-Path -Parent $resolvedOutput
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    [IO.File]::WriteAllText($resolvedOutput, $generated,
        [Text.UTF8Encoding]::new($false))
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    $existing = Get-Content -LiteralPath $resolvedOutput -Raw -Encoding UTF8
    Require ($existing -eq $generated) "GeneratedOutputOutOfDate"
}

Write-Output ("PlayableLoopPresentationValidationValid:Modules={0};Profiles={1};PlayableUnits={2}" -f
    @($catalog.modules).Count, @($catalog.loopProfiles).Count,
    $playableUnits.Count)
