# Existing inquiry search extension; no H registration, authority writes or rendering.
function Spatial-Full([string] $Ref, [switch] $Evidence) {
    if ([IO.Path]::IsPathRooted($Ref)) {
        $p=[IO.Path]::GetFullPath($Ref)
        $approved=[IO.Path]::GetFullPath('C:/Users/user/ssalddel/artifacts/local/')
        Need ($Evidence -and $p.StartsWith($approved,[StringComparison]::OrdinalIgnoreCase)) "SpatialOutsideEvidenceRoot:$Ref"
        return $p
    }
    return Full $Ref
}
function Get-InquirySpatialIndex($Database, [string] $RelationsRef) {
    $sources=[Collections.Generic.List[object]]::new(); $observed=@{}
    $nodes=[Collections.Generic.List[object]]::new(); $nodeMap=@{}
    $edges=[Collections.Generic.List[object]]::new(); $edgeKeys=@{}
    $issues=[Collections.Generic.List[object]]::new()
    function Observe([string]$Ref,[switch]$Evidence) {
        if (-not $observed.ContainsKey($Ref)) {
            $full=Spatial-Full $Ref -Evidence:$Evidence
            Need (Test-Path -LiteralPath $full -PathType Leaf) "SpatialSourceMissing:$Ref"
            $observed[$Ref]=[ordered]@{path=$Ref;sha256=(Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash}
            $sources.Add($observed[$Ref])
        }
        return $observed[$Ref]
    }
    function Json([string]$Ref) { $null=Observe $Ref; return Read-Json $Ref }
    function Node($N) {
        Need (-not $nodeMap.ContainsKey($N.id)) "SpatialDuplicateNode:$($N.id)"
        $nodeMap[$N.id]=$N; $nodes.Add($N)
    }
    function Edge([string]$From,[string]$To,[string]$Kind,$Proof,[string]$Reason) {
        Need ($nodeMap.ContainsKey($From) -and $nodeMap.ContainsKey($To)) "SpatialUnknownEndpoint:$From->$To"
        $key="$Kind|$From|$To"
        Need (-not $edgeKeys.ContainsKey($key)) "SpatialDuplicateEdge:$key"
        $edgeKeys[$key]=$true
        $edges.Add([ordered]@{from=$From;to=$To;kind=$Kind;direct=$true;proof=$Proof;rationale=$Reason})
    }
    function Proof($P) {
        Need ($P.sha256 -match '^[a-fA-F0-9]{64}$') 'SpatialInvalidProofHash'
        $s=Observe $P.path
        Need ($s.sha256 -eq $P.sha256) "SpatialProofDrift:$($P.path)"
        $lines=@(Get-Content -LiteralPath (Full $P.path) -Encoding UTF8)
        Need ($P.line -ge 1 -and $P.line -le $lines.Count) "SpatialInvalidLine:$($P.path)"
        Need (-not [string]::IsNullOrWhiteSpace($P.anchorText)) 'SpatialEmptyAnchor'
        Need ($lines[$P.line-1].Contains([string]$P.anchorText)) "SpatialAnchorMismatch:$($P.path):$($P.line)"
        return $P
    }
    function QuestionProof([string]$QuestionId,$P) {
        $verified=Proof $P
        $matches=@($Database.sections|Where-Object {$_.sourceRef -ceq $P.path -and $_.line -eq $P.line -and $_.directQuestionIds -ccontains $QuestionId})
        Need ($matches.Count -eq 1) "SpatialQuestionAnchorIdentity:$QuestionId"
        return $verified
    }
    function ManifestEntry($Document,[string]$Pointer) {
        Need ($Pointer.StartsWith('/') -and $Pointer.Length -gt 1) 'SpatialManifestPointerInvalid'
        $value=$Document
        foreach($part in $Pointer.Substring(1).Split('/')) {
            Need ($part -notmatch '~(?![01])') 'SpatialManifestPointerInvalid'
            $key=$part.Replace('~1','/').Replace('~0','~')
            if($value -is [array]) {
                Need ($key -match '^(0|[1-9][0-9]*)$' -and $key.Length -lt 10) 'SpatialManifestPointerInvalid'
                $position=[int]$key
                Need ($position -lt $value.Count) 'SpatialManifestPointerInvalid'
                $value=$value[$position]
            }else{
                Need ($null -ne $value) 'SpatialManifestPointerInvalid'
                $properties=@($value.PSObject.Properties|Where-Object Name -ceq $key)
                Need ($properties.Count -eq 1) 'SpatialManifestPointerInvalid'
                $value=$properties[0].Value
            }
        }
        return $value
    }
    $cfg=Json $RelationsRef
    Need ($cfg.schemaVersion -eq 'planning-inquiry-spatial-relations.v1') 'SpatialUnknownSchema'
    $null=Observe 'eng/planning-inquiries/spatial-query.ps1'
    $catalog=Json $cfg.catalogRef; $base=($cfg.catalogRef -replace '[^/]+$','')
    $names=Json $cfg.namesRef; $nameMap=@{}
    foreach($n in @($names.h2Patterns)+@($names.h3Patterns)) {
        Need (-not $nameMap.ContainsKey($n.stableId)) "SpatialDuplicateName:$($n.stableId)"
        $nameMap[$n.stableId]=$n
    }
    foreach($q in $Database.questions) {
        Node ([ordered]@{id=$q.questionId;kind='Question';title=$q.questionId;topic=$q.topicCode;decisionState=$q.recordStatus;sourceRef=$q.sourceRef;reviewState='Unreviewed';spatialDisposition='Unreviewed';implementationLookupRef=$q.implementationLookupRef})
    }
    $definitions=@{}
    foreach($level in @('H1','H2','H3','H4')) {
        $refs=@($catalog.($level.ToLowerInvariant()+'DefinitionRefs'))
        foreach($ref in $refs) {
            $path=$base+$ref.definitionPath; $definition=Json $path
            Need ($definition.stableId -ceq $ref.stableId) "SpatialDefinitionIdentity:$path"
            $proof=[ordered]@{path=$path;pointer='/';sha256=$observed[$path].sha256;revision=$definition.revision}
            $flags=[Collections.Generic.List[string]]::new()
            foreach($pair in @(@($path,$ref.definitionSha256,'Definition'),@(($base+$ref.documentPath),$ref.documentSha256,'Document'))) {
                $s=Observe $pair[0]
                $text=ConvertTo-DeterministicText ([IO.File]::ReadAllText((Full $pair[0])))
                $hasher=[Security.Cryptography.SHA256]::Create()
                try {$lf=([BitConverter]::ToString($hasher.ComputeHash([Text.Encoding]::UTF8.GetBytes($text)))).Replace('-','')} finally {$hasher.Dispose()}
                if($lf -ne $pair[1]) {
                    $flags.Add('Catalog'+$pair[2]+'HashMismatch')
                    $issues.Add([ordered]@{subjectId=$ref.stableId;code='Catalog'+$pair[2]+'HashMismatch';sourceRef=$pair[0];expected=$pair[1];raw=$s.sha256;lf=$lf;action='ReviewOnlyNoRepair'})
                }
            }
            if($definition.revision -ne $ref.revision) {$flags.Add('CatalogRevisionMismatch');$issues.Add([ordered]@{subjectId=$ref.stableId;code='CatalogRevisionMismatch';expected=$ref.revision;actual=$definition.revision;sourceRef=$path})}
            $title=$definition.title
            if($nameMap.ContainsKey($ref.stableId)) {$title=Field $nameMap[$ref.stableId] 'spatialDisplayNameKo' $title}
            Node ([ordered]@{id=$ref.stableId;kind=$level;cardKind=Field $definition 'cardKindCode' $(if($level -eq 'H1'){'InteractionSpace'}else{$level});title=$title;summary=$definition.summary;revision=$definition.revision;knowledgeState=$definition.knowledgeStateCode;reviewState='Unreviewed';implementationState='NotEvaluated';visualState='NoEvidence';proof=$proof;roles=@(Field $definition 'spatialRoleCodes' @());capabilities=@(Field $definition 'capabilityCodes' @());wiIds=@(Field $definition 'wiIds' @());assetCandidateRefs=@(Field $definition 'grammarVariantRefs' (Field $definition 'grammarSetRefs' @()));unresolved=@(Field $definition 'unresolvedItems' @());reviewFlags=@($flags)})
            $definitions[$ref.stableId]=$definition
        }
    }
    foreach($id in @(Ordinal @($definitions.Keys))) {
        $def=$definitions[$id]; $p=$nodeMap[$id].proof
        foreach($f in @('requiredH1Refs','optionalH1Refs','requiredH2Refs','optionalH2Refs','requiredH3Refs','optionalH3Refs','supportsInteractionH1Refs')) {
            $kind=if($f -like 'required*'){'ContainsRequired'}elseif($f -like 'optional*'){'ContainsOptional'}else{'ExpressionSupports'}
            $seen=@{}; $position=0
            foreach($child in @(Field $def $f @())) {
                Need (-not $seen.ContainsKey($child)) "SpatialDuplicateChild:$id/$f/$child"; $seen[$child]=$true
                Need ($nodeMap.ContainsKey($child)) "SpatialUnknownChild:$id/$child"
                $expected=if($f -match 'H([123])Refs'){'H'+$Matches[1]}else{'H1'}
                Need ($nodeMap[$child].kind -eq $expected) "SpatialChildLevelMismatch:$id/$child"
                Edge $id $child $kind ([ordered]@{path=$p.path;pointer="/$f/$position";sha256=$p.sha256;revision=$p.revision}) 'DeclaredInDefinitionNotRuntimeEvidence'
                $position++
            }
        }
    }
    foreach($l in @('h2','h3','h4')) {
        $key=if($l -eq 'h4'){'h4Blueprint'}else{$l}
        $actual=@($nodes|Where-Object kind -eq $l.ToUpperInvariant()).Count
        if($catalog.counts.$key -ne $actual){$issues.Add([ordered]@{subjectId='catalog';code='CatalogCountMismatch';level=$l;expected=$catalog.counts.$key;actual=$actual;sourceRef=$cfg.catalogRef})}
    }
    $patterns=Json $cfg.areaPatternsRef; $i=0
    foreach($p in $patterns.compositionPatterns) {
        Node ([ordered]@{id=$p.compositionPatternStableId;kind='AreaSetCompositionPattern';title=$p.title;areaRole=$p.areaRoleCode;reviewState='Unreviewed';implementationState='NotEvaluated';wiIds=@($p.relatedWiStableIds);unresolved=@($p.unresolvedItems);proof=[ordered]@{path=$cfg.areaPatternsRef;pointer="/compositionPatterns/$i";sha256=$observed[$cfg.areaPatternsRef].sha256;revision=$patterns.revision}})
        foreach($placement in $p.h3Placements) {Edge $p.compositionPatternStableId $placement.selectedH3PatternRef 'PatternSelectsH3' ([ordered]@{path=$cfg.areaPatternsRef;pointer="/compositionPatterns/$i/h3Placements";sha256=$observed[$cfg.areaPatternsRef].sha256;placementId=$placement.placementStableId}) 'CandidateCompositionNotActualAreaSet'}
        Edge $p.compositionPatternStableId $p.worldIntentRef 'WorldIntent' $nodeMap[$p.compositionPatternStableId].proof 'IntentNotContainment'
        $i++
    }
    foreach($ref in $cfg.areaSetRefs) {
        $area=Json $ref
        Node ([ordered]@{id=$area.areaSetStableId;kind='AreaSet';title=$area.title;revision=$area.revision;reviewState='NoExplicitHLink';areaRefs=@($area.areaRefs);landscapeGraphRefs=@($area.landscapeGraphRefs);proof=[ordered]@{path=$ref;pointer='/';sha256=$observed[$ref].sha256}})
    }
    foreach($r in $cfg.requirements) {
        Need ($r.id -like 'spatial-requirement:*') 'SpatialRequirementMustNotRegisterH'
        Node ([ordered]@{id=$r.id;kind='UnregisteredRequirement';title=$r.title;reviewState=$r.reviewState;keywords=@($r.keywords);unresolved=@($r.unresolved);implementationState='NotEvaluated';proof=(Proof $r.proof)})
    }
    $reviewedQuestions=@{}
    foreach($r in $cfg.questionReviews) {
        Need ($nodeMap.ContainsKey($r.questionId) -and $nodeMap[$r.questionId].kind -eq 'Question') "SpatialUnknownQuestion:$($r.questionId)"
        Need (-not $reviewedQuestions.ContainsKey($r.questionId)) "SpatialDuplicateReview:$($r.questionId)"; $reviewedQuestions[$r.questionId]=$true
        Need ($r.disposition -in @('Spatial','NonSpatial','Deferred','MissingSource')) "SpatialDispositionInvalid:$($r.questionId)"
        $proof=QuestionProof $r.questionId $r.proof
        Need ($r.proof.path -ceq $nodeMap[$r.questionId].sourceRef) "SpatialQuestionSourceMismatch:$($r.questionId)"
        $node=$nodeMap[$r.questionId];$node.reviewState='Reviewed';$node.spatialDisposition=$r.disposition
        $node['sourceDecisionState']=$r.sourceDecisionState;$node['reviewProof']=$proof;$node['reviewNote']=$r.note
        if($r.sourceDecisionState -ne $node.decisionState){$issues.Add([ordered]@{subjectId=$r.questionId;code='QuestionStatusNeedsReview';indexState=$node.decisionState;sourceState=$r.sourceDecisionState;proof=$proof})}
    }
    foreach($e in $cfg.links) {
        Need ($e.kind -in @('QuestionRequiresRole','QuestionSupportsH','SupportsRequirementCandidate')) "SpatialInvalidLinkKind:$($e.kind)"
        Need ($nodeMap.ContainsKey($e.from) -and $nodeMap.ContainsKey($e.to)) "SpatialUnknownEndpoint:$($e.from)->$($e.to)"
        if($e.kind -like 'Question*') {
            Need ($nodeMap[$e.from].kind -eq 'Question' -and $nodeMap[$e.from].spatialDisposition -eq 'Spatial') 'SpatialNonSpatialOrUnreviewedLink'
            $targetKind=if($e.kind -eq 'QuestionRequiresRole'){'UnregisteredRequirement'}else{'H1'}
            Need ($nodeMap[$e.to].kind -eq $targetKind) 'SpatialInvalidQuestionTarget'
            Need ($e.proof.path -ceq $nodeMap[$e.from].sourceRef) 'SpatialLinkSourceMismatch'
        }else{Need ($nodeMap[$e.from].kind -eq 'H1' -and $nodeMap[$e.to].kind -eq 'UnregisteredRequirement') 'SpatialInvalidSupportTarget'}
        $linkProof=if($e.kind -like 'Question*'){QuestionProof $e.from $e.proof}else{Proof $e.proof}
        Edge $e.from $e.to $e.kind $linkProof $e.rationale
        if($e.kind -eq 'QuestionSupportsH') {$nodeMap[$e.to].reviewState='PartialQuestionReview'}
    }
    foreach($im in $cfg.images) {
        Need ($im.kind -in @('HistoricalH1Assembly','SharedUIConcept','PrefabPreview','SharedGameView')) 'SpatialInvalidImageKind'
        Need (@($im.hIds).Count -gt 0) 'SpatialImageNoTarget'
        $exists=Test-Path -LiteralPath (Spatial-Full $im.path -Evidence) -PathType Leaf
        if($exists){$s=Observe $im.path -Evidence;Need ($s.sha256 -eq $im.sha256) "SpatialImageDrift:$($im.id)"}
        $manifest=Observe $im.manifestPath -Evidence
        Need ($manifest.sha256 -eq $im.manifestSha256) "SpatialImageManifestDrift:$($im.id)"
        $mp=Get-Content -Raw -Encoding UTF8 -LiteralPath (Spatial-Full $im.manifestPath -Evidence)|ConvertFrom-Json
        if($im.kind -eq 'HistoricalH1Assembly'){Need (@($im.hIds).Count -eq 1 -and $mp.H1StableId -ceq $im.hIds[0]) 'SpatialHistoricalImageTargetMismatch'}
        $entry=ManifestEntry $mp $im.manifestPointer
        if($im.manifestPointer -match '^/Captures/[0-9]+$') {
            $entryName=Field $entry 'FileName' ''; $entryHash=Field $entry 'ImageSha256' ''
        }elseif($im.manifestPointer -match '^/files/[0-9]+$') {
            $entryName=Field $entry 'file' ''; $entryHash=Field $entry 'sha256' ''
        }elseif($mp -is [array] -and $im.manifestPointer -match '^/[0-9]+$') {
            $entryName=Field $entry 'name' ''; $entryHash=Field $entry 'sha256' ''
        }else{throw 'SpatialManifestEntrySchemaUnsupported'}
        Need (-not [string]::IsNullOrWhiteSpace($entryName) -and $entryHash -eq $im.sha256) 'SpatialManifestImageMismatch'
        $entryPath=[IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetDirectoryName((Spatial-Full $im.manifestPath -Evidence))) $entryName))
        Need ($entryPath.Equals((Spatial-Full $im.path -Evidence),[StringComparison]::OrdinalIgnoreCase)) 'SpatialManifestImageMismatch'
        Node ([ordered]@{id=$im.id;kind='VisualEvidence';title=$im.title;evidenceKind=$im.kind;path=$im.path;sha256=$im.sha256;available=$exists;sourceRevision=$im.sourceRevision;currentDefinitionEquivalent=$false;shared=[bool]($im.kind -like 'Shared*' -or @($im.hIds).Count -gt 1);assetRefs=@($im.assetRefs);proof=[ordered]@{path=$im.manifestPath;sha256=$im.manifestSha256;pointer=$im.manifestPointer};limitations=$im.limitations})
        foreach($hid in $im.hIds){Need ($nodeMap.ContainsKey($hid) -and $nodeMap[$hid].kind -eq 'H1') 'SpatialImageUnknownH';Edge $im.id $hid 'VisualEvidenceFor' $nodeMap[$im.id].proof $im.limitations;$nodeMap[$hid].visualState=if($exists){'EvidenceAvailableNotCurrentCompletion'}else{'EvidenceMissing'}}
    }
    $circulation=$null
    $circulationConfig=Field $cfg 'circulation' $null
    if($null -ne $circulationConfig) {
        foreach($ref in $circulationConfig.basisRefs){$null=Observe $ref}
        $guideSeen=@{}
        foreach($guide in @(Field $circulationConfig 'moduleGuide' @())){Need ($guide.facet -in @('time','space','player','target') -and -not $guideSeen.ContainsKey($guide.facet)) 'CirculationModuleGuideInvalid';$guideSeen[$guide.facet]=$true;$null=Proof $guide.proof;Need ($guide.implementationState -eq 'DocumentationReferenceOnly') 'CirculationModuleIsNotRuntimeEvidence'}
        Need ($circulationConfig.documentRoot -ceq 'docs/Architecture/PlayableLoops') 'CirculationDocumentRootNotApproved'
        $documents=[Collections.Generic.List[object]]::new()
        $registeredPlanning=[Collections.Generic.List[object]]::new()
        $loopCatalog=Json 'eng/execution-ledgers/playable-loops.json'
        foreach($loop in $loopCatalog.items|Where-Object loopLevelCode -eq 'PlayableUnit') {
            $gate=$loop.planningGate;$ref=[string]$gate.designDocumentRef
            $actual=if($ref){(Observe $ref).sha256}else{$null}
            $registeredPlanning.Add([ordered]@{loopId=$loop.loopStableId;topicId=$gate.topicStableId;planningStatus=$gate.statusCode;path=$ref;expectedSha256=$gate.designHashSha256;observedSha256=$actual;rawHashMatches=$(if($ref){$actual -eq $gate.designHashSha256}else{$null});reviewState='InventoryOnly';proof=[ordered]@{path='eng/execution-ledgers/playable-loops.json';sha256=$observed['eng/execution-ledgers/playable-loops.json'].sha256;pointer='/items'}})
        }
        # A bounded directory inventory is a coverage list, not semantic approval.
        $docRefs=@(Get-ChildItem -LiteralPath (Full $circulationConfig.documentRoot) -Filter '*.md' -File -Recurse|ForEach-Object {$_.FullName.Substring($root.Length+1).Replace('\','/')})
        foreach($ref in @(Ordinal (@($docRefs)+@($registeredPlanning|Where-Object path|ForEach-Object path)))) {
            $s=Observe $ref;$lines=@(Get-Content -LiteralPath (Full $ref) -Encoding UTF8)
            $title=@($lines|Where-Object {$_ -match '^# '}|Select-Object -First 1)
            $qids=@($Database.questions|Where-Object sourceRef -ceq $ref|ForEach-Object questionId)
            $documents.Add([ordered]@{path=$ref;sha256=$s.sha256;title=$(if($title.Count){$title[0].Substring(2)}else{$ref});questionIds=$qids;indexedQuestionSource=($qids.Count -gt 0);reviewState='InventoryOnly';role='RelatedPlanningDocumentNotAutomaticallyGameRule'})
        }
        $reviewMap=@{}
        $facetKeys=@('time','space','player','target','choice','result','nextChoices')
        foreach($r in $circulationConfig.reviews) {
            Need ($nodeMap.ContainsKey($r.questionId) -and $nodeMap[$r.questionId].kind -eq 'Question') 'CirculationUnknownQuestion'
            Need (-not $reviewMap.ContainsKey($r.questionId)) 'CirculationDuplicateReview'
            $p=QuestionProof $r.questionId $r.proof
            # 주제 문서와 등록된 원답변 아카이브를 구별한다. 같은 질문의 직접 발췌만 허용한다.
            $question=@($Database.questions|Where-Object questionId -CEQ $r.questionId)[0]
            Need ($p.path -ceq $question.sourceRef -or @($question.directExcerptRefs) -ccontains "$($p.path):$($p.line)") 'CirculationQuestionSourceMismatch'
            $lineCount=@(Get-Content -LiteralPath (Full $p.path) -Encoding UTF8).Count
            Need ($p.endLine -ge $p.line -and $p.endLine -le $lineCount) 'CirculationInvalidEndLine'
            $keys=@($r.facets.PSObject.Properties.Name)
            Need ($keys.Count -eq 7 -and @($facetKeys|Where-Object {$_ -cnotin $keys}).Count -eq 0) 'CirculationFacetsIncomplete'
            foreach($key in $facetKeys) {
                $f=$r.facets.$key
                Need ($f.state -in @('Explicit','Undetermined','NotApplicable','EvidenceMissing','InterpretationProposal') -and -not [string]::IsNullOrWhiteSpace($f.text)) "CirculationFacetInvalid:$key"
            }
            Need (-not [string]::IsNullOrWhiteSpace($r.sourceDecisionState)) 'CirculationDecisionStateMissing'
            $questionNode=$nodeMap[$r.questionId]
            $missingSource=(Field $questionNode 'spatialDisposition' '') -eq 'MissingSource' -or (Field $questionNode 'decisionState' '') -eq 'NeedsSourceRecovery' -or (Field $questionNode 'sourceDecisionState' '') -eq 'NeedsSourceRecovery'
            if($missingSource) {
                Need ($r.sourceDecisionState -eq 'NeedsSourceRecovery' -and @($facetKeys|Where-Object {$r.facets.$_.state -ne 'EvidenceMissing'}).Count -eq 0) 'CirculationMissingSourceCannotBecomeExplicit'
            }
            $reviewMap[$r.questionId]=$r
        }
        $items=@(foreach($q in $Database.questions){
            $r=if($reviewMap.ContainsKey($q.questionId)){$reviewMap[$q.questionId]}else{$null}
            $facets=[ordered]@{}
            foreach($key in $facetKeys){$facets[$key]=if($null -ne $r){$r.facets.$key}else{[ordered]@{state='Unreviewed';text='원문 관점 대조 전. 미정·해당 없음으로 추정하지 않음.'}}}
            $topicContext=@($scope.topics|Where-Object topicCode -ceq $q.topicCode|ForEach-Object {[ordered]@{topicCode=$_.topicCode;worldInteractionRefs=$_.worldInteractionRefs;playableLoopRefs=$_.playableLoopRefs;basis='TopicContextNotExactQuestionImplementation';sourceRef=$config.scopeRef}})
            [ordered]@{questionId=$q.questionId;topicCode=$q.topicCode;kind=$q.kind;sourceRef=$q.sourceRef;sourceSha256=(Observe $q.sourceRef).sha256;directExcerptRefs=$q.directExcerptRefs;indexDecisionState=$q.recordStatus;sourceDecisionState=$(if($null -ne $r){$r.sourceDecisionState}else{'NotReviewed'});reviewState=$(if($null -ne $r){'SourceCompared'}else{'Unreviewed'});proof=$(if($null -ne $r){$r.proof}else{$null});facets=$facets;topicImplementationContext=$topicContext;implementationLookupRef=$q.implementationLookupRef;implementationVerified=$false}
        })
        $circulation=[ordered]@{schemaVersion='planning-inquiry-circulation-view.v1';scope='CurrentIndexedQuestionsAndBoundedPlayableLoopsDocumentInventory';authority='SourcePreservedNotNewRuleOrExecutionApproval';items=$items;documents=@($documents);registeredPlanning=@($registeredPlanning);moduleGuide=Field $circulationConfig 'moduleGuide' @();counts=[ordered]@{total=$items.Count;sourceCompared=$reviewMap.Count;unreviewed=($items.Count-$reviewMap.Count);sourceProblemItems=@($items|Where-Object {@($_.facets.Values|Where-Object state -eq EvidenceMissing).Count -gt 0}).Count;documents=$documents.Count;outsideQuestionIndex=@($documents|Where-Object {-not $_.indexedQuestionSource}).Count};completion='PartialNotAllMeaningReviewed'}
    }
    $wiView=$null
    $wiConfig=Field $cfg 'wiView' $null
    if($null -ne $wiConfig) {
        Need ($wiConfig.catalogRef -ceq 'eng/execution-ledgers/world-interactions.json' -and $wiConfig.loopsRef -ceq 'eng/execution-ledgers/playable-loops.json') 'WiViewCatalogBoundary'
        $null=Observe $wiConfig.basisRef
        $wiCatalog=Json $wiConfig.catalogRef;$loopCatalog=Json $wiConfig.loopsRef
        $wiSeen=@{};$orders=@{};$wiItems=[Collections.Generic.List[object]]::new()
        foreach($wi in $wiCatalog.items) {
            Need (-not [string]::IsNullOrWhiteSpace($wi.id) -and -not $wiSeen.ContainsKey($wi.id)) 'WiViewDuplicateOrEmptyId'
            $wiSeen[$wi.id]=$true
            $contexts=@(foreach($loop in $loopCatalog.items|Where-Object {@($_.worldInteractionIds) -ccontains $wi.id}) {
                $refs=@(@((Field $loop 'workOrderRef' '')) + @(Field $loop 'workOrderRefs' @())|Where-Object {$_}|Sort-Object -Unique)
                foreach($ref in $refs) {
                    if(-not $orders.ContainsKey($ref)) {
                        if(Test-Path -LiteralPath (Full $ref) -PathType Leaf) {
                            $order=Json $ref
                            $orders[$ref]=[ordered]@{path=$ref;sha256=(Observe $ref).sha256;state='SourceAvailableNotValidated';schemaVersion=Field $order 'schemaVersion' '';protocolRevision=Field $order 'protocolRevision' '';presentationE4Preparation=Field $order 'presentationE4Preparation' $null}
                            $orders[$ref].scopedPreparationResults=@(foreach($key in @('d396VisualCandidatePreparation','d396StateBindingPreparation')){
                                $preparation=Field $order $key $null
                                if($null -ne $preparation){[ordered]@{section=$key;data=$preparation;meaning='ReportedScopedResultNotWholeWiAchievement'}}
                            })
                        } else {$orders[$ref]=[ordered]@{path=$ref;sha256=$null;state='SourceMissing';presentationE4Preparation=$null}}
                    }
                }
                [ordered]@{loopId=$loop.loopStableId;level=$loop.loopLevelCode;status=$loop.statusCode;planningGate=Field $loop 'planningGate' $null;maturityTracks=Field $loop 'maturityTracks' $null;integratedStage=$loop.currentEvidenceStage;workOrderRefs=$refs;requiredStudies=Field $loop 'requiredStudies' @();blockers=Field $loop 'blockers' @();nextAction=Field $loop 'nextAction' '';scope='LoopContextNotIndividualWiAchievement'}
            })
            $topicCodes=@($scope.topics|Where-Object {@($_.worldInteractionRefs) -ccontains $wi.id}|ForEach-Object topicCode)
            $questionRefs=@($Database.questions|Where-Object {$topicCodes -ccontains $_.topicCode}|ForEach-Object questionId)
            $hRefs=@($nodes|Where-Object {$_.kind -eq 'H1' -and @(Field $_ 'wiIds' @()) -ccontains $wi.id}|ForEach-Object id)
            $referenceStates=@(foreach($ref in @($wi.sourceReferences)+@($wi.existingImplementationReferences)|Sort-Object -Unique) {
                if([IO.Path]::IsPathRooted($ref)){[ordered]@{path=$ref;state='LocalReferenceNotInspected';sha256=$null}}
                elseif(Test-Path -LiteralPath (Full $ref) -PathType Leaf){[ordered]@{path=$ref;state='FilePresentNotImplementationVerified';sha256=(Observe $ref).sha256}}
                else{[ordered]@{path=$ref;state='ReferencedFileMissingNotImplementationAbsent';sha256=$null}}
            })
            $wiItems.Add([ordered]@{
                id=$wi.id;title=$wi.title;kind=$wi.kind;catalogRef=$wiConfig.catalogRef;catalogRevision=$wiCatalog.revision;ruleRevision=$wi.ruleRevision
                readingState='ExistingCatalogProjectionNotFullMeaningReview'
                facets=[ordered]@{
                    time=[ordered]@{taskRule=$wi.taskRule;cancellationPolicy=$wi.cancellationPolicy;authorityReview='NotReviewed'}
                    space=[ordered]@{requirements=$wi.spatialRequirements;hRefs=$hRefs;placementVerified=$false}
                    player=[ordered]@{actorRequirements=$wi.actorRequirements;controlPolicy=$wi.controlPolicyCode}
                    target=[ordered]@{startStateCodes=$wi.startStateCodes;resourceRequirements=$wi.resourceRequirements;identityReview='NotReviewed'}
                    choice=[ordered]@{worldAction=$wi.worldAction;previewRule=$wi.previewRule;confirmRule=$wi.confirmRule;blockReasonCodes=$wi.blockReasonCodes}
                    result=[ordered]@{completionStateCodes=$wi.completionStateCodes;effectCodes=$wi.effectCodes}
                    nextChoices=[ordered]@{successorWiIds=$wi.successorWiIds;cancellationPolicy=$wi.cancellationPolicy;meaning='RecordedRelationsNotMandatoryRoute'}
                }
                sourceReferences=$wi.sourceReferences;existingImplementationReferences=$wi.existingImplementationReferences;codeAndTestVerification='NotRunByThisView'
                referenceStates=$referenceStates
                recordedImplementation=$wi.implementation;recordedIntegration=$wi.integration;loopContexts=$contexts
                topicQuestionRefs=$questionRefs;questionLinkBasis='TopicContextNotExactQuestionImplementation'
                preparationAssessment='NotAssessedUseWorkOrderAndApprovedStudy';executionAuthorized=$false
            })
        }
        $wiView=[ordered]@{catalogRef=$wiConfig.catalogRef;catalogRevision=$wiCatalog.revision;catalogSha256=(Observe $wiConfig.catalogRef).sha256;total=$wiItems.Count;items=@($wiItems);workOrders=@($orders.Keys|Sort-Object|ForEach-Object {$orders[$_]});completion='InventoryProjectionNotAllE4Prepared'}
    }
    return [ordered]@{schemaVersion='planning-inquiry-spatial-view.v1';authority='DerivedRelationsOnlyNotRegistrationOrE';nodes=@($nodes);edges=@($edges);issues=@($issues);sources=@($sources);circulation=$circulation;wiView=$wiView}
}

function Search-InquiryWi($Spatial,[string]$Id,[string]$Text,[int]$Limit) {
    Need ($null -ne $Spatial.wiView) 'WiViewNotConfigured'
    $terms=@($Text.Split(' ',[StringSplitOptions]::RemoveEmptyEntries))
    $matches=@($Spatial.wiView.items|Where-Object {$item=$_;$ok=(-not $Id -or $item.id -ceq $Id);$hay=$item|ConvertTo-Json -Depth 25 -Compress;foreach($term in $terms){if($hay.IndexOf($term,[StringComparison]::OrdinalIgnoreCase) -lt 0){$ok=$false}};$ok})
    $results=@(foreach($item in $matches|Select-Object -First $Limit){
        $refs=@($item.loopContexts|ForEach-Object workOrderRefs)
        [ordered]@{item=$item;workOrders=@($Spatial.wiView.workOrders|Where-Object {$refs -ccontains $_.path});reviewedQuestionContext=@((Field (Field $Spatial 'circulation' $null) 'items' @())|Where-Object {$item.topicQuestionRefs -ccontains $_.questionId -and $_.reviewState -eq 'SourceCompared'})}
    })
    return [ordered]@{catalogRevision=$Spatial.wiView.catalogRevision;catalogTotal=$Spatial.wiView.total;totalMatches=$matches.Count;returned=$results.Count;results=$results;completion=$Spatial.wiView.completion}
}

function Get-InquiryWiMarkdown($Spatial) {
    Need ($null -ne $Spatial.wiView) 'WiViewNotConfigured'
    function WiCell($Value){return (($Value|ConvertTo-Json -Depth 16 -Compress).Replace('|','\|').Replace("`r",'').Replace("`n",' '))}
    $v=$Spatial.wiView;$lines=[Collections.Generic.List[string]]::new()
    $lines.Add('# WI 전체 — 지금·여기·나·너·이렇게와 E4 준비 조회');$lines.Add('')
    $lines.Add("공식 대장 $($v.catalogRevision), $($v.total)개. 기존 필드를 읽기 단위로 투영했다. 전체 의미 검토/코드·시험·Prefab 준비 완료가 아니며 모든 행의 실행 권한을 false로 유지한다. 높은 기존 E를 낮추거나 원장 단계와 Loop 두 궤적을 합성하지 않는다.")
    $lines.Add('');$lines.Add('| WI | 이름 | 원장 구현 / 통합(과거 분류) | 직접 Loop 문맥 수 | 코드/시험 참조 수 |');$lines.Add('| --- | --- | --- | --- | --- |')
    foreach($item in $v.items){$lines.Add("| $($item.id) | $($item.title) | $($item.recordedImplementation.currentStage) / $($item.recordedIntegration.currentStage) | $($item.loopContexts.Count) | $(@($item.existingImplementationReferences).Count) |")}
    foreach($item in $v.items) {
        $lines.Add('');$lines.Add("## $($item.id) — $($item.title)");$lines.Add('')
        $lines.Add("원문: [공식 WI 대장](../../../$($item.catalogRef)) / $($item.ruleRevision). ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.")
        $lines.Add('');$lines.Add('| 읽기 항목 | 기존 기록 |');$lines.Add('| --- | --- |')
        $labels=@{time='지금';space='여기';player='나';target='너';choice='이렇게';result='결과';nextChoices='다음 선택'}
        foreach($key in $item.facets.Keys){$lines.Add("| $($labels[$key]) | $(WiCell $item.facets[$key]) |")}
        $lines.Add('');$lines.Add('원문/코드·시험 참조(현재 실행 검증 아님):')
        foreach($ref in $item.referenceStates) {
            if($ref.state -eq 'FilePresentNotImplementationVerified'){$lines.Add("- [원문/소스](../../../$($ref.path)) / 파일 존재·hash 확인, 구현/시험 검증 아님")}
            elseif($ref.state -eq 'LocalReferenceNotInspected'){$lines.Add("- 로컬 참조: ``$($ref.path)`` (이 조회에서 미검사)")}
            else{$lines.Add("- 참조 파일 없음: ``$($ref.path)``. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.")}
        }
        $lines.Add('');$lines.Add("문답 주제 문맥: $($item.topicQuestionRefs -join ', '). 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.")
        foreach($context in $item.loopContexts){
            $lines.Add('');$lines.Add("- $($context.loopId) / $($context.level) / 통합 $($context.integratedStage), 궤적 $(WiCell $context.maturityTracks). Loop 문맥이지 개별 WI 달성 판정이 아니다.")
            foreach($ref in $context.workOrderRefs){
                $o=$v.workOrders|Where-Object path -ceq $ref;$e4=$o.presentationE4Preparation
                $lines.Add("  - [기존 명세](../../../$ref) / $($o.state), E4 후보 항목 존재=$($null -ne $e4), 명세 hash=$($o.sha256). 후보 상세는 -Wi -Id $($item.id) 조회. 존재만으로 적합성 통과 아님.")
                if($null -ne $e4){
                    $lines.Add("  - 명세 문맥의 판독 순간: $(Field $e4 'playerReadableMoment' '미기재'); VisualKey: $(@(Field $e4 'visualKeys' @()) -join ', '). 개별 WI 적용 범위는 원명세를 다시 확인한다.")
                    $lines.Add("  - 주 후보: $(@(Field $e4 'primaryAssetCandidateRefs' @()) -join ', '); 대체: $(@(Field $e4 'alternativeAssetCandidateRefs' @()) -join ', '); fallback: $(@(Field $e4 'fallbackPresentationRefs' @()) -join ', ').")
                    $lines.Add("  - 배치/Anchor: $(Field $e4 'placementIntent' '미기재') / $(Field $e4 'interactionAnchorIntent' '미기재'). 준비 상태: $(Field $e4 'e5ReadinessCode' '미기재').")
                    $lines.Add("  - 열린 준비: $(@(Field $e4 'openGapRefs' @()) -join '; ').")
                }
                foreach($scoped in @(Field $o 'scopedPreparationResults' @())){
                    $lines.Add("  - 제한 준비 결과 $($scoped.section): $($scoped.data.currentResult). [기술보고](../../../$($scoped.data.reportRef)); 명세의 writePaths/validation 참조. 이 결과를 개별 WI 전체 또는 E 달성으로 합성하지 않는다.")
                }
            }
            $lines.Add("  - 기존 차단: $($context.blockers -join '; '). 기존 다음 작업은 자동 실행 지시가 아니다: $($context.nextAction)")
        }
        $lines.Add('');$lines.Add('이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.')
    }
    return ($lines -join "`n")+"`n"
}

function Search-InquiryCirculation($Spatial,[string]$Id,[string]$Topic,[string]$Text,[switch]$Unreviewed,[int]$Limit) {
    Need ($null -ne (Field $Spatial 'circulation' $null)) 'CirculationNotConfigured'
    $terms=@($Text.Split(' ',[StringSplitOptions]::RemoveEmptyEntries))
    $items=@($Spatial.circulation.items|Where-Object {
        $item=$_;$ok=(-not $Id -or $item.questionId -ceq $Id) -and (-not $Topic -or $item.topicCode -like "*$Topic*") -and (-not $Unreviewed -or $item.reviewState -eq 'Unreviewed')
        $hay=$item|ConvertTo-Json -Depth 15 -Compress
        foreach($term in $terms){if($hay.IndexOf($term,[StringComparison]::OrdinalIgnoreCase) -lt 0){$ok=$false}}
        $ok
    })
    $results=@(foreach($item in $items|Select-Object -First $Limit){
        [ordered]@{item=$item;spatialRelations=@($Spatial.edges|Where-Object from -ceq $item.questionId);spatialQueryId=$item.questionId}
    })
    return [ordered]@{totalMatches=$items.Count;returned=$results.Count;results=$results;coverage=$Spatial.circulation.counts;moduleGuide=$Spatial.circulation.moduleGuide;completion=$Spatial.circulation.completion}
}

function Get-InquiryCirculationMarkdown($Spatial) {
    Need ($null -ne (Field $Spatial 'circulation' $null)) 'CirculationNotConfigured'
    $c=$Spatial.circulation;$lines=[Collections.Generic.List[string]]::new()
    function Cell($Value){return ([string]$Value).Replace('|','\|').Replace("`r",'').Replace("`n",' ')}
    $lines.Add('# 전체 기획 네 관점·WI 순환 조회');$lines.Add('')
    $lines.Add("현재 문답 $($c.counts.total), 원문 관점 대조 $($c.counts.sourceCompared), 미검토 $($c.counts.unreviewed), 근거 소실/문제 $($c.counts.sourceProblemItems). 문답 대조와 모든 기획 문서의 의미 확정·게임 구현 완료는 별개다. 7칸은 네 조건과 선택·결과·다음 선택이며 의무 행동 순서가 아니다.")
    $lines.Add('D394 읽기 제목은 지금·여기·나·너·이렇게·결과·다음 선택이다. D395의 코드/시험·자산 후보·Unity·결과 왕복은 기존 명세와 증거로 확인해야 한다. 아래 주제 WI 참조나 과거 이미지만으로 같은 질문/판본의 실제 연결을 확인했다고 하지 않는다.')
    $lines.Add('');$lines.Add('## 범위와 색인 밖 자료');$lines.Add('')
    $lines.Add("docs/Architecture/PlayableLoops 아래 Markdown $($c.counts.documents)개를 열거했다. 질문 입력 밖 $($c.counts.outsideQuestionIndex)개는 관련 기획·연구·목록이 섞인 InventoryOnly이며 의미 검토 완료가 아니다. 이 폴더 밖 모든 기획 문서의 전수 조사로 확대하지 않는다.")
    $lines.Add('');$lines.Add('| 원문 | 질문 수 | 관점 대조 수 | 목록 상태 |');$lines.Add('| --- | --- | --- | --- |')
    foreach($d in $c.documents){$reviewed=@($c.items|Where-Object {$_.sourceRef -ceq $d.path -and $_.reviewState -eq 'SourceCompared'}).Count;$lines.Add("| [$(Cell $d.title)](../../../$($d.path)) | $($d.questionIds.Count) | $reviewed | $($d.reviewState) |")}
    $lines.Add('');$lines.Add('### 기존 PlayableUnit 기획 관문 목록');$lines.Add('')
    $lines.Add('| 기존 Loop | 기획 상태 | 원문 | 원바이트 hash 대조 |');$lines.Add('| --- | --- | --- | --- |')
    foreach($p in $c.registeredPlanning){$ref=if($p.path){"[원문](../../../$($p.path))"}else{'미작성/미연결'};$lines.Add("| $($p.loopId) | $($p.planningStatus) | $ref | $($p.rawHashMatches) |")}
    $lines.Add('');$lines.Add('### 네 관점의 기존 시스템 탐색');$lines.Add('')
    foreach($g in $c.moduleGuide){$lines.Add("- $($g.facet): $(Cell $g.text) — DocumentationReferenceOnly, 실제 제품 소비 미검증.")}
    # 생성 중 항목은 OrderedDictionary다. 속성명 Group-Object는 키를 읽지 못해 빈 제목으로 합쳤다.
    foreach($topic in @($c.items|Group-Object { $_.topicCode }|Sort-Object Name)) {
        Need (-not [string]::IsNullOrWhiteSpace($topic.Name)) 'CirculationTopicMissing'
        $lines.Add('');$lines.Add("## $($topic.Name)");$lines.Add('')
        $lines.Add('| 질문 / 원문 위치 | 검토 / 원문 상태 | 지금(시간) | 여기(공간) | 나(플레이어) | 너(대상) | 이렇게(선택·WI) | 결과 | 다음 선택·복귀 |')
        $lines.Add('| --- | --- | --- | --- | --- | --- | --- | --- | --- |')
        foreach($item in $topic.Group){
            $location=if($null -ne $item.proof){"L$($item.proof.line)–$($item.proof.endLine)"}else{'원문 대조 전'}
            $cells=@(foreach($key in @('time','space','player','target','choice','result','nextChoices')){Cell "$($item.facets[$key].state): $($item.facets[$key].text)"})
            $proofRef=if($null -ne $item.proof){$item.proof.path}else{$item.sourceRef}
            $lines.Add("| [$($item.questionId)](../../../$proofRef) $location | $($item.reviewState) / $($item.sourceDecisionState) | $($cells -join ' | ') |")
        }
    }
    $lines.Add('');$lines.Add('## 다음 문답·검토 공백');$lines.Add('')
    $lines.Add('Undetermined는 원문의 미정, EvidenceMissing은 근거 소실/미확인, Unreviewed는 아직 읽기 대조하지 않은 항목이다. 미검토를 새 질문으로 자동 발급하지 않는다. 구현·배치 결손은 기획 선택과 별개다.');$lines.Add('')
    foreach($item in $c.items|Where-Object reviewState -eq SourceCompared){foreach($key in @('time','space','player','target','choice','result','nextChoices')){if($item.facets[$key].state -in @('Undetermined','EvidenceMissing','InterpretationProposal') -or $item.facets[$key].text -match '미정|미제시|미기재'){$lines.Add("- $($item.questionId) / $key / $($item.facets[$key].state): $(Cell $item.facets[$key].text)")}}}
    return ($lines -join "`n")+"`n"
}

function Search-InquirySpatial($Spatial,[string]$Id,[string]$Text,[string]$Gap,[int]$Limit) {
    $nodeMap=@{};foreach($n in $Spatial.nodes){$nodeMap[$n.id]=$n}
    $terms=@($Text.Split(' ',[StringSplitOptions]::RemoveEmptyEntries))
    $selected=@($Spatial.nodes|Where-Object {
        $n=$_; $ok=(-not $Id -or $n.id -ceq $Id)
        $hay=$n|ConvertTo-Json -Depth 20 -Compress
        foreach($term in $terms){if($hay.IndexOf($term,[StringComparison]::OrdinalIgnoreCase) -lt 0){$ok=$false}}
        if($Gap -eq 'UnmappedRequirements'){$ok=$ok -and $n.kind -eq 'UnregisteredRequirement'}
        if($Gap -eq 'UnreviewedH'){$ok=$ok -and $n.kind -match '^H[1-4]$' -and $n.reviewState -eq 'Unreviewed'}
        if($Gap -eq 'NoImage'){$ok=$ok -and $n.kind -eq 'H1' -and (Field $n 'visualState' '') -in @('NoEvidence','EvidenceMissing')}
        if($Gap -eq 'ReviewRequired'){$ok=$ok -and @($Spatial.issues|Where-Object subjectId -ceq $n.id).Count -gt 0}
        $ok
    })
    $results=@(foreach($n in $selected|Select-Object -First $Limit){
        $direct=@($Spatial.edges|Where-Object {$_.from -ceq $n.id -or $_.to -ceq $n.id})
        $relatedIds=@(Ordinal @($direct|ForEach-Object {if($_.from -ceq $n.id){$_.to}else{$_.from}}))
        # Traverse only declared containment, never generic graph reachability.
        $paths=[Collections.Generic.List[object]]::new();$queue=[Collections.Generic.Queue[object]]::new()
        $requirements=@($direct|Where-Object {$_.kind -eq 'QuestionRequiresRole' -and $_.from -ceq $n.id}|ForEach-Object {$_.to})
        $supportCandidates=@($Spatial.edges|Where-Object {$_.kind -eq 'SupportsRequirementCandidate' -and $_.to -in $requirements}|ForEach-Object {$_.from})
        $seeds=@(Ordinal (@($n.id)+@($supportCandidates)+@($direct|Where-Object {$_.kind -eq 'QuestionSupportsH' -and $_.from -ceq $n.id}|ForEach-Object {$_.to})))
        foreach($seed in $seeds){$queue.Enqueue([ordered]@{at=$seed;ids=@($seed);edgeKinds=@()})}
        while($queue.Count){$step=$queue.Dequeue();foreach($e in $Spatial.edges|Where-Object {$_.to -ceq $step.at -and $_.kind -in @('ContainsRequired','ContainsOptional','PatternSelectsH3')}){
            Need ($e.from -notin $step.ids) 'SpatialContainmentCycle'
            $p=[ordered]@{at=$e.from;ids=@($step.ids)+@($e.from);edgeKinds=@($step.edgeKinds)+@($e.kind)}
            $paths.Add([ordered]@{targetId=$e.from;via=$p.ids;edgeKinds=$p.edgeKinds;basisKind=$(if($p.ids[0] -in $supportCandidates){'SupportsRequirementCandidate'}elseif($n.kind -eq 'Question'){'QuestionSupportsH'}else{'DeclaredContainment'});derived=($p.ids.Count -gt 2 -or $n.kind -eq 'Question');runtimePlacementConfirmed=$false})
            Need ($paths.Count -le 4096) 'SpatialTraversalLimit';$queue.Enqueue($p)
        }}
        $desc=[Collections.Generic.List[string]]::new();$q=[Collections.Generic.Queue[string]]::new();$q.Enqueue($n.id);$seen=@{}
        while($q.Count){$at=$q.Dequeue();if($seen.ContainsKey($at)){continue};$seen[$at]=$true;foreach($e in $Spatial.edges|Where-Object {$_.from -ceq $at -and $_.kind -in @('ContainsRequired','ContainsOptional','PatternSelectsH3')}){$desc.Add($e.to);$q.Enqueue($e.to)}}
        $descIds=@(Ordinal @($desc));$inherited=@($Spatial.edges|Where-Object {$_.kind -eq 'QuestionSupportsH' -and $_.to -in $descIds})
        $contextIds=@(Ordinal (@($relatedIds)+@($descIds)+@($seeds)))
        $context=@($Spatial.edges|Where-Object {($_.from -in $contextIds -or $_.to -in $contextIds) -and $_.kind -in @('ExpressionSupports','VisualEvidenceFor','QuestionSupportsH','SupportsRequirementCandidate')})
        [ordered]@{node=$n;directRelations=$direct;relatedNodes=@($relatedIds|ForEach-Object{$nodeMap[$_]});ancestorPaths=@($paths);descendantIds=$descIds;inheritedQuestionLinks=$inherited;contextRelations=$context;contextNodes=@(Ordinal @($context|ForEach-Object {$_.from;$_.to})|ForEach-Object {$nodeMap[$_]});issues=@($Spatial.issues|Where-Object subjectId -ceq $n.id)}
    })
    return [ordered]@{totalMatches=$selected.Count;returned=$results.Count;results=$results;authority=$Spatial.authority;allReviewIssues=$Spatial.issues}
}

function Get-InquirySpatialMarkdown($Spatial) {
    $lines=[Collections.Generic.List[string]]::new()
    $lines.Add('# 문답·공간 생성 연결표');$lines.Add('');$lines.Add('생성 조회 자료. 관계 원본은 eng/planning-inquiries/spatial-relations.json과 각 H 정의다. 승인·E·실제 배치를 뜻하지 않는다. 직접 질문 연결과 상위 포함 파생을 구별한다.');$lines.Add('')
    $lines.Add('| 종류 | 이름 / ID | 문답 직접 연결 | 시각 | 미검토·공백 |');$lines.Add('| --- | --- | --- | --- | --- |')
    foreach($n in $Spatial.nodes|Where-Object {$_.kind -ne 'Question' -and $_.kind -ne 'VisualEvidence'}){
        $qs=@($Spatial.edges|Where-Object {$_.to -ceq $n.id -and $_.kind -like 'Question*'}|ForEach-Object {$_.from}) -join ', '
        $proof=Field $n 'proof' $null;$name=$n.title.Replace('|','\|')
        $ref=if($null -ne $proof){"[$name](../../../$($proof.path))"}else{$name}
        $lines.Add("| $($n.kind) | $ref / $($n.id) | $qs | $(Field $n 'visualState' '해당없음/별도조회') | $($n.reviewState); $((@(Field $n 'unresolved' @()) -join '; ').Replace('|','\|')) |")
    }
    $lines.Add('');$lines.Add('## 검토 필요');$lines.Add('')
    foreach($issue in $Spatial.issues){$lines.Add("- $($issue.subjectId): $($issue.code) — 원문 자동수리 없음.")}
    return ($lines -join "`n")+"`n"
}
