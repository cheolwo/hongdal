$ErrorActionPreference = "Stop"

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$indexPath = Join-Path $repositoryRoot "eng\work-areas\responsibility-workstreams.json"
$index = Get-Content -LiteralPath $indexPath -Raw -Encoding UTF8 | ConvertFrom-Json

if ($index.historicalBaseline.branch -ne "codex/rename-ssalddel") {
    throw "과거 통합 기준선은 codex/rename-ssalddel이어야 합니다."
}

if ($index.historicalBaseline.allowsNewGeneralPurposeWork -ne $false) {
    throw "과거 통합 기준선에서 새 일반 목적 작업을 허용할 수 없습니다."
}

$expected = @{
    operations = @{ Prefix = "operations/"; Scope = "operations" }
    simulation = @{ Prefix = "simulation/"; Scope = "simulation" }
    unity = @{ Prefix = "unity/"; Scope = "unity" }
    integration = @{ Prefix = "integration/"; Scope = "integration" }
}

$entries = @($index.primaryWorkstreams) + @($index.sharedWorkstream)
$actualKeys = @($entries | ForEach-Object { $_.key } | Sort-Object -Unique)
if (($actualKeys -join ",") -ne (($expected.Keys | Sort-Object) -join ",")) {
    throw "책임 흐름은 operations, simulation, unity, integration 네 개여야 합니다."
}

foreach ($entry in $entries) {
    $definition = $expected[$entry.key]
    if ($entry.branchPrefix -ne $definition.Prefix -or $entry.commitScope -ne $definition.Scope) {
        throw "$($entry.key)의 브랜치 prefix 또는 commit scope가 기준과 다릅니다."
    }

    $manifestPath = Join-Path $repositoryRoot ($entry.manifest -replace "/", "\")
    if (-not (Test-Path -LiteralPath $manifestPath)) {
        throw "$($entry.key) manifest를 찾을 수 없습니다: $($entry.manifest)"
    }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($manifest.key -ne $entry.key) {
        throw "$($entry.manifest)의 key가 index와 다릅니다."
    }
    if ($manifest.branchPrefix -ne $entry.branchPrefix -or $manifest.commitScope -ne $entry.commitScope) {
        throw "$($entry.key) manifest의 브랜치 또는 commit scope가 index와 다릅니다."
    }
    if ([string]::IsNullOrWhiteSpace($manifest.authority)) {
        throw "$($entry.key) manifest에 authority가 필요합니다."
    }

    foreach ($readFirstPath in @($manifest.readFirst)) {
        $requiredPath = Join-Path $repositoryRoot ($readFirstPath -replace "/", "\")
        if (-not (Test-Path -LiteralPath $requiredPath)) {
            throw "$($entry.key)의 readFirst 경로가 존재하지 않습니다: $readFirstPath"
        }
    }

    foreach ($relativeRoot in @($manifest.sourceRoots)) {
        $sourcePath = Join-Path $repositoryRoot ($relativeRoot -replace "/", "\")
        if (-not (Test-Path -LiteralPath $sourcePath)) {
            throw "$($entry.key)의 sourceRoot가 존재하지 않습니다: $relativeRoot"
        }
    }
}

$simulationUnityPath = Join-Path $repositoryRoot "eng\work-areas\simulation-unity.json"
$simulationUnity = Get-Content -LiteralPath $simulationUnityPath -Raw -Encoding UTF8 | ConvertFrom-Json
$requiredManifests = @(
    "eng/work-areas/simulation.json",
    "eng/work-areas/unity.json",
    "eng/work-areas/integration.json"
)

foreach ($requiredManifest in $requiredManifests) {
    if ($requiredManifest -notin @($simulationUnity.responsibilityManifests)) {
        throw "simulation-unity 호환 집계에 책임 manifest가 없습니다: $requiredManifest"
    }
}

Write-Host "책임 작업 흐름 검사 통과: Operations / Simulation / Unity + Integration"
