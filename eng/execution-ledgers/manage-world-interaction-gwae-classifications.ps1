param(
    [ValidateSet('Write', 'Check', 'Validate')]
    [string] $Mode = 'Validate',
    [string] $InputPath = 'eng/execution-ledgers/world-interaction-gwae-classifications.json',
    [string] $JsonOutputPath = 'docs/AI/generated/world-interaction-gwae-classifications.json',
    [string] $MarkdownOutputPath = 'docs/AI/generated/world-interaction-gwae-classifications.md'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

function Resolve-RepositoryPath([string] $Path) {
    return Join-Path $repositoryRoot $Path
}

function Require([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Read-Json([string] $Path) {
    $fullPath = Resolve-RepositoryPath $Path
    Require (Test-Path -LiteralPath $fullPath) "MissingFile:$Path"
    return Get-Content -Raw -Encoding UTF8 -LiteralPath $fullPath | ConvertFrom-Json
}

function Has-Property($Object, [string] $Name) {
    return $null -ne $Object.PSObject.Properties[$Name]
}

function Get-OptionalValue($Override, $Default, [string] $Name) {
    if ($null -ne $Override -and (Has-Property $Override $Name)) {
        return [string] $Override.$Name
    }
    if ($null -ne $Default -and (Has-Property $Default $Name)) {
        return [string] $Default.$Name
    }
    return $null
}

function Get-OptionalArray($Override, $Default, [string] $Name) {
    if ($null -ne $Override -and (Has-Property $Override $Name)) {
        return @($Override.$Name | ForEach-Object { [string] $_ })
    }
    if ($null -ne $Default -and (Has-Property $Default $Name)) {
        return @($Default.$Name | ForEach-Object { [string] $_ })
    }
    return @()
}

function ConvertTo-CanonicalJson($Value) {
    return ($Value | ConvertTo-Json -Depth 20) + "`n"
}

function Write-Utf8NoBom([string] $Path, [string] $Content) {
    $directory = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}

$source = Read-Json $InputPath
Require ([string] $source.schemaVersion -eq 'mirror-world-interaction-gwae-classifications.v1') 'SchemaInvalid'
$wiCatalog = Read-Json ([string] $source.worldInteractionCatalogPath)
Require ([string] $wiCatalog.schemaVersion -eq '5') 'WorldInteractionCatalogSchemaInvalid'

$definitionByCode = @{}
foreach ($definition in @($source.gwaeDefinitions)) {
    $code = [string] $definition.code
    Require (-not [string]::IsNullOrWhiteSpace($code)) 'GwaeCodeMissing'
    Require (-not $definitionByCode.ContainsKey($code)) "DuplicateGwaeCode:$code"
    Require ([string] $definition.symbol -in @('☳', '☱', '☲', '☵', '☶')) "GwaeSymbolInvalid:$code"
    Require ([string] $definition.element -in @('금', '목', '수', '화', '토')) "GwaeElementInvalid:$code"
    $definitionByCode[$code] = $definition
}
Require ($definitionByCode.Count -eq 5) 'GwaeDefinitionCountInvalid'

$relationByCode = @{}
foreach ($relation in @($source.elementRelationDefinitions)) {
    $relationCode = [string] $relation.code
    Require ($relationCode -in @('SHENG', 'KE')) "ElementRelationCodeInvalid:$relationCode"
    Require (-not $relationByCode.ContainsKey($relationCode)) "DuplicateElementRelationCode:$relationCode"
    Require (@($relation.sequence).Count -eq 5) "ElementRelationSequenceInvalid:$relationCode"
    $relationByCode[$relationCode] = $relation
}
Require ($relationByCode.Count -eq 2) 'ElementRelationDefinitionCountInvalid'

$overrideByWiId = @{}
foreach ($override in @($source.overrides)) {
    $wiId = [string] $override.wiId
    Require (-not [string]::IsNullOrWhiteSpace($wiId)) 'OverrideWiIdMissing'
    Require (-not $overrideByWiId.ContainsKey($wiId)) "DuplicateOverride:$wiId"
    $overrideByWiId[$wiId] = $override
}

$wiIds = @($wiCatalog.items | ForEach-Object { [string] $_.id })
foreach ($overrideId in $overrideByWiId.Keys) {
    Require ($overrideId -in $wiIds) "UnknownOverrideWiId:$overrideId"
}

$resolvedItems = [System.Collections.Generic.List[object]]::new()
foreach ($wi in @($wiCatalog.items)) {
    $groupCode = [string] $wi.groupCode
    $groupDefaultProperty = $source.groupDefaults.PSObject.Properties[$groupCode]
    Require ($null -ne $groupDefaultProperty) "GroupDefaultMissing:$groupCode"
    $groupDefault = $groupDefaultProperty.Value
    $override = if ($overrideByWiId.ContainsKey([string] $wi.id)) { $overrideByWiId[[string] $wi.id] } else { $null }

    $actionGwae = Get-OptionalValue $override $groupDefault 'actionGwae'
    $operationGwae = Get-OptionalValue $override $groupDefault 'operationGwae'
    $targetGwae = Get-OptionalValue $override $groupDefault 'targetGwae'
    $supportGwae = Get-OptionalValue $override $groupDefault 'supportGwae'
    $targetMode = Get-OptionalValue $override $groupDefault 'targetMode'
    if ([string]::IsNullOrWhiteSpace($targetMode)) { $targetMode = 'Fixed' }
    $operationMode = Get-OptionalValue $override $groupDefault 'operationMode'
    if ([string]::IsNullOrWhiteSpace($operationMode)) {
        $operationMode = if ([string]::IsNullOrWhiteSpace($operationGwae)) { 'None' } else { 'Fixed' }
    }
    $supportMode = Get-OptionalValue $override $groupDefault 'supportMode'
    if ([string]::IsNullOrWhiteSpace($supportMode)) {
        $supportMode = if ([string]::IsNullOrWhiteSpace($supportGwae)) { 'None' } else { 'Fixed' }
    }
    $additionalTargetGwae = Get-OptionalArray $override $groupDefault 'additionalTargetGwae'
    $operationGwaeByActionCode = if ($null -ne $override -and (Has-Property $override 'operationGwaeByActionCode')) { $override.operationGwaeByActionCode } else { $null }
    $elementRelations = if ($null -ne $override -and (Has-Property $override 'elementRelations')) { @($override.elementRelations) } else { @() }

    Require ($definitionByCode.ContainsKey($actionGwae)) "ActionGwaeInvalid:$($wi.id):$actionGwae"
    Require ($targetMode -in @('Fixed', 'InheritFromTargetObject', 'InheritFromActiveInteraction')) "TargetModeInvalid:$($wi.id):$targetMode"
    Require ($operationMode -in @('None', 'Fixed', 'InheritFromTargetObject', 'ByActionCode')) "OperationModeInvalid:$($wi.id):$operationMode"
    Require ($supportMode -in @('None', 'Fixed', 'Contextual')) "SupportModeInvalid:$($wi.id):$supportMode"
    if ($targetMode -eq 'Fixed') {
        Require ($definitionByCode.ContainsKey($targetGwae)) "TargetGwaeInvalid:$($wi.id):$targetGwae"
    }
    if ($operationMode -eq 'Fixed') {
        Require ($definitionByCode.ContainsKey($operationGwae)) "OperationGwaeInvalid:$($wi.id):$operationGwae"
    }
    if ($operationMode -eq 'ByActionCode') {
        Require ($null -ne $operationGwaeByActionCode) "OperationGwaeByActionCodeMissing:$($wi.id)"
        foreach ($property in $operationGwaeByActionCode.PSObject.Properties) {
            Require ($definitionByCode.ContainsKey([string] $property.Value)) "OperationGwaeByActionCodeInvalid:$($wi.id):$($property.Name)"
        }
    }
    foreach ($optionalCode in @($operationGwae, $supportGwae)) {
        if (-not [string]::IsNullOrWhiteSpace($optionalCode)) {
            Require ($definitionByCode.ContainsKey($optionalCode)) "OptionalGwaeInvalid:$($wi.id):$optionalCode"
        }
    }
    foreach ($additionalCode in $additionalTargetGwae) {
        Require ($definitionByCode.ContainsKey($additionalCode)) "AdditionalTargetGwaeInvalid:$($wi.id):$additionalCode"
    }
    foreach ($elementRelation in $elementRelations) {
        Require ($relationByCode.ContainsKey([string] $elementRelation.relationCode)) "ElementRelationTypeInvalid:$($wi.id)"
        Require ($definitionByCode.ContainsKey([string] $elementRelation.sourceGwae)) "ElementRelationSourceInvalid:$($wi.id)"
        Require ($definitionByCode.ContainsKey([string] $elementRelation.targetGwae)) "ElementRelationTargetInvalid:$($wi.id)"
        $expectedPair = "{0}>{1}" -f [string] $elementRelation.sourceGwae, [string] $elementRelation.targetGwae
        Require (@($relationByCode[[string] $elementRelation.relationCode].sequence) -contains $expectedPair) "ElementRelationPairInvalid:$($wi.id):$expectedPair"
        Require (-not [string]::IsNullOrWhiteSpace([string] $elementRelation.displayName)) "ElementRelationDisplayNameMissing:$($wi.id)"
        Require ([string] $elementRelation.applicationMode -in @('RequiredInput', 'RequiredConstraint', 'ConditionalCare', 'Outcome')) "ElementRelationApplicationModeInvalid:$($wi.id)"
        Require (-not [string]::IsNullOrWhiteSpace([string] $elementRelation.functionalMeaning)) "ElementRelationMeaningMissing:$($wi.id)"
    }

    if ($operationMode -ne 'Fixed') { $operationGwae = $null }
    if ($targetMode -ne 'Fixed') { $targetGwae = $null }

    $subjectKind = switch ([string] $wi.controlPolicyCode) {
        'PlayerDirect' { 'Player' }
        'NpcRoutine' { 'Npc' }
        'WorldAutomatic' { 'World' }
        default { 'Variable' }
    }

    $resolvedItems.Add([ordered]@{
        wiId = [string] $wi.id
        groupCode = $groupCode
        title = [string] $wi.title
        subjectKind = $subjectKind
        actionGwae = $actionGwae
        operationMode = $operationMode
        operationGwae = $operationGwae
        operationGwaeByActionCode = $operationGwaeByActionCode
        targetMode = $targetMode
        targetGwae = $targetGwae
        additionalTargetGwae = $additionalTargetGwae
        supportMode = $supportMode
        supportGwae = $supportGwae
        elementRelations = @($elementRelations)
        classificationStatus = if ($null -ne $override) { 'ReviewedExplicit' } else { 'ReviewedByMeaningRule' }
        reviewRule = Get-OptionalValue $override $groupDefault 'reviewRule'
        reason = if ($null -ne $override) { [string] $override.reason } else { "검토된 영역 의미 규칙 '$([string] $groupDefault.reviewRule)'을 적용했다." }
    })
}

Require ($resolvedItems.Count -eq $wiIds.Count) 'ResolvedCountMismatch'
Require (@($resolvedItems.wiId | Sort-Object -Unique).Count -eq $wiIds.Count) 'ResolvedWiIdDuplicate'

$output = [ordered]@{
    schemaVersion = 'mirror-world-interaction-gwae-classification-output.v1'
    sourceRevision = [string] $source.revision
    worldInteractionCatalogRevision = [string] $wiCatalog.revision
    classificationMeaning = [string] $source.classificationMeaning
    displayFormat = [string] $source.displayFormat
    gwaeDefinitions = @($source.gwaeDefinitions)
    elementRelationDefinitions = @($source.elementRelationDefinitions)
    counts = [ordered]@{
        total = $resolvedItems.Count
        reviewedExplicit = @($resolvedItems | Where-Object classificationStatus -eq 'ReviewedExplicit').Count
        reviewedByMeaningRule = @($resolvedItems | Where-Object classificationStatus -eq 'ReviewedByMeaningRule').Count
    }
    items = $resolvedItems
}
$json = ConvertTo-CanonicalJson $output

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('# WI 괘성 분류 목록')
$lines.Add('')
$lines.Add("- 분류 입력 판본: ``$($source.revision)``")
$lines.Add("- WI 대장 판본: ``$($wiCatalog.revision)``")
$lines.Add("- 전체: $($resolvedItems.Count), 개별 의미 명시 검토: $($output.counts.reviewedExplicit), 검토된 영역 의미 규칙 적용: $($output.counts.reviewedByMeaningRule)")
$lines.Add('- 이 목록은 기획 탐색용이며 WI 권위·구현 승인·E/G/H 성숙도와 실행 순서를 변경하지 않는다.')
$lines.Add('')
$lines.Add('| WI | 제목 | 행위괘 | 작용괘 | 대상괘 | 보조괘 | 오행 관계 | 상태 |')
$lines.Add('| --- | --- | --- | --- | --- | --- | --- | --- |')
foreach ($item in $resolvedItems) {
    function Format-Gwae([string] $Code) {
        if ([string]::IsNullOrWhiteSpace($Code)) { return '-' }
        $definition = $definitionByCode[$Code]
        return "$($definition.element)($($definition.name))"
    }
    $operationDisplay = switch ($item.operationMode) {
        'ByActionCode' { '작업 코드별' }
        'InheritFromTargetObject' { '대상 객체 승계' }
        default { Format-Gwae $item.operationGwae }
    }
    $targetDisplay = switch ($item.targetMode) {
        'InheritFromTargetObject' { '대상 객체 승계' }
        'InheritFromActiveInteraction' { '진행 WI 대상 승계' }
        default { Format-Gwae $item.targetGwae }
    }
    $supportDisplay = if ($item.supportMode -eq 'Contextual') { "상황별 $(Format-Gwae $item.supportGwae)" } else { Format-Gwae $item.supportGwae }
    $relationDisplay = if (@($item.elementRelations).Count -eq 0) { '-' } else { @($item.elementRelations | ForEach-Object { "$($_.displayName)[$($_.applicationMode)]: $($_.functionalMeaning)" }) -join '<br>' }
    $lines.Add("| ``$($item.wiId)`` | $($item.title) | $(Format-Gwae $item.actionGwae) | $operationDisplay | $targetDisplay | $supportDisplay | $relationDisplay | ``$($item.classificationStatus)`` |")
}
$markdown = ($lines -join "`n") + "`n"

$jsonOutputFullPath = Resolve-RepositoryPath $JsonOutputPath
$markdownOutputFullPath = Resolve-RepositoryPath $MarkdownOutputPath

if ($Mode -eq 'Write') {
    Write-Utf8NoBom $jsonOutputFullPath $json
    Write-Utf8NoBom $markdownOutputFullPath $markdown
}
elseif ($Mode -eq 'Check') {
    Require (Test-Path -LiteralPath $jsonOutputFullPath) "GeneratedFileMissing:$JsonOutputPath"
    Require (Test-Path -LiteralPath $markdownOutputFullPath) "GeneratedFileMissing:$MarkdownOutputPath"
    Require ((Get-Content -Raw -Encoding UTF8 -LiteralPath $jsonOutputFullPath) -ceq $json) "GeneratedFileStale:$JsonOutputPath"
    Require ((Get-Content -Raw -Encoding UTF8 -LiteralPath $markdownOutputFullPath) -ceq $markdown) "GeneratedFileStale:$MarkdownOutputPath"
}

Write-Output ('WorldInteractionGwaeClassification:{0}:Passed:{1}' -f $Mode, $resolvedItems.Count)
