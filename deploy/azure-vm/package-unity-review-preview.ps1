[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/local/azure-unity-review-preview",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts\local"))
$outputRoot = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    [System.IO.Path]::GetFullPath($OutputDirectory)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
}

if (-not $outputRoot.StartsWith(
        $artifactsRoot + [System.IO.Path]::DirectorySeparatorChar,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDirectory must be below $artifactsRoot."
}

$projectPath = Join-Path $repositoryRoot "Ssalddel.Web.UnityReviewApp\Ssalddel.Web.UnityReviewApp.csproj"
if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Unity Review WebApp project was not found: $projectPath"
}

if (Test-Path -LiteralPath $outputRoot) {
    Remove-Item -LiteralPath $outputRoot -Recurse -Force
}

$publishRoot = Join-Path $outputRoot "publish"
$buildRoot = Join-Path $outputRoot "build"
$webRoot = Join-Path $outputRoot "unity-review"
$archivePath = Join-Path $outputRoot "unity-review.tar.gz"
$checksumPath = "$archivePath.sha256"
New-Item -ItemType Directory -Path $publishRoot -Force | Out-Null
New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null

$builtAtUtc = [DateTimeOffset]::UtcNow
$release = "azure-unity-review-$($builtAtUtc.ToString('yyyyMMddTHHmmssZ'))"
& dotnet publish $projectPath `
    --configuration $Configuration `
    --artifacts-path $buildRoot `
    --output $publishRoot `
    -p:ContinuousIntegrationBuild=true `
    -p:BlazorWebAssemblyEnvironment=Production
if ($LASTEXITCODE -ne 0) {
    throw "Unity Review WebApp publish failed with exit code $LASTEXITCODE."
}

$publishedWebRoot = Join-Path $publishRoot "wwwroot"
$indexPath = Join-Path $publishedWebRoot "index.html"
$productionSettingsPath = Join-Path $publishedWebRoot "appsettings.Production.json"
foreach ($requiredPath in @($indexPath, $productionSettingsPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Published Unity Review WebApp asset was not found: $requiredPath"
    }
}

$indexMarkup = [System.IO.File]::ReadAllText($indexPath)
$stylesheet = "Ssalddel.Web.UnityReviewApp.styles.css"
foreach ($requiredMarkup in @('<base href="/" />', $stylesheet)) {
    if ($indexMarkup.IndexOf($requiredMarkup, [System.StringComparison]::Ordinal) -lt 0) {
        throw "Published Unity Review WebApp markup was not found: $requiredMarkup"
    }
}
$indexMarkup = $indexMarkup.Replace('<base href="/" />', '<base href="/unity-review/" />')
$indexMarkup = $indexMarkup.Replace($stylesheet, "$stylesheet?v=$release")
[System.IO.File]::WriteAllText(
    $indexPath,
    $indexMarkup,
    [System.Text.UTF8Encoding]::new($false))

New-Item -ItemType Directory -Path $webRoot -Force | Out-Null
foreach ($item in Get-ChildItem -LiteralPath $publishedWebRoot -Force) {
    Copy-Item -LiteralPath $item.FullName -Destination $webRoot -Recurse -Force
}

$commit = (& git -C $repositoryRoot rev-parse --short=12 HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) {
    throw "Unable to resolve the current Git commit."
}
$branch = (& git -C $repositoryRoot branch --show-current).Trim()
$relevantChanges = @(
    & git -C $repositoryRoot status --porcelain -- `
        Ssalddel.Web.UnityReviewApp `
        Ssalddel.Client.Infrastructure `
        Ssalddel.Contracts `
        deploy/azure-vm/Caddyfile `
        deploy/azure-vm/package-unity-review-preview.ps1 `
        deploy/azure-vm/deploy-unity-review-preview.sh `
        Directory.Build.props `
        Directory.Packages.props
)
$commitLabel = if ($relevantChanges.Count -gt 0) { "$commit-working-tree" } else { $commit }
$manifest = [ordered]@{
    environment = "AzurePreview"
    release = $release
    commit = $commitLabel
    branch = $branch
    builtAtUtc = $builtAtUtc.ToString("O")
    webAppMode = "UnityArtifactReviewSeparated"
    basePath = "/unity-review/"
    hierarchyLevels = @("H1", "H2", "H3")
    authority = "ServerAdministratorCandidateReview"
}
[System.IO.File]::WriteAllText(
    (Join-Path $webRoot "preview-build.json"),
    ($manifest | ConvertTo-Json) + [Environment]::NewLine,
    [System.Text.UTF8Encoding]::new($false))

& tar.exe -C $webRoot -czf $archivePath .
if ($LASTEXITCODE -ne 0) {
    throw "Unity Review WebApp archive creation failed with exit code $LASTEXITCODE."
}
$checksum = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText(
    $checksumPath,
    "$checksum  unity-review.tar.gz$([Environment]::NewLine)",
    [System.Text.UTF8Encoding]::new($false))

[pscustomobject]@{
    Release = $release
    Commit = $commitLabel
    Branch = $branch
    BuiltAtUtc = $builtAtUtc
    BasePath = "/unity-review/"
    HierarchyLevels = "H1,H2,H3"
    Archive = $archivePath
    Sha256 = $checksum
}
