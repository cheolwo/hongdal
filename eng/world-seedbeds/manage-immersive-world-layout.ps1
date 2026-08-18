[CmdletBinding()]
param(
    [ValidateSet("Write", "Check")]
    [string] $Mode = "Check",
    [string] $OutputMarkdownPath = "docs/AI/generated/immersive-world-layout.md"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot "../common/deterministic-text-output.ps1")

function Require([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "ImmersiveWorldLayoutInvalid:$Code" }
}

function Escape-Markdown([string] $Value) {
    if ($null -eq $Value) { return "" }
    return $Value.Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$layoutPath = Join-Path $repositoryRoot "eng/world-seedbeds/immersive-world-layout.v1.json"
$designCatalogPath = Join-Path $repositoryRoot "eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json"
$wiCatalogPath = Join-Path $repositoryRoot "eng/execution-ledgers/world-interactions.json"
$layout = Get-Content -LiteralPath $layoutPath -Raw -Encoding UTF8 | ConvertFrom-Json
$designCatalog = Get-Content -LiteralPath $designCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$wiCatalog = Get-Content -LiteralPath $wiCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json

Require ([string] $layout.schemaVersion -eq "simulation-world-immersive-layout.v1") "Schema"
Require ([string] $layout.entrySceneCode -eq "SimulationWorldShell") "EntryScene"
Require ([string] $layout.defaultViewModeCode -eq "TacticalThirdPerson") "DefaultView"
Require ($layout.backgroundSimulationContinues -eq $true) "BackgroundSimulation"
Require ($layout.remoteFieldConfirmAllowed -eq $false) "RemoteConfirm"
Require (@($layout.instances).Count -eq 4) "InstanceCount"
Require (@($layout.instances.stableId | Sort-Object -Unique).Count -eq 4) "InstanceStableIdDuplicate"
Require (@($layout.instances.instanceKindCode | Sort-Object -Unique).Count -eq 4) "InstanceKindDuplicate"

$h1Ids = @($designCatalog.h1InteractionDefinitionRefs.stableId)
$h2Ids = @($designCatalog.h2DefinitionRefs.stableId)
$h3Ids = @($designCatalog.h3DefinitionRefs.stableId)
$wiIds = @($wiCatalog.items.id)
$instanceIds = @($layout.instances.stableId)

foreach ($instance in @($layout.instances)) {
    Require (@($instance.h3CandidateRefs | Where-Object { $h3Ids -notcontains [string] $_ }).Count -eq 0) "UnknownH3:$($instance.stableId)"
    Require (@($instance.h2CandidateRefs | Where-Object { $h2Ids -notcontains [string] $_ }).Count -eq 0) "UnknownH2:$($instance.stableId)"
    Require (@($instance.worldInteractionIds | Where-Object { $wiIds -notcontains [string] $_ }).Count -eq 0) "UnknownWi:$($instance.stableId)"
    $weights = @($instance.compositionWeights.PSObject.Properties)
    Require (($weights | Measure-Object -Property Value -Sum).Sum -eq 100) "CompositionWeightTotal:$($instance.stableId)"
    if ([string] $instance.instanceKindCode -eq "NatureHome") {
        Require ([int] $instance.compositionWeights.Nature -eq 90) "NatureWeight"
        Require ([int] $instance.compositionWeights.NetworkTransition -eq 10) "NatureTransitionWeight"
        Require ($instance.persistent -eq $true) "NaturePersistent"
    }
    else {
        Require ([int] $instance.compositionWeights.([string] $instance.primaryPackCode) -eq 70) "PrimaryWeight:$($instance.stableId)"
        Require ([int] $instance.compositionWeights.Nature -eq 20) "SpecialistNatureWeight:$($instance.stableId)"
        Require ([int] $instance.compositionWeights.NetworkTransition -eq 10) "SpecialistTransitionWeight:$($instance.stableId)"
        Require ($instance.fieldVisitRequired -eq $true) "FieldVisit:$($instance.stableId)"
    }
}

$bands = @($layout.natureRiskBands | Sort-Object order)
Require (($bands.riskBandCode -join ",") -eq "SafeCore,WarningBand,EncounterBand") "RiskBandOrder"
Require (@($bands | Where-Object { $_.riskBandCode -eq "SafeCore" -and ($_.monsterPresentationAllowed -or @($_.encounterSocketRoleCodes).Count -ne 0) }).Count -eq 0) "SafeCoreThreatLeak"
Require (@($bands | Where-Object monsterPresentationAllowed).riskBandCode -contains "EncounterBand") "EncounterPresentation"
foreach ($band in $bands) {
    Require (@($band.h1CandidateRefs | Where-Object { $h1Ids -notcontains [string] $_ }).Count -eq 0) "UnknownH1:$($band.riskBandCode)"
}

Require (@($layout.connectors).Count -eq 3) "ConnectorCount"
Require (@($layout.connectors.connectorCode | Sort-Object -Unique).Count -eq 3) "ConnectorDuplicate"
foreach ($connector in @($layout.connectors)) {
    Require ($instanceIds -contains [string] $connector.fromInstanceStableId) "ConnectorFrom"
    Require ($instanceIds -contains [string] $connector.toInstanceStableId) "ConnectorTo"
    Require ([string] $connector.fromInstanceStableId -eq [string] $layout.defaultInstanceStableId) "ConnectorMustStartAtNature"
}

$gate = @($layout.assetGates | Where-Object assetGateCode -eq "PolygonApocalypse")
Require ($gate.Count -eq 1) "ApocalypseGate"
Require ([string] $gate[0].stateCode -eq "WaitingForApocalypseAssetPack") "ApocalypseState"
Require ([string] $gate[0].fallbackPolicyCode -eq "Forbidden") "ApocalypseFallback"
Require ($layout.authorityBoundary.operationalModeThreatsForbidden -eq $true) "OperationalThreatBoundary"
Require ($layout.authorityBoundary.publicDataCannotIdentifyThreatTargets -eq $true) "PublicDataThreatBoundary"

$raw = Get-Content -LiteralPath $layoutPath -Raw -Encoding UTF8
Require (-not ($raw -match '"(absoluteWorldPosition|latitude|longitude|prefabPath|assetGuid|scenePath)"')) "ForbiddenAuthorityField"

$builder = [Text.StringBuilder]::new()
[void] $builder.AppendLine("# Nature 중심 3인칭 몰입 세계 대장")
[void] $builder.AppendLine()
[void] $builder.AppendLine("> 이 문서는 ``immersive-world-layout.v1.json``에서 결정적으로 생성한다. 후보 공간은 공식 H·E·AreaSet 권위를 만들지 않는다.")
[void] $builder.AppendLine()
[void] $builder.AppendLine("- 실행 진입점: ``$($layout.entrySceneCode)``")
[void] $builder.AppendLine("- 기본 시점: ``$($layout.defaultViewModeCode)``")
[void] $builder.AppendLine("- 배경 Simulation 지속: ``$($layout.backgroundSimulationContinues)``")
[void] $builder.AppendLine("- Apocalypse: ``$($gate[0].stateCode)`` · 대체 자산 ``$($gate[0].fallbackPolicyCode)``")
[void] $builder.AppendLine()
[void] $builder.AppendLine("## 경관 인스턴스")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 인스턴스 | 주축 팩 | 구성 | WI | 현장 방문 |")
[void] $builder.AppendLine("| --- | --- | --- | ---: | --- |")
foreach ($instance in @($layout.instances)) {
    $weights = @($instance.compositionWeights.PSObject.Properties | ForEach-Object { "$($_.Name) $($_.Value)%" }) -join ", "
    [void] $builder.AppendLine("| ``$($instance.stableId)`` $(Escape-Markdown $instance.title) | ``$($instance.primaryPackCode)`` | $(Escape-Markdown $weights) | $(@($instance.worldInteractionIds).Count) | ``$($instance.fieldVisitRequired)`` |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("## Nature 위험 단계")
[void] $builder.AppendLine()
[void] $builder.AppendLine("| 단계 | 의미 | 몬스터 표현 | 연결 지점 |")
[void] $builder.AppendLine("| --- | --- | --- | --- |")
foreach ($band in $bands) {
    [void] $builder.AppendLine("| ``$($band.riskBandCode)`` | $(Escape-Markdown $band.title) | ``$($band.monsterPresentationAllowed)`` | $(Escape-Markdown (@($band.encounterSocketRoleCodes) -join ', ')) |")
}
[void] $builder.AppendLine()
[void] $builder.AppendLine("실제 몬스터 Prefab 연결은 ``POLYGON Apocalypse`` 설치·감사 전까지 금지한다. 현재 Generic 해골로 자동 대체하지 않는다.")

$expected = ConvertTo-DeterministicText $builder.ToString()
$outputPath = Join-Path $repositoryRoot $OutputMarkdownPath
if ($Mode -eq "Write") {
    Write-DeterministicTextIfChanged $outputPath $expected | Out-Null
    Write-Output "ImmersiveWorldLayoutGenerated:Instances=4;RiskBands=3;Apocalypse=Waiting"
}
else {
    Require (Test-Path -LiteralPath $outputPath) "GeneratedMarkdownMissing"
    Require ((ConvertTo-DeterministicText ([IO.File]::ReadAllText($outputPath))) -ceq $expected) "GeneratedMarkdownOutOfDate"
    Write-Output "ImmersiveWorldLayoutValid:Instances=4;RiskBands=3;Apocalypse=Waiting"
}
