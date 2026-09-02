$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$managerPath = Join-Path $repositoryRoot 'eng/execution-ledgers/manage-world-interaction-gwae-classifications.ps1'

& pwsh -NoProfile -File $managerPath -Mode Check
if ($LASTEXITCODE -ne 0) {
    throw "WorldInteractionGwaeClassificationCheckFailed:$LASTEXITCODE"
}

$catalog = Get-Content -Raw -Encoding UTF8 (Join-Path $repositoryRoot 'eng/execution-ledgers/world-interactions.json') | ConvertFrom-Json
$output = Get-Content -Raw -Encoding UTF8 (Join-Path $repositoryRoot 'docs/AI/generated/world-interaction-gwae-classifications.json') | ConvertFrom-Json

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
