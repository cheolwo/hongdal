$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$managerPath = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-world-interaction-gwae-classifications.ps1'

& pwsh -NoProfile -File $managerPath -Mode Check
if ($LASTEXITCODE -ne 0) {
    throw "WorldInteractionGwaeClassificationCheckFailed:$LASTEXITCODE"
}

$catalog = Get-Content -Raw -Encoding UTF8 (Join-Path $repositoryRoot 'eng/execution-ledgers/world-interactions.json') | ConvertFrom-Json
$output = Get-Content -Raw -Encoding UTF8 (Join-Path $repositoryRoot 'docs/AI/generated/world-interaction-gwae-classifications.json') | ConvertFrom-Json

$elementMap = @{}
foreach ($definition in @($output.gwaeDefinitions)) { $elementMap[[string] $definition.code] = [string] $definition.element }
if ($elementMap['TAE'] -ne '금' -or $elementMap['JIN'] -ne '목' -or $elementMap['GAM'] -ne '수' -or $elementMap['RI'] -ne '화' -or $elementMap['GAN'] -ne '토') { throw 'FiveElementGwaeMapInvalid' }

if (@($catalog.items).Count -ne @($output.items).Count) { throw 'WorldInteractionCoverageMismatch' }
if (@($output.items | Where-Object { [string]::IsNullOrWhiteSpace([string] $_.actionGwae) }).Count -ne 0) { throw 'ActionGwaeMissing' }
if (@($output.items | Where-Object { $_.targetMode -eq 'Fixed' -and [string]::IsNullOrWhiteSpace([string] $_.targetGwae) }).Count -ne 0) { throw 'FixedTargetGwaeMissing' }
if (@($output.items | Where-Object classificationStatus -notin @('ReviewedExplicit', 'ReviewedByMeaningRule')).Count -ne 0) { throw 'UnreviewedClassificationRemaining' }

$consume = @($output.items | Where-Object wiId -eq 'WI-ACTOR-CONSUME')
if ($consume.Count -ne 1 -or $consume[0].targetMode -ne 'InheritFromTargetObject' -or $consume[0].operationMode -ne 'InheritFromTargetObject') { throw 'ConsumeInheritanceInvalid' }

$woodcutting = @($output.items | Where-Object wiId -eq 'WI-NATURE-06')
if ($woodcutting.Count -ne 1) { throw 'WoodcuttingClassificationMissing' }
if ($woodcutting[0].actionGwae -ne 'JIN') { throw 'WoodcuttingActionGwaeInvalid' }
if ($woodcutting[0].operationGwae -ne 'TAE') { throw 'WoodcuttingOperationGwaeInvalid' }
if ($woodcutting[0].targetGwae -ne 'JIN') { throw 'WoodcuttingTargetGwaeInvalid' }
if ($woodcutting[0].supportGwae -ne 'GAN') { throw 'WoodcuttingSupportGwaeInvalid' }

$farmProduction = @($output.items | Where-Object wiId -in @('WI-FARM-01','WI-FARM-02','WI-FARM-03','WI-FARM-04','WI-FARM-05','WI-FARM-06'))
if ($farmProduction.Count -ne 6) { throw 'FarmProductionGwaeCoverageMissing' }
if (@($farmProduction | Where-Object classificationStatus -ne 'ReviewedExplicit').Count -ne 0) { throw 'FarmProductionGwaeExplicitReviewMissing' }
if (($farmProduction | Where-Object wiId -eq 'WI-FARM-02').operationGwae -ne 'TAE') { throw 'FarmSowingOperationGwaeInvalid' }
if (($farmProduction | Where-Object wiId -eq 'WI-FARM-03').actionGwae -ne 'GAN' -or ($farmProduction | Where-Object wiId -eq 'WI-FARM-03').operationGwae -ne 'GAM') { throw 'FarmCropCareOperationGwaeInvalid' }
if (($farmProduction | Where-Object wiId -eq 'WI-FARM-04').supportGwae -ne 'GAM') { throw 'FarmHarvestSupportGwaeInvalid' }
$farmHarvest = @($farmProduction | Where-Object wiId -eq 'WI-FARM-04')
if ($farmHarvest[0].actionGwae -ne 'TAE' -or $farmHarvest[0].targetGwae -ne 'JIN') { throw 'FarmHarvestPrimaryElementInvalid' }
if (@($farmHarvest[0].elementRelations).Count -ne 1 -or $farmHarvest[0].elementRelations[0].relationCode -ne 'KE' -or $farmHarvest[0].elementRelations[0].displayName -ne '금극목') { throw 'FarmHarvestElementRelationInvalid' }
$farmCare = @($farmProduction | Where-Object wiId -eq 'WI-FARM-03')
if ((@($farmCare[0].elementRelations.displayName) -join ',') -ne '수생목,토극수,금극목,목생화') { throw 'FarmCropCareElementRelationsInvalid' }
if ((@($farmCare[0].elementRelations.applicationMode) -join ',') -ne 'RequiredInput,RequiredConstraint,ConditionalCare,Outcome') { throw 'FarmCropCareElementRelationModesInvalid' }

$recovery = @($output.items | Where-Object wiId -eq 'WI-NATURE-04')
if ($recovery.Count -ne 1) { throw 'RecoveryClassificationMissing' }
if ($recovery[0].actionGwae -ne 'GAN' -or $recovery[0].targetGwae -ne 'GAN') { throw 'RecoveryGwaeInvalid' }

$plan = @($output.items | Where-Object wiId -eq 'WI-ACTOR-PLAN-SET')
if ($plan.Count -ne 1 -or $plan[0].actionGwae -ne 'GAN' -or $plan[0].targetGwae -ne 'GAN') { throw 'PersonalPlanGwaeInvalid' }

$blueprint = @($output.items | Where-Object wiId -eq 'WI-CON-BLUEPRINT-PLACE')
if ($blueprint.Count -ne 1 -or $blueprint[0].actionGwae -ne 'GAN' -or $blueprint[0].operationGwae -ne 'TAE') { throw 'BlueprintGwaeInvalid' }

$heatSource = @($output.items | Where-Object wiId -eq 'WI-HEAT-SOURCE-STATE-CHANGE')
if ($heatSource.Count -ne 1 -or $heatSource[0].operationMode -ne 'ByActionCode') { throw 'HeatSourceOperationModeInvalid' }

$regeneration = @($output.items | Where-Object wiId -eq 'WI-WORLD-RESOURCE-REGENERATE')
if ($regeneration.Count -ne 1 -or $regeneration[0].subjectKind -ne 'World') { throw 'WorldRegenerationSubjectInvalid' }

Write-Output "WorldInteractionGwaeClassificationTests:Passed:$(@($output.items).Count)"
