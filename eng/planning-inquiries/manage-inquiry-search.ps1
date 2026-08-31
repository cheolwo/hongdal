[CmdletBinding()]
param(
    [ValidateSet('Write','Validate','Search')] [string] $Mode = 'Search',
    [string] $SourcesPath = 'eng/planning-inquiries/sources.json',
    [string] $IndexPath = 'docs/AI/generated/planning-inquiry-search.json',
    [string] $Id = '', [string] $Text = '', [string] $Topic = '', [string] $Depth = '',
    [switch] $OpenOnly,
    [switch] $Spatial, [string] $HId = '',
    [ValidateSet('','UnmappedRequirements','UnreviewedH','NoImage','ReviewRequired')] [string] $Gap = '',
    [string] $SpatialMarkdownPath = '',
    [switch] $Circulation, [switch] $Unreviewed,
    [string] $CirculationMarkdownPath = '',
    [switch] $Wi, [string] $WiMarkdownPath = '',
    [ValidateRange(1,1000)] [int] $Limit = 8
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $root 'eng/common/deterministic-text-output.ps1')
function Need([bool] $Ok, [string] $Message) { if (-not $Ok) { throw "InquirySearch:$Message" } }
function Full([string] $Ref) {
    $path = [IO.Path]::GetFullPath((Join-Path $root $Ref))
    Need ($path.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) "OutsideRepository:$Ref"
    return $path
}
function Read-Json([string] $Ref) { Get-Content -LiteralPath (Full $Ref) -Raw -Encoding UTF8 | ConvertFrom-Json }
function Field($Object, [string] $Name, $Default) {
    if ($Object -is [Collections.IDictionary] -and $Object.Contains($Name)) { return $Object[$Name] }
    if ($null -ne $Object -and $null -ne $Object.PSObject.Properties[$Name]) { return $Object.$Name }
    return $Default
}
function Ordinal([string[]] $Values) {
    [Array]::Sort($Values, [StringComparer]::Ordinal)
    $previous=$null
    foreach ($value in $Values) { if ($value -cne $previous) {$value}; $previous=$value }
}
function Numbers([string] $Selector) {
    foreach ($part in $Selector.Split(',')) {
        Need ($part -match '^(\d+)(?:-(\d+))?$') "InvalidSelector:$part"
        $first = [int] $Matches[1]; $last = $first
        if ($Matches.ContainsKey(2) -and $Matches[2]) { $last = [int] $Matches[2] }
        Need ($first -le $last) "ReversedSelector:$part"
        $first..$last
    }
}
$config = Read-Json $SourcesPath
Need ($config.schemaVersion -eq 'planning-inquiry-sources.v1') 'UnknownSchema'
$scope = Read-Json $config.scopeRef
$sourceMap = @{}
foreach ($topicItem in $scope.topics) {
    if (-not $sourceMap.ContainsKey($topicItem.sourceRef)) {
        $sourceMap[$topicItem.sourceRef] = [ordered]@{path=$topicItem.sourceRef; role='TopicSource'; topics=@()}
    }
    $sourceMap[$topicItem.sourceRef].topics += $topicItem.topicCode
}
foreach ($extra in $config.extraSources) {
    Need (-not $sourceMap.ContainsKey($extra.path)) "DuplicateSource:$($extra.path)"
    $sourceMap[$extra.path] = [ordered]@{path=$extra.path; role=$extra.role; topics=@($extra.topicCode)}
}
$sections = [Collections.Generic.List[object]]::new()
function Inline-Directions([string] $Body) {
    # 같은 절의 주 질문 상태를 물려받지 않고 해당 행의 명시 상태만 읽는다.
    [regex]::Matches($Body, '(?m)^- (?:조사·정리 방향|엔진 협력) 식별:[ \t]*`([a-z][a-z0-9-]+)`[ \t]*[—-][ \t]*`(Asked|Confirmed|ConfirmedDirection|Incorporated|Deferred|Open)`(?=[.\s]|$)')
}
function Add-Section([string] $Ref, [int] $Line, [string] $Heading, [string] $Body, [string] $DirectId) {
    if ([string]::IsNullOrWhiteSpace($Body)) { return }
    $semantic = [regex]::Match($Body, '(?:질문 식별:\s*|(?m:^- 의미 식별자:[ \t]*))`([a-z][a-z0-9-]+)`')
    if ($semantic.Success) { $DirectId = $semantic.Groups[1].Value }
    $depthMatch = [regex]::Match($Body, '(?:/\s*|\b)(D[1-5])(?:\s|[-/])')
    $sections.Add([ordered]@{
        sectionId="$Ref`:$Line"; sourceRef=$Ref; line=$Line; heading=$Heading
        sourceRole=$sourceMap[$Ref].role; topicCodes=@($sourceMap[$Ref].topics)
        directQuestionId=$DirectId
        directQuestionIds=@($DirectId.Split(',', [StringSplitOptions]::RemoveEmptyEntries)) + @(Inline-Directions $Body | ForEach-Object { $_.Groups[1].Value })
        depthCode=$(if ($depthMatch.Success) {$depthMatch.Groups[1].Value} else {'Unclassified'})
        containsOpenMarker=[bool]($Body -match '미정|미답변|답변 없음|답변 대기|질문 후보|다음 질문|보류')
        text=$Body.Trim()
    })
}
foreach ($ref in @(Ordinal @($sourceMap.Keys))) {
    $lines = @(Get-Content -LiteralPath (Full $ref) -Encoding UTF8)
    $heading = ''; $start = 1; $buffer = [Collections.Generic.List[string]]::new(); $direct = ''; $semanticTable=$false
    for ($i=0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^\|\s*의미 식별자\s*\|\s*상태\s*\|') { $semanticTable=$true }
        elseif ($line -notmatch '^\|') { $semanticTable=$false }
        if ($line -match '^#{1,6}\s+(.+)$') {
            Add-Section $ref $start $heading ($buffer -join "`n") $direct
            $heading = $Matches[1]; $start=$i+1; $buffer.Clear(); $direct=''
            # Only a single numbered heading is a direct answer location, never a range.
            if ($heading -match '^(?:다음 질문\s*[—-]\s*)?(Q-\d{3})(?!\d|[~～])(?:\s|$)') { $direct=$Matches[1] }
        }
        if ($semanticTable -and $line -match '^\|\s*`([a-z][a-z0-9-]+)`\s*\|') {
            $semanticId=$Matches[1]
            Need ($line -match '^\|\s*`[a-z][a-z0-9-]+`\s*\|\s*`(Asked|Confirmed|ConfirmedDirection|Incorporated|Deferred|Open)`(?:\s*/\s*`FutureExtension`)?\s*\|') "InvalidSemanticTableStatus:$ref`:$($i+1)"
            Add-Section $ref ($i+1) $heading $line $semanticId
        } elseif ($line -match '^\|\s*(?:`?Q-[^|]+\|\s*)?`?(Q-\d{3})(?:~(?:Q-)?(\d{3}))?`?\s*\|') {
            $rowId=$Matches[1]
            if ($Matches.ContainsKey(2) -and $Matches[2]) {
                $end=[int]$Matches[2]; $begin=[int]$rowId.Substring(2)
                Need ($begin -le $end) "ReversedTableRange:$ref"
                $rowId=($begin..$end | ForEach-Object {'Q-{0:D3}' -f $_}) -join ','
            }
            Add-Section $ref ($i+1) $heading $line $rowId
        } elseif ($line -match '^-\s*`(Q-\d{3})`(?:\s*[:：]|에서|은|는)') {
            Add-Section $ref ($i+1) $heading $line $Matches[1]
        } else { $buffer.Add($line) }
    }
    Add-Section $ref $start $heading ($buffer -join "`n") $direct
}
$depths = @{}
$depthText = Get-Content -LiteralPath (Full $config.depthIndexRef) -Raw -Encoding UTF8
foreach ($match in [regex]::Matches($depthText, '(D[1-5])-\d+/Q-(\d{3})')) {
    $depths['Q-' + $match.Groups[2].Value]=$match.Groups[1].Value
}
foreach ($prop in $scope.depthOverrides.PSObject.Properties) { $depths[$prop.Name]=$prop.Value }
$questions = [Collections.Generic.List[object]]::new(); $ids = @{}
function Add-Question([string] $QuestionId, [string] $TopicCode, [string] $Ref, [string] $Status, [string] $DepthCode, [string] $Kind) {
    Need (-not $ids.ContainsKey($QuestionId)) "DuplicateQuestion:$QuestionId"
    $ids[$QuestionId]=$true
    $directSections = @($sections | Where-Object { $_.directQuestionIds -ccontains $QuestionId })
    if ($depths.ContainsKey($QuestionId)) { $DepthCode=$depths[$QuestionId] }
    elseif ($DepthCode -eq 'Unclassified') {
        $known = @($directSections | Where-Object { $_.depthCode -ne 'Unclassified' } | ForEach-Object { $_.depthCode } | Select-Object -Unique)
        if ($known.Count -eq 1) { $DepthCode=$known[0] }
    }
    $questions.Add([ordered]@{
        questionId=$QuestionId; kind=$Kind; topicCode=$TopicCode; depthCode=$DepthCode
        sourceRef=$Ref; recordStatus=$Status
        directExcerptRefs=@($directSections | ForEach-Object { $_.sectionId })
        retrievalStatus=$(if ($directSections.Count) {'DirectExcerptAvailable'} else {'TopicSourceOnly'})
        implementationLookupRef=$(if ($Kind -eq 'LegacyNumbered') {$config.scopeRef} else {''})
    })
}
foreach ($topicItem in $scope.topics) {
    foreach ($number in @(Numbers $topicItem.questionSelector)) {
        $qid='Q-{0:D3}' -f $number
        $override=Field $scope.questionOverrides $qid $null
        # This is the recorded decision status, never executable approval or Evidence.
        Add-Question $qid $topicItem.topicCode $topicItem.sourceRef (Field $override 'decisionStatusCode' 'Confirmed') 'Unclassified' 'LegacyNumbered'
    }
}
Need ($questions.Count -eq ($scope.questionRange.last-$scope.questionRange.first+1)) 'LegacyCountMismatch'
foreach ($n in $scope.questionRange.first..$scope.questionRange.last) { Need ($ids.ContainsKey(('Q-{0:D3}' -f $n))) "MissingQuestion:$n" }
foreach ($supplement in $config.supplements) {
    Need ($sourceMap.ContainsKey($supplement.sourceRef)) "UnknownSupplementSource:$($supplement.sourceRef)"
    foreach ($n in @(Numbers $supplement.selector)) {
        $qid='Q-{0:D3}' -f $n
        Add-Question $qid $supplement.topicCode $supplement.sourceRef $supplement.recordStatus $supplement.depthCode 'SupplementNumbered'
        Need (@($sections | Where-Object { $_.sourceRef -eq $supplement.sourceRef -and $_.directQuestionIds -ccontains $qid }).Count -gt 0) "SupplementWithoutSource:$qid"
    }
}
foreach ($section in $sections) {
    if ($section.directQuestionId -cmatch '^[a-z][a-z0-9-]+$') {
        $status = 'SeeSource'
        if ($section.text -match '(?m)^- 상태:\s*`(Asked|Confirmed|ConfirmedDirection|Incorporated|Deferred|Open)`') { $status=$Matches[1] }
        elseif ($section.text -match '^\|\s*`[a-z][a-z0-9-]+`\s*\|\s*`(Asked|Confirmed|ConfirmedDirection|Incorporated|Deferred|Open)`(?:\s*/\s*`FutureExtension`)?\s*\|') { $status=$Matches[1] }
        elseif ($section.text -match '(?m)^- (?:질문 식별|의미 식별자):[ \t]*`[a-z][a-z0-9-]+`[ \t]*[—-][ \t]*`(Asked|Confirmed|ConfirmedDirection|Incorporated|Deferred|Open)`(?=[.\s]|$)') { $status=$Matches[1] }
        elseif ($section.heading -match '^사용자 방향\s*[—-]\s*ConfirmedDirection\s*/\s*D\d+$') { $status='ConfirmedDirection' }
        elseif ($section.text -match '`Incorporated`') { $status='Incorporated' }
        Add-Question $section.directQuestionId $section.topicCodes[0] $section.sourceRef $status $section.depthCode 'SemanticFollowup'
    }
    foreach ($direction in @(Inline-Directions $section.text)) {
        Add-Question $direction.Groups[1].Value $section.topicCodes[0] $section.sourceRef $direction.Groups[2].Value $section.depthCode 'SemanticFollowup'
    }
}
$inputRefs = @($SourcesPath, $config.scopeRef, $config.depthIndexRef, 'eng/planning-inquiries/manage-inquiry-search.ps1') + @($sourceMap.Keys)
$fingerprints = @(Ordinal $inputRefs | ForEach-Object {
    [ordered]@{path=$_; sha256=(Get-FileHash -LiteralPath (Full $_) -Algorithm SHA256).Hash}
})
$database=[ordered]@{
    schemaVersion='planning-inquiry-search.v1'; sourceScopeRevision=$scope.revision
    authority='DerivedSearchOnly: source documents own decisions; execution ledgers own approval and Evidence.'
    sources=$fingerprints
    questions=@(foreach ($qid in @(Ordinal @($ids.Keys))) { $questions | Where-Object { $_.questionId -ceq $qid } })
    sections=@($sections)
}
$relationsRef=Field $config 'spatialRelationsRef' ''
if ($relationsRef) {
    . (Join-Path $root 'eng/planning-inquiries/spatial-query.ps1')
    $database['spatial']=Get-InquirySpatialIndex $database $relationsRef
    $fingerprintMap=@{}
    foreach($f in @($database.sources)+@($database.spatial.sources)) {$fingerprintMap[$f.path]=$f}
    $database.sources=@(Ordinal @($fingerprintMap.Keys)|ForEach-Object {$fingerprintMap[$_]})
}
# PS5.1 uses HTML escaping by default; match it in PS7 and avoid version-dependent indentation.
if ($PSVersionTable.PSVersion.Major -ge 6) {
    $serialized=$database | ConvertTo-Json -Depth 30 -Compress -EscapeHandling EscapeHtml
} else { $serialized=$database | ConvertTo-Json -Depth 30 -Compress }
$json=ConvertTo-DeterministicText ($serialized + "`n")
if ($Mode -eq 'Write') {
    $null=Write-DeterministicTextIfChanged (Full $IndexPath) $json
    if($SpatialMarkdownPath){Need ([bool]$relationsRef) 'SpatialNotConfigured';$null=Write-DeterministicTextIfChanged (Full $SpatialMarkdownPath) (Get-InquirySpatialMarkdown $database.spatial)}
    if($CirculationMarkdownPath){Need ([bool]$relationsRef) 'SpatialNotConfigured';$null=Write-DeterministicTextIfChanged (Full $CirculationMarkdownPath) (Get-InquiryCirculationMarkdown $database.spatial)}
    if($WiMarkdownPath){Need ([bool]$relationsRef) 'SpatialNotConfigured';$null=Write-DeterministicTextIfChanged (Full $WiMarkdownPath) (Get-InquiryWiMarkdown $database.spatial)}
    Write-Output "Written: questions=$($questions.Count), sections=$($sections.Count), sources=$($database.sources.Count)"
    return
}
Need (Test-Path -LiteralPath (Full $IndexPath)) 'MissingIndex:RunWrite'
$existing=ConvertTo-DeterministicText ([IO.File]::ReadAllText((Full $IndexPath)))
Need ($existing -ceq $json) 'StaleOrModifiedIndex:RunWriteThenValidate'
if($SpatialMarkdownPath){Need ([bool]$relationsRef) 'SpatialNotConfigured';Need ((Test-Path -LiteralPath (Full $SpatialMarkdownPath)) -and (ConvertTo-DeterministicText ([IO.File]::ReadAllText((Full $SpatialMarkdownPath)))) -ceq (Get-InquirySpatialMarkdown $database.spatial)) 'SpatialMarkdownStale'}
if($CirculationMarkdownPath){Need ([bool]$relationsRef) 'SpatialNotConfigured';Need ((Test-Path -LiteralPath (Full $CirculationMarkdownPath)) -and (ConvertTo-DeterministicText ([IO.File]::ReadAllText((Full $CirculationMarkdownPath)))) -ceq (Get-InquiryCirculationMarkdown $database.spatial)) 'CirculationMarkdownStale'}
if($WiMarkdownPath){Need ([bool]$relationsRef) 'SpatialNotConfigured';Need ((Test-Path -LiteralPath (Full $WiMarkdownPath)) -and (ConvertTo-DeterministicText ([IO.File]::ReadAllText((Full $WiMarkdownPath)))) -ceq (Get-InquiryWiMarkdown $database.spatial)) 'WiMarkdownStale'}
if ($Mode -eq 'Validate') { Write-Output "Valid: questions=$($questions.Count), sections=$($sections.Count)"; return }
if ($Id -match '^Q-?(\d+)$') { $Id='Q-{0:D3}' -f [int]$Matches[1] }
if($Wi){Need ([bool]$relationsRef) 'SpatialNotConfigured';Need (-not ($Circulation -or $Spatial -or $HId -or $Gap -or $Topic -or $Depth -or $OpenOnly -or $Unreviewed)) 'WiUseIdText';Search-InquiryWi $database.spatial $Id $Text -Limit $Limit|ConvertTo-Json -Depth 40;return}
if($Circulation){Need ([bool]$relationsRef) 'SpatialNotConfigured';Need (-not ($Spatial -or $HId -or $Gap -or $Depth -or $OpenOnly)) 'CirculationUseIdTopicText';Search-InquiryCirculation $database.spatial $Id $Topic $Text -Unreviewed:$Unreviewed -Limit $Limit|ConvertTo-Json -Depth 30;return}
Need (-not $Unreviewed) 'UnreviewedRequiresCirculation'
if($Spatial -or $HId -or $Gap){
    Need ([bool]$relationsRef) 'SpatialNotConfigured'
    Need (-not ($Id -and $HId)) 'SpatialAmbiguousId'
    Need (-not ($Topic -or $Depth -or $OpenOnly)) 'SpatialUseIdTextOrGap'
    Search-InquirySpatial $database.spatial $(if($HId){$HId}else{$Id}) $Text $Gap $Limit | ConvertTo-Json -Depth 40
    return
}
$terms=@($Text.Split(' ', [StringSplitOptions]::RemoveEmptyEntries))
$hits=[Collections.Generic.List[object]]::new()
foreach ($q in $database.questions) {
    if ($Id -and $q.questionId -cne $Id) { continue }
    if ($Topic -and $q.topicCode -notlike "*$Topic*") { continue }
    if ($Depth -and $q.depthCode -ne $Depth) { continue }
    if ($OpenOnly -and $q.recordStatus -notin @('Asked','Open','Deferred','NeedsSourceRecovery')) { continue }
    $e=@($sections | Where-Object { $_.sectionId -in $q.directExcerptRefs })
    $haystack="$($q.questionId) $($q.topicCode) " + (($e | ForEach-Object {$_.text}) -join "`n")
    $matchesAll=$true
    foreach ($term in $terms) { if ($haystack.IndexOf($term,[StringComparison]::OrdinalIgnoreCase) -lt 0) {$matchesAll=$false} }
    if (-not $matchesAll) { continue }
    $hits.Add([ordered]@{kind='Question';question=$q;excerpts=$e})
}
# Full section search supplements direct Q hits; an open marker is a review lead, not an unanswered decision.
if (-not $Id -and -not $Depth) {
    foreach ($s in $sections) {
        if ($s.directQuestionId) { continue }
        if ($Topic -and -not @($s.topicCodes | Where-Object {$_ -like "*$Topic*"}).Count) { continue }
        if ($OpenOnly -and -not $s.containsOpenMarker) { continue }
        $matchesAll=$true
        foreach ($term in $terms) { if ($s.text.IndexOf($term,[StringComparison]::OrdinalIgnoreCase) -lt 0) {$matchesAll=$false} }
        if ($matchesAll) {$hits.Add([ordered]@{kind='SectionReviewLead';excerpt=$s})}
    }
}
[ordered]@{totalMatches=$hits.Count;returned=[Math]::Min($Limit,$hits.Count);results=@($hits | Select-Object -First $Limit)} | ConvertTo-Json -Depth 30
