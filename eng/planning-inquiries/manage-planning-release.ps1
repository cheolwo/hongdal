#requires -Version 7.0
[CmdletBinding()]
param(
    [ValidateSet('Validate','Write','Check')] [string] $Mode = 'Validate',
    [Parameter(Mandatory)] [string] $DocumentPath,
    [string] $OutputDirectory = 'artifacts/local/planning-releases'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $root 'eng/common/deterministic-text-output.ps1')
function Require([bool] $Condition, [string] $Code) { if (-not $Condition) { throw "PlanningRelease:$Code" } }
function HashBytes([byte[]] $Bytes) { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)) }
function HashText([string] $Value) { HashBytes ([Text.Encoding]::UTF8.GetBytes($Value)) }
function SafePath([string] $Ref, [bool] $Output = $false) {
    Require (-not [string]::IsNullOrWhiteSpace($Ref)) 'InvalidPath'
    Require (-not [IO.Path]::IsPathRooted($Ref) -and $Ref -notmatch '[:\\]' -and $Ref -notmatch '(^|/)\.\.?(/|$)') 'UnsafePath'
    if ($Output) { Require ($Ref.StartsWith('artifacts/local/', [StringComparison]::Ordinal)) 'OutputOutsideArtifacts' }
    else { Require ($Ref -match '^(docs/|eng/|artifacts/local/).+\.(md|json)$') 'SourceOutsideAllowedRoots' }
    $full = [IO.Path]::GetFullPath((Join-Path $root $Ref))
    Require ($full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) 'OutsideRepository'
    $cursor = $full
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            Require (((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0) 'ReparsePoint'
        }
        $cursor = [IO.Path]::GetDirectoryName($cursor)
    }
    $full
}
$readSet = [Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
function ReadTracked([string] $Ref) {
    $full = SafePath $Ref
    Require (Test-Path -LiteralPath $full -PathType Leaf) 'SourceMissing'
    $file = Get-Item -LiteralPath $full
    Require ($file.Length -gt 0 -and $file.Length -le 4194304) 'SourceSize'
    $bytes = [IO.File]::ReadAllBytes($full)
    $sha = HashBytes $bytes
    if ($readSet.ContainsKey($Ref)) { Require ($readSet[$Ref] -ceq $sha) 'SourceChangedDuringRead' }
    $readSet[$Ref] = $sha
    # BOM does not become part of the parser input; its bytes remain part of the fingerprint.
    ([Text.UTF8Encoding]::new($false, $true).GetString($bytes)).TrimStart([char]0xFEFF)
}
function Shape($Value, [string[]] $Keys) {
    Require ($Value -is [Collections.IDictionary]) 'ObjectExpected'
    Require ($Value.Count -eq $Keys.Count) 'UnexpectedFields'
    foreach ($key in $Keys) { Require ($Value.Contains($key)) 'MissingField' }
}
function TextValue($Value) { Require ($Value -is [string] -and -not [string]::IsNullOrWhiteSpace($Value) -and $Value.Length -le 4000) 'TextExpected' }
function Token($Value) { Require ($Value -is [string] -and $Value -cmatch '^[a-zA-Z0-9][a-zA-Z0-9:._-]{0,119}$') 'InvalidIdentifier' }
function Choice($Value, [string[]] $Allowed) { Require ($Value -is [string] -and $Allowed -ccontains $Value) 'InvalidChoice' }
function Items($Value, [int] $Max = 256) { Require ($Value -is [array] -and $Value.Count -le $Max) 'ArrayExpectedOrTooLarge' }
function Index($Rows) {
    $map = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    foreach ($row in $Rows) {
        Require ($row -is [Collections.IDictionary] -and $row.Contains('key')) 'KeyMissing'
        Token $row.key
        Require (-not $map.ContainsKey($row.key)) 'DuplicateKey'
        $map.Add($row.key, $row)
    }
    return ,$map
}
function EvidenceKeys($Keys, $Evidence, [bool] $Required = $true) {
    Items $Keys
    Require (-not $Required -or $Keys.Count -gt 0) 'EvidenceRequired'
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($key in $Keys) { Token $key; Require ($Evidence.ContainsKey($key)) 'UnknownEvidence'; Require ($seen.Add($key)) 'DuplicateEvidence' }
}
function JsonKeys([System.Text.Json.JsonElement] $Value) {
    if ($Value.ValueKind -eq [System.Text.Json.JsonValueKind]::Object) {
        $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach ($property in $Value.EnumerateObject()) {
            Require ($seen.Add($property.Name)) 'DuplicateJsonProperty'
            JsonKeys $property.Value
        }
    } elseif ($Value.ValueKind -eq [System.Text.Json.JsonValueKind]::Array) {
        foreach ($item in $Value.EnumerateArray()) { JsonKeys $item }
    }
}

$document = ReadTracked $DocumentPath
# A single typed block lives in the reviewed Markdown, not in a second manually maintained ledger.
$blocks = [regex]::Matches($document, '(?ms)^```planning-release[ \t]*\r?\n(?<json>.*?)^```[ \t]*\r?$')
Require ($blocks.Count -eq 1) 'ExactlyOneReleaseBlockRequired'
$jsonDocument = [System.Text.Json.JsonDocument]::Parse([string]$blocks[0].Groups['json'].Value)
try { JsonKeys $jsonDocument.RootElement } finally { $jsonDocument.Dispose() }
$p = ConvertFrom-Json -InputObject $blocks[0].Groups['json'].Value -AsHashtable -Depth 64
Shape $p @('schemaVersion','planningId','revision','state','context','catalog','evidence','wiDecisions','objects','uses','visuals')
Require ($p.schemaVersion -cin @('planning-release.v1','planning-release.v2')) 'UnknownSchema'
$visualImport = $p.schemaVersion -ceq 'planning-release.v2'
Token $p.planningId; Token $p.revision
Choice $p.state @('Draft','ReviewedForHandoff')
Shape $p.context @('now','here','self','target','action','result','nextChoices')
foreach ($key in @('now','here','self','target','action','result')) { TextValue $p.context[$key] }
Items $p.context.nextChoices
foreach ($item in $p.context.nextChoices) { TextValue $item }
Shape $p.catalog @('revision','sha256')
$catalogRef = 'eng/execution-ledgers/world-interactions.json'
$catalog = ConvertFrom-Json -InputObject (ReadTracked $catalogRef) -AsHashtable -Depth 64
Require ($p.catalog.revision -ceq $catalog.revision -and $p.catalog.sha256 -ceq $readSet[$catalogRef]) 'CatalogDrift'
$wiIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($entry in $catalog.items) { Token $entry.id; Require ($wiIds.Add($entry.id)) 'DuplicateCatalogWi' }
foreach ($key in @('evidence','wiDecisions','objects','uses','visuals')) { Items $p[$key] }
Require ($p.wiDecisions.Count -gt 0) 'WiDecisionRequired'
$evidence = Index $p.evidence
$decisions = Index $p.wiDecisions
$objects = Index $p.objects
$null = Index $p.uses
$null = Index $p.visuals
foreach ($e in $p.evidence) {
    Shape $e @('key','path','sha256','quote')
    Require ($e.path -cne $DocumentPath) 'SelfFingerprintCycle'
    TextValue $e.quote
    $source = ReadTracked $e.path
    Require ($e.sha256 -is [string] -and $e.sha256 -ceq $readSet[$e.path]) 'EvidenceDrift'
    Require ($source.Contains($e.quote, [StringComparison]::Ordinal)) 'QuoteMismatch'
}
foreach ($w in $p.wiDecisions) {
    Shape $w @('key','disposition','existingIds','rationale','evidenceKeys')
    Choice $w.disposition @('Reuse','Specialize','Generalize','NewCandidate','Unresolved')
    TextValue $w.rationale; EvidenceKeys $w.evidenceKeys $evidence; Items $w.existingIds
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($id in $w.existingIds) { Token $id; Require ($wiIds.Contains($id)) 'UnknownWi'; Require ($seen.Add($id)) 'DuplicateWi' }
    if ($w.disposition -cin @('Reuse','Specialize')) { Require ($w.existingIds.Count -ge 1) 'ExistingWiRequired' }
    if ($w.disposition -ceq 'Generalize') { Require ($w.existingIds.Count -ge 2) 'GeneralizationNeedsMultipleWi' }
    if ($w.disposition -ceq 'NewCandidate') { Require ($w.existingIds.Count -eq 0) 'NewCandidateCannotClaimExistingWi' }
}
foreach ($o in $p.objects) {
    Shape $o @('key','definitionId','name','kind','evidenceKeys')
    if ($null -ne $o.definitionId) { Token $o.definitionId }
    TextValue $o.name; Choice $o.kind @('Actor','Physical','Information','Unresolved')
    EvidenceKeys $o.evidenceKeys $evidence
}
$usedObjects = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($u in $p.uses) {
    Shape $u @('key','wiKey','objectKey','role','evidenceKeys','catalogProof')
    Token $u.wiKey; Token $u.objectKey
    Require ($decisions.ContainsKey($u.wiKey) -and $objects.ContainsKey($u.objectKey)) 'UnknownUseReference'
    Choice $u.role @('Actor','Target','Tool','Input','Result','Place')
    EvidenceKeys $u.evidenceKeys $evidence
    if ($null -ne $u.catalogProof) {
        Shape $u.catalogProof @('ruleRevision','field','quote')
        TextValue $u.catalogProof.quote; Token $u.catalogProof.ruleRevision
        Choice $u.catalogProof.field @('worldAction','actorRequirements','resourceRequirements','spatialRequirements','taskRule','controlPolicyCode','startStateCodes','completionStateCodes')
        $decision = $decisions[$u.wiKey]
        Require ($decision.existingIds.Count -eq 1) 'ProofNeedsSingleWi'
        $entry = @($catalog.items | Where-Object { $_.id -ceq $decision.existingIds[0] })[0]
        Require ($entry.ruleRevision -ceq $u.catalogProof.ruleRevision) 'RuleRevisionMismatch'
        $field = $entry[$u.catalogProof.field]
        Require (($field -is [string] -and $field -ceq $u.catalogProof.quote) -or
            ($field -is [array] -and $field -ccontains $u.catalogProof.quote)) 'CatalogQuoteMismatch'
    }
    $null = $usedObjects.Add($u.objectKey)
}
# The local maintenance route invokes the existing ASP.NET Core UseCase in-process, without HTTP.
# Unsupported decisions remain in the document; never silently drop them from a partial DB import.
$gaps = [Collections.Generic.List[string]]::new()
if ($p.state -cne 'ReviewedForHandoff') { $gaps.Add('DocumentNotReviewed') }
if ($p.wiDecisions.Where({ $_.disposition -cne 'Reuse' -or $_.existingIds.Count -ne 1 }).Count) { $gaps.Add('OnlySingleExistingWiReuseSupported') }
if ($p.objects.Where({ $null -eq $_.definitionId }).Count) { $gaps.Add('ExistingDefinitionIdRequired') }
if ($p.uses.Count -eq 0 -or $p.uses.Count -gt 64) { $gaps.Add('RelationCountOutsideExistingContract') }
if ($p.uses.Where({ $null -eq $_.catalogProof }).Count) { $gaps.Add('CanonicalWiProofRequiredForLocalImport') }
if ($p.visuals.Count -gt 0 -and -not $visualImport) { $gaps.Add('VisualCompositionImportRequiresSeparateExistingWorkflow') }
if ($visualImport) { Require ($p.visuals.Count -ge 1 -and $p.visuals.Count -le 10) 'VisualRoleLimit' }
foreach ($w in $p.wiDecisions) {
    if (-not $p.uses.Where({ $_.wiKey -ceq $w.key }).Count) { $gaps.Add('DecisionWithoutStoredRelation') }
}
$localRequest = $null
if ($gaps.Count -eq 0) {
    $relations = @($p.uses | ForEach-Object {
        $u = $_; $o = $objects[$u.objectKey]; $w = $decisions[$u.wiKey]
        $contextKey = $p.planningId + ':' + $p.revision + ':' + $u.key
        Token $contextKey
        $note = [ordered]@{planningId=$p.planningId; revision=$p.revision; sourceRef=$DocumentPath
            sourceHash=$readSet[$DocumentPath]; context=$p.context; rationale=$w.rationale} | ConvertTo-Json -Depth 16 -Compress
        if ($note.Length -gt 1500) { $gaps.Add('ContextExceedsExistingContract') }
        [ordered]@{WorldInteractionId=$w.existingIds[0]; DefinitionId=$o.definitionId; Role=$u.role
            ContextKey=$contextKey; ObjectKind=$o.kind; ExtractionState='ExistingDefinitionReuse'
            RuleRevision=$u.catalogProof.ruleRevision; SourceField=$u.catalogProof.field; ExactQuote=$u.catalogProof.quote; ContextNote=$note}
    })
    $identities = @($relations | ForEach-Object { @($_.WorldInteractionId,$_.DefinitionId,$_.Role,$_.ContextKey) -join '|' })
    Require (@($identities | Select-Object -Unique).Count -eq $relations.Count) 'DuplicateStoredRelation'
    if ($gaps.Count -eq 0) {
        $localRequest = [ordered]@{RequestId=('planning:' + (HashText ($p.planningId + "`n" + $p.revision)))
            SourceRef=$catalogRef; SourceRevision=$catalog.revision; SourceHash=$readSet[$catalogRef]
            Definitions=@(); Relations=$relations}
    }
}
foreach ($o in $p.objects) { Require ($usedObjects.Contains($o.key)) 'OrphanObject' }
foreach ($v in $p.visuals) {
    if ($visualImport) {
        Shape $v @('key','objectKey','role','slotKey','expectedRevision','state','inventorySnapshotId','selectionEvidence','reason','evidenceKeys')
        Token $v.objectKey; Token $v.role; Token $v.slotKey
        Require ($objects.ContainsKey($v.objectKey) -and $null -ne $objects[$v.objectKey].definitionId) 'UnknownVisualObject'
        Require ($v.expectedRevision -is [long] -and $v.expectedRevision -ge 1) 'VisualExpectedRevisionRequired'
        Choice $v.state @('AutomaticDraft','Held','NotApplicable')
        TextValue $v.reason; EvidenceKeys $v.evidenceKeys $evidence
        if ($v.state -ceq 'AutomaticDraft') {
            Require ($objects[$v.objectKey].kind -ceq 'Physical') 'AutomaticPhysicalOnly'
            Require ($v.inventorySnapshotId -is [string] -and $v.inventorySnapshotId -cmatch '^[A-F0-9]{64}$') 'InventorySnapshotRequired'
            Require ($v.selectionEvidence -is [Collections.IDictionary]) 'SelectionEvidenceRequired'
            Require ($v.selectionEvidence.Origin -ceq 'CodexAutomatic' -and $v.selectionEvidence.SchemaVersion -ceq 'visual-auto-selection.r1') 'AutomaticOriginRequired'
        } else { Require ($null -eq $v.inventorySnapshotId -and $null -eq $v.selectionEvidence) 'HeldCannotSelect' }
        continue
    }
    Shape $v @('key','objectKey','role','state','assetVersionId','evidenceKeys')
    Token $v.objectKey; TextValue $v.role
    Require ($objects.ContainsKey($v.objectKey)) 'UnknownVisualObject'
    Choice $v.state @('Unselected','Candidate','Selected')
    if ($v.state -ceq 'Unselected') { Require ($null -eq $v.assetVersionId) 'UnselectedHasAsset' }
    else { Token $v.assetVersionId }
    EvidenceKeys $v.evidenceKeys $evidence ($v.state -cne 'Unselected')
}
if ($visualImport) {
    $roles = @($p.visuals | ForEach-Object { $objects[$_.objectKey].definitionId + '|' + $_.role + '|' + $_.slotKey })
    Require (@($roles | Select-Object -Unique).Count -eq $roles.Count) 'DuplicateVisualSlot'
    foreach ($set in ($p.visuals | Group-Object objectKey)) {
        Require (@($set.Group.expectedRevision | Select-Object -Unique).Count -eq 1) 'VisualRevisionConflict'
    }
}
# Local source consistency is not semantic approval, asset fitness, live DB validation, or E maturity.
$dependencies = @($readSet.Keys | Sort-Object -CaseSensitive | ForEach-Object { [ordered]@{path=$_; sha256=$readSet[$_]} })
foreach ($d in $dependencies) { Require ((HashBytes ([IO.File]::ReadAllBytes((SafePath $d.path)))) -ceq $d.sha256) 'SourceChangedDuringRead' }
$envelope = [ordered]@{
    schemaVersion='planning-handoff.v1'
    planningId=$p.planningId; revision=$p.revision
    sourceRef=$DocumentPath; sourceSha256=$readSet[$DocumentPath]
    state=$(if ($p.state -ceq 'Draft') {'DraftPrepared_NotSubmitted'} else {'Prepared_NotSubmitted'})
    authority='AuthorDeclaredPlanning_NotProductApproval'
    serverBinding='LocalInProcess_ExistingWiUseCase_NoHttp'
    externalReferences='ObjectAndAssetIdsRequireServerValidation'
    dependencies=$dependencies; decisions=$p
    localImport=[ordered]@{state=$(if ($null -eq $localRequest) {'NotReady'} else {'Prepared_NotApplied'})
        gaps=@($gaps); request=$localRequest; scope='ExistingDefinitions_WiParticipationOnly_NoAssetSelection'}
}
if ($visualImport) {
    $envelope.localImport.scope = 'ExistingDefinitions_AutomaticVisualDraftOnly_WiRelationsNotImported'
    $envelope.localImport['visuals'] = $p.visuals
}
$json = ConvertTo-DeterministicText (($envelope | ConvertTo-Json -Depth 64) + "`n")
$packetHash = HashText $json
$directory = SafePath $OutputDirectory $true
$fileName = (HashText ($p.planningId + "`n" + $p.revision)) + '.json'
$outputRef = $OutputDirectory.TrimEnd('/') + '/' + $fileName
$output = SafePath $outputRef $true
$writeState = 'NotWritten'
if ($Mode -cin @('Write','Check')) {
    if (Test-Path -LiteralPath $output) {
        Require ((HashBytes ([IO.File]::ReadAllBytes($output))) -ceq $packetHash) 'ImmutableRevisionConflict'
        $writeState = 'ExistingIdentical'
    } elseif ($Mode -ceq 'Check') { throw 'PlanningRelease:PacketMissing' }
    else {
        $null = [IO.Directory]::CreateDirectory($directory)
        $temporary = Join-Path $directory ('.planning-' + [guid]::NewGuid().ToString('N') + '.tmp')
        try {
            [IO.File]::WriteAllText($temporary, $json, [Text.UTF8Encoding]::new($false))
            # Never replace an existing revision, including a concurrent writer's output.
            [IO.File]::Move($temporary, $output, $false)
            $writeState = 'Created'
        } finally {
            if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary }
        }
    }
}
[ordered]@{status=$envelope.state; mode=$Mode; outputRef=$outputRef; packetSha256=$packetHash; writeState=$writeState
    wiDecisions=$p.wiDecisions.Count; objects=$p.objects.Count; uses=$p.uses.Count; visuals=$p.visuals.Count
    transmission='NotAttempted'; database='NotAttempted'; serverBinding=$envelope.serverBinding
    localImport=$envelope.localImport.state; gaps=@($gaps)} | ConvertTo-Json -Depth 8
