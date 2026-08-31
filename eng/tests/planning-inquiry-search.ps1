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
$laterIds=@('forest-farm-edge-planning-focus','forest-farm-edge-reference-solar-term','forest-farm-edge-landscape-asset-survey','spring-equinox-herb-crop-research')
Assert (@($db.questions|Where-Object {$_.questionId -in $laterIds -and $_.kind -eq 'SemanticFollowup' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 4) 'D411D413FourNewIdsSeparateFromD410Baseline'
Assert (@($db.questions).Count -eq 477) 'D428Preserves474AndAddsThreeQuestions'
$perspectiveIds=@('first-person-exploration-entry','multi-area-observation-operation-perspective','optional-auto-hunting-operations','game-global-real-work-assistance','perspective-scale-wi-classification','perspective-independent-development')
Assert (@($db.questions|Where-Object {$_.questionId -in $perspectiveIds}).Count -eq 6) 'PerspectiveSixExplicitTableIds'
Assert (@($db.questions|Where-Object {$_.questionId -in $perspectiveIds -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 5) 'PerspectiveDirectionNotGameImplementation'
foreach($futureId in @('optional-auto-hunting-operations','game-global-real-work-assistance')){
 $future=@($db.sections|Where-Object {$_.directQuestionId -ceq $futureId})
 Assert ($future.Count -eq 1 -and $future[0].text -match 'FutureExtension') 'FutureImplementationQualifierPreservedInExactRow'
}
$areas=& $manager -IndexPath $index -Id multi-area-choice-parallel-development | ConvertFrom-Json
Assert ($areas.totalMatches -eq 1 -and $areas.results[0].question.recordStatus -eq 'ConfirmedDirection') 'D416ParallelDirectionNotNewGameApproval'
$brew=& $manager -IndexPath $index -Id town-brewing-first-participation | ConvertFrom-Json
Assert ($brew.totalMatches -eq 1 -and $brew.results[0].question.recordStatus -eq 'Confirmed' -and ($brew.results[0].excerpts.text -join '') -match '무료라는 뜻은 아니며') 'D428BrewingParticipationConfirmedWithoutFreeResources'
$brewOpen=& $manager -IndexPath $index -Id town-brewing-first-participation -OpenOnly | ConvertFrom-Json
Assert ($brewOpen.totalMatches -eq 0) 'D428ConfirmedParticipationNotOpen'
$detailOpen=& $manager -IndexPath $index -Id town-brewing-reality-detail-first-purpose -OpenOnly | ConvertFrom-Json
Assert ($detailOpen.totalMatches -eq 1 -and $detailOpen.results[0].question.recordStatus -eq 'Asked') 'D428DetailPurposeStillAsked'
$edgeOpen=& $manager -IndexPath $index -Topic first-play-experience -OpenOnly | ConvertFrom-Json
Assert (@($edgeOpen.results|Where-Object kind -eq 'Question').Count -eq 0) 'D415RoleSplitConfirmedNotOpen'
$northernIds=@('northern-spring-hungry-farm-encounter','hungry-farm-npc-local-knowledge-help','northern-spring-snow-conifer-survey','hungry-farm-first-meal-role-split')
Assert (@($db.questions|Where-Object {$_.questionId -in $northernIds}).Count -eq 4 -and @($db.questions|Where-Object {$_.questionId -in $northernIds -and $_.sourceRef -eq 'docs/AI/북부춘분-굶주린농장발견-기획보완-2026-08-31.md'}).Count -eq 4) 'D414SingleSourceOwnership'
Assert (@($db.questions|Where-Object {$_.questionId -in $northernIds -and $_.recordStatus -eq 'Confirmed'}).Count -eq 3 -and @($db.questions|Where-Object {$_.questionId -in $northernIds -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1) 'D415ExistingIdsRoleStateUpdated'
$springIds=@('farm-encounter-spring-without-snow','farm-meal-low-pressure-help','farm-meal-small-contribution-return','farm-meal-player-inventory-connection')
Assert (@($db.questions|Where-Object {$_.questionId -in $springIds -and $_.sourceRef -eq 'docs/AI/북부춘분-굶주린농장발견-기획보완-2026-08-31.md'}).Count -eq 4) 'D415FourNewIdsSingleSource'
Assert (@($db.questions|Where-Object {$_.questionId -in $springIds -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1 -and @($db.questions|Where-Object {$_.questionId -in $springIds -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 3) 'D415NewStates'
$pastSnow=@($db.sections|Where-Object {$_.directQuestionId -eq 'northern-spring-snow-conifer-survey'})
Assert ($pastSnow.Count -eq 1 -and $pastSnow[0].text -match 'D415 이후 효력' -and $pastSnow[0].text -match '추가 조사는 중단') 'D415HistoricalSnowExcerptRetainsSupersessionNotice'
$research=& $manager -IndexPath $index -Id spring-equinox-herb-crop-research | ConvertFrom-Json
Assert ($research.totalMatches -eq 1 -and ($research.results[0].excerpts.text -join '') -match '품목/약효' -and ($research.results[0].excerpts.text -join '') -match '미연결|별개|별도') 'D413ResearchNotGameSpeciesOrEffectApproval'
$discoveryIds=@('discovery-led-play-causality','inquiry-causal-flow-reuse')
Assert (@($db.questions | Where-Object {$_.questionId -in $discoveryIds -and $_.kind -eq 'SemanticFollowup' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 2) 'D408TwoDirectionsNoNewNumberedQuestion'
foreach($discoveryId in $discoveryIds){$closed=& $manager -IndexPath $index -Id $discoveryId -OpenOnly | ConvertFrom-Json; Assert ($closed.totalMatches -eq 0) 'D408DirectionNotAsked'}
$q59=& $manager -IndexPath $index -Id Q059 | ConvertFrom-Json
$weatherCue=& $manager -IndexPath $index -Id discovery-cue-weather-readability -OpenOnly | ConvertFrom-Json
Assert ($weatherCue.totalMatches -eq 0) 'D409WeatherCueNoLongerAsked'
$weatherCue=& $manager -IndexPath $index -Id discovery-cue-weather-readability | ConvertFrom-Json
Assert ($weatherCue.results[0].question.recordStatus -eq 'Confirmed' -and ($weatherCue.results[0].excerpts.text -join '') -match 'FutureExtension') 'D409ConfirmedDiscoveryNotCombatApproval'
Assert ($q59.totalMatches -eq 1 -and $q59.results[0].excerpts[0].text -match '보존') 'ExistingAnswerRetrieval'
Assert ($q59.results[0].excerpts[0].sourceRole -eq 'HistoricalArchive') 'ArchiveLabel'
$pending=& $manager -IndexPath $index -Id Q347 -OpenOnly | ConvertFrom-Json
Assert ($pending.totalMatches -eq 0) 'ConfirmedQuestionExcludedFromOpen'
$confirmed=& $manager -IndexPath $index -Id Q347 | ConvertFrom-Json
Assert ($confirmed.results[0].question.recordStatus -eq 'Confirmed' -and $confirmed.results[0].excerpts[0].text -match '확인된 결정') 'ConfirmedAnswerRetrieval'
$semantic=& $manager -IndexPath $index -Id construction-cancel-material-preview | ConvertFrom-Json
Assert ($semantic.totalMatches -eq 1 -and $semantic.results[0].question.kind -eq 'SemanticFollowup') 'SemanticIdRetrieval'
$grace=& $manager -IndexPath $index -Id harvest-ready-grace-window -OpenOnly | ConvertFrom-Json
Assert ($grace.totalMatches -eq 1 -and $grace.results[0].question.recordStatus -eq 'Asked') 'D398SemanticAskedOpenQuery'
$season=& $manager -IndexPath $index -Id seasonal-time-context-direction -OpenOnly | ConvertFrom-Json
Assert ($season.totalMatches -eq 0) 'D399DirectionExcludedFromAskedQuery'
$sowing=& $manager -IndexPath $index -Id seasonal-sowing-outside-window -OpenOnly | ConvertFrom-Json
Assert ($sowing.totalMatches -eq 0) 'D400ConfirmedSowingExcludedFromOpenQuery'
$greenhouse=& $manager -IndexPath $index -Id seasonal-greenhouse-protection-limit -OpenOnly | ConvertFrom-Json
Assert ($greenhouse.totalMatches -eq 0) 'D401ConfirmedGreenhouseExcludedFromOpenQuery'
$unattended=& $manager -IndexPath $index -Id seasonal-magic-protection-unattended -OpenOnly | ConvertFrom-Json
Assert ($unattended.totalMatches -eq 1 -and $unattended.results[0].question.recordStatus -eq 'Asked') 'D401UnattendedAskedOpenQuery'
$clusters=& $manager -IndexPath $index -Id inquiry-progression-clusters | ConvertFrom-Json
Assert ($clusters.totalMatches -eq 1 -and $clusters.results[0].question.recordStatus -eq 'ConfirmedDirection' -and $clusters.results[0].question.kind -eq 'SemanticFollowup') 'D402ClusterDirectionNotNewGlobalQuestion'
$disclosure=& $manager -IndexPath $index -Id tech-tree-progressive-disclosure | ConvertFrom-Json
Assert ($disclosure.totalMatches -eq 1 -and $disclosure.results[0].question.recordStatus -eq 'Confirmed' -and $disclosure.results[0].excerpts[0].text -match '전면 공개하는 승인이 아니다') 'D402DisclosureAnswerWithLimits'
$progressionOpen=& $manager -IndexPath $index -Topic inquiry-progression-clusters -OpenOnly | ConvertFrom-Json
Assert (@($progressionOpen.results | Where-Object {$_.kind -eq 'Question'}).Count -eq 0) 'D402ConfirmedRecordsNotAsked'
$solarTerms=& $manager -IndexPath $index -Id korean-24-solar-terms-planning | ConvertFrom-Json
$seasonResearch=& $manager -IndexPath $index -Id solar-term-seasonal-food-research | ConvertFrom-Json
Assert ($solarTerms.totalMatches -eq 1 -and $solarTerms.results[0].question.recordStatus -eq 'Confirmed' -and $seasonResearch.totalMatches -eq 1 -and $seasonResearch.results[0].question.recordStatus -eq 'ConfirmedDirection') 'D403PrimaryAndDirectionStates'
Assert ($solarTerms.results[0].excerpts[0].sectionId -ceq $seasonResearch.results[0].excerpts[0].sectionId -and $seasonResearch.results[0].excerpts[0].text -match '지역·기후' -and $seasonResearch.results[0].excerpts[0].text -match '품종/어종·재배/양식 방식과 기간 의미') 'D403SharedSourceSectionNotMergedQuestion'
$landscape=& $manager -IndexPath $index -Id seasonal-landscape-appearance | ConvertFrom-Json
$coordination=& $manager -IndexPath $index -Id seasonal-spatial-engine-coordination | ConvertFrom-Json
Assert ($landscape.totalMatches -eq 1 -and $coordination.totalMatches -eq 1 -and $landscape.results[0].question.recordStatus -eq 'ConfirmedDirection' -and $coordination.results[0].question.recordStatus -eq 'ConfirmedDirection') 'D405ExplicitInlineStates'
Assert ($landscape.results[0].excerpts[0].sectionId -ceq $coordination.results[0].excerpts[0].sectionId -and $coordination.results[0].excerpts[0].text -match '첫 실물 후보는 미정' -and $coordination.results[0].excerpts[0].text -match '파일 조사와 준비 제안') 'D405DirectionNotRenderOrCandidateApproval'
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
$inlineConfig=Get-Content (Join-Path $root 'eng/planning-inquiries/sources.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$inlineConfig.PSObject.Properties.Remove('spatialRelationsRef')
$inlineSource="$artifact/inline-direction-fixture.md"; $inlineConfigRef="$artifact/inline-direction-sources.json"
$fixture=@'
## Separate states
- 질문 식별: `inline-primary-fixture`
- 상태: `Asked`.
- 조사·정리 방향 식별: `inline-direction-fixture` — `ConfirmedDirection`.
과거 참고: 조사·정리 방향 식별: `inline-historical-fixture` — `Confirmed`.
## Unknown state
- 조사·정리 방향 식별: `inline-unknown-fixture` — `UnapprovedCustomState`.
## Missing state
- 조사·정리 방향 식별: `inline-missing-fixture`
## Explicit inline primary
- 질문 식별: `inline-primary-state-fixture` — `ConfirmedDirection`.
- 엔진 협력 식별: `inline-engine-fixture` — `Asked`.
## New meaning label
- 의미 식별자: `inline-meaning-fixture` — `Confirmed`.
과거 참고: 의미 식별자: `inline-historical-meaning-fixture` — `Confirmed`.
'@
[IO.File]::WriteAllText((Join-Path $root $inlineSource),$fixture,$utf8)
$inlineConfig.extraSources+= [pscustomobject]@{path=$inlineSource;role='TopicSource';topicCode='inline-fixture'}
[IO.File]::WriteAllText((Join-Path $root $inlineConfigRef),($inlineConfig | ConvertTo-Json -Depth 20),$utf8)
$null=& $manager -Mode Write -SourcesPath $inlineConfigRef -IndexPath $index
$inlineDb=Get-Content (Join-Path $root $index) -Raw -Encoding UTF8 | ConvertFrom-Json
Assert (@($inlineDb.questions|Where-Object {$_.questionId -eq 'inline-primary-fixture' -and $_.recordStatus -eq 'Asked'}).Count -eq 1 -and @($inlineDb.questions|Where-Object {$_.questionId -eq 'inline-direction-fixture' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1) 'InlineDirectionDoesNotInheritPrimaryStatus'
Assert (@($inlineDb.questions|Where-Object questionId -in @('inline-historical-fixture','inline-unknown-fixture','inline-missing-fixture')).Count -eq 0) 'InlineHistoricalUnknownOrMissingStatusNotPromoted'
Assert (@($inlineDb.questions|Where-Object {$_.questionId -eq 'inline-primary-state-fixture' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1 -and @($inlineDb.questions|Where-Object {$_.questionId -eq 'inline-engine-fixture' -and $_.recordStatus -eq 'Asked'}).Count -eq 1) 'InlinePrimaryAndEngineIndependentStates'
Assert (@($inlineDb.questions|Where-Object {$_.questionId -eq 'inline-meaning-fixture' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1 -and @($inlineDb.questions|Where-Object questionId -eq 'inline-historical-meaning-fixture').Count -eq 0) 'MeaningLabelAnchoredWithoutBreakingLegacyQuestionLabel'
$tableFixture=@'

## Explicit table
| 의미 식별자 | 상태 | 내용 |
| --- | --- | --- |
| `table-confirmed-fixture` | `Confirmed` | explicit |
| `table-future-fixture` | `ConfirmedDirection` / `FutureExtension` | not implemented |

## Ordinary reference table
| reference | status |
| `table-reference-fixture` | `Confirmed` |
'@
[IO.File]::AppendAllText((Join-Path $root $inlineSource),$tableFixture,$utf8)
$null=& $manager -Mode Write -SourcesPath $inlineConfigRef -IndexPath $index
$tableDb=Get-Content (Join-Path $root $index) -Raw -Encoding UTF8|ConvertFrom-Json
Assert (@($tableDb.questions|Where-Object {$_.questionId -eq 'table-confirmed-fixture' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1 -and @($tableDb.questions|Where-Object {$_.questionId -eq 'table-future-fixture' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1) 'SemanticTableReadsOnlyExplicitRowState'
Assert (@($tableDb.questions|Where-Object questionId -eq 'table-reference-fixture').Count -eq 0) 'OrdinaryTableDoesNotDeclareQuestion'
$validFixture=[IO.File]::ReadAllText((Join-Path $root $inlineSource))
[IO.File]::WriteAllText((Join-Path $root $inlineSource),$validFixture.Replace('`table-confirmed-fixture` | `Confirmed`','`table-confirmed-fixture` | `Unknown`'),$utf8)
Reject { & $manager -Mode Write -SourcesPath $inlineConfigRef -IndexPath $index } 'InvalidSemanticTableStatus'
[IO.File]::WriteAllText((Join-Path $root $inlineSource),$validFixture.Replace('`table-confirmed-fixture` | `Confirmed`','`table-confirmed-fixture` | '),$utf8)
Reject { & $manager -Mode Write -SourcesPath $inlineConfigRef -IndexPath $index } 'InvalidSemanticTableStatus'
[IO.File]::WriteAllText((Join-Path $root $inlineSource),$validFixture.Replace('`table-future-fixture`','`table-confirmed-fixture`'),$utf8)
Reject { & $manager -Mode Write -SourcesPath $inlineConfigRef -IndexPath $index } 'DuplicateQuestion'
[IO.File]::WriteAllText((Join-Path $root $inlineSource),$validFixture,$utf8)
[IO.File]::AppendAllText((Join-Path $root $inlineSource),"`n## Duplicate`n"+'- 조사·정리 방향 식별: `inline-direction-fixture` — `Confirmed`.', $utf8)
Reject { & $manager -Mode Write -SourcesPath $inlineConfigRef -IndexPath $index } 'DuplicateQuestion'
Write-Output "Planning inquiry search: $script:checks passed"
$report=[ordered]@{checks=$script:checks;status='Passed';powerShellVersion=$PSVersionTable.PSVersion.ToString();managerSha256=(Get-FileHash $manager).Hash}
[IO.File]::WriteAllText((Join-Path $root "$artifact/results-$($PSVersionTable.PSVersion.Major).json"),($report | ConvertTo-Json),$utf8)
