$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager=Join-Path $root 'eng/planning-inquiries/manage-inquiry-search.ps1'
$folder='artifacts/local/validation/planning-inquiry-wi-view'
$null=New-Item -ItemType Directory -Force -Path (Join-Path $root $folder)
$index="$folder/index.json";$md="$folder/wi.md";$count=0
$protected=@('eng/execution-ledgers/world-interactions.json','eng/execution-ledgers/playable-loops.json','eng/execution-ledgers/codex-playable-loop-goals.json')
$before=@{};foreach($p in $protected){$before[$p]=(Get-FileHash (Join-Path $root $p)).Hash}
function Assert([bool]$Ok,[string]$Name){if(-not $Ok){throw "Failed:$Name"};$script:count++;Write-Output "pass $Name"}
$null=& $manager -Mode Write -IndexPath $index -WiMarkdownPath $md
$hash=(Get-FileHash (Join-Path $root $index)).Hash
$null=& $manager -Mode Write -IndexPath $index -WiMarkdownPath $md
Assert ($hash -eq (Get-FileHash (Join-Path $root $index)).Hash) 'DeterministicWiProjection'
$null=& $manager -Mode Validate -IndexPath $index -WiMarkdownPath $md
$db=Get-Content (Join-Path $root $index) -Raw -Encoding UTF8|ConvertFrom-Json
$v=$db.spatial.wiView
Assert ($v.total -eq 105 -and @($v.items|Group-Object id|Where-Object Count -ne 1).Count -eq 0) 'All105UniqueOfficialWi'
Assert ($v.catalogRevision -eq 'simulation-world-interactions.r43') 'ExactCatalogRevision'
Assert (@($v.items|Where-Object {$_.executionAuthorized}).Count -eq 0) 'NoAutomaticExecutionApproval'
Assert (@($v.items|Where-Object {@($_.facets.PSObject.Properties.Name).Count -ne 7}).Count -eq 0) 'SevenViewsEveryWi'
Assert ($v.workOrders.Count -eq 6 -and @($v.workOrders|Where-Object state -eq SourceMissing).Count -eq 0) 'ArrayWorkOrderRefsNotConcatenated'
$farm=& $manager -IndexPath $index -Wi -Id WI-FARM-04|ConvertFrom-Json
Assert ($farm.totalMatches -eq 1 -and $farm.results[0].item.recordedIntegration.currentStage -eq 'E6') 'LegacyWiE6Preserved'
$context=$farm.results[0].item.loopContexts|Where-Object loopId -eq 'playable-loop:farm-crop-cycle.v1'
Assert ($context.maturityTracks.logic.currentStage -eq 'E3' -and $context.maturityTracks.presentation.currentStage -eq 'E1' -and $context.integratedStage -eq 'E1') 'DualLoopMaturitySeparate'
Assert ($farm.results[0].workOrders.Count -eq 1 -and $farm.results[0].workOrders[0].presentationE4Preparation.visualKeys.Count -eq 4) 'ExistingFarmE4PreparationReused'
Assert ($farm.results[0].workOrders[0].scopedPreparationResults[0].data.worldInteractionId -eq 'WI-FARM-04' -and $farm.results[0].workOrders[0].scopedPreparationResults[0].meaning -eq 'ReportedScopedResultNotWholeWiAchievement') 'ScopedCodeResultNotWholeWiPromotion'
Assert ($farm.results[0].item.questionLinkBasis -eq 'TopicContextNotExactQuestionImplementation' -and $farm.results[0].item.codeAndTestVerification -eq 'NotRunByThisView') 'ReferenceNotImplementationProof'
Assert ($farm.results[0].item.facets.nextChoices.meaning -eq 'RecordedRelationsNotMandatoryRoute') 'NoForcedRoute'
$unstarted=$v.items|Where-Object id -eq WI-ACTOR-CONSUME
Assert ($unstarted.recordedImplementation.currentStage -eq 'E0' -and $unstarted.facets.choice.blockReasonCodes -contains 'ApprovedDesignRequired') 'BlockedRegistrationNotDropped'
Assert ($v.completion -eq 'InventoryProjectionNotAllE4Prepared') 'NotAllE4Achievement'
Assert (@($v.items.referenceStates|Where-Object state -eq 'ReferencedFileMissingNotImplementationAbsent'|Select-Object -ExpandProperty path -Unique).Count -eq 19) 'MissingLegacyPathsVisibleNotRepaired'
Assert (@($v.items.referenceStates|Where-Object {$_.state -eq 'FilePresentNotImplementationVerified' -and $_.sha256 -notmatch '^[A-Fa-f0-9]{64}$'}).Count -eq 0) 'PresentReferenceFingerprints'
Assert ((Get-Content (Join-Path $root $md) -Raw -Encoding UTF8) -match '참조 파일 없음:') 'MissingReferenceRenderedWithoutBrokenLink'
Assert ((Get-Content (Join-Path $root $md) -Raw -Encoding UTF8) -match '지금·여기·나·너·이렇게') 'D394ReadingLabels'
$rejected=$false;try{& $manager -IndexPath $index -Wi -Circulation|Out-Null}catch{if($_.Exception.Message -notlike '*WiUseIdText*'){throw};$rejected=$true};Assert $rejected 'AmbiguousQueryRejected'
foreach($p in $protected){Assert ($before[$p] -eq (Get-FileHash (Join-Path $root $p)).Hash) "Preserved:$p"}
$result=[ordered]@{status='Passed';checks=$count;scope='FileProjectionNotGameOrE4Achievement';managerSha256=(Get-FileHash $manager).Hash}
[IO.File]::WriteAllText((Join-Path $root "$folder/results.json"),($result|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
Write-Output "WI view: $count passed"
