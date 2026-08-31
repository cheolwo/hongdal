$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$manager=Join-Path $root 'eng/planning-inquiries/manage-inquiry-search.ps1'
$folder='artifacts/local/validation/planning-inquiry-spatial-search'
$null=New-Item -ItemType Directory -Force -Path (Join-Path $root $folder)
$index="$folder/test-index.json"; $md="$folder/test-links.md"
$utf8=[Text.UTF8Encoding]::new($false);$checks=0
function Assert([bool]$Ok,[string]$Name){if(-not $Ok){throw "Failed:$Name"};$script:checks++;Write-Output "pass $Name"}
function Reject([scriptblock]$Action,[string]$Code){$caught=$false;try{& $Action|Out-Null}catch{if($_.Exception.Message -notlike "*$Code*"){throw};$caught=$true};Assert $caught $Code}
function Query([string]$Id){& $manager -IndexPath $index -Spatial -Id $Id|ConvertFrom-Json}
$protected=@('eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json','eng/planning-inquiries/sources.json','eng/planning-inquiries/spatial-relations.json','eng/execution-ledgers/playable-loop-inquiry-implementation-scope.json')
$before=@{};foreach($p in $protected){$before[$p]=(Get-FileHash (Join-Path $root $p)).Hash}
$null=& $manager -Mode Write -IndexPath $index -SpatialMarkdownPath $md
$first=(Get-FileHash (Join-Path $root $index)).Hash
$null=& $manager -Mode Write -IndexPath $index -SpatialMarkdownPath $md
Assert ($first -eq (Get-FileHash (Join-Path $root $index)).Hash) 'DeterministicSpatialRebuild'
$null=& $manager -Mode Validate -IndexPath $index -SpatialMarkdownPath $md
Assert $true 'GeneratedMarkdownAndIndexValid'
$db=Get-Content -Raw -Encoding UTF8 (Join-Path $root $index)|ConvertFrom-Json
$viewKinds=@($db.spatial.circulation.items|Where-Object questionId -eq 'perspective-scale-wi-classification')
Assert ($viewKinds.Count -eq 1 -and $viewKinds[0].facets.player.text -match '공통과 특화 중첩' -and $viewKinds[0].facets.choice.text -match '명시 승인 이유' -and -not $viewKinds[0].implementationVerified) 'D418OverlappingRolesNotExclusivePermissionEnum'
$viewDevelopment=@($db.spatial.circulation.items|Where-Object questionId -eq 'perspective-independent-development')
Assert ($viewDevelopment.Count -eq 1 -and $viewDevelopment[0].facets.result.text -match '성공으로 복사하지' -and $viewDevelopment[0].facets.nextChoices.text -match 'Session/Actor' -and -not $viewDevelopment[0].implementationVerified) 'D418IndependentDevelopmentStillRequiresTransitionEvidence'
$automation=@($db.spatial.circulation.items|Where-Object questionId -eq 'optional-auto-hunting-operations')
Assert ($automation.Count -eq 1 -and $automation[0].facets.time.state -eq 'Undetermined' -and $automation[0].facets.time.text -match 'FutureExtension' -and -not $automation[0].implementationVerified) 'D417BattleAutomationNotOpenWorldHunting'
$areaDirection=@($db.spatial.circulation.items|Where-Object questionId -eq 'multi-area-choice-parallel-development')
Assert ($areaDirection.Count -eq 1 -and $areaDirection[0].sourceDecisionState -eq 'ConfirmedDirection' -and -not $areaDirection[0].implementationVerified -and $areaDirection[0].facets.space.text -match '강제.*경로 아님') 'D416IndependentAreasNotRequiredRoute'
$brewDirection=@($db.spatial.circulation.items|Where-Object questionId -eq 'town-brewing-first-participation')
Assert ($brewDirection.Count -eq 1 -and $brewDirection[0].sourceDecisionState -eq 'Confirmed' -and $brewDirection[0].facets.choice.state -eq 'Explicit' -and $brewDirection[0].facets.time.state -eq 'Undetermined' -and $brewDirection[0].facets.result.state -eq 'Undetermined' -and -not $brewDirection[0].implementationVerified) 'D428BrewingParticipationConfirmedNotRecipeOrImplementation'
Assert (@($db.questions|Where-Object kind -ne SemanticFollowup).Count -eq 403) 'Original403NumberedQuestionsPreserved'
$grace=@($db.questions|Where-Object questionId -eq 'harvest-ready-grace-window')
Assert ($grace.Count -eq 1 -and $grace[0].recordStatus -eq 'Asked' -and $grace[0].kind -eq 'SemanticFollowup' -and $grace[0].directExcerptRefs.Count -gt 0) 'D398AskedNotApprovedOrRenumbered'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'Q-378' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1) 'ExistingQ378ApprovalPreserved'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'seasonal-time-context-direction' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1) 'D399DirectionNotAskedOrImplementationApproval'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'seasonal-sowing-outside-window' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1) 'D400SowingConfirmedWithoutChangingQ089'
Assert (@($db.questions|Where-Object {$_.questionId -in @('seasonal-cultivation-facility-mitigation','seasonal-nature-monster-variation') -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 2) 'D400TwoDirectionsNotExecutionEvidence'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'seasonal-greenhouse-protection-limit' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1) 'D401GreenhouseAnswerConfirmed'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'seasonal-cultivation-magic-protection' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1) 'D401MagicDirectionNotExecutionEvidence'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'seasonal-magic-protection-unattended' -and $_.recordStatus -eq 'Asked'}).Count -eq 1) 'D401UnattendedQuestionNotApproved'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'inquiry-progression-clusters' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1 -and @($db.questions|Where-Object {$_.questionId -eq 'tech-tree-progressive-disclosure' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1) 'D402TwoSemanticStatesPreserved'
$northern=@($db.spatial.circulation.items|Where-Object {$_.questionId -in @('northern-spring-hungry-farm-encounter','hungry-farm-npc-local-knowledge-help','northern-spring-snow-conifer-survey','hungry-farm-first-meal-role-split')})
Assert ($northern.Count -eq 4 -and @($northern|Where-Object reviewState -ne 'SourceCompared').Count -eq 0) 'D414FourSourceComparedItems'
Assert (@($northern|Where-Object {$_.questionId -eq 'hungry-farm-first-meal-role-split' -and $_.sourceDecisionState -eq 'Confirmed' -and $_.facets.choice.state -eq 'Explicit'}).Count -eq 1) 'D415RoleSplitConfirmedAfterFrozenD414'
Assert (@($northern|Where-Object {$_.questionId -eq 'northern-spring-snow-conifer-survey' -and $_.facets.choice.text -match '추가 조사·배치 준비 중단' -and -not $_.implementationVerified}).Count -eq 1) 'D415HistoricalSnowNotCurrentExecution'
$spring=@($db.spatial.circulation.items|Where-Object {$_.questionId -in @('farm-encounter-spring-without-snow','farm-meal-low-pressure-help','farm-meal-small-contribution-return','farm-meal-player-inventory-connection')})
Assert ($spring.Count -eq 4 -and @($spring|Where-Object {$_.reviewState -ne 'SourceCompared' -or $_.implementationVerified}).Count -eq 0) 'D415FourDirectionsNotGameCompletion'
Assert (@($db.spatial.edges|Where-Object {$_.from -in @($northern.questionId) -or $_.to -in @($northern.questionId)}).Count -eq 0) 'D414NoAutomaticHRelation'
$progression=@($db.spatial.circulation.items|Where-Object {$_.questionId -in @('inquiry-progression-clusters','tech-tree-progressive-disclosure')})
Assert ($progression.Count -eq 2 -and @($progression|Where-Object {$_.reviewState -ne 'SourceCompared' -or $_.implementationVerified}).Count -eq 0) 'D410ProgressionSourceReviewNotImplementation'
Assert (@($db.questions|Where-Object {$_.questionId -eq 'korean-24-solar-terms-planning' -and $_.recordStatus -eq 'Confirmed'}).Count -eq 1 -and @($db.questions|Where-Object {$_.questionId -eq 'solar-term-seasonal-food-research' -and $_.recordStatus -eq 'ConfirmedDirection'}).Count -eq 1) 'D403PrimaryAndInlineDirectionHaveIndependentStates'
Assert (@($db.spatial.nodes|Where-Object kind -eq H1).Count -eq 84) 'H1Full84NotFilteredByQuestions'
Assert (@($db.spatial.nodes|Where-Object kind -eq H2).Count -eq 38) 'H2Full38'
Assert (@($db.spatial.nodes|Where-Object kind -eq H3).Count -eq 20 -and @($db.spatial.nodes|Where-Object kind -eq H4).Count -eq 6) 'UpperDefinitions'
Assert (@($db.spatial.nodes|Where-Object kind -eq AreaSetCompositionPattern).Count -eq 8 -and @($db.spatial.nodes|Where-Object kind -eq AreaSet).Count -eq 1) 'PatternNotActualAreaSet'
Assert (@($db.spatial.nodes|Where-Object kind -eq UnregisteredRequirement).Count -eq 2) 'ProposalsNotAddedToH1'
$circle="$folder/test-circulation.md"
$null=& $manager -Mode Write -IndexPath $index -CirculationMarkdownPath $circle
$null=& $manager -Mode Validate -IndexPath $index -CirculationMarkdownPath $circle
$circleText=Get-Content -LiteralPath (Join-Path $root $circle) -Raw
Assert ($circleText -notmatch '(?m)^##[ \t]*\r?$') 'NoEmptyCirculationHeading'
foreach($topicName in @($db.spatial.circulation.items.topicCode|Sort-Object -Unique)) {
    Assert ($circleText.Contains("## $topicName")) "OriginalTopicHeading:$topicName"
}
Assert ($db.spatial.circulation.counts.total -eq $db.questions.Count -and $db.spatial.circulation.counts.sourceCompared -eq 474 -and $db.spatial.circulation.counts.unreviewed -eq 3) 'D448ReviewedCoverageNotNewMeaningCompletion'
Assert ((@($db.spatial.circulation.items|Where-Object reviewState -eq Unreviewed|ForEach-Object questionId|Sort-Object) -join ',') -eq 'town-brewing-optional-reality-reference,town-brewing-public-data-research,town-brewing-reality-detail-first-purpose') 'D428NewIdsRemainUnreviewedNotInventedRelations'
Assert (@($db.spatial.circulation.items|Where-Object {@($_.facets.PSObject.Properties.Name).Count -eq 0}).Count -eq 0) 'AllQuestionsHaveView'
foreach($item in $db.spatial.circulation.items){if(@($item.facets.PSObject.Properties.Name).Count -ne 7){throw 'Failed:SevenFacets'}}
Assert $true 'SevenFacetsIncludingUnreviewed'
Assert ($db.spatial.circulation.counts.sourceProblemItems -eq 3 -and @($db.spatial.circulation.items|Where-Object {$_.questionId -eq 'Q-272' -and $_.facets.time.state -eq 'EvidenceMissing'}).Count -eq 1) 'LostSourceNotInventedCirculation'
Assert (@($db.spatial.circulation.documents|Where-Object {-not $_.indexedQuestionSource}).Count -eq 40 -and @($db.spatial.circulation.registeredPlanning).Count -eq 20) 'OutsideQuestionIndexAndRegisteredPlansSeparate'
$heat=& $manager -IndexPath $index -Circulation -Id Q371|ConvertFrom-Json
Assert ($heat.totalMatches -eq 1 -and $heat.results[0].item.facets.target.text -match '별개' -and $heat.results[0].item.facets.result.text -match '직접가열') 'HeatMultipleTargetsAndForbiddenInference'
$waiting=& $manager -IndexPath $index -Circulation -Unreviewed -Limit 1000|ConvertFrom-Json
Assert ($waiting.totalMatches -eq 3) 'D448ThreeNewUnreviewedQuestionsRemainVisible'
$discovery=& $manager -IndexPath $index -Circulation -Id discovery-led-play-causality | ConvertFrom-Json
Assert ($discovery.results[0].item.sourceDecisionState -eq 'ConfirmedDirection' -and -not $discovery.results[0].item.implementationVerified -and $discovery.results[0].item.facets.nextChoices.text -match '필수 첫 순서 아님') 'D408DirectionNotFirstSequenceOrImplementation'
Assert ($discovery.results[0].item.facets.choice.text -match 'WI-NATURE-05 획득과 다름' -and $discovery.results[0].item.facets.result.text -match '해금·보상 확정 아님') 'D408ObservationNotAuthority'
$farmRead=& $manager -IndexPath $index -Circulation -Id Q090 | ConvertFrom-Json
Assert ($farmRead.results[0].item.facets.time.state -eq 'Undetermined' -and $farmRead.results[0].item.facets.nextChoices.state -eq 'Undetermined') 'D408MissingRulesRemainUndetermined'
$return=& $manager -IndexPath $index -Circulation -Id Q239|ConvertFrom-Json
Assert ($return.results[0].item.facets.nextChoices.text -match '반납.*미기재' -and -not $return.results[0].item.implementationVerified) 'EquipmentReturnNotInvented'
$wi=& $manager -IndexPath $index -Circulation -Text WI-FARM-04 -Limit 1000|ConvertFrom-Json
Assert ($wi.totalMatches -gt 0 -and $wi.results[0].item.topicImplementationContext[0].basis -eq 'TopicContextNotExactQuestionImplementation') 'WiKeywordIsTopicContextOnly'
Assert (@($heat.moduleGuide|Where-Object {$_.implementationState -eq 'DocumentationReferenceOnly'}).Count -eq 4) 'D393GuideNotRuntimeBinding'
$q143=Query Q143
Assert ($q143.results[0].directRelations[0].kind -eq 'QuestionRequiresRole' -and $q143.results[0].relatedNodes[0].reviewState -eq 'ExistingPlanNotRegistered') 'Q143ExistingIntakePlan'
Assert (@($q143.results[0].ancestorPaths|Where-Object basisKind -eq SupportsRequirementCandidate).Count -gt 0) 'CandidateParentPathsRemainCandidate'
$farm=Query 'h1-stock:farm-production'
Assert (@($farm.results[0].node.wiIds).Count -eq 4 -and $farm.results[0].node.capabilities -contains 'Spatial.HarvestWorkArea') 'FarmActionAndCapability'
Assert (@($farm.results[0].ancestorPaths|Where-Object {$_.targetId -eq 'h2-candidate:highland-production' -and -not $_.derived}).Count -eq 1) 'DirectH2Path'
Assert (@($farm.results[0].ancestorPaths|Where-Object {$_.derived -and $_.edgeKinds -contains 'ContainsOptional'}).Count -gt 0) 'OptionalInheritedPathNotRequired'
Assert (@($farm.results[0].contextRelations|Where-Object kind -eq ExpressionSupports).Count -gt 0) 'ExpressionSupportNotIdentity'
$h2=Query 'h2-candidate:highland-production'
Assert (@($h2.results[0].inheritedQuestionLinks|Where-Object from -eq 'Q-079').Count -eq 1 -and @($h2.results[0].directRelations|Where-Object kind -eq QuestionSupportsH).Count -eq 0) 'H2InheritedQuestionNotDirect'
$shelter=Query 'h1-stock:nature-shelter'
Assert (@($shelter.results[0].directRelations|Where-Object {$_.from -in @('Q-202','Q-347','Q-371')}).Count -eq 3) 'ShelterVisitorBrewHeatDistinct'
Assert (@($shelter.results[0].relatedNodes|Where-Object {$_.kind -eq 'VisualEvidence' -and $_.evidenceKind -in @('SharedUIConcept','PrefabPreview')}).Count -eq 2) 'RoleImagesNotH1Assembly'
Assert (@($db.spatial.nodes|Where-Object {$_.kind -eq 'VisualEvidence' -and $_.currentDefinitionEquivalent}).Count -eq 0) 'HistoricalImageNotCurrentCompletion'
$non=Query Q398;Assert ($non.results[0].node.spatialDisposition -eq 'NonSpatial' -and $non.results[0].directRelations.Count -eq 0) 'NonSpatialNoInventedH'
$unknown=Query Q001;Assert ($unknown.results[0].node.reviewState -eq 'Unreviewed') 'UnreviewedIsNotNonSpatial'
$missing=Query Q272;Assert ($missing.results[0].node.spatialDisposition -eq 'MissingSource') 'LostQuestionNotRecovered'
$deferred=Query Q089;Assert ($deferred.results[0].node.decisionState -eq 'Confirmed' -and $deferred.results[0].node.sourceDecisionState -eq 'Deferred' -and $deferred.results[0].issues.Count -gt 0) 'OriginalAndIndexStatesSeparate'
Assert (@($db.spatial.issues|Where-Object {$_.subjectId -eq 'h1-stock:farm-worker-waiting' -and $_.code -eq 'CatalogDefinitionHashMismatch'}).Count -eq 1) 'ExistingHashDriftReportedNotRepaired'
$noimage=& $manager -IndexPath $index -Gap NoImage -Limit 1000|ConvertFrom-Json
Assert ($noimage.totalMatches -eq 81) 'NoImageExactGapNotNoAssembly'
$unmapped=& $manager -IndexPath $index -Gap UnmappedRequirements|ConvertFrom-Json
Assert ($unmapped.totalMatches -eq 2) 'UnmappedRequirementFilter'
$water=& $manager -IndexPath $index -Spatial -Text '급수'|ConvertFrom-Json
Assert (@($water.results|Where-Object {$_.node.id -eq 'spatial-requirement:water-intake-access'}).Count -eq 1) 'RoleKeywordSearch'
$area=Query 'area-set:sim:pyeongchang:farm-hub-town.v1'
Assert ($area.totalMatches -eq 1 -and $area.results[0].node.reviewState -eq 'NoExplicitHLink') 'ActualAreaSetExactIdentity'
Assert (@($db.spatial.edges|Where-Object {$_.from -like 'area-set:*' -or $_.to -like 'area-set:*'}).Count -eq 0) 'NoInferredActualAreaSetLink'
# Mutated relation fixtures stay under artifacts. Production sources are never rewritten.
$config=Get-Content -Raw -Encoding UTF8 (Join-Path $root 'eng/planning-inquiries/sources.json')|ConvertFrom-Json
$original=Get-Content -Raw -Encoding UTF8 (Join-Path $root 'eng/planning-inquiries/spatial-relations.json')
$config.spatialRelationsRef="$folder/relations.json";$configRef="$folder/sources.json"
[IO.File]::WriteAllText((Join-Path $root $configRef),($config|ConvertTo-Json -Depth 30),$utf8)
function Save-Relations($Value){[IO.File]::WriteAllText((Join-Path $root $config.spatialRelationsRef),($Value|ConvertTo-Json -Depth 40),$utf8)}
function Check-Fixture {& $manager -Mode Write -SourcesPath $configRef -IndexPath "$folder/fixture-index.json"}
$archive=@($db.spatial.circulation.items|Where-Object {$null -ne $_.proof -and $_.proof.path -ne $_.sourceRef})
Assert ($archive.Count -gt 0 -and @($archive|Where-Object {@($_.directExcerptRefs) -cnotcontains "$($_.proof.path):$($_.proof.line)"}).Count -eq 0) 'D410ExactRegisteredArchiveSourceAccepted'
$r=$original|ConvertFrom-Json;$r.circulation.reviews=@($r.circulation.reviews|Where-Object questionId -ne 'Q-001');Save-Relations $r;$null=Check-Fixture
$fixture=Get-Content -Raw (Join-Path $root "$folder/fixture-index.json")|ConvertFrom-Json
$missingReview=@($fixture.spatial.circulation.items|Where-Object questionId -eq 'Q-001')[0]
Assert ($missingReview.reviewState -eq 'Unreviewed' -and $missingReview.facets.time.state -eq 'Unreviewed' -and $fixture.spatial.circulation.counts.unreviewed -eq ($db.spatial.circulation.counts.unreviewed + 1)) 'UnreviewedNotUndeterminedFixture'
$r=$original|ConvertFrom-Json;$r.circulation.reviews[0].proof=($r.circulation.reviews|Where-Object questionId -eq 'Q-001').proof;Save-Relations $r;Reject {Check-Fixture} 'SpatialQuestionAnchorIdentity'
$r=$original|ConvertFrom-Json;$r.circulation.reviews+=@($r.circulation.reviews[0]);Save-Relations $r;Reject {Check-Fixture} 'CirculationDuplicateReview'
$r=$original|ConvertFrom-Json;$r.circulation.reviews[0].facets.PSObject.Properties.Remove('time');Save-Relations $r;Reject {Check-Fixture} 'CirculationFacetsIncomplete'
$r=$original|ConvertFrom-Json;$r.circulation.reviews[0].facets.time.state='Passed';Save-Relations $r;Reject {Check-Fixture} 'CirculationFacetInvalid'
$r=$original|ConvertFrom-Json;$r.circulation.reviews[0].proof.endLine=0;Save-Relations $r;Reject {Check-Fixture} 'CirculationInvalidEndLine'
$r=$original|ConvertFrom-Json;$r.circulation.moduleGuide[0].implementationState='RuntimeReady';Save-Relations $r;Reject {Check-Fixture} 'CirculationModuleIsNotRuntimeEvidence'
$r=$original|ConvertFrom-Json;$r.circulation.documentRoot='docs';Save-Relations $r;Reject {Check-Fixture} 'CirculationDocumentRootNotApproved'
$r=$original|ConvertFrom-Json;$lost=$r.circulation.reviews|Where-Object questionId -eq 'Q-272';$lost.facets.time.state='Explicit';Save-Relations $r;Reject {Check-Fixture} 'CirculationMissingSourceCannotBecomeExplicit'
$r=$original|ConvertFrom-Json;$lost=$r.circulation.reviews|Where-Object questionId -eq 'Q-272';$lost.sourceDecisionState='Confirmed';Save-Relations $r;Reject {Check-Fixture} 'CirculationMissingSourceCannotBecomeExplicit'
$r=$original|ConvertFrom-Json;$r.links+=@($r.links[0]);Save-Relations $r;Reject {Check-Fixture} 'SpatialDuplicateEdge'
$r=$original|ConvertFrom-Json;$r.links[0].to='h1-stock:does-not-exist';Save-Relations $r;Reject {Check-Fixture} 'SpatialUnknownEndpoint'
$r=$original|ConvertFrom-Json;$r.links[0].proof.sha256='0'*64;Save-Relations $r;Reject {Check-Fixture} 'SpatialProofDrift'
$r=$original|ConvertFrom-Json;$r.links[0].proof.line=1;Save-Relations $r;Reject {Check-Fixture} 'SpatialAnchorMismatch'
$r=$original|ConvertFrom-Json;$wrong=($r.questionReviews|Where-Object questionId -eq 'Q-143').proof;$r.questionReviews[0].proof=$wrong;Save-Relations $r;Reject {Check-Fixture} 'SpatialQuestionAnchorIdentity'
$r=$original|ConvertFrom-Json;$wrong=($r.questionReviews|Where-Object questionId -eq 'Q-143').proof;$r.links[0].proof=$wrong;Save-Relations $r;Reject {Check-Fixture} 'SpatialQuestionAnchorIdentity'
$r=$original|ConvertFrom-Json;$r.links[0].from='Q-398';Save-Relations $r;Reject {Check-Fixture} 'SpatialNonSpatialOrUnreviewedLink'
$r=$original|ConvertFrom-Json;$r.images[0].hIds=@('h1-stock:nature-shelter');Save-Relations $r;Reject {Check-Fixture} 'SpatialHistoricalImageTargetMismatch'
$r=$original|ConvertFrom-Json;$r.images[0].sha256='0'*64;Save-Relations $r;Reject {Check-Fixture} 'SpatialImageDrift'
$r=$original|ConvertFrom-Json;$r.images[0].manifestPointer='/Captures/999';Save-Relations $r;Reject {Check-Fixture} 'SpatialManifestPointerInvalid'
$r=$original|ConvertFrom-Json;$r.images[0].path=$r.images[1].path;$r.images[0].sha256=$r.images[1].sha256;Save-Relations $r;Reject {Check-Fixture} 'SpatialManifestImageMismatch'
$r=$original|ConvertFrom-Json;$r.images[2].hIds+=@('h1-stock:nature-safe-recovery-camp');Save-Relations $r;$null=Check-Fixture
$shared=Get-Content -Raw (Join-Path $root "$folder/fixture-index.json")|ConvertFrom-Json
Assert (@($shared.spatial.nodes|Where-Object id -eq 'visual-evidence:visitor-concept').Count -eq 1 -and @($shared.spatial.edges|Where-Object from -eq 'visual-evidence:visitor-concept').Count -eq 2) 'SharedImageOneEvidenceTwoLinks'
$r=$original|ConvertFrom-Json;Save-Relations $r;$null=Check-Fixture
$r.links[0].rationale+=' changed';Save-Relations $r;Reject {& $manager -Mode Validate -SourcesPath $configRef -IndexPath "$folder/fixture-index.json"} 'StaleOrModifiedIndex'
$config.PSObject.Properties.Remove('spatialRelationsRef');[IO.File]::WriteAllText((Join-Path $root $configRef),($config|ConvertTo-Json -Depth 30),$utf8)
$null=& $manager -Mode Write -SourcesPath $configRef -IndexPath "$folder/legacy-index.json"
$legacy=Get-Content -Raw (Join-Path $root "$folder/legacy-index.json")|ConvertFrom-Json
Assert ($null -eq $legacy.PSObject.Properties['spatial'] -and $legacy.questions.Count -eq $db.questions.Count) 'OptionalExtensionLegacyCompatibility'
foreach($p in $protected){Assert ($before[$p] -eq (Get-FileHash (Join-Path $root $p)).Hash) "Preserved:$p"}
$report=[ordered]@{status='Passed';checks=$checks;powerShellVersion=$PSVersionTable.PSVersion.ToString();managerSha256=(Get-FileHash $manager).Hash;scope='PureFileFixturesNotEditorOrE5'}
[IO.File]::WriteAllText((Join-Path $root "$folder/results.json"),($report|ConvertTo-Json),$utf8)
Write-Output "Planning inquiry spatial search: $checks passed"
