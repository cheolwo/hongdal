$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager=Join-Path $root 'eng/planning-inquiries/manage-inquiry-search.ps1'
$artifact='artifacts/local/validation/planning-inquiry-search'
$null=New-Item -ItemType Directory -Path (Join-Path $root $artifact) -Force
$utf8=[Text.UTF8Encoding]::new($false)
$script:checks=0
function Assert([bool]$Ok,[string]$Name) { if(-not $Ok){throw "Failed:$Name"}; $script:checks++ }
function Reject([scriptblock]$Action,[string]$Code) {
    $failed=$false
    try { & $Action | Out-Null } catch { if($_.Exception.Message -notlike "*$Code*"){throw}; $failed=$true }
    Assert $failed $Code
}
$index="$artifact/index.json"
$null=& $manager -Mode Write -IndexPath $index
$first=(Get-FileHash (Join-Path $root $index)).Hash
$null=& $manager -Mode Write -IndexPath $index
Assert ($first -eq (Get-FileHash (Join-Path $root $index)).Hash) 'DeterministicRebuild'
$null=& $manager -Mode Validate -IndexPath $index
Assert $true 'Validate'
$db=Get-Content (Join-Path $root $index) -Raw -Encoding UTF8 | ConvertFrom-Json
Assert (@($db.questions | Where-Object kind -eq LegacyNumbered).Count -eq 339) 'LegacyCoverage'
Assert (@($db.questions | Group-Object questionId | Where-Object Count -gt 1).Count -eq 0) 'UniqueQuestionIds'
Assert (@($db.sections | Group-Object sectionId | Where-Object Count -gt 1).Count -eq 0) 'UniqueSectionIds'
$q59=& $manager -IndexPath $index -Id Q059 | ConvertFrom-Json
Assert ($q59.totalMatches -eq 1 -and $q59.results[0].excerpts[0].text -match '보존') 'ExistingAnswerRetrieval'
Assert ($q59.results[0].excerpts[0].sourceRole -eq 'HistoricalArchive') 'ArchiveLabel'
$pending=& $manager -IndexPath $index -Id Q347 -OpenOnly | ConvertFrom-Json
Assert ($pending.totalMatches -eq 0) 'ConfirmedQuestionExcludedFromOpen'
$confirmed=& $manager -IndexPath $index -Id Q347 | ConvertFrom-Json
Assert ($confirmed.results[0].question.recordStatus -eq 'Confirmed' -and $confirmed.results[0].excerpts[0].text -match '확인된 결정') 'ConfirmedAnswerRetrieval'
$semantic=& $manager -IndexPath $index -Id construction-cancel-material-preview | ConvertFrom-Json
Assert ($semantic.totalMatches -eq 1 -and $semantic.results[0].question.kind -eq 'SemanticFollowup') 'SemanticIdRetrieval'
$missing=& $manager -IndexPath $index -Id Q272 | ConvertFrom-Json
Assert ($missing.results[0].question.recordStatus -eq 'NeedsSourceRecovery' -and $missing.results[0].excerpts[0].text -match '추측 금지') 'MissingHistoryNotInvented'
$depth=& $manager -IndexPath $index -Depth D4 -Limit 1000 | ConvertFrom-Json
Assert ($depth.totalMatches -gt 0 -and @($depth.results | Where-Object {$_.question.depthCode -ne 'D4'}).Count -eq 0) 'DepthFilter'
$topic=& $manager -IndexPath $index -Topic nature-resource-construction -Text '중단 보존' | ConvertFrom-Json
Assert (@($topic.results | Where-Object {$_.kind -eq 'Question' -and $_.question.questionId -eq 'Q-059'}).Count -eq 1) 'DuplicateQuestionSearch'
$unknown=& $manager -IndexPath $index -Id Q999 | ConvertFrom-Json
Assert ($unknown.totalMatches -eq 0) 'UnknownId'
[IO.File]::AppendAllText((Join-Path $root $index),' ',$utf8)
Reject { & $manager -Mode Validate -IndexPath $index } 'StaleOrModifiedIndex'
$config=Get-Content (Join-Path $root 'eng/planning-inquiries/sources.json') -Raw -Encoding UTF8 | ConvertFrom-Json
# 후속 질문이 앞에 삽입되어도 동일 질문의 오류 경계를 시험한다.
# 실제로 순서를 뒤집어 고정 배열 위치에 의존하지 않는 사례를 유지한다.
[Array]::Reverse($config.supplements)
$targetSupplements=@($config.supplements | Where-Object selector -eq '347')
Assert ($targetSupplements.Count -eq 1) 'StableSupplementFixtureSelection'
$targetSupplement=$targetSupplements[0]
$configRef="$artifact/sources.json"
function Save-Config { [IO.File]::WriteAllText((Join-Path $root $configRef),($config | ConvertTo-Json -Depth 20),$utf8) }
Save-Config
$null=& $manager -Mode Write -SourcesPath $configRef -IndexPath $index
$targetSupplement.recordStatus='Open'
Save-Config
Reject { & $manager -Mode Search -SourcesPath $configRef -IndexPath $index -Id Q347 } 'StaleOrModifiedIndex'
$targetSupplement.selector='346'
Save-Config
Reject { & $manager -Mode Write -SourcesPath $configRef -IndexPath $index } 'DuplicateQuestion'
$targetSupplement.selector='999'
Save-Config
Reject { & $manager -Mode Write -SourcesPath $configRef -IndexPath $index } 'SupplementWithoutSource'
Reject { & $manager -Mode Write -IndexPath '../inquiry-outside-repository.json' } 'OutsideRepository'
Write-Output "Planning inquiry search: $script:checks passed"
$report=[ordered]@{checks=$script:checks;status='Passed';powerShellVersion=$PSVersionTable.PSVersion.ToString();managerSha256=(Get-FileHash $manager).Hash}
[IO.File]::WriteAllText((Join-Path $root "$artifact/results-$($PSVersionTable.PSVersion.Major).json"),($report | ConvertTo-Json),$utf8)
