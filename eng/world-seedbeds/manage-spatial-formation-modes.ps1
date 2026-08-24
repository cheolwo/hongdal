$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$path = Join-Path $repositoryRoot "eng/world-seedbeds/spatial-formation-modes.v1.json"
$catalog = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json

if ($catalog.schemaVersion -ne "simulation-spatial-formation-modes.v1") { throw "SpatialFormationModesInvalid:SchemaVersion" }
$modeCodes = @($catalog.formationModes.code)
if (($modeCodes -join ",") -ne "LhComposed,PlayerComposed,HybridEvolving") { throw "SpatialFormationModesInvalid:ModeOrder" }
$hLevels = @($catalog.hLevelPolicies.hLevelCode)
if (($hLevels -join ",") -ne "H1,H2,H3,H4") { throw "SpatialFormationModesInvalid:HLevelOrder" }
if ($catalog.hLevelPolicies[0].playerComposition -ne "DirectConfirmAllowed") { throw "SpatialFormationModesInvalid:H1PlayerPolicy" }
if ($catalog.hLevelPolicies[1].playerComposition -ne "QualifiedFromH1Set") { throw "SpatialFormationModesInvalid:H2PlayerPolicy" }
if ($catalog.hLevelPolicies[2].playerComposition -ne "QualifiedFromH2Loop") { throw "SpatialFormationModesInvalid:H3PlayerPolicy" }
if ($catalog.hLevelPolicies[3].playerComposition -ne "InternalEvolutionOnly") { throw "SpatialFormationModesInvalid:H4PlayerPolicy" }
foreach ($transition in $catalog.allowedTransitions) {
    if ($modeCodes -notcontains $transition.from -or $modeCodes -notcontains $transition.to) { throw "SpatialFormationModesInvalid:UnknownTransitionMode" }
}
foreach ($requiredField in @("FormationModeCode", "SpatialInstanceStableId", "SourceHDefinitionRefs", "FormationCommandId", "FormedAtWorldRevision")) {
    if (@($catalog.runtimeLineageFields) -notcontains $requiredField) { throw "SpatialFormationModesInvalid:LineageFieldMissing:$requiredField" }
}
if (-not [bool] $catalog.principles.runtimeCompositionDoesNotRewriteHDefinitions) { throw "SpatialFormationModesInvalid:HDefinitionMutationMustBeForbidden" }

Write-Output "SpatialFormationModesValid:Modes=3;Levels=H1-H4;PlayerDirect=H1"
