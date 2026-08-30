[CmdletBinding()]
param(
    [ValidateSet('Write','Check','Validate')] [string] $Mode = 'Check',
    [string] $InputPath = 'eng/execution-ledgers/world-interaction-registration-relations.json',
    [string] $CatalogPath = 'eng/execution-ledgers/world-interactions.json',
    [string] $OutputPath = 'docs/AI/generated/world-interaction-registration.md'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'world-interaction-registration-functions.ps1')
. (Join-Path $PSScriptRoot '../common/deterministic-text-output.ps1')
$저장소 = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$등록 = Read-WorldInteractionRegistration $저장소 $InputPath $CatalogPath
if ($Mode -eq 'Validate') { Write-Output 'WorldInteractionRegistrationValid:41'; return }
$문서 = [System.Collections.Generic.List[string]]::new()
$문서.Add('# WI 상하위 관계와 신규 후보 등록 결과')
$문서.Add('')
$문서.Add('> world-interaction-registration-relations.json에서 생성한다. 직접 수정하지 않는다.')
$문서.Add('')
$문서.Add('- 기존 72개 + 신규 행동 33개 = 전체 WI 105개. 41개 후보 중 특화 프로필 2개·상위 분류 5개·결과 투영 1개는 실행 WI로 중복 등록하지 않는다.')
$대장 = Get-Content (Join-Path $저장소 $CatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
$신규Id = @($등록.decisions | Where-Object dispositionCode -eq 'RegisterAction' | ForEach-Object canonicalId)
$신규상태 = @($대장.items | Where-Object { $_.id -in $신규Id })
$미착수 = @($신규상태 | Where-Object { $_.implementation.currentStage -eq 'E0' }).Count
$문서.Add("- 등록 당시 신규 33개는 E0였다. 현재 미착수 $미착수 개; 아래 실제 구현 상태는 WI 대장에서 읽는다. 등록 자체는 구현·Save·API·공간·성장 승인이 아니다.")
$문서.Add('- 상위 분류는 실행하지 않는다. 특화는 의미 관계이며 부모를 재실행하지 않는다. 작업 순서는 world-interaction-flows.json이 소유한다.')
$문서.Add('- 기존 WI ID와 공개 계약을 바꾸지 않는다. 특화 프로필의 옛 후보 ID는 이 문서와 질문 추적에 보존하며 런타임 별칭으로 자동 해석하지 않는다.')
$문서.Add('- Town 독립 보충 주문과 Hub/기존 마트 연결은 주체·원장·시작 조건이 달라 이름만으로 통합하지 않았다. 사상자 대응·안전화 같은 복합 활동은 하위 책임 추가 승인이 필요하다.')
$문서.Add('')
$문서.Add('## 상위 분류와 하위 행동')
$문서.Add('')
$문서.Add('기존 `wi-family:meditation`은 독립된 횡단 성장 축으로 유지한다. 아래는 행동 책임 분류이며 신규 등록을 명상 보상에 자동 결속하지 않는다.')
$문서.Add('')
foreach ($분류 in $등록.families) {
    $문서.Add("- $($분류.titleKo) (``$($분류.id)``): $(@($분류.memberWorldInteractionIds) -join ', ')")
}
$문서.Add('')
$문서.Add('## 공통 행동의 특화 관계')
$문서.Add('')
foreach ($관계 in $등록.specializations) { $문서.Add("- ``$($관계.parentWorldInteractionId)`` → ``$($관계.childWorldInteractionId)``: $($관계.meaningKo)") }
$문서.Add('')
$문서.Add('## 후보 41개 판정')
$문서.Add('')
$문서.Add('| 후보·한국어 이름 | 판정 | 등록 대상 | 책임·중복 판정 이유 | 질문 |')
$문서.Add('| --- | --- | --- | --- | --- |')
foreach ($항목 in $등록.decisions) {
    $문서.Add("| ``$($항목.candidateId)`` $($항목.titleKo) | $($항목.dispositionCode) | ``$($항목.canonicalId)`` | $($항목.reasonKo) | $(@($항목.questions) -join ', ') |")
}
$문서.Add('')
$문서.Add('## 원문 근거')
$문서.Add('')
$문서.Add('### 신규 33개 실제 구현 상태')
$문서.Add('')
$문서.Add('| WI | 논리 구현 | 통합 E | 구현 파일 수 |')
$문서.Add('| --- | --- | --- | --- |')
foreach ($항목 in $신규상태) {
    $문서.Add("| ``$($항목.id)`` $($항목.title) | $($항목.implementation.currentStage) / $($항목.implementation.status) | $($항목.integration.currentStage) | $(@($항목.existingImplementationReferences).Count) |")
}
$문서.Add('')
foreach ($출처 in @($등록.decisions.sources | Sort-Object -Unique)) { $문서.Add("- ``$출처``") }
$결과 = ($문서 -join "`n") + "`n"
$경로 = Join-Path $저장소 $OutputPath
if ($Mode -eq 'Write') { Write-DeterministicTextIfChanged -Path $경로 -Content $결과 | Out-Null }
else { if ((Get-Content -LiteralPath $경로 -Raw -Encoding UTF8).Replace("`r`n","`n") -cne $결과) { throw 'WiRegistrationDocumentStale' } }
Write-Output 'WorldInteractionRegistrationValid:41;New=33;Profiles=2;Families=5;Projection=1'
