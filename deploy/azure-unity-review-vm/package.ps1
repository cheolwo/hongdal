param(
    [string]$OutputDirectory = "artifacts/local/azure-unity-review-vm",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$resolvedOutput = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
$allowedRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts\local"))
if (-not $resolvedOutput.StartsWith($allowedRoot + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDirectory must stay below artifacts/local."
}

$workRoot = Join-Path $resolvedOutput "package-work"
$bundleRoot = Join-Path $workRoot "bundle"
$apiRoot = Join-Path $bundleRoot "api"
$webPublishRoot = Join-Path $workRoot "web-publish"
$webRoot = Join-Path $bundleRoot "web"
$archivePath = Join-Path $resolvedOutput "unity-review-vm.tar.gz"
$checksumPath = "$archivePath.sha256"

if (Test-Path -LiteralPath $workRoot) {
    Remove-Item -LiteralPath $workRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $resolvedOutput -Force | Out-Null
New-Item -ItemType Directory -Path $apiRoot, $webPublishRoot, $webRoot -Force | Out-Null

Push-Location $repositoryRoot
try {
    & dotnet publish "Ssalddel.UnityReview.Api\Ssalddel.UnityReview.Api.csproj" `
        -c $Configuration `
        -r linux-x64 `
        --self-contained false `
        --nologo `
        -o $apiRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Unity Review API publish failed with exit code $LASTEXITCODE."
    }

    & dotnet publish "Ssalddel.Web.UnityReviewApp\Ssalddel.Web.UnityReviewApp.csproj" `
        -c $Configuration `
        --nologo `
        -o $webPublishRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Unity Review Web publish failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

Copy-Item -Path (Join-Path $webPublishRoot "wwwroot\*") -Destination $webRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "compose.yaml") -Destination $bundleRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "Caddyfile") -Destination $bundleRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "deploy-release.sh") -Destination $bundleRoot

$commit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
$branch = (& git -C $repositoryRoot branch --show-current).Trim()
$relevantStatus = @(& git -C $repositoryRoot status --short -- `
    Ssalddel.UnityReview.Api `
    Ssalddel.UnityReview.Core `
    Ssalddel.Web.UnityReviewApp `
    Ssalddel.UnityReview.slnx `
    Ssalddel.Contracts/Common/WorldProjection `
    Ssalddel/Services/WorldProjection `
    deploy/azure-unity-review-vm `
    Directory.Packages.props)
$commitLabel = if ($relevantStatus.Count -gt 0) { "$commit-working-tree" } else { $commit }
$manifest = [ordered]@{
    environment = "AzureUnityReviewFreeVm"
    commit = $commitLabel
    branch = $branch
    builtAtUtc = [DateTime]::UtcNow.ToString("O")
    webAppMode = "UnityArtifactReviewDedicatedVm"
    database = "MySqlOnly"
    imageStorage = "ImmutableDockerVolume"
}
[System.IO.File]::WriteAllText(
    (Join-Path $webRoot "preview-build.json"),
    ($manifest | ConvertTo-Json) + [Environment]::NewLine,
    [System.Text.UTF8Encoding]::new($false))

& tar.exe -C $bundleRoot -czf $archivePath .
if ($LASTEXITCODE -ne 0) {
    throw "Unity Review VM archive creation failed with exit code $LASTEXITCODE."
}
$checksum = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText(
    $checksumPath,
    "$checksum  unity-review-vm.tar.gz$([Environment]::NewLine)",
    [System.Text.UTF8Encoding]::new($false))

[pscustomobject]@{
    Archive = $archivePath
    Sha256 = $checksum
    ApiAssembly = Join-Path $apiRoot "Ssalddel.UnityReview.Api.dll"
    WebIndex = Join-Path $webRoot "index.html"
    Commit = $commitLabel
}
