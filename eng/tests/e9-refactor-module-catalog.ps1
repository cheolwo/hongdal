$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot (
    "eng/execution-ledgers/manage-e9-refactor-module-catalog.ps1")
$architectureRoot = Join-Path $repositoryRoot "docs/Architecture"
$e9Documents = @(Get-ChildItem -LiteralPath $architectureRoot -Filter "E9*.md" -File)
$canonicalDocumentCandidates = @($e9Documents | Where-Object {
    (Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8).Contains(
        "e9-refactor-module-catalog.json")
} | Sort-Object Length -Descending)
if ($canonicalDocumentCandidates.Count -lt 1) {
    throw "E9RefactorCanonicalDocumentDiscoveryFailed:0"
}
$canonicalDocumentPath = [string] $canonicalDocumentCandidates[0].FullName
$canonicalDocumentName = Split-Path -Leaf $canonicalDocumentPath
$compatibilityDocumentCandidates = @($e9Documents | Where-Object {
    $_.FullName -ne $canonicalDocumentPath -and
    (Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8).Contains(
        $canonicalDocumentName)
} | Sort-Object Length)
if ($compatibilityDocumentCandidates.Count -lt 1) {
    throw "E9RefactorCompatibilityDocumentDiscoveryFailed:0"
}
$compatibilityDocumentPath = [string] $compatibilityDocumentCandidates[0].FullName
$rootReadmePath = Join-Path $repositoryRoot "README.md"
$docsReadmePath = Join-Path $repositoryRoot "docs/README.md"
$result = & $manager

if ([string] $result -ne
    "E9RefactorModuleCatalogValid:Modules=9;Status=Named;Cycle=E9-E1-E9-Repeat") {
    throw "E9RefactorModuleCatalogValidationFailed:$result"
}

foreach ($path in @(
    $canonicalDocumentPath,
    $compatibilityDocumentPath,
    $rootReadmePath,
    $docsReadmePath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "E9RefactorModuleDocumentMissing:$path"
    }
}

$canonicalDocument = Get-Content -LiteralPath $canonicalDocumentPath -Raw -Encoding UTF8
$compatibilityDocument = Get-Content -LiteralPath $compatibilityDocumentPath -Raw -Encoding UTF8
$rootReadme = Get-Content -LiteralPath $rootReadmePath -Raw -Encoding UTF8
$docsReadme = Get-Content -LiteralPath $docsReadmePath -Raw -Encoding UTF8

foreach ($expected in @(
    "E9↔E1 반복 왕복 구현 체계",
    "한 번의 왕복 주기",
    "E9~E1 모듈 이름과 빈 책임",
    "E9변화봉투Module",
    "E5세계발현Module",
    "E4실행문맥결속Module",
    "E1핵심계약Module",
    "모듈 상태",
    "e9-refactor-module-catalog.json",
    "e9-wi-h-module-bindings.json")) {
    if (-not $canonicalDocument.Contains($expected)) {
        throw "E9RefactorCanonicalDocumentMissing:$expected"
    }
}

if (-not $compatibilityDocument.Contains($canonicalDocumentName)) {
    throw "E9RefactorCompatibilityDocumentInvalid"
}
if ($rootReadme.Contains("docs/Architecture/E9리팩토링모듈골격.md")) {
    throw "E9RefactorRootReadmeStillUsesCompatibilityPath"
}
foreach ($expected in @(
    "게임 개발 기준 문서의 책임",
    "Architecture/E9하향식수직구현체계.md",
    "Architecture/E9리팩토링모듈골격.md")) {
    if (-not $docsReadme.Contains($expected)) {
        throw "E9RefactorDocsReadmeAuthorityMissing:$expected"
    }
}

Write-Output "E9RefactorModuleCatalogTestsPassed:Modules=9;Named=9;DocumentAuthority=CanonicalWithCompatibilityStub"
