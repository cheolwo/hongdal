[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/local/azure-web-preview",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot "..\.."))
$artifactsRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot "artifacts\local"))
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

$roleProjects = @(
    [pscustomobject]@{
        Code = "01"
        Project = "Ssalddel.Web.CommunityApp\Ssalddel.Web.CommunityApp.csproj"
        Stylesheet = "Ssalddel.Web.CommunityApp.styles.css"
    },
    [pscustomobject]@{
        Code = "02"
        Project = "Ssalddel.Web.OrdererApp\Ssalddel.Web.OrdererApp.csproj"
        Stylesheet = "Ssalddel.Web.OrdererApp.styles.css"
    },
    [pscustomobject]@{
        Code = "03"
        Project = "Ssalddel.Web.ShipperApp\Ssalddel.Web.ShipperApp.csproj"
        Stylesheet = "Ssalddel.Web.ShipperApp.styles.css"
    },
    [pscustomobject]@{
        Code = "04"
        Project = "Ssalddel.Web.DriverApp\Ssalddel.Web.DriverApp.csproj"
        Stylesheet = "Ssalddel.Web.DriverApp.styles.css"
    },
    [pscustomobject]@{
        Code = "05"
        Project = "Ssalddel.Web.WarehouseApp\Ssalddel.Web.WarehouseApp.csproj"
        Stylesheet = "Ssalddel.Web.WarehouseApp.styles.css"
    }
)

foreach ($role in $roleProjects) {
    $projectPath = Join-Path $repositoryRoot $role.Project
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        throw "Role WebApp project was not found: $projectPath"
    }
}

$launcherRoot = Join-Path $repositoryRoot "deploy\azure-vm\role-web-launcher"
$launcherIndexPath = Join-Path $launcherRoot "index.html"
$launcherStylesPath = Join-Path $launcherRoot "role-launcher.css"
$faviconPath = Join-Path $repositoryRoot "Ssalddel.WebApp\wwwroot\favicon.png"
foreach ($requiredPath in @($launcherIndexPath, $launcherStylesPath, $faviconPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Role launcher asset was not found: $requiredPath"
    }
}

if (Test-Path -LiteralPath $outputRoot) {
    Remove-Item -LiteralPath $outputRoot -Recurse -Force
}

$publishRoot = Join-Path $outputRoot "publish"
$buildRoot = Join-Path $outputRoot "build"
$webRoot = Join-Path $outputRoot "web"
$archivePath = Join-Path $outputRoot "web-preview.tar.gz"
$checksumPath = "$archivePath.sha256"

New-Item -ItemType Directory -Path $publishRoot -Force | Out-Null
New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null
New-Item -ItemType Directory -Path $webRoot -Force | Out-Null

Copy-Item -LiteralPath $launcherIndexPath -Destination (Join-Path $webRoot "index.html")
Copy-Item -LiteralPath $launcherStylesPath -Destination (Join-Path $webRoot "role-launcher.css")
Copy-Item -LiteralPath $faviconPath -Destination (Join-Path $webRoot "favicon.png")

$builtAtUtc = [DateTimeOffset]::UtcNow
$release = "azure-preview-$($builtAtUtc.ToString('yyyyMMddTHHmmssZ'))"

foreach ($role in $roleProjects) {
    $projectPath = Join-Path $repositoryRoot $role.Project
    $rolePublishRoot = Join-Path $publishRoot $role.Code
    $roleBuildRoot = Join-Path $buildRoot $role.Code

    & dotnet publish $projectPath `
        --configuration $Configuration `
        --artifacts-path $roleBuildRoot `
        --output $rolePublishRoot `
        -p:ContinuousIntegrationBuild=true `
        -p:BlazorWebAssemblyEnvironment=Production
    if ($LASTEXITCODE -ne 0) {
        throw "Role WebApp $($role.Code) publish failed with exit code $LASTEXITCODE."
    }

    $roleWebRoot = Join-Path $rolePublishRoot "wwwroot"
    $roleIndexPath = Join-Path $roleWebRoot "index.html"
    if (-not (Test-Path -LiteralPath $roleIndexPath -PathType Leaf)) {
        throw "Published Role WebApp $($role.Code) index.html was not found: $roleIndexPath"
    }

    $indexMarkup = [System.IO.File]::ReadAllText($roleIndexPath)
    if ($indexMarkup.IndexOf(
            $role.Stylesheet,
            [System.StringComparison]::Ordinal) -lt 0) {
        throw "Published Role WebApp $($role.Code) stylesheet link was not found."
    }

    $indexMarkup = $indexMarkup.Replace(
        $role.Stylesheet,
        "$($role.Stylesheet)?v=$release")
    [System.IO.File]::WriteAllText(
        $roleIndexPath,
        $indexMarkup,
        [System.Text.UTF8Encoding]::new($false))

    $roleTargetRoot = Join-Path $webRoot "roles\$($role.Code)"
    New-Item -ItemType Directory -Path $roleTargetRoot -Force | Out-Null
    foreach ($item in Get-ChildItem -LiteralPath $roleWebRoot -Force) {
        Copy-Item -LiteralPath $item.FullName -Destination $roleTargetRoot -Recurse -Force
    }
}

$commit = (& git -C $repositoryRoot rev-parse --short=12 HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) {
    throw "Unable to resolve the current Git commit."
}

$branch = (& git -C $repositoryRoot branch --show-current).Trim()
$relevantChanges = @(
    & git -C $repositoryRoot status --porcelain -- `
        Ssalddel.WebApp `
        Ssalddel.Web.CommunityApp `
        Ssalddel.Web.OrdererApp `
        Ssalddel.Web.ShipperApp `
        Ssalddel.Web.DriverApp `
        Ssalddel.Web.WarehouseApp `
        eng/web-role-app `
        deploy/azure-vm/role-web-launcher `
        Ssalddel.Ui.Common `
        Ssalddel.Contracts `
        Directory.Build.props `
        Directory.Packages.props
)
$commitLabel = if ($relevantChanges.Count -gt 0) {
    "$commit-working-tree"
}
else {
    $commit
}

$manifest = [ordered]@{
    environment = "AzurePreview"
    release = $release
    commit = $commitLabel
    branch = $branch
    builtAtUtc = $builtAtUtc.ToString("O")
    webAppMode = "RoleSeparated"
    roleApps = @("01", "02", "03", "04", "05")
}
$manifestJson = $manifest | ConvertTo-Json
$manifestTargets = @((Join-Path $webRoot "preview-build.json"))
$manifestTargets += $roleProjects | ForEach-Object {
    Join-Path $webRoot "roles\$($_.Code)\preview-build.json"
}
foreach ($manifestPath in $manifestTargets) {
    [System.IO.File]::WriteAllText(
        $manifestPath,
        $manifestJson + [Environment]::NewLine,
        [System.Text.UTF8Encoding]::new($false))
}

& tar.exe -C $webRoot -czf $archivePath .
if ($LASTEXITCODE -ne 0) {
    throw "Web preview archive creation failed with exit code $LASTEXITCODE."
}

$checksum = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText(
    $checksumPath,
    "$checksum  web-preview.tar.gz$([Environment]::NewLine)",
    [System.Text.UTF8Encoding]::new($false))

[pscustomobject]@{
    Release = $release
    Commit = $commitLabel
    Branch = $branch
    BuiltAtUtc = $builtAtUtc
    Mode = "RoleSeparated"
    RoleApps = $roleProjects.Code -join ","
    Archive = $archivePath
    Sha256 = $checksum
}
