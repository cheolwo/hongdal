$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/execution-ledgers/manage-playable-loop-topic-planning.ps1"
$inputPath = Join-Path $repositoryRoot "eng/execution-ledgers/playable-loops.json"
$outputPath = Join-Path $repositoryRoot "docs/AI/generated/playable-loop-topic-planning.md"

$first = & $manager -Mode Write
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $outputPath).Hash
$ticks = (Get-Item -LiteralPath $outputPath).LastWriteTimeUtc.Ticks
$second = & $manager -Mode Write
$validate = & $manager -Mode Validate
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $outputPath).Hash -ne $hash -or
    (Get-Item -LiteralPath $outputPath).LastWriteTimeUtc.Ticks -ne $ticks) {
    throw "PlayableLoopTopicPlanningOutputIsNotDeterministic"
}
if ($validate -notmatch "Units=16;Approved=0;Legacy=1") {
    throw "PlayableLoopTopicPlanningValidationDidNotComplete:$validate"
}
$generated = Get-Content -LiteralPath $outputPath -Raw -Encoding UTF8
foreach ($expected in @(
    "topic:nature-night-day2.v1",
    "LegacyActiveMigration",
    "현재 Goal 완료 전 승인 전환",
    "topic:nature-base-reflection.v1",
    "Draft",
    "playable-loop:nature-workbench-foundation.v1 / NotStarted")) {
    if (-not $generated.Contains($expected)) {
        throw "PlayableLoopTopicPlanningGeneratedEntryMissing:$expected"
    }
}

$artifactDirectory = Join-Path $repositoryRoot "artifacts/local/validation/playable-loop-topic-planning/negative"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
function Relative([string] $Path) {
    $rootWithSeparator = $repositoryRoot.TrimEnd("\") + "\"
    $rootUri = [Uri]::new($rootWithSeparator)
    $pathUri = [Uri]::new([IO.Path]::GetFullPath($Path))
    return [Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString())
}
function Read-Catalog() {
    Get-Content -LiteralPath $inputPath -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Require-Rejected([string] $Name, [object] $Catalog, [string] $ExpectedCode) {
    $path = Join-Path $artifactDirectory "$Name.json"
    [IO.File]::WriteAllText($path, ($Catalog | ConvertTo-Json -Depth 50), $utf8)
    $arguments = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $manager,
        "-Mode", "Write", "-PlayableLoopPath", (Relative $path),
        "-OutputPath", (Relative (Join-Path $artifactDirectory "$Name.md")))
    $failureText = ""
    try { $failureText = (& pwsh @arguments 2>&1 | Out-String) }
    catch { $failureText = $_.Exception.Message + "`n" + ($_ | Out-String) }
    if ($LASTEXITCODE -eq 0 -or -not $failureText.Contains($ExpectedCode)) {
        throw "PlayableLoopTopicPlanningNegativeWasAccepted:${Name}:$failureText"
    }
}

$missingGate = Read-Catalog
$unit = $missingGate.items | Where-Object loopStableId -eq "playable-loop:nature-shelter-foundation.v1"
$unit.PSObject.Properties.Remove("planningGate")
Require-Rejected "missing-gate" $missingGate "PlanningGateMissing"

$duplicateTopic = Read-Catalog
$unit = $duplicateTopic.items | Where-Object loopStableId -eq "playable-loop:nature-twilight-return.v1"
$unit.planningGate.topicStableId = "topic:nature-shelter-foundation.v1"
Require-Rejected "duplicate-topic" $duplicateTopic "TopicStableIdDuplicate"

$unapprovedActive = Read-Catalog
$unit = $unapprovedActive.items | Where-Object loopStableId -eq "playable-loop:nature-night-day2.v1"
$unit.planningGate.statusCode = "NotStarted"
Require-Rejected "unapproved-active" $unapprovedActive "ActiveGoalPlanningNotApproved"

$legacyTransfer = Read-Catalog
$unit = $legacyTransfer.items | Where-Object loopStableId -eq "playable-loop:nature-workbench-foundation.v1"
$unit.planningGate.statusCode = "LegacyActiveMigration"
Require-Rejected "legacy-transfer" $legacyTransfer "LegacyMigrationTransferred"

$badHash = Read-Catalog
$unit = $badHash.items | Where-Object loopStableId -eq "playable-loop:nature-base-reflection.v1"
$unit.planningGate.statusCode = "Approved"
$unit.planningGate.approvalEvidenceRef = "decision:test"
$unit.planningGate.designHashSha256 = ("0" * 64)
Require-Rejected "approved-bad-hash" $badHash "DesignHashMismatch"

$missingApproval = Read-Catalog
$unit = $missingApproval.items | Where-Object loopStableId -eq "playable-loop:nature-base-reflection.v1"
$unit.planningGate.statusCode = "Approved"
Require-Rejected "approved-missing-evidence" $missingApproval "ApprovalEvidenceMissing"

$aggregateGate = Read-Catalog
$aggregate = $aggregateGate.items | Where-Object loopStableId -eq "playable-loop:nature-survival-homestead.v1"
$aggregate | Add-Member -NotePropertyName planningGate -NotePropertyValue ([pscustomobject]@{ topicStableId="topic:bad.v1" })
Require-Rejected "aggregate-gate" $aggregateGate "AggregateHasPlanningGate"

$brokenSource = Read-Catalog
$unit = $brokenSource.items | Where-Object loopStableId -eq "playable-loop:nature-base-reflection.v1"
$unit.sourcePlanningDocumentRefs = @("docs/Architecture/does-not-exist.md")
Require-Rejected "broken-source-link" $brokenSource "SourcePlanningDocumentNotFound"

$missingHeading = Read-Catalog
$unit = $missingHeading.items | Where-Object loopStableId -eq "playable-loop:nature-base-reflection.v1"
$designDirectory = Join-Path $artifactDirectory "missing-heading-root"
New-Item -ItemType Directory -Path $designDirectory -Force | Out-Null
$designPath = Join-Path $designDirectory "nature-base-reflection.v1.md"
$content = Get-Content -LiteralPath (Join-Path $repositoryRoot $unit.planningGate.designDocumentRef) -Raw -Encoding UTF8
$content = $content.Replace("## 반복 폐루프", "## 반복 흐름")
[IO.File]::WriteAllText($designPath, $content, $utf8)
$missingHeading.designDocumentationPolicy.detailedDesignRoot = Relative $designDirectory
$unit.planningGate.designDocumentRef = Relative $designPath
$unit.planningGate.designHashSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $designPath).Hash
Require-Rejected "missing-required-heading" $missingHeading "DesignHeadingMissing"

Write-Output "PlayableLoopTopicPlanningTestsPassed:Positive=3;Negative=9;Units=16"
Write-Output $first
Write-Output $second
Write-Output $validate
