#requires -Version 7.0
[CmdletBinding()] param([string] $LocalImporterPath)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$runRef = 'artifacts/local/validation/planning-release-d439/tests-' + [guid]::NewGuid().ToString('N')
$run = Join-Path $root $runRef
$null = [IO.Directory]::CreateDirectory($run)
$tool = Join-Path $root 'eng/planning-inquiries/manage-planning-release.ps1'
$catalogRef = 'eng/execution-ledgers/world-interactions.json'
$catalogHash = (Get-FileHash (Join-Path $root $catalogRef)).Hash
$catalog = Get-Content (Join-Path $root $catalogRef) -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$wi = @($catalog.items | Where-Object id -CEQ 'WI-NATURE-06')[0]
$evidenceRef = $runRef + '/evidence.md'
[IO.File]::WriteAllText((Join-Path $root $evidenceRef),'시험 전용 근거. 실제 기획 승인이나 자산 선정 아님.',[Text.UTF8Encoding]::new($false))
$evidenceHash = (Get-FileHash (Join-Path $root $evidenceRef)).Hash
$base = [ordered]@{
 schemaVersion='planning-release.v1'; planningId='test:planning'; revision='r1'; state='ReviewedForHandoff'
 context=[ordered]@{now='시험 시점';here='시험 장소';self='시험 행위자';target='시험 대상';action='기존 WI 대조';result='기획 참조';nextChoices=@('후속 검토')}
 catalog=[ordered]@{revision=$catalog.revision;sha256=$catalogHash}
 evidence=@([ordered]@{key='source';path=$evidenceRef;sha256=$evidenceHash;quote='시험 전용 근거.'})
 wiDecisions=@([ordered]@{key='work';disposition='Reuse';existingIds=@($wi.id);rationale='시험 전용이며 실제 의미 승인 아님';evidenceKeys=@('source')})
 objects=@([ordered]@{key='tree';definitionId='design:wi:tree-resource';name='시험 나무';kind='Physical';evidenceKeys=@('source')})
 uses=@([ordered]@{key='target';wiKey='work';objectKey='tree';role='Target';evidenceKeys=@('source');
     catalogProof=[ordered]@{ruleRevision=$wi.ruleRevision;field='worldAction';quote=$wi.worldAction}})
 visuals=@()
}
$results = [Collections.Generic.List[object]]::new()
function Assert([bool] $value,[string] $name) { if(-not $value) {throw "Failed:$name"}; $results.Add([ordered]@{name=$name;passed=$true}) }
function Clone { ConvertFrom-Json ($base | ConvertTo-Json -Depth 30) -AsHashtable }
function Document($p,[string] $name='input') {
 $ref = "$runRef/$name.md"
 [IO.File]::WriteAllText((Join-Path $root $ref), "# 시험 전용 문서`n`n" + '```planning-release' + "`n" + ($p | ConvertTo-Json -Depth 30) + "`n" + '```' + "`n",[Text.UTF8Encoding]::new($false))
 $ref
}
function Execute([string] $ref,[string] $mode='Validate',[string] $directory="$runRef/packets") {
 & $tool -Mode $mode -DocumentPath $ref -OutputDirectory $directory | ConvertFrom-Json -AsHashtable
}
function Reject([string] $name,[scriptblock] $action,[string] $reason) {
 $caught = $null
 try { $null = & $action } catch { $caught = $_.Exception.Message }
 Assert ($null -ne $caught -and $caught.Contains($reason)) "$name [$caught]"
}
function Bad([string] $name,[scriptblock] $change,[string] $reason) {
 $p = Clone; & $change $p; $ref = Document $p $name
 Reject $name { Execute $ref } $reason
}
$docRef = Document (Clone)
$valid = Execute $docRef
Assert ($valid.localImport -ceq 'Prepared_NotApplied' -and $valid.database -ceq 'NotAttempted') '검사만으로 저장하지 않음'
Assert (-not (Test-Path (Join-Path $root "$runRef/packets"))) 'Validate 파일 쓰기 없음'
Reject '없는 판본 확인 거부' { Execute $docRef 'Check' } 'PacketMissing'
$first = Execute $docRef 'Write'
$output = Join-Path $root $first.outputRef
$time = (Get-Item $output).LastWriteTimeUtc
Assert ($first.writeState -ceq 'Created') '불변 판본 생성'
$second = Execute $docRef 'Write'
Assert ($second.writeState -ceq 'ExistingIdentical' -and $second.packetSha256 -ceq $first.packetSha256 -and (Get-Item $output).LastWriteTimeUtc -eq $time) '멱등 재생성 파일 불변'
Assert ((Execute $docRef 'Check').packetSha256 -ceq $first.packetSha256) '현재 문서와 판본 대조'
$packet = Get-Content $output -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
Assert ($packet.localImport.request.Definitions.Count -eq 0 -and $packet.localImport.request.Relations.Count -eq 1) '기존 정의만 참조'
Assert ($packet.localImport.request.Relations[0].ContextNote.Contains('sourceHash') -and $packet.localImport.request.Relations[0].ContextNote.Contains('nextChoices')) '관계에 문서 출처와 일곱 칸 보존'
Assert ($packet.serverBinding -ceq 'LocalInProcess_ExistingWiUseCase_NoHttp') 'HTTP 비필수 로컬 경로'
$changed = Clone; $changed.context.now='수정된 시점'; $null=Document $changed
Reject '동일 판본 변경 거부' { Execute $docRef 'Write' } 'ImmutableRevisionConflict'
Assert ((Get-FileHash $output).Hash -ceq $first.packetSha256) '충돌 시 기존 판본 보존'
$changed.revision='r2'; $null=Document $changed
$revision2 = Execute $docRef 'Write'
Assert ($revision2.outputRef -cne $first.outputRef) '새 판본 별도 보존'
$p2 = Get-Content (Join-Path $root $revision2.outputRef) -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
Assert ($p2.localImport.request.Relations[0].ContextKey -cne $packet.localImport.request.Relations[0].ContextKey) '새 판본은 과거 DB 관계를 덮지 않음'
$null=Document (Clone)
Bad '알수없는필드' {param($p) $p['rogue']=1} 'UnexpectedFields'
Bad '알수없는WI' {param($p) $p.wiDecisions[0].existingIds=@('WI-NONE-999')} 'UnknownWi'
Bad '대장판본불일치' {param($p) $p.catalog.revision='outdated'} 'CatalogDrift'
Bad '대장hash불일치' {param($p) $p.catalog.sha256='BAD'} 'CatalogDrift'
Bad '근거hash불일치' {param($p) $p.evidence[0].sha256='BAD'} 'EvidenceDrift'
Bad '근거인용불일치' {param($p) $p.evidence[0].quote='없는 문장'} 'QuoteMismatch'
Bad '존재하지않는객체' {param($p) $p.uses[0].objectKey='absent'} 'UnknownUseReference'
Bad '고립객체' {param($p) $p.uses=@()} 'OrphanObject'
Bad '중복객체키' {param($p) $p.objects+=@($p.objects[0])} 'DuplicateKey'
Bad '잘못된선택' {param($p) $p.state='Approved'} 'InvalidChoice'
Bad '누락근거' {param($p) $p.uses[0].evidenceKeys=@()} 'EvidenceRequired'
Bad '없는근거참조' {param($p) $p.uses[0].evidenceKeys=@('absent')} 'UnknownEvidence'
Bad '중복근거참조' {param($p) $p.uses[0].evidenceKeys=@('source','source')} 'DuplicateEvidence'
Bad '상대경로탈출' {param($p) $p.evidence[0].path='docs/../README.md'} 'UnsafePath'
Bad '외부경로' {param($p) $p.evidence[0].path='https://example.com/source.md'} 'UnsafePath'
Bad '미지원원천경로' {param($p) $p.evidence[0].path='Ssalddel/appsettings.json'} 'SourceOutsideAllowedRoots'
Bad '최소기존WI' {param($p) $p.wiDecisions[0].existingIds=@()} 'ExistingWiRequired'
Bad '보편화비교부족' {param($p) $p.wiDecisions[0].disposition='Generalize'} 'GeneralizationNeedsMultipleWi'
Bad '신규의기존WI사칭' {param($p) $p.wiDecisions[0].disposition='NewCandidate'} 'NewCandidateCannotClaimExistingWi'
Bad '규칙판본불일치' {param($p) $p.uses[0].catalogProof.ruleRevision='wrong'} 'RuleRevisionMismatch'
Bad 'WI전체인용불일치' {param($p) $p.uses[0].catalogProof.quote='짧은 요약'} 'CatalogQuoteMismatch'
Bad '잘못된WI근거필드' {param($p) $p.uses[0].catalogProof.field='title'} 'InvalidChoice'
Bad '잘못된관계역할' {param($p) $p.uses[0].role='Enemy'} 'InvalidChoice'
Bad 'WI결정없음' {param($p) $p.wiDecisions=@()} 'WiDecisionRequired'
Bad '미선정에자산있음' {param($p) $p.visuals=@(@{key='v';objectKey='tree';role='모델';state='Unselected';assetVersionId='asset';evidenceKeys=@()})} 'UnselectedHasAsset'
Bad '선정자산없음' {param($p) $p.visuals=@(@{key='v';objectKey='tree';role='모델';state='Selected';assetVersionId=$null;evidenceKeys=@('source')})} 'InvalidIdentifier'
Bad '과도한목록' {param($p) $p.context.nextChoices=@(1..257 | ForEach-Object {'항목'})} 'ArrayExpectedOrTooLarge'
Reject '출력범위거부' { Execute $docRef 'Write' 'docs/should-not-exist' } 'OutputOutsideArtifacts'
foreach($pair in @(@('Draft','DocumentNotReviewed'),@('NewObject','ExistingDefinitionIdRequired'),@('NoProof','CanonicalWiProofRequiredForLocalImport'),@('Visual','VisualCompositionImportRequiresSeparateExistingWorkflow'),@('LongContext','ContextExceedsExistingContract'),@('Specialize','OnlySingleExistingWiReuseSupported'))) {
 $p=Clone
 switch($pair[0]) {
  Draft {$p.state='Draft'} NewObject {$p.objects[0].definitionId=$null} NoProof {$p.uses[0].catalogProof=$null}
  Visual {$p.visuals=@(@{key='v';objectKey='tree';role='모델';state='Unselected';assetVersionId=$null;evidenceKeys=@()})}
  LongContext {$p.context.now='가'*1600} Specialize {$p.wiDecisions[0].disposition='Specialize'}
 }
 $value=Execute (Document $p $pair[0]); Assert ($value.localImport -ceq 'NotReady' -and $value.gaps -ccontains $pair[1]) ("미지원 저장 누락 대신 표시:"+$pair[0])
}
$raw=[IO.File]::ReadAllText((Join-Path $root $docRef))
$v2=Clone; $v2.schemaVersion='planning-release.v2'
$v2.visuals=@(@{key='vessel';objectKey='tree';role='Vessel';slotKey='main';expectedRevision=[long]1;state='Held';inventorySnapshotId=$null;selectionEvidence=$null;reason='용도 불일치';evidenceKeys=@('source')})
$v2Result=Execute (Document $v2 'v2-held')
Assert ($v2Result.localImport -ceq 'Prepared_NotApplied') 'v2 보류 항목도 명시 반입 결과로 전달'
$v2.visuals[0].state='AutomaticDraft';$v2.visuals[0].inventorySnapshotId='A'*64
$v2.visuals[0].selectionEvidence=@{SchemaVersion='visual-auto-selection.r1';Origin='CodexAutomatic'}
Assert ((Execute (Document $v2 'v2-auto')).localImport -ceq 'Prepared_NotApplied') 'v2 형식 준비와 서버 실근거 관문 구분'
$v2.visuals+=@($v2.visuals[0].Clone());$v2.visuals[1].key='alternative'
Reject 'v2 대안 동시선정 슬롯중복' {Execute (Document $v2 'v2-duplicate')} 'DuplicateVisualSlot'
$v2.visuals=@($v2.visuals[0]);$v2.objects[0].kind='Information'
Reject 'v2 정보형 자산선정' {Execute (Document $v2 'v2-info')} 'AutomaticPhysicalOnly'
$v2.objects[0].kind='Physical';$v2.visuals[0].state='Held'
Reject 'v2 보류의 위장선정' {Execute (Document $v2 'v2-held-selected')} 'HeldCannotSelect'
[IO.File]::WriteAllText((Join-Path $run 'double.md'),$raw+"`n"+$raw)
Reject '복수블록거부' { Execute "$runRef/double.md" } 'ExactlyOneReleaseBlockRequired'
[IO.File]::WriteAllText((Join-Path $run 'duplicate-json.md'),$raw.Replace('"schemaVersion":','"SCHEMAVERSION": "wrong", "schemaVersion":'))
Reject 'JSON중복키거부' { Execute "$runRef/duplicate-json.md" } 'DuplicateJsonProperty'
[IO.File]::WriteAllText((Join-Path $run 'no-block.md'),'본문만')
Reject '블록없음거부' { Execute "$runRef/no-block.md" } 'ExactlyOneReleaseBlockRequired'
[IO.File]::AppendAllText((Join-Path $root $evidenceRef),' 수정')
Reject '저장후근거변동거부' { Execute $docRef 'Check' } 'EvidenceDrift'
Assert ((Get-FileHash (Join-Path $root $catalogRef)).Hash -ceq $catalogHash) '공식 WI 원문 불변'
if ($LocalImporterPath) {
 $approvalHash=(Get-FileHash (Join-Path $root 'docs/Architecture/기획판본-서버반입파이프라인.md')).Hash
 function RunnerReject([string] $name,[string[]] $arguments,[string] $reason) {
  $resultText = & dotnet $LocalImporterPath @arguments
  $code=$LASTEXITCODE; $value=$resultText | ConvertFrom-Json -AsHashtable
  Assert ($code -eq 1 -and $value.reason -ceq $reason -and $value.databaseWriteAttempted -eq $false) ("로컬 실행기 저장전 차단:"+$name)
 }
 RunnerReject '인수누락' @() 'Usage: preview|apply repository packetRef packetSha256 outputRef approvalSha256'
 RunnerReject '모드거부' @('automatic',$root,$first.outputRef,$first.packetSha256,"$runRef/no-output",$approvalHash) 'ModeInvalid'
 RunnerReject '출력덮어쓰기거부' @('apply',$root,$first.outputRef,$first.packetSha256,$runRef,$approvalHash) 'OutputAlreadyExists'
 Assert (-not(Test-Path (Join-Path $run 'result.json'))) '외부 기존 출력 폴더에 실패결과 쓰지 않음'
 RunnerReject '승인원문변동' @('apply',$root,$first.outputRef,$first.packetSha256,"$runRef/no-output",('0'*64)) 'ApprovalDrift'
 RunnerReject '변조사본' @('apply',$root,$first.outputRef,('0'*64),"$runRef/no-output",$approvalHash) 'PacketHashMismatch'
 RunnerReject '현재근거변동' @('apply',$root,$first.outputRef,$first.packetSha256,"$runRef/no-output",$approvalHash) 'DocumentValidationFailed'
 Assert (-not(Test-Path (Join-Path $run 'no-output'))) '반입 전 거부는 DB와 실행 산출물 생성하지 않음'
}
$report=[ordered]@{status='Passed';cases=$results.Count;database='NotUsed';http='NotUsed';artifactRef=$runRef;results=$results}
[IO.File]::WriteAllText((Join-Path $run 'results.json'),($report | ConvertTo-Json -Depth 10),[Text.UTF8Encoding]::new($false))
[ordered]@{status=$report.status;cases=$report.cases;database=$report.database;http=$report.http;artifactRef=$runRef} | ConvertTo-Json
