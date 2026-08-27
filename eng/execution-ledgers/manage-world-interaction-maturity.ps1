[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $InputPath = "eng/execution-ledgers/world-interaction-maturity.json",
    [string] $OutputPath = "docs/AI/generated/world-interaction-maturity.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "WorldInteractionMaturityInvalid:$Code" }
}

function Resolve-AllowedSources([object] $Item, [object] $TriggerCatalog) {
    $override = $TriggerCatalog.overrides.PSObject.Properties[[string] $Item.id]
    if ($null -ne $override) { return @($override.Value) }
    $default = $TriggerCatalog.defaultAllowedByInteractionKind.PSObject.Properties[
        [string] $Item.kind]
    Require ($null -ne $default) "TriggerSourceDefaultMissing:$($Item.id)"
    return @($default.Value)
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = (Resolve-Path (Join-Path $repositoryRoot $InputPath)).Path
$ledger = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 | ConvertFrom-Json
$catalog = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.worldInteractionCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$triggerCatalog = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.triggerSourceCatalogPath)) -Raw -Encoding UTF8 | ConvertFrom-Json
$spatialLedger = Get-Content -LiteralPath (Join-Path $repositoryRoot ([string] $ledger.selectedSpatialBindingPath)) -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $ledger.schemaVersion -eq "world-interaction-maturity.v1") "SchemaInvalid"
$worldInteractionCount = @($catalog.items).Count
Require ($worldInteractionCount -gt 0) "WorldInteractionCountInvalid"
Require ([bool] $ledger.principles.worldInteractionOwnsEvidenceMaturity) "WiMaturityPrincipleMissing"
Require ([bool] $ledger.principles.spatialEvidenceIsConditional) "ConditionalSpatialPrincipleMissing"
Require ([bool] $ledger.principles.clientCannotChooseTriggerSource) "TrustedTriggerPrincipleMissing"

$expectedSelected = @(
    "WI-ACTOR-01", "WI-ACTOR-02",
    "WI-FARM-04", "WI-FARM-05", "WI-FARM-06",
    "WI-NATURE-01", "WI-NATURE-02", "WI-NATURE-03", "WI-NATURE-04",
    "WI-NATURE-05", "WI-NATURE-06", "WI-NATURE-07", "WI-NATURE-08",
    "WI-NATURE-09", "WI-NATURE-10", "WI-NATURE-11", "WI-NATURE-12",
    "WI-NATURE-13", "WI-NATURE-14", "WI-NATURE-15", "WI-NATURE-16",
    "WI-NATURE-17", "WI-NATURE-18", "WI-CON-01")
$selectedIds = @($ledger.selectedRuntimeWorldInteractionIds)
Require (($selectedIds -join ",") -eq ($expectedSelected -join ",")) "SelectedRuntimeIdsInvalid"
Require (@($ledger.bindings).Count -eq 24) "SelectedBindingCountMustBe24"

$itemsById = @{}
foreach ($item in @($catalog.items)) { $itemsById[[string] $item.id] = $item }
$spatialBindingIds = @{}
foreach ($binding in @($spatialLedger.bindings)) { $spatialBindingIds[[string] $binding.bindingId] = $true }
$bindingsById = @{}
$requiredContexts = @("Initiator", "Actor", "Target", "DataResource", "Time")
$allowedE4 = @("ContextUnbound", "ContextPartiallyBound", "ContextBound")
$allowedE5 = @("ManifestationMissing", "ManifestationPartial", "Manifested")

foreach ($binding in @($ledger.bindings)) {
    $id = [string] $binding.worldInteractionId
    Require ($itemsById.ContainsKey($id)) "WorldInteractionUnknown:$id"
    Require (-not $bindingsById.ContainsKey($id)) "BindingDuplicate:$id"
    $bindingsById[$id] = $binding
    $allowedSources = @(Resolve-AllowedSources $itemsById[$id] $triggerCatalog)
    foreach ($source in @($binding.boundTriggerSourceCodes)) {
        Require ($allowedSources -contains [string] $source) "TriggerSourceNotAllowed:${id}:$source"
    }
    foreach ($context in $requiredContexts) {
        Require (@($binding.boundContextCodes) -contains $context) "ContextMissing:${id}:$context"
    }
    $spatialApplicability = [string] $binding.spatialApplicabilityCode
    Require ($spatialApplicability -in @("Required", "NotApplicable")) `
        "SpatialApplicabilityInvalid:$id"
    if ($spatialApplicability -eq "Required") {
        Require (@($binding.spatialEvidenceRefs).Count -gt 0) "SpatialEvidenceRefMissing:$id"
        foreach ($reference in @($binding.spatialEvidenceRefs)) {
            Require ($spatialBindingIds.ContainsKey([string] $reference)) "SpatialEvidenceRefUnknown:${id}:$reference"
        }
        Require (@($binding.boundContextCodes) -contains "Spatial") "SpatialContextMissing:$id"
        Require ([string] $binding.spatialEvidenceStateCode -eq "Bound") "SpatialEvidenceStateInvalid:$id"
    }
    else {
        Require (@($itemsById[$id].spatialRequirements).Count -eq 0) `
            "SpatialNotApplicableWithRequirements:$id"
        Require (@($binding.spatialEvidenceRefs).Count -eq 0) "SpatialEvidenceUnexpected:$id"
        Require ([string] $binding.spatialEvidenceStateCode -eq "NotRequired") `
            "SpatialNotRequiredStateInvalid:$id"
    }
    $expectedE4 = if ($spatialApplicability -eq "NotApplicable" -or
        ([string] $binding.spatialEvidenceStateCode -eq "Bound" -and
            @($binding.boundContextCodes) -contains "Spatial")) { "ContextBound" }
        else { "ContextPartiallyBound" }
    Require ([string] $binding.e4StateCode -eq $expectedE4) "E4StateInvalid:$id"
    Require ($allowedE4 -contains [string] $binding.e4StateCode) "E4StateUnknown:$id"
    Require ($allowedE5 -contains [string] $binding.e5StateCode) "E5StateUnknown:$id"
}

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# WI E4·E5 성숙도 대장")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``$InputPath``와 ${worldInteractionCount}개 WI 정의에서 자동 생성된다. 직접 수정하지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 대장 개정: ``$($ledger.revision)``")
[void] $builder.AppendLine("- 전체 WI: ``$(@($catalog.items).Count)``")
[void] $builder.AppendLine("- 1차 Runtime 결속 WI: ``$($bindingsById.Count)``")
[void] $builder.AppendLine("- 공간은 WI가 Required일 때만 E4 문맥과 E5 추가 증거로 사용한다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 한국어 기능명 · 고유 식별자 | 대장 순번 | 허용 발생원 | 실제 결속 | 공간 | E4 | E5 |")
[void] $builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |")
foreach ($item in @($catalog.items | Sort-Object groupCode, sequence)) {
    $id = [string] $item.id
    $groupDisplayName = [string] $catalog.groupDisplayNames.PSObject.Properties[[string] $item.groupCode].Value
    $displayName = "$groupDisplayName · $($item.title) · ``$id``"
    $allowed = @(Resolve-AllowedSources $item $triggerCatalog) -join ", "
    if ($bindingsById.ContainsKey($id)) {
        $binding = $bindingsById[$id]
        [void] $builder.AppendLine("| $displayName | $($item.sequence) | $allowed | $(@($binding.boundTriggerSourceCodes) -join ', ') | $($binding.spatialEvidenceStateCode) | $($binding.e4StateCode) | $($binding.e5StateCode) |")
    }
    else {
        $spatial = if (@($item.spatialRequirements).Count -gt 0) {
            [string] $ledger.defaultUnselectedState.spatialEvidenceStateCode
        } else { "NotApplicable" }
        [void] $builder.AppendLine("| $displayName | $($item.sequence) | $allowed | 미결속 | $spatial | $($ledger.defaultUnselectedState.e4StateCode) | $($ledger.defaultUnselectedState.e5StateCode) |")
    }
}

$content = ConvertTo-DeterministicText $builder.ToString()
$resolvedOutput = Join-Path $repositoryRoot $OutputPath
if ($Mode -eq "Write") {
    [void] (Write-DeterministicTextIfChanged -Path $resolvedOutput -Content $content)
}
else {
    Require (Test-Path -LiteralPath $resolvedOutput) "GeneratedOutputMissing"
    $current = ConvertTo-DeterministicText ([IO.File]::ReadAllText($resolvedOutput))
    Require ($current -ceq $content) "GeneratedOutputMismatch"
}

Write-Output "WorldInteractionMaturityValid:$worldInteractionCount;Selected=$(@($ledger.selectedRuntimeWorldInteractionIds).Count);Revision=$($ledger.revision)"
