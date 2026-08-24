[CmdletBinding()]
param(
    [string] $InputPath = "eng/execution-ledgers/e9-refactor-module-catalog.json"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "E9RefactorModuleCatalogInvalid:$Code" }
}

function Require-Text([object] $Value, [string] $Code) {
    Require (-not [string]::IsNullOrWhiteSpace([string] $Value)) $Code
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$resolvedInput = (Resolve-Path (Join-Path $repositoryRoot $InputPath)).Path
$catalog = Get-Content -LiteralPath $resolvedInput -Raw -Encoding UTF8 |
    ConvertFrom-Json

Require ([string] $catalog.schemaVersion -eq
    "simulation-e9-refactor-module-catalog.v1") "SchemaInvalid"
Require-Text $catalog.revision "RevisionMissing"
Require ([string] $catalog.defaultModuleStatus -eq "Named") "DefaultStatusMustBeNamed"
Require ((@($catalog.downwardReviewOrder) -join ",") -eq
    "E9,E8,E7,E6,E5,E4,E3,E2,E1") "DownwardOrderInvalid"
Require ((@($catalog.upwardAssemblyOrder) -join ",") -eq
    "E1,E2,E3,E4,E5,E6,E7,E8,E9") "UpwardOrderInvalid"

foreach ($principle in @(
    "namesArePlanningSlotsNotCompletionClaims",
    "downwardAndUpwardReuseSameModules",
    "downwardAndUpwardCyclesMayRepeat",
    "detailsAreAddedOnlyAfterReview",
    "modulesDoNotReplaceEvidenceStages",
    "modulesDoNotReplaceManagementSystems",
    "modulesDoNotReplaceSpatialHierarchy")) {
    $property = $catalog.principles.PSObject.Properties[$principle]
    Require ($null -ne $property -and [bool] $property.Value) "PrincipleMissing:$principle"
}

$modules = @($catalog.modules)
Require (($modules.evidenceStage -join ",") -eq
    (@($catalog.downwardReviewOrder) -join ",")) "ModuleOrderInvalid"
Require (@($modules.technicalName | Select-Object -Unique).Count -eq 9) "TechnicalNameDuplicate"
Require (@($modules.koreanName | Select-Object -Unique).Count -eq 9) "KoreanNameDuplicate"

foreach ($module in $modules) {
    $stage = [string] $module.evidenceStage
    Require-Text $module.koreanName "KoreanNameMissing:$stage"
    Require-Text $module.technicalName "TechnicalNameMissing:$stage"
    Require ([string] $module.technicalName -match "^E[1-9][\p{L}\p{Nd}]+Module$") "TechnicalNameInvalid:$stage"
    Require ([string] $module.technicalName -match "^$stage") "TechnicalNameStageMismatch:$stage"
    Require (@($catalog.allowedModuleStatuses) -contains [string] $module.status) "StatusInvalid:$stage"
    Require ([string] $module.status -eq "Named") "SkeletonMustRemainNamed:$stage"
    Require-Text $module.downwardRole "DownwardRoleMissing:$stage"
    Require-Text $module.upwardRole "UpwardRoleMissing:$stage"
    Require (@($module.namedSlots).Count -gt 0) "NamedSlotsMissing:$stage"
    Require (@($module.namedSlots | Select-Object -Unique).Count -eq
        @($module.namedSlots).Count) "NamedSlotDuplicate:$stage"
    Require (@($module.namedSlots | Where-Object { [string] $_ -match "\p{IsHangulSyllables}" }).Count -eq
        @($module.namedSlots).Count) "NamedSlotMustBeKoreanCentered:$stage"

    $expectedManagement = if ($stage -eq "E9") { "G4" }
        elseif ($stage -eq "E8") { "G3" }
        elseif ($stage -eq "E7") { "G2" }
        else { "G1" }
    Require ([string] $module.managementSystem -eq $expectedManagement) "ManagementRoutingInvalid:$stage"
}

Write-Output (
    "E9RefactorModuleCatalogValid:Modules=9;Status=Named;Cycle=E9-E1-E9-Repeat")
